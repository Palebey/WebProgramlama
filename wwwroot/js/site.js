document.addEventListener('DOMContentLoaded', function () {

    // Form elementini seç
    const form = document.getElementById('fitForm');

    if (form) {
        form.addEventListener('submit', function (e) {
            // Sayfanın yenilenmesini engelle
            e.preventDefault();

            // Butonu bul ve yükleniyor efekti ver
            const btn = form.querySelector('button[type="submit"]');
            const originalText = btn.innerHTML;

            btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Oluşturuluyor...';
            btn.disabled = true;

            // Yapay zeka simülasyonu (3 saniye bekletme)
            setTimeout(() => {
                alert('Tebrikler! Yapay zeka planınızı oluşturdu. (Bu bir demosu)');

                // Butonu eski haline getir
                btn.innerHTML = originalText;
                btn.disabled = false;

                // Sonuçlar kısmına kaydır (Smooth Scroll)
                document.querySelector('.position-relative.overflow-hidden').scrollIntoView({
                    behavior: 'smooth'
                });
            }, 2000);
        });
    }
});