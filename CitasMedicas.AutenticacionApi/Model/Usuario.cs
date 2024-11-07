using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CitasMedicas.AutenticacionApi.Model
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        public string CodigoUsuario { get; set; } 
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public int Dni { get; set; }
        public string? CorreoElectronico { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public int Genero { get; set; }
        public int NumeroTelefonico { get; set; }
        public string Direccion { get; set; }
        public string? UsuarioAcceso { get; set; }
        public string Contrasenia { get; set; }
        public DateTime? ContraseniaVencimiento { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public int? RolUsuario { get; set; }
        public bool EsActivo { get; set; }
        public bool EsAdmin { get; set; }        
        public string UsuarioCreacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
