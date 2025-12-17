using System.ComponentModel.DataAnnotations;

namespace web.Models
{
    public class SporSalonu
    {
        public int Id { get; set; }

        [Display(Name = "Salon Adı")]
        public string Ad { get; set; }

        public string Aciklama { get; set; }
        public string Hizmetler { get; set; }

        // string CalismaSaatleri yerine:
        [Display(Name = "Açılış Saati")]
        [DataType(DataType.Time)]
        public TimeSpan AcilisSaati { get; set; } // Örn: 09:00

        [Display(Name = "Kapanış Saati")]
        [DataType(DataType.Time)]
        public TimeSpan KapanisSaati { get; set; } // Örn: 22:00

        public decimal UyelikUcreti { get; set; }

        public ICollection<Antrenor>? Antrenorler { get; set; }
    }
}