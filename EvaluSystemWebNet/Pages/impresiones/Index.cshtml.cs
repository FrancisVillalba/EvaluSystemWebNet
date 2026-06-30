using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EvaluSystemWebNet.Pages.impresiones;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }
}
