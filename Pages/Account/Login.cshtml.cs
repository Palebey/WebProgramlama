using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace web.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public LoginModel(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email alaný zorunludur.")]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Þifre alaný zorunludur.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Giriþ denemesi yap
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, isPersistent: false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Baþarýlýysa anasayfaya yönlendir
                    return RedirectToPage("/Index");
                }

                // Baþarýsýzsa hata mesajý göster
                ModelState.AddModelError(string.Empty, "Giriþ baþarýsýz. Email veya þifre hatalý.");
            }
            return Page();
        }
    }
}