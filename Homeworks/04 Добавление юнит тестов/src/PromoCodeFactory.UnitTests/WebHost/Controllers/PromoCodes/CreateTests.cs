using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Models.PromoCodes;
using Soenneker.Utils.AutoBogus;
using System.Linq.Expressions;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.PromoCodes;

public class CreateTests
{
    private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
    private readonly Mock<IRepository<PromoCode>> _promoCodesRepositoryMock;
    private readonly Mock<IRepository<Customer>> _customerRepositoryMock;
    private readonly Mock<IRepository<CustomerPromoCode>> _customerPromoCodeRepositoryMock;
    private readonly Mock<IRepository<Preference>> _preferenceRepositoryMock;
    private readonly PromoCodesController _sut;

    public CreateTests()
    {
        _partnersRepositoryMock = new Mock<IRepository<Partner>>();
        _promoCodesRepositoryMock = new Mock<IRepository<PromoCode>>();
        _customerRepositoryMock = new Mock<IRepository<Customer>>();
        _customerPromoCodeRepositoryMock = new Mock<IRepository<CustomerPromoCode>>();
        _preferenceRepositoryMock = new Mock<IRepository<Preference>>();
        _sut = new PromoCodesController(_promoCodesRepositoryMock.Object, _customerRepositoryMock.Object, _customerPromoCodeRepositoryMock.Object,
            _partnersRepositoryMock.Object, _preferenceRepositoryMock.Object);
    }

    [Fact]
    public async Task Create_WhenPartnerNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = CreatePromoCodeCreateRequest();

        _partnersRepositoryMock
            .Setup(r => r.GetById(request.PartnerId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result.Result;
        notFoundResult.Value.Should().BeOfType<ProblemDetails>();
        var problemDetails = (ProblemDetails)notFoundResult.Value!;
        problemDetails.Title.Should().Be("Partner not found");
        problemDetails.Detail.Should().Be($"Partner with Id {request.PartnerId} not found.");
    }

    [Fact]
    public async Task Create_WhenPreferenceNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = CreatePromoCodeCreateRequest();
        var partner = CreatePartnerWithLimit(request.PartnerId);

        _partnersRepositoryMock
            .Setup(r => r.GetById(request.PartnerId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        _preferenceRepositoryMock
            .Setup(r => r.GetById(request.PreferenceId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Preference?)null);

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result.Result;
        notFoundResult.Value.Should().BeOfType<ProblemDetails>();
        var problemDetails = (ProblemDetails)notFoundResult.Value!;
        problemDetails.Title.Should().Be("Preference not found");
        problemDetails.Detail.Should().Be($"Preference with Id {request.PreferenceId} not found.");
    }

    [Fact]
    public async Task Create_WhenNoActiveLimit_ReturnsUnprocessableEntity()
    {
        // Arrange
        var request = CreatePromoCodeCreateRequest();
        var preference = CreatePreference(request.PreferenceId);
        var partner = CreatePartnerWithLimit(request.PartnerId);
        partner.PartnerLimits.First().CanceledAt = DateTimeOffset.UtcNow.AddDays(-1);

        _partnersRepositoryMock
            .Setup(r => r.GetById(request.PartnerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        _preferenceRepositoryMock
            .Setup(r => r.GetById(request.PreferenceId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(preference);

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result.Result;
        objectResult.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        objectResult.Value.Should().BeOfType<ProblemDetails>();
        var problemDetails = (ProblemDetails)objectResult.Value!;
        problemDetails.Title.Should().Be("No active limit");
        problemDetails.Detail.Should().Be("Partner has no active promo code limit.");
    }

    [Fact]
    public async Task Create_WhenLimitExceeded_ReturnsUnprocessableEntity()
    {
        // Arrange
        var request = CreatePromoCodeCreateRequest();
        var partner = CreatePartnerWithLimit(request.PartnerId);
        var preference = CreatePreference(request.PreferenceId);
        var partnerLimit = partner.PartnerLimits.First();
        partnerLimit.Limit = partnerLimit.IssuedCount = 10;

        _partnersRepositoryMock
            .Setup(r => r.GetById(request.PartnerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        _preferenceRepositoryMock
            .Setup(r => r.GetById(request.PreferenceId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(preference);

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result.Result;
        objectResult.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        objectResult.Value.Should().BeOfType<ProblemDetails>();
        var problemDetails = (ProblemDetails)objectResult.Value!;
        problemDetails.Title.Should().Be("Limit exceeded");
        problemDetails.Detail.Should().Be($"Cannot create promo code. Limit would be exceeded (current: 10/10).");
    }

    [Fact]
    public async Task Create_WhenValidRequest_ReturnsCreatedAndIncrementsIssuedCount()
    {
        // Arrange
        var request = CreatePromoCodeCreateRequest();
        var partner = CreatePartnerWithLimit(request.PartnerId);
        var preference = CreatePreference(request.PreferenceId);
        var customer = CreateCustomer(preference);
        var oldIssuedCountLimit = partner.PartnerLimits.First().IssuedCount;

        _partnersRepositoryMock
            .Setup(r => r.GetById(request.PartnerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        _preferenceRepositoryMock
            .Setup(r => r.GetById(request.PreferenceId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(preference);

        _customerRepositoryMock.Setup(x => x.GetWhere(It.IsAny<Expression<Func<Customer, bool>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer> { customer });

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)result.Result;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.ActionName.Should().Be(nameof(PromoCodesController.GetById));

        var response = createdResult.Value.Should().BeOfType<PromoCodeShortResponse>().Subject;
        response.Should().NotBeNull();
        partner.PartnerLimits.First().IssuedCount.Should().Be(oldIssuedCountLimit + 1);
    }

    private static PromoCodeCreateRequest CreatePromoCodeCreateRequest()
    {
        return new AutoFaker<PromoCodeCreateRequest>();
    }

    private static Partner CreatePartnerWithLimit(
        Guid partnerId)
    {
        var limits = new List<PartnerPromoCodeLimit>();
        var partner = new AutoFaker<Partner>()
            .RuleFor(p => p.Id, _ => partnerId)
            .RuleFor(p => p.PartnerLimits, limits)
            .Generate();

        var limit = new AutoFaker<PartnerPromoCodeLimit>()
            .RuleFor(l => l.Id, _ => Guid.NewGuid())
            .RuleFor(l => l.Partner, partner)
            .RuleFor(l => l.CanceledAt, _ => null)
            .RuleFor(l => l.CreatedAt, _ => DateTimeOffset.UtcNow.AddDays(-1))
            .RuleFor(l => l.EndAt, _ => DateTimeOffset.UtcNow.AddDays(30))
            .RuleFor(l => l.Limit, _ => 10)
            .RuleFor(l => l.IssuedCount, _ => 5)
            .Generate();

        limits.Add(limit);
        return partner;
    }

    private static Preference CreatePreference(Guid preferenceId)
    {
        return new AutoFaker<Preference>()
            .RuleFor(r => r.Id, _ => preferenceId)
            .Generate(); ;
    }

    private static Customer CreateCustomer(Preference preference)
    {
        var preferences = new List<Preference> { preference };

        return new AutoFaker<Customer>()
            .RuleFor(x => x.Preferences, preferences)
            .Generate();
    }
}
