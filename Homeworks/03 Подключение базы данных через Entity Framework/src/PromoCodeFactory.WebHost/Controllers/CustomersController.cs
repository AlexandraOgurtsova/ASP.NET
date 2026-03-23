using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.DataAccess.Repositories;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models;
using PromoCodeFactory.WebHost.Models.Customers;
using PromoCodeFactory.WebHost.Models.Preferences;
using PromoCodeFactory.WebHost.Models.PromoCodes;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Клиенты
/// </summary>
public class CustomersController : BaseController
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Preference> _preferenceRepository;
    private readonly IRepository<PromoCode> _promoCodeRepository;

    public CustomersController(IRepository<Customer> customerRepository, IRepository<Preference> preferenceRepository, IRepository<PromoCode> promoCodeRepository)
    {
        _customerRepository = customerRepository;
        _preferenceRepository = preferenceRepository;
        _promoCodeRepository = promoCodeRepository;
    }
    /// <summary>
    /// Получить данные всех клиентов
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerShortResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerShortResponse>>> Get(CancellationToken ct)
    {
        var customers = await _customerRepository.GetAll(withIncludes: false, ct);

        var customersModels = customers.Select(CustomerMapper.ToCustomerShortResponce).ToList();

        return Ok(customersModels);
    }

    /// <summary>
    /// Получить данные клиента по Id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetById(Guid id, CancellationToken ct)
    {
        var customer = await _customerRepository.GetById(id, true, ct);
        if (customer == null)
            return NotFound();

        var promoCodes = await _promoCodeRepository.GetByRangeId(customer.CustomerPromoCodes.Select(pc => pc.PromoCodeId), true, ct);

        var customersModel = CustomerMapper.ToCustomerResponse(customer, promoCodes);

        return Ok(customersModel);
    }

    /// <summary>
    /// Создать клиента
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerShortResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerShortResponse>> Create([FromBody] CustomerCreateRequest request, CancellationToken ct)
    {
        var preferences = await _preferenceRepository.GetByRangeId(request.PreferenceIds, false, ct);
        if (preferences.Count != request.PreferenceIds.Count())
            return BadRequest("Some preferences are not found");

        var customer = CustomerMapper.ToCustomer(request, preferences);

        await _customerRepository.Add(customer, ct);

        var customerModel = CustomerMapper.ToCustomerShortResponce(customer);

        return CreatedAtAction(nameof(this.GetById), new { id = customerModel.Id }, customerModel);
    }

    /// <summary>
    /// Обновить клиента
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerShortResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerShortResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] CustomerUpdateRequest request,
        CancellationToken ct)
    {
        var customer = await _customerRepository.GetById(id, true, ct);
        if (customer == null)
            return NotFound();

        var preferences = await _preferenceRepository.GetByRangeId(request.PreferenceIds, false, ct);
        if (preferences.Count != request.PreferenceIds.Count())
            return BadRequest("Some preferences are not found");

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Email = request.Email;
        customer.Preferences = preferences.ToList();

        await _customerRepository.Update(customer, ct);

        var customerModel = CustomerMapper.ToCustomerShortResponce(customer);

        return Ok(customerModel);
    }

    /// <summary>
    /// Удалить клиента
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _customerRepository.Delete(id, ct);

            return NoContent();
        }
        catch (EntityNotFoundException)
        {
            return NotFound();
        }
    }
}
