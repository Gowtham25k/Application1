using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CRM.Models;
using Newtonsoft.Json;
using ServiceCRM;
using System.Web;
using System.Text.RegularExpressions;

namespace CRM.Controllers
{
    [ValidateState]
    public class AccountController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();

        [HttpGet]
        public ViewResult Settings()
        {
            AccountSettingsResponse accountSettingsResp = new AccountSettingsResponse();
            AccountSettingsRequest accountSettingsReq = new AccountSettingsRequest();
            BalTrans balTrans = new BalTrans();
            // 4925
            string MSISDNvalidate = string.Empty;
            ServiceInvokeCRM serviceCRM;
            try
            {
                new LoginController().AuditTrailCRM(new AuditTrailRequest() { action = "Account Settings", description = "Account Settings Page Load Start", module = "Account", subModule = "Settings", DescID = "ManageAcntSettings_Page" });

                accountSettingsReq.CountryCode = clientSetting.countryCode;
                accountSettingsReq.BrandCode = clientSetting.brandCode;
                accountSettingsReq.LanguageCode = clientSetting.langCode;
                
                accountSettingsReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                accountSettingsReq.ICCID = Convert.ToString(Session["ICCID"]);
                accountSettingsReq.isPrePost = Convert.ToString(Session["isPrePaid"]);
                MSISDNvalidate = Convert.ToString(Session["MobileNumber"]);

                accountSettingsReq.mode = "Q"; // Query Mode


                //6008

                List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
                menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "Account_Settings").ToList();
                if(menu[0].MenuFeatures.Where(a => a.Features == "PreloadedBundle" && a.Enable == "True").ToList().Count > 0)
                {
                    accountSettingsReq.CheckPreloadedMenu = "TRUE";
                }

                #region Hitting Account Settings Service
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    accountSettingsResp = serviceCRM.AccountSettingsCRM(accountSettingsReq);
                    try
                    {
                        accountSettingsResp.accountInfoDetail.EXPIRY = Utility.GetDateconvertion(accountSettingsResp.accountInfoDetail.EXPIRY, "dd/mm/yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                        accountSettingsResp.accountInfoDetail.LASTUPDATEDON = Utility.GetDateconvertion(accountSettingsResp.accountInfoDetail.LASTUPDATEDON, "dd/mm/yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                        if (accountSettingsResp.accountInfoDetail.SimExpiryDate != null)
                        {
                            accountSettingsResp.accountInfoDetail.SimExpiryDate = accountSettingsResp.accountInfoDetail.SimExpiryDate;
                        }
                        else
                        {
                            accountSettingsResp.accountInfoDetail.SimExpiryDate = "NA";
                        }
                    }
                    catch (Exception exSettings1)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exSettings1);
                    }

