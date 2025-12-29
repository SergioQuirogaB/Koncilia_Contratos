using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Koncilia_Contratos.Models
{
    public class Contrato
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El año es obligatorio")]
        [Display(Name = "Año")]
        public int Anio { get; set; }

        [Required(ErrorMessage = "La empresa es obligatoria")]
        [StringLength(200)]
        [Display(Name = "Empresa")]
        public string Empresa { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cliente es obligatorio")]
        [StringLength(200)]
        [Display(Name = "Cliente")]
        public string Cliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de contrato es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Número de Contrato")]
        public string NumeroContrato { get; set; } = string.Empty;

        [Required(ErrorMessage = "El valor en pesos es obligatorio")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Valor en Pesos (sin IVA)")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        public decimal ValorPesos { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Valor en Dólares")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal? ValorDolares { get; set; }

        [StringLength(1000)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [StringLength(100)]
        [Display(Name = "Categoría")]
        public string? Categoria { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Valor Mensual")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        public decimal? ValorMensual { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        [Display(Name = "Fecha de Vencimiento")]
        [DataType(DataType.Date)]
        public DateTime FechaVencimiento { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Valor Facturado")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        public decimal? ValorFacturado { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "% de Ejecución según Facturación")]
        [DisplayFormat(DataFormatString = "{0:N2}%", ApplyFormatInEditMode = false)]
        public decimal? PorcentajeEjecucion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Valor Pendiente por Ejecutar")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        public decimal? ValorPendiente { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [StringLength(50)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Activo";

        [Display(Name = "Número de Horas")]
        public int? NumeroHoras { get; set; }

        [StringLength(100)]
        [Display(Name = "Número de Factura")]
        public string? NumeroFactura { get; set; }

        [StringLength(100)]
        [Display(Name = "Número de Póliza")]
        public string? NumeroPoliza { get; set; }

        [Display(Name = "Fecha de Vencimiento de la Póliza")]
        [DataType(DataType.Date)]
        public DateTime? FechaVencimientoPoliza { get; set; }

        [Display(Name = "Tipo de Documento")]
        [StringLength(50)]
        public string? TipoDocumento { get; set; } // Contrato, Oferta Mercantil, Otrosí

        [Display(Name = "Número")]
        [StringLength(100)]
        public string? Numero { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Valor")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        public decimal? Valor { get; set; }

        // Propiedad calculada
        [NotMapped]
        public int DiasRestantes
        {
            get
            {
                return (FechaVencimiento - DateTime.Today).Days;
            }
        }

        // Propiedad calculada para validar el estado
        [NotMapped]
        public bool EstaVencido
        {
            get
            {
                return DateTime.Today > FechaVencimiento;
            }
        }
    }
}

