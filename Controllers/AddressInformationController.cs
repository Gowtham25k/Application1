using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace CRM.Controllers
{
    public class AddressInformationController : Controller
    {
        //
        // GET: /AddressInformation/

        public ActionResult Index()
        {
            XmlDocument doc;
            XmlNodeList xmlnode;
            string Header = "";
            string Footer = "";
            string Filepath = "";
            string Title = "";
            FileStream fs = null;
            try
            {
                doc = new XmlDocument();
                Filepath = Server.MapPath("~/App_Data/e911AddressInformation.xml");
                fs = new FileStream(Filepath, FileMode.Open, FileAccess.Read);
                doc.Load(fs);
                xmlnode = doc.GetElementsByTagName("PageInformation");
                Header = xmlnode[0].ChildNodes.Item(1).InnerText.Trim();
                Title = xmlnode[0].ChildNodes.Item(0).InnerText.Trim();
                Footer = xmlnode[0].ChildNodes.Item(2).InnerText.Trim();
                ViewBag.ProductHeader = Header;
                ViewBag.ProductFooter = Footer;
                ViewBag.Producttitle = Title;
            }
            catch(Exception ex)
            {
                CRMLogger.WriteException("PageInformation", this.ControllerContext, ex);
            }
            finally
            {
                doc = null;
                Header = string.Empty;
                Footer = string.Empty;
                if (fs!=null)
                    fs.Dispose();
                Filepath = string.Empty;
            }
            return View();
        }
    }
}
