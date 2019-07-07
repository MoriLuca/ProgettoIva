using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace LMEmail.Model
{
    class EmailHandler
    {
        private SmtpClient smtp;

        public EmailHandler()
        {
            smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential("EmailWorkerino@gmail.com", "workerino88")
            };
        }

        public async Task SendEmail(string from, string to, string subj, string body, bool isHtml)
        {
            using (var message = new MailMessage(from, to)
            {
                Subject = subj,
                Body = body,
                IsBodyHtml = isHtml
            })
            {
                if (smtp!=null)
                    await smtp.SendMailAsync(message);
            }
        }

    }
}
