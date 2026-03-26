using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.Core.Exceptions;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Models.Partners;
using Soenneker.Utils.AutoBogus;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners;

public class SetLimitTests
{
    private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
    private readonly Mock<IRepository<PartnerPromoCodeLimit>> _partnerLimitsRepositoryMock;
    private readonly PartnersController _sut;

    public SetLimitTests()
    {
        _partnersRepositoryMock = new Mock<IRepository<Partner>>();
        _partnerLimitsRepositoryMock = new Mock<IRepository<PartnerPromoCodeLimit>>();
        _sut = new PartnersController(_partnersRepositoryMock.Object, _partnerLimitsRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateLimit_WhenPartnerNotFound_ReturnsNotFound()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var request = CreatePartnerPromoCodeLimitCreateRequest();

        _partnersRepositoryMock
            .Setup(r => r.GetById(partnerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        // Act
        var result = await _sut.CreateLimit(partnerId, request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result.Result;
        notFoundResult.Value.Should().BeOfType<ProblemDetails>();
        var problemDetails = (ProblemDetails)notFoundResult.Value!;
        problemDetails.Title.Should().Be("Partner not found");
        problemDetails.Detail.Should().Be($"Partner with Id {partnerId} not found.");
    }

    [Fact]
    public async Task CreateLimit_WhenPartnerBlocked_ReturnsUnprocessableEntity()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var limitId = Guid.NewGuid();
        var request = CreatePartnerPromoCodeLimitCreateRequest();

        var partner = CreatePartnerWithLimit(partnerId, limitId, isActive: false);

        _partnersRepositoryMock
            .Setup(r => r.GetById(partnerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        // Act
        var result = await _sut.CreateLimit(partnerId, request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        var objectResult = (UnprocessableEntityObjectResult)result.Result;
        objectResult.Value.Should().BeOfType<ProblemDetails>();
        var problemDetails = (ProblemDetails)objectResult.Value!;
        problemDetails.Title.Should().Be("Partner blocked");
        problemDetails.Detail.Should().Be("Cannot create limit for a blocked partner.");
    }

    [Fact]
    public async Task CreateLimit_WhenValidRequest_ReturnsCreatedAndAddsLimit()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var limitId = Guid.NewGuid();
        var request = CreatePartnerPromoCodeLimitCreateRequest();

        var partner = CreatePartnerWithLimit(partnerId, limitId, isActive: true);

        _partnersRepositoryMock
            .Setup(r => r.GetById(partnerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        // Act
        var result = await _sut.CreateLimit(partnerId, request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdAtResult = (CreatedAtActionResult)result.Result;
        createdAtResult.ActionName.Should().Be(nameof(_sut.GetLimit));
        createdAtResult.RouteValues.Should().ContainKey("partnerId");
        createdAtResult.RouteValues.Should().ContainKey("limitId");

        var response = createdAtResult.Value.Should().BeOfType<PartnerPromoCodeLimitResponse>().Subject;
        response.Limit.Should().Be(request.Limit);
        response.EndAt.Should().Be(request.EndAt);
        response.IssuedCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateLimit_WhenValidRequestWithActiveLimits_CancelsOldLimitsAndAddsNew()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var limitId = Guid.NewGuid();
        var request = CreatePartnerPromoCodeLimitCreateRequest();

        var partner = CreatePartnerWithLimit(partnerId, limitId, isActive: true);
        var oldLimit = partner.PartnerLimits.First();

        _partnersRepositoryMock
            .Setup(r => r.GetById(partnerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        _partnersRepositoryMock
            .Setup(r => r.Update(partner, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _partnerLimitsRepositoryMock
            .Setup(r => r.Add(partner.PartnerLimits.First(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateLimit(partnerId, request, CancellationToken.None);

        // Assert
        oldLimit.CanceledAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateLimit_WhenUpdateThrowsEntityNotFoundException_ReturnsNotFound()
    {
        //Arrange
        var partnerId = Guid.NewGuid();
        var limitId = Guid.NewGuid();
        var request = CreatePartnerPromoCodeLimitCreateRequest();

        var partner = CreatePartnerWithLimit(partnerId, limitId, isActive: true);

        _partnersRepositoryMock
            .Setup(r => r.GetById(partnerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        _partnersRepositoryMock.Setup(x => x.Update(partner, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException<Partner>(partner.Id));

        //Act
        var response = await _sut.CreateLimit(partner.Id, request, CancellationToken.None);

        //Assert
        response.Result.Should().BeOfType<NotFoundResult>();
    }

    private static PartnerPromoCodeLimitCreateRequest CreatePartnerPromoCodeLimitCreateRequest()
    {
        return new AutoFaker<PartnerPromoCodeLimitCreateRequest>();
    }

    private static Partner CreatePartnerWithLimit(
        Guid partnerId,
        Guid limitId,
        bool isActive,
        DateTimeOffset? canceledAt = null)
    {
        var role = new AutoFaker<Role>()
            .RuleFor(r => r.Id, _ => Guid.NewGuid())
            .Generate();

        var employee = new AutoFaker<Employee>()
            .RuleFor(e => e.Id, _ => Guid.NewGuid())
            .RuleFor(e => e.Role, role)
            .Generate();

        var limits = new List<PartnerPromoCodeLimit>();
        var partner = new AutoFaker<Partner>()
            .RuleFor(p => p.Id, _ => partnerId)
            .RuleFor(p => p.IsActive, _ => isActive)
            .RuleFor(p => p.Manager, employee)
            .RuleFor(p => p.PartnerLimits, limits)
            .Generate();

        var limit = new AutoFaker<PartnerPromoCodeLimit>()
            .RuleFor(l => l.Id, _ => limitId)
            .RuleFor(l => l.Partner, partner)
            .RuleFor(l => l.CanceledAt, _ => canceledAt)
            .RuleFor(l => l.CreatedAt, _ => DateTimeOffset.UtcNow.AddDays(-1))
            .RuleFor(l => l.EndAt, _ => DateTimeOffset.UtcNow.AddDays(30))
            .Generate();

        limits.Add(limit);
        return partner;
    }
}

