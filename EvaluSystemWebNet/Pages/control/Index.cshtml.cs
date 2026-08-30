using EvaluSystemWebNet.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EvaluSystemWebNet.Pages.control;

public class IndexModel : PageModel
{
    private readonly IBackendApiClient _backendApiClient;

    public IndexModel(IBackendApiClient backendApiClient)
    {
        _backendApiClient = backendApiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = await _backendApiClient.GetResultAsync<IEnumerable<ControlPedidoDto>>("api/Control", cancellationToken);

        return result.StatusCode is 401 or 403
            ? RedirectToPage("/Index")
            : Page();
    }
}