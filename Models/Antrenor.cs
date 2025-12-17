using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web.Models
{
    public class Antrenor
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; }
        public string Uzmanlik { get; set; }

        // string MusaitlikSaatleri yerine:
        [Display(Name = "Mesai Başlangıç")]
        [DataType(DataType.Time)]
        public TimeSpan MesaiBaslangic { get; set; } // Örn: 10:00

        [Display(Name = "Mesai Bitiş")]
        [DataType(DataType.Time)]
        public TimeSpan MesaiBitis { get; set; } // Örn: 18:00

        public int SporSalonuId { get; set; }

        [ForeignKey("SporSalonuId")]
        public SporSalonu? SporSalonu { get; set; }

        public ICollection<Randevu>? Randevular { get; set; }
    }
}