                    if (accountSettingsResp != null && accountSettingsResp.responseDetails != null && accountSettingsResp.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("AccountSettings_" + accountSettingsResp.responseDetails.ResponseCode);
                        accountSettingsResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? accountSettingsResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                
                #endregion

                if (accountSettingsResp.responseDetails.ResponseCode == "0")
                {
                    Session["ICCID"] = accountSettingsResp.accountInfoDetail.ICCID.Trim();
                    Session["IMSI"] = accountSettingsResp.accountInfoDetail.IMSI.Trim();
                    Session["MaskMode"] = accountSettingsResp.accountInfoDetail.MaskMode;
                    Session["PlanId"] = accountSettingsResp.accountInfoDetail.PlanId;
                    Session["PUK"] = accountSettingsResp.accountInfoDetail.PUK.Trim();
                    Session["SIMSTATUS"] = accountSettingsResp.accountInfoDetail.SIMSTATUS;
                    Session["SUBSTATUS"] = accountSettingsResp.accountInfoDetail.SUBSTATUS;
                    Session["SUBSTYPE"] = accountSettingsResp.accountInfoDetail.SUBSTYPE;
                    Session["eMailID"] = accountSettingsResp.accountInfoDetail.eMailId;
                    accountSettingsReq.SIM_CATEGORY = Convert.ToString(Session["SIM_CATEGORY"]);
                    ViewBag.GO_ONLINE = accountSettingsReq.SIM_CATEGORY;
                    # region RLH Date
                    string strNA = Resources.SubscriberResources.ResourceManager.GetString("NA");
                    if (!string.IsNullOrEmpty(accountSettingsResp.accountInfoDetail.RLHStatusDate) && !string.IsNullOrEmpty(accountSettingsResp.accountInfoDetail.RLHStatus))
                    {
                        if (accountSettingsResp.accountInfoDetail.RLHStatus == "ISR" || accountSettingsResp.accountInfoDetail.RLHStatus == "RLH" || accountSettingsResp.accountInfoDetail.RLHStatus == "RLH_SUR_CHARGE")
                        {
                            try
                            {
                                accountSettingsResp.accountInfoDetail.RLHStatusDate = Utility.GetDateconvertion(accountSettingsResp.accountInfoDetail.RLHStatusDate, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                            }
                            catch (Exception exRLHDateConversion)
                            {
                                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exRLHDateConversion);
                            }
                        }
                        else
                        {
                            accountSettingsResp.accountInfoDetail.RLHStatusDate = strNA;
                            accountSettingsResp.accountInfoDetail.RLHStatus = strNA;
                        }
                    }
                    else
                    {
                        accountSettingsResp.accountInfoDetail.RLHStatusDate = strNA;
                        accountSettingsResp.accountInfoDetail.RLHStatus = strNA;
                    }
                    # endregion
                    #region LoanDate
                    if (!string.IsNullOrEmpty(accountSettingsResp.accountInfoDetail.LoanExpiryDate))
                    {
                        try
                        {
                            accountSettingsResp.accountInfoDetail.LoanExpiryDate = Utility.GetDateconvertion(accountSettingsResp.accountInfoDetail.LoanExpiryDate, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                        }
                        catch (Exception exLoanDateConversion)
                        {
                            CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exLoanDateConversion);
                        }
                    }
                    else
                    {
                        accountSettingsResp.accountInfoDetail.LoanExpiryDate = strNA;
                    }
                    #endregion
                    #region FLH validity Extension
                    if (clientSetting.mvnoSettings.enableFlhValidityExtend.Split('|')[0].ToLower() == "true")//FLHextension config  on
                    {
                        FLHValidityRes ObjRes = new FLHValidityRes();
                        FLHValidityReq objFLHReq = new FLHValidityReq();
                        try
                        {
                            objFLHReq.CountryCode = clientSetting.countryCode;
                            objFLHReq.BrandCode = clientSetting.brandCode;
                            objFLHReq.LanguageCode = clientSetting.langCode;
                            objFLHReq.MSISDN = MSISDNvalidate;
                            objFLHReq.Opcode = "3";
                            serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                                ObjRes = serviceCRM.CRMFLHValidity(objFLHReq);

                                if (!string.IsNullOrEmpty(ObjRes.FLHUsageExpDate))
                                {
                                    try
                                    {
                                        DateTime dt = DateTime.Parse(ObjRes.FLHUsageExpDate);
                                        string strDateTimeFormat = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                                        strDateTimeFormat = strDateTimeFormat.Replace("DD", "dd");
                                        strDateTimeFormat = strDateTimeFormat.Replace("YYYY", "yyyy");
                                        string str = dt.ToString(strDateTimeFormat);
                                        ObjRes.FLHUsageExpDate = str;
                                    }
                                    catch (Exception exFirstUsage)
                                    {
                                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exFirstUsage);
                                    }
                                }
                            
                            if (ObjRes.responseDetails.ResponseCode == "0")
                            {
                                accountSettingsResp.isFlhavailable = "1";
                                accountSettingsResp.flhExpirydate = ObjRes.FLHUsageExpDate;
                            }
                            else
                            {
                                accountSettingsResp.isFlhavailable = "0";
                            }
                        }
                        catch (Exception exFLHValidityExtension)
                        {
                            CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exFLHValidityExtension);
                        }
                        finally
                        {
                            ObjRes = null;
                            objFLHReq = null;
                        }
                    }
                    #endregion
                    #region Get Ticket SMS
                    if (clientSetting.mvnoSettings.EnableTickettingSMS.ToLower() == "on")//FLHextension config  on
                    {
                        SMSOptInOutResponse ObjRes = new SMSOptInOutResponse();
                        SMSOptInOutRequst objReq = new SMSOptInOutRequst();
                        try
                        {
                            objReq.CountryCode = clientSetting.countryCode;
                            objReq.BrandCode = clientSetting.brandCode;
                            objReq.LanguageCode = clientSetting.langCode;
                            objReq.Msisdn = MSISDNvalidate;
                            objReq.Mode = "GET";
                            objReq.Channel = "CRM";
                            serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                                ObjRes = serviceCRM.TicketSMS(objReq);
                                if (!string.IsNullOrEmpty(ObjRes.ResponseCode))
                                {
                                    accountSettingsResp.TickettingSMS = ObjRes.OptStatus;
                                }
                            
                        }
                        catch (Exception exTicketSMS)
                        {
                            CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exTicketSMS);
                        }
                        finally
                        {
                            ObjRes = null;
                            objReq = null;
                        }
                    }
                    #endregion
                    #region Assign Life Cycyle Status
                    try
                    {
                        Session["LIFECYCLESTATE"] = Convert.ToString(Session["isPrePaid"]) == "1" ? accountSettingsResp.accountInfoDetail.LIFECYCLESTATE : SettingsCRM.postLyfeCycStatus.FirstOrDefault(lcs => lcs.Attribute("ID").Value == accountSettingsResp.accountInfoDetail.LIFECYCLESTATE).Value;
                    }
                    catch (Exception exLifeCycleStatus)
                    {
                        Session["LIFECYCLESTATE"] = accountSettingsResp.accountInfoDetail.LIFECYCLESTATE;
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exLifeCycleStatus);
                    }
                    #endregion
                    #region Assign SLCM Life Cycle State
                    if (clientSetting.mvnoSettings.enableregularSLCM == "0")
                    {
                        string strSLCM = string.Empty;
                        try
                        {
                            strSLCM = SettingsCRM.slcmLifeCycleState.FirstOrDefault(lcs => lcs.Attribute("ID").Value == accountSettingsResp.accountInfoDetail.LIFECYCLESTATE.ToLower()).Value;
                            Session["SLCMLIFECYCLESTATE"] = Convert.ToString(strSLCM);
                            accountSettingsResp.accountInfoDetail.LIFECYCLESTATE = strSLCM;
                        }
                        catch (Exception exSLCMLifeCycleState)
                        {
                            Session["SLCMLIFECYCLESTATE"] = accountSettingsResp.accountInfoDetail.LIFECYCLESTATE;
                            CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exSLCMLifeCycleState);
                        }
                    }
                    #endregion
                    accountSettingsResp.accountInfoDetail.activatedOn = accountSettingsResp.accountInfoDetail.activatedOn.FormaDateTime(clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.MMddyyyy);
                    accountSettingsResp.accountInfoDetail.registeredOn = accountSettingsResp.accountInfoDetail.registeredOn.FormaDateTime(clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.MMddyyyy);
                    if (accountSettingsResp.linkedSimDetail != null && accountSettingsResp.linkedSimDetail.Count > 0)
                    {
                        accountSettingsResp.linkedSimDetail.ForEach(s => s.dateRegistered = s.dateRegistered.FormaDateTime(clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.yyyyMMdd));
                    }
                    //4831
                   accountSettingsResp.accountInfoDetail.RNE_Lastupdate = accountSettingsResp.accountInfoDetail.RNE_Lastupdate.FormaDateTime(clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.yyyyMMdd);

                    if (accountSettingsResp.accountInfoDetail.blockReason != null && accountSettingsResp.accountInfoDetail.blockReason != "")
                    {
                        int numericValue;
                        bool isNumeric = int.TryParse(accountSettingsResp.accountInfoDetail.blockReason, out numericValue);
                        if (isNumeric)
                        {
                            balTrans.ticketCRM.dicTicketReasons = Utility.DataTableToDictionary(Utility.GetDropdownMasterFromDB("tbl_balance_transfer_reason"));
                            //accountSettingsResp.accountInfoDetail.blockReason =  balTrans.ticketCRM.dicTicketReasons.FirstOrDefault()
                            foreach (var lAction in balTrans.ticketCRM.dicTicketReasons)
                            {
                                if (lAction.Key == accountSettingsResp.accountInfoDetail.blockReason)
                                {
                                    accountSettingsResp.accountInfoDetail.blockReason = lAction.Value;
                                }
                            }
                        }
                    }
                }
                else if (accountSettingsResp.responseDetails.ResponseCode == "1025" || accountSettingsResp.responseDetails.ResponseCode == "1000" || accountSettingsResp.responseDetails.ResponseCode == "1011" || accountSettingsResp.responseDetails.ResponseCode == "2000")
                {
                    //accountResp.responseDetails.ResponseDesc = @Resources.SubscriberResources.ServiceDown;
                    accountSettingsResp.responseDetails.ResponseCode = "1";
                }
                else if (accountSettingsResp.responseDetails.ResponseCode == "1010" || accountSettingsResp.responseDetails.ResponseCode == "1" || accountSettingsResp.responseDetails.ResponseCode == "4" || accountSettingsResp.responseDetails.ResponseCode == "003")
                {
                    //accountResp.responseDetails.ResponseDesc = @Resources.SubscriberResources.InvalidSubscriber;
                    accountSettingsResp.responseDetails.ResponseCode = "4";
                }
                else if (accountSettingsResp.responseDetails.ResponseCode == "1016")
                {
                    // accountResp.responseDetails.ResponseDesc = "MSISDN has been in black list";
                    accountSettingsResp.responseDetails.ResponseCode = "2";
                }
                else if (accountSettingsResp.responseDetails.ResponseCode == "1009")
                {
                    //accountResp.responseDetails.ResponseDesc = "Subscriber is first used or invalid";
                    accountSettingsResp.responseDetails.ResponseCode = "2";
                }
                else if (accountSettingsResp.responseDetails.ResponseCode == "1100")
                {
                    //accountResp.responseDetails.ResponseDesc = "Subscriber not activated";
                    accountSettingsResp.responseDetails.ResponseCode = "2";
                }
                else if (accountSettingsResp.responseDetails.ResponseCode == "002")
                {
                    // accountResp.responseDetails.ResponseDesc = "MSISDN MANDATORY";
                    accountSettingsResp.responseDetails.ResponseCode = "2";
                }
                else if (accountSettingsResp.responseDetails.ResponseCode == "001")
                {
                    // accountResp.responseDetails.ResponseDesc = "OPERATION UNSUCCESSFUL";
                    accountSettingsResp.responseDetails.ResponseCode = "2";
                }
                else if (accountSettingsResp.responseDetails.ResponseCode == "15")
                {
                    //  accountResp.responseDetails.ResponseDesc = "Networkid is mismatched";
                    accountSettingsResp.responseDetails.ResponseCode = "2";
                }
                else if (accountSettingsResp.responseDetails.ResponseCode == "14")
                {
                    // accountResp.responseDetails.ResponseDesc = "Msisdn is ported out";
                    accountSettingsResp.responseDetails.ResponseCode = "2";
                }
                else
                {
                    accountSettingsResp.responseDetails.ResponseDesc = accountSettingsResp.responseDetails.ResponseDesc;
                    accountSettingsResp.responseDetails.ResponseCode = "2";
                }
                if (accountSettingsResp.accountInfoDetail.LIFECYCLESTATE != null)
                {


                    if (clientSetting.preSettings.EnableReplacenameforlifeclyclestate.ToUpper() == "TRUE")
                    {
                    string MultilangSimstatus = Resources.DropdownResources.ResourceManager.GetString("Simstatus_" + Regex.Replace(accountSettingsResp.accountInfoDetail.LIFECYCLESTATE.ToLower(), @"[^0-9a-zA-Z\._]", string.Empty));
                    accountSettingsResp.multilangSimstatus = string.IsNullOrEmpty(MultilangSimstatus) ? accountSettingsResp.accountInfoDetail.LIFECYCLESTATE : MultilangSimstatus;
                    }
                    else
                    {
                        accountSettingsResp.multilangSimstatus = accountSettingsResp.accountInfoDetail.LIFECYCLESTATE;
                    }
                }
                return View(accountSettingsResp);
            }
            catch (Exception eXSettings)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eXSettings);
                return View(accountSettingsResp);
            }
            finally
            {
                new LoginController().AuditTrailCRM(new AuditTrailRequest() { action = "Account Settings", description = "Account Settings Page Load End", module = "Account", subModule = "Settings", DescID = "ManageAcntSettings_Page" });
                accountSettingsReq = null;
                balTrans = null;
                serviceCRM = null;
            }
        }
        public ActionResult ManageSettings(AccountSettingsRequest mngAcntSetReq)
        {
            AccountSettingsResponse accountSettingsResp = new AccountSettingsResponse();
            string resID = "ManageAcntSettings_Req";
            ServiceInvokeCRM serviceCRM;
            try
            {
                new LoginController().AuditTrailCRM(new AuditTrailRequest() { action = "Account Settings - " + mngAcntSetReq.mode, description = "Account Settings Request", module = "Account", subModule = "Settings", DescID = resID });

                mngAcntSetReq.CountryCode = clientSetting.countryCode;
                mngAcntSetReq.BrandCode = clientSetting.brandCode;
                mngAcntSetReq.LanguageCode = clientSetting.langCode;
                mngAcntSetReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                mngAcntSetReq.ICCID = Convert.ToString(Session["ICCID"]);
                mngAcntSetReq.IMSI = Convert.ToString(Session["IMSI"]);
                mngAcntSetReq.pukCode = Convert.ToString(Session["PUK"]);
                mngAcntSetReq.email = Convert.ToString(Session["eMailID"]);
                mngAcntSetReq.isPrePost = Convert.ToString(Session["isPrePaid"]);
                mngAcntSetReq.userName = Convert.ToString(Session["UserName"]);

                #region Account Settings CRM service Hitting
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    accountSettingsResp = serviceCRM.AccountSettingsCRM(mngAcntSetReq);
                
                #endregion

                if (accountSettingsResp.responseDetails != null && mngAcntSetReq.mode == "CIPR")
                {
                    #region Account Settings Mode = CIPR
                    resID = "ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode;
                    string errorMsg = Resources.ErrorResources.ResourceManager.GetString("ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode);
                    if (accountSettingsResp.responseDetails.ResponseDesc.Contains("|"))
                    {
                        List<string> lstResp = new List<string>(accountSettingsResp.responseDetails.ResponseDesc.Split('|'));
                        if (clientSetting.mvnoSettings.maskCIPIN.Trim().ToLower() == "true")
                        {
                            accountSettingsResp.responseDetails.ResponseDesc = Utility.MaskString(lstResp[0].Length, '*') + "|" + errorMsg;
                        }
                        else
                        {
                            accountSettingsResp.responseDetails.ResponseDesc = lstResp[0] + "|" + (string.IsNullOrEmpty(errorMsg) ? accountSettingsResp.responseDetails.ResponseDesc : errorMsg);
                        }
                        accountSettingsResp.responseDetails.ResponseCode = "0";
                    }
                    else
                    {
                        accountSettingsResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorMsg) ? accountSettingsResp.responseDetails.ResponseDesc : errorMsg;
                    }
                    #endregion
                }
                else if (accountSettingsResp.responseDetails != null && mngAcntSetReq.mode == "GMHA")
                {
                    #region Account Settings Mode = GMHA
                    if (accountSettingsResp.responseDetails.ResponseDesc.Contains("|"))
                    {
                        List<string> lstResp = new List<string>(accountSettingsResp.responseDetails.ResponseDesc.Split('|'));
                        resID = "ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode;
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode);
                        accountSettingsResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? lstResp[0] : errorInsertMsg;
                        accountSettingsResp.responseDetails.ResponseDesc = accountSettingsResp.responseDetails.ResponseDesc + "|" + lstResp[1] + "|" + lstResp[2];
                    }
                    else
                    {
                        resID = "ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode;
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode);
                        accountSettingsResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? accountSettingsResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                    #endregion
                }
                else if (accountSettingsResp.responseDetails != null && accountSettingsResp.responseDetails.ResponseCode != null)
                {
                    #region Account Settings ResponseCode is NOT NULL
                    string responsemsg = "";
                    string[] responsecode = { };
                    string[] responsemessage = { };
                    string InsertMsgResponse = "";
                    if (accountSettingsResp.responseDetails.ResponseCode.Contains("|"))
                    {
                        responsecode = accountSettingsResp.responseDetails.ResponseCode.Split('|');
                        responsemessage = accountSettingsResp.responseDetails.ResponseDesc.Split('|');
                        for (int i = 0; i < responsecode.Length; i++)
                        {
                            if (responsemessage[i].Trim() == "")
                            {
                                responsemsg += "|";
                            }
                            else
                            {
                                InsertMsgResponse = Resources.ErrorResources.ResourceManager.GetString("ManageAcntSettings_" + mngAcntSetReq.mode + "_" + responsecode[i]);
                                responsemsg += (string.IsNullOrEmpty(InsertMsgResponse) ? responsemessage[i] : InsertMsgResponse) + "|";
                            }
                        }
                        accountSettingsResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(responsemsg) ? accountSettingsResp.responseDetails.ResponseDesc : responsemsg;
                    }
                    else
                    {
                        resID = "ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode;
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode);
                        accountSettingsResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? accountSettingsResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                    #endregion
                }

                #region Account Settings Request Mode = BULK
                if (accountSettingsResp.responseDetails != null && mngAcntSetReq.mode == "Bulk")
                {
                    if (accountSettingsResp.responseDetails.ResponseCode.Split('|')[1] == "0")
                    {
                        if (Convert.ToString(Session["MaskMode"]) == "1")
                        {
                            Session["MaskMode"] = "0";
                        }
                        else
                        {
                            Session["MaskMode"] = "1";
                        }
                    }
                }
                #endregion

                #region Account Settings Mode = VPT
                if (accountSettingsResp.responseDetails != null && mngAcntSetReq.mode == "VPT")
                {
                    resID = "voucherTopup_" + accountSettingsResp.responseDetails.ResponseCode;
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("voucherTopup_" + accountSettingsResp.responseDetails.ResponseCode);
                    accountSettingsResp.responseDetails.ResponseDesc = accountSettingsResp.freePinTopup.ERRDESCRITION = string.IsNullOrEmpty(errorInsertMsg) ? accountSettingsResp.responseDetails.ResponseDesc : errorInsertMsg;

                    return PartialView("~/Views/Topup/Response.cshtml", accountSettingsResp.freePinTopup);
                }
                #endregion

                #region Account Settings Mode = SPAG FRR : 4168
                if (accountSettingsResp.responseDetails != null && mngAcntSetReq.mode == "SPAG")
                {
                    if (accountSettingsResp.responseDetails.ResponseDesc.Contains("|"))
                    {
                        List<string> lstResp = new List<string>(accountSettingsResp.responseDetails.ResponseDesc.Split('|'));

                        resID = "ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode;
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode);
                        accountSettingsResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? lstResp[0] : errorInsertMsg;

                        accountSettingsResp.responseDetails.ResponseDesc = accountSettingsResp.responseDetails.ResponseDesc + "|" + lstResp[1] + "|" + lstResp[2];
                    }
                    else
                    {
                        resID = "ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode;
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode);
                        accountSettingsResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? accountSettingsResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                #endregion

                #region Account Settings Mode = SPAS FRR : 4168
                if (accountSettingsResp.responseDetails != null && mngAcntSetReq.mode == "SPAS")
                {
                    if (accountSettingsResp.responseDetails.ResponseDesc.Contains("|"))
                    {
                        List<string> lstResp = new List<string>(accountSettingsResp.responseDetails.ResponseDesc.Split('|'));
                        resID = "ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode;
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode);
                        accountSettingsResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? lstResp[0] : errorInsertMsg;
                        accountSettingsResp.responseDetails.ResponseDesc = accountSettingsResp.responseDetails.ResponseDesc + "|" + lstResp[1] + "|" + lstResp[2];
                    }
                    else
                    {
                        resID = "ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode;
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode);
                        accountSettingsResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? accountSettingsResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                #endregion

                #region Account Settings Mode = CAS  FRR : 4268
                if (accountSettingsResp.responseDetails != null && mngAcntSetReq.mode == "CAS")
                {
                    if (accountSettingsResp.responseDetails.ResponseDesc.Contains("|"))
                    {
                        List<string> lstResp = new List<string>(accountSettingsResp.responseDetails.ResponseDesc.Split('|'));
                        resID = "ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode;
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode);
                        accountSettingsResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? lstResp[0] : errorInsertMsg;
                        accountSettingsResp.responseDetails.ResponseDesc = accountSettingsResp.responseDetails.ResponseDesc + "|" + lstResp[1] + "|" + lstResp[2];
                    }
                    else
                    {
                        resID = "ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode;
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode);
                        accountSettingsResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? accountSettingsResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                #endregion



                #region  Account Setting Mode = NIMSI FRR : 4601
                if (accountSettingsResp.responseDetails != null && mngAcntSetReq.mode == "NIMSI")
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManageAcntSettings_" + mngAcntSetReq.mode + "_" + accountSettingsResp.responseDetails.ResponseCode);
                    accountSettingsResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? accountSettingsResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                #endregion

                return Json(accountSettingsResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(accountSettingsResp.responseDetails, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                new LoginController().AuditTrailCRM(new AuditTrailRequest() { action = "Account Settings - " + mngAcntSetReq.mode, description = "Account Settings Response", module = "Account", subModule = "Settings", DescID = resID });
                serviceCRM = null;
            }
        }

        #region Disability Service
        public ActionResult DisabilityService()
        {
            AccountCRM ObjResp = new AccountCRM();
            DisabilityServiceResponse ObjRespDis = new DisabilityServiceResponse();
            DisabilityServiceRequest ObjReq = new DisabilityServiceRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Disability Service Request");
                ObjResp.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "0";
                ObjResp.strDropdown = Utility.GetDropdownMasterFromDB("19", Convert.ToString(Session["isPrePaid"]), "drop_master");

                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.Mode = "Q";
                ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRespDis = serviceCRM.CRMDisabilityService(ObjReq);
                    if (ObjRespDis != null && ObjRespDis.disabilityService != null && ObjRespDis.disabilityService.isServiceEnabled != null && ObjRespDis.disabilityService.isServiceEnabled == "SUBSCRIBE")
                    { ObjResp.Subscribestatus = "1"; }
                    else { ObjResp.Subscribestatus = "0"; }
                
                return View(ObjResp);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View(ObjResp);
            }
            finally
            {
                ObjReq = null;
                ObjRespDis = null;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Disability Service Respomse");
            }
        }
        public ActionResult NominateDetails(string strNomineeDetails)
        {
            DisabilityServiceResponse ObjResp = new DisabilityServiceResponse();
            DisabilityServiceRequest ObjReq = JsonConvert.DeserializeObject<DisabilityServiceRequest>(strNomineeDetails);
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Nominate Details Request");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjResp = serviceCRM.CRMDisabilityService(ObjReq);
                
                #region Nominate Details Service for Insertion
                if (ObjReq.Mode == "I")
                {
                    if (ObjReq.BulkUpdateFlag.ToUpper() != "YES")
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Disability_Ins_" + ObjResp.responseDetails.ResponseCode);
                        ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                    else
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("DisabilityBulk_Ins_" + ObjResp.responseDetails.ResponseCode);
                        ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                #endregion
                #region Nominate Details Service for Updation
                if (ObjReq.Mode == "U")
                {
                    if (ObjReq.BulkUpdateFlag.ToUpper() != "YES")
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Disability_Upd_" + ObjResp.responseDetails.ResponseCode);
                        ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                    else
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("DisabilityBulk_Upd_" + ObjResp.responseDetails.ResponseCode);
                        ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                #endregion
                #region Nominate Details Service for Deletion
                if (ObjReq.Mode == "D")
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Disability_Del_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;

                }
                #endregion
                #region Nominate Details Service for Query
                if (ObjReq.Mode == "Q" && ObjResp.nomineeDetails.Count > 0)
                {
                    ObjResp.nomineeDetails.ForEach(a =>
                    {
                        a.dob = Utility.GetDateconvertion(a.dob, "DD-MM-YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);
                    });
                }
                #endregion
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
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Nominate Details Response");
            }
        }
        public ActionResult NominateDetailsPostpaid(string strNomineeDetails)
        {
            PostPaidDisabilityServiceResponse ObjResp = new PostPaidDisabilityServiceResponse();
            PostPaidDisabilityServiceRequest ObjReq = JsonConvert.DeserializeObject<PostPaidDisabilityServiceRequest>(strNomineeDetails);
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Nominate Details Postpaid Request");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjResp = serviceCRM.CRMPostPaidDisabilityService(ObjReq);
                
                if (ObjReq.Mode == "I")
                {
                    if (ObjReq.BulkUpdateFlag.ToUpper() != "YES")
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Disability_Ins_" + ObjResp.responseDetails.ResponseCode);
                        ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                    else
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("DisabilityBulk_Ins_" + ObjResp.responseDetails.ResponseCode);
                        ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;

                    }

                }//for insertion
                if (ObjReq.Mode == "U")
                {
                    if (ObjReq.BulkUpdateFlag.ToUpper() != "YES")
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Disability_Upd_" + ObjResp.responseDetails.ResponseCode);
                        ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                    else
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("DisabilityBulk_Upd_" + ObjResp.responseDetails.ResponseCode);
                        ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;

                    }
                }
                if (ObjReq.Mode == "D")
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Disability_Del_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;

                }
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
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Nominate Details Postpaid Response");
            }
        }
        #endregion

        public ActionResult EligibilityFLH()
        {
            FLHEligibilityResponse ObjRes = new FLHEligibilityResponse();
            FLHEligibilityRequest objbundleReq = new FLHEligibilityRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Eligibility FLH Request");
                objbundleReq.CountryCode = clientSetting.countryCode;
                objbundleReq.BrandCode = clientSetting.brandCode;
                objbundleReq.LanguageCode = clientSetting.langCode;
                objbundleReq.Msisdn = Session["MobileNumber"].ToString();
               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMFLHEligibilitySet(objbundleReq);
                
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
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Eligibility FLH Response");
            }
        }

        #region Number Lock

        public ActionResult NumberLocker()
        {
            DoMSISDNSafeCustodyRes ObjResp = new DoMSISDNSafeCustodyRes();
            DoMSISDNSafeCustodyReq ObjReq = new DoMSISDNSafeCustodyReq();
            CRMResponse objRes = new CRMResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Number Locker Start");
                objRes = Utility.checkValidSubscriber("", clientSetting);
                if (objRes.ResponseCode != "102" && objRes.ResponseCode != "106")
                {
                    ObjReq.CountryCode = clientSetting.countryCode;
                    ObjReq.BrandCode = clientSetting.brandCode;
                    ObjReq.LanguageCode = clientSetting.langCode;
                    ObjReq.MSISDN = Session["MobileNumber"].ToString();
                    ObjReq.Mode = "1";
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        ObjResp = serviceCRM.DoMSISDNSafeCustody(ObjReq);
                    
                    if (ObjResp.Status == "1")
                    {
                        #region Date Convertion
                        if (ObjResp.StartDate != string.Empty)
                        {
                            string strDOB = Utility.FormatDateTime(ObjResp.StartDate, clientSetting.mvnoSettings.dateTimeFormat);
                            string[] strSplit = strDOB.Split(' ');
                            ObjResp.StartDate = strSplit[0];
                        }
                        else
                        {
                            ObjResp.StartDate = "";
                        }
                        if (ObjResp.ExpiryDate != string.Empty)
                        {
                            string strDOB = Utility.FormatDateTime(ObjResp.ExpiryDate, clientSetting.mvnoSettings.dateTimeFormat);
                            string[] strSplit = strDOB.Split(' ');
                            ObjResp.ExpiryDate = strSplit[0];
                        }
                        else
                        {
                            ObjResp.ExpiryDate = "";
                        }

                        #endregion
                    }
                    if (ObjResp != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("NumbLock_" + ObjResp.ResponseDeatils.ResponseCode);
                        ObjResp.ResponseDeatils.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.ResponseDeatils.ResponseDesc : errorInsertMsg;
                    }
                }
                else
                {
                    ObjResp.ResponseDeatils = new CRMResponse();
                    ObjResp.ResponseDeatils.ResponseCode = "1";
                    ObjResp.ResponseDeatils.ResponseDesc = objRes.ResponseDesc;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Number Locker End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objRes = null;
                ObjReq = null;
                serviceCRM = null;
            }
            return View(ObjResp);
        }

        [HttpPost]
        public JsonResult SaveNumberLocker(string strNumberLocker)
        {
            DoMSISDNSafeCustodyReq objReg = JsonConvert.DeserializeObject<DoMSISDNSafeCustodyReq>(strNumberLocker);
            DoMSISDNSafeCustodyRes objRes = new DoMSISDNSafeCustodyRes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Save Number Locker Start");

                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;

                #region Date Convertion
                if (objReg.StartDate != null)
                {
                    objReg.StartDate = Utility.GetDateconvertion(objReg.StartDate, "YYYY/MM/DD", true, clientSetting.mvnoSettings.dateTimeFormat);
                }
                if (objReg.ExpiryDate != null)
                {
                    objReg.ExpiryDate = Utility.GetDateconvertion(objReg.ExpiryDate, "YYYY/MM/DD", true, clientSetting.mvnoSettings.dateTimeFormat);
                }
                #endregion

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.DoMSISDNSafeCustody(objReg);
                
                if (objRes != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("NumbLock_" + objRes.ResponseDeatils.ResponseCode);
                    objRes.ResponseDeatils.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDeatils.ResponseDesc : errorInsertMsg;
                }
                else
                {
                    objRes = new DoMSISDNSafeCustodyRes();
                    objRes.ResponseDeatils.ResponseDesc = Resources.ErrorResources.Common_2;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.ResponseDeatils.ResponseDesc);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Save Number Locker End");
            }
            catch (Exception ex)
            {
                objRes = new DoMSISDNSafeCustodyRes();
                objRes.ResponseDeatils.ResponseCode = "2";
                objRes.ResponseDeatils.ResponseDesc = ex.ToString();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objReg = null;
                serviceCRM = null;
            }
            return Json(objRes);
        }

        #endregion

        public PartialViewResult ConfigEU()
        {
            EUConfigurationResponse euConfigResp = new EUConfigurationResponse();
            EUConfigurationRequest euConfigReq = new EUConfigurationRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ConfigEU Start");

                euConfigReq.CountryCode = clientSetting.countryCode;
                euConfigReq.BrandCode = clientSetting.brandCode;
                euConfigReq.LanguageCode = clientSetting.langCode;
                euConfigReq.MSISDN = Session["MobileNumber"].ToString();
                euConfigReq.mode = "Q";

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    euConfigResp = serviceCRM.EUConfigurationCRM(euConfigReq);
                
                if (euConfigResp.responseDetails != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ConfigEU_" + euConfigResp.responseDetails.ResponseCode);
                    euConfigResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? euConfigResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                euConfigResp.activationDate = string.IsNullOrEmpty(euConfigResp.activationDate) ? DateTime.Now.AddYears(-1).ToString("yyyy/MM/dd") : Convert.ToDateTime(euConfigResp.activationDate).ToString("yyyy/MM/dd");
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ConfigEU End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                euConfigReq = null;
                serviceCRM = null;
            }
            return PartialView(euConfigResp);
        }

        public JsonResult ManageConfigEU(EUConfigurationRequest euConfigReq)
        {
            EUConfigurationResponse euConfigResp = new EUConfigurationResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ManageConfigEU Start");

                euConfigReq.CountryCode = clientSetting.countryCode;
                euConfigReq.BrandCode = clientSetting.brandCode;
                euConfigReq.LanguageCode = clientSetting.langCode;
                euConfigReq.MSISDN = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    euConfigResp = serviceCRM.EUConfigurationCRM(euConfigReq);
                
                if (euConfigResp.responseDetails != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManageConfigEU_" + euConfigResp.responseDetails.ResponseCode);
                    euConfigResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? euConfigResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ManageConfigEU End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                //euConfigResp = null;
                serviceCRM = null;
            }
            return Json(euConfigResp.responseDetails);
        }

        #region PremiumNoService

        public ActionResult ManagePremiumNumber()
        {
            ManagePremiumNumberRequest ObjReq = new ManagePremiumNumberRequest();
            ManagePremiumNumberResponse ObjRes = new ManagePremiumNumberResponse();
            categories objCateResponse = new categories();
            List<categories> objCategories = new List<categories>();
            CRMResponse ObjResp = new CRMResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ManagePremiumNumber Start");
                ObjResp = Utility.checkValidSubscriber("1", clientSetting);
                if (clientSetting.mvnoSettings.ExistingPremiumNumber.ToUpper() == "ON")
                {
                    if (ObjResp.ResponseCode == "0")
                    {
                        ObjReq.CountryCode = clientSetting.countryCode;
                        ObjReq.BrandCode = clientSetting.brandCode;
                        ObjReq.LanguageCode = clientSetting.langCode;
                        ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                        ObjReq.Mode = "GQ";   //For GetMethod
                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        
                            ObjRes = serviceCRM.CRMPremiumNumberService(ObjReq);
                        
                        if (ObjRes.responseDetails != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PremiumNo_Ins_" + ObjRes.responseDetails.ResponseCode);
                            ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;

                            if (ObjRes.premiumcategories.Count > 0)
                            {
                                ObjRes.premiumcategories.ForEach(b => b.ExpirationDate = Utility.GetDateconvertion(b.ExpirationDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat));
                                ObjRes.premiumcategories.ForEach(b => b.CateServiceType = b.CateServiceType == "1" ? Resources.AccountResources.Call : @Resources.AccountResources.SMS);
                                ObjRes.premiumcategories.ForEach(b => b.Duration = b.CateActionFlag == "2" ? Resources.AccountResources.Premiumvaliditydays : "0");
                                ObjRes.premiumcategories.ForEach(b => b.CateStatus = b.CateActionFlag == "1" ? Resources.AccountResources.Disabled : Resources.AccountResources.Limited);
                                ObjRes.premiumcategories.ForEach(b => b.Duration = b.CateActionFlag == "1" ? Resources.AccountResources.NA : b.Duration);
                                ObjRes.premiumcategories.ForEach(b => b.ExpirationDate = b.CateActionFlag == "1" ? Resources.AccountResources.NA : b.ExpirationDate);
                                ObjRes.premiumcategories.ForEach(b => b.CateLimitAmount = b.CateActionFlag == "1" ? Resources.AccountResources.NA : b.CateLimitAmount);
                            }
                            if (ObjRes.SMScategories.Count > 0)
                            {
                                ObjRes.SMScategories.ForEach(b => b.ExpirationDate = Utility.GetDateconvertion(b.ExpirationDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat));
                                ObjRes.SMScategories.ForEach(b => b.CateServiceType = b.CateServiceType == "2" ? @Resources.AccountResources.SMS : Resources.AccountResources.Call);
                                ObjRes.SMScategories.ForEach(b => b.Duration = b.CateActionFlag == "2" ? Resources.AccountResources.Premiumvaliditydays : "0");
                                ObjRes.SMScategories.ForEach(b => b.CateStatus = b.CateActionFlag == "1" ? Resources.AccountResources.Disabled : Resources.AccountResources.Limited);
                                ObjRes.SMScategories.ForEach(b => b.Duration = b.CateActionFlag == "1" ? Resources.AccountResources.NA : b.Duration);
                                ObjRes.SMScategories.ForEach(b => b.ExpirationDate = b.CateActionFlag == "1" ? Resources.AccountResources.NA : b.ExpirationDate);
                                ObjRes.SMScategories.ForEach(b => b.CateLimitAmount = b.CateActionFlag == "1" ? Resources.AccountResources.NA : b.CateLimitAmount);
                            }
                        }
                    }
                    else
                    {
                        ObjRes.responseDetails = new CRMResponse();
                        ObjRes.responseDetails.ResponseCode = "21";
                        ObjRes.responseDetails.ResponseDesc = ObjResp.ResponseDesc;
                    }
                }
                else
                {
                    //FRR 3476
                    ObjReq.CountryCode = clientSetting.countryCode;
                    ObjReq.BrandCode = clientSetting.brandCode;
                    ObjReq.LanguageCode = clientSetting.langCode;
                    ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    ObjReq.Mode = "GQ";   //For GetMethod
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        ObjRes = serviceCRM.NewPremiumNoDetailService(ObjReq);
                    
                    if (ObjRes.responseDetails != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PremiumNo_Ins_" + ObjRes.responseDetails.ResponseCode);
                        ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;
                        if (ObjRes.GENERIC_SMS.Count > 0)
                        {
                            ObjRes.GENERIC_SMS.ForEach(b => b.ValidDays = Utility.GetDateconvertion(b.ValidDays, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat));
                        }
                    }
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ManagePremiumNumber End");
                    return View("NewManagePremiumNumber", ObjRes);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ManagePremiumNumber End");
                return View(ObjRes);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(ObjRes);
            }
            finally
            {
                ObjReq = null;
                objCateResponse = null;
                objCategories = null;
                ObjResp = null;
                serviceCRM = null;
            }
        }



        public ActionResult NewManagePremiumNumber()
        {
            return View();
        }
        public ActionResult PremiumNoDetailService(string strPremiumNoDetails)
        {
            ManagePremiumNumberResponse ObjResp = new ManagePremiumNumberResponse();
            ManagePremiumNumberRequest ObjReq = JsonConvert.DeserializeObject<ManagePremiumNumberRequest>(strPremiumNoDetails);
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "PremiumNoDetailService Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjResp = serviceCRM.CRMPremiumNumberService(ObjReq);
                
                if (ObjResp.responseDetails != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PremiumNo_Ins_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                    if (ObjResp.premiumcategories.Count > 0)
                    {
                        ObjResp.premiumcategories.ForEach(b => b.ExpirationDate = Utility.GetDateconvertion(b.ExpirationDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat));
                        ObjResp.premiumcategories.ForEach(b => b.CateServiceType = b.CateServiceType == "1" ? Resources.AccountResources.Call : @Resources.AccountResources.SMS);
                        ObjResp.premiumcategories.ForEach(b => b.Duration = b.CateActionFlag == "2" ? Resources.AccountResources.Premiumvaliditydays : "0");
                        ObjResp.premiumcategories.ForEach(b => b.CateStatus = b.CateActionFlag == "1" ? Resources.AccountResources.Disabled : Resources.AccountResources.Limited);
                        ObjResp.premiumcategories.ForEach(b => b.Duration = b.CateActionFlag == "1" ? Resources.AccountResources.NA : b.Duration);
                        ObjResp.premiumcategories.ForEach(b => b.ExpirationDate = b.CateActionFlag == "1" ? Resources.AccountResources.NA : b.ExpirationDate);
                        ObjResp.premiumcategories.ForEach(b => b.CateLimitAmount = b.CateActionFlag == "1" ? Resources.AccountResources.NA : b.CateLimitAmount);
                    }

                    if (ObjResp.SMScategories.Count > 0)
                    {
                        ObjResp.SMScategories.ForEach(b => b.ExpirationDate = Utility.GetDateconvertion(b.ExpirationDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat));

                        ObjResp.SMScategories.ForEach(b => b.CateServiceType = b.CateServiceType == "2" ? @Resources.AccountResources.SMS : Resources.AccountResources.Call);
                        ObjResp.SMScategories.ForEach(b => b.Duration = b.CateActionFlag == "2" ? Resources.AccountResources.Premiumvaliditydays : "0");

                        ObjResp.SMScategories.ForEach(b => b.CateStatus = b.CateActionFlag == "1" ? Resources.AccountResources.Disabled : Resources.AccountResources.Limited);
                        ObjResp.SMScategories.ForEach(b => b.Duration = b.CateActionFlag == "1" ? Resources.AccountResources.NA : b.Duration);
                        ObjResp.SMScategories.ForEach(b => b.ExpirationDate = b.CateActionFlag == "1" ? Resources.AccountResources.NA : b.ExpirationDate);
                        ObjResp.SMScategories.ForEach(b => b.CateLimitAmount = b.CateActionFlag == "1" ? Resources.AccountResources.NA : b.CateLimitAmount);
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "PremiumNoDetailService End");
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
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }




        public ActionResult NewPremiumNoDetailService(string strPremiumNoDetails)
        {
            ManagePremiumNumberResponse ObjResp = new ManagePremiumNumberResponse();
            ManagePremiumNumberRequest ObjReq = JsonConvert.DeserializeObject<ManagePremiumNumberRequest>(strPremiumNoDetails);
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "NewPremiumNoDetailService Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjResp = serviceCRM.NewPremiumNoDetailService(ObjReq);
                

                if (ObjResp.responseDetails != null)
                {
                    string[] ErrorCode = { "21", "22", "23", "24" };
                    string ResponseMsg = string.Empty;
                    if (ObjResp.FailureCategories.Count > 0)
                    {
                        foreach (var errcode in ObjResp.FailureCategories)
                        {
                            if (errcode != null && errcode.Errvalue != "")
                            {
                                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PremiumNo_Ins_" + errcode.Errvalue);
                                ResponseMsg += "," + (string.IsNullOrEmpty(errorInsertMsg) ? errcode.errMsg : errorInsertMsg);
                            }
                            else
                            {
                                ResponseMsg += errcode.errMsg;
                            }
                            if (ErrorCode.Contains(errcode.Errvalue))
                            {
                                ObjResp.responseDetails.ResponseCode = "0";
                            }
                        }
                        ObjResp.responseDetails.ResponseDesc = ResponseMsg.TrimStart(',');
                    }
                    if (ObjResp.GENERIC_SMS.Count > 0)
                    {
                        ObjResp.GENERIC_SMS.ForEach(b => b.ValidDays = Utility.GetDateconvertion(b.ValidDays, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat));
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "NewPremiumNoDetailService End");
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
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        #endregion


        [HttpPost]
        public ActionResult ExtendFLHValidity(FLHValidityReq objFLHReq)
        {
            FLHValidityRes ObjRes = new FLHValidityRes();
            FLHValidityReq objbundleReq = new FLHValidityReq();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ExtendFLHValidity Start");
                objFLHReq.CountryCode = clientSetting.countryCode;
                objFLHReq.BrandCode = clientSetting.brandCode;
                objFLHReq.LanguageCode = clientSetting.langCode;
                objFLHReq.MSISDN = Convert.ToString(Session["MobileNumber"]);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMFLHValidity(objFLHReq);
                    if (!string.IsNullOrEmpty(ObjRes.FLHUsageExpDate))
                    {
                        try
                        {
                            DateTime dt = DateTime.Parse(ObjRes.FLHUsageExpDate);
                            string strDateTimeFormat = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                            strDateTimeFormat = strDateTimeFormat.Replace("DD", "dd");
                            strDateTimeFormat = strDateTimeFormat.Replace("YYYY", "yyyy");
                            string str = dt.ToString(strDateTimeFormat);
                            ObjRes.FLHUsageExpDate = str;
                        }
                        catch (Exception exExtendFLHValidity)
                        {
                            CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exExtendFLHValidity);
                        }
                    }
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ExtendFlhValidity_" + ObjRes.responseDetails.ResponseCode);
                    ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ExtendFLHValidity End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objbundleReq = null;
                serviceCRM = null;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TicketSMS(string Mode)
        {
            SMSOptInOutResponse ObjRes = new SMSOptInOutResponse();
            SMSOptInOutRequst objReq = new SMSOptInOutRequst();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TicketSMS Start");
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.Msisdn = Session["MobileNumber"].ToString();
                objReq.Mode = Mode;
                objReq.Channel = "CRM";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                {
                    ObjRes = serviceCRM.TicketSMS(objReq);
                    if (ObjRes != null && ObjRes.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("TickettingSMS_" + ObjRes.ResponseCode);
                        ObjRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDesc : errorInsertMsg;
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TicketSMS End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(ObjRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objReq = null;
                serviceCRM = null;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }

        #region Restrict Edit

        public ActionResult UpdateRestrictEdit(string status, string Mode)
        {
            EditRestrictRes ObjRes = new EditRestrictRes();
            EditRestrict objReq = new EditRestrict();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "UpdateRestrictEdit Start");
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.MSISDN = Session["MobileNumber"].ToString();
                objReq.status = status;
                objReq.Mode = Mode;
                objReq.UploadedBy = Convert.ToString(Session["UserName"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMEditRestrict(objReq);
                    if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UpdateRestrictEdit_" + ObjRes.Response.ResponseCode);
                        ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;
                    }
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "UpdateRestrictEdit End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(ObjRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objReq = null;
                serviceCRM = null;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Update PortOut Forbidden

        public ActionResult UpdatePortOutForbidden(string status, string Mode, string Reason)
        {
            PortOutForbiddentRes ObjRes = new PortOutForbiddentRes();
            PortOutForbiddent objReq = new PortOutForbiddent();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "UpdatePortOutForbidden Start");
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.MSISDN = Session["MobileNumber"].ToString();
                objReq.status = status;
                objReq.Mode = Mode;
                objReq.ModifiedBy = Convert.ToString(Session["UserName"]);
                objReq.CreatedBy = Convert.ToString(Session["UserName"]);
                objReq.Reason = Reason;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMPortOutForbidden(objReq);

                    if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UpdateRestrictEdit_" + ObjRes.Response.ResponseCode);
                        ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;

                    }
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "UpdatePortOutForbidden End");
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
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Update announcementcall
        public ActionResult Updateannouncementcall(string status, string Mode)
        {
            announcementcallRes ObjRes = new announcementcallRes();
            announcementcallReq objReq = new announcementcallReq();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Updateannouncementcall Start");
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.MSISDN = Session["MobileNumber"].ToString();
                objReq.status = status;
                objReq.Mode = Mode;
                objReq.ModifiedBy = Convert.ToString(Session["UserName"]);
                objReq.CreatedBy = Convert.ToString(Session["UserName"]);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMannouncementcall(objReq);

                    if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UpdateRestrictEdit_" + ObjRes.Response.ResponseCode);
                        ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;
                    }
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Updateannouncementcall End");
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
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region FRR-4708
        public JsonResult HomeDataBillShock(string billShockRequest)
        {

            BillShockRequest ObjReq = JsonConvert.DeserializeObject<BillShockRequest>(billShockRequest);
            BillShockResponse ObjResp = new BillShockResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ServiceController - GetPortoutRefundReport Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.CRMBillShock(ObjReq);

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


        #region FRR 4775
        public ActionResult GetSuperRoaming()
        {
            SuperRoamingResp ObjRes = new SuperRoamingResp();
            SuperRoamingRequest objReq = new SuperRoamingRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SuperRoaming Start");
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.MSISDN = Session["MobileNumber"].ToString();
                objReq.Mode = "GET";
               
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.SuperRoaming(objReq);
                    if (ObjRes != null && ObjRes.Responsedetils != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SuperRoaming_" + ObjRes.Responsedetils.ResponseCode);
                        ObjRes.Responsedetils.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Responsedetils.ResponseDesc : errorInsertMsg;
                    }
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SuperRoaming End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(ObjRes);
            }
            finally
            {
                objReq = null;
                serviceCRM = null;
            }
            return View(ObjRes);
        }

        public JsonResult UpdateSuperRoaming(SuperRoamingRequest SuperRoamingReq)
        {
            SuperRoamingResp ObjRes = new SuperRoamingResp();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SuperRoaming Start");
                SuperRoamingReq.CountryCode = clientSetting.countryCode;
                SuperRoamingReq.BrandCode = clientSetting.brandCode;
                SuperRoamingReq.LanguageCode = clientSetting.langCode;
                SuperRoamingReq.MSISDN = Session["MobileNumber"].ToString();
          
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    ObjRes = serviceCRM.SuperRoaming(SuperRoamingReq);
                    if (ObjRes != null && ObjRes.Responsedetils != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SuperRoaming_" + ObjRes.Responsedetils.ResponseCode);
                        ObjRes.Responsedetils.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Responsedetils.ResponseDesc : errorInsertMsg;
                    }
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SuperRoaming End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(ObjRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                SuperRoamingReq = null;
                serviceCRM = null;
            }

            return Json(ObjRes, JsonRequestBehavior.AllowGet);

        }

        #endregion

    }
}
