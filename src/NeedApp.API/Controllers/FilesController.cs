using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeedApp.Application.DTOs.Message;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.API.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
public class FilesController(
    ICloudinaryService cloudinaryService,
    IMessageRepository messageRepository,
    IFileAttachmentRepository fileAttachmentRepository,
    IRequestParticipantRepository participantRepository,
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IChatHubService chatHubService,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] Guid requestId,
        [FromForm] string? caption,
        List<IFormFile> files,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var isParticipant = await participantRepository.IsParticipantAsync(requestId, userId, cancellationToken);
        if (!isParticipant)
            throw new UnauthorizedException("You are not a participant of this request.");

        if (files.Count == 0)
            throw new DomainException("At least one file is required.");

        var sender = await userRepository.GetByIdAsync(userId, cancellationToken);

        // Create a file message
        var message = new Message
        {
            RequestId = requestId,
            SenderId = userId,
            Type = MessageType.File,
            Content = caption
        };
        await messageRepository.AddAsync(message, cancellationToken);

        var attachments = new List<FileAttachment>();
        foreach (var file in files)
        {
            using var stream = file.OpenReadStream();
            var result = await cloudinaryService.UploadFileAsync(stream, file.FileName);

            var attachment = new FileAttachment
            {
                MessageId = message.Id,
                FileName = file.FileName,
                CloudinaryPublicId = result.PublicId,
                Url = result.Url,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadedBy = userId
            };
            await fileAttachmentRepository.AddAsync(attachment, cancellationToken);
            attachments.Add(attachment);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Push file message via SignalR
        var fileDtos = attachments.Select(a => new FileAttachmentDto(a.Id, a.FileName, a.Url, a.ContentType, a.FileSize)).ToList();
        var senderDto = sender != null
            ? new MessageSenderDto(sender.Id, sender.Name, sender.Role, sender.AvatarUrl)
            : new MessageSenderDto(userId, null, null, null);

        var messageDto = new MessageDto(
            message.Id, message.Type, message.Content,
            senderDto, null, null, fileDtos, message.CreatedAt);

        await chatHubService.SendMessageToRequest(requestId, messageDto);

        return Ok(new
        {
            messageId = message.Id,
            files = attachments.Select(a => new { a.Id, a.FileName, a.Url, a.ContentType, a.FileSize })
        });
    }

    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        // Delete old avatar if exists
        if (!string.IsNullOrEmpty(user.AvatarPublicId))
            await cloudinaryService.DeleteFileAsync(user.AvatarPublicId);

        using var stream = file.OpenReadStream();
        var result = await cloudinaryService.UploadImageAsync(stream, file.FileName);

        user.AvatarUrl = result.Url;
        user.AvatarPublicId = result.PublicId;
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new { avatarUrl = result.Url });
    }
    
    [HttpDelete("avatar")]
    public async Task<IActionResult> DeleteAvatar(CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        if (string.IsNullOrEmpty(user.AvatarPublicId))
            return NoContent();

        await cloudinaryService.DeleteFileAsync(user.AvatarPublicId);

        user.AvatarUrl = null;
        user.AvatarPublicId = null;
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
