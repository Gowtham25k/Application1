using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ServiceCRM;
using System.Web;
using CRM.Models;
using System.Threading;
using System.Web.UI;
using Newtonsoft.Json;
using System.Data;

namespace CRM.Controllers
{

    [ValidateState]
    public class CRMController : Controller
    {
        //ClientSetting clientSetting = new ClientSetting();

        public ViewResult Home()
        {
            //try
            //{
            //    //Session["MobileNumber"] = null;  
            //}
            //catch (Exception exp)
            //{
            //    CRMLogger.WriteException(Session["UserName"].ToString(), this.ControllerContext, exp);
            //}
            return View();
        }

        public string DOBSplit(string DOB)
        {
            string dobDate = string.Empty;
            var Year=string.Empty;
            var Month = string.Empty;
            var date = string.Empty;
            try
            {
                if (DOB != null)
                {
                    Year = DOB.Substring(0, 4);
                    Month = DOB.Substring(4, 2);
                    date = DOB.Substring(6, 2);
                    dobDate = date + "/" + Month + "/" + Year;
                }
                else
                {
                    dobDate = string.Empty;
                }
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                Year = null;
                Month = null;
                date = null;
            }
            return dobDate;
        }

        public PartialViewResult MenuCategory()
        {
            List<Category_Menu> lstCatgItem = new List<Category_Menu>();
            try
            {
                lstCatgItem = (List<Category_Menu>)Session["UserMenu"];
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMController-MenuCategory- " + this.ControllerContext, ex);
            }
            return PartialView(lstCatgItem);
        }

