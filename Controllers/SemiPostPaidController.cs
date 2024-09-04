using System;
using System.Linq;
using System.Web.Mvc;
using ServiceCRM;
using Newtonsoft.Json;
using System.IO;
using Tamir.SharpSsh;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Data;
using CRM.Models;
using System.Collections.Generic;
namespace CRM.Controllers
{

    [ValidateState]
    public class SemiPostPaidController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();

        public static bool _runningFromNUnit = Service.UnitTestDetector._runningFromNUnit;
        //_runningFromNUnit true when the Hit is from Nunit.

        public PartialViewResult LoadOBA()
        {

            return PartialView();
        }

        public PartialViewResult BundleData(string ObaBundleRequest)
        {
            OBABundleResponse obaBundleResponse = new OBABundleResponse();
            OBABundle obabundelresp = new OBABundle();
            obaBundleResponse.loadOBABundle = new System.Collections.Generic.List<LoadOBABundle>();
            OBABundleRequest ObjReq = JsonConvert.DeserializeObject<OBABundleRequest>(ObaBundleRequest);
            //s
            ServiceInvokeCRM serviceCRM;
            //LoadOBABundle obj = new LoadOBABundle();
            //obj.bundleCode = "11111";
            //obj.bundleName = "fTTT";
            //obj.expiryDate = "29-04-2016";
            //obj.autoRenewal = "1";
            //obj.autoRenewalValue = "1";
            //obj.mode = "Q";
            //obj.OBAAvailable = "5";
            //obj.currentCredit = "20";
            //obj.contractStartDate = "27-04-2016";
            //obj.contractEndDate = "28-04-2016";
            //obj.OBARenewal = "DeActive";
            //ObaBundleRequest.TypeOfPayment = "RECURRING";
            //   ObaBundleRequest.AccountHolder = "jai";
            //   ObaBundleRequest.IBANNumber = "13223435645675";
            //   ObaBundleRequest.Emailid = "jai@f.com";
            //   ObaBundleRequest.Address =  "jai";
            //   ObaBundleRequest.Address02 = "jai";
            //   ObaBundleRequest.Address03 =  "jai";
            //   ObaBundleRequest.Towercity = "jai";
            //   ObaBundleRequest.State =  "jai";
            //   ObaBundleRequest.Pincode = "23445";
            //   ObaBundleRequest.Amount = "45.00";
            //   ObaBundleRequest.Mode = "DirectDebit";
            //   ObaBundleRequest.FeatureName = "feature";
            //   ObaBundleRequest.CustomerConsent = "Y";
            try
            {
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.MSISDN = Session["MobileNumber"].ToString();
                ObjReq.Mode = "Q";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    obaBundleResponse = serviceCRM.CRMOBABundle(ObjReq);
                    obaBundleResponse.EshopResponseDesc = ObjReq.EshopDesc;
                    //obaBundleResponse.loadOBABundle.Add(obj);
                    ///FRR--3083
                    if (!_runningFromNUnit)
                    {
                        if (obaBundleResponse != null && obaBundleResponse.responseDetails != null && obaBundleResponse.responseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("OBA_" + obaBundleResponse.responseDetails.ResponseCode);
                            obaBundleResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? obaBundleResponse.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                    }
                    obabundelresp.loadOBABundle = obaBundleResponse.loadOBABundle;
                    obabundelresp.EshopResponseDesc = obaBundleResponse.EshopResponseDesc;
                    obabundelresp.responseDetails = obaBundleResponse.responseDetails;
                    ///FRR--3083
                    obabundelresp.lstDirectdebitCardDetailsoba = GetDebitCardDetails();

                
                if (obaBundleResponse != null)
                {
                    Session["OBABundleResponse"] = obabundelresp;
                }
                return PartialView("BundleData", obabundelresp);
            }
            catch (Exception ex)
            {
                obaBundleResponse.responseDetails.ResponseCode = "NA";
                obaBundleResponse.responseDetails.ResponseDesc = ex.Message.ToString();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return PartialView("BundleData", obabundelresp);
            }
            finally
            {
                // obaBundleResponse = null;
                serviceCRM = null;
            }

        }
        public JsonResult BundleDataPendappr(string ObaBundleRequest)
        {
            OBABundleResponse obaBundleResponse = new OBABundleResponse();
            OBABundle obabundelresp = new OBABundle();
            obaBundleResponse.loadOBABundle = new System.Collections.Generic.List<LoadOBABundle>();
            OBABundleRequest ObjReq = JsonConvert.DeserializeObject<OBABundleRequest>(ObaBundleRequest);
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                // ObjReq.MSISDN = Session["MobileNumber"].ToString();
                ObjReq.Mode = "Q";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    obaBundleResponse = serviceCRM.CRMOBABundle(ObjReq);
                    obaBundleResponse.EshopResponseDesc = ObjReq.EshopDesc;
                    //obaBundleResponse.loadOBABundle.Add(obj);
                    ///FRR--3083
                    if (!_runningFromNUnit)
                    {
                        if (obaBundleResponse != null && obaBundleResponse.responseDetails != null && obaBundleResponse.responseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("OBA_" + obaBundleResponse.responseDetails.ResponseCode);
                            obaBundleResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? obaBundleResponse.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                    }
                    obabundelresp.loadOBABundle = obaBundleResponse.loadOBABundle;
                    obabundelresp.EshopResponseDesc = obaBundleResponse.EshopResponseDesc;
                    obabundelresp.responseDetails = obaBundleResponse.responseDetails;
                    ///FRR--3083
                    obabundelresp.lstDirectdebitCardDetailsoba = GetDebitCardDetails();

                
                if (obaBundleResponse != null)
                {
                    Session["OBABundleResponse"] = obabundelresp;
                }
                return Json(obabundelresp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                obaBundleResponse.responseDetails.ResponseCode = "NA";
                obaBundleResponse.responseDetails.ResponseDesc = ex.Message.ToString();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(obabundelresp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                // obaBundleResponse = null;
                serviceCRM = null;
            }

        }
        //4617
        public JsonResult BundleDataDD(string ObaBundleRequest)
        {
            //OBABundleResponse obaBundleResponse = new OBABundleResponse();
            //obaBundleResponse.loadOBABundle = new System.Collections.Generic.List<LoadOBABundle>();
            OBABundleResponse obaBundleResponse = new OBABundleResponse();
            OBABundleRequest ObjReq = JsonConvert.DeserializeObject<OBABundleRequest>(ObaBundleRequest);
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                // ObjReq.MSISDN = Session["MobileNumber"].ToString();
                //ObaBundleRequest.Mode = "Q";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    obaBundleResponse = serviceCRM.CRMOBABundle(ObjReq);
                    //obaBundleResponse.EshopResponseDesc = ObaBundleRequest.EshopDesc;
                    //obaBundleResponse.loadOBABundle.Add(obj);
                    ///FRR--3083
                    if (!_runningFromNUnit)
                    {
                        if (obaBundleResponse != null && obaBundleResponse.responseDetails != null && obaBundleResponse.responseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("OBA_" + obaBundleResponse.responseDetails.ResponseCode);
                            obaBundleResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? obaBundleResponse.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                    }
                    ///FRR--3083

                
                if (obaBundleResponse != null)
                {
                    Session["OBABundleResponse"] = obaBundleResponse;
                }
                return Json(obaBundleResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                obaBundleResponse.responseDetails.ResponseCode = "NA";
                obaBundleResponse.responseDetails.ResponseDesc = ex.Message.ToString();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(obaBundleResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
               // obaBundleResponse = null;
                serviceCRM = null;
            }

        }

        //// 4617
        public Dictionary<string, string> GetDebitCardDetails()
        {

            OBABundle ObjResp = new OBABundle();
            ManagedCardResponse manageCardResp = new ManagedCardResponse();
            ManagedCardRequest manageCardReq = new ManagedCardRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller GetCreditCardDetails Start");
                manageCardReq.BrandCode = clientSetting.brandCode;
                manageCardReq.CountryCode = clientSetting.countryCode;
                manageCardReq.LanguageCode = clientSetting.langCode;
                manageCardReq.Msisdn = Session["MobileNumber"].ToString();
                manageCardReq.mode = "Q";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                manageCardResp = serviceCRM.CRMManageCardDetails(manageCardReq);
                if (manageCardResp.debitcardDetails != null)
                {
                    for (int i = 0; i < manageCardResp.debitcardDetails.Count; i++)
                    {
                        ObjResp.lstDirectdebitCardDetailsoba.Add(manageCardResp.debitcardDetails[i].cardId + "," + manageCardResp.debitcardDetails[i].Country + "," + manageCardResp.debitcardDetails[i].addressline1 + "," + manageCardResp.debitcardDetails[i].addressline2 + "," + manageCardResp.debitcardDetails[i].postcode + "," + manageCardResp.debitcardDetails[i].email + "," + manageCardResp.debitcardDetails[i].city + "," + manageCardResp.debitcardDetails[i].cardNumber + "," + manageCardResp.debitcardDetails[i].nameOnCard, manageCardResp.debitcardDetails[i].cardNumber);
                    }
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller GetCreditCardDetails End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                //serviceCRM = null;
                //ObjCreditCardResponse = null;
                //ObjCreditCardList = null;
            }
            return ObjResp.lstDirectdebitCardDetailsoba;
        }

        public PartialViewResult Payment()
        {

            return PartialView();
        }
        public PartialViewResult Upgrade()
        {

            return PartialView();
        }
        //4653
        public PartialViewResult Downgrade()
        {

            return PartialView();
        }
        public PartialViewResult CancelBundle()
        {

            return PartialView();
        }
        public JsonResult ManaageOBABundles(OBABundleUpgradeRequest bundleUpgradeRequest)
        {
            OBABundleUpgradeRequest obaBundleReq = new OBABundleUpgradeRequest();
            OBABundleUpgradeResponse obaBundleResponse = new OBABundleUpgradeResponse();
            PendingDetailsRequest req = new PendingDetailsRequest();
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
                menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SemiPostPaid_LoadOBA").ToList();
                //bool isAdmin = (bool)Session["IsAdmin"] || (clientSetting.mvnoSettings.userManagementType.ToUpper() == "ON" && Convert.ToString(Session["ROLE_CAT_TYPE"]) == "2");
                bool isAdmin = menu[0].DirectApproval.ToUpper() == "TRUE" || menu[0].Approval1.ToUpper() == "TRUE";
                if (isAdmin == true && bundleUpgradeRequest.mode == "Q")
                {
                    if (Session["PAType"] != null && Convert.ToString(Session["PAType"]) == "UPGRADE" || Convert.ToString(Session["PAType"]) == "DOWNGRADE")
                    {
                        req.CountryCode = clientSetting.countryCode;
                        req.BrandCode = clientSetting.brandCode;
                        req.LanguageCode = clientSetting.langCode;
                        req.MSISDN = Convert.ToString(Session["MobileNumber"]);
                        req.Type = "OBA Plan Upgrade/Downgrade";
                        req.Id = Convert.ToString(Session["PAId"]);

                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        
                            ObjRes = serviceCRM.CRMGetPendingDetails(req);

                            ///FRR--3083
                            if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                            {
                                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManaageOBABundle_" + ObjRes.ResponseDetails.ResponseCode);
                                ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                            }
                            ///FRR--3083

                        

                    }
                }
                obaBundleReq.BrandCode = clientSetting.brandCode;
                obaBundleReq.CountryCode = clientSetting.countryCode;
                obaBundleReq.LanguageCode = clientSetting.langCode;
                obaBundleReq.mode = bundleUpgradeRequest.mode;
                if(bundleUpgradeRequest.mode == "Q")
                {
                    obaBundleReq.oldBundleCode = bundleUpgradeRequest.oldBundleCode;
                    obaBundleReq.OBAAvailable = bundleUpgradeRequest.OBAAvailable;
                }

                obaBundleReq.requestedBy = bundleUpgradeRequest.requestedBy;
                obaBundleReq.userName = bundleUpgradeRequest.userName;
                obaBundleReq.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    obaBundleResponse = serviceCRM.CRMOBABundleUpgrade(obaBundleReq);
                    if (isAdmin == true)
                    {
                        if (ObjRes.PendingDetails != null && ObjRes.PendingDetails.Count > 0)
                        {
                            obaBundleResponse.selectedBundle = ObjRes.PendingDetails[0].NewMSISDN;
                            obaBundleResponse.IsPendingBundleUpgrade = "Y";
                            obaBundleResponse.Id = ObjRes.PendingDetails[0].Id;
                        }
                    }
                    obaBundleResponse.bundleNames = "<option title='Select' value='Select'>Select</option>";
                    if (obaBundleResponse.OBABundleName != null && obaBundleResponse.OBABundleName.Count() > 0)
                    {
                        foreach (OBABundleName item in obaBundleResponse.OBABundleName)
                        {
                            string[] splitBundleCodeAndName = item.bundleName.Split(new string[] { "$$$" }, StringSplitOptions.None);
                            if (clientSetting.preSettings.EnableOBADowngrade.ToUpper() != "TRUE")
                            {
                                obaBundleResponse.bundleNames = obaBundleResponse.bundleNames + "<option title='" + splitBundleCodeAndName[1] + "' value='" + splitBundleCodeAndName[0] + "'>" + splitBundleCodeAndName[1] + "</option>";
                            }
                            else
                            {
                                obaBundleResponse.bundleNames = obaBundleResponse.bundleNames + "<option title='" + item.BundleNametoshow + "' value='" + item.bundleName + "'>" + item.BundleNametoshow + "</option>";
                            }
                        }
                    }
                
                Session["PAType"] = null;
                Session["PAId"] = null;

                ///FRR--3083
                if (!_runningFromNUnit)
                {
                    if (obaBundleResponse != null && obaBundleResponse.responseDetails != null && obaBundleResponse.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SemiPostPaid_" + obaBundleResponse.responseDetails.ResponseCode);
                        obaBundleResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? obaBundleResponse.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                ///FRR--3083
                return Json(obaBundleResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                obaBundleResponse.responseDetails.ResponseCode = "NA";
                obaBundleResponse.responseDetails.ResponseDesc = ex.Message.ToString();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(obaBundleResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                obaBundleReq = null;
               // obaBundleResponse = null;
                req = null;
                ObjRes = null;
                serviceCRM = null;
            }


        }
        public JsonResult ManaageOBAAdditionalCredit(OBABundleUpgradeRequest bundleUpgradeRequest)
        {
            OBABundleUpgradeRequest obaBundleReq = new OBABundleUpgradeRequest();
            OBABundleUpgradeResponse obaBundleResponse = new OBABundleUpgradeResponse();
            PendingDetailsRequest req = new PendingDetailsRequest();
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
                menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SemiPostPaid_LoadOBA").ToList();
                // bool isAdmin = (bool)Session["IsAdmin"] || (clientSetting.mvnoSettings.userManagementType.ToUpper() == "ON" && Convert.ToString(Session["ROLE_CAT_TYPE"]) == "2");
                bool isAdmin = menu[0].DirectApproval.ToUpper() == "TRUE" || menu[0].Approval1.ToUpper() == "TRUE";
                if (isAdmin == true)
                {
                    if (Session["PAType"] != null && Convert.ToString(Session["PAType"]) == "OBA ADDITIONAL CREDIT")
                    {
                        req.CountryCode = clientSetting.countryCode;
                        req.BrandCode = clientSetting.brandCode;
                        req.LanguageCode = clientSetting.langCode;
                        req.MSISDN = Convert.ToString(Session["MobileNumber"]);
                        req.Type = Convert.ToString(Session["PAType"]);
                        req.Id = Convert.ToString(Session["PAId"]);

                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        
                            ObjRes = serviceCRM.CRMGetPendingDetails(req);


                            ///FRR--3083
                            if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                            {
                                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManaageOBABundle_" + ObjRes.ResponseDetails.ResponseCode);
                                ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                            }
                            ///FRR--3083

                        
                    }
                }
                obaBundleReq.BrandCode = clientSetting.brandCode;
                obaBundleReq.CountryCode = clientSetting.countryCode;
                obaBundleReq.LanguageCode = clientSetting.langCode;
                obaBundleReq.mode = bundleUpgradeRequest.mode;
                obaBundleReq.requestedBy = bundleUpgradeRequest.requestedBy;
                obaBundleReq.userName = bundleUpgradeRequest.userName;
                obaBundleReq.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
               
                    obaBundleResponse = serviceCRM.CRMOBABundleUpgrade(obaBundleReq);
                    if (isAdmin == true)
                    {
                        if (ObjRes.PendingDetails != null && ObjRes.PendingDetails.Count > 0)
                        {
                            obaBundleResponse.additionalCredit = ObjRes.PendingDetails[0].BundleCode;
                            obaBundleResponse.IsPendingAC = "Y";
                            obaBundleResponse.Id = ObjRes.PendingDetails[0].Id;
                        }
                    }
                
                Session["PAType"] = null;
                Session["PAId"] = null;
                ///FRR--3083
                if (!_runningFromNUnit)
                {
                    if (obaBundleResponse != null && obaBundleResponse.responseDetails != null && obaBundleResponse.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SemiPostPaid_" + obaBundleResponse.responseDetails.ResponseCode);
                        obaBundleResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? obaBundleResponse.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                ///FRR--3083
                return Json(obaBundleResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                obaBundleResponse.responseDetails.ResponseCode = "NA";
                obaBundleResponse.responseDetails.ResponseDesc = ex.Message.ToString();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(obaBundleResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                obaBundleReq = null;
               // obaBundleResponse = null;
                req = null;
                ObjRes = null;
                serviceCRM = null;  
            }

        }

        #region FRR-4653
        public JsonResult ManaageOBACredit(string bundleUpgradeRequest)
        {
            OBABundleUpgradeRequest obaBundleReq = JsonConvert.DeserializeObject<OBABundleUpgradeRequest>(bundleUpgradeRequest);

            //OBABundleUpgradeRequest obaBundleReq = new OBABundleUpgradeRequest();
            OBABundleUpgradeResponse obaBundleResponse = new OBABundleUpgradeResponse();
            PendingDetailsRequest req = new PendingDetailsRequest();
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
                menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SemiPostPaid_LoadOBA").ToList();
                bool isAdmin = menu[0].DirectApproval.ToUpper() == "TRUE" || menu[0].Approval1.ToUpper() == "TRUE";
                if (isAdmin == true)
                {
                  
                    req.CountryCode = clientSetting.countryCode;
                    req.BrandCode = clientSetting.brandCode;
                    req.LanguageCode = clientSetting.langCode;
                    if (Session["MobileNumber"] != null)
                    {
                        req.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    }
                    else
                    {
                        req.MSISDN = obaBundleReq.MSISDN;
                    }
                    if (Session["PAId"] != null)
                    {
                        req.Id = Convert.ToString(Session["PAId"]);
                    }
                    else
                    {
                        req.Id = obaBundleReq.Id;
                    }
                    req.Type = "OBA CREDIT LIMIT UPGRADE/DOWNGRADE";

                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        ObjRes = serviceCRM.CRMGetPendingDetails(req);


                        
                        if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManaageOBABundle_" + ObjRes.ResponseDetails.ResponseCode);
                            ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                        }
                    

                    
                }
               
                obaBundleReq.BrandCode = clientSetting.brandCode;
                obaBundleReq.CountryCode = clientSetting.countryCode;
                obaBundleReq.LanguageCode = clientSetting.langCode;
                obaBundleReq.mode = obaBundleReq.mode;
                obaBundleReq.requestedBy = obaBundleReq.requestedBy;
                obaBundleReq.userName = obaBundleReq.userName;
                if (Session["MobileNumber"] != null)
                {
                    obaBundleReq.MSISDN = Session["MobileNumber"].ToString();
                }
                else
                {
                    obaBundleReq.MSISDN = obaBundleReq.MSISDN;
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    obaBundleResponse = serviceCRM.CRMOBABundleUpgrade(obaBundleReq);
                    if (isAdmin == true)
                    {
                        if (ObjRes.PendingDetails != null && ObjRes.PendingDetails.Count > 0)
                        {
                            obaBundleResponse.OBAadditionalCredit = ObjRes.PendingDetails[0].OBACreditReq;
                            obaBundleResponse.IsPendingOBAC = "Y";
                            obaBundleResponse.Id = ObjRes.PendingDetails[0].Id;
                        }
                    }
                

                if (clientSetting.preSettings.EnableOBADowngrade.ToUpper() == "TRUE")
                {
                    if (obaBundleResponse != null && obaBundleResponse.responseDetails != null && obaBundleResponse.responseDetails.ResponseCode != "0" && obaBundleReq.mode == "Q")
                        obaBundleResponse.responseDetails.ResponseCode = "0";
                }

                Session["PAType"] = null;
                Session["PAId"] = null;
              
                if (!_runningFromNUnit)
                {
                    if (obaBundleResponse != null && obaBundleResponse.responseDetails != null && obaBundleResponse.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SemiPostPaid_" + obaBundleResponse.responseDetails.ResponseCode);
                        obaBundleResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? obaBundleResponse.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                return Json(obaBundleResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                obaBundleResponse.responseDetails.ResponseCode = "NA";
                obaBundleResponse.responseDetails.ResponseDesc = ex.Message.ToString();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(obaBundleResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                obaBundleReq = null;
                // obaBundleResponse = null;
                req = null;
                ObjRes = null;
                serviceCRM = null;
            }

        }
        #endregion
        public JsonResult InsertOBABundle(OBABundleUpgradeRequest bundleUpgradeRequest)
        {
            OBABundleUpgradeRequest obaBundleReq = new OBABundleUpgradeRequest();
            OBABundleUpgradeResponse obaBundleResponse = new OBABundleUpgradeResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
                menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SemiPostPaid_LoadOBA").ToList();


                // bool isAdmin = (bool)Session["IsAdmin"] || (clientSetting.mvnoSettings.userManagementType.ToUpper() == "ON" && Convert.ToString(Session["ROLE_CAT_TYPE"]) == "2");
                bool isAdmin = menu[0].DirectApproval.ToUpper() == "TRUE" || !string.IsNullOrEmpty(bundleUpgradeRequest.Id);
                obaBundleReq.BrandCode = clientSetting.brandCode;
                obaBundleReq.CountryCode = clientSetting.countryCode;
                obaBundleReq.LanguageCode = clientSetting.langCode;
                obaBundleReq.mode = bundleUpgradeRequest.mode;
                obaBundleReq.requestedBy = bundleUpgradeRequest.requestedBy;
                obaBundleReq.oldBundleCode = bundleUpgradeRequest.oldBundleCode;
                obaBundleReq.OBAContractStartDate = bundleUpgradeRequest.OBAContractStartDate;
                obaBundleReq.OBAContractEndDate = bundleUpgradeRequest.OBAContractEndDate;
                obaBundleReq.bundleExpiryDate = bundleUpgradeRequest.bundleExpiryDate;
                obaBundleReq.bundleAutoRenewal = bundleUpgradeRequest.bundleAutoRenewal;
                obaBundleReq.OBAAvailable = bundleUpgradeRequest.OBAAvailable;
                obaBundleReq.OBADue = bundleUpgradeRequest.OBADue;
                obaBundleReq.userName = bundleUpgradeRequest.userName;
                obaBundleReq.PCAC = bundleUpgradeRequest.PCAC;
                obaBundleReq.MSISDN = bundleUpgradeRequest.MSISDN;
                obaBundleReq.newBundleCode = bundleUpgradeRequest.newBundleCode;
                obaBundleReq.amountForRefill = bundleUpgradeRequest.amountForRefill;
                obaBundleReq.OBACreditamountForRefill = bundleUpgradeRequest.OBACreditamountForRefill;
                obaBundleReq.Id = bundleUpgradeRequest.Id;
                obaBundleReq.rejectReason = bundleUpgradeRequest.rejectReason;
                //4653
                obaBundleReq.OBACreditUpgradeDowngrade = bundleUpgradeRequest.OBACreditUpgradeDowngrade;
                obaBundleReq.OBAPlanChangeUpgradeDowngrade = bundleUpgradeRequest.OBAPlanChangeUpgradeDowngrade;
                obaBundleReq.PARequestType = obaBundleReq.PCAC == "PC" ? Resources.HomeResources.PAOBAPlanUpgrade : Resources.HomeResources.PAOBAAdditionalCredit;
                if (obaBundleReq.PCAC == "OBACREDIT")
                {
                    obaBundleReq.PARequestType = "OBA CREDIT LIMIT UPGRADE/DOWNGRADE";
                }

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    obaBundleResponse = serviceCRM.CRMOBABundleUpgrade(obaBundleReq);


                    ///FRR--3083
                    if (!_runningFromNUnit)
                    {
                        if (obaBundleResponse != null && obaBundleResponse.responseDetails != null && obaBundleResponse.responseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SemiPostPaid_" + obaBundleResponse.responseDetails.ResponseCode);
                            obaBundleResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? obaBundleResponse.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                    }
                    ///FRR--3083

                
                return Json(obaBundleResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                obaBundleResponse.responseDetails.ResponseCode = "NA";
                obaBundleResponse.responseDetails.ResponseDesc = ex.Message.ToString();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(obaBundleResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                obaBundleReq = null;
               // obaBundleResponse = null;
                serviceCRM = null;
            }

        }
        [ValidateInput(false)]
        public JsonResult EshopPayment(string ObaBundleRequest1)
        {
            OBABundleResponse obaBundleResponse = new OBABundleResponse();
            OBABundleRequest ObaBundleRequest = new OBABundleRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                //
                ObaBundleRequest = JsonConvert.DeserializeObject<OBABundleRequest>(ObaBundleRequest1);
                ObaBundleRequest.BrandCode = clientSetting.brandCode;
                ObaBundleRequest.CountryCode = clientSetting.countryCode;
                ObaBundleRequest.LanguageCode = clientSetting.langCode;
                ObaBundleRequest.MSISDN = Session["MobileNumber"].ToString();

                if (!string.IsNullOrEmpty(ObaBundleRequest.ConsentDate))
                {
                    ObaBundleRequest.ConsentDate = Utility.GetDateconvertion(ObaBundleRequest.ConsentDate, "MM/DD/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                }
                if (clientSetting.mvnoSettings.CaptureIPAddress.ToUpper() == "ON")
                {
                    ObaBundleRequest.DeviceInfo = Utility.DeviceInfo("OBA Payment");
                }


                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    obaBundleResponse = serviceCRM.CRMOBABundle(ObaBundleRequest);

                    ///FRR--3083
                    if (!_runningFromNUnit)
                    {
                        if (obaBundleResponse != null && obaBundleResponse.responseDetails != null && obaBundleResponse.responseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SemiPostPaid_" + obaBundleResponse.responseDetails.ResponseCode);
                            obaBundleResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? obaBundleResponse.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                    }
                    ///FRR--3083
                
                return Json(obaBundleResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                obaBundleResponse.responseDetails.ResponseCode = "NA";
                obaBundleResponse.responseDetails.ResponseDesc = ex.Message.ToString();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(obaBundleResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                ObaBundleRequest = null;
               // obaBundleResponse = null;
                serviceCRM = null;
            }

        }
        
        [ValidateInput(false)]
        public JsonResult GetTaxVatInfo(string ObaBundleRequest)
        {
            OBABundleRequest ObaBundleRequest1 = JsonConvert.DeserializeObject<OBABundleRequest>(ObaBundleRequest);
            getTaxDetailsRequest taxDetailsRequest = new getTaxDetailsRequest();
            TAXVATResponse taxVatResponse = new TAXVATResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                taxDetailsRequest.MSISDN = Session["MobileNumber"].ToString();
                taxDetailsRequest.IMSI = Session["IMSI"].ToString();
                taxDetailsRequest.ICCID = Session["ICCID"].ToString();
                taxDetailsRequest.ZipCode = ObaBundleRequest1.postCode;
                taxDetailsRequest.BrandCode = clientSetting.brandCode;
                taxDetailsRequest.CountryCode = clientSetting.countryCode;
                taxDetailsRequest.LanguageCode = clientSetting.langCode;
                taxDetailsRequest.Amount = ObaBundleRequest1.OBAAmount;
                taxDetailsRequest.StateCode = ObaBundleRequest1.StateCode;
                taxDetailsRequest.DeliveryZipCode = ObaBundleRequest1.DeliveryZipCode;
                taxDetailsRequest.PaymentZipCode = ObaBundleRequest1.PaymentZipCode;
                taxDetailsRequest.MsisdnZipCode = Session["Msisdnzipcode"] != null ? Session["Msisdnzipcode"].ToString() : "";
                taxDetailsRequest.FeatureName = ObaBundleRequest1.FeatureName; ;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    taxVatResponse = serviceCRM.GetMultiTaxandVat(taxDetailsRequest);
                    Session["taxVatResponse"] = taxVatResponse;


                    ///FRR--3083
                    if (!_runningFromNUnit)
                    {
                        if (taxVatResponse != null && taxVatResponse.CRMResponse != null && taxVatResponse.CRMResponse.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetTaxVatInfo_" + taxVatResponse.CRMResponse.ResponseCode);
                            taxVatResponse.CRMResponse.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? taxVatResponse.CRMResponse.ResponseDesc : errorInsertMsg;
                        }
                    }
                    ///FRR--3083
                
                return Json(taxVatResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                taxVatResponse.CRMResponse.ResponseCode = "NA";
                taxVatResponse.CRMResponse.ResponseDesc = ex.Message.ToString();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(taxVatResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                taxDetailsRequest = null;
              //  taxVatResponse = null;
                serviceCRM = null;
            }

        }
        public JsonResult ManageOBACancelBundle(OBACancelBundle obaCancelBundleReq)
        {
            OBACancelBundleResponse obaBundleResponse = new OBACancelBundleResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                obaCancelBundleReq.BrandCode = clientSetting.brandCode;
                obaCancelBundleReq.CountryCode = clientSetting.countryCode;
                obaCancelBundleReq.LanguageCode = clientSetting.langCode;
                obaCancelBundleReq.MSISDN = Session["MobileNumber"].ToString();
                obaCancelBundleReq.userName = Session["UserName"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    obaBundleResponse = serviceCRM.CRMOBACancelBundle(obaCancelBundleReq);

                    ///FRR--3083
                    if (!_runningFromNUnit)
                    {
                        if (obaBundleResponse != null && obaBundleResponse.responseDetails != null && obaBundleResponse.responseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SemiPostPaid_" + obaBundleResponse.responseDetails.ResponseCode);
                            obaBundleResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? obaBundleResponse.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                    }
                    ///FRR--3083

                
                obaBundleResponse.bundleNames = "<option title='Select' value='Select'>Select</option>";
                if (obaBundleResponse.obaCancelBundleName != null && obaBundleResponse.obaCancelBundleName.Count() > 0)
                {
                    foreach (OBACancelBundleName item in obaBundleResponse.obaCancelBundleName)
                    {
                        obaBundleResponse.bundleNames = obaBundleResponse.bundleNames + "<option title='" + item.BundleName + "' value='" + item.BundleCode + "'>" + item.BundleName + "</option>";
                    }
                }
                return Json(obaBundleResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                obaBundleResponse.responseDetails.ResponseCode = "NA";
                obaBundleResponse.responseDetails.ResponseDesc = ex.Message.ToString();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(obaBundleResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
               // obaBundleResponse = null;
                serviceCRM = null;
            }

        }

        [ValidateInput(false)]
        public PartialViewResult ShowTaxVat(string ObaBundleRequest)
        {
            TAXVATResponse taxVatResponse = new TAXVATResponse();
            OBABundleRequest ObjReq = JsonConvert.DeserializeObject<OBABundleRequest>(ObaBundleRequest);
            try
            {
                if (Session["taxVatResponse"] != null)
                    taxVatResponse = (ServiceCRM.TAXVATResponse)Session["taxVatResponse"];
                ObjReq.TaxDetailsReponseTax = taxVatResponse.TaxDetailsReponseTax;

                return PartialView("ShowTaxVat", ObjReq);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return PartialView("ShowTaxVat", ObjReq);
            }
            finally
            {
                taxVatResponse = null;
            }
        }
        public PartialViewResult ShowRenewalChange(string ObaBundleRequest)
        {

            ViewBag.OBARenewalStatusChange = ObaBundleRequest;
            return PartialView("OBARenewalStatusChange", ViewBag.OBARenewalStatusChange);
         
        }

        public JsonResult CRMOBAAutoRenewal(string ObaBundleRequestt)
        {
            CRMResponse crmResponse = new CRMResponse();
            OBABundleRequest ObaBundleRequest = new OBABundleRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObaBundleRequest = JsonConvert.DeserializeObject<OBABundleRequest>(ObaBundleRequestt);
                ObaBundleRequest.BrandCode = clientSetting.brandCode;
                ObaBundleRequest.CountryCode = clientSetting.countryCode;
                ObaBundleRequest.LanguageCode = clientSetting.langCode;
                ObaBundleRequest.MSISDN = Session["MobileNumber"].ToString();
                ObaBundleRequest.userName = Session["UserName"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    crmResponse = serviceCRM.CRMOBAAutoRenewal(ObaBundleRequest);

                    ///FRR--3083
                    if (!_runningFromNUnit)
                    {
                        if (crmResponse != null && crmResponse.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMOBAAutoRenewal_" + crmResponse.ResponseCode);
                            crmResponse.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? crmResponse.ResponseDesc : errorInsertMsg;
                        }
                    }
                    ///FRR--3083

                
                return Json(crmResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                crmResponse.ResponseCode = "NA";
                crmResponse.ResponseDesc = ex.Message.ToString();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(crmResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
               // crmResponse = null;
                serviceCRM = null;
            }
            
        }

        #region "View & Pay Bill for Postpaid"
        public ActionResult Viewpaybill()
        {

            try
            {
                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                string regFilepath = Path.Combine(clientSetting.mvnoSettings.postpaidTempfolder, FolderName);
                if (Directory.Exists(regFilepath))
                {
                    System.IO.DirectoryInfo di = new DirectoryInfo(regFilepath);

                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }

            return View();
        }


        public JsonResult GetPostPaidBillDetails(string PostPaidBillDetails)
        {
            GetPostPaidBillDetailsReq objReq = new GetPostPaidBillDetailsReq();
            GetPostPaidBillDetailsRes ObjRes = new GetPostPaidBillDetailsRes();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objReq = JsonConvert.DeserializeObject<GetPostPaidBillDetailsReq>(PostPaidBillDetails);
                objReq.BrandCode = clientSetting.brandCode;
                objReq.CountryCode = clientSetting.countryCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.Msisdn = Session["MobileNumber"].ToString();
                objReq.UserName = Session["UserName"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.GetPostPaidBillDetails(objReq);
                    if (!_runningFromNUnit)
                    {
                        if (ObjRes != null && ObjRes.Response.ResponseDesc != null && ObjRes.Response.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMSearchSim_" + ObjRes.Response.ResponseCode);
                            ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;
                        }
                    }
                
                return Json(ObjRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(ObjRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objReq = null;
               // ObjRes = null;
                serviceCRM = null;
            }
            

        }


        public FileResult FileDownloadFTPSServer(string LocalFilepath, string FileName, CRMBase Input)
        {
            FtpWebRequest reqFTP;
            var mimeType = "";
            var fileDownloadName = "";
            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), Convert.ToString(Session["UserName"]), "First Time");
            try
            {
                if (clientSetting.mvnoSettings.fTPRSFTP.ToUpper() == "FTP")
                {
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(clientSetting.mvnoSettings.postpaidfTPSHost + "/" + FileName));
                    reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                    reqFTP.UsePassive = true;
                    reqFTP.UseBinary = true;
                    reqFTP.KeepAlive = false;
                    reqFTP.EnableSsl = true;

                    ServicePointManager.ServerCertificateValidationCallback =
                        delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                        {
                            return true;
                        };
                    reqFTP.Credentials = new NetworkCredential(clientSetting.mvnoSettings.postpaidfTPSUserName, clientSetting.mvnoSettings.postpaidfTPSPassword);
                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();

                    mimeType = ReturnExtension(FileName);
                    fileDownloadName = FileName;
                    return File(response.GetResponseStream(), mimeType, fileDownloadName);

                }
                else
                {

                    var host = clientSetting.mvnoSettings.postpaidsftpServerIp;
                    // var Host = clientSetting.mvnoSettings.postpaidsftpServerIp;
                    var fUserName = clientSetting.mvnoSettings.postpaidsftpUserName;
                    var fPassword = clientSetting.mvnoSettings.postpaidsftpPassword;
                    string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;

                    string targetPath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                    //string targetPath = Path.Combine(clientSetting.mvnoSettings.postpaidTempfolder, FolderName);
                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }


                    //Host = Host.Replace("/", string.Empty);
                    //Sftp sftp = new Sftp(Host, fUserName, fPassword);
                    //try
                    //{
                    //    string remoteFile_SFTP =  clientSetting.mvnoSettings.postpaidsftpFilePath + "/" + FileName;
                    //    sftp.Connect(Convert.ToInt32(clientSetting.mvnoSettings.postpaidsftpPort.Trim()));
                    //    sftp.Get(remoteFile_SFTP, targetPath);


                    //    mimeType = ReturnExtension(FolderName);
                    //    fileDownloadName = FolderName;
                    //    return File(targetPath, mimeType, fileDownloadName);

                    //}

                    host = clientSetting.mvnoSettings.sftpServerIp;
                    host = host.Replace("/", string.Empty);
                    Sftp sftp = new Sftp(host, clientSetting.mvnoSettings.sftpUserName, clientSetting.mvnoSettings.sftpPassword);
                    try
                    {
                        string remoteFile_SFTP = "";
                        sftp.Connect(Convert.ToInt32(clientSetting.mvnoSettings.sftpPort.Trim()));

                        string registrationDocpath = clientSetting.mvnoSettings.sftpFilePath;


                        remoteFile_SFTP = registrationDocpath + "/" + FileName;

                        remoteFile_SFTP = remoteFile_SFTP.Replace("\\", "/");


                        FileName = Path.GetFileName(FileName);

                        if (!System.IO.File.Exists(remoteFile_SFTP))
                        {
                            sftp.Get(remoteFile_SFTP, targetPath + "/" + FileName);
                        }

                        mimeType = ReturnExtension(FileName);
                        fileDownloadName = FileName;
                        return File(targetPath + '\\' + FileName, mimeType, fileDownloadName);

                    }

                    catch (Exception ex)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                        return null;
                    }
                    finally
                    {
                        sftp.Close();
                    }
                }


            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return null;

            }

        }





        private string ReturnExtension(string Filename)
        {
            string fileExtension = Filename.Substring(Filename.LastIndexOf('.'));
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

        #endregion
    }
}
