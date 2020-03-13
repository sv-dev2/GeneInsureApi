using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Configuration;

namespace GensureAPIv2.Models
{
    public class EmailService
    {
        public void SendEmail(string pTo, string pCc, string pBcc, string pSubject, string pBody, List<string> pAttachments)
        {
            try
            {
                var portNumber = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["SendEmailPortNo"]);
                var enableSSL = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["SendEmailEnableSSL"]);
                var smtpAddress = Convert.ToString(ConfigurationManager.AppSettings["SendEmailSMTP"]);

                var FromMailAddress = System.Configuration.ConfigurationManager.AppSettings["SendEmailFrom"].ToString();
                var password = System.Configuration.ConfigurationManager.AppSettings["SendEmailFromPassword"].ToString();
                var client = new SmtpClient(smtpAddress, portNumber) //Port 8025, 587 and 25 can also be used.
                {
                    Credentials = new NetworkCredential(FromMailAddress, password),
                };
                client.UseDefaultCredentials = false;
                MailMessage _mailMessage = new MailMessage();
                _mailMessage.To.Add(new MailAddress(pTo));
                _mailMessage.From = new MailAddress(FromMailAddress, "GeneInsure");
                _mailMessage.Subject = pSubject;
                _mailMessage.IsBodyHtml = true;

                if (pAttachments.Count > 0)
                {
                    if (pAttachments[0] != "")
                    {
                        foreach (var item in pAttachments)
                        {
                            System.Net.Mail.Attachment attachment;
                            attachment = new System.Net.Mail.Attachment(System.Web.HttpContext.Current.Server.MapPath(item.ToString()));
                            _mailMessage.Attachments.Add(attachment);
                        }

                    }

                }


                AlternateView plainView = AlternateView.CreateAlternateViewFromString(pBody, null, "text/plain");
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(pBody, null, "text/html");
                _mailMessage.AlternateViews.Add(plainView);
                _mailMessage.AlternateViews.Add(htmlView);
                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                {
                    smtp.Credentials = new NetworkCredential(FromMailAddress, password);
                    smtp.EnableSsl = enableSSL;
                    try
                    {
                        smtp.Send(_mailMessage);
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {


                WriteLog(ex.Message);

            }
        }

        public void WriteLog(string error)
        {
            string message = string.Format("Error Time: {0}", DateTime.Now);
            message += error;
            message += "-----------------------------------------------------------";

            message += Environment.NewLine;




            string path = System.Web.HttpContext.Current.Server.MapPath("~/LogFile.txt");
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }

        private void populateMailAddresses(string pAddresses, MailAddressCollection pObj)
        {
            if (pAddresses != "")
            {
                string[] _addresses = pAddresses.Split(new char[] { ';' });
                foreach (string _addr in _addresses)
                {
                    pObj.Add(_addr);
                }
            }

        }
    }
}