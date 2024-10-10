namespace CitasMedicas.AutenticacionApi.DTO
{
    public class LoginResponseDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string UsuarioAcceso { get; set; }
        public int RolUsuario { get; set; }
        public bool EsAdmin { get; set; }
        //public string Token { get; set; }
    }
}
