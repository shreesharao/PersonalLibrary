using System.Threading.Tasks;
using SSR.PL.Web.Services.Abstractions;
using Microsoft.Extensions.Options;
using SSR.PL.Web.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SSR.PL.Web.Services.Implementations
{
    public class ApplicationEmailSender : IApplicationEmailSender
    {
        private readonly SendGridOptions _sendGridOptions;
        public ApplicationEmailSender(IOptions<SendGridOptions> sendGridOptionsSnapshot)
        {
            _sendGridOptions = sendGridOptionsSnapshot.Value;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(_sendGridOptions.SendGridApiKey,email,subject,htmlMessage);
        }

        private Task Execute(string apikey, string email, string subject, string htmlMessage)
        {
            var sendGridClient = new SendGridClient(apikey);

            var sendGridMessage = new SendGridMessage()
            {
                From = new EmailAddress("sshreesha4@gmail.com"),
                Subject = subject,
                HtmlContent = htmlMessage,
                PlainTextContent = htmlMessage,
            };

            sendGridMessage.AddTo(email);

            return sendGridClient.SendEmailAsync(sendGridMessage);
        }
    }
}
