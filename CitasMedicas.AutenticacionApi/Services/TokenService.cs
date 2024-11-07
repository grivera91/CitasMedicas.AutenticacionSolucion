using CitasMedicas.AutenticacionApi.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CitasMedicas.AutenticacionApi.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;        

        public TokenService(IConfiguration configuration)
        {            
            _configuration = configuration;            
        }

        public string GenerarToken(Usuario usuario, bool esPaciente)
        {
            string PerfilUsuario;

            if (esPaciente)
            {
                PerfilUsuario = "PACIENTE";
            }
            else
            {
                PerfilUsuario = usuario.RolUsuario switch
                {
                    1 => "RECEPCIONISTA",
                    2 => "MEDICO",
                    9 => "ADMINISTRADOR",
                    _ => "USUARIO",
                };
            }

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            // Validación de los valores de configuración esenciales
            string keyString = _configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key no está configurado");
            byte[] key = Encoding.ASCII.GetBytes(keyString);
            string issuer = _configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer no está configurado");
            string audience = _configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience no está configurado");

            if (!double.TryParse(_configuration["Jwt:ExpiresInMinutes"], out double expiresInMinutes))
            {
                throw new InvalidOperationException("Jwt:ExpiresInMinutes no es un valor numérico válido.");
            }

            DateTime expires = DateTime.UtcNow.AddMinutes(expiresInMinutes);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim("ApellidoPaterno", usuario.ApellidoPaterno),
                    new Claim("UsuarioAcceso", usuario.UsuarioAcceso),
                    new Claim(ClaimTypes.Role, usuario.RolUsuario.ToString()),
                    new Claim("EsAdmin", Convert.ToString(usuario.EsAdmin)),
                    new Claim("PerfilUsuario", PerfilUsuario)
                }),
                Expires = expires,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}