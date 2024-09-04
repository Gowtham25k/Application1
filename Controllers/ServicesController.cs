using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using CRM.Models;
using Service;
using ServiceCRM;
using System.Linq;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using System.Data.OleDb;
using System.Web;
using System.Data;
using System.IO;

namespace CRM.Controllers
{
    [ValidateState]
    public class ServicesController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();

     
        public ActionResult BalanceTransfer()
        {
            BalTrans balTrans = new BalTrans();
            List<ModelPartialTransferReason> partialTransferReason = new List<ModelPartialTransferReason>();
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            PendingDetailsRequest req = new PendingDetailsRequest();
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
            PartialTransferReasonResponse objPartialreason = new PartialTransferReasonResponse();
            CRMResponse objRes;
            CRMBase cbase;
            ServiceInvokeCRM serviceCRM;
            string ResourceMsg = string.Empty;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - BalanceTransfer Start");
                menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "Services_BalanceTransfer").ToList();
                objRes = Utility.checkValidSubscriber("", clientSetting);
                //if (Session["PAType"] != null && (Convert.ToString(Session["PAType"]) == "BALANCE TRANSFER" || Convert.ToString(Session["PAType"]) == "PARTIAL BALANCE TRANSFER"))
                if (objRes.ResponseCode == "0" || objRes.ResponseCode == "101" || (Session["PAType"] != null && (Convert.ToString(Session["PAType"]) == "BALANCE TRANSFER")))
                {
                    balTrans.ResponseCode = "0";
                    try
                    {
                        ViewBag.Approve = "Approve";
                        ViewBag.Reject = "Reject";
                        ViewBag.Reset = "Reset";
                        ViewBag.Submit = "Submit";
                        cbase = new CRMBase();
                        cbase.CountryCode = clientSetting.countryCode;
                        cbase.BrandCode = clientSetting.brandCode;
                        cbase.LanguageCode = clientSetting.langCode;
                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        balTrans.ticketCRM.dicTicketReasons = Utility.DataTableToDictionary(Utility.GetDropdownMasterFromDB("tbl_balance_transfer_reason"));
                        objPartialreason = serviceCRM.PartialBalanceTransferReasonCRM(cbase);
                        if (objPartialreason != null && objPartialreason.responseDetails != null && objPartialreason.responseDetails.ResponseCode == "0")
                        {
                            for (int i = 0; i < objPartialreason.partialTransferReason.Count; i++)
                            {
                                ResourceMsg = Resources.DropdownResources.ResourceManager.GetString(objPartialreason.partialTransferReason[i].name.Replace(" ", string.Empty));
                                objPartialreason.partialTransferReason[i].name = string.IsNullOrEmpty(ResourceMsg) ? objPartialreason.partialTransferReason[i].name : ResourceMsg;
                            }
                        }
                        if (objPartialreason != null)
                        {
                            partialTransferReason = objPartialreason.partialTransferReason
                                .Select(x => new ModelPartialTransferReason
                                {
                                    id = x.id,
                                    name = x.name,
                                }).ToList();
                        }
                        //balTrans.partialTransferReason = objPartialreason;
                        //for(int i=0; i<= objPartialreason.partialTransferReason.Count; i++)
                        //{
                        //    balTrans = new BalTrans();
                        //    balTrans .partialTransferReason= objPartialreason.partialTransferReason[i].id.ToString();
                        //    balTrans.partialTransferReason[i].name = objPartialreason.partialTransferReason[i].name.ToString();                   
                        //}
                        if (Session["PAType"] != null && (Convert.ToString(Session["PAType"]) == "BALANCE TRANSFER" || Convert.ToString(Session["PAType"]) == "PARTIAL BALANCE TRANSFER"))
                        {
                            // PendingDetails request
                            req.CountryCode = clientSetting.countryCode;
                            req.BrandCode = clientSetting.brandCode;
                            req.LanguageCode = clientSetting.langCode;
                            req.MSISDN = Convert.ToString(Session["MobileNumber"]);
                            req.Type = Convert.ToString(Session["PAType"]);
                            balTrans.TransferType = Convert.ToString(Session["PAType"]);
                            req.Id = Convert.ToString(Session["PAId"]);
                            serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                            ObjRes = serviceCRM.CRMGetPendingDetails(req);
                            ///FRR--3083
                            if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                            {
                                errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManaageOBABundle_" + ObjRes.ResponseDetails.ResponseCode);
                                ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                            }
                            ///FRR--3083
                            if (ObjRes != null && ObjRes.PendingDetails != null)
                            {
                                if (ObjRes.PendingDetails.Count > 0)
                                {
                                    balTrans.PaType = true;
                                    balTrans.Id = req.Id;
                                    balTrans.toMSISDN = ObjRes.PendingDetails[0].NewMSISDN;
                                    if (ObjRes.PendingDetails[0].TicketId != "0")
                                    {
                                        balTrans.ticketCRM.ticketID = ObjRes.PendingDetails[0].TicketId;
                                    }
                                    else
                                    {
                                        balTrans.ticketCRM.ticketID = string.Empty;
                                    }
                                    balTrans.ticketCRM.ticketComment = ObjRes.PendingDetails[0].Comments;
                                    balTrans.ticketCRM.ticketReason = ObjRes.PendingDetails[0].Reason;
                                    ViewBag.Amount = ObjRes.PendingDetails[0].TopupAmt;
                                    ViewBag.Disabled = "true";
                                    ViewBag.Readonly = "true";
                                }
                            }
                        }
                        else
                        {
                            // need to delete below condition before giving to testing
                            if (Session["MobileNumber"] != null)
                            {
                                //balTrans.toMSISDN = Convert.ToString(Session["MobileNumber"]);
                                ViewBag.Disabled = "false";
                                ViewBag.Readonly = "false";
                            }
                            else
                            {
                                ViewBag.toMSISDN = string.Empty;
                            }
                            balTrans.PaType = false;
                        }
                        // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                        Session["PAType"] = null;
                    }
                    catch (Exception ex)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                    }
                }
                else
                {
                    balTrans.ResponseCode = "1";
                    balTrans.ResponseDesc = objRes.ResponseDesc;
                }
                balTrans.partialTransferReason = partialTransferReason;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - BalanceTransfer End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                objRes = null;
                cbase = null;
                serviceCRM = null;
                ResourceMsg = string.Empty;
                errorInsertMsg = string.Empty;
                partialTransferReason = null;
                menu = null;
                req = null;
                ObjRes = null;
                objPartialreason = null;
            }
            return View(balTrans);
        }

        [HttpPost]
        public JsonResult BalanceTransfer(string jsonBalTrans)
        {
            SelectListItem btItem = new SelectListItem();
            BalTrans balTrans = new BalTrans();
            FetchTicketDetailsRequest req = new FetchTicketDetailsRequest();
            GetBalanceTransferInsertUpdateRequest balTransReq = new GetBalanceTransferInsertUpdateRequest();
            ServiceInvokeCRM serviceCRM;
            List<ServiceCRM.Menu> menu;
            BalanceTransferInsertUpdateResponse balTransResp;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServicesController - BalanceTransfer Start");
                balTrans = new JavaScriptSerializer().Deserialize<BalTrans>(jsonBalTrans);
                req.StatusID = -1;
                req.MobileNumber = Convert.ToString(Session["MobileNumber"]);
                req.RecordCount = !string.IsNullOrEmpty(clientSetting.mvnoSettings.ticketRecordCount) ? Convert.ToInt32(clientSetting.mvnoSettings.ticketRecordCount) : 0;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                balTransReq.CountryCode = clientSetting.countryCode;
                balTransReq.BrandCode = clientSetting.brandCode;
                balTransReq.LanguageCode = clientSetting.langCode;
                balTransReq.MsisdnFrom = Session["MobileNumber"].ToString();
                balTransReq.MsisdnTo = balTrans.toMSISDN;
                balTransReq.TticketId = balTrans.ticketCRM.ticketID;
                balTransReq.Reason = balTrans.ticketCRM.ticketReason;
                balTransReq.Comments = balTrans.ticketCRM.ticketComment;
                if (balTrans.Id != null)
                {
                    if (balTrans.Id.Trim().Length != 0) { balTransReq.Id = balTrans.Id.ToString(); }
                }
                balTransReq.Username = Session["UserName"].ToString();
                balTransReq.SubValue = clientSetting.preSettings.balTransSubValue;


                #region FRR : 4376 : ATR_ID : V_1.1.8.0
                balTransReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                balTransReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                #endregion

                // if ((bool)Session["IsAdmin"] || Convert.ToString(Session["ROLE_CAT_TYPE"]) == "2")
                menu = new List<ServiceCRM.Menu>();
                menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "Services_BalanceTransfer").ToList();
                if (menu[0] != null && (menu[0].Approval1.ToUpper() == "TRUE" || menu[0].DirectApproval.ToUpper() == "TRUE"))
                {
                    balTransReq.AuthBy = Session["UserName"].ToString();
                }
                if (balTrans.Status == "0" || balTrans.Status == null)
                {
                    //if ((bool)Session["IsAdmin"] || Convert.ToString(Session["ROLE_CAT_TYPE"]) == "2")
                    if (menu[0].DirectApproval.ToUpper() == "TRUE")
                    {
                        balTransReq.RequestType = "A";
                        balTransReq.Status = "0";
                    }
                    else
                    {
                        balTransReq.RequestType = "N";
                        balTransReq.Status = "0";
                    }
                }
                else
                {
                    balTransReq.RequestType = balTrans.RequestType;
                    balTransReq.Status = balTrans.Status;
                }
                balTransReq.PARequestType = Resources.HomeResources.PABalanceTransfer;
                balTransResp = new BalanceTransferInsertUpdateResponse();
                balTransResp = serviceCRM.GetCRMBalanceTransferInsertUpdate(balTransReq);
                ///FRR--3083
                if (balTransResp != null && balTransResp.ResponseDetails != null && balTransResp.ResponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BalanceTransfer_" + balTransResp.ResponseDetails.ResponseCode);
                    balTransResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? balTransResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                    if (balTransResp.ResponseDetails.ResponseCode == "0" && balTransReq.RequestType == "N")
                    {
                        balTransResp.ResponseDetails.ResponseDesc = Resources.ErrorResources.BalanceTransfer_01;//submitted
                    }
                    if (balTransResp.ResponseDetails.ResponseCode == "0" && balTransReq.RequestType == "U")
                    {
                        balTransResp.ResponseDetails.ResponseDesc = Resources.ErrorResources.BalanceTransfer_00;//authorised
                    }
                    if (balTransResp.ResponseDetails.ResponseCode == "0" && balTransReq.RequestType == "A")
                    {
                        balTransResp.ResponseDetails.ResponseDesc = Resources.ErrorResources.BalanceTransfer_00;//authorised
                    }
                    if (balTransResp.ResponseDetails.ResponseCode == "0" && balTransReq.RequestType == "R")
                    {
                        balTransResp.ResponseDetails.ResponseDesc = Resources.ErrorResources.BalanceTransfer_02;//Rejected
                    }
                }
                ///FRR--3083
                if (balTransResp != null && balTransResp.ResponseDetails != null)
                {
                    if (balTransResp.ResponseDetails.ResponseCode == "0")
                    {
                        btItem.Value = balTransResp.ResponseDetails.ResponseCode;
                        btItem.Text = balTransResp.ResponseDetails.ResponseDesc;
                    }
                    else
                    {
                        btItem.Value = balTransResp.ResponseDetails.ResponseCode;
                        btItem.Text = balTransResp.ResponseDetails.ResponseDesc;
                    }
                }
                else
                {
                    btItem.Value = "1";
                    btItem.Text = "";
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServicesController - BalanceTransfer End");
            }
            catch (Exception eX)
            {
                btItem.Value = "1";
                btItem.Text = eX.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                balTrans = null;
                req = null;
                balTransReq = null;
                serviceCRM = null;
                menu = null;
                balTransResp = null;
                errorInsertMsg = string.Empty;
            }
            return Json(btItem, JsonRequestBehavior.AllowGet);
        }
        public JsonResult PartialBalanceTransfer(string Prty)
        {
            SelectListItem selList = new SelectListItem();
            CRMResponse objResponse = new CRMResponse();
            PartialBalanceTransferRequest partialbaltrans = new PartialBalanceTransferRequest();
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            string isMode = string.Empty;
            ServiceInvokeCRM serviceCRM;
            string[] splitvalues;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServicesController - PartialBalanceTransfer Start");
                partialbaltrans = new JavaScriptSerializer().Deserialize<PartialBalanceTransferRequest>(Prty);
                partialbaltrans.CountryCode = clientSetting.countryCode;
                partialbaltrans.BrandCode = clientSetting.brandCode;
                partialbaltrans.LanguageCode = clientSetting.langCode;
                partialbaltrans.msisdnFrom = Convert.ToString(Session["MobileNumber"]);
                partialbaltrans.userName = Convert.ToString(Session["UserName"]);
                #region FRR : 4376 : ATR_ID : V_1.1.8.0
                partialbaltrans.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                partialbaltrans.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                #endregion

                menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "Services_BalanceTransfer").ToList();
                //partialbaltrans.adminNormal = ((bool)Session["IsAdmin"] || Convert.ToString(Session["ROLE_CAT_TYPE"]) == "2") ? "A" : "N";
                partialbaltrans.adminNormal = (partialbaltrans.mode != "I") ? "A" : "N";
                //partialbaltrans.adminNormal = (menu[0] != null && (menu[0].Approval1.ToUpper() == "TRUE" || menu[0].DirectApproval.ToUpper() == "TRUE" )) ? "A" : "N";
                //A-Admin   N-Normal user or Agent
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                partialbaltrans.PARequestType = Resources.HomeResources.PAPartialBalanceTransfer;
                objResponse = serviceCRM.PartialBalanceTransferCRM(partialbaltrans);
                if (objResponse != null && objResponse.ResponseCode != null)
                {
                    splitvalues = objResponse.ResponseDesc.Split('|');
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PartialBalanceTransfer_" + objResponse.ResponseCode);
                    objResponse.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objResponse.ResponseDesc : errorInsertMsg;
                    if (objResponse.ResponseCode == "280")
                    {
                        objResponse.ResponseDesc = objResponse.ResponseDesc + "." + " " + Resources.ServicesResources.Oldbalance + ":  " + clientSetting.mvnoSettings.currencySymbol + " " + splitvalues[1].ToString() + ", " + Resources.ServicesResources.Newbalance + ": " + clientSetting.mvnoSettings.currencySymbol + " " + splitvalues[2].ToString();
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServicesController - PartialBalanceTransfer End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                isMode = string.Empty;
                serviceCRM = null;
                splitvalues = null;
                errorInsertMsg = string.Empty;
                selList = null;
                partialbaltrans = null;
                menu = null;
            }
            return Json(objResponse, JsonRequestBehavior.AllowGet);
        }

      
        public ActionResult RedeemRewards()
        {
            RedeemRewardsResponse rewardResp = new RedeemRewardsResponse();
            List<UpcomingDetails> objList = new List<UpcomingDetails>();
            RedeemRewardsRequest objreq = new RedeemRewardsRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objreq.CountryCode = clientSetting.countryCode;
                    objreq.BrandCode = clientSetting.brandCode;
                    objreq.LanguageCode = clientSetting.langCode;
                    objreq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    objreq.PlanId = Convert.ToString(Session["PlanId"]);
                    objreq.OldMSISDN = Convert.ToString(Session["SwapMSISDN"]);
                    objreq.mode = "Q";

                    rewardResp = serviceCRM.RedeemRewards(objreq);

                    try
                    {
                        if (rewardResp != null && !string.IsNullOrEmpty(rewardResp.PointsExpiry))
                        {
                            rewardResp.PointsExpiry = Utility.GetDateconvertion(rewardResp.PointsExpiry, "dd-MM-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                        }

                        if (rewardResp != null && rewardResp.availableDetails != null && rewardResp.availableDetails.Count > 0)
                        {
                            UpcomingDetails obj = null;
                            DateTime dtexpirydate = new DateTime();
                            for (int i = 0; i < rewardResp.availableDetails.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(rewardResp.availableDetails[i].ExpiryDate))
                                {
                                    dtexpirydate = DateTime.ParseExact(rewardResp.availableDetails[i].ExpiryDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
                                    obj = new UpcomingDetails();
                                    obj.PurchaseType = rewardResp.availableDetails[i].PurchaseType;
                                    obj.Points = rewardResp.availableDetails[i].Points;
                                    obj.CreditWorth = rewardResp.availableDetails[i].CreditWorth;
                                    obj.ExpiryDate = dtexpirydate;
                                    objList.Add(obj);
                                }
                            }

                            if (objList.Count > 0)
                            {
                                double ExpiryPoints = 0.00, Expiryvalue = 0.00;
                                var query = objList.OrderBy(x => x.ExpiryDate).FirstOrDefault();
                                var Filter = objList.Where(x => x.ExpiryDate == query.ExpiryDate).ToList();
                                foreach (var test in Filter)
                                {
                                    ExpiryPoints = ExpiryPoints + Convert.ToDouble(test.Points);
                                    Expiryvalue = Expiryvalue + Convert.ToDouble(test.CreditWorth);
                                }

                                rewardResp.upcomingExpiryPoints = Convert.ToString(ExpiryPoints);
                                rewardResp.upcomingExpiryValue = Convert.ToString(Expiryvalue);
                                rewardResp.UpcomingExpiry = query.ExpiryDate.ToString("yyyy-MM-dd");
                                rewardResp.UpcomingExpiry = Utility.GetDateconvertion(rewardResp.UpcomingExpiry, "yyyy-MM-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                            }

                            if (rewardResp != null && rewardResp.expiredDetails != null && rewardResp.expiredDetails.Count > 0)
                            {
                                rewardResp.expiredDetails.ForEach(m => m.expiredOn = Utility.FormatDateTime(m.expiredOn, clientSetting.mvnoSettings.dateTimeFormat));
                                rewardResp.expiredDetails.ForEach(m => m.topupDate = Utility.FormatDateTime(m.topupDate, clientSetting.mvnoSettings.dateTimeFormat));
                            }

                            if (rewardResp != null && rewardResp.redeemedDetails != null && rewardResp.redeemedDetails.Count > 0)
                            {
                                rewardResp.redeemedDetails.ForEach(m => m.redeemeddate = Utility.FormatDateTime(m.redeemeddate, clientSetting.mvnoSettings.dateTimeFormat));
                            }

                            rewardResp.availableDetails.ForEach(m => m.ExpiryDate = Utility.GetDateconvertion(m.ExpiryDate, "dd-MM-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat));
                        }

                    }
                    catch
                    {
                    }
                
                return View(rewardResp);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(rewardResp);
            }
            finally
            {
                // rewardResp = null;
                objList = null;
                objreq = null;
                serviceCRM = null;
            }

        }

        [HttpPost]
        public JsonResult RedeemRewards(string jsonSelProducts)
        {
            RedeemRewardsResponse rewardResp = new RedeemRewardsResponse();
            RedeemRewardsRequest rewardReq = JsonConvert.DeserializeObject<RedeemRewardsRequest>(jsonSelProducts);
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    rewardReq.CountryCode = clientSetting.countryCode;
                    rewardReq.BrandCode = clientSetting.brandCode;
                    rewardReq.LanguageCode = clientSetting.langCode;
                    rewardReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    rewardReq.PlanId = Convert.ToString(Session["PlanId"]);
                    rewardReq.UserName = Convert.ToString(Session["UserName"]);
                    rewardReq.IMSI = Convert.ToString(Session["IMSI"]);

                    rewardResp = serviceCRM.RedeemRewards(rewardReq);

                    try
                    {
                        if (rewardResp != null && rewardResp.responseDetails != null && rewardResp.responseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Rewards_" + rewardResp.responseDetails.ResponseCode);
                            rewardResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? rewardResp.responseDetails.ResponseDesc : errorInsertMsg;
                            if (!string.IsNullOrEmpty(rewardResp.SmsStatus))
                            {
                                if (rewardResp.SmsStatus == "1")
                                {
                                    rewardResp.SmsStatus = Resources.ErrorResources.ResourceManager.GetString("RLH_SMS_1");
                                }
                                else if (rewardResp.SmsStatus == "2")
                                {
                                    rewardResp.SmsStatus = Resources.ErrorResources.ResourceManager.GetString("RLH_SMS_2");
                                }
                                else if (rewardResp.SmsStatus == "3")
                                {
                                    rewardResp.SmsStatus = Resources.ErrorResources.ResourceManager.GetString("RLH_SMS_3");
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                
                return Json(rewardResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(rewardResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                // rewardResp = null;
                rewardReq = null;
                serviceCRM = null;
            }

        }

     
        public ActionResult RedeemReport()
        {
            RewardPointsResponse rewardResp = new RewardPointsResponse();
            RewardPointsRequest rewardReq = new RewardPointsRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    rewardReq.CountryCode = clientSetting.countryCode;
                    rewardReq.BrandCode = clientSetting.brandCode;
                    rewardReq.LanguageCode = clientSetting.langCode;
                    rewardReq.AccountID = string.Empty;
                    rewardReq.MSISDN = Session["MobileNumber"].ToString();
                    rewardReq.RequestType = "T";
                    rewardReq.SessionID = Session.SessionID;
                    rewardReq.User = Session["UserName"].ToString();

                    rewardResp = serviceCRM.CRMRewardPoints(rewardReq);

                    ///FRR--3083
                    if (rewardResp != null && rewardResp.ResponseDetails != null && rewardResp.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RedeemRewards_" + rewardResp.ResponseDetails.ResponseCode);
                        rewardResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? rewardResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                    try
                    {
                        rewardResp.RewardPointsQuery.EXPIRY_DATE = Utility.GetDateconvertion(rewardResp.RewardPointsQuery.EXPIRY_DATE, "dd-MM-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                        rewardResp.RewardPointsQuery.LAST_UPDATE_ON = Utility.GetDateconvertion(rewardResp.RewardPointsQuery.LAST_UPDATE_ON, "dd-MM-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                    }
                    catch
                    {

                    }
                
                return View(rewardResp.RewardPointsQuery);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(rewardResp.RewardPointsQuery);
            }
            finally
            {
                //rewardResp = null;
                rewardReq = null;
                serviceCRM = null;
            }

        }

        [HttpPost]
        public JsonResult RedeemedReport(string fromDate, string toDate)
        {
            RewardPointsResponse rewardResp = new RewardPointsResponse();
            RewardPointsRequest rewardReq = new RewardPointsRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                

                    rewardReq.CountryCode = clientSetting.countryCode;
                    rewardReq.BrandCode = clientSetting.brandCode;
                    rewardReq.LanguageCode = clientSetting.langCode;
                    rewardReq.AccountID = string.Empty;
                    rewardReq.MSISDN = Session["MobileNumber"].ToString();
                    rewardReq.FromDate = fromDate;
                    rewardReq.ToDate = toDate;
                    rewardReq.RequestType = "R";
                    rewardReq.SessionID = Session.SessionID;
                    rewardReq.User = Session["UserName"].ToString();

                    rewardResp = serviceCRM.CRMRewardPoints(rewardReq);
                    ///FRR--3083
                    if (rewardResp != null && rewardResp.ResponseDetails != null && rewardResp.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RedeemRewards_" + rewardResp.ResponseDetails.ResponseCode);
                        rewardResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? rewardResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                    try
                    {
                        rewardResp.RewardPointsReport.ForEach(m => m.SubmittedDate = Utility.FormatDateTime(m.SubmittedDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.yyyyMMdd));
                    }
                    catch
                    {

                    }
                
                return Json(rewardResp.RewardPointsReport, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(rewardResp.RewardPointsReport, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                rewardReq = null;
                //rewardResp = null;
                serviceCRM = null;
            }

        }


        public void DownLoadRedeemedReport(string redeemedData)
        {
            try
            {
                GridView gridView = new GridView();
                RewardPointsReport[] redeemReport = new JavaScriptSerializer().Deserialize<RewardPointsReport[]>(redeemedData);
                gridView.DataSource = redeemReport;
                gridView.DataBind();

                Utility.ExportToExcell(gridView, "RedeemReport_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
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

   
        public ActionResult RLH()
        {
            RoamLikeHomeResponse objres = new RoamLikeHomeResponse();
            PendingDetailsRequest req = new PendingDetailsRequest();
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
            CRMResponse validRes = Utility.checkValidSubscriber("1", clientSetting);
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                if (validRes.ResponseCode == "0" || validRes.ResponseCode == "101")
                {
                    if (Session["PAType"] != null && (Convert.ToString(Session["PAType"]) == "RLH"))
                    {
                        //Pending Approval Section


                        req.CountryCode = clientSetting.countryCode;
                        req.BrandCode = clientSetting.brandCode;
                        req.LanguageCode = clientSetting.langCode;
                        req.MSISDN = Convert.ToString(Session["MobileNumber"]);
                        req.Type = Convert.ToString(Session["PAType"]);
                        req.Id = Convert.ToString(Session["PAId"]);

                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        
                            ObjRes = serviceCRM.CRMGetPendingDetails(req);
                            if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                            {
                                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManaageOBABundle_" + ObjRes.ResponseDetails.ResponseCode);
                                ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                            }
                        

                        if (ObjRes.PendingDetails.Count > 0)
                        {

                            # region ActionType

                            RoamLikeHomeRequest objreq = new RoamLikeHomeRequest();
                            objreq.mode = "Q";
                            objreq.BrandCode = clientSetting.brandCode;
                            objreq.CountryCode = clientSetting.countryCode;
                            objreq.LanguageCode = clientSetting.langCode;
                            objreq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                            serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                            
                                objres = serviceCRM.CRMRoamLikeHome(objreq);
                            
                            if (objres != null && objres.actionType.Count > 0)
                            {
                                try
                                {
                                    for (int i = 0; i < objres.actionType.Count; i++)
                                    {
                                        string dummystring = System.Text.RegularExpressions.Regex.Replace(objres.actionType[i].statusDesc, "[^0-9a-zA-Z]+", string.Empty);
                                        string ResourceMsg = Resources.DropdownResources.ResourceManager.GetString(dummystring);
                                        objres.actionType[i].statusDesc = string.IsNullOrEmpty(ResourceMsg) ? objres.actionType[i].statusDesc : ResourceMsg;
                                    }
                                }
                                catch
                                {
                                }
                            }

                            objres.pendingActionRLH = new PendingActionRLH();
                            objres.pendingActionRLH.tranId = ObjRes.PendingDetails[0].Id;
                            objres.pendingActionRLH.ticketId = ObjRes.PendingDetails[0].TicketId;
                            objres.pendingActionRLH.reason = ObjRes.PendingDetails[0].Reason;
                            objres.pendingActionRLH.action = ObjRes.PendingDetails[0].OldMSISDN;
                            objres.pendingActionRLH.paActionType = "true";
                            Session["PAType"] = null;

                            # endregion
                        }
                    }
                    else
                    {
                        RoamLikeHomeRequest objreq = new RoamLikeHomeRequest();
                        objreq.mode = "Q";
                        objreq.BrandCode = clientSetting.brandCode;
                        objreq.CountryCode = clientSetting.countryCode;
                        objreq.LanguageCode = clientSetting.langCode;
                        objreq.MSISDN = Convert.ToString(Session["MobileNumber"]);

                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        
                            objres = serviceCRM.CRMRoamLikeHome(objreq);
                        

                        if (objres != null && objres.actionType.Count > 0)
                        {
                            try
                            {
                                for (int i = 0; i < objres.actionType.Count; i++)
                                {
                                    string dummystring = System.Text.RegularExpressions.Regex.Replace(objres.actionType[i].statusDesc, "[^0-9a-zA-Z]+", string.Empty);
                                    string ResourceMsg = Resources.DropdownResources.ResourceManager.GetString(dummystring);
                                    objres.actionType[i].statusDesc = string.IsNullOrEmpty(ResourceMsg) ? objres.actionType[i].statusDesc : ResourceMsg;
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                else
                {
                    objres.responseDetails = new CRMResponse();
                    objres.responseDetails.ResponseCode = "12";
                    objres.responseDetails.ResponseDesc = validRes.ResponseDesc;
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
                objres = null;
                req = null;
                //ObjRes = null;
                serviceCRM = null;
            }

        }

        [HttpPost]
        public ActionResult SubmitRLH(string RLHInput)
        {
            RoamLikeHomeResponse objres = new RoamLikeHomeResponse();
            RoamLikeHomeRequest objreq = JsonConvert.DeserializeObject<RoamLikeHomeRequest>(RLHInput);
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.MSISDN = Session["MobileNumber"].ToString();
                objreq.userName = Session["UserName"].ToString();
                objreq.PARequestType = Resources.HomeResources.RLH;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objres = serviceCRM.CRMRoamLikeHome(objreq);
                

                if (objres != null && objres.responseDetails != null && objres.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RLH_" + objres.responseDetails.ResponseCode);
                    objres.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.responseDetails.ResponseDesc : errorInsertMsg;

                    try
                    {
                        if (!string.IsNullOrEmpty(objres.smsStatus))
                        {
                            if (objres.smsStatus == "1")
                            {
                                string strSMSSucess = Resources.ErrorResources.ResourceManager.GetString("RLH_SMS_1");
                                objres.responseDetails.ResponseDesc = objres.responseDetails.ResponseDesc + ", " + strSMSSucess;
                            }
                            else if (objres.smsStatus == "2")
                            {
                                string strSMSFailure = Resources.ErrorResources.ResourceManager.GetString("RLH_SMS_2");
                                objres.responseDetails.ResponseDesc = objres.responseDetails.ResponseDesc + ", " + strSMSFailure;
                            }
                            else if (objres.smsStatus == "3")
                            {
                                string strSMSFolder = Resources.ErrorResources.ResourceManager.GetString("RLH_SMS_3");
                                objres.responseDetails.ResponseDesc = objres.responseDetails.ResponseDesc + ", " + strSMSFolder;
                            }
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
                // objres = null;
                objreq = null;
                serviceCRM = null;
            }

        }


      
        public ActionResult WifiCalling()
        {
            TopupFailureReportResp ObjResponse = new TopupFailureReportResp();

           // WifiCallingResponse ObjResp = new WifiCallingResponse();
            WifiCalling ObjResp = new WifiCalling();
            WifiCallingRequest ObjReq = new WifiCallingRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjReq.CountryCode = clientSetting.countryCode;
                    ObjReq.BrandCode = clientSetting.brandCode;
                    ObjReq.LanguageCode = clientSetting.langCode;
                    //ObjReq.Mode = "QueryDD";
                    //ObjResp = serviceCRM.CRMWifiCalling(ObjReq);

                    //if (ObjResp != null && ObjResp.Servicevalues != null && ObjResp.Servicevalues.Count > 0)
                    //{
                    //    for (int i = 0; i < ObjResp.Servicevalues.Count; i++)
                    //    {
                    //        //string dummystring = ObjResp.topupStatus[i].description;

                    //        string dummystring = System.Text.RegularExpressions.Regex.Replace(ObjResp.Servicevalues[i].id, "[^0-9a-zA-Z]+", string.Empty);
                    //        string ResourceMsg = Resources.DropdownResources.ResourceManager.GetString(dummystring);
                    //        ObjResp.Servicevalues[i].id = string.IsNullOrEmpty(ResourceMsg) ? ObjResp.Servicevalues[i].id : ResourceMsg;
                    //    }
                    //}
                    ObjResp.lstDirectdebitCardDetails = GetDebitCardDetails();
                    ObjResp.lstActions = Utility.GetDropdownMasterFromDB("77", Convert.ToString(Session["isprepaid"]), "drop_master");
                
                return View(ObjResp);

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(ObjResp);
            }
            finally
            {
                ObjResponse = null;
                //ObjResp = null;
                ObjReq = null;
                serviceCRM = null;
            }
        }
       // 4617
        public Dictionary<string, string> GetDebitCardDetails()
        {

            WifiCalling ObjResp = new WifiCalling();
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
                        ObjResp.lstDirectdebitCardDetails.Add(manageCardResp.debitcardDetails[i].cardId + "," + manageCardResp.debitcardDetails[i].Country + "," + manageCardResp.debitcardDetails[i].addressline1 + "," + manageCardResp.debitcardDetails[i].addressline2 + "," + manageCardResp.debitcardDetails[i].postcode + "," + manageCardResp.debitcardDetails[i].email + "," + manageCardResp.debitcardDetails[i].city + "," + manageCardResp.debitcardDetails[i].cardNumber +"," + manageCardResp.debitcardDetails[i].nameOnCard,manageCardResp.debitcardDetails[i].cardNumber);
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

        [HttpPost]
        public JsonResult WifiCallingProcess(string jsonVariables)
        {
            WifiCallingResponse objResp = new WifiCallingResponse();
            WifiCallingRequest objReq = JsonConvert.DeserializeObject<WifiCallingRequest>(jsonVariables);
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objReq.CountryCode = clientSetting.countryCode;
                    objReq.BrandCode = clientSetting.brandCode;
                    objReq.LanguageCode = clientSetting.langCode;

                    if (!string.IsNullOrEmpty(objReq.ExpiryDate))
                    {
                        string[] dates = objReq.ExpiryDate.Split('/');
                        string dateval = dates[0].Trim() + dates[1].Trim();
                        objReq.ExpiryDate = dateval;
                    }

                    if (!string.IsNullOrEmpty(objReq.ConsentDate))
                    {
                        objReq.ConsentDate = Utility.GetDateconvertion(objReq.ConsentDate, "MM/DD/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    }

                    objResp = serviceCRM.CRMWifiCalling(objReq);

                    if (objResp != null && objResp.responseDetails != null && objResp.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("WifiCallingProcess_" + objResp.responseDetails.ResponseCode);
                        objResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                
                return Json(objResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                // objResp = null;
                objReq = null;
                serviceCRM = null;
            }

        }

     
        public ActionResult GetPromo()
        {

            return View();
        }

        [HttpPost]
        public JsonResult GetPromosearch(string jsonVariables)
        {
            GetPromoResponse objResp = new GetPromoResponse();
            GetPromoRequest objReq = JsonConvert.DeserializeObject<GetPromoRequest>(jsonVariables);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController GetPromosearch Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.EmailID = Convert.ToString(Session["eMailID"]);
                objReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                objResp = serviceCRM.CRMGetPromo(objReq);
                if (objResp != null && objResp.responseDetails != null && objResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetPromo_" + objResp.responseDetails.ResponseCode);
                    objResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetPromosearch End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(objResp, JsonRequestBehavior.AllowGet);
        }


        #region IMEIBLOCK / IMEIUNBLOCK
     
        public ActionResult blockorunblockIMEI()
        {

            SimBlockModelRequest simBlockReq = new SimBlockModelRequest();
            IMEISimBlockResponse ObjResp = new IMEISimBlockResponse();
            IMEISimBlockRequest ObjReq = new IMEISimBlockRequest();
            //POF-6290
            BlockIMEIEventtype_list ObjBlock = new BlockIMEIEventtype_list();
            UnBlockIMEIEventtype_list ObjUnblock = new UnBlockIMEIEventtype_list();
            ObjResp.UnBlockEventTypeDropDown = new List<UnBlockIMEIEventtype_list>();
            ObjResp.EventTypeDropDown = new List<BlockIMEIEventtype_list>();
            ObjReq.CountryCode = clientSetting.countryCode;
            ObjReq.BrandCode = clientSetting.brandCode;
            ObjReq.LanguageCode = clientSetting.langCode;
            ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
            //s
            ServiceInvokeCRM serviceCRM;

            #region Pending to IMEI BLOCK/UNBLOCK Transfer

            if (Session["PAType"] != null && (Convert.ToString(Session["PAType"]).ToUpper() == "IMEI BLOCK" || Convert.ToString(Session["PAType"]) == "IMEI UNBLOCK"))
            {

                if (Convert.ToString(Session["PAType"]).ToUpper() == "IMEI BLOCK")
                {
                    ObjReq.Mode = "0";
                }
                else
                {
                    ObjReq.Mode = "1";
                }
                ObjReq.ReqID = Convert.ToInt32(Session["PAId"]);
                ObjReq.Type = "PAGET";
            }
            else
            {
                ObjReq.Mode = "1";
                ObjReq.Type = "GET";
            }

            #endregion
            //POF-6290
            List<BlockIMEIEventtype_list> objEvtDrop = new List<BlockIMEIEventtype_list>();
            List<UnBlockIMEIEventtype_list> objEvtDrop1 = new List<UnBlockIMEIEventtype_list>();
            DataSet ds = Utility.BindXmlFile("~/App_Data/BlockorUnblockIMEI.xml");
            foreach (DataTable dt in ds.Tables)
            {
                switch (dt.TableName)
                {
                    case "BlockEvent":
                        foreach (DataRow row in dt.Rows)
                        {
                            ObjBlock = new BlockIMEIEventtype_list();
                            ObjBlock.ID = row["ID"].ToString();
                            ObjBlock.Value = row["BlockEvent_Text"].ToString();
                            objEvtDrop.Add(ObjBlock);
                        }
                        break;
                    case "UnblockEvent":
                        foreach (DataRow row in dt.Rows)
                        {
                            ObjUnblock = new UnBlockIMEIEventtype_list();
                            ObjUnblock.ID = row["ID"].ToString();
                            ObjUnblock.Value = row["UnBlockEvent_Text"].ToString();
                            objEvtDrop1.Add(ObjUnblock);
                        }
                        break;

                }

            }
            ObjResp.EventTypeDropDown = objEvtDrop;
            ObjResp.UnBlockEventTypeDropDown = objEvtDrop1;
            //POF-6290

            try
            {

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    if (ObjReq.Type == "PAGET") 
                    {
                        ObjResp = serviceCRM.IMEIblockorunblock(ObjReq);
                    }
                    if (Session["PAType"] != null && (Convert.ToString(Session["PAType"]).ToUpper() == "IMEI BLOCK" || Convert.ToString(Session["PAType"]) == "IMEI UNBLOCK"))
                    {
                        ObjResp.FromPending = "1";

                        if (Convert.ToString(Session["PAType"]).ToUpper() == "IMEI BLOCK")
                        {
                            ObjResp.Mode = "11";
                        }
                        else
                        {
                            ObjResp.Mode = "12";
                        }

                    }
                    else
                    {
                        ObjResp.FromPending = "0";
                    }

                    Session["PAType"] = null;


                
                return View(ObjResp);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(ObjResp);
            }
            finally
            {
                simBlockReq = null;
                //  ObjResp = null;
                ObjReq = null;
                objEvtDrop1 = null;
                objEvtDrop = null;
                serviceCRM = null;

            }

        }
        #endregion
        #region FRR-4626
        [HttpPost]
        public JsonResult IMEIblockorunblockNotifyITG(string Simsearch)
        {
            IMEISimBlockResponse ObjResp = new IMEISimBlockResponse();
            IMEISimBlockRequest ObjReq = JsonConvert.DeserializeObject<IMEISimBlockRequest>(Simsearch);
            string isMode = string.Empty;
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {

                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                

                    ObjReq.MSISDN = Session["MobileNumber"].ToString();
                    ObjReq.AuthorizedBy = Session["UserName"].ToString();
                    ObjReq.ICCID = Session["ICCID"].ToString();
                    ObjResp = serviceCRM.IMEIblockorunblockNotifyITG(ObjReq);
                    if (ObjResp != null)
                    {
                        if (ObjReq.Type == "BLOCK")
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("IMEIBLOCK_" + ObjResp.ResponseDeatils.ResponseCode);
                            ObjResp.ResponseDeatils.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.ResponseDeatils.ResponseDesc : errorInsertMsg;
                        }
                        else
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("IMEIUNBLOCK_" + ObjResp.ResponseDeatils.ResponseCode);
                            ObjResp.ResponseDeatils.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.ResponseDeatils.ResponseDesc : errorInsertMsg;
                        }
                    }
                    else
                    {
                        ObjResp = new IMEISimBlockResponse();
                        ObjResp.ResponseDeatils.ResponseDesc = Resources.ErrorResources.Common_2;
                    }
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, ObjResp.ResponseDeatils.ResponseDesc);
                
                return Json(ObjResp);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(ObjResp);
            }
            finally
            {
                ObjReq = null;
                // ObjResp = null;
                serviceCRM = null;
            }

        }

        #endregion

        [HttpPost] 

public JsonResult CRMIMEIblockorunblock(string Simsearch)

        {

            IMEISimBlockResponse ObjResp = new IMEISimBlockResponse();

            IMEISimBlockRequest ObjReq = JsonConvert.DeserializeObject<IMEISimBlockRequest>(Simsearch);

            string isMode = string.Empty;
            //s
            ServiceInvokeCRM serviceCRM;


            try

            {



                string ServerMonth = Convert.ToString(Convert.ToInt32(System.DateTime.Now.Month) - 1);



                ObjReq.CountryCode = clientSetting.countryCode;

                ObjReq.BrandCode = clientSetting.brandCode;

                ObjReq.LanguageCode = clientSetting.langCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);





                    ObjReq.MSISDN = Session["MobileNumber"].ToString();

                    ObjReq.AuthorizedBy = Session["UserName"].ToString();

                    ObjReq.ICCID = Session["ICCID"].ToString();





                    //POF-6290



                    if (ObjReq.Type == "INSERT_BLOCK" || ObjReq.Type == "INSERT_UNBLOCK")

                    {

                        var reqdate = ObjReq.RequestDate.Split('/', '-');

                        var date = reqdate[0].Length == 1 ? "0" + reqdate[0] : reqdate[0];

                        var month = reqdate[1].Length == 1 ? "0" + reqdate[1] : reqdate[1];

                        ObjReq.RequestDate = date + "/" + month + "/" + reqdate[2];

                    }



                    //4950

                    if (ObjReq.Type == "INSERT_BLOCK")

                    {




                        ObjReq.RequestDate = Utility.FormatDateTime(ObjReq.RequestDate, "yyyy/mm/dd", "1");

                        ObjReq.RequestDate = Utility.FormatDateTime(ObjReq.RequestDate, "yyyy/mm/dd", "1");

                        ObjReq.ReportDate = Utility.FormatDateTime(ObjReq.ReportDate, "yyyy/mm/dd", "1");

                        ObjReq.PoliceReportDate = Utility.FormatDateTime(ObjReq.PoliceReportDate, "yyyy/mm/dd", "1");

                        ObjReq.EventDate = Utility.FormatDateTime(ObjReq.EventDate, "yyyy/mm/dd", "1");

                    }

                    if (ObjReq.Type == "INSERT_UNBLOCK")

                    {

                        ObjReq.RequestDate = Utility.FormatDateTime(ObjReq.RequestDate, "yyyy/mm/dd", "1");

                        ObjReq.PoliceReportDate = Utility.FormatDateTime(ObjReq.PoliceReportDate, "yyyy/mm/dd", "1");

                    }

                    if (ObjReq.Type == "GETSTATUSITG")

                    {

                        ObjReq.ReasonID = Utility.FormatDateTime(ObjReq.ReasonID, "yyyy/mm/dd", "1");

                    }

                    ObjResp = serviceCRM.IMEIblockorunblock(ObjReq);

                    if (ObjResp != null)

                    {

                        //4950

                        if (ObjReq.Type != "INSERT_BLOCK" && ObjReq.Type != "INSERT_UNBLOCK" && ObjReq.Type != "GETSTATUSITG")

                        {

                            if (ObjReq.Type == "BLOCK" || ObjReq.Type == "BLOCK_HLR")

                            {

                                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("IMEIBLOCK_" + ObjResp.ResponseDeatils.ResponseCode);

                                ObjResp.ResponseDeatils.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.ResponseDeatils.ResponseDesc : errorInsertMsg;

                            }

                            else

                            {

                                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("IMEIUNBLOCK_" + ObjResp.ResponseDeatils.ResponseCode);

                                ObjResp.ResponseDeatils.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.ResponseDeatils.ResponseDesc : errorInsertMsg;

                            }

                        }

                    }

                    else

                    {

                        ObjResp = new IMEISimBlockResponse();

                        ObjResp.ResponseDeatils.ResponseDesc = Resources.ErrorResources.Common_2;

                    }

                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, ObjResp.ResponseDeatils.ResponseDesc);

                

                return Json(ObjResp);

            }

            catch (Exception ex)

            {

                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);

                return Json(ObjResp);

            }

            finally

            {

                ObjReq = null;

                // ObjResp = null;
                serviceCRM = null;


            }



        }



        public ActionResult HomeAllowanceusage()
        {

            homeAllowanceusageResponse objResp = new homeAllowanceusageResponse();
            homeAllowanceusageRequest objReq = new homeAllowanceusageRequest();
            homeAllowanceusage homeAllowance = new homeAllowanceusage();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.MSISDN = Convert.ToString(Session["MobileNumber"]);

                objReq.Mode = "Q";


                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objResp = serviceCRM.CRMHomeAllowanceusage(objReq);
                    ViewBag.RSH = objResp.homeAllowance.RSH;
                    //6318
                    ViewBag.Responsecode = objResp.responseDetails.ResponseCode;
                    ViewBag.ResponseDesc = objResp.responseDetails.ResponseDesc;
                    ViewBag.DATA_THRESHOLD_BUNDLE_OPTOUT = objResp.homeAllowance.DATA_THRESHOLD_BUNDLE_OPTOUT;
                    //6316
                    ViewBag.PAYG_DATA_USAGE_CONSENT = objResp.homeAllowance.PAYG_DATA_USAGE_CONSENT;
                    ViewBag.DATA_USAGE_STATUS = objResp.homeAllowance.DATA_USAGE_STATUS;
                
                return View(objResp);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View(objResp);
            }
            finally
            {
                objReq = null;
                // objResp = null;
                homeAllowance = null;
                serviceCRM = null;
 
            }
        }


        //[HttpPost]
        //public JsonResult CRMHomeAllowanceusage(string Value, string Type)
        //{
        //    homeAllowanceusageResponse objResp = new homeAllowanceusageResponse();
        //    homeAllowanceusageRequest objReq = new homeAllowanceusageRequest();
        //    homeAllowanceusage homeAllowance = new homeAllowanceusage();
        //    try
        //    {
        //        //  homeAllowanceusageRequest objReq = JsonConvert.DeserializeObject<homeAllowanceusageRequest>(jsonVariables);

        //        using (ServiceInvokeCRM serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl))
        //        {
        //            objReq.CountryCode = clientSetting.countryCode;
        //            objReq.BrandCode = clientSetting.brandCode;
        //            objReq.LanguageCode = clientSetting.langCode;
        //            objReq.MSISDN = Session["MobileNumber"].ToString();
        //            homeAllowance.Value = Value;
        //            homeAllowance.Type = Type;



        //            objReq.homeAllowance = homeAllowance;





        //            objResp = serviceCRM.CRMHomeAllowanceusage(objReq);







        //            if (objResp != null && objResp.responseDetails != null && objResp.responseDetails.ResponseCode != null)
        //            {
        //                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("HomeAllowanceusage_" + objResp.responseDetails.ResponseCode);
        //                objResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objResp.responseDetails.ResponseDesc : errorInsertMsg;
        //            }



        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        CRMLogger.WriteException(Session["UserName"].ToString(), this.ControllerContext, ex);
        //    }
        //    return Json(objResp, JsonRequestBehavior.AllowGet);
        //}


        //[HttpPost]
        //public JsonResult CRMHomeAllowanceusage(string Value, string Type)
        //{
        //    homeAllowanceusageResponse objResp = new homeAllowanceusageResponse();
        //    homeAllowanceusageRequest objReq = new homeAllowanceusageRequest();
        //    homeAllowanceusage homeAllowance = new homeAllowanceusage();
        //    try
        //    {
        //        //  homeAllowanceusageRequest objReq = JsonConvert.DeserializeObject<homeAllowanceusageRequest>(jsonVariables);

        //        using (ServiceInvokeCRM serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl))
        //        {
        //            objReq.CountryCode = clientSetting.countryCode;
        //            objReq.BrandCode = clientSetting.brandCode;
        //            objReq.LanguageCode = clientSetting.langCode;
        //            objReq.MSISDN = Session["MobileNumber"].ToString();
        //            homeAllowance.Value = Value;
        //            homeAllowance.Type = Type;



        //            objReq.homeAllowance = homeAllowance;





        //            objResp = serviceCRM.CRMHomeAllowanceusage(objReq);







        //            if (objResp != null && objResp.responseDetails != null && objResp.responseDetails.ResponseCode != null)
        //            {
        //                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("HomeAllowanceusage_" + objResp.responseDetails.ResponseCode);
        //                objResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objResp.responseDetails.ResponseDesc : errorInsertMsg;
        //            }



        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        CRMLogger.WriteException(Session["UserName"].ToString(), this.ControllerContext, ex);
        //    }
        //    return Json(objResp, JsonRequestBehavior.AllowGet);
        //}


        [HttpPost]
        public JsonResult CRMHomeAllowanceusage(string Value, string Type)
        {
            homeAllowanceusageResponse objResp = new homeAllowanceusageResponse();
            homeAllowanceusageRequest objReq = new homeAllowanceusageRequest();
            homeAllowanceusage homeAllowance = new homeAllowanceusage();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                //  homeAllowanceusageRequest objReq = JsonConvert.DeserializeObject<homeAllowanceusageRequest>(jsonVariables);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objReq.CountryCode = clientSetting.countryCode;
                    objReq.BrandCode = clientSetting.brandCode;
                    objReq.LanguageCode = clientSetting.langCode;
                    objReq.MSISDN = Session["MobileNumber"].ToString();
                    homeAllowance.Value = Value;
                    homeAllowance.Type = Type;
                    objReq.homeAllowance = homeAllowance;
                    string[] ErrorCode = { "0", "001","23","24","25"};
                    objResp = serviceCRM.CRMHomeAllowanceusage(objReq);
                    if (objResp != null && objResp.responseDetails != null)
                    {

                        StringBuilder errorInsertMsg = new StringBuilder();
                        string[] errorCode = objResp.responseDetails.ResponseCode.Split(',');
                        foreach (string str in errorCode)
                        {
                            
                            if (!string.IsNullOrEmpty(Resources.ErrorResources.ResourceManager.GetString("HomeAllowanceusage_" + str)))
                                errorInsertMsg.Append(string.IsNullOrEmpty(errorInsertMsg.ToString()) ? Resources.ErrorResources.ResourceManager.GetString("HomeAllowanceusage_" + str) : (" " + Resources.ErrorResources.EReg_USA_DNA_and + " " + Resources.ErrorResources.ResourceManager.GetString("HomeAllowanceusage_" + str)));
                            else
                                errorInsertMsg.Append(objResp.responseDetails.ResponseDesc);
                            if (ErrorCode.Contains(str))
                            {
                                objResp.responseDetails.ResponseCode = "0";
                            }
                        }
                        objResp.responseDetails.ResponseDesc = errorInsertMsg.ToString();
                    }
                    //if (objResp != null && objResp.responseDetails != null && objResp.responseDetails.ResponseCode != null)
                    //{
                    //    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("HomeAllowanceusage_" + objResp.responseDetails.ResponseCode);
                    //    objResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objResp.responseDetails.ResponseDesc : errorInsertMsg;
                    //}
                
                return Json(objResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objReq = null;
                objResp = null;
                homeAllowance = null;
                serviceCRM = null;
            }

        }

      
        public ActionResult portinoffer()
        {
            portinofferResponse objResp = new portinofferResponse();
            portinofferRequest objReq = new portinofferRequest();
            //portinoffer portinoffer = new portinoffer();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                objReq.mode = "Display";

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objResp = serviceCRM.CRMportinoffer(objReq);
                
                return View(objResp);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Session["UserName"].ToString(), this.ControllerContext, eX);
                return View(objResp);
            }
            finally
            {
                objReq = null;
                // objResp = null;
                serviceCRM = null;
            }
        }

       

        [HttpPost]
        public JsonResult CRMportinoffer(portinofferRequest objReq)
        {
            portinofferResponse objResp = new portinofferResponse();
            //portinofferRequest objReq = new portinofferRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objReq.CountryCode = clientSetting.countryCode;
                    objReq.BrandCode = clientSetting.brandCode;
                    objReq.LanguageCode = clientSetting.langCode;
                    objReq.portinoffer = new ServiceCRM.portinoffer();
                    objReq.portinoffer.bundlevalue = objReq.MSISDN.Split('-').GetValue(0).ToString();
                    objReq.portinoffer.bundleprice = objReq.MSISDN.Split('-').GetValue(1).ToString();
                    if (objReq.MSISDN.Split('-').GetValue(2).ToString() == "RENEWAL")
                        objReq.portinoffer.AutoFlag = "1";
                    else
                        objReq.portinoffer.AutoFlag = "0";
                    objReq.MSISDN = Session["MobileNumber"].ToString();
                    objReq.mode = "Activate";

                    string[] ErrorCode = { "0", "1" };
                    objResp = serviceCRM.CRMportinoffer(objReq);

                    if (objResp != null && objResp.responseDetails != null)
                    {
                        StringBuilder errorInsertMsg = new StringBuilder();
                        string[] errorCode = objResp.responseDetails.ResponseCode.Split(',');
                        foreach (string str in errorCode)
                        {
                            if (Resources.ErrorResources.ResourceManager.GetString("portinoffer_" + str) != string.Empty)
                                errorInsertMsg.Append(string.IsNullOrEmpty(errorInsertMsg.ToString()) ? Resources.ErrorResources.ResourceManager.GetString("portinoffer_" + str) : (" " + Resources.ErrorResources.EReg_USA_DNA_and + " " + Resources.ErrorResources.ResourceManager.GetString("portinoffer_" + str)));
                            else
                                errorInsertMsg.Append(objResp.responseDetails.ResponseDesc);
                            if (ErrorCode.Contains(str))
                            {
                                objResp.responseDetails.ResponseCode = "0";
                            }
                        }
                        objResp.responseDetails.ResponseDesc = errorInsertMsg.ToString();
                    }
                
                return Json(objResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                // objResp = null;
                serviceCRM = null;
            }
        }



        #region portinofferreport FRR :4212

        public ActionResult PortinofferReport()
        {
            portinofferreportResponse objres = new portinofferreportResponse();
            portinofferreportRequest objreq = new portinofferreportRequest();
            try
            {
                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;

                //using (ServiceInvokeCRM serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl))
                //{
                //    objres = serviceCRM.CRMPortinofferReport(objreq);
                //    ViewBag.Mode = Utility.GetDropdownMasterFromDB("79", "1", "drop_master");
                //    ViewBag.Status = objres.PortinofferReport;
                //}
                return View("PortinofferReport", objres);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View("PortinofferReport", objres);
            }
            finally
            {
                objreq = null;
                //objres = null;
            }

        }



        #endregion

        [HttpPost]
        public JsonResult portinreportsearch(portinofferreportRequest objreq)
        {
            //s
            ServiceInvokeCRM serviceCRM;
            portinofferreportResponse objres = new portinofferreportResponse();

            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.MSISDN = objreq.MSISDN;
                if (objreq.FromDate != null && objreq.ToDate != null)
                {
                    objreq.FromDate = Utility.GetDateconvertion(objreq.FromDate, "YYYY-MM-DD", true, clientSetting.mvnoSettings.dateTimeFormat);
                    objreq.ToDate = Utility.GetDateconvertion(objreq.ToDate, "YYYY-MM-DD", true, clientSetting.mvnoSettings.dateTimeFormat);
                }
                objreq.FromDate = objreq.FromDate;
                objreq.ToDate = objreq.ToDate;


                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objres = serviceCRM.CRMPortinofferReport(objreq);
                
                if (objres != null && objres.ResponseDetails != null && objres.ResponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMOnlineReport_" + objres.ResponseDetails.ResponseCode);
                    objres.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.ResponseDetails.ResponseDesc : errorInsertMsg;
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
                // objres = null;
                serviceCRM = null;
            }
            // return Json(objRes);


        }


        #region--------------------FRR-----------4457---------------
        public ActionResult DeviceCompatibilityCheck()
        {

            return View();
        }

        public JsonResult CRMDeviceCompatibilityCheck(string DeviceDetails)
        {
            CRMDeviceCompabilityCheckResponse ObjResp = new CRMDeviceCompabilityCheckResponse();
            CRMDeviceCompabilityCheckRequest ObjReq = new JavaScriptSerializer().Deserialize<CRMDeviceCompabilityCheckRequest>(DeviceDetails);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - CRMDeviceCompatibilityCheck Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.GetDeviceEligibilityCheck(ObjReq);
                
                if (ObjResp != null && ObjResp.Response != null && ObjResp.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ServicesResources.ResourceManager.GetString("DeviceCompatibilityCheck_" + ObjResp.Response.ResponseCode);
                    ObjResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.Response.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - CRMDeviceCompatibilityCheck End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-CRMDeviceCompatibilityCheck - " + this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjResp, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region******************4438*********
        public ActionResult DevicePurchaseDetails()
        {
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "Services_DevicePurchaseDetails").ToList();
            return View();
        }
        public ActionResult DeviceRefund()
        {
            CRMDeviceRefundDetailsResponse ObjResp = new CRMDeviceRefundDetailsResponse();
            CRMDeviceRefundDetailsRequest ObjReq = new CRMDeviceRefundDetailsRequest();
            string errorInsertMsg = string.Empty;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - SIMMigrateServices Start");
                List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
                menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "Services_DeviceRefund").ToList();

                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.GetDeviceRefundDetails(ObjReq);

                //FRR--3083
                if (ObjResp != null && ObjResp.Response != null && ObjResp.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LoadFailureHistory_" + ObjResp.Response.ResponseCode);
                    ObjResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.Response.ResponseDesc : errorInsertMsg;
                }

                ObjResp.DevicePurchaseDetails.ForEach(b => b.PurchasedDate = Utility.GetDateconvertion(b.PurchasedDate, "dd/mm/yyyy", false, clientSetting.mvnoSettings.dateTimeFormat));
                ObjResp.DevicePurchaseDetails.ForEach(a => a.PaymentMode = a.PaymentMode == "1" ? Resources.BundleResources.CreditCard : Resources.BundleResources.OtherPaymentMode);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }


            finally
            {
                ObjReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return View(ObjResp);

        }
        public JsonResult GetDevicePurchaseDetails(string GetDeviceDetails)
        {
            CRMDeviceRefundDetailsResponse ObjResp = new CRMDeviceRefundDetailsResponse();
            CRMDeviceRefundDetailsRequest ObjReq = JsonConvert.DeserializeObject<CRMDeviceRefundDetailsRequest>(GetDeviceDetails);
            //string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;

                

               
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.GetDeviceRefundDetails(ObjReq);

                //FRR--3083
                if (ObjResp != null && ObjResp.Response != null && ObjResp.Response.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LoadFailureHistory_" + ObjResp.Response.ResponseCode);
                    ObjResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.Response.ResponseDesc : errorInsertMsg;
                }

                ObjResp.DevicePurchaseDetails.ForEach(a =>
                {
                    a.SubmittedDate = Utility.GetDateconvertion(a.SubmittedDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                });
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult DevicePurchaseDetailsHistory(string DeviceDetails)
        {
            CRMDeviceRefundDetailsResponse ObjResp = new CRMDeviceRefundDetailsResponse();
            CRMDeviceRefundDetailsRequest ObjReq = JsonConvert.DeserializeObject<CRMDeviceRefundDetailsRequest>(DeviceDetails);
            string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            ServiceInvokeCRM serviceCRM;
            string OrderID = string.Empty;
            string TransactionID = string.Empty;
            string Name = string.Empty;
            string Address = string.Empty;
            string MVNOSubscriber = string.Empty;
            string PurchaseDate = string.Empty;
            string RefundStatus = string.Empty;
            string strGetDate = "", strDate = "", strMonth = "", strYear = "";
            string[] strSplit;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - DevicePurchaseDetailsHistory Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;

                
                strInputDate = clientSetting.mvnoSettings.dateTimeFormat;

                #region Date Split
                if (ObjReq.FromDate != string.Empty)
                {
                    strGetDate = Utility.GetDateconvertion(ObjReq.FromDate, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
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
                    ObjReq.FromDate = strYear + "-" + strMonth + "-" + strDate;
                }
                if (ObjReq.ToDate != string.Empty)
                {
                    strGetDate = Utility.GetDateconvertion(ObjReq.ToDate, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
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
                    ObjReq.ToDate = strYear + "-" + strMonth + "-" + strDate;
                }
                #endregion
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.GetDeviceReportRefundDetails(ObjReq);

                //FRR--3083
                if (ObjResp != null && ObjResp.Response != null && ObjResp.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LoadFailureHistory_" + ObjResp.Response.ResponseCode);
                    ObjResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.Response.ResponseDesc : errorInsertMsg;
                }

                ObjResp.DevicePurchaseDetails.ForEach(a =>
                {
                    a.SubmittedDate = Utility.GetDateconvertion(a.SubmittedDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                });
                OrderID = Resources.SIMResources.OrderID;
                TransactionID = Resources.SIMResources.TransactionID;
                Name = Resources.ServicesResources.Device_Refund_Name;
                Address = Resources.ServicesResources.Device_Refund_Address;
                MVNOSubscriber = Resources.ServicesResources.Device_Refund_MVNOSubscriber;
                PurchaseDate = Resources.ServicesResources.Device_Refund_Purchase_Date;
                RefundStatus = Resources.ServicesResources.Device_Refund_Status;
                TempData["DevicePurchaseDetailsRecord"] = ObjResp.DevicePurchaseDetails.Select(slctColumns => new
                {
                    OrderID = slctColumns.OrderId,
                    TransactionID = slctColumns.TransactionID,
                    Name = slctColumns.FirstName,
                    Address = slctColumns.Address,
                    MVNOSubscriber = slctColumns.MVNOSubscriber,
                    PurchaseDate = slctColumns.SubmittedDate,
                    RefundStatus = slctColumns.Status
                });
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - DevicePurchaseDetailsHistory End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                OrderID = string.Empty;
                TransactionID = string.Empty;
                Name = string.Empty;
                Address = string.Empty;
                MVNOSubscriber = string.Empty;
                PurchaseDate = string.Empty;
                RefundStatus = string.Empty;
                ObjReq = null;
                strInputDate = string.Empty;
                serviceCRM = null;
                strGetDate = string.Empty;
                strDate = string.Empty; 
                strMonth = string.Empty;
                strYear = string.Empty;
                strSplit = null;
                errorInsertMsg = string.Empty;
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        

        public JsonResult CRMGetMinimumDeviceInformation(string DeviceDetails)
        {
            CRMMinimumDeviceDetailsResponse ObjResp = new CRMMinimumDeviceDetailsResponse();
            CRMMinimumDeviceDetailsRequest ObjReq = JsonConvert.DeserializeObject<CRMMinimumDeviceDetailsRequest>(DeviceDetails);
            string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.GetMinimumDeviceInformation(ObjReq);

                //FRR--3083
                if (ObjResp != null && ObjResp.Response != null && ObjResp.Response.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LoadFailureHistory_" + ObjResp.Response.ResponseCode);
                    ObjResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.Response.ResponseDesc : errorInsertMsg;
                }

               

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        [HttpPost]
        public void DownLoadDevicePurchaseReport(string DeviceDetails)
        {
            GridView gridView = new GridView();
            try
            {
                gridView.DataSource = TempData["DevicePurchaseDetailsRecord"];
                TempData.Keep("DevicePurchaseDetailsRecord");
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "DevicePurchase_" + DateTime.Now.ToString("yyyy-MM-ddHHmmss"), this.HttpContext.Response);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                gridView.Dispose();

            }
        }
        #endregion

        //----------------s.subha------FRR-4526-------------
        #region **************FRR-4526******Static_IP_Subscription************
        public ActionResult StaticIPSubscription()
        {
            CRMStaticIPSubscriptionRequest ObjReq = new CRMStaticIPSubscriptionRequest();
            CRMStaticIPSubscriptionResponse ObjResp = new CRMStaticIPSubscriptionResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - StaticIPSubscription Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.Mode = "GET";
                ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.GetStaticIPSubscriptionDetails(ObjReq);
                if (ObjResp != null && ObjResp.Response != null && ObjResp.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("StaticIPSubscriptionResponse_" + ObjResp.Response.ResponseCode);
                    ObjResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.Response.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - StaticIPSubscription End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-StaticIPSubscription - " + this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return View(ObjResp);
        }
        public JsonResult CRMStaticIPSubscriptionDetails(CRMStaticIPSubscriptionRequest ObjReq)
        {
            CRMStaticIPSubscriptionResponse ObjResp = new CRMStaticIPSubscriptionResponse();
            //string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - CRMStaticIPSubscriptionDetails Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                ObjReq.RequestBy = Convert.ToString(Session["UserName"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.GetStaticIPSubscriptionDetails(ObjReq);
                if (ObjResp != null && ObjResp.Response != null && ObjResp.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ServicesResources.ResourceManager.GetString("StaticIPSubscriptionResponse_" + ObjResp.Response.ResponseCode);
                    ObjResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.Response.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - CRMStaticIPSubscriptionDetails End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-CRMStaticIPSubscriptionDetails - " + this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult RefundIPSubscription()
        {
            CRMStaticIPSubscriptionRequest ObjReq = new CRMStaticIPSubscriptionRequest();
            CRMStaticIPSubscriptionResponse ObjResp = new CRMStaticIPSubscriptionResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - RefundIPSubscription Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.Mode = "GET";
                ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.GetRefundIPSubscriptionDetails(ObjReq);
                if (ObjResp != null && ObjResp.Response != null && ObjResp.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RefundIPSubscriptionResponse_" + ObjResp.Response.ResponseCode);
                    ObjResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.Response.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - RefundIPSubscription End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-RefundIPSubscription - " + this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return View(ObjResp);
        }
        public JsonResult CRMRefundIPSubscriptionDetails(string RefundIPSubscriptionDetails)
        {
             CRMStaticIPSubscriptionResponse ObjResp = new CRMStaticIPSubscriptionResponse();
             CRMStaticIPSubscriptionRequest ObjReq = JsonConvert.DeserializeObject<CRMStaticIPSubscriptionRequest>(RefundIPSubscriptionDetails);
             //string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
             ServiceInvokeCRM serviceCRM;
             string errorInsertMsg = string.Empty;
             try
             {
                 CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - CRMRefundIPSubscriptionDetails Start");
                 ObjReq.CountryCode = clientSetting.countryCode;
                 ObjReq.BrandCode = clientSetting.brandCode;
                 ObjReq.LanguageCode = clientSetting.langCode;
                 serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                 ObjResp = serviceCRM.GetRefundIPSubscriptionDetails(ObjReq);
                 if (ObjResp != null && ObjResp.Response != null && ObjResp.Response.ResponseCode != null)
                 {
                     errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RefundIPSubscriptionResponse_" + ObjResp.Response.ResponseCode);
                     ObjResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.Response.ResponseDesc : errorInsertMsg;
                 }
                 CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - CRMRefundIPSubscriptionDetails End");
             }
             catch (Exception ex)
             {
                 CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-CRMRefundIPSubscriptionDetails - " + this.ControllerContext, ex);
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


        
        //--------------FRR------4494-----------
        #region *************** Discount Promo DETAILS****UPLOAD***GET***INSERT***
        public ActionResult UploadDiscountPromo()
        {
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SERVICES_UploadDiscountPromo").ToList();
            return View();
        }
        public ActionResult IndividualDiscountPromo()
        {
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SERVICES_IndividualDiscountPromo").ToList();
            return View();
        }
        public DataTable ReadCsvFile(string strFilePath)
        {
            DataTable dtCsv = new DataTable();
            string Fulltext = string.Empty;
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                while (!sr.EndOfStream)
                {
                    Fulltext = sr.ReadToEnd().ToString(); //read full file text  
                    string[] rows = Fulltext.Split('\n'); //split full file text into rows  
                    for (int i = 0; i < rows.Count(); i++)
                    {
                        string[] rowValues = rows[i].Split(','); //split each row with comma to get individual values  
                        {
                            if (i == 0)
                            {
                                for (int j = 0; j < rowValues.Count(); j++)
                                {
                                    dtCsv.Columns.Add(rowValues[j].Replace("\r", string.Empty)); //add headers  
                                }
                            }
                            else
                            {
                                DataRow dr = dtCsv.NewRow();
                                for (int k = 0; k < rowValues.Count(); k++)
                                {
                                    dr[k] = rowValues[k].Replace("\r", string.Empty).ToString().Trim();
                                }
                                if (dr[0].ToString() == string.Empty && dr[1].ToString() == string.Empty) { }
                                else { dtCsv.Rows.Add(dr); } //add other rows

                            }
                        }
                    }
                }
            }
            return dtCsv;
        }
        public static DataTable ConvertXSLXtoDataTable(string strFilePath, string connString)
        {
            OleDbConnection oledbConn = new OleDbConnection(connString);
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            try
            {
                oledbConn.Open();
                DataTable dtSchema;
                dtSchema = oledbConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string ExcelSheetName = dtSchema.Rows[0]["TABLE_NAME"].ToString();

                using (OleDbCommand cmd = new OleDbCommand("SELECT * From [" + ExcelSheetName + "]", oledbConn))
                {
                    OleDbDataAdapter oleda = new OleDbDataAdapter();
                    oleda.SelectCommand = cmd;
                    ds = new DataSet();
                    oleda.Fill(ds);
                    dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ds = null;
                oledbConn.Close();
            }

            return dt;

        }
        public JsonResult DiscountPromoUploadFile(HttpPostedFileBase file)
        {
            PromoUploadResponse ObjResp = new PromoUploadResponse();
            PromoUploadRequest ObjReq = new PromoUploadRequest();
            ObjResp.responseDetails = new CRMResponse();
            //  DataSet ds = new DataSet();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            DataTable dt = new DataTable();
            ObjReq.PromoDetails = new DataSet();
            DataTable dtcopy = new DataTable();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - GETCRMMIGRATESIMSERVICES Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.RequestBy = Session["UserName"].ToString();
                ObjReq.Mode = "BULK";
                if (clientSetting.mvnoSettings.EnablePromoCodeMappingApproval.ToLower() == "true")
                {
                    ObjReq.Status = "0";
                }
                else
                { ObjReq.Status = "1"; }
                if (file.ContentLength == 0)
                {
                    ObjResp.responseDetails.ResponseCode = "101";
                    ObjResp.responseDetails.ResponseDesc = "File Should Be Empty!Upload Sample Excel File!";
                    return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                }
                string extension = System.IO.Path.GetExtension(file.FileName).ToLower();
                string[] validFileTypes = { ".csv" };
                string path1 = string.Format("{0}/{1}", Server.MapPath("~/App_Data/TemplateDiscountPromoDetails"), file.FileName);
                if (!Directory.Exists(path1))
                {
                    Directory.CreateDirectory(Server.MapPath("~/App_Data/TemplateDiscountPromoDetails"));
                }
                dt = new DataTable();
                if (validFileTypes.Contains(extension))
                {
                    if (System.IO.File.Exists(path1))
                    { System.IO.File.Delete(path1); }
                    file.SaveAs(path1);
                    //Connection String to Excel Workbook  
                    if (extension.Trim() == ".csv")
                    {
                        dt = ReadCsvFile(path1);
                        if (System.IO.File.Exists(path1))
                        { System.IO.File.Delete(path1); }
                    }

                    if (dt.Columns.Count == 3)
                    {
                        string[] validHeaderTypes = { "PROMOCODE", "MSISDN", "PERCENTAGE" };
                        foreach (DataColumn dc in dt.Columns)
                        {
                            string colName = dc.ColumnName.ToString();
                            if (!validHeaderTypes.Contains(colName))
                            {
                                ObjResp.responseDetails.ResponseCode = "102";
                                ObjResp.responseDetails.ResponseDesc = "Excel sheet Have Wrong Headers!";
                                return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        }
                        if (Request["VariablePromo"] == "1")
                        {
                            ObjResp.responseDetails.ResponseCode = "103";
                            ObjResp.responseDetails.ResponseDesc = "Please select different variable promo!";
                            return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (dt.Columns.Count == 2)
                    {
                        string[] validHeaderTypes = { "PROMOCODE", "MSISDN" };
                        foreach (DataColumn dc in dt.Columns)
                        {
                            string colName = dc.ColumnName.ToString();
                            if (!validHeaderTypes.Contains(colName))
                            {
                                ObjResp.responseDetails.ResponseCode = "102";
                                ObjResp.responseDetails.ResponseDesc = "Excel sheet Have Wrong Headers!";
                                return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        }
                        if (Request["VariablePromo"] == "0")
                        {
                            ObjResp.responseDetails.ResponseCode = "103";
                            ObjResp.responseDetails.ResponseDesc = "Please select different variable promo!";
                            return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        ObjResp.responseDetails.ResponseCode = "103";
                        ObjResp.responseDetails.ResponseDesc = "Excel sheet Must have Two or Three Headers!";
                        return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                    }
                    #region------read------------
                    if (file != null)
                    {
                        //  ObjReq.ShippingTable = this.dt(file);
                        if (dt.Rows.Count == 0)
                        {
                            ObjResp.responseDetails.ResponseCode = "104";
                            ObjResp.responseDetails.ResponseDesc = "No record found in excel";
                            return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                        }
                        else if (dt.Rows.Count > Int32.Parse(clientSetting.mvnoSettings.UploadMaxPromoCodeMappingRowCount.Trim()))
                        {
                            ObjResp.responseDetails.ResponseCode = "105";
                            ObjResp.responseDetails.ResponseDesc = "Maximum Row Exceeds, Please upload max of " + clientSetting.mvnoSettings.UploadMaxPromoCodeMappingRowCount.Trim() + " Rows";
                            return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            if (dt.AsEnumerable().Where(r => r["PROMOCODE"].ToString() == string.Empty).Any())
                            {
                                ObjResp.responseDetails.ResponseCode = "105";
                                ObjResp.responseDetails.ResponseDesc = "PromoCode column is empty";
                                return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                            }

                            if (dt.AsEnumerable().Where(r => r["MSISDN"].ToString() == string.Empty).Any())
                            {
                                ObjResp.responseDetails.ResponseCode = "106";
                                ObjResp.responseDetails.ResponseDesc = "Msisdn column is empty";
                                return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                            }

                            //if (dt.AsEnumerable().Where(r => r["PERCENTAGE"].ToString() == string.Empty).Any())
                            //{
                            //    ObjResp.responseDetails.ResponseCode = "106";
                            //    ObjResp.responseDetails.ResponseDesc = "Percentage column is empty";
                            //    return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                            //}

                            //var duplicatesSHIPPINGID = dt.AsEnumerable().GroupBy(r => r[0]).Where(gr => gr.Count() > 1).ToList();
                            //if (duplicatesSHIPPINGID.Count > 0)
                            //{
                            //    ObjResp.responseDetails.ResponseCode = "107";
                            //    ObjResp.responseDetails.ResponseDesc = "Duplicate record found in Shipping Id";
                            //    return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                            //}
                            //var duplicatesOrderId = dt.AsEnumerable().GroupBy(r => r[1]).Where(gr => gr.Count() > 1).ToList();
                            //if (duplicatesOrderId.Count > 0)
                            //{
                            //    ObjResp.responseDetails.ResponseCode = "108";
                            //    ObjResp.responseDetails.ResponseDesc = "Duplicate record found in Order Id";
                            //    return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                            //}
                            var duplicates = dt.AsEnumerable().GroupBy(i => new { Name = i.Field<string>("MSISDN"), Subject = i.Field<string>("PROMOCODE") }).Where(g => g.Count() > 1).Select(g => new { g.Key.Name, g.Key.Subject }).ToList();
                            if (duplicates.Count > 0)
                            {
                                ObjResp.responseDetails.ResponseCode = "108";
                                ObjResp.responseDetails.ResponseDesc = "Duplicate records found in the file";
                                return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                            }

                            
                            List<int> minimumLengthForColumns = 
   Enumerable.Range(0, dt.Columns.Count)
             .Select(col => dt.AsEnumerable()
                                     .Select(row => row[col]).OfType<string>()
                                     .Min(val => val.Length)).ToList();

                            if (Int32.Parse(minimumLengthForColumns[1].ToString()) < Int32.Parse(clientSetting.preSettings.mobileMinLength.Trim()))
                            {
                                ObjResp.responseDetails.ResponseCode = "109";
                                ObjResp.responseDetails.ResponseDesc = "Please give MSISDN length min of " + clientSetting.preSettings.mobileMinLength;
                                return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                            }

                        }
                    }
                    else
                    {
                        ObjResp.responseDetails.ResponseCode = "60";
                        ObjResp.responseDetails.ResponseDesc = "File is not received";
                        return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                    }
                    #endregion

                    //Regex rg = new Regex("/^[ A-Za-z0-9_@./#&+-]*$/");
                    foreach (DataRow row in dt.Rows)
                    {
                        string PromoCode = row["PROMOCODE"].ToString();
                        string Msisdn = row["MSISDN"].ToString();
                        //string Percentage = row["PERCENTAGE"].ToString();
                        if (!string.IsNullOrEmpty(PromoCode))
                        {
                            //if(!rg.IsMatch(ORDERID))
                            //{
                            //    ObjResp.responseDetails.ResponseCode = "109";
                            //    ObjResp.responseDetails.ResponseDesc = "ORDERID Values Wrongly Inserted!";
                            //    return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                            //}
                        }
                        else
                        {
                            ObjResp.responseDetails.ResponseCode = "110";
                            ObjResp.responseDetails.ResponseDesc = "PromoCode Values Is Empty!";
                            return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                        }
                        if (!string.IsNullOrEmpty(Msisdn))
                        {
                            //if (!rg.IsMatch(SHIPPINGID))
                            //{
                            //    ObjResp.responseDetails.ResponseCode = "111";
                            //    ObjResp.responseDetails.ResponseDesc = "SHIPPINGID Values Wrongly Inserted!";
                            //    return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                            //}
                        }
                        else
                        {
                            ObjResp.responseDetails.ResponseCode = "112";
                            ObjResp.responseDetails.ResponseDesc = "Msisdn Values Is Empty!";
                            return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                        }
                        //if (!string.IsNullOrEmpty(Percentage))
                        //{
                        //    //if (!rg.IsMatch(SHIPPINGID))
                        //    //{
                        //    //    ObjResp.responseDetails.ResponseCode = "111";
                        //    //    ObjResp.responseDetails.ResponseDesc = "SHIPPINGID Values Wrongly Inserted!";
                        //    //    return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                        //    //}
                        //}
                        //else
                        //{
                        //    ObjResp.responseDetails.ResponseCode = "112";
                        //    ObjResp.responseDetails.ResponseDesc = "Percentage Values Is Empty!";
                        //    return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                        //}
                        break;
                    }
                    dt.TableName = "PROMODETAILS";
                    dtcopy = dt.Copy();
                    //ds.Tables.Add(dt);
                    // ObjReq.ShippingTable = ds;
                    ObjReq.PromoDetails.Tables.Add(dtcopy);
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    ObjResp = serviceCRM.CRMUploadPromoDetails(ObjReq);
                    if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("DiscountPromoErrUpload_" + ObjResp.responseDetails.ResponseCode);
                        ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                dt = null;
                dtcopy = null;
                ObjReq.PromoDetails = null;
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public ActionResult DownloadDiscountPromoTemplate()
        {
            string filename = "DiscountPromo.csv";
            string fullpath = "";
            try
            {
                fullpath = Path.Combine(this.Server.MapPath("~/App_Data/SampleExcelFormat/DiscountPromo.csv"));
                return this.File(fullpath, "application/vnd.ms-excel", filename);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }


            finally
            {
                filename = null;
                fullpath = null;
            }
            return null;

        }

        public JsonResult GetPromoDetails_Subscriber(string GetPromDetailsRequest)
        {
            PromoDetailsResponse ObjResp = new PromoDetailsResponse();
            GetPromoRequest ObjReq = JsonConvert.DeserializeObject<GetPromoRequest>(GetPromDetailsRequest);
            //string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetPromoDetails_Subscriber Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                //ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.CRMGetPromoList_Subscriber(ObjReq);
                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetPromDetailsRequestResponse_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetPromoDetails_Subscriber End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-GetPromoDetails_Subscriber - " + this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult InsertPromoDetails_Individual(string InsertIndividualPromDetailsRequest)
        {
            PromoUploadResponse ObjResp = new PromoUploadResponse();
            PromoUploadRequest ObjReq = JsonConvert.DeserializeObject<PromoUploadRequest>(InsertIndividualPromDetailsRequest);
            //string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - InsertPromoDetails_Individual Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                //ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                ObjReq.RequestBy = Convert.ToString(Session["UserName"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.CRMUploadPromoDetails(ObjReq);
                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetPromDetailsRequestResponse_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - InsertPromoDetails_Individual End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-InsertPromoDetails_Individual - " + this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult GetPromoMappingDetails_PendingApprove(string GetPromMappingPendingApproveRequest)
        {
            PromoDetailsResponse ObjResp = new PromoDetailsResponse();
            GetPromoRequest ObjReq = JsonConvert.DeserializeObject<GetPromoRequest>(GetPromMappingPendingApproveRequest);
            //string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetPromoMappingDetails_PendingApprove Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.CRMGetPromoList_Subscriber(ObjReq);
                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetPromDetailsRequestResponse_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetPromoMappingDetails_PendingApprove End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-GetPromoMappingDetails_PendingApprove - " + this.ControllerContext, ex);
            }
            finally
            {
                ObjReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public ActionResult PromoDetails()
        {
            //List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            //menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SERVICES_UploadDiscountPromo").ToList();
            return View();
        }
        #endregion


        #region FRR - 4521

        public ActionResult EposAppReport()
        {
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SERVICES_EposAppReport").ToList();
            return View();
        }

        public JsonResult GetEposDetails(string GetEposRequest)
        {
            EPOSDetailsResponse ObjRes = new EPOSDetailsResponse();
            GetEPOSRequest ObjReq = JsonConvert.DeserializeObject<GetEPOSRequest>(GetEposRequest);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetEposDetails Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMEPOSRequest(ObjReq);
                if (ObjRes != null && ObjRes.ResponseDetails.ResponseCode != null && ObjRes.ResponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetPromDetailsRequestResponse_" + ObjRes.ResponseDetails.ResponseCode);
                    ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetEposDetails End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-GetEposDetails - " + this.ControllerContext, ex);
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

        #region FRR-4696 MourocimmiVerificationStatus
        public ActionResult MourocimmiVerificationStatus()
        {
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SERVICES_MourocimmiVerificationStatus").ToList();
            return View();
        }
        public JsonResult GetMourocimmiVerificationStatus(string GetMourocimmiVerificationStatus)
        {
            MourocimmiVerificationStatusResponse ObjRes = new MourocimmiVerificationStatusResponse();
            MourocimmiVerificationStatusRequest ObjReq = new MourocimmiVerificationStatusRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                ObjReq = JsonConvert.DeserializeObject<MourocimmiVerificationStatusRequest>(GetMourocimmiVerificationStatus);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetMourocimmiVerificationStatus Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                //FRR4920 for Belgium Country 
                if (ObjReq.CountryCode == "BEL")
                    ObjReq.Mode = "GetStatusBEL";
                //End
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetMourocimmiVerificationStatus(ObjReq);
                if (ObjRes != null && ObjRes.responseDetails.ResponseCode != null && ObjRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetMourocimmiVerificationStatusRequestResponse" + ObjRes.responseDetails.ResponseCode);
                    ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;

                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetMourocimmiVerificationStatus End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-GetMourocimmiVerificationStatus - " + this.ControllerContext, ex);
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



        #region FRR 4710
        public ActionResult AuditTrailsDetails()
        {
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SERVICES_AuditTrailsDetails").ToList();
            return View();
        }
        #endregion

        #region FRR 4928
        public ActionResult PortoutRefundReport()
        {
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SERVICES_PortoutRefundReport").ToList();
            return View();
        }



        public JsonResult GetPortoutRefundReport(string GetPortoutRefundReportRequest)
        {
            PortoutRefundReportResponse ObjRes = new PortoutRefundReportResponse();
            PortoutRefundReportRequest ObjReq = JsonConvert.DeserializeObject<PortoutRefundReportRequest>(GetPortoutRefundReportRequest);
            ServiceInvokeCRM serviceCRM;
            List<PortoutRefundReport_list> objlist = new List<PortoutRefundReport_list>();
            string errorInsertMsg = string.Empty;
            PortoutRefundReport_list list = null;

            CRMResponse response = new CRMResponse();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetPortoutRefundReport Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                response.ResponseCode = "0";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMPortoutRefund(ObjReq);

                if(ObjRes.PortoutRefundReportDetails.Count > 0)
                {
                    List<PendingAction> showobjlist = ObjRes.PortoutRefundReportDetails.Select(a => new PendingAction { Id = a.Id, FirstName = a.FirstName, LastName = a.LastName,
                        Msisdn = a.Msisdn, BankAccNO = a.BankAccNO, AccountHolderFirstName = a.AccountHolderFirstName,
                        AccountHolderLastName = a.AccountHolderLastName, DateOfPortOut = a.DateOfPortOut, DateOfRefundRequest = a.DateOfRefundRequest,
                        PrepaidBalance = a.PrepaidBalance, AdminFee = a.AdminFee, EligibileRefundAmt = a.EligibileRefundAmt, SwiftCode = a.SwiftCode,
                        IBAN = a.IBAN,
                        Country = a.Country,
                        Document = a.Document,
                        TransactionID = a.TransactionID,
                        submittedDate = a.submittedDate,
                        submittedBy = a.submittedBy,
                        authorizedBy = a.authorizedBy,
                        authorizedDate = a.authorizedDate,
                        CurrentStatus = a.CurrentStatus,
                        Reason = a.Reason,
                        Level1 = a.Level1,
                        Level1ApprovalStatus = a.Level1ApprovalStatus,
                        Level2 = a.Level2,
                        Level2ApprovalStatus = a.Level2ApprovalStatus,
                        Type = a.Type
                    }).ToList();
                    ObjRes.ShowPendingAction = showobjlist;


                }


                if (ObjRes != null && ObjRes.ResponseDetails.ResponseCode != null && ObjRes.ResponseDetails.ResponseCode != null)
                {
                    if (ObjReq.Status == "2")
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetPortoutRefundReportApprovalIntiate_" + ObjRes.ResponseDetails.ResponseCode);
                        ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;

                    }
                    else if (ObjReq.Status == "4")
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetPortoutRefundReportApproved_" + ObjRes.ResponseDetails.ResponseCode);
                        ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    else
                    {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetPortoutRefundReport_" + ObjRes.ResponseDetails.ResponseCode);
                    ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                   
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetPortoutRefundReport End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-GetPortoutRefundReport - " + this.ControllerContext, ex);
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

        #region  FRR - 4586 FREE sim activation

        public ActionResult WhitelistAndBlacklist()
        {
            WhitelistAndBlacklistRespond ObjRes = new WhitelistAndBlacklistRespond();

            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SERVICES_WhitelistAndBlacklist").ToList();
            return View("WhitelistAndBlacklist", ObjRes);
           
           

            
        }

        [HttpPost]
        public JsonResult WhitelistAndBlacklistDetails(string GetWhiteandblackRequest)
        {
            WhitelistAndBlacklistRespond ObjRes = new WhitelistAndBlacklistRespond();
            GetWhitelistAndBlacklistRequest ObjReq = JsonConvert.DeserializeObject<GetWhitelistAndBlacklistRequest>(GetWhiteandblackRequest);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMWhilteAndBlackList(ObjReq);
                if (ObjRes != null && ObjRes.ResponseDetails.ResponseCode != null && ObjRes.ResponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetWhiteListRequestResponse_" + ObjRes.ResponseDetails.ResponseCode);
                    ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetWhiteListRequestResponse_ End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-GetEposDetails - " + this.ControllerContext, ex);
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
        
        #region FRR-4586 Manage Whitelisted/Blacklisted UserDetails for free sim

        public ActionResult ManageUserDetailsFreeSim()
        {


            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SERVICES_ManageUserDetailsFreeSim").ToList();
            return View();




        }
        public JsonResult GetManageUserDetailsFreeSim(string GetManageUserDetailsFreeSim)
        {
            ManageUserDetailsFreeSimResponse ObjRes = new ManageUserDetailsFreeSimResponse();
            ManageUserDetailsFreeSimRequest ObjReq = JsonConvert.DeserializeObject<ManageUserDetailsFreeSimRequest>(GetManageUserDetailsFreeSim);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetManageUserDetailsFreeSim Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMGetManageUserDetailsFreeSimDetails(ObjReq);
                if (ObjRes != null && ObjRes.responseDetails.ResponseCode != null && ObjRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetManageUserDetailsFreeSimRequestResponse" + ObjRes.responseDetails.ResponseCode);
                    ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetManageUserDetailsFreeSim End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-GetManageUserDetailsFreeSim - " + this.ControllerContext, ex);
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

        #region FRR-4597 Modify Port In

        public ActionResult ModifyPortIn()
        {
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SERVICES_ModifyPortIn").ToList();
            return View();
        }

        public JsonResult GetModifyPortIn(string GetModifyPortIn)
        {
            ModifyPortInResponse ObjRes = new ModifyPortInResponse();
            ModifyPortInRequest ObjReq = JsonConvert.DeserializeObject<ModifyPortInRequest>(GetModifyPortIn);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetModifyPortIn Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CrmGetModifyPortIn(ObjReq);
                if (ObjRes != null && ObjRes.responseDetails.ResponseCode != null && ObjRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetModifyPortInRequestResponse" + ObjRes.responseDetails.ResponseCode);
                    ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetModifyPortIn End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-GetModifyPortIn - " + this.ControllerContext, ex);
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


        #region FRR 4734 
        public ActionResult ESIMPage()
        {
            EsimTopup esimTopup = new EsimTopup();

            CRMeSimResponse ObjRes = new CRMeSimResponse();
            CRMeSIMRequest eSimObjreq = new CRMeSIMRequest();
            ServiceInvokeCRM serviceCRM;
            
            try
            {
                //6217
                if (!string.Equals(clientSetting.preSettings.EnableAltanIntegration, "TRUE", StringComparison.OrdinalIgnoreCase))
                {
                esimTopup.SWEdrpdown = Utility.GetDropdownMasterFromDB("23,30", "1", "drop_master");
                }
                else
                {
                    if ((Session["ATR_ID"] != null && (Session["ATR_ID"].ToString().ToUpper() == "ALL" ||string.IsNullOrEmpty(Session["ATR_ID"].ToString())) ) || Session["ATR_ID"] == null)
                    {
                        eSimObjreq.CountryCode = clientSetting.countryCode;
                        eSimObjreq.BrandCode = clientSetting.brandCode;
                        eSimObjreq.LanguageCode = clientSetting.langCode;
                        eSimObjreq.Mode = "GETATRID";
                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        ObjRes = serviceCRM.CRMeSIMDetails(eSimObjreq);
                        esimTopup.ATRIDList = ObjRes.ATRIDList;
                    }
                }
                esimTopup.lstCardTypes = Utility.GetDropdownMasterFromDB("12", "1", "drop_master"); 
                return View(esimTopup);
            }
            catch (Exception Ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - ESIMPage - exp - " + this.ControllerContext, Ex);
                return View(esimTopup);

            }
           
        }


        public JsonResult GetGenerateeSIM(CRMeSIMRequest eSimObjreq)
        {
            CRMeSimResponse ObjRes = new CRMeSimResponse();
            CRMResponse rs = new CRMResponse();
         
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
               
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - eSimRequestResponse Start");
                eSimObjreq.CountryCode = clientSetting.countryCode;
                eSimObjreq.BrandCode = clientSetting.brandCode;
                eSimObjreq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMeSIMDetails(eSimObjreq);
                int count = 1;

                if (ObjRes.eSimList.Count > 0)
                {
                    foreach (var x in ObjRes.eSimList) x.SNo = count++;
                }



                if (ObjRes != null && ObjRes.responseDetails.ResponseCode != null && ObjRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("eSimRequestResponse_" + ObjRes.responseDetails.ResponseCode);
                    ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;
                }


                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - eSimRequestResponse End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-eSimRequestResponse - " + this.ControllerContext, ex);
            }
            finally
            {
                eSimObjreq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }

            return new JsonResult() { Data = ObjRes, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

       #endregion
       

       #region FRR 4994
        public ActionResult DigitalInvoiceReport()
        {
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SERVICES_DigitalInvoiceReport").ToList();
            return View();
        }
        #endregion
        
        
         #region FRR- 4754

        //4754
        public ActionResult EuronetPromotion()
        {
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SERVICES_EuronetPromotion").ToList();
            return View();
        }

        public JsonResult GetEuronetpromotion(string Euronetpromocoderequest)
        {

            EuronetpromocodestatusRequest euronetpromocodestatusReq = JsonConvert.DeserializeObject<EuronetpromocodestatusRequest>(Euronetpromocoderequest);
            EuronetpromocodestatusResp euronetpromocodestatusResp = new EuronetpromocodestatusResp();
            ServiceInvokeCRM serviceCRM;
            try
            {
                euronetpromocodestatusReq.CountryCode = clientSetting.countryCode;
                euronetpromocodestatusReq.BrandCode = clientSetting.brandCode;
                euronetpromocodestatusReq.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetPortoutRefundReport Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                euronetpromocodestatusResp = serviceCRM.CRMEuronetPromotionStatus(euronetpromocodestatusReq);
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - EuronetpromotionRequestResponse End");

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "ServiceController - exception-EuronetpromotionRequestResponse - " + this.ControllerContext, ex);

            }
            finally
            {
                euronetpromocodestatusReq = null;
            }

            return Json(euronetpromocodestatusResp, JsonRequestBehavior.AllowGet);

        }



        #endregion

        #region FRR 4981
        public ActionResult DownloadPOA()
        {
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SERVICES_DownloadPOA").ToList();
            return View();
        }

        public JsonResult POADownload(MNPGetPOAIDFormDetails POADownreq)
        {
            MNPGetPOAIDFormRESPONSE objRes = new MNPGetPOAIDFormRESPONSE();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            string errorInsert = string.Empty;
           
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - Json DigitalInvoiceReport Start");
                POADownreq.CountryCode = clientSetting.countryCode;
                POADownreq.BrandCode = clientSetting.brandCode;
                POADownreq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.POADownload(POADownreq);
                              
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
            }
            return new JsonResult() { Data = objRes, MaxJsonLength = int.MaxValue };
        }
        #endregion
    }
}



