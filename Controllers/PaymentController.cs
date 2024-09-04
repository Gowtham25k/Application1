using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.Optimization;
using CRM.Models;
using Newtonsoft.Json;
using ServiceCRM;

namespace CRM.Controllers
{
    [ValidateState]
    public class PaymentController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();

        public ViewResult CardPayment()
        {
            ManageCardCRM manageCardCRM = new ManageCardCRM();
            ManagedCardResponse manageCardResp = new ManagedCardResponse();
            ManagedCardRequest manageCardReq = new ManagedCardRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "PaymentController - ActionResult CardPayment Start");
                manageCardCRM.crmResponse = new CRMResponse();
                manageCardCRM.crmResponse.ResponseCode = "0";
                manageCardCRM.paymentCRM = new PaymentCRM();

                if (clientSetting.preSettings.SIMRegwithFamilyInPaygFamily.ToUpper() == "TRUE")
                {
                    manageCardCRM.paymentCRM.lstCardTypes = Utility.GetDropdownMasterFromDB("12", "1", "drop_master");

                }
                else
                {
                manageCardCRM.paymentCRM.lstCardTypes = Utility.GetDropdownMasterFromDB("12", Convert.ToString(Session["isPrePaid"]), "drop_master");
                }

                manageCardReq.CountryCode = clientSetting.countryCode;
                manageCardReq.BrandCode = clientSetting.brandCode;
                manageCardReq.LanguageCode = clientSetting.langCode;
                #region FRR 4925
                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                    if (Session["SessionsampleDict"] != null)
                    {
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    manageCardReq.Msisdn = localDict.Where(x => Session["RealICCIDForMultiTab"].ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                    }
                    else
                    {
                        manageCardReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                    }
                }
                else
                {
                    manageCardReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                }
                #endregion
                manageCardReq.mode = "Q";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                if (!string.IsNullOrEmpty(manageCardReq.Msisdn))
                {
                    manageCardResp = serviceCRM.CRMManageCardDetails(manageCardReq);
                }

