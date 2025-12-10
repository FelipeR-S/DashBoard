using DashBoard.Models;
using System.Net.Mail;
using System.Text;

namespace DashBoard.Repositories
{
    // SMTP CONFIG
    //1	Gmail - smtp.gmail.com - 587
    //2	Outlook - smtp.live.com - 587
    //3	Yahoo Mail - smtp.mail.yahoo.com - 465
    //4	Yahoo Mail - Plus plus.smtp.mail.yahoo.com - 465
    //5	Hotmail - smtp.live.com - 465
    //6	Office365.com - smtp.office365.com - 587
    //7	zoho Mail - smtp.zoho.com - 465
    public interface ISendEmail
    {
        /// <summary>
        /// Envia email
        /// </summary>
        /// <param name="para"><see cref="string"/> E-mail destinatário</param>
        /// <param name="assunto"><see cref="string"/> assunto</param>
        /// <param name="html"><see cref="string"/> mensagem em html</param>
        /// <returns><see cref="bool"/> confirmação</returns>
        Task<bool> EnviarEmail(string para, string html, string assunto);
    }
    public class SendEmail : ISendEmail
    {
        private readonly IConfiguration _configuration;
        private readonly string _email;
        private readonly string _senha;
        private readonly string _smtpConnection;
        private readonly int _smtpCode;

        public SendEmail(IConfiguration configuration)
        {
            _configuration = configuration;
            _email = _configuration["EmailConfig:Email"];
            _senha = _configuration["EmailConfig:SenhaEmail"];
            _smtpConnection = _configuration["EmailConfig:SMTPConnection"];
            int.TryParse(_configuration["EmailConfig:SMTPCode"], out _smtpCode);
        }

        public async Task<bool> EnviarEmail(string para, string html, string assunto)
        {
            var to = para; //Para endereço
            var from = _email; //De endereço
            var message = new MailMessage(from, to);

            message.Subject = assunto; //Assunto
            message.Body = html;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient(_smtpConnection, _smtpCode); //Configuração SMTP
            System.Net.NetworkCredential basicCredential = new
            System.Net.NetworkCredential(_email, _senha);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = basicCredential;

            try
            {
                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
