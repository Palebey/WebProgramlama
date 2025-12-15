namespace web.Models // "web" yerine kendi proje ismin neyse onu yazmalısın!
{
    public class SporSalonu
    {
        public int Id { get; set; }

        public string Ad { get; set; } = string.Empty;

        public string Adres { get; set; } = string.Empty;

        // Proje isterlerinde "Her salonun çalışma saatleri" belirtilmeli deniyor.
        public string CalismaSaatleri { get; set; } = string.Empty;

        public string Telefon { get; set; } = string.Empty;

        // İleride buraya "Hizmetler", "Antrenörler" gibi ilişkileri de ekleyeceğiz.
    }
}