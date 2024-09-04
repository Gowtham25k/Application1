using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CRM.Models;
using ServiceCRM;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Text;
using java.lang.reflect;
using Microsoft.Office.Interop.Word;

namespace CRM.Controllers
{
    [ValidateState]
    public class SubscriberController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();
        static string opcode;
        public SubscriberInfo SubScriberBarPrepaidLoad()
        {
            SubscriberInfo subscriberInfo = new SubscriberInfo();
            GetCRMSubscriberPersonalInformationResponse objResponse = new GetCRMSubscriberPersonalInformationResponse();
            GetCRMSubscriberPersonalDetailsRequest objSubscriber = new GetCRMSubscriberPersonalDetailsRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - SubScriberBarPrepaidLoad Start");
                objSubscriber.BrandCode = clientSetting.brandCode;
                objSubscriber.CountryCode = clientSetting.countryCode;
                objSubscriber.LanguageCode = clientSetting.langCode;
                objSubscriber.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objResponse = serviceCRM.CRMPrepaidSubscriberPersonalInformation(objSubscriber);
                if (objResponse.SubscriberDetails != null)
                {
                    subscriberInfo.firstName = objResponse.SubscriberDetails.FirstName;
                    subscriberInfo.lastName = objResponse.SubscriberDetails.LastName;
                    subscriberInfo.titleName = objResponse.SubscriberDetails.Title;
                    subscriberInfo.MSIDDN = objResponse.SubscriberDetails.MSISDN;
                    subscriberInfo.ICCID = objResponse.SubscriberDetails.ICCID;
                    subscriberInfo.PIN = objResponse.SubscriberDetails.Pin;
                    if (objResponse.SubscriberDetails.BirthDD != null && objResponse.SubscriberDetails.BirthMM != null)
                    {
                        subscriberInfo.DOB = Utility.FormatDateTime(objResponse.SubscriberDetails.BirthYY + "/" + objResponse.SubscriberDetails.BirthMM + "/" + objResponse.SubscriberDetails.BirthDD, clientSetting.mvnoSettings.dateTimeFormat);
                    }
                    else
                    {
                        subscriberInfo.DOB = string.Empty;
                    }
                    subscriberInfo.houseNumber = objResponse.SubscriberDetails.HouseNumber;
                    subscriberInfo.streetName = objResponse.SubscriberDetails.Street;
                    subscriberInfo.cityName = objResponse.SubscriberDetails.City;
                    subscriberInfo.countyName = objResponse.SubscriberDetails.County;
                    subscriberInfo.stateName = objResponse.SubscriberDetails.State;
                    subscriberInfo.countryName = objResponse.SubscriberDetails.Country;
                    subscriberInfo.postCode = string.IsNullOrEmpty(objResponse.SubscriberDetails.PostCode) ? " " : objResponse.SubscriberDetails.PostCode;
                    subscriberInfo.mobileNumber = objResponse.SubscriberDetails.MSISDN;
                    subscriberInfo.homeNumber = objResponse.SubscriberDetails.ContactNumber;
                    subscriberInfo.workNumber = objResponse.SubscriberDetails.CompanyNumber;
                    subscriberInfo.idType = objResponse.SubscriberDetails.IdProof;
                    subscriberInfo.idNumber = objResponse.SubscriberDetails.IdValue;
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - SubScriberBarPrepaidLoad End");
                }
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                objResponse = null;
                objSubscriber = null;
                serviceCRM = null;
            }
            return subscriberInfo;
        }

        public SubscriberInfo SubScriberBarPostpaidLoad()
        {
            SubscriberInfo subscriberInfo = new SubscriberInfo();
            GetCRMPostpaidSubscriberDetailsResponse ObjPostPaidresponse = new GetCRMPostpaidSubscriberDetailsResponse();
            GetCRMPostpaidSubscriberDetailsRequest ObjPostPaid = new GetCRMPostpaidSubscriberDetailsRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjPostPaid.BrandCode = clientSetting.brandCode;
                ObjPostPaid.CountryCode = clientSetting.countryCode;
                ObjPostPaid.LanguageCode = clientSetting.langCode;
                ObjPostPaid.MSISDN = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    ObjPostPaidresponse = serviceCRM.CRMPostpaidSubscriberPersonalInformation(ObjPostPaid);

                if (ObjPostPaidresponse.SubscriberDetailsPostpaid != null)
                {
                    subscriberInfo.firstName = ObjPostPaidresponse.SubscriberDetailsPostpaid.FirstName;
                    subscriberInfo.lastName = ObjPostPaidresponse.SubscriberDetailsPostpaid.LastName;
                    subscriberInfo.titleName = ObjPostPaidresponse.SubscriberDetailsPostpaid.Title;

                    subscriberInfo.MSIDDN = ObjPostPaidresponse.SubscriberDetailsPostpaid.MSISDN;
                    subscriberInfo.ICCID = ObjPostPaidresponse.SubscriberDetailsPostpaid.ICCID;
                    subscriberInfo.PIN = ObjPostPaidresponse.SubscriberDetailsPostpaid.Pin;
                    if (ObjPostPaidresponse.SubscriberDetailsPostpaid.BirthDD != null && ObjPostPaidresponse.SubscriberDetailsPostpaid.BirthMM != null)
                    {
                        subscriberInfo.DOB = ObjPostPaidresponse.SubscriberDetailsPostpaid.BirthDD + "/" + ObjPostPaidresponse.SubscriberDetailsPostpaid.BirthMM + "/" + ObjPostPaidresponse.SubscriberDetailsPostpaid.BirthYY;
                    }
                    else
                    {
                        subscriberInfo.DOB = string.Empty;
                    }
                    subscriberInfo.houseNumber = ObjPostPaidresponse.SubscriberDetailsPostpaid.HouseNumber;
                    subscriberInfo.streetName = ObjPostPaidresponse.SubscriberDetailsPostpaid.Street;
                    subscriberInfo.cityName = ObjPostPaidresponse.SubscriberDetailsPostpaid.City;
                    subscriberInfo.countyName = ObjPostPaidresponse.SubscriberDetailsPostpaid.County;
                    subscriberInfo.stateName = ObjPostPaidresponse.SubscriberDetailsPostpaid.State;
                    subscriberInfo.countryName = ObjPostPaidresponse.SubscriberDetailsPostpaid.Country;
                    subscriberInfo.postCode = string.IsNullOrEmpty(ObjPostPaidresponse.SubscriberDetailsPostpaid.PostCode) ? " " : ObjPostPaidresponse.SubscriberDetailsPostpaid.PostCode;

                    subscriberInfo.mobileNumber = ObjPostPaidresponse.SubscriberDetailsPostpaid.MSISDN;
                    subscriberInfo.homeNumber = ObjPostPaidresponse.SubscriberDetailsPostpaid.ContactNumber;
                    subscriberInfo.workNumber = ObjPostPaidresponse.SubscriberDetailsPostpaid.CompanyNumber;
                    subscriberInfo.idType = ObjPostPaidresponse.SubscriberDetailsPostpaid.IDOption;
                    subscriberInfo.idNumber = ObjPostPaidresponse.SubscriberDetailsPostpaid.IDNumber;
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubScriberBar Postpaid");
                }
                return subscriberInfo;
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return subscriberInfo;
            }
            finally
            {
                //subscriberInfo = null;
                ObjPostPaidresponse = null;
                ObjPostPaid = null;
                serviceCRM = null;
            }

        }

        public SubscriberInfo SubscribarMinimalBarPrepaidLoad()
        {
            SubscriberInfo subscriberInfo = new SubscriberInfo();
            GetBundleBalanceRequest ObjBundleBalanceRequest = new GetBundleBalanceRequest();
            GetBundleBalanceResponse dataCheck = new GetBundleBalanceResponse();
            ServiceInvokeCRM serviceCRM;
            List<BUNDLE_THRES> EnumData;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - SubscribarMinimalBarPrepaidLoad Start");
                subscriberInfo = SubScriberBarPrepaidLoad();
                ObjBundleBalanceRequest.BrandCode = clientSetting.brandCode;
                ObjBundleBalanceRequest.CountryCode = clientSetting.countryCode;
                ObjBundleBalanceRequest.LanguageCode = clientSetting.langCode;
                ObjBundleBalanceRequest.Msisdn = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                dataCheck = serviceCRM.GetBundleBalance(ObjBundleBalanceRequest);
                if (dataCheck.BundleInfo != null)
                {
                    EnumData = dataCheck.BundleInfo.SelectMany(m => m.BUNDLEUSAGELIMITRES.Where(b => b != null)).ToList();
                    decimal SMS = EnumData.Where(a => a.UNIT_TYPE == "SMS").Sum(b => Convert.ToDecimal(b.CONFIGURED_LIMIT)) - EnumData.Where(a => a.UNIT_TYPE == "SMS").Sum(b => Convert.ToDecimal(b.USAGE_LIMIT));
                    decimal Data = EnumData.Where(a => a.BUCK_ID == "Data").Sum(b => Convert.ToDecimal(b.CONFIGURED_LIMIT)) - EnumData.Where(a => a.BUCK_ID == "Data").Sum(b => Convert.ToDecimal(b.USAGE_LIMIT));
                    decimal Minute = EnumData.Where(a => a.UNIT_TYPE == "Minute(s)").Sum(b => Convert.ToDecimal(b.CONFIGURED_LIMIT)) - EnumData.Where(a => a.UNIT_TYPE == "Minute(s)").Sum(b => Convert.ToDecimal(b.USAGE_LIMIT));
                    subscriberInfo.SMS = SMS;
                    subscriberInfo.minutes = Minute;
                    subscriberInfo.data = Data;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - SubscribarMinimalBarPrepaidLoad End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                ObjBundleBalanceRequest = null;
                dataCheck = null;
                serviceCRM = null;
                EnumData = null;
            }
            return subscriberInfo;
        }

        public SubscriberInfo SubscribarMinimalBarPostpaidLoad()
        {
            SubscriberInfo subscriberInfo = new SubscriberInfo();
            PostpaidBillinginfo ObjPostpaidBillinginfo = new PostpaidBillinginfo();
            Postpaidsubscriber ObjPostpaidsubscriber = new Postpaidsubscriber();
            ServiceInvokeCRM serviceCRM;
            try
            {
                subscriberInfo = SubScriberBarPostpaidLoad();
                ObjPostpaidsubscriber.BrandCode = clientSetting.brandCode;
                ObjPostpaidsubscriber.CountryCode = clientSetting.countryCode;
                ObjPostpaidsubscriber.LanguageCode = clientSetting.langCode;
                ObjPostpaidsubscriber.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    ObjPostpaidBillinginfo = serviceCRM.PostpaidBillingDetails(ObjPostpaidsubscriber);

                if (ObjPostpaidBillinginfo != null)
                {
                    subscriberInfo.dueAmount = ObjPostpaidBillinginfo.DueAmount;
                    subscriberInfo.dueDate = ObjPostpaidBillinginfo.DueDate;
                    subscriberInfo.overDueAmount = ObjPostpaidBillinginfo.OverDueAmount;
                    subscriberInfo.lastPaidAmount = ObjPostpaidBillinginfo.LastPaidAmount;
                    subscriberInfo.lastPaidAmountDate = ObjPostpaidBillinginfo.LastPaidDate;
                    subscriberInfo.lastPaidAmountStatus = ObjPostpaidBillinginfo.LastPaidBy;
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubScriber MinimalBar Postpaid");
                }
                return subscriberInfo;
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return subscriberInfo;
            }
            finally
            {
               // subscriberInfo = null;
                ObjPostpaidBillinginfo = null;
                serviceCRM = null;
                ObjPostpaidsubscriber = null;
            }

        }

        public JsonResult SubscriberMinimalBar()
        {
            SubscriberInfo subscriberInfo = new SubscriberInfo();
            ManagedCardResponse managedCardResp = new ManagedCardResponse();
            ManagedCardRequest managedCardReq = new ManagedCardRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - SubscriberMinimalBar Start");
                if (Session["isPrePaid"].ToString() == CRM.Models.constantvalues.Prepaid)
                {
                    subscriberInfo = SubscribarMinimalBarPrepaidLoad();
                }
                else
                {
                    subscriberInfo = SubscribarMinimalBarPostpaidLoad();
                }
                try
                {
                    managedCardReq.BrandCode = clientSetting.brandCode;
                    managedCardReq.CountryCode = clientSetting.countryCode;
                    managedCardReq.LanguageCode = clientSetting.langCode;
                    managedCardReq.Msisdn = Session["MobileNumber"].ToString();
                    managedCardReq.mode = "Q";
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    managedCardResp = serviceCRM.CRMManageCardDetails(managedCardReq);
                    if (managedCardResp.cardDetails != null && managedCardResp.cardDetails.Count > 0)
                    {
                        subscriberInfo.lstCardDetails.AddRange(managedCardResp.cardDetails.Select(m => m.cardNumber));
                    }
                }
                catch (Exception explstCardDetails)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, explstCardDetails);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - SubscriberMinimalBar End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                managedCardResp = null;
                managedCardReq = null;
                serviceCRM = null;
            }
            return Json(subscriberInfo, JsonRequestBehavior.AllowGet);
        }

        public SubscriberInfo SubscriberPrepaidSummary()
        {
            SubscriberInfo subscriberInfo = new SubscriberInfo();
            GetCRMSubscriberPersonalDetailsRequest subscriberPersonalReq = new GetCRMSubscriberPersonalDetailsRequest();
            GetCRMSubscriberPersonalInformationResponse subscriberPersonalResp = new GetCRMSubscriberPersonalInformationResponse();
            CRM.Models.AnatelIDHistory objanatel = new CRM.Models.AnatelIDHistory();
            ServiceInvokeCRM serviceCRM;
            try
            {

                subscriberPersonalReq.BrandCode = clientSetting.brandCode;
                subscriberPersonalReq.CountryCode = clientSetting.countryCode;
                subscriberPersonalReq.LanguageCode = clientSetting.langCode;
                subscriberPersonalReq.MSISDN = Session["MobileNumber"].ToString();
                subscriberPersonalReq.UserName = Session["UserName"].ToString();
                //6335
                subscriberPersonalReq.ICCID = Session["ICCID"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    subscriberPersonalResp = serviceCRM.CRMPrepaidSubscriberPersonalInformation(subscriberPersonalReq);

                if (subscriberPersonalResp.SubscriberDetails != null)
                {
                    subscriberInfo.houseNumber = subscriberPersonalResp.SubscriberDetails.HouseNumber;

                    subscriberInfo.HouseNoAddOn = subscriberPersonalReq.CountryCode == "GER" ? subscriberPersonalResp.SubscriberDetails.HouseNoAddOn : "";

                    subscriberInfo.Locality = subscriberPersonalReq.CountryCode == "IND" ? subscriberPersonalResp.SubscriberDetails.Locality : "";
                    subscriberInfo.Subdistrict = subscriberPersonalReq.CountryCode == "IND" ? subscriberPersonalResp.SubscriberDetails.Subdistrict : "";
                    subscriberInfo.District = subscriberPersonalReq.CountryCode == "IND" ? subscriberPersonalResp.SubscriberDetails.District : "";
                    subscriberInfo.Postoffice = subscriberPersonalReq.CountryCode == "IND" ? subscriberPersonalResp.SubscriberDetails.Postoffice : "";
                    subscriberInfo.Landmark = subscriberPersonalReq.CountryCode == "IND" ? subscriberPersonalResp.SubscriberDetails.Landmark : "";

                    subscriberInfo.UnitNumber = (subscriberPersonalReq.CountryCode == "AUS" || subscriberPersonalReq.CountryCode == "IRL") ? subscriberPersonalResp.SubscriberDetails.UnitNumber : "";

                    subscriberInfo.streetName = subscriberPersonalResp.SubscriberDetails.Street;
                    subscriberInfo.cityName = subscriberPersonalResp.SubscriberDetails.City;
                    subscriberInfo.stateName = subscriberPersonalResp.SubscriberDetails.State;
                    subscriberInfo.countyName = subscriberPersonalResp.SubscriberDetails.County;
                    subscriberInfo.countryName = subscriberPersonalResp.SubscriberDetails.Country;
                    subscriberInfo.postCode = subscriberPersonalResp.SubscriberDetails.PostCode;
                    subscriberInfo.mobileNumber = string.Empty;
                    subscriberInfo.homeNumber = subscriberPersonalResp.SubscriberDetails.ContactNumber;
                    subscriberInfo.workNumber = subscriberPersonalResp.SubscriberDetails.CompanyNumber;
                    subscriberInfo.idType = subscriberPersonalResp.SubscriberDetails.IdValue;
                    subscriberInfo.idNumber = subscriberPersonalResp.SubscriberDetails.IdProof;
                    subscriberInfo.idValidity = subscriberPersonalResp.SubscriberDetails.IdExpiryDate;
                    subscriberInfo.idIssuedby = subscriberPersonalResp.SubscriberDetails.IssuingAuthority;
                    subscriberInfo.placeofBirth = subscriberPersonalResp.SubscriberDetails.PlaceofBirth;
                    subscriberInfo.genderCode = subscriberPersonalResp.SubscriberDetails.GENDER;
                    subscriberInfo.occupation = subscriberPersonalResp.SubscriberDetails.Occupation;
                    subscriberInfo.profxlOrganxn = subscriberPersonalResp.SubscriberDetails.CompanyName; //(Ask to sakthi)
                    subscriberInfo.education = subscriberPersonalResp.SubscriberDetails.Education;
                    subscriberInfo.academicOrganxn = subscriberPersonalResp.SubscriberDetails.AcademicTitle;

                    if (subscriberPersonalReq.CountryCode.ToUpper() == "TUN")
                    {
                        subscriberInfo.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                        subscriberInfo.strDropdown = Utility.GetDropdownMasterFromDB("4", Convert.ToString(Session["isPrePaid"]), "drop_master");

                        if (subscriberPersonalResp.SubscriberDetails.PrefLanguage != "" && subscriberPersonalResp.SubscriberDetails.PrefLanguage != null && subscriberPersonalResp.SubscriberDetails.PrefLanguage != "0")
                        {
                            DropdownMaster dropdown = subscriberInfo.strDropdown.FirstOrDefault(b => b.ID == subscriberPersonalResp.SubscriberDetails.PrefLanguage && b.Master_id == "4");
                            subscriberInfo.preffLanguage = dropdown.Value;
                        }
                    }
                    else
                    {
                        subscriberInfo.preffLanguage = subscriberPersonalResp.SubscriberDetails.PrefLanguage;
                    }

                    subscriberInfo.promotionalCommxn = subscriberPersonalResp.SubscriberDetails.PromotionalCommunication;
                    subscriberInfo.titleName = subscriberPersonalResp.SubscriberDetails.Title;
                    subscriberInfo.firstName = subscriberPersonalResp.SubscriberDetails.FirstName;
                    subscriberInfo.lastName = subscriberPersonalResp.SubscriberDetails.LastName;

                    if (subscriberPersonalReq.CountryCode.ToUpper() == "BRA")
                    {

                        if (subscriberPersonalResp.SubscriberDetails.DateOfCreation != null)
                        {
                            subscriberInfo.DateOfCreation = subscriberPersonalResp.SubscriberDetails.DateOfCreation;

                        }
                        else
                        {
                            subscriberInfo.DateOfCreation = "NA";
                        }
                    }
                    else
                    {
                    }
                    //   subscriberPlanDetails.prepostAccountDetails.LastTopUpDate = string.IsNullOrEmpty(topppHistoryResp.TopUpHistory[0].Date) ? string.Empty : Utility.FormatDateTime(topppHistoryResp.TopUpHistory[0].Date, clientSetting.mvnoSettings.dateTimeFormat);

                    //FRR4204

                    if (subscriberPersonalResp.SubscriberDetails.BirthDD != null && subscriberPersonalResp.SubscriberDetails.BirthMM != null)
                    {
                        subscriberInfo.DOB = Utility.FormatDateTime(subscriberPersonalResp.SubscriberDetails.BirthYY + "/" + subscriberPersonalResp.SubscriberDetails.BirthMM + "/" + subscriberPersonalResp.SubscriberDetails.BirthDD, clientSetting.mvnoSettings.dateTimeFormat);
                    }
                    else
                    {
                        subscriberInfo.DOB = string.Empty;
                    }
                    /// for brazil

                    if (subscriberPersonalReq.CountryCode.ToUpper() == "BRA")
                    {
                        subscriberInfo.cpfNumber = subscriberPersonalResp.SubscriberDetails.cpfNumber;
                        subscriberInfo.areaCode = subscriberPersonalResp.SubscriberDetails.areaCode;
                        #region  Anatel ID List(3340-FRR)
                        subscriberInfo.showanatelID = subscriberPersonalResp.SubscriberDetails.showanatelID;
                        if (clientSetting.mvnoSettings.ShowAnatelID.ToUpper() == "ON" && subscriberPersonalResp.SubscriberDetails.AnatelIDHistory != null && subscriberPersonalResp.SubscriberDetails.AnatelIDHistory.Count() > 0)
                        {
                            subscriberInfo.AnatelIDHistory = new List<Models.AnatelIDHistory>();
                            for (int i = 0; i < subscriberPersonalResp.SubscriberDetails.AnatelIDHistory.Count(); i++)
                            {
                                CRM.Models.AnatelIDHistory obj = new CRM.Models.AnatelIDHistory();
                                obj.AnatelID = subscriberPersonalResp.SubscriberDetails.AnatelIDHistory[i].AnatelID;
                                obj.AnatelIDTime = Utility.GetDateconvertion(subscriberPersonalResp.SubscriberDetails.AnatelIDHistory[i].AnatelIDTime, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                                subscriberInfo.AnatelIDHistory.Add(obj);
                            }
                        }
                        #endregion
                    }
                    /// for brazil end
                    subscriberInfo.eMailID = subscriberPersonalResp.SubscriberDetails.Email;
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubScriber Prepaid Summary");

                    //6335
                    if (subscriberPersonalResp.freesimsubscriberpersonaldetails != null && subscriberPersonalResp.freesimsubscriberpersonaldetails.Count > 0)
                    {
                        subscriberInfo.fname_Freesim = subscriberPersonalResp.freesimsubscriberpersonaldetails[0].firstname;
                        subscriberInfo.Lname_Freesim = subscriberPersonalResp.freesimsubscriberpersonaldetails[0].LastName;
                        subscriberInfo.Emailid_Freesim = subscriberPersonalResp.freesimsubscriberpersonaldetails[0].Email;
                        subscriberInfo.Mobile_Freesim = subscriberPersonalResp.freesimsubscriberpersonaldetails[0].MobileNo;
                        subscriberInfo.post_Freesim = subscriberPersonalResp.freesimsubscriberpersonaldetails[0].PostCode;
                        subscriberInfo.House_Freesim = subscriberPersonalResp.freesimsubscriberpersonaldetails[0].HouseNo;
                        subscriberInfo.street_Freesim = subscriberPersonalResp.freesimsubscriberpersonaldetails[0].street;
                        subscriberInfo.city_Freesim = subscriberPersonalResp.freesimsubscriberpersonaldetails[0].CityName;
                        subscriberInfo.country_Freesim = subscriberPersonalResp.freesimsubscriberpersonaldetails[0].Country;
                        subscriberInfo.County_Freesim = subscriberPersonalResp.freesimsubscriberpersonaldetails[0].County;
                        subscriberInfo.Apartment_Freesim = subscriberPersonalResp.freesimsubscriberpersonaldetails[0].Apartment;

                    }

                    #region subs mini view prepaid
                    GetBundleBalanceRequest ObjBundleBalanceRequest = new GetBundleBalanceRequest();
                    ObjBundleBalanceRequest.BrandCode = clientSetting.brandCode;
                    ObjBundleBalanceRequest.CountryCode = clientSetting.countryCode;
                    ObjBundleBalanceRequest.LanguageCode = clientSetting.langCode;
                    ObjBundleBalanceRequest.Msisdn = Session["MobileNumber"].ToString();
                    GetBundleBalanceResponse dataCheck = new GetBundleBalanceResponse();
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                        // FRR 4772 
                        if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
                        {


                        dataCheck = serviceCRM.GetBundleBalance(ObjBundleBalanceRequest);

                    }
                    if (dataCheck.BundleInfo != null)
                    {
                        List<BUNDLE_THRES> EnumData = dataCheck.BundleInfo.SelectMany(m => m.BUNDLEUSAGELIMITRES.Where(b => b != null)).ToList();

                        decimal SMS = EnumData.Where(a => a.UNIT_TYPE == "SMS").Sum(b => Convert.ToDecimal(b.CONFIGURED_LIMIT)) - EnumData.Where(a => a.UNIT_TYPE == "SMS").Sum(b => Convert.ToDecimal(b.USAGE_LIMIT));
                        decimal Data = EnumData.Where(a => a.BUCK_ID == "Data").Sum(b => Convert.ToDecimal(b.CONFIGURED_LIMIT)) - EnumData.Where(a => a.BUCK_ID == "Data").Sum(b => Convert.ToDecimal(b.USAGE_LIMIT));
                        decimal Minute = EnumData.Where(a => a.UNIT_TYPE == "Minute(s)").Sum(b => Convert.ToDecimal(b.CONFIGURED_LIMIT)) - EnumData.Where(a => a.UNIT_TYPE == "Minute(s)").Sum(b => Convert.ToDecimal(b.USAGE_LIMIT));
                        subscriberInfo.SMS = SMS;
                        subscriberInfo.minutes = Minute;
                        subscriberInfo.data = Data;
                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubScriber MinimalBar Prepaid");
                    }
                    #endregion
                }
                return subscriberInfo;
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return subscriberInfo;
            }
            finally
            {
               // subscriberInfo = null;
                subscriberPersonalReq = null;
                subscriberPersonalResp = null;
                serviceCRM = null;
            }

        }

        public SubscriberInfo SubscriberPostpaidSummary()
        {
            SubscriberInfo subscriberInfo = new SubscriberInfo();
            GetCRMPostpaidSubscriberDetailsRequest subscriberPersonalReq = new GetCRMPostpaidSubscriberDetailsRequest();
            GetCRMPostpaidSubscriberDetailsResponse subscriberPersonalResp = new GetCRMPostpaidSubscriberDetailsResponse();
            ServiceInvokeCRM serviceCRM;

            try
            {

                subscriberPersonalReq.BrandCode = clientSetting.brandCode;
                subscriberPersonalReq.CountryCode = clientSetting.countryCode;
                subscriberPersonalReq.LanguageCode = clientSetting.langCode;
                subscriberPersonalReq.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    subscriberPersonalResp = serviceCRM.CRMPostpaidSubscriberPersonalInformation(subscriberPersonalReq);

                if (subscriberPersonalResp.SubscriberDetailsPostpaid != null)
                {
                    subscriberInfo.houseNumber = subscriberPersonalResp.SubscriberDetailsPostpaid.HouseNumber;
                    subscriberInfo.streetName = subscriberPersonalResp.SubscriberDetailsPostpaid.Street;
                    subscriberInfo.cityName = subscriberPersonalResp.SubscriberDetailsPostpaid.City;
                    subscriberInfo.stateName = subscriberPersonalResp.SubscriberDetailsPostpaid.State;
                    subscriberInfo.countyName = subscriberPersonalResp.SubscriberDetailsPostpaid.County;
                    subscriberInfo.countryName = subscriberPersonalResp.SubscriberDetailsPostpaid.Country;
                    subscriberInfo.postCode = subscriberPersonalResp.SubscriberDetailsPostpaid.PostCode;
                    subscriberInfo.mobileNumber = subscriberPersonalResp.SubscriberDetailsPostpaid.MSISDN;
                    subscriberInfo.homeNumber = subscriberPersonalResp.SubscriberDetailsPostpaid.ContactNumber;
                    subscriberInfo.workNumber = subscriberPersonalResp.SubscriberDetailsPostpaid.CompanyNumber;
                    subscriberInfo.idType = subscriberPersonalResp.SubscriberDetailsPostpaid.IDNumber;
                    subscriberInfo.idNumber = subscriberPersonalResp.SubscriberDetailsPostpaid.IDOption;
                    subscriberInfo.idValidity = subscriberPersonalResp.SubscriberDetailsPostpaid.IdExpiryDate;
                    subscriberInfo.idIssuedby = subscriberPersonalResp.SubscriberDetailsPostpaid.IssuingAuthority;
                    subscriberInfo.placeofBirth = subscriberPersonalResp.SubscriberDetailsPostpaid.PlaceofBirth;
                    subscriberInfo.genderCode = subscriberPersonalResp.SubscriberDetailsPostpaid.GENDER;
                    subscriberInfo.occupation = subscriberPersonalResp.SubscriberDetailsPostpaid.Occupation;
                    subscriberInfo.profxlOrganxn = subscriberPersonalResp.SubscriberDetailsPostpaid.CompanyName;
                    subscriberInfo.education = subscriberPersonalResp.SubscriberDetailsPostpaid.Education;
                    subscriberInfo.academicOrganxn = subscriberPersonalResp.SubscriberDetailsPostpaid.AcademicTitle;
                    subscriberInfo.preffLanguage = subscriberPersonalResp.SubscriberDetailsPostpaid.PrefLanguage;
                    subscriberInfo.promotionalCommxn = subscriberPersonalResp.SubscriberDetailsPostpaid.PromotionalCommunication;
                    subscriberInfo.titleName = subscriberPersonalResp.SubscriberDetailsPostpaid.Title;
                    subscriberInfo.firstName = subscriberPersonalResp.SubscriberDetailsPostpaid.FirstName;
                    subscriberInfo.lastName = subscriberPersonalResp.SubscriberDetailsPostpaid.LastName;
                    if (subscriberPersonalResp.SubscriberDetailsPostpaid.BirthDD != null && subscriberPersonalResp.SubscriberDetailsPostpaid.BirthMM != null)
                    {
                        subscriberInfo.DOB = Utility.FormatDateTime(subscriberPersonalResp.SubscriberDetailsPostpaid.BirthYY + "/" + subscriberPersonalResp.SubscriberDetailsPostpaid.BirthMM + "/" + subscriberPersonalResp.SubscriberDetailsPostpaid.BirthDD, clientSetting.mvnoSettings.dateTimeFormat);
                    }
                    else
                    {
                        subscriberInfo.DOB = string.Empty;
                    }
                    subscriberInfo.eMailID = subscriberPersonalResp.SubscriberDetailsPostpaid.Email;
                    subscriberInfo.countryID = string.Empty;
                    subscriberInfo.taxCode = string.Empty;
                    subscriberInfo.xmlDocDet = string.Empty;
                    subscriberInfo.pukCode = subscriberPersonalResp.SubscriberDetailsPostpaid.PukCode;
                    subscriberInfo.ICCID = subscriberPersonalResp.SubscriberDetailsPostpaid.ICCID;
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Subscriber Postpaid Summary");
                }
                return subscriberInfo;
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return subscriberInfo;
            }
            finally
            {
                //subscriberInfo = null;
                subscriberPersonalReq = null;
                subscriberPersonalResp = null;
                serviceCRM = null;
            }

        }

        [HttpPost]
        public ActionResult SubscriberCreditCard(CreditCardListRequest objSubscriber)
        {
            CreditCardListResponce creditCardListResp = new CreditCardListResponce();
            ServiceInvokeCRM serviceCRM;

            try
            {
                objSubscriber.BrandCode = clientSetting.brandCode;
                objSubscriber.CountryCode = clientSetting.countryCode;
                objSubscriber.LanguageCode = clientSetting.langCode;
                objSubscriber.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    creditCardListResp = serviceCRM.CRMGetCreditCardList(objSubscriber);

                return Json(creditCardListResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(creditCardListResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                serviceCRM = null;
               // creditCardListResp = null;
            }

        }

        public JsonResult SubscriberPlan(string isParentLogin, string ChildMsisdn)
        {
            SubscriberPlanDetails subscriberPlanDetails = new SubscriberPlanDetails();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - SubscriberPlan Start");
                if (Convert.ToString(Session["isPrePaid"]) == CRM.Models.constantvalues.Prepaid)
                {
                    subscriberPlanDetails = PrePaidSubscriberPlan(isParentLogin, ChildMsisdn);
                }
                else
                {
                    subscriberPlanDetails = PostPaidSubscriberPlan();
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - SubscriberPlan End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally { }
            return Json(subscriberPlanDetails, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreditCardDetails()
        {
            ManagedCardResponse managedCardResp = new ManagedCardResponse();
            ManagedCardRequest managedCardRequest = new ManagedCardRequest();
            ServiceInvokeCRM serviceCRM;

            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    managedCardRequest.BrandCode = clientSetting.brandCode;
                    managedCardRequest.CountryCode = clientSetting.countryCode;
                    managedCardRequest.LanguageCode = clientSetting.langCode;
                    managedCardRequest.Msisdn = Session["MobileNumber"].ToString();
                    managedCardRequest.mode = "Q";

                    managedCardResp = serviceCRM.CRMManageCardDetails(managedCardRequest);

                // subscriberPlanDetails.cardDetails = managedCardResp.cardDetails;
                return Json(managedCardResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(managedCardResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                managedCardRequest = null;
                serviceCRM = null;
               // managedCardResp = null;
            }
        }

        public JsonResult CreditCardDetailsAPM()
        {
            ManagedCardResponse managedCardResp = new ManagedCardResponse();
            ManagedCardRequest managedCardRequest = new ManagedCardRequest();
            ServiceInvokeCRM serviceCRM;

            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    managedCardRequest.BrandCode = clientSetting.brandCode;
                    managedCardRequest.CountryCode = clientSetting.countryCode;
                    managedCardRequest.LanguageCode = clientSetting.langCode;
                    managedCardRequest.Msisdn = Session["MobileNumber"].ToString();
                    managedCardRequest.mode = "Q";

                    managedCardResp = serviceCRM.CRMManageCardDetails(managedCardRequest);
                    if (clientSetting != null && !string.IsNullOrEmpty(clientSetting.preSettings.APMCardNumberDetails))
                    {
                        try
                        {
                            managedCardResp.cardDetailsAPM = new List<APMCardDetails>();
                            APMCardDetails objCardAPM = null;
                            List<string> lstAPMCard = new List<string>(clientSetting.preSettings.APMCardNumberDetails.Split(',')).Select(p => p.ToUpper().Trim()).ToList();
                            var queryAPM = managedCardResp.cardDetails.Where(t => lstAPMCard.Contains(t.cardNumber.ToUpper())).ToList();
                            managedCardResp.cardDetails.RemoveAll(t => lstAPMCard.Contains(t.cardNumber.ToUpper()));

                            foreach (ManageCardDetails obj in queryAPM)
                            {
                                objCardAPM = new APMCardDetails();
                                objCardAPM.cardId = obj.cardId;
                                objCardAPM.cardNumber = obj.cardNumber;
                                managedCardResp.cardDetailsAPM.Add(objCardAPM);
                            }
                        }
                        catch
                        {
                        }
                    }


                return Json(managedCardResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(managedCardResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                managedCardRequest = null;
               // managedCardResp = null;
                serviceCRM = null;
            }
        }

        public SubscriberPlanDetails PrePaidSubscriberPlan(string isParentLogin, string ChildMsisdn)
        {
            SubscriberPlanDetails subscriberPlanDetails = new SubscriberPlanDetails();

            subscriberPlanDetails.prepostAccountDetails = new PrePostAccountDetails();
            subscriberPlanDetails.preBundleList = new List<BundlesList>();
            subscriberPlanDetails.preBundleList1 = new List<BundlesList>();
            subscriberPlanDetails.FamilyMembersList = new List<FamilyMembers>();
            AccountDetailsResponse accountDetailsResp = new AccountDetailsResponse();
            AccountDetailsRequest accountDetailsReq = new AccountDetailsRequest();
            TopUpHistoryResponse topppHistoryResp = new TopUpHistoryResponse();
            TopUpHistoryRequest topppHistoryReq = new TopUpHistoryRequest();
            BundleDetailResponse bundleDetailResp = new BundleDetailResponse();
            BundleDetailRequest bundleDetailReq = new BundleDetailRequest();
            ServiceInvokeCRM serviceCRM;
            //6439
            SubscriberDataUsageRes objSubscriberDataUsageRes = new SubscriberDataUsageRes();
            SubscriberDataUsageReq objSubscriberDataUsageReq = new SubscriberDataUsageReq();

            try
            {

                #region AccountDetails


                try
                {
                    accountDetailsReq.BrandCode = clientSetting.brandCode;
                    accountDetailsReq.CountryCode = clientSetting.countryCode;
                    accountDetailsReq.LanguageCode = clientSetting.langCode;
                    accountDetailsReq.MSISDN = Session["MobileNumber"].ToString();

                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                        accountDetailsResp = serviceCRM.GetAccountDetails(accountDetailsReq);


                    subscriberPlanDetails.prepostAccountDetails.MainBalanceAmount = !string.IsNullOrEmpty(accountDetailsResp.MAINBALANCE) ? accountDetailsResp.MAINBALANCE : "0";
                    subscriberPlanDetails.prepostAccountDetails.CurrentBalanceAmount = !string.IsNullOrEmpty(accountDetailsResp.CURRENTBALANCE) ? accountDetailsResp.CURRENTBALANCE : "0";

                    if (clientSetting.mvnoSettings.enableCurrencyConversion.ToUpper() == "TRUE")
                    {
                        if (subscriberPlanDetails.prepostAccountDetails.MainBalanceAmount.Contains("."))
                        {
                            string manibal = subscriberPlanDetails.prepostAccountDetails.MainBalanceAmount;
                            subscriberPlanDetails.prepostAccountDetails.MainBalanceAmount = Convert.ToString(Math.Round(Convert.ToDecimal(manibal), Convert.ToInt32(clientSetting.mvnoSettings.decimalLimit))).Replace(".", clientSetting.mvnoSettings.currencyConversionValue);
                        }
                        if (subscriberPlanDetails.prepostAccountDetails.CurrentBalanceAmount.Contains("."))
                        {
                            string Currbal = subscriberPlanDetails.prepostAccountDetails.CurrentBalanceAmount;
                            subscriberPlanDetails.prepostAccountDetails.CurrentBalanceAmount = Convert.ToString(Math.Round(Convert.ToDecimal(Currbal), Convert.ToInt32(clientSetting.mvnoSettings.decimalLimit))).Replace(".", clientSetting.mvnoSettings.currencyConversionValue);
                        }
                    }

                    subscriberPlanDetails.prepostAccountDetails.MainBalanceDate = Utility.GetDateconvertion(accountDetailsResp.VALIDITYDATE, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                    subscriberPlanDetails.prepostAccountDetails.MainBalanceDate = subscriberPlanDetails.prepostAccountDetails.MainBalanceDate != "" ? subscriberPlanDetails.prepostAccountDetails.MainBalanceDate : "-";

                    subscriberPlanDetails.prepostAccountDetails.PromoBalanceAmount = !string.IsNullOrEmpty(accountDetailsResp.PROMOBALANCE) ? accountDetailsResp.PROMOBALANCE : "0";
                    if (clientSetting.mvnoSettings.enableCurrencyConversion.ToUpper() == "TRUE")
                    {
                        if (subscriberPlanDetails.prepostAccountDetails.PromoBalanceAmount.Contains("."))
                        {
                            string promobal = subscriberPlanDetails.prepostAccountDetails.PromoBalanceAmount;
                            subscriberPlanDetails.prepostAccountDetails.PromoBalanceAmount = Convert.ToString(Math.Round(Convert.ToDecimal(promobal), Convert.ToInt32(clientSetting.mvnoSettings.decimalLimit))).Replace(".", clientSetting.mvnoSettings.currencyConversionValue);
                        }
                    }
                    subscriberPlanDetails.prepostAccountDetails.PromoBalanceDate = Utility.GetDateconvertion(accountDetailsResp.PROMOVALIDITYDATE, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                    subscriberPlanDetails.prepostAccountDetails.PromoBalanceDate = subscriberPlanDetails.prepostAccountDetails.PromoBalanceDate != "" ? subscriberPlanDetails.prepostAccountDetails.PromoBalanceDate : "-";

                    //FRR 3413
                    subscriberPlanDetails.prepostAccountDetails.LoanBalanceAmount = !string.IsNullOrEmpty(accountDetailsResp.LOAN_BALANCE) ? accountDetailsResp.LOAN_BALANCE : "0";
                    if (clientSetting.mvnoSettings.enableCurrencyConversion.ToUpper() == "TRUE")
                    {
                        if (subscriberPlanDetails.prepostAccountDetails.LoanBalanceAmount.Contains("."))
                        {
                            string loanbal = subscriberPlanDetails.prepostAccountDetails.LoanBalanceAmount;
                            subscriberPlanDetails.prepostAccountDetails.LoanBalanceAmount = Convert.ToString(Math.Round(Convert.ToDecimal(loanbal), Convert.ToInt32(clientSetting.mvnoSettings.decimalLimit))).Replace(".", clientSetting.mvnoSettings.currencyConversionValue);
                        }
                    }
                    subscriberPlanDetails.prepostAccountDetails.LoanExpiryDate = Utility.GetDateconvertion(accountDetailsResp.LOAN_EXPIRY_DATE, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                    subscriberPlanDetails.prepostAccountDetails.LoanExpiryDate = subscriberPlanDetails.prepostAccountDetails.LoanExpiryDate != "" ? subscriberPlanDetails.prepostAccountDetails.LoanExpiryDate : "-";

                    //FRR 3413



                    //FRR 2820 phase2 - CRM FRR 3218
                    subscriberPlanDetails.prepostAccountDetails.LeastPromoBalanceAmount = !string.IsNullOrEmpty(accountDetailsResp.IMMEDIATE_EXPIRY_PROMO_BALANCE) ? accountDetailsResp.IMMEDIATE_EXPIRY_PROMO_BALANCE : "0";

                    if (clientSetting.mvnoSettings.enableCurrencyConversion.ToUpper() == "TRUE")
                    {
                        if (subscriberPlanDetails.prepostAccountDetails.LeastPromoBalanceAmount.Contains("."))
                        {
                            string leastpromo = subscriberPlanDetails.prepostAccountDetails.LeastPromoBalanceAmount;
                            subscriberPlanDetails.prepostAccountDetails.LeastPromoBalanceAmount = Convert.ToString(Math.Round(Convert.ToDecimal(leastpromo), Convert.ToInt32(clientSetting.mvnoSettings.decimalLimit))).Replace(".", clientSetting.mvnoSettings.currencyConversionValue);
                        }
                    }
                    subscriberPlanDetails.prepostAccountDetails.LeastPromoExpDate = Utility.GetDateconvertion(accountDetailsResp.IMMEDIATE_PROMO_EXPIRY_DATE, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                    subscriberPlanDetails.prepostAccountDetails.LeastPromoExpDate = subscriberPlanDetails.prepostAccountDetails.LeastPromoExpDate != "" ? subscriberPlanDetails.prepostAccountDetails.LeastPromoExpDate : "-";

                    //6439
                    if (clientSetting.preSettings.DisplayIRWalletDetails.Split('|')[0].ToUpper() == "TRUE")
                    {
                        objSubscriberDataUsageReq.BrandCode = clientSetting.brandCode;
                        objSubscriberDataUsageReq.CountryCode = clientSetting.countryCode;
                        objSubscriberDataUsageReq.LanguageCode = clientSetting.langCode;
                        objSubscriberDataUsageReq.MSISDN = Session["MobileNumber"].ToString();
                        objSubscriberDataUsageReq.ICCID = Session["ICCID"].ToString();
                        objSubscriberDataUsageReq.IMSI = Session["IMSI"].ToString();
                        objSubscriberDataUsageReq.Is_IRData = "1";
                        objSubscriberDataUsageRes = serviceCRM.CRMGetIMGSubscriberDataUsage(objSubscriberDataUsageReq);
                        double IRbal = 0;
                        if (objSubscriberDataUsageRes.IR_Data_Details.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(objSubscriberDataUsageRes.IR_Data_Details[0].Balance_IR))
                            {
                                IRbal = Convert.ToDouble(objSubscriberDataUsageRes.IR_Data_Details[0].Balance_IR) * 0.01;
                                subscriberPlanDetails.prepostAccountDetails.IR_Balance = IRbal.ToString();
                            }

                            if (!string.IsNullOrEmpty(objSubscriberDataUsageRes.IR_Data_Details[0].ExpirtyDateTime_IR))
                            {
                                subscriberPlanDetails.prepostAccountDetails.IR_ExpiryDate = objSubscriberDataUsageRes.IR_Data_Details[0].ExpirtyDateTime_IR;

                            }
                            if (!string.IsNullOrEmpty(objSubscriberDataUsageRes.IR_Data_Details[0].Status_IR))
                            {
                                subscriberPlanDetails.prepostAccountDetails.IR_Status = objSubscriberDataUsageRes.IR_Data_Details[0].Status_IR;

                            }
                        }
                    }
                
                
                }
                catch (Exception exp)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                }

                finally
                {
                    accountDetailsReq = null;
                    accountDetailsResp = null;

                }
                #endregion

                // 4772 subscriber
                if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
                {

                #region TopUpHistory



                try
                {
                    topppHistoryReq.BrandCode = clientSetting.brandCode;
                    topppHistoryReq.CountryCode = clientSetting.countryCode;
                    topppHistoryReq.LanguageCode = clientSetting.langCode;
                    topppHistoryReq.MSISDN = Session["MobileNumber"].ToString();
                    topppHistoryReq.Count = "1";
                    topppHistoryReq.oldMSISDN = Convert.ToString(Session["SwapMSISDN"]);
                        serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
    

                        topppHistoryResp = serviceCRM.GetLastTopUpHistory(topppHistoryReq);

                    if (topppHistoryResp.TopUpHistory != null)
                    {
                        if (topppHistoryResp.TopUpHistory.Count() > 0)
                        {
                            subscriberPlanDetails.prepostAccountDetails.LastTopUpStatus = topppHistoryResp.TopUpHistory[0].Mode;
                            subscriberPlanDetails.prepostAccountDetails.LastTopUpDate = string.IsNullOrEmpty(topppHistoryResp.TopUpHistory[0].Date) ? string.Empty : Utility.FormatDateTime(topppHistoryResp.TopUpHistory[0].Date, clientSetting.mvnoSettings.dateTimeFormat);
                            //subscriberPlanDetails.prepostAccountDetails.LastTopUpDate = Utility.GetDateconvertion(topppHistoryResp.TopUpHistory[0].Date, "DD-MM-YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);
                            //subscriberPlanDetails.prepostAccountDetails.LastTopUpDate = ReserveDateconvertion(topppHistoryResp.TopUpHistory[0].Date,clientSetting.mvnoSettings.dateTimeFormat);
                        }
                        else
                        {

                        }
                    }
                }
                catch (Exception exp)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                }

                finally
                {
                    topppHistoryResp = null;
                    topppHistoryReq = null;

                }
                #endregion
                }

                #region BundleDetail

                try
                {
                    bundleDetailReq.BrandCode = clientSetting.brandCode;
                    bundleDetailReq.CountryCode = clientSetting.countryCode;
                    bundleDetailReq.LanguageCode = clientSetting.langCode;
                    bundleDetailReq.MSISDN = Session["MobileNumber"].ToString();
                    bundleDetailReq.IsParentLogin = isParentLogin;

                    if (isParentLogin != null && isParentLogin == "1")
                    {
                        bundleDetailReq.MSISDN = ChildMsisdn;
                    }
                    else if (isParentLogin != null && isParentLogin == "0")
                    {
                        bundleDetailReq.MSISDN = ChildMsisdn;
                        bundleDetailReq.ChildMsisdn = Session["MobileNumber"].ToString();
                    }
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                        bundleDetailResp = serviceCRM.CRMBundleDetails(bundleDetailReq);
                        //4888
                       Session["ALLINONEBUNDLECODE"] = bundleDetailResp.lastinfos.bundlecode;

                        bundleDetailResp.BundleDetails.BundleList.ForEach(b => b.ExhaustedBundleDate = Utility.GetDateconvertion(b.ExhaustedBundleDate, "dd-mm-yyyy HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                        bundleDetailResp.BundleDetails.BundleList.ForEach(b => b.SubresponseDate = Utility.GetDateconvertion(b.SubresponseDate, "dd-mm-yyyy HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                        //bundleDetailResp.BundleDetails.BundleList.ForEach(b => b.ReserveBundles.ForEach(r => r.Activationdate = Utility.GetDateconvertion(r.Activationdate, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat)));
                        //bundleDetailResp.BundleDetails.BundleList.ForEach(b => b.ReserveBundles.ForEach(r => r.Purchasedate = Utility.GetDateconvertion(r.Purchasedate, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat)));
                        try
                        {
                            bundleDetailResp.BundleDetails.BundleList.ForEach(b => b.ReserveBundles.ForEach(r => r.Activationdate = ReserveDateconvertion(r.Activationdate, clientSetting.mvnoSettings.dateTimeFormat.ToUpper())));
                            bundleDetailResp.BundleDetails.BundleList.ForEach(b => b.ReserveBundles.ForEach(r => r.Purchasedate = ReserveDateconvertion(r.Purchasedate, clientSetting.mvnoSettings.dateTimeFormat.ToUpper())));
                        }
                        catch
                        {

                        }
                        bundleDetailResp.BundleDetails.BundleList.ForEach(b => b.Expirydate = Utility.GetDateconvertion(b.Expirydate, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat));
                        bundleDetailResp.BundleDetails.BundleList.ForEach(b => b.Activationdate = Utility.GetDateconvertion(b.Activationdate, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat));
                    //6581
                    bundleDetailResp.BundleDetails.BundleList.ForEach(b => b.cancelrenewaleffectivedate = Utility.GetDateconvertion(b.cancelrenewaleffectivedate, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat));


                        //4204

                        if (bundleDetailResp.lastinfos.Name != null && bundleDetailResp.lastinfos.EXPDATE != null)
                        {

                            subscriberPlanDetails.prepostAccountDetails.Name = bundleDetailResp.lastinfos.Name;
                            subscriberPlanDetails.prepostAccountDetails.EXPdate = string.IsNullOrEmpty(bundleDetailResp.lastinfos.EXPDATE) ? string.Empty : Utility.FormatDateTime(bundleDetailResp.lastinfos.EXPDATE, clientSetting.mvnoSettings.dateTimeFormat);
                            //4888
                            subscriberPlanDetails.prepostAccountDetails.bundlecode = bundleDetailResp.lastinfos.bundlecode;
                        }
                        else
                        {
                            subscriberPlanDetails.prepostAccountDetails.Name = "NA";
                            subscriberPlanDetails.prepostAccountDetails.EXPdate = "NA";
                        }


                        //4204

                        //4889
                        if (bundleDetailResp.RLHCounterDaysResp != null && !string.IsNullOrEmpty(bundleDetailResp.RLHCounterDaysResp.RLHContinuousCounterUsedDays) && !string.IsNullOrEmpty(bundleDetailResp.RLHCounterDaysResp.RLHAccumulatedCounterUsedDays))
                        {
                            subscriberPlanDetails.prepostAccountDetails.RLHContinuousCounterUsedDays = bundleDetailResp.RLHCounterDaysResp.RLHContinuousCounterUsedDays;
                            subscriberPlanDetails.prepostAccountDetails.RLHAccumulatedCounterUsedDays = bundleDetailResp.RLHCounterDaysResp.RLHAccumulatedCounterUsedDays;
                        }
                        else
                        {
                            subscriberPlanDetails.prepostAccountDetails.RLHContinuousCounterUsedDays = "NA";
                            subscriberPlanDetails.prepostAccountDetails.RLHAccumulatedCounterUsedDays = "NA";

                        }

                        //bundleDetailResp.BundleDetails.BundleList.ForEach(b => b.EXPDATE = Utility.GetDateconvertion(b.EXPDATE, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat));

                        //subscriberPlanDetails.prepostAccountDetails.EXPdate = bundleDetailResp.lastinfos.EXPDATE;

                    if (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode != null && bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.RETURNCODE == "0")
                    {
                        #region subscriberPlanDetails

                        PareseBundleDetails(subscriberPlanDetails, bundleDetailResp);

                        #endregion
                    }
                    else
                    {

                    }
                    if (clientSetting.mvnoSettings.enableCurrencyConversion.ToUpper() == "TRUE")
                    {
                        bundleDetailResp.BundleDetails.BundleList = DoCurrencyUnitConvertion(bundleDetailResp.BundleDetails.BundleList);
                    }
                    subscriberPlanDetails.preBundleList = bundleDetailResp.BundleDetails.BundleList;
                    subscriberPlanDetails.preBundleList1 = bundleDetailResp.BundleDetails.BundleList;
                    // 4772
                    if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() == "TRUE")
                    {
                        subscriberPlanDetails.INQUIRY_OF_UF_PROFILEList = bundleDetailResp.BundleDetails.INQUIRY_OF_UF_PROFILEList;
                        subscriberPlanDetails.Isaltan = true;
                    }

                    // 4772 Susbscriber

                    if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
                    {

                    //FamilyPlan 3218
                    #region Familyplan
                    subscriberPlanDetails.FamilyMembersList = bundleDetailResp.FamilyMembersList;

                    if (bundleDetailResp.FamilyMembersList != null)
                    {

                        subscriberPlanDetails.ChildMemberCount = Convert.ToString(bundleDetailResp.FamilyMembersList.Count - 1);

                        foreach (FamilyMembers fMember in bundleDetailResp.FamilyMembersList)
                        {
                            if (fMember.IsSelected == "1")
                            {

                                subscriberPlanDetails.IsSelectedFamMember = fMember.MSISDN;
                            }
                            if (bundleDetailResp.IsParent == "1")
                            {
                                subscriberPlanDetails.IsParentLogin = "1";
                            }
                            else
                            {
                                subscriberPlanDetails.IsParentLogin = "0";
                            }
                            string[] splitVal = fMember.MSISDN.Split('(');
                            if (splitVal[1].ToUpper() == "PARENT)")
                            {
                                subscriberPlanDetails.ParentMSISDN = splitVal[0];
                            }
                        }
                    }

                    subscriberPlanDetails.IsFamily = bundleDetailResp.IsFamily;
                    subscriberPlanDetails.IsParent = bundleDetailResp.IsParent;
                    subscriberPlanDetails.ShowRenewFamilyBundle = bundleDetailResp.ShowRenewFamilyBundle;
                    subscriberPlanDetails.MaximumChildCount = bundleDetailResp.MaximumChildCount;
                    subscriberPlanDetails.MinimumChildCount = bundleDetailResp.MinimumChildCount;
                    subscriberPlanDetails.CurrentMsisdn = bundleDetailResp.CurrentMsisdn;

                    subscriberPlanDetails.rlhStatus = bundleDetailResp.rlhStatus;

                        #region RLH

                    try
                    {
                        if (!string.IsNullOrEmpty(subscriberPlanDetails.rlhStatus) && subscriberPlanDetails.rlhStatus != "ISR")
                        {
                            int i = 0;
                            foreach (BundlesList str in subscriberPlanDetails.preBundleList)
                            {
                                if (str.FiarUsage.Count > 0)
                                {
                                    subscriberPlanDetails.preBundleList[i].smsMOOnNetCount = str.FiarUsage.Count(x => x.UnitType == "SMS" && x.BucketID == "4" && (x.RateGroup != "" && x.RateGroup != null));
                                    subscriberPlanDetails.preBundleList[i].minMOOnNetCount = str.FiarUsage.Count(x => x.UnitType == "Minute(s)" && x.BucketID == "4" && (x.RateGroup != "" && x.RateGroup != null));
                                    subscriberPlanDetails.preBundleList[i].dataCount = str.FiarUsage.Count(x => x.UnitType == "Data" && x.BucketID == "8" && (x.RateGroup != "" && x.RateGroup != null));
                                    subscriberPlanDetails.preBundleList[i].smsMTOnNetCount = str.FiarUsage.Count(x => x.UnitType == "SMS" && x.BucketID == "5" && (x.RateGroup != "" && x.RateGroup != null));
                                    subscriberPlanDetails.preBundleList[i].minMTOnNetCount = str.FiarUsage.Count(x => x.UnitType == "Minute(s)" && x.BucketID == "5" && (x.RateGroup != "" && x.RateGroup != null));
                                    subscriberPlanDetails.preBundleList[i].smsMTOffNetCount = str.FiarUsage.Count(x => x.UnitType == "SMS" && x.BucketID == "6" && (x.RateGroup != "" && x.RateGroup != null));
                                    subscriberPlanDetails.preBundleList[i].minMTOffNetCount = str.FiarUsage.Count(x => x.UnitType == "Minute(s)" && x.BucketID == "6" && (x.RateGroup != "" && x.RateGroup != null));

                                    subscriberPlanDetails.preBundleList[i].minMOOff1NetCount = str.FiarUsage.Count(x => x.UnitType == "Minute(s)" && x.BucketID == "1" && (x.RateGroup != "" && x.RateGroup != null));
                                    subscriberPlanDetails.preBundleList[i].minMOOff2NetCount = str.FiarUsage.Count(x => x.UnitType == "Minute(s)" && x.BucketID == "2" && (x.RateGroup != "" && x.RateGroup != null));
                                    subscriberPlanDetails.preBundleList[i].minMOOff3NetCount = str.FiarUsage.Count(x => x.UnitType == "Minute(s)" && x.BucketID == "3" && (x.RateGroup != "" && x.RateGroup != null));

                                    subscriberPlanDetails.preBundleList[i].smsMOOff1NetCount = str.FiarUsage.Count(x => x.UnitType == "SMS" && x.BucketID == "1" && (x.RateGroup != "" && x.RateGroup != null));
                                    subscriberPlanDetails.preBundleList[i].smsMOOff2NetCount = str.FiarUsage.Count(x => x.UnitType == "SMS" && x.BucketID == "2" && (x.RateGroup != "" && x.RateGroup != null));
                                    subscriberPlanDetails.preBundleList[i].smsMOOff3NetCount = str.FiarUsage.Count(x => x.UnitType == "SMS" && x.BucketID == "3" && (x.RateGroup != "" && x.RateGroup != null));

                                    subscriberPlanDetails.preBundleList[i].bundleBalanceCount = str.FiarUsage.Count(x => x.BucketID == "7" && (x.RateGroup != "" && x.RateGroup != null));

                                    bool bMins = str.FiarUsage.Count(x => x.UnitType == "Minute(s)") > 0 ? true : false;
                                    bool bSms = str.FiarUsage.Count(x => x.UnitType == "SMS") > 0 ? true : false;
                                    bool bData = str.FiarUsage.Count(x => x.UnitType == "Data") > 0 ? true : false;
                                    bool bbundleBalance = str.FiarUsage.Count(x => x.BucketID == "7") > 0 ? true : false;

                                    bool bMOOFFNETMinutes1 = str.FiarUsage.Count(x => x.BucketID == "1" && (x.UnitType == "Minute(s)")) > 0 ? true : false;
                                    bool bMOOFFNETMinutes2 = str.FiarUsage.Count(x => x.BucketID == "2" && (x.UnitType == "Minute(s)")) > 0 ? true : false;
                                    bool bMOOFFNETMinutes3 = str.FiarUsage.Count(x => x.BucketID == "3" && (x.UnitType == "Minute(s)")) > 0 ? true : false;

                                    bool bMOOFFNETSMS1 = str.FiarUsage.Count(x => x.BucketID == "1" && (x.UnitType == "SMS")) > 0 ? true : false;
                                    bool bMOOFFNETSMS2 = str.FiarUsage.Count(x => x.BucketID == "2" && (x.UnitType == "SMS")) > 0 ? true : false;
                                    bool bMOOFFNETSMS3 = str.FiarUsage.Count(x => x.BucketID == "3" && (x.UnitType == "SMS")) > 0 ? true : false;


                                    bool bfMOONNETSMS = str.FiarUsage.Count(x => x.BucketID == "4" && (x.UnitType == "SMS")) > 0 ? true : false;
                                    bool bfMOONNETMIN = str.FiarUsage.Count(x => x.BucketID == "4" && (x.UnitType == "Minute(s)")) > 0 ? true : false;

                                    bool bfMTONNETSMS = str.FiarUsage.Count(x => x.BucketID == "5" && (x.UnitType == "SMS")) > 0 ? true : false;
                                    bool bfMTONNETMIN = str.FiarUsage.Count(x => x.BucketID == "5" && (x.UnitType == "Minute(s)")) > 0 ? true : false;

                                    bool bfMTOFFNETSMS = str.FiarUsage.Count(x => x.BucketID == "6" && (x.UnitType == "SMS")) > 0 ? true : false;
                                    bool bfMTOFFNETMIN = str.FiarUsage.Count(x => x.BucketID == "6" && (x.UnitType == "Minute(s)")) > 0 ? true : false;

                                    bool bfDATACOUNT = str.FiarUsage.Count(x => x.BucketID == "8" && (x.UnitType == "Data")) > 0 ? true : false;

                                    subscriberPlanDetails.preBundleList[i].fMinutes = bMins ? "Minute(s)" : "";
                                    subscriberPlanDetails.preBundleList[i].fSMS = bSms ? "SMS" : "";
                                    subscriberPlanDetails.preBundleList[i].fData = bData ? "Data" : "";
                                    subscriberPlanDetails.preBundleList[i].fBB = bbundleBalance ? "BundleBalance" : "";

                                    subscriberPlanDetails.preBundleList[i].fMOOFFNETMIN1 = bMOOFFNETMinutes1 ? "1" : "";
                                    subscriberPlanDetails.preBundleList[i].fMOOFFNETMIN2 = bMOOFFNETMinutes2 ? "1" : "";
                                    subscriberPlanDetails.preBundleList[i].fMOOFFNETMIN3 = bMOOFFNETMinutes3 ? "1" : "";

                                    subscriberPlanDetails.preBundleList[i].fMOOFFNETSMS1 = bMOOFFNETSMS1 ? "1" : "";
                                    subscriberPlanDetails.preBundleList[i].fMOOFFNETSMS2 = bMOOFFNETSMS2 ? "1" : "";
                                    subscriberPlanDetails.preBundleList[i].fMOOFFNETSMS3 = bMOOFFNETSMS3 ? "1" : "";

                                    subscriberPlanDetails.preBundleList[i].fMOONNETSMS = bfMOONNETSMS ? "1" : "";
                                    subscriberPlanDetails.preBundleList[i].fMOONNETMIN = bfMOONNETMIN ? "1" : "";

                                    subscriberPlanDetails.preBundleList[i].fMTONNETSMS = bfMTONNETSMS ? "1" : "";
                                    subscriberPlanDetails.preBundleList[i].fMTONNETMIN = bfMTONNETMIN ? "1" : "";

                                    subscriberPlanDetails.preBundleList[i].fMTOFFNETSMS = bfMTOFFNETSMS ? "1" : "";
                                    subscriberPlanDetails.preBundleList[i].fMTOFFNETMIN = bfMTOFFNETMIN ? "1" : "";

                                    subscriberPlanDetails.preBundleList[i].fBBCOUNT = bbundleBalance ? "1" : "";
                                    subscriberPlanDetails.preBundleList[i].fDATACOUNT = bfDATACOUNT ? "1" : "";
                                }
                                i++;
                            }
                        }
                    }
                    catch
                    {

                    }

                        #endregion

                    #endregion
                }
                    

                    // 4772 Susbscriber
                }
                catch (Exception exp)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                }

                finally
                {
                    bundleDetailResp = null;
                    bundleDetailReq = null;
                    serviceCRM = null;
                }
                #endregion
                return subscriberPlanDetails;
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return subscriberPlanDetails;
            }
            finally
            {

                //Dispose(true);
                //GC.SuppressFinalize(subscriberPlanDetails);
               // subscriberPlanDetails = null;
                accountDetailsResp = null;
                accountDetailsReq = null;
                topppHistoryResp = null;
                topppHistoryReq = null;
                bundleDetailResp = null;
                bundleDetailReq = null;
            }


        }

        private SubscriberPlanDetails PareseBundleDetails(SubscriberPlanDetails subscriberPlanDetails, BundleDetailResponse bundleDetailResp)
        {

            subscriberPlanDetails.prepostAccountDetails.PlanName = Session["PlanName"].ToString();
            subscriberPlanDetails.prepostAccountDetails.OnNetRegularMins = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.ONM);
            subscriberPlanDetails.prepostAccountDetails.OnNetRegularSMS = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.ONS);
            subscriberPlanDetails.prepostAccountDetails.OnNetRegularMTMins = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.ONMTM);
            subscriberPlanDetails.prepostAccountDetails.OnNetRegularMTSMS = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.ONMTS);
            if (clientSetting.mvnoSettings.enableCurrencyConversion.ToUpper() == "TRUE" && !string.IsNullOrEmpty(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.FREEDATA))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularData = ConvertMBOrGB(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.FREEDATA).Replace(".", clientSetting.mvnoSettings.currencyConversionValue);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularData = bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.FREEDATA;
            }
            //4432

            // 4772 Susbcriber

            if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
            {

            if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "FREE_DATA").FirstOrDefault() != null)
                &&                 (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "FREE_DATA").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "RLH") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularDataRLH = ConvertMBOrGB(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "FREE_DATA").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "RLH").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularDataRLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "FREE_DATA").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "FREE_DATA").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "HOME") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularDataFLH = ConvertMBOrGB(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "FREE_DATA").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "HOME").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularDataFLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "FREE_DATA").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "FREE_DATA").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "ROAM") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularDataRoaming = ConvertMBOrGB(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "FREE_DATA").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "ROAM").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularDataRoaming = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "RLH") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMinsRLH = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "RLH").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMinsRLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "HOME") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMinsFLH = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "HOME").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMinsFLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "ROAM") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMinsRoaming = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "ROAM").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMinsRoaming = "0";
            }

                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "RLH") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularSMSRLH = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "RLH").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularSMSRLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "HOME") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularSMSFLH = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "HOME").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularSMSFLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "ROAM") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularRoaming = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_ONNET_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "ROAM").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularRoaming = "0";
            }

                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "RLH") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMTMinsRLH = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "RLH").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMTMinsRLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "HOME") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMTMinsFLH = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "HOME").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMTMinsFLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "ROAM") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMTMinsRoaming = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "ROAM").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMTMinsRoaming = "0";
            }


                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "RLH") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMTSMSRLH = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "RLH").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMTSMSRLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "HOME") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMTSMSFLH = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "HOME").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMTSMSFLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "ROAM") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMTSMSRoaming = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_ONNET_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "ROAM").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OnNetRegularMTSMSRoaming = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "RLH") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MTMinsRLH = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "RLH").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MTMinsRLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "HOME") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MTMinsFLH = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "HOME").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MTMinsFLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "ROAM") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MTMinsRoaming = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "ROAM").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MTMinsRoaming = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "RLH") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MTSMSRLH = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "RLH").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MTSMSRLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "HOME") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MTSMSFLH = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "HOME").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MTSMSFLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "ROAM") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MTSMSRoaming = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MT_OFFNET_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "ROAM").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MTSMSRoaming = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "RLH") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone1MinsRLH = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "RLH").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone1MinsRLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "HOME") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone1MinsFLH = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "HOME").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone1MinsFLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "ROAM") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone1MinsRoaming = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "ROAM").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone1MinsRoaming = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "RLH") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone1SMSRLH = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "RLH").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone1SMSRLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "HOME") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone1SMSFLH = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "HOME").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone1SMSFLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "ROAM") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone1SMSRoaming = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET1_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "ROAM").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone1SMSRoaming = "0";
            }

                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_CALL").FirstOrDefault() != null) &&
                    (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "RLH") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone2MinsRLH = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "RLH").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone2MinsRLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "HOME") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone2MinsFLH = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "HOME").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone2MinsFLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "ROAM") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone2MinsRoaming = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "ROAM").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone2MinsRoaming = "0";
            }

                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "RLH") != null))
            {

                subscriberPlanDetails.prepostAccountDetails.OffNetZone2SMSRLH = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "RLH").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone2SMSRLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "HOME") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone2SMSFLH = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "HOME").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone2SMSFLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "ROAM") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone2SMSRoaming = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET2_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "ROAM").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone2SMSRoaming = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "RLH") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MinsRLH = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "RLH").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MinsRLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "HOME") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MinsFLH = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "HOME").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MinsFLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_CALL").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_CALL").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "ROAM") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MinsRoaming = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_CALL").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "ROAM").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3MinsRoaming = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "RLH") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3SMSRLH = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "RLH").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3SMSRLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "HOME") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3SMSFLH = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "HOME").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);
            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3SMSFLH = "0";
            }
                if ((bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_SMS").FirstOrDefault() != null) &&
                (bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_SMS").FirstOrDefault().COUNTERS.COUNTER.FirstOrDefault(x => x.TYPE == "ROAM") != null))
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3SMSRoaming = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.BALANCES.BALANCE.Where(x => x.TYPE == "MO_OFFNET3_SMS").FirstOrDefault().COUNTERS.COUNTER.Where(x => x.TYPE == "ROAM").FirstOrDefault().LIMITS.LIMIT.Where(x => x.TYPE == "MAX").FirstOrDefault().AVAILABLE);

            }
            else
            {
                subscriberPlanDetails.prepostAccountDetails.OffNetZone3SMSRoaming = "0";
            }

            }
            // 4772 subscriber

            
            subscriberPlanDetails.prepostAccountDetails.OnNetPromoMins = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.PONM);
            subscriberPlanDetails.prepostAccountDetails.OnNetPromoSMS = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OnNetPromoMTMins = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OnNetPromoMTSMS = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OnNetPromoData = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetZone1 = bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.FZ1;
            subscriberPlanDetails.prepostAccountDetails.OffNetZone1Mins = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.FMZ1);
            subscriberPlanDetails.prepostAccountDetails.OffNetZone1SMS = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.FSZ1);
            subscriberPlanDetails.prepostAccountDetails.OffNetZone1MTMins = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetZone1MTSMS = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetZone1Data = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetZone2 = bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.FZ2;
            subscriberPlanDetails.prepostAccountDetails.OffNetZone2Mins = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.FMZ2);
            subscriberPlanDetails.prepostAccountDetails.OffNetZone2SMS = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.FSZ2);
            subscriberPlanDetails.prepostAccountDetails.OffNetZone2MTMins = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetZone2MTSMS = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetZone2Data = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetZone3 = bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.FZ3;
            subscriberPlanDetails.prepostAccountDetails.OffNetZone3Mins = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.FMZ3);
            subscriberPlanDetails.prepostAccountDetails.OffNetZone3SMS = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.FSZ3);
            subscriberPlanDetails.prepostAccountDetails.OffNetZone3MTMins = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.OFMTM);
            subscriberPlanDetails.prepostAccountDetails.OffNetZone3MTSMS = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.OFMTS);
            subscriberPlanDetails.prepostAccountDetails.OffNetZone3Data = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone1Mins = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.PFM1);
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone1SMS = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.PFZ1);
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone1MTMins = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone1MTSMS = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone1Data = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone2Mins = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.PFM2);
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone2SMS = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.PFZ2);
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone2MTMins = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone2MTSMS = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone2Data = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone3Mins = GetMinutes(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.PFM3);
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone3SMS = GetSMS(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.PFZ3);
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone3MTMins = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone3MTSMS = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone3Data = string.Empty;

            subscriberPlanDetails.prepostAccountDetails.OnNetRegularMinsExpiry = Utility.GetDateconvertion(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.ONMDT, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
            subscriberPlanDetails.prepostAccountDetails.OnNetRegularSMSExpiry = Utility.GetDateconvertion(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.ONSDT, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
            subscriberPlanDetails.prepostAccountDetails.OnNetRegularMTMinsExpiry = Utility.GetDateconvertion(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.ONMTEXP, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);

            subscriberPlanDetails.prepostAccountDetails.OnNetPromoMinsExpiry = Utility.GetDateconvertion(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.PONMINEXPDT, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
            subscriberPlanDetails.prepostAccountDetails.OnNetPromoSMSExpiry = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OnNetPromoMTMinsExpiry = string.Empty;

            subscriberPlanDetails.prepostAccountDetails.OffNetZone1Expiry = Utility.GetDateconvertion(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.OFMDT1, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
            subscriberPlanDetails.prepostAccountDetails.OffNetZone1SMSExpiry = Utility.GetDateconvertion(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.OFSDT1, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
            subscriberPlanDetails.prepostAccountDetails.OffNetZone1MTMinsExpiry = string.Empty;

            subscriberPlanDetails.prepostAccountDetails.OffNetZone2MinsExpiry = Utility.GetDateconvertion(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.OFMDT2, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
            subscriberPlanDetails.prepostAccountDetails.OffNetZone2SMSExpiry = Utility.GetDateconvertion(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.OFSDT2, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
            subscriberPlanDetails.prepostAccountDetails.OffNetZone2MTMinsExpiry = string.Empty;

            subscriberPlanDetails.prepostAccountDetails.OffNetZone3MinsExpiry = Utility.GetDateconvertion(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.OFMDT3, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
            subscriberPlanDetails.prepostAccountDetails.OffNetZone3SMSExpiry = Utility.GetDateconvertion(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.OFSDT3, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
            subscriberPlanDetails.prepostAccountDetails.OffNetZone3MTMinsExpiry = Utility.GetDateconvertion(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.OFMTEXP, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);

            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone1MinsExpiry = Utility.GetDateconvertion(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.POFMINSEXPDT1, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone1SMSExpiry = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone1MTMinsExpiry = string.Empty;

            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone2MinsExpiry = Utility.GetDateconvertion(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.POFMINSEXPDT2, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone2SMSExpiry = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone2MTMinsExpiry = string.Empty;

            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone3MinsExpiry = Utility.GetDateconvertion(bundleDetailResp.getFreeAccountBalanceCRMNotServiceCode.POFMINSEXPDT3, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone3SMSExpiry = string.Empty;
            subscriberPlanDetails.prepostAccountDetails.OffNetPromoZone3MTMinsExpiry = string.Empty;

            return subscriberPlanDetails;
        }

        public string GetMinutes(string Value)
        {
            string strValue = string.Empty;
            if (!string.IsNullOrEmpty(Value))
            {
                if (Value == "U")
                {
                    strValue = Value;
                }
                //strValue = Math.Round(Convert.ToDouble(Value) / 60, 2).ToString();
                else if (!string.IsNullOrEmpty(clientSetting.preSettings.AvailableTalkTime) && clientSetting.preSettings.AvailableTalkTime != "0")
                {
                    strValue = Math.Round(Convert.ToDouble(Value) / Convert.ToInt16(clientSetting.preSettings.AvailableTalkTime), 2).ToString();
                }
                else
                {
                    strValue = Math.Round(Convert.ToDouble(Value) / 1, 2).ToString();
                }
            }
            else
                strValue = "0.00";

            if (clientSetting.mvnoSettings.enableCurrencyConversion.ToUpper() == "TRUE")
            {
                strValue = strValue.Replace(".", clientSetting.mvnoSettings.currencyConversionValue);

            }


            return strValue;
        }

        //public string GetMinutesNew(string Value)
        //{

        //    if (clientSetting.mvnoSettings.enableCurrencyConversion.ToUpper() == "TRUE")
        //    {
        //        if (!string.IsNullOrEmpty(Value))
        //        {
        //            return Math.Round(Convert.ToDouble(Value) / Convert.ToDouble(clientSetting.mvnoSettings.voiceConversionValue), 0).ToString();
        //        }
        //        else
        //            return "0";
        //    }
        //    else
        //    {
        //        return Value;
        //    }
        //}

        public string GetSMS(string Value)
        {
            //string strValue = string.Empty;
            if (clientSetting.mvnoSettings.enableCurrencyConversion.ToUpper() == "TRUE")
            {
                if (!string.IsNullOrEmpty(Value))
                {
                    Value = Value.Replace(".", clientSetting.mvnoSettings.currencyConversionValue);
                }
            }
            //else
            //{
            //    strValue = Value;
            //}
            return Value;
        }


        public SubscriberPlanDetails PostPaidSubscriberPlan()
        {
            SubscriberPlanDetails subscriberPlanDetails = new SubscriberPlanDetails();
            PostpaidBillinginfo postpaidBillinginfo = new PostpaidBillinginfo();
            Postpaidsubscriber postpaidSubscriber = new Postpaidsubscriber();
            RetailerBundleListResponse retailerBundleResp = new RetailerBundleListResponse();
            RetailerBundleList retailerBundleReq = new RetailerBundleList();
            UnbilledUsage unbilledUsageReq = new UnbilledUsage();
            UnbilledUsageResponse unbilledUsageResp = new UnbilledUsageResponse();
            ServiceInvokeCRM serviceCRM;

            try
            {
                subscriberPlanDetails.prepostAccountDetails = new PrePostAccountDetails();
                subscriberPlanDetails.preBundleList = new List<BundlesList>();
                subscriberPlanDetails.preBundleList1 = new List<BundlesList>();

                #region PostpaidBillinginfo

                try
                {
                    postpaidSubscriber.BrandCode = clientSetting.brandCode;
                    postpaidSubscriber.CountryCode = clientSetting.countryCode;
                    postpaidSubscriber.LanguageCode = clientSetting.langCode;
                    postpaidSubscriber.MSISDN = Session["MobileNumber"].ToString();

                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                        postpaidBillinginfo = serviceCRM.PostpaidBillingDetails(postpaidSubscriber);

                    subscriberPlanDetails.prepostAccountDetails.DueAmount = postpaidBillinginfo.DueAmount;
                    subscriberPlanDetails.prepostAccountDetails.DueDate = Utility.FormatDateTime(postpaidBillinginfo.DueDate, clientSetting.mvnoSettings.dateTimeFormat);
                    subscriberPlanDetails.prepostAccountDetails.OverDueAmount = postpaidBillinginfo.OverDueAmount;
                    subscriberPlanDetails.prepostAccountDetails.LastPaidAmount = postpaidBillinginfo.LastPaidAmount;
                    subscriberPlanDetails.prepostAccountDetails.LastPaidAmountDate = Utility.FormatDateTime(postpaidBillinginfo.LastPaidDate, clientSetting.mvnoSettings.dateTimeFormat);
                    subscriberPlanDetails.prepostAccountDetails.LastPaidAmountStatus = postpaidBillinginfo.LastPaidBy;
                }
                catch (Exception exp)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                }
                finally
                {
                    postpaidBillinginfo = null;
                    postpaidSubscriber = null;
                    serviceCRM = null;
                }
                #endregion

                #region RetailerBundleList



                try
                {
                    retailerBundleReq.BrandCode = clientSetting.brandCode;
                    retailerBundleReq.CountryCode = clientSetting.countryCode;
                    retailerBundleReq.LanguageCode = clientSetting.langCode;
                    retailerBundleReq.Msisdn = Session["MobileNumber"].ToString();

                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                        retailerBundleResp = serviceCRM.PostpaidRetailerBundleList(retailerBundleReq);


                    #region postPaidBundle
                    try
                    {
                        foreach (PostPaidBundleList postPaidBundle in retailerBundleResp.postPaidBundleList)
                        {
                            postPaidBundle.discountInfo.bundleDiscountRate = new List<PostPaidBundleDiscountRate>();

                            List<string> discountTag = new List<string>(postPaidBundle.discountInfo.DISCOUNT_RATE.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

                            foreach (string discountDetail in discountTag)
                            {
                                try
                                {
                                    List<string> discountList = new List<string>(discountDetail.Split(new char[] { '|' }));

                                    PostPaidBundleDiscountRate discountRate = new PostPaidBundleDiscountRate();

                                    discountRate.serviceCategory = discountList[0];
                                    discountRate.callFeature = discountList[1];
                                    discountRate.callMode = discountList[2];
                                    discountRate.callType = discountList[3];

                                    discountRate.originZone = discountList[4];
                                    discountRate.destZone = discountList[5];
                                    discountRate.bundleAllowance = discountList[6];
                                    discountRate.availableAllowance = discountList[7];

                                    postPaidBundle.discountInfo.bundleDiscountRate.Add(discountRate);
                                }
                                catch
                                {

                                }
                            }
                        }
                        retailerBundleResp.postPaidBundleList.ForEach(p => p.ExpiryDate = Utility.FormatDateTime(p.ExpiryDate, clientSetting.mvnoSettings.dateTimeFormat));
                        retailerBundleResp.postPaidBundleList.ForEach(p => p.PurchaseDate = Utility.FormatDateTime(p.PurchaseDate, clientSetting.mvnoSettings.dateTimeFormat));
                        retailerBundleResp.postPaidBundleList.ForEach(p => p.discountInfo.EXPIRYDATE = Utility.FormatDateTime(p.discountInfo.EXPIRYDATE, clientSetting.mvnoSettings.dateTimeFormat));
                    }
                    catch (Exception eX)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                    }
                    #endregion

                    subscriberPlanDetails.postBundleList = retailerBundleResp.postPaidBundleList;

                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Postpaid Subscriber Data");
                }
                catch (Exception exp)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                }
                finally
                {
                    retailerBundleResp = null;
                    retailerBundleReq = null;
                }
                #endregion

                #region UnbilledUsage



                try
                {
                    unbilledUsageReq.BrandCode = clientSetting.brandCode;
                    unbilledUsageReq.CountryCode = clientSetting.countryCode;
                    unbilledUsageReq.LanguageCode = clientSetting.langCode;
                    unbilledUsageReq.MSISDN = Session["MobileNumber"].ToString();
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                        unbilledUsageResp = serviceCRM.CRMUnBilledUsage(unbilledUsageReq);

                    if (unbilledUsageResp.responseDetails.ResponseCode == "0")
                    {
                        subscriberPlanDetails.prepostAccountDetails.unbilledAmount = unbilledUsageResp.Amount;
                    }
                    else
                    {
                        subscriberPlanDetails.prepostAccountDetails.unbilledAmount = string.Empty;
                    }
                }
                catch (Exception eX)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                }
                finally
                {
                    unbilledUsageReq = null;
                    unbilledUsageResp = null;
                    serviceCRM = null;
                }
                #endregion
                return subscriberPlanDetails;
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return subscriberPlanDetails;
            }
            finally
            {
                //subscriberPlanDetails = null;
                postpaidBillinginfo = null;
                postpaidSubscriber = null;
                retailerBundleResp = null;
                retailerBundleReq = null;
                unbilledUsageReq = null;
                unbilledUsageResp = null;
            }

        }

        #region FRR - 3023

        public JsonResult ManageReserveBundle(ManageReserveBundleRequest reserveBundleRequest)
        {
            CRMResponse crmResponse = new CRMResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - ManageReserveBundle Start");
                reserveBundleRequest.CountryCode = clientSetting.countryCode;
                reserveBundleRequest.BrandCode = clientSetting.brandCode;
                reserveBundleRequest.LanguageCode = clientSetting.langCode;
                reserveBundleRequest.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                crmResponse = serviceCRM.CRMManageReserveBundle(reserveBundleRequest);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - ManageReserveBundle End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                reserveBundleRequest = null;
                serviceCRM = null;
            }
            return Json(crmResponse);
        }

        public JsonResult Manage_FLH_Bundle(FLHBundleRequest flhBundleReq)
        {
            FLHBundleResponse flhBundleResp = new FLHBundleResponse();
            string[] dates;
            string dateval = string.Empty;
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - Manage_FLH_Bundle Start");
                //flhBundleReq.ExpiryDate = Utility.FormatDateTimeToService(flhBundleReq.ExpiryDate, clientSetting.mvnoSettings.dateTimeFormat);
                if (!string.IsNullOrEmpty(flhBundleReq.ExpiryDate))
                {
                    dates = flhBundleReq.ExpiryDate.Split('/');
                    dateval = dates[0].Trim() + dates[1].Trim();
                    flhBundleReq.ExpiryDate = dateval;
                }
                else
                {
                    flhBundleReq.ExpiryDate = flhBundleReq.ExpiryDate;
                }
                flhBundleReq.CountryCode = clientSetting.countryCode;
                flhBundleReq.BrandCode = clientSetting.brandCode;
                flhBundleReq.LanguageCode = clientSetting.langCode;
                flhBundleReq.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                flhBundleResp = serviceCRM.CRMFLHBundle(flhBundleReq);
                try
                {
                    if (flhBundleResp != null && flhBundleResp.Response != null && flhBundleResp.Response.ResponseCode != null)
                    {
                        if (!string.IsNullOrEmpty(flhBundleReq.Status) && flhBundleReq.Status == "1")
                        {
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("FLHsubscribe_" + flhBundleResp.Response.ResponseCode);
                            flhBundleResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? flhBundleResp.Response.ResponseDesc : errorInsertMsg;
                        }
                        if (!string.IsNullOrEmpty(flhBundleReq.Status) && flhBundleReq.Status == "0")
                        {
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("FLHunsubscribe_" + flhBundleResp.Response.ResponseCode);
                            flhBundleResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? flhBundleResp.Response.ResponseDesc : errorInsertMsg;
                        }
                        if (flhBundleResp.Response.ResponseCode != "55" || flhBundleResp.Response.ResponseCode != "56" || flhBundleResp.Response.ResponseCode != "57")
                        {
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("FLHsubscribe_" + flhBundleResp.Response.ResponseCode);
                            flhBundleResp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? flhBundleResp.Response.ResponseDesc : errorInsertMsg;
                        }
                    }
                }
                catch (Exception exFLHBundle)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exFLHBundle);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - Manage_FLH_Bundle End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                flhBundleReq = null;
                dates = null;
                dateval = string.Empty;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(flhBundleResp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Modify_Renewal(ModifyRenewalRequest modifyRenewalReq)
        {
            ModifyRenewalResponse cmrResponse = new ModifyRenewalResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - Modify_Renewal Start");
                modifyRenewalReq.CountryCode = clientSetting.countryCode;
                modifyRenewalReq.BrandCode = clientSetting.brandCode;
                modifyRenewalReq.LanguageCode = clientSetting.langCode;
                if (modifyRenewalReq.cardPosition != null)
                {
                    if (modifyRenewalReq.cardPosition.Count > 0)
                    {
                        modifyRenewalReq.cardPosition = new List<string>(modifyRenewalReq.cardPosition[0].Split('|'));
                    }
                }
                if (modifyRenewalReq.FamilyAccID != "" && modifyRenewalReq.IsFamilyBundle == "1") { }
                else
                {
                    modifyRenewalReq.MSISDN = Session["MobileNumber"].ToString();
                }
                modifyRenewalReq.Email = Session["eMailID"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                cmrResponse = serviceCRM.CRMModifyRenewal(modifyRenewalReq);
                try
                {
                    //static string opcode = string.Empty;                        
                    if (cmrResponse != null && cmrResponse.Response != null && cmrResponse.Response.ResponseCode != null)
                    {
                        if (modifyRenewalReq.RenewalType == "AUTO")
                        {
                            opcode = modifyRenewalReq.Opcode;
                        }
                        if (!string.IsNullOrEmpty(modifyRenewalReq.RenewalType) && modifyRenewalReq.RenewalType == "AUTO" && modifyRenewalReq.BundleStatus == "1")
                        {
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("AUTORenewalEnable_" + cmrResponse.Response.ResponseCode);
                            cmrResponse.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? cmrResponse.Response.ResponseDesc : errorInsertMsg;
                        }
                        if (!string.IsNullOrEmpty(modifyRenewalReq.RenewalType) && modifyRenewalReq.RenewalType == "AUTO" && modifyRenewalReq.BundleStatus == "0")
                        {
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("AUTORenewalDisable_" + cmrResponse.Response.ResponseCode);
                            cmrResponse.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? cmrResponse.Response.ResponseDesc : errorInsertMsg;
                        }
                        if (!string.IsNullOrEmpty(modifyRenewalReq.RenewalType) && modifyRenewalReq.RenewalType == "MODIFY" && modifyRenewalReq.Opcode == "1")
                        {
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ModifyRenewal_" + cmrResponse.Response.ResponseCode);
                            cmrResponse.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? cmrResponse.Response.ResponseDesc : errorInsertMsg;
                        }
                        if (!string.IsNullOrEmpty(modifyRenewalReq.RenewalType) && modifyRenewalReq.RenewalType == "MODIFY" && opcode == "2" && modifyRenewalReq.BundleStatus == "1")
                        {
                            opcode = "";
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("AUTORenewalEnable_" + cmrResponse.Response.ResponseCode);
                            cmrResponse.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? cmrResponse.Response.ResponseDesc : errorInsertMsg;
                        }
                        if (!string.IsNullOrEmpty(modifyRenewalReq.RenewalType) && modifyRenewalReq.RenewalType == "MODIFY" && opcode == "2" && modifyRenewalReq.BundleStatus == "0")
                        {
                            opcode = "";
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("AUTORenewalDisable_" + cmrResponse.Response.ResponseCode);
                            cmrResponse.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? cmrResponse.Response.ResponseDesc : errorInsertMsg;
                        }

                        if (cmrResponse.Response.ResponseCode == "0" && cmrResponse.Response.ResponseCode != "109" && cmrResponse.Response.ResponseCode != "110" && cmrResponse.Response.ResponseCode != "111" && cmrResponse.Response.ResponseCode != "112" && cmrResponse.Response.ResponseCode != "113" && cmrResponse.Response.ResponseCode != "114")
                        {
                            errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ModifyRenewal_" + cmrResponse.Response.ResponseCode);
                            cmrResponse.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? cmrResponse.Response.ResponseDesc : errorInsertMsg;
                        }
                    }
                }
                catch (Exception exModifyRenewal)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exModifyRenewal);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - Modify_Renewal End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                modifyRenewalReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(cmrResponse);
        }


        public JsonResult Change_BPartyNumber(ChangeBPartyNumReq changeBPartyNumReq)
        {
            CRMResponse crmResponse = new CRMResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - Change_BPartyNumber Start");
                changeBPartyNumReq.CountryCode = clientSetting.countryCode;
                changeBPartyNumReq.BrandCode = clientSetting.brandCode;
                changeBPartyNumReq.LanguageCode = clientSetting.langCode;
                changeBPartyNumReq.Msisdn = Session["MobileNumber"].ToString();
                changeBPartyNumReq.UserName = Session["UserName"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                crmResponse = serviceCRM.CRMChangeBPartyNum(changeBPartyNumReq);
                try
                {
                    if (crmResponse != null && crmResponse.ResponseCode != null)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ChangeBPartyNumber_" + crmResponse.ResponseCode);
                        crmResponse.ResponseCode = string.IsNullOrEmpty(errorInsertMsg) ? crmResponse.ResponseCode : errorInsertMsg;
                    }
                }
                catch (Exception exChargePartyNumber)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exChargePartyNumber);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - Change_BPartyNumber End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                changeBPartyNumReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(crmResponse);
        }
        #endregion

        #region FRR - 3083
        public JsonResult CRMManageFriendnumbers(ManageFriendnumbersReq manageFrndReq)
        {
            ManageFriendnumbersRes crmResponse = new ManageFriendnumbersRes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMManageFriendnumbers Start");
                manageFrndReq.CountryCode = clientSetting.countryCode;
                manageFrndReq.BrandCode = clientSetting.brandCode;
                manageFrndReq.LanguageCode = clientSetting.langCode;
                manageFrndReq.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                crmResponse = serviceCRM.CRMManageFriendnumbers(manageFrndReq);    
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMManageFriendnumbers End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                manageFrndReq = null;
                serviceCRM = null;
            }
            return Json(crmResponse);
        }

        public JsonResult CRMBundleBucketExpiryDate(BundleBucketExpiryDate exDateReq)
        {
            CRMResponse crmResponse = new CRMResponse();
            ServiceInvokeCRM serviceCRM;

            try
            {
                exDateReq.CountryCode = clientSetting.countryCode;
                exDateReq.BrandCode = clientSetting.brandCode;
                exDateReq.LanguageCode = clientSetting.langCode;
                exDateReq.MSISDN = Session["MobileNumber"].ToString();

                exDateReq.ExpiryDate = Utility.GetDateconvertion(exDateReq.ExpiryDate, clientSetting.mvnoSettings.dateTimeFormat.ToLower(), false, "yyyyMMdd");


                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    crmResponse = serviceCRM.CRMBundleBucketExpiryDate(exDateReq);
                    if (crmResponse != null && crmResponse.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleExpiry_" + crmResponse.ResponseCode);
                        crmResponse.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? crmResponse.ResponseDesc : errorInsertMsg;
                    }

                return Json(crmResponse);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(crmResponse);
            }
            finally
            {
               // crmResponse = null;
                exDateReq = null;
                serviceCRM = null;
            }
        }

        public JsonResult CRMGetPlansfromPBS(CRMBase requestObject)
        {
            GetPlansfromPBS crmResponse = new GetPlansfromPBS();
            ServiceInvokeCRM serviceCRM;

            try
            {
                requestObject.CountryCode = clientSetting.countryCode;
                requestObject.BrandCode = clientSetting.brandCode;
                requestObject.LanguageCode = clientSetting.langCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    crmResponse = serviceCRM.CRMGetPlansfromPBS(requestObject);

                return Json(crmResponse);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(crmResponse);
            }
            finally
            {
               // crmResponse = null;
                requestObject = null;
                serviceCRM = null;
            }
        }

        public JsonResult CRMpostpaidBilcycleupdate(PostpaidBilcycleupdateReq requestObject)
        {
            PostpaidBilcycleupdateRes crmResponse = new PostpaidBilcycleupdateRes();
            ServiceInvokeCRM serviceCRM;

            try
            {
                requestObject.CountryCode = clientSetting.countryCode;
                requestObject.BrandCode = clientSetting.brandCode;
                requestObject.LanguageCode = clientSetting.langCode;
                requestObject.MSISDN = Convert.ToString(Session["MobileNumber"]);
                requestObject.Email = Convert.ToString(Session["eMailID"]);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    crmResponse = serviceCRM.CRMpostpaidBilcycleupdate(requestObject);

                Session["BillCycleID"] = crmResponse.Billcycleid;

                return Json(crmResponse);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(crmResponse);
            }
            finally
            {
                //crmResponse = null;
                requestObject = null;
                serviceCRM = null;
            }
        }


        public JsonResult GetDateconvertion(DateconvertionReq requestObject)
        {
            DateconvertionResp resp = new DateconvertionResp();
            try
            {
                resp.formattedDate = Utility.GetDateconvertion(requestObject.strDate, requestObject.strIOdate, false, requestObject.dateTimeFormat);
                return Json(resp);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(resp);
            }
            finally
            {
                //resp = null;
                requestObject = null;
            }
        }

        #endregion

        #region FamilyPlan FRR:3218
        public JsonResult CRMPayGFamilyPlan(FamilyPlanReq familyPlanReq)
        {
            FamilyPlanRes familyPlanRes = new FamilyPlanRes();
            ServiceInvokeCRM serviceCRM;

            try
            {
                familyPlanReq.CountryCode = clientSetting.countryCode;
                familyPlanReq.BrandCode = clientSetting.brandCode;
                familyPlanReq.LanguageCode = clientSetting.langCode;
                //familyPlanReq.MSISDN = Session["MobileNumber"].ToString();
                familyPlanReq.SIM_CATEGORY = Session["SIM_CATEGORY"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    familyPlanRes = serviceCRM.CRMPayGFamilyPlan(familyPlanReq);
                    if (familyPlanRes != null && familyPlanRes.ResponseDetails != null && familyPlanRes.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("FamilyPlan_" + familyPlanRes.ResponseDetails.ResponseCode);
                        familyPlanRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? familyPlanRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }


                return Json(familyPlanRes);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(familyPlanRes);
            }
            finally
            {
               // familyPlanRes = null;
                familyPlanReq = null;
                serviceCRM = null;
            }
        }

        public JsonResult ManageFamilyPlan()
        {
            ViewFamilyMembersRes ObjResp = new ViewFamilyMembersRes();
            ViewFamilyMembersReq ObjReq = new ViewFamilyMembersReq();
            ServiceInvokeCRM serviceCRM;

            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    ObjResp = serviceCRM.ViewFamilyMembers(ObjReq);

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
              //  ObjResp = null;
                serviceCRM = null;
            }
        }
        #endregion



        #region FRR - 4560 NRestrinctionBundle

        public JsonResult RestrictionNCount(string Restrictcount)
        {
            RestrictNcountResponse ObjResp = new RestrictNcountResponse();
            RestrictNcountRequest ObjReq = JsonConvert.DeserializeObject<RestrictNcountRequest>(Restrictcount);
            string errorInsertMsg = string.Empty;
            ServiceInvokeCRM serviceCRM;

            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    ObjResp = serviceCRM.RestrictonNcount(ObjReq);

                if (ObjResp != null && ObjResp.ResponseDetails.ResponseCode != null && ObjResp.ResponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RestrictionNCount" + ObjResp.ResponseDetails.ResponseCode);
                    ObjResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - RestrictionNCountRequestResponse_ End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SubscriberController - exception-RestrictonNcountDetails - " + this.ControllerContext, ex);

            }
            finally
            {
                ObjReq = null;
                errorInsertMsg = string.Empty;
                serviceCRM = null;
            }
            return Json(ObjResp, JsonRequestBehavior.AllowGet);
        }
        #endregion




        #region FRR - 4662 

        public JsonResult GetBundleAllowance(string BundleAllowanceReq)
        {
            BundleAllowancesResponse ObjResp = new BundleAllowancesResponse();
            BundleAllowanceRequest ObjReq = new BundleAllowanceRequest();
            string errorInsertMsg = string.Empty;
            ServiceInvokeCRM serviceCRM;

            try
            {
                ObjReq = JsonConvert.DeserializeObject<BundleAllowanceRequest>(BundleAllowanceReq);
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.Msisdn = Convert.ToString(Session["MobileNumber"]);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    ObjResp = serviceCRM.GetBundleAllowance(ObjReq);


               

                if (ObjResp.Allowances.DataAllowances != null)
                {
                    if (ObjResp.Allowances.DataAllowances.Roam != null)
                    {
                        ObjResp.Allowances.DataAllowances.Roam.ForEach(b => b.ApplicableDays = Resources.BundleResources.ResourceManager.GetString("GetBundleAllowance_" + b.ApplicableTime));
                    }
                    if (ObjResp.Allowances.DataAllowances.LocalRoam != null)
                    {
                        ObjResp.Allowances.DataAllowances.LocalRoam.ForEach(b => b.ApplicableDays = Resources.BundleResources.ResourceManager.GetString("GetBundleAllowance_" + b.ApplicableTime));
                    }
                    if (ObjResp.Allowances.DataAllowances.RLH != null)
                    {
                        ObjResp.Allowances.DataAllowances.RLH.ForEach(b => b.ApplicableDays = Resources.BundleResources.ResourceManager.GetString("GetBundleAllowance_" + b.ApplicableTime));
                    }
                    if (ObjResp.Allowances.DataAllowances.home != null)
                    {
                        ObjResp.Allowances.DataAllowances.home.ForEach(b => b.ApplicableDays = Resources.BundleResources.ResourceManager.GetString("GetBundleAllowance_" + b.ApplicableTime));
                    }
                }

                if (ObjResp.Allowances.VoiceAllowances != null)
                {
                    if (ObjResp.Allowances.VoiceAllowances.Roam != null)
                    {
                        ObjResp.Allowances.VoiceAllowances.Roam.ForEach(b => b.ApplicableDays = Resources.BundleResources.ResourceManager.GetString("GetBundleAllowance_" + b.ApplicableTime));
                    }
                    if (ObjResp.Allowances.VoiceAllowances.LocalRoam != null)
                    {
                        ObjResp.Allowances.VoiceAllowances.LocalRoam.ForEach(b => b.ApplicableDays = Resources.BundleResources.ResourceManager.GetString("GetBundleAllowance_" + b.ApplicableTime));
                    }
                    if (ObjResp.Allowances.VoiceAllowances.RLH != null)
                    {
                        ObjResp.Allowances.VoiceAllowances.RLH.ForEach(b => b.ApplicableDays = Resources.BundleResources.ResourceManager.GetString("GetBundleAllowance_" + b.ApplicableTime));
                    }
                    if (ObjResp.Allowances.VoiceAllowances.home != null)
                    {
                        ObjResp.Allowances.VoiceAllowances.home.ForEach(b => b.ApplicableDays = Resources.BundleResources.ResourceManager.GetString("GetBundleAllowance_" + b.ApplicableTime));
                    }
                }

                if (ObjResp.Allowances.SMSAllowances != null)
                {
                    if (ObjResp.Allowances.SMSAllowances.Roam != null)
                    {
                        ObjResp.Allowances.SMSAllowances.Roam.ForEach(b => b.ApplicableDays = Resources.BundleResources.ResourceManager.GetString("GetBundleAllowance_" + b.ApplicableTime));
                    }
                    if (ObjResp.Allowances.SMSAllowances.LocalRoam != null)
                    {
                        ObjResp.Allowances.SMSAllowances.LocalRoam.ForEach(b => b.ApplicableDays = Resources.BundleResources.ResourceManager.GetString("GetBundleAllowance_" + b.ApplicableTime));
                    }
                    if (ObjResp.Allowances.SMSAllowances.RLH != null)
                    {
                        ObjResp.Allowances.SMSAllowances.RLH.ForEach(b => b.ApplicableDays = Resources.BundleResources.ResourceManager.GetString("GetBundleAllowance_" + b.ApplicableTime));
                    }
                    if (ObjResp.Allowances.SMSAllowances.home != null)
                    {
                        ObjResp.Allowances.SMSAllowances.home.ForEach(b => b.ApplicableDays = Resources.BundleResources.ResourceManager.GetString("GetBundleAllowance_" + b.ApplicableTime));
                    }
                }


                if (ObjResp != null && ObjResp.ResponseDetails.ResponseCode != null && ObjResp.ResponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleEligibleValue" + ObjResp.ResponseDetails.ResponseCode);
                    ObjResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - BundleEligibleValueRequestResponse_ End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SubscriberController - exception-BundleEligibleValueDetails - " + this.ControllerContext, ex);

            }
            finally
            {
                ObjReq = null;
                errorInsertMsg = string.Empty;
                serviceCRM = null;
            }
            return Json(ObjResp, JsonRequestBehavior.AllowGet);
        }
        #endregion



        public JsonResult BundleEligibleValue(string BundleEligibleReq)
        {
            BundleEligibleResponse ObjResp = new BundleEligibleResponse();
            BundleEligibleValueRequest ObjReq = new BundleEligibleValueRequest();
            string errorInsertMsg = string.Empty;
            ServiceInvokeCRM serviceCRM;

            try
            {
                ObjReq = JsonConvert.DeserializeObject<BundleEligibleValueRequest>(BundleEligibleReq);
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    ObjResp = serviceCRM.BundleEligibleValue(ObjReq);

                if (ObjResp != null && ObjResp.ResponseDetails.ResponseCode != null && ObjResp.ResponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleEligibleValue" + ObjResp.ResponseDetails.ResponseCode);
                    ObjResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - BundleEligibleValueRequestResponse_ End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SubscriberController - exception-BundleEligibleValueDetails - " + this.ControllerContext, ex);

            }
            finally
            {
                ObjReq = null;
                errorInsertMsg = string.Empty;
                serviceCRM = null;
            }
            return Json(ObjResp, JsonRequestBehavior.AllowGet);
        }




        #region FRR - 2972 PlanHistory
        public JsonResult CRMPrePostPlanHis(PrePostPlanHisRequest requestObject)
        {
            PrePostPlanHisResponse RespObj = new PrePostPlanHisResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMPrePostPlanHis Start");
                requestObject.CountryCode = clientSetting.countryCode;
                requestObject.BrandCode = clientSetting.brandCode;
                requestObject.LanguageCode = clientSetting.langCode;
                requestObject.MSISDN = Session["MobileNumber"].ToString();
                requestObject.ICCID = Session["ICCID"].ToString();
                if (Convert.ToString(Session["isPrePaid"]) == "1")
                    requestObject.Mode = "PRE";
                else
                    requestObject.Mode = "POST";
                requestObject.PlanId = Convert.ToString(Session["PlanId"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                RespObj = serviceCRM.CRMPrePostPlanHis(requestObject);
                RespObj.SIMActivationDate = Utility.GetDateconvertion(RespObj.SIMActivationDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                RespObj.FirstUsageDate = Utility.GetDateconvertion(RespObj.FirstUsageDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                RespObj.ValidityDate = Utility.GetDateconvertion(RespObj.ValidityDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMPrePostPlanHis End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                requestObject = null;
                serviceCRM = null;
            }
            return Json(RespObj, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region FRR 2752 - Minimum Allowance Limit
        public JsonResult CRMGetMinimumAllowanceLimit(MinimumAllowanceLimitReq requestObject)
        {
            MinimumAllowanceLimitRes RespObj = new MinimumAllowanceLimitRes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMGetMinimumAllowanceLimit Start");
                requestObject.CountryCode = clientSetting.countryCode;
                requestObject.BrandCode = clientSetting.brandCode;
                requestObject.LanguageCode = clientSetting.langCode;
                requestObject.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                RespObj = serviceCRM.CRMGetMinimumAllowanceLimit(requestObject);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMGetMinimumAllowanceLimit End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                requestObject = null;
                serviceCRM = null;
            }
            return Json(RespObj, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Not used methods in CRM application

        public SubscriberPlanDetails LoadPrepaidselectBundle(string BundleName)
        {
            FreeAccountBalanceRequest objFreeAccountBalanace = new FreeAccountBalanceRequest();
            SubscriberPlanDetails subsBundleDetails = new SubscriberPlanDetails();
            PrePostAccountDetails objprepost = new PrePostAccountDetails();
            FreeAccountBalanceResponse ObjSubscriber = new FreeAccountBalanceResponse();
            List<PrePostAccountDetails> obj = new List<PrePostAccountDetails>();
            ServiceInvokeCRM serviceCRM;


            try
            {
                try
                {
                    objFreeAccountBalanace.BrandCode = clientSetting.brandCode;
                    objFreeAccountBalanace.CountryCode = clientSetting.countryCode;
                    objFreeAccountBalanace.LanguageCode = clientSetting.langCode;
                    objFreeAccountBalanace.Msisdn = Session["MobileNumber"].ToString();
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                        ObjSubscriber = serviceCRM.SubscriberPrepaidBalanceDetails(objFreeAccountBalanace);

                }
                catch (Exception exp)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                }

                
                objprepost = new PrePostAccountDetails();
                if (ObjSubscriber.SubscriberPrepaidBalance != null && ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance != null)
                {
                    for (int i = 0; i < ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance.Count; i++)
                    {
                        if (ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].BNAME == BundleName)
                        {
                            objprepost = new PrePostAccountDetails();
                            objprepost.BundleOnNetMin = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].ONM;
                            objprepost.BundleOnNetMinSMS = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].ONS;
                            objprepost.BundleMTOnNetMin = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].ONMTM;
                            objprepost.BundleMTOnNetMinSMS = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].ONMTS;
                            objprepost.BundleZone1 = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].FZ1;
                            objprepost.BundleZone1Min = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].FMZ1;
                            objprepost.BundleZone1MinSMS = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].FSZ1;
                            objprepost.BundleZone2 = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].FZ2;
                            objprepost.BundleZone2Min = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].FMZ2;
                            objprepost.BundleZone2MinSMS = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].FSZ2;
                            objprepost.BundleZone3 = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].FZ3;
                            objprepost.BundleZone3Min = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].FMZ3;
                            objprepost.BundleZone3MinSMS = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].FSZ3;
                            objprepost.BundleMTZone = string.Empty;
                            objprepost.BundleMTZoneMin = string.Empty;
                            objprepost.BundleMTZoneMinSMS = string.Empty;
                            objprepost.BundleBalance = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].BBS;
                            objprepost.BundleOBACredit = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].OBA_CREDIT_BALANCE;
                            objprepost.BundleName = ObjSubscriber.SubscriberPrepaidBalance.GetFreeAccountBalance[i].BNAME;
                            objprepost.OnNetMTBundle = string.Empty;
                            objprepost.AutoRenewal = string.Empty;
                            objprepost.RoamUsage = string.Empty;
                            objprepost.LocalRoamUsage = string.Empty;
                            objprepost.AddOnBundle = string.Empty;
                            objprepost.DataAllowance = string.Empty;

                            obj.Add(objprepost);
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Selected Bundle");
                        }
                    }
                    subsBundleDetails.prepostAccountDetails = obj[0];
                }

                return subsBundleDetails;
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return subsBundleDetails;
            }
            finally
            {
                objFreeAccountBalanace = null;
               // subsBundleDetails = null;
                objprepost = null;
                ObjSubscriber = null;
                obj = null;
                serviceCRM = null;
            }
        }

        [HttpPost]
        public ActionResult SubscriberPostpaidUpdate(GetCRMPostpaidUpdateSubscriberRequest objSubscriber)
        {
            SubscriberPostpaidUpdateRes objSubscriberUpdateRes = new SubscriberPostpaidUpdateRes();
            GetCRMPostpaidUpdateSubscriberResponse objSubresp = new GetCRMPostpaidUpdateSubscriberResponse();
            ServiceInvokeCRM serviceCRM;

            try
            {               

                objSubscriber.DOB = Utility.GetDateconvertion(objSubscriber.DOB, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    objSubresp = serviceCRM.CRMPostpaidUpdateSubscriberInformation(objSubscriber);

                //objSubscriberUpdateRes.Title = objSubscriber.Title;
                objSubscriberUpdateRes.FirstName = objSubscriber.FirstName;
                objSubscriberUpdateRes.Lastname = objSubscriber.LastName;
                objSubscriberUpdateRes.ResponseDetails = objSubresp;
                objSubscriberUpdateRes.DOB = Utility.GetDateconvertion(objSubscriber.DOB, "DD/MM/YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubScriber Postpaid Update");

                return Json(objSubscriberUpdateRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(objSubscriberUpdateRes, JsonRequestBehavior.AllowGet);
            }

            finally
            {
               // objSubscriberUpdateRes = null;
                objSubscriber = null;
                objSubresp = null;
                serviceCRM = null;
            }
        }


        public PartialViewResult SubscriberDetails()
        {
            Session["DateFormat"] = ConfigurationManager.AppSettings["DateFormat"].ToString();
            return PartialView();
        }

        public ActionResult SubscriberBar()
        {
            try
            {
                if (Session["isPrePaid"].ToString() == CRM.Models.constantvalues.Prepaid)
                {
                    return PartialView(SubScriberBarPrepaidLoad());
                }
                else
                {
                    return PartialView(SubScriberBarPostpaidLoad());
                }
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            return PartialView();
        }


        public JsonResult SubscriberSummary()
        {
            SubscriberInfo subscriberInfo = new SubscriberInfo();
            try
            {
                if (Session["isPrePaid"].ToString() == CRM.Models.constantvalues.Prepaid)
                {

                    subscriberInfo = SubscriberPrepaidSummary();
                }
                else
                {

                    subscriberInfo = SubscriberPostpaidSummary();
                }
                Session["LoginMode"] = null;
                //clear bypass mode FRR 3324
                return Json(subscriberInfo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(subscriberInfo, JsonRequestBehavior.AllowGet);
            }
            finally
            {
               // subscriberInfo = null;
            }
            
        }



        [HttpPost]
        public ActionResult SubscriberUpdate(GetCRMUpdateSubscriberRequest objSubscriber)
        {
            SubscriberUpdateRes objSubscriberUpdateRes = new SubscriberUpdateRes();
            GetCRMUpdateSubscriberResponse objSubresp = new GetCRMUpdateSubscriberResponse();
            ServiceInvokeCRM serviceCRM;

            try
            {
                
                objSubscriber.MSISDN = Session["MobileNumber"].ToString();
                objSubscriber.DOB = Utility.GetDateconvertion(objSubscriber.DOB, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    objSubresp = serviceCRM.CRMUpdateSubscriberInformation(objSubscriber);

                objSubscriberUpdateRes.Title = objSubscriber.Title;
                objSubscriberUpdateRes.FirstName = objSubscriber.FirstName;
                objSubscriberUpdateRes.Lastname = objSubscriber.LastName;
                objSubscriberUpdateRes.ResponseDetails = objSubresp;
                objSubscriberUpdateRes.DOB = Utility.GetDateconvertion(objSubscriber.DOB, "DD/MM/YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubScriber Update");
                return Json(objSubscriberUpdateRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(objSubscriberUpdateRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //objSubscriberUpdateRes = null;
                objSubresp = null;
                objSubscriber = null;
                serviceCRM = null;
            }
            
        }

        #endregion

        #region SubscriberPromo commented
        //public ActionResult SubscriberPromo()
        //{
        //    List<PromotionsDetail> proDetailList2 = new List<PromotionsDetail>();
        //    try
        //    {
        //        proDetailList2 = PromoList();
        //    }
        //    catch (Exception ex)
        //    {
        //        CRMLogger.WriteException(Session["UserName"].ToString(), this.ControllerContext, ex);
        //    }
        //    return PartialView(proDetailList2);
        //}

        //public JsonResult jsonSubscriberPromo()
        //{
        //    List<PromotionsDetail> proDetailList2 = new List<PromotionsDetail>();

        //    try
        //    {
        //        proDetailList2 = PromoList();
        //    }
        //    catch (Exception ex)
        //    {
        //        CRMLogger.WriteException(Session["UserName"].ToString(), this.ControllerContext, ex);
        //    }

        //    return Json(proDetailList2, JsonRequestBehavior.AllowGet);
        //}

        //public List<PromotionsDetail> PromoList()
        //{
        //    List<string> filePaths = new List<string>();
        //    List<PromotionsDetail> proDetailList = new List<PromotionsDetail>();
        //    List<PromotionsDetail> proDetailList2 = new List<PromotionsDetail>();
        //    PromotionsDetail proDet = new PromotionsDetail();
        //    try
        //    {
        //        DataSet dataSet = new DataSet();
        //        string brandPath = System.Web.HttpContext.Current.Server.MapPath(@"\Library\Promotion\" + clientSetting.countryCode + @"\" + clientSetting.brandCode);

        //        if (Directory.Exists(brandPath))
        //        {
        //            string filePath = brandPath + "/Promotion.xml";
        //            dataSet.ReadXml(filePath);
        //            proDetailList = dataSet.Tables[0].AsEnumerable().Select(p => new PromotionsDetail
        //            {
        //                Promo = p.Field<string>("Promo"),
        //                PromoFile = p.Field<string>("PromoFile"),
        //                PromoPath = p.Field<string>("PromoPath"),
        //                PromoLink = p.Field<string>("PromoLink")
        //            }).ToList();

        //            filePaths = Directory.GetFiles(brandPath, "*.*", SearchOption.TopDirectoryOnly).Select(fileName => Path.GetFileName(fileName)).ToList();

        //            foreach (string f in filePaths)
        //            {
        //                foreach (PromotionsDetail m in proDetailList)
        //                {
        //                    if (f == m.PromoFile)
        //                    {
        //                        proDet = m;
        //                        proDetailList2.Add(proDet);
        //                    }
        //                }
        //            }

        //        }
        //        else
        //        {

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        CRMLogger.WriteException(Session["UserName"].ToString(), this.ControllerContext, ex);
        //    }

        //    return proDetailList2;
        //}


        //public ActionResult SubscriberPromo()
        //{
        //    List<string> filePaths = new List<string>();
        //    try
        //    {
        //        string brandPath = System.Web.HttpContext.Current.Server.MapPath(@"\Library\Promotion\" + clientSetting.countryCode + @"\" + clientSetting.brandCode);

        //        if (Directory.Exists(brandPath))
        //        {
        //            filePaths = Directory.GetFiles(brandPath, "*.*", SearchOption.TopDirectoryOnly).Select(fileName => Path.GetFileName(fileName)).ToList();
        //        }
        //        else
        //        {

        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        CRMLogger.WriteException(Session["UserName"].ToString(), this.ControllerContext, ex);
        //    }
        //    return PartialView(filePaths);
        //}
        #endregion

        #region FRR 3246 FR 3.3 FUP
        [HttpPost]
        public JsonResult PostpaidFUP(PostpaidFUPRequest requestObject)
        {
            PostpaidFUPResponse RespObj = new PostpaidFUPResponse();
            ServiceInvokeCRM serviceCRM;

            try
            {
                requestObject.CountryCode = clientSetting.countryCode;
                requestObject.BrandCode = clientSetting.brandCode;
                requestObject.LanguageCode = clientSetting.langCode;
                requestObject.MSISDN = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    RespObj = serviceCRM.PostpaidFUP(requestObject);


                return Json(RespObj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(RespObj, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                requestObject = null;
               // RespObj = null;
                serviceCRM = null;
            }

        }
        #endregion

        public ActionResult SubscriberPromo()
        {
            PromotionsResponse responseObject = new PromotionsResponse();
            try
            {
                responseObject = GetPromotion();

                return View(responseObject);
            }
            catch (Exception ex)
            {
                return View(responseObject);
            }
            finally
            {
               // responseObject = null;
            }
        }

        public JsonResult jsonSubscriberPromo()
        {
            PromotionsResponse responseObject = new PromotionsResponse();
            try
            {
                responseObject = GetPromotion();
                return Json(responseObject, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(responseObject, JsonRequestBehavior.AllowGet);
            }
            finally
            {
               // responseObject = null;
            }
        }

        public PromotionsResponse GetPromotion()
        {
            PromotionsRequest requestObject = new PromotionsRequest();
            PromotionsResponse responseObject = new PromotionsResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - GetPromotion Start");
                requestObject.CountryCode = clientSetting.countryCode;
                requestObject.BrandCode = clientSetting.brandCode;
                requestObject.LanguageCode = SettingsCRM.langCode;
                requestObject.Mode = "GET";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                responseObject = serviceCRM.CRMPromotions(requestObject); ;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - GetPromotion End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                requestObject = null;
                serviceCRM = null;
            }
            return responseObject;
        }

        public PartialViewResult SubscriberMiniBarView()
        {
            return PartialView();
        }

        public JsonResult ResetSubscriberPassword(MsisdnpasswordresetReq msisdnPassResetReq)
        {
            MsisdnpasswordresetRes msisdnPassResetRes = new MsisdnpasswordresetRes();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - ResetSubscriberPassword Start");
                msisdnPassResetReq.BrandCode = clientSetting.brandCode;
                msisdnPassResetReq.CountryCode = clientSetting.countryCode;
                msisdnPassResetReq.LanguageCode = clientSetting.langCode;
                msisdnPassResetReq.Msisdn = Session["MobileNumber"].ToString();
                // msisdnPassResetReq.Username = Session["UserName"].ToString();
                msisdnPassResetReq.Username = Session["SubscriberTitle"].ToString() + " " + Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[0].ToString() + " " + Convert.ToString(Session["SubscriberName"]).Split(new char[] { '|' })[1].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                msisdnPassResetRes = serviceCRM.CRMMsisdnpasswordreset(msisdnPassResetReq);
                if (msisdnPassResetRes != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ResetSendPassword_" + msisdnPassResetRes.ResponseDetails.ResponseCode);
                    msisdnPassResetRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? msisdnPassResetRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - ResetSubscriberPassword End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                msisdnPassResetReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(msisdnPassResetRes, JsonRequestBehavior.AllowGet);
        }

        private string Dateconvert(string DateValue, string Index = "1,0,2")
        {

            string Date = string.Empty;
            string Month = string.Empty;
            string Year = string.Empty;
            string[] Indexsp = Index.Split(',');
            string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            if (DateValue != null)
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
            return DateValue;
        }


        #region FRR-2856
        public JsonResult MainBalanceDetailsPopPup()
        {
            MainBalanceDetailsRequest mainBalanceReq = new MainBalanceDetailsRequest();
            MainBalanceDetailsResponse mainBalanceDetailsResp = new MainBalanceDetailsResponse();
            List<MainBalanceInfo> listmainBalanceInfo = new List<MainBalanceInfo>();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - MainBalanceDetailsPopPup Start");
                mainBalanceReq.BrandCode = clientSetting.brandCode;
                mainBalanceReq.CountryCode = clientSetting.countryCode;
                mainBalanceReq.LanguageCode = clientSetting.langCode;
                mainBalanceReq.msisdn = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                mainBalanceDetailsResp = serviceCRM.CRMMainBalance(mainBalanceReq);
                try
                {
                    if (clientSetting.mvnoSettings.enableCurrencyConversion.ToUpper() == "TRUE" && mainBalanceDetailsResp != null && mainBalanceDetailsResp.responseDetails != null && mainBalanceDetailsResp.responseDetails.ResponseCode == "0")
                    {
                        mainBalanceDetailsResp.mainBalanceInfo.ForEach(b => b.topamountLeftOut = string.IsNullOrEmpty(b.topamountLeftOut) ? "0.00" : Convert.ToString(Math.Round(Convert.ToDecimal(b.topamountLeftOut), Convert.ToInt32(clientSetting.mvnoSettings.decimalLimit))).Replace(".", (clientSetting.mvnoSettings.currencyConversionValue)));
                    }
                    mainBalanceDetailsResp.mainBalanceInfo.ForEach(b => b.topUpdate = Utility.GetDateconvertion(b.topUpdate, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat));
                    mainBalanceDetailsResp.mainBalanceInfo.ForEach(b => b.expiryDate = Utility.GetDateconvertion(b.expiryDate, "dd-mm-yyyy", false, clientSetting.mvnoSettings.dateTimeFormat));
                }
                catch (Exception exCRMMainBalance)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exCRMMainBalance);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - MainBalanceDetailsPopPup End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }

            finally
            {
                mainBalanceReq = null;
                listmainBalanceInfo = null;
                serviceCRM = null;
            }
            return Json(mainBalanceDetailsResp, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region MultiIMSI
        public ActionResult MultiIMSI()
        {
            MULTIIMSIREGResponse mIRResponse = new MULTIIMSIREGResponse();
            PendingDetailsRequest req = new PendingDetailsRequest();
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
            ServiceInvokeCRM serviceCRM;

            try
            {
                bool isAdmin = (bool)Session["IsAdmin"];
                if (isAdmin == true && Session["PAType"] != null && Convert.ToString(Session["PAType"]) == "MULTIIMSI")
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
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MultiIMSIPendingList_" + ObjRes.ResponseDetails.ResponseCode);
                            ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                        }
                        ///FRR--3083


                    return View("DeRegisterMultiIMSIAuthorize", ObjRes);
                }
                else
                {
                    MULTIIMSIREGRequest mIRRequest = new MULTIIMSIREGRequest();
                    mIRRequest.Mode = "GETCOUNTRY";
                    mIRRequest.BrandCode = clientSetting.brandCode;
                    mIRRequest.CountryCode = clientSetting.countryCode;
                    mIRRequest.LanguageCode = clientSetting.langCode;
                    mIRRequest.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                        mIRResponse = serviceCRM.CRMMULTIIMSIRegister(mIRRequest);
                        if (mIRResponse != null && mIRResponse.Roamcountrylist.Count > 0)
                        {

                            mIRResponse.Roamcountrylist = mIRResponse.Roamcountrylist.OrderBy(m => m.CountryName).ToList();
                            //mIRResponse.Roamcountrylist.Sort();
                            // Array.Sort(mIRResponse.Roamcountrylist);
                        }


                        Session["GetCountry"] = mIRResponse.Roamcountrylist;

                        if (clientSetting.preSettings.MultiImsiFLHRetain == "1")
                        {
                            //mIRResponse.ExtendDays = Utility.DataTableToDictionary(Utility.GetDropdownMasterFromDB("crm_tbl_ExtendDays_Roaming"));
                            Session["SPAExtendDays"] = mIRResponse.ExtendDays;
                        }

                    return View("MultiIMSI", mIRResponse);
                }

            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View(mIRResponse);
            }
            finally
            {
               // mIRResponse = null;
                req = null;
                ObjRes = null;
                serviceCRM = null;
            }

        }
        public JsonResult MultiIMSIRegister(MULTIIMSIREGRequest mIRRequest)
        {
            MULTIIMSIREGResponse mIRResponse = new MULTIIMSIREGResponse();
            ServiceInvokeCRM serviceCRM;

            try
            {
                mIRRequest.Mode = "MUTIIMSIREG";
                mIRRequest.BrandCode = clientSetting.brandCode;
                mIRRequest.CountryCode = clientSetting.countryCode;
                mIRRequest.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    mIRResponse = serviceCRM.CRMMULTIIMSIRegister(mIRRequest);

                return Json(mIRResponse);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(mIRResponse);
            }
            finally
            {
               // mIRResponse = null;
                serviceCRM = null;
            }
           
        }
        public JsonResult ChangeAutoRenewalYesOrNo(MULTIIMSIREGRequest mIRRequest)
        {
            MULTIIMSIREGResponse mIRResponse = new MULTIIMSIREGResponse();
            ServiceInvokeCRM serviceCRM;

            try
            {
                mIRRequest.Mode = "AUTOMUTIIMSI";
                mIRRequest.BrandCode = clientSetting.brandCode;
                mIRRequest.CountryCode = clientSetting.countryCode;
                mIRRequest.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    mIRResponse = serviceCRM.CRMMULTIIMSIRegister(mIRRequest);


                if (mIRResponse != null)
                {

                    if (clientSetting.preSettings.MultiImsiFLHRetain == "1" && !string.IsNullOrEmpty(mIRRequest.ExtendDay))
                    {
                        if (mIRResponse.ResponseDetails.ResponseCode.Contains("|"))
                        {
                            List<string> lstRespCode = new List<string>(mIRResponse.ResponseDetails.ResponseCode.Split('|'));
                            List<string> lstRespMsg = new List<string>(mIRResponse.ResponseDetails.ResponseDesc.Split('|'));

                            //string errorInsertMsgFLHDays  = Resources.ErrorResources.ResourceManager.GetString("MultiIMSIAuto_" + lstRespCode[1]);
                            //string errorInsertMsgAutoRenewal = Resources.ErrorResources.ResourceManager.GetString("MultiIMSIFLHDays_" + lstRespCode[0]);

                            //mIRResponse.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsgFLHDays) ? lstRespMsg[0] : errorInsertMsgFLHDays;
                            //mIRResponse.ResponseDetails.ResponseDesc = mIRResponse.ResponseDetails.ResponseDesc + ',' + (string.IsNullOrEmpty(errorInsertMsgAutoRenewal) ? lstRespMsg[1] : errorInsertMsgAutoRenewal);

                            string errorInsertMsgAutoRenewal = Resources.ErrorResources.ResourceManager.GetString("MultiIMSIAuto_" + lstRespCode[1]);
                            string errorInsertMsgFLHDays = Resources.ErrorResources.ResourceManager.GetString("MultiIMSIFLHDays_" + lstRespCode[0]);

                            mIRResponse.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsgFLHDays) ? lstRespMsg[0] : errorInsertMsgFLHDays;
                            mIRResponse.ResponseDetails.ResponseDesc = mIRResponse.ResponseDetails.ResponseDesc + ',' + (string.IsNullOrEmpty(errorInsertMsgAutoRenewal) ? lstRespMsg[1] : errorInsertMsgAutoRenewal);


                        }
                    }
                    else
                    {

                    }
                }
                return Json(mIRResponse);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(mIRResponse);
            }
            finally
            {
               // mIRResponse = null;
                serviceCRM = null;
            }
            
        }
        public PartialViewResult ChangeAutoRenewal(MULTIIMSIREGRequest mIRRequest)
        {
            MULTIIMSIREGResponse mIRResponse = new MULTIIMSIREGResponse();
            try
            {
                mIRResponse.Roamcountrylist = Session["GetCountry"] as List<RoamCountry>;
                mIRResponse.ExtendDays = Session["SPAExtendDays"] as List<ExtendDays>;
                //mIRResponse.CC = mIRResponse.Roamcountrylist.Where(x => x.CountryName == mIRRequest.CountryName).SingleOrDefault().Countrycode;
                mIRResponse.CC = mIRRequest.CountryName;
                mIRResponse.country = mIRRequest.CountryName;
                mIRResponse.isAutoRenewal = mIRRequest.isAutoRenewal;
                return PartialView("ChangeAutoRenewal", mIRResponse);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return PartialView("ChangeAutoRenewal", mIRResponse);
            }
            finally
            {
                //mIRResponse = null;
            }
        }
        public JsonResult DeleteIMSIYesOrNo(MULTIIMSIREGRequest mIRRequest)
        {
            MULTIIMSIREGResponse mIRResponse = new MULTIIMSIREGResponse();
            ServiceInvokeCRM serviceCRM;

            try
            {

                mIRRequest.BrandCode = clientSetting.brandCode;
                mIRRequest.CountryCode = clientSetting.countryCode;
                mIRRequest.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    mIRResponse = serviceCRM.CRMMULTIIMSIRegister(mIRRequest);

                return Json(mIRResponse);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(mIRResponse);
            }
            finally
            {
               // mIRResponse = null;
                serviceCRM = null;
            }
            
        }
        public PartialViewResult DeleteIMSI(MULTIIMSIREGRequest mIRRequest)
        {
            MULTIIMSIREGResponse mIRResponse = new MULTIIMSIREGResponse();
            try
            {
                mIRResponse.Roamcountrylist = Session["GetCountry"] as List<RoamCountry>;
                //mIRResponse.CC = mIRResponse.Roamcountrylist.Where(x => x.CountryName == mIRRequest.CountryName).SingleOrDefault().Countrycode;
                mIRResponse.CC = mIRRequest.CountryName;
                return PartialView("DeleteIMSI", mIRResponse);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return PartialView("DeleteIMSI", mIRResponse);
            }
            finally
            {
              //  mIRResponse = null;
            }
        }
        public ActionResult GetLocalIMSIList(MULTIIMSIREGRequest mIRRequest)
        {
            MULTIIMSIREGResponse mIRResponse = new MULTIIMSIREGResponse();
            List<GETMultiIMSI> gtetmultiImsi = new List<GETMultiIMSI>();
            GETMultiIMSI item = new GETMultiIMSI();
            ServiceInvokeCRM serviceCRM;

            try
            {               
                
                //item.Country = "Sweden";
                //item.MSISDN = "444444444";
                //item.LocalIMSIRegDate = "09/09/2016";
                //item.ExpDate = "09/09/2016";
                //item.Status = "1";
                //item.AutoRenual = "1";
                mIRRequest.Mode = "GETMUTIIMSI";
                mIRRequest.BrandCode = clientSetting.brandCode;
                mIRRequest.CountryCode = clientSetting.countryCode;
                mIRRequest.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    mIRResponse = serviceCRM.CRMMULTIIMSIRegister(mIRRequest);

                    //gtetmultiImsi.Add(item);
                    //mIRResponse.gtetmultiImsi = gtetmultiImsi;

                    ///FRR--3083
                    if (mIRResponse != null && mIRResponse.ResponseDetails != null && mIRResponse.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetExtendMultiImsi_" + mIRResponse.ResponseDetails.ResponseCode);
                        mIRResponse.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? mIRResponse.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083

                return View("LocalIMSIList", mIRResponse);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View("LocalIMSIList", mIRResponse);
            }
            finally
            {
               // mIRResponse = null;
                gtetmultiImsi = null;
                item = null;
                serviceCRM = null;
            }
            
        }
        public ActionResult MultiIMSIShowRates(MULTIIMSIREGRequest mIRRequest)
        {
            MULTIIMSIREGResponse mIRResponse = new MULTIIMSIREGResponse();
            try
            {
                mIRResponse.rates = ShowRates(mIRRequest.CountryPrefix, "MultiIMSI.xls");
                return View(mIRResponse);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View(mIRResponse);
            }
            finally
            {
               // mIRResponse = null;
            }
            
        }
        protected List<Rates> ShowRates(string sCountry, string sFilename)
        {
            List<Rates> rates = new List<Rates>();
            Rates item = new Rates();
            ServiceInvokeCRM serviceCRM;

            try
            {
                string sExcelPath = Server.MapPath("~/App_Data/" + sFilename);
                string SQL = string.Empty;
                DataTable dtTariff = null;
                string sCurencycode = string.Empty;
                try
                {
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                        Bundle_plan objReq = new Bundle_plan();
                        objReq.country = sCountry;
                        objReq.CountryCode = clientSetting.countryCode;
                        objReq.LanguageCode = clientSetting.langCode;
                        objReq.BrandCode = clientSetting.brandCode;
                        Bundle_planlistRes objRes = new Bundle_planlistRes();
                        objRes = serviceCRM.LoadPlanList(objReq);
                        if (objRes.objBundle_planlist.Count() > 0)
                        {
                            sCurencycode = objRes.objBundle_planlist[0].currency;
                            item.data = objRes.objBundle_planlist[0].data;
                            item.SMS = objRes.objBundle_planlist[0].sms;
                            item.nationalTariff = objRes.objBundle_planlist[0].national;
                            item.voice = objRes.objBundle_planlist[0].voice;
                            item.currencyCode = sCurencycode;
                        }
                        else
                        {
                            item.data = 0;
                            item.SMS = 0;
                            item.nationalTariff = 0;
                            item.voice = 0;
                        }

                    rates.Add(item);
                }

                catch (Exception ex)
                {

                }
                return rates;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return rates;
            }
            finally
            {
               // rates = null;
                item = null;
                serviceCRM = null;
            }
            
        }
        public JsonResult MultiIMSIPendingList(MULTIIMSIREGRequest mIRRequest)
        {
            MULTIIMSIREGResponse mIRResponse = new MULTIIMSIREGResponse();
            PendingDetailsRequest req = new PendingDetailsRequest();
            PendingDetailsResponce ObjRes = new PendingDetailsResponce();
            ServiceInvokeCRM serviceCRM;

            try
            {
                bool isAdmin = (bool)Session["IsAdmin"];
                if (isAdmin == true && mIRRequest.Mode == "PA")
                {
                    if (Session["PAType"] != null && Convert.ToString(Session["PAType"]) == "MULTIIMSI")
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
                                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MultiIMSIPendingList_" + ObjRes.ResponseDetails.ResponseCode);
                                ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                            }
                            ///FRR--3083



                    }
                }


                Session["PAType"] = null;
                Session["PAId"] = null;
                return Json(mIRResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(mIRResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
               // mIRResponse = null;
                req = null;
                ObjRes = null;
                serviceCRM = null;
            }

            
        }
        #endregion

        #region Subscriber History

        public ActionResult SubscriberHistory()
        {
            SubscriberHistoryRequest objSubscriberHistoryRequest = new SubscriberHistoryRequest();
            SubscriberHistoryResponse objSubscriberHistoryResponse = new SubscriberHistoryResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - SubscriberHistory Start");
                objSubscriberHistoryRequest.mode = "Q";
                objSubscriberHistoryRequest.BrandCode = clientSetting.brandCode;
                objSubscriberHistoryRequest.CountryCode = clientSetting.countryCode;
                objSubscriberHistoryRequest.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objSubscriberHistoryResponse = serviceCRM.CRMSubscriberHistory(objSubscriberHistoryRequest);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - SubscriberHistory End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                objSubscriberHistoryRequest = null;
                serviceCRM = null;
            }
            return View(objSubscriberHistoryResponse);
        }


        public JsonResult GetSubscriberHistory(SubscriberHistoryRequest objSubscriberHistoryRequest)
        {
            SubscriberHistoryResponse objSubscriberHistoryResponse = new SubscriberHistoryResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - GetSubscriberHistory Start");
                objSubscriberHistoryRequest.MSISDN = Convert.ToString(Session["MobileNumber"]);
                objSubscriberHistoryRequest.mode = "R";
                objSubscriberHistoryRequest.BrandCode = clientSetting.brandCode;
                objSubscriberHistoryRequest.CountryCode = clientSetting.countryCode;
                objSubscriberHistoryRequest.LanguageCode = clientSetting.langCode;
                objSubscriberHistoryRequest.fromDate = Utility.GetDateconvertion(objSubscriberHistoryRequest.fromDate, clientSetting.mvnoSettings.dateTimeFormat, false, "YYYY-MM-DD");
                objSubscriberHistoryRequest.toDate = Utility.GetDateconvertion(objSubscriberHistoryRequest.toDate, clientSetting.mvnoSettings.dateTimeFormat, false, "YYYY-MM-DD");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objSubscriberHistoryResponse = serviceCRM.CRMSubscriberHistory(objSubscriberHistoryRequest);
                if (objSubscriberHistoryResponse != null && objSubscriberHistoryResponse.subscriberHistory.Count > 0)
                {
                    try
                    {
                        objSubscriberHistoryResponse.subscriberHistory.ForEach(b => b.submitedDate = Utility.GetDateconvertion(b.submitedDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                    }
                    catch (Exception exDateConversion)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exDateConversion);
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - GetSubscriberHistory End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                serviceCRM = null;
            }
            return new JsonResult() { Data = objSubscriberHistoryResponse, MaxJsonLength = int.MaxValue };
            //return View(objSubscriberHistoryResponse);
        }


        [HttpPost]
        public void DownLoadSubscriberHistory(string topupData)
        {
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - DownLoadSubscriberHistory Start");
                GridView gridView = new GridView();
                List<SubscriberHistory> topupFailure = JsonConvert.DeserializeObject<List<SubscriberHistory>>(topupData);
                gridView.DataSource = topupFailure;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "SubscriberHistory_" + Session["MobileNumber"].ToString(), this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - DownLoadSubscriberHistory End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
        }


        #endregion

        public List<BundlesList> DoCurrencyUnitConvertion(List<BundlesList> bundlesList)
        {
            if (bundlesList != null && bundlesList.Count > 0)
            {
                try
                {
                    //bundlesList.ForEach(b => b.AccountBalance.ONM = GetMinutesNew(b.AccountBalance.ONM));
                    //bundlesList.ForEach(b => b.AccountBalance.ONS = GetSMS(b.AccountBalance.ONS));
                    //bundlesList.ForEach(b => b.AccountBalance.FMZ1 = GetMinutesNew(b.AccountBalance.FMZ1));
                    //bundlesList.ForEach(b => b.AccountBalance.FSZ1 = GetSMS(b.AccountBalance.FSZ1));
                    //bundlesList.ForEach(b => b.AccountBalance.FMZ2 = GetMinutesNew(b.AccountBalance.FMZ2));
                    //bundlesList.ForEach(b => b.AccountBalance.FSZ2 = GetSMS(b.AccountBalance.FSZ2));
                    //bundlesList.ForEach(b => b.AccountBalance.FMZ3 = GetMinutesNew(b.AccountBalance.FMZ3));
                    //bundlesList.ForEach(b => b.AccountBalance.FSZ3 = GetSMS(b.AccountBalance.FSZ3));
                    //bundlesList.ForEach(b => b.AccountBalance.ONMTM = GetMinutesNew(b.AccountBalance.ONMTM));
                    //bundlesList.ForEach(b => b.AccountBalance.ONMTS = GetSMS(b.AccountBalance.ONMTS));
                    //bundlesList.ForEach(b => b.AccountBalance.OFMTM = GetMinutesNew(b.AccountBalance.OFMTM));
                    //bundlesList.ForEach(b => b.AccountBalance.OFMTS = GetSMS(b.AccountBalance.OFMTS));
                    if (clientSetting.mvnoSettings.enableCurrencyConversion.ToUpper() == "TRUE")
                    {
                        // 4772 DEFECT

                        if (clientSetting.preSettings.EnableAltanIntegration.ToUpper() != "TRUE")
                        {
                        //bundlesList.ForEach(b => b.AccountBalance.PA = string.IsNullOrEmpty(b.AccountBalance.PA) ? "0" : Convert.ToString(Math.Round(Convert.ToDecimal(b.AccountBalance.PA) / Convert.ToDecimal(clientSetting.mvnoSettings.commonunitsConversionValue), 0)).Replace(".", (clientSetting.mvnoSettings.currencyConversionValue)));
                        bundlesList.ForEach(b => b.AccountBalance.PA = string.IsNullOrEmpty(b.AccountBalance.PA) ? "0" : Convert.ToString(Math.Floor(Convert.ToDecimal(b.AccountBalance.PA) / Convert.ToDecimal(clientSetting.mvnoSettings.commonunitsConversionValue))).Replace(".", (clientSetting.mvnoSettings.currencyConversionValue)));

                        //if (bundlesList != null && bundlesList.Count > 0)
                        //{
                        //if (bundlesList[0].AccountBalance != null && !string.IsNullOrEmpty(bundlesList[0].AccountBalance.FREEDATA))
                        bundlesList.ForEach(b => b.AccountBalance.FREEDATA = string.IsNullOrEmpty(b.AccountBalance.FREEDATA) ? "0" : ConvertMBOrGB(b.AccountBalance.FREEDATA).Replace(".", clientSetting.mvnoSettings.currencyConversionValue));
                        //}
                        }

                    }
                }
                catch
                {
                }
            }
            return bundlesList;
        }

        public string ConvertMBOrGB(string strData)
        {
            if (strData == "U")
            {
                
            }
            else if (strData.Contains("KB"))
            {
                Double val = Convert.ToDouble(strData.Replace("KB", ""));
                Double freemindata = Math.Round((val / 1024.0), 2);
                strData = freemindata.ToString();
            }
            else if (strData.Contains("GB"))
            {
                Double val = Convert.ToDouble(strData.Replace("GB", ""));
                Double freemindata = Math.Round((val * 1024.0), 2);
                strData = freemindata.ToString();
            }
            else if (strData.Contains("MB"))
            {
                strData = Convert.ToString(Math.Round(Convert.ToDouble(strData.Replace("MB", "")), 2));
                return strData = strData + " MB";
            }
            else
            {
                Double val = Convert.ToDouble(strData);
                Double freemindata = Math.Round(((val / 1024) / 1024), 2);
                strData = freemindata.ToString();
            }
            return strData = strData + " MB";
        }

        #region PID 43358

        public JsonResult CRMGetIMGSubscriberDataUsage(SubscriberDataUsageReq objSubscriberDataUsageReq)
        {
            SubscriberDataUsageRes objSubscriberDataUsageRes = new SubscriberDataUsageRes();
           // SubscriberDataUsageReq objSubscriberDataUsageReq;
            ServiceInvokeCRM serviceCRM;
            //6466
            double totaltethering = 0;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMGetIMGSubscriberDataUsage Start");
                //objSubscriberDataUsageReq = new SubscriberDataUsageReq();
                objSubscriberDataUsageReq.CountryCode = clientSetting.countryCode;
                objSubscriberDataUsageReq.BrandCode = clientSetting.brandCode;
                objSubscriberDataUsageReq.LanguageCode = clientSetting.langCode;
                objSubscriberDataUsageReq.MSISDN = Session["MobileNumber"].ToString();
                objSubscriberDataUsageReq.ICCID = Session["ICCID"].ToString();
                objSubscriberDataUsageReq.IMSI = Session["IMSI"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objSubscriberDataUsageRes = serviceCRM.CRMGetIMGSubscriberDataUsage(objSubscriberDataUsageReq);

                //6466
                if (objSubscriberDataUsageRes.SubsciberDataListTethering.Count > 0)
                {
                    if (!string.IsNullOrEmpty(objSubscriberDataUsageRes.SubsciberDataListTethering[0].International_Roaming))
                    {
                        objSubscriberDataUsageRes.SubsciberDataListTethering[0].International_Roaming = ConvertMBOrGB(objSubscriberDataUsageRes.SubsciberDataListTethering[0].International_Roaming);
                        totaltethering = totaltethering + Convert.ToDouble(objSubscriberDataUsageRes.SubsciberDataListTethering[0].International_Roaming.Replace("MB", ""));
                    }
                    if (!string.IsNullOrEmpty(objSubscriberDataUsageRes.SubsciberDataListTethering[0].General_Home))
                    {
                        objSubscriberDataUsageRes.SubsciberDataListTethering[0].General_Home = ConvertMBOrGB(objSubscriberDataUsageRes.SubsciberDataListTethering[0].General_Home);
                        totaltethering = totaltethering + Convert.ToDouble(objSubscriberDataUsageRes.SubsciberDataListTethering[0].General_Home.Replace("MB", ""));
                    }
                    if (!string.IsNullOrEmpty(objSubscriberDataUsageRes.SubsciberDataListTethering[0].Tethering_Home))
                    {
                        objSubscriberDataUsageRes.SubsciberDataListTethering[0].Tethering_Home = ConvertMBOrGB(objSubscriberDataUsageRes.SubsciberDataListTethering[0].Tethering_Home);
                    }

                    objSubscriberDataUsageRes.SubsciberDataListTethering[0].Tethering_Domestic = totaltethering.ToString() + " "+ "MB";

                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMGetIMGSubscriberDataUsage End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                objSubscriberDataUsageReq = null;
                serviceCRM = null;
            }
            return Json(objSubscriberDataUsageRes);
        }

        #endregion


        #region FRR3347 ComponentDetails
        public JsonResult CRMComponentDetails(ComponentBundleDetReq objComponentBundleDetReq)
        {
            ComponentBundleDetRes crmcomponentBundleres = new ComponentBundleDetRes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMComponentDetails Start");
                objComponentBundleDetReq.CountryCode = clientSetting.countryCode;
                objComponentBundleDetReq.BrandCode = clientSetting.brandCode;
                objComponentBundleDetReq.LanguageCode = clientSetting.langCode;
                objComponentBundleDetReq.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                crmcomponentBundleres = serviceCRM.CRMComponentDetails(objComponentBundleDetReq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMComponentDetails End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                objComponentBundleDetReq = null;
                serviceCRM = null;
            }
            return Json(crmcomponentBundleres);
        }
        #endregion

        #region FRR 3406

        public JsonResult CRMGetIMGSubscriberDetails()
        {
            GetSubscriberInformationSOCRes objSubscriberRes = new GetSubscriberInformationSOCRes();
            GetSubscriberInformationSOCReq objSubscriberReq = new GetSubscriberInformationSOCReq();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMGetIMGSubscriberDetails Start");
                objSubscriberReq.CountryCode = clientSetting.countryCode;
                objSubscriberReq.BrandCode = clientSetting.brandCode;
                objSubscriberReq.LanguageCode = clientSetting.langCode;
                objSubscriberReq.MSISDN = Session["MobileNumber"].ToString();
                objSubscriberReq.ICCID = Session["ICCID"].ToString();
                objSubscriberReq.IMSI = Session["IMSI"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objSubscriberRes = serviceCRM.CRMGetIMGSubscriberDetails(objSubscriberReq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMGetIMGSubscriberDetails End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                objSubscriberReq = null;
                serviceCRM = null;
            }
            return Json(objSubscriberRes);
        }

        #endregion


        public string ReserveDateconvertion(string input, string output)
        {
            DateTime dt = DateTime.Parse(input);
            string strDateTimeFormat = output;
            strDateTimeFormat = strDateTimeFormat.Replace("DD", "dd");
            strDateTimeFormat = strDateTimeFormat.Replace("YYYY", "yyyy");
            string str = dt.ToString(strDateTimeFormat);
            return str;
        }


        public JsonResult getNotes(string mode, string notes)
        {
            NotesResponse getNotesResp = new NotesResponse();
            NotesRequest getNotesRequest = new NotesRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - getNotes Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                getNotesRequest.BrandCode = clientSetting.brandCode;
                getNotesRequest.CountryCode = clientSetting.countryCode;
                getNotesRequest.LanguageCode = clientSetting.langCode;
                getNotesRequest.AccountNo = Session["AccountNumber"].ToString();
                getNotesRequest.Mode = mode;
                getNotesRequest.Notes = notes;
                getNotesRequest.Username = Session["UserName"].ToString();
                getNotesResp = serviceCRM.CRMNotesUpdate(getNotesRequest);
                // subscriberPlanDetails.cardDetails = managedCardResp.cardDetails;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - getNotes End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                getNotesRequest = null;
                serviceCRM = null;
            }
            return Json(getNotesResp, JsonRequestBehavior.AllowGet);
        }

        //FRR 3934
        public JsonResult CRMOutOfBundleUsage(OBABundleRequest OutofBundleRequest)
        {
            CRMResponse crmResponse = new CRMResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMOutOfBundleUsage Start");
                OutofBundleRequest.BrandCode = clientSetting.brandCode;
                OutofBundleRequest.CountryCode = clientSetting.countryCode;
                OutofBundleRequest.LanguageCode = clientSetting.langCode;
                OutofBundleRequest.MSISDN = Session["MobileNumber"].ToString();
                OutofBundleRequest.userName = Session["UserName"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                crmResponse = serviceCRM.CRMOutOfBundleUsage(OutofBundleRequest);
                ///FRR--3083                   
                if (crmResponse != null && crmResponse.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMOutOfBundleUsage_" + crmResponse.ResponseCode);
                    crmResponse.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? crmResponse.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMOutOfBundleUsage End");
            }
            catch (Exception ex)
            {
                crmResponse.ResponseCode = "NA";
                crmResponse.ResponseDesc = ex.Message.ToString();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(crmResponse, JsonRequestBehavior.AllowGet);
        }
        //FRR 3934



        //FRR 4092
        public JsonResult CRMConsentDetails(ConsentDetailsRequest ObjRequest)
        {
            ConsentDetailsResponse ObjResp = new ConsentDetailsResponse();
            //ConsentDetailsRequest ObjRequest = new ConsentDetailsRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMConsentDetails Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRequest.BrandCode = clientSetting.brandCode;
                ObjRequest.CountryCode = clientSetting.countryCode;
                ObjRequest.LanguageCode = clientSetting.langCode;
                ObjRequest.MSISDN = Session["MobileNumber"].ToString();
                ObjRequest.IMSI = Session["IMSI"].ToString();
                ObjRequest.ICCID = Session["ICCID"].ToString();
                ObjRequest.Mode = ObjRequest.Mode;
                ObjRequest.UpdatedBy = Session["UserName"].ToString();
                ObjResp = serviceCRM.CRMConsentDetails(ObjRequest);
                ///FRR--3083                   
                if (ObjResp != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMConsentDetails_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                // subscriberPlanDetails.cardDetails = managedCardResp.cardDetails;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMConsentDetails End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                ObjRequest = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(ObjResp, JsonRequestBehavior.AllowGet);
        }
        //FRR 4092
        public JsonResult CRMMultipleBundleModifyRenewal(ModifyRenewalRequest modifyRenewalReq)
        {
            ModifyRenewalResponse cmrResponse = new ModifyRenewalResponse();
            ServiceInvokeCRM serviceCRM;

            try
            {
                modifyRenewalReq.CountryCode = clientSetting.countryCode;
                modifyRenewalReq.BrandCode = clientSetting.brandCode;
                modifyRenewalReq.LanguageCode = clientSetting.langCode;
                if (modifyRenewalReq.cardPosition.Count > 0)
                {
                    modifyRenewalReq.cardPosition = new List<string>(modifyRenewalReq.cardPosition[0].Split('|'));
                }
                if (modifyRenewalReq.FamilyAccID != "" && modifyRenewalReq.IsFamilyBundle == "1")
                {

                }
                else
                {
                    modifyRenewalReq.MSISDN = Session["MobileNumber"].ToString();
                }
                modifyRenewalReq.Email = Session["eMailID"].ToString();
                modifyRenewalReq.UpdatedBy = Session["UserName"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    cmrResponse = serviceCRM.CRMMultipleBundleModifyRenewal(modifyRenewalReq);



                if (cmrResponse != null && cmrResponse.Response.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMConsentCancel_" + cmrResponse.Response.ResponseCode);
                    cmrResponse.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? cmrResponse.Response.ResponseDesc : errorInsertMsg;
                }



                return Json(cmrResponse);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(cmrResponse);
            }
            finally
            {
              //  cmrResponse = null;
                modifyRenewalReq = null;
                serviceCRM = null;
            }
        }

        //4181
        #region FRR - 4181
        public JsonResult CRMManagepriority(PriorityReq ManageBundlePriorityReq)
        {
            PriorityRes crmResponse = new PriorityRes();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMManagepriority Start");
                ManageBundlePriorityReq.CountryCode = clientSetting.countryCode;
                ManageBundlePriorityReq.BrandCode = clientSetting.brandCode;
                ManageBundlePriorityReq.LanguageCode = clientSetting.langCode;
                ManageBundlePriorityReq.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                crmResponse = serviceCRM.CRMManagepriority(ManageBundlePriorityReq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMManagepriority End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                ManageBundlePriorityReq = null;
                serviceCRM = null;
            }
            return Json(crmResponse);
        }
        # endregion
        //4181        

        #region SubscriberStatusReport

        public ActionResult SubscriberStatusReport()
        {
            SubscriberStatusReportResponse objres = new SubscriberStatusReportResponse();
            SubscriberStatusReportRequest objreq = new SubscriberStatusReportRequest();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - SubscriberStatusReport Start");
                return View("SubscriberStatusReport", objres);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return View("SubscriberStatusReport", objres);
            }
            finally
            {
                // objres = null;
                objreq = null;
            }

        }


        [HttpPost]
        public JsonResult SubscriberStatusReportSearch(SubscriberStatusReportRequest objreq)
        {
            SubscriberStatusReportResponse objres = new SubscriberStatusReportResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - SubscriberStatusReportSearch Start");
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                if (objreq.FromDate != null && objreq.ToDate != null)
                {
                    objreq.FromDate = Utility.FormatDateTimeToService(objreq.FromDate, clientSetting.mvnoSettings.dateTimeFormat);
                    objreq.ToDate = Utility.FormatDateTimeToService(objreq.ToDate, clientSetting.mvnoSettings.dateTimeFormat);
                }

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.CRMSubscriberStatusReport(objreq);

                try
                {
                    objres.SubscriberStatusReport.ForEach(b => b.changedate = Utility.GetDateconvertion(b.changedate, "DD-MM-YYYY", false, clientSetting.mvnoSettings.dateTimeFormat));

                }
                catch (Exception ex)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);

                }
                if (objres != null && objres.ResponseDetails != null && objres.ResponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMSubscriberStatusReport_" + objres.ResponseDetails.ResponseCode);
                    objres.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - SubscriberStatusReportSearch End");
                return Json(objres, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objres, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //objres = null;
                objreq = null;
                serviceCRM = null;

            }
            // return Json(objRes);


        }

        #endregion
        #region ViewSubscriberOTP
        [HttpGet]
        public ActionResult ViewSubscriberOTP()
        {
            ViewSubscriberOTPResponse objResp = new ViewSubscriberOTPResponse();
            ViewSubscriberOTPRequest objReq = new ViewSubscriberOTPRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - ViewSubscriberOTP Start");
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objResp = serviceCRM.CRMViewSubscriberOTP(objReq);
                try
                {
                    foreach (ViewSubscriberOTP vsopt in objResp.ViewSubscriberOTP)
                    {
                        vsopt.generateddate = Utility.FormatDateTime(vsopt.generateddate, clientSetting.mvnoSettings.dateTimeFormat);
                        vsopt.expirydate = Utility.FormatDateTime(vsopt.expirydate, clientSetting.mvnoSettings.dateTimeFormat);
                    }
                }
                catch (Exception ex)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - ViewSubscriberOTP End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                objReq = null;
                serviceCRM = null;
            }
            return View(objResp);
        }
        #endregion

        #region FRR 4688 

        [HttpPost]
        public JsonResult RegisterTracking(string RegisterTrackingrequest)
        {
            RegtrackingResponse objres = new RegtrackingResponse();
            RegtrackingRequest objreq = JsonConvert.DeserializeObject<RegtrackingRequest>(RegisterTrackingrequest);
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - RegisterTracking Start");
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objres = serviceCRM.CRMRegisterTracking(objreq);

                if (objres != null && objres.responseDetails != null && objres.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RegisterTracking_" + objres.responseDetails.ResponseCode);
                    objres.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.responseDetails.ResponseDesc : errorInsertMsg;
                }

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
        #endregion
 #region DataThrottle FRR 4700

        public JsonResult GetDatathrottle(BundleDetailRequest viewobjreq)
        {
            BundleDetailRequest objreq = new BundleDetailRequest();
            PayGConsentBundleAccBalanceRes objres = new PayGConsentBundleAccBalanceRes();
            PayGConsentBundleAccBalanceRes viewobjres = new PayGConsentBundleAccBalanceRes();
            ClientSetting clientSetting = new ClientSetting();
            ServiceInvokeCRM serviceCRM;

            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.MSISDN = Session["MobileNumber"].ToString();
                objreq.ChildMsisdn = viewobjreq.ChildMsisdn;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                    objres = serviceCRM.CRMGetDataThrottle(objreq);

                    if (objres.RETURNCODE == "0")
                    {
                        viewobjres = objres;
                    }
                    else
                    {
                        viewobjres.ERRDESCRITION = objres.ERRDESCRITION;
                    }

                return Json(viewobjres);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objres, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                clientSetting = null;
                serviceCRM = null;
            }
        }

        #endregion


        #region FRR-5027
        public JsonResult CRMCommonunits(CommonunitsReq ObjRequest)
        {
            CommonunitsResp ObjResp = new CommonunitsResp();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMConsentDetails Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                ObjRequest.BrandCode = clientSetting.brandCode;
                ObjRequest.CountryCode = clientSetting.countryCode;
                ObjRequest.LanguageCode = clientSetting.langCode;
                ObjRequest.Msisdn = Session["MobileNumber"].ToString();

                ObjResp = serviceCRM.CRMCommonunit(ObjRequest);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SubscriberController - CRMConsentDetails End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                ObjRequest = null;
                serviceCRM = null;
            }
            return Json(ObjResp, JsonRequestBehavior.AllowGet);
        }

        #endregion

    }
}

