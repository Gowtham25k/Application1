using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CRM.Models;
using ServiceCRM;
using Newtonsoft.Json;
using System.Globalization;
using System.Web.UI.WebControls;
using System.Web;
using System.IO;
using System.Web.Management;
using System.Runtime.Remoting;

namespace CRM.Controllers
{
    [ValidateState]
    public class CustomerController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();

        public ActionResult PortIn(string PortinMsisdn , string ispostpaid)
        {
            CustomerPortin cusPortin = new CustomerPortin();
            CRMBase crmBase = new CRMBase();
            try
            {
                
                crmBase.CountryCode = clientSetting.countryCode;
                crmBase.BrandCode = clientSetting.brandCode;
                crmBase.LanguageCode = clientSetting.langCode;

                List<SPDETAILS> sp = new List<SPDETAILS>();
                sp = SP(crmBase);

                cusPortin.dicServiceProvider = sp.ToDictionary(x => x.SPCODE, x => x.SPDESC);
                cusPortin.portinMsisdn = PortinMsisdn;
                // 51000
                cusPortin.Ispostpaid = ispostpaid;


                //HolidayList
                List<HOLIDAYSLIST> HdayList = new List<HOLIDAYSLIST>();
                HdayList = dtHoliday(crmBase);

                string Dateval = string.Empty;
                for (int i = 0; i < HdayList.Count; i++)
                {
                    Dateval = Dateval + "," + HdayList[i].DATE;// HolidayDateconvert(HdayList[i].DATE, "0,1,2");
                }
                Dateval = Dateval.Replace(",0", ",").Replace("-0", "-");
                string RemoveComma = Dateval.Trim().TrimStart(',');
                cusPortin.PortinHolidayDate = RemoveComma.Trim().Replace('/', '-');
                return View(cusPortin);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return View(cusPortin);
            }
            finally
            {
               // cusPortin = null;
                crmBase = null;
            }
            
        }

        [HttpPost]
        public JsonResult CreatePortin(Portin Params)
        {
            PortinSubscriberRequest objPortinSubscriber = new PortinSubscriberRequest();
            PortinSubscriberResponse objPortinSubscriberResp = new PortinSubscriberResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objPortinSubscriber.CountryCode = clientSetting.countryCode;
                objPortinSubscriber.BrandCode = clientSetting.brandCode;
                objPortinSubscriber.LanguageCode = clientSetting.langCode;
                Params.REQUESTEDBY = Session["UserName"].ToString();
                objPortinSubscriber.Request = Params;
               // Params.PORTINDATE = Dateconvert(Params.PORTINDATE, "1,0,2");

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objPortinSubscriberResp = serviceCRM.CRMPortinSubscriber(objPortinSubscriber);
                    if (objPortinSubscriberResp != null && objPortinSubscriberResp.RETURNCODE != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Customer_Portin_" + objPortinSubscriberResp.RETURNCODE);
                        objPortinSubscriberResp.RETURNDESC = string.IsNullOrEmpty(errorInsertMsg) ? objPortinSubscriberResp.RETURNDESC : errorInsertMsg;
                    }
                
                return Json(objPortinSubscriberResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objPortinSubscriberResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objPortinSubscriber = null;
               // objPortinSubscriberResp = null;
                serviceCRM = null;
            }
            

        }
        // 51000
        public JsonResult GetPostpaidPortin(string GetPostpaidPortinRequest)
        {
            
            PortinSubscriberResponse objPortinSubscriberResp = new PortinSubscriberResponse();
            PortinSubscriberRequest objPortinSubscriber = JsonConvert.DeserializeObject<PortinSubscriberRequest>(GetPostpaidPortinRequest);
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objPortinSubscriber.CountryCode = clientSetting.countryCode;
                objPortinSubscriber.BrandCode = clientSetting.brandCode;
                objPortinSubscriber.LanguageCode = clientSetting.langCode;
               // Params.REQUESTEDBY = Session["UserName"].ToString();
                //objPortinSubscriber.Request = Params;
               // Params.PORTINDATE = Dateconvert(Params.PORTINDATE, "1,0,2");

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objPortinSubscriberResp = serviceCRM.CRMPortinSubscriber(objPortinSubscriber);
                    if (objPortinSubscriberResp != null && objPortinSubscriberResp.RETURNCODE != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Customer_Portin_" + objPortinSubscriberResp.RETURNCODE);
                        objPortinSubscriberResp.RETURNDESC = string.IsNullOrEmpty(errorInsertMsg) ? objPortinSubscriberResp.RETURNDESC : errorInsertMsg;
                    }
                
                return Json(objPortinSubscriberResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objPortinSubscriberResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objPortinSubscriber = null;
                // objPortinSubscriberResp = null;
                serviceCRM = null;
            }


        }

