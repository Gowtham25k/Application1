using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using CRM.Models;
using ServiceCRM;
using System.Data;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Web.Helpers;
using System.Web.UI;
using System.Configuration;
using System.Reflection;
using System.Web.SessionState;

namespace CRM.Controllers
{

    public class LoginController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();

        public ActionResult Login(string username, string password, string msisdn, string keyframe, string option , string iccid , string imsi,string TokenId)
        {
            LoginCRM loginCRM = new LoginCRM();
            string ContActionName = string.Empty;
            string mode = null;
            string[] splitUsername;
            string anatelid = string.Empty;
            string decurl = string.Empty;
            string planame = string.Empty;
            string bundlecode = string.Empty;
            string activationamount = string.Empty;
            string monthlyrental = string.Empty;
            string terminationfees = string.Empty;
            string customerid = string.Empty;
            string apmamount = string.Empty;
            string terminationbilled = string.Empty;
            string creditusage = string.Empty;
            string unpaidbilled = string.Empty;
            string addonbundlebilled = string.Empty;
            string terminationbilled1 = string.Empty;
            string creditusage1 = string.Empty;
            string unpaidbilled1 = string.Empty;
            string addonbundlebilled1 = string.Empty;
            string IsMultitab = string.Empty;

            //FRR4914
            string IsBundlePurchase = string.Empty;
            string ICCIDPurchasse = string.Empty;
            DateTime returntime;
            DateTime currentTime;
            double differenceMinutes;
            string culture = string.Empty;
            HttpCookie cookie1;
            HttpCookie cookie;
            HttpCookie cookieCulturetest;
            HttpCookie cookieCultureLanguage;
            string keyframesplit = string.Empty;
            


            try
            {
                CRMLogger.WriteMessage(string.Empty, this.ControllerContext, "LoginController - Login Start");
                CRMLogger.WriteMessage("", "Login", "Browser Information : Browser Type - " + Request.Browser.Type + " Browser Name - " + Request.Browser.Browser + " Browser Version - " + Request.Browser.Version + " User Agent - " + Request.UserAgent + " Major Version- " + Request.Browser.MajorVersion + " Minor Version- " + Request.Browser.MinorVersion + " Javascript- " + Request.Browser.JScriptVersion + " Javas Applets- " + Request.Browser.JavaApplets + " ActiveX Controls- " + Request.Browser.ActiveXControls);

                #region VA Scan Fix
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, " RegenerateSessionId -Method call");
                RegenerateSessionId();
                #endregion

                Session["CRMMULTITAB"] = "0";

                //PIAM
                Session["PIAMREDIRECTION"] = "0";

                if (Session["CUNTicketDetails"] != null)
                {
                    keyframe = Session["CUNTicketDetails"].ToString();
                }

                // 4987
                if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password) && string.IsNullOrEmpty(keyframe) && !string.IsNullOrEmpty(msisdn) || string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password) && string.IsNullOrEmpty(keyframe) && !string.IsNullOrEmpty(iccid) || string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password) && string.IsNullOrEmpty(keyframe) && !string.IsNullOrEmpty(imsi))
                {
                    IsMultitab = "1";
                }

                if (!string.IsNullOrEmpty(username) && username.Contains("|") && password == null && msisdn == null)
                {
                    #region Customer Comes in direct mode (regular flow)
                    splitUsername = username.Split('|');
                    if (splitUsername.Length > 0)
                    {
                        username = splitUsername[0];
                        password = splitUsername[1];
                        msisdn = "";
                        if (!string.IsNullOrEmpty(splitUsername[2]))
                        {
                            loginCRM.anatelID = splitUsername[2];
                            Session["AnatelID"] = splitUsername[2];
                        }

                        if (!string.IsNullOrEmpty(splitUsername[3]))
                        {
                            loginCRM.anatelMSISDN = splitUsername[3];
                            Session["anatelMSISDN"] = splitUsername[3];
                        }
                    }
                    #endregion
                }
                else if (keyframe != null)              
                    {
                    #region Customer comes in redirectional mode

                    if (keyframe.Contains('|'))
                    {
                        if (Session["CUNTicketDetails"] != null)
                        {
                            keyframesplit = keyframe.Split('?')[1];
                            keyframesplit = keyframesplit.Replace('=', '|');
                        }
                        else
                        {
                            keyframesplit = keyframe.Split('?')[0];
                        }
                    
                        splitUsername = keyframesplit.Split('|');
                        ContActionName = splitUsername[4];
                        if (ContActionName == "PostpaidPlanPurchaseAPM")
                        {
                            username = splitUsername[0];
                            password = splitUsername[1];
                            mode = splitUsername[2];
                            msisdn = splitUsername[3];
                            apmamount = splitUsername[7] + "," + splitUsername[6] + "," + splitUsername[5] + "," + splitUsername[8];
                            loginCRM.UserName = username;
                            loginCRM.Password = password;
                            loginCRM.qMSISDN = msisdn;
                            loginCRM.mode = mode;
                            loginCRM.ContActionName = ContActionName;
                            loginCRM.apmamount = apmamount;
                        }
                        // 4781
                        else if (ContActionName == "SelectAccountTrustlypay")
                        {
                            username = splitUsername[0];
                            password = splitUsername[1];
                            mode = splitUsername[2];
                            msisdn = splitUsername[3];
                            apmamount = splitUsername[7] + "," + splitUsername[6] + "," + splitUsername[5] + "," + splitUsername[8];
                            loginCRM.UserName = username;
                            loginCRM.Password = password;
                            loginCRM.qMSISDN = msisdn;
                            loginCRM.mode = mode;
                            loginCRM.ContActionName = ContActionName;
                            loginCRM.apmamount = apmamount;
                            TempData["DirectDebitTrustlypay"] = "1";
                        }

                        else if (ContActionName == "ChooseAccountTrustlypay")
                        {
                            username = splitUsername[0];
                            password = splitUsername[1];
                            mode = splitUsername[2];
                            msisdn = splitUsername[3];
                            apmamount = splitUsername[7] + "," + splitUsername[6] + "," + splitUsername[5] + "," + splitUsername[8];
                            loginCRM.UserName = username;
                            loginCRM.Password = password;
                            loginCRM.qMSISDN = msisdn;
                            loginCRM.mode = mode;
                            loginCRM.ContActionName = ContActionName;
                            loginCRM.apmamount = apmamount;
                            TempData["DirectDebitTrustlyview"] = "2";
                        }

                       
                        else if (ContActionName == "CUNTICKET" || ContActionName == "CUNTICKET_CHL")
                        {
                            username = splitUsername[1];
                            password = splitUsername[2];
                            mode = splitUsername[4];
                            msisdn = splitUsername[3];
                            loginCRM.UserName = username;
                            loginCRM.Password = password;
                            loginCRM.qMSISDN = msisdn;
                            loginCRM.mode = mode;

                            //6459
                            if (splitUsername.Length > 5)
                            {
                                if (splitUsername[5].ToString() == "CHL_Redirect")
                                {
                                    ContActionName = "CUNTICKET_CHL";
                                }
                                else
                                {
                                    ContActionName = "CUNTICKET";
                                }
                            }
                            
                            loginCRM.ContActionName = ContActionName;
                            loginCRM.apmamount = apmamount;
                            
                        }
                        else if (ContActionName == "ViewpostpaidPlandetails")
                        {
                            username = splitUsername[0];
                            password = splitUsername[1];
                            mode = splitUsername[2];
                            msisdn = splitUsername[3];
                            apmamount = splitUsername[7];
                            terminationbilled = splitUsername[8];
                            creditusage = splitUsername[9];
                            addonbundlebilled = splitUsername[10];
                            unpaidbilled = splitUsername[11];
                            terminationbilled1 = splitUsername[12];
                            creditusage1 = splitUsername[13];
                            addonbundlebilled1 = splitUsername[14];
                            unpaidbilled1 = splitUsername[15];
                            bundlecode = splitUsername[5];
                            loginCRM.UserName = username;
                            loginCRM.Password = password;
                            loginCRM.qMSISDN = msisdn;
                            loginCRM.mode = mode;
                            loginCRM.ContActionName = ContActionName;
                            loginCRM.apmamount = apmamount;
                            loginCRM.terminationbilled = terminationbilled;
                            loginCRM.creditusage = creditusage;
                            loginCRM.unpaidbilled = unpaidbilled;
                            loginCRM.addonbundlebilled = addonbundlebilled;
                            TempData["ViewpostpaidPlandetailsredirect"] = "1";
                            TempData["terminationbilled"] = terminationbilled;
                            TempData["creditusage"] = creditusage;
                            TempData["unpaidbilled"] = unpaidbilled;
                            TempData["addonbundlebilled"] = addonbundlebilled;
                            TempData["terminationbilled1"] = terminationbilled1;
                            TempData["creditusage1"] = creditusage1;
                            TempData["unpaidbilled1"] = unpaidbilled1;
                            TempData["addonbundlebilled1"] = addonbundlebilled1;
                            TempData["msisdn"] = msisdn;
                            TempData["bundlecode"] = bundlecode;
                        }

                        else if (ContActionName == "PostpaidPlanPurchaseDirectDebit")
                        {

                            username = splitUsername[0];
                            password = splitUsername[1];
                            mode = splitUsername[2];
                            msisdn = splitUsername[3];
                            apmamount = splitUsername[7];
                            loginCRM.UserName = username;
                            loginCRM.Password = password;
                            loginCRM.qMSISDN = msisdn;
                            loginCRM.mode = mode;
                            loginCRM.ContActionName = ContActionName;
                            loginCRM.apmamount = apmamount;
                            TempData["DirectDebitUserName"] = username;
                            TempData["DirectDebitBudldeCode"] = splitUsername[5];
                            TempData["DirectDebitAmount"] = apmamount;
                            TempData["DirectDebitBillcycle"] = splitUsername[6];
                            TempData["DirectDebitContActionName"] = ContActionName;
                            TempData["DirectDebitPromocode"] = splitUsername[8];
                            TempData["DirectDebitPaymentMode"] = splitUsername[9];
                            TempData["DirectDebitCountrySepa"] = splitUsername[10];
                            TempData["DirectDebitcardnumber"] = splitUsername[11];
                            TempData["DirectDebitname"] = splitUsername[12];
                            TempData["DirectDebitcardnicname"] = splitUsername[13];
                            TempData["DirectDebitcardtype"] = splitUsername[14];
                            TempData["DirectDebitcardExpiryDate"] = splitUsername[15];
                            TempData["DirectDebitcardIssueDate"] = splitUsername[16];
                            TempData["DirectDebitcardCVV"] = splitUsername[17];
                            TempData["DirectDebitcardCardID"] = splitUsername[18];
                            TempData["DirectDebitConsentDate"] = splitUsername[19];
                            TempData["DirectDebitaddressPostCode"] = splitUsername[20];
                            TempData["DirectDebitAddressstreet"] = splitUsername[21];
                            TempData["DirectDebitAddresscity"] = splitUsername[22];
                            TempData["DirectDebitaddresscountry"] = splitUsername[23];
                            TempData["DirectDebitHouseNo"] = splitUsername[24];
                            TempData["DirectDebitaddressApartmentNo"] = splitUsername[25];
                            TempData["DirectDebitFloor"] = splitUsername[26];
                            TempData["DirectDebitMode"] = splitUsername[27];
                            TempData["DirectDebitEmailID"] = splitUsername[28];
                            TempData["DirectDebitFirstName"] = splitUsername[29];
                            TempData["DirectDebitLastName"] = splitUsername[30];
                            TempData["DirectDebitTypeOfPayment"] = splitUsername[31];
                            TempData["DirectDebitPincode"] = splitUsername[32];
                            TempData["DirectDebitSUBSCRIBER_ID"] = splitUsername[33];
                            TempData["DirectDebitFeatureName"] = splitUsername[34];
                            TempData["DirectDebitAddress1"] = splitUsername[35];

                            TempData["DirectDebitBankCode"] = splitUsername[36];

                        }

                        else
                        {
                            username = splitUsername[0];
                            password = splitUsername[1];
                            mode = splitUsername[2];
                            msisdn = splitUsername[3];
                            apmamount = splitUsername[7];
                            loginCRM.UserName = username;
                            loginCRM.Password = password;
                            loginCRM.qMSISDN = msisdn;
                            loginCRM.mode = mode;
                            loginCRM.ContActionName = ContActionName;
                            loginCRM.apmamount = apmamount;
                            if (ContActionName == "PostpaidAutonewal")
                            {
                                
                                TempData["PostPaidAutorenewal"] = "1";
                                TempData["PostPaidAutorenewalBundleCode"] = splitUsername[5];
                                TempData["PostPaidAutorenewalid"] = splitUsername[6];
                            }
                        }
                    }

                    else
                    {
                        keyframe = keyframe.Replace(' ', '+');
                        decurl = Utility.Decrypt(keyframe);
                        splitUsername = decurl.Split('|');
                        if (splitUsername.Length > 0)
                        {
                            username = splitUsername[0];
                            password = splitUsername[1];
                            mode = splitUsername[2];
                            msisdn = splitUsername[3];
                            returntime = Convert.ToDateTime(splitUsername[4]);
                            currentTime = DateTime.Now;
                            differenceMinutes = (currentTime - returntime).TotalMinutes;
                            if (!(differenceMinutes <= Convert.ToDouble(SettingsCRM.routingExpirytime))) // if the response time is expired the clear values (not to login)
                            {
                                password = "";
                                username = "";
                                mode = "";
                            }
                            Session["LoginMode"] = null;


                            if (splitUsername.Length > 4 && clientSetting.countryCode.ToUpper() == "MEX")
                            {
                                IsBundlePurchase = splitUsername[5];
                                ICCIDPurchasse = splitUsername[6];


                            }


                            if (splitUsername.Length > 4 && clientSetting.countryCode.ToUpper() == "BRA") // only for Brazil
                            {
                                anatelid = splitUsername[5];
                                Session["AnatelID"] = anatelid;
                                Session["LoginMode"] = "Bypass";
                                WriteLogAnatelid(mode, anatelid, msisdn);
                            }
                            if (splitUsername.Length > 5 && clientSetting.countryCode.ToUpper() != "BRA" && clientSetting.countryCode.ToUpper() != "MEX")
                            {
                                ContActionName = splitUsername[5];

                                if (splitUsername.Length > 6 && mode != "NEW_MNPPORTIN") //4909
                                {
                                    ContActionName = splitUsername[10];
                                    if (ContActionName == "PostpaidPlanPurchase")
                                    {
                                        planame = splitUsername[5];
                                        bundlecode = splitUsername[6];
                                        activationamount = splitUsername[7];
                                        monthlyrental = splitUsername[8];
                                        terminationfees = splitUsername[9];
                                        customerid = splitUsername[11];
                                        Session["MobileNumber"] = msisdn;
                                    }
                                }

                            }
                            if (clientSetting.countryCode.ToUpper() == "IND" && mode == "VIEW_REGN_IND")
                            {
                                Session["Televerification"] = "TELEVERIFICATION";
                            }
                        }
                    }
                        #endregion
                    }
                //PIAM
                else if(TokenId != null & System.Configuration.ConfigurationManager.AppSettings["ENABLE_PIAM"].Trim().ToUpper()=="ON")
                {
                    #region PIAM comes in redirectional mode
                    Get_Roles_PIAM_Response ObjResp = null;
                    Get_Roles_PIAM_Req ObjReq = null;
                    //s
                    ServiceInvokeCRM serviceCRM;
                    try
                    {
                        ObjResp = new Get_Roles_PIAM_Response();
                        ObjReq = new Get_Roles_PIAM_Req();
                        ObjReq.CountryCode = System.Configuration.ConfigurationManager.AppSettings["DefaultCountryCode"].Trim();
                        ObjReq.BrandCode = System.Configuration.ConfigurationManager.AppSettings["DefaultBrandCode"].Trim();
                        ObjReq.LanguageCode = System.Configuration.ConfigurationManager.AppSettings["DefaultLanguageCode"].Trim();
                        ObjReq.Mode = "GET_TOKENID";
                        ObjReq.TokenId = TokenId;
                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        
                            ObjResp = serviceCRM.CRM_PIAM_Service(ObjReq);
                            if(ObjResp.responseDetails.ResponseCode == "0")
                            {
                                    var Timeout = System.Configuration.ConfigurationManager.AppSettings["SET_PIAM_TOKENID_TIMEOUT"].Trim();
                                    var Currentdate = DateTime.Now;
                                    var CreatedDate = DateTime.Parse(ObjResp.UpdateDate);

                                    if ((Currentdate - CreatedDate).TotalMinutes >= Convert.ToInt32(Timeout))
                                    {
                                        Session["PIAMREDIRECTION"] = "3";
                                    }
                                    else
                                    {
                                username = ObjResp.UserName;
                                password = ObjResp.Password;
                                mode = "PIAMUSER";
                                Session["PIAMREDIRECTION"] = "1";
                                Session["PIAMTOKENID"] = TokenId;
                            }
                        }
                            else
                            {
                                Session["PIAMREDIRECTION"] = "2";
                            }
                        
                    }
                    catch(Exception Ex)
                    {
                        CRMLogger.WriteException(loginCRM.UserName, this.ControllerContext, Ex);
                    }
                    finally
                    {
                        ObjReq = null;
                        ObjResp = null;
                        serviceCRM = null;
                    }
                  #endregion
                }


                    #region Default Cookie Settings
                    if (Request.Cookies["UserLanguage"] != null)
                    {
                        if (Request.Cookies["cookieCulturetest"] != null)
                        {
                            culture = string.Empty;
                            cookie1 = Request.Cookies["cookieCulturetest"];
                            cookie = Request.Cookies["UserLanguage"];
                            if (cookie1 != null && cookie1.Value != null && cookie1.Value.Trim() != string.Empty && cookie1.Value == "14f2a6f64f3b218d9e39c4e16b67a8c2b")
                            {
                                if (cookie != null && cookie.Value != null && cookie.Value.Trim() != string.Empty)
                                    culture = cookie.Value;
                            }
                            else if (cookie != null && cookie.Value != null && cookie.Value.Trim() != string.Empty)
                            {
                                cookieCulturetest = new HttpCookie("cookieCulturetest") { Value = "2" };
                                Response.Cookies.Set(cookieCulturetest);
                                culture = SettingsCRM.langCode.ToLower();
                            }
                        //VA SCAN FIX
                        if (!string.IsNullOrEmpty(culture))
                        {
                            culture = culture.Split('|')[0];
                        }
                        culture = culture + "|d7a23e81e52e3d11b847f5f8a45c4e76";
                            cookieCultureLanguage = new HttpCookie("UserLanguage") { Value = culture };
                            Response.Cookies.Set(cookieCultureLanguage);
                        }
                        else
                        {
                            culture = string.Empty;
                            cookie = Request.Cookies["UserLanguage"];
                            if (cookie != null && cookie.Value != null && cookie.Value.Trim() != string.Empty)
                            {
                                culture = SettingsCRM.langCode.ToLower();
                            }
                        
                        //VA SCAN FIX
                        if (!string.IsNullOrEmpty(culture))
                        {
                            culture = culture.Split('|')[0];
                        }
                        culture = culture + "|d7a23e81e52e3d11b847f5f8a45c4e76";

                            cookieCultureLanguage = new HttpCookie("UserLanguage") { Value = culture };
                            Response.Cookies.Set(cookieCultureLanguage);
                            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(SettingsCRM.langCode.ToLower());
                            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(SettingsCRM.langCode.ToLower());
                        }
                    }
                    else
                    {
                        Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(SettingsCRM.langCode.ToLower());
                        Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(SettingsCRM.langCode.ToLower());
                        //VA SCAN FIX
                        culture = SettingsCRM.langCode.ToLower() + "|d7a23e81e52e3d11b847f5f8a45c4e76";
                        cookieCultureLanguage = new HttpCookie("UserLanguage") { Value = culture };
                        Response.Cookies.Set(cookieCultureLanguage);
                    }
                    #endregion


                    //PIAM
                    if (System.Configuration.ConfigurationManager.AppSettings["ENABLE_PIAM"].Trim().ToUpper() == "ON" && (Convert.ToString(Session["PIAMREDIRECTION"]) == "0" || Convert.ToString(Session["PIAMREDIRECTION"]) == "2" || Convert.ToString(Session["PIAMREDIRECTION"]) == "3"))
                    {
                        return PartialView("PIAMError");
                    }

                    loginCRM.UserName = username;
                    loginCRM.Password = password;
                    loginCRM.qMSISDN = msisdn;
                    loginCRM.mode = mode;
                    loginCRM.option = option;
                    loginCRM.ContActionName = ContActionName;
                    loginCRM.planname = planame;
                    loginCRM.bundlecode = bundlecode;
                    loginCRM.activationamount = activationamount;
                    loginCRM.monthlyrental = monthlyrental;
                    loginCRM.terminationfees = terminationfees;
                    loginCRM.customerid = customerid;



                loginCRM.IsBundlePrchase = IsBundlePurchase;
                loginCRM.ICCIDPurchase = ICCIDPurchasse;

                if (msisdn != null || iccid != null || imsi != null)
                    {
                  
                    if (IsMultitab == "1")
                    {
                        if (Session["UserName"] != null && Session["Password"] != null)
                        {
                            loginCRM.UserName = Session["UserName"].ToString();
                            loginCRM.Password = Session["Password"].ToString();

                            if(msisdn != null)
                            {
                            loginCRM.qMSISDN = msisdn;
                                loginCRM.option = "1";
                            }
                            else if(iccid != null)
                            {
                                loginCRM.qMSISDN = iccid;
                                loginCRM.option = "2";
                            }
                            else
                            {
                                loginCRM.qMSISDN = imsi;
                                loginCRM.option = "3";
                            }
                            loginCRM.mode = "MULTITAB";
                            loginCRM.MobileValidated = false;
                        }
                    }
                    else
                    {
                        Session["UserName"] = null;
                        loginCRM.MobileValidated = true;
                    }

                    }
              
                    CRMLogger.WriteMessage(string.Empty, this.ControllerContext, "LoginController - Login End");
                }
            
            catch (Exception ex)
            {
                  
                CRMLogger.WriteException(loginCRM.UserName, this.ControllerContext, ex);
            }
            finally
            {
                ContActionName = string.Empty;
                mode = string.Empty;
                splitUsername = null;
                anatelid = string.Empty;
                decurl = string.Empty;
                culture = string.Empty;
                cookie1 = null;
                cookie = null;
                cookieCulturetest = null;
                cookieCultureLanguage = null;
            }
            return View(loginCRM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public JsonResult Login(LoginCRM loginCRM)
        {
            return Json(CRMLogin(loginCRM), JsonRequestBehavior.AllowGet);
        }

        public void WriteLogAnatelid(string mode, string anatelid, string msisdn)
        {
            using (LoginController lgnctr = new LoginController())
            {

                string type = (mode == "NEW_REGN") ? "Registration" : (mode == "EDIT_REGN") ? "Edit Registration" : "View Registration";
                string typedesc = (mode == "EDIT_REGN") ? "EditRegisterresponseBrazil" : (mode == "VIEW_REGN") ? "ViewRegisterresponseBrazil" : (mode == "NEW_REGN" && !string.IsNullOrEmpty(msisdn)) ? "RegisterresponseBrazilsuccess" : "RegisterresponseBrazilfailure";
                Session["AnatelID"] = anatelid;
                AuditTrailRequest auditTrailRequest = new AuditTrailRequest();
                auditTrailRequest.module = "Subscriber";
                auditTrailRequest.subModule = type + " Response";
                auditTrailRequest.action = "Brazil " + type;
                auditTrailRequest.description = type + " Response";
                auditTrailRequest.anatelId = anatelid;
                auditTrailRequest.MSISDN = !string.IsNullOrEmpty(msisdn) ? msisdn : null;
                auditTrailRequest.DescID = typedesc;
                lgnctr.AuditTrailCRM(auditTrailRequest);

            }
        }


        private UserLoginResponse CRMLogin(LoginCRM loginCRM)
        {
            UserLoginResponse userLoginResp = new UserLoginResponse();
            string UserName = string.Empty;
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                #region UserLogin Resp
                Session["ClientIPAddress"] = Request.UserHostAddress;

                UserLoginRequest userLoginReq = new UserLoginRequest();
                userLoginReq.CountryCode = SettingsCRM.countryCode;
                userLoginReq.BrandCode = SettingsCRM.brandCode;
                userLoginReq.LanguageCode = SettingsCRM.langCode;
                userLoginReq.UserName = loginCRM.UserName;
                userLoginReq.Password = loginCRM.Password;
                Session["Password"] = loginCRM.Password;
                
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl, "", ConfigurationManager.AppSettings["LogFilePath"].ToString(), SettingsCRM.countryCode, SettingsCRM.brandCode);
                
                    userLoginResp = serviceCRM.CRMUserLogin(userLoginReq);

                    ///FRR--3083
                    if (userLoginResp != null && userLoginResp.reponseDetails != null && userLoginResp.reponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Login_" + userLoginResp.reponseDetails.ResponseCode);
                        userLoginResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? userLoginResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083

                    if (userLoginResp != null && userLoginResp.reponseDetails != null)
                    {
                        if (userLoginResp.reponseDetails.ResponseCode == "0" || userLoginResp.reponseDetails.ResponseCode == "6")
                        {
                            if (!string.IsNullOrEmpty(userLoginResp.LevelAuth) && userLoginResp.LevelAuth == "2")
                            {
                                UserName = userLoginResp.userName;
                                Session["LoginUserName"] = userLoginResp.userName;
                            }
                            else
                            {
                                Session["UserName"] = userLoginResp.userName;
                            }

                                // 4836
                                 Session["SemiVarificationSuccess"] = "0";
                                if (userLoginResp.Enablesemiautoverification)
                                    Session["UserLevelSemiverication"] = "1";
                                else
                                {
                                    Session["UserLevelSemiverication"] = "0";
                                }

                            //6129
                            if (userLoginResp.EnableOTPVerificationprocess)
                                Session["UserlevelOTPVerification"] = "TRUE";
                            else
                                Session["UserlevelOTPVerification"] = "FALSE";

                            Session["UserID"] = userLoginResp.userID;
                            Session["FullName"] = userLoginResp.firstName;
                            Session["EMail"] = userLoginResp.emailID;
                            Session["UserGroup"] = userLoginResp.roleName;
                            Session["UserGroupID"] = userLoginResp.roleCatID;
                            Session["UserLanguage"] = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                            Session["ViewRightsOnly"] = false;
                            Session["IsAdmin"] = false;
                            Session["IsRootUser"] = false;
                            Session["IsAdminwithMenu"] = false;
                            Session["ROLE_CAT_TYPE"] = userLoginResp.Role_Cat_Type;
                            if (userLoginResp.Role_Cat_Type == "0")
                                Session["ROLE_CATTYPE_Value"] = true;
                            else
                                Session["ROLE_CATTYPE_Value"] = false;
                            Session["PendingApprovel"] = false;

                            Session["EditRegRights"] = userLoginResp.EditRegRights;

                            #region FRR : 4376 : ATR_ID : V_1.1.8.0

                            Session["ATR_ID"] = userLoginResp.ATR_ID;

                            // 4734
                            Session["eSIMATR"] = userLoginResp.ATR_ID;

                            #endregion

                            string countryCode = string.Empty;
                            string brandCode = string.Empty;

                            Session["CountryCode"] = countryCode = SettingsCRM.countryCode; //Need to change based on response
                            Session["BrandCode"] = brandCode = SettingsCRM.brandCode; //Need to change based on response
                            Session["MenuAndFeatures"] = userLoginResp.MenuList;

                            Session["Features"] = userLoginResp.MenuList.Where(a => a.SubCatUrl == "Home_Block3").ToList();
                           
                            #region MapPath
                            try
                            {
                                string FolderName = UserName + "_" + SettingsCRM.countryCode;
                                if (Server.MapPath("~/App_Data/UploadFile") != null)
                                {
                                    string filepath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                                    if (Directory.Exists(filepath))
                                    {
                                        System.IO.DirectoryInfo di = new DirectoryInfo(filepath);
                                        foreach (FileInfo file in di.GetFiles())
                                        {
                                            file.Delete();
                                        }
                                    }
                                }
                                if (Server.MapPath("~/App_Data/PdfDownload") != null)
                                {
                                    string filepath = Path.Combine(Server.MapPath("~/App_Data/PdfDownload"), FolderName);
                                    if (Directory.Exists(filepath))
                                    {
                                        System.IO.DirectoryInfo di = new DirectoryInfo(filepath);
                                        foreach (FileInfo file in di.GetFiles())
                                        {
                                            file.Delete();
                                        }
                                    }
                                }
                            }
                            catch (Exception eX)
                            {
                                CRMLogger.WriteException(loginCRM.UserName, this.ControllerContext, eX);
                            }
                            #endregion

                            #region configSettings
                            try
                            {
                                if ((ConfigCRM.configSettings.countrySettings == null) || !(ConfigCRM.configSettings.countrySettings.Any(c => c.countryCode == countryCode)) || !(ConfigCRM.configSettings.countrySettings.Where(c => c.countryCode == countryCode).Any(b => b.brandSettings.Any(d => d.brandCode == brandCode))))
                                {
                                    CRMBase crmBaseReq = new CRMBase();
                                    crmBaseReq.CountryCode = countryCode;
                                    crmBaseReq.BrandCode = brandCode;
                                    crmBaseReq.LanguageCode = SettingsCRM.langCode;

                                    ConfigSettings configSettings = new ConfigSettings();
                                    configSettings = serviceCRM.AuthenticateUser(crmBaseReq);
                                    if (configSettings.ResponseCode == "0")
                                    {
                                        if (ConfigCRM.configSettings.countrySettings != null)
                                        {
                                            if (ConfigCRM.configSettings.countrySettings.Any(c => c.countryCode == countryCode))
                                            {
                                                if (ConfigCRM.configSettings.countrySettings.Where(c => c.countryCode == countryCode).Any(b => b.brandSettings.Any(d => d.brandCode == brandCode)))
                                                {
                                                    ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == countryCode).brandSettings.RemoveAll(b => b.brandCode == brandCode);
                                                    ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == countryCode).brandSettings.Add(configSettings.countrySettings.FirstOrDefault().brandSettings.FirstOrDefault());
                                                }
                                                else
                                                {
                                                    ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == countryCode).brandSettings.Add(configSettings.countrySettings.FirstOrDefault().brandSettings.FirstOrDefault());
                                                }
                                            }
                                            else
                                            {
                                                ConfigCRM.configSettings.countrySettings.Add(configSettings.countrySettings.FirstOrDefault());
                                            }
                                        }
                                        else
                                        {
                                            ConfigCRM.configSettings.countrySettings = new List<CountrySettings>();
                                            ConfigCRM.configSettings.countrySettings.Add(configSettings.countrySettings.FirstOrDefault());
                                        }
                                    }
                                }
                            }
                            catch (Exception eX)
                            {
                                CRMLogger.WriteException(loginCRM.UserName, this.ControllerContext, eX);
                            }
                            #endregion

                            #region rootUser
                            try
                            {
                                if (clientSetting.mvnoSettings.userManagementType.ToUpper() == "OFF")
                                {
                                    if (SettingsCRM.rootUserIDs.Contains(userLoginResp.userID))
                                    {
                                        Session["IsRootUser"] = true;
                                        Session["IsAdmin"] = true;
                                        Session["ViewRightsOnly"] = false;
                                        Session["IsAdminwithMenu"] = true;
                                        Session["ATR_ID"] = "ALL";
                                    }
                                    else
                                    {
                                        Session["IsRootUser"] = false;
                                        Session["IsAdminwithMenu"] = true;

                                        if (clientSetting.mvnoSettings.adminGroupID.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Contains(userLoginResp.roleCatID.ToString()))
                                        {
                                            Session["IsAdmin"] = string.IsNullOrEmpty(userLoginResp.roleCatID) ? true : clientSetting.mvnoSettings.adminGroupID.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Contains(userLoginResp.roleCatID.ToString());
                                            Session["ViewRightsOnly"] = false;
                                        }
                                        else
                                        {
                                            //Session["IsAdmin"] = false;
                                            Session["IsAdmin"] = true; //EMP ID : 2527
                                            // Session["ViewRightsOnly"] = userLoginResp.viewRightsOnly.Trim() == "1" ? true : false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (userLoginResp.Role_Cat_Type == "0")
                                    {
                                        Session["IsRootUser"] = true;
                                        Session["IsAdmin"] = true;
                                        Session["ViewRightsOnly"] = false;
                                        Session["IsAdminwithMenu"] = true;
                                        Session["PendingApprovel"] = true;
                                        Session["ATR_ID"] = "ALL";
                                    }
                                    else if (userLoginResp.Role_Cat_Type == "1")
                                    {
                                        Session["IsRootUser"] = false;
                                        Session["IsAdmin"] = true;
                                        Session["ViewRightsOnly"] = false;
                                        Session["IsAdminwithMenu"] = true;
                                        Session["PendingApprovel"] = true;
                                    }
                                    else if (userLoginResp.Role_Cat_Type == "2")
                                    {
                                        Session["IsRootUser"] = false;
                                        Session["IsAdminwithMenu"] = false;
                                        Session["IsAdmin"] = false;
                                        Session["ViewRightsOnly"] = false;
                                        Session["PendingApprovel"] = false;
                                    }
                                    else if (userLoginResp.Role_Cat_Type == "3")
                                    {
                                        Session["IsRootUser"] = false;
                                        Session["IsAdminwithMenu"] = false;
                                        Session["IsAdmin"] = false;
                                        Session["ViewRightsOnly"] = false;
                                        Session["PendingApprovel"] = true;
                                    }
                                }
                            }
                            catch (Exception eX)
                            {
                                CRMLogger.WriteException(loginCRM.UserName, this.ControllerContext, eX);
                            }
                            #endregion

                            #region RoleMgtMaster
                            try
                            {
                                RoleMgtMasterItemResponse roleMgtResp = new RoleMgtMasterItemResponse();
                                RoleMgtMasterItemRequest menuItemReq = new RoleMgtMasterItemRequest();
                                menuItemReq.CountryCode = SettingsCRM.countryCode;
                                menuItemReq.BrandCode = SettingsCRM.brandCode;
                                menuItemReq.LanguageCode = SettingsCRM.langCode;
                                menuItemReq.roleID = Convert.ToString(Session["UserGroupID"]);

                                roleMgtResp = serviceCRM.CRMRoleMgtMaster(menuItemReq);

                                if (roleMgtResp != null && roleMgtResp.settingMenu != null && roleMgtResp.settingMenu.Count == 9)
                                {
                                    if (clientSetting.mvnoSettings.userManagementType.ToUpper() == "OFF")
                                    {
                                        if ((bool)Session["IsRootUser"])
                                        {
                                            roleMgtResp.settingMenu.ForEach(s => { s.editRights = "1"; s.adminRights = "1"; });
                                            Session["IsAdminwithMenu"] = true;
                                        }
                                        else
                                        {
                                            Session["IsAdminwithMenu"] = false;
                                        }
                                        Session["MenuItemResp"] = roleMgtResp;
                                    }
                                    else
                                    {
                                        // roleMgtResp.settingMenu.ForEach(s => { s.editRights = "1"; s.adminRights = "1"; });
                                        Session["IsAdminwithMenu"] = true;
                                        Session["MenuItemResp"] = roleMgtResp;
                                    }
                                }
                                else
                                {
                                    if (clientSetting.mvnoSettings.userManagementType.ToUpper() == "OFF")
                                    {
                                        roleMgtResp = new RoleMgtMasterItemResponse();
                                        roleMgtResp.settingMenu = new List<SettingsMenu>();
                                        if ((bool)Session["IsRootUser"])
                                        {
                                            for (int i = 1; i <= 9; i++)
                                            {
                                                roleMgtResp.settingMenu.Add(new SettingsMenu() { adminRights = "1", editRights = "1", roleMgtId = i.ToString() });
                                            }
                                            Session["IsAdminwithMenu"] = true;
                                        }
                                        else
                                        {
                                            for (int i = 1; i <= 9; i++)
                                            {
                                                roleMgtResp.settingMenu.Add(new SettingsMenu() { adminRights = "0", editRights = "0", roleMgtId = i.ToString() });
                                            }
                                            Session["IsAdminwithMenu"] = false;
                                        }
                                        Session["MenuItemResp"] = roleMgtResp;
                                    }
                                    else
                                    {
                                        for (int i = 1; i <= 9; i++)
                                        {
                                            roleMgtResp.settingMenu.Add(new SettingsMenu() { adminRights = "0", editRights = "0", roleMgtId = i.ToString() });
                                        }
                                        Session["IsAdminwithMenu"] = false;
                                        Session["MenuItemResp"] = roleMgtResp;
                                    }
                                }
                            }
                            catch (Exception eX)
                            {
                                CRMLogger.WriteException(loginCRM.UserName, this.ControllerContext, eX);
                            }
                            #endregion
                            Session["ServerDateTime"] = clientSetting.mvnoSettings.ServerDate + "/" + clientSetting.mvnoSettings.ServerMonth + "/" + clientSetting.mvnoSettings.ServerYear;

                            // 4925
                            if(clientSetting.preSettings.EnableCRMMultiTab != null)
                            Session["Enablemultitab"] = clientSetting.preSettings.EnableCRMMultiTab.ToUpper();


                            if (clientSetting.preSettings.EnableUserlevelPasswordPolicy.ToUpper() == "TRUE")
                            {

                                // FRR 4710
                                //Password Policy Enabled Flag checking 
                                var networklevelpwd = clientSetting.mvnoSettings.isEnablePasswordPolicy.ToUpper() == "TRUE";
                                if (networklevelpwd)
                                {
                                    if (userLoginResp.ENABLEPASSWORDPOLICY)
                                    {
                                        if (userLoginResp.UserlevelPwdpolicy)
                                        {
                                            userLoginResp.ENABLEPASSWORDPOLICY = true;
                                        }
                                        else
                                        {
                                            userLoginResp.ENABLEPASSWORDPOLICY = false;
                                        }
                                    }
                                    else
                                    {
                                        userLoginResp.ENABLEPASSWORDPOLICY = false;
                                    }
                                }
                                else
                                {
                                    userLoginResp.ENABLEPASSWORDPOLICY = false;
                                }
                                Session["EnablePwdpolicy"] = userLoginResp.ENABLEPASSWORDPOLICY;
                                if (userLoginResp.ENABLEPASSWORDPOLICY)
                                {
                                    userLoginResp.Is_PasswordPolicy = "ON";
                                }
                                else
                                {
                                    if (userLoginResp.NotificationDays > 0 && Convert.ToInt16(clientSetting.mvnoSettings.notificationDays) < userLoginResp.NotificationDays)
                                    {
                                        userLoginResp.Is_PasswordPolicy = "OFF";
                                    }
                                    else
                                    {
                                        userLoginResp.Is_PasswordPolicy = "ON";
                                    }
                                }

                                // FRR 4710 end 
                            }
                            else
                            {

                                if (userLoginResp.ENABLEPASSWORDPOLICY)
                                    userLoginResp.ENABLEPASSWORDPOLICY = clientSetting.mvnoSettings.isEnablePasswordPolicy.ToUpper() == "TRUE";
                            }

                            //Notification Days 
                            //if (userLoginResp.NotificationDays < 0 || (Convert.ToInt16(clientSetting.mvnoSettings.notificationDays) < userLoginResp.NotificationDays && userLoginResp.NotificationDays > 0))
                            if (userLoginResp.NotificationDays > 0 && Convert.ToInt16(clientSetting.mvnoSettings.notificationDays) < userLoginResp.NotificationDays)
                                userLoginResp.NotificationDays = -1;
                        }
                        else
                        {
                            loginCRM.Message = Resources.HomeResources.InvalidLogin;
                            CRMLogger.UnAuthorizeMessage(loginCRM.UserName, this.ControllerContext, loginCRM.Message);
                        }
                    }
                    else
                    {
                        loginCRM.Message = Resources.HomeResources.InvalidLogin;
                        CRMLogger.UnAuthorizeMessage(loginCRM.UserName, this.ControllerContext, loginCRM.Message);

                        if (userLoginResp.reponseDetails == null)
                        {
                            userLoginResp.reponseDetails = new CRMResponse();
                            userLoginResp.reponseDetails.ResponseCode = "1";
                        }
                    }
                
                #endregion


                #region VA Scan Fix
                string cookieToken = string.Empty;
                string formToken = string.Empty;
                HttpCookie cookie;
                HttpCookie newAntiForgeryCookie;
                try
                {

                    if (userLoginResp.reponseDetails.ResponseCode == "0")
                    {
                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, " RegenerateSessionId -Method call");
                        RegenerateSessionId();

                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AntiForgeryTokenid Regenrate");
                       
                        
                        AntiForgery.GetTokens("", out cookieToken, out formToken);
                        if (Request.Cookies["__RequestVerificationToken"] != null)
                        {
                            cookie = new HttpCookie("__RequestVerificationToken");
                            cookie.Value = string.Empty;
                            cookie.Expires = DateTime.Now.AddDays(-1);
                            Response.Cookies.Set(cookie);
                        }
                        
                        newAntiForgeryCookie = new HttpCookie("__RequestVerificationToken", cookieToken)
                        {
                            HttpOnly = true,
                            Secure = Request.IsSecureConnection // Ensure it's secure if using HTTPS
                        };
                        Response.Cookies.Set(newAntiForgeryCookie);
                    }
                }
                catch (Exception EX)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, EX);
                }
                finally
                {
                    cookieToken = string.Empty;
                    formToken = string.Empty;
                    cookie = null;
                    newAntiForgeryCookie=null;
                }
                #endregion

                return userLoginResp;
            }
            catch (Exception eX)
            {
                loginCRM.Message = eX.Message;
                if (UserName != null)
                {
                    CRMLogger.WriteException(UserName, this.ControllerContext, eX);
                }
                else
                {
                    CRMLogger.UnAuthorizeException(loginCRM.UserName, this.ControllerContext, eX);
                }

                userLoginResp.reponseDetails = new CRMResponse();
                userLoginResp.reponseDetails.ResponseCode = "9";
                userLoginResp.reponseDetails.ResponseDesc = eX.Message;

                return userLoginResp;
            }
            finally
            {
                try
                {
                    if (UserName != null)
                    {
                        List<string> lstProfileImg = Directory.GetFiles(SettingsCRM.myProfileImgPath, UserName + ".*", SearchOption.TopDirectoryOnly).ToList();
                        string fileName = string.Empty;
                        string fileExtn = string.Empty;

                        if (lstProfileImg.Count > 0)
                        {
                            fileName = lstProfileImg[0];
                            fileExtn = new FileInfo(fileName).Extension;
                            if (!System.IO.File.Exists(Server.MapPath("~/Content/MyProfile/" + UserName + fileExtn)))
                            {
                                System.IO.File.Copy(fileName, Server.MapPath("~/Content/MyProfile/" + UserName + fileExtn));
                            }
                        }
                    }

                   
                }
                catch (Exception eX)
                {
                    CRMLogger.WriteException(UserName, this.ControllerContext, eX);
                }

                Dispose(true);
                GC.SuppressFinalize(userLoginResp);
                //userLoginResp = null;
                //loginCRM = null;
                serviceCRM = null;
            }
        }

        public ActionResult Logout()
        {
            AuditTrailRequest auditTrailRequest = new AuditTrailRequest();
            string cookieToken = string.Empty;
            string formToken = string.Empty;
            HttpCookie cookie;
            HttpCookie newAntiForgeryCookie;
            try
            {
                try
                {
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Logout Success");

                    auditTrailRequest.module = "Login";
                    auditTrailRequest.subModule = "User Logout";
                    auditTrailRequest.action = "Logout";
                    auditTrailRequest.description = "User Logout from CRM";

                    AuditTrailCRM(auditTrailRequest);

                    System.IO.Directory.GetFiles(Server.MapPath("~/Content/MyProfile/")).Where(m => new FileInfo(m).LastAccessTime < DateTime.Now.AddHours(-1)).ToList().ForEach(m => System.IO.File.Delete(m));
                }
                catch (Exception eX)
                {
                    CRMLogger.WriteException(Session["UserName"] != null ? Session["UserName"].ToString():"", this.ControllerContext, eX);
                }
                //VA SCAN ISSUE
                RegenerateSessionId();

                AntiForgery.GetTokens("", out cookieToken, out formToken);
                if (Request.Cookies["__RequestVerificationToken"] != null)
                {
                    cookie = new HttpCookie("__RequestVerificationToken");
                    cookie.Value = string.Empty;
                    cookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Set(cookie);
                }

                newAntiForgeryCookie = new HttpCookie("__RequestVerificationToken", cookieToken)
                {
                    HttpOnly = true,
                    Secure = Request.IsSecureConnection // Ensure it's secure if using HTTPS
                };
                Response.Cookies.Set(newAntiForgeryCookie);

                Session.Abandon();
                Session.RemoveAll();
                Session.Clear();
                Dispose();
                //PIAM
                if (System.Configuration.ConfigurationManager.AppSettings["ENABLE_PIAM"].Trim().ToUpper() == "ON")
                    return Redirect(System.Configuration.ConfigurationManager.AppSettings["PIAMRedirectionalURL"].Trim());
                else
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(string.Empty, this.ControllerContext, ex);
                return RedirectToAction("Login");
            }
            finally
            {
                auditTrailRequest = null;
                cookieToken = string.Empty;
                formToken = string.Empty;
                cookie = null;
                newAntiForgeryCookie = null;
            }
        }


        public ActionResult SessionExpired()
        {
            AuditTrailRequest auditTrailRequest = new AuditTrailRequest();
            try
            {
                try
                {
                    auditTrailRequest.module = "Login";
                    auditTrailRequest.subModule = "User Logout";
                    auditTrailRequest.action = "Logout";
                    auditTrailRequest.description = "User Logout from CRM";

                    AuditTrailCRM(auditTrailRequest);

                    System.IO.Directory.GetFiles(Server.MapPath("~/Content/MyProfile/")).Where(m => new FileInfo(m).LastAccessTime < DateTime.Now.AddHours(-1)).ToList().ForEach(m => System.IO.File.Delete(m));
                }
                catch (Exception eX)
                {
                    CRMLogger.WriteException(string.Empty, this.ControllerContext, eX);
                }

                Session.Clear();
                Session.RemoveAll();
                Session.Abandon();
                Dispose();
                return View();
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View();
            }
            finally
            {
                auditTrailRequest = null;
            }
        }

        public ActionResult MultiLanguage(string cultureCode)
        {
           
            string VaCulture = string.Empty;
            try
            {
                VaCulture = cultureCode + "|d7a23e81e52e3d11b847f5f8a45c4e76";

                HttpCookie cookieCultureLanguage = new HttpCookie("UserLanguage") { Value = VaCulture };
                HttpCookie cookieCulturetest = new HttpCookie("cookieCulturetest") { Value = "14f2a6f64f3b218d9e39c4e16b67a8c2b" };

                Response.Cookies.Set(cookieCultureLanguage);
                Response.Cookies.Set(cookieCulturetest);

                //Sets  Culture for Current thread  
                Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(cultureCode);

                //Ui Culture for Localized text in the UI  
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(cultureCode);

            }
            catch (Exception ex)
            {

             

                CRMLogger.WriteException(string.Empty, this.ControllerContext, ex);
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        public JsonResult FPServiceCall(string UserName)
        {
            

            PrePostForgotPasswordReq objCRMForgotpassword = new PrePostForgotPasswordReq();
            PrePostForgotPasswordRes objRes = new PrePostForgotPasswordRes();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objCRMForgotpassword.UserName = UserName;
                //objCRMForgotpassword.FromEmail = SettingsCRM.fpMailFrom;
                objCRMForgotpassword.BrandCode = SettingsCRM.brandCode;
                objCRMForgotpassword.CountryCode = SettingsCRM.countryCode;
                objCRMForgotpassword.LanguageCode = SettingsCRM.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.CRMPrePostForgotPassword(objCRMForgotpassword);
                    ///FRR--3083
                    if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Forgotpassword_" + objRes.responseDetails.ResponseCode);
                        objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                
                if (objRes.responseDetails != null && (objRes.responseDetails.ResponseCode == "0" || objRes.responseDetails.ResponseCode == "55" || objRes.responseDetails.ResponseCode == "56" || objRes.responseDetails.ResponseCode == "57"))
                {

                }
                return Json(objRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

              

                CRMLogger.WriteException(objCRMForgotpassword.UserName, this.ControllerContext, ex);
                return Json(objRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objCRMForgotpassword = null;
                //objRes = null;
                serviceCRM = null;
            }

        }


        // FRR 4710
        public JsonResult ResetPasswordwithOTP(string UserName)
        {
            ResetPasswordwithOPTresponse objres = new ResetPasswordwithOPTresponse();
            ResetPasswordwithOPTresquest objreq = JsonConvert.DeserializeObject<ResetPasswordwithOPTresquest>(UserName); ;
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {


                objreq.BrandCode = SettingsCRM.brandCode;
                objreq.CountryCode = SettingsCRM.countryCode;
                objreq.LanguageCode = SettingsCRM.langCode;


                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objres = serviceCRM.CRMResetPasswordwithOTP(objreq);

                    if (objres != null && objres.responseDetails != null && objres.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ResetPasswordwithOTP_" + objres.responseDetails.ResponseCode);
                        objres.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.responseDetails.ResponseDesc : errorInsertMsg;
                    }

                
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objres, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objreq = null;
                serviceCRM = null;
            }


            return Json(objres, JsonRequestBehavior.AllowGet);
        }


        // FRR 4710

        public void AuditTrailCRM(AuditTrailRequest auditTrailReq)
        {
            CRMResponse crmResp = new CRMResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                HttpContext httpContext = System.Web.HttpContext.Current;
                auditTrailReq.CountryCode = SettingsCRM.countryCode;
                auditTrailReq.BrandCode = SettingsCRM.brandCode;
                auditTrailReq.LanguageCode = SettingsCRM.langCode;

                auditTrailReq.userId = httpContext.Session["UserName"] != null ? httpContext.Session["UserName"].ToString() : "";
                auditTrailReq.userIp = httpContext.Request.UserHostAddress;
                auditTrailReq.sessionId = httpContext.Session.SessionID;
                auditTrailReq.channel = "CRM";
                //auditTrailReq.anatelId = httpContext.Session["AnatelID"] != null ? httpContext.Session["AnatelID"].ToString() : null;
                if (!string.IsNullOrEmpty(auditTrailReq.DescID))
                    auditTrailReq.anatelId = httpContext.Session["AnatelID"] != null ? httpContext.Session["AnatelID"].ToString() : null;
                else
                    auditTrailReq.anatelId = null;

                auditTrailReq.DescID = clientSetting.countryCode.ToUpper() == "BRA" ? auditTrailReq.DescID : null;

                if (string.IsNullOrEmpty(auditTrailReq.MSISDN) && httpContext.Session["MobileNumber"] != null)
                {
                    auditTrailReq.MSISDN = Convert.ToString(httpContext.Session["MobileNumber"]);
                }
                CRMLogger.WriteMessage(auditTrailReq.userId, auditTrailReq.module + "-" + auditTrailReq.subModule + "-" + auditTrailReq.action, auditTrailReq.description);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    crmResp = serviceCRM.AuditTrailCRM(auditTrailReq);
                
            }
            catch (Exception exAuditTrialCRM)
            {
                CRMLogger.WriteException(auditTrailReq.userId, "AuditTrialCRM", exAuditTrialCRM);
            }
            finally
            {
                auditTrailReq = null;
                //crmResp = null;
                serviceCRM = null;
            }
        }

        public ActionResult ANATELENQUIRY()
        {
            return View();
        }

        public ActionResult PasswordPolicy(int Feature = 0, int iExpireDays = 1)
        {
            PasswordPolicy objPwdPolicy = new PasswordPolicy();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objPwdPolicy.PasswordPolictyTypeID = Feature;
                objPwdPolicy.NotificationMessage = string.Format(Resources.HomeResources.lblPasswordNotificationMessage, Convert.ToString(Session["UserName"]), iExpireDays);
                List<Dropdown> objLstDrop = new List<Dropdown>();
                Dropdown ObjDrop = new Dropdown();

                if (Feature == 1 && iExpireDays <= 0)
                {
                    objPwdPolicy.NotificationMessage = string.Format(Resources.HomeResources.lblPasswordNotificationExpiredMessage, Convert.ToString(Session["UserName"]));
                    objPwdPolicy.QuestionA1 = "expired";
                }

                else if (Feature == 2)
                {
                    DataSet ds = Utility.BindXmlFile("~/App_Data/ConfigItems.xml");

                    objPwdPolicy.objSecurityQuestion = new Models.SecurityQuestion();
                    foreach (DataTable dt in ds.Tables)
                    {
                        switch (dt.TableName)
                        {
                            case "Questions1":
                                objLstDrop = new List<Dropdown>();
                                foreach (DataRow row in dt.Rows)
                                {
                                    ObjDrop = new Dropdown();
                                    ObjDrop.ID = row["id"].ToString();
                                    ObjDrop.Value = row["Questions1_Text"].ToString();
                                    objLstDrop.Add(ObjDrop);
                                }
                                objPwdPolicy.objSecurityQuestion.Question1 = objLstDrop;
                                break;
                            case "Questions2":
                                objLstDrop = new List<Dropdown>();
                                foreach (DataRow row in dt.Rows)
                                {
                                    ObjDrop = new Dropdown();
                                    ObjDrop.ID = row["id"].ToString();
                                    ObjDrop.Value = row["Questions2_Text"].ToString();
                                    objLstDrop.Add(ObjDrop);
                                }
                                objPwdPolicy.objSecurityQuestion.Question2 = objLstDrop;
                                break;
                            case "Questions3":
                                objLstDrop = new List<Dropdown>();
                                foreach (DataRow row in dt.Rows)
                                {
                                    ObjDrop = new Dropdown();
                                    ObjDrop.ID = row["id"].ToString();
                                    ObjDrop.Value = row["Questions3_Text"].ToString();
                                    objLstDrop.Add(ObjDrop);
                                }
                                objPwdPolicy.objSecurityQuestion.Question3 = objLstDrop;
                                break;
                            default:
                                break;

                        }
                    }
                }
                else if (Feature == 3)
                {
                    objPwdPolicy.objSecurityQuestion = new Models.SecurityQuestion();
                    SecurityAnswer objReq = new SecurityAnswer();
                    objReq.BrandCode = clientSetting.brandCode;
                    objReq.CountryCode = clientSetting.countryCode;
                    objReq.LanguageCode = clientSetting.langCode;

                    if (string.IsNullOrEmpty(Convert.ToString(Session["UserName"])))
                    {
                        objReq.Username = Session["LoginUserName"] != null ? Session["LoginUserName"].ToString() : Session.SessionID;
                    }
                    else
                    {
                        objReq.Username = Session["UserName"] != null ? Session["UserName"].ToString() : Session.SessionID;
                    }
                    GetSecurityAnswer objRes = new GetSecurityAnswer();
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        objRes = serviceCRM.GetSecurityQuestion(objReq);
                        if (objRes.SecurityQuestion.Length > 0)
                        {
                            string[] Question = objRes.SecurityQuestion.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                            EncryptDecrypt.Crypt objCrypt = new EncryptDecrypt.Crypt();
                            objRes.SecurityAnswer = objCrypt.Decrypt(objRes.SecurityAnswer);
                            string[] Answer = objRes.SecurityAnswer.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                            objLstDrop = new List<Dropdown>();
                            objPwdPolicy.QuestionA1 = Question[0];
                            for (int i = 0; i < Question.Count(); i++)
                            {
                                ObjDrop = new Dropdown();
                                ObjDrop.ID = Answer[i];
                                ObjDrop.Value = Question[i];
                                objLstDrop.Add(ObjDrop);
                            }
                            objPwdPolicy.objSecurityQuestion.Question1 = objLstDrop;
                        }
                    
                }

                return View(objPwdPolicy);
            }
            catch (Exception ex)
            {
                return View(objPwdPolicy);
            }
            finally
            {
                Dispose(true);
                GC.SuppressFinalize(objPwdPolicy);
                //objPwdPolicy = null;
                serviceCRM = null;
            }
        }

        public JsonResult SecurityAnswerUpdate(ServiceCRM.SecurityQuestion objSecurityQuestion)
        {
            CRMResponse objRes = new CRMResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objSecurityQuestion.BrandCode = clientSetting.brandCode;
                objSecurityQuestion.CountryCode = clientSetting.countryCode;
                objSecurityQuestion.LanguageCode = clientSetting.langCode;

                if (string.IsNullOrEmpty(Convert.ToString(Session["UserName"])))
                {
                    objSecurityQuestion.Username = Session["LoginUserName"] != null ? Session["LoginUserName"].ToString() : Session.SessionID;
                }
                else
                {
                    objSecurityQuestion.Username = Session["UserName"] != null ? Session["UserName"].ToString() : Session.SessionID;
                }

                //objSecurityQuestion.Username = Session["UserName"] != null ? Session["UserName"].ToString() : Session.SessionID;
                EncryptDecrypt.Crypt objCrypt = new EncryptDecrypt.Crypt();
                objSecurityQuestion.Answer = objCrypt.Encrypt(objSecurityQuestion.Answer);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.InsertSecurityQuestion(objSecurityQuestion);

                    if (objRes != null && objRes.ResponseCode != null)
                    {
                        string errorMsg = Resources.ErrorResources.ResourceManager.GetString("passwordPolicy_" + objRes.ResponseCode);
                        objRes.ResponseDesc = string.IsNullOrEmpty(errorMsg) ? objRes.ResponseDesc : errorMsg;
                    }

                    if (objRes != null && objRes.ResponseCode != null && objRes.ResponseCode == "0" && string.IsNullOrEmpty(Convert.ToString(Session["UserName"])))
                    {
                        Session["UserName"] = objSecurityQuestion.Username;
                    }

                
                return Json(objRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //objRes = null;
                objSecurityQuestion = null;
                 serviceCRM = null;
            }
        }
        public JsonResult SecurityAnswerDateUpdate(Boolean iFailedAnswer)
        {
            CRMResponse objRes = new CRMResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                ServiceCRM.SecurityAnswer objSecurityQuestion = new SecurityAnswer();
                objSecurityQuestion.BrandCode = clientSetting.brandCode;
                objSecurityQuestion.CountryCode = clientSetting.countryCode;
                objSecurityQuestion.LanguageCode = clientSetting.langCode;
                objSecurityQuestion.AccBlocked = iFailedAnswer;

                if (string.IsNullOrEmpty(Convert.ToString(Session["UserName"])))
                {
                    objSecurityQuestion.Username = Session["LoginUserName"] != null ? Session["LoginUserName"].ToString() : Session.SessionID;
                }
                else
                {
                    objSecurityQuestion.Username = Session["UserName"] != null ? Session["UserName"].ToString() : Session.SessionID;
                }

                //objSecurityQuestion.Username = Session["UserName"] != null ? Session["UserName"].ToString() : Session.SessionID;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.UpdateSecurityAnswerDate(objSecurityQuestion);
                
                return Json(objRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //objRes = null;
                serviceCRM = null;
            }

        }

        [HttpGet]
        public bool ResetSession()
        {
            return Session.IsNewSession;
        }


        public JsonResult ResendPassword(string strResend)
        {
            UserRegister userDetails = JsonConvert.DeserializeObject<UserRegister>(strResend);
            UserRegisterResponse userRegisterResponse = new UserRegisterResponse();
            try
            {
                userRegisterResponse = CRMUserRegister(userDetails, "RESEND");
                ///FRR--3083
                if (userRegisterResponse != null && userRegisterResponse.reponseDetails != null && userRegisterResponse.reponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RESENDPassword_" + userRegisterResponse.reponseDetails.ResponseCode);
                    userRegisterResponse.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? userRegisterResponse.reponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                return Json(userRegisterResponse, JsonRequestBehavior.AllowGet);

            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(userRegisterResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                // userRegisterResponse = null;
            }
        }

        public UserRegisterResponse CRMUserRegister(UserRegister userDetails, string mode)
        {
            UserRegisterRequest userRegisterReq = new UserRegisterRequest();
            UserRegisterResponse userRegisterResponse = new UserRegisterResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                userRegisterReq.BrandCode = SettingsCRM.brandCode;
                userRegisterReq.CountryCode = SettingsCRM.countryCode;
                userRegisterReq.LanguageCode = SettingsCRM.langCode;
                userRegisterReq.UserName = userDetails.userName;
                userRegisterReq.userId = userDetails.userID;

                userRegisterReq.FirstName = userDetails.firstName;
                userRegisterReq.LastName = userDetails.lastName;
                userRegisterReq.PhoneNumber = userDetails.phoneNumber;
                userRegisterReq.EmailId = userDetails.emailID;
                userRegisterReq.PasswordUpd = "NO";
                userRegisterReq.Status = userDetails.status;
                userRegisterReq.Mode = mode;
                userRegisterReq.roleCatId = userDetails.roleCatId;
                userRegisterReq.AccBlocked = userDetails.AccBlocked;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    userRegisterResponse = serviceCRM.CRMUserRegister(userRegisterReq);
                
                userRegisterResponse.UserRegister = userRegisterResponse.UserRegister.OrderBy(m => m.roleCatId).ToList();
                return userRegisterResponse;
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return userRegisterResponse;
            }
            finally
            {
                userRegisterReq = null;
                // userRegisterResponse = null;
                serviceCRM = null;
            }
        }



        #region CRMSecondLevelAuth
        public ActionResult CRMSecondLevelAuth(string Msisdn, string UserName)
        {
            SecondLevelAuthRes ObjRes = new SecondLevelAuthRes();
            SecondLevelAuthReq objReq = new SecondLevelAuthReq();
            //s
            ServiceInvokeCRM serviceCRM;

            try
            {
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.MSISDN = Msisdn;
                objReq.UserName = UserName;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMSecondLevelAuth(objReq);
                    if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SecondLevelAuth_" + ObjRes.Response.ResponseCode);
                        ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;
                    }
                
                return Json(ObjRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(ObjRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                // ObjRes = null;
                objReq = null;
                serviceCRM = null;
            }

        }


        #endregion



        #region This method is for Appdynamics error view issue not used in code.
        public ActionResult Error()
        {
            return View();
        }
        #endregion



        #region Generate new sessionid for after login
       
        private void RegenerateSessionId()
        {

            string oldId = string.Empty;
            string newId = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, " RegenerateSessionId- Start");
                System.Web.SessionState.SessionIDManager manager = new System.Web.SessionState.SessionIDManager();
                System.Web.HttpContext Context = System.Web.HttpContext.Current;
                 oldId = manager.GetSessionID(Context);
                 newId = manager.CreateSessionID(Context);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, " RegenerateNEWSessionId- "+ newId);

                bool isAdd = false, isRedir = false;

                manager.SaveSessionID(Context, newId, out isRedir, out isAdd);

                HttpApplication ctx = (HttpApplication)System.Web.HttpContext.Current.ApplicationInstance;

                HttpModuleCollection mods = ctx.Modules;

                System.Web.SessionState.SessionStateModule ssm = (SessionStateModule)mods.Get("Session");

                System.Reflection.FieldInfo[] fields = ssm.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

                SessionStateStoreProviderBase store = null;

                System.Reflection.FieldInfo rqIdField = null, rqLockIdField = null, rqStateNotFoundField = null;

                foreach (System.Reflection.FieldInfo field in fields)
                {
                    if (field.Name.Equals("_store")) store = (SessionStateStoreProviderBase)field.GetValue(ssm);
                    if (field.Name.Equals("_rqId")) rqIdField = field;
                    if (field.Name.Equals("_rqLockId")) rqLockIdField = field;
                    if (field.Name.Equals("_rqSessionStateNotFound")) rqStateNotFoundField = field;
                }

                object lockId = rqLockIdField.GetValue(ssm);

                if ((lockId != null) && (oldId != null)) store.ReleaseItemExclusive(Context, oldId, lockId);

                rqStateNotFoundField.SetValue(ssm, true);

                rqIdField.SetValue(ssm, newId);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, " RegenerateSessionId- END");
            }
            catch(Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);

            }
            finally
            {
                oldId = string.Empty;
                newId = string.Empty;
            }
        }
        #endregion
    }
}
