using System;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Security;

namespace Dror.Common.Utils
{
    public class Email
    {
        /// <summary>
        /// address of SMTP server to send the message through
        /// </summary>
        private const string SMTP_SERVER_ADDRESS = "smtp.sendgrid.net";

        /// <summary>
        /// Port of SMTP server
        /// </summary>
        private const int SMTP_SERVER_PORT = 587;

        /// <summary>
        /// username used to send emails
        /// </summary>
        private const string SMTP_SERVER_USERNAME = "SMTP username";
        
        /// <summary>
        /// pasword used to send emails
        /// </summary>
        private static readonly SecureString SMTP_SERVER_PASSWORD =
            new SecureString();

        /// <summary>
        /// default constructor
        /// </summary>
        public Email()
        {
            // TODO: fix this usage of unsecure string
            "SMTP password"
                .ToCharArray()
                .ToList()
                .ForEach(SMTP_SERVER_PASSWORD.AppendChar);
        }

        /// <summary>
        /// constructor of the email object
        /// </summary>
        ///
        /// <param name="to">
        /// destination address of the email
        /// </param>
        ///
        /// <param name="toName">
        /// name of the destination of the email
        /// </param>
        ///
        /// <param name="from">
        /// "from" address of the email
        /// </param>
        ///
        /// <param name="fromName">
        /// name of the sender
        /// </param>
        ///
        /// <param name="subject">
        /// subject of the email
        /// </param>
        ///
        /// <param name="body">
        /// body of the email
        /// </param>
        public Email(
            string to,
            string toName,
            string from,
            string fromName,
            string subject,
            string body) : this()
        {
            To = to;
            ToName = toName;
            From = from;
            FromName = fromName;
            Subject = subject;
            Body = body;
        }

        /// <summary>
        /// send the email
        /// </summary>
        ///
        /// <returns>
        /// true if the sending succeeded, false otherwise
        /// </returns>
        public bool Send()
        {
            try
            {
                var mailMsg = new MailMessage();
                mailMsg.To.Add(new MailAddress(To, ToName));
                mailMsg.From = new MailAddress(From, FromName);
                mailMsg.Subject = Subject;
                mailMsg.AlternateViews.Add(
                    AlternateView.CreateAlternateViewFromString(
                        Body, null, MediaTypeNames.Text.Html));

                /* Init SmtpClient and send */
                var smtpClient = new SmtpClient(SMTP_SERVER_ADDRESS,
                    Convert.ToInt32(SMTP_SERVER_PORT));
                smtpClient.Credentials = new System.Net.NetworkCredential(
                    SMTP_SERVER_USERNAME, SMTP_SERVER_PASSWORD);

                smtpClient.Send(mailMsg);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// email destination address
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// destnation person name
        /// </summary>
        public string ToName { get; set; }

        /// <summary>
        /// email "from" address
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// name of sending entity
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// email subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// email body
        /// </summary>
        public string Body { get; set; }
    }
}
