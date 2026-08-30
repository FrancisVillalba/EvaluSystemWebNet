using PrintKm.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PrintKm.Pages.impresiones;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }
}
