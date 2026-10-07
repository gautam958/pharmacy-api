using AbcPharmacy.Api.Models;
using AbcPharmacy.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AbcPharmacy.Api.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    [HttpGet]
    public ActionResult<PagedResult<Sale>> GetAll(int page = 1, int pageSize = 10)
    {
        return Ok(_saleService.GetSales(page, pageSize));
    }

    [HttpPost]
    public ActionResult<Sale> Add(AddSaleRequest request)
    {
        try
        {
            var sale = _saleService.AddSale(request);
            return Created($"/api/sales/{sale.Id}", sale);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
