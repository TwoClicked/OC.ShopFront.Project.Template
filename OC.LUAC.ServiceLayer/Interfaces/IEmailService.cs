namespace OC.LUAC.ServiceLayer.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, byte[]? pdfAttachment = null, string? attachmentName = null);
    }
}
