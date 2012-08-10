using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Threading;
using System.Net.Mime;

namespace SenderService
{
    public class Email
    {
        private short MaxTrailNumber = 3;
        private short TrailNumber = 0;

        public void SendMail(string title, string messageContent, string femail, string temail, 
                              string[] attachements, int iterationNumber)
        {
            //string fromEmail = "WillGetOverridenAnyways@gmail.com";
            //var fromMessage = new MailAddress(fromEmail);
            //var toMessage = new MailAddress(temail);
            //MailMessage message = new MailMessage(fromMessage, toMessage);
            
            //message.BodyEncoding = System.Text.Encoding.UTF8;
            //message.IsBodyHtml = true;
            //message.Subject = title;
            //message.Body = messageContent;
            
            //foreach (var attachment in attachements)
            //{
            //    Attachment attach = new Attachment(attachment);
            //    message.Attachments.Add(attach);
            //}

            //string userName;
            //string password;
            //GetGmailInfo(iterationNumber, out userName, out password);
            //System.Net.NetworkCredential cred = new System.Net.NetworkCredential(userName, password);

            //message.IsBodyHtml = true;
            //System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com");
            //smtp.Timeout = 1000*5 * 60 ; //10 minutes of timeout
            //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            //smtp.UseDefaultCredentials = false;
            //smtp.EnableSsl = true;
            //smtp.Credentials = cred;
            
            //smtp.Port = 587;
            
            try
            {
                MailMessage mailMsg = new MailMessage();
        
                // To
                mailMsg.To.Add(new MailAddress(temail));
                 
                // From
                mailMsg.From = new MailAddress(femail);
 
                // Subject and multipart/alternative Body
                mailMsg.Subject = title;

                mailMsg.BodyEncoding = System.Text.Encoding.UTF8;
                mailMsg.IsBodyHtml = true;
                mailMsg.Subject = title;
                mailMsg.Body = messageContent;

                foreach (var attachment in attachements)
                {
                    Attachment attach = new Attachment(attachment);
                    mailMsg.Attachments.Add(attach);
                }
                
                mailMsg.IsBodyHtml = true;

                //mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
                //mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));
        
                // Init SmtpClient and send
                SmtpClient smtpClient = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));
                System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("Dror", "dror12345");
                smtpClient.Credentials = credentials;
                smtpClient.Timeout =  5 * 60 * 1000; //10 minutes of timeout
                smtpClient.Send(mailMsg);
                mailMsg.Attachments.Dispose();
            }
            catch (Exception ex)
            {
                ////ungly, isn't it?
                //try
                //{
                //    if (TrailNumber < MaxTrailNumber)
                //    {
                //        TrailNumber++;
                //        int waitBetweenSendings;
                //        int.TryParse(Utils.GetSetting("WaitBetweenSendings"), out waitBetweenSendings);
                //        Thread.Sleep(waitBetweenSendings);
                //        SendMail(title, messageContent, femail, temail, sendMailCompleteCallback, attachements, iterationNumber);
                //    }
                //    else
                //    {
                //        TrailNumber = 0;
                //        ErrorLogger.Log("Test", temail, title, messageContent, attachements.Count);
                //        sendMailCompleteCallback(ex, temail);
                //    }
                //}
                //catch (Exception innerException)
                //{
                //    var errorLog = new ErrorLog();
                //    errorLog.WriteException(innerException);
                //    sendMailCompleteCallback(ex, temail);
                //}
            }
        }
    }
}