using System.ComponentModel.DataAnnotations;

namespace Koncilia_Contratos.Models
{
    public class Configuracion
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Clave")]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Display(Name = "Valor")]
        public string Valor { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Categoría")]
        [StringLength(50)]
        public string? Categoria { get; set; }
    }
}

