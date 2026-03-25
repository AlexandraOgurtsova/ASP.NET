using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models.PromoCodes;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Промокоды
/// </summary>
public class PromoCodesController : BaseController
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Preference> _preferenceRepository;
    private readonly IRepository<PromoCode> _promoCodeRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<CustomerPromoCode> _customerPromoCodeRepository;

    public PromoCodesController(IRepository<Customer> customerRepository, IRepository<Preference> preferenceRepository, IRepository<PromoCode> promoCodeRepository,
        IRepository<Employee> employeeRepository, IRepository<CustomerPromoCode> customerPromoCodeRepository)
    {
        _customerRepository = customerRepository;
        _preferenceRepository = preferenceRepository;
        _promoCodeRepository = promoCodeRepository;
        _employeeRepository = employeeRepository;
        _customerPromoCodeRepository = customerPromoCodeRepository;
    }
    /// <summary>
    /// Получить все промокоды
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PromoCodeShortResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PromoCodeShortResponse>>> Get(CancellationToken ct)
    {
        var promoCodes = await _promoCodeRepository.GetAll(true, ct);

        var promoCodeModels = promoCodes.Select(PromoCodesMapper.ToPromoCodeShortResponse).ToList();

        return Ok(promoCodes);
    }

    /// <summary>
    /// Получить промокод по id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PromoCodeShortResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromoCodeShortResponse>> GetById(Guid id, CancellationToken ct)
    {
        var promoCode = await _promoCodeRepository.GetById(id, true, ct);
        if(promoCode == null)
            return NotFound();

        var promoCodeModel = PromoCodesMapper.ToPromoCodeShortResponse(promoCode);

        return Ok(promoCodeModel);
    }

    /// <summary>
    /// Создать промокод и выдать его клиентам с указанным предпочтением
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PromoCodeShortResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromoCodeShortResponse>> Create(PromoCodeCreateRequest request, CancellationToken ct)
    {
        if (request.BeginDate >= request.EndDate)
            return BadRequest("Begin date must be less than end date");

        if (request.EndDate <= DateTimeOffset.UtcNow)
            return BadRequest("End date must be in the future");

        var partnerManager = await _employeeRepository.GetById(request.PartnerManagerId, false, ct);
        if (partnerManager == null)
            return BadRequest($"Partner manager with Id {request.PartnerManagerId} not found");

        var preference = await  _preferenceRepository.GetById(request.PreferenceId, false, ct);
        if (preference == null)
            return BadRequest($"Preference with id {request.PreferenceId} not found");

        var customersWithPreference = await _customerRepository.GetWhere(c => c.Preferences.Any(p => p.Id == request.PreferenceId), true, ct);
        if (!customersWithPreference.Any())
            return NotFound($"Customers with preference: {preference.Name} not found");

        var promoCode = PromoCodesMapper.ToPromoCode(request, partnerManager, preference);

        foreach ( var customer in customersWithPreference)
        {
            var customerPromoCode = CustomerPromoCodeMapper.ToCustomerPromoCode(customer.Id, promoCode.Id);
            promoCode.CustomerPromoCodes.Add(customerPromoCode);
        }

        await _promoCodeRepository.Add(promoCode, ct);

        var promoCodeModel = PromoCodesMapper.ToPromoCodeShortResponse(promoCode);

        return CreatedAtAction(nameof(this.GetById), new { id = promoCodeModel.Id }, promoCodeModel);
    }

    /// <summary>
    /// Применить промокод (отметить, что клиент использовал промокод)
    /// </summary>
    [HttpPost("{id:guid}/apply")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Apply(
        [FromRoute] Guid id,
        [FromBody] PromoCodeApplyRequest request,
        CancellationToken ct)
    {
        var promoCode = await _promoCodeRepository.GetById(id, ct: ct);
        if (promoCode == null)
            return NotFound("Promo code not found");

        var customerPromoCodes = await _customerPromoCodeRepository.GetWhere(cpc => cpc.PromoCodeId == id && cpc.CustomerId == request.CustomerId
            && cpc.AppliedAt == null, ct: ct);

        var customerPromoCode = customerPromoCodes.FirstOrDefault();
        if (customerPromoCode == null)
            return NotFound($"No active promo code with id {id} for the customer with id {request.CustomerId}");

        customerPromoCode.AppliedAt = DateTime.UtcNow;
        await _customerPromoCodeRepository.Update(customerPromoCode, ct);

        return NoContent();
    }
}
