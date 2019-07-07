using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace LMEmail.Model
{
    public class EmailHandler
    {
        private SmtpClient smtp;
        private string _from = "CoreApp@C#.run";
        private string _to = "mori.luca@hotmail.it";

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

        public void SendEmail(string from, string to, string subj, string body, bool isHtml)
        {
            using (var message = new MailMessage(from, to)
            {
                Subject = subj,
                Body = body,
                IsBodyHtml = isHtml
            })
            {
                if (smtp != null)
                    smtp.Send(message);
            }
        }

        public void SendEmailWithDefaultSettings(string subj, string body, bool isHtml)
        {
            SendEmail(_from, _to, subj, body, isHtml);
        }

    }
}