        public ActionResult MenuItem(string catgID)
        {
            List<Menu_Item> lstMenuItem = new List<Menu_Item>();
            List<UserFavorites> lstUserFavs = new List<UserFavorites>();
            List<Category_Menu> lstCatMenu = new List<Category_Menu>();
            int i = 0;
            int j = 0;
            try
            {
                if (catgID != "CRM_MenuFavorite")
                {
                    lstCatMenu = (List<Category_Menu>)Session["UserMenu"];
                    lstUserFavs = (List<UserFavorites>)Session["UserFavMenu"];
                    lstMenuItem.AddRange(lstCatMenu.Where(m => m.catgID == catgID).SelectMany(n => n.menuItems));
                    for (i = 0; i < lstMenuItem.Count(); i++)
                    {
                        for (j = 0; j < lstUserFavs.Count(); j++)
                        {
                            if (lstMenuItem[i].menuURL == lstUserFavs[j].subCatUrl)
                            {
                                lstMenuItem[i].FavMenu = true;
                            }
                        }
                    }
                }
                else
                {
                    return RedirectToAction("MenuFavorite", "CRM");
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                lstUserFavs = null;
                lstCatMenu = null;
                i = 0;
                j = 0;
            }
            return PartialView(lstMenuItem);
        }

        public ActionResult ByPass(string msisdn, string option, string ContActionName)
        {
            LoginCRM loginCRM = new LoginCRM();
            string mode = "Bypass";

            var result = "";
            if (ContActionName == "AllInOnePurchase")
            {
                var jsonResp = ValidateSubscriber("2", msisdn, string.Empty, string.Empty, 0, string.Empty, mode);
                dynamic Data = jsonResp as JsonResult;
                result = Data.Data.Value;
               
            }
            // 4925
            else if (ContActionName == "CRMMULTITAB")
            {
                var jsonResp = ValidateSubscriber(option, msisdn, string.Empty, string.Empty, 0, string.Empty, mode);
                option = null;
                dynamic Data = jsonResp as JsonResult;
                result = Data.Data.Value;
                ContActionName = null;
                if (result != "0")
                {
                Session["CRMMULTITAB"] = "1";
                    return RedirectToAction("Home", "CRM");
                }
                else
                {
                    Session["CRMMULTITAB"] = "0";
                }
            }
            else
            {
                var jsonResp = ValidateSubscriber(string.Empty, msisdn, string.Empty, string.Empty, 0, string.Empty, mode);
                dynamic Data = jsonResp as JsonResult;
                result = Data.Data.Value;
            }
            try
            {
                if (result == "0")
                {
                    loginCRM.MobileValidated = true;
                    loginCRM.mode = "1";

                    if (!string.IsNullOrEmpty(option))
                    {
                        loginCRM.mode = "2";
                        loginCRM.option = option;
                    }
                    loginCRM.ContActionName = ContActionName;

                    if (!string.IsNullOrEmpty(ContActionName))
                    {
                        loginCRM.mode = "3";
                    }
                    return RedirectToAction("Dashboard", "CRM", loginCRM);
                }
                else
                {
                    Session["TacticalDesktopMSISDN"] = msisdn;
                    return RedirectToAction("Home", "CRM");
                }
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMController-ByPass- " + this.ControllerContext, exp);
                return RedirectToAction("Dashboard", "CRM", loginCRM);
            }
            finally
            {
                mode = string.Empty;
                result = null;
            }
        }

        public JsonResult ValidateSubscriber(string loadType, string loadParameter, string LoadMSISDNNumber, string PAType, int? ID, string PageName, string Loginmode)
        {
            ClientSetting clientSetting = new ClientSetting();
            SelectListItem vsItem = new SelectListItem();
            GetMSISDNResponse objCrmResp = new GetMSISDNResponse();
            Postpaidsubscriber accountReq = new Postpaidsubscriber();
            GetMSISDNRequest objGetMSISDNReq = new GetMSISDNRequest();
            ValidateSubscriber accountResp = new ValidateSubscriber();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            Decimal decMainBalance = 0;
            #region ValidateSubscriber
            try
            {
                accountReq.CountryCode = clientSetting.countryCode;
                accountReq.BrandCode = clientSetting.brandCode;
                accountReq.LanguageCode = clientSetting.langCode;
                // 4836
                Session["CRMMULTITAB"] = "0";
                Session["CheckloadSubscriber"] = loadType;
                #region Switch Cases
                switch (loadType)
                {
                    case "1":
                        accountReq.MSISDN = loadParameter;
                     
                        break;
                    case "2":
                        accountReq.ICCID = loadParameter;
                    
                        break;
                    case "3":
                        accountReq.IMSI = loadParameter;
                  
                        break;
                    case "4":
                        accountReq.PukCode = loadParameter;
                        break;
                    case "5":
                        accountReq.MHA = loadParameter;
                        break;
                    case "6":
                        accountReq.AccountNo = loadParameter;
                        break;
                    case "7":
                        accountReq.EmailID = loadParameter;
                        break;
                    case "8":
                        accountReq.RetailerID = loadParameter;
                        break;
                    case "9":
                        objGetMSISDNReq.FiscalTaxCode = loadParameter;
                        break;
                    case "10":
                        objGetMSISDNReq.VATNumber = loadParameter;
                        break;
                    default:
                        accountReq.MSISDN = loadParameter;
                        break;
                }
                #endregion
                if (LoadMSISDNNumber == string.Empty && (accountReq.EmailID != null || accountReq.PukCode != null || objGetMSISDNReq.FiscalTaxCode != null || objGetMSISDNReq.VATNumber != null))
                {
                    objGetMSISDNReq.Email = accountReq.EmailID;
                    objGetMSISDNReq.PukCode = accountReq.PukCode;
                    objGetMSISDNReq.CountryCode = accountReq.CountryCode;
                    objGetMSISDNReq.BrandCode = accountReq.BrandCode;
                    objGetMSISDNReq.LanguageCode = accountReq.LanguageCode;
                    //objGetMSISDNReq.FiscalTaxCode=objGetMSISDNReq.FiscalTaxCode
                    
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    objCrmResp = serviceCRM.CRMGetMSISDNByPukEmail(objGetMSISDNReq);
                    #region GetMSISDNByPUKEmail
                    if (objCrmResp.reponseDetails.ResponseCode == "0")
                    {
                        if (objCrmResp.SubscriberNo.Count() > 0)
                        {
                            if (objCrmResp.SubscriberNo.Count() == 1)
                            {
                                LoadMSISDNNumber = objCrmResp.SubscriberNo[0].MSISDN;
                                accountReq.MSISDN = LoadMSISDNNumber;
                                accountReq.PukCode = string.Empty;
                                accountReq.EmailID = string.Empty;
                            }
                            else
                            {
                                return Json(objCrmResp, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            objCrmResp.reponseDetails.ResponseDesc = @Resources.SubscriberResources.InvalidSubscriber;
                            objCrmResp.reponseDetails.ResponseCode = "4";
                            //return Json(vsItem, JsonRequestBehavior.AllowGet);
                            return Json(objCrmResp, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        objCrmResp.reponseDetails.ResponseDesc = @Resources.SubscriberResources.InvalidSubscriber;
                        objCrmResp.reponseDetails.ResponseCode = "4";
                        return Json(objCrmResp, JsonRequestBehavior.AllowGet);
                    }
                    #endregion
                }
                #region Assigning Values to AccountRequest Object
                if (LoadMSISDNNumber != string.Empty)
                {
                    accountReq.MSISDN = LoadMSISDNNumber;
                    accountReq.PukCode = string.Empty;
                    accountReq.EmailID = string.Empty;
                }
                if (loadType == "MSISDN")
                {
                    accountReq.MSISDN = loadParameter;
                    accountReq.PukCode = string.Empty;
                    accountReq.EmailID = string.Empty;
                }
                if (SettingsCRM.countryCode.ToUpper() == "BRA")
                {
                    accountReq.userId = Session["UserName"].ToString();
                    if (Loginmode == "Bypass")
                    {
                        accountReq.LoginMode = "Bypass";
                    }
                }
                accountReq.ATR_ID = Session["ATR_ID"].ToString(); //#region FRR : 4376 : ATR_ID : V_1.1.8.0
                accountReq.IsRootUser = Session["IsRootUser"].ToString(); //#region FRR : 4376 : ATR_ID : V_1.1.8.0
                #endregion

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                accountResp = serviceCRM.ValidatePrepaidPostpaidSubscriber(accountReq);
                //5022
                Session["Alternativecontactno"] = accountResp.Alternatecontactno;

                //POF-6007
                Session["IScustomerlegalswap"] = accountResp.IScustomerlegalsimswap;
                //POF-6129
                Session["CheckOTPVerificationprocess"] = "FALSE";
                //6632
                Session["SubscriberLanguageCode"] = accountResp.Subscriber_Lang_code;
                if (accountResp != null && accountResp.Response != null && accountResp.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Services_" + accountResp.Response.ResponseCode);
                    vsItem.Text = string.IsNullOrEmpty(errorInsertMsg) ? accountResp.Response.ResponseDesc : errorInsertMsg;
                }
                #region Comments Section
                // if (!string.IsNullOrEmpty(accountResp.RetailerID))
                //if (loadType == "8")
                //{
                //    //------------3314-----------gopi2296-------------------
                //    #region Session Value Empty

                //    Session["ICCID"] = string.Empty;
                //    Session["IMSI"] = string.Empty;
                //    Session["MaskMode"] = string.Empty;
                //    Session["PlanId"] = string.Empty;
                //    Session["PUK"] = string.Empty;
                //    Session["AnatelID"] = string.Empty;
                //    LoadAudittrail();
                //    Session["Verify"] = string.Empty;
                //    Session["SIMSTATUS"] = string.Empty;
                //    Session["SUBSTATUS"] = string.Empty;
                //    Session["SUBSTYPE"] = string.Empty;
                //    Session["Msisdnzipcode"] = string.Empty;
                //    Session["LIFECYCLESTATE"] = string.Empty;
                //    Session["PlanName"] = string.Empty;
                //    Session["DISCOUNT_CODE_AVAILABLE"] = string.Empty;
                //    Session["eMailID"] = string.Empty;
                //    Session["TopupInd"] = string.Empty;
                //    Session["commonReg"] = string.Empty;
                //    Session["NumberLockerStatus"] = string.Empty;
                //    Session["FamilyAccID"] = string.Empty;
                //    Session["PAType"] = string.Empty;
                //    Session["PAId"] = string.Empty;
                //    Session["PageName"] = string.Empty;
                //    Session["PA4URL"] = string.Empty;
                //    Session["SubscriberTitle"] = string.Empty;
                //    Session["SubscriberName"] = string.Empty;
                //    Session["BillCycleID"] = string.Empty;
                //    Session["isRetailer"] = string.Empty;
                //    Session["DOB"] = string.Empty;
                //    Session["Pin"] = string.Empty;
                //    Session["CPIN"] = string.Empty;
                //    Session["SwapMSISDN"] = string.Empty;

                //    #endregion

                //    Session["RetailerID"] = accountResp.RetailerID;
                //    Session["ContactPerson"] = accountResp.ContactPerson;
                //    Session["ContactMobile"] = accountResp.ContactMobile;
                //    Session["ContactEmail"] = accountResp.ContactEmail;
                //    Session["ContactHouseNo"] = accountResp.ContactHouseNo;
                //    Session["ContactStreetNo"] = accountResp.ContactStreetNo;
                //    Session["ContactAddress1"] = accountResp.ContactAddress1;
                //    Session["ContactAddress2"] = accountResp.ContactAddress2;
                //    Session["ContactCityName"] = accountResp.ContactCityName;
                //    Session["ContactState"] = accountResp.ContactState;
                //    Session["ContactCountry"] = accountResp.ContactCountry;
                //    Session["ContactPostcode"] = accountResp.ContactPostcode;

                //    Session["MobileNumber"] = accountResp.ContactMobile;
                //    Session["isPrePaid"] = accountResp.isPrepaid ? "1" : "0";

                //    vsItem.Value = accountResp.Response.ResponseCode;
                //    vsItem.Text = accountResp.Response.ResponseDesc;
                //    //------------3314-----------gopi2296-------------------
                //}
                //else
                //{
                //}
                #endregion
                #region accountResp
                if (accountResp.Response.ResponseCode == "0") //&& (string.IsNullOrEmpty(Convert.ToString(Session["MobileNumber"])) || Session["MobileNumber"] == accountResp.MSISDN)
                {
                    string PAID = string.Empty;

                    #region FRR 4925
                    if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                    {
                        if (PAType != string.Empty && PAType != null && ID != 0)
                        {
                            PAType = PAType.ToUpper();
                            PAID = ID.ToString();
                        }
                        else
                        {
                            PAType = "";
                        }

                        if (Session["SessionsampleDict"] != null)
                        {
                            Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                            try
                            {
                               
                                localDict.Add(accountResp.ICCID, new MultitabResponse { MSISDN = accountResp.MSISDN, MainBalance = accountResp.MainBalance, DOB = accountResp.DOB, FamilyAccID = accountResp.FamilyAccID, ContactEmail = accountResp.eMailId, IMSI = accountResp.IMSI, LIFECYCLESTATE = accountResp.LIFECYCLESTATE, PUK = accountResp.PUK, IsPostpaid = accountResp.isPrepaid, SIMSTATUS = accountResp.SIMSTATUS, RetailerID = accountResp.RetailerID, SwapMSISDN = accountResp.swapMSISDN, Mask = accountResp.MaskMode, PAType = PAType, PAID = PAID, SIMCategory = accountResp.GO_ONLINE, isRetailer = accountResp.isRetailer, existingStateEnt = "", existingStateInfo = "" });
                                Session["SessionsampleDict"] = localDict;

                                if (localDict.ContainsKey(accountResp.ICCID))
                                {
                                    Session["MobileNumber"] = accountResp.ContactMobile;
                                    Session["MobileNumber"] = accountResp.MSISDN;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (PAType == "SIM BLOCK" || PAType == "CANCEL AUTO RENEWAL")
                                {
                                    localDict[accountResp.ICCID] = new MultitabResponse { MSISDN = accountResp.MSISDN, MainBalance = accountResp.MainBalance, DOB = accountResp.DOB, FamilyAccID = accountResp.FamilyAccID, ContactEmail = accountResp.eMailId, IMSI = accountResp.IMSI, LIFECYCLESTATE = accountResp.LIFECYCLESTATE, PUK = accountResp.PUK, IsPostpaid = accountResp.isPrepaid, SIMSTATUS = accountResp.SIMSTATUS, RetailerID = accountResp.RetailerID, SwapMSISDN = accountResp.swapMSISDN, Mask = accountResp.MaskMode, PAType = PAType, PAID = PAID, SIMCategory = accountResp.GO_ONLINE, isRetailer = accountResp.isRetailer, existingStateEnt = "", existingStateInfo = "" };

                                }
                                else
                                {
                                vsItem.Text = ex.Message;
                                vsItem.Value = "1";
                                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMCOntroller-ValidateSubscriber- " + this.ControllerContext, ex);
                           }
                            }
                        }
                        else
                        {


                            Dictionary<string, MultitabResponse> localDict = new Dictionary<string, MultitabResponse>();
                            localDict.Add(accountResp.ICCID, new MultitabResponse { MSISDN = accountResp.MSISDN, MainBalance = accountResp.MainBalance, DOB = accountResp.DOB, FamilyAccID = accountResp.FamilyAccID, ContactEmail = accountResp.eMailId, IMSI = accountResp.IMSI, LIFECYCLESTATE = accountResp.LIFECYCLESTATE, PUK = accountResp.PUK, IsPostpaid = accountResp.isPrepaid, SIMSTATUS = accountResp.SIMSTATUS, RetailerID = accountResp.RetailerID, SwapMSISDN = accountResp.swapMSISDN, Mask = accountResp.MaskMode, PAType = PAType, PAID = PAID, SIMCategory = accountResp.GO_ONLINE, isRetailer = accountResp.isRetailer, existingStateEnt = "", existingStateInfo = "" });
                            Session["SessionsampleDict"] = localDict;
                            Session["MobileNumber"] = accountResp.ContactMobile;
                            Session["MobileNumber"] = accountResp.MSISDN;
                        }
                    }
                    else
                    {
                        Session["MobileNumber"] = accountResp.MSISDN;
                    }

                    #endregion

                    #region FRR 4739
                    if(clientSetting.preSettings.EnableOBADowngrade.ToUpper() == "TRUE")
                    {
                        Session["OBABundle"] = accountResp.OBABundle;
                        Session["OBACreditout"] = accountResp.OBAcreditLimit;
                    }
                    else
                    {
                        Session["OBABundle"] = string.Empty;
                        Session["OBACreditout"] = string.Empty;
                    }
                    #endregion

                    if (!string.IsNullOrEmpty(accountResp.RetailerID))
                    {
                        Session["RetailerID"] = accountResp.RetailerID;
                        Session["ContactPerson"] = accountResp.ContactPerson;
                        Session["ContactMobile"] = accountResp.ContactMobile;
                        Session["ContactEmail"] = accountResp.ContactEmail;
                        Session["ContactHouseNo"] = accountResp.ContactHouseNo;
                        Session["ContactStreetNo"] = accountResp.ContactStreetNo;
                        Session["ContactAddress1"] = accountResp.ContactAddress1;
                        Session["ContactAddress2"] = accountResp.ContactAddress2;
                        Session["ContactCityName"] = accountResp.ContactCityName;
                        Session["ContactState"] = accountResp.ContactState;
                        Session["ContactCountry"] = accountResp.ContactCountry;
                        Session["ContactPostcode"] = accountResp.ContactPostcode;
                    }
                    else
                    {
                        Session["RetailerID"] = string.Empty;
                        Session["ContactPerson"] = string.Empty;
                        Session["ContactMobile"] = string.Empty;
                        Session["ContactEmail"] = string.Empty;
                    }
                    
                    Session["isPrePaid"] = accountResp.isPrepaid ? "1" : "0";

                    if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() == "TRUE")
                    {
                        Session["isPrePaid"] = "1";
                    }

                    if (accountResp.SUBSTYPE == "2")
                    {
                        // 4788
                        Session["Postpaidsubscriber"] = "isPostpaid";
                    }
                    else
                    {
                        Session["Postpaidsubscriber"] = "isprepaid";
                    }

                   
                    Session["isPrePaid"] = accountResp.isPrepaid ? "1" : "0";
                    Session["ICCID"] = accountResp.ICCID.Trim();
                    Session["IMSI"] = accountResp.IMSI.Trim();
                    Session["MaskMode"] = accountResp.MaskMode;
                    Session["PlanId"] = accountResp.PlanId;
                    Session["PUK"] = accountResp.PUK.Trim();
                    //4808
                    Session["IsRealMsisdn"] = accountResp.IsRealMsisdn;

                    if (accountReq.LoginMode != "Bypass")
                    {
                        Session["AnatelID"] = accountResp.anatelId;
                        LoadAudittrail();
                    }
                    Session["Verify"] = accountResp.Verify;
                    Session["SIMSTATUS"] = accountResp.SIMSTATUS;
                    Session["SUBSTATUS"] = accountResp.SUBSTATUS;
                    Session["SUBSTYPE"] = accountResp.SUBSTYPE;
                    Session["Msisdnzipcode"] = accountResp.Msisdnzipcode;
                    Session["AccountNumber"] = accountResp.AccountNumber;
                    try
                    {
                        Session["LIFECYCLESTATE"] = accountResp.isPrepaid ? accountResp.LIFECYCLESTATE : SettingsCRM.postLyfeCycStatus.FirstOrDefault(lcs => lcs.Attribute("ID").Value == accountResp.LIFECYCLESTATE).Value;
                    }
                    catch
                    {
                        Session["LIFECYCLESTATE"] = accountResp.LIFECYCLESTATE;
                    }
                    if (clientSetting.mvnoSettings.enableregularSLCM == "0")
                    {
                        string strSLCM = string.Empty;
                        try
                        {
                            strSLCM = SettingsCRM.slcmLifeCycleState.FirstOrDefault(lcs => lcs.Attribute("ID").Value == accountResp.LIFECYCLESTATE.ToLower()).Value;
                            Session["SLCMLIFECYCLESTATE"] = Convert.ToString(strSLCM);
                        }
                        catch
                        {
                            Session["SLCMLIFECYCLESTATE"] = accountResp.LIFECYCLESTATE;
                        }
                        finally
                        {
                            strSLCM = string.Empty;
                        }
                    }
                    if (Convert.ToString(clientSetting.mvnoSettings.TicketingToolSMSNotifyEnhance).ToLower() == "on") //#region FRR 3922
                    {
                        TempData["languageId"] = accountResp.languageIDforTicketting;
                        TempData["Language"] = accountResp.Language;
                        TempData.Keep("languageId");
                        TempData.Keep("Language");
                    } //#endregion FRR 3922
                    Session["PlanName"] = accountResp.PlanName;
                    Session["DISCOUNT_CODE_AVAILABLE"] = accountResp.DISCOUNT_CODE_AVAILABLE;
                    Session["eMailID"] = accountResp.eMailId;
                    Session["TopupInd"] = accountResp.TOPUPIND;
                    Session["commonReg"] = accountResp.commonReg;
                    Session["NumberLockerStatus"] = accountResp.NumberLockerStaus;
                    Session["FamilyAccID"] = accountResp.FamilyAccID;
                    Session["ModeReg"] = accountResp.ModeReg;
                    Session["EditRestrictStatus"] = accountResp.EditRestrictStatus;
                    Session["NewChild"] = string.Empty;
                    if (PAType != string.Empty && PAType != null)
                    {
                        Session["PAType"] = PAType.ToUpper();
                        Session["PAId"] = ID;
                        Session["PageName"] = PageName;
                        Session["PA4URL"] = PageName;
                    }
                    else
                    {
                        Session["PAType"] = string.Empty;
                        Session["PAId"] = string.Empty;
                        Session["PA4URL"] = string.Empty;
                    }
                    Session["SubscriberTitle"] = accountResp.Title;
                    Session["SubscriberName"] = (string.IsNullOrEmpty(accountResp.FirstName) ? " " : accountResp.FirstName) + "|" + (string.IsNullOrEmpty(accountResp.LastName) ? " " : accountResp.LastName);
                    Session["DOB"] = accountResp.DOB;
                    Session["BillCycleID"] = accountResp.BillCycleId;
                    Session["isRetailer"] = accountResp.isRetailer;
                    Session["SwapMSISDN"] = accountResp.swapMSISDN;
                    Session["PORTOUT"] = accountResp.PortOut;
                    Session["DedicatedBalance"] = accountResp.DedicatedBalance;
                    decMainBalance = !string.IsNullOrEmpty(accountResp.MainBalance) ? Math.Round(Convert.ToDecimal(accountResp.MainBalance), Convert.ToInt32(clientSetting.mvnoSettings.decimalLimit)) : 0;
                    Session["MainBalance"] = decMainBalance;
                    Session["TotalBalance"] = Convert.ToString(decMainBalance + (!string.IsNullOrEmpty(accountResp.DedicatedBalance) ? Math.Round(Convert.ToDecimal(accountResp.DedicatedBalance), Convert.ToInt32(clientSetting.mvnoSettings.decimalLimit)) : 0));
                    //FRR-3485  
                    Session["InvoiceVatNo"] = accountResp.InvoiceVatNo;
                    if (clientSetting.mvnoSettings.enableCurrencyConversion.ToUpper() == "TRUE" && !string.IsNullOrEmpty(Convert.ToString(Session["TotalBalance"])))
                    {
                        Session["TotalBalance"] = Convert.ToString(Session["TotalBalance"]).Replace(".", clientSetting.mvnoSettings.currencyConversionValue);
                    }
                    string Dob = accountResp.DOB;
                    if (!string.IsNullOrEmpty(accountResp.DOB))
                    {
                        string Date = string.Empty;
                        string Month = string.Empty;
                        string Year = string.Empty;
                        string dateDOB = accountResp.DOB;
                        string format = clientSetting.mvnoSettings.dateTimeFormat.ToLower();
                        string[] SplitDOB;
                        try
                        {
                            if (dateDOB != null)
                            {
                                SplitDOB = dateDOB.Split('-', '/');
                                Date = SplitDOB[0].ToString();
                                if (Date.Length != 2)
                                {
                                    Date = "0" + Date;
                                }
                                Month = SplitDOB[1].ToString();
                                if (Month.Length != 2)
                                {
                                    Month = "0" + Month;
                                }
                                Year = SplitDOB[2].ToString();
                                Dob = format.Replace("dd", Date).Replace("mm", Month).Replace("yyyy", Year);
                            }
                            else
                            {
                                Dob = string.Empty;
                            }
                        }
                        catch
                        {
                            Dob = string.Empty;
                        }
                        finally
                        {
                            Date = string.Empty;
                            Month = string.Empty;
                            Year = string.Empty;
                            dateDOB = string.Empty;
                            format = string.Empty;
                            SplitDOB = null;
                        }
                    }
                    else
                    {
                        Dob = string.Empty;
                    }
                    Session["DOB"] = Dob;
                    Session["Pin"] = accountResp.Pin;
                    Session["CPIN"] = accountResp.CPIN;
                    #region FRR : 4357 : ATR_ID : V_1.1.9.0
                    Session["SIM_CATEGORY"] = accountResp.GO_ONLINE;
                    Session["ACTIVE_OBA_FLAG"] = accountResp.ACTIVEOBAFLAG;
                    Session["FAMILY_STATUS"] = accountResp.FAMILYSTATUS;
                    #endregion 
                    vsItem.Value = "0";
                }
                else if (accountResp.Response.ResponseCode == "1025" || accountResp.Response.ResponseCode == "1000" || accountResp.Response.ResponseCode == "1011")
                {
                    // vsItem.Text = @Resources.SubscriberResources.ServiceDown;
                    vsItem.Value = "1";
                }
                else if (accountResp.Response.ResponseCode == "1010" || accountResp.Response.ResponseCode == "1" || accountResp.Response.ResponseCode == "4" || accountResp.Response.ResponseCode == "003" || accountResp.Response.ResponseCode == "5" || accountResp.Response.ResponseCode == "55")//6007
                {
                    // vsItem.Text = @Resources.SubscriberResources.InvalidSubscriber;
                    vsItem.Value = "4";
                }
                else if (accountResp.Response.ResponseCode == "1016")
                {
                    //vsItem.Text = "MSISDN has been in black list";
                    vsItem.Value = "2";
                }
                else if (accountResp.Response.ResponseCode == "1009")
                {
                    // vsItem.Text = "Subscriber is first used or invalid";
                    vsItem.Value = "2";
                }
                else if (accountResp.Response.ResponseCode == "1100")
                {
                    //vsItem.Text = "Subscriber not activated";
                    vsItem.Value = "2";
                }
                else if (accountResp.Response.ResponseCode == "002")
                {
                    //vsItem.Text = "MSISDN MANDATORY";
                    vsItem.Value = "2";
                }
                else if (accountResp.Response.ResponseCode == "001")
                {
                    //  vsItem.Text = "OPERATION UNSUCCESSFUL";
                    vsItem.Value = "2";
                }
                else if (accountResp.Response.ResponseCode == "15")
                {
                    //   vsItem.Text = "Networkid is mismatched";
                    vsItem.Value = "2";
                }
                else if (accountResp.Response.ResponseCode == "14")
                {
                    // vsItem.Text = "Msisdn is ported out";
                    vsItem.Value = "2";
                }
                //else if (accountResp.Response.ResponseCode == "0")
                //{                        
                //    vsItem.Text = "Kindly use same msisdn";
                //    vsItem.Value = "2";
                //}

                //6431            
                else if (accountResp.Is_PP_PortedoutCustomer =="1" && ! string.IsNullOrEmpty(accountResp.Pendingdue) && Convert.ToDouble(accountResp.Pendingdue) > 0)
                {
                    vsItem.Text = accountResp.Pendingdue_unit +"  "+accountResp.Pendingdue;
                    vsItem.Value = "11";
                }
                else
                {
                    vsItem.Text = accountResp.Response.ResponseDesc;
                    vsItem.Value = "2";
                }
                #endregion
                #region Server Date Update
                CRMLogger.ServerDateTimeUpdate(clientSetting.countryCode, clientSetting.brandCode);
                Session["ServerDateTime"] = clientSetting.mvnoSettings.ServerDate + "/" + clientSetting.mvnoSettings.ServerMonth + "/" + clientSetting.mvnoSettings.ServerYear;
                #endregion
                return Json(vsItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                vsItem.Text = ex.Message;
                vsItem.Value = "1";
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMCOntroller-ValidateSubscriber- " + this.ControllerContext, ex);
                return Json(vsItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                Dispose(true);
                GC.SuppressFinalize(objCrmResp);
                Dispose(true);
                GC.SuppressFinalize(vsItem);
                accountReq = null;
                objGetMSISDNReq = null;
                accountResp = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            #endregion
        }

        public ActionResult Dashboard(string mode, string option, string ContActionName)
        {
            try
            {
                Session["RegHistory"] = null;
                if (Request.IsAjaxRequest())
                {
                    if (Session["MobileNumber"] != null)
                    {
                        ViewBag.LoginMode = mode;
                        //login mode for uganda
                        ViewBag.ContActionName = ContActionName;
                        if (mode == "2")
                            ViewBag.Option = option;
                        return View();
                    }
                    else
                    {
                        return RedirectToAction("Home");
                    }
                }
                else
                {
                    return RedirectToAction("SessionExpired", "Login");
                }
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMController-Dashboard- " + this.ControllerContext, exp);
            }
            return View();
        }

        public ActionResult RenderPage(string linkID)
        {
            try
            {
                List<string> catgMenu = new List<string>(linkID.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries));
                if (catgMenu.Count == 2)
                {
                    return RedirectToAction(catgMenu[1], catgMenu[0]);
                }
                else
                {
                    return RedirectToAction("MenuFavorite", "CRM");
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMController-RenderPage- " + this.ControllerContext, ex);
            }
            return View();
        }

        public PartialViewResult MenuFavorite()
        {
            ClientSetting clientSetting = new ClientSetting();
            List<Menu_Item> lstMenuItem = new List<Menu_Item>();
            CategoryItemsResponse catgItemsResp = new CategoryItemsResponse();
            UserFavoritesMapPResponse userFavMapResp = new UserFavoritesMapPResponse();
            List<Category_Menu> lstCatItem = new List<Category_Menu>();
            List<UserFavorites> lstUserFavs = new List<UserFavorites>();
            CategoryItemsRequest catgItemsReq = new CategoryItemsRequest();
            UserFavoritesMapRequest userFavReq = new UserFavoritesMapRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                // 4836
                if (Session["SemiVarificationSuccess"] == null)
                    Session["SemiVarificationSuccess"] = "0";
                //if (Session["UserMenu"] == null || Session["UserFavMenu"] == null)
                //{
                    catgItemsReq.CountryCode = clientSetting.countryCode;
                    catgItemsReq.BrandCode = clientSetting.brandCode;
                    catgItemsReq.LanguageCode = clientSetting.langCode;
                    catgItemsReq.roleCatID = Convert.ToString(Session["UserGroupID"]);

                    userFavReq.CountryCode = clientSetting.countryCode;
                    userFavReq.BrandCode = clientSetting.brandCode;
                    userFavReq.LanguageCode = clientSetting.langCode;
                    userFavReq.ROLE_CAT_ID = Convert.ToString(Session["UserGroupID"]);
                    userFavReq.MODE = "GET";
                    //------------------ gopi2296 V_1.0.23.0----------------------------
                    userFavReq.UserID = Convert.ToString(Session["UserID"]);
                    if (@clientSetting.mvnoSettings.userManagementType.ToUpper() == "ON")
                    {
                        userFavReq.UserRoleID = "";
                    }
                    else
                    {
                        userFavReq.UserRoleID = System.Configuration.ConfigurationManager.AppSettings["RootUserIDs"].Trim();
                    }
                    //------------------------ End -------------------------------------

                    if (Session["isPrePaid"] != null && Session["isPrePaid"].ToString() == "1")
                    {
                        catgItemsReq.simType = "Pre";
                        userFavReq.simType = "Pre";
                    }
                    else
                    {
                        catgItemsReq.simType = "Post";
                        userFavReq.simType = "Post";
                    }
                    //----------FRR 3314---------------Gopi2296-------------------------
                    if (!string.IsNullOrEmpty(Session["RetailerID"].ToString()))
                    {
                        catgItemsReq.RetailerID = Session["RetailerID"].ToString();
                        userFavReq.RetailerID = Session["RetailerID"].ToString();
                    }
                    else
                    {
                        catgItemsReq.RetailerID = "";
                        userFavReq.RetailerID = "";
                    }

                    //POSTPAID CUSTOMER ALLOW ONLY REFUND MODULE SCOPE PID
                    catgItemsReq.IS_POSTPAID = Convert.ToString(Session["isPrePaid"]);

                    //----------FRR 3314---------------Gopi2296-------------------------
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    catgItemsResp = serviceCRM.CRMMenuResource(catgItemsReq);
                    userFavMapResp = serviceCRM.CRMUserFavotitesMap(userFavReq);
                    // 4836
                    Session["UserMenu"] = lstCatItem = catgItemsResp.categoryMenus;
                    Session["UserFavMenu"] = lstUserFavs = userFavMapResp.UserFavorites;
               // }
                //else
                //{
                //    lstCatItem = (List<Category_Menu>)Session["UserMenu"];
                //    lstUserFavs = (List<UserFavorites>)Session["UserFavMenu"];
                //}

                lstMenuItem.AddRange(lstCatItem.SelectMany(c => c.menuItems.Where(m => lstUserFavs.Select(u => u.subCatID).Contains(m.menuID))).GroupBy(x => x.menuID).Select(y => y.First()));
                return PartialView(lstMenuItem);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMController-MenuFavorite- " + this.ControllerContext, ex);
                return PartialView(lstMenuItem);
            }
            finally
            {
                clientSetting = null;
                catgItemsResp = null;
                userFavMapResp = null;
                lstCatItem = null;
                lstUserFavs = null;
                catgItemsReq = null;
                userFavReq = null;
                serviceCRM = null;
            }
        }

        public string GetDateconvertion(string strDate, string strIOdate, Boolean isRequest)
        {
            ClientSetting clientSetting = new ClientSetting();
            string strInputDate = string.Empty;
            string strSwap = string.Empty;
            Int64 IValidate = 0;
            string strValidateDate = string.Empty;
            string strDay, strMonth, strYear;
            try
            {
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
                            return strDate;
                        }
                        else
                        {
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
                                return strIOdate;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException("UserName", "CRMController-GetDateconvertion- " + this.ControllerContext, ex);
                throw ex;
            }
            finally
            {
                clientSetting = null;
                strInputDate = string.Empty;
                strSwap = string.Empty;
                strValidateDate = string.Empty;
                strDay = string.Empty;
                strMonth = string.Empty;
                strYear = string.Empty;
            }
            if (string.IsNullOrEmpty(strDate))
                return string.Empty;
            else
            {
                return string.Empty;
            }
        }

        private string Dateconvert(string DateValue, string Index = "1,0,2")
        {
            ClientSetting clientSetting = new ClientSetting();
            string Date = string.Empty;
            string Month = string.Empty;
            string Year = string.Empty;
            string[] Indexsp = Index.Split(',');
            string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            string[] SplitDOB;
            try
            {
                if (DateValue != null)
                {
                    SplitDOB = DateValue.Split('-', '/');
                    Date = SplitDOB[Convert.ToInt16(Indexsp[0])].ToString();
                    if (Date.Length != 2 && Date.Length != 0)
                    {
                        Date = "0" + Date;
                    }
                    Month = SplitDOB[Convert.ToInt16(Indexsp[1])].ToString();
                    if (Month.Length != 2 && Month.Length != 0)
                    {
                        Month = "0" + Month;
                    }
                    Year = SplitDOB[Convert.ToInt16(Indexsp[2])].ToString();
                }
                else
                {
                    DateValue = string.Empty;
                }

                DateValue = strInputDate.Replace("dd", Date).Replace("mm", Month).Replace("yyyy", Year);
                if (DateValue == "//")
                {
                    DateValue = string.Empty;
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException("UserName", "CRMController-Dateconvert- " + this.ControllerContext, ex);
            }
            finally
            {
                clientSetting = null;
                Date = string.Empty;
                Month = string.Empty;
                Year = string.Empty;
                Indexsp = null;
                strInputDate = string.Empty;
                SplitDOB = null;
            }
            return DateValue;
        }

        public ActionResult PartialErrorMsg()
        {
            return View();
        }


        public ActionResult ErrorMsg(string msg, string FileName)
        {
            CRM.Models.ErrorMsgAlert obj = new CRM.Models.ErrorMsgAlert();
            if (FileName == null || FileName == string.Empty)
            {
                obj.ErrorMsg = msg;
            }
            else
            {
                obj.FileName = FileName;
            }
            return View(obj);
        }

        private string Dateconvert(DateTime DateValue)
        {
            ClientSetting clientSetting = new ClientSetting();
            string ReturnVal = string.Empty;
            string Date = string.Empty;
            string Month = string.Empty;
            string Year = string.Empty;
            string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            try
            {
                if (DateValue != null)
                {
                    Date = Convert.ToString(DateValue.Day);
                    if (Date.Length != 2 && Date.Length != 0)
                    {
                        Date = "0" + Date;
                    }
                    Month = Convert.ToString(DateValue.Month);
                    if (Month.Length != 2 && Month.Length != 0)
                    {
                        Month = "0" + Month;
                    }
                    Year = Convert.ToString(DateValue.Year);
                }

                ReturnVal = strInputDate.Replace("dd", Date).Replace("mm", Month).Replace("yyyy", Year);
                if (ReturnVal == "//")
                {
                    ReturnVal = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ReturnVal = string.Empty;
                CRMLogger.WriteException("UserName", "CRMController-Dateconvert-DateValue- " + this.ControllerContext, ex);
            }
            finally
            {
                clientSetting = null;
                Date = string.Empty;
                Month = string.Empty;
                Year = string.Empty;
                strInputDate = string.Empty;
            }
            return ReturnVal;
        }

        public JsonResult CheckNUmber(string msisdn)
        {
            ClientSetting clientSetting = new ClientSetting();
            ValidateMSISDNRequest accountReq = new ValidateMSISDNRequest();
            ValidateMSISDNResponse accountResp = new ValidateMSISDNResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                accountReq.CountryCode = clientSetting.countryCode;
                accountReq.BrandCode = clientSetting.brandCode;
                accountReq.LanguageCode = clientSetting.langCode;
                accountReq.MSISDN = msisdn;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                accountResp = serviceCRM.CRMValidateMSISDN(accountReq);
                if (accountResp != null && accountResp.reponseDetails != null && accountResp.reponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Services_" + accountResp.reponseDetails.ResponseCode);
                    accountResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? accountResp.reponseDetails.ResponseDesc : errorInsertMsg;
                }

                if (accountResp.reponseDetails.ResponseCode == "0")
                {
                    Session["isPrePaid"] = accountResp.PrePost == "1" ? "1" : "0";
                    accountResp.isPrepaid = accountResp.PrePost == "1";
                    accountResp.reponseDetails.ResponseDesc = @Resources.SubscriberResources.Success;
                    accountResp.reponseDetails.ResponseCode = accountResp.reponseDetails.ResponseCode;
                }
                else if (accountResp.reponseDetails.ResponseCode == "1025" || accountResp.reponseDetails.ResponseCode == "1000" || accountResp.reponseDetails.ResponseCode == "1011" || accountResp.reponseDetails.ResponseCode == "2000")
                {
                    accountResp.reponseDetails.ResponseDesc = @Resources.SubscriberResources.ServiceDown;
                    accountResp.reponseDetails.ResponseCode = "1";
                }
                else if (accountResp.reponseDetails.ResponseCode == "1010" || accountResp.reponseDetails.ResponseCode == "3" || accountResp.reponseDetails.ResponseCode == "4" || accountResp.reponseDetails.ResponseCode == "003" || accountResp.reponseDetails.ResponseCode == "503")
                {
                    accountResp.reponseDetails.ResponseDesc = @Resources.SubscriberResources.InvalidSubscriber;
                    accountResp.reponseDetails.ResponseCode = "3";
                }
                else if (accountResp.reponseDetails.ResponseCode == "1")
                {
                    accountResp.reponseDetails.ResponseDesc = @Resources.ErrorResources.EReg_GBR4;
                    accountResp.reponseDetails.ResponseCode = "4";
                }
                else if (accountResp.reponseDetails.ResponseCode == "14")
                {
                    accountResp.reponseDetails.ResponseDesc = @Resources.ErrorResources.EReg_GBR5;
                    accountResp.reponseDetails.ResponseCode = "5";
                }
                else if (accountResp.reponseDetails.ResponseCode == "1016")
                {
                    //accountResp.reponseDetails.ResponseDesc = "MSISDN has been in black list";
                    accountResp.reponseDetails.ResponseCode = "2";
                }
                else if (accountResp.reponseDetails.ResponseCode == "1009")
                {
                    // accountResp.reponseDetails.ResponseDesc = "Subscriber is first used or invalid";
                    accountResp.reponseDetails.ResponseCode = "2";
                }
                else if (accountResp.reponseDetails.ResponseCode == "1100")
                {
                    // accountResp.reponseDetails.ResponseDesc = "Subscriber not activated";
                    accountResp.reponseDetails.ResponseCode = "2";
                }
                else if (accountResp.reponseDetails.ResponseCode == "002")
                {
                    // accountResp.reponseDetails.ResponseDesc = "MSISDN MANDATORY";
                    accountResp.reponseDetails.ResponseCode = "2";
                }
                else if (accountResp.reponseDetails.ResponseCode == "001")
                {
                    // accountResp.reponseDetails.ResponseDesc = "OPERATION UNSUCCESSFUL";
                    accountResp.reponseDetails.ResponseCode = "2";
                }
                else if (accountResp.reponseDetails.ResponseCode == "3")
                {
                    // accountResp.reponseDetails.ResponseDesc = accountResp.reponseDetails.ResponseDesc;
                    accountResp.reponseDetails.ResponseCode = accountResp.reponseDetails.ResponseCode;
                }
                else
                {
                    accountResp.reponseDetails.ResponseDesc = accountResp.reponseDetails.ResponseDesc;
                    accountResp.reponseDetails.ResponseCode = "2";
                }
                return Json(accountResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMController-CheckNUmber- " + this.ControllerContext, eX);
                return Json(accountResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                clientSetting = null;
                accountReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
                Dispose(true);
                GC.SuppressFinalize(accountResp);
            }
        }

        public JsonResult RedirecttoReg(string isPrePaid)
        {
            CRMResponse objRes = new CRMResponse();
            try
            {
                objRes.ResponseCode = "0";
                objRes.ResponseDesc = "Success";
                Session["isPrePaid"] = isPrePaid;
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMController-RedirecttoReg- " + this.ControllerContext, eX);
            }
            finally
            {
               // objRes = null;
            }
            return Json(objRes, JsonRequestBehavior.AllowGet);
        }

        public PlanChangerResponse CRMPlanChanger(PlanChangerRequest objplanchangerreq)
        {
            ClientSetting clientSetting = new ClientSetting();
            PlanChangerResponse ObjRes = new PlanChangerResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                objplanchangerreq.CountryCode = clientSetting.countryCode;
                objplanchangerreq.BrandCode = clientSetting.brandCode;
                objplanchangerreq.LanguageCode = clientSetting.langCode;
                objplanchangerreq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMPlanChanger(objplanchangerreq);
                if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_PlanChanger_" + ObjRes.ResponseDetails.ResponseCode);
                    ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMController-CRMPlanChanger- " + this.ControllerContext, exp);
            }
            finally
            {
                clientSetting = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return ObjRes;
        }
        public ViewResult TermsConditions(string refFileName)
        {
            ViewBag.refFileName = refFileName;
            return View(ViewBag);
        }

        [HttpPost]
        public string RenderReference(string refFileName)
        {
            StringBuilder sbRefHtml = new StringBuilder();
            try
            {
              sbRefHtml.Append(System.IO.File.ReadAllText(Server.MapPath("~/App_Data/TermsandConditions/" + refFileName + ".html")));
                
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMController-RenderReference- " + this.ControllerContext, ex);
            }
            finally
            {
               // sbRefHtml = null;
            }
            return sbRefHtml.ToString();
        }


        public JsonResult getSterEncryptDecrypt()
        {
            string val = string.Empty;
            string baseUrl = string.Empty;
            List<string> lstStr = new List<string>();
            try
            {
                val = Utility.SterEncryptDecrypt();
                baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');
                lstStr.Add(val);
                lstStr.Add(baseUrl);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException("UserName", "CRMController-getSterEncryptDecrypt- " + this.ControllerContext, ex);
            }
            finally
            {
                val = string.Empty;
                baseUrl = string.Empty;
            }
            return Json(lstStr, JsonRequestBehavior.AllowGet);
        }
        public void LoadAudittrail()
        {
            AuditTrailRequest auditTrailRequest = new AuditTrailRequest();
            LoginController lgnctr = new LoginController();
            HttpContext httpContext;
            try
            {
                httpContext = System.Web.HttpContext.Current;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "LoadAudittrail Start");
                auditTrailRequest.module = "CRM";
                auditTrailRequest.subModule = "Subscriber";
                auditTrailRequest.action = "Load Subscriber";
                auditTrailRequest.description = "Load Subscriber details";
                auditTrailRequest.anatelId = httpContext.Session["AnatelID"] != null ? httpContext.Session["AnatelID"].ToString() : null;
                auditTrailRequest.DescID = "LoadSubscriberSuccess";
                lgnctr.AuditTrailCRM(auditTrailRequest);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "LoadAudittrail End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMController-LoadAudittrail- " + this.ControllerContext, ex);
            }
            finally
            {
                auditTrailRequest = null;
                lgnctr = null;
                httpContext = null;
            }
        }


        //FRR : 4390 ----1.1.8.0  ----subha----- MNP Redirection For UKR
        public JsonResult MNPgetEncryptDecrypt(string msisdn, string mode, string act)
        {
            #region FRR:4390 MNP REDIRECTION AND CONFIG NAME:redirectionUrl and enableMNPRouting
            ClientSetting clientSetting = new ClientSetting();
            var requrl = "";
            string SessionID = string.Empty;
            string returl = string.Empty;
            string enurl = string.Empty;
            string baseUrlID = string.Empty;
            string val = string.Empty;
            try 
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "MNPgetEncryptDecrypt Start");
                mode = mode == null ? "NEW_MNPPORTIN" : mode;
                requrl = Request.UrlReferrer.ToString().TrimEnd('?');
                SessionID = Utility.mthdSessionID();
                CRMLogger.WriteMessage(string.Empty, this.ControllerContext, requrl);
                returl = Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + mode + "|" + Convert.ToString(Session["MobileNumber"]) + "|" + DateTime.Now;
                returl += "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "| ";
                CRMLogger.WriteMessage(string.Empty, this.ControllerContext, returl);
                enurl = Utility.EncryptDecrypt(returl);
                baseUrlID = clientSetting.mvnoSettings.mnpBrand + "|" + "CRM|" + clientSetting.brandCode + "|" + msisdn + "|" + Session["UserName"].ToString() + ",";
                baseUrlID += Session["UserGroup"].ToString() + "|" + mode + "|" + requrl + "?Keyframe=" + enurl + "|";
                baseUrlID += Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                baseUrlID += "|" + SessionID;
                if (clientSetting.countryCode.ToUpper() == "IND" || clientSetting.countryCode.ToUpper() == "POL" || clientSetting.countryCode.ToUpper() == "ESP")
                {
                    if (act == "PORTIN")
                    {
                        baseUrlID = clientSetting.countryCode.ToUpper() + "|" + "CRM" + "|" + clientSetting.brandCode.ToUpper() + "|" + "PORTIN" + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|" + SessionID;
                    }
                    else if (act == "PORTINSEARCH")
                    {
                        baseUrlID = clientSetting.countryCode.ToUpper() + "|" + "CRM" + "|" + clientSetting.brandCode.ToUpper() + "|" + "PORTINSEARCH" + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|" + SessionID;

                    }
                    else
                    {
                    baseUrlID += "|" + "1";
                    }
                }
                // 4749
                else if (clientSetting.countryCode.ToUpper() == "COL")
                {

                    if (act == "PORTIN")
                    {
                        baseUrlID = clientSetting.countryCode.ToUpper() + "|" + "CRM" + "|" + clientSetting.brandCode.ToUpper() + "|" + "PORTIN" + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|" + SessionID;
                    }
                    else if (act == "PORTINSEARCH")
                    {
                        baseUrlID = clientSetting.countryCode.ToUpper() + "|" + "CRM" + "|" + clientSetting.brandCode.ToUpper() + "|" + "PORTINSEARCH" + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|" + SessionID;

                    }
                    else
                    {
                        baseUrlID = clientSetting.countryCode.ToUpper() + "|" + "CRM" + "|" + clientSetting.brandCode.ToUpper() + "|" + "PORTIN" + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|" + SessionID;
                    }
                }
                //5002
                else if (clientSetting.countryCode.ToUpper() == "CHL")
                {
                    if (act == "PORTIN")
                    {
                        baseUrlID = clientSetting.countryCode.ToUpper() + "|" + "CRM" + "|" + clientSetting.brandCode.ToUpper() + "|" + "PORTIN" + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|" + SessionID;
                    }
                    else if (act == "PORTINSEARCH")
                    {
                        baseUrlID = clientSetting.countryCode.ToUpper() + "|" + "CRM" + "|" + clientSetting.brandCode.ToUpper() + "|" + "PORTINSEARCH" + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|" + SessionID;

                    }
                    else
                    {
                        baseUrlID = clientSetting.countryCode.ToUpper() + "|" + "CRM" + "|" + clientSetting.brandCode.ToUpper() + "|" + "PORTIN FEASIBILITY" + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|" + SessionID;
                    }
                }

                //POF-6192 
                else if (string.Equals(clientSetting.countryCode,clientSetting.preSettings.MNPRedirectionCountryCode,StringComparison.OrdinalIgnoreCase))
                {
                    //POF-6283
                    if (clientSetting.countryCode == "MEX")
                    {
                        if (act == "PORTINSEARCH")
                        {
                            baseUrlID = clientSetting.countryCode.ToUpper() + "|" + "CRM" + "|" + clientSetting.brandCode.ToUpper() + "|" + "PORTINSEARCH" + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|" + SessionID + "|" + "ATRID:" + (Session["ATR_ID"] != null ? Session["ATR_ID"].ToString() : String.Empty);
                        }
                        else
                        {
                            baseUrlID = clientSetting.countryCode.ToUpper() + "|" + "CRM" + "|" + clientSetting.brandCode.ToUpper() + "|" + "PORTIN" + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|" + SessionID + "|" + "ATRID:" + (Session["ATR_ID"] != null ? Session["ATR_ID"].ToString() : String.Empty);
                        }

                    }

                    else
                    {
                        if (act == "PORTINSEARCH")
                        {
                            baseUrlID = clientSetting.countryCode.ToUpper() + "|" + "CRM" + "|" + clientSetting.brandCode.ToUpper() + "|" + "PORTINSEARCH" + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|" + SessionID;
                        }
                        else
                        {
                            baseUrlID = clientSetting.countryCode.ToUpper() + "|" + "CRM" + "|" + clientSetting.brandCode.ToUpper() + "|" + "PORTIN" + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|" + SessionID;
                        }
                    }
                   
                }

                else
                {
                    baseUrlID += "|" + "0";
                }
                val = Utility.EncryptDecrypt(baseUrlID);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "MNPgetEncryptDecrypt End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(string.Empty, "CRMController-MNPgetEncryptDecrypt- " + this.ControllerContext, ex);
            }
            finally 
            {
                clientSetting = null;
                requrl = "";
                SessionID = string.Empty;
                returl = string.Empty;
                enurl = string.Empty;
                baseUrlID = string.Empty;
            }
            return Json(val, JsonRequestBehavior.AllowGet);
            #endregion
        }


        public JsonResult getEncryptDecrypt(string msisdn, string mode, string Ispostpaid, string planname, string bundlecode, string activationamount, string monthlyrental, string terminationfees,string STATUS)
        {
            ClientSetting clientSetting = new ClientSetting();
            string anatelid = string.Empty;
            var requrl = "";
            string returl = string.Empty;
            LoginController lgnctr;
            string type = string.Empty;
            string typedesc = string.Empty;
            AuditTrailRequest auditTrailRequest;
            string enurl = string.Empty;
            string SessionID = string.Empty;
            string baseUrlID = string.Empty;
            string val = string.Empty;

            try
            {
                mode = mode == null ? "NEW_REGN" : mode;
                //var requrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');
                // var requrl = Request.Url.ToString().Replace("CRM/getEncryptDecrypt", "").Split('?')[0];
                requrl = Request.UrlReferrer.ToString().TrimEnd('?');
                CRMLogger.WriteMessage(string.Empty, this.ControllerContext, requrl);
                if (Ispostpaid == "1")
                {
                    returl = Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + mode + "|" + msisdn + "|" + DateTime.Now;
                }
                else
                {
                returl = Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + mode + "|" + Convert.ToString(Session["MobileNumber"]) + "|" + DateTime.Now;
                }
                if (mode == "VIEW_REGN_IND")
                {
                    mode = "VIEW_REGN";
                }
                CRMLogger.WriteMessage(string.Empty, this.ControllerContext, returl);
                #region Brazil Specific
                if (clientSetting.countryCode == "BRA" && (mode == "NEW_REGN" || mode == "EDIT_REGN" || mode == "VIEW_REGN"))
                {
                    if (mode == "NEW_REGN")
                    {
                        anatelid = CRMGenerateAnatelid();
                    }
                    else
                    {
                        anatelid = Convert.ToString(Session["AnatelID"]);
                    }
                    returl = returl + '|' + anatelid;

                    lgnctr = new LoginController();
                    type = (mode == "NEW_REGN") ? "Registration" : (mode == "EDIT_REGN") ? "Edit Registration" : "View Registration";
                    typedesc = (mode == "NEW_REGN") ? "RegisterrequestBrazil" : (mode == "EDIT_REGN") ? "EditRegisterrequestBrazil" : "ViewRegisterrequestBrazil";
                    auditTrailRequest = new AuditTrailRequest();
                    auditTrailRequest.module = "Subscriber";
                    auditTrailRequest.subModule = type + " Request";
                    auditTrailRequest.action = "Brazil " + type;
                    auditTrailRequest.description = type + " Request";
                    auditTrailRequest.anatelId = anatelid;
                    auditTrailRequest.DescID = typedesc;
                    Session["AnatelID"] = anatelid;
                    lgnctr.AuditTrailCRM(auditTrailRequest);

                }
                #endregion
                if (Ispostpaid == "1")
                {
                    returl = returl + "|" + planname + "|" + bundlecode + "|" + activationamount + "|" + monthlyrental + "|" + terminationfees;
                }
                enurl = Utility.EncryptDecrypt(returl);
                //RM parameters
                // string baseUrlID = "CRM|" + clientSetting.brandCode + "|" + msisdn + "|" + Session["UserName"].ToString() + "|" + mode + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                SessionID = Utility.mthdSessionID();
                baseUrlID = "CRM|" + clientSetting.brandCode + "|" + msisdn + "|" + Session["UserName"].ToString() + ",";
                baseUrlID += Session["UserGroup"].ToString() + "|" + mode + "|" + requrl + "?Keyframe=" + enurl + "|";
                baseUrlID += Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                baseUrlID += "|" + SessionID;
                if (clientSetting.countryCode.ToUpper() == "IND")
                {
                    baseUrlID += "|" + "1";
                }
                else
                {
                    baseUrlID += "|" + "0";
                }
                //4788
                if (Ispostpaid == "1")
                {
                    baseUrlID += "|" + "1";
                }
                else
                {
                    baseUrlID += "|" + "0";
                }
                //5022
                if (clientSetting.countryCode.ToUpper() == "ITA")
                {
                    if (STATUS == "CHANGEOWNERSHIP")
                    {
                        baseUrlID += "|0|0|0|0|1";
                    }
                }
                val = Utility.EncryptDecrypt(baseUrlID);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(string.Empty, "CRMController-getEncryptDecrypt- " + this.ControllerContext, ex);
            }
            finally
            {
                clientSetting = null;
                requrl = null;
                returl = string.Empty;
                lgnctr = null;
                type = string.Empty;
                typedesc = string.Empty;
                auditTrailRequest = null;
                enurl = string.Empty;
                SessionID = string.Empty;
                baseUrlID = string.Empty;
            }
            return Json(val, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getEncryptDecryptWithConActionName(string msisdn, string mode, string ContActionName)
        {
            ClientSetting clientSetting = new ClientSetting();
            string anatelid = "";
            var requrl = "";
            string returl = string.Empty;
            LoginController lgnctr;
            string type = string.Empty;
            string typedesc = string.Empty;
            AuditTrailRequest auditTrailRequest;
            string enurl = string.Empty;
            string SessionID = string.Empty;
            string baseUrlID = string.Empty;
            string val = string.Empty;

            try
            {
                if (clientSetting.preSettings.RMRedirectMSISDNPayG.Trim().ToLower() == "false")
                {
                    msisdn = msisdn.Substring(clientSetting.mvnoSettings.countryPrefix.Length);
                }
                mode = mode == null ? "NEW_REGN" : mode;
                //var requrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');
                // var requrl = Request.Url.ToString().Replace("CRM/getEncryptDecrypt", "").Split('?')[0];
                requrl = Request.UrlReferrer.ToString().TrimEnd('?');
                CRMLogger.WriteMessage(string.Empty, this.ControllerContext, requrl);
                returl = Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + mode + "|" + Convert.ToString(Session["MobileNumber"]) + "|" + DateTime.Now + "|" + ContActionName; ;
                CRMLogger.WriteMessage(string.Empty, this.ControllerContext, returl);
                #region Brazil Specific
                if (clientSetting.countryCode == "BRA" && (mode == "NEW_REGN" || mode == "EDIT_REGN" || mode == "VIEW_REGN"))
                {
                    if (mode == "NEW_REGN")
                    {
                        anatelid = CRMGenerateAnatelid();
                    }
                    else
                    {
                        anatelid = Convert.ToString(Session["AnatelID"]);
                    }
                    returl = returl + '|' + anatelid;

                    lgnctr = new LoginController();
                    type = (mode == "NEW_REGN") ? "Registration" : (mode == "EDIT_REGN") ? "Edit Registration" : "View Registration";
                    typedesc = (mode == "NEW_REGN") ? "RegisterrequestBrazil" : (mode == "EDIT_REGN") ? "EditRegisterrequestBrazil" : "ViewRegisterrequestBrazil";
                    auditTrailRequest = new AuditTrailRequest();
                    auditTrailRequest.module = "Subscriber";
                    auditTrailRequest.subModule = type + " Request";
                    auditTrailRequest.action = "Brazil " + type;
                    auditTrailRequest.description = type + " Request";
                    auditTrailRequest.anatelId = anatelid;
                    auditTrailRequest.DescID = typedesc;
                    Session["AnatelID"] = anatelid;
                    lgnctr.AuditTrailCRM(auditTrailRequest);
                }
                #endregion

                enurl = Utility.EncryptDecrypt(returl);
                //RM parameters
                // string baseUrlID = "CRM|" + clientSetting.brandCode + "|" + msisdn + "|" + Session["UserName"].ToString() + "|" + mode + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                SessionID = Utility.mthdSessionID();
                baseUrlID = "CRM|" + clientSetting.brandCode + "|" + msisdn + "|" + Session["UserName"].ToString() + ",";
                baseUrlID += Session["UserGroup"].ToString() + "|" + mode + "|" + requrl + "?Keyframe=" + enurl + "|";
                baseUrlID += Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                baseUrlID += "|" + SessionID;
                if (clientSetting.countryCode.ToUpper() == "IND")
                {
                    baseUrlID += "|" + "1";
                }
                else
                {
                    baseUrlID += "|" + "0";
                }
                val = Utility.EncryptDecrypt(baseUrlID);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(string.Empty, "CRMController-getEncryptDecryptWithConActionName- " + this.ControllerContext, ex);
            }
            finally
            {
                clientSetting = null;
                requrl = null;
                returl = string.Empty;
                lgnctr = null;
                type = string.Empty;
                typedesc = string.Empty;
                auditTrailRequest = null;
                enurl = string.Empty;
                SessionID = string.Empty;
                baseUrlID = string.Empty;
            }
            return Json(val, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getEncryptDecryptWithConActionNameForMigration(string OldMsisdn, string OldICCID, string NewMsisdn, string NewICCID, string mode, string ContActionName)
        {
            ClientSetting clientSetting = new ClientSetting();
            string anatelid = "";
            var requrl = "";
            string returl = string.Empty;
            LoginController lgnctr;
            string type = string.Empty;
            string typedesc = string.Empty;
            AuditTrailRequest auditTrailRequest;
            string enurl = string.Empty;
            string SessionID = string.Empty;
            string baseUrlID = string.Empty;
            string val = string.Empty;

            try
            {
                if (clientSetting.preSettings.RMRedirectMSISDNPayG.Trim().ToLower() == "false")
                {
                    OldMsisdn = OldMsisdn.Substring(clientSetting.mvnoSettings.countryPrefix.Length);
                }
                mode = mode == null ? "NEW_REGN" : mode;
                requrl = Request.UrlReferrer.ToString().TrimEnd('?');
                CRMLogger.WriteMessage(string.Empty, this.ControllerContext, requrl);
                returl = Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + mode + "|" + Convert.ToString(Session["MobileNumber"]) + "|" + DateTime.Now + "|" + ContActionName; ;
                CRMLogger.WriteMessage(string.Empty, this.ControllerContext, returl);
                #region Brazil Specific
                if (clientSetting.countryCode == "BRA" && (mode == "NEW_REGN" || mode == "EDIT_REGN" || mode == "VIEW_REGN" || mode == "MIGRANT"))
                {
                    if (mode == "NEW_REGN")
                    {
                        anatelid = CRMGenerateAnatelid();
                    }
                    else
                    {
                        anatelid = Convert.ToString(Session["AnatelID"]);
                    }
                    returl = returl + '|' + anatelid;

                    lgnctr = new LoginController();
                    type = (mode == "NEW_REGN") ? "Registration" : (mode == "EDIT_REGN") ? "Edit Registration" : "View Registration";
                    typedesc = (mode == "NEW_REGN") ? "RegisterrequestBrazil" : (mode == "EDIT_REGN") ? "EditRegisterrequestBrazil" : "ViewRegisterrequestBrazil";
                    auditTrailRequest = new AuditTrailRequest();
                    auditTrailRequest.module = "Subscriber";
                    auditTrailRequest.subModule = type + " Request";
                    auditTrailRequest.action = "Brazil " + type;
                    auditTrailRequest.description = type + " Request";
                    auditTrailRequest.anatelId = anatelid;
                    auditTrailRequest.DescID = typedesc;
                    Session["AnatelID"] = anatelid;
                    lgnctr.AuditTrailCRM(auditTrailRequest);
                }
                #endregion

                enurl = Utility.EncryptDecrypt(returl);
                //RM parameters
                // string baseUrlID = "CRM|" + clientSetting.brandCode + "|" + msisdn + "|" + Session["UserName"].ToString() + "|" + mode + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                SessionID = Utility.mthdSessionID();
                baseUrlID = "CRM|" + clientSetting.brandCode + "|" + OldMsisdn + "|" + Session["UserName"].ToString() + ",";
                baseUrlID += Session["UserGroup"].ToString() + "|" + mode + "|" + OldICCID + "|" + NewMsisdn + "|" + NewICCID + "|";
                baseUrlID += requrl + "?Keyframe=" + enurl + "|";
                baseUrlID += Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                baseUrlID += "|" + SessionID;
                if (clientSetting.countryCode.ToUpper() == "IND")
                {
                    baseUrlID += "|" + "1";
                }
                else
                {
                    baseUrlID += "|" + "0";
                }
                val = Utility.EncryptDecrypt(baseUrlID);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(string.Empty, "CRMController-getEncryptDecryptWithConActionName- " + this.ControllerContext, ex);
            }
            finally
            {
                clientSetting = null;
                requrl = null;
                returl = string.Empty;
                lgnctr = null;
                type = string.Empty;
                typedesc = string.Empty;
                auditTrailRequest = null;
                enurl = string.Empty;
                SessionID = string.Empty;
                baseUrlID = string.Empty;
            }
            return Json(val, JsonRequestBehavior.AllowGet);
        }

        public string CRMGenerateAnatelid()
        {
            ClientSetting clientSetting = new ClientSetting();
            GenerateAnatelIdResponse objres = new GenerateAnatelIdResponse();
            GenerateAnatelIdRequest objreq = new GenerateAnatelIdRequest();
            string anatelid = string.Empty;
            ServiceInvokeCRM serviceCRM;
            try
            {                
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.UserName = Convert.ToString(Session["UserName"]);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.CRMGenerateAnatelid(objreq);
                if (objres != null && objres.responseDetails != null && objres.responseDetails.ResponseCode != null)
                {
                    anatelid = objres.AnatelId;
                }
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMController-CRMGenerateAnatelid- " + this.ControllerContext, eX);
            }
            finally
            {
                clientSetting = null;
                objres = null;
                objreq = null;
                serviceCRM = null;
            }
            return anatelid;
        }
        [HttpPost]
        public JsonResult GetSimOptions(string str)
        {
            SimOption objSimOption = new SimOption();
            try
            {
                objSimOption.DropMaster = Utility.GetDropdownMasterFromDB("68", "1", "drop_master");
                objSimOption.DropMaster = objSimOption.DropMaster.OrderBy(x => x.Value).ToList();
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMController-GetSimOptions- " + this.ControllerContext, eX);
            }
            finally
            {
                //objSimOption = null;
            }
            return Json(objSimOption, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetPageAccess(string SubCatUrl)
        {
            Menu objMenu = new Menu();
            List<Menu> objMenuList = new List<Menu>();
            try
            {
                objMenuList = (List<Menu>)Session["MenuAndFeatures"];
                if (objMenuList != null && objMenuList.Count > 0)
                {
                    objMenuList = objMenuList.Where(a => a.SubCatUrl == SubCatUrl).ToList();
                    if (objMenuList != null && objMenuList.Count > 0)
                    {
                        objMenu = objMenuList[0];
                    }
                }
                return Json(objMenu, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(string.Empty, "CRMController-GetPageAccess- " + this.ControllerContext, ex);
                return null;
            }
            finally
            {
              //  objMenu = null;
                objMenuList = null;
            }
        }

        [HttpPost]
        public ViewResult TicketPanel(string Mode, string MSISDN)
        {
            try
            {
                if (!string.IsNullOrEmpty(MSISDN))
                {
                    Session["MobileNumber"] = MSISDN;
                }
                //6459
                if (Session["CUNTicketDetails"] != null && Mode != "43" && Mode != "44")
                {
                    Mode = "6";
                    Session["CUNTicketDetails"] = null;
                }

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(string.Empty, "CRMController-TicketPanel- " + this.ControllerContext, ex);
            }
            return View(Json(Mode));
        }


        //------------------ gopi2296 V_1.0.23.0----------------------------
        [HttpPost]
        public JsonResult FavMenuStar(MenuFav objMenu)
        {
            ClientSetting clientSetting = new ClientSetting();
            CRMResponse objResp = new CRMResponse();
            List<Menu_Item> lstMenuItem = new List<Menu_Item>();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Favorite Menu List Start");
                
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objMenu.CountryCode = clientSetting.countryCode;
                objMenu.BrandCode = clientSetting.brandCode;
                objMenu.LanguageCode = clientSetting.langCode;

                objMenu.UserID = Convert.ToString(Session["UserID"]);
                if (@clientSetting.mvnoSettings.userManagementType.ToUpper() == "ON")
                {
                    objMenu.UserRoleID = "";
                }
                else
                {
                    objMenu.UserRoleID = System.Configuration.ConfigurationManager.AppSettings["RootUserIDs"].Trim();
                }
                objResp = serviceCRM.CRMFavMenuStar(objMenu);
                if (objResp != null && !string.IsNullOrEmpty(objResp.ResponseCode) && objResp.ResponseCode == "0")
                {
                    #region GET
                    CategoryItemsResponse catgItemsResp = new CategoryItemsResponse();
                    UserFavoritesMapPResponse userFavMapResp = new UserFavoritesMapPResponse();
                    List<Category_Menu> lstCatItem = new List<Category_Menu>();
                    List<UserFavorites> lstUserFavs = new List<UserFavorites>();
                    CategoryItemsRequest catgItemsReq = new CategoryItemsRequest();
                    UserFavoritesMapRequest userFavReq = new UserFavoritesMapRequest();
                    ServiceInvokeCRM serviceCRM2;
                    try
                    {
                        catgItemsReq.CountryCode = clientSetting.countryCode;
                        catgItemsReq.BrandCode = clientSetting.brandCode;
                        catgItemsReq.LanguageCode = clientSetting.langCode;
                        catgItemsReq.roleCatID = Convert.ToString(Session["UserGroupID"]);

                        userFavReq.CountryCode = clientSetting.countryCode;
                        userFavReq.BrandCode = clientSetting.brandCode;
                        userFavReq.LanguageCode = clientSetting.langCode;
                        userFavReq.ROLE_CAT_ID = Convert.ToString(Session["UserGroupID"]);
                        userFavReq.MODE = "GET";
                        //------------------ gopi2296 V_1.0.23.0----------------------------
                        userFavReq.UserID = Convert.ToString(Session["UserID"]);
                        if (@clientSetting.mvnoSettings.userManagementType.ToUpper() == "ON")
                        {
                            userFavReq.UserRoleID = "";
                        }
                        else
                        {
                            userFavReq.UserRoleID = System.Configuration.ConfigurationManager.AppSettings["RootUserIDs"].Trim();
                        }
                        //------------------------ End -------------------------------------

                        if (Session["isPrePaid"].ToString() == "1")
                        {
                            catgItemsReq.simType = "Pre";
                            userFavReq.simType = "Pre";
                        }
                        else
                        {
                            catgItemsReq.simType = "Post";
                            userFavReq.simType = "Post";
                        }
                        //----------FRR 3314---------------Gopi2296-------------------------
                        if (!string.IsNullOrEmpty(Session["RetailerID"].ToString()))
                        {
                            catgItemsReq.RetailerID = Session["RetailerID"].ToString();
                            userFavReq.RetailerID = Session["RetailerID"].ToString();
                        }
                        else
                        {
                            catgItemsReq.RetailerID = "";
                            userFavReq.RetailerID = "";
                        }

                        //POSTPAID CUSTOMER ALLOW ONLY REFUND MODULE SCOPE PID
                        catgItemsReq.IS_POSTPAID = Convert.ToString(Session["isPrePaid"]);
                        //----------FRR 3314---------------Gopi2296-------------------------
                        serviceCRM2 = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        catgItemsResp = serviceCRM2.CRMMenuResource(catgItemsReq);
                        userFavMapResp = serviceCRM2.CRMUserFavotitesMap(userFavReq);

                        Session["UserMenu"] = lstCatItem = catgItemsResp.categoryMenus;
                        Session["UserFavMenu"] = lstUserFavs = userFavMapResp.UserFavorites;

                        lstMenuItem.AddRange(lstCatItem.SelectMany(c => c.menuItems.Where(m => lstUserFavs.Select(u => u.subCatID).Contains(m.menuID))).GroupBy(x => x.menuID).Select(y => y.First()));
                    }
                    catch (Exception ex)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMController-FavMenuStar-Service2- " + this.ControllerContext, ex);
                    }
                    finally
                    {
                        serviceCRM2 = null;
                        catgItemsResp = null;
                        userFavMapResp = null;
                        lstCatItem = null;
                        lstUserFavs = null;
                        catgItemsReq = null;
                        userFavReq = null;
                    }
                    #endregion
                }
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Favorite Menu List End");
                return Json(objResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMController-FavMenuStar- " + this.ControllerContext, exp);
                return Json(objResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                clientSetting = null;
                serviceCRM = null;
            }
        }
        //------------------------- End ------------------------------------

        [HttpPost]
        public void DownLoadSubscriberList(string strSubscriberList, string strDropSelected, string strFormattedText, string strMsisdnColumnName, string strDropSelectedValue)
        {
            System.Web.UI.WebControls.GridView gridView = new System.Web.UI.WebControls.GridView();
            DataTable dt = new DataTable();
            try
            {
                dt = (DataTable)JsonConvert.DeserializeObject(strSubscriberList, (typeof(DataTable)));
                dt.Columns.Add(strDropSelected);
                dt.Rows[0].Delete();
                dt.Columns[0].ColumnName = strMsisdnColumnName;
                dt.Columns[1].ColumnName = strFormattedText + "_" + strMsisdnColumnName;
                dt.Rows[0][2] = strDropSelectedValue;
                gridView.DataSource = dt;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "SubscriberList_" + strDropSelected + "_", this.HttpContext.Response);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CRMController-DownLoadSubscriberList- " + this.ControllerContext, exp);
            }
            finally
            {
                dt = null;
                gridView = null;
            }
        }





        #region FRR 4836
        [HttpPost]
        public JsonResult SemiAutoVerification(string GetSemiautoverificaitonRequest)
        {

            SemiAutoVerificationResponse objres = new SemiAutoVerificationResponse();
            ClientSetting clientSetting = new ClientSetting();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                SemiAutoVerificationRequest objreq = JsonConvert.DeserializeObject<SemiAutoVerificationRequest>(GetSemiautoverificaitonRequest);
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.MSISDN = Convert.ToString(Session["MobileNumber"]);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objres = serviceCRM.CRMSemiAutoVerification(objreq);
                    if (!string.IsNullOrEmpty(objres.Status) && objreq.Mode == "GET_SEARCH")
                    {
                        Session["SemiVarification"] = objres.Status;
                        if (Session["SemiVarification"].ToString() != null)
                        {
                            if ((Convert.ToInt32(Session["SemiVarification"].ToString())) >= Convert.ToInt32(clientSetting.preSettings.MaximumAnswersforSemiautomatedverification.Trim()))
                                Session["SemiVarificationSuccess"] = "1";
                            else
                                Session["SemiVarificationSuccess"] = "0";
                        }
                    }

                    if (objres.responseDetails!= null && objres.responseDetails.ResponseCode == "0" && objreq.Mode == "CHECK_CPIN")
                        Session["SemiVarificationSuccess"] = "1";
                    else if (objres.responseDetails != null && objres.responseDetails.ResponseCode != "0" && objreq.Mode == "CHECK_CPIN")
                        Session["SemiVarificationSuccess"] = "0";

                    if (objres.responseDetails != null && objres.responseDetails.ResponseCode == "0" && objreq.Mode == "UPDATE_CPIN")
                        Session["SemiVarificationSuccess"] = "1";

                    if (objres.responseDetails != null && objreq.Mode == "REG")
                    {
                        ViewBag.FailureCount = objres.FailureCount;
                        if (objres.responseDetails.ResponseCode == "1")
                        {
                         
                            ViewBag.IsRegCountry = "1";
                        }
                        else
                        {
                            ViewBag.IsRegCountry = "0";
                        }
                    }
                
                if (objres != null && objres.responseDetails != null && objres.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMOnlineReport_" + objres.responseDetails.ResponseCode);
                    objres.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.responseDetails.ResponseDesc : errorInsertMsg;
                }
                return Json(objres, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objres, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                clientSetting = null;
                serviceCRM = null;
            }


        }



        #endregion


        #region FRR 4887

        public JsonResult GetPayGConsentDetails(BundleDetailRequest viewobjreq)
        {
            BundleDetailRequest objreq = new BundleDetailRequest();
            PayGConsentBundleAccBalanceRes objres = new PayGConsentBundleAccBalanceRes();
            PayGConsentBundleAccBalanceRes viewobjres = new PayGConsentBundleAccBalanceRes();
            ClientSetting clientSetting = new ClientSetting();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.MSISDN = Session["MobileNumber"].ToString();
                objreq.ChildMsisdn = viewobjreq.ChildMsisdn;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objres = serviceCRM.CRMGetPayGConsentDetails(objreq);

                    if (objres.RETURNCODE == "0")
                    {
                        viewobjres = objres;
                    }
                    else
                    {
                        viewobjres.ERRDESCRITION = objres.ERRDESCRITION;
                    }
                
                return Json(viewobjres);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objres, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                clientSetting = null;
                serviceCRM = null;
            }
        }


        public JsonResult SetPayGConsentDetails(PayGConsentDetailsRequest objreq)
        {

            PayGConsentDetailsResponse objres = new PayGConsentDetailsResponse();
            PayGConsentDetailsResponse viewobjres = new PayGConsentDetailsResponse();
            ClientSetting clientSetting = new ClientSetting();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {

                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.MSISDN = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objres = serviceCRM.CRMSetPayGConsentDetails(objreq);

                    if (objres.ResponseCode == "0")
                    {
                        viewobjres.ResponseCode = objres.ResponseCode;
                        viewobjres.ResponseDesc = objres.ResponseDesc;

                    }
                

                return Json(viewobjres);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objres, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                clientSetting = null;
                serviceCRM = null;
            }


        }
        #endregion

    }
}








