using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using CRM.Models;
using Newtonsoft.Json;
using ServiceCRM;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System.Net.Mail;
using System.Collections;
using System.Net;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace CRM.Controllers
{
    [ValidateState]
    public class TopupController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();
        ServiceCRM.ServiceInvokeCRM crmNewService = new ServiceCRM.ServiceInvokeCRM(Convert.ToString(SettingsCRM.crmServiceUrl));

        #region FRR 4925
        private string RealICCIDForMultiTab;
        #endregion
        public ActionResult History(string Textdata)
        {
            TopupHistory topupHistory = new TopupHistory();
            CRMActivationDateRequest actDateReq = new CRMActivationDateRequest();
            CRMActivationDateResponse actDateResp;
            
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller History start");
                actDateReq.CountryCode = clientSetting.countryCode;
                actDateReq.BrandCode = clientSetting.brandCode;
                actDateReq.LanguageCode = clientSetting.langCode;

                #region FRR 4925

                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                    Session["RealICCIDForMultiTab"] = Textdata;

                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    actDateReq.MSISDN = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    actDateReq.Iccid = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Key).First().ToString();
                   
                }
                else
                {
                    actDateReq.MSISDN = Session["MobileNumber"].ToString();
                    actDateReq.Iccid = Convert.ToString(Session["ICCID"]);
                }


                #endregion

                actDateResp = crmNewService.GetCRMActivationDate(actDateReq);
                topupHistory.topupDate = string.IsNullOrEmpty(actDateResp.activationDate) ? "" : Convert.ToDateTime(actDateResp.activationDate).ToString("yyyy/MM/dd");
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller History End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                actDateReq = null;
                actDateResp = null;
            }
            return View(topupHistory);
        }

        [HttpPost]
        public JsonResult LoadTopupHistory(TopupHistory topupHistory)
        {
            CRMTopupHistoryResponse crmTopupHistory = new CRMTopupHistoryResponse();
            CRMTopupHistoryRequest crmTopupHistReq = new CRMTopupHistoryRequest();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "LoadTopupHistory Start");
                crmTopupHistReq.CountryCode = clientSetting.countryCode;
                crmTopupHistReq.BrandCode = clientSetting.brandCode;
                crmTopupHistReq.LanguageCode = clientSetting.langCode;

                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    crmTopupHistReq.Msisdn = localDict.Where(x => topupHistory.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    crmTopupHistReq.OldNo = localDict.Where(x => topupHistory.textdata.ToString().Contains(x.Key)).Select(x => x.Value.SwapMSISDN).First().ToString();
                    crmTopupHistReq.ICCID = localDict.Where(x => topupHistory.textdata.ToString().Contains(x.Key)).Select(x => x.Key).First().ToString();
                }
                else
                {
                crmTopupHistReq.Msisdn = Session["MobileNumber"].ToString();
                    crmTopupHistReq.OldNo = Convert.ToString(Session["SwapMSISDN"]);
                    crmTopupHistReq.ICCID = Convert.ToString(Session["ICCID"]);
                }

              

                crmTopupHistReq.StartDate = Utility.GetDateconvertion(topupHistory.fromDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
                crmTopupHistReq.EndDate = Utility.GetDateconvertion(topupHistory.toDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
                

                crmTopupHistory = crmNewService.GetCRMTopupHistoryDetails(crmTopupHistReq);
                try
                {
                    crmTopupHistory.TopupHistoryDetails.Where(a => a.TopupDate != string.Empty).ToList().ForEach(b => b.TopupDate = Utility.FormatDateTime(b.TopupDate, clientSetting.mvnoSettings.dateTimeFormat));
                    crmTopupHistory.TopupHistoryDetails.Where(a => a.validity != string.Empty).ToList().ForEach(b => b.validity = Utility.FormatDateTime(b.validity, clientSetting.mvnoSettings.dateTimeFormat));
                    crmTopupHistory.TopupHistoryDetails.Where(a => a.Free_minutes_expiry_date != string.Empty).ToList().ForEach(b => b.Free_minutes_expiry_date = Utility.FormatDateTime(b.Free_minutes_expiry_date, clientSetting.mvnoSettings.dateTimeFormat));
                    crmTopupHistory.TopupHistoryDetails.Where(a => a.Free_SMS_expiry_date != string.Empty).ToList().ForEach(b => b.Free_SMS_expiry_date = Utility.FormatDateTime(b.Free_SMS_expiry_date, clientSetting.mvnoSettings.dateTimeFormat));
                    crmTopupHistory.TopupHistoryDetails.Where(a => a.PromoExpiry != string.Empty).ToList().ForEach(b => b.PromoExpiry = Utility.FormatDateTime(b.PromoExpiry, clientSetting.mvnoSettings.dateTimeFormat));
                    crmTopupHistory.TopupHistoryDetails.Where(a => a.Contract_start_date != string.Empty).ToList().ForEach(b => b.Contract_start_date = Utility.FormatDateTime(b.Contract_start_date, clientSetting.mvnoSettings.dateTimeFormat));
                    crmTopupHistory.TopupHistoryDetails.Where(a => a.Contract_end_date != string.Empty).ToList().ForEach(b => b.Contract_end_date = Utility.FormatDateTime(b.Contract_end_date, clientSetting.mvnoSettings.dateTimeFormat));
                    crmTopupHistory.TopupHistoryDetails.Where(a => a.CoolingExpiryDate != string.Empty).ToList().ForEach(b => b.CoolingExpiryDate = Utility.FormatDateTime(b.CoolingExpiryDate, clientSetting.mvnoSettings.dateTimeFormat));
                }
                catch (Exception exFormatDateTimeConversion)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exFormatDateTimeConversion);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "LoadTopupHistory End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                crmTopupHistReq = null;
            }
            return new JsonResult() { Data = crmTopupHistory, MaxJsonLength = int.MaxValue };
        }

        [HttpPost]
        public void DownLoadTopupHistory(string topupData, string hidedata)
        {
            GridView gridView = new GridView();
            string colNames = string.Empty;
            TopupHistoryDetails topupTemp = new TopupHistoryDetails();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "DownLoadTopupHistory Start");
                List<TopupHistoryDetails> topupHisDetails = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<TopupHistoryDetails>>(topupData);
                var props = topupTemp.GetType().GetProperties();
                List<string> hideCol = new List<string>(hidedata.Split(','));
                foreach (var prop in props)
                {                  
                    // DEFECT
                    if (prop.Name == "msisdn")
                    {
                        colNames = Resources.ResgisterPortInResources.ResourceManager.GetString(Convert.ToString(prop.Name));
                    }
                    else
                    {
                    colNames = Resources.TopupResources.ResourceManager.GetString(Convert.ToString(prop.Name));
                    }
                    if (!hideCol.Contains(prop.Name))
                    {
                        if (!string.IsNullOrEmpty(colNames))
                        {
                            BoundField bfield = new BoundField();
                            bfield.HeaderText = colNames;
                            bfield.DataField = prop.Name;
                            gridView.Columns.Add(bfield);
                        }
                        else
                        {
                        BoundField bfield = new BoundField();
                        bfield.HeaderText = prop.Name;
                        bfield.DataField = prop.Name;
                        gridView.Columns.Add(bfield);
                    }
                       
                    }
                }
                gridView.AutoGenerateColumns = false;
                gridView.DataSource = topupHisDetails;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "TopupHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "DownLoadTopupHistory End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                topupTemp = null;
                colNames = string.Empty;
            }
        }

        [HttpPost]
        public JsonResult TopupSummary(TopupHistory topupSummary)
        {
            TopupSummaryResponse topupSummaryResp = new TopupSummaryResponse();
            TopupSummaryRequest topupSummaryReq = new TopupSummaryRequest();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupSummary Start");
                topupSummaryReq.CountryCode = clientSetting.countryCode;
                topupSummaryReq.BrandCode = clientSetting.brandCode;
                topupSummaryReq.LanguageCode = clientSetting.langCode;

                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {

                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    topupSummaryReq.msisdn = localDict.Where(x => topupSummary.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    topupSummaryReq.oldNo = localDict.Where(x => topupSummary.textdata.ToString().Contains(x.Key)).Select(x => x.Value.SwapMSISDN).First().ToString();
                }
                else
                {
                    topupSummaryReq.oldNo = Convert.ToString(Session["SwapMSISDN"]);
                topupSummaryReq.msisdn = Session["MobileNumber"].ToString();
                }

                topupSummaryReq.fromDate = Utility.GetDateconvertion(topupSummary.fromDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
                topupSummaryReq.toDate = Utility.GetDateconvertion(topupSummary.toDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
               
                topupSummaryResp = crmNewService.TopupSummaryCRM(topupSummaryReq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupSummary End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                topupSummaryReq = null;
            }
            return Json(topupSummaryResp);
        }

        [HttpPost]
        public void DownLoadTopupFailureHistory(string topupData)
        {
            GridView gridView = new GridView();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "DownLoadTopupFailureHistory Start");
                //TopupFailure[] topupFailure = new JavaScriptSerializer().Deserialize<TopupFailure[]>(topupData);
                List<TopupFailure> topupFailure = JsonConvert.DeserializeObject<List<TopupFailure>>(topupData);
                gridView.DataSource = topupFailure;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "TopupFailureHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "DownLoadTopupFailureHistory End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
        }

        [HttpPost]
        public void DownLoadVoucherTopupFailureHistory(string topupvoucherData)
        {
            GridView gridView = new GridView();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "DownLoadVoucherTopupFailureHistory Start");
                //VoucherFailure[] topupFailure = new JavaScriptSerializer().Deserialize<VoucherFailure[]>(topupvoucherData);
                List<VoucherFailure> topupFailure = JsonConvert.DeserializeObject<List<VoucherFailure>>(topupvoucherData);
                gridView.DataSource = topupFailure;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "VoucherTopupFailureHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "DownLoadVoucherTopupFailureHistory End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
        }

        [HttpPost]
        public void DownLoadAutoTopupHistory(string topupvoucherData)
        {
            GridView gridView = new GridView();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "DownLoadAutoTopupHistory Start");
                AutoTopupStatusReport[] topupFailure = new JavaScriptSerializer().Deserialize<AutoTopupStatusReport[]>(topupvoucherData);
                gridView.DataSource = topupFailure;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "AutoTopupFailureHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "DownLoadAutoTopupHistory End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
        }

        [HttpPost]
        public void DownLoadAutoTopupFailureException(string topupvoucherData)
        {
            GridView gridView = new GridView();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "DownLoadAutoTopupFailureException Start");
                //AutoTopupFailure[] topupFailure = new JavaScriptSerializer().Deserialize<AutoTopupFailure[]>(topupvoucherData);
                List<AutoTopupFailure> topupFailure = JsonConvert.DeserializeObject<List<AutoTopupFailure>>(topupvoucherData);
                gridView.DataSource = topupFailure;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "AutoTopupFailureExceptionHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "DownLoadAutoTopupFailureException End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
        }


      
        public PartialViewResult Card(string Textdata)
        {
            CardTopup cardTopup = new CardTopup();
            CRMResponse objRes;
            CRMAutoTopupResponse objautopupres;
            AccountDetailsRequest objAccountDetailsRequest = new AccountDetailsRequest();
            AccountDetailsResponse objAccountDetailsResponse = new AccountDetailsResponse();
            InvoiceAddressRequest invoicereq = new InvoiceAddressRequest();
            InvoiceAddressResponse invoiceres = new InvoiceAddressResponse();

            string MSISDNretailer = string.Empty;
            string MSISNDvalidate = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController Card Start");
                objRes = Utility.checkValidSubscriber("", clientSetting);

                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {

                    Session["RealICCIDForMultiTab"] = Textdata;
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    MSISDNretailer = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.isRetailer).First().ToString();
                    MSISNDvalidate = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                }
                else
                {
                    if (Session["isRetailer"] != null)
                    {
                        MSISDNretailer = Session["isRetailer"].ToString();
                    }
                    MSISNDvalidate = Session["MobileNumber"].ToString();
                }




                if (MSISDNretailer != null && Convert.ToString(MSISDNretailer) == "0")
                {
                    objRes.ResponseCode = "2";
                    objRes.ResponseDesc = "The subscrber is white listed";
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("OnlineTopup_" + objRes.ResponseCode);
                    objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;
                }
                if (objRes.ResponseCode == "0")
                {
                    objautopupres = ScheduleTopupLoad("A");
                    if (objautopupres.ResponseDetails.ResponseCode == "0")
                    {
                        cardTopup.isautotopup = objautopupres.AutoTopup.isautotopup;
                        cardTopup.ismonthlyplan = objautopupres.AutoTopup.IsMonthlyPlan;
                    }
                    cardTopup.ResponseCode = "0";

                    objAccountDetailsRequest.CountryCode = clientSetting.countryCode;
                    objAccountDetailsRequest.BrandCode = clientSetting.brandCode;
                    objAccountDetailsRequest.LanguageCode = clientSetting.langCode;
                    objAccountDetailsRequest.MSISDN = MSISNDvalidate;
                    objAccountDetailsResponse = crmNewService.GetAccountDetails(objAccountDetailsRequest);
                    cardTopup.MAINBALANCE = objAccountDetailsResponse.MAINBALANCE;
                    cardTopup.LOAN_BALANCE = objAccountDetailsResponse.LOAN_BALANCE;
                    cardTopup.LOAN_EXPIRY_DATE = objAccountDetailsResponse.LOAN_EXPIRY_DATE;
                    cardTopup.LOAN_OUTSTANDING = objAccountDetailsResponse.LOAN_OUTSTANDING;

                    
                    /*****************************FRR-3485******************************/
                    invoicereq.BrandCode = clientSetting.brandCode;
                    invoicereq.CountryCode = clientSetting.countryCode;
                    invoicereq.LanguageCode = clientSetting.langCode;
                    invoicereq.Msisdn = MSISNDvalidate;
                    invoiceres = crmNewService.InvoiceAddressCRM(invoicereq);
                    cardTopup.InvPdfAddress = invoiceres.contentPDF;
                    cardTopup.EmailLanguage = invoiceres.EmailLanguage;
                    /*****************************FRR-3485******************************/


                    if (clientSetting.preSettings.AllowTopupCheck.ToUpper() == "ON")
                    {
                        ServiceInvokeCRM serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        GetTopupDiscountAmountRes ObjRes = new GetTopupDiscountAmountRes();
                        GetTopupDiscountAmountReq objbundleReq = new GetTopupDiscountAmountReq();
                        try
                        {
                            objbundleReq.CountryCode = clientSetting.countryCode;
                            objbundleReq.BrandCode = clientSetting.brandCode;
                            objbundleReq.LanguageCode = clientSetting.langCode;
                            objbundleReq.MSISDN = MSISNDvalidate;
                            objbundleReq.TopupAmount = "10";
                            ObjRes = serviceCRM.CRMGETCHECKTOPUPELIGIBILITY(objbundleReq);
                            if (ObjRes.Response.ResponseCode != "0")
                            {
                                cardTopup.ResponseCode = "1";
                                cardTopup.ResponseDescription = ObjRes.Response.ResponseDesc;
                            }
                        }
                        catch (Exception ex)
                        {
                            CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                        }
                        finally
                        {
                            serviceCRM = null;
                            ObjRes = null;
                            objbundleReq = null;
                        }
                    }
                }
                else
                {
                    cardTopup.ResponseCode = "1";
                    cardTopup.ResponseDescription = objRes.ResponseDesc;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController Card End");
           
            
            }
            catch (Exception exp)
            {
                cardTopup.ResponseCode = "1";
                cardTopup.ResponseDescription = exp.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                objRes = null;
                objautopupres = null;
                objAccountDetailsRequest = null;
                objAccountDetailsResponse = null;
                invoicereq = null;
                invoiceres = null;
            }
            return PartialView(cardTopup);
        }


        public ActionResult Settings()
        {
            return View();
        }

        public ActionResult Schedule()
        {
            ScheduleTopup objschedule = new ScheduleTopup();
            CRMAutoTopupResponse objautopupres;
            CRMResponse objRes;
            Session["Cardkey"] = null;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController Schedule Start");
                objautopupres = ScheduleTopupLoad("S");
                objschedule.lstCreditCardDetails = GetCreditCardDetails("S");

                //FRR 4617

                objschedule.lstDirectdebitCardDetails = GetDirectDebitCardDetails("S");


                objRes = Utility.checkValidSubscriber("", clientSetting);
                if (objRes.ResponseCode == "0")
                {
                    if (Session["Cardkey"] != null)
                    {
                        objschedule.CardTypeValue = Session["Cardkey"].ToString();
                    }
                    else
                    {
                        objschedule.CardTypeValue = "0";
                    }


                    if (objautopupres.AutoTopup != null)
                    {
                        if (objautopupres.AutoTopup.IsScheduleTopup == "1")
                        {
                            objschedule.IsScheduleTopup = true;
                        }
                        else
                        {
                            objschedule.IsScheduleTopup = false;
                        }
                        objschedule.SchTopupAmt = objautopupres.AutoTopup.SchTopupAmt;
                        objschedule.SchTopupDay = objautopupres.AutoTopup.SchTopupDay;
                        objschedule.SchTopupMode = objautopupres.AutoTopup.SchTopupMode;
                    }
                    objschedule.ResponseCode = "0";
                    objschedule.ResponseDescription = objRes.ResponseDesc;
                }
                else
                {
                    objschedule.ResponseCode = "1";
                    objschedule.ResponseDescription = objRes.ResponseDesc;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController Schedule End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                objautopupres=null;
                objRes=null;
            }
            return View(objschedule);
        }

        #region Schedule Topup
        //------------------------------Start - FRR3328--------------------------------
        public ActionResult ScheduleTopup()
        {
            ScheduleTopupRequest objScheduleTopupRequest = new ScheduleTopupRequest();
            ScheduleTopupResponse objschedule = new ScheduleTopupResponse();
            CRMResponse objRes = new CRMResponse();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ScheduleTopup Start");
                objschedule.ResponseDetails = new CRMResponse();
                #region MSISDN Validation
                if (clientSetting.preSettings.scheduleTopupRegisterMSISDN.ToUpper() == "ON")
                {
                    objRes = Utility.checkValidSubscriber("", clientSetting);
                }
                else
                {
                    objRes = Utility.checkValidSubscriber("1", clientSetting);
                }
                if (Session["isRetailer"] != null && Convert.ToString(Session["isRetailer"]) == "0")
                {
                    objRes.ResponseCode = "2";
                    objRes.ResponseDesc = "The subscrber is white listed";
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CSBundleandTopup_AllinOnePurchase_" + objRes.ResponseCode);
                    objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;
                }
                if (objRes != null && !String.IsNullOrEmpty(objRes.ResponseCode) && objRes.ResponseCode == "0")
                {
                    #region GET
                    objScheduleTopupRequest.MSISDN = Session["MobileNumber"].ToString();
                    objScheduleTopupRequest.Username = Session["UserName"].ToString();
                    objScheduleTopupRequest.Mode = "GET";
                    objScheduleTopupRequest.CountryCode = clientSetting.countryCode;
                    objScheduleTopupRequest.BrandCode = clientSetting.brandCode;
                    objScheduleTopupRequest.LanguageCode = clientSetting.langCode;
                    objschedule = crmNewService.CRMScheduleTopup(objScheduleTopupRequest);
                    if (objschedule != null && !String.IsNullOrEmpty(objschedule.ResponseDetails.ResponseCode) && objschedule.ResponseDetails.ResponseCode == "0")
                    {
                        #region Last Status
                        string[] LastTopupStausDetails = objschedule.LastTopupStatus.Split(',');
                        if (!string.IsNullOrEmpty(LastTopupStausDetails[0]) && !string.IsNullOrEmpty(LastTopupStausDetails[1]))
                            objschedule.LastTopupStatus = @Resources.TopupResources.LastTopuStatus + LastTopupStausDetails[0] + " , " + @Resources.TopupResources.NextTopupDate + LastTopupStausDetails[1];
                        else if (!string.IsNullOrEmpty(LastTopupStausDetails[0]) && string.IsNullOrEmpty(LastTopupStausDetails[1]))
                            objschedule.LastTopupStatus = @Resources.TopupResources.LastTopuStatus + LastTopupStausDetails[0];
                        else if (string.IsNullOrEmpty(LastTopupStausDetails[0]) && !string.IsNullOrEmpty(LastTopupStausDetails[1]))
                            objschedule.LastTopupStatus = @Resources.TopupResources.NextTopupDate + LastTopupStausDetails[1];
                        #endregion
                        objschedule.ResponseDetails.ResponseCode = "0";
                        objschedule.ResponseDetails.ResponseDesc = objRes.ResponseDesc;
                    }
                    else
                    {
                        objschedule.ResponseDetails.ResponseCode = "1";
                        objschedule.ResponseDetails.ResponseDesc = objRes.ResponseDesc;
                    }
                    #endregion
                }
                else
                {
                    objschedule.ResponseDetails.ResponseCode = objRes.ResponseCode;
                    objschedule.ResponseDetails.ResponseDesc = objRes.ResponseDesc;
                }
                #endregion
                #region Topup Amount get and Append
                objschedule.lstTopupAmonut = Utility.GetDropdownMasterFromDB("tbl_topup_amount");
                if (!String.IsNullOrEmpty(objschedule.TopUpAmount))
                {
                    if (!objschedule.lstTopupAmonut.Any(cs => cs.ID == objschedule.TopUpAmount))
                    {
                        DropdownMaster objDropdownMaster = new DropdownMaster();
                        //for (int i = 0; i < objschedule.lstTopupAmonut.Count(); i++)
                        //{
                        //    objDropdownMaster.ID = objschedule.lstTopupAmonut[i].Value;
                        //    objDropdownMaster.Value = objschedule.lstTopupAmonut[i].Value;
                        //}
                        objDropdownMaster.ID = objschedule.TopUpAmount;
                        objDropdownMaster.Value = objschedule.TopUpAmount;
                        objschedule.lstTopupAmonut.Add(objDropdownMaster);
                        if (clientSetting.mvnoSettings.EnableOthersAmount.ToUpper() == "ON")
                        {
                            objDropdownMaster = new DropdownMaster();
                            objDropdownMaster.ID = @Resources.TopupResources.AutoTopupOthers;
                            objDropdownMaster.Value = @Resources.TopupResources.AutoTopupOthers;
                            objschedule.lstTopupAmonut.Add(objDropdownMaster);
                        }
                    }
                }
                else
                {
                    if (clientSetting.mvnoSettings.EnableOthersAmount.ToUpper() == "ON")
                    {
                        DropdownMaster objDropdownMaster = new DropdownMaster();
                        objDropdownMaster.ID = @Resources.TopupResources.AutoTopupOthers;
                        objDropdownMaster.Value = @Resources.TopupResources.AutoTopupOthers;
                        objschedule.lstTopupAmonut.Add(objDropdownMaster);
                    }
                }
                #endregion
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ScheduleTopup End");
            }
            catch (Exception ex)
            {
                objschedule.ResponseDetails.ResponseCode = "1";
                objschedule.ResponseDetails.ResponseDesc = ex.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objScheduleTopupRequest = null;
                objRes = null;
            }
            return View(objschedule);
        }
        [HttpPost]
        public ActionResult SaveScheduleTopup(string strScheduleTopupRequest)
        {
            ScheduleTopupRequest objScheduleTopupRequest = new ScheduleTopupRequest(); 
            ScheduleTopupResponse objschedule = new ScheduleTopupResponse();
            CRMResponse objRes = new CRMResponse();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ScheduleTopup Start");
                objschedule.ResponseDetails = new CRMResponse();
                objScheduleTopupRequest = JsonConvert.DeserializeObject<ScheduleTopupRequest>(strScheduleTopupRequest);
                objScheduleTopupRequest.MSISDN = Session["MobileNumber"].ToString();
                objScheduleTopupRequest.Username = Session["UserName"].ToString();
                objScheduleTopupRequest.CountryCode = clientSetting.countryCode;
                objScheduleTopupRequest.BrandCode = clientSetting.brandCode;
                objScheduleTopupRequest.LanguageCode = clientSetting.langCode;

                if (objScheduleTopupRequest.CardDetails != null)
                {
                    objScheduleTopupRequest.CardDetails.ConsentDate = Utility.GetDateconvertion(objScheduleTopupRequest.CardDetails.ConsentDate, "MM/DD/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                }
                if (clientSetting.preSettings.scheduleTopupPaymentType == "1" && objScheduleTopupRequest.CardDetails != null && !string.IsNullOrEmpty(objScheduleTopupRequest.CardDetails.ExpiryDate))
                    objScheduleTopupRequest.CardDetails.ExpiryDate = objScheduleTopupRequest.CardDetails.ExpiryDate.Replace(" / ", "");


                if (clientSetting.mvnoSettings.CaptureIPAddress.ToUpper() == "ON")
                {
                    objScheduleTopupRequest.DeviceInfo = Utility.DeviceInfo("Schedule Topup");
                }

                objschedule = crmNewService.CRMScheduleTopup(objScheduleTopupRequest);
                if (objschedule != null && objschedule.ResponseDetails != null && !string.IsNullOrEmpty(objschedule.ResponseDetails.ResponseCode))
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ScheduleTopup_" + objschedule.ResponseDetails.ResponseCode);
                    objschedule.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objschedule.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ScheduleTopup End");
            }
            catch (Exception ex)
            {
                objschedule.ResponseDetails.ResponseCode = "1";
                objschedule.ResponseDetails.ResponseDesc = ex.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objScheduleTopupRequest = null;
                objRes = null;
            }
            return Json(objschedule);
        }

        //------------------------------End - FRR3328----------------------------------

        #endregion

        public ActionResult Auto(string Textdata)
        {
            ScheduleAutoTopup objschedule = new ScheduleAutoTopup();
            CRMResponse objRes;
            CRMAutoTopupResponse objautopupres;
            try
            {
                #region  FRR 4925
                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                    Session["RealICCIDForMultiTab"] = Textdata;
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                }
                #endregion

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller Auto Start");
                objRes = Utility.checkValidSubscriber("", clientSetting);
                if (objRes.ResponseCode == "0")
                {
                    Session["Cardkey"] = null;
                    objautopupres = ScheduleTopupLoad("A");
                    objschedule.lstCreditCardDetails = GetCreditCardDetails("A");

                    // 4617

                    objschedule.lstDirectdebitCardDetails = GetDirectDebitCardDetails("A");


                    if (Session["Cardkey"] != null)
                    {
                        objschedule.CardTypeValue = Session["Cardkey"].ToString();
                    }
                    else
                    {
                        objschedule.CardTypeValue = "0";
                    }
                    if (objautopupres.AutoTopup != null)
                    {
                        if (objautopupres.AutoTopup.isautotopup == "2")
                        {
                            objschedule.isautotopup = true;
                        }
                        else
                        {
                            objschedule.isautotopup = false;
                        }

                        if (string.IsNullOrEmpty(objautopupres.AutoTopup.BalanceLimit) || objautopupres.AutoTopup.BalanceLimit == "0.00")
                        {
                            objschedule.BalanceLimit = string.Empty;
                        }
                        else
                        {
                            objschedule.BalanceLimit = objautopupres.AutoTopup.BalanceLimit;
                        }

                        if (string.IsNullOrEmpty(objautopupres.AutoTopup.Topupamount) || objautopupres.AutoTopup.Topupamount == "0.00")
                        {
                            objschedule.Topupamount = string.Empty;
                        }
                        else
                        {
                            objschedule.Topupamount = objautopupres.AutoTopup.Topupamount;
                        }

                        if (objautopupres.AutoTopup.Maxlimit == "0")
                        {
                            objschedule.Maxlimit = string.Empty;
                        }
                        else
                        {
                            objschedule.Maxlimit = objautopupres.AutoTopup.Maxlimit;
                        }

                        if (objautopupres.AutoTopup.AutoDays == "0")
                        {
                            objschedule.AutoDays = string.Empty;
                        }
                        else
                        {
                            objschedule.AutoDays = objautopupres.AutoTopup.AutoDays;
                        }

                        if (string.IsNullOrEmpty(objautopupres.AutoTopup.PreferredCurrency) || objautopupres.AutoTopup.PreferredCurrency == "EURO")
                        {
                            objschedule.PreferredCurrency = "EURO";
                        }
                        else
                        {
                            objschedule.PreferredCurrency = "RON";
                        }
                    }
                    objschedule.ResponseCode = "0";
                    objschedule.ResponseDescription = objRes.ResponseDesc;
                }
                else
                {
                    objschedule.ResponseCode = "1";
                    objschedule.ResponseDescription = objRes.ResponseDesc;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller Auto End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                objRes=null;
                objautopupres=null;
            }
            return View(objschedule);
        }

        public Dictionary<string, string> GetCreditCardDetails(string CardType)
        {
            ScheduleTopup objschedule = new ScheduleTopup();
            CreditCardListRequest ObjCreditCardList = new CreditCardListRequest();
            CreditCardListResponce ObjCreditCardResponse = new CreditCardListResponce();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller GetCreditCardDetails Start");
                ObjCreditCardList.BrandCode = clientSetting.brandCode;
                ObjCreditCardList.CountryCode = clientSetting.countryCode;
                ObjCreditCardList.LanguageCode = clientSetting.langCode;
                #region FRR 4925
                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {

                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    ObjCreditCardList.MSISDN = localDict.Where(x => Session["RealICCIDForMultiTab"].ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                }
                else
                {
                    ObjCreditCardList.MSISDN = Session["MobileNumber"].ToString();
                }
                #endregion


                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjCreditCardResponse = serviceCRM.CRMGetCreditCardList(ObjCreditCardList);
                
                if (ObjCreditCardResponse.CreditCardList != null)
                {
                    if (CardType == "A" || CardType == "S")
                    {
                        List<CreditCardList> EnumData = ObjCreditCardResponse.CreditCardList;
                        if (EnumData.Count > 0)
                        {
                            Session["Cardkey"] = Convert.ToString(EnumData[0].CardKey + "," + EnumData[0].Country);
                        }
                    }
                }
                if (ObjCreditCardResponse.CreditCardList != null)
                {
                    for (int i = 0; i < ObjCreditCardResponse.CreditCardList.Count; i++)
                    {
                        objschedule.lstCreditCardDetails.Add(ObjCreditCardResponse.CreditCardList[i].CardKey + "," + ObjCreditCardResponse.CreditCardList[i].Country, ObjCreditCardResponse.CreditCardList[i].CardNo);
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
                serviceCRM = null;
                ObjCreditCardResponse = null;
                ObjCreditCardList = null;
            }
            return objschedule.lstCreditCardDetails;
        }



        // FRR 4617




        public Dictionary<string, string> GetDirectDebitCardDetails(string CardType)
        {
            ScheduleTopup objschedule = new ScheduleTopup();
            CreditCardListRequest ObjCreditCardList = new CreditCardListRequest();
            CreditCardListResponce ObjCreditCardResponse = new CreditCardListResponce();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller GetDirectDebitCardDetails Start");
                ObjCreditCardList.BrandCode = clientSetting.brandCode;
                ObjCreditCardList.CountryCode = clientSetting.countryCode;
                ObjCreditCardList.LanguageCode = clientSetting.langCode;
                #region FRR 4925
                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {

                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    ObjCreditCardList.MSISDN = localDict.Where(x => Session["RealICCIDForMultiTab"].ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                }
                else
                {
                    ObjCreditCardList.MSISDN = Session["MobileNumber"].ToString();
                }
                #endregion

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjCreditCardResponse = serviceCRM.CRMGetCreditCardList(ObjCreditCardList);

                if (ObjCreditCardResponse.DirectCardDebitdetails != null)
                {
                    if (CardType == "A" || CardType == "S")
                    {
                        List<DirectCardDebitdetails> EnumData = ObjCreditCardResponse.DirectCardDebitdetails;
                        if (EnumData.Count > 0)
                        {
                            Session["Cardkey"] = Convert.ToString(EnumData[0].CardKey + "," + EnumData[0].Country);
                        }
                    }
                }
                if (ObjCreditCardResponse.DirectCardDebitdetails != null)
                {
                    for (int i = 0; i < ObjCreditCardResponse.DirectCardDebitdetails.Count; i++)
                    {



                        objschedule.lstDirectdebitCardDetails.Add(ObjCreditCardResponse.DirectCardDebitdetails[i].CardKey + "," + ObjCreditCardResponse.DirectCardDebitdetails[i].Country + "," +
                            ObjCreditCardResponse.DirectCardDebitdetails[i].addressline1 + "," + ObjCreditCardResponse.DirectCardDebitdetails[i].addressline2 + "," + ObjCreditCardResponse.DirectCardDebitdetails[i].CardNo + "," +
                            ObjCreditCardResponse.DirectCardDebitdetails[i].email + "," + ObjCreditCardResponse.DirectCardDebitdetails[i].Nameoncard + "," + ObjCreditCardResponse.DirectCardDebitdetails[i].postcode + "," +
                              ObjCreditCardResponse.DirectCardDebitdetails[i].mobile + "," + ObjCreditCardResponse.DirectCardDebitdetails[i].city, ObjCreditCardResponse.DirectCardDebitdetails[i].CardNo);




                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller GetDirectDebitCardDetails End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
                ObjCreditCardResponse = null;
                ObjCreditCardList = null;
            }
            return objschedule.lstDirectdebitCardDetails;
        }


        public ActionResult RepeatCreditCards(string Card1, string Card2)
        {
            CreditCardListRequest ObjCreditCardList = new CreditCardListRequest();
            CreditCardListResponce ObjCreditCardResponse = new CreditCardListResponce();
            CreditCardList data = null;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult RepeatCreditCards Start");
                ObjCreditCardList.BrandCode = clientSetting.brandCode;
                ObjCreditCardList.CountryCode = clientSetting.countryCode;
                ObjCreditCardList.LanguageCode = clientSetting.langCode;
                ObjCreditCardList.MSISDN = Session["MobileNumber"].ToString();
                
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjCreditCardResponse = serviceCRM.CRMGetCreditCardList(ObjCreditCardList);
                
                data = ObjCreditCardResponse.CreditCardList.Where(m => m.CardNo.StartsWith(Card1) && m.CardNo.EndsWith(Card2)).First();
                data.cardexpirydate = Dateconvert(data.cardexpirydate);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult RepeatCreditCards End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "TopupController - RepeatCreditCards - exp - " + this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
                ObjCreditCardList = null;
                ObjCreditCardResponse = null;
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPlan()
        {
            CRMBase ObjReq = new CRMBase();
            TopupPlanIdResponse ObjRes = new TopupPlanIdResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller GetPlan Start");
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.LanguageCode = clientSetting.langCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetTopupPlanID(ObjReq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller GetPlan End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                ObjReq = null;
            }
            return Json(ObjRes.TopupPlanId);
        }

        public JsonResult GetBundle(TopupBundleRequest ObjReq)
        {
            TopupBundleResponse ObjRes = new TopupBundleResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller GetBundle Start");
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetTopupBundle(ObjReq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller GetBundle End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return Json(ObjRes.TopupBundle);
        }


        #region Staff Topup


        public JsonResult StaffTopupGetPlan()
        {
            StaffSpecialBundleReq ObjReq = new StaffSpecialBundleReq();
            StaffSpecialBundleRes ObjRes = new StaffSpecialBundleRes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller StaffTopupGetPlan Start");
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                if (clientSetting.preSettings.planChangeEnabler.ToUpper() == "TRUE")
                    ObjReq.Mode = "P";
                else
                    ObjReq.Mode = "B";

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMStaffSpecialBundle(ObjReq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller StaffTopupGetPlan End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
            }
            if (clientSetting.preSettings.planChangeEnabler.ToUpper() == "TRUE")
                return Json(ObjRes.staffTopupPlanId);
            else
                return Json(ObjRes.staffTopupBundle);
        }

        public JsonResult StaffTopupGetBundle(string Planid)
        {
            StaffSpecialBundleReq ObjReq = new StaffSpecialBundleReq();
            StaffSpecialBundleRes ObjRes = new StaffSpecialBundleRes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller StaffTopupGetBundle Start");
                ObjReq = new JavaScriptSerializer().Deserialize<StaffSpecialBundleReq>(Planid);
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                if (ObjReq.BundleType == "1")
                    ObjReq.Mode = "PR";
                else
                    ObjReq.Mode = "PS";

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMStaffSpecialBundle(ObjReq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller StaffTopupGetBundle End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                ObjReq = null;
            }
            return Json(ObjRes.staffTopupBundle);
        }

        public ActionResult Staff()
        {
            StaffTopup staffTopupObj = new StaffTopup();
            // Pending Approval        
            PendingDetailsRequest req = new PendingDetailsRequest();
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
            string bundleType = string.Empty;
            CRMResponse objRes;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller Staff Start");
                objRes = Utility.checkValidSubscriber("1", clientSetting);
                if (objRes.ResponseCode == "0")
                {
                    staffTopupObj.ResponseCode = "0";
                }
                else
                {
                    staffTopupObj.ResponseCode = "1";
                    staffTopupObj.ResponseDescription = objRes.ResponseDesc;
                }
                if (Session["PAType"] != null && Convert.ToString(Session["PAType"]) == "STAFF TOPUP" || Convert.ToString(Session["PAType"]) == "VIP STAFF TOPUP")
                {
                    // PendingDetails request
                    req.CountryCode = clientSetting.countryCode;
                    req.BrandCode = clientSetting.brandCode;
                    req.LanguageCode = clientSetting.langCode;
                    req.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    req.Type = Convert.ToString(Session["PAType"]);
                    req.Id = Convert.ToString(Session["PAId"]);

                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    ObjRes = serviceCRM.CRMGetPendingDetails(req);

                    StaffSpecialBundleRes ObjBundleRes = new StaffSpecialBundleRes();
                    StaffSpecialBundleReq ObjReq = new StaffSpecialBundleReq();
                    try
                    {
                        ObjReq.BrandCode = clientSetting.brandCode;
                        ObjReq.CountryCode = clientSetting.countryCode;
                        ObjReq.LanguageCode = clientSetting.langCode;
                        ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                        if (clientSetting.preSettings.planChangeEnabler.ToUpper() == "TRUE")
                            ObjReq.Mode = "PP";
                        else
                            ObjReq.Mode = "B";

                        if (ObjRes.PendingDetails.Count > 0 && !string.IsNullOrEmpty(ObjRes.PendingDetails[0].PlanID))
                        {
                            ObjReq.planid = ObjRes.PendingDetails[0].PlanID;
                        }
                        ObjBundleRes = serviceCRM.CRMStaffSpecialBundle(ObjReq);
                        if (clientSetting.preSettings.planChangeEnabler.ToUpper() == "TRUE")
                        {
                            if (ObjRes.PendingDetails.Count > 0 && !string.IsNullOrEmpty(ObjRes.PendingDetails[0].PlanID))
                            {
                                bundleType = (ObjBundleRes.staffTopupPlanId.Count > 0 ? ObjBundleRes.staffTopupPlanId.FirstOrDefault(a => a.ID == ObjRes.PendingDetails[0].PlanID && a.BundleCode == ObjRes.PendingDetails[0].BundleCode).Type : string.Empty);
                            }
                        }
                        else
                        {
                            if (ObjRes.PendingDetails.Count > 0 && !string.IsNullOrEmpty(ObjRes.PendingDetails[0].BundleCode))
                            {
                                bundleType = (ObjBundleRes.staffTopupBundle.Count > 0 ? ObjBundleRes.staffTopupBundle.FirstOrDefault(a => a.Bundlecode == ObjRes.PendingDetails[0].BundleCode).Type : string.Empty);
                            }
                        }
                    }
                    catch (Exception exStaff)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exStaff);
                    }
                    finally
                    { }

                    ///FRR--3083
                    if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManaageOBABundle_" + ObjRes.ResponseDetails.ResponseCode);
                        ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                    ///

                    if (ObjRes.PendingDetails.Count > 0)
                    {
                        staffTopupObj.StaffTopupAmt = ObjRes.PendingDetails[0].TopupAmt;
                        staffTopupObj.StaffTopupBundle = ObjRes.PendingDetails[0].BundleCode;
                        staffTopupObj.StaffTopupComments = ObjRes.PendingDetails[0].Comments;
                        staffTopupObj.StaffTopupPlan = string.IsNullOrEmpty(ObjRes.PendingDetails[0].PlanID) ? "" : ObjRes.PendingDetails[0].PlanID;
                        staffTopupObj.StaffTopupReason = ObjRes.PendingDetails[0].Reason;
                        staffTopupObj.StaffTopupTicketID = ObjRes.PendingDetails[0].TicketId;
                        staffTopupObj.SubscriberType = ObjRes.PendingDetails[0].NewICCID;
                        staffTopupObj.Autotopup = ObjRes.PendingDetails[0].OldMSISDN;
                        staffTopupObj.Frequency = ObjRes.PendingDetails[0].OldICCID;
                        staffTopupObj.ThresholdLimit = ObjRes.PendingDetails[0].oldIMSI;
                        staffTopupObj.paymentType = ObjRes.PendingDetails[0].NewMSISDN;
                        staffTopupObj.Id = ObjRes.PendingDetails[0].Id;
                        staffTopupObj.BundleAutoRenewal = ObjRes.PendingDetails[0].FrequentcalledNumber;
                        staffTopupObj.BundleType = string.IsNullOrEmpty(bundleType) ? string.Empty : bundleType;
                        staffTopupObj.PAType = true;
                        ViewBag.Disabled = "true";
                        ViewBag.Readonly = "true";
                        //ViewBag.selectval = staffTopupObj.StaffTopupReason.ToUpper() == "VIP TOPUP" ? "2" : "1";
                        ViewBag.selectval = staffTopupObj.StaffTopupReason.ToUpper().Contains("VIP") ? "2" : "1";
                        ViewBag.StaffTopupBundle = ObjRes.bundleName;
                    }
                }
                else
                {
                    if (Session["MobileNumber"] != null)
                    {
                        staffTopupObj.PAType = false;
                        ViewBag.Disabled = "false";
                        ViewBag.Readonly = "false";
                        VIPStafftopupRequest requestVIPStaffTopup = new VIPStafftopupRequest();
                        VIPStafftopupResponse responseVIPStaffTopup = new VIPStafftopupResponse();
                        requestVIPStaffTopup.CountryCode = clientSetting.countryCode;
                        requestVIPStaffTopup.BrandCode = clientSetting.brandCode;
                        requestVIPStaffTopup.LanguageCode = clientSetting.langCode;
                        requestVIPStaffTopup.MSISDN = Convert.ToString(Session["MobileNumber"]);
                        requestVIPStaffTopup.Mode = "GETVIP";

                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        responseVIPStaffTopup = serviceCRM.CRMVIPStafftopup(requestVIPStaffTopup);
                        staffTopupObj.ThresholdLimit = responseVIPStaffTopup.Balancelimit;
                        staffTopupObj.Frequency = responseVIPStaffTopup.TopupFrequency;
                        staffTopupObj.SubscriberType = responseVIPStaffTopup.SubscriberType;
                        staffTopupObj.ExistingAutoTopupCustomer = responseVIPStaffTopup.ExistingAutoTopupCustomer;
                        staffTopupObj.Autotopup = responseVIPStaffTopup.Autotopup;
                        //FRR--3083
                        if (responseVIPStaffTopup != null && responseVIPStaffTopup.ResponseDetails != null && responseVIPStaffTopup.ResponseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetMultiTaxData_" + responseVIPStaffTopup.ResponseDetails.ResponseCode);
                            responseVIPStaffTopup.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? responseVIPStaffTopup.ResponseDetails.ResponseDesc : errorInsertMsg;
                        }
                        ///FRR--3083
                        ///
                    }
                    else { }
                }
                Session["PAType"] = null;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller Staff End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                req = null;
                ObjRes = null;
                bundleType = string.Empty;
                objRes=null;
                serviceCRM=null;
            }
            return View(staffTopupObj);
        }

        [HttpPost]
        public ActionResult StafftopupInsert(InsertStaffTopupRequest ObjReq)
        {
            InsertStaffTopupResponse objRes = new InsertStaffTopupResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller StafftopupInsert Start");
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.Msisdn = Session["MobileNumber"].ToString();
                ObjReq.UserName = Convert.ToString(Session["UserName"]);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.CRMInsertStaffTopup(ObjReq);
                ///FRR--3083
                if (objRes != null && objRes.ResponseDetails != null && objRes.ResponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("StafftopupInsert_" + objRes.ResponseDetails.ResponseCode);
                    objRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller StafftopupInsert End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
            }
            return Json(objRes, JsonRequestBehavior.AllowGet);
        }

        /*Worked on 17-12-15*/
        [HttpPost]
        public ActionResult CRMStaffTopup(string jsonCRMStaffTopupForm)
        {
            SelectListItem selList = new SelectListItem();
            StaffTopupResponse staffTopupRespObj = new StaffTopupResponse();
            StaffTopupRequest StaffTopupReqObj = new StaffTopupRequest();
            StaffTopupRequest ObjReq = new StaffTopupRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMStaffTopup Start");
                ObjReq = new JavaScriptSerializer().Deserialize<StaffTopupRequest>(jsonCRMStaffTopupForm);
                StaffTopupReqObj.BrandCode = clientSetting.brandCode;
                StaffTopupReqObj.CountryCode = clientSetting.countryCode;
                StaffTopupReqObj.LanguageCode = clientSetting.langCode;
                StaffTopupReqObj.Msisdn = Convert.ToString(Session["MobileNumber"]);
                StaffTopupReqObj.Comments = ObjReq.Comments;
                StaffTopupReqObj.TicketId = ObjReq.TicketId;
                StaffTopupReqObj.User = Convert.ToString(Session["UserName"]);
                StaffTopupReqObj.TopupAmount = ObjReq.TopupAmount;
                StaffTopupReqObj.Reason = ObjReq.Reason;
                StaffTopupReqObj.PlanID = ObjReq.PlanID;
                StaffTopupReqObj.BundleCode = ObjReq.BundleCode;
                StaffTopupReqObj.StaffType = "1";
                StaffTopupReqObj.BundleAutoRenewal = ObjReq.BundleAutoRenewal;
                StaffTopupReqObj.PaymentType = ObjReq.PaymentType;
                StaffTopupReqObj.AdminComments = ObjReq.AdminComments;
                if (ObjReq.Mode == "1")
                {
                    if (Session["PAId"] != null)
                    {
                        StaffTopupReqObj.Id = Session["PAId"].ToString();
                    }
                    StaffTopupReqObj.Mode = ObjReq.Mode;
                    StaffTopupReqObj.Type = "A";
                }
                else if (ObjReq.Mode == "2" || ObjReq.Mode == "3")
                {
                    if (Session["PAId"] != null)
                    {
                        StaffTopupReqObj.Id = Session["PAId"].ToString();
                    }
                    StaffTopupReqObj.Mode = ObjReq.Mode;
                    StaffTopupReqObj.Type = "A";
                }
                else
                {
                    StaffTopupReqObj.Mode = "4";
                    StaffTopupReqObj.Type = "N";
                }
                StaffTopupReqObj.PARequestType = Resources.HomeResources.PAStaffTopup;
                if (clientSetting.mvnoSettings.CaptureIPAddress.ToUpper() == "ON")
                {
                    StaffTopupReqObj.DeviceInfo = Utility.DeviceInfo("Staff Topup");
                }

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                staffTopupRespObj = serviceCRM.CRMStaffTopup(StaffTopupReqObj);
                //FRR--3083
                if (staffTopupRespObj != null && staffTopupRespObj.reponseDetails != null && staffTopupRespObj.reponseDetails.ResponseCode != null)
                {
                    if (staffTopupRespObj.reponseDetails.ResponseCode == "258" || staffTopupRespObj.reponseDetails.ResponseCode == "301" || staffTopupRespObj.reponseDetails.ResponseCode == "302" || staffTopupRespObj.reponseDetails.ResponseCode == "303")
                    {
                        string errorInsertMsg;
                        if (staffTopupRespObj.reponseDetails.ResponseDesc != null)
                        {
                            string[] splitDesc = (staffTopupRespObj.reponseDetails.ResponseDesc).Split('|').ToArray();
                            errorInsertMsg = string.Format(Resources.ErrorResources.ResourceManager.GetString("StaffTopup_" + staffTopupRespObj.reponseDetails.ResponseCode), splitDesc[1], splitDesc[2]);
                        }
                        else
                        {
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("StaffTopup_" + staffTopupRespObj.reponseDetails.ResponseCode);

                        }
                        staffTopupRespObj.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? staffTopupRespObj.reponseDetails.ResponseDesc : errorInsertMsg;

                    }
                    else
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("StaffTopup_" + staffTopupRespObj.reponseDetails.ResponseCode);
                        staffTopupRespObj.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? staffTopupRespObj.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                ///FRR--3083
                if (staffTopupRespObj.reponseDetails.ResponseCode != null)
                {
                    selList.Value = staffTopupRespObj.reponseDetails.ResponseCode;
                    selList.Text = staffTopupRespObj.reponseDetails.ResponseDesc;
                }
                else
                {
                    selList.Value = "Response returned null";
                    selList.Text = "0";
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMStaffTopup End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                staffTopupRespObj = null;
                StaffTopupReqObj = null;
                ObjReq = null;
                serviceCRM=null;
            }
            return Json(selList, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult OthersAmount()
        {
            return View();
        }

        public ActionResult VoucherStatus()
        {
            return View();
        }
        public ActionResult VoucherTopup()
        {
            return View();
        }

        public CRMAutoTopupResponse ScheduleTopupLoad(string Autoplan)
        {
            CRMAutoTopupRequest objautotopup = new CRMAutoTopupRequest();
            CRMAutoTopupResponse objautopupRes = new CRMAutoTopupResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller ScheduleTopupLoad Start");
                objautotopup.BrandCode = clientSetting.brandCode;
                objautotopup.CountryCode = clientSetting.countryCode;
                objautotopup.LanguageCode = clientSetting.langCode;
                #region FRR 4925
                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {

                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    objautotopup.MSISDN = localDict.Where(x => Session["RealICCIDForMultiTab"].ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                }
                else
                {
                    objautotopup.MSISDN = Session["MobileNumber"].ToString();
                }
                #endregion
                objautotopup.Autoplan = Autoplan;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objautopupRes = serviceCRM.CRMAutoTopupDetails(objautotopup);
                ///FRR--3083
                if (objautopupRes != null && objautopupRes.ResponseDetails != null && objautopupRes.ResponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_LoadBillingHistory_" + objautopupRes.ResponseDetails.ResponseCode);
                    objautopupRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objautopupRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller ScheduleTopupLoad End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                objautotopup = null;
                serviceCRM = null;
            }
            return objautopupRes;
           
        }

        [HttpPost]
        public ActionResult ScheduleTopUpUpdate(ScheduleTopupUpdateRequest ObjReq)
        {
            ScheduleTopupUpdateResponse scheduleTopupUpdateResp = new ScheduleTopupUpdateResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller ScheduleTopUpUpdate Start");
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.MSISDN = Session["MobileNumber"].ToString();
                ObjReq.UpdatedBy = Session["UserName"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                scheduleTopupUpdateResp = serviceCRM.CRMScheduleTopupUpdate(ObjReq);
                ///FRR--3083
                if (scheduleTopupUpdateResp != null && scheduleTopupUpdateResp.ResponseDetails != null && scheduleTopupUpdateResp.ResponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ScheduleTopUpUpdate_" + scheduleTopupUpdateResp.ResponseDetails.ResponseCode);
                    scheduleTopupUpdateResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? scheduleTopupUpdateResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller ScheduleTopUpUpdate End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(scheduleTopupUpdateResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                serviceCRM = null;
            }
            return Json(scheduleTopupUpdateResp, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ScheduleAutoTopUpUpdate(AutoTopupUpdateRequest ObjReq)
        {
            AutoTopupUpdateResponse autoTopupUpdateRes = new AutoTopupUpdateResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller ScheduleAutoTopUpUpdate Start");
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.LanguageCode = clientSetting.langCode;

                #region FRR 4925
                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    ObjReq.MSISDN = localDict.Where(x => ObjReq.Textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                   
                }
                else
                {
                    ObjReq.MSISDN = Session["MobileNumber"].ToString();
                    
                }

                #endregion

                ObjReq.CsAgent = Session["UserName"].ToString();
                if (clientSetting.mvnoSettings.CaptureIPAddress.ToUpper() == "ON")
                {
                    ObjReq.DeviceInfo = Utility.DeviceInfo("Auto Topup");
                }

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                autoTopupUpdateRes = serviceCRM.CRMAutoTopupUpdate(ObjReq);
                ///FRR--3083
                if (autoTopupUpdateRes != null && autoTopupUpdateRes.ResponseDetails != null && autoTopupUpdateRes.ResponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ScheduleAutoTopUpUpdate_" + autoTopupUpdateRes.ResponseDetails.ResponseCode);
                    autoTopupUpdateRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? autoTopupUpdateRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller ScheduleAutoTopUpUpdate End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
            }
            return Json(autoTopupUpdateRes, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CRMOnlineTopup(string CardTopup)
        {
            DoOnlineTopupResponse ObjRes = new DoOnlineTopupResponse();
            OnlineTopupRequest objtopupreq = new OnlineTopupRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult CRMOnlineTopup Start");
                objtopupreq = JsonConvert.DeserializeObject<OnlineTopupRequest>(CardTopup);


                //6416
                if (clientSetting.preSettings.PrepaidPaymentUsingLink.ToUpper() != "TRUE")
                {
                string[] dates = objtopupreq.ExpiryDate.Split('/');
                string dateval = dates[0].Trim() + dates[1].Trim();
                objtopupreq.ExpiryDate = dateval;
                }

                objtopupreq.BrandCode = clientSetting.brandCode;
                objtopupreq.CountryCode = clientSetting.countryCode;
                objtopupreq.LanguageCode = clientSetting.langCode;
                

                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    objtopupreq.MSISDN = localDict.Where(x => objtopupreq.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();

                }
                else
                {
                objtopupreq.MSISDN = Session["MobileNumber"].ToString();
                }

                //6366
                objtopupreq.ICCID = Session["ICCID"].ToString();

                if (string.IsNullOrEmpty(objtopupreq.Restrict_Promo))
                    objtopupreq.Restrict_Promo = "1";
                if (!string.IsNullOrEmpty(objtopupreq.ConsentDate))
                {
                    objtopupreq.ConsentDate = Utility.GetDateconvertion(objtopupreq.ConsentDate, "MM/DD/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                }
                if (clientSetting.mvnoSettings.CaptureIPAddress.ToUpper() == "ON")
                {
                    objtopupreq.DeviceInfo = Utility.DeviceInfo("Online Topup");
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMOnlineTopup(objtopupreq);
                ///FRR--3083
                if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("OnlineTopup_" + ObjRes.ResponseDetails.ResponseCode);
                    ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083 
                /// 
                //FRR-3485  
                if (!string.IsNullOrEmpty(objtopupreq.InvoiceVatNo) && clientSetting.mvnoSettings.EnablePortugalVATScope != "0" && Convert.ToString(Session["Verify"]) == "1")
                {
                    Session["InvoiceVatNo"] = objtopupreq.InvoiceVatNo;
                }
                ObjRes.dtDateTime = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                ObjRes.dtDateTime = ObjRes.dtDateTime.Replace("DD", Convert.ToString(System.DateTime.Now.Day));
                ObjRes.dtDateTime = ObjRes.dtDateTime.Replace("MM", Convert.ToString(Convert.ToInt32(System.DateTime.Now.Month) - 1));
                ObjRes.dtDateTime = ObjRes.dtDateTime.Replace("YYYY", Convert.ToString(System.DateTime.Now.Year));
                ObjRes.dtDateTime = ObjRes.dtDateTime + " " + Convert.ToString(System.DateTime.Now.Hour) + ":" + Convert.ToString(System.DateTime.Now.Minute) + ":" + Convert.ToString(System.DateTime.Now.Second);
                //FRR-3485

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult CRMOnlineTopup End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "TopupController - CRMOnlineTopup - exp - " + this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
                objtopupreq = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);

        }

        /*Worked on 23-10-15*/
        [HttpPost]
        public TopupInsertResponse CRMTopupInsertMBOS(TopupInsertRequest objTopupreq)
        {
            TopupInsertResponse Objres = new TopupInsertResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMTopupInsertMBOS Start");
                objTopupreq.BrandCode = clientSetting.brandCode;
                objTopupreq.CountryCode = clientSetting.countryCode;
                objTopupreq.LanguageCode = clientSetting.langCode;
                objTopupreq.MobileNumber = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objres = serviceCRM.CRMTopupInsertMBOS(objTopupreq);
                ///FRR--3083
                if (Objres != null && Objres.ResponseDetails != null && Objres.ResponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("TopupInsertMBOS_" + Objres.ResponseDetails.ResponseCode);
                    Objres.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? Objres.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMTopupInsertMBOS End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Objres;
            }
            finally
            {
                serviceCRM = null;
            }
            return Objres;
        }
        /*Worked on 23-10-15*/
        public UpdateTopupResponse CRMUpdateMSTTopup(UpdateTopupRequest objUpdTopupreq)
        {
            UpdateTopupResponse ObjRes = new UpdateTopupResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMUpdateMSTTopup Start");
                objUpdTopupreq.BrandCode = clientSetting.brandCode;
                objUpdTopupreq.CountryCode = clientSetting.countryCode;
                objUpdTopupreq.LanguageCode = clientSetting.langCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMUpdateMSTTopup(objUpdTopupreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMUpdateMSTTopup End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return ObjRes;
            }
            finally
            {
                serviceCRM = null;
            }
            return ObjRes;
        }
        /*Worked on 23-10-15*/
        public InsertTopupResponse CRMInsertMSTTopup(InsertTopupRequest objInsTopupreq)
        {
            InsertTopupResponse ObjRes = new InsertTopupResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMInsertMSTTopup Start");
                objInsTopupreq.BrandCode = clientSetting.brandCode;
                objInsTopupreq.CountryCode = clientSetting.countryCode;
                objInsTopupreq.LanguageCode = clientSetting.langCode;
                objInsTopupreq.MSISDN = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMInsertMSTTopup(objInsTopupreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMInsertMSTTopup End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return ObjRes;
            }
            finally
            {
                serviceCRM = null;
            }
            return ObjRes;
        }
        /*Worked on 23-10-15*/
        public TopupVouchersResponse CRMGetTopupVouchers(CRMBase objreq)
        {
            TopupVouchersResponse ObjRes = new TopupVouchersResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetTopupVouchers Start");
                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetTopupVouchers(objreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetTopupVouchers End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
            }
            return ObjRes;

        }
        /*Worked on 23-10-15*/
        public InsertTopupLogResponse CRMMstInsertTopupLog(InsertTopupLogRequest objreq)
        {
            InsertTopupLogResponse ObjRes = new InsertTopupLogResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMMstInsertTopupLog Start");
                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.MSISDN = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMMstInsertTopupLog(objreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMMstInsertTopupLog End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
            }
            return ObjRes;
        }
        /*Worked on 27-10-15*/
        public TopupRequestNoResponse CRMGetTopupRequestNumber(TopupRequestNoRequest objTopupNoreq)
        {
            TopupRequestNoResponse ObjRes = new TopupRequestNoResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetTopupRequestNumber Start");
                objTopupNoreq.BrandCode = clientSetting.brandCode;
                objTopupNoreq.CountryCode = clientSetting.countryCode;
                objTopupNoreq.LanguageCode = clientSetting.langCode;
                objTopupNoreq.Requestid = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetTopupRequestNumber(objTopupNoreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetTopupRequestNumber End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
            }
            return ObjRes;
        }
        /*Worked on 27-10-15*/
        public RepeatTopupStatusResponse CRMGetRepeatTopupStatus(RepeatTopupStatusRequest objRepeatTopupStatusreq)
        {
            RepeatTopupStatusResponse ObjRes = new RepeatTopupStatusResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetRepeatTopupStatus Start");
                objRepeatTopupStatusreq.BrandCode = clientSetting.brandCode;
                objRepeatTopupStatusreq.CountryCode = clientSetting.countryCode;
                objRepeatTopupStatusreq.LanguageCode = clientSetting.langCode;
                objRepeatTopupStatusreq.MSISDN = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetRepeatTopupStatus(objRepeatTopupStatusreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetRepeatTopupStatus End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
            }
            return ObjRes;
        }
        /*Worked on 27-10-15*/
        public RealMSISDNResponse CRMGetRealMSISDN(RealMSISDNRequest objrealmsisdnreq)
        {
            RealMSISDNResponse ObjRes = new RealMSISDNResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetRealMSISDN Start");
                objrealmsisdnreq.BrandCode = clientSetting.brandCode;
                objrealmsisdnreq.CountryCode = clientSetting.countryCode;
                objrealmsisdnreq.LanguageCode = clientSetting.langCode;
                objrealmsisdnreq.MSISDN = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetRealMSISDN(objrealmsisdnreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetRealMSISDN End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
            }
            return ObjRes;
        }
        /*Worked on 27-10-15*/
        public ValidateAutoTopupResponse CRMValidateAutoTopupdetail(ValidateAutoTopupRequest objAutotopupreq)
        {
            ValidateAutoTopupResponse ObjRes = new ValidateAutoTopupResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMValidateAutoTopupdetail Start");
                objAutotopupreq.BrandCode = clientSetting.brandCode;
                objAutotopupreq.CountryCode = clientSetting.countryCode;
                objAutotopupreq.LanguageCode = clientSetting.langCode;
                objAutotopupreq.MSISDN = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMValidateAutoTopupdetail(objAutotopupreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMValidateAutoTopupdetail End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
            }
            return ObjRes;
        }
        /*Worked on 27-10-15*/
        public AutoTopupStatusResponse CRMGetAutoTopupStatus(AutoTopupStatusRequest objAutotopupstatusreq)
        {
            AutoTopupStatusResponse ObjRes = new AutoTopupStatusResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetAutoTopupStatus Start");
                objAutotopupstatusreq.BrandCode = clientSetting.brandCode;
                objAutotopupstatusreq.CountryCode = clientSetting.countryCode;
                objAutotopupstatusreq.LanguageCode = clientSetting.langCode;
                objAutotopupstatusreq.Status = 2;
                //objAutotopupstatusreq.StatusSpecified = true;
                objAutotopupstatusreq.Fromdate = string.Empty;
                objAutotopupstatusreq.Todate = string.Empty;
                objAutotopupstatusreq.Autoplan = string.Empty;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetAutoTopupStatus(objAutotopupstatusreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetAutoTopupStatus End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
            }
            return ObjRes;
        }
        /*Worked on 27-10-15*/
        public UpdateAutopTopupCSResponse CRMUpdateAutoTopupCS(UpdateAutopTopupCSRequest objupdateAutotopupcsreq)
        {
            UpdateAutopTopupCSResponse ObjRes = new UpdateAutopTopupCSResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMUpdateAutoTopupCS Start");
                objupdateAutotopupcsreq.BrandCode = clientSetting.brandCode;
                objupdateAutotopupcsreq.CountryCode = clientSetting.countryCode;
                objupdateAutotopupcsreq.LanguageCode = clientSetting.langCode;
                objupdateAutotopupcsreq.Msisdn = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMUpdateAutoTopupCS(objupdateAutotopupcsreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMUpdateAutoTopupCS End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
            }
            return ObjRes;
        }
        /*Worked on 29-10-15*/
        public ActionResult CRMValidatePromoCodeTopup(ValidatePromoCodeRequest objpromocodereq)
        {
            string ErrorMsg = string.Empty;
            List<string> ErrorMsgData = new List<string>();
            decimal promoTwoValue = 0;
            decimal disCountTwoVal = 0;
            decimal FirstDisount = 0;
            string DiscountAmount;
            ValidatePromoCodeResponse ObjRes = new ValidatePromoCodeResponse();
            ServiceInvokeCRM serviceCRM;

            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMValidatePromoCodeTopup Start");
                objpromocodereq.BrandCode = clientSetting.brandCode;
                objpromocodereq.CountryCode = clientSetting.countryCode;
                objpromocodereq.LanguageCode = clientSetting.langCode;
                objpromocodereq.Msisdn = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMValidatePromoCodeTopup(objpromocodereq);

                if (ObjRes.VALIDATEPROMOCODERES != null)
                {
                    if (ObjRes.VALIDATEPROMOCODERES.RETURNCODE == "0")
                    {
                        if (ObjRes.VALIDATEPROMOCODERES.PROMO_TYPE.Equals("2"))
                        {
                            if (Convert.ToDecimal(ObjRes.VALIDATEPROMOCODERES.TOP_UP_DENOMINATION_AMT) == Convert.ToDecimal(objpromocodereq.TopupAmount))
                            {
                                if (ObjRes.VALIDATEPROMOCODERES.DISCOUNT_TYPE.Equals("1"))
                                {
                                    if (Convert.ToDecimal(objpromocodereq.TopupAmount) >= Convert.ToDecimal(ObjRes.VALIDATEPROMOCODERES.DISCOUNT_VALUE))
                                    {
                                        FirstDisount = Convert.ToDecimal(objpromocodereq.TopupAmount) - Convert.ToDecimal(ObjRes.VALIDATEPROMOCODERES.DISCOUNT_VALUE);
                                        DiscountAmount = "(" + @Resources.TopupResources.PayableAmt + " : " + Math.Round(FirstDisount, 2).ToString() + ")";
                                        ErrorMsg = @Resources.TopupResources.PromoCode + " : " + ObjRes.VALIDATEPROMOCODERES.PROMO_CODE + @Resources.TopupResources.appliedSuccessfully;
                                        ErrorMsgData.Add(DiscountAmount);
                                        ErrorMsgData.Add(ErrorMsg);
                                        ErrorMsgData.Add(Math.Round(FirstDisount, 2).ToString());
                                        ErrorMsgData.Add(ObjRes.VALIDATEPROMOCODERES.DISCOUNT_VALUE);
                                    }
                                    else
                                    {
                                        ErrorMsg = @Resources.TopupResources.Topuptoolow; // "Topup Amount is too low to apply this promo";
                                        ErrorMsgData.Add(ErrorMsg);
                                    }
                                }
                                else if (ObjRes.VALIDATEPROMOCODERES.DISCOUNT_TYPE.Equals("2"))
                                {
                                    disCountTwoVal = (Convert.ToDecimal(ObjRes.VALIDATEPROMOCODERES.DISCOUNT_VALUE) * Convert.ToDecimal(objpromocodereq.TopupAmount)) / 100;
                                    if (Convert.ToDecimal(objpromocodereq.TopupAmount) >= Convert.ToDecimal(disCountTwoVal))
                                    {
                                        promoTwoValue = Convert.ToDecimal(objpromocodereq.TopupAmount) - Convert.ToDecimal(disCountTwoVal);
                                        DiscountAmount = "(" + @Resources.TopupResources.PayableAmt + " : " + Math.Round(promoTwoValue, 2).ToString() + ")";
                                        ErrorMsg = @Resources.TopupResources.PromoCode + " : " + ObjRes.VALIDATEPROMOCODERES.PROMO_CODE + @Resources.TopupResources.appliedSuccessfully;
                                        ErrorMsgData.Add(DiscountAmount);
                                        ErrorMsgData.Add(ErrorMsg);
                                        ErrorMsgData.Add(Math.Round(promoTwoValue, 2).ToString());
                                        ErrorMsgData.Add(ObjRes.VALIDATEPROMOCODERES.DISCOUNT_VALUE);
                                    }
                                    else
                                    {
                                        ErrorMsg = @Resources.TopupResources.Topuptoolow;// "Topup Amount is too low to apply this promo";
                                        ErrorMsgData.Add(ErrorMsg);
                                    }
                                }
                            }
                            else
                            {
                                ErrorMsg = @Resources.TopupResources.PromoNotapplicable; //"Entered Promo Not Applicable for the selected denomination";
                                ErrorMsgData.Add(ErrorMsg);
                            }
                        }
                        else
                        {
                            ErrorMsg = @Resources.TopupResources.InvalidPromo;//"Invalid Promo";
                            ErrorMsgData.Add(ErrorMsg);
                        }
                    }
                    else
                    {
                        ErrorMsg = ObjRes.VALIDATEPROMOCODERES.ERRDESCRITION;
                        ErrorMsgData.Add(ErrorMsg);
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMValidatePromoCodeTopup End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
                ObjRes = null;
            }
            return Json(ErrorMsgData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTax(getTaxDetailsRequest ObjgetTaxDetailsRequest)
        {
            TaxDetailsReponse ObjRes = new TaxDetailsReponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult GetTax Start");
                ObjgetTaxDetailsRequest.BrandCode = clientSetting.brandCode;
                ObjgetTaxDetailsRequest.CountryCode = clientSetting.countryCode;
                ObjgetTaxDetailsRequest.LanguageCode = clientSetting.langCode;
                ObjgetTaxDetailsRequest.MSISDN = Session["MobileNumber"].ToString();
                ObjgetTaxDetailsRequest.ICCID = Convert.ToString(Session["ICCID"]); //ObjgetTaxDetailsRequest.ICCID;
                ObjgetTaxDetailsRequest.IMSI = Convert.ToString(Session["IMSI"]); //ObjgetTaxDetailsRequest.IMSI;
                ObjgetTaxDetailsRequest.ZipCode = ObjgetTaxDetailsRequest.ZipCode;
                ObjgetTaxDetailsRequest.Amount = ObjgetTaxDetailsRequest.Amount;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.GetTax(ObjgetTaxDetailsRequest);
                ///FRR--3083
                if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetTax_" + ObjRes.Response.ResponseCode);
                    ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult GetTax End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "TopupController - GetTax - exp - " + this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CRMGetVat(VatCalcRequest objvatreq)
        {
            VatCalcResponse ObjRes = new VatCalcResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult CRMGetVat Start");
                objvatreq.BrandCode = clientSetting.brandCode;
                objvatreq.CountryCode = clientSetting.countryCode;
                objvatreq.LanguageCode = clientSetting.langCode;
                objvatreq.SubscriberId = Session["MobileNumber"] == null ? "0000000000" : Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetVat(objvatreq);
                ///FRR--3083
                if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetVat_" + ObjRes.ResponseDetails.ResponseCode);
                    ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult CRMGetVat End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "TopupController - CRMGetVat - exp - " + this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }

     
        public ActionResult Voucher(string requestType , string textdata = "")
        {
            VoucherTopupResponse voucherTopupResp = new VoucherTopupResponse();
            VoucherTopupRequest voucherTopupReq = new VoucherTopupRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller Voucher Start");
                voucherTopupReq.CountryCode = clientSetting.countryCode;
                voucherTopupReq.BrandCode = clientSetting.brandCode;
                voucherTopupReq.LanguageCode = clientSetting.langCode;

                if(clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    voucherTopupReq.MSISDN = localDict.Where(x => textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                }
                else
                {
                voucherTopupReq.MSISDN = Session["MobileNumber"].ToString();

                }

               


                voucherTopupReq.mode = "R";
                voucherTopupReq.userName = Session["UserName"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                voucherTopupResp = serviceCRM.CRMVoucherTopup(voucherTopupReq);

                if (voucherTopupResp.ResponseDetails != null && voucherTopupResp.ResponseDetails.ResponseCode == "0")
                {
                    voucherTopupResp.ResponseDetails = Utility.ValidateTopupSubscriber(clientSetting);
                }
                if (voucherTopupResp.voucherTopupHistory != null)
                {
                    voucherTopupResp.voucherTopupHistory.FindAll(ld => !string.IsNullOrEmpty(ld.logDate)).ForEach(ld => ld.logDate = Utility.FormatDateTime(ld.logDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.yyyyMMdd));
                }
                if (!string.IsNullOrEmpty(requestType) && requestType == "R")
                {
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller Voucher End");
                    return Json(voucherTopupResp.voucherTopupHistory, JsonRequestBehavior.AllowGet);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller Voucher End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                //voucherTopupResp = new VoucherTopupResponse();
                if (voucherTopupResp.TOPUPRESPONSE == null)
                    voucherTopupResp.TOPUPRESPONSE = new TOPUPRESPONSE();
                voucherTopupResp.ResponseDetails = new CRMResponse();
                voucherTopupResp.ResponseDetails.ResponseCode = "2";
                voucherTopupResp.ResponseDetails.ResponseDesc = eX.ToString();
            }
            finally
            {
                voucherTopupReq = null;
                serviceCRM = null;
            }
            return PartialView(voucherTopupResp);
        }

        public PartialViewResult VoucherTopupSubmit(string voucherCode, string voucherMode , string Textdata,string Bundlecode,string Category,string Activationtype)
        {
            VoucherTopupResponse voucherTopupResp = new VoucherTopupResponse();
            VoucherTopupRequest voucherTopupReq = new VoucherTopupRequest();
            ServiceInvokeCRM serviceCRM = null;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller VoucherTopupSubmit Start");
                voucherTopupReq.CountryCode = clientSetting.countryCode;
                voucherTopupReq.BrandCode = clientSetting.brandCode;
                voucherTopupReq.LanguageCode = clientSetting.langCode;

                if(clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    voucherTopupReq.MSISDN = localDict.Where(x => Textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    voucherTopupReq.SIM_CATEGORY = localDict.Where(x => Textdata.ToString().Contains(x.Key)).Select(x => x.Value.SIMCategory).First().ToString();
                    voucherTopupReq.Imsi = localDict.Where(x => Textdata.ToString().Contains(x.Key)).Select(x => x.Value.IMSI).First().ToString();

                }
                else
                {
                    voucherTopupReq.Imsi = Convert.ToString(Session["IMSI"]);
                    voucherTopupReq.SIM_CATEGORY = Convert.ToString(Session["SIM_CATEGORY"]);
                voucherTopupReq.MSISDN = Session["MobileNumber"].ToString();
                }


               
                voucherTopupReq.userName = Session["UserName"].ToString();
                voucherTopupReq.PIN = voucherCode;
                voucherTopupReq.mode = voucherMode;
                //6366
                voucherTopupReq.Bundlecode = Bundlecode;
                voucherTopupReq.Category = Category;
                //6537
                voucherTopupReq.Activationtype = Activationtype;


                if (string.IsNullOrEmpty(voucherTopupReq.mode))
                {
                    voucherTopupReq.mode = "T";
                }
               

                if (clientSetting.mvnoSettings.CaptureIPAddress.ToUpper() == "ON")
                {
                    voucherTopupReq.DeviceInfo = Utility.DeviceInfo("Voucher Topup");
                }


                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                voucherTopupResp = serviceCRM.CRMVoucherTopup(voucherTopupReq);

                string complistsingle = string.Empty;
                try
                {
                    if (voucherMode != null && voucherMode == "FS")
                    {
                        if (voucherTopupResp != null && voucherTopupResp.ResponseDetails != null && voucherTopupResp.ResponseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("FSVoucherTopup_" + voucherTopupResp.ResponseDetails.ResponseCode);
                            voucherTopupResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? voucherTopupResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                        }
                        if (voucherTopupResp.ResponseDetails.ResponseCode == "0")
                        {
                            voucherTopupResp.TOPUPRESPONSE.RETURNCODE = "00000";
                        }
                    }
                    else
                    {
                        if (voucherTopupResp != null && voucherTopupResp.ResponseDetails != null && voucherTopupResp.ResponseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("voucherTopup_" + voucherTopupResp.ResponseDetails.ResponseCode);
                            voucherTopupResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? voucherTopupResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                        }
                    }
                    if (voucherTopupResp.TOPUPRESPONSE != null)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(voucherTopupResp.TOPUPRESPONSE.BN) && !string.IsNullOrEmpty(voucherTopupResp.TOPUPRESPONSE.strCompBundleName))
                            {
                                voucherTopupResp.TOPUPRESPONSE.ERRDESCRITION = Resources.BundleResources.Bundle + " " + voucherTopupResp.TOPUPRESPONSE.BN + " " + Resources.BundleResources.ComponentBundle + " " + voucherTopupResp.TOPUPRESPONSE.strCompBundleName + " " + Resources.BundleResources.Purchase + " " + voucherTopupResp.ResponseDetails.ResponseDesc;
                            }
                            else if (!string.IsNullOrEmpty(voucherTopupResp.TOPUPRESPONSE.BN) && string.IsNullOrEmpty(voucherTopupResp.TOPUPRESPONSE.strCompBundleName))
                            {
                                voucherTopupResp.TOPUPRESPONSE.ERRDESCRITION = Resources.BundleResources.Bundle + " " + voucherTopupResp.TOPUPRESPONSE.BN + " " + Resources.BundleResources.Purchase + " " + voucherTopupResp.ResponseDetails.ResponseDesc;
                            }
                            else if (!string.IsNullOrEmpty(voucherTopupResp.TOPUPRESPONSE.FACEVALUE))
                            {
                                voucherTopupResp.TOPUPRESPONSE.ERRDESCRITION = Resources.BundleResources.Topup + " " + voucherTopupResp.TOPUPRESPONSE.FACEVALUE + clientSetting.mvnoSettings.currencySymbol + " " + voucherTopupResp.ResponseDetails.ResponseDesc;
                            }
                            else
                            {
                                voucherTopupResp.TOPUPRESPONSE.ERRDESCRITION = voucherTopupResp.ResponseDetails.ResponseDesc;
                            }
                        }
                        catch (Exception exVoucherTopup)
                        {
                            CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exVoucherTopup);
                        }
                        voucherTopupResp.TOPUPRESPONSE.FREEDATAEXPDATE = string.IsNullOrEmpty(voucherTopupResp.TOPUPRESPONSE.FREEDATAEXPDATE) ? string.Empty : Utility.FormatDateTime(voucherTopupResp.TOPUPRESPONSE.FREEDATAEXPDATE, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.ddMMyyyy);
                        voucherTopupResp.TOPUPRESPONSE.PROMOVALIDITYDATE = string.IsNullOrEmpty(voucherTopupResp.TOPUPRESPONSE.PROMOVALIDITYDATE) ? string.Empty : Utility.FormatDateTime(voucherTopupResp.TOPUPRESPONSE.PROMOVALIDITYDATE, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.ddMMyyyy);
                        voucherTopupResp.TOPUPRESPONSE.VALIDITYDATE = string.IsNullOrEmpty(voucherTopupResp.TOPUPRESPONSE.VALIDITYDATE) ? string.Empty : Utility.FormatDateTime(voucherTopupResp.TOPUPRESPONSE.VALIDITYDATE, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.ddMMyyyy);
                        voucherTopupResp.TOPUPRESPONSE.BEXP = string.IsNullOrEmpty(voucherTopupResp.TOPUPRESPONSE.BEXP) ? string.Empty : Utility.FormatDateTime(voucherTopupResp.TOPUPRESPONSE.BEXP, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.ddMMyyyy);
                    }
                }
                catch (Exception ex)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                    voucherTopupResp = new VoucherTopupResponse();
                    if (voucherTopupResp.TOPUPRESPONSE == null)
                        voucherTopupResp.TOPUPRESPONSE = new TOPUPRESPONSE();
                    voucherTopupResp.ResponseDetails = new CRMResponse();
                    voucherTopupResp.ResponseDetails.ResponseCode = "2";
                    voucherTopupResp.ResponseDetails.ResponseDesc = ex.ToString();
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller VoucherTopupSubmit End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                voucherTopupResp = new VoucherTopupResponse();
                if (voucherTopupResp.TOPUPRESPONSE == null)
                    voucherTopupResp.TOPUPRESPONSE = new TOPUPRESPONSE();
                voucherTopupResp.ResponseDetails = new CRMResponse();
                voucherTopupResp.ResponseDetails.ResponseCode = "2";
                voucherTopupResp.ResponseDetails.ResponseDesc = ex.ToString();
            }
            finally
            {
                voucherTopupReq = null;
                serviceCRM = null;
            }
            return PartialView("Response", voucherTopupResp.TOPUPRESPONSE);
        }

      //6366
      public JsonResult Vouchertopupquery(VoucherTopupRequest Objreq)
        {
            VoucherTopupResponse Objresp = new VoucherTopupResponse();
            ServiceInvokeCRM serviceCRM = null;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - Vouchertopupquery Start");
                Objreq.CountryCode = clientSetting.countryCode;
                Objreq.BrandCode = clientSetting.brandCode;
                Objreq.LanguageCode = clientSetting.langCode;
                Objreq.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objresp = serviceCRM.CRMVoucherTopup(Objreq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - Vouchertopupquery End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "TopupController - Vouchertopupquery - " + this.ControllerContext, ex);
            }
            finally
            {
                Objreq = null;
                serviceCRM = null;
            }
            return Json(Objresp, JsonRequestBehavior.AllowGet);
      
        }
        public PartialViewResult Refund()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult RefundReport()
        {
            RefundTopupResponse refundReportResp = new RefundTopupResponse();
            ServiceInvokeCRM serviceCRM;
            RefundTopupRequest refundReportReq = new RefundTopupRequest();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller RefundReport Start");

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                refundReportReq.CountryCode = clientSetting.countryCode;
                refundReportReq.BrandCode = clientSetting.brandCode;
                refundReportReq.LanguageCode = clientSetting.langCode;
                refundReportReq.MSISDN = Session["MobileNumber"].ToString();
                refundReportReq.RequestType = "Q";
                refundReportResp = serviceCRM.CRMRefundTopup(refundReportReq);
                try
                {
                    refundReportResp.TopupReport.Where(a => a.RequestDate != string.Empty).ToList().ForEach(b => b.RequestDate = Utility.FormatDateTime(b.RequestDate, clientSetting.mvnoSettings.dateTimeFormat));
                    refundReportResp.TopupReport.Where(a => a.ProcessedDate != string.Empty).ToList().ForEach(b => b.ProcessedDate = Utility.FormatDateTime(b.ProcessedDate, clientSetting.mvnoSettings.dateTimeFormat));
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller RefundReport End");
                }
                catch (Exception exFormatDateTimeConversion)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exFormatDateTimeConversion);
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                refundReportReq = null;
            }
            return Json(refundReportResp.TopupReport, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RefundRequest(string transID)
        {
            SelectListItem rtItem = new SelectListItem();
            ServiceInvokeCRM serviceCRM;
            RefundTopupResponse refundResp = new RefundTopupResponse();
            RefundTopupRequest refundReq = new RefundTopupRequest();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller RefundRequest Start");

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                refundReq.CountryCode = clientSetting.countryCode;
                refundReq.BrandCode = clientSetting.brandCode;
                refundReq.LanguageCode = clientSetting.langCode;
                refundReq.MSISDN = Session["MobileNumber"].ToString();
                refundReq.RequestType = "R";
                refundReq.TransID = transID;
                refundResp = serviceCRM.CRMRefundTopup(refundReq);
                //FRR--3083
                if (refundResp != null && refundResp.ResponseDetails != null && refundResp.ResponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RefundRequest_" + refundResp.ResponseDetails.ResponseCode);
                    refundResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? refundResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                if (refundResp.ResponseDetails != null)
                {
                    rtItem.Value = refundResp.ResponseDetails.ResponseCode;
                    rtItem.Text = refundResp.ResponseDetails.ResponseDesc;
                }
                else
                {
                    rtItem.Value = "1";
                    rtItem.Text = "Failed";
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller RefundRequest End");
            }
            catch (Exception eX)
            {
                rtItem.Value = "1";
                rtItem.Text = eX.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                serviceCRM = null;
                refundResp = null;
                refundReq = null;
            }
            return Json(rtItem, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult TopupRequest(string transID)
        {
            SelectListItem tuItem = new SelectListItem();
            ServiceInvokeCRM serviceCRM;
            RefundTopupResponse topupResp = new RefundTopupResponse();
            RefundTopupRequest topuptReq = new RefundTopupRequest();

            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller TopupRequest Start");

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                topuptReq.CountryCode = clientSetting.countryCode;
                topuptReq.BrandCode = clientSetting.brandCode;
                topuptReq.LanguageCode = clientSetting.langCode;
                topuptReq.MSISDN = Session["MobileNumber"].ToString();
                topuptReq.RequestType = "T";
                topuptReq.TransID = transID;
                topupResp = serviceCRM.CRMRefundTopup(topuptReq);
                //FRR--3083
                if (topupResp != null && topupResp.ResponseDetails != null && topupResp.ResponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("TopupRequest_" + topupResp.ResponseDetails.ResponseCode);
                    topupResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? topupResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                if (topupResp.ResponseDetails != null)
                {
                    tuItem.Value = topupResp.ResponseDetails.ResponseCode;
                    tuItem.Text = topupResp.ResponseDetails.ResponseDesc;
                }
                else
                {
                    tuItem.Value = "1";
                    tuItem.Text = "Failed";
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller TopupRequest End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                topupResp = null;
                topuptReq = null;
            }
            return Json(tuItem, JsonRequestBehavior.AllowGet);
        }


        public void DownLoadTopupRefund(string refundData)
        {
            GridView gridView = new GridView();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller DownLoadTopupRefund Start");
                TopupReport[] refundDetails = new JavaScriptSerializer().Deserialize<TopupReport[]>(refundData);
                gridView.DataSource = refundDetails;
                gridView.DataBind();

                Utility.ExportToExcell(gridView, "TopupHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller DownLoadTopupRefund End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
        }


        public JsonResult CRMGetIVRLanguage()
        {
            CRMBase ObjReq = new CRMBase();
            IVRLanguageResponse ObjRes = new IVRLanguageResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetIVRLanguage Start");
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.LanguageCode = clientSetting.langCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetIVRLanguage(ObjReq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetIVRLanguage End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                ObjReq = null;
            }
            return Json(ObjRes);
        }

        public JsonResult CRMGetBundleDetails()
        {
            BundleDetailsRequest objbundledetailsreq = new BundleDetailsRequest();
            BundleDetailsResponse ObjRes = new BundleDetailsResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetBundleDetails Start");
                objbundledetailsreq.BrandCode = clientSetting.brandCode;
                objbundledetailsreq.CountryCode = clientSetting.countryCode;
                objbundledetailsreq.LanguageCode = clientSetting.langCode;
                objbundledetailsreq.Planid = "141";

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetBundleDetails(objbundledetailsreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetBundleDetails End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objbundledetailsreq = null;
                serviceCRM = null;
            }
            return Json(ObjRes);
        }

        [HttpPost]
        public JsonResult CRMDynamicMSISDNAllocation(DynamicMSISDNAllocationRequest ObjReq)
        {
            DynamicMSISDNAllocationResponse ObjRes = new DynamicMSISDNAllocationResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMDynamicMSISDNAllocation Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMDynamicMSISDNAllocation(ObjReq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMDynamicMSISDNAllocation End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }


        string splitYear, splitTime, splitMeridian;
        private string Dateconvert(string DateValue, string Index = "1,0,2")
        {
            string Date = string.Empty;
            string Month = string.Empty;
            string Year = string.Empty;
            string time = string.Empty;

            try
            {
                string[] Indexsp = Index.Split(',');
                string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
                if (DateValue != null)
                {

                    string[] SplitDOB = DateValue.Replace("  ", " ").Split('-', '/', ' ');
                    Date = SplitDOB[Convert.ToInt16(Indexsp[0])].ToString();
                    // if (Date.Length != 2)
                    if (Date.Length < 2)
                    {
                        Date = "0" + Date;
                    }
                    Month = SplitDOB[Convert.ToInt16(Indexsp[1])].ToString();
                    if (Month.Length < 2)
                    {
                        if (Month.Length <= 2)
                        {
                            Month = "0" + Month;
                        }
                    }
                    Year = SplitDOB[Convert.ToInt16(Indexsp[2])].ToString();
                    if (SplitDOB.Length > 3)
                    {
                        time = SplitDOB[3].ToString();
                    }

                    if (Year.Length > 4)
                    {
                        string[] split = Year.Split(' ');
                        if (split.Count() > 0)
                        {
                            splitYear = split[0].ToString();
                        }
                        if (split.Count() > 1)
                        {
                            splitTime = split[1].ToString();
                        }
                        if (split.Count() > 2)
                        {
                            splitMeridian = split[2].ToString();
                        }
                        Year = splitYear;
                        DateValue = strInputDate.Replace("dd", Date).Replace("mm", Month).Replace("yyyy", Year).Replace("DD", Date).Replace("MM", Month).Replace("YYYY", Year);
                        if (split.Count() > 1)
                        {
                            DateValue = DateValue + " " + splitTime;
                        }
                        if (split.Count() > 2)
                        {
                            splitMeridian = split[2].ToString();
                            DateValue = DateValue + " " + splitTime + " " + splitMeridian;
                        }
                    }
                    else
                    {

                        DateValue = strInputDate.Replace("dd", Date).Replace("mm", Month).Replace("yyyy", Year).Replace("DD", Date).Replace("MM", Month).Replace("YYYY", Year);
                        if (time != string.Empty)
                        {
                            DateValue = DateValue + " " + time;
                        }
                    }
                }
                else
                {
                    DateValue = string.Empty;
                }
            }
            catch (Exception ex)
            {
                DateValue = string.Empty;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                Date = string.Empty;
                Month = string.Empty;
                Year = string.Empty;
                time = string.Empty;
            }
            return DateValue;
        }


        /*karthick started for bug fixing on 08-01-2016*/

        CreditCardListRequest ObjCreditCardList = new CreditCardListRequest();
        CreditCardListResponce ObjCreditCardResponse = new CreditCardListResponce();

        public ActionResult CRMGetCreditCardList()
        {
            CreditCardListResponce Objres = new CreditCardListResponce();
            CreditCardListRequest ObjCreditCardList = new CreditCardListRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetCreditCardList Start");
                ObjCreditCardList.BrandCode = clientSetting.brandCode;
                ObjCreditCardList.CountryCode = clientSetting.countryCode;
                ObjCreditCardList.LanguageCode = clientSetting.langCode;
                ObjCreditCardList.MSISDN = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjCreditCardResponse = serviceCRM.CRMGetCreditCardList(ObjCreditCardList);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMGetCreditCardList End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                Objres = null;
                ObjCreditCardList = null;
                serviceCRM = null;
            }
            return Json(ObjCreditCardResponse, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        public CRMResponse CRMValidateCardNo(ValidateCardRequest objreq)
        {
            CRMResponse Objres = new CRMResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult CRMValidateCardNo Start");
                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objres = serviceCRM.CRMValidateCardNo(objreq);
                //FRR--3083
                if (Objres != null && Objres.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ValidateCardNo_" + Objres.ResponseCode);
                    Objres.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? Objres.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult CRMValidateCardNo End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "TopupController - CRMValidateCardNo - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Objres;
        }

        [ValidateInput(false)]
        public ActionResult ValidateCardNo(ValidateCardRequest objreq)
        {
            CRMResponse Objres = new CRMResponse();
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult ValidateCardNo Start");
                Objres = CRMValidateCardNo(objreq);
                //FRR--3083
                if (Objres != null && Objres.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ValidateCardNo_" + Objres.ResponseCode);
                    Objres.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? Objres.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult ValidateCardNo End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "TopupController - ValidateCardNo - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                errorInsertMsg = string.Empty;
            }
            return Json(Objres, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMultiTaxData(getTaxDetailsRequest getTaxDetailsReq)
        {
            TAXVATResponse ObjRes = new TAXVATResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult GetMultiTaxData Start");

                getTaxDetailsReq.ICCID = string.Empty;
                getTaxDetailsReq.IMSI = string.Empty;
                getTaxDetailsReq.CountryCode = clientSetting.countryCode;
                getTaxDetailsReq.BrandCode = clientSetting.brandCode;
                getTaxDetailsReq.LanguageCode = clientSetting.langCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.GetMultiTaxVat(getTaxDetailsReq);
                //FRR--3083
                if (ObjRes != null && ObjRes.CRMResponse != null && ObjRes.CRMResponse.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetMultiTaxData_" + ObjRes.CRMResponse.ResponseCode);
                    ObjRes.CRMResponse.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.CRMResponse.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult GetMultiTaxData End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "TopupController - GetMultiTaxData - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM=null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }


        #region OPT-IN

        //----------------------C.GOPIKUMAR (EmpID-2296) and Vignesh (EmpID-3538)-----------------------------------
        public ActionResult Optin()
        {
            SubscriberOptinResponse ObjResp = new SubscriberOptinResponse();
            SubscriberOptinRequest ObjReq = new SubscriberOptinRequest();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller Optin Start");

                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                // Session["MobileNumber"] = "449564500004";
                ObjReq.MSISDN = Session["MobileNumber"].ToString();
                ObjResp = crmNewService.CRMSubscriberOptin(ObjReq);
                if (ObjResp != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SaveOptin_" + ObjResp.responsedetails.ResponseCode);
                    ObjResp.responsedetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responsedetails.ResponseDesc : errorInsertMsg;
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller Optin End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
            }
            return View(ObjResp);
        }

        [HttpPost]
        public JsonResult SaveOptin(string strOptin)
        {
            ChangeOptincodeRequest objReg = JsonConvert.DeserializeObject<ChangeOptincodeRequest>(strOptin);
            CRMResponse objRes = new CRMResponse();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller SaveOptin Start");

                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;

                objRes = crmNewService.CRMSubscriberOptin(objReg);
                if (objRes != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SaveOptin_" + objRes.ResponseCode);
                    objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;
                }
                else
                {
                    objRes = new CRMResponse();
                    objRes.ResponseDesc = Resources.ErrorResources.Common_2;
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.ResponseDesc);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller SaveOptin End");
            }
            catch (Exception ex)
            {
                objRes = new CRMResponse();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.ResponseCode = "2";
                objRes.ResponseDesc = ex.ToString();
            }
            finally
            {
                objReg = null;
            }
            return Json(objRes);
        }


        #endregion

        #region Data usage Limit
        //----------------------C.GOPIKUMAR (EmpID-2296) and Vignesh (EmpID-3538)-----------------------------------

        public ActionResult DataUsageLimit()
        {
            DataUsageLimitPostpaid objModelRespPP = new DataUsageLimitPostpaid();
            PostpaidDataUsageSettingsResponse ObjRespPP = new PostpaidDataUsageSettingsResponse();
            PostpaidDataUsageSettings ObjReqPP = new PostpaidDataUsageSettings();
            DataUsageLimitPrepaid objModelResp = new DataUsageLimitPrepaid();
            DataUsageSettingsResponse ObjResp = new DataUsageSettingsResponse();
            DataUsageSettings ObjReq = new DataUsageSettings();
            string IsPrepaid = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller - DataUsageLimit Start");
                IsPrepaid = "";
                IsPrepaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "0";
                if (IsPrepaid == "0")
                {
                    ObjReqPP.CountryCode = clientSetting.countryCode;
                    ObjReqPP.BrandCode = clientSetting.brandCode;
                    ObjReqPP.LanguageCode = clientSetting.langCode;
                    // Session["MobileNumber"] = "445511177250";
                    ObjReqPP.MSISDN = Session["MobileNumber"].ToString();
                    ObjReqPP.mode = "Q";
                    ObjRespPP = crmNewService.CRMPostpaidDataUsageSettings(ObjReqPP);
                    objModelRespPP.getBillShockLimit = ObjRespPP.getBillShockLimit;
                    objModelRespPP.setBillShockLimit = ObjRespPP.setBillShockLimit;
                    objModelRespPP.responseDetails = ObjRespPP.responseDetails;
                    objModelRespPP.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                    objModelRespPP.strDropdown = Utility.GetDropdownMasterFromDB("25,26", Convert.ToString(Session["isPrePaid"]), "drop_master");
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller - DataUsageLimit End");
                    return View("DatausageLimitPostpaid", objModelRespPP);
                }
                else
                {
                    ObjReq.CountryCode = clientSetting.countryCode;
                    ObjReq.BrandCode = clientSetting.brandCode;
                    ObjReq.LanguageCode = clientSetting.langCode;
                    // Session["MobileNumber"] = "445511177250";
                    ObjReq.MSISDN = Session["MobileNumber"].ToString();
                    ObjReq.mode = "Q";
                    ObjResp = crmNewService.CRMDataUsageSettings(ObjReq);
                    objModelResp.manageDataThreshold = ObjResp.manageDataThreshold;
                    objModelResp.queryDataThreshold = ObjResp.queryDataThreshold;
                    objModelResp.responseDetails = ObjResp.responseDetails;
                    objModelResp.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                    objModelResp.strDropdown = Utility.GetDropdownMasterFromDB("25,26", Convert.ToString(Session["isPrePaid"]), "drop_master");
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller - DataUsageLimit End");
                    return View(objModelResp);
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                ObjRespPP = null;
                ObjReqPP = null;
                ObjResp = null;
                ObjReq = null;
                IsPrepaid = string.Empty;
            }
            return View();
        }

        public JsonResult SaveDataUsageLimit(string strDataUsage)
        {
            DataUsageSettings ObjReq = JsonConvert.DeserializeObject<DataUsageSettings>(strDataUsage);
            DataUsageSettingsResponse ObjResp = new DataUsageSettingsResponse();
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller - SaveDataUsageLimit Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.MSISDN = Session["MobileNumber"].ToString();
                ObjResp = crmNewService.CRMDataUsageSettings(ObjReq);
                if (ObjResp != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("DataUsage_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller - SaveDataUsageLimit End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjResp, JsonRequestBehavior.AllowGet);
        }




        public JsonResult SaveDataUsageLimitPostpaid(string strDataUsage)
        {
            PostpaidDataUsageSettings ObjReq = JsonConvert.DeserializeObject<PostpaidDataUsageSettings>(strDataUsage);
            PostpaidDataUsageSettingsResponse ObjResp = new PostpaidDataUsageSettingsResponse();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller SaveDataUsageLimitPostpaid Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.MSISDN = Session["MobileNumber"].ToString();
                ObjResp = crmNewService.CRMPostpaidDataUsageSettings(ObjReq);
                if (ObjResp != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PPDataUsage_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller SaveDataUsageLimitPostpaid End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
            }
            return Json(ObjResp, JsonRequestBehavior.AllowGet);
        }





        #endregion


        #region DISCOUNT CODE
        //----------------------C.GOPIKUMAR (EmpID-2296) and Vignesh (EmpID-3538)-----------------------------------

        public ActionResult DiscountCode()
        {
            // Session["MobileNumber"] = "449677788904";
            return View();
        }

        public JsonResult SaveDiscountCode(string DiscountDatails)
        {
            MapDiscountToMSISDNRes ObjResp = new MapDiscountToMSISDNRes();
            MapDiscountToMSISDNReq ObjReq = JsonConvert.DeserializeObject<MapDiscountToMSISDNReq>(DiscountDatails);
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller SaveDiscountCode Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjResp = crmNewService.CRMMapDiscountToMSISDN(ObjReq);
                //FRR--3083
                if (ObjResp != null && ObjResp.reponseDetails != null && ObjResp.reponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SaveDiscountCode_" + ObjResp.reponseDetails.ResponseCode);
                    ObjResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    if (ObjReq.Mode == "SUBMIT" && ObjResp.reponseDetails.ResponseCode == "0")
                        Session["DISCOUNT_CODE_AVAILABLE"] = "1";
                }
                ///FRR--3083
                ///
                #region date format change
                for (int i = 0; i < ObjResp.mapdiscount.dicountInformation.Count(); i++)
                {
                    string replaceDISCOUNT_EXPIRY_DATE = ObjResp.mapdiscount.dicountInformation[i].DISCOUNT_EXPIRY_DATE.Replace('-', '/');
                    if (ObjResp.mapdiscount.dicountInformation[i].DISCOUNT_EXPIRY_DATE != string.Empty)
                    {
                        //ObjResp.mapdiscount.dicountInformation[i].DISCOUNT_EXPIRY_DATE = Utility.GetDateconvertion(replaceDISCOUNT_EXPIRY_DATE, "DD/MM/YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);
                        ObjResp.mapdiscount.dicountInformation[i].DISCOUNT_EXPIRY_DATE = Dateconvert(replaceDISCOUNT_EXPIRY_DATE, "0,1,2");
                    }
                }
                DateTimeFormatInfo uSDtfi = new CultureInfo("en-US", false).DateTimeFormat;
                DateTimeFormatInfo ukDtfi = new CultureInfo("en-GB", false).DateTimeFormat;
                DateTimeFormatInfo zhDtfi = new CultureInfo("zh-TW", false).DateTimeFormat;
                if (clientSetting.mvnoSettings.dateTimeFormat.ToUpper().StartsWith("MM"))
                {
                    ObjResp.mapdiscount.dicountInformation.Where(a => a.DISCOUNT_EXPIRY_DATE != null).ToList().ForEach(
                       b =>
                       {
                           DateTime dtDISCOUNT_EXPIRY_DATE = Convert.ToDateTime(b.DISCOUNT_EXPIRY_DATE);
                           if (dtDISCOUNT_EXPIRY_DATE >= Convert.ToDateTime("1900-01-01T12:30:00"))
                           {
                               b.DISCOUNT_EXPIRY_DATE = Convert.ToDateTime(b.DISCOUNT_EXPIRY_DATE, ukDtfi).ToString(uSDtfi.ShortDatePattern);
                           }
                           else
                           {
                               b.DISCOUNT_EXPIRY_DATE = string.Empty;
                           }
                       }
                       );
                }
                else if (clientSetting.mvnoSettings.dateTimeFormat.ToUpper().StartsWith("YY"))
                {

                    ObjResp.mapdiscount.dicountInformation.Where(a => a.DISCOUNT_EXPIRY_DATE != null).ToList().ForEach(
                       b =>
                       {
                           DateTime dtDISCOUNT_EXPIRY_DATE = Convert.ToDateTime(b.DISCOUNT_EXPIRY_DATE);
                           if (dtDISCOUNT_EXPIRY_DATE >= Convert.ToDateTime("1900-01-01T12:30:00"))
                           {
                               b.DISCOUNT_EXPIRY_DATE = Convert.ToDateTime(b.DISCOUNT_EXPIRY_DATE, ukDtfi).ToString(zhDtfi.ShortDatePattern);
                           }
                           else
                           {
                               b.DISCOUNT_EXPIRY_DATE = string.Empty;
                           }
                       }
                       );
                }
                else
                {
                    ObjResp.mapdiscount.dicountInformation.Where(a => a.DISCOUNT_EXPIRY_DATE != null).ToList().ForEach(
                       b =>
                       {
                           DateTime dtDISCOUNT_EXPIRY_DATE = Convert.ToDateTime(b.DISCOUNT_EXPIRY_DATE);
                           if (dtDISCOUNT_EXPIRY_DATE >= Convert.ToDateTime("1900-01-01T12:30:00"))
                           {
                               b.DISCOUNT_EXPIRY_DATE = Convert.ToDateTime(b.DISCOUNT_EXPIRY_DATE, uSDtfi).ToString(ukDtfi.ShortDatePattern);
                           }
                           else
                           {
                               b.DISCOUNT_EXPIRY_DATE = string.Empty;
                           }
                       }
                       );
                }
                #endregion
                for (int i = 0; i < ObjResp.mapdiscount.dicountInformation.Count(); i++)
                {
                    string[] splitDISCOUNT_EXPIRY_DATE = ObjResp.mapdiscount.dicountInformation[i].DISCOUNT_EXPIRY_DATE.Split('/');
                    if (ObjResp.mapdiscount.dicountInformation[i].DISCOUNT_EXPIRY_DATE != string.Empty)
                    {
                        if (splitDISCOUNT_EXPIRY_DATE.Length == 3)
                        {
                            if (splitDISCOUNT_EXPIRY_DATE[0].Length == 1)
                            {
                                splitDISCOUNT_EXPIRY_DATE[0] = "0" + splitDISCOUNT_EXPIRY_DATE[0];
                            }
                            if (splitDISCOUNT_EXPIRY_DATE[1].Length == 1)
                            {
                                splitDISCOUNT_EXPIRY_DATE[1] = "0" + splitDISCOUNT_EXPIRY_DATE[1];
                            }
                            if (splitDISCOUNT_EXPIRY_DATE[2].Length == 1)
                            {
                                splitDISCOUNT_EXPIRY_DATE[2] = "0" + splitDISCOUNT_EXPIRY_DATE[2];
                            }
                            ObjResp.mapdiscount.dicountInformation[i].DISCOUNT_EXPIRY_DATE = splitDISCOUNT_EXPIRY_DATE[0] + "/" + splitDISCOUNT_EXPIRY_DATE[1] + "/" + splitDISCOUNT_EXPIRY_DATE[2];
                        }
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller SaveDiscountCode End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
            }
            //return Json(ObjResp, JsonRequestBehavior.AllowGet);
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        #endregion

        #region TOPUP FAILURE HISTORY
        //----------------------C.GOPIKUMAR (EmpID-2296) and Vignesh (EmpID-3538)-----------------------------------
        public ActionResult TopupFailureHistory()
        {
            // Session["MobileNumber"] = "449677788904";
            TopupFailureReportResp ObjResponse = new TopupFailureReportResp();
            TopupFailureReportResponse ObjResp = new TopupFailureReportResponse();
            TopupFailureReportRequest ObjReq = new TopupFailureReportRequest();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller TopupFailureHistory Start");

                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.mode = "Q";
                ObjResp = crmNewService.CRMTopupFailureReport(ObjReq);
                if (ObjResp != null && ObjResp.topupStatus != null && ObjResp.topupStatus.Count > 0)
                {
                    for (int i = 0; i < ObjResp.topupStatus.Count; i++)
                    {
                        //string dummystring = ObjResp.topupStatus[i].description;
                        string dummystring = System.Text.RegularExpressions.Regex.Replace(ObjResp.topupStatus[i].description, "[^0-9a-zA-Z]+", string.Empty);
                        string ResourceMsg = Resources.DropdownResources.ResourceManager.GetString(dummystring);
                        ObjResp.topupStatus[i].description = string.IsNullOrEmpty(ResourceMsg) ? ObjResp.topupStatus[i].description : ResourceMsg;
                    }
                }
                ObjResponse.responseDetails = ObjResp.responseDetails;
                ObjResponse.topupStatus = ObjResp.topupStatus;
                ObjResponse.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                ObjResponse.strDropdown = Utility.GetDropdownMasterFromDB("33,34,35", Convert.ToString(Session["isPrePaid"]), "drop_master");

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller TopupFailureHistory End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                // ObjResp = null;
                ObjReq = null;
            }
            return View(ObjResponse);
        }
        // 4925
        public JsonResult LoadTopupFailureHistory(string FailureDatails)
        {
            TopupFailureReportResponse ObjResp = new TopupFailureReportResponse();
            TopupFailureReportRequest ObjReq = JsonConvert.DeserializeObject<TopupFailureReportRequest>(FailureDatails);
            string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller LoadTopupFailureHistory Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;

                string strGetDate = "", strDate = "", strMonth = "", strYear = "";
                string[] strSplit;

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
                    ObjReq.fromDate = strYear + "/" + strMonth + "/" + strDate;
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
                    ObjReq.toDate = strYear + "/" + strMonth + "/" + strDate;
                }
                #endregion


                #region FRR 4925

                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {

                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    ObjReq.Msisdn = localDict.Where(x => ObjReq.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                }
                #endregion

                ObjResp = crmNewService.CRMTopupFailureReport(ObjReq);

                //FRR--3083
                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LoadTopupFailureHistory_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }

                ObjResp.topupFailure.ForEach(a =>
                {
                    a.processedDate = Utility.GetDateconvertion(a.processedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                    a.requestedDate = Utility.GetDateconvertion(a.requestedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                });
                ObjResp.voucherFailure.ForEach(a =>
                {
                    a.planChangeDate = Utility.GetDateconvertion(a.planChangeDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                });

                #region Date Convertion for Grid

                if (ObjReq.mode == "AUTOTOPUPFAILURE")
                {
                    ObjResp.autoTopupFailure.ForEach(b => b.processDate = Utility.GetDateconvertion(b.processDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                }
                else if (ObjReq.mode == "AUTOTOPUPSTATUS")
                {
                    ObjResp.autoTopupStatusReport.ForEach(b => b.activatedDate = Utility.GetDateconvertion(b.activatedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                    ObjResp.autoTopupStatusReport.ForEach(b => b.deActivatedDate = Utility.GetDateconvertion(b.deActivatedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                    ObjResp.autoTopupStatusReport.ForEach(b => b.registeredDate = Utility.GetDateconvertion(b.registeredDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                }

                #endregion
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller LoadTopupFailureHistory End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        #endregion
        #region ZongOptinOpoutServices

        public ActionResult ZongOptinOptOutService()
        {
            ZongOptinOptoutServiceRequest OptinReq = new ZongOptinOptoutServiceRequest();
            ZongOptinOptoutServiceResponse OptinResp = new ZongOptinOptoutServiceResponse();
            ServiceInvokeCRM serviceCRM;

            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller ZongOptinOptOutService Start");

                OptinReq.BrandCode = clientSetting.brandCode;
                OptinReq.CountryCode = clientSetting.countryCode;
                OptinReq.LanguageCode = clientSetting.langCode;
                OptinReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                OptinReq.operationCode = "";
                OptinReq.userName = Convert.ToString(Session["UserName"]);
                OptinReq.mode = "Q";

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                OptinResp = serviceCRM.CRMZongInOutService(OptinReq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller ZongOptinOptOutService End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                serviceCRM = null;
                OptinReq = null;
            }
            return View(OptinResp);
        }

        public JsonResult StatusZongService(ZongService inZongService)
        {
            ZongOptinOptoutServiceRequest OptinReq = new ZongOptinOptoutServiceRequest();
            ZongOptinOptoutServiceResponse OptinResp = new ZongOptinOptoutServiceResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller StatusZongService Start");
                OptinReq.BrandCode = clientSetting.brandCode;
                OptinReq.CountryCode = clientSetting.countryCode;
                OptinReq.LanguageCode = clientSetting.langCode;
                OptinReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                OptinReq.operationCode = inZongService.operationCode;
                OptinReq.userName = Convert.ToString(Session["UserName"]);
                OptinReq.mode = inZongService.Mode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                OptinResp = serviceCRM.CRMZongInOutService(OptinReq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller StatusZongService End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                serviceCRM = null;
                OptinReq = null;
            }
            return Json(OptinResp, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region VIPStaffTopup
        public ActionResult CRMVIPStafftopup(VIPStafftopupRequest VIPRequest)
        {
            VIPStafftopupResponse ObjRes = new VIPStafftopupResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMVIPStafftopup Start");
                VIPRequest.CountryCode = clientSetting.countryCode;
                VIPRequest.BrandCode = clientSetting.brandCode;
                VIPRequest.LanguageCode = clientSetting.langCode;
                VIPRequest.PARequestType = Resources.HomeResources.PAVIPStaffTopup;
                if (clientSetting.mvnoSettings.CaptureIPAddress.ToUpper() == "ON")
                {
                    VIPRequest.DeviceInfo = Utility.DeviceInfo("VIP Staff Topup");
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMVIPStafftopup(VIPRequest);
                //FRR--3083
                if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                {
                    if (ObjRes.ResponseDetails.ResponseCode == "258" || ObjRes.ResponseDetails.ResponseCode == "301" || ObjRes.ResponseDetails.ResponseCode == "302" || ObjRes.ResponseDetails.ResponseCode == "303")
                    {
                        string errorInsertMsg;
                        if (ObjRes.ResponseDetails.ResponseDesc != null)
                        {
                            string[] splitDesc = (ObjRes.ResponseDetails.ResponseDesc).Split('|').ToArray();
                            errorInsertMsg = string.Format(Resources.ErrorResources.ResourceManager.GetString("VIPSTAFFTOPUP_" + ObjRes.ResponseDetails.ResponseCode), splitDesc[1], splitDesc[2]);
                        }
                        else
                        {
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("VIPSTAFFTOPUP_" + ObjRes.ResponseDetails.ResponseCode);

                        }
                        ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;

                    }
                    else
                    {
                        if (VIPRequest.Mode != "REJECT")
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("VIPSTAFFTOPUP_" + ObjRes.ResponseDetails.ResponseCode);
                            ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                        }
                        else
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("VIPSTAFFTOPUPREJECT_" + ObjRes.ResponseDetails.ResponseCode);
                            ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                        }
                    }
                }
                ///FRR--3083
                //if (VIPRequest.Option == "TOPUPALREADYEXISTS" && ObjRes.ResponseDetails.ResponseCode == "0")
                //{
                //    return PartialView("StaffTopupPromt", VIPRequest);
                //}
                //if (VIPRequest.Mode == "CHECKBALENCE" && ObjRes.ResponseDetails.ResponseCode == "0")
                //{
                //    VIPRequest.CurrentBalance = ObjRes.CurrentBalance;
                //    return View("StaffTopupPromt", VIPRequest);
                //}

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller CRMVIPStafftopup End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return Json(ObjRes);
        }
        public PartialViewResult TOPUPALREADYEXISTS(VIPStafftopupRequest VIPRequest)
        {
            return PartialView("StaffTopupPromt", VIPRequest);
        }
        public PartialViewResult CHECKBALENCE(VIPStafftopupRequest VIPRequest)
        {
            return PartialView("CheckBalancePromt", VIPRequest);
        }
        public PartialViewResult CHECKIMMTopup(VIPStafftopupRequest VIPRequest)
        {
            return PartialView("CheckImmediateTopup", VIPRequest);
        }
        public ViewResult TopupHistory()
        {
            VIPStaffHistoryRequest objReq = new VIPStaffHistoryRequest();
            VIPStaffHistoryResponse objRes = new VIPStaffHistoryResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller TopupHistory Start");
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.MSISDN = Session["MobileNumber"] as string;
                objReq.planId = Convert.ToString(Session["PlanId"]);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.CRMVIPStaffHistory(objReq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller TopupHistory End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                objReq = null;
            }
            return View(objRes);
        }
        #endregion

        #region FRR3485
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult OnlineInvoicePDF(string tablePDFData, string emailID, string strSubject, string Language)
        {
            CRMResponse objRes = new CRMResponse();
            MailMessage msg = new MailMessage();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller OnlineInvoicePDF Start");
                //if (clientSetting.countryCode.ToUpper() == "PRT")
                //{
                StringReader sr = new StringReader(tablePDFData.ToString());
                Document pdfDoc = new Document(PageSize.A4, 50, 50, 20, 20);
                HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                StyleSheet styles = new StyleSheet();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    string PDFfileName1 = Resources.TopupResources.InvoiceFilename + DateTime.Now.ToString("ddMMyyyy_hh_mm_ss_fff") + "_" + clientSetting.countryCode.ToUpper() + ".pdf";
                    PdfWriter.GetInstance(pdfDoc, memoryStream);
                    pdfDoc.Open();
                    #region Image
                    string logoPath = @"~\Library\DefaultTheme\Images\logo.png";
                    iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(Server.MapPath(logoPath));
                    pdfImage.ScaleToFit(115, 70);
                    pdfImage.Alignment = iTextSharp.text.Image.UNDERLYING;
                    pdfImage.SetAbsolutePosition(10, 770);
                    pdfDoc.Add(pdfImage);

                    if (strSubject == Resources.SIMResources.pdfSimSubject)
                    {
                        string promoPathOne = @"~\Library\DefaultTheme\Images\adv-promo.png";
                        iTextSharp.text.Image promoImageOne = iTextSharp.text.Image.GetInstance(System.Web.Hosting.HostingEnvironment.MapPath(promoPathOne));
                        promoImageOne.ScaleToFit(260, 240);
                        promoImageOne.Alignment = iTextSharp.text.Image.UNDERLYING;
                        promoImageOne.SetAbsolutePosition(303, 570);
                        pdfDoc.Add(promoImageOne);
                    }
                    else if (strSubject == Resources.TopupResources.EmailSubAllinone)
                    {
                        string promoPath = @"~\Library\DefaultTheme\Images\promo-adv.png";
                        iTextSharp.text.Image promoImage = iTextSharp.text.Image.GetInstance(Server.MapPath(promoPath));
                        promoImage.ScaleToFit(260, 250);
                        promoImage.Alignment = iTextSharp.text.Image.UNDERLYING;
                        if (Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[0].ToString() != null && Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[0].ToString() == " ")
                        {
                            int imgposition;
                            //promoImage.SetAbsolutePosition(303, 450);
                            try
                            {
                                imgposition = (int)Convert.ToDouble(Resources.TopupResources.AllinoneImagePositionUnRegister);
                            }
                            catch
                            {
                                imgposition = 450;
                            }
                            imgposition = imgposition < 300 ? 300 : imgposition;
                            promoImage.SetAbsolutePosition(303, imgposition);
                        }
                        else
                        {
                            //promoImage.SetAbsolutePosition(303, 400);
                            int imgposition;
                            try
                            {
                                //imgposition = (int)Convert.ToDouble(Resources.TopupResources.AllinoneImagePositionRegister);
                                imgposition = Convert.ToInt32(Resources.TopupResources.AllinoneImagePositionRegister);
                            }
                            catch
                            {
                                imgposition = 400;
                            }
                            imgposition = imgposition < 300 ? 300 : imgposition;
                            promoImage.SetAbsolutePosition(303, imgposition);
                        }
                        pdfDoc.Add(promoImage);
                    }
                    else if (strSubject == Resources.TopupResources.PayGInvoiceEmailSubject)
                    {
                        string promoPath = @"~\Library\DefaultTheme\Images\promo-adv.png";
                        iTextSharp.text.Image promoImage = iTextSharp.text.Image.GetInstance(Server.MapPath(promoPath));
                        promoImage.ScaleToFit(260, 250);
                        promoImage.Alignment = iTextSharp.text.Image.UNDERLYING;

                        if (Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[0].ToString() != null && Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[0].ToString() == " ")
                        {
                            int imgposition;
                            try
                            {
                                imgposition = Convert.ToInt32(Resources.TopupResources.PAyGPositionUnRegister);
                            }
                            catch
                            {
                                imgposition = 480;
                            }
                            //promoImage.SetAbsolutePosition(303, 420);
                            imgposition = imgposition < 300 ? 300 : imgposition;
                            promoImage.SetAbsolutePosition(303, imgposition);
                        }
                        else
                        {
                            int imgposition;
                            try
                            {
                                imgposition = Convert.ToInt32(Resources.TopupResources.PAyGPositionRegister);
                            }
                            catch
                            {
                                imgposition = 440;
                            }
                            //promoImage.SetAbsolutePosition(303, 400);
                            imgposition = imgposition < 300 ? 300 : imgposition;
                            promoImage.SetAbsolutePosition(303, imgposition);
                        }
                        pdfDoc.Add(promoImage);
                    }
                    else if (strSubject == Resources.TopupResources.pdfOnlineTopup)
                    {
                        string promoPath = @"~\Library\DefaultTheme\Images\promo-adv.png";
                        iTextSharp.text.Image promoImage = iTextSharp.text.Image.GetInstance(Server.MapPath(promoPath));
                        promoImage.ScaleToFit(260, 250);
                        promoImage.Alignment = iTextSharp.text.Image.UNDERLYING;
                        if (Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[0].ToString() != null && Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[0].ToString() == " ")
                        {

                            // promoImage.SetAbsolutePosition(303, 450);

                            int imgposition;
                            try
                            {
                                imgposition = Convert.ToInt32(Resources.TopupResources.CardTopupPositionUnRegister);
                            }
                            catch
                            {
                                imgposition = 450;
                            }
                            imgposition = imgposition < 300 ? 300 : imgposition;
                            promoImage.SetAbsolutePosition(303, imgposition);
                        }
                        else
                        {
                            // promoImage.SetAbsolutePosition(303, 400);
                            int imgposition;
                            try
                            {
                                imgposition = Convert.ToInt32(Resources.TopupResources.CardTopupPositionRegister);
                            }
                            catch
                            {
                                imgposition = 400;
                            }
                            imgposition = imgposition < 300 ? 300 : imgposition;
                            promoImage.SetAbsolutePosition(303, imgposition);
                        }
                        pdfDoc.Add(promoImage);
                    }
                    else
                    {
                        string promoPath = @"~\Library\DefaultTheme\Images\promo-adv.png";
                        iTextSharp.text.Image promoImage = iTextSharp.text.Image.GetInstance(Server.MapPath(promoPath));
                        promoImage.ScaleToFit(260, 250);
                        promoImage.Alignment = iTextSharp.text.Image.UNDERLYING;
                        promoImage.SetAbsolutePosition(303, 425);
                        pdfDoc.Add(promoImage);
                    }

                    #endregion
                    tablePDFData = tablePDFData.Replace(@"\n", string.Empty);
                    tablePDFData = tablePDFData.Replace(@"\", string.Empty);
                    ArrayList htmlArrList = HTMLWorker.ParseToList(new StringReader(tablePDFData.Substring(1, tablePDFData.Length - 2)), styles);
                    foreach (IElement strLn in htmlArrList)
                    {
                        pdfDoc.Add(strLn);
                    }
                    pdfDoc.Close();
                    byte[] bytes = memoryStream.ToArray();
                    memoryStream.Close();

                    string[] paranamEmail = null;
                    string[] paraValueEmail = null;
                    if (strSubject == Resources.SIMResources.pdfSimSubject)
                    {
                        paranamEmail = new string[] { "##NAME##" };
                        paraValueEmail = new string[] { Convert.ToString(Session["invTitle"]) + " " + Convert.ToString(Session["invFirstName"]) + " " + Convert.ToString(Session["invLastName"]) };
                    }
                    else
                    {
                        paranamEmail = new string[] { "##NAME##" };
                        paraValueEmail = new string[] { Convert.ToString(Session["SubscriberTitle"]) + " " + Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[0].ToString() + " " + Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[1].ToString() };
                    }
                    string emailFilePath = string.Empty, emailBody = string.Empty;
                    emailFilePath = clientSetting.mvnoSettings.emailFilePath + @"\" + Language + @"\" + "PRTINVOICE" + ".txt";
                    if (!System.IO.File.Exists(emailFilePath))
                    {
                        emailFilePath = clientSetting.mvnoSettings.emailFilePath + @"\" + "ENGLISH" + @"\" + "PRTINVOICE" + ".txt";
                        if (System.IO.File.Exists(emailFilePath))
                        {
                            emailBody = System.IO.File.ReadAllText(emailFilePath);
                            var NameandValue = paranamEmail.Zip(paraValueEmail, (n, w) => new { Name = n, Value = w });
                            foreach (var nw in NameandValue)
                            {
                                emailBody = emailBody.Replace(nw.Name, nw.Value);
                            }
                        }
                    }
                    else
                    {
                        emailBody = System.IO.File.ReadAllText(emailFilePath);
                        var NameandValue = paranamEmail.Zip(paraValueEmail, (n, w) => new { Name = n, Value = w });
                        foreach (var nw in NameandValue)
                        {
                            emailBody = emailBody.Replace(nw.Name, nw.Value);
                        }
                    }

                    msg.To.Add(emailID);
                    msg.From = new MailAddress(clientSetting.mvnoSettings.contactEmail);
                    msg.Subject = strSubject;
                    msg.Body = emailBody;
                    msg.IsBodyHtml = true;
                    msg.Attachments.Add(new Attachment(new MemoryStream(bytes), PDFfileName1));
                    CRMLogger.WriteMessage(clientSetting.langCode + "," + clientSetting.brandCode, this.ControllerContext, " SMTP port Value " + clientSetting.mvnoSettings.smtpPort);
                    SmtpClient client = new SmtpClient(clientSetting.mvnoSettings.smtpAddress, Convert.ToInt32(clientSetting.mvnoSettings.smtpPort));
                    client.UseDefaultCredentials = false;
                    System.Net.NetworkCredential basicAuthenticationInfo = new System.Net.NetworkCredential(clientSetting.mvnoSettings.contactEmail, clientSetting.mvnoSettings.smtpPassword);
                    client.Credentials = basicAuthenticationInfo;
                    client.Send(msg);
                    objRes.ResponseCode = "0";
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]) + "CountryCode:" + clientSetting.brandCode + " BrandCode:" + clientSetting.brandCode, this.ControllerContext, "Email send success");
                }
                //}
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller OnlineInvoicePDF End");
            }
            catch (System.Threading.ThreadAbortException tt)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, tt);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                msg = null;
            }
            return Json(objRes);
        }
        #endregion




        public JsonResult SendEmailPostpaidcustomer(string emailID, string strSubject, string Language, string filename, string filepath)
        {
            string path = "";
            byte[] theFolders;
            bool Result = false;
            CRMResponse objRes = new CRMResponse();
            MailMessage msg = new MailMessage();
            NetworkCredential Credentials;
            try
            {

                if (clientSetting.mvnoSettings.enableFileupload.ToUpper() != "TRUE")
                {


                var ftpRequest = (FtpWebRequest)FtpWebRequest.Create(filepath + "/" + filename);
                ftpRequest.Credentials = new NetworkCredential(clientSetting.mvnoSettings.FTPUserName, clientSetting.mvnoSettings.FTPPassword);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                // FIX in  PID 50227

                ftpRequest.EnableSsl = true;

                ServicePointManager.ServerCertificateValidationCallback =
                    delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                    {
                        return true;
                    };
                //

                var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                var ftpStream = ftpResponse.GetResponseStream();



                // Coverting stream to byte.

                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    theFolders =  ms.ToArray();
                }
                }
                else
                {
                    Credentials = new NetworkCredential(clientSetting.mvnoSettings.UNCUserName, clientSetting.mvnoSettings.UNCPassword, clientSetting.mvnoSettings.UNCDomain);
                    using (new RegistrationController.NetworkConnection(@clientSetting.mvnoSettings.UNCFilePath, Credentials))
                    {
                        path = clientSetting.mvnoSettings.UNCFilePath;
                        path = path.Replace("\\", "//");
                        path = path + "//" + filename;
                        theFolders = System.IO.File.ReadAllBytes(path);
                        
                    }
                }

                //

                //WebClient request = new WebClient();
                //string url = filepath + "/" + filename;
                //request.Credentials = new NetworkCredential(clientSetting.mvnoSettings.FTPUserName, clientSetting.mvnoSettings.FTPPassword);
                //byte[] newFileData = request.DownloadData(url);
                //Result = sendpostpaidemail(newFileData);

                string[] paranamEmail = null;
                string[] paraValueEmail = null;
                if (strSubject == Resources.SIMResources.pdfSimSubject)
                {
                    paranamEmail = new string[] { "##NAME##" };
                    paraValueEmail = new string[] { Convert.ToString(Session["invTitle"]) + " " + Convert.ToString(Session["invFirstName"]) + " " + Convert.ToString(Session["invLastName"]) };
                }
                else
                {
                    paranamEmail = new string[] { "##NAME##" };
                    paraValueEmail = new string[] { Convert.ToString(Session["SubscriberTitle"]) + " " + Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[0].ToString() + " " + Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[1].ToString() };
                }
                string emailFilePath = string.Empty, emailBody = string.Empty;
                emailFilePath = clientSetting.mvnoSettings.emailFilePath + @"\" + Language + @"\" + "PRTINVOICE" + ".txt";
                if (!System.IO.File.Exists(emailFilePath))
                {
                    emailFilePath = clientSetting.mvnoSettings.emailFilePath + @"\" + "ENGLISH" + @"\" + "PRTINVOICE" + ".txt";
                    if (System.IO.File.Exists(emailFilePath))
                    {
                        emailBody = System.IO.File.ReadAllText(emailFilePath);
                        var NameandValue = paranamEmail.Zip(paraValueEmail, (n, w) => new { Name = n, Value = w });
                        foreach (var nw in NameandValue)
                        {
                            emailBody = emailBody.Replace(nw.Name, nw.Value);
                        }
                    }
                }
                else
                {
                    emailBody = System.IO.File.ReadAllText(emailFilePath);
                    var NameandValue = paranamEmail.Zip(paraValueEmail, (n, w) => new { Name = n, Value = w });
                    foreach (var nw in NameandValue)
                    {
                        emailBody = emailBody.Replace(nw.Name, nw.Value);
                    }
                }
                msg.To.Add(emailID);
                msg.From = new MailAddress(clientSetting.mvnoSettings.contactEmail);
                msg.Subject = strSubject;
                msg.Body = emailBody;
                msg.IsBodyHtml = true;
                msg.Attachments.Add(new Attachment(new MemoryStream(theFolders), filename));
                CRMLogger.WriteMessage(clientSetting.langCode + "," + clientSetting.brandCode, this.ControllerContext, " SMTP port Value " + clientSetting.mvnoSettings.smtpPort);
                SmtpClient client = new SmtpClient(clientSetting.mvnoSettings.smtpAddress, Convert.ToInt32(clientSetting.mvnoSettings.smtpPort));
                client.UseDefaultCredentials = false;
                System.Net.NetworkCredential basicAuthenticationInfo = new System.Net.NetworkCredential(clientSetting.mvnoSettings.contactEmail, clientSetting.mvnoSettings.smtpPassword);
                client.Credentials = basicAuthenticationInfo;
                client.Send(msg);
                objRes.ResponseCode = "0";
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]) + "CountryCode:" + clientSetting.brandCode + " BrandCode:" + clientSetting.brandCode, this.ControllerContext, "Email send success");


            }
            catch (Exception ex)
            {

                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.ResponseCode = "404";
            }
            finally
            {
                theFolders = null;
            }

            return Json(objRes);
        }



        public ActionResult LoadPostpaidPopup(string OnlineReport)
        {
            string path = "";
            byte[] theFolders;
            bool Result = false;
            CRMResponse objRes = new CRMResponse();
            MailMessage msg = new MailMessage();
            NetworkCredential Credentials;
                 
            string value = string.Empty;
            try
            {
                var SplitValue = OnlineReport.Split(',');
                var filename = SplitValue[0];
                var Filepath = SplitValue[1];

                if (clientSetting.mvnoSettings.enableFileupload.ToUpper() != "TRUE")
                {
                var ftpRequest = (FtpWebRequest)FtpWebRequest.Create(Filepath + "/" + filename);
                ftpRequest.Credentials = new NetworkCredential(clientSetting.mvnoSettings.FTPUserName, clientSetting.mvnoSettings.FTPPassword);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                // FIX in  PID 50227

                ftpRequest.EnableSsl = true;

                ServicePointManager.ServerCertificateValidationCallback =
                    delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                    {
                        return true;
                    };
                //

                var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                var ftpStream = ftpResponse.GetResponseStream();
                const string contentType = "application/pdf";

                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    theFolders = ms.ToArray();
                }
                }
                else
                {

                    Credentials = new NetworkCredential(clientSetting.mvnoSettings.UNCUserName, clientSetting.mvnoSettings.UNCPassword, clientSetting.mvnoSettings.UNCDomain);
                    using (new RegistrationController.NetworkConnection(@clientSetting.mvnoSettings.UNCFilePath, Credentials))
                    {
                        path = clientSetting.mvnoSettings.UNCFilePath;
                        path = path.Replace("\\", "//");
                        path = path + "//" + filename;
                        theFolders = System.IO.File.ReadAllBytes(path);

                    }
                }

                return File(theFolders, "application/pdf");

                //string filepath = Server.MapPath(@"~\Library\DefaultTheme\Images\logo.png");
                //byte[] pdfByte = GetBytesFromFile(filepath);
                //return File(pdfByte, "image/png");

                //  File(ftpStream, contentType);

                //byte[] bytes = Encoding.ASCII.GetBytes(OnlineReport);
                //return File(bytes, "application/pdf");

            }
            catch(Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                theFolders = System.Text.Encoding.ASCII.GetBytes("Internal Error");
                return File(theFolders, "application/pdf");
            }
            finally
            {
                theFolders = null;
            }
        }

       
        #region Download Reg history
        [HttpPost]
        public void DownLoadRegHistory(string regData)
        {
            GridView gridView = new GridView();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller DownLoadRegHistory Start");
                //TopupFailure[] topupFailure = new JavaScriptSerializer().Deserialize<TopupFailure[]>(topupData);
                List<RegReport> topupFailure = JsonConvert.DeserializeObject<List<RegReport>>(regData);
                gridView.DataSource = topupFailure;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "RegHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup Controller DownLoadRegHistory End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
        }
        #endregion
		 #region Failure History  FRR 4340

        public ActionResult FailureHistory()
        {
            TopupFailureReportResp ObjResponse = new TopupFailureReportResp();
            try
            {
                ObjResponse.strDropdown = Utility.GetDropdownMasterFromDB("81", Convert.ToString(Session["isPrePaid"]), "drop_master");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(ObjResponse);
        }

        public JsonResult LoadFailureHistory(string FailureDatails)
        {
            FailureHistroyResponse ObjResp = new FailureHistroyResponse();
            FailureHistroyRequest ObjReq = JsonConvert.DeserializeObject<FailureHistroyRequest>(FailureDatails);
            string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;

                string strGetDate = "", strDate = "", strMonth = "", strYear = "";
                string[] strSplit;

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

                ObjResp = crmNewService.CRMFailureHistroy(ObjReq);

                //FRR--3083
                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LoadFailureHistory_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }

                ObjResp.FailureHistroyStatus.ForEach(a =>
                {
                    a.CallDate = Utility.GetDateconvertion(a.CallDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                });

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        [HttpPost]
        public void DownLoadFailureHistory(string topupData)
        {
            try
            {
                GridView gridView = new GridView();
                List<FailureHistroyStatus> Failure = JsonConvert.DeserializeObject<List<FailureHistroyStatus>>(topupData);
                gridView.DataSource = Failure;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "FailureHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
        }

        #endregion



        #region-------------4389-
        public ActionResult GetCheckTopupAppliedPromoCode(string QueryTopup)
        {
            GetTopupDiscountAmountRes ObjRes = new GetTopupDiscountAmountRes();
            GetTopupDiscountAmountReq objTopupReq = new JavaScriptSerializer().Deserialize<GetTopupDiscountAmountReq>(QueryTopup);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            bool blnMaxCreditCheck_Error= false;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult GetCheckTopupAppliedPromoCode Start");
                objTopupReq.CountryCode = clientSetting.countryCode;
                objTopupReq.BrandCode = clientSetting.brandCode;
                objTopupReq.LanguageCode = clientSetting.langCode;
                if (string.IsNullOrEmpty(objTopupReq.simreg))
                {
                if (string.IsNullOrEmpty(objTopupReq.MSISDN))
                {
                        if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                        {
                            Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                            objTopupReq.MSISDN = localDict.Where(x => objTopupReq.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();

                        }
                        else
                        {
                            objTopupReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                        }
                    }
                }


                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                if (clientSetting.mvnoSettings.MaxCreditCheck.Trim().ToUpper()=="ON")
                {
                    ObjRes = serviceCRM.CRMMaxiumCreditCheck(objTopupReq);
                    if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                    {
                        blnMaxCreditCheck_Error = true;
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMMaxiumCreditCheck_" + ObjRes.Response.ResponseCode);
                        ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;
                    }
                }
                if (blnMaxCreditCheck_Error == false)
                {
                    ObjRes = serviceCRM.GetTopup_Existing_PromoCode_Applied(objTopupReq);
                    if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMGetCHECKTOPUPELIGIBILITY_" + ObjRes.Response.ResponseCode);
                        ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ActionResult GetCheckTopupAppliedPromoCode End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "TopupController - GetCheckTopupAppliedPromoCode - ex - " + this.ControllerContext, ex);
            }
            finally
            {
                objTopupReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
                blnMaxCreditCheck_Error = false;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region FRR-4668
     
        public ViewResult ChannelVAS()
        {
            return View("ChannelVAS");
        }

        public JsonResult ChannelVASS(string GetChannelVASRequest)
        {
            ChannelVASResponse ObjResp = new ChannelVASResponse();
            ChannelVASRequest ObjReq = JsonConvert.DeserializeObject<ChannelVASRequest>(GetChannelVASRequest);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - ChannelVAS Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.CRMGetChannelVAS(ObjReq);
                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ChannelVASRequestResponse_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Topup - ChannelVAS End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "Topup - ChannelVAS - " + this.ControllerContext, ex);
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


        #region FRR 4734
        public JsonResult eSIMSendEmail(string emailID, string strSubject, string Language, string filename, string filepath , string ICCID , string PUK)
        {
            string path = "";
            byte[] theFolders;
            CRMResponse objRes = new CRMResponse();
            MailMessage msg = new MailMessage();
            NetworkCredential Credentials;
            try
            {


                Credentials = new NetworkCredential(clientSetting.mvnoSettings.UNCUserName, clientSetting.mvnoSettings.UNCPassword, clientSetting.mvnoSettings.UNCDomain);
                using (new RegistrationController.NetworkConnection(@clientSetting.mvnoSettings.UNCFilePath, Credentials))
                {
                    path = clientSetting.mvnoSettings.UNCFilePath;
                    path = path.Replace("\\", "//");
                    path = path + "//" + filename;
                    theFolders = System.IO.File.ReadAllBytes(path);

                }

                string[] paranamEmail = null;
                string[] paraValueEmail = null;
                if (strSubject == Resources.SIMResources.pdfSimSubject)
                {
                    paranamEmail = new string[] { "##NAME##" , "##ICCID##" , "##PUK##" };
                    paraValueEmail = new string[] { filepath  , ICCID , PUK};
                }
                else
                {
                    paranamEmail = new string[] { "##NAME##", "##ICCID##", "##PUK##" };
                    paraValueEmail = new string[] { filepath, ICCID, PUK };
                }
                string emailFilePath = string.Empty, emailBody = string.Empty;
                emailFilePath = clientSetting.mvnoSettings.emailFilePath + @"\" + Language + @"\" + "eSIM" + ".txt";
                if (!System.IO.File.Exists(emailFilePath))
                {
                    emailFilePath = clientSetting.mvnoSettings.emailFilePath + @"\" + "ENGLISH" + @"\" + "eSIM" + ".txt";
                    if (System.IO.File.Exists(emailFilePath))
                    {
                        emailBody = System.IO.File.ReadAllText(emailFilePath);
                        var NameandValue = paranamEmail.Zip(paraValueEmail, (n, w) => new { Name = n, Value = w });
                        foreach (var nw in NameandValue)
                        {
                            emailBody = emailBody.Replace(nw.Name, nw.Value);
                        }
                    }
                }
                else
                {
                    emailBody = System.IO.File.ReadAllText(emailFilePath);
                    var NameandValue = paranamEmail.Zip(paraValueEmail, (n, w) => new { Name = n, Value = w });
                    foreach (var nw in NameandValue)
                    {
                        emailBody = emailBody.Replace(nw.Name, nw.Value);
                    }
                }
                msg.To.Add(emailID);
                msg.From = new MailAddress(clientSetting.mvnoSettings.contactEmail);
                msg.Subject = strSubject;
                msg.Body = emailBody;
                msg.IsBodyHtml = true;
                msg.Attachments.Add(new Attachment(new MemoryStream(theFolders), filename));
                CRMLogger.WriteMessage(clientSetting.langCode + "," + clientSetting.brandCode, this.ControllerContext, " SMTP port Value " + clientSetting.mvnoSettings.smtpPort);
                SmtpClient client = new SmtpClient(clientSetting.mvnoSettings.smtpAddress, Convert.ToInt32(clientSetting.mvnoSettings.smtpPort));
                client.UseDefaultCredentials = false;
                System.Net.NetworkCredential basicAuthenticationInfo = new System.Net.NetworkCredential(clientSetting.mvnoSettings.contactEmail, clientSetting.mvnoSettings.smtpPassword);
                client.Credentials = basicAuthenticationInfo;
                client.Send(msg);
                objRes.ResponseCode = "0";
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]) + "CountryCode:" + clientSetting.brandCode + " BrandCode:" + clientSetting.brandCode, this.ControllerContext, "Email send success");


            }
            catch (Exception ex)
            {
                objRes.ResponseCode = "404";
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);

            }
            finally
            {
                theFolders = null;
            }

            return Json(objRes);
        }


        #endregion


        #region FRR-4735
        public ActionResult ManageOptoutBenefits()
        {
            return View();
        }

        public JsonResult GetManageOptoutBenefits(string GetManageOptoutBenefits)
        {
            OptoutBenefitsResponse ObjRes = new OptoutBenefitsResponse();
            OptoutBenefitsRequest ObjReq = JsonConvert.DeserializeObject<OptoutBenefitsRequest>(GetManageOptoutBenefits);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - CRMGetOptoutBenefitsDetails Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetOptoutBenefitsDetails(ObjReq);
                //if (ObjRes != null && ObjRes.responseDetails.ResponseCode != null && ObjRes.responseDetails.ResponseCode != null)
                //{
                //    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMGetOptoutBenefitsDetailsRequestResponse" + ObjRes.responseDetails.ResponseCode);
                //    ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;
                //}
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - CRMGetOptoutBenefitsDetails End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "TopupController - exception-CRMGetOptoutBenefitsDetails - " + this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return new JsonResult() { Data = ObjRes, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }



        #endregion

        #region 6366
        public JsonResult GetTopupRecommendedBundles(RecommendedbundlesReq ObjReq)
        {
            RecommendedbundlesResp ObjRes = new RecommendedbundlesResp();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - GetTopupRecommendedBundles Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.Msisdn = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetTopupRecommendedBundles(ObjReq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - GetTopupRecommendedBundles End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "TopupController - exception-GetTopupRecommendedBundles - " + this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return new JsonResult() { Data = ObjRes, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        #endregion

        #region 6439 IR-Topup
        public ActionResult IRTopup(IRTopupReq ObjReq)
        {
            ModelIRTopup Model = new ModelIRTopup();
            IRTopupResp ObjResp = new IRTopupResp();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - GetTopupRecommendedBundles Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                ObjReq.ICCID = Convert.ToString(Session["ICCID"]);
                ObjReq.IMSI = Convert.ToString(Session["IMSI"]);

                if (string.IsNullOrEmpty(ObjReq.Mode))
                    ObjReq.Mode = "GETSTATUS";

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.CRMIRTopup(ObjReq);

                if (ObjReq.Mode == "GETSTATUS" && ObjResp != null && !string.IsNullOrEmpty(ObjResp.Status) && ObjResp.Status.ToUpper() == "ACTIVE")
                {
                    Model.TopupAmonut = Utility.DataTableToDictionary(Utility.GetDropdownMasterFromDB("tbl_topup_amount"));
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - GetTopupRecommendedBundles End");
                if (ObjReq.Mode == "GETSTATUS")
                {
                    Model.IRTopupResp = ObjResp;
                    return View(Model);
                }
                else
                {
                    return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "TopupController - exception-GetTopupRecommendedBundles - " + this.ControllerContext, ex);
                if (ObjReq.Mode == "GETSTATUS")
                {
                    Model.IRTopupResp = ObjResp;
                    return View(Model);
                }
                else
                {
                    return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }
            finally
            {
               if (ObjReq.Mode == "GETSTATUS")
                    ObjResp = null;
                ObjReq = null;
                serviceCRM = null;
               
            }
        }
        #endregion
    }
}

