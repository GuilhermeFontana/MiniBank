using System.Net.Mail;
using System.Net;

namespace MiniBank.Dependencies
{
    public class Mail
    {
        public void SendMail(string recipientAddress, string subject, string templateName, string? attachmentFileName = "")
        {
            string templateContent = File.ReadAllText($"{Directory.GetCurrentDirectory()}\\Files\\Templates\\{templateName}.html");

            var client = new SmtpClient(
                Environment.Variables.GetValue<string>("MailCredentials:Host"),
                int.Parse(Environment.Variables.GetValue<string>("MailCredentials:Port"))
            )
            {
                Credentials = new NetworkCredential(
                    Environment.Variables.GetValue<string>("MailCredentials:Username"),
                    Environment.Variables.GetValue<string>("MailCredentials:Password")
                ),
                EnableSsl = true
            };

            MailMessage message = new MailMessage();
            message.From = new MailAddress(Environment.Variables.GetValue<string>("MailCredentials:SenderAddress"));
            message.To.Add(recipientAddress);
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = templateContent;

            if (!String.IsNullOrEmpty(attachmentFileName))
            {
                string attachmentPath = $"{Directory.GetCurrentDirectory()}{Environment.Variables.GetValue<string>("TempFilesPath")}\\{attachmentFileName}";

                System.Net.Mime.ContentType contentType = new System.Net.Mime.ContentType();
                contentType.MediaType = System.Net.Mime.MediaTypeNames.Application.Octet;
                contentType.Name = attachmentFileName;
                message.Attachments.Add(new Attachment(attachmentPath, contentType));
            }

            client.Send(message);
        }
    }
}
