using CitasMedicas.AutenticacionApi.DTO;
using CitasMedicas.AutenticacionApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CitasMedicas.AutenticacionApi.Data;

namespace CitasMedicas.AutenticacionUsuario.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutenticacionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AutenticacionController> _logger;

        public AutenticacionController(ApplicationDbContext context, ILogger<AutenticacionController> logger)
        {
            _context = context;
            _logger = logger;    
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            try
            {
                // Validación de datos de entrada
                if (string.IsNullOrWhiteSpace(loginDto.UsuarioAcceso) || string.IsNullOrWhiteSpace(loginDto.Contrasenia))
                {
                    return BadRequest("El nombre de usuario y la contraseña son obligatorios.");
                }

                // Buscar el usuario por UsuarioAcceso
                Usuario? usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.UsuarioAcceso == loginDto.UsuarioAcceso);

                // Validar que el usuario exista y la contraseña sea correcta
                if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginDto.Contrasenia, usuario.Contrasenia))
                {
                    return Unauthorized("Nombre de usuario o contraseña incorrectos.");
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

                // Crear la respuesta
                LoginResponseDto response = new LoginResponseDto
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    ApellidoPaterno = usuario.ApellidoPaterno,
                    UsuarioAcceso = usuario.UsuarioAcceso,
                    RolUsuario = usuario.RolUsuario,
                    EsAdmin = usuario.EsAdmin
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

        [HttpPost("cambiar-contrasenia")]
        public async Task<IActionResult> CambiarContrasenia([FromBody] CambiarContraseniaDto cambiarContraseniaDto)
        {
            // Buscar el usuario por UsuarioAcceso
            Usuario? usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioAcceso == cambiarContraseniaDto.UsuarioAcceso);

            if (usuario == null)
            {                
                return NotFound(new {message = "Usuario no encontrado." });
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
                return BadRequest(new {message = "La nueva contraseña debe tener al menos 6 caracteres." });
            }

            // Encriptar la nueva contraseña y actualizar
            usuario.Contrasenia = BCrypt.Net.BCrypt.HashPassword(cambiarContraseniaDto.ContraseniaNueva);
            usuario.ContraseniaVencimiento = DateTime.Now.AddDays(90);  // Ejemplo: vence en 90 días
            usuario.FechaModificacion = DateTime.Now;            

            await _context.SaveChangesAsync();

            return Ok(new { message = "Contraseña cambiada exitosamente" });
        }
    }
}