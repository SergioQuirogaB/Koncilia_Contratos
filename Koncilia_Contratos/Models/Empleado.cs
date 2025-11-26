using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Koncilia_Contratos.Models
{
    public class Empleado
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de cumpleaños es obligatoria")]
        [Display(Name = "Fecha de Cumpleaños")]
        [DataType(DataType.Date)]
        public DateTime FechaCumpleanos { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [StringLength(200)]
        [Display(Name = "Correo Electrónico")]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Display(Name = "Nombre Completo")]
        [NotMapped]
        public string NombreCompleto
        {
            get
            {
                return $"{Nombre} {Apellido}";
            }
        }

        [Display(Name = "Edad")]
        [NotMapped]
        public int Edad
        {
            get
            {
                var hoy = DateTime.Today;
                var edad = hoy.Year - FechaCumpleanos.Year;
                if (FechaCumpleanos.Date > hoy.AddYears(-edad)) edad--;
                return edad;
            }
        }

        [Display(Name = "Próximo Cumpleaños")]
        [NotMapped]
        public DateTime ProximoCumpleanos
        {
            get
            {
                var hoy = DateTime.Today;
                var proximoCumpleanos = new DateTime(hoy.Year, FechaCumpleanos.Month, FechaCumpleanos.Day);
                if (proximoCumpleanos < hoy)
                {
                    proximoCumpleanos = proximoCumpleanos.AddYears(1);
                }
                return proximoCumpleanos;
            }
        }

        [Display(Name = "Días para Cumpleaños")]
        [NotMapped]
        public int DiasParaCumpleanos
        {
            get
            {
                var proximoCumpleanos = ProximoCumpleanos;
                return (proximoCumpleanos - DateTime.Today).Days;
            }
        }

        [Display(Name = "¿Cumple Años Hoy?")]
        [NotMapped]
        public bool CumpleAniosHoy
        {
            get
            {
                return DateTime.Today.Month == FechaCumpleanos.Month && 
                       DateTime.Today.Day == FechaCumpleanos.Day;
            }
        }
    }
}

