using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using CRM.Models;
using Newtonsoft.Json;
using Service;
using ServiceCRM;

namespace CRM.Controllers
{
    [ValidateState]
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class TickettingController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();
        //TicketingTool objTicket = new TicketingTool(SettingsCRM.crmticketUrl);
        public ActionResult Ticket()
        {
            Ticketing objTicket = new Ticketing();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - Ticket Start");
                objTicket.Fileformat = clientSetting.mvnoSettings.ticketAttachFileformat;
                objTicket.Filesize = clientSetting.mvnoSettings.ticketAttachFileSize;
                objTicket.DateFormat = clientSetting.mvnoSettings.dateTimeFormat.ToLower().Replace("yyyy", "yy");
                objTicket.Sitecode = clientSetting.countryCode;
                objTicket.FileuploadCount = Convert.ToInt32(clientSetting.mvnoSettings.ticketFileuploadmaxcount);
                // 4919
                objTicket.strDropdown = Utility.GetDropdownMasterFromDB("", "1", "TblCountry");
                Session["ServerTime"] = Convert.ToString(System.DateTime.Now.Hour) + "|" + Convert.ToString(System.DateTime.Now.Minute) + "|" + Convert.ToString(System.DateTime.Now.Second);
                TempData.Keep("languageId");
                TempData.Keep("Language");
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - Ticket End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            return PartialView(objTicket);
        }
        public ActionResult TicketPage()
        {
            Ticketing objTicket = new Ticketing();
            GetLanguageRequest req = new GetLanguageRequest();
            GetLanguageResponse objres = new GetLanguageResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - TicketPage Start");
                objTicket.Fileformat = clientSetting.mvnoSettings.ticketAttachFileformat;
                objTicket.Filesize = clientSetting.mvnoSettings.ticketAttachFileSize;
                objTicket.DateFormat = clientSetting.mvnoSettings.dateTimeFormat.ToLower().Replace("yyyy", "yy");
                objTicket.Sitecode = clientSetting.countryCode;
                objTicket.FileuploadCount = Convert.ToInt32(clientSetting.mvnoSettings.ticketFileuploadmaxcount);
                // 4919
                objTicket.strDropdown = Utility.GetDropdownMasterFromDB("", "1", "TblCountry");
                Session["ServerTime"] = Convert.ToString(System.DateTime.Now.Hour) + "|" + Convert.ToString(System.DateTime.Now.Minute) + "|" + Convert.ToString(System.DateTime.Now.Second);


                if (clientSetting.mvnoSettings.TicketingToolSMSNotifyEnhance.ToLower() == "on")
                {
                    req.CountryCode = clientSetting.countryCode;
                    req.BrandCode = clientSetting.brandCode;
                    req.LanguageCode = clientSetting.langCode;

                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    objres = serviceCRM.CRMGetLanguageforTicketting(req);

                    TempData["languageId"] = clientSetting.mvnoSettings.DefaultLangIDforTicketting;
                    TempData["Language"] = objres.Language;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - TicketPage End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                req = null;
                objres = null;
                serviceCRM = null;
            }
            return View("Ticket",objTicket);
        }
        public JsonResult GetProduct()
        {
            CRMBase req = new CRMBase();
            ProductListResponse res;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetProduct Start");
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                //if (HttpRuntime.Cache["GetProduct"] != null)
                //{
                //    res = (ProductListResponse)HttpRuntime.Cache["GetProduct"];
                //}
                //else
                //{
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                res = serviceCRM.CRMProductList(req);

                //res = objTicket.GetProduct(Convert.ToString(clientSetting.mvnoSettings.enableLinkedDropdown.ToUpper() == "TRUE" ? clientSetting.mvnoSettings.tickettingSiteCode.ToUpper() : null));
                //res = objTicket.GetProduct( clientSetting.mvnoSettings.tickettingSiteCode.ToUpper());
                HttpRuntime.Cache.Insert("GetProduct", res, null, DateTime.Now.AddSeconds(30), TimeSpan.Zero);
                HttpRuntime.Cache.Insert("GetProduct", res);
                //}
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, res.productList.ResponseDesc);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetProduct End");
                return Json(res.productList.ProductList);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                req = null;
                serviceCRM = null;
            }
            return null;
        }

        public ActionResult GetCountry()
        {
            //try
            //{
            //    CRMLogger.WriteMessage(Session["UserName"].ToString(), this.ControllerContext, "Get Country start");
            //    GetCountryListres res;
            //    if (HttpRuntime.Cache["GetCountry"] != null)
            //    {
            //        res = (GetCountryListres)HttpRuntime.Cache["GetCountry"];
            //    }
            //    else
            //    {
            //        res = objTicket.GetCountryList();
            //        HttpRuntime.Cache.Insert("GetCountry", res);
            //    }
            //    CRMLogger.WriteMessage(Session["UserName"].ToString(), this.ControllerContext, res.ResponseDesc);
            //    return Json(res.CountryList);
            //}
            //catch (Exception ex)
            //{
            //    CRMLogger.WriteException(Session["UserName"].ToString(), this.ControllerContext, ex);
            //}
            return null;
        }

        public JsonResult GetJonCatagory(MobileCategoryRequest req)
        {
            MobileCategoryResponse res;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetJonCatagory Start");
                req.SiteCode = clientSetting.mvnoSettings.tickettingSiteCode;
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                if (clientSetting.mvnoSettings.enableLinkedDropdown.ToUpper() == "FALSE")
                { req.ProductID = null; }

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                res = serviceCRM.CRMMobileCategory(req);

                //   MobileCategoryResponse res = objTicket.CRMMobileCategory(req);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, res.responseDetails.ResponseDesc);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetJonCatagory End");
                return Json(res.mobileCategory.MobileCategoryList);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return null;
        }

        public JsonResult GetJonSubCatagory(MobileSubCategoryRequest req)
        {
            MobileSubCategoryResponse Objres;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetJonSubCatagory Start");
                req.SiteCode = clientSetting.mvnoSettings.tickettingSiteCode;
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                if (clientSetting.mvnoSettings.enableLinkedDropdown.ToUpper() == "FALSE")
                {
                    req.ProductID = null;
                }

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objres = serviceCRM.CRMMobileSubCategory(req);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, Objres.mobileSubCategory.ResponseDesc);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetJonSubCatagory End");
                return Json(Objres.mobileSubCategory.MobileSubCategoryList);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return null;
        }

        public JsonResult GetAction(string x = "0")
        {
            MobileActionGroupRequest req = new MobileActionGroupRequest();
            MobileActionGroupResponse res;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetAction Start");
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Start get Action");
                //if (HttpRuntime.Cache["GetAction" + x] != null)
                //{
                //    res = (MobileActionGroupResponse)HttpRuntime.Cache["GetAction" + x];
                //}
                //else
                //{
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    req.GroupType = x;
                    res = serviceCRM.CRMMobileActionGroup(req);

                //    HttpRuntime.Cache.Insert("GetAction" + x, res);
                //}
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, res.mobileAction.ResponseDesc);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetAction End");
                return Json(res.mobileAction.MobileActionGroupList);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                req = null;
                serviceCRM = null;
            }
            return null;
        }

        public JsonResult GetDesignation()
        {
            MobileDestinationZoneResponse res;
            CRMBase req = new CRMBase();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetDesignation Start");
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                //if (HttpRuntime.Cache["GetDesignation"] != null)
                //{
                //    res = (MobileDestinationZoneResponse)HttpRuntime.Cache["GetDesignation"];
                //}
                //else
                //{
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                res = serviceCRM.CRMMobileDestinationZone(req);

                // HttpRuntime.Cache.Insert("GetDesignation", res);
                //}
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, res.mobileDest.ResponseDesc);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetDesignation End");
                return Json(res.mobileDest.DestinationZoneList);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                req = null;
                serviceCRM = null;
            }
            return null;
        }

        public JsonResult GetPriority()
        {
            CRMBase req = new CRMBase();
            PriorityListResponse res;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetPriority Start");
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                //if (HttpRuntime.Cache["GetPriority"] != null)
                //{
                //    res = (PriorityListResponse)HttpRuntime.Cache["GetPriority"];
                //}
                //else
                //{
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    res = serviceCRM.CRMPriorityList(req);
                //    HttpRuntime.Cache.Insert("GetPriority", res);
                //}
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, res.priorityList.ResponseDesc);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetPriority End");
                return Json(res.priorityList.PriorityList);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                req = null;
                serviceCRM = null;
            }
            return null;
        }

        public JsonResult GetcustomerFeedback()
        {
            CRMBase req = new CRMBase();
            CustomerFeedbackResponse res;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetcustomerFeedback Start");
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                //if (HttpRuntime.Cache["GetcustomerFeedback"] != null)
                //{
                //    res = (CustomerFeedbackResponse)HttpRuntime.Cache["GetcustomerFeedback"];
                //}
                //else
                //{
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    res = serviceCRM.CRMCustomerFeedback(req);
                //    HttpRuntime.Cache.Insert("GetcustomerFeedback", res);
                //}
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, res.customerFeedback.ResponseDesc);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetcustomerFeedback End");
                return Json(res.customerFeedback.GetCustFeedback);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                req = null;
                serviceCRM = null;
            }
            return null;
        }

        public JsonResult GetMobileTicket(FetchTicketDetailsRequest req)
        {
            FetchTicketDetailsResponse objres;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetMobileTicket Start");
                req.StatusID = req.StatusID == 0 ? -1 : req.StatusID;
                if (req.Mode != "HomePanel" || (req.Mode == "HomePanel" && string.IsNullOrEmpty(req.TicketID)))
                {
                    req.MobileNumber = Convert.ToString(Session["MobileNumber"]);
                }
                else { req.MobileNumber = ""; }

                req.RecordCount = !string.IsNullOrEmpty(clientSetting.mvnoSettings.ticketRecordCount) ? Convert.ToInt32(clientSetting.mvnoSettings.ticketRecordCount) : 0;
                //FetchTicketDetailsResponse res = objTicket.GetMobileTicket(req);
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.GetMobileTicket(req);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objres.responseDetails.ResponseDesc);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetMobileTicket End");
                //Assgin designation
                return Json(objres.mobileTicket.TicketCollection);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return null;
        }
        /*Ticket ID mandatory/optional configuration*/
        public JsonResult TicketSettings(string TicketID)
        {
            // case 1: TicketID is Optional without validation
            // case 2: TicketID is Mandatory without validation
            // case 3: TicketID is Optional with validation
            // case 4: TicketID is Mandatory with validation
            CRMResponse objRes = new CRMResponse();
            int ticketidconfig = 0;
            int retobj = 0;
            FetchTicketDetailsRequest req = new FetchTicketDetailsRequest();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - TicketSettings Start");
                ticketidconfig = !string.IsNullOrEmpty(clientSetting.mvnoSettings.ticketIdConfig) ? Convert.ToInt32(clientSetting.mvnoSettings.ticketIdConfig) : 1;
                switch (ticketidconfig)
                {
                    case 1:
                        objRes.ResponseCode = "0";
                        objRes.ResponseDesc = Resources.HomeResources.Success;
                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.ResponseDesc);
                        break;

                    case 2:
                        if (TicketID == string.Empty)
                        {
                            objRes.ResponseCode = "200";
                            objRes.ResponseDesc = Resources.HomeResources.TicketIDEmpty;
                        }
                        else
                        {
                            objRes.ResponseCode = "0";
                            objRes.ResponseDesc = Resources.HomeResources.Success;
                        }
                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.ResponseDesc);
                        break;
                    case 3:
                        if (TicketID != string.Empty)
                        {
                            req.StatusID = -1;
                            req.RecordCount = 1;
                            req.MobileNumber = Convert.ToString(Session["MobileNumber"]);
                            req.TicketID = TicketID;
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - TicketSettings Validating Mobile Ticket Start");
                            retobj = ValidateMobileTicket(TicketID);
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - TicketSettings Validating Mobile Ticket End");
                            if (retobj > 0)
                            {
                                objRes.ResponseCode = "0";
                                objRes.ResponseDesc = Resources.HomeResources.Success;
                            }
                            else
                            {
                                objRes.ResponseCode = "310";
                                objRes.ResponseDesc = Resources.HomeResources.InvalidTicketID;
                            }
                        }
                        else
                        {
                            objRes.ResponseCode = "0";
                            objRes.ResponseDesc = Resources.HomeResources.Success;
                        }
                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.ResponseDesc);
                        break;
                    case 4:
                        if (TicketID != string.Empty)
                        {
                            req.StatusID = -1;
                            req.RecordCount = 1;
                            req.MobileNumber = Convert.ToString(Session["MobileNumber"]);
                            req.TicketID = TicketID;
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - TicketSettings Validating Mobile Ticket Start");
                            retobj = ValidateMobileTicket(TicketID);
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - TicketSettings Validating Mobile Ticket End");
                            if (retobj > 0)
                            {
                                objRes.ResponseCode = "0";
                                objRes.ResponseDesc = Resources.HomeResources.Success;
                            }
                            else
                            {
                                objRes.ResponseCode = "310";
                                objRes.ResponseDesc = Resources.HomeResources.InvalidTicketID;
                            }
                        }
                        else
                        {
                            objRes.ResponseCode = "200";
                            objRes.ResponseDesc = Resources.HomeResources.TicketIDEmpty;
                        }
                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.ResponseDesc);
                        break;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - TicketSettings End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                ticketidconfig = 0;
                retobj = 0;
                req=null;
            }
            return Json(objRes);
        }

        #region FRR 4871 CUN Ticket
        public JsonResult CreateCUNTicket(string CunTicketDetails, string country)//4962
        {
       
            ServiceInvokeCRM serviceCRM;
            CunTicketResponse res = new CunTicketResponse();
            CUNTicketDetailsRequest req = JsonConvert.DeserializeObject<CUNTicketDetailsRequest>(CunTicketDetails);
            string keyframe = "?Keyframe=";
            string returnurl = string.Empty;
            try
            {

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetcustomerFeedback Start");
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                if (country == "CHL")
                {
                    req.Username = clientSetting.preSettings.EnableTickettingToolForCHL.Split('|')[4].ToString();
                    req.password = clientSetting.preSettings.EnableTickettingToolForCHL.Split('|')[5].ToString();
                    req.operatorname = clientSetting.preSettings.EnableTickettingToolForCHL.Split('|')[6].ToString();
                    req.operatorresponse = clientSetting.preSettings.EnableTickettingToolForCHL.Split('|')[7].ToString();
                }
                else
                {
                req.Username = clientSetting.preSettings.CUNTicketCredentials.Split('|')[0].ToString();
                req.password = clientSetting.preSettings.CUNTicketCredentials.Split('|')[1].ToString();
                req.operatorname = clientSetting.preSettings.CUNTicketCredentials.Split('|')[2].ToString();
                req.operatorresponse = clientSetting.preSettings.CUNTicketCredentials.Split('|')[3].ToString();
                }
                if (clientSetting.mvnoSettings.enableTicketingHome == "0" && country == "CHL")
                {
                    req.MobileNumber = "";
                }
                else
                {
                    req.MobileNumber = !string.IsNullOrEmpty(req.MobileNumber) ? req.MobileNumber : Convert.ToString(Session["MobileNumber"]);
                }

                if (string.IsNullOrEmpty(req.MobileNumber) && country == "CHL")
                {
                    Session["istickethome"] = "1";
                }
                else
                {
                    Session["istickethome"] = "1";
                }

                //6459
                if (clientSetting.preSettings.EnableTickettingToolForCHL.Split('|')[0].ToUpper()=="TRUE" && country == "CHL" )
                {
                    if (req.CountryCode == "CHL")
                    {
                        returnurl = Request.UrlReferrer.ToString().TrimEnd('?') + keyframe + Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + Convert.ToString(Session["MobileNumber"]) + "|" + "CUNTICKET_CHL"+ "|"+ "CHL_Redirect";
                    }
                    else

                    {
                        returnurl = Request.UrlReferrer.ToString().TrimEnd('?') + keyframe + Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + Convert.ToString(Session["MobileNumber"]) + "|" + "CUNTICKET" + "|" + "CHL_Redirect";
                        
                    }
                }
                else
                {
                returnurl = Request.UrlReferrer.ToString().TrimEnd('?') + keyframe + Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + Convert.ToString(Session["MobileNumber"]) + "|" + "CUNTICKET" ;
                }
                Session["CUNTicketDetails"] = returnurl;
                req.returnurl = returnurl;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                res = serviceCRM.CreateCUNTicket(req);
         
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetcustomerFeedback End");
                
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                req = null;
                serviceCRM = null;
            }
            return new JsonResult() { Data = res, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet }; ;
        }


        #endregion

        public JsonResult TicketSettingsPP(string ticketID)
        {
            // 1 denotes TicketID is optional without validation, 
            // 2 TicketID is Mandatory without validation, 
            // 3 TicketID is optional with validation, 
            // 4 TicketID is Mandatory with validation
            CRMResponse objRes = new CRMResponse();
            int ticketidconfig = 0;
            int flag = 0;
            FetchTicketDetailsRequest req = new FetchTicketDetailsRequest();
            int retobj = 0;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - TicketSettingsPP Start");
                ticketidconfig = !string.IsNullOrEmpty(clientSetting.mvnoSettings.ticketIdConfig) ? Convert.ToInt32(clientSetting.mvnoSettings.ticketIdConfig) : 5;
                if (ticketidconfig == 4 || ticketidconfig == 3)
                    flag = 1;
                else flag = 2;

                switch (flag)
                {
                    case 1:
                        if (ticketID != string.Empty)
                        {
                            req.StatusID = -1;
                            req.RecordCount = 1;
                            req.MobileNumber = Convert.ToString(Session["MobileNumber"]);
                            req.TicketID = ticketID;
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - TicketSettingsPP Validating Mobile Ticket Start");
                            retobj = ValidateMobileTicket(ticketID);
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - TicketSettingsPP Validating Mobile Ticket End");
                            if (retobj > 0)
                            {
                                objRes.ResponseCode = "0";
                                objRes.ResponseDesc = Resources.HomeResources.Success;
                            }
                            else
                            {
                                objRes.ResponseCode = "310";
                                objRes.ResponseDesc = Resources.HomeResources.InvalidTicketID;
                            }
                        }
                        else
                        {
                            objRes.ResponseCode = "200";
                            objRes.ResponseDesc = Resources.HomeResources.TicketIDEmpty;
                        }
                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.ResponseDesc);
                        break;

                    case 2:
                        objRes.ResponseCode = "0";
                        objRes.ResponseDesc = Resources.HomeResources.Success;
                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.ResponseDesc);
                        break;
                    default:
                        objRes.ResponseCode = "310";
                        objRes.ResponseDesc = Resources.HomeResources.InvalidTicketID;
                        break;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - TicketSettingsPP End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                ticketidconfig = 0;
                flag = 0;
                retobj = 0;
                req = null;
            }
            return Json(objRes);
        }

        public int ValidateMobileTicket(string ticketid)
        {
            FetchTicketDetailsRequest req = new FetchTicketDetailsRequest();
            ServiceInvokeCRM serviceCRM;
            FetchTicketDetailsResponse objres;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - ValidateMobileTicket Start");
                req.StatusID = -1;
                req.TicketID = ticketid;
                req.MobileNumber = Convert.ToString(Session["MobileNumber"]);
                req.RecordCount = !string.IsNullOrEmpty(clientSetting.mvnoSettings.ticketRecordCount) ? Convert.ToInt32(clientSetting.mvnoSettings.ticketRecordCount) : 0;
                //FetchTicketDetailsResponse res = objTicket.GetMobileTicket(req);
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.GetMobileTicket(req);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objres.responseDetails.ResponseDesc);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - ValidateMobileTicket End");
                //Assgin designation
                if (objres.mobileTicket.TicketCollection.Length > 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                req = null;
                serviceCRM=null;
                objres=null;
            }
            return 0;
        }

        public JsonResult LoadMobileTicketstatus(Int16 statusID)
        {
            FetchTicketDetailsRequest req = new FetchTicketDetailsRequest();
            FetchTicketDetailsResponse res;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - LoadMobileTicketstatus Start");
                //ServiceCRM.TT_Service.GetMobileTicketsRequest req = new GetMobileTicketsRequest();
                req.MobileNumber = Convert.ToString(Session["MobileNumber"]);
                req.StatusID = statusID;
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                req.RecordCount = !string.IsNullOrEmpty(clientSetting.mvnoSettings.ticketRecordCount) ? Convert.ToInt32(clientSetting.mvnoSettings.ticketRecordCount) : 0;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                res = serviceCRM.GetMobileTicket(req);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, res.mobileTicket.ResponseDesc);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - LoadMobileTicketstatus End");
                return Json(res.mobileTicket.TicketCollection);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                req = null;
                serviceCRM = null;
            }
            return null;
        }

        public JsonResult InsertMobileTicket(CreateTicketRequest req)
        {
            List<FileUpload> oblistFile = new List<FileUpload>();
            FileUpload obFile = null;
            string filepath = string.Empty;
            string[] fileEntries;
            CreateTicketResponse res = new CreateTicketResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - InsertMobileTicket Start");
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                req.SiteCode = clientSetting.mvnoSettings.tickettingSiteCode;
                req.MobileNumber = Convert.ToString(Session["MobileNumber"]);
                req.AnaTelID = Convert.ToString(clientSetting.countryCode.ToUpper() == "BRA" ? Convert.ToString(Session["AnatelID"]) : "");
                #region Commented Line
                //if (Session["FullName"] != null ? Convert.ToString(Session["FullName"]) != string.Empty : false)
                //{
                //    string strFullName = Convert.ToString(Session["FullName"]);
                //    string[] splitFullname = strFullName.Split(' ');
                //    if (splitFullname.Count() > 1)
                //    {
                //        req.Firstname = splitFullname[0].ToString();
                //        req.Lastname = splitFullname[1].ToString();
                //    }
                //    else if (splitFullname.Count() > 0)
                //    {
                //        req.Firstname = splitFullname[0].ToString();
                //    }
                //}
                // req.Email = Session["EMail"] != null ? Session["EMail"].ToString() : string.Empty;
                #endregion
                req.EstimatedClosureDate = GetDateconvertion(req.EstimatedClosureDate, "MM/DD/YYYY", true);
                if (!string.IsNullOrEmpty(req.EMailReceivedDate))
                {
                    req.EMailReceivedDate = GetDateconvertion(req.EMailReceivedDate, "MM/DD/YYYY", true);
                }
                filepath = HostingEnvironment.MapPath(@"\App_Data\" + Convert.ToString(Session["MobileNumber"]));
                if (Directory.Exists(filepath))
                {
                    fileEntries = Directory.GetFiles(filepath);
                    foreach (string fileName in fileEntries)
                    {
                        obFile = new FileUpload();
                        obFile.FileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                        FileStream stream = System.IO.File.OpenRead(fileName);
                        byte[] fileBytes = new byte[stream.Length];
                        stream.Read(fileBytes, 0, fileBytes.Length);
                        obFile.FileInBytes = Convert.ToBase64String(fileBytes);
                        stream.Close();
                        oblistFile.Add(obFile);
                    }
                }
                req.UploadFile = oblistFile.ToArray();
                if (clientSetting.mvnoSettings.enableDestination.ToUpper() == "FALSE")
                {
                    req.DestinationName = clientSetting.mvnoSettings.destinationValue;
                }
                if (clientSetting.mvnoSettings.enableAssignto.ToUpper() == "FALSE")
                {
                    req.ActionGroup = Convert.ToInt32(clientSetting.mvnoSettings.assigntoValue);
                }
                if (clientSetting.mvnoSettings.enableFCR.ToUpper() == "FALSE")
                {
                    req.FirstCallResolution = clientSetting.mvnoSettings.FCRValue;
                }
                if (clientSetting.mvnoSettings.enableCustomerFeedback.ToUpper() == "FALSE")
                {
                    req.CustomerFeedback = Convert.ToInt32(clientSetting.mvnoSettings.customerFeedbackValue);
                }
                if (clientSetting.mvnoSettings.disableProductName.ToUpper() == "TRUE")
                {
                    req.ProductID = Convert.ToInt32(clientSetting.mvnoSettings.tickettingProductName.Split('|')[1]);
                }
                if (clientSetting.mvnoSettings.enableResolvebyDate.ToUpper() == "FALSE")
                {
                    req.EstimatedClosureDate = clientSetting.mvnoSettings.resolvebyDateValue;
                    //mmddyy always
                }

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                req.UserName = Convert.ToString(Session["UserName"]);
                res = serviceCRM.CRMCreateTicket(req);
                Session["ServerTime"] = Convert.ToString(System.DateTime.Now.Hour) + "|" + Convert.ToString(System.DateTime.Now.Minute) + "|" + Convert.ToString(System.DateTime.Now.Second);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, res);

                if (Directory.Exists(filepath))
                {
                    if (Convert.ToInt32(string.IsNullOrEmpty(res.mobileTicketCreation.ResponseCode) ? 1 : Convert.ToInt32((res.mobileTicketCreation.ResponseCode))) == 0)
                    {
                        Directory.Delete(filepath, true);
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - InsertMobileTicket End");
                return Json(res.mobileTicketCreation);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                fileEntries = null;
                filepath = string.Empty;
                oblistFile = null;
                obFile = null;
            }
            return null;
        }

        public void DeleteAllfiles()
        {
            string filepath = string.Empty;
            try
            {
                
                if (Session["MobileNumber"] != null && Session["MobileNumber"].ToString() != string.Empty)
                {
                    CRMLogger.WriteMessage(Convert.ToString(Session["MobileNumber"]), this.ControllerContext, "");
                    filepath = HostingEnvironment.MapPath(@"\App_Data\" + Convert.ToString(Session["MobileNumber"]));
                    CRMLogger.WriteMessage("File Path :"+ filepath, this.ControllerContext,"");

                    if (Directory.Exists(filepath))
                    {
                        Directory.Delete(filepath, true);
                    }
                }
            }
            catch (Exception exDeleteAllFiles)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exDeleteAllFiles);
            }
            finally
            {
                filepath = string.Empty;
            }
            
        }

        public JsonResult ModifyMobileTicket(ModifyTicketRequest_Test req)
        {
            ModifyTicketRsponse_Test Objres;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - ModifyMobileTicket Start");
                req.MobileNumber = Convert.ToString(Session["MobileNumber"]);
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, req);
                //if (clientSetting.mvnoSettings.enableDestination.ToUpper() == "FALSE")
                //{
                //    req.DestinationName = clientSetting.mvnoSettings.destinationValue;
                //}
                if (clientSetting.mvnoSettings.enableAssignto.ToUpper() == "FALSE")
                {
                    req.ActionGroupID = clientSetting.mvnoSettings.assigntoValue;
                }
                if (clientSetting.mvnoSettings.enableFCR.ToUpper() == "FALSE")
                {
                    req.FirstCallResolution = clientSetting.mvnoSettings.FCRValue;
                }
                if (clientSetting.mvnoSettings.enableCustomerFeedback.ToUpper() == "FALSE")
                {
                    req.CustomerFeedback = Convert.ToInt32(clientSetting.mvnoSettings.customerFeedbackValue);
                }
                //if (clientSetting.mvnoSettings.disableProductName.ToUpper() == "TRUE")
                //{
                //    req.ProductID = Convert.ToInt32(clientSetting.mvnoSettings.tickettingProductName.Split('|')[1]);
                //}

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                req.UserName = Convert.ToString(Session["UserName"]);
                Objres = serviceCRM.ModifyMobileTicket(req);
                Session["ServerTime"] = Convert.ToString(System.DateTime.Now.Hour) + "|" + Convert.ToString(System.DateTime.Now.Minute) + "|" + Convert.ToString(System.DateTime.Now.Second);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, Objres.modifyMobileTicket.ResponseDesc);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - ModifyMobileTicket End");
                return Json(Objres.modifyMobileTicket);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return null;
        }

        public JsonResult LoadAttachmentList(TicketAttachmentRequest req)
        {
            TicketAttachmentResponse objres = new TicketAttachmentResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - LoadAttachmentList Start");
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, req);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.CRMTicketAttachment(req);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objres.ticketAttachment.ResponseDesc);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - LoadAttachmentList End");
                return Json(objres.ticketAttachment.FilePathNames);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return null;
        }

        public JsonResult TicketStatus()
        {
            TicketStatusResponse res;
            CRMBase req = new CRMBase();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - TicketStatus Start");
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                if (HttpRuntime.Cache["TicketStatus"] != null)
                {
                    res = (TicketStatusResponse)HttpRuntime.Cache["TicketStatus"];
                    if (res.responseDetails != null && res.responseDetails.ResponseCode != "0")
                    {
                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        res = serviceCRM.CRMTicketStatus(req);
                        if (res.responseDetails != null && res.responseDetails.ResponseCode == "0")
                        {
                            HttpRuntime.Cache.Insert("TicketStatus", res, null, DateTime.Now.AddMinutes(2), System.Web.Caching.Cache.NoSlidingExpiration);
                        }
                    }
                }
                else
                {
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    res = serviceCRM.CRMTicketStatus(req);
                    if (res.responseDetails != null && res.responseDetails.ResponseCode == "0")
                    {
                        HttpRuntime.Cache.Insert("TicketStatus", res, null, DateTime.Now.AddMinutes(2), System.Web.Caching.Cache.NoSlidingExpiration);
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, res.ticketStatus.ResponseDesc);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - TicketStatus End");
                if (res != null && res.ticketStatus != null && res.ticketStatus.TicketStatusList != null)
                    return Json(res.ticketStatus.TicketStatusList.Where(a => a.StatusID != 5 && a.StatusID != 8));
                else
                    return Json(res.ticketStatus.TicketStatusList);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                req = null;
                serviceCRM = null;
            }
            return null;
        }

        [HttpPost]
        public JsonResult FileUpload()
        {
            AttachmentResponse objres = new AttachmentResponse();
            string filepath = string.Empty;
            List<Attachement> objLisAttach = new List<Attachement>();
            Attachement objAttach = null;
            string[] fileEntries;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - FileUpload Start");
                filepath = HostingEnvironment.MapPath(@"\App_Data\" + Convert.ToString(Session["MobileNumber"]));
                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }
                var file = Request.Files[0];
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(filepath, fileName);
                    if (!System.IO.File.Exists(path))
                    {
                        file.SaveAs(path);
                        objres.ResponseCode = "0";
                    }
                    else
                    {
                        objres.ResponseCode = "1";
                        objres.ResponseDesc = "File Already added";
                    }
                    //file.SaveAs(path);
                }
                fileEntries = Directory.GetFiles(filepath);
                foreach (string fileName in fileEntries)
                {
                    objAttach = new Attachement();
                    objAttach.Name = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                    objAttach.Externsion = fileName.Substring(fileName.LastIndexOf(".") + 1);
                    objAttach.FilePath = fileName;
                    objLisAttach.Add(objAttach);
                }
                objres.objLstAttachement = objLisAttach;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - FileUpload End");
                return Json(objres);
                //return Json(objLisAttach);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                filepath = string.Empty;
                objLisAttach = null;
                objAttach = null;
                fileEntries=null;
            }
            return null;
        }

        public FileResult Downloadfilefrompath(string path, string FileName)
        {
            try 
            {
                var mimeType = ReturnExtension(FileName);
                var fileDownloadName = FileName;
                return File(path, mimeType, fileDownloadName);
            }
            catch (Exception exDownloadfilefrompath) 
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exDownloadfilefrompath);
            }
            return null;
        }

        public JsonResult DeleteFile(string path)
        {
            List<Attachement> objLisAttach = new List<Attachement>();
            Attachement objAttach = null;
            string[] fileEntries = null;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - DeleteFile Start");
                System.IO.File.Delete(path);
                fileEntries = Directory.GetFiles(path.Substring(0, path.LastIndexOf("\\")));
                foreach (string fileName in fileEntries)
                {
                    objAttach = new Attachement();
                    objAttach.Name = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                    objAttach.Externsion = fileName.Substring(fileName.LastIndexOf(".") + 1);
                    objAttach.FilePath = fileName;
                    objLisAttach.Add(objAttach);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - DeleteFile End");
                return Json(objLisAttach);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objAttach = null;
                fileEntries = null;
            }
            return null;
        }

        private string ReturnExtension(string Filename)
        {
            string fileExtension = string.Empty;
            try
            {
                fileExtension = Filename.Substring(Filename.LastIndexOf('.'));
                switch (fileExtension)
                {
                    case ".htm":
                    case ".html":
                    case ".log":
                        return "text/HTML";
                    case ".txt":
                        return "text/plain";
                    case ".docx":
                        return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    case ".doc":
                        return "application/msword";
                    case ".tiff":
                    case ".tif":
                        return "image/tiff";
                    case ".asf":
                        return "video/x-ms-asf";
                    case ".avi":
                        return "video/avi";
                    case ".zip":
                        return "application/zip";
                    case ".xls":
                    case ".csv":
                        return "application/vnd.ms-excel";
                    case ".gif":
                        return "image/gif";
                    case ".jpg":
                    case "jpeg":
                        return "image/jpeg";
                    case ".bmp":
                        return "image/bmp";
                    case ".wav":
                        return "audio/wav";
                    case ".mp3":
                        return "audio/mpeg3";
                    case ".mpg":
                    case "mpeg":
                        return "video/mpeg";
                    case ".rtf":
                        return "application/rtf";
                    case ".asp":
                        return "text/asp";
                    case ".pdf":
                        return "application/pdf";
                    case ".fdf":
                        return "application/vnd.fdf";
                    case ".ppt":
                        return "application/mspowerpoint";
                    case ".dwg":
                        return "image/vnd.dwg";
                    case ".msg":
                        return "application/msoutlook";
                    case ".xml":
                    case ".sdxl":
                        return "application/xml";
                    case ".xdp":
                        return "application/vnd.adobe.xdp+xml";
                    default:
                        return "application/octet-stream";
                }
            }
            catch (Exception exReturnExtension)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exReturnExtension);
            }
            finally
            {
                fileExtension = string.Empty;
            }
            return string.Empty;
        }
        /**/
        public FileResult DownloadfilefromBytes(string path)
        {
            FileDownloadRequest req = new FileDownloadRequest();
            FileDownloadResponse Objres;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - DownloadfilefromBytes Start");
                req.FilePathName = path;
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, req);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objres = serviceCRM.TicketFileDownload(req);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, Objres.downloadAttachment.ResponseDesc);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - DownloadfilefromBytes End");
                return File(Convert.FromBase64String(Objres.downloadAttachment.FileInBytes), ReturnExtension(path), path);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                req = null;
                serviceCRM = null;
            }
            return null;
        }

        //public JsonResult GetSampleTemplate()
        //{
        //    try
        //    {
        //        DataSet dsCountryList = new DataSet();
        //        string path = HostingEnvironment.MapPath("~/App_Data/SampleTemplate.xml");
        //        dsCountryList.ReadXml(path);
        //        var myData = dsCountryList.Tables[0].AsEnumerable().Select(r => new SampleTemplate
        //        {
        //            Title = r.Field<string>("Title"),
        //            TemplateDesc = r.Field<string>("TemplateDesc")
        //        }).ToArray();
        //        return Json(myData);
        //    }
        //    catch (Exception ex)
        //    {
        //        CRMLogger.WriteException(Session["UserName"].ToString(), this.ControllerContext, ex);
        //    }
        //    return null;
        //}

        //public string GetTemplate(string Title)
        //{
        //    try
        //    {
        //        DataSet dsCountryList = new DataSet();
        //        string path = HostingEnvironment.MapPath("~/App_Data/SampleTemplate.xml");
        //        dsCountryList.ReadXml(path);
        //        var myData = dsCountryList.Tables[0].AsEnumerable().Select(r => new SampleTemplate
        //        {
        //            Title = r.Field<string>("Title"),
        //            TemplateDesc = r.Field<string>("TemplateDesc")
        //        }).ToArray();
        //        string TemplDesc = myData.Where(a => a.Title == Title).Single().TemplateDesc;
        //        return TemplDesc;
        //    }
        //    catch (Exception ex)
        //    {
        //        CRMLogger.WriteException(Session["UserName"].ToString(), this.ControllerContext, ex);
        //    }
        //    return null;
        //}

        public JsonResult LoadTicketResolution(ResolutionRequest req)
        {
            ResolutionResponse Objres = new ResolutionResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - LoadTicketResolution Start");
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, req);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objres = serviceCRM.GetResolution(req);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, Objres.ticketResolution.ResponseDesc);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - LoadTicketResolution End");
                return Json(Objres.ticketResolution.TicketResolutionList);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return null;
        }


        public JsonResult LoadTicketRootCauseAnalysis(RootCauseRequest req)
        {
            TickettingRootCause Objres = new TickettingRootCause();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - LoadTicketRootCauseAnalysis Start");
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, req);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objres = serviceCRM.GetRootCauseAnalysis(req);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, Objres.ticketResolution.ResponseDesc);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - LoadTicketRootCauseAnalysis End");
                return Json(Objres.ticketResolution.RootCauseCls);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return null;
        }



        public string GetDateconvertion(string strDate, string strIOdate, Boolean isRequest)
        {
            string strInputDate = string.Empty;
            string strSwap = string.Empty;
            Int64 IValidate = 0;
            string strValidateDate = string.Empty;
            string strDay, strMonth, strYear;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetDateconvertion Start");
                if (!string.IsNullOrEmpty(strDate))
                {
                    strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
                    //Swapping when input from response
                    if (!isRequest)
                    {
                        strSwap = strInputDate;
                        strInputDate = strIOdate;
                        strIOdate = strSwap;
                    }
                    if (strDate.Length == strInputDate.Length)
                    {
                        strIOdate = strIOdate.ToUpper();
                        if (strIOdate == strInputDate.ToUpper())
                        {
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetDateconvertion End");
                            return strDate;
                        }
                        strValidateDate = strDate.Replace("/", string.Empty);
                        strValidateDate = strValidateDate.Replace("-", string.Empty);
                        strDay = strDate.Substring(strInputDate.ToUpper().LastIndexOf("DD"), 2);
                        strMonth = strDate.Substring(strInputDate.ToUpper().LastIndexOf("MM"), 2);
                        strYear = strDate.Substring(strInputDate.ToUpper().LastIndexOf("YYYY"), 4);
                        // string  strDatevalid = strValidateDate.replace('DD', date).replace('MM', month).replace('YYYY', year);
                        if (Int64.TryParse(strValidateDate, out IValidate))
                        {
                            strIOdate = strIOdate.ToUpper();
                            strIOdate = strIOdate.Replace("DD", strDay.Length == 1 ? "0" + strDay : strDay);
                            strIOdate = strIOdate.Replace("MM", strMonth.Length == 1 ? "0" + strMonth : strMonth);
                            strIOdate = strIOdate.Replace("YYYY", strYear);
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetDateconvertion End");
                            return strIOdate;
                        }
                        else
                        {
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetDateconvertion End");
                            return string.Empty;
                        }
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetDateconvertion End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                strInputDate = string.Empty;
                strSwap = string.Empty;
                IValidate = 0;
                strValidateDate = string.Empty;
            }
            if (string.IsNullOrEmpty(strDate))
                return string.Empty;
            else
            {
                return string.Empty;
            }
        }

        public JsonResult LoadDesc(Int32 CategoryID, Int32 SubCategoryID)
        {
            LoadDecscriptionResponse objres = new LoadDecscriptionResponse();
            LoadDecscriptionRequest req = new LoadDecscriptionRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - LoadDesc Start");
                req.SiteCode = clientSetting.mvnoSettings.tickettingSiteCode;
                req.CategoryID = CategoryID;
                req.SubCategoryID = SubCategoryID;
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, SubCategoryID);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.CRMLoadDecscription(req);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objres.Description);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - LoadDesc End");
                return Json(objres);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                req = null;
                serviceCRM = null;
            }
            return null;
        }

        public JsonResult GetTicketHistory(string Ticketid)
        {
            TicketHistoryResponse objres = new TicketHistoryResponse();
            TicketHistoryRequest req = new TicketHistoryRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetTicketHistory Start");
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                req.TicketID = Ticketid;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.CRMTicketHistory(req);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TickettingController - GetTicketHistory End");
                return Json(objres.mobileTicketHistory);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                req = null;
            }
            return null;
        }
    }
}
