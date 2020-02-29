using myfoodapp.Hub.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Web.Configuration;

namespace myfoodapp.Hub.Common
{
    public class MailManager
    {
        private static string MailSendGridAPIKey = WebConfigurationManager.AppSettings["mailSendGridAPIKey"];

        public static void PioneerUnitOfflineMessage(ProductionUnit currentProductionUnit)
        {
            var dbLog = new ApplicationDbContext();

            try
            {
                var client = new SendGridClient(MailSendGridAPIKey);
                var from = new EmailAddress("hub@myfood.eu", "Myfood Hub Bot");

                var pioneerName = string.Format("{0} #{1}", currentProductionUnit.owner.pioneerCitizenName, currentProductionUnit.owner.pioneerCitizenNumber);

                List<EmailAddress> tos = new List<EmailAddress>
            {
                  new EmailAddress(currentProductionUnit.owner.contactMail, pioneerName)
            };

                var subject = string.Empty;
                var htmlContent = string.Empty;

                if (currentProductionUnit.owner != null && currentProductionUnit.owner.language != null)
                {
                    switch (currentProductionUnit.owner.language.description)
                    {
                        case "fr":
                            subject = "[myfood] Votre serre est déconnectée";
                            htmlContent = string.Format("Bonjour {0}, </br></br>" +
                                                        "Nous avons détecté que votre serre #{1} n'émet plus de signal depuis quelques heures.</br>" +
                                                        "Vérifiez l'état de fonctionnement électrique de votre installation.</br>" +
                                                        @"N'hésitez pas à consulter la rubrique de dépannage sur le <a href=""https://wiki.myfood.eu/docs/boitier-puissance"">WIKI</a>.</br></br>" +
                                                        "Bien à vous,", currentProductionUnit.owner.pioneerCitizenName, currentProductionUnit.owner.pioneerCitizenNumber);
                            break;
                        default:
                            subject = "[myfood] Your greenhouse is disconnected";
                            htmlContent = string.Format("Hi {0}, </br></br>" +
                                                        "We have detected that your greenhouse #{1} is no longer signaling since few hours.</br>" +
                                                        "Check the electrical operating status of your installation.</br>" +
                                                         @"Do not hesitate to consult the troubleshooting page on the <a href=""https://wiki.myfood.eu/docs/boitier-puissance"">WIKI</a>.</br></br>" +
                                                        "Have a nice day,", currentProductionUnit.owner.pioneerCitizenName, currentProductionUnit.owner.pioneerCitizenNumber);
                            break;
                    }
                }else
                {
                    subject = "[myfood] Your greenhouse is disconnected";
                    htmlContent = string.Format("Hi {0}, </br></br>" +
                                                "We have detected that your greenhouse #{1} is no longer signaling since few hours.</br>" +
                                                "Check the electrical operating status of your installation.</br>" +
                                                 @"Do not hesitate to consult the troubleshooting page on the <a href=""https://wiki.myfood.eu/docs/boitier-puissance"">WIKI</a>.</br></br>" +
                                                "Have a nice day,", currentProductionUnit.owner.pioneerCitizenName, currentProductionUnit.owner.pioneerCitizenNumber);
                }

                var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, "", htmlContent, false);
                //msg.AddCc("agro@myfood.eu");
                var response = client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                dbLog.Logs.Add(Log.CreateErrorLog(String.Format("Error with Mail Notification"), ex));
                dbLog.SaveChanges();
            }
                        
        }

