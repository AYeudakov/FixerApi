using CurrencyExchange.Application.Entities;
using CurrencyExchange.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.Controllers;

[ApiController]
[Route("api/v1")]
public class ExchangeCurrencyController : ControllerBase
{
    private readonly IExchangeCurrencyService _exchangeCurrencyService;

    public ExchangeCurrencyController(IExchangeCurrencyService exchangeCurrencyService)
    {
        _exchangeCurrencyService = exchangeCurrencyService;
    }

    [HttpPost]
    public async Task<ActionResult> GetExchangeCurrency(
        [FromBody]TransactionCreationRequest request)
    {
        var userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId")?.Value!);
        
        return Ok(await _exchangeCurrencyService.ExchangeCurrency(request, userId));
    }
}