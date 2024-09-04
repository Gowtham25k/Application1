using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using System.Xml;
using CRM.Models;
using Microsoft.Office.Interop.Word;
using Newtonsoft.Json;
using Resources;
using ServiceCRM;
using System.Data.OleDb;
using System.Web;
using System.Text.RegularExpressions;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;


namespace CRM.Controllers
{
    [ValidateState]
    public class SimController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();

        ServiceCRM.ServiceInvokeCRM crmNewService = new ServiceCRM.ServiceInvokeCRM(Convert.ToString(SettingsCRM.crmServiceUrl));

        #region Sim Block

        public ActionResult SimBlock(string Textdata)
        {
            SimBlockModelRequest simBlockReq = new SimBlockModelRequest();
            PendingDetailsRequest req = new PendingDetailsRequest();
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
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
                    simBlockReq.MSISDN = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    PAType = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.PAType).First().ToString();
                    req.MSISDN = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    req.Type = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.PAType).First().ToString();
                    req.Id = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.PAID).First().ToString();
                    MSISDNvalidate = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();

                }
                else
                {
                    simBlockReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    PAType = Session["PAType"] != null ? Session["PAType"].ToString() : string.Empty;
                    req.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    req.Type = Convert.ToString(Session["PAType"]);
                    req.Id = Convert.ToString(Session["PAId"]);
                    MSISDNvalidate = Convert.ToString(Session["MobileNumber"]);
                }
                #endregion

                ViewBag.selectval = "0";
                simBlockReq.lstActions = Utility.GetDropdownMasterFromDB("9", Convert.ToString(Session["isprepaid"]), "drop_master");
                ViewBag.Approve = "Approve";
                ViewBag.Reject = "Reject";
                ViewBag.Reset = "Reset";
                ViewBag.Submit = "Submit";
                if (PAType != null && (Convert.ToString(PAType) == "SIM BLOCK" || Convert.ToString(PAType) == "SIM UNBLOCK"))
                {
                    // PendingDetails request
                    req.CountryCode = clientSetting.countryCode;
                    req.BrandCode = clientSetting.brandCode;
                    req.LanguageCode = clientSetting.langCode;
                   
                    simBlockReq.PaType = true;
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
                        simBlockReq.Id = ObjRes.PendingDetails[0].Id;
                        simBlockReq.MSISDN = ObjRes.PendingDetails[0].MSISDN;
                        simBlockReq.TicketId = ObjRes.PendingDetails[0].TicketId;
                        simBlockReq.SubmittedBy = ObjRes.PendingDetails[0].RequestBy;
                        simBlockReq.Reason = ObjRes.PendingDetails[0].Reason;
                        ViewBag.Disabled = "true";
                        ViewBag.Readonly = "true";
                        ViewBag.selectval = req.Type.ToUpper() == "SIM BLOCK" ? "1" : "2";
                    }
                }
                else
                {
                    if (clientSetting.preSettings.EnableSimBlockHLR == "1")
                    {
                        SimBlockResponse objResp = new SimBlockResponse();
                        SimBlockRequest objReq = new SimBlockRequest();
                        objReq.CountryCode = clientSetting.countryCode;
                        objReq.BrandCode = clientSetting.brandCode;
                        objReq.LanguageCode = clientSetting.langCode;
                        objReq.MSISDN = MSISDNvalidate;
                        objReq.Mode = "5";
                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        
                            objResp = serviceCRM.CRMSimBlock(objReq);
                            if (objResp != null && objResp.ResponseDetails != null && objResp.ResponseDetails.ResponseCode == "0")
                                simBlockReq.BlockCheckHLR = objResp.IsBlocked;
                            else
                                simBlockReq.BlockCheckHLR = objResp.IsBlocked;
                        
                    }

                    simBlockReq.PaType = false;
                    if (MSISDNvalidate != null)
                    {
                        simBlockReq.MSISDN = MSISDNvalidate;
                        ViewBag.Disabled = "false";
                        ViewBag.Readonly = "false";
                    }
                }
                
                Session["PAType"] = null;
                return View(simBlockReq);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(simBlockReq);
            }
            finally
            {
                //simBlockReq = null;
                req = null;
                ObjRes = null;
                serviceCRM = null;
            }

        }

        [HttpPost]
        public JsonResult SimBlocks(SimBlockRequest Prty)
        {
            SelectListItem selList = new SelectListItem();
            CRMSimBlock blockmodel = new CRMSimBlock();
            SimBlockResponse simBlockResp = new SimBlockResponse();
            CRMResponse objResponse = new CRMResponse();
            SimBlockRequest simBlockReq = new SimBlockRequest();
            string isMode = string.Empty;
            ServiceInvokeCRM serviceCRM;
            try
            {
                simBlockReq.CountryCode = clientSetting.countryCode;
                simBlockReq.BrandCode = clientSetting.brandCode;
                simBlockReq.LanguageCode = clientSetting.langCode;
                simBlockReq.TicketId = Prty.TicketId;
                simBlockReq.SubmittedBy = Convert.ToString(Session["UserName"]);
                simBlockReq.Reason = Prty.Reason;
                simBlockReq.AdminComments = Prty.AdminComments;
                simBlockReq.Id = Prty.Id;
                simBlockReq.SimBlocKUnBlock = Prty.SimBlocKUnBlock;

                #region FRR 4925
                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                  
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    simBlockReq.MSISDN = localDict.Where(x => Prty.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    simBlockReq.IMSI = localDict.Where(x => Prty.textdata.ToString().Contains(x.Key)).Select(x => x.Value.IMSI).First().ToString();

                }
                else
                {
                    simBlockReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    simBlockReq.IMSI = Convert.ToString(Session["IMSI"]);
                }
                #endregion

                simBlockReq.PARequestType = simBlockReq.SimBlocKUnBlock == "1" ? Resources.HomeResources.PASIMBlock : Resources.HomeResources.PASIMUnBlock;
               


                List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
                menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SIM_SimBlock").ToList();


                if (menu[0] != null && string.IsNullOrEmpty(simBlockReq.Id) && menu[0].DirectApproval.ToUpper() == "TRUE")
                { //Admin process
                    //Approve
                    isMode = "2";
                    simBlockReq.Mode = isMode;

                }
                else if (!string.IsNullOrEmpty(simBlockReq.Id) && menu[0].Approval1.ToUpper() == "TRUE")
                {
                    //Pending Approval case
                    if (Prty.Mode == "3" || Prty.Mode == "4")
                    {
                        isMode = Prty.Mode;
                        simBlockReq.Mode = isMode;
                    }
                }
                else
                {
                 
                    isMode = "1";
                    simBlockReq.Mode = isMode;

                }

                

              serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    simBlockResp = serviceCRM.CRMSimBlock(simBlockReq);

                    ///FRR--3083
                    if (simBlockResp != null && simBlockResp.ResponseDetails != null && simBlockResp.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SimBlock_" + simBlockResp.ResponseDetails.ResponseCode);
                        simBlockResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? simBlockResp.ResponseDetails.ResponseDesc : errorInsertMsg;

                        if (simBlockResp.ResponseDetails.ResponseCode == "0" && (menu[0] != null && ((menu[0].Approval1.ToUpper() == "TRUE" && !string.IsNullOrEmpty(Convert.ToString(Session["PAId"]))) || menu[0].DirectApproval.ToUpper() == "TRUE")) && simBlockReq.SimBlocKUnBlock == "1" && simBlockReq.Mode != "4")
                        {
                            simBlockResp.ResponseDetails.ResponseDesc = Resources.ErrorResources.SimBlock_AdminBlock;
                        }
                        else if (simBlockResp.ResponseDetails.ResponseCode == "0" && (menu[0] != null && ((menu[0].Approval1.ToUpper() == "TRUE" && !string.IsNullOrEmpty(Convert.ToString(Session["PAId"]))) || menu[0].DirectApproval.ToUpper() == "TRUE")) && simBlockReq.SimBlocKUnBlock == "2" && simBlockReq.Mode != "3" && simBlockReq.Mode != "4")
                        {
                            simBlockResp.ResponseDetails.ResponseDesc = Resources.ErrorResources.SimBlock_AdminUnblock;
                        }

                        else if (simBlockResp.ResponseDetails.ResponseCode == "0" && (menu[0] != null && (menu[0].Approval1.ToUpper() == "TRUE" || menu[0].DirectApproval.ToUpper() == "TRUE")) && simBlockReq.SimBlocKUnBlock == "1" && simBlockReq.Mode == "4")
                        {
                            simBlockResp.ResponseDetails.ResponseDesc = Resources.ErrorResources.SimBlock_AdminBlockReject;
                        }
                        else if (simBlockResp.ResponseDetails.ResponseCode == "0" && (menu[0] != null && (menu[0].Approval1.ToUpper() == "TRUE" || menu[0].DirectApproval.ToUpper() == "TRUE")) && simBlockReq.SimBlocKUnBlock == "2" && simBlockReq.Mode == "4")
                        {
                            simBlockResp.ResponseDetails.ResponseDesc = Resources.ErrorResources.SimBlock_AdminUnblockReject;
                        }
                        else if (simBlockResp.ResponseDetails.ResponseCode == "0" && (menu[0] != null && ((menu[0].Approval1.ToUpper() == "TRUE" && !string.IsNullOrEmpty(Convert.ToString(Session["PAId"]))) || menu[0].DirectApproval.ToUpper() == "TRUE")) && simBlockReq.SimBlocKUnBlock == "1")
                        {
                            simBlockResp.ResponseDetails.ResponseDesc = Resources.ErrorResources.SimBlock_AdminBlock;
                        }
                        else if (simBlockResp.ResponseDetails.ResponseCode == "0" && (menu[0] != null && ((menu[0].Approval1.ToUpper() == "TRUE" && !string.IsNullOrEmpty(Convert.ToString(Session["PAId"]))) || menu[0].DirectApproval.ToUpper() == "TRUE")) && simBlockReq.SimBlocKUnBlock == "2")
                        {
                            simBlockResp.ResponseDetails.ResponseDesc = Resources.ErrorResources.SimBlock_AdminUnblock;
                        }

                        else if (simBlockResp.ResponseDetails.ResponseCode == "0" && simBlockReq.SimBlocKUnBlock == "1")
                        {
                            simBlockResp.ResponseDetails.ResponseDesc = Resources.ErrorResources.SimBlock_AgentBlock;
                        }
                        else if (simBlockResp.ResponseDetails.ResponseCode == "0" && simBlockReq.SimBlocKUnBlock == "2")
                        {
                            simBlockResp.ResponseDetails.ResponseDesc = Resources.ErrorResources.SimBlock_AgentUnblock;
                        }


                    }
                    if (Prty.SimBlocKUnBlock == "1" && simBlockResp.ResponseDetails.ResponseCode == "0" && ((menu[0] != null && (menu[0].Approval1.ToUpper() == "TRUE" && !string.IsNullOrEmpty(Convert.ToString(Session["PAId"]))) || menu[0].DirectApproval.ToUpper() == "TRUE")) && simBlockReq.Mode != "4")
                    {
                        if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                        {
                            Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                            localDict.Where(w => w.Key == Prty.textdata).ToList().ForEach(i => i.Value.SIMSTATUS = "1");
                        }
                        else
                        {
                        Session["SIMSTATUS"] = "1";
                    }
                    }
                    else if (Prty.SimBlocKUnBlock == "2" && simBlockResp.ResponseDetails.ResponseCode == "0" && ((menu[0] != null && (menu[0].Approval1.ToUpper() == "TRUE" && string.IsNullOrEmpty(Convert.ToString(Session["PAId"]))) || menu[0].DirectApproval.ToUpper() == "TRUE")) && simBlockReq.Mode != "4")
                    {
                        if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                        {
                            Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                            localDict.Where(w => w.Key == Prty.textdata).ToList().ForEach(i => i.Value.SIMSTATUS = "0");
                        }
                        else
                        {
                        Session["SIMSTATUS"] = "0";
                    }
                    }


                


                Session["PAId"] = null;
                if (simBlockResp.ResponseDetails.ResponseCode == null)
                {
                    selList.Value = simBlockResp.ResponseDetails.ResponseCode;
                    selList.Text = (simBlockResp.ResponseDetails.ResponseDesc).Trim();
                }
                else
                {
                    selList.Value = simBlockResp.ResponseDetails.ResponseCode;
                    selList.Text = (simBlockResp.ResponseDetails.ResponseDesc).Trim();
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
                // selList = null;
                blockmodel = null;
                simBlockResp = null;
                objResponse = null;
                simBlockReq = null;
                serviceCRM = null;
            }


        }

        [HttpPost]
        public JsonResult PostpaidSimBlock(PostpaidSimBlockUnBlockRequest Prty)
        {
            SelectListItem selList = new SelectListItem();
            //CRMSimBlock blockmodel = new CRMSimBlock();

            CRMResponse objResponse = new CRMResponse();
            PostpaidSimBlockUnBlockRequest simBlockReq = new PostpaidSimBlockUnBlockRequest();
            string isMode = string.Empty;
            ServiceInvokeCRM serviceCRM;
            try
            {

                simBlockReq.CountryCode = clientSetting.countryCode;
                simBlockReq.BrandCode = clientSetting.brandCode;
                simBlockReq.LanguageCode = clientSetting.langCode;
                simBlockReq.ticketId = Prty.ticketId;
                simBlockReq.userName = Convert.ToString(Session["UserName"]);
                simBlockReq.reason = Prty.reason;
                simBlockReq.blockUnBlock = Prty.blockUnBlock;

                if(clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    simBlockReq.MSISDN = localDict.Where(x => Prty.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                }
                else
                {
                    simBlockReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                }


            serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objResponse = serviceCRM.CRMPostpaidSimBlockUnBlock(simBlockReq);

                    ///FRR--3083
                    if (objResponse != null && objResponse.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SimBlock_" + objResponse.ResponseCode);
                        objResponse.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objResponse.ResponseDesc : errorInsertMsg;


                        if (objResponse.ResponseCode == "0" && simBlockReq.blockUnBlock == "1")
                        {
                            objResponse.ResponseDesc = Resources.ErrorResources.SimBlock_AdminBlock;
                        }
                        else if (objResponse.ResponseCode == "0" && simBlockReq.blockUnBlock == "2")
                        {
                            objResponse.ResponseDesc = Resources.ErrorResources.SimBlock_AdminUnblock;
                        }
                    }
                  
                


               
                if (simBlockReq.blockUnBlock == "1" && objResponse.ResponseCode == "0")
                {
                    if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                    {
                        Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                        localDict.Where(w => w.Key == Prty.textdata).ToList().ForEach(i => i.Value.SIMSTATUS = "1");
                    }
                    else
                    {
                    Session["SIMSTATUS"] = "1";
                }
                }
                else if (simBlockReq.blockUnBlock == "2" && objResponse.ResponseCode == "0")
                {
                    if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                    {
                        Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                        localDict.Where(w => w.Key == Prty.textdata).ToList().ForEach(i => i.Value.SIMSTATUS = "0");
                    }
                    else
                    {
                    Session["SIMSTATUS"] = "0";
                }
                }
                if (objResponse.ResponseCode == null)
                {
                    selList.Value = objResponse.ResponseCode;
                    selList.Text = (objResponse.ResponseDesc).Trim();
                }
                else
                {
                    selList.Value = objResponse.ResponseCode;
                    selList.Text = (objResponse.ResponseDesc).Trim();
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
                //selList = null;
                objResponse = null;
                simBlockReq = null;
                serviceCRM = null;
            }


        }
        #endregion

        #region Msisdn Swap

        public ActionResult MsisdnSwap()
        {
            CRMMsisdnSwapReqModel cRMMsisdnSwapReqModel = new CRMMsisdnSwapReqModel();
            CRMBase crmBase = new CRMBase();
            PostSwapMSISDNReasonResponse reasonResp = new PostSwapMSISDNReasonResponse();
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
            PendingDetailsRequest req = new PendingDetailsRequest();
            ServiceInvokeCRM serviceCRM;
            CRMResponse objRes = null;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - ActionResult MsisdnSwap Start");
                #region Postpaid ReasonField
                crmBase.CountryCode = clientSetting.countryCode;
                crmBase.BrandCode = clientSetting.brandCode;
                crmBase.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                reasonResp = serviceCRM.CRMPostSwapMSISDNReason(crmBase);
                if (Convert.ToString(Session["isPrePaid"]) == "0")//For Postpaid 
                {
                    cRMMsisdnSwapReqModel.lstPostReason = reasonResp.SwapMSISDNReason.ToDictionary(x => x.ReasonName, x => x.ReasonID);
                }
                #endregion

                if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
                {
                if (clientSetting.mvnoSettings.isRegistrationMandatoryforSIM.ToUpper() == "TRUE")
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
                    objRes = new CRMResponse();
                    objRes.ResponseCode = "0";
                }


                if (objRes.ResponseCode == "0")
                {
                    cRMMsisdnSwapReqModel.ResponseCode = "0";
                }
                else
                {
                    cRMMsisdnSwapReqModel.ResponseCode = "1";
                    cRMMsisdnSwapReqModel.ResponseDescription = objRes.ResponseDesc;
                }
                //IsAdmin condition - for view button
                if (Convert.ToString(Session["IsAdmin"]).ToLower() == "true")
                {
                    ViewBag.Approve = "Approval";
                }
                else
                {
                    ViewBag.Approve = "Submit";
                }
                if (Session["PAType"] != null && Convert.ToString(Session["PAType"]) == "SWAP MSISDN")
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
                    ///FRR--3083
                    if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManaageOBABundle_" + ObjRes.ResponseDetails.ResponseCode);
                        ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                    if (ObjRes.PendingDetails.Count > 0)
                    {
                        cRMMsisdnSwapReqModel.OldMSISDN = ObjRes.PendingDetails[0].OldMSISDN;
                        cRMMsisdnSwapReqModel.NewMSISDN = ObjRes.PendingDetails[0].NewMSISDN;
                        cRMMsisdnSwapReqModel.NewICCID = ObjRes.PendingDetails[0].NewICCID;
                        cRMMsisdnSwapReqModel.OldICCID = ObjRes.PendingDetails[0].OldICCID;
                        cRMMsisdnSwapReqModel.old_IMSI = ObjRes.PendingDetails[0].oldIMSI;
                        cRMMsisdnSwapReqModel.Ticket_Id = ObjRes.PendingDetails[0].TicketId;
                        cRMMsisdnSwapReqModel.FrequentCalNumber = ObjRes.PendingDetails[0].FrequentcalledNumber;
                        cRMMsisdnSwapReqModel.CurrentStatus = ObjRes.bundleName;
                        cRMMsisdnSwapReqModel.AdminComment = ObjRes.PendingDetails[0].NewPlanname;
                        cRMMsisdnSwapReqModel.AuthenticationMode = ObjRes.PendingDetails[0].AuthenticationMode;
                        cRMMsisdnSwapReqModel.AuthenticationComments = ObjRes.PendingDetails[0].AuthenticationComments;
                        cRMMsisdnSwapReqModel.Disabled = "true";
                        cRMMsisdnSwapReqModel.PAType = true;
                        ViewBag.Disabled = "true";
                        ViewBag.Readonly = "true";
                        //FOR ALTAN BUG
                        cRMMsisdnSwapReqModel.AreaCode = ObjRes.PendingDetails[0].CIP;
                    }
                }
                else
                {
                    if (Session["MobileNumber"] != null)
                    {
                        cRMMsisdnSwapReqModel.OldMSISDN = Convert.ToString(Session["MobileNumber"]);
                        cRMMsisdnSwapReqModel.OldICCID = Convert.ToString(Session["ICCID"]);
                        ViewBag.OldICCID = Convert.ToString(Session["ICCID"]);
                        ViewBag.OldMSISDN = Convert.ToString(Session["MobileNumber"]);
                        cRMMsisdnSwapReqModel.Disabled = "false";
                        cRMMsisdnSwapReqModel.PAType = false;
                        ViewBag.Disabled = "false";
                        ViewBag.Readonly = "false";
                    }
                    else
                    {
                        cRMMsisdnSwapReqModel.OldICCID = string.Empty;
                        cRMMsisdnSwapReqModel.OldMSISDN = string.Empty;
                        cRMMsisdnSwapReqModel.PAType = false;
                        ViewBag.OldICCID = string.Empty;
                        ViewBag.OldMSISDN = string.Empty;
                    }
                }
                // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                Session["PAType"] = null;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - ActionResult MsisdnSwap End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                crmBase = null;
                reasonResp = null;
                ObjRes = null;
                req = null;
                serviceCRM = null;
                objRes = null;
            }
            return View(cRMMsisdnSwapReqModel);
        }

        [HttpPost]
        public ActionResult MsisdnSwap(string jsonMsisdnSwapReqForm)
        {
            SelectListItem selList = new SelectListItem();
            SwapMSISDNResponse cRMMsisdnSwapRes = new SwapMSISDNResponse();
            SwapMSISDNRequest cRMMsisdnSwapReqModel = new SwapMSISDNRequest();
            SwapMSISDNRequest cRMMsisdnSwapReq = new SwapMSISDNRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            MsisdnSwapReqModel msisdnSwapReqModel;
            SwapMSISDNResponse msisdnSwapResp;
            SwapMSISDNRequest msisdnSwapReq;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - MsisdnSwap Start");
                cRMMsisdnSwapReqModel = new JavaScriptSerializer().Deserialize<SwapMSISDNRequest>(jsonMsisdnSwapReqForm);
                cRMMsisdnSwapReq.CountryCode = clientSetting.countryCode;
                cRMMsisdnSwapReq.BrandCode = clientSetting.brandCode;
                cRMMsisdnSwapReq.LanguageCode = clientSetting.langCode;
                cRMMsisdnSwapReq.OldICCID = Convert.ToString(Session["ICCID"]);
                cRMMsisdnSwapReq.OldMSISDN = Convert.ToString(Session["MobileNumber"]);
                cRMMsisdnSwapReq.NewICCID = cRMMsisdnSwapReqModel.NewICCID;
                cRMMsisdnSwapReq.NewMSISDN = cRMMsisdnSwapReqModel.NewMSISDN;
                cRMMsisdnSwapReq.TicketID = cRMMsisdnSwapReqModel.TicketID;
                cRMMsisdnSwapReq.RequestBy = Convert.ToString(Session["UserName"]);
                cRMMsisdnSwapReq.FrequentCalNumber = cRMMsisdnSwapReqModel.FrequentCalNumber;
                cRMMsisdnSwapReq.ReqID = cRMMsisdnSwapReqModel.ReqID;
                cRMMsisdnSwapReq.Status = cRMMsisdnSwapReqModel.Status;
                cRMMsisdnSwapReq.adminComment = cRMMsisdnSwapReqModel.adminComment;
                cRMMsisdnSwapReq.PARequestType = Resources.HomeResources.PASwapMSISDN;
                cRMMsisdnSwapReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                cRMMsisdnSwapReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                cRMMsisdnSwapReq.AuthenticationMode = cRMMsisdnSwapReqModel.AuthenticationMode;
                cRMMsisdnSwapReq.AuthenticationComments = cRMMsisdnSwapReqModel.AuthenticationComments;

                if(clientSetting.preSettings.EnableAltanIntegration.ToUpper() == "TRUE")
                {
                   // cRMMsisdnSwapReqModel.Mode = "FA";
                    cRMMsisdnSwapReq.AreaCode = cRMMsisdnSwapReqModel.AreaCode;
                }


                if (cRMMsisdnSwapReqModel.Mode == "N" || (Convert.ToString(Session["isPrePaid"]) == "0"))//For Postpaid there is no approval
                {
                    #region agent permission
                    if (Convert.ToString(Session["isPrePaid"]) == "1") //prepaid
                    {
                        #region Prepaid
                        cRMMsisdnSwapReq.Mode = cRMMsisdnSwapReqModel.Mode;
                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        cRMMsisdnSwapRes = serviceCRM.CRMSwapMSISDN(cRMMsisdnSwapReq);
                        ///FRR--3083
                        if (cRMMsisdnSwapRes != null && cRMMsisdnSwapRes.ResponseDetails != null && cRMMsisdnSwapRes.ResponseDetails.ResponseCode != null)
                        {
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MsisdnSwap_" + cRMMsisdnSwapRes.ResponseDetails.ResponseCode);
                            cRMMsisdnSwapRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? cRMMsisdnSwapRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                        }
                        ///FRR--3083
                        #endregion
                    }
                    else if (Convert.ToString(Session["isPrePaid"]) == "0")
                    {
                        #region postpaid
                        PostSwapMSISDNRequest PostSwapMSISDNReq = new PostSwapMSISDNRequest();
                        CRMResponse crmResp = new CRMResponse();
                        try
                        {
                            PostSwapMSISDNReq.CountryCode = clientSetting.countryCode;
                            PostSwapMSISDNReq.BrandCode = clientSetting.brandCode;
                            PostSwapMSISDNReq.LanguageCode = clientSetting.langCode;
                            PostSwapMSISDNReq.OldICCID = Convert.ToString(Session["ICCID"]);
                            PostSwapMSISDNReq.OldMSISDN = Convert.ToString(Session["MobileNumber"]);
                            PostSwapMSISDNReq.NewMSISDN = cRMMsisdnSwapReqModel.NewMSISDN;
                            PostSwapMSISDNReq.TicketID = cRMMsisdnSwapReqModel.TicketID;
                            PostSwapMSISDNReq.RequestBy = Convert.ToString(Session["UserName"]);
                            PostSwapMSISDNReq.FrequentCalNumber = cRMMsisdnSwapReqModel.FrequentCalNumber; ;
                            PostSwapMSISDNReq.Reason = cRMMsisdnSwapReqModel.Reason;
                            PostSwapMSISDNReq.ReasonID = string.Empty;
                            PostSwapMSISDNReq.ReqID = string.Empty;
                            serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                            crmResp = serviceCRM.CRMPostSwapMSISDN(PostSwapMSISDNReq);
                            ///FRR--3083
                            if (cRMMsisdnSwapRes != null && cRMMsisdnSwapRes.ResponseDetails != null && cRMMsisdnSwapRes.ResponseDetails.ResponseCode != null)
                            {
                                errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PostMsisdnSwap_" + cRMMsisdnSwapRes.ResponseDetails.ResponseCode);
                                cRMMsisdnSwapRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? cRMMsisdnSwapRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                            }
                            ///FRR--3083
                            if (crmResp != null)
                            {
                                selList.Value = crmResp.ResponseCode;
                                selList.Text = (crmResp.ResponseDesc).Trim();
                            }
                            else
                            {
                                selList.Value = "10";
                                selList.Text = "Error while processing the request :(";
                            }
                        }
                        catch (Exception exPostPaidMsisdnSwap)
                        {
                            CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exPostPaidMsisdnSwap);
                        }
                        finally
                        {
                            PostSwapMSISDNReq = null;
                            crmResp = null;
                        }
                        #endregion
                    }
                    if (Convert.ToString(Session["isPrePaid"]) == "1")
                    {
                        if (cRMMsisdnSwapRes.ResponseDetails == null)
                        {
                            selList.Value = cRMMsisdnSwapRes.SwapMSISDN[0].ErrNo;
                            selList.Text = (cRMMsisdnSwapRes.SwapMSISDN[0].ErrMsg).Trim();
                        }
                        else
                        {
                            selList.Value = cRMMsisdnSwapRes.ResponseDetails.ResponseCode;
                            selList.Text = (cRMMsisdnSwapRes.ResponseDetails.ResponseDesc).Trim();
                        }
                    }
                    #endregion
                }
                else if (cRMMsisdnSwapReqModel.Mode == "R" && Convert.ToString(Session["isPrePaid"]) == "1")
                {
                    #region admin reject
                    cRMMsisdnSwapReq.Mode = cRMMsisdnSwapReqModel.Mode;
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    cRMMsisdnSwapRes = serviceCRM.CRMSwapMSISDN(cRMMsisdnSwapReq);
                    ///FRR--3083
                    if (cRMMsisdnSwapRes != null && cRMMsisdnSwapRes.ResponseDetails != null && cRMMsisdnSwapRes.ResponseDetails.ResponseCode != null)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MsisdnSwap_" + cRMMsisdnSwapRes.ResponseDetails.ResponseCode);
                        cRMMsisdnSwapRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? cRMMsisdnSwapRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                    if (cRMMsisdnSwapRes.ResponseDetails == null)
                    {
                        selList.Value = cRMMsisdnSwapRes.SwapMSISDN[0].ErrNo;
                        selList.Text = (cRMMsisdnSwapRes.SwapMSISDN[0].ErrMsg).Trim();
                    }
                    else
                    {
                        selList.Value = cRMMsisdnSwapRes.ResponseDetails.ResponseCode;
                        selList.Text = (cRMMsisdnSwapRes.ResponseDetails.ResponseDesc).Trim();
                    }
                    #endregion
                }
                else
                {
                    #region admin authorize
                    msisdnSwapReqModel = new MsisdnSwapReqModel();
                    msisdnSwapReqModel = new JavaScriptSerializer().Deserialize<MsisdnSwapReqModel>(jsonMsisdnSwapReqForm);
                    msisdnSwapResp = new SwapMSISDNResponse();
                    msisdnSwapReq = new SwapMSISDNRequest();
                    msisdnSwapReq.CountryCode = clientSetting.countryCode;
                    msisdnSwapReq.BrandCode = clientSetting.brandCode;
                    msisdnSwapReq.LanguageCode = clientSetting.langCode;
                    msisdnSwapReq.OldICCID = Convert.ToString(Session["ICCID"]);
                    // msisdnSwapReq.OldIMSI = msisdnSwapReqModel.OldIMSI;
                    msisdnSwapReq.OldMSISDN = Convert.ToString(Session["MobileNumber"]);
                    msisdnSwapReq.NewMSISDN = msisdnSwapReqModel.NewMsisdn;
                    msisdnSwapReq.TicketID = msisdnSwapReqModel.Ticket_Id;
                    msisdnSwapReq.RequestBy = Convert.ToString(Session["UserName"]);
                    msisdnSwapReq.FrequentCalNumber = cRMMsisdnSwapReqModel.FrequentCalNumber;
                    msisdnSwapReq.ReqID = cRMMsisdnSwapReqModel.ReqID;
                    msisdnSwapReq.adminComment = cRMMsisdnSwapReqModel.adminComment;
                    msisdnSwapReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                    msisdnSwapReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                    msisdnSwapReq.AuthenticationMode = cRMMsisdnSwapReqModel.AuthenticationMode;
                    msisdnSwapReq.AuthenticationComments = cRMMsisdnSwapReqModel.AuthenticationComments;
                    msisdnSwapReq.AreaCode = cRMMsisdnSwapReq.AreaCode;

                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    if (cRMMsisdnSwapReqModel.Mode == "PA")
                    {
                        msisdnSwapReq.Mode = "PA";
                    }
                    else if (cRMMsisdnSwapReqModel.Mode == "FA")
                    {
                        msisdnSwapReq.Mode = "FA";
                    }
                    else
                    {
                        msisdnSwapReq.Mode = "A";
                    }
                    msisdnSwapResp = serviceCRM.CRMSwapMSISDN(msisdnSwapReq);
                    ///FRR--3083
                    if (msisdnSwapResp != null && msisdnSwapResp.ResponseDetails != null && msisdnSwapResp.ResponseDetails.ResponseCode != null)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MsisdnSwap_" + msisdnSwapResp.ResponseDetails.ResponseCode);
                        msisdnSwapResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? msisdnSwapResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                    if (msisdnSwapResp.ResponseDetails != null)
                    {
                        selList.Value = msisdnSwapResp.ResponseDetails.ResponseCode;
                        selList.Text = (msisdnSwapResp.ResponseDetails.ResponseDesc).Trim();
                    }
                    else
                    {
                        selList.Value = "value null";
                        selList.Text = "Invalid Data";
                    }
                    #endregion
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - MsisdnSwap End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                cRMMsisdnSwapRes = null;
                cRMMsisdnSwapReqModel = null;
                cRMMsisdnSwapReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
                msisdnSwapReqModel = null;
                msisdnSwapResp = null;
                msisdnSwapReq = null;
            }
            return Json(selList, JsonRequestBehavior.AllowGet);
        }

        #region USA

        public ActionResult MsisdnSwapUSA()
        {
            ChangeMSISDNRequest cRMMsisdnSwapReqModel = new ChangeMSISDNRequest();
            CRMResponse objRes;
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
            PendingDetailsRequest req = new PendingDetailsRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - ActionResult MsisdnSwapUSA Start");
                objRes = Utility.checkValidSubscriber("1", clientSetting);
                cRMMsisdnSwapReqModel.responseDetails = new CRMResponse();
                if (objRes.ResponseCode == "0")
                {
                    cRMMsisdnSwapReqModel.responseDetails.ResponseCode = "0";
                    if (clientSetting.countryCode == "USA")
                    {
                        if (Session["PAType"] != null && Convert.ToString(Session["PAType"]) == "ZIPCODE SWAP MSISDN")
                        {
                            // PendingDetails request
                            req.CountryCode = clientSetting.countryCode;
                            req.BrandCode = clientSetting.brandCode;
                            req.LanguageCode = clientSetting.langCode;
                            req.MSISDN = Convert.ToString(Session["MobileNumber"]);
                            req.Type = Convert.ToString(Session["PAType"]);
                            req.Id = Convert.ToString(Session["PAId"]);
                            cRMMsisdnSwapReqModel.PAid = req.Id;
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
                                cRMMsisdnSwapReqModel.zipCode = ObjRes.PendingDetails[0].TicketId;
                                cRMMsisdnSwapReqModel.reason = ObjRes.PendingDetails[0].Reason;
                            }
                        }
                        // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                        Session["PAType"] = null;
                    }
                    if (clientSetting.countryCode == "BRA")
                    {
                        cRMMsisdnSwapReqModel.AreaCodeList = new List<AreaCodeDropdown>();
                        AreaCodeDropdown objTypeR = new AreaCodeDropdown();
                        string[] strSplit = clientSetting.mvnoSettings.crmBrazilAreaCode.Split(',');
                        for (int i = 0; i < strSplit.Count(); i++)
                        {
                            objTypeR = new AreaCodeDropdown();
                            objTypeR.ID = Convert.ToString(strSplit[i]);
                            objTypeR.Value = Convert.ToString(strSplit[i]);
                            cRMMsisdnSwapReqModel.AreaCodeList.Add(objTypeR);
                        }
                        cRMMsisdnSwapReqModel.oldMSISDN = Convert.ToString(Session["MobileNumber"]);
                    }
                }
                else
                {
                    cRMMsisdnSwapReqModel.responseDetails.ResponseCode = "1";
                    cRMMsisdnSwapReqModel.responseDetails.ResponseDesc = objRes.ResponseDesc;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - ActionResult MsisdnSwapUSA End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objRes = null;
                ObjRes = null;
                req = null;
                serviceCRM = null;
            }
            return View("MsisdnSwap_USA", cRMMsisdnSwapReqModel);
        }

        public JsonResult SaveMsisdnSwap_USA(ChangeMSISDNRequest objReq)
        {
            ChangeMSISDNResponse objRes = new ChangeMSISDNResponse();
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - SaveMsisdnSwap_USA Start");
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.oldMSISDN = Convert.ToString(Session["MobileNumber"]);
                objReq.userName = Convert.ToString(Session["UserName"]);
                objReq.imsi = Convert.ToString(Session["IMSI"]);
                objReq.PARequestType = Resources.HomeResources.PAZipcodeSwapMSISDN;
                if (clientSetting.countryCode == "BRA")
                {
                    objReq.mode = "Approve";
                    objReq.requestBy = "A";
                }
                objRes = crmNewService.ChangeMsisdnUSA(objReq);
                ///FRR--3083
                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {
                    if (clientSetting.countryCode == "USA")
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SaveMsisdnSwap_USA_" + objRes.responseDetails.ResponseCode);
                    }
                    if (clientSetting.countryCode == "BRA")
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SaveMsisdnSwap_BRA_" + objRes.responseDetails.ResponseCode);
                    }
                    objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - SaveMsisdnSwap_USA End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                errorInsertMsg = string.Empty;
            }
            return Json(objRes);

        }

        public JsonResult Swapmsisdnhistory(ReportChangeMSISDN requestObject)
        {
            ReportChangeMSISDNResponse objRes = new ReportChangeMSISDNResponse();
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - Swapmsisdnhistory Start");
                requestObject.CountryCode = clientSetting.countryCode;
                requestObject.BrandCode = clientSetting.brandCode;
                requestObject.LanguageCode = clientSetting.langCode;
                requestObject.IMSI = Convert.ToString(Session["IMSI"]);
                objRes = crmNewService.SwapMsisdnHistory(requestObject);
                ///FRR--3083
                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Swapmsisdnhistory_" + objRes.responseDetails.ResponseCode);
                    objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
                }
                if (objRes.changeMSISDNSearch != null && objRes.changeMSISDNSearch.Count > 0)
                {
                    try
                    {
                        objRes.changeMSISDNSearch.ForEach(b => b.submittedDate = Utility.GetDateconvertion(b.submittedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                        objRes.changeMSISDNSearch.ForEach(b => b.authorisedDate = Utility.GetDateconvertion(b.authorisedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                    }
                    catch (Exception exDateConversion)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exDateConversion);
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - Swapmsisdnhistory End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                errorInsertMsg = string.Empty;
            }
            return Json(objRes);
        }

        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadBillingHistory(string billingData)
        {
            try
            {
                GridView gridView = new GridView();
                //List<BillingHistoryDetailsList_Export> billHisDetails = new JavaScriptSerializer().Deserialize<List<BillingHistoryDetailsList_Export>>(billingData);
                //
                List<ChangeMSISDNSearch> billHisDetails = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<ChangeMSISDNSearch>>(billingData);
                gridView.DataSource = billHisDetails;
                gridView.DataBind();

                Utility.ExportToExcell(gridView, "Change-MSISDN-" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
        }

        #endregion
        #endregion


        #region USAIMSISwapForBothLMandLMPlus

        //USAIMSISwapForBothLM and LMPlus  

        #region FRR-4888
        //4888
        //public JsonResult ValidateICCID(string validatereq)
        //{
        //    expirydateresp objresp = new expirydateresp();
        //    expirydatereq objreq = JsonConvert.DeserializeObject <expirydatereq>(validatereq);
        //    ServiceInvokeCRM serviceCRM;
        //    try
        //    {
        //        objreq.CountryCode = clientSetting.countryCode;
        //        objreq.BrandCode = clientSetting.brandCode;
        //        objreq.LanguageCode = clientSetting.langCode;
        //        CRMLogger.WriteMessage(Session["UserName"].ToString(), this.ControllerContext, "SIMController - ValidateICCID Start");
        //        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
        //        objresp = serviceCRM.CRMvalidateexpiry(objreq);

        //        CRMLogger.WriteMessage(Session["UserName"].ToString(), this.ControllerContext, "SIMController - ValidateICCIDRequestResponse End");

        //    }
        //    catch (Exception ex)
        //    {
        //        CRMLogger.WriteException(Session["UserName"].ToString(), "SIMController - exception-ValidateICCIDRequestResponse - " + this.ControllerContext, ex);

        //    }
        //    finally
        //    {
        //        objreq = null;
        //    }
        //    return Json(objresp, JsonRequestBehavior.AllowGet);

        //}


        public JsonResult ValidateCPIN(string validatecpinreq)
        {
            CPINValidationresp objresp = new CPINValidationresp();
            CPINValidationreq objreq = JsonConvert.DeserializeObject<CPINValidationreq>(validatecpinreq);
            ServiceInvokeCRM serviceCRM;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - ValidateCPIN Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objresp = serviceCRM.CRMCPINValidation(objreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - ValidateCPINRequestResponse End");

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SimController - exception-ValidateCPINRequestResponse - " + this.ControllerContext, ex);

            }
            finally
            {
                objreq = null;
            }
            return Json(objresp, JsonRequestBehavior.AllowGet);

        }

        #endregion
        public ActionResult ImsiSwapLMLMPlus()
        {
            USASwapIMSI objReq = new USASwapIMSI();
            CRMResponse objRes = new CRMResponse();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - ImsiSwapLMLMPlus Start");

                if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
                {

                if (clientSetting.mvnoSettings.isRegisteredCustomerSwapImsi == "1")
                {
                    objRes = Utility.checkValidSubscriberSwapIMSI("", clientSetting, "Allow IMSI Swap");
                }
                else
                {
                    objRes = Utility.checkValidSubscriberSwapIMSI("1", clientSetting, "Allow IMSI Swap");
                }
                //CRMResponse objRes = Utility.checkValidSubscriberSwapIMSI();
                objReq.responseDetails = new CRMResponse();
                if (objRes.ResponseCode == "0")
                {
                    objReq.responseDetails.ResponseCode = "0";
                    objReq.oldSimNumber = Convert.ToString(Session["ICCID"]);
                    objReq.zipCode = clientSetting.preSettings.defaultZipCode;
                }
                else
                {
                    objReq.responseDetails.ResponseCode = "1";
                    objReq.responseDetails.ResponseDesc = objRes.ResponseDesc;
                }
                }
                else
                {
                    objReq.responseDetails = new CRMResponse();
                    objReq.responseDetails.ResponseCode = "0";
                    objReq.oldSimNumber = Convert.ToString(Session["ICCID"]);
                    objReq.zipCode = clientSetting.preSettings.defaultZipCode;
                }



                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - ImsiSwapLMLMPlus End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objRes = null;
            }
            return View("ImsiSwap_USA", objReq);
        }

        #endregion

        #region IMSI Swap

        //ImsiSwap

        public ActionResult ImsiSwap() //#region Except USA prepaid
        {
            ImsiSwapReqModel simSwapReq = new ImsiSwapReqModel();
            PendingDetailsRequest req = new PendingDetailsRequest();
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
            CRMResponse objRes = null;
            ServiceInvokeCRM serviceCRM;
            //5022
            List<Documentfields> documentfields = new List<Documentfields>();

            Documentfields documentfield = null;
            try //#region  PendingDetails
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - Action Result - ImsiSwap Start");
                simSwapReq.lstSimType = Utility.GetDropdownMasterFromDB("11", Convert.ToString(Session["isprepaid"]), "drop_master");
                if (clientSetting.mvnoSettings.isRegistrationMandatoryforSIM.ToUpper() == "TRUE")
                {
                    objRes = Utility.checkValidSubscriberSwapIMSI("", clientSetting);
                }
                else
                {
                    objRes = Utility.checkValidSubscriberSwapIMSI("1", clientSetting);
                }
                simSwapReq.responseDetails = new CRMResponse();
                if (objRes.ResponseCode == "0")
                {
                    simSwapReq.responseDetails.ResponseCode = "0";
                    #region Valid Subscriber for SWAP IMSI
                    if (Session["PAType"] != null && Convert.ToString(Session["PAType"]) == "SWAP IMSI")
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
                        if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null) //FRR--3083
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManaageOBABundle_" + ObjRes.ResponseDetails.ResponseCode);
                            ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                        } //FRR--3083
                        if (ObjRes.PendingDetails.Count > 0)
                        {
                            simSwapReq.OldMSISDN = ObjRes.PendingDetails[0].OldMSISDN;
                            simSwapReq.NewMSISDN = ObjRes.PendingDetails[0].NewMSISDN;
                            simSwapReq.NewICCID = ObjRes.PendingDetails[0].NewICCID;
                            simSwapReq.OldICCID = ObjRes.PendingDetails[0].OldICCID;
                            simSwapReq.Ticket_Id = ObjRes.PendingDetails[0].TicketId;
                            simSwapReq.FrequentCalNumber = ObjRes.PendingDetails[0].FrequentcalledNumber;
                            simSwapReq.CIP = ObjRes.PendingDetails[0].CIP;
                            simSwapReq.PanNumber = ObjRes.PendingDetails[0].PanNumber;
                            simSwapReq.AuthenticationMode = ObjRes.PendingDetails[0].AuthenticationMode;
                            simSwapReq.AuthenticationComments = ObjRes.PendingDetails[0].AuthenticationComments;
                            simSwapReq.CurrentStatus = ObjRes.bundleName;
                            simSwapReq.PAType = true;
                            simSwapReq.Disabled = "true";
                            ViewBag.Disabled = "true";
                            ViewBag.Readonly = "true";
                            //5022
                            simSwapReq.STATUS = ObjRes.PendingDetails[0].status;
                            simSwapReq.Idproofpath = ObjRes.PendingDetails[0].IDProofpath;
                            simSwapReq.simcopypath = ObjRes.PendingDetails[0].simcardcopyproof;
                            simSwapReq.taxcopypath = ObjRes.PendingDetails[0].taxcopyproof;
                        }
                    }
                    else
                    {
                        // need to delete below condition before giving to testing
                        if (Session["MobileNumber"] != null)
                        {
                            simSwapReq.OldMSISDN = Convert.ToString(Session["MobileNumber"]);
                            simSwapReq.OldICCID = Convert.ToString(Session["ICCID"]);
                            ViewBag.OldICCID = Convert.ToString(Session["ICCID"]);
                            ViewBag.OldMSISDN = Convert.ToString(Session["MobileNumber"]);
                            simSwapReq.PAType = false;
                            simSwapReq.Disabled = "false";
                            ViewBag.Disabled = "false";
                            ViewBag.Readonly = "false";
                        }
                        else
                        {
                            simSwapReq.PAType = false;
                            simSwapReq.OldICCID = "";
                            simSwapReq.OldMSISDN = "";
                            ViewBag.OldICCID = "";
                            ViewBag.OldMSISDN = "";
                        }

                        //5022
                        string[] fields;
                        string[] fieldtype;
                        string[] fieldvalue;
                        fields = clientSetting.preSettings.PortinDocumentFields.Split(',');
                        fieldtype = fields[0].Split('|');
                        fieldvalue = fields[1].Split('|');
                        for (var i = 0; fieldtype.Count() > i; i++)
                        {
                            if (fieldvalue.Count() > i)
                            {
                                documentfield = new Documentfields();
                                documentfield.fields = fieldtype[i];
                                documentfield.value = fieldvalue[i];
                                documentfields.Add(documentfield);
                            }
                        }
                        simSwapReq.documentfields = documentfields;


                    }
                    #endregion
                    // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                    Session["PAType"] = null;
                }
                else
                {
                    simSwapReq.responseDetails.ResponseCode = "1";
                    simSwapReq.responseDetails.ResponseDesc = objRes.ResponseDesc;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - Action Result - ImsiSwap End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                // need to delete below condition before giving to testing
                if (Session["MobileNumber"] != null)
                {
                    simSwapReq.OldMSISDN = Convert.ToString(Session["MobileNumber"]);
                    simSwapReq.OldICCID = Convert.ToString(Session["ICCID"]);
                    ViewBag.OldICCID = Convert.ToString(Session["ICCID"]);
                    ViewBag.OldMSISDN = Convert.ToString(Session["MobileNumber"]);
                    simSwapReq.PAType = false;
                    simSwapReq.Disabled = "false";
                    ViewBag.Disabled = "false";
                    ViewBag.Readonly = "false";
                }
                else
                {
                    simSwapReq.PAType = false;
                    simSwapReq.OldICCID = string.Empty;
                    simSwapReq.OldMSISDN = string.Empty;
                    ViewBag.OldICCID = string.Empty;
                    ViewBag.OldMSISDN = string.Empty;
                }
            }
            finally
            {
                req = null;
                ObjRes = null;
                objRes = null;
                serviceCRM = null;
            }
            return View("ImsiSwap", simSwapReq);
        }

        //imsiSwapResp - json result
        [HttpPost]

        public JsonResult ImsiSwap(string jsonImsiSwapReqForm)
        {
            SelectListItem selList = new SelectListItem();
            Object JsonObj = new Object();
            SIMSwapResponse simSwapResp = new SIMSwapResponse();
            SwapIMSIResponse imsiSwapResp = new SwapIMSIResponse();
            ImsiSwapReqModel imsiSwapReqModel = new ImsiSwapReqModel();
            SwapIMSIRequest imsiSwapReq = new SwapIMSIRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - JsonResult - ImsiSwap Start");
                imsiSwapReqModel = JsonConvert.DeserializeObject<ImsiSwapReqModel>(jsonImsiSwapReqForm);
             //   imsiSwapReqModel = new JavaScriptSerializer().Deserialize<ImsiSwapReqModel>(jsonImsiSwapReqForm);
                imsiSwapReq.CountryCode = clientSetting.countryCode;
                imsiSwapReq.BrandCode = clientSetting.brandCode;
                imsiSwapReq.LanguageCode = clientSetting.langCode;
                imsiSwapReq.OldICCID = Convert.ToString(Session["ICCID"]);
                imsiSwapReq.OldMSISDN = Convert.ToString(Session["MobileNumber"]);
                imsiSwapReq.NewICCID = imsiSwapReqModel.NewICCID;
                imsiSwapReq.NewMSISDN = imsiSwapReqModel.NewMSISDN;
                imsiSwapReq.TicketID = imsiSwapReqModel.Ticket_Id;
                imsiSwapReq.RequestBy = Convert.ToString(Session["UserName"]);
                imsiSwapReq.FrequentCalNumber = imsiSwapReqModel.FrequentCalNumber;
                imsiSwapReq.CIP = imsiSwapReqModel.CIP;
                imsiSwapReq.PanNumber = imsiSwapReqModel.PanNumber;
                imsiSwapReq.ReqID = imsiSwapReqModel.ReqID;
                imsiSwapReq.Status = imsiSwapReqModel.Status;
                imsiSwapReq.adminComment = imsiSwapReqModel.adminComment;
                imsiSwapReq.PARequestType = Resources.HomeResources.PASwapIMSI;
                imsiSwapReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                imsiSwapReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                imsiSwapReq.AuthenticationMode = imsiSwapReqModel.AuthenticationMode;
                imsiSwapReq.AuthenticationComments = imsiSwapReqModel.AuthenticationComments;
                imsiSwapReq.GO_ONLINE = Convert.ToString(Session["SIM_CATEGORY"]);
                //5022
                imsiSwapReq.IDProof = imsiSwapReqModel.IDProof;
                imsiSwapReq.Simcardcopy = imsiSwapReqModel.Simcardcopy;
                imsiSwapReq.Taxcodecopy = imsiSwapReqModel.Taxcodecopy;
                imsiSwapReq.SimStatus = imsiSwapReqModel.SimStatus;
                imsiSwapReq.EmailID=  Convert.ToString(Session["eMailID"]);
                imsiSwapReq.Menuselect = imsiSwapReqModel.Menuselect;
                imsiSwapReq.Documenttype = imsiSwapReqModel.Documenttype;
                imsiSwapReq.OTPverified = imsiSwapReqModel.OTPverified;
                imsiSwapReq.OTPverifieddateandtime = imsiSwapReqModel.OTPverifieddateandtime;
                imsiSwapReq.Firstname = imsiSwapReqModel.Firstname;
                imsiSwapReq.Lastname = imsiSwapReqModel.Lastname;
                //POF-6357
                imsiSwapReq.EffectiveDate = imsiSwapReqModel.EffectiveDate;
                imsiSwapReq.SimBlockRestrictions = imsiSwapReqModel.SimBlockRestrictions;
                if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() == "TRUE")
                {
                    //imsiSwapReq.Mode = "A";
                }

                if (imsiSwapReqModel.Mode == "N" || Convert.ToString(Session["isPrePaid"]) == "0") //for agent 
                {
                    #region SIMSwap Agent
                    if (Convert.ToString(Session["isPrePaid"]) == "1")
                    {
                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        imsiSwapReq.Mode = "N";
                        imsiSwapResp = serviceCRM.CRMSwapIMSI(imsiSwapReq);
                        ///FRR--3083
                        if (imsiSwapResp != null && imsiSwapResp.ResponseDetails != null && imsiSwapResp.ResponseDetails.ResponseCode != null)
                        {
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("IMSISwap_" + imsiSwapResp.ResponseDetails.ResponseCode);
                            imsiSwapResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? imsiSwapResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                        }
                        ///FRR--3083
                    }
                    else if (Convert.ToString(Session["isPrePaid"]) == "0")
                    {
                        #region postpaid
                        PostSwapIMSIRequest postIMSISwapReq = new PostSwapIMSIRequest();
                        PostSwapIMSIResponse crmResp = new PostSwapIMSIResponse();
                        try
                        {
                            postIMSISwapReq.CountryCode = clientSetting.countryCode;
                            postIMSISwapReq.BrandCode = clientSetting.brandCode;
                            postIMSISwapReq.LanguageCode = clientSetting.langCode;
                            postIMSISwapReq.OldICCID = Convert.ToString(Session["ICCID"]);
                            postIMSISwapReq.OldMSISDN = Convert.ToString(Session["MobileNumber"]);
                            postIMSISwapReq.NewICCID = imsiSwapReqModel.NewICCID;
                            postIMSISwapReq.NewMSISDN = imsiSwapReqModel.NewMSISDN;
                            postIMSISwapReq.TicketID = imsiSwapReqModel.Ticket_Id;
                            postIMSISwapReq.RequestBy = Convert.ToString(Session["UserName"]);
                            postIMSISwapReq.FrequentCalNumber = imsiSwapReqModel.FrequentCalNumber;
                            postIMSISwapReq.SimType = imsiSwapReqModel.SimType;
                            postIMSISwapReq.Remarks = imsiSwapReqModel.Remarks;
                            serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                            crmResp = serviceCRM.CRMPostSwapIMSI(postIMSISwapReq);
                            ///FRR--3083
                            if (imsiSwapResp != null && imsiSwapResp.ResponseDetails != null && imsiSwapResp.ResponseDetails.ResponseCode != null)
                            {
                                errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("IMSISwapPost_" + imsiSwapResp.ResponseDetails.ResponseCode);
                                imsiSwapResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? imsiSwapResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                            }
                            ///FRR--3083
                            if (crmResp.responseDetails != null)
                            {
                                selList.Value = crmResp.responseDetails.ResponseCode;
                                selList.Text = (crmResp.responseDetails.ResponseDesc).Trim() + "||" + crmResp.newMSISDN;
                            }
                            else
                            {
                                selList.Value = "10";
                                selList.Text = "Error while processing the request :(";
                            }
                        }
                        catch (Exception exPostPaidImsiSwap)
                        {
                            CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exPostPaidImsiSwap);
                        }
                        finally
                        {
                            postIMSISwapReq = null;
                            crmResp = null;
                        }




                        #endregion
                    }
                    #endregion
                }
                else if ((imsiSwapReqModel.Mode == "R" && Convert.ToString(Session["isPrePaid"]) == "1") || imsiSwapReqModel.Mode == "R2") //for admin reject
                {
                    #region SIMSwap Reject
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    imsiSwapReq.Mode = imsiSwapReqModel.Mode;
                    imsiSwapResp = serviceCRM.CRMSwapIMSI(imsiSwapReq);
                    ///FRR--3083
                    if (imsiSwapResp != null && imsiSwapResp.ResponseDetails != null && imsiSwapResp.ResponseDetails.ResponseCode != null)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("IMSISwap_" + imsiSwapResp.ResponseDetails.ResponseCode);
                        imsiSwapResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? imsiSwapResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                    #endregion
                }
                else if (imsiSwapReqModel.Mode == "A" && Convert.ToString(Session["isPrePaid"]) == "1") //for admin authorize
                {
                    #region SIMSwap Admin
                    imsiSwapReq.Mode = "A";
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    imsiSwapResp = serviceCRM.CRMSwapIMSI(imsiSwapReq);
                    ///FRR--3083
                    if (imsiSwapResp != null && imsiSwapResp.ResponseDetails != null && imsiSwapResp.ResponseDetails.ResponseCode != null)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("IMSISwap_" + imsiSwapResp.ResponseDetails.ResponseCode);
                        imsiSwapResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? imsiSwapResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                    #endregion
                }
                else if ((imsiSwapReqModel.Mode == "FA" || imsiSwapReqModel.Mode == "PA") && Convert.ToString(Session["isPrePaid"]) == "1") //for pending approval
                {
                    #region SIMSwap PendingApproval
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    if (imsiSwapReqModel.Mode == "FA")
                    {
                        imsiSwapReq.Mode = "FA";
                    }
                    else
                    {
                        imsiSwapReq.Mode = "PA";
                    }
                    imsiSwapResp = serviceCRM.CRMSwapIMSI(imsiSwapReq);
                    ///FRR--3083
                    if (imsiSwapResp != null && imsiSwapResp.ResponseDetails != null && imsiSwapResp.ResponseDetails.ResponseCode != null)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("IMSISwap_" + imsiSwapResp.ResponseDetails.ResponseCode);
                        imsiSwapResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? imsiSwapResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                    #endregion
                }

                if (Convert.ToString(Session["isPrePaid"]) == "1" || imsiSwapReq.Mode =="R2")
                {
                    if (imsiSwapResp.ResponseDetails == null)
                    {
                        selList.Value = imsiSwapResp.SwapIMSI[0].ErrNo;
                        selList.Text = (imsiSwapResp.SwapIMSI[0].ErrMsg).Trim();
                    }
                    else
                    {
                        selList.Value = imsiSwapResp.ResponseDetails.ResponseCode;
                        selList.Text = (imsiSwapResp.ResponseDetails.ResponseDesc).Trim();
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - JsonResult - ImsiSwap End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                JsonObj = null;
                simSwapResp = null;
                imsiSwapResp = null;
                imsiSwapReqModel = null;
                imsiSwapReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;

            }
            return Json(selList, JsonRequestBehavior.AllowGet);
        }

        #region FRR-5022
        //5022 simswapcancel

        public ActionResult Cancelsimswap()
        {
            Cancelsimswapresponse Objresp = new Cancelsimswapresponse();
            Cancelsimswapreq ObjReq = new Cancelsimswapreq();
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - Cancelsimswap Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objresp = serviceCRM.CRMSIMSwapCancel(ObjReq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - Cancelsimswap End");

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SIMController - exception-CancelsimswapResponse - " + this.ControllerContext, ex);

            }
            finally
            {
                ObjReq = null;
            }

            return View(Objresp);
        }

        public JsonResult Cancelswap(Cancelsimswapreq ObjReq)
        {
            Cancelsimswapresponse Objresp = new Cancelsimswapresponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - Cancelswap Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objresp = serviceCRM.CRMSIMSwapCancel(ObjReq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - Cancelswap End");

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SIMController - exception-CancelswapResponse - " + this.ControllerContext, ex);

            }
            finally
            {
                ObjReq = null;
            }

            return Json(Objresp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Validatefiscalcode(string fiscalcodereq)
        {

            FiscalcodeValidationresp objresp = new FiscalcodeValidationresp();
            FiscalcodeValidationreq objreq = JsonConvert.DeserializeObject<FiscalcodeValidationreq>(fiscalcodereq);
                ServiceInvokeCRM serviceCRM;
                try
                {
                    objreq.CountryCode = clientSetting.countryCode;
                    objreq.BrandCode = clientSetting.brandCode;
                    objreq.LanguageCode = clientSetting.langCode;
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - ValidateCPIN Start");
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    objresp = serviceCRM.CRMFiscalValidation(objreq);

                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - ValidateCPINRequestResponse End");

                }
                catch (Exception ex)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SimController - exception-ValidateCPINRequestResponse - " + this.ControllerContext, ex);

                }
                finally
                {
                    objreq = null;
                }
                return Json(objresp, JsonRequestBehavior.AllowGet);

            

        }

        public ActionResult Changeownership()
        {
            changeownershipResp Objresp = new changeownershipResp();
            changeownershipreq ObjReq = new changeownershipreq();
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - Cancelsimswap Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objresp = serviceCRM.CRMchangeownership(ObjReq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - Cancelsimswap End");

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SIMController - exception-CancelsimswapResponse - " + this.ControllerContext, ex);

            }
            finally
            {
                ObjReq = null;
            }

            return View(Objresp);
            
        }

        #endregion


        //imsiSwapResp - json result
        [HttpPost]

        public JsonResult SubmitSwapIMSIUSA(USASwapIMSI SwapReqForm)
        {
            USASwapIMSIResponse simSwapResp = new USASwapIMSIResponse();
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - SubmitSwapIMSIUSA Start");
                SwapReqForm.CountryCode = clientSetting.countryCode;
                SwapReqForm.BrandCode = clientSetting.brandCode;
                SwapReqForm.LanguageCode = clientSetting.langCode;
                SwapReqForm.oldSimNumber = Convert.ToString(Session["ICCID"]);
                SwapReqForm.oldMSISDN = Convert.ToString(Session["MobileNumber"]);
                SwapReqForm.oldIMSI = Convert.ToString(Session["IMSI"]);
                SwapReqForm.userName = Convert.ToString(Session["UserName"]);
                SwapReqForm.zipCode = string.IsNullOrEmpty(SwapReqForm.zipCode) ? "0" : SwapReqForm.zipCode;
                simSwapResp = crmNewService.CRMSubmitSwapIMSI(SwapReqForm);
                ///FRR--3083
                if (simSwapResp != null && simSwapResp.responseDetails != null && simSwapResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SubmitSwapIMSIUSA_" + simSwapResp.responseDetails.ResponseCode);
                    simSwapResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? simSwapResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - SubmitSwapIMSIUSA End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                errorInsertMsg = string.Empty;
            }
            return Json(simSwapResp, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region FRR-4844

       
        public ViewResult IMEI()
        {
            return View("IMEI");
        }


        public JsonResult CRMIMEITreatmentService(string CRMIMEITreatmentServicereq)
        {
            CRMResponse objres = new CRMResponse();
            CRMIMEITreatmentResp objresp = new CRMIMEITreatmentResp();
            CRMIMEITreatmentRequest ObjReq = JsonConvert.DeserializeObject<CRMIMEITreatmentRequest>(CRMIMEITreatmentServicereq);
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objresp = serviceCRM.CRMIMEITreatmentService(ObjReq);
                   
                
                return Json(objresp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(objres, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                serviceCRM = null;
            }
        }


        #endregion
        public ActionResult Dynamicsim()
        {
            return View();
        }
        public ActionResult DynamicsimUSA()
        {
            return View();
        }
        public FileResult referencetest(string appID)
        {
            //string html = "<ul>";
            //Application word = new Application();
            //Document doc = new Document();
            //object filename = "e:\\test.docx";
            //// define an object to pass to the api for missing parameters
            //object missing = System.Type.Missing;
            //doc = word.Documents.Open(ref filename,
            //        ref missing, ref missing, ref missing, ref missing,
            //        ref missing, ref missing, ref missing, ref missing,
            //        ref missing, ref missing, ref missing, ref missing,
            //        ref missing, ref missing, ref missing);

            //string read = string.Empty;
            //List<string> data = new List<string>();
            //for (int i = 0; i < doc.Paragraphs.Count; i++)
            //{
            //    html += "<li>" + doc.Paragraphs[i + 1].Range.Text.Trim() + "</li>";

            //    //string temp = doc.Paragraphs[i + 1].Range.Text.Trim();
            //    //// if (temp != string.empty)
            //    //data.Add(temp);
            //}
            //((_Document)doc).Close();
            //((_Application)word).Quit();
            //html += "</ul>";
            //return Content(html, "text/html");


            string filepath = "E:\\Karthick\\Temp.pdf";
            byte[] pdfbyte = GetBytesFromFile(filepath);
            return File(pdfbyte, "application/pdf");
            //return File("E:\\Karthick\\SBI NOC.pdf", "application/pdf");

        }

        public static byte[] GetBytesFromFile(string fullFilePath)
        {
            // this method is limited to 2^32 byte files (4.2 GB)

            FileStream fs = null;
            try
            {
                fs = System.IO.File.OpenRead(fullFilePath);
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                return bytes;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }

        }

        public FileStreamResult GetPDF()
        {
            FileStream fs = new FileStream("c:\\PeterPDF2.pdf", FileMode.Open, FileAccess.Read);
            return File(fs, "application/pdf");
        }

        public ActionResult References()
        {
            return View();
        }

        public ActionResult SendSim()
        {
            return View();
        }


        public ActionResult SendSIMCircle()
        {

            Session["isPrePaid"] = "1";
            SendSim obj = new Models.SendSim();
            //obj.strDropdown = Utility.GetDropdownMasterFromDB(string.Empty, "3", "drop_master");

            //obj.strDropdown = Utility.GetDropdownMasterFromDB(string.Empty, "101", "Tbl_Nationality");
            //obj.strDropdown = Utility.GetDropdownMasterFromDB(string.Empty, "102", "Tbl_Nationality_EU");
            try
            {
                DataSet ds = Utility.BindXmlFile("~/App_Data/CountryListSepaCheckout.xml");
                obj.objcountrydd = new Models.CountryDropdown();
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
                obj.objcountrydd.CountryDD = objLstDrop;
            }
            catch(Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            obj.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,7,8,78", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, "1", "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, "1", "Tbl_Nationality")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, "1", "Tbl_Nationality_EU")).ToList();



            obj.lstcountries = Utility.GetDropdownMasterFromDB(string.Empty, "1", "TblCountry");
            obj.lstcounty = Utility.GetDropdownMasterFromDB("tbl_county");
            return View("SendSIMCircleNew", obj);
        }

        public JsonResult FreeSimWithBundle_Tax(string SendSim)
        {
            FreeSimWithBundle_Tax objReq = new FreeSimWithBundle_Tax();
            FreeSimWithBundleResponse ObjRes = new FreeSimWithBundleResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objReq = JsonConvert.DeserializeObject<FreeSimWithBundle_Tax>(SendSim);
                objReq.BrandCode = clientSetting.brandCode;
                objReq.CountryCode = clientSetting.countryCode;
                objReq.LanguageCode = clientSetting.langCode;
                Session["invTitle"] = objReq.Title;
                Session["invFirstName"] = objReq.FirstName;
                Session["invLastName"] = objReq.LastName;
                //string DOB = Utility.GetDateconvertion(objReq.DateOfBirth, "DD/MM/YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);
                string DOB = Utility.GetDateconvertion(objReq.DateOfBirth, clientSetting.mvnoSettings.dateTimeFormat, false, "DD/MM/YYYY");
                string[] DOBSplit = DOB.Split('/');
                objReq.DateOfBirth = DOBSplit[0] + DOBSplit[1] + DOBSplit[2];

                if (!string.IsNullOrEmpty(objReq.DebitMode) && objReq.DebitMode == "APM" || clientSetting.mvnoSettings.eshopCountryCode == "MEX")
                {
                    objReq.WpPayment.UserAgentHeader = Utility.UseragentAPM("SIM Purchase");
                }


                string validuntill = Utility.GetDateconvertion(objReq.Validuntil, clientSetting.mvnoSettings.dateTimeFormat, false, "YYYY/MM/DD");

                string isserby = Utility.GetDateconvertion(objReq.Issuer, clientSetting.mvnoSettings.dateTimeFormat, false, "YYYY/MM/DD");

                objReq.Validuntil = validuntill;
                objReq.Dateofissue = isserby;

                if (!string.IsNullOrEmpty(objReq.CardDetails.ConsentDate))
                {
                    objReq.CardDetails.ConsentDate = Utility.GetDateconvertion(objReq.CardDetails.ConsentDate, "MM/DD/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                }

                if (clientSetting.mvnoSettings.CaptureIPAddress.ToUpper() == "ON")
                {
                    objReq.DeviceInfo = Utility.DeviceInfo("SIM Purchase");
                }



                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.FreeSimWithBundle_Tax(objReq);


                    ///FRR--3083
                    if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                    {
                        if (ObjRes.Response.ResponseCode == "0" && !string.IsNullOrEmpty(objReq.IsFamily))
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Sendsim_FreeSimWithBundleTax_family");
                            ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;
                        }
                        else
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Sendsim_FreeSimWithBundleTax_" + ObjRes.Response.ResponseCode);
                            ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;

                            //6399
                            if (ObjRes.Response.ResponseCode.Split('_')[0] == "22000")
                                ObjRes.Response.ResponseCode = "22000";
                        }
                    }
                    ///FRR--3083
                
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
                serviceCRM = null;
                //ObjRes = null;
            }


        }


        public JsonResult FreeSimWithOutCredit(string SendSim)
        {
            FreeSimWithoutCredit objReq = new FreeSimWithoutCredit();
            Response ObjRes = new Response();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objReq = JsonConvert.DeserializeObject<FreeSimWithoutCredit>(SendSim);
                objReq.BrandCode = clientSetting.brandCode;
                objReq.CountryCode = clientSetting.countryCode;
                objReq.LanguageCode = clientSetting.langCode;
                //string DOB1 = Utility.GetDateconvertion(objReq.DateOfBirth, "DD/MM/YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);
                string DOB1 = Utility.GetDateconvertion(objReq.DateOfBirth, clientSetting.mvnoSettings.dateTimeFormat, false, "DD/MM/YYYY");
                string[] DOBSplit1 = DOB1.Split('/');
                objReq.DateOfBirth = DOBSplit1[0] + DOBSplit1[1] + DOBSplit1[2];
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.FreeSimWithoutCredit(objReq);
                    ///FRR--3083
                    if (ObjRes != null && ObjRes.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Sendsim_FreeSimWithOutCredit_" + ObjRes.ResponseCode);
                        ObjRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                
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
                serviceCRM = null;
                //  ObjRes = null;
            }

        }
        public string GetSIMtypes()
        {
            string strSIMtype = string.Empty;
            Dictionary<string, string> lstSIMtypes = new Dictionary<string, string>();
            string SelectSimTypes = SIMResources.ResourceManager.GetString("Select");
            try
            {
                List<DropdownMaster> objAction = Utility.GetDropdownMasterFromDB("11", "1", "drop_master");
                strSIMtype = strSIMtype + "<option title='" + SelectSimTypes + "' value=''>" + SelectSimTypes + "</option>";
                for (int i = 0; i < objAction.Count; i++)
                {
                    strSIMtype = strSIMtype + "<option title='" + objAction[i].Value + "' value='" + objAction[i].ID + "'>" + objAction[i].Value + "</option>";

                }
                return strSIMtype;
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return strSIMtype;
            }
            finally
            {
                // lstSIMtypes = null;
            }

        }

        public string GetBundletypes()
        {
            string strBundletype = string.Empty;
            Dictionary<string, string> lstSIMtypes = new Dictionary<string, string>();
            string SelectVal = SIMResources.ResourceManager.GetString("Select");

            try
            {
                lstSIMtypes = Utility.DataTableToDictionary(Utility.GetDropdownMasterFromDB("36", Convert.ToString(Session["isPrePaid"]), "drop_master"));


                strBundletype = strBundletype + "<option title='" + SelectVal + "' value=''>" + SelectVal + "</option>";
                foreach (KeyValuePair<string, string> KVPBundle in lstSIMtypes)
                {
                    strBundletype = strBundletype + "<option title='" + KVPBundle.Value + "' value='" + KVPBundle.Key + "'>" + KVPBundle.Value + "</option>";
                }
                return strBundletype;
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return strBundletype;
            }
            finally
            {
                lstSIMtypes = null;
            }

        }
        public JsonResult GetBundles()
        {
            string strgetbundles = string.Empty;
            List<BundleDetails> lstgetbundles = new List<BundleDetails>();
            BundleDetails bundleDetails = new BundleDetails();
            Bundles lstBundles = new Bundles();
            try
            {
                CRMBase objReq = new CRMBase();
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                GetBundleDetails objRes = new GetBundleDetails();
                objRes = crmNewService.LoadBundleDetails(objReq);
                if (objRes.objBundleDetails != null)
                {
                    lstBundles.lstaddon = objRes.objBundleDetails.Where(m => m.Category == "AddOn").ToList();
                    lstBundles.lstOthers = objRes.objBundleDetails.Where(m => m.Category != "AddOn").ToList();
                }
                return Json(lstBundles, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(lstBundles, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                lstgetbundles = null;
                bundleDetails = null;
                //lstBundles = null;
            }
        }
        public string GetTopupAmount()
        {
            string strTopupAmount = string.Empty;
            Dictionary<string, string> lstTopupamount = new Dictionary<string, string>();
            string SelectTopupAmt = SIMResources.ResourceManager.GetString("Select");
            try
            {
                lstTopupamount = Utility.DataTableToDictionary(Utility.GetDropdownMasterFromDB("tbl_topup_amount"));
                strTopupAmount = strTopupAmount + "<option title='" + SelectTopupAmt + "' value=''>" + SelectTopupAmt + "</option>";
                foreach (KeyValuePair<string, string> KVPSIM in lstTopupamount)
                {
                    strTopupAmount = strTopupAmount + "<option title='" + KVPSIM.Value + "' value='" + KVPSIM.Key + "'>" + KVPSIM.Value + "</option>";
                }
                return strTopupAmount;
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return strTopupAmount;
            }
            finally
            {
                //strTopupAmount = null;
                lstTopupamount = null;
            }

        }

        public string GetTitle()
        {
            string strques = string.Empty;
            Dictionary<string, string> lstTopupamount = new Dictionary<string, string>();
            string SelectTitles = SIMResources.ResourceManager.GetString("Select");
            try
            {
                List<DropdownMaster> objAction = Utility.GetDropdownMasterFromDB("1", "1", "drop_master");
                strques = strques + "<option title='" + SelectTitles + "' value=''>" + SelectTitles + "</option>";
                for (int i = 0; i < objAction.Count; i++)
                {
                    strques = strques + "<option title='" + objAction[i].Value + "' value='" + objAction[i].ID + "'>" + objAction[i].Value + "</option>";
                }
                return strques;
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return strques;
            }
            finally
            {
                lstTopupamount = null;
                // strques = null;

            }

        }


        public JsonResult CRMHLRCheckMSISDN(HLRCheckMSISDNRequest objreq)
        {
            CRMResponse objres = new CRMResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;
              serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objres = serviceCRM.CRMHLRCheckMSISDN(objreq);
                    ///FRR--3083
                    if (objres != null && objres.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Sendsim_CRMHLRCheckMSISDN_" + objres.ResponseCode);
                        objres.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                
                return Json(objres, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(objres, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                // objres = null;
                serviceCRM = null;
            }
        }

        public DynSIMAllocResponse CRMDynamicSIMAllocation_US(DynSIMAllocRequest objDSAReq)
        {
            DynSIMAllocResponse ObjRes = new DynSIMAllocResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
             serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objDSAReq.CountryCode = clientSetting.countryCode;
                    objDSAReq.BrandCode = clientSetting.brandCode;
                    objDSAReq.LanguageCode = clientSetting.langCode;
                    objDSAReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    objDSAReq.IMSI = Convert.ToString(Session["IMSI"]);
                    ObjRes = serviceCRM.CRMDynamicSIMAllocation_US(objDSAReq);

                    ///FRR--3083
                    if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("DynamicSIMAllocation_" + ObjRes.ResponseDetails.ResponseCode);
                        ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                
                return ObjRes;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return ObjRes;
            }
            finally
            {
                serviceCRM = null;
                // ObjRes = null;
            }


        }


        public JsonResult DynamciSIMForUSA(string dynamicsimdata)
        {
            DynSIMAllocResponse ObjRes = new DynSIMAllocResponse();
            DynSIMAllocRequest ObjReq = new DynSIMAllocRequest();
            try
            {
                ObjReq = JsonConvert.DeserializeObject<DynSIMAllocRequest>(dynamicsimdata);
                ObjRes = CRMDynamicSIMAllocation_US(ObjReq);
                ///FRR--3083
                if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("DynamicSIMAllocation_" + ObjRes.ResponseDetails.ResponseCode);
                    ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                return Json(ObjRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(ObjRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                ObjReq = null;
                // ObjRes = null;
            }

        }

        public JsonResult GetLoadStaticBundles()
        {
            string Data = string.Empty;
            string strgetbundles = string.Empty;
            List<BundleDetails> lstgetbundles = new List<BundleDetails>();
            BundleDetails bundleDetails = new BundleDetails();
            Bundles lstBundles = new Bundles();
            CRMBase objReq = new CRMBase();
            GetBundleDetails objRes = new GetBundleDetails();
            try
            {

                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objRes = crmNewService.LoadBundleDetails(objReq);
                if (objRes.objBundleDetails.Count > 0)
                {
                    foreach (BundleDetails objBundle in objRes.objBundleDetails)
                    {
                        Data += "<h4><strong>" + objBundle.PlanName + "</strong></h4>";
                        Data += "<p>" + objBundle.Description + "</p>";
                        if (clientSetting.preSettings.enableComponentBundle.ToUpper() == "TRUE" && !string.IsNullOrEmpty(objBundle.ComponentBundleName) && !string.IsNullOrEmpty(objBundle.ComponentBundleCode))
                        {
                            Data += "<p>" + objBundle.ComponentBundleName + " : " + objBundle.ComponentBundleDesc + "</p>";
                        }
                    }
                }
                return Json(Data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(Data, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                lstgetbundles = null;
                bundleDetails = null;
                lstBundles = null;
                objReq = null;
                objRes = null;
                //Data = null;
                strgetbundles = null;
            }

        }

        public JsonResult CRMCheckandCalculatePromoCode(string promoCode)
        {
            PromoCodeResponse ObjRes = new PromoCodeResponse();
            CheckandCalculatePromoCode ObjReq = JsonConvert.DeserializeObject<CheckandCalculatePromoCode>(promoCode);
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode.ToString();
               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMCheckandCalculatePromoCode(ObjReq);

                    ///FRR--3083
                    if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CheckandCalculatePromoCode_" + ObjRes.ResponseDetails.ResponseCode);
                        ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                
                return Json(ObjRes);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(ObjRes);
            }
            finally
            {
                // ObjRes = null;
                ObjReq = null;
                serviceCRM = null;
            }

        }

        public JsonResult CRMChannelSpeicficBundleCost(string ChannelCost)
        {
            ChannelSpecificbundleCost ObjRes = new ChannelSpecificbundleCost();
            SpecificbundleCost ObjReq = JsonConvert.DeserializeObject<SpecificbundleCost>(ChannelCost);
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode.ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMChannelSpeicficBundleCost(ObjReq);

                    ///FRR--3083
                    if (ObjRes != null && ObjRes.reponseDetails != null && ObjRes.reponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ChannelSpeicficBundleCost_" + ObjRes.reponseDetails.ResponseCode);
                        ObjRes.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                
                return Json(ObjRes);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(ObjRes);
            }
            finally
            {
                // ObjRes = null;
                serviceCRM = null;
                ObjReq = null;
            }

        }

        #region InitialCancelLocation
        public ActionResult InitiateCancelLocation(string Textdata)
        {
            Initial_Location objInitialLocation = new Initial_Location();
            try
            {
               

                #region FRR 4925
                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {

                    Session["RealICCIDForMultiTab"] = Textdata;
                }
                #endregion

                objInitialLocation.strDropdown = Utility.GetDropdownMasterFromDB("22", Convert.ToString(Session["isprepaid"]), "drop_master");
                return View(objInitialLocation);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View(objInitialLocation);
            }
            finally
            {
                //objInitialLocation = null;
            }
        }

        public JsonResult HistoryCancelLocation(Initial_Location objCancelReq)
        {
            CancelInitialLocation CancelLocationReq = new CancelInitialLocation();
            CancelInitialLocationResponse CancelLocationResp = new CancelInitialLocationResponse();
            Initial_Location objInitialLocation = new Initial_Location();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CancelLocationReq.CountryCode = clientSetting.countryCode;
                CancelLocationReq.BrandCode = clientSetting.brandCode;
                CancelLocationReq.LanguageCode = clientSetting.langCode;
              

                #region FRR 4925
                if(clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                  
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    CancelLocationReq.MSISDN = localDict.Where(x => objCancelReq.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    CancelLocationReq.IMSI= localDict.Where(x => objCancelReq.textdata.ToString().Contains(x.Key)).Select(x => x.Value.IMSI).First().ToString();
                }
                else
                {
                    CancelLocationReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    CancelLocationReq.IMSI = Convert.ToString(Session["IMSI"]);
                  
                }
                #endregion

                CancelLocationReq.userName = Convert.ToString(Session["UserName"]);
                CancelLocationReq.mode = objCancelReq.mode;
                CancelLocationReq.drpNAMText = objCancelReq.NAMText;
                if (CancelLocationReq.mode == "I")
                {
                    CancelLocationReq.drpNAM = objCancelReq.NAM;
                    CancelLocationReq.mode = "I";
                    if (objCancelReq.retainLocation == "true")
                        CancelLocationReq.retainLocation = "1";
                    else
                        CancelLocationReq.retainLocation = "0";
     serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        CancelLocationResp = serviceCRM.InitialCancelLocation(CancelLocationReq);
                        ///FRR--3083
                        if (CancelLocationResp != null && CancelLocationResp.responseDetails != null && CancelLocationResp.responseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("HistoryCancelLocation_" + CancelLocationResp.responseDetails.ResponseCode);
                            CancelLocationResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CancelLocationResp.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                        ///FRR--3083
                    
                }
                else if (CancelLocationReq.mode == "R")
                {
               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        CancelLocationResp = serviceCRM.InitialCancelLocation(CancelLocationReq);

                        ///FRR--3083
                        if (CancelLocationResp != null && CancelLocationResp.responseDetails != null && CancelLocationResp.responseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("HistoryCancelLocation_" + CancelLocationResp.responseDetails.ResponseCode);
                            CancelLocationResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CancelLocationResp.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                        ///FRR--3083

                    

                }
                return Json(CancelLocationResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(CancelLocationResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                CancelLocationReq = null;
                // CancelLocationResp = null;
                objInitialLocation = null;
                serviceCRM = null;
            }
        }



        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadInitialCancelLocation(string CancelLocationData)
        {
            try
            {
                GridView gridView = new GridView();
                XmlDocument XMLSer = new XmlDocument();
                List<CancelInitialLocationReport> CancelLocationDetails = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<CancelInitialLocationReport>>(CancelLocationData);
                if (Convert.ToString(Session["isPrePaid"]) == "1")
                {
                    if (clientSetting.mvnoSettings.iclPlintronAmerica.ToUpper() == "ON")
                    {
                        List<CancelInitialLocationReport> objDownloadOn = CancelLocationDetails.Select(a => new CancelInitialLocationReport { IMSI = a.IMSI, MSISDN = a.MSISDN, submittedBy = a.submittedBy, submittedDate = a.submittedDate }).ToList();
                        XMLSer = Common.CreateXML(CancelLocationDetails);
                    }
                    else
                    {
                        List<CancelInitialLocationReport> objDownloadOn = CancelLocationDetails.Select(a => new CancelInitialLocationReport { IMSI = a.IMSI, MSISDN = a.MSISDN, submittedBy = a.submittedBy, submittedDate = a.submittedDate, drpNAM = a.drpNAM, retainLocation = a.retainLocation }).ToList();
                        XMLSer = Common.CreateXML(CancelLocationDetails);

                    }
                }

                StringReader reader = new StringReader(XMLSer.InnerXml);
                DataSet ds = new DataSet();
                ds.ReadXml(reader);
                DataTable dt = new DataTable();
                dt = ds.Tables[0];
                string colNames = string.Empty;
                if (clientSetting.mvnoSettings.iclPlintronAmerica.ToUpper() == "ON")
                {
                    dt.Columns.Remove("drpNAM");
                    dt.Columns.Remove("retainLocation");
                }

                //// On all tables' columns
                foreach (DataColumn dc in dt.Columns)
                {
                    colNames = Resources.SIMResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
                    if (colNames != string.Empty && colNames != null)
                        dt.Columns[dc.ColumnName].ColumnName = colNames;
                }
                dt.AcceptChanges();
                gridView.DataSource = dt;
                gridView.DataBind();

                Utility.ExportToExcell(gridView, "InitialCancelLocation", this.HttpContext.Response);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
        }



        #endregion

        public ActionResult RetryDNA()
        {
            RetryDNARequest ObjReq = new RetryDNARequest();
            RetryDNAResponse ObjResp = new RetryDNAResponse();
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.mode = "Q";
                ObjResp = crmNewService.RetryDNACRM(ObjReq);

                ObjResp.retryDNA.ForEach(b => b.requestedDate = Utility.GetDateconvertion(b.requestedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));

                return View("RetryDNA", ObjResp);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View("RetryDNA", ObjResp);
            }
            finally
            {
                ObjReq = null;
                // ObjResp = null;
            }

        }

        public ActionResult RetryDNAProcess(string RetryDNAProcessDetails)
        {
            RetryDNAResponse ObjResp = new RetryDNAResponse();
            RetryDNARequest ObjReq = JsonConvert.DeserializeObject<RetryDNARequest>(RetryDNAProcessDetails);
            try
            {

                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjResp = crmNewService.RetryDNACRM(ObjReq);
                ObjResp.retryDNA.ForEach(b => b.requestedDate = Utility.GetDateconvertion(b.requestedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RetryDNAProcess_" + ObjResp.responseDetails.ResponseCode);
                ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            finally
            {
                // ObjResp = null;
                ObjReq = null;
            }
            //return Json(ObjResp, JsonRequestBehavior.AllowGet);

        }


        #region De-ActivateMsisdn
        public ActionResult DeActivateMsisdn(string PageName, string MSISDN, string PAId)
        {
            DeActivateModelRequest DeactivateReq = new DeActivateModelRequest();
            PendingDetailsRequest pendingAppReq = new PendingDetailsRequest();
            PendingDetailsResponce ObjAppResp = new PendingDetailsResponce();
            ServiceInvokeCRM serviceCRM;
            try
            {

                if (PageName != null)
                {
                    DeactivateReq.PageName = PageName;
                    Session["isPrePaid"] = "1";
                }
                else
                    ViewBag.selectval = Convert.ToString(Session["SIMSTATUS"]) == "0" ? "2" : "1";

                if (Session["MobileNumber"] != null)
                {
                    DeactivateReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                }
                DeactivateReq.lstDeActions = Utility.GetDropdownMasterFromDB("41", Convert.ToString(Session["isPrePaid"]), "drop_master");
                if ((Session["PAType"] != null && (Convert.ToString(Session["PAType"]) == "DEACTIVATE" || Convert.ToString(Session["PAType"]) == "ACTIVATE")) || (!string.IsNullOrEmpty(PageName) && !string.IsNullOrEmpty(PAId) && !string.IsNullOrEmpty(MSISDN)))
                {
                    /* PendingApproveDetails request*/
                    pendingAppReq.CountryCode = clientSetting.countryCode;
                    pendingAppReq.BrandCode = clientSetting.brandCode;
                    pendingAppReq.LanguageCode = clientSetting.langCode;
                    pendingAppReq.MSISDN = string.IsNullOrEmpty(MSISDN) ? Convert.ToString(Session["MobileNumber"]) : MSISDN;
                    pendingAppReq.Type = string.IsNullOrEmpty(PAId) ? Convert.ToString(Session["PAType"]) : "ACTIVATE";
                    pendingAppReq.Id = string.IsNullOrEmpty(PAId) ? Convert.ToString(Session["PAId"]) : PAId;
                    DeactivateReq.papproveType = true;


                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        ObjAppResp = serviceCRM.CRMGetPendingDetails(pendingAppReq);

                        if (ObjAppResp != null && ObjAppResp.ResponseDetails != null && ObjAppResp.ResponseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("DeActivate_" + ObjAppResp.ResponseDetails.ResponseCode);
                            ObjAppResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjAppResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                        }
                    
                    if (ObjAppResp.PendingDetails.Count > 0)
                    {
                        DeactivateReq.ID = ObjAppResp.PendingDetails[0].Id;
                        DeactivateReq.MSISDN = ObjAppResp.PendingDetails[0].MSISDN;
                        DeactivateReq.UserName = ObjAppResp.PendingDetails[0].RequestBy;
                        DeactivateReq.Reason = ObjAppResp.PendingDetails[0].Reason;
                        ViewBag.selectval = pendingAppReq.Type.ToUpper() == "DEACTIVATE" ? "2" : "1";
                        ViewBag.TktID = ObjAppResp.PendingDetails[0].TicketId;
                    }
                }
                else
                {
                    DeactivateReq.Mode = "QUERY";
                    DeactivateReq.papproveType = false;
                }
                // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                Session["PAType"] = null;
                return View(DeactivateReq);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(DeactivateReq);
            }
            finally
            {
                // DeactivateReq = null;
                pendingAppReq = null;
                ObjAppResp = null;
                serviceCRM = null;
            }

        }

        public JsonResult ActivateDeActivateMsisdn(CRMSActivateDeActivateRequest DeaActiveSreq)
        {
            CRMActivateDeActivateResponse DeActiveResp = new CRMActivateDeActivateResponse();
            SelectListItem lstItem = new SelectListItem();
            string flgMode = string.Empty;
            CRMSActivateDeActivateRequest deActivateJsonReq = new CRMSActivateDeActivateRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {

                deActivateJsonReq.CountryCode = clientSetting.countryCode;
                deActivateJsonReq.BrandCode = clientSetting.brandCode;
                deActivateJsonReq.LanguageCode = clientSetting.langCode;

                if (DeaActiveSreq.MSISDN != null)
                {
                    Session["MobileNumber"] = DeaActiveSreq.MSISDN;
                    deActivateJsonReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                }
                else
                {
                    deActivateJsonReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    deActivateJsonReq.IMSI = Convert.ToString(Session["IMSI"]);
                }
                deActivateJsonReq.UserName = Convert.ToString(Session["UserName"]);
                deActivateJsonReq.Reason = DeaActiveSreq.Reason;
                deActivateJsonReq.Action = DeaActiveSreq.Action;
                //PID-43151
                deActivateJsonReq.AdminComment = DeaActiveSreq.AdminComment;
                List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
                menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SIM_DeActivateMsisdn").ToList();
                if (DeaActiveSreq.Mode != "QUERY")
                {
                    //if (!Convert.ToBoolean(Session["IsAdmin"]))
                    //if (menu[0] != null && (menu[0].Approval1.ToUpper() != "TRUE" && menu[0].DirectApproval.ToUpper() != "TRUE"))
                    if (menu[0] != null && (menu[0].DirectApproval.ToUpper() != "TRUE") && string.IsNullOrEmpty(Convert.ToString(Session["PAId"])))
                    {

                        //if ((Convert.ToString(Session["ROLE_CAT_TYPE"]) == "2") && clientSetting.mvnoSettings.userManagementType.ToLower() == "on")
                        //if (menu[0] != null && (menu[0].Approval1.ToUpper() != "TRUE" && menu[0].DirectApproval.ToUpper() != "TRUE"))

                        /* set the Mode for Submit(Normal User or Agent) */
                        flgMode = "NORMALAGENT";
                        deActivateJsonReq.Mode = flgMode;

                    }
                    else
                    {
                        /*Pending Approve and Reject*/
                        if (DeaActiveSreq.Mode == "3")
                        {
                            flgMode = "ADMIN";

                            if (!string.IsNullOrEmpty(Convert.ToString(Session["PAId"])))
                            {
                                deActivateJsonReq.ID = Session["PAId"].ToString();
                            }
                            else
                            {
                                deActivateJsonReq.ID = DeaActiveSreq.ID;
                            }
                            deActivateJsonReq.Mode = flgMode;
                        }

                        else if (DeaActiveSreq.Mode == "4")
                        {
                            flgMode = "REJECT";
                            if (!string.IsNullOrEmpty(Convert.ToString(Session["PAId"])))
                            {
                                deActivateJsonReq.ID = Session["PAId"].ToString();
                            }
                            else
                            {
                                deActivateJsonReq.ID = DeaActiveSreq.ID;
                            }
                            deActivateJsonReq.Mode = flgMode;
                        }
                        else if (DeaActiveSreq.Mode == "5")
                        {
                            deActivateJsonReq.Mode = "OUTADMIN";
                        }
                        else
                        {
                            //Approve 
                            flgMode = "FULLADMIN";
                            deActivateJsonReq.Mode = flgMode;
                        }

                    }
                }
                else
                {
                    deActivateJsonReq.Mode = DeaActiveSreq.Mode;
                }

               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    //FRR-3406forAGENT 

                    if (DeaActiveSreq.Action == "ACTIVATE")
                        deActivateJsonReq.PARequestType = Resources.HomeResources.PAActivate;
                    if (DeaActiveSreq.Action == "DEACTIVATE")
                        deActivateJsonReq.PARequestType = Resources.HomeResources.PADeActivate;
                    DeActiveResp = serviceCRM.CRMActivateDeActivate(deActivateJsonReq);
                    if (DeaActiveSreq.Mode != "QUERY")
                    {
                        if (DeActiveResp != null && DeActiveResp.responseDetails != null && DeActiveResp.responseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("DeActivate_" + DeActiveResp.responseDetails.ResponseCode);
                            DeActiveResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? DeActiveResp.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                        // if (Convert.ToBoolean(Session["IsAdmin"]) || ((Convert.ToString(Session["ROLE_CAT_TYPE"]) == "2") && clientSetting.mvnoSettings.userManagementType.ToLower() == "on"))
                        if (menu[0] != null && (menu[0].Approval1.ToUpper() == "TRUE" || menu[0].DirectApproval.ToUpper() == "TRUE"))
                        {
                            if ((DeActiveResp != null) && DeaActiveSreq.Action.ToUpper().Trim() == "ACTIVATE" && (DeActiveResp.responseDetails.ResponseCode == "0" || DeActiveResp.responseDetails.ResponseCode == "17"))
                            {
                                Session["SIMSTATUS"] = "0";
                            }
                            else if ((DeActiveResp != null) && DeaActiveSreq.Action.ToUpper().Trim() == "DEACTIVATE" && (DeActiveResp.responseDetails.ResponseCode == "0" || DeActiveResp.responseDetails.ResponseCode == "17"))
                            {
                                Session["SIMSTATUS"] = "1";
                            }
                        }
                    }

                
                if (DeActiveResp.responseDetails != null)
                {
                    if (DeaActiveSreq.Mode != "QUERY")
                    {
                        lstItem.Value = DeActiveResp.responseDetails.ResponseCode;
                        lstItem.Text = (DeActiveResp.responseDetails.ResponseDesc).Trim();
                    }
                    else
                    {
                        lstItem.Value = DeActiveResp.responseDetails.ResponseCode + "|" + DeActiveResp.LycaMode;
                        lstItem.Text = (DeActiveResp.responseDetails.ResponseDesc).Trim();
                    }
                }
                return Json(lstItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(lstItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                DeActiveResp = null;
                //lstItem = null;
                deActivateJsonReq = null;
                serviceCRM = null;
            }
        }
        #endregion


        #region SuspendRestore
        public ActionResult SuspendRestore()
        {
            SuspendRestoreModelRequest SuspendRestoreReq = new SuspendRestoreModelRequest();
            PendingDetailsRequest pendingReq = new PendingDetailsRequest();
            PendingDetailsResponce ObjResp = new PendingDetailsResponce();
            ServiceInvokeCRM serviceCRM;
            try
            {
                ViewBag.selectval = Convert.ToString(Session["SIMSTATUS"]) == "0" ? "1" : "2";
                SuspendRestoreReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                SuspendRestoreReq.lstSuspendActions = Utility.GetDropdownMasterFromDB("42", Convert.ToString(Session["isPrePaid"]), "drop_master");
                if (Session["PAType"] != null && (Convert.ToString(Session["PAType"]) == "SUSPEND" || Convert.ToString(Session["PAType"]) == "RESTORE"))
                {
                    /* PendingApproveDetails request*/
                    pendingReq.CountryCode = clientSetting.countryCode;
                    pendingReq.BrandCode = clientSetting.brandCode;
                    pendingReq.LanguageCode = clientSetting.langCode;
                    pendingReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    pendingReq.Type = Convert.ToString(Session["PAType"]);
                    pendingReq.Id = Convert.ToString(Session["PAId"]);
                    SuspendRestoreReq.PappType = true;

                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        ObjResp = serviceCRM.CRMGetPendingDetails(pendingReq);

                        if (ObjResp != null && ObjResp.ResponseDetails != null && ObjResp.ResponseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SuspendRestore_" + ObjResp.ResponseDetails.ResponseCode);
                            ObjResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                        }
                    
                    if (ObjResp.PendingDetails.Count > 0)
                    {
                        SuspendRestoreReq.ID = ObjResp.PendingDetails[0].Id;
                        SuspendRestoreReq.MSISDN = ObjResp.PendingDetails[0].MSISDN;
                        SuspendRestoreReq.UserName = ObjResp.PendingDetails[0].RequestBy;
                        SuspendRestoreReq.Reason = ObjResp.PendingDetails[0].Reason;
                        ViewBag.selectval = pendingReq.Type.ToUpper() == "SUSPEND" ? "1" : "2";
                        ViewBag.TktID = ObjResp.PendingDetails[0].TicketId;
                    }
                }
                else
                {
                    SuspendRestoreReq.Mode = "QUERY";
                    SuspendRestoreReq.PappType = false;
                    if (Session["MobileNumber"] != null)
                    {
                        SuspendRestoreReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    }
                }
                // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                Session["PAType"] = null;
                return View(SuspendRestoreReq);

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(SuspendRestoreReq);
            }
            finally
            {
                // SuspendRestoreReq = null;
                serviceCRM = null;
                pendingReq = null;
                ObjResp = null;
            }

        }

        [HttpPost]
        public JsonResult SuspendRestore(CRMSuspendRestoreRequest SuspendRest)
        {
            CRMSuspendRestoreResponse SuspendRestResp = new CRMSuspendRestoreResponse();
            SelectListItem lstItem = new SelectListItem();
            string flagMode = string.Empty;
            CRMSuspendRestoreRequest SuspendRestReq = new CRMSuspendRestoreRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                SuspendRestReq.CountryCode = clientSetting.countryCode;
                SuspendRestReq.BrandCode = clientSetting.brandCode;
                SuspendRestReq.LanguageCode = clientSetting.langCode;
                SuspendRestReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                SuspendRestReq.UserName = Convert.ToString(Session["UserName"]);
                SuspendRestReq.Reason = SuspendRest.Reason;
                SuspendRestReq.Action = SuspendRest.Action;
                SuspendRestReq.IMSI = Convert.ToString(Session["IMSI"]);
                //PID-43151
                SuspendRestReq.AdminComment = SuspendRest.AdminComment;
                List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
                menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SIM_SuspendRestore").ToList();
                if (SuspendRest.Mode != "QUERY")
                {

                    //if (!Convert.ToBoolean(Session["IsAdmin"]))
                    // if (menu[0] != null && (menu[0].Approval1.ToUpper() != "TRUE" && menu[0].DirectApproval.ToUpper() != "TRUE"))
                    if (menu[0] != null && (menu[0].DirectApproval.ToUpper() != "TRUE") && string.IsNullOrEmpty(Convert.ToString(Session["PAId"])))
                    {
                        /* set the Mode for Submit(Normal User or Agent) */
                        flagMode = "NORMALAGENT";
                        SuspendRestReq.Mode = flagMode;

                    }
                    else
                    {
                        /*Pending Approve and Reject*/
                        if (SuspendRest.Mode == "3")
                        {
                            flagMode = "ADMIN";
                            if (Session["PAId"] != null)
                            {
                                SuspendRestReq.ID = Session["PAId"].ToString();
                            }
                            SuspendRestReq.Mode = flagMode;
                        }
                        else if (SuspendRest.Mode == "4")
                        {
                            flagMode = "REJECT";
                            if (Session["PAId"] != null)
                            {
                                SuspendRestReq.ID = Session["PAId"].ToString();
                            }
                            SuspendRestReq.Mode = flagMode;
                        }
                        else
                        {
                            //Approve 
                            flagMode = "FULLADMIN";
                            SuspendRestReq.Mode = flagMode;
                        }

                    }
                }
                else
                {
                    SuspendRestReq.Mode = SuspendRest.Mode;
                }
               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    //FRR-3406PendingforAgent
                    //if ((Convert.ToString(Session["ROLE_CAT_TYPE"]) == "3") && clientSetting.mvnoSettings.userManagementType.ToLower() == "on")

                    if (!string.IsNullOrEmpty(Convert.ToString(SuspendRest.Action)) && SuspendRest.Action == "SUSPEND")
                        SuspendRestReq.PARequestType = Resources.HomeResources.PASuspend;
                    if (!string.IsNullOrEmpty(Convert.ToString(SuspendRest.Action)) && SuspendRest.Action == "RESTORE")
                        SuspendRestReq.PARequestType = Resources.HomeResources.PARestore;


                    SuspendRestResp = serviceCRM.CRMSuspendRestore(SuspendRestReq);
                    if (SuspendRest.Mode != "QUERY")
                    {
                        if (SuspendRestResp != null && SuspendRestResp.responseDetails != null && SuspendRestResp.responseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SuspendRestore_" + SuspendRestResp.responseDetails.ResponseCode);
                            SuspendRestResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? SuspendRestResp.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                        if ((SuspendRestResp != null) && SuspendRest.Action.ToUpper().Trim() == "RESTORE" && (SuspendRestResp.responseDetails.ResponseCode == "0" || SuspendRestResp.responseDetails.ResponseCode == "25"))
                        {
                            Session["SIMSTATUS"] = "0";
                        }
                        else if ((SuspendRestResp != null) && SuspendRest.Action.ToUpper().Trim() == "SUSPEND" && (SuspendRestResp.responseDetails.ResponseCode == "0" || SuspendRestResp.responseDetails.ResponseCode == "25"))
                        {
                            Session["SIMSTATUS"] = "1";
                        }
                    }

                
                if (SuspendRestResp != null)
                {
                    if (SuspendRestResp.responseDetails.ResponseCode != null)
                    {
                        if (SuspendRest.Mode != "QUERY")
                        {

                            lstItem.Value = SuspendRestResp.responseDetails.ResponseCode;
                            lstItem.Text = (SuspendRestResp.responseDetails.ResponseDesc).Trim();
                        }
                        else
                        {
                            lstItem.Value = SuspendRestResp.responseDetails.ResponseCode + "|" + SuspendRestResp.LycaMode;
                            lstItem.Text = (SuspendRestResp.responseDetails.ResponseDesc).Trim();
                        }
                    }
                }
                return Json(lstItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(lstItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                SuspendRestResp = null;
                //lstItem = null;
                SuspendRestReq = null;
                serviceCRM = null;
            }
        }



        #endregion

        #region LMSISDN
        public ActionResult LMSISDNSubscription()
        {
            LMSISDNApproveResp reactiveLMSISDNResp = new LMSISDNApproveResp();
            PendingDetailsRequest req = new PendingDetailsRequest();
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
            ServiceInvokeCRM serviceCRM;

            try
            {

                if (Session["PAType"] != null && (Convert.ToString(Session["PAType"]) == "REACTIVATE LMSISDN"))
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

                        if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LMSISDNSubscription_" + ObjRes.ResponseDetails.ResponseCode);
                            ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;

                            if (ObjRes.PendingDetails.Count > 0)
                            {
                                reactiveLMSISDNResp.Id = ObjRes.PendingDetails[0].Id;
                                reactiveLMSISDNResp.CountryCode = ObjRes.PendingDetails[0].CIP;
                                reactiveLMSISDNResp.LMSISDN = ObjRes.PendingDetails[0].NewMSISDN;
                                reactiveLMSISDNResp.Status = ObjRes.PendingDetails[0].Blockstatus;
                                reactiveLMSISDNResp.TicketID = ObjRes.PendingDetails[0].TicketId;
                                reactiveLMSISDNResp.Comments = ObjRes.PendingDetails[0].Comments;

                                reactiveLMSISDNResp.PAType = Convert.ToString(Session["PAType"]);
                                reactiveLMSISDNResp.PAId = Convert.ToString(Session["PAId"]);

                                Session["PAType"] = null;
                                Session["PAId"] = null;
                            }
                        }
                    

                }

                return View(reactiveLMSISDNResp);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View(reactiveLMSISDNResp);
            }
            finally
            {
                //reactiveLMSISDNResp = null;
                req = null;
                ObjRes = null;
                serviceCRM = null;
            }

        }

        [HttpPost]
        public JsonResult LMSISDNGetSub(ReactiveGetSubscriberRequest LMSISDNRequest)
        {
            ReactiveGetSubscriberResponse LMSISDNResponse = new ReactiveGetSubscriberResponse();
            ServiceInvokeCRM serviceCRM;

            try
            {
               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    LMSISDNRequest.CountryCode = clientSetting.countryCode;
                    LMSISDNRequest.BrandCode = clientSetting.brandCode;
                    LMSISDNRequest.LanguageCode = clientSetting.langCode;

                    LMSISDNResponse = serviceCRM.LMSISDNReactiveGetSubscriber(LMSISDNRequest);

                    if (LMSISDNResponse.responseDetails != null && LMSISDNResponse.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LMSISDNGetSubResponse_" + LMSISDNResponse.responseDetails.ResponseCode);
                        LMSISDNResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? LMSISDNResponse.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                
                return Json(LMSISDNResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(LMSISDNResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                // LMSISDNResponse = null;
                serviceCRM = null;
            }

        }

        [HttpPost]
        public JsonResult LMSISDNCountryList(ReactiveMultiCountryListReq LMSISDNRequest)
        {
            ReactiveMultiCountryListResponse LMSISDNResponse = new ReactiveMultiCountryListResponse();
            ServiceInvokeCRM serviceCRM;

            try
            {
              serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    LMSISDNRequest.CountryCode = clientSetting.countryCode;
                    LMSISDNRequest.BrandCode = clientSetting.brandCode;
                    LMSISDNRequest.LanguageCode = clientSetting.langCode;

                    LMSISDNResponse = serviceCRM.LMSISDNCountryList(LMSISDNRequest);

                    try
                    {
                        if (LMSISDNResponse != null && LMSISDNResponse.CountryList != null && LMSISDNResponse.CountryList.Count > 0)
                        {
                            for (int i = 0; i < LMSISDNResponse.CountryList.Count; i++)
                            {
                                //string dummystring = ObjResp.topupStatus[i].description;

                                string dummystring = System.Text.RegularExpressions.Regex.Replace(LMSISDNResponse.CountryList[i].Country, "[^0-9a-zA-Z]+", string.Empty);
                                string ResourceMsg = Resources.DropdownResources.ResourceManager.GetString(dummystring);
                                LMSISDNResponse.CountryList[i].Country = string.IsNullOrEmpty(ResourceMsg) ? LMSISDNResponse.CountryList[i].Country : ResourceMsg;
                            }
                        }
                    }
                    catch
                    {

                    }

                    if (LMSISDNResponse.responseDetails != null && LMSISDNResponse.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LMSISDNGetSubResponse_" + LMSISDNResponse.responseDetails.ResponseCode);
                        LMSISDNResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? LMSISDNResponse.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                
                return Json(LMSISDNResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(LMSISDNResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //LMSISDNResponse = null;
                serviceCRM = null;
            }

        }

        [HttpPost]
        public JsonResult LMSISDNCountryOnChange(MultiIMSICountryRequest LMSISDNRequest)
        {
            MultiIMSICountryResponse LMSISDNResponse = new MultiIMSICountryResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    LMSISDNRequest.CountryCode = clientSetting.countryCode;
                    LMSISDNRequest.BrandCode = clientSetting.brandCode;
                    LMSISDNRequest.LanguageCode = clientSetting.langCode;

                    LMSISDNResponse = serviceCRM.LMSISDNCountryChanging(LMSISDNRequest);

                    if (LMSISDNResponse.responseDetails != null && LMSISDNResponse.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LMSISDNGetSubResponse_" + LMSISDNResponse.responseDetails.ResponseCode);
                        LMSISDNResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? LMSISDNResponse.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                
                return Json(LMSISDNResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(LMSISDNResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                serviceCRM = null;
                // LMSISDNResponse = null;
            }

        }

        [HttpPost]
        public JsonResult LMSISDNHistoryDetails(ReactiveHistoryREQ LMSISDNRequest)
        {
            ReactiveHistoryResponse LMSISDNResponse = new ReactiveHistoryResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    LMSISDNRequest.CountryCode = clientSetting.countryCode;
                    LMSISDNRequest.BrandCode = clientSetting.brandCode;
                    LMSISDNRequest.LanguageCode = clientSetting.langCode;

                    LMSISDNResponse = serviceCRM.LMSISDNHistory(LMSISDNRequest);

                    if (LMSISDNResponse.ResponseDetails != null && LMSISDNResponse.ResponseDetails.ResponseCode != null && LMSISDNResponse.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LMSISDNGetSubResponse_" + LMSISDNResponse.ResponseDetails.ResponseCode);
                        LMSISDNResponse.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? LMSISDNResponse.ResponseDetails.ResponseDesc : errorInsertMsg;
                        //LMSISDNResponse.ReactiveHistory.FindAll(a => a.RequestedDate != string.Empty).ForEach(b => b.RequestedDate = Utility.FormatDateTime(b.RequestedDate, clientSetting.mvnoSettings.dateTimeFormat));
                        // LMSISDNResponse.ReactiveHistory.FindAll(a => a.AuthorisedDate != string.Empty).ForEach(b => b.AuthorisedDate = Utility.FormatDateTime(b.AuthorisedDate, clientSetting.mvnoSettings.dateTimeFormat));
                    }
                    if (LMSISDNResponse.ReactiveHistory != null)
                    {
                        LMSISDNResponse.ReactiveHistory.ForEach(b => b.RequestedDate = Utility.GetDateconvertion(b.RequestedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                        LMSISDNResponse.ReactiveHistory.ForEach(b => b.AuthorisedDate = Utility.GetDateconvertion(b.AuthorisedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));

                    }
                
                return Json(LMSISDNResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(LMSISDNResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                // LMSISDNResponse = null;
                serviceCRM = null;
            }

        }

        [HttpPost]
        public JsonResult LMSISDNApproveRej(AuthoriseReactiveReq LMSISDNRequest)
        {
            AuthoriseReactiveResonse LMSISDNResponse = new AuthoriseReactiveResonse();
            ServiceInvokeCRM serviceCRM;

            try
            {
             serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    LMSISDNRequest.CountryCode = clientSetting.countryCode;
                    LMSISDNRequest.BrandCode = clientSetting.brandCode;
                    LMSISDNRequest.LanguageCode = clientSetting.langCode;
                    LMSISDNRequest.Username = LMSISDNRequest.RequestedBy = Convert.ToString(Session["UserName"]);
                    LMSISDNRequest.Msisdn = Convert.ToString(Session["MobileNumber"]);
                    LMSISDNRequest.PARequestType = Resources.HomeResources.PAReActivateLMSISDN;

                    LMSISDNResponse = serviceCRM.LMSISDNApproveReject(LMSISDNRequest);

                    if (LMSISDNResponse.ResponseDetails != null && LMSISDNResponse.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LMSISDNGetSubResponse_" + LMSISDNResponse.ResponseDetails.ResponseCode);
                        LMSISDNResponse.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? LMSISDNResponse.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                
                return Json(LMSISDNResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(LMSISDNResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //LMSISDNResponse = null;
            }

        }

        #endregion

        #region MHA
        public ActionResult ActivateMHASubscriber()
        {
            MHAApproveResp MHAResp = new MHAApproveResp();
            PendingDetailsRequest req = new PendingDetailsRequest();
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
            ServiceInvokeCRM serviceCRM;
            try
            {

                if (Session["PAType"] != null && (Convert.ToString(Session["PAType"]) == "MHA"))
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

                        if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManaageOBABundle_" + ObjRes.ResponseDetails.ResponseCode);
                            ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;

                            if (ObjRes.PendingDetails.Count > 0)
                            {
                                MHAResp.Id = ObjRes.PendingDetails[0].Id;
                                MHAResp.AccountID = ObjRes.PendingDetails[0].OldMSISDN;
                                MHAResp.TicketID = ObjRes.PendingDetails[0].TicketId;
                                MHAResp.Reason = ObjRes.PendingDetails[0].Reason;

                                MHAResp.PAType = Convert.ToString(Session["PAType"]);
                                MHAResp.PAId = Convert.ToString(Session["PAId"]);

                                Session["PAType"] = null;
                                Session["PAId"] = null;
                            }
                        }
                    

                }

                return View(MHAResp);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View(MHAResp);
            }
            finally
            {
                //MHAResp = null;
                req = null;
                ObjRes = null;
                serviceCRM = null;
            }

        }

        [HttpPost]
        public JsonResult MHASubmitApproveRej(ActivateMHASubscriberRequest MHARequest)
        {
            ActivateMHASubscriberResponse MHAResponse = new ActivateMHASubscriberResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                 serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    MHARequest.CountryCode = clientSetting.countryCode;
                    MHARequest.BrandCode = clientSetting.brandCode;
                    MHARequest.LanguageCode = clientSetting.langCode;
                    MHARequest.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    MHARequest.userName = Convert.ToString(Session["UserName"]);
                    MHARequest.PARequestType = Resources.HomeResources.PAMHA;

                    MHAResponse = serviceCRM.ActivateMHASubscriber(MHARequest);

                    if (MHAResponse != null && MHAResponse.responseDetails.ResponseCode != null && MHAResponse.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ActivateMHASubscriber_" + MHAResponse.responseDetails.ResponseCode);
                        MHAResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? MHAResponse.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                
                return Json(MHAResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(MHAResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                // MHAResponse = null;
                serviceCRM = null;
            }

        }

        public ActionResult ActivateMHASubscriberSearch()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ActivateMHASubscriberSearch(ActivateMHASubscriberRequest MHARequest)
        {
            ActivateMHASubscriberResponse MHAResponse = new ActivateMHASubscriberResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    MHARequest.CountryCode = clientSetting.countryCode;
                    MHARequest.BrandCode = clientSetting.brandCode;
                    MHARequest.LanguageCode = clientSetting.langCode;
                    MHARequest.userName = Convert.ToString(Session["UserName"]);

                    MHAResponse = serviceCRM.ActivateMHASubscriber(MHARequest);

                    if (MHAResponse != null && MHAResponse.responseDetails.ResponseCode != null && MHAResponse.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ActivateMHASubscriber_" + MHAResponse.responseDetails.ResponseCode);
                        MHAResponse.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? MHAResponse.responseDetails.ResponseDesc : errorInsertMsg;
                        MHAResponse.activateMHASubscriber.FindAll(a => a.requestDate != string.Empty).ForEach(b => b.requestDate = Utility.FormatDateTime(b.requestDate, clientSetting.mvnoSettings.dateTimeFormat));
                    }
                
                return Json(MHAResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(MHAResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //MHAResponse = null;
                serviceCRM = null;
            }

        }

        #endregion

        #region SearchSIM
        public ActionResult SearchSim()
        {
            CRM.Models.SendSim objReg = new CRM.Models.SendSim();
            try
            {
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("73,74,75,76,78", "1", "drop_master");
                return View(objReg);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View(objReg);
            }
            finally
            {
                // objReg = null;
            }

        }

        public JsonResult CRMSearchSim(string SearchSim)
        {
            SearchSimReq objReq = new SearchSimReq();
            SearchSimRes ObjRes = new SearchSimRes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - CRMSearchSim Start");
                objReq = JsonConvert.DeserializeObject<SearchSimReq>(SearchSim);
                objReq.BrandCode = clientSetting.brandCode;
                objReq.CountryCode = clientSetting.countryCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.MSISDN = Convert.ToString(Session["MobileNumber"]);//4920
                //string DOB = Utility.GetDateconvertion(objReq.FromDate, clientSetting.mvnoSettings.dateTimeFormat, false, "DD/MM/YYYY");
                //objReq.FromDate = DOB;
                //string DOB1 = Utility.GetDateconvertion(objReq.ToDate, clientSetting.mvnoSettings.dateTimeFormat, false, "DD/MM/YYYY");
                //objReq.ToDate = DOB1;


                string strGetDate = "", strDate = "", strMonth = "", strYear = "";
                string[] strSplit;

                #region Date Split
                if (objReq.FromDate != string.Empty)
                {
                    strGetDate = Utility.GetDateconvertion(objReq.FromDate, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
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
                    objReq.FromDate = strYear + "-" + strMonth + "-" + strDate;
                }
                if (objReq.ToDate != string.Empty)
                {
                    strGetDate = Utility.GetDateconvertion(objReq.ToDate, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
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
                    objReq.ToDate = strYear + "-" + strMonth + "-" + strDate;
                }
                #endregion

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMSearchSim(objReq);
                //6546
                if (clientSetting.preSettings.Enabledeliverystatusanddeliverytime.ToUpper() == "TRUE")
                {
                    if (ObjRes.OnlineSearchRecord.Count > 0)
                {
                    Parallel.ForEach(ObjRes.OnlineSearchRecord, (record, state, index) =>
                    {
                        string errorInserdtMsg = Resources.ErrorResources.ResourceManager.GetString("CRMSearchsimstatus_" + record.deliverystatusID);
                        ObjRes.OnlineSearchRecord[(int)index].deliverystatus = errorInserdtMsg;
                    });
                }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - CRMSearchSim - Conversion Start");
                try
                {
                    ObjRes.SearchSimDetailsList.Where(a => a.SubmitDate != string.Empty).ToList().ForEach(b => b.SubmitDate = Utility.FormatDateTime(b.SubmitDate, clientSetting.mvnoSettings.dateTimeFormat));
                    ObjRes.SearchSimDetailsList.Where(a => a.DOB != string.Empty).ToList().ForEach(b => b.DOB = Utility.FormatDateTime(b.DOB, clientSetting.mvnoSettings.dateTimeFormat));
                }
                catch (Exception expConversion)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, expConversion);
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - CRMSearchSim - Conversion Exception");
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - CRMSearchSim - Conversion End");
                if (objReq.Mode != "6")
                {
                if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMSearchSim_" + ObjRes.ResponseDetails.ResponseCode);
                    ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                }
                if(objReq.Mode=="6")
                {
                    var emailResponse = string.Empty;
                    var smsResponse = string.Empty;
                    string responseCode = string.Empty;
                    if (!string.IsNullOrEmpty(ObjRes.ResponseDetails.ResponseCode))
                     responseCode = ObjRes.ResponseDetails.ResponseCode;
                    var splitvalue = responseCode.Split(',');

                    for (int i = 0; i < splitvalue.Length; i++)
                    {
                        smsResponse += Resources.ErrorResources.ResourceManager.GetString("CRMSearchSimEmail_" + splitvalue[i]);
                        ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(smsResponse) ? ObjRes.ResponseDetails.ResponseDesc : smsResponse;

                    }

                }
                //ObjRes.SearchSimDetailsList.ForEach(a =>
                //{
                //    a.Date = Utility.GetDateconvertion(a.Date, "YYYY-MM-DD", false, clientSetting.mvnoSettings.dateTimeFormat);
                //    a.SimStatus = (a.SimStatus != null && a.SimStatus == "UnBlocked") ? Resources.SIMResources.UnBlocked : a.SimStatus;
                //    a.SimStatus = (a.SimStatus != null && a.SimStatus == "Blocked") ? Resources.SIMResources.Blocked : a.SimStatus;

                //});
                TempData["OnlineSearchRecord"] = ObjRes.OnlineSearchRecord;

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - CRMSearchSim End");
                return new JsonResult() { Data = ObjRes, MaxJsonLength = int.MaxValue };

            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return new JsonResult() { Data = ObjRes, MaxJsonLength = int.MaxValue };
            }
            finally
            {
                objReq = null;
                serviceCRM = null;
            }

            //return Json(ObjRes, JsonRequestBehavior.AllowGet);

        }

        public JsonResult CRMupdateSearchSim(string UpdateSearchSim)
        {
            SearchSimReq objReq = new SearchSimReq();
            SearchSimRes ObjRes = new SearchSimRes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objReq = JsonConvert.DeserializeObject<SearchSimReq>(UpdateSearchSim);
                objReq.BrandCode = clientSetting.brandCode;
                objReq.CountryCode = clientSetting.countryCode;
                objReq.LanguageCode = clientSetting.langCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMUpdateSearchSim(objReq);

                    try
                    {
                        ObjRes.SearchSimDetailsList.Where(a => a.SubmitDate != string.Empty).ToList().ForEach(b => b.SubmitDate = Utility.FormatDateTime(b.SubmitDate, clientSetting.mvnoSettings.dateTimeFormat));
                        ObjRes.SearchSimDetailsList.Where(a => a.DOB != string.Empty).ToList().ForEach(b => b.DOB = Utility.FormatDateTime(b.DOB, clientSetting.mvnoSettings.dateTimeFormat));
                    }
                    catch
                    {

                    }

                    if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMSearchSim_" + ObjRes.ResponseDetails.ResponseCode);
                        ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }

                    //ObjRes.SearchSimDetailsList.ForEach(a =>
                    //{
                    //    a.Date = Utility.GetDateconvertion(a.Date, "YYYY-MM-DD", false, clientSetting.mvnoSettings.dateTimeFormat);
                    //    a.SimStatus = (a.SimStatus != null && a.SimStatus == "UnBlocked") ? Resources.SIMResources.UnBlocked : a.SimStatus;
                    //    a.SimStatus = (a.SimStatus != null && a.SimStatus == "Blocked") ? Resources.SIMResources.Blocked : a.SimStatus;

                    //});
                
                return new JsonResult() { Data = ObjRes, MaxJsonLength = int.MaxValue };
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return new JsonResult() { Data = ObjRes, MaxJsonLength = int.MaxValue };
            }
            finally
            {
                objReq = null;
                // ObjRes = null;
                serviceCRM = null;
            }
            //return Json(ObjRes, JsonRequestBehavior.AllowGet);

        }
        #endregion

        [HttpPost]
        public void DownLoadMHASearch(string LMSISDNHistoryData)
        {
            try
            {
                GridView gridView = new GridView();
                //TopupFailure[] topupFailure = new JavaScriptSerializer().Deserialize<TopupFailure[]>(topupData);
                List<ReactiveHistoryRES> objMHA = JsonConvert.DeserializeObject<List<ReactiveHistoryRES>>(LMSISDNHistoryData);

                gridView.DataSource = objMHA;
                gridView.DataBind();

                Utility.ExportToExcell(gridView, "LMMSISDNHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
            }
            catch (Exception exp)
            {

                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
        }

        public ViewResult RetrySwapImsiUSA()
        {
            RetryUSASwapIMSIResponse objres = new RetryUSASwapIMSIResponse();
            RetryUSASwapIMSIRequest objreq = new RetryUSASwapIMSIRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - RetrySwapImsiUSA Start");
                objreq.mode = "Q";
                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.oldMSISDN = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.CRMRetryUSASwapIMSI(objreq);
                if (objres != null && objres.responseDetails != null && objres.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SubmitSwapIMSIUSA_" + objres.responseDetails.ResponseCode);
                    objres.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - RetrySwapImsiUSA End");
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

        [HttpPost]
        public ActionResult SubmitRetrySwapIMSI(string reqId)
        {
            RetryUSASwapIMSIRequest objreq = JsonConvert.DeserializeObject<RetryUSASwapIMSIRequest>(reqId);
            RetryUSASwapIMSIResponse objres = new RetryUSASwapIMSIResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - SubmitRetrySwapIMSI Start");
                objreq.userName = Session["UserName"].ToString();
                objreq.mode = "R";
                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.oldMSISDN = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.CRMRetryUSASwapIMSI(objreq);
                if (objres != null && objres.responseDetails != null && objres.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SubmitSwapIMSIUSA_" + objres.responseDetails.ResponseCode);
                    objres.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - SubmitRetrySwapIMSI End");
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
            return Json(objres);
        }

        public ViewResult PortInStatus()
        {
            return View();
        }

        [HttpPost]
        public JsonResult FetchPortInStatus(string PortInStatus)
        {
            PortinBelgiumRequest objreq = JsonConvert.DeserializeObject<PortinBelgiumRequest>(PortInStatus);
            PortinBelgiumResponse objRes = new PortinBelgiumResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;

               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.CRMPortinBelgium(objreq);

                    if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BELPortIn_" + objRes.responseDetails.ResponseCode);
                        objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
                    }

                    try
                    {
                        if (clientSetting.countryCode == "PRT" || clientSetting.countryCode == "IND")
                        {
                            
                        }
                        else if (clientSetting.countryCode != "BRA")
                        {
                            if (objRes != null && objRes.portInStatus != null && objRes.portInStatus.pDate != null && objRes.portInStatus.pDate != "")
                            {
                                objRes.portInStatus.pDate = Utility.GetDateconvertion(objRes.portInStatus.pDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                            }
                        }
                        else
                        {

                            if (objRes != null && objRes.portInStatus != null && objRes.portInStatus.pDate != null && objRes.portInStatus.pDate != "")
                            {
                                objRes.portInStatus.pDate = Utility.GetDateconvertion(objRes.portInStatus.pDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                            }
                            if (objRes != null && objRes.portInStatus != null && objRes.portInStatus.Dateofrequest != null && objRes.portInStatus.Dateofrequest != "")
                            {
                                objRes.portInStatus.Dateofrequest = Utility.GetDateconvertion(objRes.portInStatus.Dateofrequest, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                            }

                        }
                    }
                    catch
                    {
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
                objreq = null;
                serviceCRM = null;
                //objRes = null;
            }

        }

        public JsonResult GetFreeSimDeliveryCharge(FreeSimDeliveryChargeReq objFreeSimDeliveryChargeReq)
        {
            FreeSimDeliveryChargeRes objFreeSimDeliveryChargeRes = new FreeSimDeliveryChargeRes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objFreeSimDeliveryChargeReq.CountryCode = clientSetting.countryCode;
                objFreeSimDeliveryChargeReq.BrandCode = clientSetting.brandCode;
                objFreeSimDeliveryChargeReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objFreeSimDeliveryChargeRes = serviceCRM.GetFreeSimDeliveryCharge(objFreeSimDeliveryChargeReq);
                    if (objFreeSimDeliveryChargeRes != null && objFreeSimDeliveryChargeRes.Response != null && objFreeSimDeliveryChargeRes.Response.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BELPortIn_" + objFreeSimDeliveryChargeRes.Response.ResponseCode);
                        objFreeSimDeliveryChargeRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objFreeSimDeliveryChargeRes.Response.ResponseDesc : errorInsertMsg;
                    }
                
                return Json(objFreeSimDeliveryChargeRes);
            }
            catch
            {
                return Json(objFreeSimDeliveryChargeRes);
            }
            finally
            {
                //objFreeSimDeliveryChargeRes = null;
                serviceCRM = null;
            }
        }

        public ActionResult SwapCPF()
        {
            SwapCPFResponse objres = new SwapCPFResponse();
            SwapCPFRequest objreq = new SwapCPFRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;

            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - SwapCPF Start");
                objreq.Mode = "Query";
                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.SwapCPFBrazil(objreq);
                if (objres != null && objres.responseDetails != null && objres.responseDetails.ResponseCode != null)
                {
                    try
                    {
                        if (objres.SwapCpfHistory != null && objres.SwapCpfHistory.Count > 0)
                        {
                            objres.SwapCpfHistory.ForEach(b => b.Date = Utility.GetDateconvertion(b.Date, "yyyy/mm/dd", false, clientSetting.mvnoSettings.dateTimeFormat));
                        }
                    }
                    catch (Exception ex)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                    }
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SwapCPF_" + objres.responseDetails.ResponseCode);
                    objres.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.responseDetails.ResponseDesc : errorInsertMsg;
                }
                else
                {
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - SwapCPF - Unable to Fetch Response");
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - SwapCPF End");
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
                objreq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }

        }

        [HttpPost]
        public JsonResult SubmitSwapCPF(string strSwapRequest)
        {
            SwapCPFResponse objres = new SwapCPFResponse();
            SwapCPFRequest objreq = JsonConvert.DeserializeObject<SwapCPFRequest>(strSwapRequest);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - SubmitSwapCPF Start");
                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                objreq.UserName = Convert.ToString(Session["UserName"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.SwapCPFBrazil(objreq);
                if (objres != null && objres.responseDetails != null && objres.responseDetails.ResponseCode != null)
                {
                    try
                    {
                        if (objres.SwapCpfHistory != null && objres.SwapCpfHistory.Count > 0)
                        {
                            objres.SwapCpfHistory.ForEach(b => b.Date = Utility.GetDateconvertion(b.Date, "yyyy/mm/dd", false, clientSetting.mvnoSettings.dateTimeFormat));
                        }
                    }
                    catch (Exception ex)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                    }
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SwapCPF_" + objres.responseDetails.ResponseCode);
                    objres.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.responseDetails.ResponseDesc : errorInsertMsg;
                }
                else
                {
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - SubmitSwapCPF - Unable to Fetch Response");
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - SubmitSwapCPF End");
                return Json(objres);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(objres);
            }
            finally
            {
                serviceCRM = null;
                //objres = null;
                objreq = null;
                errorInsertMsg = string.Empty;
            }
        }

        public ActionResult Paymentenquiry()
        {

            return View();
        }

        public JsonResult crmpaymentenquiry(string TransID)
        {

            paymentenquiryResponse objResp = new paymentenquiryResponse();
            paymentenquiryRequest objReq = new paymentenquiryRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objReq.CountryCode = clientSetting.countryCode;
                    objReq.BrandCode = clientSetting.brandCode;
                    objReq.LanguageCode = clientSetting.langCode;
                    objReq.TransID = TransID;
                    objResp = serviceCRM.paymentenquiry(objReq);
                
                return Json(objResp);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objResp);
            }
            finally
            {
                objReq = null;
                serviceCRM = null;
                // objResp = null;
            }
        }

        public ActionResult SIMBlockUnblock()
        {
            CRM.Models.Registration objReg = new CRM.Models.Registration();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - ActionResult SIMBlockUnblock Start");
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("72", "1", "drop_master");
                objReg.strDropdown = objReg.strDropdown.OrderBy(x => x.Value).ToList();
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - ActionResult SIMBlockUnblock End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                //  objReg = null;
            }
            return View(objReg);
        }
        public JsonResult SIMBlockUnblockSearch(string Simsearch)
        {
            SIMBlockUnblockResponse objResp = new SIMBlockUnblockResponse();
            SIMBlockUnblockRequest objReq = new SIMBlockUnblockRequest();
            string strGetDate = "", strDate = "", strMonth = "", strYear = "";
            string[] strSplit;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - SIMBlockUnblockSearch Start");
                objReq = JsonConvert.DeserializeObject<SIMBlockUnblockRequest>(Simsearch);
                //string DOB = Utility.GetDateconvertion(objReq.Fromdate, clientSetting.mvnoSettings.dateTimeFormat, false, "DD/MM/YYYY");
                //objReq.Fromdate = DOB;
                //string DOB1 = Utility.GetDateconvertion(objReq.ToDate, clientSetting.mvnoSettings.dateTimeFormat, false, "DD/MM/YYYY");
                // objReq.ToDate = DOB1;             
                #region Date Split
                if (objReq.Fromdate != string.Empty)
                {
                    strGetDate = Utility.GetDateconvertion(objReq.Fromdate, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
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
                    objReq.Fromdate = strYear + "." + strMonth + "." + strDate;
                }
                if (objReq.ToDate != string.Empty)
                {
                    strGetDate = Utility.GetDateconvertion(objReq.ToDate, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
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
                    objReq.ToDate = strYear + "." + strMonth + "." + strDate;
                }
                #endregion
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objResp = serviceCRM.SIMBlockUnblock(objReq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - SIMBlockUnblockSearch End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objReq = null;
                strGetDate = string.Empty;
                strDate = string.Empty;
                strMonth = string.Empty;
                strYear = string.Empty;
                serviceCRM = null;
            }
            return Json(objResp);
        }

        #region SwapIMSISearch
        public ActionResult CRMswapimsisearch()
        {
            CRM.Models.Registration objReg = new CRM.Models.Registration();
            try
            {
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("70", "1", "drop_master");
                objReg.strDropdown = objReg.strDropdown.OrderBy(x => x.Value).ToList();
                return View(objReg);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View(objReg);
            }
            finally
            {
                //objReg = null;
            }
        }
        public JsonResult swapimsisearch(string imsisearch)
        {
            swapimsisearchreq objReq = new swapimsisearchreq();
            swapimsisearchres ObjRes = new swapimsisearchres();
            ServiceInvokeCRM serviceCRM;
            string DOB = string.Empty;
            string DOB1 = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - swapimsisearch Start");
                objReq = JsonConvert.DeserializeObject<swapimsisearchreq>(imsisearch);
                objReq.BrandCode = clientSetting.brandCode;
                objReq.CountryCode = clientSetting.countryCode;
                objReq.LanguageCode = clientSetting.langCode;
                DOB = Utility.GetDateconvertion(objReq.fromdate, clientSetting.mvnoSettings.dateTimeFormat, false, "YYYY/MM/DD");
                objReq.fromdate = DOB;
                DOB1 = Utility.GetDateconvertion(objReq.todate, clientSetting.mvnoSettings.dateTimeFormat, false, "YYYY/MM/DD");
                objReq.todate = DOB1;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMswapimsisearch(objReq);
                try
                {
                    ObjRes.SearchSimDetailsList.Where(a => a.REQUESTDATE != string.Empty).ToList().ForEach(b => b.REQUESTDATE = Utility.FormatDateTime(b.REQUESTDATE, clientSetting.mvnoSettings.dateTimeFormat));

                }
                catch
                {

                }
                if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMSwapIMSISearch_" + ObjRes.ResponseDetails.ResponseCode);
                    ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }

                //ObjRes.SearchSimDetailsList.ForEach(a =>
                //{
                //    a.Date = Utility.GetDateconvertion(a.Date, "YYYY-MM-DD", false, clientSetting.mvnoSettings.dateTimeFormat);
                //    a.SimStatus = (a.SimStatus != null && a.SimStatus == "UnBlocked") ? Resources.SIMResources.UnBlocked : a.SimStatus;
                //    a.SimStatus = (a.SimStatus != null && a.SimStatus == "Blocked") ? Resources.SIMResources.Blocked : a.SimStatus;

                //});
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - swapimsisearch End");
                return new JsonResult() { Data = ObjRes, MaxJsonLength = int.MaxValue };
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return new JsonResult() { Data = ObjRes, MaxJsonLength = int.MaxValue };
            }
            finally
            {
                objReq = null;
                serviceCRM = null;
                // ObjRes = null;
            }

        }
        #endregion

        #region SwapMSISDNSearch
        public ActionResult CRMswapmsisdnsearch()
        {
            CRM.Models.Registration objReg = new CRM.Models.Registration();
            try
            {
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("71", "1", "drop_master");
                objReg.strDropdown = objReg.strDropdown.OrderBy(x => x.Value).ToList();
                return View(objReg);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View(objReg);
            }
            finally
            {
                //  objReg = null;
            }
        }
        public JsonResult swapmsisdnsearch(string msidnsearch)
        {
            swapmsisdnsearchreq objReq = new swapmsisdnsearchreq();
            swapmsisdnsearchres ObjRes = new swapmsisdnsearchres();
            ServiceInvokeCRM serviceCRM;
            string DOB = string.Empty;
            string DOB1 = string.Empty;
            try
            {
                objReq = JsonConvert.DeserializeObject<swapmsisdnsearchreq>(msidnsearch);
                objReq.BrandCode = clientSetting.brandCode;
                objReq.CountryCode = clientSetting.countryCode;
                objReq.LanguageCode = clientSetting.langCode;
                DOB = Utility.GetDateconvertion(objReq.fromdate, clientSetting.mvnoSettings.dateTimeFormat, false, "DD/MM/YYYY");
                objReq.fromdate = DOB;
                DOB1 = Utility.GetDateconvertion(objReq.todate, clientSetting.mvnoSettings.dateTimeFormat, false, "DD/MM/YYYY");
                objReq.todate = DOB1;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - ViewSubscriberOTP Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMswapmsisdnsearch(objReq);
                try
                {
                    ObjRes.SwapMSISDNDetailsList.Where(a => a.REQUESTDATE != string.Empty).ToList().ForEach(b => b.REQUESTDATE = Utility.FormatDateTime(b.REQUESTDATE, clientSetting.mvnoSettings.dateTimeFormat));

                }
                catch (Exception ex)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                }
                if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMSwapMSISDNSearch_" + ObjRes.ResponseDetails.ResponseCode);
                    ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - ViewSubscriberOTP End");


            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }

            finally
            {
                objReq = null;
                serviceCRM = null;
                DOB = string.Empty;
                DOB1 = string.Empty;
            }
            return new JsonResult() { Data = ObjRes, MaxJsonLength = int.MaxValue };

        }
        #endregion

        public ActionResult PortInSearch()
        {
            return View();
        }

        [HttpPost]
        public JsonResult PortcheckInSearch(string PortInStatus)
        {
            PortinItalyRequest objreq = JsonConvert.DeserializeObject<PortinItalyRequest>(PortInStatus);
            PortinItalyResponse objRes = new PortinItalyResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;

               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.CRMPortinItaly(objreq);
                
                try
                {
                    objRes.portInItalyStatus.REQUESTDATE = Utility.GetDateconvertion(objRes.portInItalyStatus.REQUESTDATE, "dd/mm/yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                    objRes.portInItalyStatus.PORTINDATE = Utility.GetDateconvertion(objRes.portInItalyStatus.PORTINDATE, "dd/mm/yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                }
                catch
                {

                }
                return new JsonResult() { Data = objRes, MaxJsonLength = int.MaxValue };
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return new JsonResult() { Data = objRes, MaxJsonLength = int.MaxValue };
            }
            finally
            {
                objreq = null;
                // objRes = null;
                serviceCRM = null;
            }
            // return Json(objRes);

        }

        public ActionResult PortOutSearch()
        {
            return View();
        }

        public ActionResult OnlineReport()
        {
            TopupStatusResponse objres = new TopupStatusResponse();
            CRMBase objreq = new CRMBase();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;

              serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objres = serviceCRM.CRMGetTopupStatus(objreq);
                    ViewBag.Mode = Utility.GetDropdownMasterFromDB("79", "1", "drop_master");
                    ViewBag.ServiceType = Utility.GetDropdownMasterFromDB("80", "1", "drop_master");
                    ViewBag.Status = objres.TopupStatus;
                    ViewBag.BundleStatus = objres.BundleStatus;
                
                return View(objres);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View(objres);
            }
            finally
            {
                // objres = null;
                objreq = null;
                serviceCRM = null;
            }
        }


        [HttpPost]
        public JsonResult PortcheckOutSearch(string PortInStatus)
        {
            PortinItalyRequest objreq = JsonConvert.DeserializeObject<PortinItalyRequest>(PortInStatus);
            PortinItalyResponse objRes = new PortinItalyResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;

               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.CRMPortinItaly(objreq);
                
                try
                {
                    objRes.portInItalyStatus.REQUESTDATE = Utility.GetDateconvertion(objRes.portInItalyStatus.REQUESTDATE, "dd/mm/yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                    objRes.portInItalyStatus.PORTINDATE = Utility.GetDateconvertion(objRes.portInItalyStatus.PORTINDATE, "dd/mm/yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);

                }
                catch
                {

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
                objreq = null;
                // objRes = null;
                serviceCRM = null;
            }
        }


        [HttpPost]
        public JsonResult OnlinePaymentsearch(string OnlinePayment)
        {

            OnlineSearchRequest objreq = JsonConvert.DeserializeObject<OnlineSearchRequest>(OnlinePayment);
            OnlineSearchResponse objRes = new OnlineSearchResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;

                if (objreq.Type != "UPDATE")
                {
                    objreq.fromdate = Utility.GetDateconvertion(objreq.fromdate, "YYYY-MM-DD", true, clientSetting.mvnoSettings.dateTimeFormat);
                    objreq.todate = Utility.GetDateconvertion(objreq.todate, "YYYY-MM-DD", true, clientSetting.mvnoSettings.dateTimeFormat);
                    objreq.fromdate = objreq.fromdate + " " + objreq.fromtime;
                    objreq.todate = objreq.todate + " " + objreq.totime;
                }
            serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.GetOnlineSearchReport(objreq);
                

                objRes.OnlineSearchRecord.ForEach(a =>
                {
                    a.RequestDate = Utility.GetDateconvertion(a.RequestDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                    a.ProcessedDate = Utility.GetDateconvertion(a.ProcessedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                });

                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {

                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMOnlineReport_" + objRes.responseDetails.ResponseCode);
                    objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
                }
                //6232
                if (objRes != null && objRes.OnlineSearchRecord.Count > 0)
                {
                    Parallel.ForEach(objRes.OnlineSearchRecord, (record, state, index) =>
                    {
                        string errorInserdtMsg = Resources.ErrorResources.ResourceManager.GetString("CRMOnlineReport1_" + record.EshopErrorcode);
                        if (!string.IsNullOrEmpty(errorInserdtMsg))
                        {
                            objRes.OnlineSearchRecord[(int)index].EshopErrDesc = errorInserdtMsg;
                        }
                    });
                }
                return new JsonResult() { Data = objRes, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return new JsonResult() { Data = objRes, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            finally
            {
                // objRes = null;
                objreq = null;
                serviceCRM = null;
            }
            // return Json(objRes);           

        }


        public List<PortInLang> PortInPrefLang(CRMBase crmBaseReq)
        {
            IVRLanguageResponse ObjRes = new IVRLanguageResponse();
            PortInLang objlang = null;
            List<PortInLang> lang = new List<PortInLang>();
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMGetIVRLanguage(crmBaseReq);
                

                if (ObjRes.IVRLanguage.Count > 0)
                {
                    foreach (IVRLanguage str in ObjRes.IVRLanguage)
                    {
                        objlang = new PortInLang();
                        objlang.langId = Convert.ToString(str.LanguageId);
                        objlang.langDesc = str.Language;
                        lang.Add(objlang);
                    }
                }
                return lang;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return lang;
            }
            finally
            {
                // ObjRes = null;
                serviceCRM = null;
                objlang = null;
                lang = null;
            }
        }

        #region PACNPAC 4203 and 4373
        public ActionResult CreatePACNPAC(string Textdata)
        {
            NPACPACCreationResponse portres = new NPACPACCreationResponse();
            CRMBase crmBase = new CRMBase();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - CreatePACNPAC Start");
                crmBase.CountryCode = clientSetting.countryCode;
                crmBase.BrandCode = clientSetting.brandCode;
                crmBase.LanguageCode = clientSetting.langCode;
                portres.TextData = Textdata;
                portres.portInPrefLang = PortInPrefLang(crmBase);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - CreatePACNPAC End");
                return View(portres);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View(portres);
            }
            finally
            {
                //  portres = null;
                crmBase = null;
            }
        }
        [HttpPost]
        public JsonResult CreatePACNPACcheck(string PortInStatus)
        {
            NPACPACCreationRequest objreq = JsonConvert.DeserializeObject<NPACPACCreationRequest>(PortInStatus);
            NPACPACCreationResponse objRes = new NPACPACCreationResponse();
            string expirydate = string.Empty;
            string createddate = string.Empty;
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - CreatePACNPACcheck Start");
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;

                if(clientSetting.preSettings.EnableCRMMultiTab.ToUpper() != "TRUE")
                {
                    objreq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    objreq.ICCID = Convert.ToString(Session["ICCID"]);
                }
                else
                {
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    objreq.MSISDN = localDict.Where(x => objreq.Textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    objreq.ICCID = localDict.Where(x => objreq.Textdata.ToString().Contains(x.Key)).Select(x => x.Key).First().ToString();
                    
                }


                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.PACCreation(objreq);

                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PACCreation_" + objRes.responseDetails.ResponseCode);
                    objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
                }
                


                try
                {
                    objRes.NPACPACCreation.CREATEPACCREATEDDATE = Utility.GetDateconvertion(objRes.NPACPACCreation.CREATEPACCREATEDDATE, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);

                    objRes.NPACPACCreation.CREATEPACEXPIRYDATE = Utility.GetDateconvertion(objRes.NPACPACCreation.CREATEPACEXPIRYDATE, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                }
                catch (Exception ex)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - CreatePACNPACcheck End");
                return new JsonResult() { Data = objRes, MaxJsonLength = int.MaxValue };
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return new JsonResult() { Data = objRes, MaxJsonLength = int.MaxValue };
            }
            finally
            {
                // objRes = null;
                objreq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
                expirydate = string.Empty;
                createddate = string.Empty;
            }
            // return Json(objRes);            
        }
        public ActionResult CancelPACNPAC(string Textdata)
        {
            NPACPACCreationRequest objreq = new NPACPACCreationRequest();
            NPACPACCreationResponse objRes = new NPACPACCreationResponse();
            string expirydate = "";
            string createddate = "";
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;

            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - CancelPACNPAC Start");
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;

                #region FRR 4925
                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                    Session["RealICCIDForMultiTab"] = Textdata;
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    objreq.MSISDN = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                }
                else
                {
                    objreq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                }
                #endregion

                objreq.mode = "RETRIVE";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);


                objRes = serviceCRM.PACCreation(objreq);

                try
                {

                    if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PACCancel_" + objRes.responseDetails.ResponseCode);
                        objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                    


                    if (!string.IsNullOrEmpty(objRes.NPACPACCreation.CANCELEXPIRYDATE))
                    {
                        ViewBag.CANCELEXPIRYDATE = objRes.NPACPACCreation.CANCELEXPIRYDATE;
                    }
                    else
                    {
                        objRes.NPACPACCreation.CANCELEXPIRYDATE = string.Empty;
                    }


                    if (!string.IsNullOrEmpty(objRes.NPACPACCreation.CANCELPACNPACCODE))
                    {
                        ViewBag.CANCELPACNPACCODE = objRes.NPACPACCreation.CANCELPACNPACCODE;
                    }
                    else
                    {
                        objRes.NPACPACCreation.CANCELPACNPACCODE = string.Empty;
                    }

                    objRes.NPACPACCreation.CANCELCREATEDDATE = Utility.GetDateconvertion(objRes.NPACPACCreation.CANCELCREATEDDATE, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                    objRes.NPACPACCreation.CANCELEXPIRYDATE = Utility.GetDateconvertion(objRes.NPACPACCreation.CANCELEXPIRYDATE, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                }
                catch (Exception ex)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - CancelPACNPAC End");
                return View(objRes);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(objRes);
            }
            finally
            {
                // objRes = null;
                objreq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
                expirydate = string.Empty;
                createddate = string.Empty;
            }
        }
        #endregion

        #region managesubcriberdetails 4238
        public ActionResult managesubcriberdetails()
        {
            CRMsubcriberdetailsResp objresponse = new CRMsubcriberdetailsResp();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - managesubcriberdetails Start");
                objresponse.strDropdown = Utility.GetDropdownMasterFromDB("80", "1", "drop_master");
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - managesubcriberdetails End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                //objresponse = null;
            }
            return View(objresponse);
        }
        [HttpPost]
        public JsonResult subcriberdetails(string subcriberMSISDN)
        {
            CRMsubcriberdetailsRequest objreq = JsonConvert.DeserializeObject<CRMsubcriberdetailsRequest>(subcriberMSISDN);
            CRMsubcriberdetailsResponse objRes = new CRMsubcriberdetailsResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - subcriberdetails Start");
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                //  objreq.MSISDN = subcriberMSISDN;
                //  objreq.CSAGENTUSERNAME = Convert.ToString(Session["UserName"]);
                // objreq.iccid = Convert.ToString(Session["ICCID"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.CRMmanagesubcriberdetails(objreq);
                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManageSubcriberDetails_" + objRes.responseDetails.ResponseCode);
                    objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - subcriberdetails End");
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
            return new JsonResult() { Data = objRes, MaxJsonLength = int.MaxValue };
        }
        [HttpPost]
        public JsonResult updatesubcriberdetails(string updatecustomeremail)
        {
            CRMsubcriberdetailsRequest objreq = JsonConvert.DeserializeObject<CRMsubcriberdetailsRequest>(updatecustomeremail);
            CRMsubcriberdetailsResponse objRes = new CRMsubcriberdetailsResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - updatesubcriberdetails Start");
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.CRMmanagesubcriberdetails(objreq);
                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManageSubcriberDetails_" + objRes.responseDetails.ResponseCode);
                    objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SimController - updatesubcriberdetails End");
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
            return new JsonResult() { Data = objRes, MaxJsonLength = int.MaxValue };
        }
        #endregion

        #region PORTIN SWEDEN
        public ActionResult checkportinstatussweden()
        {

            return View();
        }
        #endregion

        #region FRR-4283 PinReactivation
        public ActionResult PinReactivation()
        {

            return View();
        }

        [HttpPost]
        public JsonResult CRMPin(string PinReactivation)
        {
            CRMPINdetailsRequest objreq = JsonConvert.DeserializeObject<CRMPINdetailsRequest>(PinReactivation);
            CRMPINdetailsResponse objRes = new CRMPINdetailsResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - CRMPin Start");
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.CRMPinReactivation(objreq);
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
            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - CRMPin End");
            return Json(objRes);
        }

        #endregion


        #region FRR-4283 PinRedemption
        public ActionResult PinRedemption()
        {

            return View();
        }

        [HttpPost]
        public JsonResult CRMPinRedemption(string PinRedemption)
        {
            CRMPINdetailsRequest objreq = JsonConvert.DeserializeObject<CRMPINdetailsRequest>(PinRedemption);
            CRMPINdetailsResponse objRes = new CRMPINdetailsResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - CRMPinRedemption Start");
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.CRMPinRedemption(objreq);
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
            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - CRMPinRedemption End");
            return Json(objRes);
        }


        #endregion


        #region FRR-4345 SEARCH SIM DOWNLOAD
        [HttpPost]
        public void DownLoadSearchSIM(string NominateDetails)
        {
            GridView gridView = new GridView();
            XmlDocument XMLSer = new XmlDocument();
            string[] strList;
         //   List<OnlineSearchRecord> onlinesearchrecordss;
            StringReader reader;
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string colNames = string.Empty;
            try
            {
                List<OnlineSearchRecord> onlinesearchrecords = JsonConvert.DeserializeObject<List<OnlineSearchRecord>>(NominateDetails);

                XMLSer = Common.CreateXML(onlinesearchrecords);
                reader = new StringReader(XMLSer.InnerXml);
                ds = new DataSet();
                ds.ReadXml(reader);
                dt = new DataTable();
                dt = ds.Tables[0];
                colNames = String.Empty;
                foreach (DataColumn dc in dt.Columns)
                {
                    try
                    {
                        if (dc.ColumnName == "MSISDN")
                        {
                            colNames = Resources.RegistrationResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
                            if (colNames != string.Empty && colNames != null)
                                dt.Columns[dc.ColumnName].ColumnName = colNames;
                        }
                        else
                        {
                            colNames = Resources.SIMResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
                            if (colNames != string.Empty && colNames != null)
                                dt.Columns[dc.ColumnName].ColumnName = colNames;
                        }
                    }
                    catch (Exception ex)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                    }
                }
                dt.AcceptChanges();
                gridView.DataSource = dt;
                //gridView.DataSource = TempData["OnlineSearchRecord"];
                //TempData.Peek("OnlineSearchRecord");
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "SearchSIM_" + DateTime.Now.ToString("yyyy-MM-ddHHmmss"), this.HttpContext.Response);
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

        #region 4368 OBAReport
        public ActionResult OBAReport()
        {

            return View();
        }
        public JsonResult LoadOBAReport(string OBAReport)
        {
            OBAReportResponse ObjResp = new OBAReportResponse();
            OBAReportRequest ObjReq = JsonConvert.DeserializeObject<OBAReportRequest>(OBAReport);
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

                ObjResp = crmNewService.CRMOBAReport(ObjReq);

                //FRR--3083
                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LoadFailureHistory_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }


                TempData["LoadOBAHistory"] = ObjResp.OBAReportStatus;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        [HttpPost]
        public void DownLoadOBAHistory(string OBAData)
        {
            GridView gridView = new GridView();
            try
            {
                gridView.DataSource = TempData["LoadOBAHistory"];
                TempData.Keep("LoadOBAHistory");
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "CRMOBAReport_" + DateTime.Now.ToString("yyyy-MM-ddHHmmss"), this.HttpContext.Response);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
        }
        #endregion


        #region 4772 altan
        public ActionResult AltanGetDeviceInfo()
        {

            return View();
        }


        public JsonResult GetAltanDeviceInfo(string GetAltanDeviceInfoDetails)
        {
            AltanGetDeviceinfoResponse ObjResp = new AltanGetDeviceinfoResponse();
            AltanGetDeviceinfoRequest ObjReq = JsonConvert.DeserializeObject<AltanGetDeviceinfoRequest>(GetAltanDeviceInfoDetails);
            string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode; 

                ObjResp = crmNewService.CRMAltanGetinfo(ObjReq);

                //FRR--3083
                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LoadFailureHistory_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
               
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        #endregion


        //------FRR--------3750------------s.subha-----------1.1.9.0
        #region ----FRR----3750------Swap MSISDN Rollback

        public ActionResult MsisdnSwapRollBack()
        {
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SIM_MsisdnSwapRollBack").ToList();
            return View();
        }

        public JsonResult GETCRMMsisdnSwapRollBack(string MsisdnSwapRollBackDetails)
        {
            MsisdnSwapRollBackResponse ObjRes = new MsisdnSwapRollBackResponse();
            MsisdnSwapRollBackRequest objbundleReq = new JavaScriptSerializer().Deserialize<MsisdnSwapRollBackRequest>(MsisdnSwapRollBackDetails);
            ServiceInvokeCRM serviceCRM;
            try
            {
                objbundleReq.CountryCode = clientSetting.countryCode;
                objbundleReq.BrandCode = clientSetting.brandCode;
                objbundleReq.LanguageCode = clientSetting.langCode;
                objbundleReq.RequestBy = Session["UserName"].ToString();
                //if (!string.IsNullOrEmpty(objbundleReq.MSISDN))
                //{
                //    objbundleReq.MSISDN = objbundleReq.MSISDN;
                //}
                //else
                //{
                //    objbundleReq.MSISDN = Session["MobileNumber"].ToString();
                //}
               serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMMsisdnSwapRollBack(objbundleReq);

                    if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMMsisdnSwapRollBack_" + ObjRes.ResponseDetails.ResponseCode);
                        ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
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
                objbundleReq = null;
                serviceCRM = null;
            }

        }
        #endregion



        //--------------FRR------4357-----------S.subha------------1.1.9.0
        #region ----FRR----4357------CRMMIGRATESIMSERVICES

        public ActionResult SIMMigrateServices()
        {
            GetMigrateSimServicesRes objres = new GetMigrateSimServicesRes();
            GetMigrateSimServicesReq objreq = new GetMigrateSimServicesReq();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - SIMMigrateServices Start");
                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.OLDMSISDN = Convert.ToString(Session["MobileNumber"]); //18855770000
                objreq.OLDICCID = Convert.ToString(Session["ICCID"]);         //8654596000000000146
                objreq.MODE = "REPORT";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.CRMMIGRATESIMSERVICES(objreq);
                objreq.OLDMSISDN = Convert.ToString(Session["MobileNumber"]);
                objreq.OLDICCID = Convert.ToString(Session["ICCID"]);
                ViewBag.OLDMSISDN = objreq.OLDMSISDN;
                ViewBag.OLDICCID = objreq.OLDICCID;
                ViewBag.GO_ONLINE_SIM = Convert.ToString(Session["SIM_CATEGORY"]);
                if (objres != null && objres.ResponseDetails != null && objres.ResponseDetails.ResponseCode != null)
                {

                    //Waiting for approval
                    try
                    {
                        if (objres.MigrateSimServiceReportList != null && objres.MigrateSimServiceReportList.Count > 0)
                        {
                            if (objres.MigrateSimServiceReportList.Any(a => a.STATUS == "Waiting for approval"))
                            {
                                objres.ResponseDetails.ResponseCode = "100";
                            }
                            objres.MigrateSimServiceReportList.ForEach(b => b.SUBMITTEDDATE = Utility.GetDateconvertion(b.SUBMITTEDDATE, "dd/mm/yyyy", false, clientSetting.mvnoSettings.dateTimeFormat));
                            objres.MigrateSimServiceReportList.ForEach(b => b.APROVEDATE = Utility.GetDateconvertion(b.APROVEDATE, "dd/mm/yyyy", false, clientSetting.mvnoSettings.dateTimeFormat));
                            objres.MigrateSimServiceReportList.ForEach(b => b.SIMType = (b.SIMType == "1" ? Resources.SIMResources.MigrationSimToStandard : Resources.SIMResources.MigrationSimToGoOnline));
                        }
                    }
                    catch (Exception ex)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                    }
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SwapCPF_" + objres.ResponseDetails.ResponseCode);
                    objres.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                else
                {
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - SIMMigrateServices - Unable to Fetch Response");
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - SIMMigrateServices End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SIMController - eXSIMMigrateServices - " + this.ControllerContext, eX);
            }
            finally
            {
                objreq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return View(objres);
        }

        public JsonResult GETCRMMIGRATESIMSERVICES(string SIMMigrateDatails)
        {
            GetMigrateSimServicesRes ObjRes = new GetMigrateSimServicesRes();
            GetMigrateSimServicesReq objbundleReq = new JavaScriptSerializer().Deserialize<GetMigrateSimServicesReq>(SIMMigrateDatails);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - GETCRMMIGRATESIMSERVICES Start");
                objbundleReq.CountryCode = clientSetting.countryCode;
                objbundleReq.BrandCode = clientSetting.brandCode;
                objbundleReq.LanguageCode = clientSetting.langCode;
                objbundleReq.REQUESTEDBY = Session["UserName"].ToString();
                objbundleReq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                objbundleReq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                objbundleReq.TYPE = Convert.ToString(Session["SIM_CATEGORY"]);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMMIGRATESIMSERVICES(objbundleReq);
                if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.SIMResources.ResourceManager.GetString("CRMMIGRATESIMSERVICES_" + ObjRes.ResponseDetails.ResponseCode);
                    ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - GETCRMMIGRATESIMSERVICES End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SIMController - exception-SIMMigrateServices - " + this.ControllerContext, ex);
            }
            finally
            {
                objbundleReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjRes, JsonRequestBehavior.AllowGet);
        }
        #endregion


        //--------------FRR------4438-----------S.subha------------1.1.11.0
        #region *************** SHIPPING DETAILS****UPLOAD***GET***EDIT***DELETE***
        public ActionResult UploadShippingIDdetail()
        {
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "SIM_UploadShippingIDdetail").ToList();
            return View();
        }
        public JsonResult CRMGetShippingIDReport(string ShippingIDDetails)
        {

            ShippingIDResponse ObjResp = new ShippingIDResponse();
            ShippingIDRequest ObjReq = JsonConvert.DeserializeObject<ShippingIDRequest>(ShippingIDDetails);
            string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            ServiceInvokeCRM serviceCRM;
            string strGetDate = "", strDate = "", strMonth = "", strYear = "";
            string[] strSplit;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
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

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjResp = serviceCRM.CRMGetShippingIDReport(ObjReq);
                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LoadFailureHistory_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }

                ObjResp.ShippingIDStatus.ForEach(a =>
                {
                    a.SubmittedDate = Utility.GetDateconvertion(a.SubmittedDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                });
                TempData["ShippingDetailsRecord"] = ObjResp.ShippingIDStatus;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public DataTable ReadCsvFile(string strFilePath)
        {
            DataTable dtCsv = new DataTable();
            string Fulltext=string.Empty;
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
                                    dtCsv.Columns.Add(rowValues[j].Replace("\r",string.Empty)); //add headers  
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
        public JsonResult ShippingUploadFile(HttpPostedFileBase file)
        {
            ShippingIDResponse ObjResp = new ShippingIDResponse();
            ShippingIDRequest ObjReq = new ShippingIDRequest();
            ObjResp.responseDetails = new CRMResponse();
            //  DataSet ds = new DataSet();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            DataTable dt = new DataTable();
            ObjReq.ShippingTable = new DataSet();
            DataTable dtcopy = new DataTable();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - GETCRMMIGRATESIMSERVICES Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.RequestBy = Session["UserName"].ToString();
                ObjReq.Mode = "UPLOAD";

                if (file.ContentLength == 0)
                {
                    ObjResp.responseDetails.ResponseCode = "101";
                    ObjResp.responseDetails.ResponseDesc = "File Should Be Empty!Upload Sample Excel File!";
                    return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                }
                string extension = System.IO.Path.GetExtension(file.FileName).ToLower();
                string[] validFileTypes = { ".csv" };
                string path1 = string.Format("{0}/{1}", Server.MapPath("~/App_Data/TemplateShippingDetails"), file.FileName);
                if (!Directory.Exists(path1))
                {
                    Directory.CreateDirectory(Server.MapPath("~/App_Data/TemplateShippingDetails"));
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
                    
                    if (dt.Columns.Count == 2)
                    {
                        string[] validHeaderTypes = { "SHIPPINGID", "ORDERID" };
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
                    }
                    else
                    {
                        ObjResp.responseDetails.ResponseCode = "103";
                        ObjResp.responseDetails.ResponseDesc = "Excel sheet Must have Two Headers!";
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
                        else if (dt.Rows.Count > Int64.Parse(clientSetting.mvnoSettings.UploadShippingIDRowCount))
                        {
                            ObjResp.responseDetails.ResponseCode = "105";
                            ObjResp.responseDetails.ResponseDesc = "Maximum Row Exceeds, Please upload max of " + clientSetting.mvnoSettings.UploadShippingIDRowCount + " Rows";
                            return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            if (dt.AsEnumerable().Where(r => r["SHIPPINGID"].ToString() == string.Empty).Any())
                            {
                                ObjResp.responseDetails.ResponseCode = "105";
                                ObjResp.responseDetails.ResponseDesc = "Shipping Id column is empty";
                                return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                            }

                            if (dt.AsEnumerable().Where(r => r["ORDERID"].ToString() == string.Empty).Any())
                            {
                                ObjResp.responseDetails.ResponseCode = "106";
                                ObjResp.responseDetails.ResponseDesc = "Order Id column is empty";
                                return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                            }

                            //var duplicatesSHIPPINGID = dt.AsEnumerable().GroupBy(r => r[0]).Where(gr => gr.Count() > 1).ToList();
                            //if (duplicatesSHIPPINGID.Count > 0)
                            //{
                            //    ObjResp.responseDetails.ResponseCode = "107";
                            //    ObjResp.responseDetails.ResponseDesc = "Duplicate record found in Shipping Id";
                            //    return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                            //}
                            var duplicatesOrderId = dt.AsEnumerable().GroupBy(r => r[1]).Where(gr => gr.Count() > 1).ToList();
                            if (duplicatesOrderId.Count > 0)
                            {
                                ObjResp.responseDetails.ResponseCode = "108";
                                ObjResp.responseDetails.ResponseDesc = "Duplicate record found in Order Id";
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
                       string ORDERID = row["ORDERID"].ToString();
                        string SHIPPINGID  = row["SHIPPINGID"].ToString();
                        if(!string.IsNullOrEmpty(ORDERID))
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
                            ObjResp.responseDetails.ResponseDesc = "ORDERID Values Is Empty!";
                            return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                        }
                        if (!string.IsNullOrEmpty(SHIPPINGID))
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
                            ObjResp.responseDetails.ResponseDesc = "SHIPPINGID Values Is Empty!";
                            return this.Json(ObjResp, JsonRequestBehavior.AllowGet);
                        }
                        break;
                    }
                    dt.TableName = "SHIPPINGDETAILS";
                    dtcopy = dt.Copy();
                    //ds.Tables.Add(dt);
                    // ObjReq.ShippingTable = ds;
                    ObjReq.ShippingTable.Tables.Add(dtcopy);
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    ObjResp = serviceCRM.CRMGetShippingIDReport(ObjReq);
                    if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ShippingIDErrUpload_" + ObjResp.responseDetails.ResponseCode);
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
                ObjReq.ShippingTable = null;
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public ActionResult DownloadShippingTemplate()
        {
            string filename = "ShippingDetails.csv";
            string fullpath = "";
            try
            {
                fullpath = Path.Combine(this.Server.MapPath("~/App_Data/SampleExcelFormat/ShippingDetails.csv"));
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
        [HttpPost]
        public void DownLoadShippingDetailsReport(string ShippingDetails)
        {
            GridView gridView = new GridView();
            try
            {
                gridView.DataSource = TempData["ShippingDetailsRecord"];
                TempData.Keep("ShippingDetailsRecord");
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "ShippingDetails_" + DateTime.Now.ToString("yyyy-MM-ddHHmmss"), this.HttpContext.Response);
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


        public JsonResult CRMGetMinimumDeviceDetails(string strGetMinimumDeviceDetails)
        {
            CRMMinimumDeviceDetailsRequest objReq = new CRMMinimumDeviceDetailsRequest();
            CRMMinimumDeviceDetailsResponse ObjRes = new CRMMinimumDeviceDetailsResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objReq = JsonConvert.DeserializeObject<CRMMinimumDeviceDetailsRequest>(strGetMinimumDeviceDetails);
                objReq.BrandCode = clientSetting.brandCode;
                objReq.CountryCode = clientSetting.countryCode;
                objReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.Get_Minimum_Device_Information(objReq);
                    //    ObjRes.SearchSimDetailsList.Where(a => a.SubmitDate != string.Empty).ToList().ForEach(b => b.SubmitDate = Utility.FormatDateTime(b.SubmitDate, clientSetting.mvnoSettings.dateTimeFormat));
                    if (ObjRes != null && ObjRes.Response != null && ObjRes.Response.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMSearchSim_" + ObjRes.Response.ResponseCode);
                        ObjRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.Response.ResponseDesc : errorInsertMsg;
                    }
                
                return new JsonResult() { Data = ObjRes, MaxJsonLength = int.MaxValue };

            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return new JsonResult() { Data = ObjRes, MaxJsonLength = int.MaxValue };
            }
            finally
            {
                objReq = null;
                serviceCRM = null;
            }


        }
        public JsonResult CRMUpdateDeviceRefundStatus(string strUpdateRefundDeviceStatus)
        {
            Device_Payment_Refund_Request objReq = new Device_Payment_Refund_Request();
            Device_Payment_Refund_Response ObjRes = new Device_Payment_Refund_Response();
            ServiceInvokeCRM serviceCRM;
            try
            {
                objReq = JsonConvert.DeserializeObject<Device_Payment_Refund_Request>(strUpdateRefundDeviceStatus);
                objReq.BrandCode = clientSetting.brandCode;
                objReq.CountryCode = clientSetting.countryCode;
                objReq.LanguageCode = clientSetting.langCode;
                 serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.Update_Device_Refund_Status(objReq);
                    //    ObjRes.SearchSimDetailsList.Where(a => a.SubmitDate != string.Empty).ToList().ForEach(b => b.SubmitDate = Utility.FormatDateTime(b.SubmitDate, clientSetting.mvnoSettings.dateTimeFormat));
                    if (ObjRes != null && ObjRes.responseDetails != null && ObjRes.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMUpdateDeviceRefundStatus_" + ObjRes.responseDetails.ResponseCode);
                        ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                
                return new JsonResult() { Data = ObjRes, MaxJsonLength = int.MaxValue };

            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return new JsonResult() { Data = ObjRes, MaxJsonLength = int.MaxValue };
            }
            finally
            {
                objReq = null;
                serviceCRM = null;
            }


        }


        #region POF-6007

        public ActionResult LegalSIMSWAPandROLLBACK()
        {
            legalsimswapResp Objresp = new legalsimswapResp();
            legalsimswapreq ObjReq = new legalsimswapreq();
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.Mode = "GET";
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - LegalSIMSWAPandROLLBACK Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objresp = serviceCRM.CRMlegalsimswap(ObjReq);


                if (Objresp.Responsedetails.ResponseCode == "0")
                {
                    Objresp.list_legalsimswapdtls.FindAll(a => a.Dateandtime != string.Empty).ForEach(b => b.Dateandtime = Utility.FormatDateTime(b.Dateandtime, "MM-dd-yyyy","","YES"));
                    Objresp.list_legalsimswapdtls.FindAll(a => a.Scheduledswapdate_Against_Rollback != string.Empty).ForEach(b => b.Scheduledswapdate_Against_Rollback = Utility.FormatDateTime(b.Scheduledswapdate_Against_Rollback, "MM-dd-yyyy", "", "YES"));
                    Objresp.list_legalsimswapdtls.FindAll(a => a.Authorised_Rejected_Date != string.Empty).ForEach(b => b.Authorised_Rejected_Date = Utility.FormatDateTime(b.Authorised_Rejected_Date, "MM-dd-yyyy HH:mm:ss", "", "YES"));
                    Objresp.list_legalsimswapdtls.FindAll(a => a.CompletedDate != string.Empty).ForEach(b => b.CompletedDate = Utility.FormatDateTime(b.CompletedDate, "MM-dd-yyyy", "", "YES"));
                    Objresp.list_legalsimswapdtls.FindAll(a => a.Rollback_scheduled_date != string.Empty).ForEach(b => b.Rollback_scheduled_date = Utility.FormatDateTime(b.Rollback_scheduled_date, "MM-dd-yyyy HH:mm:ss", "", "YES"));

                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - LegalSIMSWAPandROLLBACK End");

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SIMController - exception-LegalSIMSWAPandROLLBACK - " + this.ControllerContext, ex);

            }
            finally
            {
                ObjReq = null;
            }

            return View(Objresp);

        }
         public JsonResult SimswapandRollback(legalsimswapreq ObjReq)
        {
            legalsimswapResp Objresp = new legalsimswapResp();
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - LegalSIMSWAPandROLLBACK Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objresp = serviceCRM.CRMlegalsimswap(ObjReq);

                if (Objresp.Responsedetails.ResponseCode == "0")
                {
                    Objresp.list_legalsimswapdtls.FindAll(a => a.Dateandtime != string.Empty).ForEach(b => b.Dateandtime = Utility.FormatDateTime(b.Dateandtime, "MM-dd-yyyy", "", "YES"));
                    Objresp.list_legalsimswapdtls.FindAll(a => a.Scheduledswapdate_Against_Rollback != string.Empty).ForEach(b => b.Scheduledswapdate_Against_Rollback = Utility.FormatDateTime(b.Scheduledswapdate_Against_Rollback, "MM-dd-yyyy", "", "YES"));
                    Objresp.list_legalsimswapdtls.FindAll(a => a.Authorised_Rejected_Date != string.Empty).ForEach(b => b.Authorised_Rejected_Date = Utility.FormatDateTime(b.Authorised_Rejected_Date, "MM-dd-yyyy HH:mm:ss", "", "YES"));
                    Objresp.list_legalsimswapdtls.FindAll(a => a.CompletedDate != string.Empty).ForEach(b => b.CompletedDate = Utility.FormatDateTime(b.CompletedDate, "MM-dd-yyyy", "", "YES"));
                    Objresp.list_legalsimswapdtls.FindAll(a => a.Rollback_scheduled_date != string.Empty).ForEach(b => b.Rollback_scheduled_date = Utility.FormatDateTime(b.Rollback_scheduled_date, "MM-dd-yyyy HH:mm:ss", "", "YES"));

                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - LegalSIMSWAPandROLLBACK End");

            }
            catch (Exception ex)
            {

                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SIMController - exception-LegalSIMSWAPandROLLBACK - " + this.ControllerContext, ex);

            }
            finally
            {
                ObjReq = null;
            }
            return Json(Objresp, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Getdownloaddetails(legalsimswapreq ObjReq)
        {
            legalsimswapResp Objresp = new legalsimswapResp();
            ServiceInvokeCRM serviceCRM;

            try
            {

                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - Getdownloaddetails Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objresp = serviceCRM.CRMlegalsimswap(ObjReq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - Getdownloaddetails End");
                Session["DOWNLOADLIST"] = Objresp.list_legalsimswapdtls;

            }
            catch (Exception Ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SIMController - exception-Getdownloaddetails - " + this.ControllerContext, Ex);

            }
            finally
            {
                ObjReq = null;
            }

            return Json(Objresp, JsonRequestBehavior.AllowGet);
        }

        public void DownloadLegalSimswapDetails()
        {
            legalsimswapResp Objresp = new legalsimswapResp();
            GridView gridView = new GridView();
            try
            {

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - LegalSIMSWAPandROLLBACK Start");

                Objresp.list_legalsimswapdtls = ((List<legalsimswapdtls>)Session["DOWNLOADLIST"]);

                    StringBuilder Sb = new StringBuilder();
                    DataTable Dt = new DataTable();
                    Dt.Columns.Add("Msisdn", typeof(string));
                    Dt.Columns.Add("NewMsisdn", typeof(string));
                    Dt.Columns.Add("Action", typeof(string));
                    Dt.Columns.Add("Actiontype", typeof(string));
                    Dt.Columns.Add("Status", typeof(string));
                    Dt.Columns.Add("Dateandtime", typeof(string));
                    foreach (legalsimswapdtls item in Objresp.list_legalsimswapdtls)
                    {
                        DataRow Dr = Dt.NewRow();
                        Dr["Msisdn"] = item.Msisdn;
                        Dr["NewMsisdn"] = item.NewMsisdn;
                        Dr["Action"] = item.Action;
                        Dr["Actiontype"] = item.Actiontype;
                        Dr["Status"] = item.Status;
                        Dr["Dateandtime"] = item.Dateandtime;
                        Dt.Rows.Add(Dr);
                        //Sb.AppendLine(item.Msisdn);
                    }
                    IEnumerable<string> columnnames = Dt.Columns.Cast<DataColumn>().Select(Column => Column.ColumnName);
                    Sb.AppendLine(string.Join(",", columnnames));

                    foreach (DataRow Row in Dt.Rows)
                    {
                        IEnumerable<string> Fields = Row.ItemArray.Select(Field => Field.ToString());
                        Sb.AppendLine(string.Join(",", Fields));
                    }
                    byte[] filebytes = Encoding.UTF8.GetBytes(Sb.ToString());

                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - LegalSIMSWAPandROLLBACK End");
                    gridView.DataSource = Dt;
                    gridView.DataBind();

                    Utility.ExportToExcell(gridView, "LegalSimswap&Rollback_" + DateTime.Now.ToString("yyyy-MM-ddHHmmss"), this.HttpContext.Response);

                //return File(filebytes, "text/csv", "SimSwap & Rollback Details");
            }
            catch (Exception ex)
            {

                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SIMController - exception-LegalSIMSWAPandROLLBACK - " + this.ControllerContext, ex);
                //return File("", "text/csv", "Unable to download");
            }
            finally
            {
                gridView = null;
                Objresp = null;
            }
            
        }






        #endregion


        #region POF-6212
        public ActionResult SIMLifecycleManagement()
        {
            return View();
        }

        public JsonResult SIMLifecycleManagementMethod(string GetSIMLifecycleManagement)
        {
            SIMLifecycleManagementResponse ObjRes = new SIMLifecycleManagementResponse();
            SIMLifecycleManagementRequest ObjReq = JsonConvert.DeserializeObject<SIMLifecycleManagementRequest>(GetSIMLifecycleManagement);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "TopupController - CRMGetOptoutBenefitsDetails Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRes = serviceCRM.CRMSIMLifecycleManagementMethod(ObjReq);

                if (ObjRes != null && ObjRes.responseDetails != null && ObjRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.SIMResources.ResourceManager.GetString("SimLifecycle_Success_" + ObjRes.responseDetails.ResponseCode);
                    ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;
                }

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
        
        #region POF-6212
        public ActionResult ActiveBatchBulk()
        {
            Activebatchbulkuploadeddetails Objresp = new Activebatchbulkuploadeddetails();
            ActivebatchbulkuploadReq ObjReq = new ActivebatchbulkuploadReq();
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.Mode = "GET";
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - ActiveBatchBulk Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objresp = serviceCRM.CRMActiveBatchbulk(ObjReq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - ActiveBatchBulk End");

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SIMController - exception-ActiveBatchBulk - " + this.ControllerContext, ex);

            }
            finally
            {
                ObjReq = null;
            }

            return View(Objresp);
          
        }


        public ActionResult DownloadActivebatchBulk()
        {
            string filename = "SampleActiveBatchBulkupload.csv";
            string fullpath = "";
            try
            {
                fullpath = Path.Combine(this.Server.MapPath("~/App_Data/SampleExcelFormat/SampleActiveBatchBulkupload.csv"));
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


        //public void UploadTemppath(HttpPostedFileBase file)
        //{
        //    string DirectoryPath = string.Empty;
        //    string strFileName = string.Empty;
        //    try
        //    {
        //        DirectoryPath = clientSetting.preSettings.Temp_FolderPath_BulkUpload;
        //        CRMLogger.WriteMessage(Session["UserName"].ToString(), this.ControllerContext, "SIMController - UploadTemppath Start");

        //        if (Session["BUlkUPLOADFILENAME"] != null)
        //        {
        //            CRMLogger.WriteMessage(Session["UserName"].ToString(), this.ControllerContext, "Delete Temppath file");
        //            System.IO.File.Delete(clientSetting.preSettings.Temp_FolderPath_BulkUpload + Session["BUlkUPLOADFILENAME"].ToString());
        //        }
        //        if (!Directory.Exists(DirectoryPath))
        //        {
        //            DirectoryInfo dir = Directory.CreateDirectory(DirectoryPath);
        //            strFileName = Path.GetFileName(file.FileName);
        //            file.SaveAs(DirectoryPath + strFileName);
        //        }
        //        else
        //        {
        //            strFileName = Path.GetFileName(file.FileName);
        //            file.SaveAs(DirectoryPath + strFileName);
        //        }
        //        CRMLogger.WriteMessage(Session["UserName"].ToString(), this.ControllerContext, "Bulk File saved to temp path");

        //        Session["BUlkUPLOADFILENAME"] = strFileName;
        //        CRMLogger.WriteMessage(Session["UserName"].ToString(), this.ControllerContext, "SIMController - UploadTemppath END");
        //    }catch(Exception ex)
        //    {
        //        CRMLogger.WriteException(Session["UserName"].ToString(), "SIMController - exception-UploadTemppath - " + this.ControllerContext, ex);

        //    }
        //    finally
        //    {
        //         DirectoryPath = string.Empty;
        //         strFileName = string.Empty;
        //    }

        //}

        public JsonResult Activebatchbulkupload(ActivebatchbulkuploadReq activebatchbulkuploadreq)
        {
            Activebatchbulkuploadeddetails Objresp = new Activebatchbulkuploadeddetails();
            ServiceInvokeCRM serviceCRM;
            try
            {
                activebatchbulkuploadreq.CountryCode = clientSetting.countryCode;
                activebatchbulkuploadreq.BrandCode = clientSetting.brandCode;
                activebatchbulkuploadreq.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - Activebatchbulkupload Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                Objresp = serviceCRM.CRMActiveBatchbulk(activebatchbulkuploadreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - Activebatchbulkupload End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SIMController - exception-Activebatchbulkupload - " + this.ControllerContext, ex);
            }
            finally
            {
                activebatchbulkuploadreq = null;
            }
            return Json(Objresp, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public JsonResult PIAM_GETProducts(Get_Roles_PIAM_Req Objreq)
        {
            Get_Roles_PIAM_Response Objresp = new Get_Roles_PIAM_Response();
            ServiceInvokeCRM serviceCRM;
            try
            {
                Objreq.CountryCode = clientSetting.countryCode;
                Objreq.BrandCode = clientSetting.brandCode;
                Objreq.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - PIAM_GETProducts Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                Objresp = serviceCRM.CRM_PIAM_Service(Objreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - PIAM_GETProducts End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SIMController - exception-PIAM_GETProducts - " + this.ControllerContext, ex);
            }
            finally
            {
                Objreq = null;
            }
            return Json(Objresp, JsonRequestBehavior.AllowGet);
        }

        #region pof-6614
        public ActionResult PrePostRequestStatus(PrePostPendingReq ObjReq)
        {
            PrePostPendingResp Objresp = new PrePostPendingResp();
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                if(string.IsNullOrEmpty(ObjReq.Mode))
                    ObjReq.Mode = "GET";
                ObjReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - PrePostRequestStatus Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objresp = serviceCRM.CRMPrePostPending(ObjReq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - PrePostRequestStatus End");
                if (ObjReq.Mode == "GET")
                    return View(Objresp);
                else
                    return Json(Objresp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SIMController - exception-PrePostRequestStatus - " + this.ControllerContext, ex);
                if (ObjReq.Mode == "GET")
                    return View(Objresp);
                else
                    return Json(Objresp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                ObjReq = null;
            }
            

        }
        #endregion
    }
}
