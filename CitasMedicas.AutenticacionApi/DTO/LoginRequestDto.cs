namespace CitasMedicas.AutenticacionApi.DTO
{
    public class LoginRequestDto
    {
        public string? UsuarioAcceso { get; set; }
        public string? CorreoElectronico { get; set; }
        public required string Contrasenia { get; set; }
    }
}