        [HttpPost]
        public List<SPDETAILS> SP(CRMBase crmBaseReq)
        {
            PortinSubscriberRequest objPortinSubscriber = new PortinSubscriberRequest();
            ServiceProviderResponse serviceProviderResp = new ServiceProviderResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    serviceProviderResp = serviceCRM.CRMMNPServiceProvider(crmBaseReq);

                    if (serviceProviderResp != null && serviceProviderResp.ResponseDetails != null && serviceProviderResp.ResponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Customer_Portin_SP_" + serviceProviderResp.ResponseDetails.ResponseCode);
                        serviceProviderResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? serviceProviderResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }

                
                return serviceProviderResp.ServiceProvider;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return serviceProviderResp.ServiceProvider;
            }
            finally
            {
                objPortinSubscriber = null;
                //serviceProviderResp = null;
                serviceCRM = null;
            }
            
        }


        string splitYear, splitTime, splitMeridian;
        private string Dateconvert(string DateValue, string Index = "1,0,2")
        {
            string Date = string.Empty;
            string Month = string.Empty;
            string Year = string.Empty;
            string time = string.Empty;
            string[] Indexsp = Index.Split(',');
            string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            if (DateValue != null)
            {

                string[] SplitDOB = DateValue.Replace("  ", " ").Split('-', '/', ' ');
                Date = SplitDOB[Convert.ToInt16(Indexsp[0])].ToString();
                if (Date.Length != 2)
                {
                    Date = "0" + Date;
                }
                Month = SplitDOB[Convert.ToInt16(Indexsp[1])].ToString();
                if (Month.Length != 2)
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
                    DateValue = strInputDate.Replace("dd", Date).Replace("mm", Month).Replace("yyyy", Year);
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

                    DateValue = strInputDate.Replace("dd", Date).Replace("mm", Month).Replace("yyyy", Year);
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



            return DateValue;
        }

        private string HolidayDateconvert(string DateValue, string Index = "1,0,2")
        {
            string Date = string.Empty;
            string Month = string.Empty;
            string Year = string.Empty;
            string time = string.Empty;
            string[] Indexsp = Index.Split(',');
            string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            if (DateValue != null)
            {

                string[] SplitDOB = DateValue.Replace("  ", " ").Split('-', '/', ' ');
                Date = SplitDOB[Convert.ToInt16(Indexsp[0])].ToString();
                if (Date.StartsWith("0"))
                {
                    Date = Date.Remove(0, 1);
                }
                Month = SplitDOB[Convert.ToInt16(Indexsp[1])].ToString();
                if (Month.StartsWith("0"))
                {
                    Month = Month.Remove(0, 1);
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
                    DateValue = strInputDate.Replace("dd", Date).Replace("mm", Month).Replace("yyyy", Year);
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

                    DateValue = strInputDate.Replace("dd", Date).Replace("mm", Month).Replace("yyyy", Year);
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



            return DateValue;
        }

        [HttpPost]
        public List<HOLIDAYSLIST> dtHoliday(CRMBase BaseReq)
        {
            PortinSubscriberRequest objPSubscriber = new PortinSubscriberRequest();
            HolidayListResponse holidayRes = new HolidayListResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    holidayRes = serviceCRM.CRMMNPHolidayList(BaseReq);
                
                return holidayRes.HolidayList;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return holidayRes.HolidayList;
            }
            finally
            {
                objPSubscriber = null;
              //  holidayRes = null;
                serviceCRM = null;
            }
            
        }

        public ActionResult BELPortIn(string PortinMsisdn , string ispostpaid)
        {
            // 51000
            PortinBelgiumResponse cusPortin = new PortinBelgiumResponse();
            CRMBase crmBase = new CRMBase();
            List<SPDETAILS> sp = new List<SPDETAILS>();
            try
            {
               
                crmBase.CountryCode = clientSetting.countryCode;
                crmBase.BrandCode = clientSetting.brandCode;
                crmBase.LanguageCode = clientSetting.langCode;

                
                sp = SP(crmBase);

                if (sp.Count > 0)
                {
                    ServiceProvider objService = null;
                    foreach (SPDETAILS str in sp)
                    {
                        objService = new ServiceProvider();
                        objService.spcode = str.SPCODE;
                        objService.spdesc = str.SPDESC;
                        cusPortin.serviceProvider.Add(objService);
                    }
                }

                cusPortin.portinMsisdn = PortinMsisdn;
                cusPortin.ispostpaid = ispostpaid;

                //HolidayList
                List<HOLIDAYSLIST> HdayList = new List<HOLIDAYSLIST>();
                HdayList = dtHoliday(crmBase);

                string Dateval = string.Empty;
                for (int i = 0; i < HdayList.Count; i++)
                {
                    Dateval = Dateval + "," + HdayList[i].DATE;// HolidayDateconvert(HdayList[i].DATE, "0,1,2");
                }
                Dateval = Dateval.Replace(",0", ",").Replace("-0", "-");
                string RemoveComma = Dateval.Trim().TrimStart(',');
                cusPortin.PortinHolidayDate = RemoveComma.Trim().Replace('/', '-');

                cusPortin.portInPrefLang = PortInPrefLang(crmBase);

                if (!string.IsNullOrEmpty(clientSetting.preSettings.internalServiceProvider))
                {
                    try
                    {
                        List<string> lstResp = new List<string>(clientSetting.preSettings.internalServiceProvider.Split('|'));
                        List<string> lstText = new List<string>(lstResp[0].Split(','));
                        List<string> lstValue = new List<string>(lstResp[1].Split(','));
                        if (lstText.Count > 0 && lstValue.Count > 0)
                        {
                            InternalServiceProvider objInternal = null;
                            cusPortin.internalServiceProvider = new List<InternalServiceProvider>();
                            for (int i = 0; i < lstText.Count; i++)
                            {
                                string dummystring = System.Text.RegularExpressions.Regex.Replace(lstText[i], "[^0-9a-zA-Z]+", string.Empty);
                                string ResourceMsg = Resources.DropdownResources.ResourceManager.GetString(dummystring);
                                lstText[i] = string.IsNullOrEmpty(ResourceMsg) ? lstText[i] : ResourceMsg;
                                objInternal = new InternalServiceProvider();
                                objInternal.internalServiceProvText = lstText[i];
                                objInternal.internalServiceProvValue = lstValue[i];
                                cusPortin.internalServiceProvider.Add(objInternal);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                return View(cusPortin);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return View(cusPortin);
            }
            finally
            {
                //cusPortin = null;
                crmBase = null;
                sp = null;
            }
            
        }

        [HttpPost]
        public JsonResult BELCreatePortin(string CreatePortIn)
        {
            PortinBelgiumRequest objreq = JsonConvert.DeserializeObject<PortinBelgiumRequest>(CreatePortIn);
            PortinBelgiumResponse objRes = new PortinBelgiumResponse();
            //s
            ServiceInvokeCRM serviceCRM = null;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.userName = Convert.ToString(Session["UserName"]);
                try
                {
                    objreq.portInDate = Utility.GetDateconvertion(objreq.portInDate, clientSetting.mvnoSettings.dateTimeFormat, false, clientSetting.preSettings.portInDateFormat);
                }
                catch
                {
                    objreq.portInDate = Utility.GetDateconvertion(objreq.portInDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy-mm-dd");
                }

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.CRMPortinBelgium(objreq);

                    if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BELPortIn_" + objRes.responseDetails.ResponseCode);
                        objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
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
                //objRes = null;
                objreq = null;
                serviceCRM = null;
            }           

        }

        [HttpPost]
        public JsonResult OTPGeneration(string OTPPortIn)
        {
            MNPOTPRequest objreq = JsonConvert.DeserializeObject<MNPOTPRequest>(OTPPortIn);
            MNPOTPResponse objRes = new MNPOTPResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.CRMMnpOtp(objreq);
                
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

        public List<PortInLang> PortInPrefLang(CRMBase crmBaseReq)
        {
            IVRLanguageResponse ObjRes = new IVRLanguageResponse();
            PortInLang objlang = null;
            List<PortInLang> lang = new List<PortInLang>();
            //s
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
                ObjRes = null;
                objlang = null;
               // lang = null;
                serviceCRM = null;
            }
            
        }

        public Tuple<string, string> BrazilAddPortInDays(int NoOfDays, DateTime dtToday, List<DateTime> HolidayDate)
        {
            string PortInSelectedDate = string.Empty, WeekEndDays = string.Empty;
            string DayOfWeek = string.Empty, MinDate = string.Empty, MinYear = string.Empty, MinMonth = string.Empty;
            DayOfWeek dow = new DayOfWeek();
            DateTime PortInDate = new DateTime();
            int AddDays = 0;
            try
            {
                PortInDate = dtToday;
                for (int i = 0; i < NoOfDays; i++)
                {
                    dtToday = dtToday.AddDays(1);
                    dow = dtToday.DayOfWeek;
                    if (dow.ToString() == "Sunday" || dow.ToString() == "Saturday")
                    {
                        AddDays = AddDays + 1;
                    }
                    else
                    {
                        if (HolidayDate.Count > 0)
                        {
                            if (CheckDate(HolidayDate, dtToday))
                            {
                                AddDays = AddDays + 1;
                            }
                        }
                    }
                }

                PortInDate = PortInDate.AddDays(AddDays + NoOfDays);
                dow = PortInDate.DayOfWeek;

                if (dow.ToString() == "Sunday")
                    PortInDate = PortInDate.AddDays(1);

                if (dow.ToString() == "Saturday")
                    PortInDate = PortInDate.AddDays(2);

                # region CheckDate

                if (HolidayDate.Count > 0)
                {
                    if (CheckDate(HolidayDate, PortInDate))
                    {
                        PortInDate = PortInDate.AddDays(1);
                        dow = PortInDate.DayOfWeek;

                        if (dow.ToString() == "Sunday")
                            PortInDate = PortInDate.AddDays(1);

                        if (dow.ToString() == "Saturday")
                            PortInDate = PortInDate.AddDays(2);

                        if (CheckDate(HolidayDate, PortInDate))
                        {
                            PortInDate = PortInDate.AddDays(1);
                            dow = PortInDate.DayOfWeek;

                            if (dow.ToString() == "Sunday")
                                PortInDate = PortInDate.AddDays(1);

                            if (dow.ToString() == "Saturday")
                                PortInDate = PortInDate.AddDays(2);

                            if (CheckDate(HolidayDate, PortInDate))
                            {
                                PortInDate = PortInDate.AddDays(1);
                                dow = PortInDate.DayOfWeek;

                                if (dow.ToString() == "Sunday")
                                    PortInDate = PortInDate.AddDays(1);

                                if (dow.ToString() == "Saturday")
                                    PortInDate = PortInDate.AddDays(2);

                                if (CheckDate(HolidayDate, PortInDate))
                                {
                                    PortInDate = PortInDate.AddDays(1);
                                    dow = PortInDate.DayOfWeek;

                                    if (dow.ToString() == "Sunday")
                                        PortInDate = PortInDate.AddDays(1);

                                    if (dow.ToString() == "Saturday")
                                        PortInDate = PortInDate.AddDays(2);
                                }
                            }
                        }
                    }
                }

                # endregion

                MinDate = Convert.ToString(PortInDate.Day);
                if (MinDate.Length == 1)
                    MinDate = "0" + MinDate;
                MinYear = Convert.ToString(PortInDate.Year);
                MinMonth = Convert.ToString(PortInDate.ToString("MM"));
                PortInSelectedDate = MinYear + '/' + MinMonth + '/' + MinDate;
                PortInSelectedDate = Utility.GetDateconvertion(PortInSelectedDate, "yyyy/mm/dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                WeekEndDays = "1";
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }

            return new Tuple<string, string>(PortInSelectedDate, WeekEndDays);
        }

        public Tuple<string, string> PortInDate(List<HOLIDAYSLIST> HolidayList)
        {
            string PortInSelectedDate = string.Empty, WeekEndDays = string.Empty;
            DateTime dtToday = System.DateTime.Now;
            string replace = string.Empty;
            DateTime dt = new DateTime();
            try
            {
                List<DateTime> objDate = new List<DateTime>();
                if (HolidayList.Count > 0)
                {
                    foreach (HOLIDAYSLIST date in HolidayList)
                    {
                        bool contains = date.DATE.Contains("-");
                        if (contains)
                        {
                            replace = date.DATE.Replace('-', '/');
                            replace = GetDateconvertion(replace, "dd/mm/yyyy", false, "yyyy/mm/dd");

                            try
                            {
                                dt = DateTime.ParseExact(replace, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                            }
                            catch
                            {
                                dt = Convert.ToDateTime(replace);
                            }
                        }

                        objDate.Add(dt);
                    }
                }

                if (!string.IsNullOrEmpty(clientSetting.mvnoSettings.BrazilPortInDisplayDate))
                {
                    var OutputData = BrazilAddPortInDays(Convert.ToInt32(clientSetting.mvnoSettings.BrazilPortInDisplayDate), dtToday, objDate);
                    PortInSelectedDate = OutputData.Item1.ToString();
                    WeekEndDays = OutputData.Item2.ToString();
                }
                else
                {
                    var OutputData = BrazilAddPortInDays(0, dtToday, objDate);
                    PortInSelectedDate = OutputData.Item1.ToString();
                    WeekEndDays = OutputData.Item2.ToString();
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }

            return new Tuple<string, string>(PortInSelectedDate, WeekEndDays);
        }

        public bool CheckDate(List<DateTime> DateCheck, DateTime CurrentDate)
        {
            bool Return = false;

            try
            {
                Return = DateCheck.Any(d => d.Month == CurrentDate.Month && d.Day == CurrentDate.Day && d.Year == CurrentDate.Year);
            }
            catch (Exception)
            {
            }
            return Return;
        }

        public static string GetDateconvertion(string strDate, string strIOdate, Boolean isRequest, string dateTimeFormat)
        {
            string hhmmss = "";
            try
            {
                if (!string.IsNullOrEmpty(strDate))
                {
                    string strInputDate = dateTimeFormat;

                    var idx = strDate.IndexOf(' ');
                    //if hhmmss mentioned in strIOdate
                    if (idx != -1)
                    {
                        string[] splitDate = strDate.Split(' ');
                        strDate = splitDate[0];
                        hhmmss = splitDate[1];
                        string[] splitIOdate = strIOdate.Split(' ');
                        strIOdate = splitIOdate[0];
                    }

                    //Swapping when input from response
                    if (!isRequest)
                    {
                        string strSwap = strInputDate;
                        strInputDate = strIOdate;
                        strIOdate = strSwap;
                    }

                    if (strDate.Length == strInputDate.Length)
                    {
                        strIOdate = strIOdate.ToUpper();
                        if (strIOdate == strInputDate.ToUpper())
                        {
                            return strDate;
                        }
                        Int64 IValidate = 0;
                        string strValidateDate = strDate.Replace("/", "");
                        strValidateDate = strValidateDate.Replace("-", "");
                        string strDay, strMonth, strYear;
                        strDay = strDate.Substring(strInputDate.ToUpper().LastIndexOf("DD"), 2);
                        strMonth = strDate.Substring(strInputDate.ToUpper().LastIndexOf("MM"), 2);
                        strYear = strDate.Substring(strInputDate.ToUpper().LastIndexOf("YYYY"), 4);

                        strIOdate = strIOdate.ToUpper();
                        strIOdate = strIOdate.Replace("DD", strDay.Length == 1 ? "0" + strDay : strDay);
                        strIOdate = strIOdate.Replace("MM", strMonth.Length == 1 ? "0" + strMonth : strMonth);
                        strIOdate = strIOdate.Replace("YYYY", strYear);

                        if (idx != -1)
                        {
                            return strIOdate + " " + hhmmss;
                        }
                        else
                        {
                            return strIOdate;
                        }

                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                strDate = null;
                strIOdate = null;
                dateTimeFormat = null;
            }
            if (string.IsNullOrEmpty(strDate))
                return string.Empty;
            else
            {
                return "";
            }

        }

        public ActionResult BRAPortIn(string PortinMsisdn)
        {
            PortinBrazilResponse cusPortin = new PortinBrazilResponse();
            CRMBase crmBase = new CRMBase();
            List<SPDETAILS> sp = new List<SPDETAILS>();
            List<HOLIDAYSLIST> HdayList = new List<HOLIDAYSLIST>();
            try
            {
                
                crmBase.CountryCode = clientSetting.countryCode;
                crmBase.BrandCode = clientSetting.brandCode;
                crmBase.LanguageCode = clientSetting.langCode;                
                sp = SP(crmBase);

                if (sp.Count > 0)
                {
                    ServiceProvider objService = null;
                    foreach (SPDETAILS str in sp)
                    {
                        objService = new ServiceProvider();
                        objService.spcode = str.SPCODE;
                        objService.spdesc = str.SPDESC;
                        cusPortin.serviceProvider.Add(objService);
                    }
                }

                cusPortin.portinMsisdn = PortinMsisdn;
                //HolidayList                
                HdayList = dtHoliday(crmBase);

                //string Dateval = string.Empty;
                //for (int i = 0; i < HdayList.Count; i++)
                //{
                //    Dateval = Dateval + "," + HdayList[i].DATE;// HolidayDateconvert(HdayList[i].DATE, "0,1,2");
                //}
                //Dateval = Dateval.Replace(",0", ",").Replace("-0", "-");
                //string RemoveComma = Dateval.Trim().TrimStart(',');
                //cusPortin.PortinHolidayDate = RemoveComma.Trim().Replace('/', '-');

                var PortDate = PortInDate(HdayList);
                cusPortin.PortInSelectedDate = PortDate.Item1.ToString();
                cusPortin.WeekEndDays = PortDate.Item2.ToString();
                return View(cusPortin);

            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return View(cusPortin);
            }
            finally
            {
               // cusPortin = null;
                crmBase = null;
                sp = null;
                HdayList = null;
            }
            
        }

        [HttpPost]
        public JsonResult BRACreatePortin(string CreatePortIn)
        {
            PortinBrazilRequest objreq = JsonConvert.DeserializeObject<PortinBrazilRequest>(CreatePortIn);
            PortinBrazilResponse objRes = new PortinBrazilResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.UserName = Convert.ToString(Session["UserName"]);

                if (objreq.Mode == "Portin" && !string.IsNullOrEmpty(objreq.PortInDate))
                {
                    try
                    {
                        objreq.PortInDate = Utility.GetDateconvertion(objreq.PortInDate, clientSetting.mvnoSettings.dateTimeFormat, false, clientSetting.preSettings.portInDateFormat);
                    }
                    catch
                    {
                        objreq.PortInDate = Utility.GetDateconvertion(objreq.PortInDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy-mm-dd");
                    }
                }

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.CRMPortinBrazil(objreq);

                    if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BELPortIn_" + objRes.responseDetails.ResponseCode);
                        objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
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
                //objRes = null;
                serviceCRM = null;
            }

        }

        public ContentResult CheckMigrationWindow(string MigrationDate)
        {
            string WeekEndDays = string.Empty, replace = string.Empty;
            DateTime dt = new DateTime();
            DateTime dtDate = new DateTime();
            try
            {
                CRMBase crmBase = new CRMBase();
                crmBase.CountryCode = clientSetting.countryCode;
                crmBase.BrandCode = clientSetting.brandCode;
                crmBase.LanguageCode = clientSetting.langCode;

                //HolidayList
                List<HOLIDAYSLIST> HdayList = new List<HOLIDAYSLIST>();
                HdayList = dtHoliday(crmBase);

                List<DateTime> objHolidayDate = new List<DateTime>();
                if (HdayList.Count > 0)
                {
                    foreach (HOLIDAYSLIST date in HdayList)
                    {
                        bool contains = date.DATE.Contains("-");
                        if (contains)
                        {
                            replace = date.DATE.Replace('-', '/');
                            replace = GetDateconvertion(replace, "dd/mm/yyyy", false, "yyyy/mm/dd");

                            try
                            {
                                dt = DateTime.ParseExact(replace, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                            }
                            catch
                            {
                                dt = Convert.ToDateTime(replace);
                            }
                        }
                        objHolidayDate.Add(dt);
                    }

                    try
                    {
                        MigrationDate = Utility.GetDateconvertion(MigrationDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy/mm/dd");
                        dtDate = DateTime.ParseExact(MigrationDate, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                        if (CheckDate(objHolidayDate, dtDate))
                        {
                            WeekEndDays = "2";
                        }
                        else
                        {
                            WeekEndDays = "1";
                        }
                    }
                    catch
                    {
                    }

                }
                else
                {
                    WeekEndDays = "1";
                }

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Content(WeekEndDays);
        }
        public ActionResult ITAPortIn(string PortinMsisdn)
        {
            PortInITAResponse portres = new PortInITAResponse();
            CRMBase crmBase = new CRMBase();
            List<SPDETAILS> sp = new List<SPDETAILS>();
            //5022
            List<Documentfields> documentfields = new List<Documentfields>();

        Documentfields documentfield = null;

            try
            {                
                crmBase.CountryCode = clientSetting.countryCode;
                crmBase.BrandCode = clientSetting.brandCode;
                crmBase.LanguageCode = clientSetting.langCode;                
                sp = SP(crmBase);
                //5022

                

                string[] fields;
                string[] fieldtype;
                string[] fieldvalue;
                fields = clientSetting.preSettings.PortinDocumentFields.Split(',');
                fieldtype = fields[0].Split('|');
                fieldvalue = fields[1].Split('|');
                for (var i = 0; fieldtype.Count() > i;  i++)
                {
                    if (fieldvalue.Count() > i)
                    {
                        documentfield = new Documentfields();
                        documentfield.fields = fieldtype[i];
                        documentfield.value = fieldvalue[i];
                        documentfields.Add(documentfield);
                    }
                }
                portres.documentfields = documentfields;

                if (sp.Count > 0)
                {
                    ServiceProvider objService = null;
                    foreach (SPDETAILS str in sp)
                    {
                        objService = new ServiceProvider();
                        objService.spcode = str.SPCODE;
                        objService.spdesc = str.SPDESC;
                        portres.serviceProvider.Add(objService);
                    }
                }
                portres.portinMsisdn = PortinMsisdn;
                List<HOLIDAYSLIST> HdayList = new List<HOLIDAYSLIST>();
                HdayList = dtHoliday(crmBase);
                string Dateval = string.Empty;
                for (int i = 0; i < HdayList.Count; i++)
                {
                    Dateval = Dateval + "," + HdayList[i].DATE;// HolidayDateconvert(HdayList[i].DATE, "0,1,2");
                }
                Dateval = Dateval.Replace(",0", ",").Replace("-0", "-");
                string RemoveComma = Dateval.Trim().TrimStart(',');
                portres.PortinHolidayDate = RemoveComma.Trim().Replace('/', '-');

                portres.portInPrefLang = PortInPrefLang(crmBase);
                if (!string.IsNullOrEmpty(clientSetting.preSettings.internalServiceProvider))
                {
                    try
                    {
                        List<string> lstResp = new List<string>(clientSetting.preSettings.internalServiceProvider.Split('|'));
                        List<string> lstText = new List<string>(lstResp[0].Split(','));
                        List<string> lstValue = new List<string>(lstResp[1].Split(','));
                        if (lstText.Count > 0 && lstValue.Count > 0)
                        {
                            InternalServiceProvider objInternal = null;
                            portres.internalServiceProvider = new List<InternalServiceProvider>();
                            for (int i = 0; i < lstText.Count; i++)
                            {
                                string dummystring = System.Text.RegularExpressions.Regex.Replace(lstText[i], "[^0-9a-zA-Z]+", string.Empty);
                                string ResourceMsg = Resources.DropdownResources.ResourceManager.GetString(dummystring);
                                lstText[i] = string.IsNullOrEmpty(ResourceMsg) ? lstText[i] : ResourceMsg;
                                objInternal = new InternalServiceProvider();
                                objInternal.internalServiceProvText = lstText[i];
                                objInternal.internalServiceProvValue = lstValue[i];
                                portres.internalServiceProvider.Add(objInternal);
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                return View(portres);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return View(portres);
            }
            finally
            {
              //  portres = null;
                crmBase = null;
                sp = null;
            }
            
        }
      //  public JsonResult ITACreatePortin(PortInITARequest objreq)

        public JsonResult ITACreatePortin(PortInITARequest objreq)
        {
            //5022
           
            //PortInITARequest objreq = JsonConvert.DeserializeObject<PortInITARequest>(CreatePortIn);
            PortInITAResponse objres = new PortInITAResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.UserName = Convert.ToString(Session["UserName"]);
                objreq.IsRootUser = Convert.ToString(Session["IsRootUser"]);
                objreq.ATR_ID = Convert.ToString(Session["ATR_ID"]);
                //5022
                if (objreq.mode == "File")
                {
                    if (clientSetting.preSettings.SimswapandportinvalidationforItaly.ToUpper() == "TRUE")
                    {
                        if (clientSetting.preSettings.Documentdisplayandmandate.Split('|')[3] == "1")
                        {
                            HttpPostedFileBase additionaldocu1 = Request.Files["additional"];
                            objreq.additionaldocufilename = additionaldocu1.FileName;
                            byte[] AdditionaldocuBytes = new byte[additionaldocu1.ContentLength];
                            using (BinaryReader theReader = new BinaryReader(additionaldocu1.InputStream))
                            {
                                AdditionaldocuBytes = theReader.ReadBytes(additionaldocu1.ContentLength);
                            }
                            objreq.additionaldocu = Convert.ToBase64String(AdditionaldocuBytes);
                        }
                        if (clientSetting.preSettings.Documentdisplayandmandate.Split('|')[0] == "1")
                        {
                            HttpPostedFileBase simcopy1 = Request.Files["simcopy"];
                            objreq.simcopyfilename = simcopy1.FileName;
                            byte[] simcopyBytes = new byte[simcopy1.ContentLength];
                            using (BinaryReader theReader = new BinaryReader(simcopy1.InputStream))
                            {
                                simcopyBytes = theReader.ReadBytes(simcopy1.ContentLength);
                            }
                            objreq.simcopy = Convert.ToBase64String(simcopyBytes);
                        }
                        if (clientSetting.preSettings.Documentdisplayandmandate.Split('|')[1] == "1")
                        {
                            HttpPostedFileBase taxcopy1 = Request.Files["taxcopy"];
                            objreq.taxcopyfilename = taxcopy1.FileName;
                            byte[] taxBytes = new byte[taxcopy1.ContentLength];
                            using (BinaryReader theReader = new BinaryReader(taxcopy1.InputStream))
                            {
                                taxBytes = theReader.ReadBytes(taxcopy1.ContentLength);
                            }
                            objreq.taxcopy = Convert.ToBase64String(taxBytes);
                        }
                        if (clientSetting.preSettings.Documentdisplayandmandate.Split('|')[2] == "1")
                        {
                            HttpPostedFileBase mnpform1 = Request.Files["mnpform"];
                            objreq.mnpformfilename = mnpform1.FileName;
                            byte[] mnpformBytes = new byte[mnpform1.ContentLength];
                            using (BinaryReader theReader = new BinaryReader(mnpform1.InputStream))
                            {
                                mnpformBytes = theReader.ReadBytes(mnpform1.ContentLength);
                            }
                            objreq.mnpform = Convert.ToBase64String(mnpformBytes);
                        }
                    }
                }
                //
                try
                {
                    objreq.PORTINDATE = Utility.GetDateconvertion(objreq.PORTINDATE, clientSetting.mvnoSettings.dateTimeFormat, false, clientSetting.preSettings.portInDateFormat);


                    if (objreq.mode == "get tax")
                    {
                        objreq.CustomerDOB = Utility.GetDateconvertion(objreq.CustomerDOB, clientSetting.mvnoSettings.dateTimeFormat, false, "MM/DD/YYYY");
                    }
                    else
                    {
                        objreq.CustomerDOB = Utility.GetDateconvertion(objreq.CustomerDOB, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy-mm-dd");
                    }
                }
                catch
                {
                    objreq.PORTINDATE = Utility.GetDateconvertion(objreq.PORTINDATE, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy-mm-dd");
                }

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objres = serviceCRM.PortInITA(objreq);

                    if (objres != null && objres.responseDetails != null && objres.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BELPortIn_" + objres.responseDetails.ResponseCode);
                        objres.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objres.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                
                return Json(objres);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objres);
            }
            finally
            {
                objreq = null;
              //  objres = null;
                serviceCRM = null;
            }
            

        }

        //3884
          /// <summary>
          /// 
          /// </summary>
          /// <param name="PortinMsisdn"></param>
          /// <returns></returns>
 
        //5022
      public ActionResult CancelPortinRequest()
        {
            return View("CancelPortinRequest");
        }


        public JsonResult Cancelportinreq(Cancelportinreq Objreq)
        {
            cancelportinresponse Objresp = new cancelportinresponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                Objreq.CountryCode = clientSetting.countryCode;
                Objreq.BrandCode = clientSetting.brandCode;
                Objreq.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - Cancelswap Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                Objresp = serviceCRM.CRMCancelportin(Objreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "SIMController - Cancelswap End");

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "SIMController - exception-CancelswapResponse - " + this.ControllerContext, ex);

            }
            finally
            {
                Objreq = null;
            }

            return Json(Objresp, JsonRequestBehavior.AllowGet);
        }



        public ActionResult SWEPortIn(string PortinMsisdn)
        {
            PortInSWEResponse portres = new PortInSWEResponse();
            CRMBase crmBase = new CRMBase();
            List<SPDETAILS> sp = new List<SPDETAILS>();
            try
            {
                
                crmBase.CountryCode = clientSetting.countryCode;
                crmBase.BrandCode = clientSetting.brandCode;
                crmBase.LanguageCode = clientSetting.langCode;
                
                sp = SP(crmBase);

                if (sp.Count > 0)
                {
                    ServiceProvider objService = null;
                    foreach (SPDETAILS str in sp)
                    {
                        objService = new ServiceProvider();
                        objService.spcode = str.SPCODE;
                        objService.spdesc = str.SPDESC;
                        portres.serviceProvider.Add(objService);
                    }
                }
                portres.portinMsisdn = PortinMsisdn;
                List<HOLIDAYSLIST> HdayList = new List<HOLIDAYSLIST>();
                HdayList = dtHoliday(crmBase);
                string Dateval = string.Empty;
                for (int i = 0; i < HdayList.Count; i++)
                {
                    Dateval = Dateval + "," + HdayList[i].DATE;// HolidayDateconvert(HdayList[i].DATE, "0,1,2");
                }
                Dateval = Dateval.Replace(",0", ",").Replace("-0", "-");
                string RemoveComma = Dateval.Trim().TrimStart(',');
                portres.PortinHolidayDate = RemoveComma.Trim().Replace('/', '-');

                portres.portInPrefLang = PortInPrefLang(crmBase);
                if (!string.IsNullOrEmpty(clientSetting.preSettings.internalServiceProvider))
                {
                    try
                    {
                        List<string> lstResp = new List<string>(clientSetting.preSettings.internalServiceProvider.Split('|'));
                        List<string> lstText = new List<string>(lstResp[0].Split(','));
                        List<string> lstValue = new List<string>(lstResp[1].Split(','));
                        if (lstText.Count > 0 && lstValue.Count > 0)
                        {
                            InternalServiceProvider objInternal = null;
                            portres.internalServiceProvider = new List<InternalServiceProvider>();
                            for (int i = 0; i < lstText.Count; i++)
                            {
                                string dummystring = System.Text.RegularExpressions.Regex.Replace(lstText[i], "[^0-9a-zA-Z]+", string.Empty);
                                string ResourceMsg = Resources.DropdownResources.ResourceManager.GetString(dummystring);
                                lstText[i] = string.IsNullOrEmpty(ResourceMsg) ? lstText[i] : ResourceMsg;
                                objInternal = new InternalServiceProvider();
                                objInternal.internalServiceProvText = lstText[i];
                                objInternal.internalServiceProvValue = lstValue[i];
                                portres.internalServiceProvider.Add(objInternal);
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                return View(portres);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return View(portres);
            }
            finally
            {
               // portres = null;
                crmBase = null;
                sp = null;
            }
            
        }
        public JsonResult SWECreatePortin(string CreatePortIn)
        {
            PortInSWERequest objreq = JsonConvert.DeserializeObject<PortInSWERequest>(CreatePortIn);
            PortInSWEResponse objres = new PortInSWEResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.UserName = Convert.ToString(Session["UserName"]);


                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                //CreatePortIn.REQUESTEDBY = Session["UserName"].ToString();
                //objreq.Request = CreatePortIn;
                //Params.PORTINDATE = Dateconvert(CreatePortIn.PORTINDATE, "1,0,2");                    

                objreq.PORTINDATE = Utility.GetDateconvertion(objreq.PORTINDATE, clientSetting.mvnoSettings.dateTimeFormat, false, clientSetting.preSettings.portInDateFormat);
                objreq.CustomerDOB = Utility.GetDateconvertion(objreq.CustomerDOB, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy-mm-dd");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objres = serviceCRM.PortInSWE(objreq);
                    if (objres != null && objres.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Customer_Portin_" + objres.responseDetails.ResponseCode);
                        objres.responseDetails.ResponseCode = string.IsNullOrEmpty(errorInsertMsg) ? objres.responseDetails.ResponseCode : errorInsertMsg;
                    }
                
                return Json(objres);
             }
            
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(objres);
            }
            finally
            {
                objreq = null;
               // objres = null;
                serviceCRM = null;
            }
            

        }

        //3884
        public ActionResult AUSPortIn(string PortinMsisdn)
        {
            PortInITAResponse portres = new PortInITAResponse();
            CRMBase crmBase = new CRMBase();
            List<SPDETAILS> sp = new List<SPDETAILS>();
            try
            {                
                crmBase.CountryCode = clientSetting.countryCode;
                crmBase.BrandCode = clientSetting.brandCode;
                crmBase.LanguageCode = clientSetting.langCode;                
                sp = SP(crmBase);
                if (sp.Count > 0)
                {
                    ServiceProvider objService = null;
                    foreach (SPDETAILS str in sp)
                    {
                        objService = new ServiceProvider();
                        objService.spcode = str.SPCODE;
                        objService.spdesc = str.SPDESC;
                        portres.serviceProvider.Add(objService);
                    }
                }
                ViewBag.CurrentTime = DateTime.Now.ToString("HHmm");
                portres.portinMsisdn = PortinMsisdn;
                List<HOLIDAYSLIST> HdayList = new List<HOLIDAYSLIST>();
                HdayList = dtHoliday(crmBase);
                string Dateval = string.Empty;
                for (int i = 0; i < HdayList.Count; i++)
                {
                    Dateval = Dateval + "," + HdayList[i].DATE;// HolidayDateconvert(HdayList[i].DATE, "0,1,2");
                }
                Dateval = Dateval.Replace(",0", ",").Replace("-0", "-");
                string RemoveComma = Dateval.Trim().TrimStart(',');
                portres.PortinHolidayDate = RemoveComma.Trim().Replace('/', '-');

                portres.portInPrefLang = PortInPrefLang(crmBase);
                if (!string.IsNullOrEmpty(clientSetting.preSettings.internalServiceProvider))
                {
                    try
                    {
                        List<string> lstResp = new List<string>(clientSetting.preSettings.internalServiceProvider.Split('|'));
                        List<string> lstText = new List<string>(lstResp[0].Split(','));
                        List<string> lstValue = new List<string>(lstResp[1].Split(','));
                        if (lstText.Count > 0 && lstValue.Count > 0)
                        {
                            InternalServiceProvider objInternal = null;
                            portres.internalServiceProvider = new List<InternalServiceProvider>();
                            for (int i = 0; i < lstText.Count; i++)
                            {
                                string dummystring = System.Text.RegularExpressions.Regex.Replace(lstText[i], "[^0-9a-zA-Z]+", string.Empty);
                                string ResourceMsg = Resources.DropdownResources.ResourceManager.GetString(dummystring);
                                lstText[i] = string.IsNullOrEmpty(ResourceMsg) ? lstText[i] : ResourceMsg;
                                objInternal = new InternalServiceProvider();
                                objInternal.internalServiceProvText = lstText[i];
                                objInternal.internalServiceProvValue = lstValue[i];
                                portres.internalServiceProvider.Add(objInternal);
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                return View(portres);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return View(portres);
            }
            finally
            {
               // portres = null;
                crmBase = null;
                sp = null;
            }
            
        }

        [HttpPost]
        public JsonResult AUSCreatePortin(string CreatePortIn)
        {
            PortInAUSRequest objreq = JsonConvert.DeserializeObject<PortInAUSRequest>(CreatePortIn);
            PortInAUSResponse objRes = new PortInAUSResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.RequestedBy = Convert.ToString(Session["UserName"]);

                if (!string.IsNullOrEmpty(objreq.PORTINDATE))
                {
                    try
                    {
                        objreq.PORTINDATE = Utility.GetDateconvertion(objreq.PORTINDATE, clientSetting.mvnoSettings.dateTimeFormat, false, clientSetting.preSettings.portInDateFormat);
                    }
                    catch
                    {
                        objreq.PORTINDATE = Utility.GetDateconvertion(objreq.PORTINDATE, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy-mm-dd");
                    }
                }
                if (!string.IsNullOrEmpty(objreq.CustomerDOB))
                {
                    try
                    {
                        objreq.CustomerDOB = Utility.GetDateconvertion(objreq.CustomerDOB, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy-mm-dd");
                    }
                    catch
                    {
                        CRMLogger.WriteMessage("", "", "");
                    }
                }
                

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.PortInAUS(objreq);

                    if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("AUSPortIn_" + objRes.responseDetails.ResponseCode);
                        objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
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
                //objRes = null;
                objreq = null;
                serviceCRM = null;
            }
            

        }


        public JsonResult CRMGenericOTPViaITG(string CRMGenericOTP)
        {
            CRMGenericOTPReq objReq = new CRMGenericOTPReq();
            CRMGenericOTPRes objRes = new CRMGenericOTPRes();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objReq = JsonConvert.DeserializeObject<CRMGenericOTPReq>(CRMGenericOTP);
                objReq.BrandCode = clientSetting.brandCode;
                objReq.CountryCode = clientSetting.countryCode;
                objReq.LanguageCode = clientSetting.langCode;
                //objReq.MSISDN = Session["MobileNumber"].ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.CRMGenericOTPViaITG(objReq);
                    objRes.OTPdatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    //6129
                   if(objReq.Mode == "3" && clientSetting.preSettings.EnableOTPforVerificationprocess.ToUpper()=="TRUE" && clientSetting.preSettings.EnableSemiautomatedverification.ToUpper() == "TRUE")
                   {
                       Session["CheckOTPVerificationprocess"] = objRes.reponseDetails.ResponseCode == "0" ? "TRUE" : "FALSE";

                   }
                
                if (objRes != null && objRes.reponseDetails != null && objRes.reponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleInfo_" + objRes.reponseDetails.ResponseCode);
                    objRes.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.reponseDetails.ResponseDesc : errorInsertMsg;
                }
                return Json(objRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(objRes, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objReq = null;
               // objRes = null;
                serviceCRM = null;
            }
            
        }
		
		
		        //4296
        public ActionResult MACPortIn(string PortinMsisdn)
        {
            PortInSWEResponse portres = new PortInSWEResponse();
            try
            {
                CRMBase crmBase = new CRMBase();
                crmBase.CountryCode = clientSetting.countryCode;
                crmBase.BrandCode = clientSetting.brandCode;
                crmBase.LanguageCode = clientSetting.langCode;

                List<SPDETAILS> sp = new List<SPDETAILS>();
                sp = SP(crmBase);

                if (sp.Count > 0)
                {
                    ServiceProvider objService = null;
                    foreach (SPDETAILS str in sp)
                    {
                        objService = new ServiceProvider();
                        objService.spcode = str.SPCODE;
                        objService.spdesc = str.SPDESC;
                        portres.serviceProvider.Add(objService);
                    }
                }
                portres.portinMsisdn = PortinMsisdn;
                List<HOLIDAYSLIST> HdayList = new List<HOLIDAYSLIST>();
                HdayList = dtHoliday(crmBase);
                string Dateval = string.Empty;
                for (int i = 0; i < HdayList.Count; i++)
                {
                    Dateval = Dateval + "," + HdayList[i].DATE;// HolidayDateconvert(HdayList[i].DATE, "0,1,2");
                }
                Dateval = Dateval.Replace(",0", ",").Replace("-0", "-");
                string RemoveComma = Dateval.Trim().TrimStart(',');
                portres.PortinHolidayDate = RemoveComma.Trim().Replace('/', '-');

                portres.portInPrefLang = PortInPrefLang(crmBase);
                if (!string.IsNullOrEmpty(clientSetting.preSettings.internalServiceProvider))
                {
                    try
                    {
                        List<string> lstResp = new List<string>(clientSetting.preSettings.internalServiceProvider.Split('|'));
                        List<string> lstText = new List<string>(lstResp[0].Split(','));
                        List<string> lstValue = new List<string>(lstResp[1].Split(','));
                        if (lstText.Count > 0 && lstValue.Count > 0)
                        {
                            InternalServiceProvider objInternal = null;
                            portres.internalServiceProvider = new List<InternalServiceProvider>();
                            for (int i = 0; i < lstText.Count; i++)
                            {
                                string dummystring = System.Text.RegularExpressions.Regex.Replace(lstText[i], "[^0-9a-zA-Z]+", string.Empty);
                                string ResourceMsg = Resources.DropdownResources.ResourceManager.GetString(dummystring);
                                lstText[i] = string.IsNullOrEmpty(ResourceMsg) ? lstText[i] : ResourceMsg;
                                objInternal = new InternalServiceProvider();
                                objInternal.internalServiceProvText = lstText[i];
                                objInternal.internalServiceProvValue = lstValue[i];
                                portres.internalServiceProvider.Add(objInternal);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                portres.strDropdown = Utility.GetDropdownMasterFromDB("85,86", "1", "drop_master");

            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            return View(portres);
        }

        [HttpPost]
        public JsonResult MACCreatePortin(string CreatePortIn)
        {
            PortInMACRequest objreq = JsonConvert.DeserializeObject<PortInMACRequest>(CreatePortIn);
            PortInMACResponse objres = new PortInMACResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.RequestedBy = Convert.ToString(Session["UserName"]);
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                //CreatePortIn.REQUESTEDBY = Session["UserName"].ToString();
                //objreq.Request = CreatePortIn;
                //Params.PORTINDATE = Dateconvert(CreatePortIn.PORTINDATE, "1,0,2");  
                objreq.PORTINDATE = Utility.GetDateconvertion(objreq.PORTINDATE, clientSetting.mvnoSettings.dateTimeFormat, false, clientSetting.preSettings.portInDateFormat);
                objreq.CustomerDOB = Utility.GetDateconvertion(objreq.CustomerDOB, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy-mm-dd");

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objres = serviceCRM.PortInMAC(objreq);
                    if (objres != null && objres.responseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Customer_Portin_" + objres.responseDetails.ResponseCode);
                        objres.responseDetails.ResponseCode = string.IsNullOrEmpty(errorInsertMsg) ? objres.responseDetails.ResponseCode : errorInsertMsg;
                    }
                
            }

            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                CreatePortIn = null;
                objreq = null;
                serviceCRM = null;
            }
            return Json(objres);

        }
        //--------------------FRR-4470---------------s.subha---------------
        #region-----------------FRR-4470---------------------
        public ActionResult MKDportinApproval()
        {
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "Customer_MKDportinApproval").ToList();
            return View();
        }
        public ActionResult MKDSearchPortinApprovalStatus()
        {
            List<ServiceCRM.Menu> menu = new List<ServiceCRM.Menu>();
            menu = ((List<ServiceCRM.Menu>)Session["MenuAndFeatures"]).Where(a => a.SubCatUrl == "Customer_MKDSearchPortinApprovalStatus").ToList();
            return View();
        }
        public ActionResult MKDPortinStatusCheck()
        {
            return View();
        }
        public ActionResult MKDRetrieveReferenceNumber()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CRMGetPortinApprovalRecords_Macedonia(string GetPortInRecords)
        {
            PortinGetApprovalRequest_Macedonia objreq = JsonConvert.DeserializeObject<PortinGetApprovalRequest_Macedonia>(GetPortInRecords);
            PortinGetApprovalResponse_Macedonia objRes = new PortinGetApprovalResponse_Macedonia();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                try
                {
                    if(!string.IsNullOrEmpty(objreq.FromDate))
                        objreq.FromDate = Utility.GetDateconvertion(objreq.FromDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy-MM-dd");
                    if (!string.IsNullOrEmpty(objreq.ToDate))
                        objreq.ToDate = Utility.GetDateconvertion(objreq.ToDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy-MM-dd");
                }
                catch
                {}
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.GetPortinApprovalRecords_Macedonia(objreq);
                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MKDPortInErr_" + objRes.responseDetails.ResponseCode);
                    objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
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
                errorInsertMsg = string.Empty;
            }

        }
        [HttpPost]
        public JsonResult CRMGetPortinIndividualRecord_Macedonia(string GetPortInIndividualRecord)
        {
            PortinGetIndividualRequest_Macedonia objreq = JsonConvert.DeserializeObject<PortinGetIndividualRequest_Macedonia>(GetPortInIndividualRecord);
            PortinGetIndividualResponse_Macedonia objRes = new PortinGetIndividualResponse_Macedonia();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.NeedPdfContent = false;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.GetPortinIndividualRecord_Macedonia(objreq);
                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MKDPortInErr_" + objRes.responseDetails.ResponseCode);
                    objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
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
                errorInsertMsg = string.Empty;
            }
        }
        [HttpPost]
        public JsonResult CRMPortinAuthorizeRejectMacedonia(string GetPortInPortInAuthorizeRecord)
        {
            PortinAuthorizationRequest_Macedonia objreq = JsonConvert.DeserializeObject<PortinAuthorizationRequest_Macedonia>(GetPortInPortInAuthorizeRecord);
            PortinAuthorizationResponse_Macedonia objRes = new PortinAuthorizationResponse_Macedonia();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                objreq.AuthorizeRejectDate = DateTime.Now.ToString("yyyy-MM-dd");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.UpdatePortinIndividualAuthorizeReject_Macedonia(objreq);
                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MKDPortInErr_" + objRes.responseDetails.ResponseCode);
                    objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
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
                errorInsertMsg = string.Empty;
            }
        }
        [HttpPost]
        public JsonResult CRMGetPortinApprovalStatusRecords_Macedonia(string GetPortInApprovalStatusRecords)
        {
            PortinGetApprovalRequest_Macedonia objreq = JsonConvert.DeserializeObject<PortinGetApprovalRequest_Macedonia>(GetPortInApprovalStatusRecords);
            PortinGetApprovalStatusResponse_Macedonia objRes = new PortinGetApprovalStatusResponse_Macedonia();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                try
                {
                    if (!string.IsNullOrEmpty(objreq.FromDate))
                        objreq.FromDate = Utility.GetDateconvertion(objreq.FromDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy-MM-dd");
                    if (!string.IsNullOrEmpty(objreq.ToDate))
                        objreq.ToDate = Utility.GetDateconvertion(objreq.ToDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy-MM-dd");
                }
                catch
                {
                    //objreq.portInDate = "2018-06-02"; // Utility.GetDateconvertion(objreq.portInDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy-mm-dd");
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.GetPortinApprovalStatusRecords_Macedonia(objreq);
                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MKDPortInErr_" + objRes.responseDetails.ResponseCode);
                    objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
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
                errorInsertMsg = string.Empty;
            }
        }
        [HttpPost]
        public JsonResult CRMGetPortinStatusCheckRecords_Macedonia(string GetPortInStatusCheckRecords)
        {
            PortinGetApprovalRequest_Macedonia objreq = JsonConvert.DeserializeObject<PortinGetApprovalRequest_Macedonia>(GetPortInStatusCheckRecords);
            PortinStatusCheckResponse_Macedonia objRes = new PortinStatusCheckResponse_Macedonia();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.GetPortinStatusCheckRecords_Macedonia(objreq);
                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MKDPortInErr_" + objRes.responseDetails.ResponseCode);
                    objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
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
                serviceCRM = null;
                objreq = null;
                errorInsertMsg = string.Empty;
            }
        }
        [HttpPost]
        public JsonResult CRMGetPortinReferenceNumber_Macedonia(string GetPortInReferenceNumber)
        {
            PortinGetApprovalRequest_Macedonia objreq = JsonConvert.DeserializeObject<PortinGetApprovalRequest_Macedonia>(GetPortInReferenceNumber);
            PortinGetReferenceNumberResponsser_Macedonia objRes = new PortinGetReferenceNumberResponsser_Macedonia();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                objRes = serviceCRM.GetPortinReferenceNumber_Macedonia(objreq);
                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MKDPortInErr_" + objRes.responseDetails.ResponseCode);
                    objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
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
                serviceCRM = null;
                objreq = null;
                errorInsertMsg = string.Empty;
            }
        }

        [HttpPost]
        public void DownloadMKDExcel(string Details)
        {
            GridView gridView = new GridView();
            try
            {
                System.Data.DataTable dt = (System.Data.DataTable)JsonConvert.DeserializeObject(Details, (typeof(System.Data.DataTable)));
                gridView.DataSource = dt; 
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "Download_" + DateTime.Now.ToString("yyyy-MM-ddHHmmss"), this.HttpContext.Response);
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

        public FileResult DownloadMKDPDF(string strParameterName)
        {
            PortinGetIndividualRequest_Macedonia objreq = new PortinGetIndividualRequest_Macedonia();
            PortinGetIndividualResponse_Macedonia objRes = new PortinGetIndividualResponse_Macedonia();
            ServiceInvokeCRM serviceCRM;
            byte[] pdfBytes;
            try
            {
                objreq.CountryCode = clientSetting.countryCode;
                objreq.BrandCode = clientSetting.brandCode;
                objreq.LanguageCode = clientSetting.langCode;
                if (strParameterName.Split('-').Length == 2)
                {
                    objreq.PMsisdn = strParameterName.Split('-').GetValue(0).ToString();
                    objreq.ReferenceNumber = strParameterName.Split('-').GetValue(1).ToString();
                    objreq.NeedPdfContent = true;
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    objRes = serviceCRM.GetPortinIndividualRecord_Macedonia(objreq);
                    if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                    {
                        if (string.IsNullOrEmpty(objRes.PdfContent))
                        {
                            CRMLogger.WriteInformationLog(Convert.ToString(Session["UserName"]) + "DownloadMKDPDF - PDFContent is empty");
                            pdfBytes = System.Text.Encoding.ASCII.GetBytes("Internal Error");
                            return File(pdfBytes, "application/pdf", "UnableToDownload.pdf");
                        }
                        else
                        {
                            pdfBytes = (byte[])(Convert.FromBase64String(objRes.PdfContent));
                            return File(pdfBytes, "application/pdf", objRes.ReferenceNumber + ".pdf");
                        }
                    }
                    else
                    {
                        CRMLogger.WriteInformationLog(Convert.ToString(Session["UserName"]) + "DownloadMKDPDF - Service Response is empty");
                        pdfBytes = System.Text.Encoding.ASCII.GetBytes("Internal Error");
                        return File(pdfBytes, "application/pdf", "UnableToDownload.pdf");
                    }
                }
                else
                {
                    CRMLogger.WriteInformationLog(Convert.ToString(Session["UserName"]) + "DownloadMKDPDF - Parameter Mismatch Error");
                    pdfBytes = System.Text.Encoding.ASCII.GetBytes("Internal Error");
                    return File(pdfBytes, "application/pdf", "UnableToDownload.pdf");
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                pdfBytes = System.Text.Encoding.ASCII.GetBytes("Internal Error");
                return File(pdfBytes, "application/pdf", "UnableToDownload.pdf");
            }
            finally
            {
                objreq = null;
                objRes = null;
                serviceCRM = null;
            }
        }

        public ActionResult DownloadMKDPDFFile(string FileName)
        {
            #region***************DownloadMKDPDFFile*****************
            try
            {
                string path = "";
                if (clientSetting.countryCode.ToUpper() == "MKD")
                {
                    byte[] pdfBytes = System.IO.File.ReadAllBytes("D:\\PDFUploadPath\\tt.pdf");
                    return File(pdfBytes, "application/pdf", "AccountSummary.pdf");
                    //path = clientSetting.mvnoSettings.DownloadPDFFilePath;
                    //path = path + '\\' + FileName;
                    //path = "D:\\PDFUploadPath";
                    //var mimeType = "application/pdf";
                    //var fileDownloadName = "tt.pdf"; // FileName;
                    //return this.File(path, mimeType, fileDownloadName);
                    ////if (System.IO.File.Exists(path))
                    ////{
                    ////    var mimeType = "application/pdf";
                    ////    var fileDownloadName = FileName;
                    ////    return this.File(path, mimeType, fileDownloadName);
                    ////}
                    ////else
                    ////{
                    ////    CRMLogger.WriteMessage(Session["UserName"].ToString(), this.ControllerContext, "File is not exits in given path");
                    ////    return null;
                    ////}
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return null;
            }
            return null;
            #endregion
        }
        //[HttpPost]
        //public JsonResult CRMPortinApprovalMacedoniaCRM(string CreatePortIn)
        //{
        //    PortinApprovalMacedoniaRequest objreq = JsonConvert.DeserializeObject<PortinApprovalMacedoniaRequest>(CreatePortIn);
        //    PortinApprovalMacedoniaResponse objRes = new PortinApprovalMacedoniaResponse();
        //    try
        //    {
        //        objreq.CountryCode = clientSetting.countryCode;
        //        objreq.BrandCode = clientSetting.brandCode;
        //        objreq.LanguageCode = clientSetting.langCode;
        //        objreq.userName = Convert.ToString(Session["UserName"]);
        //        try
        //        {
        //            objreq.fromDate = "2017-06-02"; // Utility.GetDateconvertion(objreq.fromDate, clientSetting.mvnoSettings.dateTimeFormat, false, clientSetting.preSettings.portInDateFormat);
        //            objreq.toDate = Utility.GetDateconvertion(objreq.toDate, clientSetting.mvnoSettings.dateTimeFormat, false, clientSetting.preSettings.portInDateFormat);
        //        }
        //        catch
        //        {
        //            objreq.portInDate = "2018-06-02"; // Utility.GetDateconvertion(objreq.portInDate, clientSetting.mvnoSettings.dateTimeFormat, false, "yyyy-mm-dd");
        //        }

        //        using (ServiceInvokeCRM serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl))
        //        {
        //            objRes = serviceCRM.CRMPortinApprovalMacedoniaCRM(objreq);

        //            if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
        //            {
        //                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BELPortIn_" + objRes.responseDetails.ResponseCode);
        //                objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
        //            }
        //        }
        //        return Json(objRes);
        //    }
        //    catch (Exception ex)
        //    {
        //        CRMLogger.WriteException(Session["UserName"].ToString(), this.ControllerContext, ex);
        //        return Json(objRes);
        //    }
        //    finally
        //    {
        //        //objRes = null;
        //        objreq = null;
        //    }

        //}
        //[HttpPost]
        //public JsonResult CRMApprovalStatusMacedonia(string PortInStatus)
        //{
        //    PortinApprovalMacedoniaRequest objreq = JsonConvert.DeserializeObject<PortinApprovalMacedoniaRequest>(PortInStatus);
        //    PortinApprovalMacedoniaResponse objRes = new PortinApprovalMacedoniaResponse();
        //    try
        //    {
        //        objreq.CountryCode = clientSetting.countryCode;
        //        objreq.BrandCode = clientSetting.brandCode;
        //        objreq.LanguageCode = clientSetting.langCode;

        //        using (ServiceInvokeCRM serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl))
        //        {
        //            objRes = serviceCRM.CRMApprovalStatusMacedonia(objreq);

        //            if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
        //            {
        //                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BELPortIn_" + objRes.responseDetails.ResponseCode);
        //                objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
        //            }

        //            try
        //            {
        //                if (clientSetting.countryCode == "MKD")
        //                {
        //                    if (objRes != null && objRes.PortInApprovalMacedoniaStatus != null && objRes.PortInApprovalMacedoniaStatus.pDate != null && objRes.PortInApprovalMacedoniaStatus.pDate != "")
        //                    {
        //                        objRes.PortInApprovalMacedoniaStatus.pDate = Utility.GetDateconvertion(objRes.PortInApprovalMacedoniaStatus.pDate, "yyyy/mm/dd", false, clientSetting.mvnoSettings.dateTimeFormat);
        //                    }
        //                    if (objRes != null && objRes.PortInApprovalMacedoniaStatus != null && objRes.PortInApprovalMacedoniaStatus.Dateofrequest != null && objRes.PortInApprovalMacedoniaStatus.Dateofrequest != "")
        //                    {
        //                        objRes.PortInApprovalMacedoniaStatus.Dateofrequest = Utility.GetDateconvertion(objRes.PortInApprovalMacedoniaStatus.Dateofrequest, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
        //                    }
        //                }
                       
        //            }
        //            catch
        //            {
        //            }
        //        }
        //        return Json(objRes);
        //    }
        //    catch (Exception ex)
        //    {
        //        CRMLogger.WriteException(Session["UserName"].ToString(), this.ControllerContext, ex);
        //        return Json(objRes);
        //    }
        //    finally
        //    {
        //        objreq = null;
        //        //objRes = null;
        //    }

        //}
        #endregion
        //--------------------FRR-4470---------------


        #region POF-6612
        public ActionResult PortoutRequestedCustomer()
        {
            return View();
        }

        public JsonResult PortoutRequestedCust(PortoutRequestedCustReq Objreq)
        {
            PortoutRequestedCustResp Objresp = new PortoutRequestedCustResp();
            ServiceInvokeCRM serviceCRM;
            try
            {
                Objreq.CountryCode = clientSetting.countryCode;
                Objreq.BrandCode = clientSetting.brandCode;
                Objreq.LanguageCode = clientSetting.langCode;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "CustomerController - PortoutRequestedCust Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);

                Objresp = serviceCRM.CRMPortoutRequestedCustomer(Objreq);
                if (Objresp != null && Objresp.Response != null && Objresp.Response.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PortoutRequestedCust_" + Objresp.Response.ResponseCode);
                    Objresp.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? Objresp.Response.ResponseDesc : errorInsertMsg;
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "CustomerController - PortoutRequestedCust End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "CustomerController - exception-PortoutRequestedCust - " + this.ControllerContext, ex);
            }
            finally
            {
                Objreq = null;
            }
            return Json(Objresp, JsonRequestBehavior.AllowGet);

        }
        #endregion

    }
}
