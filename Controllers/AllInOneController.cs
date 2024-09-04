using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CRM.Models;
using Newtonsoft.Json;
using ServiceCRM;


namespace CRM.Controllers
{
    [ValidateState]
    public class AllInOneController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();

        public ActionResult CSBundleandTopup()
        {
            AllinOne allinone = new AllinOne();
            AccountDetailsResponse accountDetailsResp = new AccountDetailsResponse();
            AccountDetailsRequest accountDetailsReq = new AccountDetailsRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CSBundleandTopup Start");
                accountDetailsReq.BrandCode = clientSetting.brandCode;
                accountDetailsReq.CountryCode = clientSetting.countryCode;
                accountDetailsReq.LanguageCode = clientSetting.langCode;
                accountDetailsReq.MSISDN = Session["MobileNumber"].ToString();
                //4937
                accountDetailsReq.AtrID = Session["ATR_ID"].ToString();
                //4888
                
                    if (Session["ALLINONEBUNDLECODE"] != null)
                {
                    accountDetailsReq.sessionbundlecode = Session["ALLINONEBUNDLECODE"].ToString();
                    }

               
                //6218
                DataSet ds = Utility.BindXmlFile("~/App_Data/CountryListSepaCheckout.xml");
                allinone.objcountrydd = new Models.CountryDropdown();
                List<Dropdown> objLstDrop = new List<Dropdown>();
                Dropdown ObjDrop = new Dropdown();
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
                allinone.objcountrydd.CountryDD = objLstDrop;



                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    accountDetailsResp = serviceCRM.GetAccountDetails(accountDetailsReq);

                    //4888
                    if(accountDetailsResp.redirectallinone != null)
                    {
                        allinone.redirectallione = accountDetailsResp.redirectallinone;
                    }

                    if (accountDetailsResp.Response != null)
                    {
                        if (accountDetailsResp.Response.ResponseCode == "0")
                        {
                            //Session["MobileNumber"] = accountDetailsResp.MSISDN;
                            Session["PlanId"] = accountDetailsResp.PLANID;
                            Session["SIMSTATUS"] = accountDetailsResp.SIMSTATUS;
                            Session["SUBSTATUS"] = accountDetailsResp.SUBSTATUS;
                            Session["SUBSTYPE"] = accountDetailsResp.SUBSTYPE;
                            Session["LIFECYCLESTATE"] = accountDetailsResp.LIFECYCLESTATE;
                            Session["DISCOUNT_CODE_AVAILABLE"] = accountDetailsResp.DISCOUNT_CODE_AVAILABLE;
                            Session["TopupInd"] = accountDetailsResp.TOPUPIND;
                            allinone.LOAN_BALANCE = accountDetailsResp.LOAN_BALANCE;
                            allinone.LOAN_EXPIRY_DATE = accountDetailsResp.LOAN_EXPIRY_DATE;
                            allinone.LOAN_OUTSTANDING = accountDetailsResp.LOAN_OUTSTANDING;
                            allinone.MAINBALANCE = accountDetailsResp.MAINBALANCE;
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
                            invoicereq.Msisdn = Session["MobileNumber"].ToString();
                            invoiceres = serviceCRM.InvoiceAddressCRM(invoicereq);
                            allinone.PdfAddress = invoiceres.contentPDF;
                            allinone.EmailLanguage = invoiceres.EmailLanguage;
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
                                allinone.EmailLanguage = invoiceres.EmailLanguage;
                                allinone.PdfAddress = invoiceres.contentPDF;
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

                

                CRMResponse objRes = new CRMResponse();



                //4937
                if (clientSetting.preSettings.EnablePrimarySupplimentary.ToUpper() == "TRUE" && clientSetting.preSettings.EnableAltanIntegration.ToUpper() =="TRUE")
                {
                    allinone.BDMode = accountDetailsResp.Response.Mode;
                    allinone.primaryandSupplementarybundles = accountDetailsResp.primaryandSupplementarybundle;
                    allinone.responsedetails = accountDetailsResp.responseDetails;
                }

                // 4772 allinone
                if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
                {

                if (clientSetting.preSettings.allowonlyregcusforallinone.ToUpper() == "TRUE")
                {
                    objRes = Utility.checkValidSubscriber("", clientSetting);
                }
                else
                {
                    objRes = Utility.checkValidSubscriber("1", clientSetting);
                }
                }
                else
                {
                    allinone.ResponseCode = "0";
                }



                if (Session["isRetailer"] != null && Convert.ToString(Session["isRetailer"]) == "0")
                {
                    objRes.ResponseCode = "2";
                    objRes.ResponseDesc = "The subscrber is white listed";
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CSBundleandTopup_AllinOnePurchase_" + objRes.ResponseCode);
                    objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;
                }
                // 4772 allinone
                if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
                {

                if (objRes.ResponseCode == "0")
                {
                    allinone.ResponseCode = "0";

                }
                else
                {
                    allinone.ResponseCode = "1";
                    allinone.ResponseDescription = objRes.ResponseDesc;
                }
                }
                allinone.SWEdrpdown = Utility.GetDropdownMasterFromDB("23,30", Convert.ToString(Session["isPrePaid"]), "drop_master");
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CSBundleandTopup End");
                return View(allinone);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "AllInOneController - CSBundleandTopup - exp - " + this.ControllerContext, exp);
                return View(allinone);
            }
            finally
            {
                //allinone = null;
                accountDetailsResp = null;
                accountDetailsReq = null;
                serviceCRM = null;
               
            }
            
        }

