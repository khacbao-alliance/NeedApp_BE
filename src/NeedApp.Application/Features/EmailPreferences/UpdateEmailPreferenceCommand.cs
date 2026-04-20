using MediatR;
using NeedApp.Application.DTOs.EmailPreference;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.EmailPreferences;

public record UpdateEmailPreferenceCommand(
    bool OnAssignment,
    bool OnStatusChange,
    bool OnOverdue,
    bool OnNewRequest,
    DigestFrequency DigestFrequency
) : IRequest<EmailPreferenceDto>;

public class UpdateEmailPreferenceCommandHandler(
    IEmailPreferenceRepository emailPreferenceRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdateEmailPreferenceCommand, EmailPreferenceDto>
{
    public async Task<EmailPreferenceDto> Handle(UpdateEmailPreferenceCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedAccessException();

        var pref = await emailPreferenceRepository.GetByUserIdAsync(userId, cancellationToken);

        if (pref == null)
        {
            pref = new EmailPreference { UserId = userId };
            await emailPreferenceRepository.AddAsync(pref, cancellationToken);
        }

        pref.OnAssignment = request.OnAssignment;
        pref.OnStatusChange = request.OnStatusChange;
        pref.OnOverdue = request.OnOverdue;
        pref.OnNewRequest = request.OnNewRequest;
        pref.DigestFrequency = request.DigestFrequency;

        await unitOfWork.SaveChangesAsync(cancellationToken);

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
