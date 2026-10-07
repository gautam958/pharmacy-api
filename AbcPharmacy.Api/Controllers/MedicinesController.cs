using AbcPharmacy.Api.Models;
using AbcPharmacy.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AbcPharmacy.Api.Controllers;

[ApiController]
[Route("api/medicines")]
public class MedicinesController : ControllerBase
{
    private readonly IMedicineService _medicineService;

    public MedicinesController(IMedicineService medicineService)
    {
        _medicineService = medicineService;
    }

    // GET api/medicines?search=para&filter=expiring&sortBy=price&sortDir=desc&page=1&pageSize=20
    [HttpGet]
    public ActionResult<PagedResult<MedicineDto>> GetAll(
        string? search, string? filter, string? sortBy, string? sortDir, int page = 1, int pageSize = 10)
    {
        return Ok(_medicineService.GetMedicines(search, filter, sortBy, sortDir, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public ActionResult<Medicine> GetById(int id)
    {
        try
        {
            return Ok(_medicineService.GetMedicine(id));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public ActionResult<Medicine> Add(AddMedicineRequest request)
    {
        try
        {
            var medicine = _medicineService.AddMedicine(request);
            return CreatedAtAction(nameof(GetById), new { id = medicine.Id }, medicine);
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