        public ActionResult CRMGetBundles(string Allinone)
        {
            GetBundlesRes ObjRes = new GetBundlesRes();
            GetBundlesReq objbundleReq = new JavaScriptSerializer().Deserialize<GetBundlesReq>(Allinone);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMGetBundles Start");
                objbundleReq.SIM_CATEGORY = Session["SIM_CATEGORY"].ToString();
                objbundleReq.CountryCode = clientSetting.countryCode;
                objbundleReq.BrandCode = clientSetting.brandCode;
                objbundleReq.LanguageCode = clientSetting.langCode;
                // 4772 allinone
                if (clientSetting.mvnoSettings.ATRIDALLOWCHECK.ToLower() == "true")
                {
                    objbundleReq.PlanId = Session["ATR_ID"].ToString();
                }

                objbundleReq.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetBundles(objbundleReq);
                if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CSBundleandTopup_GetBundle_" + ObjRes.Response.ResponseCode);
                    ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMGetBundles End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "AllInOneController - CRMGetBundles - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                objbundleReq = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }


        // FRR 4724

        public ActionResult CRMGETAPMModes(string GetAPMReqDetails)
        {
            GetAPMRespose ObjRes = new GetAPMRespose();
            GetAPMRequest objReq = new JavaScriptSerializer().Deserialize<GetAPMRequest>(GetAPMReqDetails);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMGETAPMModes Start");      
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGETAPMModes(objReq);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMGETAPMModes End");
            }
            catch(Exception ex) 
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "AllInOneController - CRMGETAPMModes - ex - " + this.ControllerContext, ex);

            }
            finally
            {
                serviceCRM = null;
                objReq = null;
                errorInsertMsg = string.Empty;
            }

            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }




        public ActionResult CRMTopupvalidationRequest(string topupvalue)
        {
            TopupvalidateResponse ObjRes = new TopupvalidateResponse();
            TopupvalidationRequest objReq = new JavaScriptSerializer().Deserialize<TopupvalidationRequest>(topupvalue);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMGETAPMModes Start");
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMTopupvalidationRequest(objReq);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CSBundleandTopup_GetBundle_" + ObjRes.Response.ResponseCode);
                    ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMGETAPMModes End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "AllInOneController - CRMGETAPMModes - ex - " + this.ControllerContext, ex);

            }
            finally
            {
                serviceCRM = null;
                objReq = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }




        // End FRR 4724

        public ActionResult CRMReservebundle()
        {
            ReservebundleResponse ObjRes = new ReservebundleResponse();
            ReservebundleRequest objbundleReq = new ReservebundleRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMReservebundle Start");
                objbundleReq.CountryCode = clientSetting.countryCode;
                objbundleReq.BrandCode = clientSetting.brandCode;
                objbundleReq.LanguageCode = clientSetting.langCode;
                objbundleReq.MSISDN = Session["MobileNumber"].ToString();
                objbundleReq.PlanID = Session["PlanId"].ToString() == null ? "" : Session["PlanId"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMReservebundle(objbundleReq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMReservebundle End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "AllInOneController - CRMReservebundle - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                objbundleReq = null;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }


        public ActionResult CRMQueruBundleSubscription(string QueryBundle)
        {
            QueryBundleSubscriptionRes ObjRes = new QueryBundleSubscriptionRes();
            QueryBundleSubscriptionReq objbundleReq = new JavaScriptSerializer().Deserialize<QueryBundleSubscriptionReq>(QueryBundle);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMQueruBundleSubscription Start");
                objbundleReq.CountryCode = clientSetting.countryCode;
                objbundleReq.BrandCode = clientSetting.brandCode;
                objbundleReq.LanguageCode = clientSetting.langCode;
                if(string.IsNullOrEmpty(objbundleReq.simregwithusa))
                {
                objbundleReq.MSISDN = Session["MobileNumber"].ToString();
                }
              
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMQueruBundleSubscription(objbundleReq);
                //#region 6563
                //RULES_INFO rULES_INFO = new RULES_INFO();
                //rULES_INFO.Action = "ADD";
                //rULES_INFO.Type = "BUNDLE_ACTIVATION";
                //rULES_INFO.ACTIVATION_POSSIBLE = "1";
                //rULES_INFO.RESERVATION_POSSIBLE = "1";
                //ObjRes.RULES_APPLICABLE = "1";
                //ObjRes.rules_info = rULES_INFO;
                //#endregion
                if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CSBundleandTopup_QueruBundleSubs_" + ObjRes.Response.ResponseCode);
                    ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMQueruBundleSubscription End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "AllInOneController - CRMQueruBundleSubscription - ex - " + this.ControllerContext, ex);
            }
            finally
            {
               serviceCRM = null;
               objbundleReq = null;
               errorInsertMsg = string.Empty;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CRMQueruBundleSubscriptionChild(string QueryBundle)
        {
            QueryBundleSubscriptionRes ObjRes = new QueryBundleSubscriptionRes();
            QueryBundleSubscriptionReq objbundleReq = new JavaScriptSerializer().Deserialize<QueryBundleSubscriptionReq>(QueryBundle);
            ServiceInvokeCRM serviceCRM;
            try
            {
                objbundleReq.CountryCode = clientSetting.countryCode;
                objbundleReq.BrandCode = clientSetting.brandCode;
                objbundleReq.LanguageCode = clientSetting.langCode;
                
                //objbundleReq.MSISDN = Session["MobileNumber"].ToString();
              serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMQueruBundleSubscription(objbundleReq);

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
                serviceCRM = null;
                //ObjRes = null;
            }
            
        }
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult CRMAllinOnePurchase(string QueryBundle)
        {
            AllinOnePurchaseReq objReq = new AllinOnePurchaseReq();
            AllinOnePurchaseRes ObjRes = new AllinOnePurchaseRes();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
             
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMAllinOnePurchase Start");
                objReq = JsonConvert.DeserializeObject<AllinOnePurchaseReq>(QueryBundle);
                objReq.BrandCode = clientSetting.brandCode;
                objReq.CountryCode = clientSetting.countryCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.SIM_CATEGORY = Convert.ToString(Session["SIM_CATEGORY"]);
                //6632
                objReq.Subscriberlangcode = Convert.ToString(Session["SubscriberLanguageCode"]);
                if (!string.IsNullOrEmpty(objReq.PaymentMode) && objReq.PaymentMode == "3" || clientSetting.mvnoSettings.eshopCountryCode == "MEX")
                {
                    objReq.WpPayment.UserAgentHeader = Utility.UseragentAPM("All in One");
                }

                if (objReq.PaymentMode != "GET_PURCHASE_OFFER" && objReq.PaymentMode != "GET_RENEWAL_OFFER" && objReq.PaymentMode != "GET_PURCHASERENEWAL_OFFER" && objReq.PaymentMode != "PortOut_Validate")
                {
                
                        if (!string.IsNullOrEmpty(objReq.CardDetails.ConsentDate))
                        {
                            objReq.CardDetails.ConsentDate = Utility.GetDateconvertion(objReq.CardDetails.ConsentDate, "MM/DD/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                        }
                    
                }
                if (clientSetting.mvnoSettings.CaptureIPAddress.ToUpper() == "ON")
                {
                    objReq.DeviceInfo = Utility.DeviceInfo("All in One");
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMAllinOnePurchase(objReq);
                if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CSBundleandTopup_AllinOnePurchase_" + ObjRes.Response.ResponseCode);
                    ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;

                    if (ObjRes.Response.ResponseCode.Split('_')[0] == "22000")
                        ObjRes.Response.ResponseCode = "22000";
                    

                    if (!string.IsNullOrEmpty(objReq.PRTVatNo) && Convert.ToString(Session["Verify"]) == "1" && clientSetting.mvnoSettings.EnablePortugalVATScope != "0" && (ObjRes.Response.ResponseCode == "0" || ObjRes.Response.ResponseCode == "100" || ObjRes.Response.ResponseCode == "110" || ObjRes.Response.ResponseCode == "111"))
                    {
                        Session["InvoiceVatNo"] = objReq.PRTVatNo;
                    }
                    ObjRes.DateTime = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                    ObjRes.DateTime = ObjRes.DateTime.Replace("DD", Convert.ToString(System.DateTime.Now.Day));
                    ObjRes.DateTime = ObjRes.DateTime.Replace("MM", Convert.ToString(Convert.ToInt32(System.DateTime.Now.Month) - 1));
                    ObjRes.DateTime = ObjRes.DateTime.Replace("YYYY", Convert.ToString(System.DateTime.Now.Year));
                    ObjRes.DateTime = ObjRes.DateTime + " " + Convert.ToString(System.DateTime.Now.Hour) + ":" + Convert.ToString(System.DateTime.Now.Minute) + ":" + Convert.ToString(System.DateTime.Now.Second);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMAllinOnePurchase End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "AllInOneController - CRMAllinOnePurchase - exp - " + this.ControllerContext, exp);
            }
            finally
            {
                objReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CRMGetTopupDiscountAmount(string QueryBundle)
        {
            GetTopupDiscountAmountRes ObjRes = new GetTopupDiscountAmountRes();
            GetTopupDiscountAmountReq objbundleReq = new JavaScriptSerializer().Deserialize<GetTopupDiscountAmountReq>(QueryBundle);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMGetTopupDiscountAmount Start");
                objbundleReq.CountryCode = clientSetting.countryCode;
                objbundleReq.BrandCode = clientSetting.brandCode;
                objbundleReq.LanguageCode = clientSetting.langCode;
                objbundleReq.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetTopupDiscountAmount(objbundleReq);
                if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CSBundleandTopup_QueruBundleSubs_" + ObjRes.Response.ResponseCode);
                    ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMGetTopupDiscountAmount End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "AllInOneController - CRMGetTopupDiscountAmount - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                objbundleReq = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CRMGetCHECKTOPUPELIGIBILITY(string QueryBundle)
        {
            GetTopupDiscountAmountRes ObjRes = new GetTopupDiscountAmountRes();
            GetTopupDiscountAmountReq objbundleReq = new JavaScriptSerializer().Deserialize<GetTopupDiscountAmountReq>(QueryBundle);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMGetCHECKTOPUPELIGIBILITY Start");
                objbundleReq.CountryCode = clientSetting.countryCode;
                objbundleReq.BrandCode = clientSetting.brandCode;
                objbundleReq.LanguageCode = clientSetting.langCode;
                if (!string.IsNullOrEmpty(objbundleReq.MSISDN))
                {
                    objbundleReq.MSISDN = objbundleReq.MSISDN;
                }
                else
                {
                    objbundleReq.MSISDN = Session["MobileNumber"].ToString();
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGETCHECKTOPUPELIGIBILITY(objbundleReq);
                if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMGetCHECKTOPUPELIGIBILITY_" + ObjRes.Response.ResponseCode);
                    ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMGetCHECKTOPUPELIGIBILITY End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "AllInOneController - CRMGetCHECKTOPUPELIGIBILITY - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                objbundleReq = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }


        public ActionResult CRMMaxiumCreditCheck(string QueryBundle)
        {
            GetTopupDiscountAmountRes ObjRes = new GetTopupDiscountAmountRes();
            GetTopupDiscountAmountReq objbundleReq = new JavaScriptSerializer().Deserialize<GetTopupDiscountAmountReq>(QueryBundle);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMMaxiumCreditCheck Start");
                objbundleReq.CountryCode = clientSetting.countryCode;
                objbundleReq.BrandCode = clientSetting.brandCode;
                objbundleReq.LanguageCode = clientSetting.langCode;
                if (!string.IsNullOrEmpty(objbundleReq.MSISDN))
                {
                    objbundleReq.MSISDN = objbundleReq.MSISDN;
                }
                else
                {
                    objbundleReq.MSISDN = Session["MobileNumber"].ToString();
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMMaxiumCreditCheck(objbundleReq);
                if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMMaxiumCreditCheck_" + ObjRes.Response.ResponseCode);
                    ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult CRMMaxiumCreditCheck End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "AllInOneController - CRMMaxiumCreditCheck - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                objbundleReq = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult ProductTaxDetails(string ProductTaxInput, string page)
        {
            ProductTaxResponse ObjRes = new ProductTaxResponse();
            ProductTaxRequest objbundleReq = new JavaScriptSerializer().Deserialize<ProductTaxRequest>(ProductTaxInput);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult ProductTaxDetails Start");
                objbundleReq.CountryCode = clientSetting.countryCode;
                objbundleReq.BrandCode = clientSetting.brandCode;
                objbundleReq.LanguageCode = clientSetting.langCode;
                if (string.IsNullOrEmpty(page))
                {

                    if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                    {
                        Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                        objbundleReq.Msisdn = localDict.Where(x => objbundleReq.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();

                    }
                    else
                    {
                        objbundleReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                    }
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.ProductTaxDetails(objbundleReq);
                if (ObjRes != null && ObjRes.responseDetails != null && ObjRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RRBSProductTaxDetails_" + ObjRes.responseDetails.ResponseCode);
                    ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult ProductTaxDetails End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "AllInOneController - ProductTaxDetails - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                objbundleReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult MultipleProductTaxDetails(string ProductTaxInput, string page)
        {
            ProductTaxResponse ObjRes = new ProductTaxResponse();
            ProductTaxRequest objbundleReq = new JavaScriptSerializer().Deserialize<ProductTaxRequest>(ProductTaxInput);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult MultipleProductTaxDetails Start");
                objbundleReq.CountryCode = clientSetting.countryCode;
                objbundleReq.BrandCode = clientSetting.brandCode;
                objbundleReq.LanguageCode = clientSetting.langCode;
                if (string.IsNullOrEmpty(page))
                {
                    objbundleReq.Msisdn = Session["MobileNumber"].ToString();
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.MultipleProductTaxDetails(objbundleReq);
                if (ObjRes != null && ObjRes.responseDetails != null && ObjRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RRBSProductTaxDetails_" + ObjRes.responseDetails.ResponseCode);
                    ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult MultipleProductTaxDetails End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "AllInOneController - MultipleProductTaxDetails - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                objbundleReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult ReccommandedBundles(string ProductTaxInput)
        {
            RecommendedbundlesResp ObjRes = new RecommendedbundlesResp();
            RecommendedbundlesReq objbundleReq = new JavaScriptSerializer().Deserialize<RecommendedbundlesReq>(ProductTaxInput);
            ServiceInvokeCRM serviceCRM;
            List<Recommendedbundles> recommendedbundles = new List<Recommendedbundles>();
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult MultipleProductTaxDetails Start");
                objbundleReq.CountryCode = clientSetting.countryCode;
                objbundleReq.BrandCode = clientSetting.brandCode;
                objbundleReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
               
                for (int i = 0; i < 2; i++)
                {
                    Recommendedbundles recommendedbundles1 = new Recommendedbundles();
                    recommendedbundles1.Productname = "NormalBundle1";
                    recommendedbundles1.Productype = "BUNDLE";
                    recommendedbundles1.Bundlecode = "76767";
                    recommendedbundles1.Bundleamount = "10";
                    recommendedbundles.Add(recommendedbundles1);    
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AllInOneController - ActionResult MultipleProductTaxDetails End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "AllInOneController - MultipleProductTaxDetails - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                objbundleReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }


        #region Flexibuundle POF-6046

        public JsonResult FlexibundlePurchase(FlexibundleReq Objreq)
        {
            FlexibundleResp Objresp = new FlexibundleResp();
            ServiceInvokeCRM serviceCRM;
            try
            {
                Objreq.CountryCode = clientSetting.countryCode;
                Objreq.BrandCode = clientSetting.brandCode;
                Objreq.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Allinone - FlexibundlePurchase Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                Objresp = serviceCRM.CRMFlexibundle(Objreq);
              
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Allinone - Activebatchbulkupload End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "Allinone - exception-Activebatchbulkupload - " + this.ControllerContext, ex);
            }
            finally
            {
                Objreq = null;
            }
            return Json(Objresp, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ModifyBundleAllowance()
        {
            FlexibundleReq Objreq = new FlexibundleReq();
            FlexibundleResp Objresp = new FlexibundleResp();
            ServiceInvokeCRM serviceCRM;
            try
            {
                Objreq.CountryCode = clientSetting.countryCode;
                Objreq.BrandCode = clientSetting.brandCode;
                Objreq.LanguageCode = clientSetting.langCode;
                Objreq.Msisdn = Session["MobileNumber"].ToString();
                Objreq.Mode = "GetFlexibundles";
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Allinone - FlexibundlePurchase Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                Objresp = serviceCRM.CRMFlexibundle(Objreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Allinone - Activebatchbulkupload End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "Allinone - exception-Activebatchbulkupload - " + this.ControllerContext, ex);
            }
            finally
            {
                Objreq = null;
            }

            return View(Objresp);
        }

        #endregion
    }
}
