namespace NeedApp.Application.DTOs.IntakeQuestion;

public record IntakeQuestionSetDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    bool IsDefault,
    List<IntakeQuestionDto> Questions,
    DateTime CreatedAt
);

public record IntakeQuestionDto(
    Guid Id,
    string Content,
    int OrderIndex,
    bool IsRequired,
    string? Placeholder
);

public record CreateIntakeQuestionSetRequest(
    string Name,
    string? Description,
    bool IsDefault = false,
    List<CreateIntakeQuestionRequest>? Questions = null
);

public record CreateIntakeQuestionRequest(
    string Content,
    int OrderIndex,
    bool IsRequired = true,
    string? Placeholder = null
);

public record UpdateIntakeQuestionRequest(
    string Content,
    int OrderIndex,
    bool IsRequired = true,
    string? Placeholder = null
);

public record UpdateIntakeQuestionSetRequest(
    string Name,
    string? Description,
    bool IsDefault = false,
    bool IsActive = true,
    List<CreateIntakeQuestionRequest>? Questions = null
);
