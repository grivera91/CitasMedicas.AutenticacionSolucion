using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CitasMedicas.AutenticacionApi.Model
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }  // Clave primaria generada automáticamente
        public string Nombre { get; set; }  // Nombre del usuario
        public string ApellidoPaterno { get; set; }  // Apellido paterno del usuario
        public string ApellidoMaterno { get; set; }  // Apellido materno del usuario
        public int Dni { get; set; }  // Documento de identidad del usuario, debe ser único
        public string CorreoElectronico { get; set; }  // Correo electrónico, debe ser único
        public DateTime FechaNacimiento { get; set; }  // Fecha de nacimiento del usuario
        public int Genero { get; set; }  // Género del usuario (referencia a una tabla de géneros)
        public int NumeroTelefonico { get; set; }  // Número telefónico del usuario
        public string Direccion { get; set; }  // Dirección del usuario
        public string UsuarioAcceso { get; set; }  // Nombre de usuario para acceso al sistema, debe ser único
        public string Contrasenia { get; set; }  // Contraseña del usuario (debería ser encriptada)
        public DateTime? ContraseniaVencimiento { get; set; }  // Contraseña del usuario (debería ser encriptada)
        public int RolUsuario { get; set; }  // Rol del usuario (referencia a una tabla de roles)
        public bool EsAdmin { get; set; }  // Indica si el usuario es administrador
        public DateTime? UltimoAcceso { get; set; }  // Última fecha de acceso del usuario (nullable)
        public bool EsActivo { get; set; }  // Estado activo/inactivo del usuario
        public string UsuarioCreacion { get; set; }  // Usuario que creó este registro
        public DateTime FechaCreacion { get; set; }  // Fecha de creación del registro
        public string? UsuarioModificacion { get; set; }  // Usuario que modificó este registro
        public DateTime? FechaModificacion { get; set; }  // Fecha de la última modificación (nullable)
    }
}
