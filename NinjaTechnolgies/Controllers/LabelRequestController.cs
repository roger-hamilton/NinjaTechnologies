using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NinjaTechnolgies.FedExShipServiceWebReference;
using System.Web.Services.Protocols;
using NinjaTechnolgies.Models;
using System.Collections.Specialized;

namespace NinjaTechnolgies.Controllers
{
    public class LabelRequestController : Controller
    {
        // GET: LabelRequest
        public ActionResult Index(string Ref)
        {

            HttpCookie LoginCookie = HttpContext.Request.Cookies.Get("MethodLogin");

            if (LoginCookie == null)
                return RedirectToAction("Login", new { returnURL = Request.Url.PathAndQuery });


            var contact = MethodData.GetContact(Ref,LoginCookie["username"] ,LoginCookie["password"]);
            if(contact==null)
            {
                ViewBag.Errors = "Error retrieving Contact Information for RecordID: "+Ref+"<br>";
                return View("Error");
            }
            var request = FedExLabelGenerator.CreateRequest(contact);
            ShipService service = new ShipService();
            string errors = "";
            try
            {
                ProcessShipmentReply reply = service.processShipment(request);
                if(reply.HighestSeverity == NotificationSeverityType.ERROR)
                {

                    foreach (var note in reply.Notifications)
                        errors += note.Message + "<br>";

                    

                    return View();
                }


                //test adding to document library
                var FileName = contact.CompanyName.Replace(" ","").Replace(",","").Replace(".","");
                FileName = FileName + "Label(" + DateTime.Now.ToString().Replace("/", "-").Replace(":","-") + ")";
                var ImageData = reply.CompletedShipmentDetail.CompletedPackageDetails[0].Label.Parts[0].Image;
                var SavedAs = MethodData.WritePDFToDocumentStore(LoginCookie["username"], LoginCookie["password"], FileName, ImageData);
                if (SavedAs != null)
                {

                    var DataURI = "data:application/pdf;base64," + Convert.ToBase64String(ImageData);
                    ViewBag.PDFDataURI = DataURI;
                    ViewBag.FileName = SavedAs;
                    return View();
                }

                return File(reply.CompletedShipmentDetail.CompletedPackageDetails[0].Label.Parts[0].Image, "application/pdf");
            }
            catch (SoapException e)
            {

            }
            catch (Exception e)
            {

            }

            ViewBag.Errors = errors;
            return View("Error");
            
        }

        public ActionResult Test()
        {
            HttpCookie LoginCookie = HttpContext.Request.Cookies.Get("MethodLogin");

            MethodData.Test(LoginCookie["username"], LoginCookie["password"]);
            return View("Error");
        }
        [HttpGet]
        public ActionResult LogIn(string returnURL)
        {
            TempData["ReturnURL"] = returnURL;
            return View();
        }

        [HttpPost]
        public ActionResult LogIn(string username, string password)
        {
            //test credintials
            var valid = MethodData.ValidLogin(username, password);
            if (valid)
            {
                var LoginCookie = new HttpCookie("MethodLogin");
                LoginCookie.Values.Add("username", username);
                LoginCookie.Values.Add("password", password);
                HttpContext.Response.Cookies.Add(LoginCookie);
                if (TempData["ReturnURL"] != null)
                {
                    return Redirect(TempData["ReturnURL"] as string);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
                
            }
            else{
                TempData["ReturnURL"] = TempData["ReturnURL"];
                ViewBag.Message = "Invalid Username or Password";
                return View();
            }
            
        }

        
    }
}