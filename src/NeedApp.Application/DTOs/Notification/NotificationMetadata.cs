using System.Text.Json.Serialization;

namespace NeedApp.Application.DTOs.Notification;

/// <summary>
/// Typed metadata records for each notification type.
/// Properties use JsonPropertyName to emit camelCase JSON keys
/// matching the frontend i18n interpolation variables.
/// </summary>

public sealed record NewMessageMetadata(
    [property: JsonPropertyName("requestTitle")] string RequestTitle,
    [property: JsonPropertyName("messagePreview")] string MessagePreview
);

public sealed record MissingInfoMetadata(
    [property: JsonPropertyName("requestTitle")] string RequestTitle,
    [property: JsonPropertyName("messageContent")] string MessageContent
);

public sealed record StatusChangeMetadata(
    [property: JsonPropertyName("requestTitle")] string RequestTitle,
    [property: JsonPropertyName("fromStatus")] string FromStatus,
    [property: JsonPropertyName("toStatus")] string ToStatus
);

/// <summary>Admin assigns a specific staff member to a request.</summary>
public sealed record AssignmentToMeMetadata(
    [property: JsonPropertyName("requestTitle")] string RequestTitle
);

/// <summary>Staff self-assigns; all admins are notified.</summary>
public sealed record AssignmentSelfAcceptMetadata(
    [property: JsonPropertyName("requestTitle")] string RequestTitle,
    [property: JsonPropertyName("staffName")] string StaffName
);

public sealed record NewRequestMetadata(
    [property: JsonPropertyName("requestTitle")] string RequestTitle
);

public sealed record InvitationMetadata(
    [property: JsonPropertyName("clientName")] string ClientName,
    [property: JsonPropertyName("inviterName")] string InviterName,
    [property: JsonPropertyName("role")] string Role
);
