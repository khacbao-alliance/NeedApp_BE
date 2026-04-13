using FluentAssertions;
using NeedApp.Application.Features.Requests.Commands;
using NeedApp.Application.Features.Notifications.Queries;
using NeedApp.Domain.Enums;
using Xunit;

namespace NeedApp.Tests.Features.Requests.Commands;

/// <summary>
/// Tests for FluentValidation validators — no mocking needed,
/// pure validator instantiation + .Validate() call.
/// </summary>
public class CommandValidatorTests
{
    // ── AssignRequestCommandValidator ─────────────────────────────

    [Fact]
    public void AssignRequestValidator_EmptyRequestId_IsInvalid()
    {
        var validator = new AssignRequestCommandValidator();
        var result = validator.Validate(new AssignRequestCommand(Guid.Empty, Guid.NewGuid()));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "RequestId");
    }

    [Fact]
    public void AssignRequestValidator_EmptyStaffId_IsInvalid()
    {
        var validator = new AssignRequestCommandValidator();
        var result = validator.Validate(new AssignRequestCommand(Guid.NewGuid(), Guid.Empty));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "StaffUserId");
    }

    [Fact]
    public void AssignRequestValidator_ValidCommand_IsValid()
    {
        var validator = new AssignRequestCommandValidator();
        var result = validator.Validate(new AssignRequestCommand(Guid.NewGuid(), Guid.NewGuid()));
        result.IsValid.Should().BeTrue();
    }

    // ── UpdateRequestStatusCommandValidator ───────────────────────

    [Fact]
    public void UpdateStatusValidator_EmptyRequestId_IsInvalid()
    {
        var validator = new UpdateRequestStatusCommandValidator();
        var result = validator.Validate(new UpdateRequestStatusCommand(Guid.Empty, RequestStatus.InProgress));
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(RequestStatus.Draft)]
    [InlineData(RequestStatus.Intake)]
    public void UpdateStatusValidator_DraftOrIntakeStatus_IsInvalid(RequestStatus status)
    {
        var validator = new UpdateRequestStatusCommandValidator();
        var result = validator.Validate(new UpdateRequestStatusCommand(Guid.NewGuid(), status));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.ErrorMessage.Contains("Draft or Intake"));
    }

    [Theory]
    [InlineData(RequestStatus.Pending)]
    [InlineData(RequestStatus.InProgress)]
    [InlineData(RequestStatus.Done)]
    [InlineData(RequestStatus.Cancelled)]
    [InlineData(RequestStatus.MissingInfo)]
    public void UpdateStatusValidator_AllowedStatuses_IsValid(RequestStatus status)
    {
        var validator = new UpdateRequestStatusCommandValidator();
        var result = validator.Validate(new UpdateRequestStatusCommand(Guid.NewGuid(), status));
        result.IsValid.Should().BeTrue();
    }

    // ── CreateRequestCommandValidator ─────────────────────────────

    [Fact]
    public void CreateRequestValidator_EmptyTitle_IsInvalid()
    {
        var validator = new CreateRequestCommandValidator();
        var result = validator.Validate(new CreateRequestCommand("", null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Title");
    }

    [Fact]
    public void CreateRequestValidator_TitleExceeds500Chars_IsInvalid()
    {
        var validator = new CreateRequestCommandValidator();
        var longTitle = new string('A', 501);
        var result = validator.Validate(new CreateRequestCommand(longTitle, null));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateRequestValidator_ValidTitle_IsValid()
    {
        var validator = new CreateRequestCommandValidator();
        var result = validator.Validate(new CreateRequestCommand("Valid title", "Desc"));
        result.IsValid.Should().BeTrue();
    }

    // ── GetNotificationsQuery (pagination sanity) ─────────────────

    [Fact]
    public void GetNotificationsQuery_DefaultValues_AreCorrect()
    {
        var query = new GetNotificationsQuery();
        query.Page.Should().Be(1);
        query.PageSize.Should().Be(20);
    }
}
