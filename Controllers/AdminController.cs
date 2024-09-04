using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using ServiceCRM;
using Newtonsoft.Json;

namespace CRM.Controllers
{
    [ValidateState]
    public class AdminController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();

        public PartialViewResult AdminControl()
        {
            return PartialView();
        }

        public JsonResult PendingApprovalCount()
        {
            PendingApprovalCountResponse pendingResp = new PendingApprovalCountResponse();
            PendingApprovalCountRequest pendingReq = new PendingApprovalCountRequest();
            ServiceInvokeCRM serviceCRM;
            string dtCurrentDate = string.Empty;
            string dateCount = string.Empty;

            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - PendingApprovalCount Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                pendingReq.CountryCode = clientSetting.countryCode;
                pendingReq.BrandCode = clientSetting.brandCode;
                pendingReq.LanguageCode = clientSetting.langCode;
                dtCurrentDate = System.DateTime.Now.ToString("yyyy/MM/dd");
                dateCount = clientSetting.mvnoSettings.paDateRange;
                TimeSpan dateNumber = new TimeSpan(Convert.ToInt16(dateCount), 0, 0, 0);
                DateTime dtFromDate = Convert.ToDateTime(dtCurrentDate) - dateNumber;
                pendingReq.FromDate = dtFromDate.ToString("yyyy/MM/dd");
                pendingReq.ToDate = dtCurrentDate.ToString();
                pendingReq.userName = Convert.ToString(Session["UserName"]);
                if (Convert.ToString(Session["IsRootUser"]).ToLower() == "true")
                    pendingReq.RoleCatType = "0";
                else
                    pendingReq.RoleCatType = "1";
                pendingResp = serviceCRM.FetchPendingApprovalCount(pendingReq);
                if (pendingResp.PendingApprovalCount != null && pendingResp.PendingApprovalCount.Count > 0)
                {
                   
                    pendingResp.PendingApprovalCount = pendingResp.PendingApprovalCount.OrderBy(m => m.Type).ToList();
                }
                for (var i = 0; i < pendingResp.PendingApprovalCount.Count; i++)
                {
                    if (pendingResp.PendingApprovalCount[i].Type != null && pendingResp.PendingApprovalCount[i].Type != "Portout Refund Approval")
                    {
                        switch (pendingResp.PendingApprovalCount[i].Type)
                        {
                            #region switch cases
                            case "SIM Block":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PASIMBlock;
                                break;

                            case "SIM UnBlock":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PASIMUnBlock;
                                break;

                            case "Swap IMSI":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PASwapIMSI;
                                break;

                            case "Swap MSISDN":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PASwapMSISDN;
                                break;


                            case "Balance Transfer":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PABalanceTransfer;
                                break;


                            case "Credit":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.BillingResources.Credit;
                                break;

                            case "Debit":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.BillingResources.Debit;
                                break;

                            case "Staff Topup":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PAStaffTopup;
                                break;

                            case "Change Plan":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PAChangePlan;
                                break;

                            case "OBA Plan Upgrade/Downgrade":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PAOBAPlanUpgrade;
                                break;

                            case "OBA Additional Credit":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PAOBAAdditionalCredit;
                                break;

                            case "Bundle Bucket":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PABundleBucket;
                                break;

                            case "Zipcode Swap MSISDN":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PAZipcodeSwapMSISDN;
                                break;

                            case "Partial Balance Transfer":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PAPartialBalanceTransfer;
                                break;

                            case "Suspend":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PASuspend;
                                break;

                            case "Restore":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PARestore;
                                break;

                            case "Activate":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PAActivate;
                                break;

                            case "DeActivate":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PADeActivate;
                                break;

                            case "ReActivate LMSISDN":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PAReActivateLMSISDN;
                                break;

                            case "Refund Payment":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PARefundPayment;
                                break;

                            case "MHA":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PAMHA;
                                break;

                            case "VIP Staff Topup":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PAVIPStaffTopup;
                                break;
                            case "Cancel Auto Renewal":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.CancelAutoRenewal;
                                break;
                            case "Bundle Transfer":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.BundleTransfer;
                                break;
                            case "RLH":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.RLH;
                                break;
                            case "TeleVerification":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.Televerification;
                                break;
                            case "IMEI Block":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PABlock;
                                break;

                            case "IMEI UnBlock":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PAUnBlock;
                                break;

                            case "Device Refund Payment":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.DeviceRefundPayment;
                                break;

                            case "Cancel Family Plan":
                                pendingResp.PendingApprovalCount[i].Desc = "Cancel Family Plan";
                                break;

                            case "Promo Code Mapping":
                                pendingResp.PendingApprovalCount[i].Desc = "Promo Code Mapping";
                                break;
                            //4653
                            case "OBA Credit Limit Upgrade/Downgrade":
                                pendingResp.PendingApprovalCount[i].Desc = "OBA CREDIT LIMIT UPGRADE/DOWNGRADE";
                                break;

                            // 4928
                            case "Portout Refund Approval":

                                pendingResp.PendingApprovalCount[i].Desc = "Portout Refund Approval";

                                break;

                            //POF-6007
                            case "LegalSIMSWAPandROLLBACK":

                                pendingResp.PendingApprovalCount[i].Desc = "LegalSIMSWAPandROLLBACK";

                                break;

                            default:
                                pendingResp.PendingApprovalCount[i].Desc = pendingResp.PendingApprovalCount[i].Type;
                                break;
                                #endregion
                        }
                    }
                    else
                    {
                        if (clientSetting.preSettings.EnablePortinRefund.ToUpper() == "TRUE")
                        {
                            pendingResp.PendingApprovalCount[i].Desc = "Portout Refund Approval";
                        }
                        else
                        {
                            pendingResp.PendingApprovalCount[i].Desc = string.Empty;
                            pendingResp.PendingApprovalCount[i].Count = string.Empty;
                        }
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - PendingApprovalCount End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                pendingReq = null;
                serviceCRM = null;
                dtCurrentDate = string.Empty;
                dateCount = string.Empty;
            }
            return Json(pendingResp.PendingApprovalCount, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Management()
        {
            try
            {
                if ((Convert.ToString(Session["UserGroupID"]) == "1") || (bool)Session["IsAdmin"] || (Session["MenuItemResp"] != null && ((RoleMgtMasterItemResponse)Session["MenuItemResp"]).settingMenu.Count == 9))
                {

                }
                else
                {
                    return RedirectToAction("Home", "CRM");
                }
            }
            catch
            {

            }
            return View();
        }

        public ActionResult ManagementControl(string linkID)
        {
            return RedirectToAction(linkID, linkID);
        }

        public ViewResult PendingApprovalItems(string value)
        {
            PendingActionsResponse pendAction = new PendingActionsResponse();
            PendingActionsRequest objRequest = new PendingActionsRequest();
            string dtCurrentDate = string.Empty;
            string dateCount = string.Empty;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - PendingApprovalItems Start");
                objRequest.BrandCode = clientSetting.brandCode;
                objRequest.CountryCode = clientSetting.countryCode;
                objRequest.LanguageCode = clientSetting.langCode;
                objRequest.userName = Convert.ToString(Session["UserName"]);
                if (Convert.ToString(Session["IsRootUser"]).ToLower() == "true")
                    objRequest.RoleCatType = "0";
                else
                    objRequest.RoleCatType = "1";

                if (value == null)
                {
                    objRequest.Type = "ALL";
                }
                else
                {
                    objRequest.Type = value.ToUpper();
                }

                if ((bool)Session["IsAdmin"])
                {
                    objRequest.Status = "P";
                }
                else
                {
                    objRequest.Status = "A";
                }
                if (objRequest.Type == "TELEVERIFICATION")
                {
                    objRequest.Status = "";
                    Session["Televerification"] = null;
                }
                //string paType = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
                ViewData["PendingName"] = value;
                dtCurrentDate = System.DateTime.Now.ToString("yyyy/MM/dd");
                dateCount = clientSetting.mvnoSettings.paDateRange;
                TimeSpan dateNumber = new TimeSpan(Convert.ToInt16(dateCount), 0, 0, 0);
                DateTime dtFromDate = Convert.ToDateTime(dtCurrentDate) - dateNumber;
                objRequest.FromDate = dtFromDate.ToString("yyyy/MM/dd");
                objRequest.ToDate = dtCurrentDate.ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                pendAction = serviceCRM.ShowPendingActions(objRequest);

                List<PendingApprovalNames> lstPendingNames;
                try
                {
                    if (pendAction.ShowPendingAction != null && pendAction.ShowPendingAction.Count > 0)
                    {
                        lstPendingNames = PendingApprovalList();

                        pendAction.ShowPendingAction.FindAll(a => !string.IsNullOrEmpty(a.submittedDate)).ForEach(b => b.submittedDate = Utility.GetDateconvertion(b.submittedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                        pendAction.ShowPendingAction.FindAll(a => !string.IsNullOrEmpty(a.authorizedDate)).ForEach(b => b.authorizedDate = Utility.GetDateconvertion(b.authorizedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                        pendAction.ShowPendingAction.FindAll(a => !string.IsNullOrEmpty(a.rejectedDate)).ForEach(b => b.rejectedDate = Utility.GetDateconvertion(b.rejectedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));

                        
                        //pendAction.ShowPendingAction.FindAll(a => !string.IsNullOrEmpty(a.submittedDate)).ForEach(b => b.submittedDate = Utility.FormatDateTime(b.submittedDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.yyyyMMdd));
                      //  pendAction.ShowPendingAction.FindAll(a => !string.IsNullOrEmpty(a.authorizedDate)).ForEach(b => b.authorizedDate = Utility.FormatDateTime(b.authorizedDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.yyyyMMdd));
                       // pendAction.ShowPendingAction.FindAll(a => !string.IsNullOrEmpty(a.rejectedDate)).ForEach(b => b.rejectedDate = Utility.FormatDateTime(b.rejectedDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.yyyyMMdd));
                        pendAction.ShowPendingAction.FindAll(a => !string.IsNullOrEmpty(a.Type)).ForEach(b => b.Type = lstPendingNames.FirstOrDefault(m => m.Pendingvalue.ToLower() == b.Type.ToLower()).Pendingvalue);
                    }
                    if (pendAction.ShowPendingAction != null && pendAction.ShowPendingAction.Count > 0)
                    {
                        pendAction.ShowPendingAction.ForEach(a =>
                        {
                            a.CurrentStatus = (a.CurrentStatus != null && a.CurrentStatus == "Waiting For 1st Level Approval") ? Resources.HomeResources.WaitingFor1stlevel : a.CurrentStatus;
                            a.CurrentStatus = (a.CurrentStatus != null && a.CurrentStatus == "Waiting For 2nd Level Approval") ? Resources.HomeResources.WaitingFor2ndlevel : a.CurrentStatus;
                            a.CurrentStatus = (a.CurrentStatus != null && a.CurrentStatus == "Transfer Failed") ? Resources.HomeResources.TransferFailed : a.CurrentStatus;
                            a.CurrentStatus = (a.CurrentStatus != null && a.CurrentStatus == "Approved") ? Resources.HomeResources.typeApproved : a.CurrentStatus;
                            a.CurrentStatus = (a.CurrentStatus != null && a.CurrentStatus == "Rejected") ? Resources.HomeResources.typeRejected : a.CurrentStatus;
                            a.CurrentStatus = (a.CurrentStatus != null && a.CurrentStatus == "APM Eshop Fail") ? Resources.BundleResources.APMEshopFail : a.CurrentStatus;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "SIM BLOCK") ? Resources.HomeResources.PASIMBlock : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "SIM UNBLOCK") ? Resources.HomeResources.PASIMUnBlock : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "SWAP IMSI") ? Resources.HomeResources.PASwapIMSI : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "SWAP MSISDN") ? Resources.HomeResources.PASwapMSISDN : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "BALANCE TRANSFER") ? Resources.HomeResources.PABalanceTransfer : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "CREDIT") ? Resources.BillingResources.Credit : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "DEBIT") ? Resources.BillingResources.Debit : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "STAFF TOPUP") ? Resources.HomeResources.PAStaffTopup : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "CHANGE PLAN") ? Resources.HomeResources.PAChangePlan : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "OBA Plan Upgrade/Downgrade") ? Resources.HomeResources.PAOBAPlanUpgrade : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "OBA ADDITIONAL CREDIT") ? Resources.HomeResources.PAOBAAdditionalCredit : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "BUNDLE BUCKET") ? Resources.HomeResources.PABundleBucket : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "ZIPCODE SWAP MSISDN") ? Resources.HomeResources.PAZipcodeSwapMSISDN : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "PARTIAL BALANCE TRANSFER") ? Resources.HomeResources.PAPartialBalanceTransfer : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "SUSPEND") ? Resources.HomeResources.PASuspend : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "RESTORE") ? Resources.HomeResources.PARestore : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "ACTIVATE") ? Resources.HomeResources.PAActivate : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "DEACTIVATE") ? Resources.HomeResources.PADeActivate : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "REACTIVATE LMSISDN") ? Resources.HomeResources.PAReActivateLMSISDN : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "REFUND PAYMENT") ? Resources.HomeResources.PARefundPayment : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "MHA") ? Resources.HomeResources.PAMHA : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "VIP STAFF TOPUP") ? Resources.HomeResources.PAVIPStaffTopup : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "CANCEL AUTO RENEWAL") ? Resources.HomeResources.CancelAutoRenewal : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "BUNDLE TRANSFER") ? Resources.HomeResources.BundleTransfer : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "RLH") ? Resources.HomeResources.RLH : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "TELEVERIFICATION") ? Resources.HomeResources.Televerification : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "IMEI BLOCK") ? Resources.HomeResources.PABlock : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "IMEI UNBLOCK") ? Resources.HomeResources.PAUnBlock : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "OBA CREDIT LIMIT UPGRADE/DOWNGRADE") ? "OBA CREDIT LIMIT UPGRADE/DOWNGRADE" : a.Type;
                            // 4928

                            a.Type = (a.Type != null && a.Type.ToUpper() == "PORTOUT REFUND APPROVAL") ? Resources.ServicesResources.PORTOUTREFUNDAPPROVAL : a.Type;

                            //POF-6007
                            a.Type = (a.Type != null && a.Type.ToUpper() == "LegalSIMSWAPandROLLBACK") ? "LegalSIMSWAPandROLLBACK" : a.Type;

                        });
                    }
                }
                catch (Exception exlstPendingNames)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exlstPendingNames);
                }
                finally
                {
                    lstPendingNames = null;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - PendingApprovalItems End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                objRequest = null;
                dtCurrentDate = string.Empty;
                dateCount = string.Empty;
                serviceCRM = null;
            }
            if (value != null && value.ToUpper() == "TELEVERIFICATION")
            {
                return View("Televerification", pendAction);
            }
            else
            {
                return View(pendAction);
            }
        }

        [HttpPost]
        public JsonResult PendingApproval(string value, string status)
        {
            PendingActionsResponse pendAction = new PendingActionsResponse();
            PendingActionsRequest objRequest = new PendingActionsRequest();
            string dtCurrentDate = string.Empty;
            string dateCount = string.Empty;
            ServiceInvokeCRM serviceCRM;

            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - PendingApproval Start");
                objRequest.BrandCode = clientSetting.brandCode;
                objRequest.CountryCode = clientSetting.countryCode;
                objRequest.LanguageCode = clientSetting.langCode;
                objRequest.userName = Convert.ToString(Session["UserName"]);
                if (Convert.ToString(Session["IsRootUser"]).ToLower() == "true")
                    objRequest.RoleCatType = "0";
                else
                    objRequest.RoleCatType = "1";

                if (value == null)
                {
                    objRequest.Type = "ALL";
                }
                else
                {
                    objRequest.Type = value.ToUpper();
                }
                if (string.IsNullOrEmpty(status))
                {
                    objRequest.Status = "P";
                }
                else
                {
                    objRequest.Status = status;
                }
                if (objRequest.Type == "TELEVERIFICATION")
                {
                    objRequest.Status = "";
                }
                dtCurrentDate = System.DateTime.Now.ToString("yyyy/MM/dd");
                dateCount = clientSetting.mvnoSettings.paDateRange;
                TimeSpan dateNumber = new TimeSpan(Convert.ToInt16(dateCount), 0, 0, 0);
                DateTime dtFromDate = Convert.ToDateTime(dtCurrentDate) - dateNumber;
                objRequest.FromDate = dtFromDate.ToString("yyyy/MM/dd");
                objRequest.ToDate = dtCurrentDate.ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                pendAction = serviceCRM.ShowPendingActions(objRequest);
                List<PendingApprovalNames> lsPendingNames;
                try
                {
                    if (pendAction.ShowPendingAction != null && pendAction.ShowPendingAction.Count > 0)
                    {
                        lsPendingNames = PendingApprovalList();

                        pendAction.ShowPendingAction.FindAll(a => !string.IsNullOrEmpty(a.submittedDate)).ForEach(b => b.submittedDate = Utility.GetDateconvertion(b.submittedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                        pendAction.ShowPendingAction.FindAll(a => !string.IsNullOrEmpty(a.authorizedDate)).ForEach(b => b.authorizedDate = Utility.GetDateconvertion(b.authorizedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                        pendAction.ShowPendingAction.FindAll(a => !string.IsNullOrEmpty(a.rejectedDate)).ForEach(b => b.rejectedDate = Utility.GetDateconvertion(b.rejectedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));

                       // pendAction.ShowPendingAction.FindAll(a => !string.IsNullOrEmpty(a.submittedDate)).ForEach(b => b.submittedDate = Utility.FormatDateTime(b.submittedDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.yyyyMMdd));
                       // pendAction.ShowPendingAction.FindAll(a => !string.IsNullOrEmpty(a.authorizedDate)).ForEach(b => b.authorizedDate = Utility.FormatDateTime(b.authorizedDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.yyyyMMdd));
                       // pendAction.ShowPendingAction.FindAll(a => !string.IsNullOrEmpty(a.rejectedDate)).ForEach(b => b.rejectedDate = Utility.FormatDateTime(b.rejectedDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.yyyyMMdd));
                       pendAction.ShowPendingAction.FindAll(a => !string.IsNullOrEmpty(a.Type)).ForEach(b => b.Type = lsPendingNames.FirstOrDefault(m => m.Pendingvalue.ToLower() == b.Type.ToLower()).Pendingvalue);
                    }
                    if (pendAction.ShowPendingAction != null && pendAction.ShowPendingAction.Count > 0)
                    {
                        pendAction.ShowPendingAction.ForEach(a =>
                        {
                            a.CurrentStatus = (a.CurrentStatus != null && a.CurrentStatus == "Waiting For 1st Level Approval") ? Resources.HomeResources.WaitingFor1stlevel : a.CurrentStatus;
                            a.CurrentStatus = (a.CurrentStatus != null && a.CurrentStatus == "Waiting For 2nd Level Approval") ? Resources.HomeResources.WaitingFor2ndlevel : a.CurrentStatus;
                            a.CurrentStatus = (a.CurrentStatus != null && a.CurrentStatus == "Approved") ? Resources.HomeResources.typeApproved : a.CurrentStatus;
                            a.CurrentStatus = (a.CurrentStatus != null && a.CurrentStatus == "Rejected") ? Resources.HomeResources.typeRejected : a.CurrentStatus;
                            a.CurrentStatus = (a.CurrentStatus != null && a.CurrentStatus == "HLR FAIL") ? Resources.HomeResources.HLRFail : a.CurrentStatus;
                            a.CurrentStatus = (a.CurrentStatus != null && a.CurrentStatus == "SPA FAIL") ? Resources.HomeResources.SPAFail : a.CurrentStatus;
                            a.CurrentStatus = (a.CurrentStatus != null && a.CurrentStatus == "ITG FAIL") ? Resources.HomeResources.ITGFail : a.CurrentStatus;
                            a.CurrentStatus = (a.CurrentStatus != null && a.CurrentStatus == "ETU FAIL") ? Resources.HomeResources.ETUFail : a.CurrentStatus;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "SIM BLOCK") ? Resources.HomeResources.PASIMBlock : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "SIM UNBLOCK") ? Resources.HomeResources.PASIMUnBlock : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "SWAP IMSI") ? Resources.HomeResources.PASwapIMSI : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "SWAP MSISDN") ? Resources.HomeResources.PASwapMSISDN : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "BALANCE TRANSFER") ? Resources.HomeResources.PABalanceTransfer : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "CREDIT") ? Resources.BillingResources.Credit : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "DEBIT") ? Resources.BillingResources.Debit : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "STAFF TOPUP") ? Resources.HomeResources.PAStaffTopup : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "CHANGE PLAN") ? Resources.HomeResources.PAChangePlan : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "OBA Plan Upgrade/Downgrade") ? Resources.HomeResources.PAOBAPlanUpgrade : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "OBA ADDITIONAL CREDIT") ? Resources.HomeResources.PAOBAAdditionalCredit : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "BUNDLE BUCKET") ? Resources.HomeResources.PABundleBucket : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "ZIPCODE SWAP MSISDN") ? Resources.HomeResources.PAZipcodeSwapMSISDN : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "PARTIAL BALANCE TRANSFER") ? Resources.HomeResources.PAPartialBalanceTransfer : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "SUSPEND") ? Resources.HomeResources.PASuspend : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "RESTORE") ? Resources.HomeResources.PARestore : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "ACTIVATE") ? Resources.HomeResources.PAActivate : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "DEACTIVATE") ? Resources.HomeResources.PADeActivate : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "REACTIVATE LMSISDN") ? Resources.HomeResources.PAReActivateLMSISDN : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "REFUND PAYMENT") ? Resources.HomeResources.PARefundPayment : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "MHA") ? Resources.HomeResources.PAMHA : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "VIP STAFF TOPUP") ? Resources.HomeResources.PAVIPStaffTopup : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "CANCEL AUTO RENEWAL") ? Resources.HomeResources.CancelAutoRenewal : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "BUNDLE TRANSFER") ? Resources.HomeResources.BundleTransfer : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "RLH") ? Resources.HomeResources.RLH : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "IMEI BLOCK") ? Resources.HomeResources.PABlock : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "IMEI UNBLOCK") ? Resources.HomeResources.PAUnBlock : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "DEVICE REFUND PAYMENT") ? "DEVICE REFUND PAYMENT" : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "Cancel Family Plan") ? "Cancel Family Plan" : a.Type;
                            a.Type = (a.Type != null && a.Type.ToUpper() == "Promo Code Mapping") ? "Promo Code Mapping" : a.Type;
                            //4653
                            a.Type = (a.Type != null && a.Type.ToUpper() == "OBA CREDIT LIMIT UPGRADE/DOWNGRADE") ? "OBA CREDIT LIMIT UPGRADE/DOWNGRADE" : a.Type;
                          // 4928
                            a.Type = (a.Type != null && a.Type.ToUpper() == "PORTOUT REFUND APPROVAL") ? "Portout Refund Approval" : a.Type;

                            //POF-6007
                            a.Type = (a.Type != null && a.Type.ToUpper() == "LegalSIMSWAPandROLLBACK") ? "LegalSIMSWAPandROLLBACK" : a.Type;

                        });
                    }
                }
                catch (Exception exlsPendingNames)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exlsPendingNames);
                }
                finally
                {
                    lsPendingNames = null;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - PendingApproval End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                objRequest = null;
            }
            //return Json(pendAction.ShowPendingAction, JsonRequestBehavior.AllowGet);
            return new JsonResult() { Data = pendAction.ShowPendingAction, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetPendingApproval()
        {
            return Json(PendingApprovalList());
        }

        public List<PendingApprovalNames> PendingApprovalList()
        {
            List<PendingApprovalNames> lstPendingNames = new List<PendingApprovalNames>();
            PendingApprovalCountResponse pendingResp = new PendingApprovalCountResponse();
            PendingApprovalNames obj = new PendingApprovalNames();
            ServiceInvokeCRM serviceCRM;
            try
            {

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    PendingApprovalCountRequest pendingReq = new PendingApprovalCountRequest();
                    pendingReq.CountryCode = clientSetting.countryCode;
                    pendingReq.BrandCode = clientSetting.brandCode;
                    pendingReq.LanguageCode = clientSetting.langCode;

                    string dtCurrentDate = System.DateTime.Now.ToString("yyyy/MM/dd");
                    string dateCount = clientSetting.mvnoSettings.paDateRange;
                    TimeSpan dateNumber = new TimeSpan(Convert.ToInt16(dateCount), 0, 0, 0);
                    DateTime dtFromDate = Convert.ToDateTime(dtCurrentDate) - dateNumber;

                    pendingReq.FromDate = dtFromDate.ToString("yyyy/MM/dd");
                    pendingReq.ToDate = dtCurrentDate.ToString();
                    pendingReq.userName = Convert.ToString(Session["UserName"]);
                    if (Convert.ToString(Session["IsRootUser"]).ToLower() == "true")
                        pendingReq.RoleCatType = "0";
                    else
                        pendingReq.RoleCatType = "1";
                    pendingResp = serviceCRM.FetchPendingApprovalCount(pendingReq);

                    if (pendingResp.PendingApprovalCount.Count > 0)
                    {
                        obj = new PendingApprovalNames();
                        obj.PendingName = Resources.HomeResources.PAAll;
                        obj.Pendingvalue = "All";
                        lstPendingNames.Add(obj);
                    }

                    for (var i = 0; i < pendingResp.PendingApprovalCount.Count; i++)
                    {
                        switch (pendingResp.PendingApprovalCount[i].Type)
                        {

                            #region switch cases
                            case "SIM Block":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PASIMBlock;
                                obj.Pendingvalue = "SIM Block";
                                lstPendingNames.Add(obj);
                                break;

                            case "SIM UnBlock":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PASIMUnBlock;
                                obj.Pendingvalue = "SIM UnBlock";
                                lstPendingNames.Add(obj);
                                break;

                            case "Swap IMSI":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PASwapIMSI;
                                obj.Pendingvalue = "Swap IMSI";
                                lstPendingNames.Add(obj);
                                break;

                            case "Swap MSISDN":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PASwapMSISDN;
                                obj.Pendingvalue = "Swap MSISDN";
                                lstPendingNames.Add(obj);
                                break;

                            case "Balance Transfer":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PABalanceTransfer;
                                obj.Pendingvalue = "Balance Transfer";
                                lstPendingNames.Add(obj);
                                break;

                            case "Credit":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.BillingResources.Credit;
                                obj.Pendingvalue = "Credit";
                                lstPendingNames.Add(obj);
                                break;

                            case "Debit":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.BillingResources.Debit;
                                obj.Pendingvalue = "Debit";
                                lstPendingNames.Add(obj);
                                break;

                            case "Staff Topup":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PAStaffTopup;
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PAStaffTopup;
                                obj.Pendingvalue = "Staff Topup";
                                lstPendingNames.Add(obj);
                                break;

                            case "Change Plan":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PAChangePlan;
                                obj.Pendingvalue = "Change Plan";
                                lstPendingNames.Add(obj);
                                break;

                            case "OBA Plan Upgrade/Downgrade":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PAOBAPlanUpgrade;
                                obj.Pendingvalue = "OBA Plan Upgrade/Downgrade";
                                lstPendingNames.Add(obj);
                                break;

                            case "OBA Additional Credit":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PAOBAAdditionalCredit;
                                obj.Pendingvalue = "OBA Additional Credit";
                                lstPendingNames.Add(obj);
                                break;

                            case "Bundle Bucket":
                                pendingResp.PendingApprovalCount[i].Desc = Resources.HomeResources.PABundleBucket;
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PABundleBucket;
                                obj.Pendingvalue = "Bundle Bucket";
                                lstPendingNames.Add(obj);
                                break;

                            case "Zipcode Swap MSISDN":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PAZipcodeSwapMSISDN;
                                obj.Pendingvalue = "Zipcode Swap MSISDN";
                                lstPendingNames.Add(obj);
                                break;

                            case "Partial Balance Transfer":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PAPartialBalanceTransfer;
                                obj.Pendingvalue = "Partial Balance Transfer";
                                lstPendingNames.Add(obj);
                                break;

                            case "Suspend":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PASuspend;
                                obj.Pendingvalue = "Suspend";
                                lstPendingNames.Add(obj);
                                break;

                            case "Restore":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PARestore;
                                obj.Pendingvalue = "Restore";
                                lstPendingNames.Add(obj);
                                break;

                            case "Activate":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PAActivate;
                                obj.Pendingvalue = "Activate";
                                lstPendingNames.Add(obj);
                                break;

                            case "DeActivate":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PADeActivate;
                                obj.Pendingvalue = "DeActivate";
                                lstPendingNames.Add(obj);
                                break;

                            case "ReActivate LMSISDN":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PAReActivateLMSISDN;
                                obj.Pendingvalue = "ReActivate LMSISDN";
                                lstPendingNames.Add(obj);
                                break;

                            case "Refund Payment":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PARefundPayment;
                                obj.Pendingvalue = "Refund Payment";
                                lstPendingNames.Add(obj);
                                break;

                            case "MHA":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PAMHA;
                                obj.Pendingvalue = "MHA";
                                lstPendingNames.Add(obj);
                                break;

                            case "VIP Staff Topup":
                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PAVIPStaffTopup;
                                obj.Pendingvalue = "VIP Staff Topup";
                                lstPendingNames.Add(obj);
                                break;

                            case "Cancel Auto Renewal":

                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.CancelAutoRenewal;
                                obj.Pendingvalue = "Cancel Auto Renewal";
                                lstPendingNames.Add(obj);
                                break;
                            case "Bundle Transfer":

                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.BundleTransfer;
                                obj.Pendingvalue = "Bundle Transfer";
                                lstPendingNames.Add(obj);
                                break;
                            case "RLH":

                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.RLH;
                                obj.Pendingvalue = "RLH";
                                lstPendingNames.Add(obj);
                                break;


                            case "IMEI Block":

                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PABlock;
                                obj.Pendingvalue = "IMEI Block";
                                lstPendingNames.Add(obj);
                                break;
                            case "IMEI Unblock":

                                obj = new PendingApprovalNames();
                                obj.PendingName = Resources.HomeResources.PAUnBlock;
                                obj.Pendingvalue = "IMEI Unblock";
                                lstPendingNames.Add(obj);
                                break;

                            case "Device Refund Payment":

                                obj = new PendingApprovalNames();
                                obj.PendingName = "Device Refund Payment";
                                obj.Pendingvalue = "Device Refund Payment";
                                lstPendingNames.Add(obj);
                                break;

                            case "Cancel Family Plan":

                                obj = new PendingApprovalNames();
                                obj.PendingName = "Cancel Family Plan";
                                obj.Pendingvalue = "Cancel Family Plan";
                                lstPendingNames.Add(obj);
                                break;

                            case "Promo Code Mapping":

                                obj = new PendingApprovalNames();
                                obj.PendingName = "Promo Code Mapping";
                                obj.Pendingvalue = "Promo Code Mapping";
                                lstPendingNames.Add(obj);
                                break;    
                                //4653
                            case "OBA Credit Limit Upgrade/Downgrade":
                                
                                obj = new PendingApprovalNames();
                                obj.PendingName = "OBA Credit Limit Upgrade/Downgrade";
                                obj.Pendingvalue = "OBA Credit Limit Upgrade/Downgrade";
                                lstPendingNames.Add(obj);
                                break;
                            // 4928
                            case "Portout Refund Approval":
                                obj = new PendingApprovalNames();
                                obj.PendingName = "Portout Refund Approval";
                                obj.Pendingvalue = "Portout Refund Approval";
                                lstPendingNames.Add(obj);
                                break; 

                            //POF-6007
                            // 4928
                            case "LegalSIMSWAPandROLLBACK":
                                obj = new PendingApprovalNames();
                                obj.PendingName = "LegalSIMSWAPandROLLBACK";
                                obj.Pendingvalue = "LegalSIMSWAPandROLLBACK";
                                lstPendingNames.Add(obj);
                                break;
                            default:
                                break;
                            #endregion
                        }
                    }
                
                return lstPendingNames.OrderBy(m => m.PendingName != "All").ThenBy(m => m.PendingName).ToList();
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return lstPendingNames.OrderBy(m => m.PendingName != "All").ThenBy(m => m.PendingName).ToList();
            }
            finally
            {
                serviceCRM = null;
                //lstPendingNames = null;
                //pendingResp = null;
            }


        }

        public JsonResult SwapRetryFetch(string Msisdn, string PAType, string PAId)
        {
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
            PendingDetailsRequest req = new PendingDetailsRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - SwapRetryFetch Start");
                req.CountryCode = clientSetting.countryCode;
                req.BrandCode = clientSetting.brandCode;
                req.LanguageCode = clientSetting.langCode;
                req.MSISDN = Msisdn;
                req.Type = PAType;
                req.Id = PAId;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetPendingDetails(req);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - SwapRetryFetch End");
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
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MsisdnSwapRetrySubmit(string reqId, string PaComments,string Areacode)
        {
            SwapMSISDNResponse objres = new SwapMSISDNResponse();
            SwapMSISDNRequest objreq = new SwapMSISDNRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - MsisdnSwapRetrySubmit Start");
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.ReqID = reqId;
                objreq.RequestBy = Convert.ToString(Session["UserName"]);
                objreq.adminComment = PaComments;
                objreq.Mode = "FA";
                // altan bug
                objreq.AreaCode = Areacode;
                objreq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                objreq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.CRMSwapMSISDN(objreq);
                ///FRR--3083
                if (objres != null && objres.ResponseDetails != null && objres.ResponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MsisdnSwap_" + objres.ResponseDetails.ResponseCode);
                    objres.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - MsisdnSwapRetrySubmit End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objreq = null;
                serviceCRM = null;
            }
            return Json(objres, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ImsiSwapRetrySubmit(string reqId, string PaComments, string PanNumber)
        {
            SwapIMSIResponse objres = new SwapIMSIResponse();
            SwapIMSIRequest objreq = new SwapIMSIRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - ImsiSwapRetrySubmit Start");
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.ReqID = reqId;
                objreq.RequestBy = Convert.ToString(Session["UserName"]);
                objreq.adminComment = PaComments;
                objreq.Mode = "FA";
                objreq.PanNumber = PanNumber;
                objreq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                objreq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.CRMSwapIMSI(objreq);
                ///FRR--3083
                if (objres != null && objres.ResponseDetails != null && objres.ResponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("IMSISwap_" + objres.ResponseDetails.ResponseCode);
                    objres.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - ImsiSwapRetrySubmit End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objreq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(objres, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PaymentRefundDetails(string reqId, string Msisdn)
        {
            PaymentRefundResponse paymentRefundResp = new PaymentRefundResponse();
            PaymentRefundRequest paymentRefundReq = new PaymentRefundRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - PaymentRefundDetails Start");
                paymentRefundReq.CountryCode = clientSetting.countryCode;
                paymentRefundReq.BrandCode = clientSetting.brandCode;
                paymentRefundReq.LanguageCode = clientSetting.langCode;
                paymentRefundReq.requestID = reqId;
                paymentRefundReq.mobileNumber = Msisdn;
                paymentRefundReq.mode = "Q";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                paymentRefundResp = serviceCRM.PaymentRefundCRM(paymentRefundReq);
                try
                {
                    if (paymentRefundResp.refundSubmittedRecords != null && paymentRefundResp.refundSubmittedRecords.Count > 0)
                    {

                        paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.transactionDate)).ForEach(b => b.transactionDate = Utility.GetDateconvertion(b.transactionDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                        paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.expiryDate)).ForEach(b => b.expiryDate = Utility.GetDateconvertion(b.expiryDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                        paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.submittedOn)).ForEach(b => b.submittedOn = Utility.GetDateconvertion(b.submittedOn, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                        paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.authorizedOn)).ForEach(b => b.authorizedOn = Utility.GetDateconvertion(b.authorizedOn, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                        paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.rejectedOn)).ForEach(b => b.rejectedOn = Utility.GetDateconvertion(b.rejectedOn, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                        paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.reinitiationDate)).ForEach(b => b.reinitiationDate = Utility.GetDateconvertion(b.reinitiationDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));


                        //paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.transactionDate)).ForEach(b => b.transactionDate = Utility.FormatDateTime(b.transactionDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.MMddyyyy));
                        //paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.expiryDate)).ForEach(b => b.expiryDate = Utility.FormatDateTime(b.expiryDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.MMddyyyy));
                        //paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.submittedOn)).ForEach(b => b.submittedOn = Utility.FormatDateTime(b.submittedOn, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.MMddyyyy));
                        //paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.authorizedOn)).ForEach(b => b.authorizedOn = Utility.FormatDateTime(b.authorizedOn, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.MMddyyyy));
                        //paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.rejectedOn)).ForEach(b => b.rejectedOn = Utility.FormatDateTime(b.rejectedOn, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.MMddyyyy));
                        //paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.reinitiationDate)).ForEach(b => b.reinitiationDate = Utility.FormatDateTime(b.reinitiationDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.MMddyyyy));
                    }
                }
                catch (Exception expaymentRefundResp)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, expaymentRefundResp);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - PaymentRefundDetails End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                paymentRefundReq = null;
                serviceCRM = null;
            }
            return Json(paymentRefundResp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BundleTransferDetails(string reqId)
        {
            BundleTransferResp ObjRes = new BundleTransferResp();
            BundleTransferReq ObjReq = new BundleTransferReq();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - BundleTransferDetails Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.ReqId = reqId;
                ObjReq.Mode = "GETBYID";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMBundleTransfer(ObjReq);
                try
                {
                    if (ObjRes != null && ObjRes.BundleTransferDetailIDResp != null)
                    {
                        if (!string.IsNullOrEmpty(ObjRes.BundleTransferDetailIDResp.TransactionDate))
                        {
                            ObjRes.BundleTransferDetailIDResp.TransactionDate = Utility.GetRRBSDateconvertion(ObjRes.BundleTransferDetailIDResp.TransactionDate, "yyyy/mm/dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                        }
                        if (!string.IsNullOrEmpty(ObjRes.BundleTransferDetailIDResp.AuthDate))
                        {
                            ObjRes.BundleTransferDetailIDResp.AuthDate = Utility.GetDateconvertion(ObjRes.BundleTransferDetailIDResp.AuthDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                        }
                        if (!string.IsNullOrEmpty(ObjRes.BundleTransferDetailIDResp.RequestedDate))
                        {
                            ObjRes.BundleTransferDetailIDResp.RequestedDate = Utility.GetDateconvertion(ObjRes.BundleTransferDetailIDResp.RequestedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                        }
                    }
                }
                catch (Exception exBundleTransferDetailIDResp)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exBundleTransferDetailIDResp);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - BundleTransferDetails End");
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
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Televerify(string reqId, string Msisdn)
        {
            TeleverificationResponse ObjRes = new TeleverificationResponse();
            TeleverificationRequest ObjReq = new TeleverificationRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.reqId = reqId;
                ObjReq.Msisdn = Msisdn;
                ObjReq.SubmittedBy = Convert.ToString(Session["UserName"]);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.Televerify(ObjReq);

                    if (ObjRes != null && ObjRes.responseDetails != null && ObjRes.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Televerification_" + ObjRes.responseDetails.ResponseCode);
                        ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;
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
                //ObjRes = null;
                ObjReq = null;
                serviceCRM = null;
            }

        }


        [HttpPost]
        public JsonResult RefundPaymentRetryRenewal(string paymentRefundJson)
        {
            PaymentRefundResponse paymentRefundResp = new PaymentRefundResponse();
            PaymentRefundRequest paymentRefundReq;
            string resID = "RefundPayment_Req";
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - RefundPaymentRetryRenewal Start");
                paymentRefundReq = JsonConvert.DeserializeObject<PaymentRefundRequest>(paymentRefundJson);
                paymentRefundReq.CountryCode = clientSetting.countryCode;
                paymentRefundReq.BrandCode = clientSetting.brandCode;
                paymentRefundReq.LanguageCode = clientSetting.langCode;
                paymentRefundReq.authorizeInfo.submittedBy = Session["UserName"].ToString();
                paymentRefundReq.authorizeInfo.authorizedBy = Session["UserName"].ToString();
                paymentRefundReq.authorizeInfo.rejectedBy = Session["UserName"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                paymentRefundResp = serviceCRM.PaymentRefundCRM(paymentRefundReq);
                if (paymentRefundResp != null && paymentRefundResp.responseDetails != null && paymentRefundResp.responseDetails.ResponseCode != null)
                {
                    if (paymentRefundResp.responseDetails.ResponseCode == "100028")
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RefundTopupBundlePayment_" + "A" + "_" + paymentRefundResp.responseDetails.ResponseCode);
                        paymentRefundResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? paymentRefundResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - RefundPaymentRetryRenewal End");
            }
            catch (Exception eX)
            {
                paymentRefundResp.responseDetails = new CRMResponse();
                paymentRefundResp.responseDetails.ResponseCode = "9";
                paymentRefundResp.responseDetails.ResponseDesc = eX.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                paymentRefundReq = null;
                resID = string.Empty;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(paymentRefundResp);
        }

        [HttpPost]
        public JsonResult SwapImsiDetails(string reqId, string Msisdn)
        {
            PendingDetailsRequest objReq = new PendingDetailsRequest();
            PendingDetailsResponce objRes = new PendingDetailsResponce();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - SwapImsiDetails Start");
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.MSISDN = Msisdn;
                objReq.Type = "SWAP IMSI";
                objReq.Id = reqId;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.CRMGetPendingDetails(objReq);
                try
                {
                    if (objRes != null && objRes.PendingDetails.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(objRes.PendingDetails[0].TopupAmt))
                        {
                            objRes.PendingDetails[0].TopupAmt = Utility.GetDateconvertion(objRes.PendingDetails[0].TopupAmt, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                        }
                        if (!string.IsNullOrEmpty(objRes.PendingDetails[0].PlanID))
                        {
                            objRes.PendingDetails[0].PlanID = Utility.GetDateconvertion(objRes.PendingDetails[0].PlanID, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                        }
                    }
                }
                catch (Exception exPendingDetails)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exPendingDetails);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - SwapImsiDetails End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objReq = null;
                serviceCRM = null;
            }
            return Json(objRes, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SwapMsisdnDetails(string reqId, string Msisdn)
        {
            PendingDetailsRequest objReq = new PendingDetailsRequest();
            PendingDetailsResponce objRes = new PendingDetailsResponce();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - SwapMsisdnDetails Start");
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.MSISDN = Msisdn;
                objReq.Type = "SWAP MSISDN";
                objReq.Id = reqId;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.CRMGetPendingDetails(objReq);
                try
                {
                    if (objRes != null && objRes.PendingDetails.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(objRes.PendingDetails[0].TopupAmt))
                        {
                            objRes.PendingDetails[0].TopupAmt = Utility.GetDateconvertion(objRes.PendingDetails[0].TopupAmt, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                        }
                        if (!string.IsNullOrEmpty(objRes.PendingDetails[0].PlanID))
                        {
                            objRes.PendingDetails[0].PlanID = Utility.GetDateconvertion(objRes.PendingDetails[0].PlanID, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                        }
                    }
                }
                catch (Exception exPendingDetails)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exPendingDetails);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "AdminController - SwapMsisdnDetails End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objReq = null;
                serviceCRM = null;
            }
            return Json(objRes, JsonRequestBehavior.AllowGet);
        }

    }

    public class PendingApprovalNames
    {
        public String PendingName { get; set; }
        public String Pendingvalue { get; set; }

    }

}


