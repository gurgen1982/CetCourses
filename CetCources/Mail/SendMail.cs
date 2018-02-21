using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace CetCources
{
    public class Mail
    {
        public static string AdminInfo { get; set; }
        public static void Send(string subject, string body, string to, string toName = "", bool sendToAdmin = true)
        {
            try
            {
                var smtpSection = (System.Net.Configuration.SmtpSection)System.Configuration.ConfigurationManager.GetSection("system.net/mailSettings/smtp");

                string userName = smtpSection.Network.UserName;
                string fromPassword = smtpSection.Network.Password;
                int port = smtpSection.Network.Port;
                bool enableSsl = Convert.ToBoolean(smtpSection.Network.EnableSsl);
                string smtpServerName = smtpSection.Network.Host;

                var fromAddress = new MailAddress(userName, "CET Registration system");
                var toAddress = new MailAddress(to, toName);


                var smtp = new SmtpClient
                {
                    Host = smtpServerName,
                    Port = port,
                    EnableSsl = enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(fromAddress.Address, fromPassword)
                };

                Func<MailAddress, bool, Task> send = async (sendToAddress, isAdmin) =>
                  {
                      try
                      {
                          using (var message = new MailMessage(fromAddress, sendToAddress)
                          {
                              Subject = subject + (isAdmin ? " (Copy of mail for Admin)" : ""),
                              Body = body + (isAdmin ? $"<br /><hr /> Original mail for <b> {toName}</b> contact info: {AdminInfo}, {to}" : ""),
                              IsBodyHtml = true,
                              HeadersEncoding = Encoding.UTF8,
                              SubjectEncoding = Encoding.UTF8,
                              BodyEncoding = Encoding.UTF8,
                          })
                          {
                              await smtp.SendMailAsync(message);
                          }
                      }
                      catch (Exception ex) { }
                  };

                Task t = new Task(async () =>
                {
                    await send(toAddress, false);
                    if (sendToAdmin)
                    {
                        var db = new Database.dbEntities();
                        var admins = from roles in db.AspNetUserRoles
                                     join users in db.AspNetUsers on roles.UserId equals users.Id
                                     where roles.RoleId == "1"
                                     select users;

                        foreach (var admin in admins)
                        {
                            var toAdmin = new MailAddress(admin.Email, admin.FullName);
                            await send(toAdmin, true);
                        }
                    }
                });
                t.Start();
            }
            catch { }
        }
    }
}