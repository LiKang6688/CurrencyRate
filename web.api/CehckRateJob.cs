using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using AngleSharp.Html.Parser;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Pomelo.AspNetCore.TimedJob;

namespace web.api {
    public class CehckRateJob : Job {

        public readonly IConfiguration _configuration;

        public CehckRateJob (IConfiguration configuration) {
            _configuration = configuration;
        }

        public static string GetHtml (string url) {
            HttpWebRequest myReq = (HttpWebRequest) WebRequest.Create (url);
            HttpWebResponse response = (HttpWebResponse) myReq.GetResponse ();
            Stream receiveStream = response.GetResponseStream ();
            StreamReader readStream = new StreamReader (receiveStream, Encoding.UTF8);
            return readStream.ReadToEnd ();
        }

        public async void parseHtml () {
            var parser = new HtmlParser ();
            var HtmlDoc = GetHtml ("https://www.valutafx.com/SEK-CNY.htm");

            var document = parser.ParseDocument (HtmlDoc);
            var titleItemList = document.All.Where (m => m.ClassName == "rate-value");
            foreach (var element in titleItemList) {
                var rate = element.InnerHtml.ToString ();
                Console.WriteLine (rate);
                string ratrString = rate.Substring (3, 5);
                int rateNumber = 0;
                Int32.TryParse (ratrString, out rateNumber);
                // if (rateNumber >= 74564) {
                if (rateNumber >= 78000) {
                    Console.WriteLine ("the currency rate is above 0.8");
                    MimeMessage mailMessage = new MimeMessage ();
                    MailboxAddress from = new MailboxAddress ("Admin", "li@scientificedtech.com");
                    mailMessage.From.Add (from);
                    MailboxAddress to = new MailboxAddress ("User", "kkllpaul7766@gmail.com");
                    mailMessage.To.Add (to);
                    mailMessage.Subject = "Currency rate/LK";
                    BodyBuilder bodyBuilder = new BodyBuilder ();
                    bodyBuilder.HtmlBody = "<h1>Hello World!</h1>";
                    bodyBuilder.HtmlBody = string.Format (@"Please confirm your Currency rate</a>");
                    mailMessage.Body = bodyBuilder.ToMessageBody ();
                    using (SmtpClient emailClient = new SmtpClient ()) {
                        emailClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                        await emailClient.ConnectAsync (_configuration["EmailConfiguration:SmtpServer"], int.Parse (_configuration["EmailConfiguration:SmtpPort"]), SecureSocketOptions.Auto);
                        emailClient.AuthenticationMechanisms.Remove ("XOAUTH2");
                        await emailClient.AuthenticateAsync (_configuration["EmailConfiguration:SmtpUsername"], _configuration["EmailConfiguration:SmtpPassword"]);
                        await emailClient.SendAsync (mailMessage);
                        await emailClient.DisconnectAsync (true);
                        // Clean up.
                        emailClient.Dispose ();
                    }
                }
            }
        }

        // Interval = 1000 * 60 * 60 * 2
        [Invoke (Begin = "2019-01-26 19:32", Interval = 1000 * 60 * 60 * 2, SkipWhileExecuting = true)]
        public void Run () {
            parseHtml ();
            Console.WriteLine (DateTime.Now.ToString () + ",TestJob run...");
        }

    }

}