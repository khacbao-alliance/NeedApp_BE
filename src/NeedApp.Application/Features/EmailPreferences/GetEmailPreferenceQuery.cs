using MediatR;
using NeedApp.Application.DTOs.EmailPreference;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.EmailPreferences;

public record GetEmailPreferenceQuery : IRequest<EmailPreferenceDto>;

public class GetEmailPreferenceQueryHandler(
    IEmailPreferenceRepository emailPreferenceRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetEmailPreferenceQuery, EmailPreferenceDto>
{
    public async Task<EmailPreferenceDto> Handle(GetEmailPreferenceQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedAccessException();

        var pref = await emailPreferenceRepository.GetByUserIdAsync(userId, cancellationToken);

        if (pref == null)
        {
            // Return defaults when no record exists yet
            return new EmailPreferenceDto();
        }

        return new EmailPreferenceDto
        {
            OnAssignment = pref.OnAssignment,
            OnStatusChange = pref.OnStatusChange,
            OnOverdue = pref.OnOverdue,
            OnNewRequest = pref.OnNewRequest,
            DigestFrequency = pref.DigestFrequency
        };
    }
}
