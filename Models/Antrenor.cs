namespace web.Models // Burayı kendi proje isminle kontrol et
{
    public class Antrenor
    {
        public int Id { get; set; }

        public string AdSoyad { get; set; } = string.Empty;

        // "Kas geliştirme, Yoga vb." gibi uzmanlıklar [cite: 16]
        public string UzmanlikAlani { get; set; } = string.Empty;

       // Müsaitlik saatleri [cite: 17]
        public string CalismaSaatleri { get; set; } = string.Empty;

        // İlişki: Bir antrenör bir salonda çalışır [cite: 15]
        public int SporSalonuId { get; set; }
        public SporSalonu? SporSalonu { get; set; }
    }
}