                if (manageCardResp != null && manageCardResp.responseDetails != null && manageCardResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CardPayment_" + manageCardResp.responseDetails.ResponseCode);
                    manageCardResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? manageCardResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                if (manageCardResp.cardDetails != null && manageCardResp.cardDetails.Count > 0)
                {
                    manageCardCRM.creditCardList = manageCardResp.cardDetails;
                    //manageCardCRM.creditCardList.ForEach(m => m.cardexpirydate = m.cardexpirydate.Replace('.'.ToString(), string.Empty));
                    //manageCardCRM.creditCardList.ForEach(m => m.cardexpirydate = DateTime.ParseExact(m.cardexpirydate, "dd/MM/yyyy hh:mm:ss tt",System.Globalization.CultureInfo.InvariantCulture).ToString("MM '/' yyyy"));
                    manageCardCRM.creditCardList.ForEach(m => m.cardexpirydate = Convert.ToDateTime(m.cardexpirydate).ToString("MM '/' yyyy"));

                }
                else
                {
                    manageCardCRM.creditCardList = new List<ManageCardDetails>();
                }


                // FRR 4617-allinone


                if (manageCardResp.debitcardDetails != null && manageCardResp.debitcardDetails.Count > 0)
                {
                    manageCardCRM.directCardList = manageCardResp.debitcardDetails;
                    manageCardCRM.directCardList.ForEach(m => m.cardexpirydate = Convert.ToDateTime(m.cardexpirydate).ToString("MM '/' yyyy"));
                }
                else
                {
                    manageCardCRM.directCardList = new List<ManageDebitCardDetails>();
                }



                // 4617-allinone-Dic

                if (manageCardCRM.directCardList != null)
                {
                    for (int i = 0; i < manageCardCRM.directCardList.Count; i++)
                    {


                        manageCardCRM.lstDirectdebitCardDetails.Add(manageCardResp.debitcardDetails[i].cardId + "," + manageCardResp.debitcardDetails[i].Country + "," +
                            manageCardResp.debitcardDetails[i].addressline1 + "," + manageCardResp.debitcardDetails[i].addressline2 + "," + manageCardResp.debitcardDetails[i].cardNumber + "," +
                            manageCardResp.debitcardDetails[i].email + "," + manageCardResp.debitcardDetails[i].nameOnCard + "," + manageCardResp.debitcardDetails[i].postcode + "," +
                              manageCardResp.debitcardDetails[i].mobile + "," + manageCardResp.debitcardDetails[i].city, manageCardResp.debitcardDetails[i].cardNumber);


                    }
                }

                // 4617-allinone-Dic



                manageCardCRM.crmResponse = manageCardCRM.crmResponse;
                if (!string.IsNullOrEmpty(manageCardResp.ConsentDate))
                {
                    manageCardCRM.ConsentDate = Utility.GetDateconvertion(manageCardResp.ConsentDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "PaymentController - ActionResult CardPayment End");
            }
            catch (Exception eX)
            {
                manageCardCRM.crmResponse.ResponseCode = "9";
                manageCardCRM.crmResponse.ResponseDesc = eX.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "PaymentController - CardPayment - eX - " + this.ControllerContext, eX);
            }
            finally
            {
                serviceCRM = null;
                manageCardResp = null;
                manageCardReq = null;
                errorInsertMsg = string.Empty;
            }
            return View(manageCardCRM);
        }


        // 4788

      public  JsonResult PostpaidCardManageCard()
        {
            ManageCardCRM manageCardCRM = new ManageCardCRM();
            ManagedCardRequest manageCardReq = new ManagedCardRequest();
            try
            {
                manageCardCRM.crmResponse = new CRMResponse();
                manageCardCRM.crmResponse.ResponseCode = "0";
                manageCardCRM.paymentCRM = new PaymentCRM();
                manageCardCRM.paymentCRM.lstCardTypes = Utility.GetDropdownMasterFromDB("12", Convert.ToString(Session["isPrePaid"]), "drop_master");


                manageCardReq.CountryCode = clientSetting.countryCode;
                manageCardReq.BrandCode = clientSetting.brandCode;
                manageCardReq.LanguageCode = clientSetting.langCode;
                manageCardReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                manageCardReq.mode = "Q";

                manageCardCRM.creditCardList = ManageCardCRMResponse(manageCardReq).cardDetails;
                //POF-6152
                manageCardCRM.creditCardList.ForEach(m => m.cardexpirydate = Convert.ToDateTime(m.cardexpirydate).ToString("MM '/' yyyy"));
                manageCardCRM.directCardList = ManageCardCRMResponse(manageCardReq).debitcardDetails;              
            }
            catch (Exception eX)
            {
                manageCardCRM.crmResponse.ResponseCode = "9";
                manageCardCRM.crmResponse.ResponseDesc = eX.Message;
            }
            finally
            {
                //manageCardCRM = null;
                manageCardReq = null;
            }
            return new JsonResult() { Data = manageCardCRM, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }



        public ViewResult ManageCard()
        {
            // 4781
            ViewBag.IsRedirect = TempData["DirectDebitTrustlypay"];

            ManageCardCRM manageCardCRM = new ManageCardCRM();
            ManagedCardRequest manageCardReq = new ManagedCardRequest();
            Dropdown ObjDrop = new Dropdown();
            ManageDebitCardDetails objdebitcard = null;
            List<ManageDebitCardDetails> objlistdebitcard = new List<ManageDebitCardDetails>();
            try
            {
                manageCardCRM.crmResponse = new CRMResponse();
                manageCardCRM.crmResponse.ResponseCode = "0";
                manageCardCRM.paymentCRM = new PaymentCRM();
                manageCardCRM.paymentCRM.lstCardTypes = Utility.GetDropdownMasterFromDB("12", Convert.ToString(Session["isPrePaid"]), "drop_master");
                DataSet ds = Utility.BindXmlFile("~/App_Data/CountryListSepaCheckout.xml");
                manageCardCRM.objcountrydd = new Models.CountryDropdownManagecard();
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
                manageCardCRM.objcountrydd.CountryDDManagecard = objLstDrop;

                manageCardReq.CountryCode = clientSetting.countryCode;
                manageCardReq.BrandCode = clientSetting.brandCode;
                manageCardReq.LanguageCode = clientSetting.langCode;
                manageCardReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                manageCardReq.mode = "Q";
                manageCardCRM.creditCardList = ManageCardCRMResponse(manageCardReq).cardDetails;
                manageCardCRM.directCardList = ManageCardCRMResponse(manageCardReq).debitcardDetails;

                //6152

                if (manageCardCRM.creditCardList != null && manageCardCRM.creditCardList.Count > 0) { 
                manageCardCRM.creditCardList.ForEach(m => m.cardexpirydate = Convert.ToDateTime(m.cardexpirydate).ToString("MM '/' yyyy"));
                }

                if (clientSetting.preSettings.EnableCheckoutAutorenewal.ToUpper() == "TRUE")
                {
                    if (manageCardCRM != null && manageCardCRM.creditCardList != null)
                    {

                        foreach(ManageCardDetails m in manageCardCRM.creditCardList)
                        {
                            objdebitcard = new ManageDebitCardDetails();
                            objdebitcard.cardId = m.cardId;
                            objdebitcard.cardNumber = m.cardNumber;
                            objdebitcard.Type = m.Type;
                            objdebitcard.nameOnCard = m.nameOnCard;
                            objdebitcard.Country = m.Country;
                            objdebitcard.bundleCode = m.bundleCode;
                            objdebitcard.bundleName = m.bundleName;
                            objdebitcard.renevalStatus = m.renevalStatus;
                            objdebitcard.cardtypeName = m.cardtypeName;
                            objdebitcard.cardstartdate = m.cardstartdate;
                            objdebitcard.cardexpirydate = m.cardexpirydate;
                            objdebitcard.cardissuenumber = m.cardissuenumber;
                            objdebitcard.housenumber = m.housenumber;
                            objdebitcard.addressline1 = m.addressline1;
                            objdebitcard.addressline2 = m.addressline2;
                            objdebitcard.city = m.city;
                            objdebitcard.county = m.county;
                            objdebitcard.CountryCode = m.CountryCode;
                            objdebitcard.postcode = m.postcode;
                            objdebitcard.email = m.email;
                            objdebitcard.mobile = m.mobile;
                            objdebitcard.contactnumber = m.contactnumber;
                            objdebitcard.cardPosition = m.cardPosition;
                            objdebitcard.TopupAmount = m.TopupAmount;
                            objdebitcard.ConsentStartDate = m.ConsentStartDate;
                            objdebitcard.ConsentExpiredDate = m.ConsentExpiredDate;
                            objdebitcard.ConsentStatus = m.ConsentStatus;
                            objdebitcard.ConsentStatusDescription = m.ConsentStatusDescription;
                            objlistdebitcard.Add(objdebitcard);
                        }
                        manageCardCRM.directCardList.AddRange(objlistdebitcard);

                    }
                }
                return View(manageCardCRM);
            }
            catch (Exception eX)
            {
                manageCardCRM.crmResponse.ResponseCode = "9";
                manageCardCRM.crmResponse.ResponseDesc = eX.Message;
                return View(manageCardCRM);
            }
            finally
            {
                //manageCardCRM = null;
                manageCardReq = null;
            }
        }

        public JsonResult ManageCardDetails(PaymentCRM paymentCRM, ManageCardDetails creditCard, string oper, string id, string cardID, string ccNumber, string bundleCodes, string cardPosition)
        {
            ManagedCardResponse manageCardResp = new ManagedCardResponse();
            ManagedCardRequest manageCardReq = new ManagedCardRequest();
            ManageDebitCardDetails managedebitcardreq = new ManageDebitCardDetails();
            try
            {

                manageCardReq.CountryCode = clientSetting.countryCode;
                manageCardReq.BrandCode = clientSetting.brandCode;
                manageCardReq.LanguageCode = clientSetting.langCode;

                if (paymentCRM != null && string.IsNullOrEmpty(oper) && paymentCRM.repeatCardNumber1 != "CardSwap" && paymentCRM.repeatCardNumber1 != "del" && paymentCRM.repeatCardNumber1 != "ADDCARDLINK")
                {
                    
                    manageCardReq.mode = "A";

                    manageCardReq.cardType = paymentCRM.cardType;
                    manageCardReq.nameOnCard = paymentCRM.nameOnCard;
                    manageCardReq.cardNumber = paymentCRM.cardNumber1 + paymentCRM.cardNumber2 + paymentCRM.cardNumber3 + paymentCRM.cardNumber4;
                    manageCardReq.expiryDate = paymentCRM.expiryDate.Replace(" / ", string.Empty);
                    manageCardReq.CVV = paymentCRM.cvvNumber;
                    manageCardReq.postCode = paymentCRM.postCode;
                    manageCardReq.emailID = paymentCRM.eMailID;
                    //4757
                    manageCardReq.PayerState = paymentCRM.PayerState;
                    manageCardReq.PayerpostCode = paymentCRM.PayerpostCode;
                    manageCardReq.PayerDOB = paymentCRM.PayerDOB;
                    manageCardReq.Docnumber = paymentCRM.Docnumber;
                    manageCardReq.Doctype = paymentCRM.Doctype;
                    manageCardReq.UserName = paymentCRM.FirstName;
                    manageCardReq.Lastname = paymentCRM.Lastname;
                    manageCardReq.city = paymentCRM.City;
                    manageCardReq.addressline1 = paymentCRM.StreetName;
                    //6169
                    manageCardReq.bundleId = paymentCRM.BundleID;
                    if (!string.IsNullOrEmpty(paymentCRM.ConsentDate))
                    {
                        manageCardReq.ConsentDate = Utility.GetDateconvertion(paymentCRM.ConsentDate, "MM/DD/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    }
                 
                }
                else if (oper == "edit")
                {
                    manageCardReq.mode = "E";

                    manageCardReq.cardId = cardID;
                    manageCardReq.cardNumber = ccNumber;
                    manageCardReq.cardPosition = new List<string>(cardPosition.Split('|'));

                    List<string> lstBundleCode = new List<string>();
                    if (bundleCodes.Contains(","))
                    {
                        string bundles = string.Empty;
                        lstBundleCode = bundleCodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        foreach (string bundle in lstBundleCode)
                        {
                            bundles += bundle.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim() + ",";
                        }
                        manageCardReq.bundleId = bundles.TrimEnd(new char[] { ',' });
                    }
                    else if (bundleCodes.Contains(" - "))
                    {
                        manageCardReq.bundleId = bundleCodes.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                    }
                    else
                    {
                        manageCardReq.bundleId = bundleCodes.Trim();
                    }
                }
                else if (paymentCRM.repeatCardNumber1 == "CardSwap")
                {
                    oper = "CardSwap";
                    manageCardReq.mode = "CardSwap";
                    manageCardReq.cardId = paymentCRM.cardNumber4;
                    manageCardReq.cardNumber = paymentCRM.cardNumber2;
                    manageCardReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                    manageCardReq.cardType = paymentCRM.cardType;
                    manageCardReq.nameOnCard = paymentCRM.nameOnCard;        
                    manageCardReq.expiryDate = paymentCRM.expiryDate;
                    manageCardReq.CVV = paymentCRM.cvvNumber;
                    manageCardReq.postCode = paymentCRM.postCode;
                    manageCardReq.emailID = paymentCRM.eMailID;
                    manageCardReq.bundleId = paymentCRM.repeatCardNumber2;
                    manageCardReq.strSourceCardId = paymentCRM.strSourceCardId;

                }
                else if (oper == "del")
                {
                    manageCardReq.mode = "D";

                    manageCardReq.cardId = creditCard.cardId;
                    manageCardReq.cardNumber = creditCard.cardNumber;

                    //new
                    if (!string.IsNullOrEmpty(bundleCodes))
                    {
                        manageCardReq.bundleId = bundleCodes;
                        manageCardReq.UserName = Convert.ToString(Session["UserName"]);
                    }
                    
                   
                }
                else if (paymentCRM.repeatCardNumber1 == "del")
                {
                    manageCardReq.mode = "D";

                    manageCardReq.cardId = creditCard.cardId;
                    manageCardReq.cardNumber = creditCard.cardNumber;
                }
                //6587
                else if(paymentCRM.repeatCardNumber1 == "ADDCARDLINK")
                {
                    manageCardReq.mode = "GENERATELINK_ADDCARD";
                    manageCardReq.emailID = paymentCRM.eMailID;
                    if (!string.IsNullOrEmpty(paymentCRM.BundleID))
                        manageCardReq.bundleId = paymentCRM.BundleID;
                }
                manageCardReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                manageCardReq.userName = Session["UserName"].ToString();
                manageCardResp = ManageCardCRMResponse(manageCardReq);

                return Json(manageCardResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(manageCardResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
               // manageCardResp = null;
                manageCardReq = null;
            }

        }

        private ManagedCardResponse ManageCardCRMResponse(ManagedCardRequest manageCardReq)
        {
            ManagedCardResponse manageCardResp = new ManagedCardResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    manageCardReq.Msisdn = Session["MobileNumber"].ToString();
                    manageCardResp = serviceCRM.CRMManageCardDetails(manageCardReq);

                    if (manageCardResp != null && manageCardResp.responseDetails != null && manageCardResp.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CardPayment_" + manageCardResp.responseDetails.ResponseCode);
                        manageCardResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? manageCardResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }



                    if (!string.IsNullOrEmpty(manageCardResp.ConsentDate))
                    {
                        ViewBag.ConsentDate = Utility.GetDateconvertion(manageCardResp.ConsentDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                    }

                
                #region credit card
                if (manageCardResp.responseDetails.ResponseCode == "0" && manageCardResp.cardDetails != null)
                {
                    manageCardResp.cardDetails = manageCardResp.cardDetails.OrderByDescending(m => m.bundleCode).ToList();
                    try
                    {
                        foreach (ManageCardDetails manageCardDet in manageCardResp.cardDetails)
                        {
                            if (!string.IsNullOrEmpty(manageCardDet.renewalDate))
                            {
                                List<string> lstRenewDate = new List<string>(manageCardDet.renewalDate.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

                                manageCardDet.renewalDate = string.Empty;

                                foreach (string reDate in lstRenewDate)
                                {
                                    manageCardDet.renewalDate += Utility.FormatDateTime(reDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.ddMMyyyy) + ",";
                                }
                            }
                        }

                        manageCardResp.cardDetails.ForEach(
                            m =>
                            {
                                m.bundleCode = string.IsNullOrEmpty(m.bundleCode) ? m.bundleCode : m.bundleCode.TrimEnd(new char[] { ',' });
                                m.renewalDate = string.IsNullOrEmpty(m.renewalDate) ? m.renewalDate : m.renewalDate.TrimEnd(new char[] { ',' });
                                m.renevalStatus = string.IsNullOrEmpty(m.renevalStatus) ? m.renevalStatus : m.renevalStatus.TrimEnd(new char[] { ',' });
                            });
                        try
                        {
                            CRMBase objBase = new CRMBase();
                            objBase.CountryCode = clientSetting.countryCode;
                            objBase.BrandCode = clientSetting.brandCode;
                            objBase.LanguageCode = clientSetting.langCode;

                            getBundleResource bundleResource = new getBundleResource();

                            serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                            
                                bundleResource = serviceCRM.LoadBundleResource(objBase);
                            

                            foreach (ManageCardDetails cardDetail in manageCardResp.cardDetails)
                            {
                                string bundleName = cardDetail.bundleCode;
                                string bundleCode = cardDetail.bundleCode;

                                try
                                {
                                    if (!string.IsNullOrEmpty(cardDetail.bundleCode) && !string.IsNullOrWhiteSpace(cardDetail.bundleCode))
                                    {
                                        List<string> lstBundleCode = new List<string>(cardDetail.bundleCode.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                                        if (lstBundleCode.Count > 1)
                                        {
                                            bundleName = string.Empty;
                                            foreach (string bc in lstBundleCode)
                                            {
                                                try
                                                {
                                                    if (bundleResource.objBundleResource.Count > 0)
                                                    {
                                                        if (bundleResource.objBundleResource.FindAll(a => a.Value == bc).Count() > 0)
                                                        {
                                                            bundleName += bc + " - " + bundleResource.objBundleResource.FindAll(a => a.Value == bc)[0].BundleDesc;
                                                        }
                                                        else
                                                        {
                                                            bundleName += bc + " - " + Resources.BundleResources.NA + ",";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        bundleName += bc + " - " + Resources.BundleResources.NA + ",";
                                                    }
                                                }
                                                catch
                                                {
                                                    bundleName += bc + " - " + Resources.BundleResources.NA + ",";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (bundleResource.objBundleResource.Count > 0)
                                            {
                                                if (bundleResource.objBundleResource.FindAll(a => a.Value == bundleCode).Count() > 0)
                                                {
                                                    bundleName = bundleCode + " - " + bundleResource.objBundleResource.FindAll(a => a.Value == bundleCode)[0].BundleDesc;
                                                }
                                                else
                                                {
                                                    bundleName = bundleCode + " - " + Resources.BundleResources.NA;
                                                }
                                            }
                                            else
                                            {
                                                bundleName = bundleCode + " - " + Resources.BundleResources.NA;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bundleName = string.Empty;
                                    }
                                }
                                catch
                                {

                                }
                                cardDetail.bundleName = bundleName;
                            }
                            manageCardResp.cardDetails.ForEach(m => m.bundleName = string.IsNullOrEmpty(m.bundleCode) ? m.bundleName : m.bundleName.Trim().TrimEnd(new char[] { ',' }));
                        }
                        catch
                        {

                        }
                    }
                    catch
                    {

                    }
                }
                #endregion
               if (manageCardResp.responseDetails.ResponseCode == "0" && manageCardResp.debitcardDetails != null)
                {
                    manageCardResp.debitcardDetails = manageCardResp.debitcardDetails.OrderByDescending(m => m.bundleCode).ToList();
                    try
                    {
                        foreach (ManageDebitCardDetails manageCardDet in manageCardResp.debitcardDetails)
                        {
                            if (!string.IsNullOrEmpty(manageCardDet.renewalDate))
                            {
                                List<string> lstRenewDate = new List<string>(manageCardDet.renewalDate.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

                                manageCardDet.renewalDate = string.Empty;

                                foreach (string reDate in lstRenewDate)
                                {
                                    manageCardDet.renewalDate += Utility.FormatDateTime(reDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.ddMMyyyy) + ",";
                                }
                            }
                        }

                        manageCardResp.debitcardDetails.ForEach(
                            m =>
                            {
                                m.bundleCode = string.IsNullOrEmpty(m.bundleCode) ? m.bundleCode : m.bundleCode.Contains("0000") ? m.bundleCode.Replace("0000","").Trim(new char[] { ',' }) : m.bundleCode.TrimEnd(new char[] { ',' });
                                m.renewalDate = string.IsNullOrEmpty(m.renewalDate) ? m.renewalDate : m.renewalDate.TrimEnd(new char[] { ',' });
                                m.renevalStatus = string.IsNullOrEmpty(m.renevalStatus) ? m.renevalStatus : m.renevalStatus.TrimEnd(new char[] { ',' });
                                m.cardNumber = string.IsNullOrEmpty(m.cardNumber) ? m.cardNumber : m.cardNumber = new String('*', m.cardNumber.Length - 4) + m.cardNumber.Substring(m.cardNumber.Length - 4);

                            });
                        try
                        {
                            CRMBase objBase = new CRMBase();
                            objBase.CountryCode = clientSetting.countryCode;
                            objBase.BrandCode = clientSetting.brandCode;
                            objBase.LanguageCode = clientSetting.langCode;

                            getBundleResource bundleResource = new getBundleResource();

                            serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                            
                                bundleResource = serviceCRM.LoadBundleResource(objBase);
                            

                            foreach (ManageDebitCardDetails cardDetail in manageCardResp.debitcardDetails)
                            {
                                string bundleName = cardDetail.bundleCode;
                                string bundleCode = cardDetail.bundleCode;

                                try
                                {
                                    if (!string.IsNullOrEmpty(cardDetail.bundleCode) && !string.IsNullOrWhiteSpace(cardDetail.bundleCode))
                                    {
                                        List<string> lstBundleCode = new List<string>(cardDetail.bundleCode.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                                        if (lstBundleCode.Count > 1)
                                        {
                                            bundleName = string.Empty;
                                            foreach (string bc in lstBundleCode)
                                            {
                                                try
                                                {
                                                    if (bundleResource.objBundleResource.Count > 0)
                                                    {
                                                        if (bundleResource.objBundleResource.FindAll(a => a.Value == bc).Count() > 0)
                                                        {
                                                            bundleName += bc + " - " + bundleResource.objBundleResource.FindAll(a => a.Value == bc)[0].BundleDesc;
                                                        }
                                                        else
                                                        {
                                                            bundleName += bc + " - " + Resources.BundleResources.NA + ",";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        bundleName += bc + " - " + Resources.BundleResources.NA + ",";
                                                    }
                                                }
                                                catch
                                                {
                                                    bundleName += bc + " - " + Resources.BundleResources.NA + ",";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (bundleResource.objBundleResource.Count > 0)
                                            {
                                                if (bundleResource.objBundleResource.FindAll(a => a.Value == bundleCode).Count() > 0)
                                                {
                                                    bundleName = bundleCode + " - " + bundleResource.objBundleResource.FindAll(a => a.Value == bundleCode)[0].BundleDesc;
                                                }
                                                else
                                                {
                                                    bundleName = bundleCode + " - " + Resources.BundleResources.NA;
                                                }
                                            }
                                            else
                                            {
                                                bundleName = bundleCode + " - " + Resources.BundleResources.NA;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bundleName = string.Empty;
                                    }
                                }
                                catch
                                {

                                }
                                cardDetail.bundleName = bundleName;
                            }
                            manageCardResp.debitcardDetails.ForEach(m => m.bundleName = string.IsNullOrEmpty(m.bundleCode) ? m.bundleName : m.bundleName.Trim().TrimEnd(new char[] { ',' }));
                        }
                        catch
                        {

                        }
                    }
                    catch
                    {

                    }
                }
                else
                {
                    manageCardResp.cardDetails = new List<ManageCardDetails>();
                    manageCardResp.debitcardDetails = new List<ManageDebitCardDetails>();
                }
                return manageCardResp;
            }
            catch
            {
                return manageCardResp;
            }
            finally
            {
               // manageCardResp = null;
                serviceCRM = null;
            }

        }

    
        public PartialViewResult RefundPayment()
        {
            PaymentRefundResponse paymentRefundResp = new PaymentRefundResponse();
            try
            {
                paymentRefundResp = PaymentRefundCRM();
                return PartialView(paymentRefundResp);
            }
            catch (Exception eX)
            {
                paymentRefundResp.responseDetails.ResponseCode = "9";
                paymentRefundResp.responseDetails.ResponseDesc = eX.Message;
                return PartialView(paymentRefundResp);
            }
            finally
            {
                new LoginController().AuditTrailCRM(new AuditTrailRequest() { action = "Refund Payment", description = "Refund Payment Page Loaded", module = "Payment", subModule = "Refund", DescID = "RefundPayment_Page" });
               // paymentRefundResp = null;
            }

        }

        [HttpPost]
        public JsonResult RefundPaymentTopupBundle(string paymentRefundJson)
        {
            List<Menu> menu = new List<Menu>();
            menu = ((List<Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "Payment_RefundPayment").ToList();
            PaymentRefundResponse paymentRefundResp = new PaymentRefundResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            string resID = "RefundPayment_Req";
            try
            {
                new LoginController().AuditTrailCRM(new AuditTrailRequest() { action = "Refund Payment", description = "Refund Payment Request", module = "Payment", subModule = "Refund", DescID = resID });

                PaymentRefundRequest paymentRefundReq = JsonConvert.DeserializeObject<PaymentRefundRequest>(paymentRefundJson);
                paymentRefundReq.CountryCode = clientSetting.countryCode;
                paymentRefundReq.BrandCode = clientSetting.brandCode;
                paymentRefundReq.LanguageCode = clientSetting.langCode;

                paymentRefundReq.mobileNumber = Convert.ToString(Session["MobileNumber"]);

                if (paymentRefundReq.mode == "BR" || paymentRefundReq.mode == "TR" || paymentRefundReq.mode == "RI" || paymentRefundReq.mode == "BAR")
                {
                    if (paymentRefundReq.refundBundleTopupDetail != null)
                    {
                        paymentRefundReq.refundBundleTopupDetail.transactionDate = Utility.FormatDateTimeToService(paymentRefundReq.refundBundleTopupDetail.transactionDate, clientSetting.mvnoSettings.dateTimeFormat);
                        if (!string.IsNullOrEmpty(paymentRefundReq.refundBundleTopupDetail.expiryDate))
                            paymentRefundReq.refundBundleTopupDetail.expiryDate = Utility.FormatDateTimeToService(paymentRefundReq.refundBundleTopupDetail.expiryDate, clientSetting.mvnoSettings.dateTimeFormat);
                    }
                    if (paymentRefundReq.refundTopupDetail != null)
                    {
                        paymentRefundReq.refundTopupDetail.transactionDate = Utility.FormatDateTimeToService(paymentRefundReq.refundTopupDetail.transactionDate, clientSetting.mvnoSettings.dateTimeFormat);
                    }
                    if (paymentRefundReq.authorizeInfo != null)
                    {
                        paymentRefundReq.authorizeInfo.submittedBy = Session["UserName"].ToString();
                    }
                    if (paymentRefundReq.reinitiateInfo != null)
                    {

                    }
                }

                if ((paymentRefundReq.mode == "A" || paymentRefundReq.mode == "R" || paymentRefundReq.mode == "PA") || (paymentRefundReq.authorizeInfo != null))
                {
                    paymentRefundReq.authorizeInfo.submittedBy = Session["UserName"].ToString();
                    paymentRefundReq.authorizeInfo.authorizedBy = Session["UserName"].ToString();
                    paymentRefundReq.authorizeInfo.rejectedBy = Session["UserName"].ToString();
                    paymentRefundReq.authorizeInfo.emailID = Session["eMailID"].ToString();
                }

                if ((paymentRefundReq.IsPendingApproval == "1" && menu[0].Approval1 != null && menu[0].Approval1.ToUpper() == "TRUE") || (menu[0].DirectApproval != null && menu[0].DirectApproval.ToUpper() == "TRUE"))
                {
                    paymentRefundReq.approvalLevel = "1";
                }

                if (clientSetting.mvnoSettings.CaptureIPAddress.ToUpper() == "ON")
                {
                    paymentRefundReq.DeviceInfo = Utility.DeviceInfo("RefundPayment Topup/Bundle");
                }

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    paymentRefundResp = serviceCRM.PaymentRefundCRM(paymentRefundReq);
                
                if (paymentRefundResp != null && paymentRefundResp.responseDetails != null && paymentRefundResp.responseDetails.ResponseCode != null)
                {
                    if (!string.IsNullOrEmpty(paymentRefundReq.mode) && paymentRefundReq.mode == "A")
                    {
                        if (paymentRefundResp.responseDetails.ResponseCode == "100028")
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RefundTopupBundlePayment_" + "A" + "_" + paymentRefundResp.responseDetails.ResponseCode);
                            paymentRefundResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? paymentRefundResp.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                    }
                    else
                    {
                        resID = "RefundTopupBundlePayment_" + paymentRefundReq.mode + "_" + paymentRefundResp.responseDetails.ResponseCode;
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RefundTopupBundlePayment_" + paymentRefundReq.mode + "_" + paymentRefundResp.responseDetails.ResponseCode);
                        paymentRefundResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? paymentRefundResp.responseDetails.ResponseDesc : errorInsertMsg;
                    }

                    if ((paymentRefundReq.mode == "BR" || paymentRefundReq.mode == "TR" || paymentRefundReq.mode == "RI" || paymentRefundReq.mode == "BAR" || paymentRefundReq.mode == "REMOVE" || paymentRefundReq.mode == "RETURN_BALANCE") && paymentRefundResp.responseDetails.ResponseCode == "0")
                    {
                        PaymentRefundResponse tmpPaymentRefundResp = new PaymentRefundResponse();
                        tmpPaymentRefundResp = PaymentRefundCRM();

                        paymentRefundResp.bundleTopupInfo = tmpPaymentRefundResp.bundleTopupInfo;
                        paymentRefundResp.topupInfo = tmpPaymentRefundResp.topupInfo;
                        paymentRefundResp.refundSubmittedRecords = tmpPaymentRefundResp.refundSubmittedRecords;
                        paymentRefundResp.autoRenewalRefund = tmpPaymentRefundResp.autoRenewalRefund;
                        paymentRefundResp.BundleAutoFailedInfo = tmpPaymentRefundResp.BundleAutoFailedInfo;
                        paymentRefundResp.BalDeductedAMOUNT = tmpPaymentRefundResp.BalDeductedAMOUNT;
                        paymentRefundResp.BalDeductedRetainDays = tmpPaymentRefundResp.BalDeductedRetainDays;

                        tmpPaymentRefundResp = null;
                    }
                }
                return Json(paymentRefundResp);
            }
            catch (Exception eX)
            {
                paymentRefundResp.responseDetails = new CRMResponse();
                paymentRefundResp.responseDetails.ResponseCode = "9";
                paymentRefundResp.responseDetails.ResponseDesc = eX.Message;
                return Json(paymentRefundResp);
            }
            finally
            {
                new LoginController().AuditTrailCRM(new AuditTrailRequest() { action = "Refund Payment", description = "Refund Payment Response", module = "Payment", subModule = "Refund", DescID = resID });
                menu = null;
                //paymentRefundResp = null;
                serviceCRM = null;
            }

        }

        private PaymentRefundResponse PaymentRefundCRM()
        {
            PaymentRefundResponse paymentRefundResp = new PaymentRefundResponse();
            PaymentRefundRequest paymentRefundReq = new PaymentRefundRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {

                paymentRefundReq.CountryCode = clientSetting.countryCode;
                paymentRefundReq.BrandCode = clientSetting.brandCode;
                paymentRefundReq.LanguageCode = clientSetting.langCode;
                paymentRefundReq.mobileNumber = Convert.ToString(Session["MobileNumber"]);
                paymentRefundReq.mode = "Q";

                if ((Session["PAType"] != null && Convert.ToString(Session["PAType"]) == "REFUND PAYMENT") && (Session["PAId"] != null && Convert.ToString(Session["PAId"]) != string.Empty))
                {
                    paymentRefundReq.requestID = Convert.ToString(Session["PAId"]);
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    paymentRefundResp = serviceCRM.PaymentRefundCRM(paymentRefundReq);
                
                if (paymentRefundResp != null && paymentRefundResp.responseDetails != null && paymentRefundResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RefundPayment_" + paymentRefundResp.responseDetails.ResponseCode);
                    paymentRefundResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? paymentRefundResp.responseDetails.ResponseDesc : errorInsertMsg;
                    paymentRefundResp.DecimalLimit = Convert.ToInt32(clientSetting.mvnoSettings.decimalLimit);
                    if (paymentRefundResp.bundleTopupInfo != null && paymentRefundResp.bundleTopupInfo.Count > 0)
                    {
                        paymentRefundResp.bundleTopupInfo.FindAll(a => !string.IsNullOrEmpty(a.expiryDate)).ForEach(b => b.expiryDate = Utility.FormatDateTime(b.expiryDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.ddMMyyyy));
                        paymentRefundResp.bundleTopupInfo.FindAll(a => !string.IsNullOrEmpty(a.transactionDate)).ForEach(b => b.transactionDate = Utility.FormatDateTime(b.transactionDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.ddMMyyyy));
                    }

                    if (paymentRefundResp.topupInfo != null && paymentRefundResp.topupInfo.Count > 0)
                    {
                        paymentRefundResp.topupInfo.FindAll(a => !string.IsNullOrEmpty(a.transactionDate)).ForEach(b => b.transactionDate = Utility.FormatDateTime(b.transactionDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.ddMMyyyy));
                    }

                    // FRR 3892
                    if (paymentRefundResp.autoRenewalRefund != null && paymentRefundResp.autoRenewalRefund.Count > 0)
                    {
                        paymentRefundResp.autoRenewalRefund.FindAll(a => !string.IsNullOrEmpty(a.TransactionDate)).ForEach(b => b.TransactionDate = Utility.GetDateconvertion(b.TransactionDate, "yyyy-mm-dd HH:mm:ss", false, clientSetting.mvnoSettings.dateTimeFormat));
                    }
                    // POF-6169
                    if (paymentRefundResp.BundleAutoFailedInfo != null && paymentRefundResp.BundleAutoFailedInfo.Count > 0 && paymentRefundResp.BundleAutoFailedInfo[0].RENEWAL_MODE =="Credit Card")
                    {
                        paymentRefundResp.BundleAutoFailedInfo.ForEach(b => b.FAILED_DATE = !string.IsNullOrEmpty(b.FAILED_DATE) ? DateTime.ParseExact(b.FAILED_DATE, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString() : "");
                    }
                    // FRR 3892
                    else if (paymentRefundResp.BundleAutoFailedInfo != null && paymentRefundResp.BundleAutoFailedInfo.Count > 0)
                    {
                        paymentRefundResp.BundleAutoFailedInfo.FindAll(a => !string.IsNullOrEmpty(a.FAILED_DATE)).ForEach(b => b.FAILED_DATE = Utility.GetDateconvertion(b.FAILED_DATE, "DD-MM-YYYY", false, clientSetting.mvnoSettings.dateTimeFormat));
                    }

                    if (paymentRefundResp.refundSubmittedRecords != null && paymentRefundResp.refundSubmittedRecords.Count > 0)
                    {
                        paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.transactionDate)).ForEach(b => b.transactionDate = Utility.FormatDateTime(b.transactionDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.MMddyyyy));
                        paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.expiryDate)).ForEach(b => b.expiryDate = Utility.FormatDateTime(b.expiryDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.MMddyyyy));
                        paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.submittedOn)).ForEach(b => b.submittedOn = Utility.FormatDateTime(b.submittedOn, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.MMddyyyy));
                        paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.authorizedOn)).ForEach(b => b.authorizedOn = Utility.FormatDateTime(b.authorizedOn, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.MMddyyyy));
                        paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.rejectedOn)).ForEach(b => b.rejectedOn = Utility.FormatDateTime(b.rejectedOn, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.MMddyyyy));
                        paymentRefundResp.refundSubmittedRecords.FindAll(a => !string.IsNullOrEmpty(a.reinitiationDate)).ForEach(b => b.reinitiationDate = Utility.FormatDateTime(b.reinitiationDate, clientSetting.mvnoSettings.dateTimeFormat, CultureInfoCRM.MMddyyyy));
                    }
                }
                return paymentRefundResp;
            }
            catch
            {
                return paymentRefundResp;
            }
            finally
            {
                //paymentRefundResp = null;
                paymentRefundReq = null;
                serviceCRM = null;

            }

        }

        [HttpPost]
        public JsonResult RefundFeeSplitUp(string TopupType, string BundleCode, string TrnsactionId)
        {
            PaymentRefundResponse objResp = new PaymentRefundResponse();
            PaymentRefundRequest paymentRefundReq = new PaymentRefundRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {

                paymentRefundReq.CountryCode = clientSetting.countryCode;
                paymentRefundReq.BrandCode = clientSetting.brandCode;
                paymentRefundReq.LanguageCode = clientSetting.langCode;
                paymentRefundReq.mobileNumber = Convert.ToString(Session["MobileNumber"]);
                paymentRefundReq.TopupType = TopupType;
                paymentRefundReq.BundleCode = BundleCode;
                paymentRefundReq.TransactionId = TrnsactionId;
                paymentRefundReq.mode = "FSU";

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objResp = serviceCRM.PaymentRefundCRM(paymentRefundReq);
                
                return Json(objResp);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(objResp);
            }
            finally
            {
                paymentRefundReq = null;
               // objResp = null;
                serviceCRM = null;
            }


        }

        [HttpPost]
        public JsonResult DummyMethod()
        {
            PaymentRefundResponse objResp = new PaymentRefundResponse();
            try
            {

                return Json(objResp);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(objResp);
            }
            finally
            {
              //  objResp = null;
            }


        }

        public ViewResult NewcardEntry(string Textdata)
        {

            NewcardEntry cardTopup = new NewcardEntry();
            try
            {
                #region FRR 4925
                if (clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {

                    Session["RealICCIDForMultiTab"] = Textdata;
                }
                #endregion

                CRMResponse objRes = Utility.checkValidSubscriber("", clientSetting);
                if (objRes.ResponseCode == "0")
                {
                    cardTopup.ResponseCode = "0";
                }
                else
                {
                    cardTopup.ResponseCode = "1";
                    cardTopup.ResponseDescription = objRes.ResponseDesc;
                }

                //6300
                cardTopup.SWEdrpdown = Utility.GetDropdownMasterFromDB("23,30", Convert.ToString(Session["isPrePaid"]), "drop_master");

            }
            catch (Exception)
            {

                throw;
            }
            return View(cardTopup);
        }

        [HttpPost]
        public ActionResult CRMMaintelAPINewCardEntry(string Newcardentry) // CRMSabioNewCardEntry changed to CRMMaintelAPINewCardEntry
        {
            NewCardEntryRes ObjRes = new NewCardEntryRes();
            NewCardEntryReq objtopupreq = new NewCardEntryReq();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objtopupreq = JsonConvert.DeserializeObject<NewCardEntryReq>(Newcardentry);
                objtopupreq.BrandCode = clientSetting.brandCode;
                objtopupreq.CountryCode = clientSetting.countryCode;
                objtopupreq.LanguageCode = clientSetting.langCode;
                objtopupreq.Country = objtopupreq.CountryCode;

                //POF-6301
                objtopupreq.UserAgentHeader = Utility.UseragentAPM("Autorenewal Maintel");


                #region FRR 4925
                if(clientSetting.preSettings.EnableCRMMultiTab.ToUpper() == "TRUE")
                {
                    Dictionary<string, MultitabResponse> localDict = (Dictionary<string, MultitabResponse>)Session["SessionsampleDict"];
                    objtopupreq.Msisdn = localDict.Where(x => objtopupreq.textdata.ToString().Contains(x.Key)).Select(x => x.Value.MSISDN).First().ToString();
                   
                }
                else
                {
                    objtopupreq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                }
                #endregion

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMMaintelAPINewCardEntry(objtopupreq);

                    ///FRR--3083
                    if (ObjRes != null && ObjRes.ResponseDetails != null && ObjRes.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CRMSabioNewcardentry_" + ObjRes.ResponseDetails.ResponseCode);
                        ObjRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDetails.ResponseDesc : errorInsertMsg;
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
                //ObjRes = null;
                objtopupreq = null;
                serviceCRM = null;
            }


        }

        [HttpPost]
        public ActionResult SplitFee(string BundleInput)
        {
            TransactionInfoRequest objreq = JsonConvert.DeserializeObject<TransactionInfoRequest>(BundleInput);
            TransactionInfoResponse objres = new TransactionInfoResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {

                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.Msisdn = Session["MobileNumber"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objres = serviceCRM.TransactionInfoCRM(objreq);
                
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
                //objres = null;
                serviceCRM = null;
            }


        }

        [HttpPost]
        public ActionResult BundleTransferSplitFee(string BundleInput)
        {
            TransactionInfoRequest objreq = JsonConvert.DeserializeObject<TransactionInfoRequest>(BundleInput);
            TransactionInfoResponse objres = new TransactionInfoResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {

                objreq.BrandCode = clientSetting.brandCode;
                objreq.CountryCode = clientSetting.countryCode;
                objreq.LanguageCode = clientSetting.langCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objres = serviceCRM.TransactionInfoCRM(objreq);
                
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
                //objres = null;
                serviceCRM = null;
            }


        }


        #region Manage Consent

        public ActionResult ManageConsent()
        {

            ManageCardCRM manageCardCRM = new ManageCardCRM();
            ManagedCardResponse manageCardResp = new ManagedCardResponse();
            ManagedCardRequest manageCardReq = new ManagedCardRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                manageCardCRM.crmResponse = new CRMResponse();
                manageCardCRM.crmResponse.ResponseCode = "0";
                manageCardCRM.paymentCRM = new PaymentCRM();


                manageCardReq.CountryCode = clientSetting.countryCode;
                manageCardReq.BrandCode = clientSetting.brandCode;
                manageCardReq.LanguageCode = clientSetting.langCode;
                manageCardReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                manageCardReq.mode = "Q";

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    manageCardResp = serviceCRM.CRMManageCardDetails(manageCardReq);
                

                if (manageCardResp != null && manageCardResp.responseDetails != null && manageCardResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("CardPayment_" + manageCardResp.responseDetails.ResponseCode);
                    manageCardResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? manageCardResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                if (manageCardResp.cardDetails != null && manageCardResp.cardDetails.Count > 0)
                {
                    manageCardCRM.creditCardList = manageCardResp.cardDetails;
                    manageCardCRM.creditCardList.ForEach(m => m.cardexpirydate = Convert.ToDateTime(m.cardexpirydate).ToString("MM '/' yyyy"));
                }
                else
                {
                    manageCardCRM.creditCardList = new List<ManageCardDetails>();
                }

                manageCardCRM.crmResponse = manageCardCRM.crmResponse;
                if (!string.IsNullOrEmpty(manageCardResp.ConsentDate))
                {
                    manageCardCRM.ConsentDate = Utility.GetDateconvertion(manageCardResp.ConsentDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                }

                manageCardCRM.creditCardList.ForEach(a =>
                {
                    a.ConsentStartDate = Utility.GetDateconvertion(a.ConsentStartDate, "mm/dd/yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                    a.ConsentExpiredDate = Utility.GetDateconvertion(a.ConsentExpiredDate, "mm/dd/yyyy", false, clientSetting.mvnoSettings.dateTimeFormat);
                });
                return View(manageCardCRM);
            }
            catch (Exception eX)
            {
                manageCardCRM.crmResponse.ResponseCode = "9";
                manageCardCRM.crmResponse.ResponseDesc = eX.Message;
                return View(manageCardCRM);
            }
            finally
            {
               // manageCardCRM = null;
                manageCardResp = null;
                manageCardReq = null;
                serviceCRM = null;
            }


        }


        public ActionResult GetPaymentDetails(string Details)
        {
            PaymentDetailsResponse ObjResp = new PaymentDetailsResponse();
            PaymentDetailsRequest ObjReq = JsonConvert.DeserializeObject<PaymentDetailsRequest>(Details);
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjResp = serviceCRM.GetPaymentDetails(ObjReq);
                
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
                serviceCRM = null;

            }


        }



        public JsonResult RenewConsent(string cardID, string ccNumber, string ConsentDate)
        {
            ManagedCardResponse manageCardResp = new ManagedCardResponse();
            ManagedCardRequest manageCardReq = new ManagedCardRequest();
            try
            {

                manageCardReq.CountryCode = clientSetting.countryCode;
                manageCardReq.BrandCode = clientSetting.brandCode;
                manageCardReq.LanguageCode = clientSetting.langCode;

                manageCardReq.cardId = cardID;
                manageCardReq.cardNumber = ccNumber;
                manageCardReq.Msisdn = Convert.ToString(Session["MobileNumber"]);
                manageCardReq.userName = Session["UserName"].ToString();
                manageCardReq.emailID = Convert.ToString(Session["eMailID"]);

                manageCardReq.mode = "RENEW";
                if (!string.IsNullOrEmpty(ConsentDate))
                {
                    manageCardReq.ConsentDate = Utility.GetDateconvertion(ConsentDate, "MM/DD/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                }

                manageCardResp = ManageCardCRMResponse(manageCardReq);

                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("RenewConsent_" + manageCardResp.responseDetails.ResponseCode);
                manageCardResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? manageCardResp.responseDetails.ResponseDesc : errorInsertMsg;
                return Json(manageCardResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(manageCardResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                manageCardReq = null;
               // manageCardResp = null;
            }

        }

        #endregion


        #region FRR 4781 ManageIBAN
        public ActionResult ManageIBAN()
        {

            return View();
        }

        public JsonResult GETManageIBAN(string GetAltanDeviceInfoDetails)
        {
            GETManageIBANResponse ObjResp = new GETManageIBANResponse();
            GETManageIBANRequest ObjReq = JsonConvert.DeserializeObject<GETManageIBANRequest>(GetAltanDeviceInfoDetails);
            string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjResp = serviceCRM.GETManageIBAN(ObjReq);

                
                    if (ObjResp != null && ObjResp.ResponseDetails != null && ObjResp.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ManageIBAN_" + ObjResp.ResponseDetails.ResponseCode);
                        ObjResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.ResponseDetails.ResponseDesc : errorInsertMsg;
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
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        #endregion

        #region POF-6220

        public ActionResult PostpaidRefund()
        {
            PostpaidRefundRequest Objreq = new PostpaidRefundRequest();
            PostpaidRefundResponse Objresp = new PostpaidRefundResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                Objreq.CountryCode = clientSetting.countryCode;
                Objreq.BrandCode = clientSetting.brandCode;
                Objreq.LanguageCode = clientSetting.langCode;
                Objreq.MSISDN = Session["MobileNumber"].ToString();
                Objreq.Mode = string.IsNullOrEmpty(Objreq.Mode) ? "GET" : Objreq.Mode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "PostpaidRefund Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objresp = serviceCRM.CRMPostpaidRefund(Objreq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "PostpaidRefund End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "PostpaidRefund - " + this.ControllerContext, ex);
            }
            finally
            {
                Objreq = null;
            }

            return View(Objresp);
        }
        public JsonResult PostpaidRefund_refund(string strCRMpostpaidrefund)
        {
            PostpaidRefundRequest Objreq = JsonConvert.DeserializeObject<PostpaidRefundRequest>(strCRMpostpaidrefund);
            PostpaidRefundResponse Objresp = new PostpaidRefundResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                Objreq.CountryCode = clientSetting.countryCode;
                Objreq.BrandCode = clientSetting.brandCode;
                Objreq.LanguageCode = clientSetting.langCode;
                Objreq.MSISDN = Session["MobileNumber"].ToString();
                Objreq.Mode = string.IsNullOrEmpty(Objreq.Mode) ? "GET" : Objreq.Mode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "PostpaidRefund Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objresp = serviceCRM.CRMPostpaidRefund(Objreq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "PostpaidRefund End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "PostpaidRefund - " + this.ControllerContext, ex);
            }
            finally
            {
                Objreq = null;
            }

            return new JsonResult() { Data = Objresp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        #endregion

    }
}
