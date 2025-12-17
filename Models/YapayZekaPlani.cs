using System;
using System.ComponentModel.DataAnnotations;

namespace web.Models
{
    public class YapayZekaPlani
    {
        [Key]
        public int Id { get; set; }

        public string? UyeId { get; set; }

        public string? ResimUrl { get; set; }

        public float? Boy { get; set; }

        public float? Kilo { get; set; }

        [Required]
        public string Hedef { get; set; }       // ESKİSİ: KullaniciIstegi

        public string ProgramMetni { get; set; }// ESKİSİ: AiCevabi

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
    }
}