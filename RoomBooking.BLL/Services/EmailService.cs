using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Services
{
    public class EmailService : IEmailService
    {
        public bool SendEmail(EmailData emailData)
        {
            try
            {
                EmailSettings _emailSettings = new EmailSettings();
                // Gán giá trị cho các thuộc tính của đối tượng
                _emailSettings.EmailId = "quangdiep957@gmail.com";
                _emailSettings.Name = "Support - Pro Code Guide";
                _emailSettings.Password = "jkzrfapsjgwbjrih";
                _emailSettings.Host = "smtp.gmail.com";
                _emailSettings.Port = 465;
                _emailSettings.UseSSL = true;
                MimeMessage emailMessage = new MimeMessage();
                MailboxAddress emailFrom = new MailboxAddress(_emailSettings.Name, _emailSettings.EmailId);
                emailMessage.From.Add(emailFrom);
                MailboxAddress emailTo = new MailboxAddress(emailData.EmailToName, emailData.EmailToId);
                emailMessage.To.Add(emailTo);
                emailMessage.Subject = emailData.EmailSubject;
                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.TextBody = emailData.EmailBody;
                emailMessage.Body = emailBodyBuilder.ToMessageBody();
                SmtpClient emailClient = new SmtpClient();
                emailClient.Connect(_emailSettings.Host, _emailSettings.Port, _emailSettings.UseSSL);
                emailClient.Authenticate(_emailSettings.EmailId, _emailSettings.Password);
                emailClient.Send(emailMessage);
                emailClient.Disconnect(true);
                emailClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                //Log Exception Details
                return false;
            }
        }
    }
}
