using CitasMedicas.AutenticacionApi.DTO;
using CitasMedicas.AutenticacionApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CitasMedicas.AutenticacionApi.Data;
using CitasMedicas.AutenticacionApi.Services;
using System.Text.RegularExpressions;

namespace CitasMedicas.AutenticacionUsuario.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutenticacionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AutenticacionController> _logger;        
        private readonly TokenService _tokenService;        

        public AutenticacionController(ApplicationDbContext context, ILogger<AutenticacionController> logger, TokenService tokenService)
        {
            _context = context;
            _logger = logger;            
            _tokenService = tokenService;            
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            try
            {
                // Validación de datos de entrada
                if (string.IsNullOrWhiteSpace(loginDto.UsuarioAcceso) || string.IsNullOrWhiteSpace(loginDto.Contrasenia))
                {
                    return BadRequest("El nombre de usuario y la contraseña son obligatorios.");
                }

                // Determinar si el identificador es un correo electrónico o un nombre de usuario
                bool esPaciente = Regex.IsMatch(loginDto.UsuarioAcceso, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");


                // Buscar el usuario en la base de datos por correo o nombre de usuario
                Usuario? usuario = esPaciente
                    ? await _context.Usuarios.FirstOrDefaultAsync(u => u.CorreoElectronico == loginDto.UsuarioAcceso)
                    : await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioAcceso == loginDto.UsuarioAcceso);

                // Validar que el usuario exista y la contraseña sea correcta
                if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginDto.Contrasenia, usuario.Contrasenia))
                {
                    return Unauthorized("Nombre de usuario o contraseña incorrectos.");
                }

                // Validación adicional: Si es paciente, verificar que exista en la tabla Pacientes
                if (esPaciente)
                {
                    bool pacienteExiste = await _context.Pacientes.AnyAsync(p => p.IdUsuario == usuario.IdUsuario);
                    if (!pacienteExiste)
                    {
                        return Unauthorized("No se encontró un registro de paciente asociado a este correo electrónico.");
                    }
                }

                // Verificar si el usuario está activo
                if (!usuario.EsActivo)
                {
                    return StatusCode(403, new { message = "Tu cuenta está desactivada. Contacta al administrador." });
                }

                // Verificar si la contraseña está vencida
                if (usuario.ContraseniaVencimiento != null && usuario.ContraseniaVencimiento <= DateTime.Today)
                {
                    return StatusCode(403, new { message = "La contraseña ha vencido, por favor cámbiala para continuar." });
                }

                // Actualizar el último acceso del usuario
                usuario.UltimoAcceso = DateTime.Now;
                await _context.SaveChangesAsync();

                LoginResponseTokenDto response = new LoginResponseTokenDto
                {
                    // Generar el JWT
                    Token = _tokenService.GenerarToken(usuario, esPaciente)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Loguear el error para auditoría
                _logger.LogError(ex, "Error inesperado en el proceso de autenticación");

                // Devolver un error genérico al usuario
                return StatusCode(500, "Ocurrió un error en el servidor. Por favor, intenta nuevamente.");
            }
        }

        [HttpPatch]
        public async Task<IActionResult> CambiarContrasenia([FromBody] CambiarContraseniaDto cambiarContraseniaDto)
        {
            // Iniciar la transacción
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Buscar el usuario por UsuarioAcceso
                Usuario? usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.UsuarioAcceso == cambiarContraseniaDto.UsuarioAcceso);

                if (usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado." });
                }

                // Verificar la contraseña actual
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(cambiarContraseniaDto.ContraseniaActual, usuario.Contrasenia);
                if (!isPasswordValid)
                {
                    return Unauthorized(new { message = "La contraseña actual es incorrecta." });
                }

                // Validar que la nueva contraseña cumpla con las políticas de seguridad
                if (cambiarContraseniaDto.ContraseniaNueva.Length < 6)  // Puedes agregar más criterios de seguridad
                {
                    return BadRequest(new { message = "La nueva contraseña debe tener al menos 6 caracteres." });
                }

                // Encriptar la nueva contraseña y actualizar
                usuario.Contrasenia = BCrypt.Net.BCrypt.HashPassword(cambiarContraseniaDto.ContraseniaNueva);
                usuario.ContraseniaVencimiento = DateTime.Now.AddDays(90);  // Ejemplo: vence en 90 días
                usuario.FechaModificacion = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Contraseña cambiada exitosamente" });
            }
            catch (Exception ex)
            {
                // Deshacer la transacción si algo falla
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error en el registro: {ex.Message}");
            }            
        }
    }
}