namespace CitasMedicas.AutenticacionApi.DTO
{
    public class CambiarContraseniaDto
    {        
        public string UsuarioAcceso { get; set; }
        public string ContraseniaActual { get; set; }
        public string ContraseniaNueva { get; set; }
    }
}
