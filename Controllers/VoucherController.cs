using System;
using System.Linq;
using System.Web.Mvc;
using ServiceCRM;
using System.Collections.Generic;
namespace CRM.Controllers
{
    [ValidateState]
    public class VoucherController : Controller
    {

        ClientSetting clientSetting = new ClientSetting();
        ServiceCRM.ServiceInvokeCRM crmNewService = new ServiceCRM.ServiceInvokeCRM(Convert.ToString(SettingsCRM.crmServiceUrl));

        public ActionResult Status(string Textdata)
        {
            #region FRR 4925
            if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
            {

                Session["RealICCIDForMultiTab"] = Textdata;
            }
            #endregion

            return View();
        }

        [HttpPost]
        public ActionResult VoucherStatus(string voucherCode, string voucherType, string voucherPin , string textdata = "")
        {
            ClientSetting clientSetting = new ClientSetting();
            VoucherStatusResponse voucherStatusResp = new VoucherStatusResponse();
            VoucherStatusRequest voucherStatusRequest = null;
            ServiceInvokeCRM serviceCRM;
            List<ServiceCRM.Menu> menu;
            SearchSimReq objReq = new SearchSimReq();
            SearchSimRes ObjRes = new SearchSimRes();
            string errorMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "VoucherController - VoucherStatus Start");
                voucherStatusRequest = new VoucherStatusRequest();
                voucherStatusRequest.CountryCode = clientSetting.countryCode;
                voucherStatusRequest.BrandCode = clientSetting.brandCode;
                voucherStatusRequest.LanguageCode = clientSetting.langCode;
                voucherStatusRequest.VoucherCode = voucherCode;
                voucherStatusRequest.VoucherType = voucherType;
                voucherStatusRequest.VoucherPin = voucherPin;

                

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                if (voucherType != "FS")
                {
                    menu = new List<ServiceCRM.Menu>();
                    menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "Voucher_Status").ToList();
                    voucherStatusRequest.RetreiveVoucherPinEnabler = Convert.ToString(menu[0].MenuFeatures.Where(a => a.Features == "RetrievePin" && !string.IsNullOrEmpty(a.Enable) && a.Enable.ToUpper() == "TRUE").ToList().Count);
                    voucherStatusResp = serviceCRM.GetCRMVoucherStatus(voucherStatusRequest);
                }
                else
                {
                    voucherStatusResp.VoucherStatus = new ServiceCRM.VoucherStatus();
                    voucherStatusResp.bundleVocuher = new BundleVocuher();
                    voucherStatusResp.VoucherStatus.VoucherType = "FS";
                    objReq = new SearchSimReq();
                    ObjRes = new SearchSimRes();
                    objReq.BrandCode = clientSetting.brandCode;
                    objReq.CountryCode = clientSetting.countryCode;
                    objReq.LanguageCode = clientSetting.langCode;
                    objReq.PinNumber = voucherCode;
                    objReq.Mode = "4";
                    ObjRes = serviceCRM.CRMSearchSim(objReq);
                    voucherStatusResp.ResponseDetails = ObjRes.ResponseDetails;
                    voucherStatusResp.VoucherStatus.AddOnPrice = ObjRes.AddOnPrice;
                    voucherStatusResp.VoucherStatus.NoOfMonth = ObjRes.NoOfMonth;
                    voucherStatusResp.VoucherStatus.TotalPrice = ObjRes.TotalPrice;
                    voucherStatusResp.VoucherStatus.TopupAmount = ObjRes.TopupAmount;
                    voucherStatusResp.VoucherStatus.CardStatus = ObjRes.VoucherStatus;
                    voucherStatusResp.VoucherStatus.BundleName = ObjRes.BundleName;
                    voucherStatusResp.VoucherStatus.BundlePrice = ObjRes.BundlePrice;
                }

                if (voucherType == "FS")
                {
                    if (voucherStatusResp != null && voucherStatusResp.ResponseDetails != null && voucherStatusResp.ResponseDetails.ResponseCode != null)
                    {
                        errorMsg = Resources.ErrorResources.ResourceManager.GetString("FSVoucherStatus_" + voucherStatusResp.ResponseDetails.ResponseCode);
                        voucherStatusResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorMsg) ? voucherStatusResp.ResponseDetails.ResponseDesc : errorMsg;
                    }
                }
                else
                {
                    if (voucherStatusResp != null && voucherStatusResp.ResponseDetails != null && voucherStatusResp.ResponseDetails.ResponseCode != null)
                    {
                        errorMsg = Resources.ErrorResources.ResourceManager.GetString("VoucherStatus_" + voucherStatusResp.ResponseDetails.ResponseCode);
                        voucherStatusResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorMsg) ? voucherStatusResp.ResponseDetails.ResponseDesc : errorMsg;
                        if (voucherStatusResp.ResponseDetails.ResponseCode == "100" && voucherStatusRequest.VoucherType == "EV")
                            voucherStatusResp.ResponseDetails.ResponseDesc = Resources.ErrorResources.VoucherStatus_EV_100 + voucherStatusRequest.VoucherCode;
                        else if (voucherStatusResp.ResponseDetails.ResponseCode == "100" && voucherStatusRequest.VoucherType == "SN")
                            voucherStatusResp.ResponseDetails.ResponseDesc = Resources.ErrorResources.VoucherStatus_SN_100 + voucherStatusRequest.VoucherCode;
                        else if (voucherStatusResp.ResponseDetails.ResponseCode == "100" && voucherStatusRequest.VoucherType == "VP")
                            voucherStatusResp.ResponseDetails.ResponseDesc = Resources.ErrorResources.VoucherStatus_VP_100 + voucherStatusRequest.VoucherCode;
                    }
                    if (voucherStatusResp.VoucherStatus != null)
                    {
                        voucherStatusResp.VoucherStatus.ActivationDate = Utility.GetDateconvertion(voucherStatusResp.VoucherStatus.ActivationDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                        voucherStatusResp.VoucherStatus.BlockedDate = Utility.GetDateconvertion(voucherStatusResp.VoucherStatus.BlockedDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                        voucherStatusResp.VoucherStatus.RechargeDate = Utility.GetDateconvertion(voucherStatusResp.VoucherStatus.RechargeDate, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "VoucherController - VoucherStatus End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "VoucherController - VoucherStatus - " + this.ControllerContext, exp);
            }
            finally
            {
                clientSetting = null;
                voucherStatusRequest = null;
                serviceCRM = null;
                menu = null;
                objReq = null;
                ObjRes = null;
                errorMsg = string.Empty;
            }
            return View("StatusResponse", voucherStatusResp);
        }
    }
}
