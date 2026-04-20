using MediatR;
using NeedApp.Application.DTOs.SlaConfig;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.SlaConfig;

public record UpdateSlaConfigsCommand(
    List<SlaConfigItemRequest> Configs
) : IRequest<IEnumerable<SlaConfigDto>>;

public class UpdateSlaConfigsCommandHandler(
    ISlaConfigRepository slaConfigRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSlaConfigsCommand, IEnumerable<SlaConfigDto>>
{
    public async Task<IEnumerable<SlaConfigDto>> Handle(UpdateSlaConfigsCommand command, CancellationToken cancellationToken)
    {
        var existing = (await slaConfigRepository.GetAllAsync(cancellationToken)).ToList();

        foreach (var item in command.Configs)
        {
            var priority = item.Priority;

            var config = existing.FirstOrDefault(c => c.Priority == priority);
            if (config != null)
            {
                // Update existing
                config.DeadlineHours = item.DeadlineHours;
                config.Description = item.Description;
                slaConfigRepository.Update(config);
            }
            else
            {
                // Create new
                var newConfig = new Domain.Entities.SlaConfig
                {
                    Priority = priority,
                    DeadlineHours = item.DeadlineHours,
                    Description = item.Description,
                };
                await slaConfigRepository.AddAsync(newConfig, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Return updated configs
        var configs = await slaConfigRepository.GetAllConfigsAsync(cancellationToken);
        return configs.Select(c => new SlaConfigDto(
            c.Id,
            c.Priority.ToString(),
            c.DeadlineHours,
            c.Description
        ));
    }
}
