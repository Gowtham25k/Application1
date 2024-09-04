using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using CRM.Models;

namespace CRM.Controllers
{
    [ValidateState]
    public class GAFController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();

        public PartialViewResult AddressFinderCRM(string showpopup)
        {
            AddressCRM addressCRM = new AddressCRM();
            try
            {
                addressCRM.isShowPopup = string.IsNullOrEmpty(showpopup) || (!string.IsNullOrEmpty(showpopup) && showpopup.ToUpper() != "FALSE");
                return PartialView(addressCRM);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return PartialView(addressCRM);
            }
            finally
            {
               // addressCRM = null;
            }

        }

        

        public JsonResult FindAddress(AddressInput inputAddressCRM)
        {
            List<SelectListItem> lstAddressItem = new List<SelectListItem>();
            try
            {
                using (Service.GAFService.GlobalAddress gaf = new Service.GAFService.GlobalAddress())
                {
                    gaf.Url = SettingsCRM.crmGAFUrl;
                    Service.GAFService.Contact contactInput = new Service.GAFService.Contact();
                    Service.GAFService.Contact[] contactOutput = null;
                    Object[] Exclude = new Object[1];
                    Exclude[0] = Service.GAFService.OFT.Country;
                    if (clientSetting.mvnoSettings.countryPrefix.Trim() == "237") //Camerron +237 CMR
                    {
                        contactInput.Region = inputAddressCRM.cityName;
                        contactInput.City = inputAddressCRM.postCode;
                    }
                    else if (clientSetting.mvnoSettings.countryPrefix.Trim() == "7") //Russia
                    {
                        contactInput.Postcode = inputAddressCRM.postCode;
                        contactInput.City = inputAddressCRM.cityName;
                        contactInput.Street = inputAddressCRM.streetName;
                    }
                    else
                        if (clientSetting.mvnoSettings.countryPrefix.Trim() != "852")
                        {
                            contactInput.Postcode = inputAddressCRM.postCode;
                            contactInput.DPS = inputAddressCRM.terittorialCode;
                            contactInput.Street = inputAddressCRM.streetName;
                            contactInput.AddressLine1 = inputAddressCRM.addressLine1;
                            contactInput.AddressLine2 = inputAddressCRM.addressLine2;
                            contactInput.Region = inputAddressCRM.districtName;
                            contactInput.City = inputAddressCRM.cityName;
                        }
                        else if (clientSetting.mvnoSettings.countryPrefix.Trim() == "852")
                        {
                            contactInput.City = inputAddressCRM.cityName;
                        }
                        else
                        {
                            contactInput.Street = inputAddressCRM.streetName;
                            contactInput.City = inputAddressCRM.areaName;
                            contactInput.SubCity = inputAddressCRM.districtName;
                        }

                    contactInput.CountryISO = clientSetting.mvnoSettings.countryCodeGAF.Trim();
                    contactInput.Country = clientSetting.mvnoSettings.countryName.Trim();

                    if (gaf.SearchAddressEx(clientSetting.mvnoSettings.userNameGAF, clientSetting.mvnoSettings.passWordGAF, contactInput, Convert.ToInt32(clientSetting.mvnoSettings.autReturnMaxResults), Exclude, out contactOutput))
                    {
                        foreach (Service.GAFService.Contact contact in contactOutput)
                        {
                            SelectListItem addressItem = new SelectListItem();

                            string listValue = string.Empty;
                            string listText = contact.AddressLine1;

                            if (contact.AddressLine2 != string.Empty) listText += ", " + contact.AddressLine2;
                            if (contact.AddressLine3 != string.Empty) listText += ", " + contact.AddressLine3;
                            if (contact.AddressLine4 != string.Empty) listText += ", " + contact.AddressLine4;
                            if (contact.AddressLine5 != string.Empty) listText += ", " + contact.AddressLine5;
                            if (contact.AddressLine6 != string.Empty) listText += ", " + contact.AddressLine6;
                            if (contact.AddressLine7 != string.Empty) listText += ", " + contact.AddressLine7;
                            if (contact.AddressLine8 != string.Empty) listText += ", " + contact.AddressLine8;
                            if (contact.Region != string.Empty) listText += ", " + contact.Region;

                            if (clientSetting.mvnoSettings.countryPrefix.Trim() == "237") //Camerron +237 CMR
                            {
                                //listValue = contact.AddressLine1 + " " + contact.AddressLine2 + "," + contact.City + "," + contact.Region + ","  + contact.Country + "," + contact.CountryISO;
                                listValue = contact.AddressLine1 + "," + contact.City + "," + contact.Region;
                            }
                            else if (clientSetting.mvnoSettings.countryPrefix.Trim() == "380") //Ukraine +380 UKR
                            {
                                listValue = contact.Premise + "," + contact.Street + "," + contact.City + "," + contact.Postcode;
                            }
                            else if (clientSetting.mvnoSettings.countryPrefix.Trim() == "7") //Russia
                            {
                                listValue = contact.Premise + " " + contact.Street + "," + contact.City + "," + contact.Other3 + "," + contact.Region + "," + contact.Country + "," + contact.CountryISO;
                            }
                            else
                                if (clientSetting.mvnoSettings.countryPrefix.Trim() != "852" && clientSetting.mvnoSettings.countryPrefix.Trim() != "61")
                                {
                                    if (clientSetting.mvnoSettings.countryPrefix.Trim() != "27")
                                    {
                                        listValue = contact.Premise + "|" + contact.Company + contact.SubBuilding + " " + contact.Building + " " + contact.Street + "|" + contact.City + "|" + contact.Postcode;
                                        listValue += "|" + contact.Region + "|" + contact.AddressLine1.ToString().Trim() + "|" + contact.SubCity.ToString().Trim() + "|" + contact.DPS;
                                    }
                                    else
                                    {
                                        listValue = contact.Premise + "|" + contact.Street + "|" + contact.City + "|" + contact.Postcode;
                                        listValue += "|" + contact.Region + "|" + contact.AddressLine1.ToString().Trim() + "|" + contact.SubCity.ToString().Trim() + "|" + contact.DPS;
                                    }
                                }
                                else if (clientSetting.mvnoSettings.countryPrefix.Trim() == "61")
                                {
                                    listValue = contact.Premise + "|" + contact.Company + " " + contact.Building + " " + contact.Street + "|" + contact.City + "|" + contact.Postcode;
                                    listValue += "|" + contact.Region + "|" + contact.AddressLine1.ToString().Trim() + "|" + contact.SubCity.ToString().Trim() + "|" + contact.SubBuilding;
                                }
                                else
                                {
                                    listValue = listText;
                                }
                            addressItem.Text = listText;
                            addressItem.Value = listValue;

                            lstAddressItem.Add(addressItem);
                        }
                    }
                    else
                    {

                    }
                }
                return Json(lstAddressItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(lstAddressItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //lstAddressItem = null;
            }

        }

       

        public JsonResult FetchCityByState(AddressInput inputAddressCRM)
        {
            List<SelectListItem> lstCityItem = new List<SelectListItem>();
            try
            {
                using (Service.GAFService.GlobalAddress gaf = new Service.GAFService.GlobalAddress())
                {
                    gaf.Url = SettingsCRM.crmGAFUrl;
                    Service.GAFService.Contact[] contactOutput = null;

                    Service.GAFService.OFT[] returnFields = new Service.GAFService.OFT[1];
                    returnFields[0] = Service.GAFService.OFT.City;

                    Service.GAFService.Criteria[] criteria = new Service.GAFService.Criteria[2];

                    Service.GAFService.Criteria myCriteria = new Service.GAFService.Criteria();
                    myCriteria.setCriteriaValue = inputAddressCRM.State;
                    myCriteria.setCriteriaSearchType = Service.GAFService.SearchType.OPTIMA_EXACT_SEARCH;

                    Service.GAFService.OFT[] criteriaFields = new Service.GAFService.OFT[2];
                    criteriaFields[0] = Service.GAFService.OFT.Region;

                    Service.GAFService.Criteria myCriteria1 = new Service.GAFService.Criteria();
                    myCriteria1.setCriteriaValue = inputAddressCRM.postCode;
                    myCriteria1.setCriteriaSearchType = Service.GAFService.SearchType.OPTIMA_EXACT_SEARCH;

                    criteriaFields[1] = Service.GAFService.OFT.Postcode;
                    myCriteria.addCriteriaFields = criteriaFields;
                    myCriteria1.addCriteriaFields = criteriaFields;

                    criteria[0] = myCriteria;
                    criteria[1] = myCriteria1;

                    if (gaf.Select(clientSetting.mvnoSettings.userNameGAF, clientSetting.mvnoSettings.passWordGAF, clientSetting.mvnoSettings.countryCodeGAF, criteria, returnFields, true, true, 1, Convert.ToInt32(clientSetting.mvnoSettings.autReturnMaxResults), out contactOutput))
                    {
                        foreach (Service.GAFService.Contact contact in contactOutput)
                        {
                            if (!string.IsNullOrEmpty(contact.City))
                            {
                                SelectListItem cityItem = new SelectListItem();

                                cityItem.Text = contact.City;
                                cityItem.Value = contact.City;

                                lstCityItem.Add(cityItem);
                            }
                        }
                    }
                    else
                    {

                    }
                }
                return Json(lstCityItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(lstCityItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //lstCityItem = null;
            }
        }

        

        public JsonResult FetchDistrict(string areaName)
        {
            List<SelectListItem> lstDistrictItem = new List<SelectListItem>();
            try
            {
                List<string> lstDistrict = Utility.DataTableToList(Utility.GetDropdownMasterFromDB(areaName, System.Web.HttpContext.Current.Session["UserName"].ToString(), "tbl_city"));

                foreach (string distr in lstDistrict)
                {
                    SelectListItem distrItem = new SelectListItem();

                    distrItem.Text = distr;
                    distrItem.Value = distr;

                    lstDistrictItem.Add(distrItem);
                }
                return Json(lstDistrictItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(lstDistrictItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
               // lstDistrictItem = null;
            }

        }




        #region GAF for Common
        public JsonResult GAFFindAddress(AddressInput inputAddressCRM)
        {
            List<SelectListItem> lstAddressItem = new List<SelectListItem>();
            try
            {
                using (Service.GAFService.GlobalAddress gaf = new Service.GAFService.GlobalAddress())
                {
                    gaf.Url = SettingsCRM.crmGAFUrl;
                    Service.GAFService.Contact contactInput = new Service.GAFService.Contact();
                    Service.GAFService.Contact[] contactOutput = null;
                    Object[] Exclude = new Object[1];
                    Exclude[0] = Service.GAFService.OFT.Country;
                    if (clientSetting.brandCode.ToUpper().Trim() == clientSetting.preSettings.ToggleBrandCode.ToUpper().Trim() && clientSetting.countryCode == "NLD")
                    {
                        //for toggle
                        //contactInput.Postcode = inputAddressCRM.postCode;
                        //contactInput.Street = inputAddressCRM.streetName;
                        if (inputAddressCRM.allowCountry == "NLD" || inputAddressCRM.allowCountry == "SWE")
                        {
                            contactInput.Postcode = inputAddressCRM.postCode;
                            contactInput.Street = inputAddressCRM.streetName;
                            contactInput.City = inputAddressCRM.cityName;
                        }
                        else
                        {
                            contactInput.AddressLine1 = inputAddressCRM.streetName;
                            contactInput.AddressLine2 = inputAddressCRM.postCode;
                            contactInput.AddressLine3 = inputAddressCRM.cityName;
                            contactInput.Region = inputAddressCRM.districtName;
                        }

                        //contactInput.City = inputAddressCRM.cityName;


                    }
                    else
                    {

                        //For non toggle
                        contactInput.Postcode = inputAddressCRM.postCode;
                        contactInput.Street = inputAddressCRM.streetName;
                        contactInput.AddressLine1 = inputAddressCRM.addressLine1;
                        contactInput.AddressLine2 = inputAddressCRM.addressLine2;
                        contactInput.Region = inputAddressCRM.districtName;
                        contactInput.City = inputAddressCRM.cityName;

                    }
                    if (clientSetting.brandCode.Trim() == clientSetting.preSettings.ToggleBrandCode.Trim() && clientSetting.mvnoSettings.AllowCountrySelection.Trim() == "1" && (clientSetting.countryCode.ToUpper() == "GBR" || clientSetting.countryCode == "NLD"))
                    {  //Toggle Registration 
                        contactInput.CountryISO = inputAddressCRM.allowCountry;
                        contactInput.Country = inputAddressCRM.allowCountryName;
                        if (string.IsNullOrEmpty(inputAddressCRM.allowCountry) || string.IsNullOrEmpty(inputAddressCRM.allowCountryName))
                        {
                            contactInput.CountryISO = clientSetting.mvnoSettings.countryCodeGAF.Trim();
                            contactInput.Country = clientSetting.mvnoSettings.countryName.Trim();
                        }
                    }
                    else
                    {
                        contactInput.CountryISO = clientSetting.mvnoSettings.countryCodeGAF.Trim();
                        contactInput.Country = clientSetting.mvnoSettings.countryName.Trim();
                    }


                    if (gaf.SearchAddressEx(clientSetting.mvnoSettings.userNameGAF, clientSetting.mvnoSettings.passWordGAF, contactInput, Convert.ToInt32(clientSetting.mvnoSettings.autReturnMaxResults), Exclude, out contactOutput))
                    {
                        foreach (Service.GAFService.Contact contact in contactOutput)
                        {
                            SelectListItem addressItem = new SelectListItem();

                            string listValue = string.Empty;
                            string listText = contact.AddressLine1;

                            if (contact.AddressLine2 != string.Empty && contact.AddressLine2 != "null") listText += ", " + contact.AddressLine2;
                            if (contact.AddressLine3 != string.Empty && contact.AddressLine3 != "null") listText += ", " + contact.AddressLine3;
                            //if (clientSetting.mvnoSettings.countryCodeGAF.Trim().ToUpper() != "IRL")
                            //{
                            //if (contact.AddressLine3 != string.Empty) listText += ", " + contact.AddressLine3;

                            //}
                            //else
                            //{
                            //    if (contact.AddressLine3 != null && contact.AddressLine3 != string.Empty) listText += ", " + contact.AddressLine3;//only for IRL

                            //}
                            if (contact.AddressLine4 != string.Empty && contact.AddressLine4 != "null") listText += ", " + contact.AddressLine4;
                            //if (clientSetting.mvnoSettings.countryCodeGAF.Trim().ToUpper() != "IRL")
                            //{
                            //if (contact.AddressLine4 != string.Empty) listText += ", " + contact.AddressLine4;
                            //}
                            //else
                            //{
                            //    if (contact.AddressLine4 != null && contact.AddressLine4 != string.Empty) listText += ", " + contact.AddressLine4;
                            //}
                            if (contact.AddressLine5 != string.Empty && contact.AddressLine5 != "null") listText += ", " + contact.AddressLine5;
                            if (contact.AddressLine6 != string.Empty && contact.AddressLine6 != "null") listText += ", " + contact.AddressLine6;
                            if (contact.AddressLine7 != string.Empty && contact.AddressLine7 != "null") listText += ", " + contact.AddressLine7;
                            if (contact.AddressLine8 != string.Empty && contact.AddressLine8 != "null") listText += ", " + contact.AddressLine8;
                            if (contact.Region != string.Empty) listText += ", " + contact.Region;

                            if (clientSetting.mvnoSettings.countryCodeGAF.Trim().ToUpper() == "HKG")
                            {
                                listValue = contact.Premise + "|" + contact.Street + "|" + contact.City + "|" + contact.Postcode + "|" + contact.AddressLine3 + "|" + contact.AddressLine3 + "|" + contact.SubBuilding + "|" + contact.Building + "|" + contact.Other10 + "|" + contact.SubCity + "|" + contact.DPS;
                            }
                            else if (clientSetting.mvnoSettings.countryCodeGAF.Trim().ToUpper() == "GBR")
                            {
                                listValue = contact.Premise + "|" + contact.Company + " " + contact.SubBuilding + " " + contact.Building + " " + contact.Street + "|" + contact.City + "|" + contact.Postcode + "|" + contact.Region + "|" + contact.AddressLine3 + "|" + contact.SubBuilding + "|" + contact.Building + "|" + contact.Other10 + "|" + contact.SubCity + "|" + contact.DPS;
                            }
                            else if (clientSetting.mvnoSettings.countryCodeGAF.Trim().ToUpper() == "PRT")
                            {
                                if (contact.Postcode.Contains("-"))
                                {
                                    string[] PostalCode = contact.Postcode.Split('-');
                                    contact.Postcode = PostalCode[0].ToString();
                                    contact.DPS = PostalCode[1].ToString();
                                }
                                listValue = contact.Premise + "|" + contact.Company + " " + contact.SubBuilding + " " + contact.Building + " " + contact.Street + "|" + contact.City + "|" + contact.Postcode + "|" + contact.Region + "|" + contact.AddressLine3 + "|" + contact.SubBuilding + "|" + contact.Building + "|" + contact.Other10 + "|" + contact.SubCity + "|" + contact.DPS;
                            }
                            else if (clientSetting.mvnoSettings.countryCodeGAF.Trim().ToUpper() == "IRL")
                            {
                                //listValue = contact.AddressLine1 + "|" + contact.AddressLine2 + "|"+contact.AddressLine4;
                                listValue = contact.Premise + "|" + contact.Street + "|" + contact.City + "|" + contact.SubCity + "|" + contact.Region + "|" + contact.AddressLine3 + "|" + contact.SubBuilding + "|" + contact.Building + "|" + contact.Other10 + "|" + contact.DPS + "|" + contact.AddressLine1;
                            }
                            else
                            {
                                listValue = contact.Premise + "|" + contact.Street + "|" + contact.City + "|" + contact.Postcode + "|" + contact.Region + "|" + contact.AddressLine3 + "|" + contact.SubBuilding + "|" + contact.Building + "|" + contact.Other10 + "|" + contact.SubCity + "|" + contact.DPS;
                            }
                            addressItem.Text = listText;
                            addressItem.Value = listValue;
                            lstAddressItem.Add(addressItem);
                        }
                    }
                    else
                    {

                    }
                }
                return Json(lstAddressItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(lstAddressItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //lstAddressItem = null;
            }

        }

        public JsonResult GAFFetchState(AddressInput inputAddressCRM)
        {
            List<SelectListItem> lstCityItem = new List<SelectListItem>();
            try
            {
                using (Service.GAFService.GlobalAddress gaf = new Service.GAFService.GlobalAddress())
                {
                    gaf.Url = SettingsCRM.crmGAFUrl;
                    Service.GAFService.Contact[] contactOutput = null;

                    Service.GAFService.OFT[] returnFields = new Service.GAFService.OFT[1];
                    returnFields[0] = Service.GAFService.OFT.Region;

                    Service.GAFService.Criteria[] criteria = new Service.GAFService.Criteria[1];

                    Service.GAFService.Criteria myCriteria = new Service.GAFService.Criteria();
                    myCriteria.setCriteriaValue = inputAddressCRM.postCode;
                    myCriteria.setCriteriaSearchType = Service.GAFService.SearchType.OPTIMA_EXACT_SEARCH;

                    Service.GAFService.OFT[] criteriaFields = new Service.GAFService.OFT[1];
                    criteriaFields[0] = Service.GAFService.OFT.Postcode;
                    myCriteria.addCriteriaFields = criteriaFields;

                    criteria[0] = myCriteria;

                    if (gaf.Select(clientSetting.mvnoSettings.userNameGAF, clientSetting.mvnoSettings.passWordGAF, clientSetting.mvnoSettings.countryCodeGAF.Trim(), criteria, returnFields, true, true, 1, Convert.ToInt32(clientSetting.mvnoSettings.autReturnMaxResults), out contactOutput))
                    {
                        foreach (Service.GAFService.Contact contact in contactOutput)
                        {
                            //if (!string.IsNullOrEmpty(contact.City))
                            if ((clientSetting.mvnoSettings.countryCodeGAF.Trim().ToUpper() == "HKG" && !string.IsNullOrEmpty(contact.Other10)) || (clientSetting.mvnoSettings.countryCodeGAF.Trim().ToUpper() != "HKG" && !string.IsNullOrEmpty(contact.Region)))
                            {
                                SelectListItem StateItem = new SelectListItem();

                                StateItem.Text = clientSetting.mvnoSettings.countryCodeGAF.Trim().ToUpper() == "HKG" ? contact.Other10 : contact.Region;
                                StateItem.Value = StateItem.Text;

                                lstCityItem.Add(StateItem);
                            }
                        }
                    }
                    else
                    {

                    }
                }
                return Json(lstCityItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(lstCityItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //lstCityItem = null;
            }

        }

        

        public JsonResult GAFFetchStatenCity(AddressInput inputAddressCRM)
        {
            List<SelectListItem> lstCityItem = new List<SelectListItem>();
            try
            {
                using (Service.GAFService.GlobalAddress gaf = new Service.GAFService.GlobalAddress())
                {
                    gaf.Url = SettingsCRM.crmGAFUrl;
                    Service.GAFService.Contact[] contactOutput = null;

                    Service.GAFService.OFT[] returnFields = new Service.GAFService.OFT[2];
                    //returnFields[0] = Service.GAFService.OFT.Street;
                    returnFields[0] = Service.GAFService.OFT.City;
                    returnFields[1] = Service.GAFService.OFT.Region;

                    Service.GAFService.Criteria[] criteria = new Service.GAFService.Criteria[1];
                    Service.GAFService.OFT[] criteriaFields = new Service.GAFService.OFT[1];

                    Service.GAFService.Criteria myCriteriaStreet = new Service.GAFService.Criteria();
                    myCriteriaStreet.setCriteriaValue = inputAddressCRM.postCode;
                    myCriteriaStreet.setCriteriaSearchType = Service.GAFService.SearchType.OPTIMA_EXACT_SEARCH;
                    criteriaFields[0] = Service.GAFService.OFT.Postcode;
                    myCriteriaStreet.addCriteriaFields = criteriaFields;

                    criteria[0] = myCriteriaStreet;

                    if (gaf.Select(clientSetting.mvnoSettings.userNameGAF, clientSetting.mvnoSettings.passWordGAF, clientSetting.mvnoSettings.countryCodeGAF.Trim(), criteria, returnFields, true, true, 1, Convert.ToInt32(clientSetting.mvnoSettings.autReturnMaxResults), out contactOutput))
                    {
                        int count = Convert.ToInt32(clientSetting.mvnoSettings.autReturnMaxResults) > contactOutput.Length ? contactOutput.Length : Convert.ToInt32(clientSetting.mvnoSettings.autReturnMaxResults);
                        for (int i = 0; i < count; i++)
                        {   //if (!string.IsNullOrEmpty(contact.City))
                            if (!string.IsNullOrEmpty(contactOutput[i].City))
                            {
                                SelectListItem CityName = new SelectListItem();
                                CityName.Text = contactOutput[i].City;
                                CityName.Value = contactOutput[i].City;
                                lstCityItem.Add(CityName);

                                SelectListItem StateItem = new SelectListItem();
                                StateItem.Selected = true;
                                StateItem.Text = clientSetting.mvnoSettings.countryCodeGAF.Trim().ToUpper() == "HKG" ? contactOutput[i].Other10 : contactOutput[i].Region;
                                StateItem.Value = StateItem.Text;
                                lstCityItem.Add(StateItem);
                            }
                        }
                    }
                    else
                    {

                    }
                }
                return Json(lstCityItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(lstCityItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //lstCityItem = null;
            }
            
        }
        #endregion

        public PartialViewResult IRLGAF()
        {
            IRLAddress objirladdress = new IRLAddress();

            try
            {
                objirladdress.lstcounty = Utility.GetDropdownMasterFromDB("tbl_county");
                return PartialView(objirladdress);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return PartialView(objirladdress);
            }
            finally
            {
               // objirladdress = null;
            }

        }

        # region BillingAddressGAF

        public PartialViewResult IRLBillingAddressGAF()
        {
            IRLBillingAddress objirladdress = new IRLBillingAddress();

            try
            {
                objirladdress.lstcountyBA = Utility.GetDropdownMasterFromDB("tbl_county");
                return PartialView(objirladdress);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return PartialView(objirladdress);
            }
            finally
            {
                //objirladdress = null;
            }            

        }

        public PartialViewResult BillingAddressGAF(string showpopup)
        {
            BillingAddress addressCRM = new BillingAddress();
            try
            {
                addressCRM.isShowPopupBA = string.IsNullOrEmpty(showpopup) || (!string.IsNullOrEmpty(showpopup) && showpopup.ToUpper() != "FALSE");
                return PartialView(addressCRM);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return PartialView(addressCRM);
            }
            finally
            {
               // addressCRM = null;
            }
            
        }

        # endregion

        #region *****GERMANY COUNTRY *****FRR-3785 *****USEGERGAF ON CASE *****DROPDOWN STRUCTURE*****
        public ActionResult GERGAF()
        {
            return View();
        }
        public ActionResult GERBillingAddressGAF()
        {
            return View();
        }
        public PartialViewResult GAF(string Postalcode, string Controls, string TypeID, string State, string City, string Street, string houseNumber, string houseNumberAddon, string Drop, string Norecordfound, string DivID, string EnableControls, string gencountry, string gencountry_name)
        {
            #region ****** CITY * STREET * PREMISE ***** FROM ***** POSTCODE *****
            AddressCRM addressCRM = new AddressCRM();
            try
            {
                addressCRM.postCode = Postalcode;
                addressCRM.AssignID = Controls;
                addressCRM.TypeID = TypeID;
                if (clientSetting.countryCode == "IRL")
                    addressCRM.districtName = State;
                else
                    addressCRM.stateName = State;
                addressCRM.cityName = City;
                addressCRM.streetName = Street;
                addressCRM.houseNumber = houseNumber;
                addressCRM.houseNumberAddon = houseNumberAddon;
                addressCRM.Drop = Drop;
                addressCRM.isNorecord = string.IsNullOrEmpty(Norecordfound) ? "1" : Norecordfound;
                addressCRM.DivControl = DivID;
                addressCRM.EnableControls = EnableControls;
                addressCRM.allowCountry = gencountry;
                addressCRM.allowCountryName = gencountry_name;
                return PartialView(addressCRM);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return PartialView(addressCRM);
            }
            finally
            {
                // addressCRM = null;
            }
            #endregion
        }
        public JsonResult FetchCity(AddressInput inputAddressCRM)
        {
            #region *****CITY****FROM *****POSTCODE *****
            List<SelectListItem> lstCityItem = new List<SelectListItem>();
            try
            {
                using (Service.GAFService.GlobalAddress gaf = new Service.GAFService.GlobalAddress())
                {
                    gaf.Url = SettingsCRM.crmGAFUrl;
                    Service.GAFService.Contact[] contactOutput = null;

                    Service.GAFService.OFT[] returnFields = new Service.GAFService.OFT[1];
                    returnFields[0] = Service.GAFService.OFT.City;

                    Service.GAFService.Criteria[] criteria = new Service.GAFService.Criteria[1];

                    Service.GAFService.Criteria myCriteria = new Service.GAFService.Criteria();
                    myCriteria.setCriteriaValue = inputAddressCRM.postCode;
                    myCriteria.setCriteriaSearchType = Service.GAFService.SearchType.OPTIMA_EXACT_SEARCH;

                    Service.GAFService.OFT[] criteriaFields = new Service.GAFService.OFT[1];
                    criteriaFields[0] = Service.GAFService.OFT.Postcode;
                    myCriteria.addCriteriaFields = criteriaFields;

                    criteria[0] = myCriteria;

                    if (gaf.Select(clientSetting.mvnoSettings.userNameGAF, clientSetting.mvnoSettings.passWordGAF, clientSetting.mvnoSettings.countryCodeGAF.Trim(), criteria, returnFields, true, true, 1, Convert.ToInt32(clientSetting.mvnoSettings.autReturnMaxResults), out contactOutput))
                    {
                        foreach (Service.GAFService.Contact contact in contactOutput)
                        {
                            if (!string.IsNullOrEmpty(contact.City))
                            {
                                SelectListItem cityItem = new SelectListItem();

                                cityItem.Text = contact.City;
                                cityItem.Value = contact.City;

                                lstCityItem.Add(cityItem);
                            }
                        }
                    }
                    else
                    {

                    }
                }
                return Json(lstCityItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(lstCityItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                // lstCityItem = null;
            }
            #endregion
        }
        public JsonResult FetchStreet(AddressInput inputAddressCRM)
        {
            #region *****STREET *****FROM ***** CITY*****
            List<SelectListItem> lstStreetItem = new List<SelectListItem>();
            try
            {
                using (Service.GAFService.GlobalAddress gaf = new Service.GAFService.GlobalAddress())
                {
                    gaf.Url = SettingsCRM.crmGAFUrl;
                    Service.GAFService.Contact[] contactOutput = null;

                    Service.GAFService.OFT[] returnFields = new Service.GAFService.OFT[1];
                    returnFields[0] = Service.GAFService.OFT.Street;

                    Service.GAFService.Criteria[] criteria = new Service.GAFService.Criteria[2];

                    Service.GAFService.Criteria myCriteriaCity = new Service.GAFService.Criteria();
                    myCriteriaCity.setCriteriaValue = inputAddressCRM.cityName;
                    myCriteriaCity.setCriteriaSearchType = Service.GAFService.SearchType.OPTIMA_EXACT_SEARCH;

                    Service.GAFService.Criteria myCriteriaPostCode = new Service.GAFService.Criteria();
                    myCriteriaPostCode.setCriteriaValue = inputAddressCRM.postCode;
                    myCriteriaPostCode.setCriteriaSearchType = Service.GAFService.SearchType.OPTIMA_EXACT_SEARCH;

                    Service.GAFService.OFT[] criteriaFields = new Service.GAFService.OFT[2];
                    criteriaFields[0] = Service.GAFService.OFT.City;
                    myCriteriaCity.addCriteriaFields = criteriaFields;

                    criteriaFields[1] = Service.GAFService.OFT.Postcode;
                    myCriteriaPostCode.addCriteriaFields = criteriaFields;

                    criteria[0] = myCriteriaCity;
                    criteria[1] = myCriteriaPostCode;

                    if (gaf.Select(clientSetting.mvnoSettings.userNameGAF, clientSetting.mvnoSettings.passWordGAF, clientSetting.mvnoSettings.countryCodeGAF.Trim(), criteria, returnFields, true, true, 1, Convert.ToInt32(clientSetting.mvnoSettings.autReturnMaxResults), out contactOutput))
                    {
                        foreach (Service.GAFService.Contact contact in contactOutput)
                        {
                            if (!string.IsNullOrEmpty(contact.Street))
                            {
                                SelectListItem streetItem = new SelectListItem();

                                streetItem.Text = contact.Street;
                                streetItem.Value = contact.Street;

                                lstStreetItem.Add(streetItem);
                            }
                        }
                    }
                    else
                    {

                    }
                }
                return Json(lstStreetItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(lstStreetItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                // lstStreetItem = null;
            }
#endregion
        }
        public JsonResult GAFFetchHouseNo(AddressInput inputAddressCRM)
        {
            #region ***** PREMISE[HOUSE NUMBER AND HOUSE ADDON NUMBER]***** FROM ***** STREET *****
            List<SelectListItem> lstCityItem = new List<SelectListItem>();
            try
            {
                using (Service.GAFService.GlobalAddress gaf = new Service.GAFService.GlobalAddress())
                {
                    gaf.Url = SettingsCRM.crmGAFUrl;
                    Service.GAFService.Contact[] contactOutput = null;

                    Service.GAFService.OFT[] returnFields = new Service.GAFService.OFT[1];
                    //returnFields[0] = Service.GAFService.OFT.Street;
                    returnFields[0] = Service.GAFService.OFT.Premise;

                    Service.GAFService.Criteria[] criteria = new Service.GAFService.Criteria[3];
                    Service.GAFService.OFT[] criteriaFields = new Service.GAFService.OFT[3];

                    Service.GAFService.Criteria myCriteriaStreet = new Service.GAFService.Criteria();
                    myCriteriaStreet.setCriteriaValue = inputAddressCRM.streetName;
                    myCriteriaStreet.setCriteriaSearchType = Service.GAFService.SearchType.OPTIMA_EXACT_SEARCH;
                    criteriaFields[0] = Service.GAFService.OFT.Street;
                    myCriteriaStreet.addCriteriaFields = criteriaFields;

                    Service.GAFService.Criteria myCriteriaCity = new Service.GAFService.Criteria();
                    myCriteriaCity.setCriteriaValue = inputAddressCRM.cityName;
                    myCriteriaCity.setCriteriaSearchType = Service.GAFService.SearchType.OPTIMA_EXACT_SEARCH;
                    criteriaFields[1] = Service.GAFService.OFT.City;
                    myCriteriaCity.addCriteriaFields = criteriaFields;

                    Service.GAFService.Criteria myCriteriaPostCode = new Service.GAFService.Criteria();
                    myCriteriaPostCode.setCriteriaValue = inputAddressCRM.postCode;
                    myCriteriaPostCode.setCriteriaSearchType = Service.GAFService.SearchType.OPTIMA_EXACT_SEARCH;
                    criteriaFields[2] = Service.GAFService.OFT.Postcode;
                    myCriteriaPostCode.addCriteriaFields = criteriaFields;

                    criteria[0] = myCriteriaStreet;
                    criteria[1] = myCriteriaCity;
                    criteria[2] = myCriteriaPostCode;

                    if (gaf.Select(clientSetting.mvnoSettings.userNameGAF, clientSetting.mvnoSettings.passWordGAF, clientSetting.mvnoSettings.countryCodeGAF.Trim(), criteria, returnFields, true, true, 1, Convert.ToInt32(clientSetting.mvnoSettings.autReturnMaxResults), out contactOutput))
                    {
                        foreach (Service.GAFService.Contact contact in contactOutput)
                        {
                            //if (!string.IsNullOrEmpty(contact.City))
                            if (!string.IsNullOrEmpty(contact.Premise))
                            {
                                SelectListItem HouseNo = new SelectListItem();

                                HouseNo.Text = contact.Other4 + (string.IsNullOrEmpty(contact.Other5) ? string.Empty : "," + contact.Other5);
                                HouseNo.Value = contact.Other4 + (string.IsNullOrEmpty(contact.Other5) ? string.Empty : "," + contact.Other5);


                                lstCityItem.Add(HouseNo);
                            }
                        }
                    }
                    else
                    {

                    }
                }
                return Json(lstCityItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(lstCityItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //lstCityItem = null;
            }
#endregion
        }

        #endregion

    }
}
