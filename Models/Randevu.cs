using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Randevu Tarihi")]
        public DateTime TarihSaat { get; set; }

        [Display(Name = "Durum")]
        public string Durum { get; set; } = "Onay Bekliyor"; // Onaylandı, Reddedildi, Bekliyor

        // Randevuyu alan üye (IdentityUser ile ilişki)
        public string UyeId { get; set; }

        [ForeignKey("UyeId")]
        public IdentityUser? Uye { get; set; }

        // Randevu alınan antrenör
        public int AntrenorId { get; set; }

        [ForeignKey("AntrenorId")]
        public Antrenor? Antrenor { get; set; }
    }
}