        public static void PioneerUnitOnlineMessage(ProductionUnit currentProductionUnit)
        {
            var dbLog = new ApplicationDbContext();

            try
            {
                var client = new SendGridClient(MailSendGridAPIKey);
                var from = new EmailAddress("hub@myfood.eu", "Myfood Hub Bot");

                var pioneerName = string.Format("{0} #{1}", currentProductionUnit.owner.pioneerCitizenName, currentProductionUnit.owner.pioneerCitizenNumber);

                List<EmailAddress> tos = new List<EmailAddress>
            {
                  new EmailAddress(currentProductionUnit.owner.contactMail, pioneerName)
            };

                var subject = string.Empty;
                var htmlContent = string.Empty;

                if (currentProductionUnit.owner != null && currentProductionUnit.owner.language != null)
                {
                    switch (currentProductionUnit.owner.language.description)
                    {
                        case "fr":
                            subject = "[myfood] Votre serre est connectée";
                            htmlContent = string.Format("Bonjour {0}, </br></br>" +
                                                        "Votre serre #{1} est actuellement connectée à notre infrastructure.</br>" +
                                                        "Vos données sont synchronisées sur l'application HUB.</br></br>" +
                                                        "Bien à vous,", currentProductionUnit.owner.pioneerCitizenName, currentProductionUnit.owner.pioneerCitizenNumber);
                            break;
                        default:
                            subject = "[myfood] Your greenhouse is connected";
                            htmlContent = string.Format("Hi {0}, </br></br>" +
                                                        "Your smart greenhouse #{1} is currently connected to our infrastructure.</br>" +
                                                        "Your data is synchronized on the HUB application.</br></br>" +
                                                        "Have a nice day,", currentProductionUnit.owner.pioneerCitizenName, currentProductionUnit.owner.pioneerCitizenNumber);
                            break;
                    }
                }
                else
                {
                    subject = "[myfood] Your greenhouse is connected";
                    htmlContent = string.Format("Hi {0}, </br></br>" +
                                                "Your smart greenhouse #{1} is currently connected to our infrastructure.</br>" +
                                                "Your data is synchronized on the HUB application.</br></br>" +
                                                "Have a nice day,", currentProductionUnit.owner.pioneerCitizenName, currentProductionUnit.owner.pioneerCitizenNumber);
                }

                var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, "", htmlContent, false);
                //msg.AddCc("agro@myfood.eu");
                var response = client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                dbLog.Logs.Add(Log.CreateErrorLog(String.Format("Error with Mail Notification"), ex));
                dbLog.SaveChanges();
            }

        }

        public static void PioneerUnitIssueMessage(ProductionUnit currentProductionUnit, string note, string details)
        {
            var dbLog = new ApplicationDbContext();

            try
            {
                var client = new SendGridClient(MailSendGridAPIKey);
                var from = new EmailAddress("hub@myfood.eu", "Myfood Hub Bot");

                var pioneerName = string.Format("{0} #{1}", currentProductionUnit.owner.pioneerCitizenName, currentProductionUnit.owner.pioneerCitizenNumber);

                List<EmailAddress> tos = new List<EmailAddress>
            {
                  new EmailAddress("support@myfood.eu", pioneerName)
            };

                var subject = string.Empty;
                var htmlContent = string.Empty;

                if (currentProductionUnit.owner != null && currentProductionUnit.owner.language != null)
                {
                    switch (currentProductionUnit.owner.language.description)
                    {
                        case "fr":
                            subject = string.Format("[myfood] Incident enregistré chez {0} #{1}", currentProductionUnit.owner.pioneerCitizenName, currentProductionUnit.owner.pioneerCitizenNumber);
                            htmlContent = string.Format("Bonjour, </br></br>" +
                                                        "La serre {0} #{1} vient d'enregistrer un incident critique.</br>" +
                                                        "Detail : {2} {3}</br></br>" +
                                                        "Bien à vous,", currentProductionUnit.owner.pioneerCitizenName, currentProductionUnit.owner.pioneerCitizenNumber, note, details);
                            break;
                        default:
                            subject = string.Format("[myfood] Issue recorded at {0} #{1}", currentProductionUnit.owner.pioneerCitizenName, currentProductionUnit.owner.pioneerCitizenNumber);
                            htmlContent = string.Format("Hi {0}, </br></br>" +
                                                        "The greenhouse {0} #{1} has met a critical issue.</br>" +
                                                        "Detail : {2} {3}</br></br>" +
                                                        "Have a nice day,", currentProductionUnit.owner.pioneerCitizenName, currentProductionUnit.owner.pioneerCitizenNumber, note, details);
                            break;
                    }
                }
                else
                {
                    subject = string.Format("[myfood] Issue recorded at {0} #{1}", currentProductionUnit.owner.pioneerCitizenName, currentProductionUnit.owner.pioneerCitizenNumber);
                    htmlContent = string.Format("Hi {0}, </br></br>" +
                                                "The greenhouse {0} #{1} has met a critical issue.</br>" +
                                                "Detail : {2} {3}</br></br>" +
                                                "Have a nice day,", currentProductionUnit.owner.pioneerCitizenName, currentProductionUnit.owner.pioneerCitizenNumber, note, details);
                }

                var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, "", htmlContent, false);
                msg.AddCc("agro@myfood.eu");
                var response = client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                dbLog.Logs.Add(Log.CreateErrorLog(String.Format("Error with Mail Notification"), ex));
                dbLog.SaveChanges();
            }

        }

        public static void ResetPasswordMessage(string to, string callbackUrl)
        {
            var client = new SendGridClient(MailSendGridAPIKey);
            string subject = "MyFood password recovery";
            string plainText = "";
            string htmlText = string.Format("To reset your password follow the <a href=\"{0}\">link</a>", callbackUrl);
            var msg = MailHelper.CreateSingleEmail(new EmailAddress("support@myfood.eu", "Myfood Hub Support"), new EmailAddress(to), subject,
                plainText, htmlText);
            var response = client.SendEmailAsync(msg);
        }
    }
}