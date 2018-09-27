using LoginRegistration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace LoginRegistration.Controllers
{
    public class RegistrationController : Controller
    {
        // GET: Registration
        shawonDbEntities Db = new shawonDbEntities();
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult Savedata(Registration reg)
        {
            reg.IsValid = false;
            Db.Registrations.Add(reg);
            Db.SaveChanges();
            BuildEmailTemplate(reg.Id);
            return Json("Registration SuccessFull", JsonRequestBehavior.AllowGet);
        }
        public void BuildEmailTemplate(int regID)
        {
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemPlate/") + "Text" + ".cshtml");
            var RegInfo = Db.Registrations.Where(x => x.Id == regID).FirstOrDefault();
            var url = "http://localhost:64992/" + "Register/Confirm?regId=" + regID;
            body = body.Replace("@ViewBag.ConfirmationLink",url);
            body = body.ToString();
            BuildEmailTemplate("your Account is Successfully Created", body, RegInfo.Email);
        }
        public static void BuildEmailTemplate(string subjectText, string bodyText, string sendto)
        {
            string from, bcc, cc, to, subject, body;
            from = "shawonaiub96@gmail.com";
            to = sendto.Trim();
            bcc = "";
            cc = "";
            subject = subjectText;
            StringBuilder sb = new StringBuilder();
            sb.Append(bodyText);
            body = sb.ToString();
            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress(to));
            if (!string.IsNullOrEmpty(bcc))
            {
                msg.Bcc.Add(new MailAddress(bcc));
            }
            if (!string.IsNullOrEmpty(cc))
            {
                msg.CC.Add(new MailAddress(cc));
            }
            msg.Subject = subject;
            msg.Body = body;
            msg.IsBodyHtml = true;
            sendEmail(msg);
        }
        public static void sendEmail(MailMessage msg)
        {
            SmtpClient client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new System.Net.NetworkCredential("shawonaiub96@gmail.com", "shawon96");
            try
            {
                client.Send(msg);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult Confirm(int regId)
        {
            ViewBag.regId = regId;

            return View();
        }
        public JsonResult RegistrationConfirm(int regID)
        {
            Registration registration = Db.Registrations.Where(x => x.Id == regID).FirstOrDefault();
            registration.IsValid = true;
            Db.SaveChanges();
            var msg = "Your Email is Verified";
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        public JsonResult checkValidUser(Registration Reg)
        {
            string Result = "Fail";
            var dataItem = Db.Registrations.Where(x => x.Email == Reg.Email && x.Password==Reg.Password).FirstOrDefault();
            if (dataItem != null)
            {
                Session["UserId"] = dataItem.Id.ToString();
                Session["UserName"] = dataItem.UserName.ToString();
                Result = "Success";
            }
            return Json(Result, JsonRequestBehavior.AllowGet);

        }
        public ActionResult afterlogin()
        {
            if (Session["UserId"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index");
        }
    }
}