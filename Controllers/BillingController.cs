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
using Resources;
using ServiceCRM;
using Newtonsoft.Json;
using System.Web;
using iTextSharp.text.html;
using System.Collections;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;

namespace CRM.Controllers
{
    [ValidateState]
    public class BillingController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();
        ServiceCRM.ServiceInvokeCRM crmNewService = new ServiceCRM.ServiceInvokeCRM(Convert.ToString(SettingsCRM.crmServiceUrl));

        #region FRR 4925
       private string RealICCIDForMultiTab;
        #endregion

        public ActionResult Service(string Textdata)
        {
            #region Declaration
            GetProvisionedServicesResponse objProvisionServiceResp = new GetProvisionedServicesResponse();
            //NetworkServiceListResp objResp = new NetworkServiceListResp();
            ServiceInfo Info = new Models.ServiceInfo();
            GetProvisionedServicesRequest objServiceListReq = new GetProvisionedServicesRequest();
            ServiceStatus NwStatus = new ServiceStatus();
            string Bundle_Topup_Index = string.Empty;
            string strServiceInfo = string.Empty;
            string strServiceEnt = string.Empty;
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            string[] strobjProvisionedArray;
            #endregion

            string validatemsisnd = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - Service Start");
                #region initialization
                NwStatus.BrandCode = clientSetting.brandCode;
                NwStatus.CountryCode = clientSetting.countryCode;
                NwStatus.LanguageCode = clientSetting.langCode;

                #region FRR 4925

                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                    Session["RealICCIDForMultiTab"] = Textdata;
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    NwStatus.Msisdn = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    NwStatus.ICCID = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Key).First().ToString();
                    NwStatus.IMSI = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.IMSI).First().ToString();
                    validatemsisnd = localDict.Where(x => Textdata.Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                }
                else
                {
                    NwStatus.Msisdn = Session["MobileNumber"].ToString();
                    NwStatus.ICCID = Convert.ToString(Session["ICCID"]);
                    NwStatus.IMSI = Convert.ToString(Session["IMSI"]);
                    validatemsisnd = Session["MobileNumber"].ToString();
                }

                #endregion

                objServiceListReq.ServiceStatus = NwStatus;
                #endregion

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objProvisionServiceResp = serviceCRM.CrmGetProvisionedServices(objServiceListReq);
                if (objProvisionServiceResp != null && objProvisionServiceResp.GETSERVICESTATUSRESP != null && objProvisionServiceResp.GETSERVICESTATUSRESP.RETURNCODE != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_Service_" + objProvisionServiceResp.GETSERVICESTATUSRESP.RETURNCODE);
                    objProvisionServiceResp.GETSERVICESTATUSRESP.ERRDESCRITION = string.IsNullOrEmpty(errorInsertMsg) ? objProvisionServiceResp.GETSERVICESTATUSRESP.ERRDESCRITION : errorInsertMsg;
                }
                if (objProvisionServiceResp != null && objProvisionServiceResp.GETInternationalRESP != null && objProvisionServiceResp.GETInternationalRESP.RETURNCODE != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_Service_" + objProvisionServiceResp.GETInternationalRESP.RETURNCODE);
                    objProvisionServiceResp.GETInternationalRESP.ERRDESCRITION = string.IsNullOrEmpty(errorInsertMsg) ? objProvisionServiceResp.GETInternationalRESP.ERRDESCRITION : errorInsertMsg;
                }


                #region FRR 4868
                if(objProvisionServiceResp != null  && clientSetting.preSettings.EnableRegulatorBlock.ToUpper() == "TRUE")
                {
                    Info.ALL_MO_SERVICES = objProvisionServiceResp.GETSERVICESTATUSRESP.ALL_MO_SERVICES;
                    Info.ALL_MT_SERVICES = objProvisionServiceResp.GETSERVICESTATUSRESP.ALL_MT_SERVICES;
                    Info.ALL_MO_MT_SERVICES = objProvisionServiceResp.GETSERVICESTATUSRESP.ALL_MO_MT_SERVICES;
                }
                #endregion


                

                if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() == "TRUE")
                {
                    
                    if (objProvisionServiceResp.AltanVMS == "SI")
                    {
                        Info.AltanVMS = true;
                        Info.AltanMSISDN = objProvisionServiceResp.AltanMSISDN;
                    }
                    else
                    {
                        Info.AltanVMS = false;
                        Info.AltanMSISDN = objProvisionServiceResp.AltanMSISDN;
                    }
                    if (objProvisionServiceResp.SHOWPRIVATENUMBERS == "SI")
                    {
                        Info.SHOWPRIVATENUMBERS = true;
                    }
                    else
                    {
                        Info.SHOWPRIVATENUMBERS = false;
                    }
                    if (objProvisionServiceResp.TRIPARTITECALLWAITING == "SI")
                    {
                        Info.TRIPARTITECALLWAITING = true;
                    }
                    else
                    {
                        Info.TRIPARTITECALLWAITING = false;
                    }

                    if(objProvisionServiceResp.AltanMOvoiceSmsBlockingUnblocking == "BLOCK")
                    {
                        Info.AltanMOvoiceSmsBlockingUnblocking = true;
                    }
                    else
                    {
                        Info.AltanMOvoiceSmsBlockingUnblocking = false;
                    }
                    if (objProvisionServiceResp.AltanMTVoiceSmsBlockingUnblocking == "BLOCK")
                    {
                        Info.AltanMTVoiceSmsBlockingUnblocking = true;
                    }
                    else
                    {
                        Info.AltanMTVoiceSmsBlockingUnblocking = false;
                    }


                }
               



                if (objProvisionServiceResp.GETSERVICESTATUSRESP.SERVICEINFO != null && objProvisionServiceResp.GETSERVICESTATUSRESP.SERVICEINFO != string.Empty && (objProvisionServiceResp.GETSERVICESTATUSRESP.RETURNCODE == "0" || objProvisionServiceResp.GETSERVICESTATUSRESP.RETURNCODE == "1" || objProvisionServiceResp.GETSERVICESTATUSRESP.RETURNCODE == "204"))
                {
                    strobjProvisionedArray = objProvisionServiceResp.GETSERVICESTATUSRESP.SERVICEINFO.ToString().Split(',').ToArray();
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MOCall)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MOCall)]) != string.Empty)
                        Info.MOCall = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MOCall)]));
                    Info.OFFNET = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.GETSERVICESTATUSRESP.OFFNET));
                    Info.LYCATOINTERNATIONAL = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.GETSERVICESTATUSRESP.LYCATOINTERNATIONAL));
                    Info.LYCATOLYCA = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.GETSERVICESTATUSRESP.LYCATOLYCA));
                    Info.LYCATOMVNO = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.GETSERVICESTATUSRESP.LYCATOMVNO));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MORoamingCall)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MORoamingCall)]) != string.Empty)
                        Info.MORoamingCall = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MORoamingCall)]));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MOSMS)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MOSMS)]) != string.Empty)
                        Info.MOSMS = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MOSMS)]));
                    Info.SMSOFFNET = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.GETSERVICESTATUSRESP.SMSOFFNET));
                    Info.SMSLYCATOINTERNATIONAL = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.GETSERVICESTATUSRESP.SMSLYCATOINTERNATIONAL));
                    Info.SMSLYCATOLYCA = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.GETSERVICESTATUSRESP.SMSLYCATOLYCA));
                    Info.SMSLYCATOMVNO = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.GETSERVICESTATUSRESP.SMSLYCATOMVNO));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MORoamingSMS)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MORoamingSMS)]) != string.Empty)
                        Info.MORoamingSMS = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MORoamingSMS)]));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.IVR)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.IVR)]) != string.Empty)
                        Info.IVR = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.IVR)]));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.USSD)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.USSD)]) != string.Empty)
                        Info.USSD = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.USSD)]));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MORoamingVideoCall)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MORoamingVideoCall)]) != string.Empty)
                        Info.MORoamingVideoCall = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MORoamingVideoCall)]));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.CRBT)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.CRBT)]) != string.Empty)
                        Info.CRBT = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.CRBT)]));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.SMSTopup)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.SMSTopup)]) != string.Empty)
                        Info.SMSTopup = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.SMSTopup)]));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MobileHomeAccount)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MobileHomeAccount)]) != string.Empty)
                        Info.MobileHomeAccount = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MobileHomeAccount)]));
                    
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MOVideoCall)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MOVideoCall)]) != string.Empty)
                        Info.MOVideoCall = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MOVideoCall)]));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MOData)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MOData)]) != string.Empty)
                        Info.MOData = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MOData)]));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MORoamData)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MORoamData)]) != string.Empty)
                        Info.MORoamData = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MORoamData)]));
                    
                    //MT VIDEOCALL 
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MTCall)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MTCall)]) != string.Empty)
                        Info.MTCall = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MTCall)]));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MTRoamingCall)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MTRoamingCall)]) != string.Empty)
                        Info.MTRoamingCall = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MTRoamingCall)]));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MTSMS)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MTSMS)]) != string.Empty)
                        Info.MTSMS = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MTSMS)]));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MTRoamingSMS)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MTRoamingSMS)]) != string.Empty)
                        Info.MTRoamingSMS = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MTRoamingSMS)]));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MTVideoCall)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MTVideoCall)]) != string.Empty)
                        Info.MTVideoCall = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MTVideoCall)]));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MTRoamingVideoCall)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MTRoamingVideoCall)]) != string.Empty)
                        Info.MTRoamingVideoCall = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MTRoamingVideoCall)]));

                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.VMS)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.VMS)]) != string.Empty)
                    {
                        Info.VMS = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.VMS)]));
                    }
                    
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MCA)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MCA)]) != string.Empty)
                    {
                        Info.MCA = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MCA)]));
                    }

                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MO4G)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MO4G)]) != string.Empty)
                        Info.MO4G = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MO4G)]));
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.MORoam4G)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.MORoam4G)]) != string.Empty)
                        Info.MORoam4G = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.MORoam4G)]));

                    //BundleTopup     
                    Bundle_Topup_Index = !string.IsNullOrEmpty(clientSetting.preSettings.bundleTopupServiceIndex) ? clientSetting.preSettings.bundleTopupServiceIndex : "26";
                    if ((strobjProvisionedArray[Convert.ToInt16(EnumService.bundleTopup)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.bundleTopup)]) != string.Empty)
                        Info.BundleTopup = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(Bundle_Topup_Index)]));

                    //4GLocalRoam 
                    if (strobjProvisionedArray.Count() > 27 && (strobjProvisionedArray[Convert.ToInt16(EnumService.FGLocalRoam)]) != null && (strobjProvisionedArray[Convert.ToInt16(EnumService.FGLocalRoam)]) != string.Empty)
                        Info.FourGLocalRoam = Convert.ToBoolean(Convert.ToInt16(strobjProvisionedArray[Convert.ToInt16(EnumService.FGLocalRoam)]));

                    if (!string.IsNullOrEmpty(objProvisionServiceResp.PSInfoStatus) || !string.IsNullOrEmpty(objProvisionServiceResp.PSEntStatus) || objProvisionServiceResp.GETSERVICESTATUSRESP.RETURNCODE == "2000")
                    {
                        strServiceInfo = objProvisionServiceResp.PSInfoStatus;
                        strServiceEnt = objProvisionServiceResp.PSEntStatus;
                        //ServiceInfo          
                        if (strServiceInfo == "0")
                            Info.PremiumServiceInfo = true;
                        else
                            Info.PremiumServiceInfo = false;
                       
                        //EntertaintmentandServiceInfo                                       
                        if (strServiceEnt == "0")
                            Info.PremiumServiceEnt = true;
                        else
                            Info.PremiumServiceEnt = false;
                        if(clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                        {
                            strServiceEnt =  strServiceEnt == "0" ? "1" : "0";
                            strServiceInfo = strServiceInfo == "0" ? "1" : "0";

                            Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                            localDict.Where(w => w.Value.MSISDN == validatemsisnd).Select(w =>  w.Value.existingStateEnt = strServiceEnt );
                            localDict.Where(w => w.Value.MSISDN == validatemsisnd).Select(w => w.Value.existingStateInfo = strServiceInfo);
                        }
                        else
                        {
                        Session["PremiumServiceEnt"] = strServiceEnt == "0" ? "1" : "0";
                            Session["PremiumServiceInfo"] = strServiceInfo == "0" ? "1" : "0";
                        }
                        
                    }
                }
                else
                {
                    Info.ErrorDesc = objProvisionServiceResp.GETSERVICESTATUSRESP.ERRDESCRITION;
                    Info.ErrorCode = objProvisionServiceResp.GETSERVICESTATUSRESP.RETURNCODE;
                }

                //BATIssues  
                if (objProvisionServiceResp.GETInternationalRESP != null && (objProvisionServiceResp.GETSERVICESTATUSRESP.RETURNCODE == "0" || objProvisionServiceResp.GETInternationalRESP.RETURNCODE == "1" || objProvisionServiceResp.GETInternationalRESP.RETURNCODE == "2000"))
                {
                    if (!string.IsNullOrEmpty(objProvisionServiceResp.GETInternationalRESP.MoHomeIntlVoiceCallStatus))
                        Info.MOHomeIntlVoiceCall = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.GETInternationalRESP.MoHomeIntlVoiceCallStatus));
                    if (!string.IsNullOrEmpty(objProvisionServiceResp.GETInternationalRESP.MoRoamIntlVoiceCallStatus))
                        Info.MORoamIntlVoiceCall = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.GETInternationalRESP.MoRoamIntlVoiceCallStatus));
                    if (!string.IsNullOrEmpty(objProvisionServiceResp.GETInternationalRESP.MoLRoamIntlVoiceCallStatus))
                        Info.MOLRoamIntlVoiceCall = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.GETInternationalRESP.MoLRoamIntlVoiceCallStatus));
                    if (!string.IsNullOrEmpty(objProvisionServiceResp.GETInternationalRESP.MtRoamIntlVoiceCallStatus))
                        Info.MTRoamIntlVoiceCall = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.GETInternationalRESP.MtRoamIntlVoiceCallStatus));
                    if (!string.IsNullOrEmpty(objProvisionServiceResp.GETInternationalRESP.MtLRoamIntlVoiceCallStatus))
                        Info.MTLRoamIntlVoiceCall = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.GETInternationalRESP.MtLRoamIntlVoiceCallStatus));
                }
                else
                {
                    if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
                    {
                    Info.ErrorDesc = objProvisionServiceResp.GETInternationalRESP != null ? objProvisionServiceResp.GETInternationalRESP.ERRDESCRITION : "";
                    Info.ErrorCode = objProvisionServiceResp.GETInternationalRESP != null ? objProvisionServiceResp.GETInternationalRESP.RETURNCODE : "";
                }
                }

                //AdultContent 
                if (!string.IsNullOrEmpty(objProvisionServiceResp.adultstatus))
                {
                    Info.AdultContent = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.adultstatus));
                }
                //6547
                if (!string.IsNullOrEmpty(objProvisionServiceResp.GETSERVICESTATUSRESP.WIFI))
                {
                    Info.WIFI = objProvisionServiceResp.GETSERVICESTATUSRESP.WIFI;
                }
                if(!string.IsNullOrEmpty(objProvisionServiceResp.GETSERVICESTATUSRESP.HOME))
                {
                    Info.HOME = objProvisionServiceResp.GETSERVICESTATUSRESP.HOME;
                }
                if (!string.IsNullOrEmpty(objProvisionServiceResp.GETSERVICESTATUSRESP.ROME))
                {
                    Info.ROME = objProvisionServiceResp.GETSERVICESTATUSRESP.ROME;
                }
                if (!string.IsNullOrEmpty(objProvisionServiceResp.GETSERVICESTATUSRESP.LOCALROME))
                {
                    Info.LOCALROME = objProvisionServiceResp.GETSERVICESTATUSRESP.LOCALROME;
                }

                //POF-6647
                if (!string.IsNullOrEmpty(objProvisionServiceResp.GETSERVICESTATUSRESP.IVR_Portability_Announcement))
                {
                    Info.IVR_Portability_Announcement = objProvisionServiceResp.GETSERVICESTATUSRESP.IVR_Portability_Announcement;
                }

                //collectcall
                if (!string.IsNullOrEmpty(objProvisionServiceResp.collectcallstatus))
                {
                    Info.collectcall = objProvisionServiceResp.collectcallstatus;
                }
                if (!string.IsNullOrEmpty(objProvisionServiceResp.MSISDNSwapRequest) || !string.IsNullOrEmpty(objProvisionServiceResp.simSwapRequest) || !string.IsNullOrEmpty(objProvisionServiceResp.portOutRequest))
                {
                    #region FRR-2682
                    Info.SimSwapSuspend = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.simSwapRequest));
                    Session["SimSuspendRequest"] = Convert.ToInt16(objProvisionServiceResp.simSwapRequest);
                    Info.MsisdnSwapSuspend = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.MSISDNSwapRequest));
                    Session["MsisdnSuspendRequest"] = Convert.ToInt16(objProvisionServiceResp.MSISDNSwapRequest);
                    Info.PortOutSwapSuspend = Convert.ToBoolean(Convert.ToInt16(objProvisionServiceResp.portOutRequest));
                    Session["PortoutSuspendRequest"] = Convert.ToInt16(objProvisionServiceResp.portOutRequest);
                    #endregion
                }

                if (objProvisionServiceResp.GETSERVICESTATUSRESP.RETURNCODE == "1" || objProvisionServiceResp.GETSERVICESTATUSRESP.RETURNCODE == "204")
                {
                    Info.ErrorDesc = objProvisionServiceResp.GETSERVICESTATUSRESP.ERRDESCRITION;
                }
                Info.ErrorCode = objProvisionServiceResp.GETSERVICESTATUSRESP.RETURNCODE;

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - Service End");
            }
            catch (Exception Ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, Ex);
            }
            finally
            {
                objProvisionServiceResp = null;
                objServiceListReq = null;
                NwStatus = null;
                serviceCRM = null;
                Bundle_Topup_Index = string.Empty;
                strServiceInfo = string.Empty;
                strServiceEnt = string.Empty;
                errorInsertMsg = string.Empty;
            }
            return View(Info);
        }

        public JsonResult NetworkServiceList(ServiceUpdate UpdateNewtwork)
        {
            ServiceUpdateRes objUpdateServiceResp = new ServiceUpdateRes();
            ServiceUpdate objUpdateServiceReq = new ServiceUpdate();
            ServiceInfo ServInfo = new Models.ServiceInfo();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - NetworkServiceList Start");
                if ((!string.IsNullOrEmpty(UpdateNewtwork.OPERATIONCODE)) && (!string.IsNullOrEmpty(UpdateNewtwork.SERVICEID)) || (!string.IsNullOrEmpty(UpdateNewtwork.IVR_Portability_Announcement)))
                {
                    objUpdateServiceReq.BrandCode = clientSetting.brandCode;
                    objUpdateServiceReq.CountryCode = clientSetting.countryCode;
                    objUpdateServiceReq.LanguageCode = clientSetting.langCode;

                    #region FRR 4925
                    if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                    {

                        Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                        objUpdateServiceReq.MSISDN = localDict.Where(x => UpdateNewtwork.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                        objUpdateServiceReq.IMSI = localDict.Where(x => UpdateNewtwork.textdata.ToString().Contains(x.Key)).Select(x => x.Value.IMSI).First().ToString();
                        objUpdateServiceReq.existingStateInfo = localDict.Where(x => UpdateNewtwork.textdata.ToString().Contains(x.Key)).Select(x => x.Value.existingStateInfo).First().ToString();
                        objUpdateServiceReq.existingStateEnt = localDict.Where(x => UpdateNewtwork.textdata.ToString().Contains(x.Key)).Select(x => x.Value.existingStateEnt).First().ToString();
                       
                    }
                    else
                    {
                    objUpdateServiceReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    objUpdateServiceReq.IMSI = Convert.ToString(Session["IMSI"]);
                        objUpdateServiceReq.existingStateInfo = Convert.ToString(Session["PremiumServiceInfo"]);
                        objUpdateServiceReq.existingStateEnt = Convert.ToString(Session["PremiumServiceEnt"]);
                    }
                    #endregion




                    objUpdateServiceReq.OPERATIONCODE = UpdateNewtwork.OPERATIONCODE;
                    objUpdateServiceReq.SERVICEID = UpdateNewtwork.SERVICEID;
                    objUpdateServiceReq.VMSStatus = UpdateNewtwork.VMSStatus;
                    objUpdateServiceReq.TICKETID = UpdateNewtwork.TICKETID;
                    objUpdateServiceReq.REQUESTEDBY = Convert.ToString(Session["UserName"]);
                    objUpdateServiceReq.REASON = UpdateNewtwork.REASON;
                    objUpdateServiceReq.SERVICEVALUE = UpdateNewtwork.SERVICEVALUE;
                    objUpdateServiceReq.PROFILENAME = UpdateNewtwork.PROFILENAME;
                    objUpdateServiceReq.MCAStatus = UpdateNewtwork.MCAStatus;
                    //6547
                    objUpdateServiceReq.MODE = UpdateNewtwork.MODE;
                    objUpdateServiceReq.PROFILETYPE = UpdateNewtwork.PROFILETYPE;
                    objUpdateServiceReq.STATUS = UpdateNewtwork.STATUS;
                    
                    //6647
                    objUpdateServiceReq.IVR_Portability_Announcement = UpdateNewtwork.IVR_Portability_Announcement;

                    objUpdateServiceReq.services = UpdateNewtwork.services;
                    if (UpdateNewtwork.services == "INFOTAINMENT")
                        objUpdateServiceReq.selectedStateInfo = UpdateNewtwork.selectedStateInfo;
                    if (UpdateNewtwork.services == "ENTERTAINMENT")
                        objUpdateServiceReq.selectedStateEnt = UpdateNewtwork.selectedStateEnt;
                    if (UpdateNewtwork.services == "SIMSWAP")
                    {
                        if (objUpdateServiceReq.OPERATIONCODE == "A")
                        {
                            objUpdateServiceReq.simSwapRequest = "1";
                            objUpdateServiceReq.msisdnSwapRequest = Convert.ToString(Session["MsisdnSuspendRequest"]);
                            objUpdateServiceReq.portOutRequest = Convert.ToString(Session["PortoutSuspendRequest"]);
                        }
                        else
                        {
                            objUpdateServiceReq.simSwapRequest = "0";
                            objUpdateServiceReq.msisdnSwapRequest = Convert.ToString(Session["MsisdnSuspendRequest"]);
                            objUpdateServiceReq.portOutRequest = Convert.ToString(Session["PortoutSuspendRequest"]);
                        }
                    }
                    else if (UpdateNewtwork.services == "MSISDNSWAP")
                    {
                        if (objUpdateServiceReq.OPERATIONCODE == "A")
                        {
                            objUpdateServiceReq.simSwapRequest = Convert.ToString(Session["SimSuspendRequest"]);
                            objUpdateServiceReq.msisdnSwapRequest = "1";
                            objUpdateServiceReq.portOutRequest = Convert.ToString(Session["PortoutSuspendRequest"]);
                        }
                        else
                        {
                            objUpdateServiceReq.simSwapRequest = Convert.ToString(Session["SimSuspendRequest"]);
                            objUpdateServiceReq.msisdnSwapRequest = "0";
                            objUpdateServiceReq.portOutRequest = Convert.ToString(Session["PortoutSuspendRequest"]);
                        }
                    }
                    else if (UpdateNewtwork.services == "PORTOUTSWAP")
                    {
                        if (objUpdateServiceReq.OPERATIONCODE == "A")
                        {
                            objUpdateServiceReq.simSwapRequest = Convert.ToString(Session["SimSuspendRequest"]);
                            objUpdateServiceReq.msisdnSwapRequest = Convert.ToString(Session["MsisdnSuspendRequest"]);
                            objUpdateServiceReq.portOutRequest = "1";
                        }
                        else
                        {
                            objUpdateServiceReq.simSwapRequest = Convert.ToString(Session["SimSuspendRequest"]);
                            objUpdateServiceReq.msisdnSwapRequest = Convert.ToString(Session["MsisdnSuspendRequest"]);
                            objUpdateServiceReq.portOutRequest = "0";
                        }
                    }
                    else if (UpdateNewtwork.services == "ADULTCONTENT")
                    {
                        if (objUpdateServiceReq.OPERATIONCODE == "A")
                            objUpdateServiceReq.adultstatus = "1";
                        else
                            objUpdateServiceReq.adultstatus = "0";
                    }
                    else if (UpdateNewtwork.services == "COLLECTCALL")
                    {
                        if (objUpdateServiceReq.OPERATIONCODE == "A")
                            objUpdateServiceReq.collectcallstatus = "A";
                        else
                            objUpdateServiceReq.collectcallstatus = "D";
                    }
                    else if (UpdateNewtwork.services == "INTL")
                    {
                        if (objUpdateServiceReq.SERVICEID == "MO_HOME_INTL_VOICE_CALL")
                            objUpdateServiceReq.SERVICEID = "37";
                        if (objUpdateServiceReq.SERVICEID == "MO_ROAM_INTL_VOICE_CALL")
                            objUpdateServiceReq.SERVICEID = "38";
                        if (objUpdateServiceReq.SERVICEID == "MO_LROAM_INTL_VOICE_CALL")
                            objUpdateServiceReq.SERVICEID = "39";
                        if (objUpdateServiceReq.SERVICEID == "MT_ROAM_VOICE_CALL")
                            objUpdateServiceReq.SERVICEID = "40";
                        if (objUpdateServiceReq.SERVICEID == "MT_LROAM_VOICE_CALL")
                            objUpdateServiceReq.SERVICEID = "41";
                    }

                    if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() == "TRUE")
                    {
                        objUpdateServiceReq.VMSMsisdn = UpdateNewtwork.VMSMsisdn;
                        objUpdateServiceReq.VMSupdatetag = UpdateNewtwork.VMSupdatetag;
                        objUpdateServiceReq.callwaiting = UpdateNewtwork.callwaiting;
                        objUpdateServiceReq.showprivatenumber = UpdateNewtwork.showprivatenumber;
                        if(!string.IsNullOrEmpty(UpdateNewtwork.AltanMTVoiceSmsBlockingUnblocking))
                        {
                            objUpdateServiceReq.AltanMOvoiceSmsBlockingUnblocking = UpdateNewtwork.AltanMTVoiceSmsBlockingUnblocking;
                        }
                        else
                        {
                            objUpdateServiceReq.AltanMOvoiceSmsBlockingUnblocking = UpdateNewtwork.AltanMOvoiceSmsBlockingUnblocking;
                        }
                 
                        objUpdateServiceReq.AltanMTVoiceSmsBlockingUnblocking = UpdateNewtwork.AltanMTVoiceSmsBlockingUnblocking;
                    }

                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    objUpdateServiceResp = serviceCRM.UpdateService(objUpdateServiceReq);
                    if (objUpdateServiceResp != null && objUpdateServiceResp.Response != null && objUpdateServiceResp.Response.ResponseCode == "0")
                    {
                        Session["SimSuspendRequest"] = Convert.ToInt16(objUpdateServiceReq.simSwapRequest);
                        Session["MsisdnSuspendRequest"] = Convert.ToInt16(objUpdateServiceReq.msisdnSwapRequest);
                        Session["PortoutSuspendRequest"] = Convert.ToInt16(objUpdateServiceReq.portOutRequest);
                    }
                    if (objUpdateServiceResp != null && objUpdateServiceResp.Response != null && objUpdateServiceResp.Response.ResponseCode != null)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_NetworkServiceList_" + objUpdateServiceResp.Response.ResponseCode);
                        objUpdateServiceResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objUpdateServiceResp.Response.ResponseDesc : errorInsertMsg;
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - NetworkServiceList End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objUpdateServiceReq = null;
                ServInfo = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(objUpdateServiceResp);
        }

        #region PrepaidNewtworkfeatureReason
        public ActionResult PrepaidNWServiceReason(PrepaidNFBlockReasonRequest nwReasonValue)
        {
            PrepaidNFBlockReasonResponse objNWReasonResp = new PrepaidNFBlockReasonResponse();
            PrepaidNFBlockReasonRequest objNFReasonReq = new PrepaidNFBlockReasonRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - PrepaidNWServiceReason Start");

                objNFReasonReq.BrandCode = clientSetting.brandCode;
                objNFReasonReq.CountryCode = clientSetting.countryCode;
                objNFReasonReq.LanguageCode = clientSetting.langCode;

                #region FRR 4925
                if(clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    objNFReasonReq.MSISDN = localDict.Where(x => nwReasonValue.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    
                }
                else
                {
                objNFReasonReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                }
                #endregion




                objNFReasonReq.Type = nwReasonValue.Type;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objNWReasonResp = serviceCRM.GetPrepaidNWBlockReason(objNFReasonReq);
                if (objNWReasonResp != null && objNWReasonResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_NetworkServiceList_Prepaid_" + objNWReasonResp.responseDetails.ResponseCode);
                    objNWReasonResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objNWReasonResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                objNWReasonResp.prepaidNFBlockReason.ForEach(pbr => pbr.submitDate = pbr.submitDate.FormaDateTime(clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.yyyyMMdd));

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - PrepaidNWServiceReason End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                objNFReasonReq = null;
                errorInsertMsg = string.Empty;
            }
            return Json(objNWReasonResp);
        }
        #endregion

        // 4925
        public ViewResult History(string Textdata)
        {
            BillHistory billHistory = new BillHistory();
            CRMActivationDateResponse actDateResp = new CRMActivationDateResponse();
            CRMActivationDateRequest actDateReq = new CRMActivationDateRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - History Start");
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



                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                actDateResp = serviceCRM.GetCRMActivationDate(actDateReq);
                billHistory.activationDate = string.IsNullOrEmpty(actDateResp.activationDate) ? "" : Convert.ToDateTime(actDateResp.activationDate).ToString("yyyy/MM/dd");

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - History End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                actDateResp = null;
                actDateReq = null;
            }
            return View(billHistory);
            
        }




        #region FRR - 4644 - AugustBundle

        public ViewResult CallBarring()
        {
            CallBarringUnBarring callbarring = new CallBarringUnBarring();

            CallBarringUnbarringResponse callbarringRes = new CallBarringUnbarringResponse();
            CallBarringUnbarringRequest callbarringReq = new CallBarringUnbarringRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;

            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - CallBarring Start");
                callbarringReq.CountryCode = clientSetting.countryCode;
                callbarringReq.BrandCode = clientSetting.brandCode;
                callbarringReq.LanguageCode = clientSetting.langCode;
                callbarringReq.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                callbarringRes = serviceCRM.GetCRMCallBarrringUnBarring(callbarringReq);

                if (callbarringRes.responseDetails.ResponseCode == "0")
                {
                    callbarring.CallBarringUnbarringList = callbarringRes.CallBarringUnbarringList;
                }
                callbarring.crmResponse = callbarringRes.responseDetails;

                if (callbarringRes != null && callbarringRes.responseDetails.ResponseCode != null && callbarringRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CallBarringRequestResponse_" + callbarringRes.responseDetails.ResponseCode);
                    callbarringRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? callbarringRes.responseDetails.ResponseDesc : errorInsertMsg;
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
                callbarringReq = null;
                errorInsertMsg = string.Empty;

            }
            return View(callbarring);
        }

        [HttpPost]
        public JsonResult UpdateCallBarring(string GetUpdateCallBarring)
        {
            CallBarringUnbarringResponse callbarringRes = new CallBarringUnbarringResponse();
            CallBarringUnbarringRequest callbarringReq = JsonConvert.DeserializeObject<CallBarringUnbarringRequest>(GetUpdateCallBarring);
            string errorInsertMsg = string.Empty;
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - UpdateCallBarring Start");
                callbarringReq.CountryCode = clientSetting.countryCode;
                callbarringReq.BrandCode = clientSetting.brandCode;
                callbarringReq.LanguageCode = clientSetting.langCode;
                callbarringReq.MSISDN = Session["MobileNumber"].ToString();

                if (string.IsNullOrEmpty(callbarringReq.Submittedby))
                    callbarringReq.Submittedby = Session["UserName"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                callbarringRes = serviceCRM.GetCRMCallBarrringUnBarring(callbarringReq);

                if (callbarringRes != null && callbarringRes.responseDetails.ResponseCode != null && callbarringRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UpdateCallBarringRequestResponse_" + callbarringRes.responseDetails.ResponseCode);
                    callbarringRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? callbarringRes.responseDetails.ResponseDesc : errorInsertMsg;
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
                callbarringReq = null;
                errorInsertMsg = string.Empty;
            }
            return Json(callbarringRes);
        }




        #endregion




        [HttpPost]
        public JsonResult LoadBillingHistory(BillHistory billHistory)
        {
            CRMBillingHistoryResponse crmPrePaidBillHistoryResp = new CRMBillingHistoryResponse();
            CRMBillingHistoryRequest crmPrePaidBillHistReq = new CRMBillingHistoryRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - LoadBillingHistory Start");
                if (Convert.ToString(Session["isPrePaid"]) == "1")
                {                   
                    crmPrePaidBillHistReq.CountryCode = clientSetting.countryCode;
                    crmPrePaidBillHistReq.BrandCode = clientSetting.brandCode;
                    crmPrePaidBillHistReq.LanguageCode = clientSetting.langCode;
                    crmPrePaidBillHistReq.Calltype = billHistory.callType;
                    

                    #region FRR 4925
                    if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                    {
                   
                        Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                        crmPrePaidBillHistReq.Msisdn = localDict.Where(x => billHistory.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                        crmPrePaidBillHistReq.ICCID = localDict.Where(x => billHistory.textdata.ToString().Contains(x.Key)).Select(x => x.Key).First().ToString();
                        crmPrePaidBillHistReq.OldNo = localDict.Where(x => billHistory.textdata.ToString().Contains(x.Key)).Select(x => x.Value.SwapMSISDN).First().ToString();
                        crmPrePaidBillHistReq.Mask = localDict.Where(x => billHistory.textdata.ToString().Contains(x.Key)).Select(x => x.Value.Mask).First().ToString();

                    }
                    else
                    {
                        crmPrePaidBillHistReq.Msisdn = Session["MobileNumber"].ToString();
                        crmPrePaidBillHistReq.OldNo = Convert.ToString(Session["SwapMSISDN"]);
                        crmPrePaidBillHistReq.Mask = Session["MaskMode"].ToString();
                        crmPrePaidBillHistReq.ICCID = Session["ICCID"].ToString();
                    }

                   

                    #endregion

                    if (billHistory.subCallType != null)
                    {
                        crmPrePaidBillHistReq.Callsubtype = billHistory.subCallType;
                    }
                    else
                    {
                        crmPrePaidBillHistReq.Callsubtype = string.Empty;
                    }

                    crmPrePaidBillHistReq.startDate = Utility.GetDateconvertion(billHistory.fromDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
                    crmPrePaidBillHistReq.endDate = Utility.GetDateconvertion(billHistory.toDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");

                    #region Hitting service GetCRMBillingHistoryDetails
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    crmPrePaidBillHistoryResp = serviceCRM.GetCRMBillingHistoryDetails(crmPrePaidBillHistReq);
                    if (crmPrePaidBillHistoryResp != null && crmPrePaidBillHistoryResp.Response != null && crmPrePaidBillHistoryResp.Response.ResponseCode != null)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_LoadBillingHistory_" + crmPrePaidBillHistoryResp.Response.ResponseCode);
                        crmPrePaidBillHistoryResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? crmPrePaidBillHistoryResp.Response.ResponseDesc : errorInsertMsg;
                    }
                    #endregion

              
                    crmPrePaidBillHistoryResp.BillingHistoryDetails.ForEach(pbr => pbr.DateTime_Of_Registration = pbr.DateTime_Of_Registration.FormaDateTime(clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.yyyyMMdd));


                    #region Fetching Billing Bundle Details
                    try
                    {
                        Dictionary<string, string> dicCallFeatures = Utility.DataTableToDictionary(Utility.GetDropdownMasterFromDB("tbl_cdr_call_feature"));
                        foreach (BillingHistoryDetails billHistDet in crmPrePaidBillHistoryResp.BillingHistoryDetails)
                        {
                            try
                            {
                                if (clientSetting.preSettings.billingHistoryContentChargeEnabler.ToLower() == "true")
                                {
                                    if (billHistDet.call_feature != "31")
                                    {
                                        billHistDet.content_charge_Bundle_code = "NA";
                                        billHistDet.content_charge_minutes = "NA";
                                        billHistDet.content_charge_from_Main_Balance = "NA";
                                    }
                                }
                                billHistDet.Call_Date = string.IsNullOrEmpty(billHistDet.Call_Date) ? string.Empty : Utility.GetDateconvertion(billHistDet.Call_Date, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat);
                                //billHistDet.Call_Date = string.IsNullOrEmpty(billHistDet.Call_Date) ? string.Empty : Utility.FormatDateTime(billHistDet.Call_Date, clientSetting.mvnoSettings.dateTimeFormat);
                                billHistDet.FreeZoneexpirydate = string.IsNullOrEmpty(billHistDet.FreeZoneexpirydate) ? string.Empty : Utility.FormatDateTime(billHistDet.FreeZoneexpirydate, clientSetting.mvnoSettings.dateTimeFormat);
                                billHistDet.call_feature = string.IsNullOrEmpty(billHistDet.call_feature) ? string.Empty : (dicCallFeatures.ContainsKey(billHistDet.call_feature.Trim()) ? dicCallFeatures[billHistDet.call_feature.Trim()] : billHistDet.call_feature.Trim());
                            }
                            catch (Exception exbillingHistoryContentChargeEnabler)
                            {
                                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exbillingHistoryContentChargeEnabler);
                            }
                        }
                    }
                    catch (Exception dicCallFeatures)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, dicCallFeatures);
                    }
                    #endregion

                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "LoadBillingHistory End");
                    return new JsonResult() { Data = crmPrePaidBillHistoryResp, MaxJsonLength = int.MaxValue };
                }
                else
                {
                    CRMPostpaidBillingHistoryResponse crmPostPaidBillHistResp = new CRMPostpaidBillingHistoryResponse();
                    CRMPostpaidBillingHistoryRequest crmPostPaidBillHistReq = new CRMPostpaidBillingHistoryRequest();
                    try
                    {
                        crmPostPaidBillHistReq.CountryCode = clientSetting.countryCode;
                        crmPostPaidBillHistReq.BrandCode = clientSetting.brandCode;
                        crmPostPaidBillHistReq.LanguageCode = clientSetting.langCode;
                        crmPostPaidBillHistReq.Msisdn = Session["MobileNumber"].ToString();
                        crmPostPaidBillHistReq.Mask = Session["MaskMode"].ToString();
                        crmPostPaidBillHistReq.oldno = string.Empty;
                        crmPostPaidBillHistReq.SizeType = "BYTES";
                        crmPostPaidBillHistReq.fromDate = Utility.FormatDateTimeToService(billHistory.fromDate, clientSetting.mvnoSettings.dateTimeFormat);
                        crmPostPaidBillHistReq.toDate = Utility.FormatDateTimeToService(billHistory.toDate, clientSetting.mvnoSettings.dateTimeFormat);

                        #region Hitting service GetCRMPostpaidBillingHistoryDetails
                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        crmPostPaidBillHistResp = serviceCRM.GetCRMPostpaidBillingHistoryDetails(crmPostPaidBillHistReq);
                        if (crmPostPaidBillHistResp != null && crmPostPaidBillHistResp.responseDetails != null && crmPostPaidBillHistResp.responseDetails.ResponseCode != null)
                        {
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_LoadBillingHistory_" + crmPostPaidBillHistResp.responseDetails.ResponseCode);
                            crmPostPaidBillHistResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? crmPostPaidBillHistResp.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                        #endregion
                        #region FindPostpaid details
                        try
                        {
                            crmPostPaidBillHistResp.BillingHistoryDetailsPostpaid.FindAll(b => string.IsNullOrEmpty(b.Call_Date)).ForEach(b => Utility.FormatDateTime(b.Call_Date, clientSetting.mvnoSettings.dateTimeFormat));
                        }
                        catch (Exception exFindPostPaidDetails)
                        {
                            CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exFindPostPaidDetails);
                        }
                        #endregion
                    }
                    catch (Exception exPostPaid)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exPostPaid);
                    }
                    finally
                    {
                        crmPostPaidBillHistReq = null;
                    }
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - LoadBillingHistory End");
                    return new JsonResult() { Data = crmPostPaidBillHistResp, MaxJsonLength = int.MaxValue };
                }
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
                crmPrePaidBillHistReq = null;
                errorInsertMsg = string.Empty;
            }
            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - LoadBillingHistory End");
            return Json(string.Empty, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void LoadNonMaskBillingHistory(FormCollection formNonMaskBill)
        {
            GridView gridView = new GridView();
            CRMBillingHistoryResponse crmBillHistory = new CRMBillingHistoryResponse();
            CRMBillingHistoryRequest crmBillHistReq = new CRMBillingHistoryRequest();
            ServiceInvokeCRM serviceCRM;
            CRMBase objBase = new CRMBase();
            getBundleResource objResource;
            BillingHistoryDetails billTemp = new BillingHistoryDetails();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - LoadNonMaskBillingHistory Start");
                if (Convert.ToString(Session["isPrePaid"]) == "1")
                {
                    #region Prepaid Feature
                    crmBillHistReq.CountryCode = clientSetting.countryCode;
                    crmBillHistReq.BrandCode = clientSetting.brandCode;
                    crmBillHistReq.LanguageCode = clientSetting.langCode;
                    crmBillHistReq.Msisdn = Session["MobileNumber"].ToString();
                    crmBillHistReq.OldNo = Convert.ToString(Session["SwapMSISDN"]);
                    crmBillHistReq.Mask = "0";
                    crmBillHistReq.Calltype = formNonMaskBill["callType"].Trim();
                    crmBillHistReq.Callsubtype = formNonMaskBill["subCallType"].Trim();
                    crmBillHistReq.startDate = Utility.FormatDateTimeToService(formNonMaskBill["fromDate"].Trim(), clientSetting.mvnoSettings.dateTimeFormat);
                    crmBillHistReq.endDate = Utility.FormatDateTimeToService(formNonMaskBill["toDate"].Trim(), clientSetting.mvnoSettings.dateTimeFormat);

                    #region Hitting Service GetCRMBillingHistoryDetails
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    crmBillHistory = serviceCRM.GetCRMBillingHistoryDetails(crmBillHistReq);
                    if (crmBillHistory != null && crmBillHistory.Response != null && crmBillHistory.Response.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_LoadNonMaskBillingHistory_" + crmBillHistory.Response.ResponseCode);
                        crmBillHistory.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? crmBillHistory.Response.ResponseDesc : errorInsertMsg;
                    }
                    #endregion
                    #region Binding GrdView Feature
                    Dictionary<string, string> dicCallFeatures = null;
                    List<string> hideCol = null;
                    BoundField bfield;
                    string bundleName = string.Empty;
                    try
                    {
                        objBase.CountryCode = clientSetting.countryCode;
                        objBase.BrandCode = clientSetting.brandCode;
                        objBase.LanguageCode = clientSetting.langCode;
                        objResource = crmNewService.LoadBundleResource(objBase);
                        dicCallFeatures = Utility.DataTableToDictionary(Utility.GetDropdownMasterFromDB("tbl_cdr_call_feature"));
                        foreach (BillingHistoryDetails billHistDet in crmBillHistory.BillingHistoryDetails)
                        {
                            #region For Each Loop for BillingHistoryDetails
                            try
                            {
                                if (clientSetting.preSettings.billingHistoryContentChargeEnabler.ToLower() == "true")
                                {
                                    if (billHistDet.call_feature != "31")
                                    {
                                        billHistDet.content_charge_Bundle_code = "NA";
                                        billHistDet.content_charge_minutes = "NA";
                                        billHistDet.content_charge_from_Main_Balance = "NA";
                                    }
                                }
                                billHistDet.Call_Date = string.IsNullOrEmpty(billHistDet.Call_Date) ? string.Empty : Utility.FormatDateTime(billHistDet.Call_Date, clientSetting.mvnoSettings.dateTimeFormat);
                                billHistDet.FreeZoneexpirydate = string.IsNullOrEmpty(billHistDet.FreeZoneexpirydate) ? string.Empty : Utility.FormatDateTime(billHistDet.FreeZoneexpirydate, clientSetting.mvnoSettings.dateTimeFormat);
                                billHistDet.call_feature = string.IsNullOrEmpty(billHistDet.call_feature) ? string.Empty : (dicCallFeatures.ContainsKey(billHistDet.call_feature.Trim()) ? dicCallFeatures[billHistDet.call_feature.Trim()] : billHistDet.call_feature.Trim());
                                if (objResource.objBundleResource.Count > 0)
                                {
                                    bundleName = string.Empty;
                                    if (clientSetting.preSettings.planChangeEnabler.ToLower().Trim() == "true" && (!string.IsNullOrEmpty(billHistDet.Plan_ID) && !string.IsNullOrEmpty(billHistDet.Bundle_Code)))
                                    {
                                        if (objResource.objBundleResource.Any(b => (billHistDet.Plan_ID == Convert.ToString(b.PLAN_ID) && billHistDet.Bundle_Code == b.ID)))
                                        {
                                            bundleName = objResource.objBundleResource.FirstOrDefault(b => (billHistDet.Plan_ID == Convert.ToString(b.PLAN_ID) && billHistDet.Bundle_Code == b.ID)).BundleDesc;
                                        }
                                        else
                                        {
                                            bundleName = string.Empty;
                                        }
                                    }
                                    else if (!string.IsNullOrEmpty(billHistDet.Bundle_Code))
                                    {
                                        if (objResource.objBundleResource.Any(b => billHistDet.Bundle_Code == b.ID))
                                        {
                                            bundleName = objResource.objBundleResource.FirstOrDefault(b => billHistDet.Bundle_Code == b.ID).BundleDesc;
                                        }
                                        else
                                        {
                                            bundleName = string.Empty;
                                        }
                                    }
                                    else
                                    {
                                        bundleName = string.Empty;
                                    }
                                    billHistDet.Bundle_Name = bundleName;
                                }
                                else
                                {
                                    billHistDet.Bundle_Name = string.Empty;
                                }
                            }
                            catch (Exception exBillingHistoryDetails)
                            {
                                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exBillingHistoryDetails);
                            }
                            #endregion
                        }
                        var props = billTemp.GetType().GetProperties();
                        foreach (var prop in props)
                        {
                            hideCol = new List<string>(new string[] { "CDRTYPEID" });
                            if (!hideCol.Contains(prop.Name))
                            {
                                bfield = new BoundField();
                                bfield.HeaderText = prop.Name;
                                bfield.DataField = prop.Name;
                                gridView.Columns.Add(bfield);
                            }
                        }
                    }
                    catch (Exception exBindingGridView)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exBindingGridView);
                    }
                    finally
                    {
                        dicCallFeatures = null;
                        hideCol = null;
                        bfield = null;
                        bundleName = string.Empty;
                    }
                    #endregion
                    gridView.AutoGenerateColumns = false;
                    gridView.DataSource = crmBillHistory.BillingHistoryDetails;
                    #endregion
                }
                else
                {
                    #region PostPaid Feature
                    CRMPostpaidBillingHistoryResponse crmPostPaidBillHistResp = new CRMPostpaidBillingHistoryResponse();
                    CRMPostpaidBillingHistoryRequest crmPostPaidBillHistReq = new CRMPostpaidBillingHistoryRequest();
                    string errorInsertMsg = string.Empty;
                    try
                    {
                        crmPostPaidBillHistReq.CountryCode = clientSetting.countryCode;
                        crmPostPaidBillHistReq.BrandCode = clientSetting.brandCode;
                        crmPostPaidBillHistReq.LanguageCode = clientSetting.langCode;
                        crmPostPaidBillHistReq.Msisdn = Session["MobileNumber"].ToString();
                        crmPostPaidBillHistReq.Mask = "0";
                        crmPostPaidBillHistReq.oldno = string.Empty;
                        crmPostPaidBillHistReq.SizeType = "BYTES";

                        crmPostPaidBillHistReq.fromDate = Utility.FormatDateTimeToService(formNonMaskBill["fromDate"].Trim(), clientSetting.mvnoSettings.dateTimeFormat);
                        crmPostPaidBillHistReq.toDate = Utility.FormatDateTimeToService(formNonMaskBill["toDate"].Trim(), clientSetting.mvnoSettings.dateTimeFormat);

                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                        crmPostPaidBillHistResp = serviceCRM.GetCRMPostpaidBillingHistoryDetails(crmPostPaidBillHistReq);
                        if (crmPostPaidBillHistResp != null && crmPostPaidBillHistResp.responseDetails != null && crmPostPaidBillHistResp.responseDetails.ResponseCode != null)
                        {
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_LoadBillingHistory_" + crmPostPaidBillHistResp.responseDetails.ResponseCode);
                            crmPostPaidBillHistResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? crmPostPaidBillHistResp.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                        gridView.DataSource = crmPostPaidBillHistResp.BillingHistoryDetailsPostpaid;
                    }
                    catch (Exception exPostPaid)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exPostPaid);
                    }
                    finally
                    {
                        errorInsertMsg = string.Empty;
                        crmPostPaidBillHistReq = null;
                        crmPostPaidBillHistResp = null;
                    }
                    #endregion
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - LoadNonMaskBillingHistory End");
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "BillingHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                crmBillHistory = null;
                crmBillHistReq = null;
                serviceCRM=null;
                objBase = null;
                objResource=null;
                billTemp = null;
            }
        }

        [HttpPost]
        public JsonResult BillingSummary(BillHistory billHistory)
        {
            BillingSummaryResponse billSummaryResp = new BillingSummaryResponse();
            BillingSummaryRequest billSummaryReq = new BillingSummaryRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - BillingSummary Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                billSummaryReq.CountryCode = clientSetting.countryCode;
                billSummaryReq.BrandCode = clientSetting.brandCode;
                billSummaryReq.LanguageCode = clientSetting.langCode;
                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                   
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    billSummaryReq.msisdn = localDict.Where(x => billHistory.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    billSummaryReq.oldNo = localDict.Where(x => billHistory.textdata.ToString().Contains(x.Key)).Select(x => x.Value.SwapMSISDN).First().ToString();
                }
                else
                {
                    billSummaryReq.msisdn = Session["MobileNumber"].ToString();
                    billSummaryReq.oldNo = Convert.ToString(Session["SwapMSISDN"]);
                }

                
                billSummaryReq.fromDate = Utility.GetDateconvertion(billHistory.fromDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
                billSummaryReq.toDate = Utility.GetDateconvertion(billHistory.toDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
                billSummaryReq.mode = billHistory.mode;
                billSummaryReq.calltype = billHistory.callType;
                billSummaryResp = serviceCRM.BillingSummaryCRM(billSummaryReq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - BillingSummary End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
                billSummaryReq = null;
            }
            return Json(billSummaryResp);
        }

        [HttpGet]
        public string SubCallTypes(string callType)
        {
            string htmlSubCallType = string.Empty;
            CRMSubCallTypeRequest subCallTypeReq = new CRMSubCallTypeRequest();
            CRMSubCallTypeResponse subCallTypeResp = new CRMSubCallTypeResponse();
            ServiceInvokeCRM serviceCRM;
            string subcallType = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - SubCallTypes Start");
                subCallTypeReq.CountryCode = clientSetting.countryCode;
                subCallTypeReq.BrandCode = clientSetting.brandCode;
                subCallTypeReq.LanguageCode = clientSetting.langCode;
                subCallTypeReq.InputType = callType;
                
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                subCallTypeResp = serviceCRM.GetCRMSubCallType(subCallTypeReq);
                
                foreach (string sCT in subCallTypeResp.SubCallTypeValues.Select(m => m.CDRSubType))
                {
                    subcallType = sCT;
                    if (subcallType.Contains("/"))
                    {
                        subcallType = sCT.Replace("/", string.Empty);
                    }
                    subcallType = BillingResources.ResourceManager.GetString(subcallType.ToUpper());
                    subcallType = string.IsNullOrEmpty(subcallType) ? sCT : subcallType;
                    htmlSubCallType += "<option title='" + subcallType + "' selectVal='" + sCT + "' >" + subcallType + "</option>";
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - SubCallTypes End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                subcallType = string.Empty;
                subCallTypeReq = null;
                subCallTypeResp = null;
                serviceCRM = null;
            }
            return htmlSubCallType;
        }

        [HttpGet]
        public JsonResult FourGServiceHistory() // Postpaid
        {
            Postpaid4GserviceHistoryRes postpaid4GHistoryRes = new Postpaid4GserviceHistoryRes();
            Postpaid4GserviceHistoryReq postpaid4GHistoryReq = new Postpaid4GserviceHistoryReq();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - FourGServiceHistory Start");
                postpaid4GHistoryReq.CountryCode = clientSetting.countryCode;
                postpaid4GHistoryReq.BrandCode = clientSetting.brandCode;
                postpaid4GHistoryReq.LanguageCode = clientSetting.langCode;
                postpaid4GHistoryReq.MSISDN = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                postpaid4GHistoryRes = serviceCRM.CRMPostpaid4GserviceHistory(postpaid4GHistoryReq);

                Dictionary<string, string> dic4GChannels = new Dictionary<string, string>();
                try
                {
                    postpaid4GHistoryRes.ListGet4GserviceHistory.ForEach(m => m.FourGStartDate = Utility.FormatDateTime(m.FourGStartDate, clientSetting.mvnoSettings.dateTimeFormat));
                    postpaid4GHistoryRes.ListGet4GserviceHistory.ForEach(m => m.FourGEndDate = Utility.FormatDateTime(m.FourGEndDate, clientSetting.mvnoSettings.dateTimeFormat));
                    dic4GChannels = Utility.DataTableToDictionary(Utility.GetDropdownMasterFromDB("tbl_4G_servicechannel"));
                    postpaid4GHistoryRes.ListGet4GserviceHistory.ForEach(m => m.FourGModeOptIn = dic4GChannels[string.IsNullOrEmpty(m.FourGModeOptIn) ? "NA" : m.FourGModeOptIn]);
                    postpaid4GHistoryRes.ListGet4GserviceHistory.ForEach(m => m.FourGModeOptOut = dic4GChannels[string.IsNullOrEmpty(m.FourGModeOptOut) ? "NA" : m.FourGModeOptOut]);
                }
                catch (Exception exPostPaidFourGServiceHistory)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exPostPaidFourGServiceHistory);
                }
                finally
                {
                    dic4GChannels = null;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - FourGServiceHistory End");
                return Json(postpaid4GHistoryRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(postpaid4GHistoryRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                postpaid4GHistoryReq = null;
                serviceCRM = null;
            }
            
        }

        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadBillingHistory(string billingData)
        {
            GridView gridView = new GridView();
            BillingHistoryDetails billTemp = new BillingHistoryDetails();
            List<BillingHistoryDetails> billHisDetails;
            List<BillingHistoryDetailsPostpaid> billHisDetailsPP;
            List<string> hideCol;
            BoundField bfield;
            string colNames = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownLoadBillingHistory Start");
                if (Convert.ToString(Session["isPrePaid"]) == "1")
                {
                    billHisDetails = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<BillingHistoryDetails>>(billingData);
                    var props = billTemp.GetType().GetProperties();
                    foreach (var prop in props)
                    {
                        // DEFECT


                        colNames = Resources.BillingResources.ResourceManager.GetString(Convert.ToString(prop.Name));
      
                        hideCol = new List<string>(new string[] { "CDRTYPEID", "Traceid", "Extn_Record_Type" });
                        if (clientSetting.preSettings.continuedCallIDconfig == "0")
                        {
                            hideCol.Add("ContinuedCallID");
                        }
                        if (!hideCol.Contains(prop.Name))
                        {

                            if(!string.IsNullOrEmpty(colNames))
                            {
                                bfield = new BoundField();
                                bfield.HeaderText = colNames;
                                bfield.DataField = prop.Name;
                                gridView.Columns.Add(bfield);
                            }
                            else
                            {
                            bfield = new BoundField();
                            bfield.HeaderText = prop.Name;
                            bfield.DataField = prop.Name;
                            gridView.Columns.Add(bfield);
                        }
                          
                        }
                    }
                    gridView.AutoGenerateColumns = false;
                    gridView.DataSource = billHisDetails;
                }
                else
                {
                    billHisDetailsPP = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<BillingHistoryDetailsPostpaid>>(billingData);
                    gridView.DataSource = billHisDetailsPP;
                }
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "BillingHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownLoadBillingHistory End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                billTemp = null;
                billHisDetails = null;
                billHisDetailsPP = null;
                hideCol = null;
                bfield = null;
                colNames = string.Empty;
            }
        }

        [HttpPost]
        public void DownLoadPremiumHistory(string premiumData)
        {
            GridView gridView = new GridView();
            List<BillingHistoryFeatures> billHisDetails;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownLoadPremiumHistory Start");
                billHisDetails = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<BillingHistoryFeatures>>(premiumData);
                gridView.DataSource = billHisDetails;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "PremiumHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownLoadPremiumHistory Start");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                billHisDetails = null;
            }
        }
        public ActionResult Transaction()
        {
            ViewBag.DateFormat = clientSetting.mvnoSettings.dateTimeFormat.ToLower().Replace("yyyy", "yy");
            return View();
        }

        [HttpPost]
        public JsonResult TransactionDetail(string strTicketIDVal, string strTypeVal, string strStatusVal, string tranFDateVal, string tranToDateVal)
        {
            CRMSearchTransactionRequest objSearchTransaction = new CRMSearchTransactionRequest();
            CRMSearchTransactionResponse crmSearchTransactionResponse = new CRMSearchTransactionResponse();
            CRMResponse objResp = new CRMResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - TransactionDetail Start");
                objSearchTransaction.CountryCode = clientSetting.countryCode;
                objSearchTransaction.BrandCode = clientSetting.brandCode;
                objSearchTransaction.LanguageCode = clientSetting.langCode;
                objSearchTransaction.MSISDN = Session["MobileNumber"].ToString();
                objSearchTransaction.TransType = strTypeVal;
                objSearchTransaction.Status = strStatusVal;
                objSearchTransaction.TicketID = strTicketIDVal;
                objSearchTransaction.FromDate = Utility.GetDateconvertion(tranFDateVal, "YYYY-MM-DD", true, clientSetting.mvnoSettings.dateTimeFormat);
                objSearchTransaction.ToDate = Utility.GetDateconvertion(tranToDateVal, "YYYY-MM-DD", true, clientSetting.mvnoSettings.dateTimeFormat);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                crmSearchTransactionResponse = serviceCRM.GetCRMSearchTransaction(objSearchTransaction);
                if (crmSearchTransactionResponse != null && crmSearchTransactionResponse.Response != null && crmSearchTransactionResponse.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_Transaction_" + crmSearchTransactionResponse.Response.ResponseCode);
                    crmSearchTransactionResponse.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? crmSearchTransactionResponse.Response.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - TransactionDetail End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objSearchTransaction = null;
                objResp = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(crmSearchTransactionResponse.SearchTransaction, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GenericReportDetail(CRMGenericReportRequest objSearchTransaction)
        {
            CRMGenericReportResponse crmSearchTransactionResponse = new CRMGenericReportResponse();
            CRMResponse objResp = new CRMResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage("DIRECT", this.ControllerContext, "BillingController - TransactionDetail Start");
                objSearchTransaction.CountryCode = clientSetting.countryCode;
                objSearchTransaction.BrandCode = clientSetting.brandCode;
                objSearchTransaction.LanguageCode = clientSetting.langCode;
                objSearchTransaction.FromDate = Utility.GetDateconvertion(objSearchTransaction.FromDate, "YYYY-MM-DD", true, clientSetting.mvnoSettings.dateTimeFormat);
                objSearchTransaction.ToDate = Utility.GetDateconvertion(objSearchTransaction.ToDate, "YYYY-MM-DD", true, clientSetting.mvnoSettings.dateTimeFormat);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                crmSearchTransactionResponse = serviceCRM.GetCRMGenericReport(objSearchTransaction);
                if (crmSearchTransactionResponse != null && crmSearchTransactionResponse.Response != null && crmSearchTransactionResponse.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_Transaction_" + crmSearchTransactionResponse.Response.ResponseCode);
                    crmSearchTransactionResponse.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? crmSearchTransactionResponse.Response.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage("DIRECT", this.ControllerContext, "BillingController - TransactionDetail End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException("DIRECT", this.ControllerContext, ex);
            }
            finally
            {
                objSearchTransaction = null;
                objResp = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(crmSearchTransactionResponse.GenericReport, JsonRequestBehavior.AllowGet);
        }

//46714
        public ActionResult GenericReport()
        {
            ViewBag.DateFormat = clientSetting.mvnoSettings.dateTimeFormat.ToLower().Replace("yyyy", "yy");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadGenericReport(string OnlineReport)
        {
            GridView gridView = new GridView();
            List<GenericReportTransaction> Failure;
            try
            {
                Failure = JsonConvert.DeserializeObject<List<GenericReportTransaction>>(OnlineReport);
                gridView.DataSource = Failure;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "GenericReport_" + DateTime.Now.ToString("yyyy-MM-ddHHmmss"), this.HttpContext.Response);
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

        public ActionResult transactionhistory()
        {
            ViewBag.DateFormat = clientSetting.mvnoSettings.dateTimeFormat.ToLower().Replace("yyyy", "yy");
            return View();
        }

        [HttpPost]
        public JsonResult MsisdnTransactionDetail(string strTicketIDVal, string strTypeVal, string strStatusVal, string tranFDateVal, string tranToDateVal ,string msisdn)
        {
            CRMSearchTransactionRequest objSearchTransaction = new CRMSearchTransactionRequest();
            CRMSearchTransactionResponse crmSearchTransactionResponse = new CRMSearchTransactionResponse();
            CRMResponse objResp = new CRMResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - MsisdnTransactionDetail Start");
                objSearchTransaction.CountryCode = clientSetting.countryCode;
                objSearchTransaction.BrandCode = clientSetting.brandCode;
                objSearchTransaction.LanguageCode = clientSetting.langCode;
                objSearchTransaction.MSISDN = msisdn;
                objSearchTransaction.TransType = strTypeVal;
                objSearchTransaction.Status = strStatusVal;
                objSearchTransaction.TicketID = strTicketIDVal;
                objSearchTransaction.FromDate = Utility.GetDateconvertion(tranFDateVal, "YYYY-MM-DD", true, clientSetting.mvnoSettings.dateTimeFormat);
                objSearchTransaction.ToDate = Utility.GetDateconvertion(tranToDateVal, "YYYY-MM-DD", true, clientSetting.mvnoSettings.dateTimeFormat);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                crmSearchTransactionResponse = serviceCRM.GetCRMSearchTransaction(objSearchTransaction);
                if (crmSearchTransactionResponse != null && crmSearchTransactionResponse.Response != null && crmSearchTransactionResponse.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_Transaction_" + crmSearchTransactionResponse.Response.ResponseCode);
                    crmSearchTransactionResponse.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? crmSearchTransactionResponse.Response.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - MsisdnTransactionDetail Start");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objSearchTransaction = null;
                serviceCRM = null;
                objResp = null;
                errorInsertMsg = string.Empty;
            }
            return Json(crmSearchTransactionResponse.SearchTransaction, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public void DownLoadSearchTransaction(string transactionData, string transType)
        {
            GridView gridView = new GridView();
            XmlDocument XMLSer = new XmlDocument();
            List<SearchTransaction> TransDetails;
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string colNames = string.Empty;
            StringReader reader;
            List<SearchPartialBalance> objListSearchPartialBalance;
            List<SearchDebit> objListSearchDebit;
            List<SearchRecredit> objListSearchRecredit;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownLoadSearchTransaction Start");
                TransDetails = new JavaScriptSerializer().Deserialize<List<SearchTransaction>>(transactionData);
                if (!string.IsNullOrEmpty(transType))
                {
                    if (transType == "1")
                    {
                        objListSearchDebit = TransDetails.Select(a => new SearchDebit { Id = a.Id, MSISDN = a.MSISDN, DebitAmt = a.DebitAmt, TicketId = a.TicketId, SubmittedBy = a.SubmittedBy, Reason = a.Reason, Comments = a.Comments, RequestDate = a.RequestDate, AuthorisedBy = a.AuthorisedBy, Authoriseddate = a.Authoriseddate, AuthorisedStatus = a.AuthorisedStatus, AdminComments = a.AdminComments }).ToList();
                        XMLSer = Common.CreateXML(objListSearchDebit);
                    }

                    if (transType == "2")
                    {
                        objListSearchRecredit = TransDetails.Select(a => new SearchRecredit { Id = a.Id, MSISDN = a.MSISDN, TicketId = a.TicketId, Reason = a.Reason, Comments = a.Comments, TransferAmt = a.TransferAmt, SubmittedBy = a.SubmittedBy, RequestDate = a.RequestDate, AuthorisedBy = a.AuthorisedBy, Authoriseddate = a.Authoriseddate, AuthorisedStatus = a.AuthorisedStatus, RecreditAmt = a.RecreditAmt, AdminComments = a.AdminComments, SmsDeliveryDate = a.SmsDeliveryDate }).ToList();
                        XMLSer = Common.CreateXML(objListSearchRecredit);
                    }

                    if (transType == "3")
                    {
                        objListSearchPartialBalance = TransDetails.Select(a => new SearchPartialBalance { Id = a.Id, MSISDNFrom = a.MSISDNFrom, MSISDNTo = a.MSISDNTo, TicketId = a.TicketId, Reason = a.Reason, Comments = a.Comments, TransferAmt = a.TransferAmt, SubmittedBy = a.SubmittedBy, RequestDate = a.RequestDate, AuthorisedBy = a.AuthorisedBy, Authoriseddate = a.Authoriseddate, AuthorisedStatus = a.AuthorisedStatus, AdminComments = a.AdminComments }).ToList();
                        XMLSer = Common.CreateXML(objListSearchPartialBalance);
                    }

                    if (transType == "4")
                    {
                        objListSearchPartialBalance = TransDetails.Select(a => new SearchPartialBalance { Id = a.Id, MSISDNFrom = a.MSISDNFrom, MSISDNTo = a.MSISDNTo, TicketId = a.TicketId, Reason = a.Reason, Comments = a.Comments, TransferAmt = a.TransferAmt, SubmittedBy = a.SubmittedBy, RequestDate = a.RequestDate, AuthorisedBy = a.AuthorisedBy, Authoriseddate = a.Authoriseddate, AuthorisedStatus = a.AuthorisedStatus }).ToList();
                        XMLSer = Common.CreateXML(objListSearchPartialBalance);
                    }

                    reader = new StringReader(XMLSer.InnerXml);
                    ds = new DataSet();
                    ds.ReadXml(reader);
                    dt = new DataTable();
                    dt = ds.Tables[0];
                    colNames = string.Empty;

                    // On all tables' columns
                    foreach (DataColumn dc in dt.Columns)
                    {
                        colNames = Resources.SubscriberResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
                        if (colNames != string.Empty && colNames != null)
                            dt.Columns[dc.ColumnName].ColumnName = colNames;
                    }
                    dt.AcceptChanges();
                    gridView.DataSource = dt;
                    gridView.DataBind();

                    Utility.ExportToExcell(gridView, Resources.SubscriberResources.BillingTrans + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownLoadSearchTransaction End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                XMLSer = null;
                dt.Dispose();
                ds.Dispose();
                TransDetails=null;
                colNames = string.Empty;
                reader=null;
                objListSearchPartialBalance=null;
                objListSearchDebit=null;
                objListSearchRecredit=null;
            }
        }

        private string Dateconvert(string DateValue, string Index = "1,0,2")
        {
            string Date = string.Empty;
            string Month = string.Empty;
            string Year = string.Empty;
            try
            {
                string[] Indexsp = Index.Split(',');
                string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
                if (DateValue != null && DateValue != string.Empty)
                {
                    string[] SplitDOB = DateValue.Split('-', '/');
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
            catch (Exception exDateConvert)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exDateConvert);
            }
            return DateValue;
        }

        private string Dateconvert(DateTime DateValue)
        {
            string ReturnVal = string.Empty;
            string Date = string.Empty;
            string Month = string.Empty;
            string Year = string.Empty;
            try
            {
                string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
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
            catch (Exception exDateConvert)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exDateConvert);
            }
            return ReturnVal;
        }


        public ActionResult CallForward()
        {
            CallForwardInfo callInfo = new Models.CallForwardInfo();
            CallForwardingRequest objCallForwardingReq = new CallForwardingRequest();
            CallForwardingResponse objCallForwardingResp = new CallForwardingResponse();
            ServiceInvokeCRM serviceCRM;
            CallForwardingGet objGet = new CallForwardingGet();
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - CallForward Start");
                var strCallForwarding = clientSetting.preSettings.callForwardingMode;
                if (strCallForwarding == "1")
                {
                    objCallForwardingReq.BrandCode = clientSetting.brandCode;
                    objCallForwardingReq.CountryCode = clientSetting.countryCode;
                    objCallForwardingReq.LanguageCode = clientSetting.langCode;
                    objCallForwardingReq.IMSI = Convert.ToString(Session["IMSI"]);
                    if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() == "TRUE")
                    {
                        objCallForwardingReq.MSISDN = Session["MobileNumber"].ToString();
                    }
                    objCallForwardingReq.Mode = "Get";
                    
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    objCallForwardingResp = serviceCRM.CRMCallForwarding(objCallForwardingReq);
                    if (objCallForwardingResp != null && objCallForwardingResp.ResponseDetails != null && objCallForwardingResp.ResponseDetails.ResponseCode != null)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_CallForward_" + objCallForwardingResp.ResponseDetails.ResponseCode);
                        objCallForwardingResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objCallForwardingResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                        callInfo.ResponseCode = objCallForwardingResp.ResponseDetails.ResponseCode;
                        callInfo.ResponseDesc = objCallForwardingResp.ResponseDetails.ResponseDesc;
                    }
                    
                    // 4772 callforward

                    if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
                    {

                    if (objCallForwardingResp != null && objCallForwardingResp.CallForwardingGet.Count() > 0)
                    {
                        callInfo.blCallForward = objCallForwardingResp.CallForwardingGet.FindAll(a => !string.IsNullOrEmpty(a.FWD_TO_NUMBER)).Count > 0;
                        objGet = objCallForwardingResp.CallForwardingGet.Find(a => a.SS_STATUS == "A" && a.SS_CODE == "CFNRY");
                        if ((objGet != null) && (!string.IsNullOrEmpty(objGet.FWD_TO_NUMBER)))
                        {
                            callInfo.strNoAnswer = objGet.FWD_TO_NUMBER.Substring(2);
                            callInfo.blNoAnswer = true;
                        }
                        else
                        {
                            callInfo.strNoAnswer = string.Empty;
                            callInfo.blNoAnswer = false;
                        }
                        objGet = objCallForwardingResp.CallForwardingGet.Find(a => a.SS_STATUS == "A" && a.SS_CODE == "CFU");
                        if ((objGet != null) && (!string.IsNullOrEmpty(objGet.FWD_TO_NUMBER)))
                        {
                            callInfo.strUnconditional = objGet.FWD_TO_NUMBER.Substring(2);
                            callInfo.blUnconditional = true;

                        }
                        else
                        {
                            callInfo.strUnconditional = string.Empty;
                            callInfo.blUnconditional = false;
                        }
                        objGet = objCallForwardingResp.CallForwardingGet.Find(a => a.SS_STATUS == "A" && a.SS_CODE == "CFB");
                        if ((objGet != null) && (!string.IsNullOrEmpty(objGet.FWD_TO_NUMBER)))
                        {
                            callInfo.strBusy = objGet.FWD_TO_NUMBER.Substring(2);
                            callInfo.blBusy = true;
                        }
                        else
                        {
                            callInfo.strBusy = string.Empty;
                            callInfo.blBusy = false;
                        }
                        objGet = objCallForwardingResp.CallForwardingGet.Find(a => a.SS_STATUS == "A" && a.SS_CODE == "CFNRC");

                        if ((objGet != null) && (!string.IsNullOrEmpty(objGet.FWD_TO_NUMBER)))
                        {
                            callInfo.strSwitchOff = objGet.FWD_TO_NUMBER.Substring(2);
                            callInfo.blSwitchOff = true;
                        }
                        else
                        {
                            callInfo.strSwitchOff = string.Empty;
                            callInfo.blSwitchOff = false;
                        }
                    }
                    else
                    {
                        callInfo.ResponseCode = objCallForwardingResp.ResponseDetails.ResponseCode;
                        callInfo.ResponseDesc = objCallForwardingResp.ResponseDetails.ResponseDesc;
                    }
                    }
                    else
                    {
                        if(callInfo.ResponseCode == "0")
                        {
                            if (!string.IsNullOrEmpty(objCallForwardingResp.ConditionCallForwarding) && objCallForwardingResp.ConditionCallForwarding == "SI")
                                callInfo.ConditionCallForwarding = true;
                            callInfo.ConditionCallForwardingMsisdn = objCallForwardingResp.ConditionCallForwardingMsisdn;
                            if (!string.IsNullOrEmpty(objCallForwardingResp.UnConditionCallForwarding) && objCallForwardingResp.UnConditionCallForwarding == "SI")
                                callInfo.UnConditionCallForwarding = true;
                         
                            callInfo.UNConditionCallForwardingMsisdn = objCallForwardingResp.UNConditionCallForwardingMsisdn;
                        }

                }

                    // 4772 
                }
                Session["callInfo"] = callInfo;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - CallForward End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                objGet = null;
                serviceCRM = null;
                objCallForwardingReq = null;
                objCallForwardingResp = null;
                errorInsertMsg = string.Empty;
            }
            return View(callInfo);
        }

        [HttpPost]
        public JsonResult UpdteCallForwarding(bool chkCallFwd, string valUncondition, string valNoAnswer, string valSwitchedOff, string valBusy, bool bolChkUnConditional, bool bolChkNoAnswer, bool bolChkSwitchedOff, bool bolChkBusy, string TicketID, string Reason , string ConditionCallForwarding , string ConditionCallForwardingMsisdn , string UnConditionCallForwarding , string UNConditionCallForwardingMsisdn)
        {
            string strMsisdn = string.Empty;
            string strCall = string.Empty;
            string hdnvalue = string.Empty;
            string strFTN = string.Empty;
            string htShort = string.Empty;
            string tSelectedOptions = string.Empty;
            string CallItemVal = string.Empty;
            string strCallForward = string.Empty;
            string MSISDN = string.Empty;
            string errorInsertMsg = string.Empty;

            CallForwardInfo calInfo = new CallForwardInfo();
            CallForwardingRequest objCrmCallForward = new CallForwardingRequest();
            CallForwardingResponse objResp = new CallForwardingResponse();
            CallForwardInfo fwdInfo = new CallForwardInfo();
            CRMResponse objResponse = new CRMResponse();
            ServiceInvokeCRM serviceCRM;
            System.Collections.Generic.IDictionary<String, String> dicCallForwardCategory = new System.Collections.Generic.Dictionary<string, string>();
            System.Collections.Generic.IDictionary<String, String> strDicCallWaitingCategory = new System.Collections.Generic.Dictionary<string, string>();
            CallForwardInfo callInfo;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - UpdteCallForwarding Start");
                strCallForward = clientSetting.preSettings.callForward;

                if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
                {


                if (!string.IsNullOrEmpty(strCallForward) && strCallForward.Contains('|'))
                {
                    for (int i = 0; i < strCallForward.Split('|').Length; i++)
                    {
                        strDicCallWaitingCategory.Add(strCallForward.Split('|')[i].Split('-')[0].ToString(), strCallForward.Split('|')[i].Split('-')[1]);
                    }
                }
                else if (!string.IsNullOrEmpty(strCallForward))
                {
                    strDicCallWaitingCategory.Add(strCallForward.Split('-')[0].ToString(), strCallForward.Split('-')[1].ToString());
                }
                }

                // 4772 callforward

                if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
                {

                callInfo = (CallForwardInfo)Session["callInfo"];
                foreach (var Item in strDicCallWaitingCategory)
                {
                    objCrmCallForward = new CallForwardingRequest();
                    if ((bolChkUnConditional && Item.Key.ToUpper() == "CFU") || !bolChkUnConditional)
                    {
                        if (((Item.Key.ToUpper() == "CFU") && (callInfo.blUnconditional != bolChkUnConditional || callInfo.strUnconditional != valUncondition)) || (Item.Key.ToUpper() == "CFNRP" && (callInfo.blNoAnswer != bolChkNoAnswer || callInfo.strNoAnswer != valNoAnswer)) || (Item.Key.ToUpper() == "CFNR" && (callInfo.blSwitchOff != bolChkSwitchedOff || callInfo.strSwitchOff != valSwitchedOff)) || (Item.Key.ToUpper() == "CFB" && (callInfo.blBusy != bolChkBusy || callInfo.strBusy != valBusy)))
                        {
                            MSISDN = (Item.Key.ToUpper() == "CFU" && bolChkUnConditional ? valUncondition : Item.Key.ToUpper() == "CFNRP" && bolChkNoAnswer ? valNoAnswer : Item.Key.ToUpper() == "CFNR" && bolChkSwitchedOff ? valSwitchedOff : Item.Key.ToUpper() == "CFB" && bolChkBusy ? valBusy : string.Empty);
                            objCrmCallForward.FTN = MSISDN;
                            if (Item.Key.ToUpper() == "CFU")
                                CallItemVal = "UnConditional";
                            else if (Item.Key.ToUpper() == "CFNRP")
                                CallItemVal = "NoAnswer";
                            else if (Item.Key.ToUpper() == "CFNR")
                                CallItemVal = "SwitchedOff";
                            else if (Item.Key.ToUpper() == "CFB")
                                CallItemVal = "Busy";

                            if (!string.IsNullOrEmpty(tSelectedOptions))
                            {
                                tSelectedOptions += ", " + CallItemVal + "--" + MSISDN;
                            }
                            else
                            {
                                tSelectedOptions = CallItemVal + "--" + MSISDN;
                            }

                            objCrmCallForward.BrandCode = clientSetting.brandCode;
                            objCrmCallForward.CountryCode = clientSetting.countryCode;
                            objCrmCallForward.LanguageCode = clientSetting.langCode;
                            objCrmCallForward.IMSI = Convert.ToString(Session["IMSI"]);
                            objCrmCallForward.Mode = "Update";
                            objCrmCallForward.Operation = Convert.ToString(MSISDN != "" ? "REGISTER" : "ERASE");
                            objCrmCallForward.ServiceShortCode = Item.Value.ToString().Trim();//htShort;
                            objCrmCallForward.NoReplyConTimer = (Item.Value == "42" ? clientSetting.preSettings.noReplCondTimer : string.Empty);
                            objCrmCallForward.SelectedOptions = tSelectedOptions;
                            objCrmCallForward.CallForwardingValue = Convert.ToString(chkCallFwd == true ? "1" : "0");
                            objCrmCallForward.MSISDN = Session["MobileNumber"].ToString();
                            objCrmCallForward.TicketID = TicketID;
                            objCrmCallForward.Reason = Reason;
                            objCrmCallForward.RequestedBy = Session["UserName"].ToString();

                            serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                            objResp = serviceCRM.CRMCallForwarding(objCrmCallForward);
                            if (objResp != null && objResp.ResponseDetails != null && objResp.ResponseDetails.ResponseCode != null)
                            {
                                errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_CallForward_" + objResp.ResponseDetails.ResponseCode);
                                objResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                            }
                        }
                        }
                    }
                }
                else
                {
                    objCrmCallForward = new CallForwardingRequest();
                    objCrmCallForward.BrandCode = clientSetting.brandCode;
                    objCrmCallForward.CountryCode = clientSetting.countryCode;
                    objCrmCallForward.LanguageCode = clientSetting.langCode;
                    objCrmCallForward.IMSI = Convert.ToString(Session["IMSI"]);
                    objCrmCallForward.Mode = "Update";
                    objCrmCallForward.Operation = Convert.ToString(MSISDN != "" ? "REGISTER" : "ERASE");
                    //objCrmCallForward.ServiceShortCode = Item.Value.ToString().Trim();//htShort;
                    //objCrmCallForward.NoReplyConTimer = (Item.Value == "42" ? clientSetting.preSettings.noReplCondTimer : string.Empty);
                    objCrmCallForward.SelectedOptions = tSelectedOptions;
                    objCrmCallForward.CallForwardingValue = Convert.ToString(chkCallFwd == true ? "1" : "0");
                    objCrmCallForward.MSISDN = Session["MobileNumber"].ToString();
                    objCrmCallForward.TicketID = TicketID;
                    objCrmCallForward.Reason = Reason;
                    objCrmCallForward.RequestedBy = Session["UserName"].ToString();
                    objCrmCallForward.ConditionCallForwarding = ConditionCallForwarding;
                    objCrmCallForward.ConditionCallForwardingMsisdn = ConditionCallForwardingMsisdn;
                    objCrmCallForward.UnConditionCallForwarding = UnConditionCallForwarding;
                    objCrmCallForward.UNConditionCallForwardingMsisdn = UNConditionCallForwardingMsisdn;

                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    objResp = serviceCRM.CRMCallForwarding(objCrmCallForward);
                    if (objResp != null && objResp.ResponseDetails != null && objResp.ResponseDetails.ResponseCode != null)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_CallForward_" + objResp.ResponseDetails.ResponseCode);
                        objResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - UpdteCallForwarding End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                strMsisdn = string.Empty;
                strCall = string.Empty;
                hdnvalue = string.Empty;
                strFTN = string.Empty;
                htShort = string.Empty;
                tSelectedOptions = string.Empty;
                CallItemVal = string.Empty;
                strCallForward = string.Empty;
                MSISDN = string.Empty;
                errorInsertMsg = string.Empty;

                calInfo = null;
                objCrmCallForward = null;
                serviceCRM = null;
                fwdInfo = null;
                objResponse = null;
                dicCallForwardCategory = null;
                strDicCallWaitingCategory = null;
                callInfo=null;
            }
            return Json(objResp, JsonRequestBehavior.AllowGet);
        }


        #region Postpaid Network Services -- FRR 3083
        public ActionResult NetworkServicePP()
        {
            PostpaidNetworkFeatureRequest objServiceListReq = new PostpaidNetworkFeatureRequest();
            PostpaidNetworkFeatureResponse objServiceListResp = new PostpaidNetworkFeatureResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - NetworkServicePP Start");
                objServiceListReq.BrandCode = clientSetting.brandCode;
                objServiceListReq.CountryCode = clientSetting.countryCode;
                objServiceListReq.LanguageCode = clientSetting.langCode;
                objServiceListReq.msisdn = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objServiceListResp = serviceCRM.CrmGetProvisionedServices(objServiceListReq);
                if (objServiceListResp != null && objServiceListResp.responseDetails != null && objServiceListResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_NetworkServicePP_" + objServiceListResp.responseDetails.ResponseCode);
                    objServiceListResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objServiceListResp.responseDetails.ResponseDesc : errorInsertMsg;
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - NetworkServicePP End");
                return View(objServiceListResp);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(objServiceListResp);
            }
            finally
            {
                objServiceListReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            
        }


        public ActionResult NetworkServiceListPP(UpdateNetworkFeatureRequest UpdateNetwork)
        {
            CRMResponse objUpdateServiceResp = new CRMResponse();
            UpdateNetworkFeatureRequest objUpdateServiceReq = new UpdateNetworkFeatureRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - NetworkServiceListPP Start");
                objUpdateServiceReq.BrandCode = clientSetting.brandCode;
                objUpdateServiceReq.CountryCode = clientSetting.countryCode;
                objUpdateServiceReq.LanguageCode = clientSetting.langCode;
                objUpdateServiceReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                objUpdateServiceReq.IMSI = Convert.ToString(Session["IMSI"]);
                objUpdateServiceReq.userName = Convert.ToString(Session["UserName"]);
                objUpdateServiceReq.keyValue = UpdateNetwork.keyValue;

                objUpdateServiceReq.ticketId = UpdateNetwork.ticketId;
                objUpdateServiceReq.reason = UpdateNetwork.reason;
                objUpdateServiceReq.FTN = UpdateNetwork.FTN;
                objUpdateServiceReq.adultStatus = UpdateNetwork.adultStatus;
                objUpdateServiceReq.collectcallstatus = UpdateNetwork.collectcallstatus;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objUpdateServiceResp = serviceCRM.UpdateServicePP(objUpdateServiceReq);
                if (objUpdateServiceResp != null && objUpdateServiceResp.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_NetworkServiceListPP_" + objUpdateServiceResp.ResponseCode);
                    objUpdateServiceResp.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objUpdateServiceResp.ResponseDesc : errorInsertMsg;
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - NetworkServiceListPP End");
                return Json(objUpdateServiceResp);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objUpdateServiceResp);
            }
            finally
            {
                objUpdateServiceReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
        }


        public ActionResult NetworkServiceReason(PostPaidNFBlockReasonRequest nwReasonValue)
        {
            PostPaidNFBlockReasonResponse objNWReasonResp = new PostPaidNFBlockReasonResponse();
            PostPaidNFBlockReasonRequest objNFBlockReasonReq = new PostPaidNFBlockReasonRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - NetworkServiceReason Start");
                objNFBlockReasonReq.BrandCode = clientSetting.brandCode;
                objNFBlockReasonReq.CountryCode = clientSetting.countryCode;
                objNFBlockReasonReq.LanguageCode = clientSetting.langCode;
                objNFBlockReasonReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                objNFBlockReasonReq.Type = nwReasonValue.Type;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objNWReasonResp = serviceCRM.GetNWBlockReasonPP(objNFBlockReasonReq);
                if (objNWReasonResp != null && objNWReasonResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_NetworkServiceListPP_" + objNWReasonResp.responseDetails.ResponseCode);
                    objNWReasonResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objNWReasonResp.responseDetails.ResponseDesc : errorInsertMsg;
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - NetworkServiceReason End");
                return Json(objNWReasonResp);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objNWReasonResp);
            }
            finally
            {
                objNFBlockReasonReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            
        }

        #endregion Postpaid Network Services

        #region EDR History -- FRR 3219

        [HttpPost]
        public PartialViewResult EDRHistory(EDRHistoryRequest cdrEDRHistoryReq)
        {
            EDRHistoryResponse EDRResp = new EDRHistoryResponse();
            ServiceInvokeCRM serviceCRM;
            string dateTimeFormat = string.Empty;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - EDRHistory Start");
                cdrEDRHistoryReq.CountryCode = clientSetting.countryCode;
                cdrEDRHistoryReq.BrandCode = clientSetting.brandCode;
                cdrEDRHistoryReq.LanguageCode = clientSetting.langCode;
                cdrEDRHistoryReq.MSISDN = Session["MobileNumber"].ToString();             // 443322110021
                cdrEDRHistoryReq.OldMsisdn = Convert.ToString(Session["SwapMSISDN"]);
                cdrEDRHistoryReq.startDate = Utility.FormatDateTimeToService(cdrEDRHistoryReq.startDate, clientSetting.mvnoSettings.dateTimeFormat); //2015-09-01
                cdrEDRHistoryReq.endDate = Utility.FormatDateTimeToService(cdrEDRHistoryReq.endDate, clientSetting.mvnoSettings.dateTimeFormat);   //2015-09-15

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                EDRResp = serviceCRM.GetEDRHistoryDetails(cdrEDRHistoryReq);
                if (EDRResp.responseDetails.ResponseCode == "0")
                {
                    if (cdrEDRHistoryReq.calltype == "OBARefill")
                    {
                        EDRResp.CallType = "OBARefill";
                    }
                   
                    dateTimeFormat = clientSetting.mvnoSettings.dateTimeFormat;
                    if (EDRResp.CallType == "VASLOAN")
                    {
                        EDRResp.VasLoanDetails.FindAll(a => a.DateAndTime != string.Empty).ForEach(b => b.DateAndTime = Utility.FormatDateTime(b.DateAndTime, dateTimeFormat));

                    }
                   
                    if (EDRResp.CallType == "BundleValueUsage")
                    {
                        EDRResp.BundleValueUsageDetails.FindAll(a => a.DateAndTime != string.Empty).ForEach(b => b.DateAndTime = Utility.FormatDateTime(b.DateAndTime, dateTimeFormat));
                        EDRResp.BundleValueUsageDetails.Where(w => w.ServiceType == "1").ToList().ForEach(s => s.ServiceType = "Balance Transfer");
                    }
                    //PID-52729
                    if (EDRResp.CallType == "RoamingMovement")
                    {
                        EDRResp.EDRRoamingMovementDetails.ForEach(b => b.ToRoaming = !string.IsNullOrEmpty(b.ToRoaming) ? DateTime.ParseExact(b.ToRoaming, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString() : "");
                    }
                    if (EDRResp.CallType == "EDRFAILCASES")
                    {
                        EDRResp.LIST_EDRfailcases.ForEach(b => b.DateTime = !string.IsNullOrEmpty(b.DateTime) ? DateTime.ParseExact(b.DateTime, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString() : "");
                    }
                    if (EDRResp.CallType == "TRANSFERSERVICE")
                    {
                        EDRResp.EdrTransferServicedetails.FindAll(a => a.receiveddatetime != string.Empty).ForEach(b => b.receiveddatetime = Utility.FormatDateTime(b.receiveddatetime, dateTimeFormat));
                        EDRResp.EdrTransferServicedetails.FindAll(a => a.validitydate != string.Empty).ForEach(b => b.validitydate = Utility.FormatDateTime(b.validitydate, dateTimeFormat));
                        EDRResp.EdrTransferServicedetails.FindAll(a => a.transferreddatetime != string.Empty).ForEach(b => b.transferreddatetime = Utility.FormatDateTime(b.transferreddatetime, dateTimeFormat));
                        if (EDRResp.EdrTransferServicedetails != null)
                        {
                            EDRResp.EdrTransferServicedetails.FindAll(a => a.transferredallowance != string.Empty).ForEach(b => b.transferredallowance = Utility.GetConvertBytesToMB(b.transferredallowance, b.Typeofservicetransferred, clientSetting.mvnoSettings.decimalLimit));
                        }
                    }

                    EDRResp.edrHistory.FindAll(a => a.CDRTimeStamp != string.Empty).ForEach(b => b.CDRTimeStamp = Utility.FormatDateTime(b.CDRTimeStamp, dateTimeFormat));
                    EDRResp.edrHistory.FindAll(a => a.PlanChangeDate != string.Empty).ForEach(b => b.PlanChangeDate = Utility.FormatDateTime(b.PlanChangeDate, dateTimeFormat));
                    EDRResp.edrHistory.ForEach(b => b.BundleExpiredOn = Utility.GetDateconvertion(b.BundleExpiredOn, "dd-mm-yyyy HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                    EDRResp.edrHistory.ForEach(b => b.SubscriberResponseDate = Utility.GetDateconvertion(b.SubscriberResponseDate, "dd-mm-yyyy HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                    EDRResp.edrHistory.ForEach(b => b.Dateoftransfer = Utility.GetDateconvertion(b.Dateoftransfer, "dd-mm-yyyy HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                    EDRResp.edrHistory.ForEach(b => b.PriorityChangeDate = Utility.GetDateconvertion(b.PriorityChangeDate, "dd-mm-yyyy HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                    Int32 i = 0;
                    EDRResp.edrHistory.ForEach(a => a.Id = Convert.ToString(i++));
                }
                if (EDRResp != null && EDRResp.responseDetails != null && EDRResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Billing_EDRHistory_" + EDRResp.responseDetails.ResponseCode);
                    EDRResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? EDRResp.responseDetails.ResponseDesc : errorInsertMsg;
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - EDRHistory End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
                dateTimeFormat = string.Empty;
            }
            return PartialView("~/Views/Billing/History/EDRHistory.cshtml", EDRResp);
        }

        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadEdrHistory(string edrData, string filterdata)
        {
            GridView gridView = new GridView();
            XmlDocument XMLSer = new XmlDocument();
            DataSet ds = new DataSet();
            List<EDRHistory> edrHisDetails;
            List<EDRHistory> objList;
            string[] strList;
            StringReader reader;
            DataTable dt=new DataTable();
            string colNames = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownLoadEdrHistory Start");
                if (Convert.ToString(Session["isPrePaid"]) == "1")  // Prepaid
                {
                    edrHisDetails = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<EDRHistory>>(edrData);
                    objList = edrHisDetails.Select(a => new EDRHistory
                    {
                        Id = a.Id,
                        CDRType = a.CDRType,
                        CDRDESC = a.CDRDESC,
                        NetworkId = a.NetworkId,
                        ClientType = a.ClientType,
                        MSISDNNO = a.MSISDNNO,
                        ChildMSISDN = a.ChildMSISDN,
                        CardID = a.CardID,
                        OldPlanId = a.OldPlanId,
                        NewPlanId = a.NewPlanId,
                        OperationCode = a.OperationCode,

                        InitialAccountBalance = a.InitialAccountBalance,
                        AmountTransferred = a.AmountTransferred,
                        FinalAccountBalance = a.FinalAccountBalance,
                        CDRTimeStamp = a.CDRTimeStamp,
                        PlanType = a.PlanType,
                        PlanChangeDate = a.PlanChangeDate,
                        RewardPointType = a.RewardPointType,
                        RewardedPoints = a.RewardedPoints,
                        Amountind = a.Amountind,
                        onnetLimitType = a.onnetLimitType,
                        roamLimitType = a.roamLimitType,
                        onnetLimit = a.onnetLimit,
                        roamLimit = a.roamLimit,
                        onnetActionFlag = a.onnetActionFlag,
                        roamActionFlag = a.roamActionFlag,
                        notifyChannel = a.notifyChannel,
                        onnetmins = a.onnetmins,
                        offnetmins1 = a.offnetmins1,
                        offnetmins2 = a.offnetmins2,
                        offnetmins3 = a.offnetmins3,

                        onnetsms = a.onnetsms,
                        offnetsms1 = a.offnetsms1,
                        offnetsms2 = a.offnetsms2,
                        offnetsms3 = a.offnetsms3,
                        onnetmtmins = a.onnetmtmins,
                        offnetmtmins = a.offnetmtmins,
                        onnetmtsms = a.onnetmtsms,
                        offnetmtsms = a.offnetmtsms,
                        data = a.data,
                        promoamount = a.promoamount,
                        Reason = a.Reason,
                        NewBpartyNumber = a.NewBpartyNumber,
                        ExistingBpartyNumber = a.ExistingBpartyNumber,
                        Vat = a.Vat,
                        Opcode = a.Opcode,
                        BundleExpiredOn = a.BundleExpiredOn,
                        SubscriberResponseDate = a.SubscriberResponseDate,
                        Subresponse = a.Subresponse,
                        RecipientMSISDN = a.RecipientMSISDN,
                        Dateoftransfer = a.Dateoftransfer,
                        DataTransferred = a.DataTransferred,
                        //4181
                        EDRBundleCode = a.EDRBundleCode,
                        ExistingPriority = a.ExistingPriority,
                        NewPriority = a.NewPriority,
                        PriorityChangeDate = a.PriorityChangeDate
                        //    BundleName = a.BundleName
                    }).ToList();
                    strList = filterdata.Split(',');
                    objList = objList.Join(strList, a => a.Id, b => b.ToString(), (a, b) => a).ToList();
                    XMLSer = Common.CreateXML(objList);
                    reader = new StringReader(XMLSer.InnerXml);
                    ds.ReadXml(reader);
                    dt = new DataTable();
                    dt = ds.Tables[0];
                    dt.Columns.Remove("Id");
                    colNames = string.Empty;
                    // On all tables' columns
                    foreach (DataColumn dc in dt.Columns)
                    {
                        try
                        {
                            if (Convert.ToString(dc.ColumnName) == "data")
                            {
                                colNames = Resources.BillingResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
                                if (colNames != string.Empty && colNames != null)
                                    dt.Columns[dc.ColumnName].ColumnName = colNames + " (" + clientSetting.preSettings.billingDataConversion + ") ";
                            }
                            else if (Convert.ToString(dc.ColumnName) == "CDRTimeStamp" || Convert.ToString(dc.ColumnName) == "PlanChangeDate" || Convert.ToString(dc.ColumnName) == "BundleExpiredOn" || Convert.ToString(dc.ColumnName) == "SubscriberResponseDate")
                            {
                                colNames = Resources.BillingResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
                                if (colNames != string.Empty && colNames != null)
                                    dt.Columns[dc.ColumnName].ColumnName = colNames + " " + Resources.BillingResources.ResourceManager.GetString("GMTimezone");
                            }
                            else
                            {
                                colNames = Resources.BillingResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
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
                    gridView.DataBind();
                    Utility.ExportToExcell(gridView, "edrHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownLoadEdrHistory End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                ds.Dispose();
                XMLSer = null;
                dt.Dispose();
                edrHisDetails=null;
                objList=null;
                reader=null;
                colNames = string.Empty;
            }
        }
        #endregion


        #region FRR_4636
        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadEdrVasHistory(string GridData, string filterdata)
        {

            GridView gridView = new GridView();
            XmlDocument XMLSer = new XmlDocument();
            DataSet ds = new DataSet();
            List<VASLoanDetails> edrHisvasDetails;
            List<VASLoanDetails> objList;
            string[] strList;
            StringReader reader;
            DataTable dt = new DataTable();
            string colNames = string.Empty;

            try
            {
                 CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownLoadEdrHistory Start");
                 if (Convert.ToString(Session["isPrePaid"]) == "1")  // Prepaid
                 {
                     edrHisvasDetails = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<VASLoanDetails>>(GridData);

                     objList = edrHisvasDetails.Select(a => new VASLoanDetails
                     {
                         Category = a.Category,
                         BundleCode = a.BundleCode,
                         BundleName = a.BundleName,
                         BundleAmount = a.BundleAmount,
                         DebitAmount = a.DebitAmount,
                         CreditAmount = a.CreditAmount,
                         LoanDue = a.LoanDue,
                         DateAndTime = a.DateAndTime,
                         Requestoy = a.Requestoy,
                         Reason = a.Reason             
                     }).ToList();
                     strList = filterdata.Split(',');
                     //objList = objList.Join(strList, a => a.Category, b => b.ToString(), (a, b) => a).ToList();
                     XMLSer = Common.CreateXML(objList);
                     reader = new StringReader(XMLSer.InnerXml);
                     ds.ReadXml(reader);
                     dt = new DataTable();
                     dt = ds.Tables[0];
                    
                     colNames = string.Empty;
                     foreach (DataColumn dc in dt.Columns)
                     {
                         try
                         {
                             if (Convert.ToString(dc.ColumnName) == "data")
                             {
                                 colNames = Resources.BillingResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
                                 if (colNames != string.Empty && colNames != null)
                                     dt.Columns[dc.ColumnName].ColumnName = colNames + " (" + clientSetting.preSettings.billingDataConversion + ") ";
                             }
                             else if (Convert.ToString(dc.ColumnName) == "Category" || Convert.ToString(dc.ColumnName) == "PlanChangeDate" || Convert.ToString(dc.ColumnName) == "BundleExpiredOn" || Convert.ToString(dc.ColumnName) == "SubscriberResponseDate")
                             {
                                 colNames = Resources.BillingResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
                                 if (colNames != string.Empty && colNames != null)
                                     dt.Columns[dc.ColumnName].ColumnName = colNames + " " + Resources.BillingResources.ResourceManager.GetString("GMTimezone");
                             }
                             else
                             {
                                 colNames = Resources.BillingResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
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
                     gridView.DataBind();
                     Utility.ExportToExcell(gridView, "edrVasHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);


                 }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                ds.Dispose();
                XMLSer = null;
                dt.Dispose();
                edrHisvasDetails = null;
                objList = null;
                reader = null;
                colNames = string.Empty;
            }


        }
        #endregion

        #region FRR 4726
        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadEdrTransHistory(string GridData, string filterdata)
        {

            GridView gridView = new GridView();
            XmlDocument XMLSer = new XmlDocument();
            DataSet ds = new DataSet();
            List<EDRTransferServiceDetails> edrHisvasDetails;
            List<EDRTransferServiceDetails> objList;
            string[] strList;
            StringReader reader;
            DataTable dt = new DataTable();
            string colNames = string.Empty;

            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownLoadEdrHistory Start");
                if (Convert.ToString(Session["isPrePaid"]) == "1")  // Prepaid
                {
                    edrHisvasDetails = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<EDRTransferServiceDetails>>(GridData);

                    objList = edrHisvasDetails.Select(a => new EDRTransferServiceDetails
                    {
                        Transfertype = a.Transfertype,
                        RecipientMSISDN = a.RecipientMSISDN,
                        DonorMSISDN = a.DonorMSISDN,
                        transferredbundle = a.transferredbundle,
                        Typeofservicetransferred = a.Typeofservicetransferred,
                        transferredallowance = a.transferredallowance,
                        Bucket = a.Bucket,
                        receiveddatetime = a.receiveddatetime,
                        transferreddatetime = a.transferreddatetime,
                        validitydate = a.validitydate,
                        ModeofTransfer = a.ModeofTransfer

                    }).ToList();
                    strList = filterdata.Split(',');
                    //objList = objList.Join(strList, a => a.Category, b => b.ToString(), (a, b) => a).ToList();
                    XMLSer = Common.CreateXML(objList);
                    reader = new StringReader(XMLSer.InnerXml);
                    ds.ReadXml(reader);
                    dt = new DataTable();
                    dt = ds.Tables[0];

                    colNames = string.Empty;
                    foreach (DataColumn dc in dt.Columns)
                    {
                        try
                        {
                            if (Convert.ToString(dc.ColumnName) == "data")
                            {
                                colNames = Resources.BillingResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
                                if (colNames != string.Empty && colNames != null)
                                    dt.Columns[dc.ColumnName].ColumnName = colNames + " (" + clientSetting.preSettings.billingDataConversion + ") ";
                            }
                            else if (Convert.ToString(dc.ColumnName) == "Category" || Convert.ToString(dc.ColumnName) == "PlanChangeDate" || Convert.ToString(dc.ColumnName) == "BundleExpiredOn" || Convert.ToString(dc.ColumnName) == "SubscriberResponseDate")
                            {
                                colNames = Resources.BillingResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
                                if (colNames != string.Empty && colNames != null)
                                    dt.Columns[dc.ColumnName].ColumnName = colNames + " " + Resources.BillingResources.ResourceManager.GetString("GMTimezone");
                            }
                            else
                            {
                                colNames = Resources.BillingResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
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
                    gridView.DataBind();
                    Utility.ExportToExcell(gridView, "edrTrasHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);


                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                ds.Dispose();
                XMLSer = null;
                dt.Dispose();
                edrHisvasDetails = null;
                objList = null;
                reader = null;
                colNames = string.Empty;
            }


        }
        #endregion
        #region FRR 4853
        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadEdrBundleValueHistory(string GridData, string filterdata)
        {

            GridView gridView = new GridView();
            XmlDocument XMLSer = new XmlDocument();
            DataSet ds = new DataSet();
            List<BundleValueUsageDetail> edrBundleValueDetails;
            List<BundleValueUsageDetail> objList;
            string[] strList;
            StringReader reader;
            DataTable dt = new DataTable();
            string colNames = string.Empty;

            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownLoadEdrHistory Start");
                if (Convert.ToString(Session["isPrePaid"]) == "1")  // Prepaid
                {
                    edrBundleValueDetails = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<BundleValueUsageDetail>>(GridData);

                    objList = edrBundleValueDetails.Select(a => new BundleValueUsageDetail
                    {
                        BundleCode = a.BundleCode,
                        ServiceType = a.ServiceType,
                        ValueUsed = a.ValueUsed,
                        DateAndTime = a.DateAndTime,
                        Recipient = a.Recipient,
                    }).ToList();
                    strList = filterdata.Split(',');
                    XMLSer = Common.CreateXML(objList);
                    reader = new StringReader(XMLSer.InnerXml);
                    ds.ReadXml(reader);
                    dt = new DataTable();
                    dt = ds.Tables[0];

                    colNames = string.Empty;
                    foreach (DataColumn dc in dt.Columns)
                    {
                        try
                        {
                           
                             if (Convert.ToString(dc.ColumnName) == "DateAndTime")
                            {
                                colNames = Resources.BillingResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
                                if (colNames != string.Empty && colNames != null)
                                    dt.Columns[dc.ColumnName].ColumnName = colNames + " " + Resources.BillingResources.ResourceManager.GetString("GMTimezone");
                            }
                            else
                            {
                                colNames = Resources.BillingResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
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
                    gridView.DataBind();
                    Utility.ExportToExcell(gridView, "edrBundleValueHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);


                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                ds.Dispose();
                XMLSer = null;
                dt.Dispose();
                edrBundleValueDetails = null;
                objList = null;
                reader = null;
                colNames = string.Empty;
            }


        }
        #endregion
        #region VAS History -- FRR 3219
        [HttpPost]
        public PartialViewResult VASHistory(CDRVASHistoryRequest cdrVASHistoryReq)
        {
            CDRVASHistoryResponse VASResp = new CDRVASHistoryResponse();
            ServiceInvokeCRM serviceCRM;
            Int32 i = 0;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - VASHistory start");
                cdrVASHistoryReq.CountryCode = clientSetting.countryCode;
                cdrVASHistoryReq.BrandCode = clientSetting.brandCode;
                cdrVASHistoryReq.LanguageCode = clientSetting.langCode;
                cdrVASHistoryReq.MSISDN = Session["MobileNumber"].ToString();            // 17530000001
                cdrVASHistoryReq.OldMsisdn = Convert.ToString(Session["SwapMSISDN"]);
                cdrVASHistoryReq.startDate = Utility.FormatDateTimeToService(cdrVASHistoryReq.startDate, clientSetting.mvnoSettings.dateTimeFormat);   //2015-09-01
                cdrVASHistoryReq.endDate = Utility.FormatDateTimeToService(cdrVASHistoryReq.endDate, clientSetting.mvnoSettings.dateTimeFormat);    //2015-09-15


                #region FRR 4925
                if(clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    cdrVASHistoryReq.MSISDN = localDict.Where(x => cdrVASHistoryReq.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    cdrVASHistoryReq.OldMsisdn = localDict.Where(x => cdrVASHistoryReq.textdata.ToString().Contains(x.Key)).Select(x => x.Value.SwapMSISDN).First().ToString();
                }
                #endregion 


                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                VASResp = serviceCRM.GetVASHistoryDetails(cdrVASHistoryReq);
                if (VASResp.ResponseDetails.ResponseCode == "0")
                {
                    i = 0;
                    VASResp.cdrVASHistory.ForEach(cd => cd.Id = Convert.ToString(i++));
                    VASResp.cdrVASHistory.FindAll(a => a.CDRTimeStamp != string.Empty).ForEach(b => b.CDRTimeStamp = Utility.FormatDateTime(b.CDRTimeStamp, clientSetting.mvnoSettings.dateTimeFormat));
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - VASHistory End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
                i = 0;
            }
            return PartialView("~/Views/Billing/History/VASHistory.cshtml", VASResp);
        }

        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadVasHistory(string vasData, string filterdata)
        {
            GridView gridView = new GridView();
            XmlDocument XMLSer = new XmlDocument();
            string[] strList;
            List<CDRVASHistory> vasHisDetails;
            StringReader reader;
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string colNames = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownLoadVasHistory Start");
                if (Convert.ToString(Session["isPrePaid"]) == "1")  // Prepaid
                {
                    strList = filterdata.Split(',');
                    vasHisDetails = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<CDRVASHistory>>(vasData);
                    vasHisDetails = vasHisDetails.Join(strList, a => a.Id, b => b.ToString(), (a, b) => a).ToList();

                    XMLSer = Common.CreateXML(vasHisDetails);

                    reader = new StringReader(XMLSer.InnerXml);
                    ds = new DataSet();
                    ds.ReadXml(reader);
                    dt = new DataTable();
                    dt = ds.Tables[0];
                    dt.Columns.Remove("Id");

                    colNames = String.Empty;
                    foreach (DataColumn dc in dt.Columns)
                    {
                        try
                        {
                            colNames = Resources.BillingResources.ResourceManager.GetString(Convert.ToString(dc.ColumnName));
                                if (colNames != string.Empty && colNames != null)
                                    dt.Columns[dc.ColumnName].ColumnName = colNames;                            
                        }
                        catch (Exception ex)
                        {
                            CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                        }
                    }
                    dt.AcceptChanges();
                    gridView.DataSource = dt;
                }
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "vasHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownLoadVasHistory End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                XMLSer = null;
                strList=null;
                vasHisDetails=null;
                reader=null;
                ds.Dispose();
                dt.Dispose();
            }
        }
        #endregion

        #region Credit / Debit Transaction

        public ActionResult CreditDebitTransaction()
        {
            DoCreditDebitRes ObjResp = new DoCreditDebitRes();
            DOCreditDebitReq ObjReq = new DOCreditDebitReq();
            string dummystring = string.Empty;
            string ResourceMsg = string.Empty;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - CreditDebitTransaction Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.MSISDN = Session["MobileNumber"].ToString();

                #region Pending to Credit Debit Transfer
                if (Session["PAType"] != null && (Convert.ToString(Session["PAType"]).ToUpper() == "CREDIT" || Convert.ToString(Session["PAType"]) == "DEBIT"))
                {
                    ObjReq.Mode = "3";
                    ObjReq.ReqID = Convert.ToString(Session["PAId"]);
                    ObjReq.Type = Convert.ToString(Session["PAType"]).ToUpper() == "CREDIT" ? "1" : "2";
                }
                else
                {
                    ObjReq.Mode = "1";
                }
                #endregion
                ObjResp = crmNewService.CRMDoCreditDebit(ObjReq);

                #region MULTI LANGUAGE
                if (ObjResp != null && ObjResp.CReditReasonList != null && ObjResp.CReditReasonList.Count > 0)
                {
                    for (int i = 0; i < ObjResp.CReditReasonList.Count; i++)
                    {
                        dummystring = System.Text.RegularExpressions.Regex.Replace(ObjResp.CReditReasonList[i].ReasonDesc, "[^0-9a-zA-Z]+", string.Empty);
                        ResourceMsg = Resources.DropdownResources.ResourceManager.GetString(dummystring);
                        ObjResp.CReditReasonList[i].ReasonDesc = string.IsNullOrEmpty(ResourceMsg) ? ObjResp.CReditReasonList[i].ReasonDesc : ResourceMsg;
                    }
                }
                if (ObjResp != null && ObjResp.DebitReasonList != null && ObjResp.DebitReasonList.Count > 0)
                {
                    for (int i = 0; i < ObjResp.DebitReasonList.Count; i++)
                    {
                        dummystring = System.Text.RegularExpressions.Regex.Replace(ObjResp.DebitReasonList[i].ReasonDesc, "[^0-9a-zA-Z]+", string.Empty);
                        ResourceMsg = Resources.DropdownResources.ResourceManager.GetString(dummystring);
                        ObjResp.DebitReasonList[i].ReasonDesc = string.IsNullOrEmpty(ResourceMsg) ? ObjResp.DebitReasonList[i].ReasonDesc : ResourceMsg;
                    }
                }

                #endregion

                #region Pending to Credit Debit Transfer

                if (Session["PAType"] != null && (Convert.ToString(Session["PAType"]).ToUpper() == "CREDIT" || Convert.ToString(Session["PAType"]) == "DEBIT"))
                {
                    ObjResp.PAType = "TRUE";
                    ObjResp.ReqID = Convert.ToString(Session["PAId"]);
                    ObjResp.Type = Convert.ToString(Session["PAType"]).ToUpper() == "CREDIT" ? "CREDIT" : "DEBIT";
                    if (ObjResp.ReCreditDetails != null)
                    {
                    if (ObjResp.ReCreditDetails.ReasonDesc != null)
                    {
                        dummystring = System.Text.RegularExpressions.Regex.Replace(ObjResp.ReCreditDetails.ReasonDesc, "[^0-9a-zA-Z]+", string.Empty);
                        ResourceMsg = Resources.DropdownResources.ResourceManager.GetString(dummystring);
                        ObjResp.ReCreditDetails.ReasonDesc = string.IsNullOrEmpty(ResourceMsg) ? ObjResp.ReCreditDetails.ReasonDesc : ResourceMsg;
                        }
                    }
                }
                else
                {
                    ObjResp.PAType = "FALSE";
                    ObjResp.ReqID = Convert.ToString(Session["PAId"]);
                    ObjResp.Type = Convert.ToString(Session["PAType"]).ToUpper() == "CREDIT" ? "CREDIT" : "DEBIT";
                }

                #endregion

                if (ObjResp != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CreditDebit_" + ObjResp.ResponseDeatils.ResponseCode);
                    ObjResp.ResponseDeatils.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.ResponseDeatils.ResponseDesc : errorInsertMsg;
                }
                Session["PAType"] = null;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - CreditDebitTransaction End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                ObjResp = new DoCreditDebitRes();
                ObjResp.ResponseDeatils = new CRMResponse();
                ObjResp.ResponseDeatils.ResponseCode = "2";
                ObjResp.ResponseDeatils.ResponseDesc = ex.ToString();
            }
            finally
            {
               // ObjResp = null;
                ObjReq = null;
                dummystring = string.Empty;
                ResourceMsg = string.Empty;
            }
            return View(ObjResp);
        }

        [HttpPost]
        public JsonResult SaveCreditDebitTransaction(string strCreditDebitTransaction)
        {
            DOCreditDebitReq objReg = JsonConvert.DeserializeObject<DOCreditDebitReq>(strCreditDebitTransaction);
            DoCreditDebitRes objRes = new DoCreditDebitRes();
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - SaveCreditDebitTransaction Start");
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                if (objReg.Type == "1")
                {
                    objReg.PARequestType = Resources.BillingResources.Credit;
                }
                else
                {
                    objReg.PARequestType = Resources.BillingResources.Debit;
                }
                if (objReg.DialledDate != null)
                {
                    objReg.DialledDate = Utility.GetDateconvertion(objReg.DialledDate, "YYYY/MM/DD", true, clientSetting.mvnoSettings.dateTimeFormat);
                }
                objRes = crmNewService.CRMDoCreditDebit(objReg);
                if (objRes != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CreditDebit_" + objRes.ResponseDeatils.ResponseCode);
                    objRes.ResponseDeatils.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDeatils.ResponseDesc : errorInsertMsg;
                    objRes.ReCreditDetailsList.ForEach(b => b.RequestDate = Utility.GetDateconvertion(b.RequestDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                    objRes.ReCreditDetailsList.ForEach(b => b.AuthDate = Utility.GetDateconvertion(b.AuthDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                    objRes.ReCreditDetailsList.ForEach(b => b.DialledDate = Utility.GetDateconvertion(b.DialledDate, "yyyy/mm/dd", false, clientSetting.mvnoSettings.dateTimeFormat));
                    objRes.ReCreditDetailsList.ForEach(b => b.SmsDeliveryDate = Utility.GetDateconvertion(b.SmsDeliveryDate, "yyyy/mm/dd", false, clientSetting.mvnoSettings.dateTimeFormat));
                }
                else
                {
                    objRes = new DoCreditDebitRes();
                    objRes.ResponseDeatils.ResponseDesc = Resources.ErrorResources.Common_2;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.ResponseDeatils.ResponseDesc);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - SaveCreditDebitTransaction End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes = new DoCreditDebitRes();
                objRes.ResponseDeatils = new CRMResponse();
                objRes.ResponseDeatils.ResponseCode = "2";
                objRes.ResponseDeatils.ResponseDesc = ex.ToString();
            }
            finally
            {
                objReg = null;
                errorInsertMsg = string.Empty;
            }
            return new JsonResult() { Data = objRes, MaxJsonLength = int.MaxValue };
        }

        #endregion

        [HttpPost]
        public PartialViewResult SpecificHistoryEDR(EDRSpecificRequest edrSpecificReq)
        {
            EDRSpecificResponse edrSpecificResp = new EDRSpecificResponse();
            EDRHistoryResponse edrHistoryResp = new EDRHistoryResponse();
            ServiceInvokeCRM serviceCRM;
            string dateTimeFormat = string.Empty;
            Int32 i = 0;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - SpecificHistoryEDR Start");
                edrSpecificReq.CountryCode = clientSetting.countryCode;
                edrSpecificReq.BrandCode = clientSetting.brandCode;
                edrSpecificReq.LanguageCode = clientSetting.langCode;
                edrSpecificReq.MSISDN = Session["MobileNumber"].ToString();             // 443322110021
                edrSpecificReq.oldMSISDN = Convert.ToString(Session["SwapMSISDN"]);
                //cdrEDRHistoryReq.OldMsisdn = string.Empty;
                edrSpecificReq.fromDate = Utility.GetDateconvertion(edrSpecificReq.fromDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
                edrSpecificReq.toDate = Utility.GetDateconvertion(edrSpecificReq.toDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
                //edrSpecificReq.fromDate = Utility.FormatDateTimeToService(edrSpecificReq.fromDate, clientSetting.mvnoSettings.dateTimeFormat); //2015-09-01
                //edrSpecificReq.toDate = Utility.FormatDateTimeToService(edrSpecificReq.toDate, clientSetting.mvnoSettings.dateTimeFormat);   //2015-09-15
                
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                edrSpecificResp = serviceCRM.SpecificEDR(edrSpecificReq);
                if (edrSpecificResp.responseDetails.ResponseCode == "0")
                {
                    dateTimeFormat = clientSetting.mvnoSettings.dateTimeFormat;
                    edrSpecificResp.edrHistory.ForEach(b =>
                    {
                        try
                        {
                            b.CDRTimeStamp = string.IsNullOrEmpty(b.CDRTimeStamp) ? string.Empty : Utility.FormatDateTime(b.CDRTimeStamp, dateTimeFormat);
                            b.PlanChangeDate = string.IsNullOrEmpty(b.PlanChangeDate) ? string.Empty : Utility.FormatDateTime(b.PlanChangeDate, dateTimeFormat);
                            b.BundleExpiredOn = string.IsNullOrEmpty(b.BundleExpiredOn) ? string.Empty : Utility.FormatDateTime(b.BundleExpiredOn, dateTimeFormat);
                            b.SubscriberResponseDate = string.IsNullOrEmpty(b.SubscriberResponseDate) ? string.Empty : Utility.FormatDateTime(b.SubscriberResponseDate, dateTimeFormat);
                        }
                        catch (Exception exServiceInvoke)
                        {
                            CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exServiceInvoke);
                        }
                    });
                    i = 0;
                    edrSpecificResp.edrHistory.ForEach(a => a.Id = Convert.ToString(i++));
                }
                edrHistoryResp.responseDetails = edrSpecificResp.responseDetails;
                edrHistoryResp.edrHistory = edrSpecificResp.edrHistory;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - SpecificHistoryEDR End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
                edrSpecificResp = null;
                dateTimeFormat = string.Empty;
                i = 0;
            }
            return PartialView("~/Views/Billing/History/EDRHistory.cshtml", edrHistoryResp);
        }

        public ViewResult Tracker()
        {
            BillingTrackerResponse billTracker = new BillingTrackerResponse();
            BillingTrackerRequest objreq = new BillingTrackerRequest();
            CRMActivationDateResponse actDateResp = new CRMActivationDateResponse();
            CRMActivationDateRequest actDateReq = new CRMActivationDateRequest();
            ServiceInvokeCRM serviceCRM;
            DateTime date = DateTime.Now;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - Tracker Start");
                actDateReq.CountryCode = clientSetting.countryCode;
                actDateReq.BrandCode = clientSetting.brandCode;
                actDateReq.LanguageCode = clientSetting.langCode;
                actDateReq.MSISDN = Session["MobileNumber"].ToString();
                actDateReq.Iccid = Convert.ToString(Session["ICCID"]);

                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.msisdn = Session["MobileNumber"].ToString();

                objreq.mode = "Q";
                
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                actDateResp = serviceCRM.GetCRMActivationDate(actDateReq);
                billTracker = serviceCRM.CRMBillingTracker(objreq);

                billTracker.lstCallTypes = Utility.DataTableToList(Utility.GetDropdownValueforHistory("tbl_cdr_call_type"));
                billTracker.activationDate = string.IsNullOrEmpty(actDateResp.activationDate) ? DateTime.Now.AddYears(-1).ToString("yyyy/MM/dd") : Convert.ToDateTime(actDateResp.activationDate).ToString("yyyy/MM/dd");

                try
                {
                    date = DateTime.Now;
                    var formattedDate = string.Format(new MyCustomDateProvider(), "{0}", date);
                    billTracker.tdyDate = formattedDate.ToString();
                }
                catch (Exception exDateConversion)
                {
                    billTracker.tdyDate = DateTime.Now.ToString();
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exDateConversion);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - Tracker End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                objreq = null;
                actDateResp = null;
                actDateReq = null;
            }
            return View(billTracker);
        }

        [HttpPost]
        public JsonResult LoadBillingTracker(string billTracker)
        {
            BillingTrackerResponse objRes = new BillingTrackerResponse();
            BillingTrackerRequest objreq = JsonConvert.DeserializeObject<BillingTrackerRequest>(billTracker);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            Int32 i = 0;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - LoadBillingTracker Start");
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.msisdn = Session["MobileNumber"].ToString();
                objreq.oldNo = Convert.ToString(Session["SwapMSISDN"]);
                objreq.mask = Session["MaskMode"].ToString();
                objreq.ICCID = Session["ICCID"].ToString();

                objreq.fromDate = Utility.GetDateconvertion(objreq.fromDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
                objreq.toDate = Utility.GetDateconvertion(objreq.toDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.CRMBillingTracker(objreq);

                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Tracker_" + objRes.responseDetails.ResponseCode);
                    objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
                }

                try
                {
                    if (objRes != null && objRes.billingTracker.Count > 0)
                    {
                        i = 0;
                        objRes.billingTracker.ForEach(cd => cd.Id = Convert.ToString(i++));
                        objRes.billingTracker.ForEach(m => m.CallDate = Utility.FormatDateTime(m.CallDate, clientSetting.mvnoSettings.dateTimeFormat));
                    }

                    if (objRes != null && objRes.billingTrackerSummary != null && objRes.billingTrackerSummary.trackerOtherCharges != null && objRes.billingTrackerSummary.trackerOtherCharges.trackerOCBundle != null)
                    {
                        if (objRes.billingTrackerSummary.trackerOtherCharges.trackerOCBundle.Count > 0)
                        {
                            objRes.billingTrackerSummary.trackerOtherCharges.trackerOCBundle.ForEach(m => m.topupDate = Utility.FormatDateTime(m.topupDate, clientSetting.mvnoSettings.dateTimeFormat));
                        }
                    }

                    if (objRes != null && objRes.billingTrackerSummary != null && objRes.billingTrackerSummary.trackerOtherCharges != null && objRes.billingTrackerSummary.trackerOtherCharges.trackerOCTopup != null)
                    {
                        if (objRes.billingTrackerSummary.trackerOtherCharges.trackerOCTopup.Count > 0)
                        {
                            objRes.billingTrackerSummary.trackerOtherCharges.trackerOCTopup.ForEach(m => m.topupDate = Utility.FormatDateTime(m.topupDate, clientSetting.mvnoSettings.dateTimeFormat));
                        }
                    }
                }
                catch (Exception exBillingTrackerSummary)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exBillingTrackerSummary);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - LoadBillingTracker End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
                objreq = null;
                errorInsertMsg = string.Empty;
                i = 0;
            }
            return new JsonResult() { Data = objRes, MaxJsonLength = int.MaxValue };
        }

        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadBillingTracker(string billingData, string filterdata)
        {
            XmlDocument XMLSer = new XmlDocument();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            GridView gridView = new GridView();
            string[] strList;
            List<BillingTracker> vasHisDetails;
            StringReader reader;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - LoadBillingTracker Start");
                strList = filterdata.Split(',');
                vasHisDetails = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<BillingTracker>>(billingData);
                vasHisDetails = vasHisDetails.Join(strList, a => a.Id, b => b.ToString(), (a, b) => a).ToList();
                XMLSer = Common.CreateXML(vasHisDetails);
                reader = new StringReader(XMLSer.InnerXml);
                ds.ReadXml(reader);
                dt = ds.Tables[0];
                dt.Columns.Remove("Id");
                dt.AcceptChanges();
                //List<BillingTracker> billHisDetails = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<BillingTracker>>(billingData);
                gridView.DataSource = dt;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "BillingTracker_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - LoadBillingTracker End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                XMLSer = null;
                ds.Dispose();
                dt.Dispose();
                strList=null;
                vasHisDetails=null;
                reader=null;
            }
        }

        [HttpPost]
        public void LoadNonMaskBillingTracker(FormCollection formNonMaskBill)
        {
            GridView gridView = new GridView();
            BillingTrackerResponse objRes = new BillingTrackerResponse();
            BillingTrackerRequest crmBillHistReq = new BillingTrackerRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - LoadNonMaskBillingTracker Start");
                crmBillHistReq.CountryCode = clientSetting.countryCode;
                crmBillHistReq.BrandCode = clientSetting.brandCode;
                crmBillHistReq.LanguageCode = clientSetting.langCode;

                crmBillHistReq.msisdn = Session["MobileNumber"].ToString();
                crmBillHistReq.oldNo = Convert.ToString(Session["SwapMSISDN"]);
                crmBillHistReq.mask = "0";
                crmBillHistReq.callType = formNonMaskBill["callType"].Trim();
                crmBillHistReq.subType = formNonMaskBill["subCallType"].Trim();
                crmBillHistReq.mode = "T";

                crmBillHistReq.fromDate = Utility.GetDateconvertion(formNonMaskBill["fromDate"].Trim(), clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
                crmBillHistReq.toDate = Utility.GetDateconvertion(formNonMaskBill["toDate"].Trim(), clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.CRMBillingTracker(crmBillHistReq);

                if (objRes.billingTracker.Count > 0)
                {
                    try
                    {
                        objRes.billingTracker.ForEach(m => m.CallDate = Utility.FormatDateTime(m.CallDate, clientSetting.mvnoSettings.dateTimeFormat));
                    }
                    catch (Exception exDateConversion)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exDateConversion);
                    }
                    gridView.DataSource = objRes.billingTracker;
                    gridView.DataBind();
                    Utility.ExportToExcell(gridView, "BillingTracker_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - LoadNonMaskBillingTracker End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                crmBillHistReq = null;
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult SaveBillingTracker(string tablePDFDate, string tracker, string fDate, string tDate, string emailID)
        {
            BillingTrackerResponse objRes = new BillingTrackerResponse();
            BillingTrackerRequest objreq = new BillingTrackerRequest();
            Document pdfDoc = new Document();
            ServiceInvokeCRM serviceCRM;
            StyleSheet styles = new StyleSheet();
            string FolderName = string.Empty;
            string PDFName = string.Empty;
            string PDFfileName = string.Empty;
            string logoPath = string.Empty;
            Font titleFont1;
            Paragraph text1;
            string strDate = string.Empty;
            DateTime date = DateTime.Now;
            Font titleFont;
            Paragraph text;
            ArrayList htmlArrList;
            ArrayList htmlArrList1;
            string strName = string.Empty;
            string[] paranamEmail;
            string[] paraValueEmail;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - SaveBillingTracker Start");
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.msisdn = Session["MobileNumber"].ToString();
                objreq.mode = "E";

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.CRMBillingTracker(objreq);

                FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                var filepath = Path.Combine(clientSetting.preSettings.emailAttachmentPath, FolderName);
                if (Directory.Exists(filepath))
                {
                    Array.ForEach(Directory.GetFiles(filepath), System.IO.File.Delete);
                }
                
                pdfDoc.NewPage();
                pdfDoc.SetPageSize(PageSize.A4);

                PDFName = Session["MobileNumber"].ToString() + "_" + DateTime.Now.ToString("ddMMyyyy_hh_mm_ss") + ".pdf";
                PDFfileName = filepath + "\\" + PDFName;
                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }
                PdfWriter.GetInstance(pdfDoc, new FileStream(PDFfileName, FileMode.Create));
                pdfDoc.Open();

                logoPath = @"~\Library\DefaultTheme\Images\LogoPdf.png";
                iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(Server.MapPath(logoPath));
                pdfImage.ScaleToFit(100, 80);
                pdfImage.Alignment = iTextSharp.text.Image.ALIGN_RIGHT;
                pdfImage.SetAbsolutePosition(450, 770);
                pdfImage.SpacingAfter = 20f;
                pdfDoc.Add(pdfImage);

                titleFont1 = FontFactory.GetFont("Arial", 20);
                text1 = new Paragraph("", titleFont1);
                text1.Alignment = Element.ALIGN_RIGHT;
                text1.SpacingAfter = 20f;
                pdfDoc.Add(text1);
                pdfDoc.Add(text1);
                pdfDoc.Add(text1);

                strDate = string.Empty;
                try
                {
                    date = DateTime.Now;
                    var formattedDate = string.Format(new MyCustomDateProvider(), "{0}", date);
                    strDate = formattedDate.ToString();
                }
                catch (Exception exDateConversion)
                {
                    strDate = DateTime.Now.ToString();
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exDateConversion);
                }

                titleFont = FontFactory.GetFont("Arial", 8);
                text = new Paragraph(strDate, titleFont);
                text.Alignment = Element.ALIGN_RIGHT;
                pdfDoc.Add(text);

                tablePDFDate = tablePDFDate.Replace(@"\n", string.Empty);
                tablePDFDate = tablePDFDate.Replace(@"\", string.Empty);
                htmlArrList = HTMLWorker.ParseToList(new StringReader(tablePDFDate.Substring(1, tablePDFDate.Length - 2)), styles);
                foreach (IElement strLn in htmlArrList)
                {
                    pdfDoc.Add(strLn);
                }
                pdfDoc.SetPageSize(PageSize.A4.Rotate());
                pdfDoc.NewPage();

                tracker = tracker.Replace(@"\n", string.Empty);
                tracker = tracker.Replace(@"\", string.Empty);
                htmlArrList1 = HTMLWorker.ParseToList(new StringReader(tracker.Substring(1, tracker.Length - 2)), styles);
                foreach (IElement strLn in htmlArrList1)
                {
                    pdfDoc.Add(strLn);
                }

                pdfDoc.Close();


                strName = string.Empty;
                if (objRes != null && !string.IsNullOrEmpty(objRes.firstName) && !string.IsNullOrEmpty(objRes.lastName))
                {
                    strName = objRes.firstName + " " + objRes.lastName;
                }
                else
                {
                    strName = "Customer";
                }

                paranamEmail = new string[] { "##FROMDATE##", "##TODATE##", "##NAME##" };
                paraValueEmail = new string[] { fDate, tDate, strName };
                objRes.responseDetails = new CRMResponse();
                if (Utility.CRMEmail(clientSetting, emailID, "Billing Tracker", paranamEmail, paraValueEmail, "BILLINGTRACKER", objRes.EmailLanguage, PDFfileName))
                {
                    objRes.responseDetails.ResponseCode = "40";
                    objRes.responseDetails.ResponseDesc = "Email sent success";
                }
                else
                {
                    objRes.responseDetails.ResponseCode = "41";
                    objRes.responseDetails.ResponseDesc = "Email sent failed";
                }

                if (Directory.Exists(filepath))
                {
                    Array.ForEach(Directory.GetFiles(filepath), System.IO.File.Delete);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - SaveBillingTracker End");
            }
            catch (System.Threading.ThreadAbortException)
            {
                pdfDoc.Close();
            }
            catch (Exception ex)
            {
                pdfDoc.Close();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                objreq = null;

                styles = null;
                FolderName = string.Empty;
                PDFName = string.Empty;
                PDFfileName = string.Empty;
                logoPath = string.Empty;
                titleFont1=null;
                text1=null;
                strDate = string.Empty;
                titleFont=null;
                text=null;
                htmlArrList=null;
                htmlArrList1=null;
                strName = string.Empty;
                paranamEmail=null;
                paraValueEmail=null;
            }
            return Json(objRes);
        }

        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadBillingTrackerPDF(string tablePDFDate)
        {
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownLoadBillingTrackerPDF Start");
                ExportToPDF(tablePDFDate, "BillingTracker_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownLoadBillingTrackerPDF End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
        }

        public void ExportToPDF(string HTML, string fileName, HttpResponseBase httpResponseBase)
        {
            Document pdfDoc;
            StyleSheet styles = new StyleSheet();
            string logoPath = string.Empty;
            Font titleFont1;
            Paragraph text1;
            string strDate = string.Empty;
            DateTime date = DateTime.Now;
            Font titleFont;
            Paragraph text;
            ArrayList htmlArrList;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - ExportToPDF Start");
                pdfDoc = new Document(PageSize.A4, 50, 50, 20, 20);
                
                httpResponseBase.ContentType = "application/pdf";
                httpResponseBase.AddHeader("content-disposition", "attachment; filename=" + fileName + "_" + DateTime.Now.ToString() + ".pdf");
                httpResponseBase.AddHeader("Pragma", "public");
                httpResponseBase.Cache.SetCacheability(HttpCacheability.NoCache);

                PdfWriter.GetInstance(pdfDoc, httpResponseBase.OutputStream);
                pdfDoc.Open();

                logoPath = @"~\Library\DefaultTheme\Images\LogoPdf.png";
                iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(Server.MapPath(logoPath));
                pdfImage.ScaleToFit(100, 80);
                pdfImage.Alignment = iTextSharp.text.Image.ALIGN_RIGHT;
                pdfImage.SetAbsolutePosition(450, 770);
                pdfImage.SpacingAfter = 20f;
                pdfDoc.Add(pdfImage);

                titleFont1 = FontFactory.GetFont("Arial", 20);
                text1 = new Paragraph("", titleFont1);
                text1.Alignment = Element.ALIGN_RIGHT;
                text1.SpacingAfter = 20f;
                pdfDoc.Add(text1);
                pdfDoc.Add(text1);
                pdfDoc.Add(text1);

                strDate = string.Empty;
                try
                {
                    date = DateTime.Now;
                    var formattedDate = string.Format(new MyCustomDateProvider(), "{0}", date);
                    strDate = formattedDate.ToString();
                }
                catch (Exception exDateConversion)
                {
                    strDate = DateTime.Now.ToString();
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exDateConversion);
                }
                titleFont = FontFactory.GetFont("Arial", 8);
                text = new Paragraph(strDate, titleFont);
                text.Alignment = Element.ALIGN_RIGHT;
                pdfDoc.Add(text);

                HTML = HTML.Replace(@"\n", string.Empty);
                HTML = HTML.Replace(@"\", string.Empty);
                //HTML = HTML.Replace(@"px", string.Empty);
                htmlArrList = HTMLWorker.ParseToList(new StringReader(HTML.Substring(1, HTML.Length - 2)), styles);
                foreach (IElement strLn in htmlArrList)
                {
                    pdfDoc.Add(strLn);
                }
                pdfDoc.Close();

                httpResponseBase.Write(pdfDoc);
                httpResponseBase.End();
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - ExportToPDF End");
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
                styles = null;
                logoPath = string.Empty;
                titleFont1=null;
                text1=null;
                strDate = string.Empty;
                titleFont=null;
                text=null;
                htmlArrList=null;
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public void DownloadInvoicePDF(string trackerSummary, string trackerDetails)
        {
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownloadInvoicePDF Start");
                InvoicePDF(trackerSummary, trackerDetails, "BillingTracker_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - DownloadInvoicePDF End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
        }

        public void InvoicePDF(string summary, string tracker, string fileName, HttpResponseBase httpResponseBase)
        {
            Document pdfDoc = new Document();
            StyleSheet styles = new StyleSheet();
            string logoPath = string.Empty;
            Font titleFont1;
            Paragraph text1;
            string strDate = string.Empty;
            DateTime date = DateTime.Now;
            Font titleFont;
            Paragraph text;
            ArrayList htmlArrList;
            ArrayList htmlArrList1;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - InvoicePDF start");
                pdfDoc.NewPage();
                pdfDoc.SetPageSize(PageSize.A4);
                httpResponseBase.ContentType = "application/pdf";
                httpResponseBase.AddHeader("content-disposition", "attachment; filename=" + fileName + "_" + DateTime.Now.ToString() + ".pdf");
                httpResponseBase.AddHeader("Pragma", "public");
                httpResponseBase.Cache.SetCacheability(HttpCacheability.NoCache);

                PdfWriter.GetInstance(pdfDoc, httpResponseBase.OutputStream);
                pdfDoc.Open();

                logoPath = @"~\Library\DefaultTheme\Images\LogoPdf.png";
                iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(Server.MapPath(logoPath));
                pdfImage.ScaleToFit(100, 80);
                pdfImage.Alignment = iTextSharp.text.Image.ALIGN_RIGHT;
                pdfImage.SetAbsolutePosition(450, 770);
                pdfImage.SpacingAfter = 20f;
                pdfDoc.Add(pdfImage);

                titleFont1 = FontFactory.GetFont("Arial", 20);
                text1 = new Paragraph("", titleFont1);
                text1.Alignment = Element.ALIGN_RIGHT;
                text1.SpacingAfter = 20f;
                pdfDoc.Add(text1);
                pdfDoc.Add(text1);
                pdfDoc.Add(text1);

                strDate = string.Empty;
                try
                {
                    date = DateTime.Now;
                    var formattedDate = string.Format(new MyCustomDateProvider(), "{0}", date);
                    strDate = formattedDate.ToString();
                }
                catch (Exception exDateConversion)
                {
                    strDate = DateTime.Now.ToString();
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exDateConversion);
                }
                titleFont = FontFactory.GetFont("Arial", 8);
                text = new Paragraph(strDate, titleFont);
                text.Alignment = Element.ALIGN_RIGHT;
                pdfDoc.Add(text);

                summary = summary.Replace(@"\n", string.Empty);
                summary = summary.Replace(@"\", string.Empty);
                //HTML = HTML.Replace(@"px", string.Empty);
                htmlArrList = HTMLWorker.ParseToList(new StringReader(summary.Substring(1, summary.Length - 2)), styles);
                foreach (IElement strLn in htmlArrList)
                {
                    pdfDoc.Add(strLn);
                }

                pdfDoc.SetPageSize(PageSize.A4.Rotate());
                pdfDoc.NewPage();

                tracker = tracker.Replace(@"\n", string.Empty);
                tracker = tracker.Replace(@"\", string.Empty);
                htmlArrList1 = HTMLWorker.ParseToList(new StringReader(tracker.Substring(1, tracker.Length - 2)), styles);
                foreach (IElement strLn in htmlArrList1)
                {
                    pdfDoc.Add(strLn);
                }

                pdfDoc.Close();

                httpResponseBase.Write(pdfDoc);
                httpResponseBase.End();
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "BillingController - InvoicePDF End");
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
                styles = null;
                logoPath = string.Empty;
                titleFont1=null;
                text1=null;
                strDate = string.Empty;
                titleFont=null;
                text=null;
                htmlArrList=null;
                htmlArrList1=null;
            }
        }
        #region Billing Promotional EDR
        public JsonResult LoadEDR(string FailureDatails)
        {
            EDRResponse ObjResp = new EDRResponse();
            EDRRequest ObjReq = JsonConvert.DeserializeObject<EDRRequest>(FailureDatails);
            string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            string strGetDate = "", strDate = "", strMonth = "", strYear = "";
            string[] strSplit;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode; 
                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {

                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    ObjReq.Msisdn = localDict.Where(x => ObjReq.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                }



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

                ObjResp = crmNewService.LoadEDR(ObjReq);

                //FRR--3083
                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LoadEDR_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }

                ObjResp.PromoEDR.ForEach(a =>
                {
                    a.Transactiondate = Utility.GetDateconvertion(a.Transactiondate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                });

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
                strDate  = string.Empty;
                strMonth = string.Empty;
                strYear = string.Empty;
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        [HttpPost]
        public void DownLoadPromotionEDR(string topupData)
        {
            try
            {
                GridView gridView = new GridView();
                List<PromoEDR> Failure = JsonConvert.DeserializeObject<List<PromoEDR>>(topupData);
                gridView.DataSource = Failure;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "PromotionEDR_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
        }


        #endregion

        public class MyCustomDateProvider : IFormatProvider, ICustomFormatter
        {
            public object GetFormat(Type formatType)
            {
                if (formatType == typeof(ICustomFormatter))
                    return this;

                return null;
            }

            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                try
                {
                    if (!(arg is DateTime)) throw new NotSupportedException();

                    var dt = (DateTime)arg;

                    string suffix;

                    if (new[] { 11, 12, 13 }.Contains(dt.Day))
                    {
                        suffix = "th";
                    }
                    else if (dt.Day % 10 == 1)
                    {
                        suffix = "st";
                    }
                    else if (dt.Day % 10 == 2)
                    {
                        suffix = "nd";
                    }
                    else if (dt.Day % 10 == 3)
                    {
                        suffix = "rd";
                    }
                    else
                    {
                        suffix = "th";
                    }

                    return string.Format("{0:MMMM} {1}{2}, {0:yyyy}", arg, dt.Day, suffix);
                }
                catch (Exception ex)
                {
                    return string.Empty;
                }
            }
        }
    }

    enum EnumService
    {
        MOCall = 0,
        MTCall = 1,
        MORoamingCall = 2,
        MTRoamingCall = 3,
        MOSMS = 4,
        MTSMS = 5,
        MORoamingSMS = 6,
        MTRoamingSMS = 7,
        IVR = 8,
        USSD = 9,
        VMS = 10,
        SMSTopup = 11,
        MobileHomeAccount = 12,
        MOVideoCall = 13,   //20
        MTVideoCall = 14, //21
        MORoamingVideoCall = 15,    //22
        MTRoamingVideoCall = 16,    //23  
        MCA = 17,   //18
        CRBT = 18,   //24      
        MOData = 19,
        MORoamData = 20,  //26 
        ussd_call_back = 21,
        voip = 22,
        MO4G = 23,   //29
        MORoam4G = 24,   //30              
        PremiumInfo = 25,    //31   
        PremiumEnt = 25,  //32     
        bundleTopup = 26,
        FGLocalRoam = 27


    }


}

