using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CRM.Models;
using Newtonsoft.Json;
using ServiceCRM;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System.Net.Mail;
using System.Web.UI.WebControls;

namespace CRM.Controllers
{
    [ValidateState]
    public class BundleController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();

        public ActionResult UpgradeDowngradePlan()
        {
            ActivePlanDetailsResponse bundlePurchaseResp = new ActivePlanDetailsResponse();
            ActivePlanDetailsRequest bundlePurchaseReq = new ActivePlanDetailsRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    bundlePurchaseReq.CountryCode = clientSetting.countryCode;
                    bundlePurchaseReq.BrandCode = clientSetting.brandCode;
                    bundlePurchaseReq.LanguageCode = clientSetting.langCode;
                    bundlePurchaseReq.MSISDN = Session["MobileNumber"].ToString();
                    bundlePurchaseResp = serviceCRM.CRMActivePlanDetails(bundlePurchaseReq);

                    if (bundlePurchaseResp != null && bundlePurchaseResp.responseDetails != null && bundlePurchaseResp.responseDetails.ResponseCode != null)
                    {
                        if (bundlePurchaseResp.responseDetails.ResponseCode != "0")
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_Purchase_" + bundlePurchaseResp.responseDetails.ResponseCode);
                            bundlePurchaseResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundlePurchaseResp.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                    }
                    return View(bundlePurchaseResp);

                
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(bundlePurchaseResp);
            }
            finally
            {
                //bundlePurchaseResp = null;
                serviceCRM = null;
                bundlePurchaseReq = null;
            }

        }
        public JsonResult ValidateActivePlanUpgradeDowngrade(ValidateActivePlanRequest ValidateActivePlanReq)
        {
            ValidateActivePlanResponse ValidateActivarePlanResp = new ValidateActivePlanResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - ValidateActivePlanUpgradeDowngrade Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ValidateActivePlanReq.CountryCode = clientSetting.countryCode;
                ValidateActivePlanReq.BrandCode = clientSetting.brandCode;
                ValidateActivePlanReq.LanguageCode = clientSetting.langCode;
                ValidateActivePlanReq.MSISDN = Session["MobileNumber"].ToString();
                ValidateActivarePlanResp = serviceCRM.CRMValidateActivePlanDetails(ValidateActivePlanReq);
                if (ValidateActivarePlanResp != null && ValidateActivarePlanResp.responseDetails != null && ValidateActivarePlanResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_BundleCost_" + ValidateActivarePlanResp.responseDetails.ResponseCode);
                    ValidateActivarePlanResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ValidateActivarePlanResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - ValidateActivePlanUpgradeDowngrade End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - ValidateActivePlanUpgradeDowngrade - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ValidateActivarePlanResp, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CalculateChangeOffering(CalculateChangeOfferingBundleCostPlanRequest ChangeOfferingPlanReq)
        {
            CalculateChangeOfferingBundleCostPlanResponse CalcualteChangeOfferingResp = new CalculateChangeOfferingBundleCostPlanResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            ProductChange_Offering ProductOffering = new ProductChange_Offering();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - CalculateChangeOffering Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ChangeOfferingPlanReq.CountryCode = clientSetting.countryCode;
                ChangeOfferingPlanReq.BrandCode = clientSetting.brandCode;
                ChangeOfferingPlanReq.LanguageCode = clientSetting.langCode;
                ChangeOfferingPlanReq.MSISDN = Session["MobileNumber"].ToString();
                //ChangeOfferingPlanReq.ChangeOfferingType = "UPGRADE";
                //ChangeOfferingPlanReq.CurrentActiveBundle = "101";
                ChangeOfferingPlanReq.ChangeOffering_Bundles = new List<ProductChange_Offering>();
                ProductOffering.TYPE = "BUNDLE";
                ProductOffering.VALUE = ChangeOfferingPlanReq.InstallmentInfo.ToString().Split('-').GetValue(0).ToString();
                ProductOffering.INSTALLMENT = ChangeOfferingPlanReq.InstallmentInfo.ToString().Split('-').GetValue(1).ToString();
                ChangeOfferingPlanReq.ChangeOffering_Bundles.Add(ProductOffering);
                CalcualteChangeOfferingResp = serviceCRM.CRMCalculate_ChangeOffering_Cost(ChangeOfferingPlanReq);
                if (CalcualteChangeOfferingResp != null && CalcualteChangeOfferingResp.responseDetails != null && CalcualteChangeOfferingResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_BundleCost_" + CalcualteChangeOfferingResp.responseDetails.ResponseCode);
                    CalcualteChangeOfferingResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CalcualteChangeOfferingResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - CalculateChangeOffering End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - CalculateChangeOffering - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(CalcualteChangeOfferingResp, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpgradeDowngradePlanPurchase(CalculateChangeOfferingBundleCostPlanRequest ChangeOfferingPlanReq)
        {
            CalculateChangeOfferingBundleCostPlanResponse CalcualteChangeOfferingResp = new CalculateChangeOfferingBundleCostPlanResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            ProductChange_Offering ProductOffering = new ProductChange_Offering();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - UpgradeDowngradePlanPurchase Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ChangeOfferingPlanReq.CountryCode = clientSetting.countryCode;
                ChangeOfferingPlanReq.BrandCode = clientSetting.brandCode;
                ChangeOfferingPlanReq.LanguageCode = clientSetting.langCode;
                ChangeOfferingPlanReq.MSISDN = Session["MobileNumber"].ToString();
                
                ChangeOfferingPlanReq.ChangeOffering_Bundles = new List<ProductChange_Offering>();
                ProductOffering.TYPE = "BUNDLE";
                ProductOffering.VALUE = ChangeOfferingPlanReq.CurrentActiveBundle.Split('-').GetValue(1).ToString();
                ProductOffering.INSTALLMENT = ChangeOfferingPlanReq.CurrentActiveBundle.Split('-').GetValue(2).ToString();
                ChangeOfferingPlanReq.CurrentActiveBundle = ChangeOfferingPlanReq.CurrentActiveBundle.Split('-').GetValue(0).ToString();
                ChangeOfferingPlanReq.ChangeOffering_Bundles.Add(ProductOffering);
                CalcualteChangeOfferingResp = serviceCRM.CRMUpgradeDowngradeBundlePurchase(ChangeOfferingPlanReq);
                if (CalcualteChangeOfferingResp != null && CalcualteChangeOfferingResp.responseDetails != null && CalcualteChangeOfferingResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_BundleCost_" + CalcualteChangeOfferingResp.responseDetails.ResponseCode);
                    CalcualteChangeOfferingResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CalcualteChangeOfferingResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - UpgradeDowngradePlanPurchase End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - UpgradeDowngradePlanPurchase - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(CalcualteChangeOfferingResp, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Purchase(string Textdata)
        {
            BundlePurchaseResponse bundlePurchaseResp = new BundlePurchaseResponse();
            BundlePurchaseRequest bundlePurchaseReq = new BundlePurchaseRequest();
            string isretailer = string.Empty;
            string simcategory = string.Empty;
            ServiceInvokeCRM serviceCRM;
            try
            {
                 serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    bundlePurchaseReq.CountryCode = clientSetting.countryCode;
                    bundlePurchaseReq.BrandCode = clientSetting.brandCode;
                    bundlePurchaseReq.LanguageCode = clientSetting.langCode;

                    #region FRR 4925
                    if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                    {
                        Session["RealICCIDForMultiTab"] = Textdata;
                        Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                        bundlePurchaseReq.MSISDN = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                        bundlePurchaseReq.ICCID = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Key).First().ToString();
                        bundlePurchaseReq.SIM_CATEGORY = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.SIMCategory).First().ToString();
                        isretailer = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.isRetailer).First().ToString();
                        simcategory = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.SIMCategory).First().ToString();


                    }
                    else
                    {
                        bundlePurchaseReq.MSISDN = Session["MobileNumber"].ToString();
                        bundlePurchaseReq.SIM_CATEGORY = Session["SIM_CATEGORY"].ToString();
                        bundlePurchaseReq.ICCID = Session["ICCID"].ToString();
                        isretailer = Session["isRetailer"] != null ? Session["isRetailer"].ToString() : string.Empty;
                        simcategory = Session["SIM_CATEGORY"] != null ? Session["SIM_CATEGORY"].ToString() : string.Empty;
                    }
                    #endregion

                    bundlePurchaseReq.Mode = "Q";
                    if (Session["isPrePaid"].ToString() == "1")
                    {
                        if (!string.IsNullOrEmpty(isretailer) && Convert.ToString(isretailer) == "0")
                        {
                            bundlePurchaseResp.responseDetails = new CRMResponse();
                            bundlePurchaseResp.responseDetails.ResponseCode = "52";
                            bundlePurchaseResp.responseDetails.ResponseDesc = "The subscrber is white listed";
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_Purchase_" + bundlePurchaseResp.responseDetails.ResponseCode);
                            bundlePurchaseResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundlePurchaseResp.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                        if (!string.IsNullOrEmpty(simcategory) && Convert.ToString(simcategory) == "GO_ONLINE_SIM")
                        {
                            bundlePurchaseResp.responseDetails = new CRMResponse();
                            bundlePurchaseResp.responseDetails.ResponseCode = "4357";
                            bundlePurchaseResp.responseDetails.ResponseDesc = "The Go_Online subscriber Can Purchase Bundle through only CC/DC";
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_Purchase_" + bundlePurchaseResp.responseDetails.ResponseCode);
                            bundlePurchaseResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundlePurchaseResp.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                        else
                        {
                            bundlePurchaseReq.ServiceType = "Pre";

                            bundlePurchaseResp = serviceCRM.CRMBundlePurchase(bundlePurchaseReq);

                            if (bundlePurchaseResp != null && bundlePurchaseResp.responseDetails != null && bundlePurchaseResp.responseDetails.ResponseCode != null)
                            {
                                if (bundlePurchaseResp.responseDetails.ResponseCode == "907")
                                {
                                    string MultilangSimstatus = Resources.DropdownResources.ResourceManager.GetString("Simstatus_" + Regex.Replace(Session["LIFECYCLESTATE"].ToString().ToLower(), @"[^0-9a-zA-Z\._]", string.Empty));
                                    bundlePurchaseResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(MultilangSimstatus) ? @Resources.ErrorResources.CrmValidate889 : (@Resources.ErrorResources.GeneralMsg + ' ' + MultilangSimstatus);
                                }
                                else
                                {
                                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_Purchase_" + bundlePurchaseResp.responseDetails.ResponseCode);
                                    bundlePurchaseResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundlePurchaseResp.responseDetails.ResponseDesc : errorInsertMsg;
                                }
                            }

                            if (clientSetting.preSettings.recurringBundleEnabler.Trim().ToLower() == "true")
                            {
                                CRMBase crmBase = new CRMBase();
                                crmBase.CountryCode = clientSetting.countryCode;
                                crmBase.BrandCode = clientSetting.brandCode;
                                crmBase.LanguageCode = clientSetting.langCode;

                                BundleCategoryRes objBundleCategory = serviceCRM.LoadBundleCategory(crmBase);
                                try
                                {
                                    if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
                                    {
                                    bundlePurchaseResp.lstPreBundle.ForEach(
                                        bundle =>
                                        {
                                            bundle.BundleCategory = (objBundleCategory.objBundleCategory.Count() > 0 ? objBundleCategory.objBundleCategory.FirstOrDefault(a => a.bundle_mode == bundle.RecurringMode).bundle_category : string.Empty);
                                            bundle.AutoRenewal = (objBundleCategory.objBundleCategory.Count() == 0 ? "FASLE" : objBundleCategory.objBundleCategory.FirstOrDefault(a => a.bundle_mode == bundle.RecurringMode).isActive ? "TRUE" : "FALSE");
                                        }
                                        );
                                    }

                                    if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
                                    {

                                    bundlePurchaseResp.lstPreSpecialBundle.ForEach(
                                     bundle =>
                                     {
                                         bundle.BundleCategory = (objBundleCategory.objBundleCategory.Count() > 0 ? objBundleCategory.objBundleCategory.FirstOrDefault(a => a.bundle_mode == bundle.RecurringMode).bundle_category : string.Empty);
                                         bundle.AutoRenewal = (objBundleCategory.objBundleCategory.Count() == 0 ? "FASLE" : objBundleCategory.objBundleCategory.FirstOrDefault(a => a.bundle_mode == bundle.RecurringMode).isActive ? "TRUE" : "FALSE");
                                     }
                                     );
                                }
                                    //POF-6046
                                    if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
                                    {

                                        bundlePurchaseResp.lstFlexiBundle.ForEach(
                                         bundle =>
                                         {
                                             bundle.BundleCategory = (objBundleCategory.objBundleCategory.Count() > 0 ? objBundleCategory.objBundleCategory.FirstOrDefault(a => a.bundle_mode == bundle.RecurringMode).bundle_category : string.Empty);
                                             bundle.AutoRenewal = (objBundleCategory.objBundleCategory.Count() == 0 ? "FASLE" : objBundleCategory.objBundleCategory.FirstOrDefault(a => a.bundle_mode == bundle.RecurringMode).isActive ? "TRUE" : "FALSE");
                                         });                                         
                                    }
                                }
                                catch (Exception eX)
                                {
                                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                                }
                                finally
                                {
                                    crmBase = null;
                                    objBundleCategory = null;
                                }
                            }

                            if (clientSetting.mvnoSettings.EnablePortugalVATScope != "0")
                            {
                                InvoiceAddressRequest invoicereq = new InvoiceAddressRequest();
                                InvoiceAddressResponse invoiceres = new InvoiceAddressResponse();
                                try
                                {
                                    invoicereq.BrandCode = clientSetting.brandCode;
                                    invoicereq.CountryCode = clientSetting.countryCode;
                                    invoicereq.LanguageCode = clientSetting.langCode;
                                    invoicereq.Msisdn = bundlePurchaseReq.MSISDN.ToString();
                                    invoiceres = serviceCRM.InvoiceAddressCRM(invoicereq);
                                    //bundlePurchaseResp.PdfAddress = invoiceres.contentPDF;
                                    bundlePurchaseResp.EmailLanguage = invoiceres.EmailLanguage;
                                    Session["PRTAddress"] = invoiceres.contentPDF;
                                }
                                catch (Exception eX)
                                {
                                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                                }
                                finally
                                {
                                    invoicereq = null;
                                    invoiceres = null;
                                }

                            }
                            else
                            {
                                if (clientSetting.preSettings.EnableRegulatoryFee.ToLower() == "true")
                                {
                                    InvoiceAddressRequest invoicereq = new InvoiceAddressRequest();
                                    InvoiceAddressResponse invoiceres = new InvoiceAddressResponse();
                                    try
                                    {
                                        invoicereq.BrandCode = clientSetting.brandCode;
                                        invoicereq.CountryCode = clientSetting.countryCode;
                                        invoicereq.LanguageCode = clientSetting.langCode;
                                        invoicereq.Msisdn = bundlePurchaseReq.MSISDN;
                                        invoiceres = serviceCRM.InvoiceAddressCRM(invoicereq);
                                        //bundlePurchaseResp.PdfAddress = invoiceres.contentPDF;
                                        bundlePurchaseResp.EmailLanguage = invoiceres.EmailLanguage;
                                        Session["GeneralAddress"] = invoiceres.contentPDF;
                                    }
                                    catch (Exception eX)
                                    {
                                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                                    }
                                    finally
                                    {
                                        invoicereq = null;
                                        invoiceres = null;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        bundlePurchaseReq.ServiceType = "Post";

                        bundlePurchaseResp = serviceCRM.CRMBundlePurchase(bundlePurchaseReq);

                        if (bundlePurchaseResp != null && bundlePurchaseResp.responseDetails != null && bundlePurchaseResp.responseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_Purchase_Req_" + bundlePurchaseResp.responseDetails.ResponseCode);
                            bundlePurchaseResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundlePurchaseResp.responseDetails.ResponseDesc : errorInsertMsg;
                        }

                        Session["eMail"] = bundlePurchaseResp.postPlanName.Email;
                        Session["SubscriberName"] = bundlePurchaseResp.postPlanName.UserName;
                    }
                
                return View(bundlePurchaseResp);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(bundlePurchaseResp);
            }
            finally
            {
                //bundlePurchaseResp = null;
                serviceCRM = null;
                bundlePurchaseReq = null;
            }

        }


        //FRR-4946
        [HttpPost]
        public JsonResult Purchasechangemonth(BundlePurchaseRequest bundlePurchaseReq)
        {
            BundleMultiMonthBundle bundlePurchaseResp = new BundleMultiMonthBundle();
            ServiceInvokeCRM serviceCRM;

            try
            {
                bundlePurchaseReq.CountryCode = clientSetting.countryCode;
                bundlePurchaseReq.BrandCode = clientSetting.brandCode;
                bundlePurchaseReq.LanguageCode = clientSetting.langCode;
                #region FRR 4925
                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {

                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    bundlePurchaseReq.MSISDN = localDict.Where(x => bundlePurchaseReq.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();

                }
                else
                {
                    bundlePurchaseReq.MSISDN = Session["MobileNumber"].ToString();
                }
                #endregion
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    bundlePurchaseResp = serviceCRM.CRMchangemonth(bundlePurchaseReq);
                
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                bundlePurchaseReq = null;
            }
            return Json(bundlePurchaseResp);
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult BundlePurchase(BundlePurchaseRequest bundlePurchaseReq)
        {
            BundlePurchaseResponse bundlePurchaseResp = new BundlePurchaseResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - ActionResult BundlePurchase Start");
               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    bundlePurchaseReq.CountryCode = clientSetting.countryCode;
                    bundlePurchaseReq.BrandCode = clientSetting.brandCode;
                    bundlePurchaseReq.LanguageCode = clientSetting.langCode;
                    bundlePurchaseReq.Channel = "CRM";

                    if(clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                    {
                        Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                        bundlePurchaseReq.MSISDN = localDict.Where(x => bundlePurchaseReq.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                        bundlePurchaseReq.ICCID = localDict.Where(x => bundlePurchaseReq.textdata.ToString().Contains(x.Key)).Select(x => x.Key).First().ToString();
                        bundlePurchaseReq.SIM_CATEGORY = localDict.Where(x => bundlePurchaseReq.textdata.ToString().Contains(x.Key)).Select(x => x.Value.SIMCategory).First().ToString();
                        bundlePurchaseReq.ParentMSISDN = localDict.Where(x => bundlePurchaseReq.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    }
                    else
                    {
                        bundlePurchaseReq.MSISDN = Session["MobileNumber"].ToString();
                        bundlePurchaseReq.SIM_CATEGORY = Session["SIM_CATEGORY"].ToString();
                        bundlePurchaseReq.ICCID = Session["ICCID"].ToString();
                        bundlePurchaseReq.ParentMSISDN = Session["MobileNumber"].ToString();
                    }



                    bundlePurchaseReq.User = Session["UserName"].ToString();
                    bundlePurchaseReq.Mode = "I";
                    bool containsAny = false;
          
                  
                    if (Session["isPrePaid"].ToString() == "1")
                    {
                      
                        bundlePurchaseReq.DISCOUNTTAG = Session["DiscountTag"] != null ? Session["DiscountTag"].ToString() : string.Empty;
                        bundlePurchaseReq.ServiceType = "Pre";

                        if (!string.IsNullOrEmpty(bundlePurchaseReq.effectiveDate))
                        {
                            try
                            {
                                bundlePurchaseReq.effectiveDate = Utility.GetDateconvertion(bundlePurchaseReq.effectiveDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
                            }
                            catch
                            {

                            }
                        }

                        if (clientSetting.mvnoSettings.CaptureIPAddress.ToUpper() == "ON")
                        {
                            bundlePurchaseReq.DeviceInfo = Utility.DeviceInfo("Bundle Purchase");
                        }


                        bundlePurchaseResp = serviceCRM.CRMBundlePurchase(bundlePurchaseReq);

                        if (bundlePurchaseResp != null && bundlePurchaseResp.responseDetails != null && bundlePurchaseResp.responseDetails.ResponseCode != null)
                        {
                            string[] values = new[] { "11", "22", "33", "44", "55", "66", "77", "88", "99" };
                            containsAny = Array.Exists(values, element => element == bundlePurchaseResp.responseDetails.ResponseCode);

                            if (containsAny)
                            {
                                if (clientSetting.preSettings.enableComponentBundle.ToLower() == "true")
                                {
                                    string strbundles = Resources.ErrorResources.ResourceManager.GetString("Bundle_Purchase_Pre_201");
                                    string strand = Resources.ErrorResources.ResourceManager.GetString("Bundle_Purchase_Pre_202");
                                    string strSuccess = Resources.ErrorResources.ResourceManager.GetString("Bundle_Purchase_Pre_203");

                                    string errCode = Resources.ErrorResources.ResourceManager.GetString("Bundle_Purchase_Pre_" + bundlePurchaseResp.responseDetails.ResponseCode);

                                    bundlePurchaseResp.responseDetails.ResponseDesc = strbundles + " " + bundlePurchaseResp.preBundleResponse.bundleName + " " + strand + " " + bundlePurchaseResp.componentBundle.BundleName + " " + strSuccess;

                                    bundlePurchaseResp.responseDetails.ResponseDesc = bundlePurchaseResp.responseDetails.ResponseDesc + " " + errCode;
                                    bundlePurchaseResp.preBundleResponse.ErrDesc = bundlePurchaseResp.responseDetails.ResponseDesc;
                                }
                                else
                                {
                                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_Purchase_Pre_" + bundlePurchaseResp.responseDetails.ResponseCode);
                                    bundlePurchaseResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundlePurchaseResp.responseDetails.ResponseDesc : errorInsertMsg;
                                    if (bundlePurchaseResp.preBundleResponse != null)
                                    {
                                        bundlePurchaseResp.preBundleResponse.ErrDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundlePurchaseResp.preBundleResponse.ErrDesc : errorInsertMsg;
                                    }
                                }
                            }
                            else
                            {
                                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_Purchase_Pre_" + bundlePurchaseResp.responseDetails.ResponseCode);
                                bundlePurchaseResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundlePurchaseResp.responseDetails.ResponseDesc : errorInsertMsg;
                                if (bundlePurchaseResp.preBundleResponse != null)
                                {
                                    bundlePurchaseResp.preBundleResponse.ErrDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundlePurchaseResp.preBundleResponse.ErrDesc : errorInsertMsg;
                                }
                            }
                        }

                        try
                        {
                            if (bundlePurchaseResp.preBundleResponse != null && !string.IsNullOrEmpty(bundlePurchaseResp.preBundleResponse.ExpiryDate))
                            {
                                bundlePurchaseResp.preBundleResponse.ExpiryDate = Utility.FormatDateTime(bundlePurchaseResp.preBundleResponse.ExpiryDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.ddMMyyyy);
                            }
                        }
                        catch (Exception)
                        {

                        }

                        if (bundlePurchaseResp.preBundleResponse != null)
                        {
                            if ((bundlePurchaseResp.preBundleResponse.ReturnCode == "0" || bundlePurchaseResp.preBundleResponse.ReturnCode == "00" || bundlePurchaseResp.preBundleResponse.ReturnCode == "000" || bundlePurchaseResp.preBundleResponse.ReturnCode == "0000" || bundlePurchaseResp.preBundleResponse.ReturnCode == "00000" || bundlePurchaseResp.preBundleResponse.ReturnCode == "2001"
                               || bundlePurchaseResp.preBundleResponse.ReturnCode == "2002" || bundlePurchaseResp.preBundleResponse.ReturnCode == "2003" || bundlePurchaseResp.preBundleResponse.ReturnCode == "2004" || bundlePurchaseResp.preBundleResponse.ReturnCode == "2005" || bundlePurchaseResp.preBundleResponse.ReturnCode == "2006"
                                || bundlePurchaseResp.preBundleResponse.ReturnCode == "997" || bundlePurchaseResp.preBundleResponse.ReturnCode == "998") || containsAny)
                            {
                                bundlePurchaseResp.preBundleResponse.ReturnCode = "0";

                                try
                                {
                                    if (!string.IsNullOrEmpty(bundlePurchaseReq.PreReceiverMSISDN))
                                    {
                                        string strSlmMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_Purchase_Pre_78");
                                        if (!string.IsNullOrEmpty(bundlePurchaseResp.preBundleResponse.ErrDesc))
                                        {
                                            bundlePurchaseResp.preBundleResponse.ErrDesc = bundlePurchaseResp.preBundleResponse.ErrDesc + " " + strSlmMsg;
                                        }
                                    }

                                    if (bundlePurchaseResp.componentBundle != null && !string.IsNullOrEmpty(bundlePurchaseResp.componentBundle.BundleCode) && bundlePurchaseResp.componentBundle.BundleCode == "1145")
                                    {
                                        string strResMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_Purchase_Pre_1145");
                                        if (!string.IsNullOrEmpty(bundlePurchaseResp.preBundleResponse.ErrDesc))
                                        {
                                            bundlePurchaseResp.preBundleResponse.ErrDesc = strResMsg + ", " + bundlePurchaseResp.preBundleResponse.ErrDesc;
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(bundlePurchaseReq.InvoiceVatNo) && Convert.ToString(Session["Verify"]) == "1" && clientSetting.mvnoSettings.EnablePortugalVATScope != "0")
                                    {
                                        Session["InvoiceVatNo"] = bundlePurchaseReq.InvoiceVatNo;
                                    }
                                    if (clientSetting.mvnoSettings.EnablePortugalVATScope != "0" && bundlePurchaseResp != null && bundlePurchaseResp.preBundleResponse != null)
                                    {
                                        bundlePurchaseResp.preBundleResponse.VatPerc = bundlePurchaseResp.preBundleResponse.VatPerc;
                                        bundlePurchaseResp.preBundleResponse.DateTime = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                                        bundlePurchaseResp.preBundleResponse.DateTime = bundlePurchaseResp.preBundleResponse.DateTime.Replace("DD", Convert.ToString(System.DateTime.Now.Day));
                                        //bundlePurchaseResp.preBundleResponse.DateTime = bundlePurchaseResp.preBundleResponse.DateTime.Replace("MM", Convert.ToString(Convert.ToInt32(System.DateTime.Now.Month) - 1));
                                        bundlePurchaseResp.preBundleResponse.DateTime = bundlePurchaseResp.preBundleResponse.DateTime.Replace("MM", Convert.ToString(Convert.ToInt32(System.DateTime.Now.Month)));
                                        bundlePurchaseResp.preBundleResponse.DateTime = bundlePurchaseResp.preBundleResponse.DateTime.Replace("YYYY", Convert.ToString(System.DateTime.Now.Year));
                                        bundlePurchaseResp.preBundleResponse.DateTime = bundlePurchaseResp.preBundleResponse.DateTime + " " + Convert.ToString(System.DateTime.Now.Hour) + ":" + Convert.ToString(System.DateTime.Now.Minute) + ":" + Convert.ToString(System.DateTime.Now.Second);

                                    }
                                    else
                                    {
                                        if (clientSetting.preSettings.EnableRegulatoryFee.ToLower() == "true")
                                        {
                                            bundlePurchaseResp.preBundleResponse.DateTime = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                                            bundlePurchaseResp.preBundleResponse.DateTime = bundlePurchaseResp.preBundleResponse.DateTime.Replace("DD", Convert.ToString(System.DateTime.Now.Day));
                                           // bundlePurchaseResp.preBundleResponse.DateTime = bundlePurchaseResp.preBundleResponse.DateTime.Replace("MM", Convert.ToString(Convert.ToInt32(System.DateTime.Now.Month) - 1));
                                            bundlePurchaseResp.preBundleResponse.DateTime = bundlePurchaseResp.preBundleResponse.DateTime.Replace("MM", Convert.ToString(Convert.ToInt32(System.DateTime.Now.Month)));
                                            bundlePurchaseResp.preBundleResponse.DateTime = bundlePurchaseResp.preBundleResponse.DateTime.Replace("YYYY", Convert.ToString(System.DateTime.Now.Year));
                                            bundlePurchaseResp.preBundleResponse.DateTime = bundlePurchaseResp.preBundleResponse.DateTime + " " + Convert.ToString(System.DateTime.Now.Hour) + ":" + Convert.ToString(System.DateTime.Now.Minute) + ":" + Convert.ToString(System.DateTime.Now.Second);
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }

                        return PartialView("~/Views/Bundle/Purchase/Response.cshtml", bundlePurchaseResp.preBundleResponse);
                    }
                    else
                    {
                        bundlePurchaseReq.BoosterName = new List<string>(bundlePurchaseReq.MYBCode.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                        bundlePurchaseReq.ServiceType = "Post";

                        bundlePurchaseResp = serviceCRM.CRMBundlePurchase(bundlePurchaseReq);

                        if (bundlePurchaseResp != null && bundlePurchaseResp.responseDetails != null && bundlePurchaseResp.responseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_Purchase_Post_" + bundlePurchaseResp.responseDetails.ResponseCode);
                            bundlePurchaseResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundlePurchaseResp.responseDetails.ResponseDesc : errorInsertMsg;
                        }

                        return Json(bundlePurchaseResp.postBundleReponse, JsonRequestBehavior.AllowGet);
                    }
                
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - BundlePurchase - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - ActionResult BundlePurchase End");
            }
            return View();
        }

        //4635
        [HttpPost]
        public JsonResult FlexiBundleCustomize(BundlePurchaseRequest bundlePurchaseReq)
        {
            BundlePurchaseResponse bundlePurchaseResp = new BundlePurchaseResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - ActionResult FlexiBundleCustomize Start");
                bundlePurchaseReq.CountryCode = clientSetting.countryCode;
                bundlePurchaseReq.BrandCode = clientSetting.brandCode;
                bundlePurchaseReq.LanguageCode = clientSetting.langCode;
                bundlePurchaseReq.Channel = "CRM";
                bundlePurchaseReq.User = Session["UserName"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                              
                    bundlePurchaseResp = serviceCRM.CRMBundlePurchase(bundlePurchaseReq);
                

                if (bundlePurchaseResp != null && bundlePurchaseResp.responseDetails != null && bundlePurchaseResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_Purchase_Post_" + bundlePurchaseResp.responseDetails.ResponseCode);
                    bundlePurchaseResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundlePurchaseResp.responseDetails.ResponseDesc : errorInsertMsg;
                }

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - FlexiBundleCustomize - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - ActionResult FlexiBundleCustomize End");
                serviceCRM = null;
            }
            return Json(bundlePurchaseResp, JsonRequestBehavior.AllowGet);

        }


        public JsonResult BundleCost(CalculateBundleCostRequest calcBundleCostReq)
        {
            CalculateBundleCostResponse calcBundleCostResp = new CalculateBundleCostResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - ActionResult BundleCost Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                calcBundleCostReq.CountryCode = clientSetting.countryCode;
                calcBundleCostReq.BrandCode = clientSetting.brandCode;
                calcBundleCostReq.LanguageCode = clientSetting.langCode;
                calcBundleCostReq.MSISDN = Session["MobileNumber"].ToString();
                calcBundleCostResp = serviceCRM.CRMCalculateBundleCost(calcBundleCostReq);
                if (calcBundleCostResp != null && calcBundleCostResp.ResponseDetails != null && calcBundleCostResp.ResponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_BundleCost_" + calcBundleCostResp.ResponseDetails.ResponseCode);
                    calcBundleCostResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? calcBundleCostResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                if (calcBundleCostResp != null && calcBundleCostResp.CalculateBundleCost != null && calcBundleCostResp.CalculateBundleCost.RETURNCODE != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_BundleCost_" + calcBundleCostResp.CalculateBundleCost.RETURNCODE);
                    calcBundleCostResp.CalculateBundleCost.ERRDESCRITION = string.IsNullOrEmpty(errorInsertMsg) ? calcBundleCostResp.CalculateBundleCost.ERRDESCRITION : errorInsertMsg;
                }
                Session["DiscountTag"] = calcBundleCostResp.CalculateBundleCost.DISCOUNTTAG;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - ActionResult BundleCost End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - BundleCost - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(calcBundleCostResp.CalculateBundleCost, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PromoCode(string promoCode,string strPaymentMode,string strProductDetails)
        {
            PromoDiscountResponse promoDiscountResp = new PromoDiscountResponse();
            PromoDiscountRequest promoDiscountReq = new PromoDiscountRequest();
            string strxmlFrameProductDetails = string.Empty;
            string errorInsertMsg = string.Empty;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - JsonResult PromoCode Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                promoDiscountReq.CountryCode = clientSetting.countryCode;
                promoDiscountReq.BrandCode = clientSetting.brandCode;
                promoDiscountReq.LanguageCode = clientSetting.langCode;
                promoDiscountReq.MSISDN = Session["MobileNumber"].ToString();
                promoDiscountReq.PromoCode = promoCode;
                //4808
                promoDiscountReq.ICCID = Session["ICCID"].ToString();
                if (!string.IsNullOrEmpty(strProductDetails))
                {
                    //strProductDetails="101,BUNDLE,3"
                    strxmlFrameProductDetails = "<PRODUCT_INFO><PRODUCTS>";
                    foreach (string strTmpProductDetails in strProductDetails.Split('|'))
                    {
                        if (strTmpProductDetails != string.Empty)
                        {
                            strxmlFrameProductDetails += "<PRODUCT>";
                            strxmlFrameProductDetails += "<VALUE>" + strTmpProductDetails.Split(',').GetValue(0).ToString() + "</VALUE>";
                            strxmlFrameProductDetails += "<TYPE>" + strTmpProductDetails.Split(',').GetValue(1).ToString() + "</TYPE>";
                            strxmlFrameProductDetails += "<NO_OF_INSTALLS>" + strTmpProductDetails.Split(',').GetValue(2).ToString() + "</NO_OF_INSTALLS>";
                            strxmlFrameProductDetails += "</PRODUCT>";
                        }
                    }
                    strxmlFrameProductDetails += "</PRODUCTS></PRODUCT_INFO>";
                    //promoDiscountReq.ProductInformationInXML = "<PRODUCT_INFO><PRODUCTS><PRODUCT><VALUE>101</VALUE><TYPE>BUNDLE</TYPE><NO_OF_INSTALLS>3</NO_OF_INSTALLS></PRODUCT></PRODUCTS></PRODUCT_INFO>";
                    promoDiscountReq.ProductInformationInXML = strxmlFrameProductDetails;
                }
                if (!string.IsNullOrEmpty(strPaymentMode))
                {
                    promoDiscountReq.PaymentMode = strPaymentMode;
                }
                else
                {
                    promoDiscountReq.PaymentMode = "0";
                }
                promoDiscountResp = serviceCRM.CRMValidatePromoCode(promoDiscountReq);
                if (promoDiscountResp != null && promoDiscountResp.ResponseDetails != null && promoDiscountResp.ResponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_PromoCode_" + promoDiscountResp.ResponseDetails.ResponseCode);
                    promoDiscountResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? promoDiscountResp.ResponseDetails.ResponseDesc : errorInsertMsg;

                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - JsonResult PromoCode End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - PromoCode - " + this.ControllerContext, ex);
            }
            finally
            {
                promoDiscountReq = null;
                strxmlFrameProductDetails = string.Empty;
                errorInsertMsg = string.Empty;
                serviceCRM = null;
            }
            return Json(promoDiscountResp.PromoDiscount, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BoosterTax(string selectedBooster)
        {
            TaxDetailsResponse taxDetailsResp = new TaxDetailsResponse();
            TaxDetailsRequest taxDetailsReq = new TaxDetailsRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                List<string> lstBundleName = new List<string>(selectedBooster.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                

                    taxDetailsReq.CountryCode = clientSetting.countryCode;
                    taxDetailsReq.BrandCode = clientSetting.brandCode;
                    taxDetailsReq.LanguageCode = clientSetting.langCode;
                    taxDetailsReq.BoosterName = lstBundleName;

                    taxDetailsResp = serviceCRM.CRMPostTaxDetails(taxDetailsReq);

                    if (taxDetailsResp != null && taxDetailsResp.ResponseDetails != null && taxDetailsResp.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_BoosterTax_" + taxDetailsResp.ResponseDetails.ResponseCode);
                        taxDetailsResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? taxDetailsResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                
                return Json(taxDetailsResp.BundleTaxDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(taxDetailsResp.BundleTaxDetails, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                taxDetailsReq = null;
                serviceCRM = null;
                //taxDetailsResp = null;
            }
        }


        public ActionResult ChangePlan()
        {
            ChangePlan planchangerReq = new ChangePlan();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMResponse objRes = Utility.checkValidSubscriber("1", clientSetting);
                if (objRes.ResponseCode == "0")
                {
                    planchangerReq.ResponseCode = "0";
                }
                else
                {
                    planchangerReq.ResponseCode = "1";
                    planchangerReq.ResponseDesc = objRes.ResponseDesc;
                }
                if (Session["PAType"] != null && Convert.ToString(Session["PAType"]) == "CHANGE PLAN")
                {
                    PendingDetailsRequest req = new PendingDetailsRequest();
                    PendingDetailsResponce ObjRes = new PendingDetailsResponce();
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

                    
                    if (ObjRes.PendingDetails.Count > 0)
                    {
                        planchangerReq.CurrentPlan = ObjRes.PendingDetails[0].Currentplanname;
                        planchangerReq.NewPlan = ObjRes.PendingDetails[0].NewPlan;
                        planchangerReq.Reason = ObjRes.PendingDetails[0].Reason;
                        planchangerReq.Id = Convert.ToString(Session["PAId"]);
                    }
                }

                else
                {
                    Session["PAType"] = string.Empty;
                    Session["PAId"] = string.Empty;

                }
                return View(planchangerReq);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(planchangerReq);
            }
            finally
            {
                serviceCRM = null;
                //planchangerReq = null;
            }

        }


        public PlanChangerResponse CRMPlanChanger(PlanChangerRequest objplanchangerreq)
        {
            PlanChangerResponse ObjRes = new PlanChangerResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objplanchangerreq.CountryCode = clientSetting.countryCode;
                objplanchangerreq.BrandCode = clientSetting.brandCode;
                objplanchangerreq.LanguageCode = clientSetting.langCode;
                objplanchangerreq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objplanchangerreq.PARequestType = Resources.HomeResources.PAChangePlan;
                    ObjRes = serviceCRM.CRMPlanChanger(objplanchangerreq);

                    if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_PlanChanger_" + ObjRes.ResponseDetails.ResponseCode);
                        ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                        if (ObjRes.ResponseDetails.ResponseCode == "12" || ObjRes.ResponseDetails.ResponseCode == "15" || ObjRes.ResponseDetails.ResponseCode == "16" || ObjRes.ResponseDetails.ResponseCode == "20")
                        {
                            Session["PlanId"] = objplanchangerreq.NewPlan;
                        }
                    }
                
                return ObjRes;
                //Session["PlanName"] = (ObjRes.PlanMaster.Length > 0) ? ObjRes.PlanMaster[0].PlanName : Session["PlanName"];
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return ObjRes;
            }
            finally
            {
                serviceCRM = null;
                //ObjRes = null;
            }

        }


        public PostChangePlanResponse CRMPostChangePlan(PostChangePlanRequest objchangeplanreq)
        {

            PostChangePlanResponse ObjRes = new PostChangePlanResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objchangeplanreq.CountryCode = clientSetting.countryCode;
                objchangeplanreq.BrandCode = clientSetting.brandCode;
                objchangeplanreq.LanguageCode = clientSetting.langCode;
                objchangeplanreq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMPostChangePlan(objchangeplanreq);

                    if (ObjRes != null && ObjRes.reponseDetails != null && ObjRes.reponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_PostPlanChanger_" + ObjRes.reponseDetails.ResponseCode);
                        ObjRes.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                
                return ObjRes;
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return ObjRes;
            }
            finally
            {
                serviceCRM = null;
                // ObjRes = null;
            }

        }

        [HttpPost]
        public JsonResult GetRequiredPlan(PlanChangerRequest objplanchangerreq)
        {
            Required_Plan reqPlan = new Required_Plan();
            reqPlan.strRequiredPlan = string.Empty;
            Dictionary<string, string> lstSIMtypes = new Dictionary<string, string>();
            ServiceInvokeCRM serviceCRM;
            try
            {
                PlanChangerResponse ObjRes = new PlanChangerResponse();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = CRMPlanChanger(objplanchangerreq);

                    if (ObjRes.PlanMaster.Count > 0)
                    {

                        ObjRes.PlanMaster = ObjRes.PlanMaster.Where(m => m.PlanId != Convert.ToString(Session["PlanId"])).ToList();
                    }

                    reqPlan.strRequiredPlan = reqPlan.strRequiredPlan + "<option title='Select' value=''>Select</option>";
                    for (int i = 0; i < ObjRes.PlanMaster.Count; i++)
                    {
                        reqPlan.strRequiredPlan = reqPlan.strRequiredPlan + "<option title='" + ObjRes.PlanMaster[i].PlanName + "' value='" + ObjRes.PlanMaster[i].PlanId + "'>" + ObjRes.PlanMaster[i].PlanName + "</option>";
                    }

                    reqPlan.PlanName = Convert.ToString(Session["PlanName"]);
                
                return Json(reqPlan, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(reqPlan, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                serviceCRM = null;
                //reqPlan = null;
                lstSIMtypes = null;
            }

        }


        public ActionResult GetPostRequiredPlan(PostChangePlanRequest objchangeplanreq)
        {
            string strRequiredPlan = string.Empty;
            Dictionary<string, string> lstSIMtypes = new Dictionary<string, string>();
            PostChangePlanResponse ObjRes = new PostChangePlanResponse();
            try
            {
                ObjRes = CRMPostChangePlan(objchangeplanreq);
                if (ObjRes.PlanList != null)
                {
                    if (objchangeplanreq.Mode == "Q")
                    {
                        if (ObjRes.PlanList.Count > 0)
                        {

                            ObjRes.PlanList = ObjRes.PlanList.Where(m => m.PlanCode != Convert.ToString(Session["PlanId"])).ToList();
                        }
                    }
                    else if (objchangeplanreq.Mode == "R")
                    {
                        strRequiredPlan = ObjRes.PlanDetails.Deposit;
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
                lstSIMtypes = null;
                //ObjRes = null;
            }

        }

        [HttpPost]
        public JsonResult NewChangePlan(string plnchngreq, string newPlanName)
        {
            PlanChangerRequest objplanchangerreq = new PlanChangerRequest();
            objplanchangerreq = new JavaScriptSerializer().Deserialize<PlanChangerRequest>(plnchngreq);
            PlanChangerResponse ObjRes = new PlanChangerResponse();
            try
            {
                objplanchangerreq.User = Convert.ToString(Session["UserName"]);
                ObjRes = CRMPlanChanger(objplanchangerreq);

                if ((ObjRes.ResponseDetails.ResponseCode == "0" || ObjRes.ResponseDetails.ResponseCode == "11" || ObjRes.ResponseDetails.ResponseCode == "12" || ObjRes.ResponseDetails.ResponseCode == "13" || ObjRes.ResponseDetails.ResponseCode == "15" || ObjRes.ResponseDetails.ResponseCode == "16" || ObjRes.ResponseDetails.ResponseCode == "17" || ObjRes.ResponseDetails.ResponseCode == "18" || ObjRes.ResponseDetails.ResponseCode == "19" || ObjRes.ResponseDetails.ResponseCode == "20") && ((objplanchangerreq.Mode == "A" && objplanchangerreq.Type == "A") || (objplanchangerreq.Mode == "I" && objplanchangerreq.Type == "A")))
                {
                    Session["PlanName"] = newPlanName;
                    Session["PlanId"] = objplanchangerreq.NewPlan;
                }
                Session["PAType"] = null;
                return Json(ObjRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(ObjRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objplanchangerreq = null;
                //ObjRes = null;
            }
        }


        [HttpPost]
        public JsonResult NewChangePlanpost(PostChangePlanRequest objchangeplanreq)
        {
            PostChangePlanResponse ObjRes = new PostChangePlanResponse();
            try
            {
                objchangeplanreq.User = Convert.ToString(Session["UserName"]);
                ObjRes = CRMPostChangePlan(objchangeplanreq);
                Session["PAType"] = null;
                return Json(ObjRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(ObjRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //ObjRes = null;
            }
        }

        [HttpPost]
        public JsonResult ChangePlan(string jsonChangePlanForm)
        {
            PlanChangerResponse cpRes = new PlanChangerResponse();
            SelectListItem selList = new SelectListItem();

            ChangePlan cpReqModel = new ChangePlan();
            cpReqModel = new JavaScriptSerializer().Deserialize<ChangePlan>(jsonChangePlanForm);
            PlanChangerRequest cpReq = new PlanChangerRequest();
            ServiceInvokeCRM serviceCRM;

            try
            {
                cpReq.LanguageCode = clientSetting.langCode;
                cpReq.BrandCode = clientSetting.brandCode;
                cpReq.CountryCode = clientSetting.countryCode;
                cpReq.CurrentPlan = cpReqModel.CurrentPlan;
                cpReq.NewPlan = cpReqModel.NewPlan;
                cpReq.Reason = cpReqModel.Reason;
                cpReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                cpReq.User = Convert.ToString(Session["UserName"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    if (Convert.ToString(Session["isPrePaid"]) == "1")
                    {
                        cpRes = serviceCRM.CRMPlanChanger(cpReq);
                    }
                    else if (Convert.ToString(Session["isPostPaid"]) == "1")
                    {
                        cpRes = serviceCRM.CRMPlanChanger(cpReq);
                    }
                
                //cpRes = serviceCRM.CRMPlanChanger(cpReq);

                if (cpRes.PlanChanger.ErrNo != null)
                {
                    selList.Value = cpRes.PlanChanger.ErrNo;
                    selList.Text = cpRes.PlanChanger.ErrMsg;
                }
                else
                {
                    selList.Value = cpRes.ResponseDetails.ResponseCode;
                    selList.Text = cpRes.ResponseDetails.ResponseDesc;
                }
                return Json(selList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(selList, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                cpRes = null;
                //selList = null;
                serviceCRM = null;
                cpReqModel = null;
            }
        }

        #region ShareBundle
        public ActionResult ShareBundle()
        {
            BundleShareRequest sbReq = new BundleShareRequest();
            BundleShareResponse sbRes = new BundleShareResponse();
            sbReq.LanguageCode = clientSetting.langCode;
            sbReq.BrandCode = clientSetting.brandCode;
            sbReq.CountryCode = clientSetting.countryCode;
            sbReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
            sbReq.Mode = "P";
            sbReq.PreMSISDN = string.Empty;
            sbReq.SharedFriendMSISDN = string.Empty;
            sbReq.PlanEnabler = "true";
            sbReq.Owner = Convert.ToString(Session["UserName"]);
            sbReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
            sbReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    sbRes = serviceCRM.CRMBundleShare(sbReq);
                    if (sbRes != null && sbRes.ResponseDetails != null && sbRes.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_ShareBundle_" + sbRes.ResponseDetails.ResponseCode);
                        sbRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? sbRes.ResponseDetails.ResponseDesc : errorInsertMsg;

                    }
                
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }

            sbReq.listPreMSISDN = sbRes.PreMSISDNResp;

            //List<PreMSISDN> lst = new List<PreMSISDN>();
            //PreMSISDN pMsdn = new PreMSISDN();
            //pMsdn.ID = "abc";
            //pMsdn.Text = "445522277250";
            //lst.Add(pMsdn);
            //sbReq.listPreMSISDN = lst;
            return View(sbReq);
        }

        [HttpPost]
        public JsonResult ShareBundle(string jsonshareBundleForm)
        {
            SelectListItem selList = new SelectListItem();
            BundleShareResponse sbRes = new BundleShareResponse();
            BundleShareRequest sbReq = new BundleShareRequest();
            sbReq = new JavaScriptSerializer().Deserialize<BundleShareRequest>(jsonshareBundleForm);
            ServiceInvokeCRM serviceCRM;
            try
            {
                sbReq.LanguageCode = clientSetting.langCode;
                sbReq.BrandCode = clientSetting.brandCode;
                sbReq.CountryCode = clientSetting.countryCode;
                sbReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                sbReq.Mode = "A";
                sbReq.PreMSISDN = sbReq.PreMSISDN;
                sbReq.SharedFriendMSISDN = sbReq.SharedFriendMSISDN;
                sbReq.PlanEnabler = "true";
                sbReq.Owner = Convert.ToString(Session["UserName"]);
                sbReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                sbReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    sbRes = serviceCRM.CRMBundleShare(sbReq);

                    if (sbRes != null && sbRes.ResponseDetails != null && sbRes.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_ShareBundle_" + sbRes.ResponseDetails.ResponseCode);
                        sbRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? sbRes.ResponseDetails.ResponseDesc : errorInsertMsg;

                    }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (sbRes.ResponseDetails == null)
            {
                //selList.Value = sbRes.PlanChanger[0].ErrNo;
                //selList.Text = cpRes.PlanChanger[0].ErrMsg;
            }
            else
            {
                selList.Value = sbRes.ResponseDetails.ResponseCode;
                selList.Text = sbRes.ResponseDetails.ResponseDesc;
            }
            return Json(selList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetSharedFriendMSISDN()
        {
            BundleShareResponse sbRes = new BundleShareResponse();
            BundleShareRequest sbReq = new BundleShareRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                sbReq.LanguageCode = clientSetting.langCode;
                sbReq.BrandCode = clientSetting.brandCode;
                sbReq.CountryCode = clientSetting.countryCode;
                sbReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                sbReq.Mode = "S";
                sbReq.PreMSISDN = string.Empty;
                sbReq.SharedFriendMSISDN = string.Empty;
                sbReq.PlanEnabler = "true";
                sbReq.Owner = Convert.ToString(Session["UserName"]);
                sbReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                sbReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    sbRes = serviceCRM.CRMBundleShare(sbReq);

                    if (sbRes != null && sbRes.ResponseDetails != null && sbRes.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_ShareBundle_" + sbRes.ResponseDetails.ResponseCode);
                        sbRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? sbRes.ResponseDetails.ResponseDesc : errorInsertMsg;

                    }


            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return Json(sbRes.SharedFriendResp, JsonRequestBehavior.AllowGet);
        }

        #endregion


        public ViewResult Refund()
        {
            RefundBundleResponse refundBundleResp = new RefundBundleResponse();
            RefundBundleRequest refundBundleReq = new RefundBundleRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                refundBundleReq.CountryCode = clientSetting.countryCode;
                refundBundleReq.BrandCode = clientSetting.brandCode;
                refundBundleReq.LanguageCode = clientSetting.langCode;
                refundBundleReq.MSISDN = Session["MobileNumber"].ToString();
                refundBundleReq.Mode = "Q";
                refundBundleReq.Type = "Pre";

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    refundBundleResp = serviceCRM.CRMRefundBundle(refundBundleReq);

                    // RefundBundle obj = new RefundBundle();                    
                    //obj.BundleCode = "fTTT";
                    //obj.BPartyNumber = "29-04-2016";
                    //obj.ExpiryDate = "1";
                    //obj.Minutes = "1";
                    //obj.NoticePeriod = "Q";
                    //obj.BundleName = "5";                    
                    //obj.TotalSMS = "27-04-2016";                    
                    //obj.IsActive = "DeActive";
                    //refundBundleResp.RefundBundle.Add(obj);
                    //refundBundleResp.reponseDetails.ResponseCode = "0";

                    if (refundBundleResp != null && refundBundleResp.reponseDetails != null && refundBundleResp.reponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_Refund_" + refundBundleResp.reponseDetails.ResponseCode);
                        refundBundleResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? refundBundleResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                
                return View(refundBundleResp);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(refundBundleResp);
            }
            finally
            {
                refundBundleReq = null;
                serviceCRM = null;
                //refundBundleResp = null;
            }

        }

        public JsonResult RefundBundle(RefundBundleRequest refundBundleReq)
        {
            SelectListItem rbItem = new SelectListItem();
            CRMBase cbase = new CRMBase();
            ServiceInvokeCRM serviceCRM;
            try
            {
                cbase.CountryCode = clientSetting.countryCode;
                cbase.BrandCode = clientSetting.brandCode;
                cbase.LanguageCode = clientSetting.langCode;

                if (Utility.ValidateTicketID(Session["MobileNumber"].ToString(), refundBundleReq.TicketId, cbase))
                {
                    RefundBundleResponse refundBundleResp = new RefundBundleResponse();

                    refundBundleReq.CountryCode = clientSetting.countryCode;
                    refundBundleReq.BrandCode = clientSetting.brandCode;
                    refundBundleReq.LanguageCode = clientSetting.langCode;
                    refundBundleReq.MSISDN = Session["MobileNumber"].ToString();
                    refundBundleReq.User = Session["UserName"].ToString();

                    refundBundleReq.Mode = "I";
                    refundBundleReq.Type = "Pre";

                    if ((bool)Session["IsAdmin"])
                    {
                        refundBundleReq.AdminNormal = "A";
                    }
                    else
                    {
                        refundBundleReq.AdminNormal = "N";
                    }
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        refundBundleResp = serviceCRM.CRMRefundBundle(refundBundleReq);

                        if (refundBundleResp != null && refundBundleResp.reponseDetails != null && refundBundleResp.reponseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_Refund_" + refundBundleResp.reponseDetails.ResponseCode);
                            refundBundleResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? refundBundleResp.reponseDetails.ResponseDesc : errorInsertMsg;
                        }
                    
                    if (refundBundleResp != null && refundBundleResp.reponseDetails != null)
                    {
                        rbItem.Value = refundBundleResp.reponseDetails.ResponseCode;
                        rbItem.Text = refundBundleResp.reponseDetails.ResponseDesc;
                    }
                    else
                    {
                        rbItem.Value = "1";
                        rbItem.Text = "Error. Contact technical team.";
                    }
                }
                else
                {
                    rbItem.Value = "1";
                    rbItem.Text = Resources.ErrorResources.InvalidTicket;
                }
                return Json(rbItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception eX)
            {
                rbItem.Value = "1";
                rbItem.Text = eX.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(rbItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //rbItem = null;
                cbase = null;
                serviceCRM = null;
            }

        }

        public void DestroySession()
        {
            Session["PAType"] = string.Empty;
            Session["PAId"] = string.Empty;
        }

        private string Dateconvert(string DateValue, string Index = "1,0,2")
        {
            try
            {
                string Date = string.Empty;
                string Month = string.Empty;
                string Year = string.Empty;
                string time = string.Empty;

                string[] Indexsp = Index.Split(',');
                string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
                if (DateValue != null && DateValue != string.Empty)
                {
                    string[] SplitDOB = DateValue.Trim().Replace("  ", " ").Split('-', '/', ' ');
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
                    if (SplitDOB.Length > 3)
                    {
                        for (int i = 3; i < SplitDOB.Length; i++)
                        {
                            time += " " + SplitDOB[i];
                        }
                    }
                }
                else
                {
                    DateValue = string.Empty;
                }
                DateValue = (strInputDate.Replace("dd", Date).Replace("mm", Month).Replace("yyyy", Year) + time).Trim();
                if (DateValue == "//")
                {
                    DateValue = string.Empty;
                }
            }
            catch
            {

            }
            return DateValue;
        }

        #region Bucket Mgt

        public ViewResult ModifyBundle()
        {

            BundleDetail objBundleDetail = new BundleDetail();
            BundleDetailResponse objMgBucket = new BundleDetailResponse();
            BundleDetailRequest objRequest = new BundleDetailRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objRequest.CountryCode = clientSetting.countryCode;
                objRequest.BrandCode = clientSetting.brandCode;
                objRequest.LanguageCode = clientSetting.langCode;
                objRequest.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    if (Session["PAType"] != null && Convert.ToString(Session["PAType"]).ToUpper() == "BUNDLE BUCKET")
                    {
                        objRequest.SubmitID = Convert.ToString(Session["PAId"]);
                        objBundleDetail.PAid = Convert.ToString(Session["PAId"]);
                    }
                    objMgBucket = serviceCRM.CRMManageBucketDetails(objRequest);

                    if (objMgBucket != null && objMgBucket.responsedetails != null && objMgBucket.responsedetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_ModifyBundle_" + objMgBucket.responsedetails.ResponseCode);
                        objMgBucket.responsedetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objMgBucket.responsedetails.ResponseDesc : errorInsertMsg;

                    }

                    objBundleDetail = objMgBucket.BundleDetails;
                    objBundleDetail.PAid = Convert.ToString(Session["PAId"]);
                

                DataSet ds = Utility.BindXmlFile("~/App_Data/ConfigItems.xml");
                List<BundlesList> objstBundleList = new List<BundlesList>();
                BundlesList objBundlesList = new BundlesList();
                List<BundleLis> ListBundleList = new List<BundleLis>();
                BundleLis objBundleLis = new BundleLis();
                foreach (DataTable dt in ds.Tables)
                {
                    if (new string[] { "BucketData", "BucketVoiceSms", "BucketCommon" }.Contains(dt.TableName))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            objBundleLis = new BundleLis();
                            objBundleLis.TableName = dt.TableName;
                            objBundleLis.BundleID = row["id"].ToString();
                            if (objBundleLis.TableName == "BucketVoiceSms")
                                objBundleLis.BundleName = row["BucketVoiceSms_Text"].ToString();
                            else if (objBundleLis.TableName == "BucketData")
                                objBundleLis.BundleName = row["BucketData_Text"].ToString();
                            if (Session["PAType"] != null && Convert.ToString(Session["PAType"]).ToUpper() == "BUNDLE BUCKET")
                            {
                                objBundleLis.Disabled = "disabled";
                                if (objMgBucket.BundleDetails.Bucketlist != null && objMgBucket.BundleDetails.Bucketlist.Count > 0)
                                {
                                    if (objBundleLis.TableName == "BucketVoiceSms")
                                    {
                                        if (objMgBucket.BundleDetails.Bucketlist.FindAll(b => b.Category == objBundleLis.BundleID && b.Servicetype == clientSetting.mvnoSettings.serviceTypeSMS).Count > 0)
                                        {
                                            objBundleLis.SMSBundleAllowance = Convert.ToString(objMgBucket.BundleDetails.Bucketlist.FindAll(b => b.Category == objBundleLis.BundleID && b.Servicetype == clientSetting.mvnoSettings.serviceTypeSMS).Select(a => a.Allowance).First());
                                            objBundleLis.Checked = "checked";
                                        }
                                        if (objMgBucket.BundleDetails.Bucketlist.FindAll(b => b.Category == objBundleLis.BundleID && b.Servicetype == clientSetting.mvnoSettings.serviceTypeVoice).Count > 0)
                                        {
                                            objBundleLis.BundleAllowance = Convert.ToString(objMgBucket.BundleDetails.Bucketlist.FindAll(b => b.Category == objBundleLis.BundleID && b.Servicetype == clientSetting.mvnoSettings.serviceTypeVoice).Select(a => a.Allowance).First());
                                            objBundleLis.Checked = "checked";
                                        }
                                    }
                                    else if (objBundleLis.TableName == "BucketData")
                                    {
                                        if (objMgBucket.BundleDetails.Bucketlist.FindAll(b => b.Category == objBundleLis.BundleID && b.Servicetype == clientSetting.mvnoSettings.serviceTypeData).Count > 0)
                                        {
                                            objBundleLis.Checked = "checked";
                                            objBundleLis.BundleAllowance = Convert.ToString(objMgBucket.BundleDetails.Bucketlist.FindAll(b => b.Category == objBundleLis.BundleID && b.Servicetype == clientSetting.mvnoSettings.serviceTypeData).Select(a => a.Allowance).First());
                                        }
                                    }
                                    else if (objBundleLis.TableName == "BucketCommon")
                                    {
                                        if (objMgBucket.BundleDetails.Bucketlist.FindAll(b => b.Servicetype == clientSetting.mvnoSettings.serviceTypeCommon).Count > 0)
                                        {
                                            objBundleDetail.CommonAllance = Convert.ToString(objMgBucket.BundleDetails.Bucketlist.FindAll(b => b.Servicetype == clientSetting.mvnoSettings.serviceTypeCommon).Select(a => a.Allowance).First());
                                            objBundleDetail.isCommonAllowance = true;
                                        }
                                    }
                                }
                            }
                            ListBundleList.Add(objBundleLis);
                        }
                    }
                }
                if (objMgBucket.BundleDetails != null && objMgBucket.BundleDetails.Bucketlist != null && objMgBucket.BundleDetails.Bucketlist.Count > 0)
                {
                    objBundleDetail.Bundlecode = objMgBucket.BundleDetails.Bucketlist[0].Bundlecode;
                    objBundleDetail.Operationtype = objMgBucket.BundleDetails.Bucketlist[0].Operationtype;
                    objBundleDetail.HiddenValues = objBundleDetail.Bundlecode + "|" + objBundleDetail.Operationtype;
                }
                if (objBundleDetail.BundleList.Count > 0)
                {
                    objBundleDetail.BundleList[0].BundleList = ListBundleList;
                }
                else
                {
                    objBundlesList.BundleList = ListBundleList;
                    objstBundleList.Add(objBundlesList);
                    objBundleDetail.BundleList = objstBundleList;
                }
                //Expire date change
                if (objBundleDetail.BundleList.Count > 0)
                    objBundleDetail.BundleList.FindAll(a => a.Expirydate != string.Empty).ForEach(b => b.Expirydate = Utility.GetDateconvertion(b.Expirydate, "DD/MM/YYYY", false, clientSetting.mvnoSettings.dateTimeFormat.ToUpper()));
                return View(objBundleDetail);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(objBundleDetail);
            }
            finally
            {
                Session["PAType"] = Session["PAId"] = null;
                //objBundleDetail = null;
                objMgBucket = null;
                objRequest = null;
                serviceCRM = null;
            }

        }

        public JsonResult CRMInsertBucketDetails(string objrequest)
        {
            CRMResponse objRes = new CRMResponse();
            BundleBucketRequest objManBucket = JsonConvert.DeserializeObject<BundleBucketRequest>(objrequest);
            ServiceInvokeCRM serviceCRM;
            try
            {
                objManBucket.CountryCode = clientSetting.countryCode;
                objManBucket.BrandCode = clientSetting.brandCode;
                objManBucket.LanguageCode = clientSetting.langCode;
                objManBucket.MSISDN = Session["MobileNumber"].ToString();
                objManBucket.Username = Session["UserName"].ToString();
                objManBucket.PARequestType = Resources.HomeResources.PABundleBucket;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.CRMInsertBucketDetails(objManBucket);

                    if (objRes != null && objRes.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_InsertBucket_" + objRes.ResponseCode);
                        objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;
                    }
                
                //switch (objRes.ResponseCode)
                //{
                //    case "0":
                //        if (objManBucket.Mode == "UPDATE")
                //        {
                //            objRes.ResponseDesc = Resources.ErrorResources.EUpdat_AUT_0;
                //        }
                //        else
                //        {
                //            objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_0;
                //        }
                //        break;
                //    case "1":
                //        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_1;
                //        break;
                //    case "3":
                //        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_3;
                //        break;
                //    case "4":
                //        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_4;
                //        break;
                //    case "7":
                //        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_7;
                //        break;
                //    case "503 ":
                //        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_503;
                //        break;
                //    case "100":
                //        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_100;
                //        break;
                //    case "110":
                //        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_110;
                //        break;
                //    case "111":
                //        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_111;
                //        break;
                //    case "112":
                //        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_112;
                //        break;
                //    case "113":
                //        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_113;
                //        break;
                //    case "114":
                //        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_114;
                //        break;

                //    default:
                //        objRes.ResponseDesc = Resources.ErrorResources.Common_2;
                //        break;
                //}
                return Json(objRes);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objRes);
            }
            finally
            {
                //objRes = null;
                serviceCRM = null;
                objManBucket = null;
            }

        }

        public JsonResult CRMBundleBucketExpiryDate(BundleBucketExpiryDate objrequest)
        {
            CRMResponse objRes = new CRMResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objrequest.CountryCode = clientSetting.countryCode;
                objrequest.BrandCode = clientSetting.brandCode;
                objrequest.LanguageCode = clientSetting.langCode;
                objrequest.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.CRMBundleBucketExpiryDate(objrequest);

                    if (objRes != null && objRes.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_BundleBucketExpiryDate_" + objRes.ResponseCode);
                        objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;

                    }
                
                return Json(objRes);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objRes);
            }
            finally
            {
                // objRes = null;
                serviceCRM = null;
            }

        }

        #endregion

        #region MANAGE FAMILY PLAN
        //----------------------C.GOPIKUMAR (EmpID-2296) and Vignesh (EmpID-3538)-----------------------------------

        public ActionResult ManageFamilyPlan()
        {
            ViewFamilyMembersRes ObjResp = new ViewFamilyMembersRes();
            ViewFamilyMembersReq ObjReq = new ViewFamilyMembersReq();
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                //Session["MobileNumber"] = "447564500002";
                ObjReq.MSISDN = Session["MobileNumber"].ToString();

                ObjReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                ObjReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                //4808
                ObjReq.ICCID= Convert.ToString(Session["ICCID"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjResp = serviceCRM.ViewFamilyMembers(ObjReq);
                
                return View(ObjResp);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(ObjResp);
            }
            finally
            {
                serviceCRM = null;
                //ObjResp = null;
                ObjReq = null;
            }
            //return Json(ObjResp, JsonRequestBehavior.AllowGet);            
        }

        public ActionResult QueryLineAdditionCharge(string strMSISDN, string strFamilyAccountID)
        {
            QueryLineAdditionChargeRes ObjResp = new QueryLineAdditionChargeRes();
            QueryLineAdditionChargeReq ObjReq = new QueryLineAdditionChargeReq();
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.ChildMsisdn = strMSISDN;
                ObjReq.FamilyAccountID = strFamilyAccountID;
                ObjReq.ParentMsisdn = Session["MobileNumber"].ToString();
                ObjReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                ObjReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                //4808
                if (Convert.ToString(Session["IsRealMsisdn"]) == "1")
                    {
                    ObjReq.ICCID = Convert.ToString(Session["ICCID"]);
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjResp = serviceCRM.QueryLineAdditionCharge(ObjReq);
                
                if (ObjResp != null)
                {
                    if (ObjResp.Response != null)
                    {
                        string errorInsertMsg = Resources.BundleResources.ResourceManager.GetString("FamilyPlanQuery_" + ObjResp.Response.ResponseCode);
                        ObjResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.Response.ResponseDesc : errorInsertMsg;
                    }
                }
                return Json(ObjResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(ObjResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
                //ObjResp = null;
            }

        }

        public ActionResult UpdateManageFamilyPlan(string ManageFamilyPlan, string Flag)
        {
            ManageFamilyPlanRes ObjResp = new ManageFamilyPlanRes();
            ManageFamilyPlanReq ObjReq = JsonConvert.DeserializeObject<ManageFamilyPlanReq>(ManageFamilyPlan);
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.SessionId = Convert.ToString(Session["UserName"]);
                ObjReq.IPAddress = Convert.ToString(Session["ClientIPAddress"]);
                ObjReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                ObjReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                //4808
                //4808
                if (Convert.ToString(Session["IsRealMsisdn"]) == "1")
                {
                    ObjReq.ICCID = Convert.ToString(Session["ICCID"]);
                }

                if (ObjReq.CardDetails != null)
                {
                    if (ObjReq.CardDetails.ExpiryDate != string.Empty)
                    {
                        if (ObjReq.CardDetails.ExpiryDate.Length > 6)
                        {
                            ObjReq.CardDetails.ExpiryDate = ObjReq.CardDetails.ExpiryDate.Replace(" / ", string.Empty);
                        }
                    }
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjResp = serviceCRM.ManageFamilyPlan(ObjReq);
                
                if (ObjResp != null)
                {
                    if (ObjResp.Response != null)
                    {
                        if (ObjResp.Response.ResponseCode != "11")
                        {
                            if (ObjReq.Mode.ToUpper() == "DELETE")
                            {
                                string errorInsertMsg = Resources.BundleResources.ResourceManager.GetString("FamilyPlanDelete_" + ObjResp.Response.ResponseCode);
                                ObjResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.Response.ResponseDesc : errorInsertMsg;
                                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, ObjResp.Response.ResponseDesc);

                            }
                            else if (ObjReq.Mode.ToUpper() == "SWAP")
                            {
                                string errorInsertMsg = Resources.BundleResources.ResourceManager.GetString("FamilyPlanSwap_" + ObjResp.Response.ResponseCode);
                                ObjResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.Response.ResponseDesc : errorInsertMsg;
                                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, ObjResp.Response.ResponseDesc);
                            }
                            else if (ObjReq.Mode.ToUpper() == "ADD")
                            {
                                string errorInsertMsg = Resources.BundleResources.ResourceManager.GetString("FamilyPlanInsert_" + ObjResp.Response.ResponseCode);
                                ObjResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.Response.ResponseDesc : errorInsertMsg;
                                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, ObjResp.Response.ResponseDesc);
                            }
                        }
                    }
                }

                return Json(ObjResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(ObjResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
                //ObjResp = null;
            }

        }

        #endregion

        public ActionResult WiFiCallingAddress()
        {
            AddressDetails Objaddress = new AddressDetails();

            return View(Objaddress);
        }
        #region Plan Change History
        public PartialViewResult PlanChangeSuccessHistory()
        {
            PlanChangeHistoryResponse cpRes = new PlanChangeHistoryResponse();
            PlanChangeHistoryRequest cpReq = new PlanChangeHistoryRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                cpReq.LanguageCode = clientSetting.langCode;
                cpReq.BrandCode = clientSetting.brandCode;
                cpReq.CountryCode = clientSetting.countryCode;
                cpReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    cpRes = serviceCRM.CRMPlanChangeHistory(cpReq);
                    if (cpRes != null && cpRes.planChangeHistory.Count > 0)
                    {
                        cpRes.planChangeHistory.FindAll(a => a.planChangeDate != string.Empty).ForEach(b => b.planChangeDate = Utility.GetDateconvertion(b.planChangeDate, "DD/MM/YYYY", false, clientSetting.mvnoSettings.dateTimeFormat.ToUpper()));
                    }
                
                return PartialView("PlanChangeHistory", cpRes);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return PartialView("PlanChangeHistory", cpRes);
            }
            finally
            {
                //cpRes = null;
                cpReq = null;
                serviceCRM = null;
            }

        }


        #endregion

        [HttpPost]
        public JsonResult BundleInfo(string bundleCode)
        {
            BundleInfoResponse bundleInfoResp = new BundleInfoResponse();
            BundleInfoRequest bundleInfoReq = new BundleInfoRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                bundleInfoReq.CountryCode = clientSetting.countryCode;
                bundleInfoReq.BrandCode = clientSetting.brandCode;
                bundleInfoReq.LanguageCode = clientSetting.langCode;

                bundleInfoReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                bundleInfoReq.bundleCode = bundleCode;
                //bug
                #region FRR : 4376 : ATR_ID : V_1.1.8.0
                bundleInfoReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                bundleInfoReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                #endregion
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    bundleInfoResp = serviceCRM.BundleInfoCRM(bundleInfoReq);
                
                if (bundleInfoResp != null && bundleInfoResp.bundleProp != null && bundleInfoResp.bundleProp.Expirydate != null)
                {
                    bundleInfoResp.bundleProp.Expirydate = Utility.GetDateconvertion(bundleInfoResp.bundleProp.Expirydate, "DD-MM-YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);
                }
                if (bundleInfoResp != null && bundleInfoResp.responseDetails != null && bundleInfoResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleInfo_" + bundleInfoResp.responseDetails.ResponseCode);
                    bundleInfoResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundleInfoResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                return Json(bundleInfoResp.bundleProp);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(bundleInfoResp.bundleProp);
            }
            finally
            {
                //bundleInfoResp = null;
                serviceCRM = null;
                bundleInfoReq = null;
            }

        }

        [HttpPost]
        public JsonResult BundleInfoforMSISDN(string Details)
        {
            BundleInfoResponse bundleInfoResp = new BundleInfoResponse();
            BundleInfoRequest bundleInfoReq = new BundleInfoRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {

                bundleInfoReq.CountryCode = clientSetting.countryCode;
                bundleInfoReq.BrandCode = clientSetting.brandCode;
                bundleInfoReq.LanguageCode = clientSetting.langCode;

                if (!string.IsNullOrEmpty(Details))
                {
                    bundleInfoReq.bundleCode = Details.Split(',')[0];
                    bundleInfoReq.MSISDN = Details.Split(',')[1];
                }

               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    bundleInfoResp = serviceCRM.BundleInfoCRM(bundleInfoReq);
                
                if (bundleInfoResp != null && bundleInfoResp.bundleProp != null && bundleInfoResp.bundleProp.Expirydate != null)
                {
                    bundleInfoResp.bundleProp.Expirydate = Utility.GetDateconvertion(bundleInfoResp.bundleProp.Expirydate, "DD-MM-YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);
                }
                if (bundleInfoResp != null && bundleInfoResp.responseDetails != null && bundleInfoResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleInfo_" + bundleInfoResp.responseDetails.ResponseCode);
                    bundleInfoResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundleInfoResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                return Json(bundleInfoResp.bundleProp);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(bundleInfoResp.bundleProp);
            }
            finally
            {
                bundleInfoReq = null;
                serviceCRM = null;
                //bundleInfoResp = null;
            }

        }

        [HttpPost]
        public JsonResult CRMBundleProperties(string bundleCode)
        {
            ProductOfferResposne bundleInfoResp = new ProductOfferResposne();
            ProductOfferRequest bundleInfoReq = new ProductOfferRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                bundleInfoReq.CountryCode = clientSetting.countryCode;
                bundleInfoReq.BrandCode = clientSetting.brandCode;
                bundleInfoReq.LanguageCode = clientSetting.langCode;
                bundleInfoReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                bundleInfoReq.bundleCode = bundleCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    bundleInfoResp = serviceCRM.CRMBundleProperties(bundleInfoReq);
                
                //if (bundleInfoResp != null && bundleInfoResp.bundleProp != null && bundleInfoResp.bundleProp.Expirydate != null)
                //{
                //    bundleInfoResp.bundleProp.Expirydate = Utility.GetDateconvertion(bundleInfoResp.bundleProp.Expirydate, "DD-MM-YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);
                //}
                if (bundleInfoResp != null && bundleInfoResp.responseDetails != null && bundleInfoResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleProp_" + bundleInfoResp.responseDetails.ResponseCode);
                    bundleInfoResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundleInfoResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                return Json(bundleInfoResp);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(bundleInfoResp);
            }
            finally
            {
                //bundleInfoResp = null;
                serviceCRM = null;
                bundleInfoReq = null;
            }

        }

        #region BICS Airtime transfer
        public ActionResult ThirdPartyBalanceTransfer(BICSRequest bicsRequest)
        {
            BICSResponse bicsResponse = new BICSResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                bicsRequest.LanguageCode = clientSetting.langCode;
                bicsRequest.BrandCode = clientSetting.brandCode;
                bicsRequest.CountryCode = clientSetting.countryCode;
                bicsRequest.Mode = bicsRequest.Mode == null ? "GETPIN" : bicsRequest.Mode;
                bicsRequest.MSISDN = Convert.ToString(Session["MobileNumber"]);

                bicsRequest.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                bicsRequest.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    bicsResponse = serviceCRM.CRMBICSTransfer(bicsRequest);
                
                bicsResponse.strDropdown = Utility.GetDropdownMasterFromDB("", Convert.ToString(Session["isPrePaid"]), "TblCountry");
                bicsResponse.toMSISDN = (string.IsNullOrEmpty(bicsResponse.toMSISDN) ? string.Empty : bicsResponse.toMSISDN);
                bicsResponse.amount = (string.IsNullOrEmpty(bicsResponse.amount) ? string.Empty : bicsResponse.amount);
                bicsResponse.MSISDN = bicsRequest.MSISDN;
                bicsResponse.isPinAvailable = bicsResponse.ResponseDetails.ResponseCode;
                return View(bicsResponse);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(bicsResponse);
            }
            finally
            {
                //bicsResponse = null;
                serviceCRM = null;
            }
        }

        public JsonResult ValidateBICSTransfer(BICSTransRequest cpReq)
        {
            BICSTransResponse cpRes = new BICSTransResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                cpReq.LanguageCode = clientSetting.langCode;
                cpReq.BrandCode = clientSetting.brandCode;
                cpReq.CountryCode = clientSetting.countryCode;
                cpReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                cpReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                cpReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    cpRes = serviceCRM.CRMBICSTransProcess(cpReq);
                
                return Json(cpRes);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(cpRes);
            }
            finally
            {
                serviceCRM = null;
                // cpRes = null;
            }
        }
        public JsonResult LoadNonLycaDenomination(string countryCode)
        {
            RegisterThirdPartyBalanceTransfer rTBT = new RegisterThirdPartyBalanceTransfer();
            try
            {
                rTBT.bicsDropDown = Utility.GetDropdownMasterFromDB(countryCode, Convert.ToString(Session["isPrePaid"]), "tbl_bics_transfer_value");
                if (rTBT.bicsDropDown.Count > 0)
                {
                    rTBT.ResponseCode = "0";
                }
                else
                {
                    rTBT.ResponseCode = "1000";
                    rTBT.ResponseDesc = "Error in mdb";
                }
                return Json(rTBT);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                rTBT.ResponseCode = "1000";
                rTBT.ResponseDesc = "Error in mdb";
                return Json(rTBT);
            }
            finally
            {
                //rTBT = null;
            }

        }
        public JsonResult RegisterPin(BICSRequest cpReq)
        {
            BICSResponse cpRes = new BICSResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                cpReq.LanguageCode = clientSetting.langCode;
                cpReq.BrandCode = clientSetting.brandCode;
                cpReq.CountryCode = clientSetting.countryCode;
                cpReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                cpReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                cpReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    cpRes = serviceCRM.CRMBICSTransfer(cpReq);
                
                return Json(cpRes);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(cpRes);
            }
            finally
            {
                serviceCRM = null;
                // cpRes = null;
            }

        }
        public JsonResult SubmitBICSOrTransferTo(BICSTransRequest cpReq)
        {
            BICSTransResponse cpRes = new BICSTransResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                cpReq.LanguageCode = clientSetting.langCode;
                cpReq.BrandCode = clientSetting.brandCode;
                cpReq.CountryCode = clientSetting.countryCode;
                cpReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                cpReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                cpReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    cpRes = serviceCRM.CRMBICSTransProcess(cpReq);
                
                return Json(cpRes);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(cpRes);
            }
            finally
            {
                //cpRes = null;
                serviceCRM = null;
            }

        }
        #endregion

        public ViewResult CancelBundleAutoRenewal(string Textdata)
        {
            CancelBundleAutoRenewalResponse objres = new CancelBundleAutoRenewalResponse();
            PendingDetailsRequest req = new PendingDetailsRequest();
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
            CancelBundleAutoRenewalRequest objreq = new CancelBundleAutoRenewalRequest();
            string PAType = string.Empty;
            string MSISDNvalidate = string.Empty;
            ServiceInvokeCRM serviceCRM;
            try
            {
                #region FRR 4925
                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                    Session["RealICCIDForMultiTab"] = Textdata;
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    PAType = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.PAType).First().ToString();
                    req.MSISDN = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    req.Type = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.PAType).First().ToString();
                    req.Id = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.PAID).First().ToString();
                    MSISDNvalidate = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                }
                else
                {
                    PAType = Session["PAType"] != null ? Session["PAType"].ToString() : string.Empty;
                    MSISDNvalidate = Convert.ToString(Session["MobileNumber"]);
                    req.Type = Convert.ToString(Session["PAType"]);
                    req.Id = Convert.ToString(Session["PAId"]);
                }
                #endregion


                if (PAType != null && PAType == "CANCEL AUTO RENEWAL")
                {
                    //Pending Approval Section


                    req.CountryCode = clientSetting.countryCode;
                    req.BrandCode = clientSetting.brandCode;
                    req.LanguageCode = clientSetting.langCode;
                    req.MSISDN = MSISDNvalidate;
                   

                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        ObjRes = serviceCRM.CRMGetPendingDetails(req);
                        if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManaageOBABundle_" + ObjRes.ResponseDetails.ResponseCode);
                            ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                        }
                    
                    if (ObjRes.PendingDetails.Count > 0)
                    {
                        try
                        {
                            objres.pendingDetailsValue = new PendingDetailsValue();
                            objres.pendingDetailsValue.id = ObjRes.PendingDetails[0].Id;
                            objres.pendingDetailsValue.bundleCode = ObjRes.PendingDetails[0].BundleCode;
                            objres.pendingDetailsValue.reason = ObjRes.PendingDetails[0].Reason;
                            objres.pendingDetailsValue.cancelDate = ObjRes.PendingDetails[0].NewMSISDN;
                            objres.pendingDetailsValue.prereceiverMsisdn = ObjRes.PendingDetails[0].OldMSISDN;
                            objres.pendingDetailsValue.submitDate = ObjRes.PendingDetails[0].OldICCID;
                            objres.pendingDetailsValue.submittedBy = ObjRes.PendingDetails[0].RequestBy;
                            objres.pendingDetailsValue.bundleName = ObjRes.bundleName;

                            objres.pendingDetailsValue.cancelDate = Utility.GetDateconvertion(objres.pendingDetailsValue.cancelDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                            objres.pendingDetailsValue.submitDate = Utility.GetDateconvertion(objres.pendingDetailsValue.submitDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);

                            objres.pendingDetailsValue.paType = "true";
                            Session["PAType"] = null;

                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    objreq.mode = "Q";
                    objreq.BrandCode = clientSetting.brandCode;
                    objreq.CountryCode = clientSetting.countryCode;
                    objreq.LanguageCode = clientSetting.langCode;
                    objreq.MSISDN = MSISDNvalidate;

                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        objres = serviceCRM.CancelBundleAutoRenewal(objreq);

                        if (objres != null && objres.responseDetails != null && objres.responseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("AutoRenewalCancel_" + objres.responseDetails.ResponseCode);
                            objres.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.responseDetails.ResponseDesc : errorInsertMsg;
                        }

                        if (objres != null && objres.normalAutoBundle.Count > 0)
                        {
                            try
                            {
                                if (objres.normalAutoBundle.Count > 0)
                                {
                                    objres.normalAutoBundle.ForEach(b => b.ExpiryDate = Utility.GetDateconvertion(b.ExpiryDate, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat));
                                }

                                if (!string.IsNullOrEmpty(objres.expiryDate))
                                {
                                    objres.expiryDate = Utility.GetDateconvertion(objres.expiryDate, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                                }


                            //6581

                            if (objres.normalAutoBundle != null && objres.normalAutoBundle.Count > 0)
                            {
                                objres.normalAutoBundle.FindAll(a => !string.IsNullOrEmpty(a.RENEWAL_CANCELLATION_EFFECTIVE_DATE)).ForEach(b => b.RENEWAL_CANCELLATION_EFFECTIVE_DATE = Utility.FormatDateTime(b.RENEWAL_CANCELLATION_EFFECTIVE_DATE, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.ddMMyyyy));

                            }
                            }
                            catch
                            {

                            }
                        }

                        if (objres != null && objres.bundleDetails.Count > 0)
                        {
                            try
                            {
                                for (int i = 0; i < objres.bundleDetails.Count; i++)
                                {
                                    string dummystring = System.Text.RegularExpressions.Regex.Replace(objres.bundleDetails[i].value, "[^0-9a-zA-Z]+", string.Empty);
                                    string ResourceMsg = Resources.DropdownResources.ResourceManager.GetString(dummystring);
                                    objres.bundleDetails[i].value = string.IsNullOrEmpty(ResourceMsg) ? objres.bundleDetails[i].value : ResourceMsg;
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                
                return View(objres);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View(objres);
            }
            finally
            {
                //objres = null;
                req = null;
                //ObjRes = null;
                serviceCRM = null;
                objreq = null;
            }


        }

        [HttpPost]
        public ActionResult SubmitBundleAutoRenewal(string BundleInput)
        {
            CancelBundleAutoRenewalRequest objreq = JsonConvert.DeserializeObject<CancelBundleAutoRenewalRequest>(BundleInput);
            CancelBundleAutoRenewalResponse objres = new CancelBundleAutoRenewalResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {

                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.userName = Session["UserName"].ToString();
                objreq.PARequestType = Resources.HomeResources.CancelAutoRenewal;

                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {

                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    objreq.MSISDN = localDict.Where(x => objreq.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                   
                }
                else
                {
                    objreq.MSISDN = Session["MobileNumber"].ToString();
                }


                if (objreq.mode == "Activation Date")
                {
                    objreq.expiryDate = Utility.GetDateconvertion(objreq.expiryDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
                }

                if (objreq.mode == "I" || objreq.mode == "Admin")
                {
                    objreq.cancelDate = Utility.GetDateconvertion(objreq.cancelDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
                    objreq.expiryDate = Utility.GetDateconvertion(objreq.expiryDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
                }
              
               

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objres = serviceCRM.CancelBundleAutoRenewal(objreq);

                    if (objres != null && objres.responseDetails != null && objres.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("AutoRenewalCancel_" + objres.responseDetails.ResponseCode);
                        objres.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.responseDetails.ResponseDesc : errorInsertMsg;
                    }

                    if (objres != null)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(objres.expiryDate))
                            {
                                objres.expiryDate = Utility.GetDateconvertion(objres.expiryDate, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                            }

                            if (!string.IsNullOrEmpty(objres.deActivationMonth1))
                            {
                                objres.deActivationMonth1 = Utility.GetDateconvertion(objres.deActivationMonth1, "yyyy-MM-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                            }

                            if (!string.IsNullOrEmpty(objres.deActivationMonth2))
                            {
                                objres.deActivationMonth2 = Utility.GetDateconvertion(objres.deActivationMonth2, "yyyy-MM-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                            }

                            if (!string.IsNullOrEmpty(objres.deActivationMonth3))
                            {
                                objres.deActivationMonth3 = Utility.GetDateconvertion(objres.deActivationMonth3, "yyyy-MM-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                            }
                        }
                        catch
                        {

                        }
                    }
                
                return Json(objres);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(objres);
            }
            finally
            {
                objreq = null;
                serviceCRM = null;
                //objres = null;
            }


        }

        #region Bundle Transfer - 3314

        public ActionResult BundleTransfer()
        {
            BundleTransferReq objBundleTransferReq = new BundleTransferReq();
            BundleTransferResp objBundleTransferResp = new BundleTransferResp();
            CRMResponse objCRMResponse = new CRMResponse();
            objBundleTransferResp.ResponseDetails = new CRMResponse();
            Session["PendingID"] = null;
            ServiceInvokeCRM serviceCRM;
            try
            {
             serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    #region Pending Approve

                    if (Session["PAType"] != null && Convert.ToString(Session["PAType"]).ToUpper() == "BUNDLE TRANSFER")
                    {
                        Session["PendingID"] = Convert.ToString(Session["PAId"]);
                        objBundleTransferResp.ResponseDetails.ResponseCode = "0";
                    }
                    else
                    {
                        Session["PendingID"] = "";
                        #region enableRetailerIDBundleTransfer
                        if (clientSetting.mvnoSettings.enableRetailerIDBundleTransfer.ToUpper() == "ON")
                        {
                            #region MSISDN Validation
                            if (clientSetting.mvnoSettings.bundleTransferMSISDNRegister.ToUpper() == "ON")
                            {
                                if (Session["RetailerID"].ToString() == "")
                                    objCRMResponse = Utility.checkValidSubscriber("", clientSetting);  // From MSISDN
                                else
                                {
                                    objCRMResponse.ResponseCode = "0";
                                    objCRMResponse.ResponseDesc = "success";
                                }
                            }
                            else
                            {
                                if (Session["RetailerID"].ToString() == "")
                                    objCRMResponse = Utility.checkValidSubscriber("1", clientSetting);  // From MSISDN
                                else
                                {
                                    objCRMResponse.ResponseCode = "0";
                                    objCRMResponse.ResponseDesc = "success";
                                }
                            }
                            #endregion

                            if (objCRMResponse != null && !String.IsNullOrEmpty(objCRMResponse.ResponseCode) && objCRMResponse.ResponseCode == "0")
                            {
                                if (Session["RetailerID"].ToString() == "")
                                {
                                    objBundleTransferReq.CountryCode = clientSetting.countryCode;
                                    objBundleTransferReq.BrandCode = clientSetting.brandCode;
                                    objBundleTransferReq.LanguageCode = clientSetting.langCode;
                                    objBundleTransferReq.FromMSISDN = Session["MobileNumber"].ToString();
                                    objBundleTransferReq.Mode = "GET";
                                    objBundleTransferReq.PlanID = Convert.ToString(Session["PlanId"]);
                                    if (Session["RetailerID"].ToString() != "")
                                    {
                                        objBundleTransferReq.RetailerID = Session["RetailerID"].ToString();
                                        objBundleTransferReq.LogingMode = "2";
                                    }
                                    else
                                    {
                                        objBundleTransferReq.LogingMode = "1";
                                    }
                                    objBundleTransferResp = serviceCRM.CRMBundleTransfer(objBundleTransferReq);

                                    #region Date Convertion
                                    if (objBundleTransferResp != null && objBundleTransferResp.ResponseDetails != null && objBundleTransferResp.ResponseDetails.ResponseCode == "0")
                                    {
                                        //if (objBundleTransferResp.BundleTransferBundleListResp != null)
                                        //{
                                        //    for (int i = 0; i < objBundleTransferResp.BundleTransferBundleListResp.Count(); i++)
                                        //    {
                                        //        if (!string.IsNullOrEmpty(objBundleTransferResp.BundleTransferBundleListResp[i].TransactionDate))
                                        //        {
                                        //            string strTransactionDate = Utility.GetRRBSDateconvertion(objBundleTransferResp.BundleTransferBundleListResp[i].TransactionDate, "dd/mm/yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                                        //            objBundleTransferResp.BundleTransferBundleListResp[i].TransactionDate = strTransactionDate;
                                        //        }
                                        //        if (!string.IsNullOrEmpty(objBundleTransferResp.BundleTransferBundleListResp[i].ExpiryDate))
                                        //        {
                                        //            string strExpiryDate = Utility.GetRRBSDateconvertion(objBundleTransferResp.BundleTransferBundleListResp[i].ExpiryDate, "dd/mm/yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                                        //            objBundleTransferResp.BundleTransferBundleListResp[i].ExpiryDate = strExpiryDate;
                                        //        }
                                        //    }
                                        //}
                                        if (objBundleTransferResp.BundleTransferBundleListResp != null && objBundleTransferResp.BundleTransferBundleListResp.Count > 0)
                                        {
                                            objBundleTransferResp.BundleTransferBundleListResp.FindAll(a => !string.IsNullOrEmpty(a.TransactionDate)).ForEach(b => b.TransactionDate = Utility.FormatDateTime(b.TransactionDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.ddMMyyyy));
                                            objBundleTransferResp.BundleTransferBundleListResp.FindAll(a => !string.IsNullOrEmpty(a.ExpiryDate)).ForEach(b => b.ExpiryDate = Utility.FormatDateTime(b.ExpiryDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.ddMMyyyy));
                                        }

                                    }
                                    #endregion
                                }
                                else
                                {
                                    objBundleTransferResp.ResponseDetails.ResponseCode = "0";
                                }
                            }
                            else
                            {
                                objBundleTransferResp.ResponseDetails.ResponseCode = objCRMResponse.ResponseCode;
                                objBundleTransferResp.ResponseDetails.ResponseDesc = objCRMResponse.ResponseDesc;
                            }
                        }
                        else
                        {
                            objBundleTransferResp.ResponseDetails.ResponseCode = "1";
                            objBundleTransferResp.ResponseDetails.ResponseDesc = "Please enable the enableRetailerIDBundleTransfer config.";
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Please enable the enableRetailerIDBundleTransfer config.");
                        }
                        #endregion
                    }

                    #endregion

                    objBundleTransferResp.lstReason = Utility.GetDropdownMasterFromDB("MstBundleTransfer");

                    List<DropdownMaster> objDropdown = new List<DropdownMaster>();
                    objDropdown = objBundleTransferResp.lstReason;
                    objBundleTransferResp.lstReason = null;
                    objBundleTransferResp.lstReason = new List<DropdownMaster>();
                    for (int i = 0; i < objDropdown.Count(); i++)
                    {
                        DropdownMaster objDropdownMaster = new DropdownMaster();
                        if (string.IsNullOrEmpty(Session["RetailerID"].ToString()))
                        {
                            if (objDropdown[i].ID == "0")
                            {
                                objDropdownMaster.Master_id = objDropdown[i].Master_id;
                                objDropdownMaster.Value = objDropdown[i].Value;
                                objBundleTransferResp.lstReason.Add(objDropdownMaster);
                            }
                        }
                        else
                        {
                            if (objDropdown[i].ID == "1")
                            {
                                objDropdownMaster.Master_id = objDropdown[i].Master_id;
                                objDropdownMaster.Value = objDropdown[i].Value;
                                objBundleTransferResp.lstReason.Add(objDropdownMaster);
                            }
                        }
                    }


               
                Session["PAType"] = null;
                return View(objBundleTransferResp);
            }
            catch (Exception ex)
            {
                objBundleTransferResp.ResponseDetails.ResponseCode = "1";
                objBundleTransferResp.ResponseDetails.ResponseDesc = ex.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(objBundleTransferResp);
            }
            finally
            {
                objBundleTransferReq = null;
                //objBundleTransferResp = null;
                objCRMResponse = null;
                serviceCRM = null;

            }

        }

        public ActionResult BundleTransferMSISDNValidation(string MSISDN)
        {
            BundleMSISDNDetailsRequest objBundleTransferReq = JsonConvert.DeserializeObject<BundleMSISDNDetailsRequest>(MSISDN);
            BundleMSISDNDetailsResponse objBundleMSISDNDetailsResponse = new BundleMSISDNDetailsResponse();
            BundleTransferReq objBundleTransferRequest = new BundleTransferReq();
            BundleTransferResp objBundleTransferResponse = new BundleTransferResp();
            CRMResponse objCRMResponse = new CRMResponse();
            objBundleMSISDNDetailsResponse.ResponseDetails = new CRMResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    #region MSISDN Validation
                    if (clientSetting.mvnoSettings.bundleTransferMSISDNRegister.ToUpper() == "ON")
                    {
                        objCRMResponse = Utility.checkValidMSISDN(objBundleTransferReq, clientSetting);  // From MSISDN
                    }
                    else
                    {
                        objBundleTransferReq.MSISDNCheck = "1";
                        objCRMResponse = Utility.checkValidMSISDN(objBundleTransferReq, clientSetting);  // From MSISDN
                    }
                    #endregion

                    if (objCRMResponse != null && !String.IsNullOrEmpty(objCRMResponse.ResponseCode) && objCRMResponse.ResponseCode == "0")
                    {
                        #region Bundle Values

                        objBundleTransferRequest.CountryCode = clientSetting.countryCode;
                        objBundleTransferRequest.BrandCode = clientSetting.brandCode;
                        objBundleTransferRequest.LanguageCode = clientSetting.langCode;
                        objBundleTransferRequest.FromMSISDN = objBundleTransferReq.MSISDN;
                        objBundleTransferRequest.Mode = "GET";
                        objBundleTransferRequest.PlanID = Convert.ToString(Session["PlanId"]);
                        #region FRR : 4376 : ATR_ID : V_1.1.8.0
                        objBundleTransferRequest.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                        objBundleTransferRequest.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                        #endregion

                        if (Session["RetailerID"].ToString() != "")
                        {
                            objBundleTransferRequest.RetailerID = Session["RetailerID"].ToString();
                            objBundleTransferRequest.LogingMode = "2";
                        }
                        else
                        {
                            objBundleTransferRequest.LogingMode = "1";
                        }
                        objBundleTransferResponse = serviceCRM.CRMBundleTransfer(objBundleTransferRequest);

                        #region Date Convertion
                        if (objBundleTransferResponse != null && objBundleTransferResponse.ResponseDetails != null && objBundleTransferResponse.ResponseDetails.ResponseCode == "0")
                        {
                            if (objBundleTransferResponse.BundleTransferBundleListResp != null)
                            {
                                for (int i = 0; i < objBundleTransferResponse.BundleTransferBundleListResp.Count(); i++)
                                {
                                    if (!string.IsNullOrEmpty(objBundleTransferResponse.BundleTransferBundleListResp[i].TransactionDate))
                                    {
                                        string strTransactionDate = Utility.GetRRBSDateconvertion(objBundleTransferResponse.BundleTransferBundleListResp[i].TransactionDate, "dd/mm/yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                                        objBundleTransferResponse.BundleTransferBundleListResp[i].TransactionDate = strTransactionDate;
                                    }
                                    if (!string.IsNullOrEmpty(objBundleTransferResponse.BundleTransferBundleListResp[i].ExpiryDate))
                                    {
                                        string strExpiryDate = Utility.GetRRBSDateconvertion(objBundleTransferResponse.BundleTransferBundleListResp[i].ExpiryDate, "dd/mm/yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                                        objBundleTransferResponse.BundleTransferBundleListResp[i].ExpiryDate = strExpiryDate;
                                    }
                                }
                            }

                        }
                        #endregion

                        if (objBundleTransferResponse != null && objBundleTransferResponse.BundleTransferBundleListResp != null && objBundleTransferResponse.BundleTransferBundleListResp.Count() > 0)
                        {
                            objBundleMSISDNDetailsResponse.BundleTransferBundleListResp = objBundleTransferResponse.BundleTransferBundleListResp;
                            objBundleMSISDNDetailsResponse.ResponseDetails.ResponseCode = "0";
                        }
                        else
                        {
                            objBundleMSISDNDetailsResponse.ResponseDetails.ResponseCode = "1";
                            objBundleMSISDNDetailsResponse.ResponseDetails.ResponseDesc = @Resources.BillingResources.NoRecord;
                        }
                        #endregion

                        //if (Session["RetailerID"].ToString() != "")
                        //{
                        //    objBundleMSISDNDetailsResponse.ResponseDetails.ResponseCode = "0";
                        //}
                    }
                    else
                    {
                        objBundleMSISDNDetailsResponse.ResponseDetails = new CRMResponse();
                        objBundleMSISDNDetailsResponse.ResponseDetails.ResponseCode = objCRMResponse.ResponseCode;
                        objBundleMSISDNDetailsResponse.ResponseDetails.ResponseDesc = objCRMResponse.ResponseDesc;
                    }
                
                return Json(objBundleMSISDNDetailsResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                objBundleMSISDNDetailsResponse.ResponseDetails = new CRMResponse();
                objBundleMSISDNDetailsResponse.ResponseDetails.ResponseCode = "1";
                objBundleMSISDNDetailsResponse.ResponseDetails.ResponseDesc = ex.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objBundleMSISDNDetailsResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objBundleTransferReq = null;
                //objBundleMSISDNDetailsResponse = null;
                objBundleTransferRequest = null;
                objBundleTransferResponse = null;
                objCRMResponse = null;
                serviceCRM = null;
            }

        }


        public ActionResult BundleTransferDetails(string reqObjValues)
        {
            BundleTransferReq objBundleTransferRequest = JsonConvert.DeserializeObject<BundleTransferReq>(reqObjValues);
            BundleTransferResp objBundleTransferResponse = new BundleTransferResp();
            ServiceInvokeCRM serviceCRM;
            try
            {
              serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    #region Bundle Values

                    objBundleTransferRequest.CountryCode = clientSetting.countryCode;
                    objBundleTransferRequest.BrandCode = clientSetting.brandCode;
                    objBundleTransferRequest.LanguageCode = clientSetting.langCode;
                    objBundleTransferRequest.PARequestType = Resources.HomeResources.BundleTransfer;
                    #region FRR : 4376 : ATR_ID : V_1.1.8.0
                    objBundleTransferRequest.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                    objBundleTransferRequest.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                    #endregion

                    if (objBundleTransferRequest.Mode == "INSERT")
                    {
                        if (!string.IsNullOrEmpty(objBundleTransferRequest.TransactionDate))
                        {
                            string strTransactionDate = Utility.GetRRBSDateconvertion(objBundleTransferRequest.TransactionDate, "YYYY/MM/DD", true, clientSetting.mvnoSettings.dateTimeFormat);
                            objBundleTransferRequest.TransactionDate = strTransactionDate;
                        }
                        if (!string.IsNullOrEmpty(objBundleTransferRequest.BundleExpiry))
                        {
                            string strBundleExpiryDate = Utility.GetRRBSDateconvertion(objBundleTransferRequest.BundleExpiry, "YYYY/MM/DD", true, clientSetting.mvnoSettings.dateTimeFormat);
                            objBundleTransferRequest.BundleExpiry = strBundleExpiryDate;
                        }
                    }
                    objBundleTransferResponse = serviceCRM.CRMBundleTransfer(objBundleTransferRequest);
                    if (objBundleTransferRequest.Mode == "GETBYID")
                    {
                        if (objBundleTransferResponse != null && objBundleTransferResponse.BundleTransferDetailIDResp != null)
                        {
                            if (!string.IsNullOrEmpty(objBundleTransferResponse.BundleTransferDetailIDResp.TransactionDate))
                            {
                                string strTransactionDate = Utility.GetRRBSDateconvertion(objBundleTransferResponse.BundleTransferDetailIDResp.TransactionDate, "yyyy/mm/dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                                objBundleTransferResponse.BundleTransferDetailIDResp.TransactionDate = strTransactionDate;
                            }
                            if (!string.IsNullOrEmpty(objBundleTransferResponse.BundleTransferDetailIDResp.BundleExpiry))
                            {
                                string strBundleExpiryDate = Utility.GetRRBSDateconvertion(objBundleTransferResponse.BundleTransferDetailIDResp.BundleExpiry, "yyyy/mm/dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                                objBundleTransferResponse.BundleTransferDetailIDResp.BundleExpiry = strBundleExpiryDate;
                            }
                        }
                    }
                    if (objBundleTransferResponse != null && objBundleTransferResponse.ResponseDetails != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleTranser_" + objBundleTransferResponse.ResponseDetails.ResponseCode);
                        objBundleTransferResponse.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objBundleTransferResponse.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    else
                    {
                    }

                    #endregion
                
                return Json(objBundleTransferResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                objBundleTransferResponse.ResponseDetails = new CRMResponse();
                objBundleTransferResponse.ResponseDetails.ResponseCode = "1";
                objBundleTransferResponse.ResponseDetails.ResponseDesc = ex.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objBundleTransferResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objBundleTransferRequest = null;
                serviceCRM = null;
                //objBundleTransferResponse = null;
            }

        }

        [HttpPost]
        public JsonResult BundleInfoDetails(string reqObjValues)
        {
            BundleInfoResponse bundleInfoResp = new BundleInfoResponse();
            BundleInfoRequest bundleInfoReq = JsonConvert.DeserializeObject<BundleInfoRequest>(reqObjValues);
            ServiceInvokeCRM serviceCRM;
            try
            {
                bundleInfoReq.CountryCode = clientSetting.countryCode;
                bundleInfoReq.BrandCode = clientSetting.brandCode;
                bundleInfoReq.LanguageCode = clientSetting.langCode;
                #region FRR : 4376 : ATR_ID : V_1.1.8.0
                bundleInfoReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                bundleInfoReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                #endregion

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    bundleInfoResp = serviceCRM.BundleInfoCRM(bundleInfoReq);
                

                if (bundleInfoResp != null && bundleInfoResp.bundleProp != null && bundleInfoResp.bundleProp.Expirydate != null)
                {
                    bundleInfoResp.bundleProp.Expirydate = Utility.GetDateconvertion(bundleInfoResp.bundleProp.Expirydate, "DD-MM-YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);
                }
                if (bundleInfoResp != null && bundleInfoResp.responseDetails != null && bundleInfoResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleInfo_" + bundleInfoResp.responseDetails.ResponseCode);
                    bundleInfoResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundleInfoResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                return Json(bundleInfoResp.bundleProp);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(bundleInfoResp.bundleProp);
            }
            finally
            {
                //bundleInfoResp = null;
                bundleInfoReq = null;
                serviceCRM = null;
            }

        }



        #endregion

        #region FRR 3442 and FRR 3825
        public ActionResult PayGFamilyPlan()
        {

            PayGFamilyBundle ObjResp = new PayGFamilyBundle();
            PayGFamilyBundleRes objPayGFamilyBundleRes = new PayGFamilyBundleRes();
            PayGFamilyBundleReq objPayGFamilyBundleReq = new PayGFamilyBundleReq();
            CRMResponse objRes = new CRMResponse();
            //POF-6218
            Dropdown ObjDrop = new Dropdown();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objPayGFamilyBundleReq.CountryCode = clientSetting.countryCode;
                objPayGFamilyBundleReq.BrandCode = clientSetting.brandCode;
                objPayGFamilyBundleReq.LanguageCode = clientSetting.langCode;
                objPayGFamilyBundleReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                objPayGFamilyBundleReq.Mode = "1";
                //4808
                if (Convert.ToString(Session["IsRealMsisdn"]) == "1")
                    {
                    objPayGFamilyBundleReq.ICCID = Convert.ToString(Session["ICCID"]);
                }
                objPayGFamilyBundleReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                objPayGFamilyBundleReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                objPayGFamilyBundleReq.SIM_CATEGORY = Session["SIM_CATEGORY"].ToString();

                //POF-6218
                DataSet ds = Utility.BindXmlFile("~/App_Data/CountryListSepaCheckout.xml");
                ObjResp.objcountrydd = new Models.CountryDropdown();
                List<Dropdown> objLstDrop = new List<Dropdown>();
                foreach (DataTable dt in ds.Tables)
                {
                    switch (dt.TableName)
                    {
                        case "country":
                            foreach (DataRow row in dt.Rows)
                            {
                                ObjDrop = new Dropdown();
                                ObjDrop.ID = row["ID"].ToString();
                                ObjDrop.Value = row["country_Text"].ToString();
                                objLstDrop.Add(ObjDrop);
                            }
                            break;
                    }
                }
                ObjResp.objcountrydd.CountryDD = objLstDrop;


                objRes = Utility.checkValidSubscriber("1", clientSetting);

                if (objRes.ResponseCode == "0")
                {
                   serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        objPayGFamilyBundleRes = serviceCRM.CRMPayGFullFamilyBundle(objPayGFamilyBundleReq);

                        if (clientSetting.mvnoSettings.EnablePortugalVATScope != "0")
                        {
                            InvoiceAddressRequest invoicereq = new InvoiceAddressRequest();
                            InvoiceAddressResponse invoiceres = new InvoiceAddressResponse();
                            try
                            {
                                invoicereq.BrandCode = clientSetting.brandCode;
                                invoicereq.CountryCode = clientSetting.countryCode;
                                invoicereq.LanguageCode = clientSetting.langCode;
                                invoicereq.Msisdn = Session["MobileNumber"].ToString();
                                invoiceres = serviceCRM.InvoiceAddressCRM(invoicereq);
                                objPayGFamilyBundleRes.EmailLanguage = invoiceres.EmailLanguage;
                                Session["PRTAddress"] = invoiceres.contentPDF;
                            }
                            catch (Exception eX)
                            {
                                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                            }
                            finally
                            {
                                invoicereq = null;
                                invoiceres = null;
                            }
                        }
                        else
                        {
                            if (clientSetting.preSettings.EnableRegulatoryFee.ToLower() == "true")
                            {
                                InvoiceAddressRequest invoicereq = new InvoiceAddressRequest();
                                InvoiceAddressResponse invoiceres = new InvoiceAddressResponse();
                                try
                                {
                                    invoicereq.BrandCode = clientSetting.brandCode;
                                    invoicereq.CountryCode = clientSetting.countryCode;
                                    invoicereq.LanguageCode = clientSetting.langCode;
                                    invoicereq.Msisdn = Session["MobileNumber"].ToString();
                                    invoiceres = serviceCRM.InvoiceAddressCRM(invoicereq);
                                    objPayGFamilyBundleRes.EmailLanguage = invoiceres.EmailLanguage;
                                    Session["GeneralAddress"] = invoiceres.contentPDF;
                                }
                                catch (Exception eX)
                                {
                                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                                }
                                finally
                                {
                                    invoicereq = null;
                                    invoiceres = null;
                                }
                            }
                        }

                    

                    if (objPayGFamilyBundleRes != null && objPayGFamilyBundleRes.FamilyExpiryDate != null)
                    {
                        if (objPayGFamilyBundleRes.FamilyExpiryDate != null)
                        {
                            if (clientSetting.preSettings.EnableFamilyBundleretry.ToUpper() == "TRUE")
                            {
                                string CurrentDate = string.Empty;
                                try
                                {

                                    DateTime dateTime = DateTime.UtcNow.Date;
                                    CurrentDate = dateTime.ToString("dd-MM-yyyy");
                                    DateTime Expirydatecheck = DateTime.ParseExact(objPayGFamilyBundleRes.FamilyExpiryDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                    DateTime Currentdatevalue = DateTime.ParseExact(CurrentDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                    if (Expirydatecheck >= Currentdatevalue)
                                    {
                                        ObjResp.checkDate = "1";
                                    }
                                    else
                                    {
                                        ObjResp.checkDate = "0";
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }

                        objPayGFamilyBundleRes.FamilyRenewalOn = DateTime.ParseExact(objPayGFamilyBundleRes.FamilyExpiryDate, "dd-MM-yyyy", System.Globalization.CultureInfo.CurrentCulture).AddDays(1).ToString(clientSetting.mvnoSettings.dateTimeFormat.ToLower().Replace("mm", "MM"));
                        objPayGFamilyBundleRes.FamilyExpiryDate = Utility.GetDateconvertion(objPayGFamilyBundleRes.FamilyExpiryDate, "DD-MM-YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);
                    }
                    if (objPayGFamilyBundleRes != null && objPayGFamilyBundleRes.Response != null && objPayGFamilyBundleRes.Response.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PayGFamilyPlan_" + objPayGFamilyBundleRes.Response.ResponseCode);
                        objPayGFamilyBundleRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objPayGFamilyBundleRes.Response.ResponseDesc : errorInsertMsg;
                    }
                }
                else
                {
                    objPayGFamilyBundleRes.Response = objRes;
                    objPayGFamilyBundleRes.PaygFamilyMemberDetailsList = new List<PaygFamilyMemberDetails>();
                }

                if (objPayGFamilyBundleRes.Response != null && clientSetting.preSettings.newPayGFamilyCreationSuccessErrorCode.Split('|').Contains(objPayGFamilyBundleRes.Response.ResponseCode))
                {
                    if (!string.IsNullOrEmpty(clientSetting.preSettings.checkRegisterdCustForPayG) && clientSetting.preSettings.checkRegisterdCustForPayG.ToUpper() == "TRUE")
                    {
                        objRes = Utility.checkValidSubscriber("", clientSetting);

                        if (objRes.ResponseCode == "101")
                        {
                            objPayGFamilyBundleRes.Response = objRes;
                        }
                    }
                }

                //Parallel.ForEach(objPayGFamilyBundleRes.PaygFamilyMemberDetailsList, (record, state, index) =>
                //{
                //    record.SubscriberBundles.FindAll(a => !string.IsNullOrEmpty(a.RENEWAL_CANCELLATION_EFFECTIVE_DATE)).ForEach(b => b.RENEWAL_CANCELLATION_EFFECTIVE_DATE = Utility.FormatDateTime(b.RENEWAL_CANCELLATION_EFFECTIVE_DATE, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.ddMMyyyy));
                //});

                //6581

                if (objPayGFamilyBundleRes.PaygFamilyMemberDetailsList != null && objPayGFamilyBundleRes.PaygFamilyMemberDetailsList.Count > 0)
                {
                    foreach (var list in objPayGFamilyBundleRes.PaygFamilyMemberDetailsList)
                    {
                        if (list.SubscriberBundles != null && list.SubscriberBundles.Count > 0)
                        {
                            list.SubscriberBundles.FindAll(a => !string.IsNullOrEmpty(a.RENEWAL_CANCELLATION_EFFECTIVE_DATE)).ForEach(b => b.RENEWAL_CANCELLATION_EFFECTIVE_DATE = Utility.FormatDateTime(b.RENEWAL_CANCELLATION_EFFECTIVE_DATE, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.ddMMyyyy));
                        }
                    }
                }

                ObjResp.lstDirectdebitCardDetails = GetDebitCardDetails();
                ObjResp.FamilyBundles = objPayGFamilyBundleRes.FamilyBundles;
                ObjResp.ChildFamilyBundles = objPayGFamilyBundleRes.ChildFamilyBundles;
                ObjResp.AddOnBundles = objPayGFamilyBundleRes.AddOnBundles;
                ObjResp.EmailLanguage = objPayGFamilyBundleRes.EmailLanguage;
                ObjResp.FamilyRenewalOn = objPayGFamilyBundleRes.FamilyRenewalOn;
                ObjResp.FamilyExpiryDate = objPayGFamilyBundleRes.FamilyExpiryDate;
                ObjResp.Response = objPayGFamilyBundleRes.Response;
                ObjResp.Response.ResponseDesc = objPayGFamilyBundleRes.Response.ResponseDesc;
                ObjResp.PaygFamilyMemberDetailsList = objPayGFamilyBundleRes.PaygFamilyMemberDetailsList;
                ObjResp.MaximumChildCount = objPayGFamilyBundleRes.MaximumChildCount;
                ObjResp.MinimumChildCount = objPayGFamilyBundleRes.MinimumChildCount;
                ObjResp.LoginMsisdnIndicator = objPayGFamilyBundleRes.LoginMsisdnIndicator;
                ObjResp.ModeOfRenewal = objPayGFamilyBundleRes.ModeOfRenewal;
                ObjResp.FamilyStatus = objPayGFamilyBundleRes.FamilyStatus;
                ObjResp.SubscriberAccountBalance = objPayGFamilyBundleRes.SubscriberAccountBalance;
                ObjResp.TopupMaster = objPayGFamilyBundleRes.TopupMaster;
                ObjResp.TotalAmount = objPayGFamilyBundleRes.TotalAmount;
                ObjResp.ReferenceNumber = objPayGFamilyBundleRes.ReferenceNumber;
                ObjResp.TransactionNumber = objPayGFamilyBundleRes.TransactionNumber;
                ObjResp.FamilyAccID = objPayGFamilyBundleRes.FamilyAccID;
                ObjResp.FamilyAutoRenewalCount = objPayGFamilyBundleRes.FamilyAutoRenewalCount;
                ObjResp.NextRenewalCost = objPayGFamilyBundleRes.NextRenewalCost;
                ObjResp.ParentName = objPayGFamilyBundleRes.ParentName;
                ObjResp.PdfAddress = objPayGFamilyBundleRes.PdfAddress;
                ObjResp.VatPerc = objPayGFamilyBundleRes.VatPerc;
                ObjResp.DateTime = objPayGFamilyBundleRes.DateTime;
                // 4858
                ObjResp.bundlecode = objPayGFamilyBundleRes.bundleCode;
                ObjResp.bundlename = objPayGFamilyBundleRes.bundlename;
                ObjResp.isportin = objPayGFamilyBundleRes.isportinvalue;

                //4927
                ObjResp.IsRemoveReserveFlag = objPayGFamilyBundleRes.IsRemoveReserveFlag;

                return View(ObjResp);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View(ObjResp);
            }
            finally
            {
                //objPayGFamilyBundleRes = null;
                objPayGFamilyBundleReq = null;
                objRes = null;
                serviceCRM = null;
            }

        }

        //4617
        // 4617
        public Dictionary<string, string> GetDebitCardDetails()
        {

            PayGFamilyBundle ObjResp = new PayGFamilyBundle();
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
                        ObjResp.lstDirectdebitCardDetails.Add(manageCardResp.debitcardDetails[i].cardId + "," + manageCardResp.debitcardDetails[i].Country + "," + manageCardResp.debitcardDetails[i].addressline1 + "," + manageCardResp.debitcardDetails[i].addressline2 + "," + manageCardResp.debitcardDetails[i].postcode + "," + manageCardResp.debitcardDetails[i].email + "," + manageCardResp.debitcardDetails[i].city + "," + manageCardResp.debitcardDetails[i].cardNumber + "," + manageCardResp.debitcardDetails[i].nameOnCard, manageCardResp.debitcardDetails[i].cardNumber);
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
            return ObjResp.lstDirectdebitCardDetails;
        }

        public JsonResult getEncryptDecrypt(string msisdn, string mode, string URL, string ContActionName)
        {
            mode = mode == null ? "NEW_REGN" : mode;
            string anatelid = "";
            var requrl = "";
            //var requrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');
            if (!string.IsNullOrEmpty(URL))
            {
                requrl = URL.Replace(ContActionName, "").Split('?')[0];
            }
            else
            {
                requrl = Request.Url.ToString().Replace("Bundle/getEncryptDecrypt", "").Split('?')[0];
            }
            //var requrl = Request.Url.ToString().Replace("CRM/getEncryptDecrypt", "").Split('?')[0];

            CRMLogger.WriteMessage(string.Empty, this.ControllerContext, requrl);
            string returl = "";
            if (!string.IsNullOrEmpty(ContActionName))
            {
                returl = Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + mode + "|" + Convert.ToString(Session["MobileNumber"]) + "|" + DateTime.Now + "|" + ContActionName;
            }
            else
            {
                returl = Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + mode + "|" + Convert.ToString(Session["MobileNumber"]) + "|" + DateTime.Now;
            }

            CRMLogger.WriteMessage(string.Empty, this.ControllerContext, returl);
            #region Brazil Specific
            if (clientSetting.countryCode == "BRA" && (mode == "NEW_REGN" || mode == "EDIT_REGN" || mode == "VIEW_REGN"))
            {
                if (mode == "NEW_REGN")
                {
                    //anatelid = CRMGenerateAnatelid();
                }
                else
                {
                    anatelid = Convert.ToString(Session["AnatelID"]);
                }
                returl = returl + '|' + anatelid;
                using (LoginController lgnctr = new LoginController())
                {
                    string type = (mode == "NEW_REGN") ? "Registration" : (mode == "EDIT_REGN") ? "Edit Registration" : "View Registration";
                    string typedesc = (mode == "NEW_REGN") ? "RegisterrequestBrazil" : (mode == "EDIT_REGN") ? "EditRegisterrequestBrazil" : "ViewRegisterrequestBrazil";
                    AuditTrailRequest auditTrailRequest = new AuditTrailRequest();
                    auditTrailRequest.module = "Subscriber";
                    auditTrailRequest.subModule = type + " Request";
                    auditTrailRequest.action = "Brazil " + type;
                    auditTrailRequest.description = type + " Request";
                    auditTrailRequest.anatelId = anatelid;
                    auditTrailRequest.DescID = typedesc;
                    Session["AnatelID"] = anatelid;
                    lgnctr.AuditTrailCRM(auditTrailRequest);

                }
            }
            #endregion

            string enurl = Utility.EncryptDecrypt(returl);
            //RM parameters
            // string baseUrlID = "CRM|" + clientSetting.brandCode + "|" + msisdn + "|" + Session["UserName"].ToString() + "|" + mode + "|" + requrl + "?Keyframe=" + enurl + "|" + Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            string SessionID = Utility.mthdSessionID();
            string baseUrlID = "CRM|" + clientSetting.brandCode + "|" + msisdn + "|" + Session["UserName"].ToString() + ",";
            baseUrlID += Session["UserGroup"].ToString() + "|" + mode + "|" + requrl + "?Keyframe=" + enurl + "|";
            baseUrlID += Thread.CurrentThread.CurrentUICulture.Name.ToUpper() + "|" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            baseUrlID += "|" + SessionID;
            string val = Utility.EncryptDecrypt(baseUrlID);

            return Json(val, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult ChildValidation(string PayGFamilyBundleReq)
        {
            PayGFamilyBundleReq objPayGFamilyBundleReq = new JavaScriptSerializer().Deserialize<PayGFamilyBundleReq>(PayGFamilyBundleReq);
            PayGFamilyBundleRes objPayGFamilyBundleRes = new PayGFamilyBundleRes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objPayGFamilyBundleReq.CountryCode = clientSetting.countryCode;
                objPayGFamilyBundleReq.BrandCode = clientSetting.brandCode;
                objPayGFamilyBundleReq.LanguageCode = clientSetting.langCode;

                objPayGFamilyBundleReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                objPayGFamilyBundleReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                objPayGFamilyBundleReq.SIM_CATEGORY = Session["SIM_CATEGORY"].ToString();
                 serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objPayGFamilyBundleRes = serviceCRM.CRMPayGFullFamilyBundle(objPayGFamilyBundleReq);
                
                if (objPayGFamilyBundleRes != null && objPayGFamilyBundleRes.Response != null && objPayGFamilyBundleRes.Response.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleInfo_" + objPayGFamilyBundleRes.Response.ResponseCode);
                    objPayGFamilyBundleRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objPayGFamilyBundleRes.Response.ResponseDesc : errorInsertMsg;
                }
                return Json(objPayGFamilyBundleRes);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(objPayGFamilyBundleRes);
            }
            finally
            {
                objPayGFamilyBundleReq = null;
                //objPayGFamilyBundleRes = null;
                serviceCRM = null;
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult PurchasePayGFamilyPlan(string PurchasePayGFamilyPlan)
        {
            PayGFamilyBundleReq objPayGFamilyBundleReq = new PayGFamilyBundleReq();
            PayGFamilyBundleRes objPayGFamilyBundleRes = new PayGFamilyBundleRes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objPayGFamilyBundleReq = JsonConvert.DeserializeObject<PayGFamilyBundleReq>(PurchasePayGFamilyPlan);
                objPayGFamilyBundleReq.BrandCode = clientSetting.brandCode;
                objPayGFamilyBundleReq.CountryCode = clientSetting.countryCode;
                objPayGFamilyBundleReq.LanguageCode = clientSetting.langCode;

                objPayGFamilyBundleReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                objPayGFamilyBundleReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                objPayGFamilyBundleReq.SIM_CATEGORY = Convert.ToString(Session["SIM_CATEGORY"]);
                objPayGFamilyBundleReq.UserName = Convert.ToString(Session["UserName"]);
                //4808
                if (Convert.ToString(Session["IsRealMsisdn"]) == "1")
                    {
                    objPayGFamilyBundleReq.ICCID = Convert.ToString(Session["ICCID"]);
                }

                if (objPayGFamilyBundleReq.Cardetails != null && !string.IsNullOrEmpty(objPayGFamilyBundleReq.Cardetails.ConsentDate))
                {
                    objPayGFamilyBundleReq.Cardetails.ConsentDate = Utility.GetDateconvertion(objPayGFamilyBundleReq.Cardetails.ConsentDate, "MM/DD/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                }


                if (clientSetting.mvnoSettings.CaptureIPAddress.ToUpper() == "ON")
                {
                    objPayGFamilyBundleReq.DeviceInfo = Utility.DeviceInfo("Purchase PayG FamilyPlan");
                }
                if (objPayGFamilyBundleReq.PaymentMode == "APM")
                {
                    objPayGFamilyBundleReq.AccountHolder = Utility.UseragentAPM("PayGFamily");
                }

               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objPayGFamilyBundleRes = serviceCRM.CRMPayGFullFamilyBundle(objPayGFamilyBundleReq);

                

                if (objPayGFamilyBundleRes != null && objPayGFamilyBundleRes.Response != null && objPayGFamilyBundleRes.Response.ResponseCode == "40" && objPayGFamilyBundleReq.DeletionType == "0" && objPayGFamilyBundleReq.MSISDN == Convert.ToString(Session["MobileNumber"]) && objPayGFamilyBundleReq.Mode == "4")
                {
                    Session["FamilyAccID"] = "";
                }

                if (objPayGFamilyBundleRes != null && objPayGFamilyBundleRes.Response != null && objPayGFamilyBundleRes.Response.ResponseCode != null)
                {

                    objPayGFamilyBundleRes.DateTime = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                    objPayGFamilyBundleRes.DateTime = objPayGFamilyBundleRes.DateTime.Replace("DD", Convert.ToString(System.DateTime.Now.Day));
                    objPayGFamilyBundleRes.DateTime = objPayGFamilyBundleRes.DateTime.Replace("MM", Convert.ToString(Convert.ToInt32(System.DateTime.Now.Month) - 1));
                    objPayGFamilyBundleRes.DateTime = objPayGFamilyBundleRes.DateTime.Replace("YYYY", Convert.ToString(System.DateTime.Now.Year));
                    objPayGFamilyBundleRes.DateTime = objPayGFamilyBundleRes.DateTime + " " + Convert.ToString(System.DateTime.Now.Hour) + ":" + Convert.ToString(System.DateTime.Now.Minute) + ":" + Convert.ToString(System.DateTime.Now.Second);


                    objPayGFamilyBundleRes.Response.ResponseCode = objPayGFamilyBundleRes.Response.ResponseCode.TrimEnd(',');
                    if (objPayGFamilyBundleRes.Response.ResponseCode.Contains(','))
                    {
                        string RetrunCode = "";
                        if (objPayGFamilyBundleRes.Response.ResponseCode.Split(',').Contains("0"))
                        {
                            RetrunCode = "0";


                            if (objPayGFamilyBundleReq.Mode == "3" && RetrunCode == "0")
                            {
                                for (int i = 0; i < objPayGFamilyBundleReq.PaygFamilyMemberDetailsList.Count; i++)
                                {

                                    Session["NewChild"] += "," + objPayGFamilyBundleReq.PaygFamilyMemberDetailsList[i].MSISDN;
                                }

                            }
                        }
                        string strReturnDesc = string.Empty;
                        for (int i = 0; i < objPayGFamilyBundleRes.Response.ResponseCode.Split(',').Length; i++)
                        {
                            //string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleInfo_" + objPayGFamilyBundleRes.Response.ResponseCode[i]);
                            string errorInsertMsg = "";
                            if (objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[i].Contains('|'))
                            {
                                if (objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[i].Split('|').Length > 3)
                                {
                                    if (objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[i].Split('|')[3] == "1")
                                    {
                                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PayGFamilyPlan_P" + objPayGFamilyBundleRes.Response.ResponseCode.Split(',')[i]);
                                    }
                                    else
                                    {
                                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PayGFamilyPlan_C" + objPayGFamilyBundleRes.Response.ResponseCode.Split(',')[i]);
                                    }
                                    if (!string.IsNullOrEmpty(errorInsertMsg))
                                    {
                                        errorInsertMsg = errorInsertMsg.Replace("#MSISDN#", objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[i].Split('|')[1]);
                                        errorInsertMsg = errorInsertMsg.Replace("#BUNDLECODE#", objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[i].Split('|')[2]);
                                        errorInsertMsg = errorInsertMsg.Replace("#TOPUPAMOUNT#", objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[i].Split('|')[2]);
                                    }
                                    if (!string.IsNullOrEmpty(errorInsertMsg) && objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[i].Split('|').Length > 4 && !string.IsNullOrEmpty(objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[i].Split('|')[4]))
                                    {
                                        errorInsertMsg += Resources.ErrorResources.ResourceManager.GetString("PayGFamilyPlan_" + objPayGFamilyBundleRes.Response.ResponseCode.Split(',')[i]);
                                    }
                                    strReturnDesc += string.IsNullOrEmpty(errorInsertMsg) ? objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[i].Split('|')[0] : errorInsertMsg + ",";
                                }
                                else
                                {
                                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PayGFamilyPlan_" + objPayGFamilyBundleRes.Response.ResponseCode.Split(',')[i]);
                                    strReturnDesc += string.IsNullOrEmpty(errorInsertMsg) ? objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[i] : errorInsertMsg + ",";
                                }

                            }
                            else
                            {
                                strReturnDesc += string.IsNullOrEmpty(errorInsertMsg) ? objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[i] : errorInsertMsg + ",";
                            }
                        }
                        if (!string.IsNullOrEmpty(strReturnDesc))
                        {
                            strReturnDesc = strReturnDesc.TrimEnd(',');
                            objPayGFamilyBundleRes.Response.ResponseDesc = strReturnDesc;
                        }
                        if (RetrunCode == "0")
                        {
                            objPayGFamilyBundleRes.Response.ResponseCode = "0";
                        }
                    }
                    else if (objPayGFamilyBundleReq.Mode == "3" && objPayGFamilyBundleRes.Response.ResponseCode == "0")
                    {
                        string errorInsertMsg = "";
                        if (objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[0].Split('|')[3] == "1")
                        {
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PayGFamilyPlan_P" + objPayGFamilyBundleRes.Response.ResponseCode);
                        }
                        else
                        {
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PayGFamilyPlan_C" + objPayGFamilyBundleRes.Response.ResponseCode);
                        }
                        errorInsertMsg = errorInsertMsg.Replace("#MSISDN#", objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[0].Split('|')[1]);
                        errorInsertMsg = errorInsertMsg.Replace("#BUNDLECODE#", objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[0].Split('|')[2]);
                        errorInsertMsg = errorInsertMsg.Replace("#TOPUPAMOUNT#", objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[0].Split('|')[2]);
                        objPayGFamilyBundleRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objPayGFamilyBundleRes.Response.ResponseDesc : errorInsertMsg;
                        if (objPayGFamilyBundleReq.Mode == "3" && objPayGFamilyBundleRes.Response.ResponseCode == "0")
                        {
                            for (int i = 0; i < objPayGFamilyBundleReq.PaygFamilyMemberDetailsList.Count; i++)
                            {
                                Session["NewChild"] += "," + objPayGFamilyBundleReq.PaygFamilyMemberDetailsList[i].MSISDN;
                            }
                        }
                    }
                    else
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleInfo_" + objPayGFamilyBundleRes.Response.ResponseCode);
                        objPayGFamilyBundleRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objPayGFamilyBundleRes.Response.ResponseDesc : errorInsertMsg;
                        if (objPayGFamilyBundleReq.Mode == "3" && objPayGFamilyBundleRes.Response.ResponseCode == "0")
                        {
                            for (int i = 0; i < objPayGFamilyBundleReq.PaygFamilyMemberDetailsList.Count; i++)
                            {
                                Session["NewChild"] += "," + objPayGFamilyBundleReq.PaygFamilyMemberDetailsList[i].MSISDN;
                            }

                        }
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(Session["FamilyAccID"])))
                    {
                        if (!string.IsNullOrEmpty(objPayGFamilyBundleRes.FamilyAccID))
                        {
                            Session["FamilyAccID"] = objPayGFamilyBundleRes.FamilyAccID;
                        }
                    }
                }


                if (!string.IsNullOrEmpty(objPayGFamilyBundleReq.PRTVatNo) && Convert.ToString(Session["Verify"]) == "1" && clientSetting.mvnoSettings.EnablePortugalVATScope != "0")
                {
                    Session["InvoiceVatNo"] = objPayGFamilyBundleReq.PRTVatNo;
                }
                return Json(objPayGFamilyBundleRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(objPayGFamilyBundleRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                // objPayGFamilyBundleRes = null;
                serviceCRM = null;
                objPayGFamilyBundleReq = null;
            }
        }
        //4724
        [ValidateInput(false)]
        public JsonResult PurchasePayGFamilyPlanForEmailAPM(string PurchasePayGFamilyPlan)
        {
            PayGFamilyBundleReq objPayGFamilyBundleReq = new PayGFamilyBundleReq();
            PayGFamilyBundleRes objPayGFamilyBundleRes = new PayGFamilyBundleRes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                PayGFamilyBundleRes PurchasePayGFamilyPlan1 = new PayGFamilyBundleRes();
                objPayGFamilyBundleReq = JsonConvert.DeserializeObject<PayGFamilyBundleReq>(PurchasePayGFamilyPlan);
                objPayGFamilyBundleReq.PaygFamilyMemberDetailsList = PurchasePayGFamilyPlan1.PaygFamilyMemberDetailsList;

                objPayGFamilyBundleReq.ExpiryDate = PurchasePayGFamilyPlan1.FamilyExpiryDate;
                objPayGFamilyBundleReq.PaymentMode = PurchasePayGFamilyPlan1.ModeOfRenewal;
                objPayGFamilyBundleReq.UserName = Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[0].ToString() + " " + Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[1].ToString();

                //objPayGFamilyBundleReq = JsonConvert.DeserializeObject<PayGFamilyBundleReq>(PurchasePayGFamilyPlan);
                objPayGFamilyBundleReq.BrandCode = clientSetting.brandCode;
                objPayGFamilyBundleReq.CountryCode = clientSetting.countryCode;
                objPayGFamilyBundleReq.LanguageCode = clientSetting.langCode;

                objPayGFamilyBundleReq.Mode = "6";
               // objPayGFamilyBundleReq.EmailId = Convert.ToString(Session["eMailID"]);
                objPayGFamilyBundleReq.MSISDN = Convert.ToString(Session["MobileNumber"]);


                objPayGFamilyBundleReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                objPayGFamilyBundleReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                objPayGFamilyBundleReq.SIM_CATEGORY = Session["SIM_CATEGORY"].ToString();
             serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objPayGFamilyBundleRes = serviceCRM.CRMPayGFullFamilyBundle(objPayGFamilyBundleReq);
                
                if (objPayGFamilyBundleRes != null && objPayGFamilyBundleRes.Response != null && objPayGFamilyBundleRes.Response.ResponseCode != null)
                {
                    objPayGFamilyBundleRes.Response.ResponseCode = objPayGFamilyBundleRes.Response.ResponseCode.TrimEnd(',');
                    if (objPayGFamilyBundleRes.Response.ResponseCode.Contains(','))
                    {
                        string RetrunCode = "";
                        if (objPayGFamilyBundleRes.Response.ResponseCode.Split(',').Contains("0"))
                        {
                            RetrunCode = "0";

                            if (objPayGFamilyBundleReq.Mode == "3" && RetrunCode == "0")
                            {
                                for (int i = 0; i < objPayGFamilyBundleReq.PaygFamilyMemberDetailsList.Count; i++)
                                {

                                    Session["NewChild"] += "," + objPayGFamilyBundleReq.PaygFamilyMemberDetailsList[i].MSISDN;
                                }

                            }
                        }
                        string strReturnDesc = string.Empty;
                        for (int i = 0; i < objPayGFamilyBundleRes.Response.ResponseCode.Split(',').Length; i++)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleInfo_" + objPayGFamilyBundleRes.Response.ResponseCode[i]);
                            strReturnDesc += string.IsNullOrEmpty(errorInsertMsg) ? objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[i] : errorInsertMsg;
                        }
                        if (!string.IsNullOrEmpty(strReturnDesc))
                        {
                            objPayGFamilyBundleRes.Response.ResponseDesc = strReturnDesc;
                        }
                        if (RetrunCode == "0")
                        {
                            objPayGFamilyBundleRes.Response.ResponseCode = "0";
                        }
                    }
                    else
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleInfo_" + objPayGFamilyBundleRes.Response.ResponseCode);
                        objPayGFamilyBundleRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objPayGFamilyBundleRes.Response.ResponseDesc : errorInsertMsg;
                        if (objPayGFamilyBundleReq.Mode == "3" && objPayGFamilyBundleRes.Response.ResponseCode == "0")
                        {
                            for (int i = 0; i < objPayGFamilyBundleReq.PaygFamilyMemberDetailsList.Count; i++)
                            {
                                Session["NewChild"] += "," + objPayGFamilyBundleReq.PaygFamilyMemberDetailsList[i].MSISDN;
                            }

                        }
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(Session["FamilyAccID"])))
                    {
                        if (!string.IsNullOrEmpty(objPayGFamilyBundleRes.FamilyAccID))
                        {
                            Session["FamilyAccID"] = objPayGFamilyBundleRes.FamilyAccID;
                        }
                    }
                }
                return Json(objPayGFamilyBundleRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(objPayGFamilyBundleRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objPayGFamilyBundleReq = null;
                //objPayGFamilyBundleRes = null;
                serviceCRM = null;
            }

        }


        [ValidateInput(false)]
        public JsonResult PurchasePayGFamilyPlanForEmail(string PurchasePayGFamilyPlan)
        {
            PayGFamilyBundleReq objPayGFamilyBundleReq = new PayGFamilyBundleReq();
            PayGFamilyBundleRes objPayGFamilyBundleRes = new PayGFamilyBundleRes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                PayGFamilyBundleRes PurchasePayGFamilyPlan1 = new PayGFamilyBundleRes();
                PurchasePayGFamilyPlan1 = JsonConvert.DeserializeObject<PayGFamilyBundleRes>(PurchasePayGFamilyPlan);
                objPayGFamilyBundleReq.PaygFamilyMemberDetailsList = PurchasePayGFamilyPlan1.PaygFamilyMemberDetailsList;

                objPayGFamilyBundleReq.ExpiryDate = PurchasePayGFamilyPlan1.FamilyExpiryDate;
                objPayGFamilyBundleReq.PaymentMode = PurchasePayGFamilyPlan1.ModeOfRenewal;
                objPayGFamilyBundleReq.UserName = Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[0].ToString() + " " + Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[1].ToString();

                //objPayGFamilyBundleReq = JsonConvert.DeserializeObject<PayGFamilyBundleReq>(PurchasePayGFamilyPlan);
                objPayGFamilyBundleReq.BrandCode = clientSetting.brandCode;
                objPayGFamilyBundleReq.CountryCode = clientSetting.countryCode;
                objPayGFamilyBundleReq.LanguageCode = clientSetting.langCode;

                objPayGFamilyBundleReq.Mode = "6";
                objPayGFamilyBundleReq.EmailId = Convert.ToString(Session["eMailID"]);
                objPayGFamilyBundleReq.MSISDN = Convert.ToString(Session["MobileNumber"]);


                objPayGFamilyBundleReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                objPayGFamilyBundleReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                objPayGFamilyBundleReq.SIM_CATEGORY = Session["SIM_CATEGORY"].ToString();
              serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objPayGFamilyBundleRes = serviceCRM.CRMPayGFullFamilyBundle(objPayGFamilyBundleReq);
                
                if (objPayGFamilyBundleRes != null && objPayGFamilyBundleRes.Response != null && objPayGFamilyBundleRes.Response.ResponseCode != null)
                {
                    objPayGFamilyBundleRes.Response.ResponseCode = objPayGFamilyBundleRes.Response.ResponseCode.TrimEnd(',');
                    if (objPayGFamilyBundleRes.Response.ResponseCode.Contains(','))
                    {
                        string RetrunCode = "";
                        if (objPayGFamilyBundleRes.Response.ResponseCode.Split(',').Contains("0"))
                        {
                            RetrunCode = "0";

                            if (objPayGFamilyBundleReq.Mode == "3" && RetrunCode == "0")
                            {
                                for (int i = 0; i < objPayGFamilyBundleReq.PaygFamilyMemberDetailsList.Count; i++)
                                {

                                    Session["NewChild"] += "," + objPayGFamilyBundleReq.PaygFamilyMemberDetailsList[i].MSISDN;
                                }

                            }
                        }
                        string strReturnDesc = string.Empty;
                        for (int i = 0; i < objPayGFamilyBundleRes.Response.ResponseCode.Split(',').Length; i++)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleInfo_" + objPayGFamilyBundleRes.Response.ResponseCode[i]);
                            strReturnDesc += string.IsNullOrEmpty(errorInsertMsg) ? objPayGFamilyBundleRes.Response.ResponseDesc.Split(',')[i] : errorInsertMsg;
                        }
                        if (!string.IsNullOrEmpty(strReturnDesc))
                        {
                            objPayGFamilyBundleRes.Response.ResponseDesc = strReturnDesc;
                        }
                        if (RetrunCode == "0")
                        {
                            objPayGFamilyBundleRes.Response.ResponseCode = "0";
                        }
                    }
                    else
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleInfo_" + objPayGFamilyBundleRes.Response.ResponseCode);
                        objPayGFamilyBundleRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objPayGFamilyBundleRes.Response.ResponseDesc : errorInsertMsg;
                        if (objPayGFamilyBundleReq.Mode == "3" && objPayGFamilyBundleRes.Response.ResponseCode == "0")
                        {
                            for (int i = 0; i < objPayGFamilyBundleReq.PaygFamilyMemberDetailsList.Count; i++)
                            {
                                Session["NewChild"] += "," + objPayGFamilyBundleReq.PaygFamilyMemberDetailsList[i].MSISDN;
                            }

                        }
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(Session["FamilyAccID"])))
                    {
                        if (!string.IsNullOrEmpty(objPayGFamilyBundleRes.FamilyAccID))
                        {
                            Session["FamilyAccID"] = objPayGFamilyBundleRes.FamilyAccID;
                        }
                    }
                }
                return Json(objPayGFamilyBundleRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(objPayGFamilyBundleRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objPayGFamilyBundleReq = null;
                serviceCRM = null;
                //objPayGFamilyBundleRes = null;
            }

        }


        public ActionResult CRMQueruBundleSubscriptionForMultipleMsidn(string objQueryBundleSubscriptionReqList)
        {
            QueryBundleSubscriptionListRes ObjRes = new QueryBundleSubscriptionListRes();
            QueryBundleSubscriptionReqList objbundleReq = new JavaScriptSerializer().Deserialize<QueryBundleSubscriptionReqList>(objQueryBundleSubscriptionReqList);
            ServiceInvokeCRM serviceCRM;
            try
            {
                objbundleReq.CountryCode = clientSetting.countryCode;
                objbundleReq.BrandCode = clientSetting.brandCode;
                objbundleReq.LanguageCode = clientSetting.langCode;
               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMQueruBundleSubscriptionForMultipleMsisdn(objbundleReq);

                    if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CSBundleandTopup_QueruBundleSubs_" + ObjRes.Response.ResponseCode);
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
                objbundleReq = null;
                //ObjRes = null;
                serviceCRM = null;
            }

        }


        public JsonResult CRMGenericOTP(string CRMGenericOTP)
        {
            CRMGenericOTPReq objReq = new CRMGenericOTPReq();
            CRMGenericOTPRes objRes = new CRMGenericOTPRes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objReq = JsonConvert.DeserializeObject<CRMGenericOTPReq>(CRMGenericOTP);
                objReq.BrandCode = clientSetting.brandCode;
                objReq.CountryCode = clientSetting.countryCode;
                objReq.LanguageCode = clientSetting.langCode;
                //objReq.MSISDN = Session["MobileNumber"].ToString();
               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.CRMGenericOTP(objReq);
                
                if (objRes != null && objRes.reponseDetails != null && objRes.reponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleInfo_" + objRes.reponseDetails.ResponseCode);
                    objRes.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.reponseDetails.ResponseDesc : errorInsertMsg;
                }

                if (!string.IsNullOrEmpty(objReq.Username) && objReq.Mode == "3" && (objRes.reponseDetails.ResponseCode == "0" || objRes.reponseDetails.ResponseCode == "55" || objRes.reponseDetails.ResponseCode == "56" || objRes.reponseDetails.ResponseCode == "57"))
                {
                    Session["UserName"] = objReq.Username;
                }
                return Json(objRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(objRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objReq = null;
                serviceCRM = null;
                //objRes = null;
            }

        }
        public JsonResult AssignUser(string UserName)
        {
            CRMBase obj = new CRMBase();
            if (!string.IsNullOrEmpty(UserName))
                Session["UserName"] = UserName;
            return Json(obj);
        }

        public ActionResult GetPayGFamilyMSISDNHistory(string Details)
        {
            PayGHistoryResponse ObjResp = new PayGHistoryResponse();
            PayGHistoryRequest ObjReq = JsonConvert.DeserializeObject<PayGHistoryRequest>(Details);
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;

                if (!string.IsNullOrEmpty(ObjReq.FromDate))
                {
                    ObjReq.FromDate = Utility.GetDateconvertion(ObjReq.FromDate, "YYYY-MM-DD", true, clientSetting.mvnoSettings.dateTimeFormat);
                    ObjReq.ToDate = Utility.GetDateconvertion(ObjReq.ToDate, "YYYY-MM-DD", true, clientSetting.mvnoSettings.dateTimeFormat);
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjResp = serviceCRM.GetPayGFamilyMSISDNHistory(ObjReq);
                

                ObjResp.FamilyRecord.ForEach(a =>
                {
                    a.CreatedDate = Utility.GetDateconvertion(a.CreatedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                    a.RemovedDate = Utility.GetDateconvertion(a.RemovedDate, "DD-MM-YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);

                });

                ObjResp.MSISDNRecord.ForEach(a =>
                {
                    a.CreatedDate = Utility.GetDateconvertion(a.CreatedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                    a.DNADate = Utility.GetDateconvertion(a.DNADate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);

                });
                return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
                //ObjResp = null;
            }

        }
        #endregion

        #region PromotionalBundle  FRR 4356
        public ActionResult PromotionalBundle()
        {
            PromotionalBundleResponse objres = new PromotionalBundleResponse();
            PromotionalBundleRequest objreq = new PromotionalBundleRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - PromotionalBundle Start");
                objreq.Mode = "QUERY";
                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.CRMPromotionalBundleReport(objreq);
                if (objres != null && objres.responseDetails != null && objres.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SwapCPF_" + objres.responseDetails.ResponseCode);
                    objres.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.responseDetails.ResponseDesc : errorInsertMsg;
                }
                else
                {
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - PromotionalBundle - Unable to Fetch Response");
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - PromotionalBundle End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                objreq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return View(objres);
        }
        public JsonResult LoadPromotionalBundleReport(string FailureDatails)
        {
            PromotionalBundleResponse ObjResp = new PromotionalBundleResponse();
            PromotionalBundleRequest ObjReq = JsonConvert.DeserializeObject<PromotionalBundleRequest>(FailureDatails);
            string strInputDate = string.Empty;
            string strGetDate = "", strDate = "", strMonth = "", strYear = "";
            string[] strSplit;
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - LoadPromotionalBundleReport Start");
                strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                if (ObjReq.Mode == "REPORT")
                {
                    #region Date Split
                    if (ObjReq.fromDate != string.Empty)
                    {
                        strGetDate = Utility.GetDateconvertion(ObjReq.fromDate, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                        strSplit = strGetDate.Split('/');
                        if (strSplit.Length == 1)
                        {
                            strDate = Convert.ToString(strSplit[0].ToString());
                        }
                        if (strSplit.Length == 2)
                        {
                            strDate = Convert.ToString(strSplit[0].ToString());
                            strMonth = Convert.ToString(strSplit[1].ToString());
                        }
                        if (strSplit.Length == 3)
                        {
                            strDate = Convert.ToString(strSplit[0].ToString());
                            strMonth = Convert.ToString(strSplit[1].ToString());
                            strYear = Convert.ToString(strSplit[2].ToString());
                        }
                        ObjReq.fromDate = strYear + "-" + strMonth + "-" + strDate;
                    }
                    if (ObjReq.toDate != string.Empty)
                    {
                        strGetDate = Utility.GetDateconvertion(ObjReq.toDate, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                        strSplit = strGetDate.Split('/');
                        if (strSplit.Length == 1)
                        {
                            strDate = Convert.ToString(strSplit[0].ToString());
                        }
                        if (strSplit.Length == 2)
                        {
                            strDate = Convert.ToString(strSplit[0].ToString());
                            strMonth = Convert.ToString(strSplit[1].ToString());
                        }
                        if (strSplit.Length == 3)
                        {
                            strDate = Convert.ToString(strSplit[0].ToString());
                            strMonth = Convert.ToString(strSplit[1].ToString());
                            strYear = Convert.ToString(strSplit[2].ToString());
                        }
                        ObjReq.toDate = strYear + "-" + strMonth + "-" + strDate;
                    }
                    #endregion
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.CRMPromotionalBundleReport(ObjReq);
                //FRR--3083
                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LoadFailureHistory_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                //ObjResp.PromotionalBundleStatus.ForEach(a =>
                //{
                //    a.Bundleactivationdateandtime = Utility.GetDateconvertion(a.Bundleactivationdateandtime, "mm-dd-yyyy HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                //    a.Consentdateandtime = Utility.GetDateconvertion(a.Bundleactivationdateandtime, "mm-dd-yyyy HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                //    a.campaignremovaldateandtime = Utility.GetDateconvertion(a.Bundleactivationdateandtime, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                //    a.CampaignOptindateandtime = Utility.GetDateconvertion(a.Bundleactivationdateandtime, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                //});
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - LoadPromotionalBundleReport End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
                strInputDate = string.Empty;
                strGetDate = string.Empty;
                strDate = string.Empty;
                strMonth = string.Empty;
                strYear = string.Empty;
                strSplit = null;
                serviceCRM=null;
                errorInsertMsg = string.Empty;
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        [HttpPost]
        public void DownLoadPromotionalBundleReport(string topupData)
        {
            GridView gridView = new GridView();
            List<PromotionalBundleStatus> Failure;
            try
            {
                Failure = JsonConvert.DeserializeObject<List<PromotionalBundleStatus>>(topupData);
                gridView.DataSource = Failure;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "PromotionalBundleReport_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                Failure = null;
            }
        }

        #endregion


        public JsonResult FetchPaygFamilyPlanDetails(QueryPayGFamilyPlanRequest PaygFamilyPlanRequest)
        {
            QueryPayGFamilyPlanResponse PaygFamilyPlanResponse = new QueryPayGFamilyPlanResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - FetchPaygFamilyPlanDetails Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                PaygFamilyPlanRequest.CountryCode = clientSetting.countryCode;
                PaygFamilyPlanRequest.BrandCode = clientSetting.brandCode;
                PaygFamilyPlanRequest.LanguageCode = clientSetting.langCode;
                PaygFamilyPlanResponse = serviceCRM.FetchFamilyPlanDetails(PaygFamilyPlanRequest);
                if (PaygFamilyPlanResponse != null && PaygFamilyPlanResponse.responseDetails != null && PaygFamilyPlanResponse.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_BundleCost_" + PaygFamilyPlanResponse.responseDetails.ResponseCode);
                    PaygFamilyPlanResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? PaygFamilyPlanResponse.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - FetchPaygFamilyPlanDetails End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - FetchPaygFamilyPlanDetails - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(PaygFamilyPlanResponse, JsonRequestBehavior.AllowGet);
        }
        public JsonResult RefundPagyFamilyPlan(QueryPayGFamilyPlanRequest PaygFamilyPlanRequest)
        {
            QueryPayGFamilyPlanResponse PaygFamilyPlanResponse = new QueryPayGFamilyPlanResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - RefundPagyFamilyPlan Start");
                menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "Bundle_PayGFamilyPlan").ToList();
                if (menu.Count != 0)
                {
                    if (PaygFamilyPlanRequest.Status_Modify == "1" && menu[0].Approval2.ToLower() == "false")
                    {
                        PaygFamilyPlanRequest.Status_Modify = "3";
                        PaygFamilyPlanRequest.Mode = "A";
                    }
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                PaygFamilyPlanRequest.CountryCode = clientSetting.countryCode;
                PaygFamilyPlanRequest.BrandCode = clientSetting.brandCode;
                PaygFamilyPlanRequest.LanguageCode = clientSetting.langCode;
                PaygFamilyPlanResponse = serviceCRM.RefundPaygFamilyPlan(PaygFamilyPlanRequest);
                if (PaygFamilyPlanResponse != null && PaygFamilyPlanResponse.responseDetails != null && PaygFamilyPlanResponse.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_BundleCost_" + PaygFamilyPlanResponse.responseDetails.ResponseCode);
                    PaygFamilyPlanResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? PaygFamilyPlanResponse.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - RefundPagyFamilyPlan End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - RefundPagyFamilyPlan - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                errorInsertMsg = string.Empty;
                menu = null;
            }
            return Json(PaygFamilyPlanResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult QueryPagyFamilyPreservePlan(string QueryPaygFamilyPreserve)
        {
            QueryPayGFamilyPlanPreserveFamilyResponse QueryPaygFamilyPreservePlanResponse = new QueryPayGFamilyPlanPreserveFamilyResponse();
            QueryPayGFamilyPlanPreserveFamilyRequest ObjReq = JsonConvert.DeserializeObject<QueryPayGFamilyPlanPreserveFamilyRequest>(QueryPaygFamilyPreserve);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - QueryPagyFamilyPreservePlan Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                QueryPaygFamilyPreservePlanResponse = serviceCRM.CRMQueryPaygPreserveFamilyPlan(ObjReq);
                if (QueryPaygFamilyPreservePlanResponse != null && QueryPaygFamilyPreservePlanResponse.responseDetails != null && QueryPaygFamilyPreservePlanResponse.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Bundle_BundleCost_" + QueryPaygFamilyPreservePlanResponse.responseDetails.ResponseCode);
                    QueryPaygFamilyPreservePlanResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? QueryPaygFamilyPreservePlanResponse.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - QueryPagyFamilyPreservePlan End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - QueryPagyFamilyPreservePlan - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(QueryPaygFamilyPreservePlanResponse, JsonRequestBehavior.AllowGet);
        }

       
        public ActionResult GetMultiMonthBundle()
        {
            return View("MultiMonthBundle");
        }

        //4579
        public JsonResult GetMultimonthBundle(string GetMultimonthBundleRequest)
        {
            MultiBundleDetailsResponse ObjResp = new MultiBundleDetailsResponse();
            MultiBundleDetailsRequest ObjReq = JsonConvert.DeserializeObject<MultiBundleDetailsRequest>(GetMultimonthBundleRequest);
            //string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - GetMultiMonthBundleDetails Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                // ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.CRMGetMultiMonthBundleDetails(ObjReq);
                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetMultiMonthBundleDetailsRequestResponse_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetMultiMonthBundleDetails End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - GetMultiMonthBundleDetails - " + this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        // FRR - 4559 


      
        public ActionResult GetProfileMappedBundle()
        {
            return View("ProfileMappedBundle");
        }


        public JsonResult GetProfileMappedBundle(string GetProfileMappedBundleRequest)
        {
            #region FRR 4559 ***** June Bundle *****

            ProfileMappedBundleResponse ObjResp = new ProfileMappedBundleResponse();
            ProfileMappedBundleRequest ObjReq = JsonConvert.DeserializeObject<ProfileMappedBundleRequest>(GetProfileMappedBundleRequest);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - GetProfileMappedBundleDetails Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                // ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.CRMGetProfileMappedBundle(ObjReq);
                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetProfileMappedBundleDetailsRequestResponse_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - GetProfileMappedBundle End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - GetProfileMappedBundleDetails - " + this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            #endregion

        }




        #region FRR 4788
        //[HttpGet]
        //public ActionResult ViewpostpaidPlandetails()
        //{
        //    return View("ViewPostpaidPlanDetails");
        //}



    
        public PartialViewResult ViewpostpaidPlandetails()
        {
            PostpaidPlanResponse PostpaidResp = new PostpaidPlanResponse();
            PostpaidplanRequest PostpaidReq = new PostpaidplanRequest();
            PostpaidResp.bundleInfo = new List<PostPaidBundleinfo>();
            PostpaidResp.objcountrydd = new List<DropdownView>();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            DropdownView ObjDrop = new DropdownView();
            string viewdata = string.Empty;

            try
            {

               CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - PostPaidPlanDetails Start");
                PostpaidReq.CountryCode = clientSetting.countryCode;
                PostpaidReq.BrandCode = clientSetting.brandCode;
                PostpaidReq.LanguageCode = clientSetting.langCode;
                PostpaidReq.mobileNumber = Convert.ToString(Session["MobileNumber"]);
                PostpaidReq.mode = "Q";
       
              
                
                List<DropdownView> objLstDrop = new List<DropdownView>();
                DataSet ds = Utility.BindXmlFile("~/App_Data/CountryListSepaCheckout.xml");
                
                
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                PostpaidResp = serviceCRM.PostpaidPlanDetails(PostpaidReq);
                if (PostpaidResp.bundleInfo != null && PostpaidResp.bundleInfo.Count > 0)
                {
                    PostpaidResp.bundleInfo.Where(w => w.autoRenewalStatus == "1").ToList().ForEach(s => s.autoRenewalStatus = "True");
                    PostpaidResp.bundleInfo.Where(w => w.autoRenewalStatus == "0").ToList().ForEach(s => s.autoRenewalStatus = "False");
                    PostpaidResp.bundleInfo.Where(w => w.AutoRenewalMode == "1").ToList().ForEach(s => s.AutoRenewalMode = "Account Balance");
                    PostpaidResp.bundleInfo.Where(w => w.AutoRenewalMode == "2").ToList().ForEach(s => s.AutoRenewalMode = "Credit Card");
                    if (clientSetting.preSettings.EnableAPMMode.ToUpper() == "TRUE" || clientSetting.preSettings.PaymentMethodInPaymentRefund.ToUpper() == "TRUE")
                    {
                        PostpaidResp.bundleInfo.Where(w => w.Paymentmode == "1").ToList().ForEach(s => s.Paymentmode = "ONLINE");
                        PostpaidResp.bundleInfo.Where(w => w.Paymentmode == "0").ToList().ForEach(s => s.Paymentmode = "Account Balance");
                    }
                    else
                    {
                        PostpaidResp.bundleInfo.Where(w => w.Paymentmode == "1").ToList().ForEach(s => s.Paymentmode = "Credit Card");
                        PostpaidResp.bundleInfo.Where(w => w.Paymentmode == "0").ToList().ForEach(s => s.Paymentmode = "Account Balance");
                    }
                }
                try
                {
                    PostpaidResp.paymentCRM = new PaymentPostpaidCRM();
                    PostpaidResp.paymentCRM.lstCardTypes = Utility.GetDropdownMasterFromDB("12", Convert.ToString(Session["isPrePaid"]), "drop_master");
                }
                catch (Exception ex)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - PostPaidPlanCardtypeDetails - " + this.ControllerContext, ex);
                }


                if (PostpaidResp != null && PostpaidResp.responseDetails != null && PostpaidResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PostPaidPlanDetails_" + PostpaidResp.responseDetails.ResponseCode);
                    PostpaidResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? PostpaidResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - PostPaidPlanDetails End");
                foreach (DataTable dt in ds.Tables)
                {
                    switch (dt.TableName)
                    {
                        case "country":
                            foreach (DataRow row in dt.Rows)
                            {
                                ObjDrop = new DropdownView();
                                ObjDrop.ID = row["ID"].ToString();
                                ObjDrop.Value = row["country_Text"].ToString();
                                objLstDrop.Add(ObjDrop);
                            }
                            break;
                    }
               

                }
                PostpaidResp.objcountrydd = objLstDrop;
                ViewBag.IsRedirect = TempData["ViewpostpaidPlandetailsredirect"];
                if (ViewBag.IsRedirect == null)
                {
                    ViewBag.IsRedirect = TempData["DirectDebitTrustlyview"];
                }
               
                ViewBag.terminationbilled = TempData["terminationbilled"];
                ViewBag.creditusage = TempData["creditusage"];
                ViewBag.unpaidbilled = TempData["unpaidbilled"];
                ViewBag.addonbundlebilled = TempData["addonbundlebilled"];
                ViewBag.terminationbilled1 = TempData["terminationbilled1"];
                ViewBag.creditusage1 = TempData["creditusage1"];
                ViewBag.unpaidbilled1 = TempData["unpaidbilled1"];
                ViewBag.addonbundlebilled1= TempData["addonbundlebilled1"];
                ViewBag.msisdn = TempData["msisdn"];
                ViewBag.bundlecode = TempData["bundlecode"];
                return PartialView(PostpaidResp);
            }
            catch (Exception eX)
            {
                PostpaidResp.responseDetails.ResponseCode = "9";
                PostpaidResp.responseDetails.ResponseDesc = eX.Message;
                return PartialView(PostpaidResp);
            }
            finally
            {
                new LoginController().AuditTrailCRM(new AuditTrailRequest() { action = "Refund Payment", description = "Refund Payment Page Loaded", module = "Payment", subModule = "Refund", DescID = "RefundPayment_Page" });
                // paymentRefundResp = null;
            }

        }


        
        public JsonResult GETPostpaidUnbilledAmout(string unbillamtrequest)
        {
            PostpaidPlanResponse PostpaidResp = new PostpaidPlanResponse();
            PostpaidplanRequest PostpaidReq = JsonConvert.DeserializeObject<PostpaidplanRequest>(unbillamtrequest);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            decimal Totalvalue = 0;
            decimal BilledTotalvalue = 0;
            decimal remainingbillamt = 0;
            string returnurl;
            string keyframe = "?Keyframe=";
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - GETPostpaidUnbilledAmount Start");
                PostpaidReq.CountryCode = clientSetting.countryCode;
                PostpaidReq.BrandCode = clientSetting.brandCode;
                PostpaidReq.LanguageCode = clientSetting.langCode;
                //PostpaidReq.mode = "Get";              
                if (PostpaidReq.DebitMode == "APM")
                {
                    returnurl = Request.UrlReferrer.ToString().TrimEnd('?') + keyframe + Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + "NEW_POST" + "|" + PostpaidReq.mobileNumber + "|" + "ViewpostpaidPlandetails" + "|" + PostpaidReq.Bundlecode + "|" + "" + "|" + PostpaidReq.TotalAmount + "|" + PostpaidReq.unpaidamout + "|" + PostpaidReq.Creditusage + "|" + PostpaidReq.addonbundleamout + "|" + PostpaidReq.bundleamout + "|" + PostpaidReq.Terminationbilled + "|" + PostpaidReq.creditusagebilled + "|" + PostpaidReq.addonbundleamoutbilled + "|" + PostpaidReq.bundleamoutbilled;
                    PostpaidReq.ReturnUrl = returnurl;
                    PostpaidReq.EmailID = Session["eMailID"].ToString();
                }

                // 4885
                if (PostpaidReq.mode == "PostpaidpaymentViaLink" || clientSetting.preSettings.EnablePostpaidPaymentViaLink.ToUpper() == "TRUE")
                    PostpaidReq.EmailID = Session["eMailID"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                PostpaidResp = serviceCRM.PostpaidPlanDetails(PostpaidReq);
               if(PostpaidResp.responseDetails != null && PostpaidResp.responseDetails.ResponseCode == "0")
                {
                    if (string.IsNullOrEmpty(PostpaidResp.addonbundleamout))
                        PostpaidResp.addonbundleamout = "0.0";
                    if (string.IsNullOrEmpty(PostpaidResp.Creditusage))
                        PostpaidResp.Creditusage = "0.0";
                    if (string.IsNullOrEmpty(PostpaidResp.bundleamout))
                        PostpaidResp.bundleamout = "0.0";
                    if (string.IsNullOrEmpty(PostpaidResp.unpaidamout))
                        PostpaidResp.unpaidamout = "0.0";
                    Totalvalue = Convert.ToDecimal(PostpaidResp.addonbundleamout) + Convert.ToDecimal(PostpaidResp.Creditusage)  + Convert.ToDecimal(PostpaidResp.unpaidamout);
                    if (!string.IsNullOrEmpty(Totalvalue.ToString()))
                    {
                        PostpaidResp.Totalamount = Totalvalue.ToString();
                    }

                    // unpaidbilled balance

                    if (string.IsNullOrEmpty(PostpaidResp.addonbundleamoutbilled))
                        PostpaidResp.addonbundleamoutbilled = "0.0";
                    if (string.IsNullOrEmpty(PostpaidResp.creditusagebilled))
                        PostpaidResp.creditusagebilled = "0.0";
                    if (string.IsNullOrEmpty(PostpaidResp.bundleamoutbilled))
                        PostpaidResp.bundleamoutbilled = "0.0";
                    if (string.IsNullOrEmpty(PostpaidResp.Terminationbilled))
                        PostpaidResp.Terminationbilled = "0.0";
                      BilledTotalvalue = Convert.ToDecimal(PostpaidResp.addonbundleamoutbilled) + Convert.ToDecimal(PostpaidResp.creditusagebilled) + Convert.ToDecimal(PostpaidResp.bundleamoutbilled) + Convert.ToDecimal(PostpaidResp.Terminationbilled);
                    if (!string.IsNullOrEmpty(BilledTotalvalue.ToString()))
                    {
                        PostpaidResp.unpaidbalance = BilledTotalvalue.ToString();
                    }

                    if (string.IsNullOrEmpty(PostpaidResp.OriginalCreditusage))
                        PostpaidResp.OriginalCreditusage = "0.0";
                     remainingbillamt = Convert.ToDecimal(PostpaidResp.OriginalCreditusage) - Convert.ToDecimal(PostpaidResp.Creditusage);

                    if (!string.IsNullOrEmpty(remainingbillamt.ToString()))
                    {
                        PostpaidResp.Remainingbill = remainingbillamt.ToString();
                    }
                }

                if (PostpaidResp != null && PostpaidResp.responseDetails != null && PostpaidResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GETPostpaidUnbilledAmountRequestResponse_" + PostpaidResp.responseDetails.ResponseCode);
                    PostpaidResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? PostpaidResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - GETPostpaidUnbilledAmout End");
            }
            catch(Exception ex)
            {
                PostpaidResp.responseDetails.ResponseCode = "9";
                PostpaidResp.responseDetails.ResponseDesc = ex.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - GETPostpaidUnbilledAmount - " + this.ControllerContext, ex);
            }
            finally
            {

            }



            return new JsonResult() { Data = PostpaidResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }





     
        public ActionResult PostpaidAutonewal(string msisdn)
        {
            ViewBag.IsRedirect = TempData["PostPaidAutorenewal"];
            ViewBag.PostPaidAutorenewalBundleCode = TempData["PostPaidAutorenewalBundleCode"];
            ViewBag.PostPaidAutorenewalid = TempData["PostPaidAutorenewalid"];
            return View("PostPaidAutorenewal");
        }
   
        public JsonResult GetPostpaidAutonewal(string PostpaidplanAutorenewalRequest)
        {
            PostpaidAutorenewalPlanResponse PostpaidautoResp = new PostpaidAutorenewalPlanResponse();
            PostpaidplanAutorenewalRequest ObjReq = JsonConvert.DeserializeObject<PostpaidplanAutorenewalRequest>(PostpaidplanAutorenewalRequest);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
         
            string returnurl;
            string keyframe = "?Keyframe=";
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - CRMPostpaidAutorenewalDetails Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                // 4788 APM mode redirect

                if (ObjReq.DebitMode == "APM")
                {
                    returnurl = Request.UrlReferrer.ToString().TrimEnd('?') + keyframe + Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + "NEW_POST" + "|" + ObjReq.mobileNumber + "|" + "PostpaidAutonewal" + "|" + ObjReq.BundleCode + "|" + ObjReq.repeatCardNumber1 + "|" + ObjReq.TotalAmount;
                    ObjReq.Typeofpayment = returnurl;
                }




                PostpaidautoResp = serviceCRM.CRMPostpaidAutorenewalDetails(ObjReq);
                if (PostpaidautoResp != null && PostpaidautoResp.responseDetails != null && PostpaidautoResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMPostpaidAutorenewalDetailsRequestResponse_" + PostpaidautoResp.responseDetails.ResponseCode);
                    PostpaidautoResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? PostpaidautoResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - CRMPostpaidAutorenewalDetails End");


            }
            catch (Exception eX)
            {
                PostpaidautoResp.responseDetails.ResponseCode = "9";
                PostpaidautoResp.responseDetails.ResponseDesc = eX.Message;
                
            }
            finally
            {
                new LoginController().AuditTrailCRM(new AuditTrailRequest() { action = "Refund Payment", description = "Refund Payment Page Loaded", module = "Payment", subModule = "Refund", DescID = "RefundPayment_Page" });
                // paymentRefundResp = null;
            }
            return new JsonResult() { Data = PostpaidautoResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        public JsonResult PostpaidPlanPurchaseCRMEncodeDecode(string getpostpaidplanpurchase)
        {
            PostpaidPlanPurchaseResponse ObjResp = new PostpaidPlanPurchaseResponse();
            PostpaidPlanPurchaseRequest ObjReq = JsonConvert.DeserializeObject<PostpaidPlanPurchaseRequest>(getpostpaidplanpurchase);
            EncodeDecode objReq = new EncodeDecode();
            objReq.CountryCode = clientSetting.countryCode;
            objReq.BrandCode = clientSetting.brandCode;
            objReq.LanguageCode = clientSetting.langCode;
            string ErrorCode = string.Empty;
            string ErrorMsg = string.Empty;
            string TransactioID = string.Empty;
            string Orderid = string.Empty;
            EncodeDecoderes objRes = new EncodeDecoderes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objReq.returnUrl = ObjReq.IssuerId;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                objRes = serviceCRM.GetDecodeValue(objReq);
                ErrorCode = objRes.hashval.FindAll(a => a.ID == "ResponseCode").Count > 0 ? objRes.hashval.FindAll(a => a.ID == "ResponseCode")[0].Values : objRes.responseDetails.ResponseCode;
                ErrorMsg = objRes.hashval.FindAll(a => a.ID == "ResponseMessage").Count > 0 ? objRes.hashval.FindAll(a => a.ID == "ResponseMessage")[0].Values : objRes.responseDetails.ResponseDesc;
                TransactioID = objRes.hashval.FindAll(a => a.ID == "TransId").Count > 0 ? objRes.hashval.FindAll(a => a.ID == "TransId")[0].Values : string.Empty;
                Orderid = objRes.hashval.FindAll(a => a.ID == "PaymentOrderID").Count > 0 ? objRes.hashval.FindAll(a => a.ID == "PaymentOrderID")[0].Values : string.Empty;
                ObjResp.RescodeAPM = ErrorCode;
                ObjResp.ResmsgAPM = ErrorMsg;
                ObjResp.TranIdAPM = TransactioID;
                ObjResp.OrderIdAPM = Orderid;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "Registration - PostpaidPlanPurchase - " + this.ControllerContext, ex);
            }
            finally
            {
                objRes = null;
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }



        #endregion




        // FRR - 4618
       
        public ActionResult GetCampaignBundle()
        {
            return View("CampaignBundle");
        }
       [HttpPost]
       public JsonResult GetCampaignBundle(string GetCampaignBundleRequest)
       {
           #region FRR 4618 ***AugustBundle ****

           CampaignBundleResponse ObjResp = new CampaignBundleResponse();
           CampaignBundleRequest ObjReq = JsonConvert.DeserializeObject<CampaignBundleRequest>(GetCampaignBundleRequest);
           ServiceInvokeCRM serviceCRM;
           string errorInsertMsg = string.Empty;
           try
           {
               CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - GetCampaignBundleDetails Start");
               ObjReq.CountryCode = clientSetting.countryCode;
               ObjReq.BrandCode = clientSetting.brandCode;
               ObjReq.LanguageCode = clientSetting.langCode;
              if(string.IsNullOrEmpty(ObjReq.Applicablechannels))
               ObjReq.Applicablechannels="CRM";
               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
               ObjResp = serviceCRM.CRMGetCampaignBundleDetails(ObjReq);
               if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
               {
                   errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetCampaignBundleDetailsRequestResponse_" + ObjResp.responseDetails.ResponseCode);
                   ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
               }
               CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - GetCampaignBundleDetails End");

           }
           catch(Exception  ex)
           {
               CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - GetCampaignBundleDetails - " + this.ControllerContext, ex);
           }
           finally
           {
               ObjReq = null;
               serviceCRM = null;
               errorInsertMsg = string.Empty;
           }

           return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

           #endregion
       }

        #region FRR 4818
      
        public ActionResult PreloadBundleDetails()
        {
            return View("PreloadBundleDetails");
        }
    
    [HttpPost]
    public JsonResult GetPreloadBundleDetails(string GetCampaignBundleRequest)
    {


        PreloadBundleDetailsResponse ObjResp = new PreloadBundleDetailsResponse();
        PreloadBundleDetailsRequest ObjReq = JsonConvert.DeserializeObject<PreloadBundleDetailsRequest>(GetCampaignBundleRequest);
        ServiceInvokeCRM serviceCRM;
        string errorInsertMsg = string.Empty;
        try
        {
            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - GetPreloadBundleDetails Start");
            ObjReq.CountryCode = clientSetting.countryCode;
            ObjReq.BrandCode = clientSetting.brandCode;
            ObjReq.LanguageCode = clientSetting.langCode;        
            serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
            ObjResp = serviceCRM.CRMGetPreloadBundleDetails(ObjReq);

                if (ObjResp.PreloadBundleDetailsResponselist.Count > 0)
                {
                    ObjResp.PreloadBundleDetailsResponselist.FindAll(a => a.ActivationDate != string.Empty).ForEach(b => b.ActivationDate = Utility.GetDateconvertion(b.ActivationDate, "DD/MM/YYYY", false, clientSetting.mvnoSettings.dateTimeFormat.ToUpper()));
                }


                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetPreloadBundleDetails_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
    
            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - GetPreloadBundleDetails End");

        }
        catch (Exception ex)
        {
            CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - GetPreloadBundleDetails - " + this.ControllerContext, ex);
        }
        finally
        {
            ObjReq = null;
            serviceCRM = null;
            errorInsertMsg = string.Empty;
        }

        return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

       
    }
    #endregion


        #region FRR 4807

        public ViewResult ProlongBundle()
        {
            ProlongBundles Prolongdetails = new ProlongBundles();

            ProlongBundlesRespose Prolongres = new ProlongBundlesRespose();
            ProlongBundlesRequest Prolongreq = new ProlongBundlesRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;

            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ProlongBundle - ProlongBundle Start");
                Prolongreq.CountryCode = clientSetting.countryCode;
                Prolongreq.BrandCode = clientSetting.brandCode;
                Prolongreq.LanguageCode = clientSetting.langCode;
                Prolongreq.MSISDN = Session["MobileNumber"].ToString();
                Prolongreq.Mode = "GET";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Prolongres = serviceCRM.GetCRMProlongDetails(Prolongreq);

                if (Prolongres.responseDetails.ResponseCode == "0")
                {
                    Prolongdetails.BundleStatus = Prolongres.BundleStatus;
                    Prolongdetails.PrimaryBundleCode = Prolongres.PrimaryBundleCode;
                    Prolongdetails.PrimaryBundle = Prolongres.PrimaryBundle;
                    Prolongdetails.SMSServicetype = Prolongres.SMSServicetype;
                    Prolongdetails.SMSServiceStatus = Prolongres.SMSServiceStatus;
                    Prolongdetails.VoiceServiceStatus = Prolongres.VoiceServiceStatus;
                    Prolongdetails.VoiceServicetype = Prolongres.VoiceServicetype;
                    Prolongdetails.DataServicetype = Prolongres.DataServicetype;
                    Prolongdetails.DataServiceStatus = Prolongres.DataServiceStatus;
                }
                Prolongdetails.crmResponse = Prolongres.responseDetails;

                if (Prolongres != null && Prolongres.responseDetails.ResponseCode != null && Prolongres.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ProlongrequestResponse_" + Prolongres.responseDetails.ResponseCode);
                    Prolongres.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? Prolongres.responseDetails.ResponseDesc : errorInsertMsg;
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - History End");

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);

            }
            finally
            {
                serviceCRM = null;
                Prolongreq = null;
                errorInsertMsg = string.Empty;

            }
            return View(Prolongdetails);
        }



        [HttpPost]
        public JsonResult UpdateProlongBundle(string GetUpdateProlongBundle)
        {
            ProlongBundlesRespose Prolongres = new ProlongBundlesRespose();
            ProlongBundlesRequest Prolongreq = JsonConvert.DeserializeObject<ProlongBundlesRequest>(GetUpdateProlongBundle);
            string errorInsertMsg = string.Empty;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "UpdateProlongBundle - UpdateProlongBundle Start");
                Prolongreq.CountryCode = clientSetting.countryCode;
                Prolongreq.BrandCode = clientSetting.brandCode;
                Prolongreq.LanguageCode = clientSetting.langCode;
                Prolongreq.MSISDN = Session["MobileNumber"].ToString();

          

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Prolongres = serviceCRM.GetCRMProlongDetails(Prolongreq);

                if (Prolongres != null && Prolongres.responseDetails.ResponseCode != null && Prolongres.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UpdateProlongrequestResponse_" + Prolongres.responseDetails.ResponseCode);
                    Prolongres.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? Prolongres.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - UpdateCallBarring End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                Prolongreq = null;
                errorInsertMsg = string.Empty;
            }
            return Json(Prolongres);
        }


        #endregion


        #region FRR 4836
 
        public ActionResult SemiAutoVerification()
        {
            return View("SemiAutoVerification");
        }

        #endregion

        #region frr 4942 
        public ActionResult SetDataThreshold()
        {
            return View("SetDataThreshold");
        }
        public JsonResult CRMGetRoamingDataThreshold(DataUsageSettings threshReq)
        {

            DataUsageSettingsResponse threshResp = new DataUsageSettingsResponse();
            SetDataThresholdResponse viewResp = new SetDataThresholdResponse();
            DataUsageSettings jsonReq = new DataUsageSettings();
            ServiceInvokeCRM serviceCRM;
            try
            {
                jsonReq.CountryCode = clientSetting.countryCode;
                jsonReq.BrandCode = clientSetting.brandCode;
                jsonReq.LanguageCode = clientSetting.langCode;

                if (jsonReq.MSISDN != null)
                {
                    Session["MobileNumber"] = jsonReq.MSISDN;
                    jsonReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                }
                else
                {
                    jsonReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                   
                }
               

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    threshResp = serviceCRM.CRMGetRoamingDataThreshold(jsonReq);
                    if (threshResp.queryDataThreshold.RETURNCODE == "0")
                    {
                        viewResp.ResponseCode = threshResp.queryDataThreshold.RETURNCODE;
                        viewResp.ResponseDesc = threshResp.queryDataThreshold.ERRDESCRITION;
                        viewResp.DataThresholdList = threshResp.queryDataThreshold.DataThresholdList;
                        viewResp.CurrentThreshValue = threshResp.queryDataThreshold.CURRENT_DATA_THRESH_VALUE;
                        viewResp.RoamDataMaxLimit = threshResp.queryDataThreshold.ROAM_DATA_MAX_LIMIT;
                    //6473
                    viewResp.UsedThreshValue = threshResp.queryDataThreshold.USED_DATA_THRESH_VALUE;

                    }
                    
                
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(viewResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                threshResp = null;
                jsonReq = null;
                serviceCRM = null;
            }
            return Json(viewResp);
        }

        public JsonResult CRMSetDataThreshold(DataUsageSettings threshReq)
        {

            DataUsageSettingsResponse threshResp = new DataUsageSettingsResponse();
            SetDataThresholdResponse viewResp = new SetDataThresholdResponse();
            DataUsageSettings jsonReq = new DataUsageSettings();
            ServiceInvokeCRM serviceCRM;
            try
            {
                jsonReq.CountryCode = clientSetting.countryCode;
                jsonReq.BrandCode = clientSetting.brandCode;
                jsonReq.LanguageCode = clientSetting.langCode;

                if (jsonReq.MSISDN != null)
                {
                    Session["MobileNumber"] = jsonReq.MSISDN;
                    jsonReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                }
                else
                {
                    jsonReq.MSISDN = Convert.ToString(Session["MobileNumber"]);   
                }
                jsonReq.roamDataLimit = threshReq.roamDataLimit;

               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    threshResp = serviceCRM.CRMSetDataThreshold(jsonReq);
                    if (threshResp.manageDataThreshold.RETURNCODE == "0")
                    {
                       viewResp.ResponseCode = threshResp.manageDataThreshold.RETURNCODE;
                       viewResp.ResponseDesc = threshResp.manageDataThreshold.ERRDESCRITION;
                       viewResp.CurrentThreshValue = threshResp.manageDataThreshold.ROAM_DATA_LIMIT;
                    //6473
                    viewResp.UsedThreshValue = threshResp.manageDataThreshold.ROAM_CONSUMPTION;
                    }
                    else if (!string.IsNullOrEmpty(threshResp.responseDetails.ResponseCode))
                    {
                        viewResp.ResponseCode = threshResp.responseDetails.ResponseCode;
                    }
                   
                
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(viewResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                threshResp = null;
                jsonReq = null;
                serviceCRM = null;
            }
            return Json(viewResp);
        }

        #endregion

  
  
  #region FRR 4994
        public ActionResult DigitalInvoice()
        {
            return View("DigitalInvoice");
        }

        public JsonResult SetGenerateInvoice(CRMSetGenerateInvoiceReq GenerateInvoiceReq)
        {

            CRMSetGenerateInvoiceRes generateInvRes = new CRMSetGenerateInvoiceRes();
            CRMSetGenerateInvoicetoView viewResp = new CRMSetGenerateInvoicetoView();
            CRMSetGenerateInvoiceReq generateInvReq = new CRMSetGenerateInvoiceReq();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - SubmitGenerateInvoice Start");
                GenerateInvoiceReq.CountryCode = clientSetting.countryCode;
                GenerateInvoiceReq.BrandCode = clientSetting.brandCode;
                GenerateInvoiceReq.LanguageCode = clientSetting.langCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                generateInvRes = serviceCRM.GenerateInvoiceDetail(GenerateInvoiceReq);
                //   viewResp.ResponseCode = "0";
                if (generateInvRes.Response != null && generateInvRes.Response.ResponseCode == "0")
                {
                    viewResp.ResponseCode = generateInvRes.Response.ResponseCode;

                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - SubmitGenerateInvoice End");
            }
            catch (Exception ex)
            {
                generateInvRes.Response.ResponseCode = "9";
                generateInvRes.Response.ResponseDesc = ex.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - SubmitGenerateInvoice - " + this.ControllerContext, ex);
            }
            finally
            {

            }
            //viewResp.Response.ResponseCode = "0";
            return Json(viewResp);
        }



        public JsonResult DigitalInvoiceReport(CRMSetGenerateInvoiceReq DigitalInvRptreq)
        {
            CRMSetGenerateInvoiceRes DigitalInvRptres = new CRMSetGenerateInvoiceRes();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            string errorInsert = string.Empty;
            string[] strSplit;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - Json DigitalInvoiceReport Start");
                DigitalInvRptreq.CountryCode = clientSetting.countryCode;
                DigitalInvRptreq.BrandCode = clientSetting.brandCode;
                DigitalInvRptreq.LanguageCode = SettingsCRM.langCode;              

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                DigitalInvRptres = serviceCRM.DigitalInvRport(DigitalInvRptreq);
                
                try
                {
                    //DigitalInvRptres.auditTrailReport.ForEach(atr => atr.actionDate = Utility.FormatDateTime(atr.actionDate, clientSetting.mvnoSettings.dateTimeFormat));
                }
                catch (Exception exDateFormat)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exDateFormat);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - Json DigitalInvoiceReport End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                serviceCRM = null;
                errorInsertMsg = string.Empty;
                errorInsert = string.Empty;
                strSplit = null;
            }
            return new JsonResult() { Data = DigitalInvRptres, MaxJsonLength = int.MaxValue };
        }
		
		 [HttpPost]
        public JsonResult CRMLoadDigitalInvoice(LoadDigitalInvoiceReq objReq)
        {
            LoadDigitalInvoiceRes reportResp = new LoadDigitalInvoiceRes();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            string errorInsert = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - Json AuditTrailReport Start");
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = SettingsCRM.langCode;

                objReq.fromdate = Utility.FormatDateTimeToService(objReq.fromdate, clientSetting.mvnoSettings.dateTimeFormat);
                objReq.todate = Utility.FormatDateTimeToService(objReq.todate, clientSetting.mvnoSettings.dateTimeFormat);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                reportResp = serviceCRM.CRMLoadDigitalInvoice(objReq);
               
                if(reportResp.LoadList != null && reportResp.LoadList.Count>0)
                {
                    reportResp.LoadList.ForEach(bundle => bundle.PurchDateTime = (!string.IsNullOrEmpty(bundle.PurchDateTime)) ? DateTime.ParseExact((bundle.PurchDateTime.Length == 17) ? bundle.PurchDateTime.Remove(14) : bundle.PurchDateTime, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString() : "");
                                       
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - Json AuditTrailReport End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                serviceCRM = null;
                errorInsertMsg = string.Empty;
                errorInsert = string.Empty;
            }
            return new JsonResult() { Data = reportResp.LoadList, MaxJsonLength = int.MaxValue };
        }



        #region FRR 4994 
        public void DownLoadDigitalInvoiceReport(string DigitalInvoiceReport)
        {
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadAuditTrailReport Start");
                GridView gridView = new GridView();
                List<DigitalInvoiceReport> lstDigitalInvoiceReport = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<DigitalInvoiceReport>>(DigitalInvoiceReport);

                // List<AuditTrailDetail> lstAuditTrailDetails = new List<AuditTrailDetail>();

                List<DigitalInvoiceReport> lstDigitalInvoiceReportlist = lstDigitalInvoiceReport.Where(m => m.UID != null)
                 .Select(m => new DigitalInvoiceReport
                 {
                     Channel = m.Channel,
                     Formapago = m.Formapago,
                     UsoCFDI = m.UsoCFDI,
                     RegimenFiscal = m.RegimenFiscal,
                     DomicilioFiscal = m.DomicilioFiscal,
                     CodigoPostal = m.CodigoPostal,
                     Email = m.Email,
                     MobileNumber = m.MobileNumber,
                     TransactionReferenceNumber = m.TransactionReferenceNumber,
                     RetailPrice = m.RetailPrice,
                     BundleName = m.BundleName
                 }).ToList();

                gridView.DataSource = lstDigitalInvoiceReportlist;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "DigitalInvoiceReport", this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadDigitalInvoiceReport End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
        }
        #endregion




        #endregion

        #region FRR-4890

        public ActionResult ManageRefferalCode()
        {
            return View("ManageRefferalCode");
        }

        public JsonResult RefferalCode(string Refferalcoderequest)
        {

            ManageRefferalcodeReq ObjReq = JsonConvert.DeserializeObject<ManageRefferalcodeReq>(Refferalcoderequest);
            ManageRefferalcodeResp ObjResp = new ManageRefferalcodeResp();
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetPortoutRefundReport Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.CRMManageRefferalCode(ObjReq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - EuronetpromotionRequestResponse End");

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-EuronetpromotionRequestResponse - " + this.ControllerContext, ex);

            }
            finally
            {
                ObjReq = null;
            }

            return Json(ObjResp, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region FRR 4739
        public ActionResult ViewOBAUsageDetails()
        {
            return View("ViewOBAUsageDetails");
        }

        [HttpPost]
        public JsonResult GetOBABundleDetails(string GetBundleinfo)
        {


            OBABundleDetailsResponse ObjResp = new OBABundleDetailsResponse();
            OBABundleDetailsRequest ObjReq = JsonConvert.DeserializeObject<OBABundleDetailsRequest>(GetBundleinfo);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - GetOBABundleDetails Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.CRMGetOBABundleDetails(ObjReq);

               

                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetOBABundleDetails_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - GetOBABundleDetails End");

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - GetOBABundleDetails - " + this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }

            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };


        }


        #endregion

       
        #region FRR-6344
        public ActionResult Bundleoperations()
        {
            BundleOperationsResp Objresp = new BundleOperationsResp();
            BundleOperationsReq Objreq = new BundleOperationsReq();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - Bundleoperations Start");
                Objreq.CountryCode = clientSetting.countryCode;
                Objreq.BrandCode = clientSetting.brandCode;
                Objreq.LanguageCode = clientSetting.langCode;
                Objreq.Msisdn = Session["MobileNumber"].ToString();
                Objreq.mobileNumber = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objresp = serviceCRM.CRMBundleoperations(Objreq);
                Objresp.DecimalLimit = Convert.ToInt32(clientSetting.mvnoSettings.decimalLimit);

                if (Objresp.BundlePurchaseFailedInfo != null && Objresp.BundlePurchaseFailedInfo.Count > 0)
                {
                    Objresp.BundlePurchaseFailedInfo.FindAll(a => !string.IsNullOrEmpty(a.FAILED_DATE)).ForEach(b => b.FAILED_DATE = Utility.GetDateconvertion(b.FAILED_DATE, "DD-MM-YYYY", false, clientSetting.mvnoSettings.dateTimeFormat));
                }

                if (Objresp.BundleAutoFailedInfo != null && Objresp.BundleAutoFailedInfo.Count > 0 && Objresp.BundleAutoFailedInfo[0].RENEWAL_MODE == "Credit Card")
                {
                    Objresp.BundleAutoFailedInfo.ForEach(b => b.FAILED_DATE = !string.IsNullOrEmpty(b.FAILED_DATE) ? DateTime.ParseExact(b.FAILED_DATE, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString() : "");
                }
                else
                {
                    Objresp.BundleAutoFailedInfo.FindAll(a => !string.IsNullOrEmpty(a.FAILED_DATE)).ForEach(b => b.FAILED_DATE = Utility.GetDateconvertion(b.FAILED_DATE, "DD-MM-YYYY", false, clientSetting.mvnoSettings.dateTimeFormat));
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - Bundleoperations End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - Bundleoperations - " + this.ControllerContext, ex);
            }
            finally
            {
                Objreq = null;
                serviceCRM = null;
            }

            return View(Objresp);
        }


        public JsonResult BundleoperationsSMS(BundleOperationsReq Objreq)
        {
            BundleOperationsResp Objresp = new BundleOperationsResp();           
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - Bundleoperations Start");
                Objreq.CountryCode = clientSetting.countryCode;
                Objreq.BrandCode = clientSetting.brandCode;
                Objreq.LanguageCode = clientSetting.langCode;
                Objreq.Msisdn = Session["MobileNumber"].ToString();
                Objreq.mobileNumber = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objresp = serviceCRM.CRMBundleoperations(Objreq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BundleController - Bundleoperations End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "BundleController - Bundleoperations - " + this.ControllerContext, ex);
            }
            finally
            {
                Objreq = null;
                serviceCRM = null;
            }

            return new JsonResult() { Data = Objresp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        #endregion

    }
}
