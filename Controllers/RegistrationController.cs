using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using CRM.Models;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Resources;
using Service;
using ServiceCRM;
using Tamir.SharpSsh;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO.Compression;
using Ionic.Zip;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Web.WebPages;
using System.Configuration;
using System.Net.NetworkInformation;
using SelectPdf;
using System.Runtime.Remoting;

namespace CRM.Controllers
{

    [ValidateState]
    public class RegistrationController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();
        public static bool _runningFromNUnit = Service.UnitTestDetector._runningFromNUnit;
        Service.Registration crmService = new Service.Registration(Convert.ToString(SettingsCRM.crmServiceUrl));

        ServiceCRM.ServiceInvokeCRM crmNewService = new ServiceCRM.ServiceInvokeCRM(Convert.ToString(SettingsCRM.crmServiceUrl));

        #region Italy (Prepaid/Postpaid)
        #region SubscriberRegistration_ITA
        public ActionResult SubscriberRegistration_ITA(string RegisterMsisdn, string Mode)
        {
            Session["PDFfileName1"] = null;

            CRM.Models.Registration objRegistration = new CRM.Models.Registration();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
                string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                objRegistration.CountryDateFormat = Datestring;
                objRegistration.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,7,8", objRegistration.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "Tbl_Nationality")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "Tbl_Nationality_EU")).ToList();
                objRegistration.Mode = "INSERT";

                objRegistration.MSISDN = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                objRegistration.DateCalender = Datestring.ToLower().Replace("yyyy", "yy");
                int iDocCount = 0;
                string ErrorMsg = ""; 
                if (!_runningFromNUnit)
                {
                    ErrorMsg = Resources.ErrorResources.EReg_ITA_doc3;
                }

                #region Create path
                CreateUploadedDir(objRegistration.IsPostpaid);
                #endregion

                #region Delete file from path
                if (!_runningFromNUnit)
                {
                    DeleteUploadedFile();
                }
                #endregion

                if (objRegistration.IsPostpaid != null && objRegistration.IsPostpaid == "1")   // Prepaid
                {

                    if (clientSetting.preSettings.showPlanReg == "1")
                    {

                        RegisterSubscriber objreq = new RegisterSubscriber();
                        RegisterSubscriberRes objRes = new RegisterSubscriberRes();

                        objreq.CountryCode = clientSetting.countryCode;
                        objreq.BrandCode = clientSetting.brandCode;
                        objreq.LanguageCode = clientSetting.langCode;

                        objreq.Mode = "GETPLAN";
                        objreq.MSISDN = RegisterMsisdn;

                        objRes = crmService.InsertSubscriber(objreq);
                        objRegistration.PlanList = objRes.PlanList;

                        Session["PlanId"] = objRes.RegisterInsertDetails.AccId != null ? objRes.RegisterInsertDetails.AccId : "";
                    }



                    objRegistration.DocFormat = clientSetting.preSettings.itaDocumentFormat.ToUpper();
                    objRegistration.Filesize = clientSetting.preSettings.itaAttachFileSize;
                    iDocCount = (clientSetting.preSettings.itaSignedDoc != null && Convert.ToString(clientSetting.preSettings.itaSignedDoc).ToUpper() == "TRUE") ? 1 : 0;

                    if (!_runningFromNUnit)
                    {
                        if (clientSetting.preSettings.isITAAttachmentMand.ToUpper() == "TRUE")
                        {
                            if (clientSetting.preSettings.itaMandatoryDoc != null && Convert.ToInt16(clientSetting.preSettings.itaMandatoryDoc) > 0)
                            {
                                if (iDocCount == 1)
                                    ErrorMsg = string.Format(Resources.ErrorResources.EReg_ITA_doc2, Convert.ToInt16(clientSetting.preSettings.itaMandatoryDoc));
                                else
                                    ErrorMsg = string.Format(Resources.ErrorResources.EReg_ITA_doc1, Convert.ToInt16(clientSetting.preSettings.itaMandatoryDoc));

                                iDocCount = iDocCount + Convert.ToInt16(clientSetting.preSettings.itaMandatoryDoc);
                            }
                            objRegistration.DocDetails = iDocCount.ToString() + "|" + ErrorMsg;
                        }
                        else
                        {
                            objRegistration.DocDetails = "0|";
                        }
                    }
                }

                if (objRegistration.IsPostpaid != null && objRegistration.IsPostpaid == "0") // Postpaid
                {
                    CRMBase cbase = new CRMBase();
                    cbase.CountryCode = clientSetting.countryCode;
                    cbase.BrandCode = clientSetting.brandCode;
                    cbase.LanguageCode = clientSetting.langCode;
                    GetPlansfromPBS objres = crmNewService.CRMGetPlansfromPBS(cbase);
                    objRegistration.drpdownBillCycle = objres.Billcycledatelist;
                    objRegistration.drpCorporate = objres.Corporatelist;
                    TempData["Corporate"] = objres.Corporatelist;

                    objRegistration.DocFormat = clientSetting.postSettings.itaDocumentFormat.ToUpper();
                    objRegistration.Filesize = clientSetting.postSettings.itaAttachFileSize;
                    iDocCount = (clientSetting.postSettings.itaSignedDoc != null && Convert.ToString(clientSetting.postSettings.itaSignedDoc).ToUpper() == "TRUE") ? 1 : 0;

                    if (clientSetting.postSettings.isITAAttachmentMand.ToUpper() == "TRUE")
                    {
                        if (iDocCount == 1)
                            ErrorMsg = string.Format(Resources.ErrorResources.EReg_ITA_doc2, Convert.ToInt16(clientSetting.postSettings.itaMandatoryDoc));
                        else
                            ErrorMsg = string.Format(Resources.ErrorResources.EReg_ITA_doc1, Convert.ToInt16(clientSetting.postSettings.itaMandatoryDoc));

                        iDocCount = iDocCount + Convert.ToInt16(clientSetting.postSettings.itaMandatoryDoc);

                        objRegistration.DocDetails = iDocCount.ToString() + "|" + ErrorMsg;
                    }
                    else
                    {
                        objRegistration.DocDetails = "-1|";
                    }
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,7,8", objRegistration.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "Tbl_Nationality")).ToList();
            }
            return View(objRegistration);
        }
        #endregion SubscriberRegistration_ITA

        #region ValidateTaxCode_ITA
        public JsonResult ValidateTaxCode_ITA(TaxCodeValidationRequest objReq)
        {
            CRMResponse objRes = new CRMResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objReq.CountryCode = clientSetting.countryCode;
                    objReq.BrandCode = clientSetting.brandCode;
                    objReq.LanguageCode = clientSetting.langCode;

                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, Convert.ToString(objReq));
                    objRes = serviceCRM.CRMValidateTaxCode(objReq);
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes);
                
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return Json(objRes);
        }
        #endregion

        #region ITA PostpaidPlanDetails
        public ActionResult PostpaidPlanDetails(string RegisterMsisdn)
        {
            CRM.Models.Registration objRegistration = new CRM.Models.Registration();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
                string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                objRegistration.CountryDateFormat = Datestring;

                objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,5,6,7,8,10", "0", "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, "0", "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, "0", "Tbl_Nationality")).ToList();

                objRegistration.MSISDN = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                objRegistration.DateCalender = Datestring.ToLower().Replace("yyyy", "yy");
                objRegistration.DocFormat = clientSetting.preSettings.itaDocumentFormat.ToUpper();
                objRegistration.Filesize = clientSetting.preSettings.itaAttachFileSize;
                CRMBase objBase = new CRMBase();
                objBase.CountryCode = clientSetting.countryCode;
                objBase.BrandCode = clientSetting.brandCode;
                objBase.LanguageCode = clientSetting.langCode;
                objRegistration.objITAPlanListResponse = crmService.LoadPlan(objBase);
                /* To upload handsettings image as manually start */
                string dirHandsettings = Path.Combine(Server.MapPath("~/Library/DefaultTheme/Images/Handsettings"));
                if (!Directory.Exists(dirHandsettings))
                {
                    Directory.CreateDirectory(dirHandsettings);
                }
                /* To upload handsettings image as manually end */
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objRegistration);
        }
        #endregion

        #region ITA Postpaid Insert/Update
        public JsonResult InserPostpaidRegistration(string Registration)
        {
            List<RegisterInsertPostpaid> objInsertPostpaid = new List<RegisterInsertPostpaid>();
            RegisterInsertPostpaidResponse objInsertRes = new RegisterInsertPostpaidResponse();
            string AccountNo = string.Empty;
            string TransactionNumber = string.Empty;
            string ReferenceNumber = string.Empty;
            string TransactionRefMsg = string.Empty;
            string PaymentMessage = string.Empty;

            try
            {
                RegisterPostpaidRequest objReg = JsonConvert.DeserializeObject<RegisterPostpaidRequest>(Registration);

                objReg.CountryCode = clientSetting.countryCode;
                objReg.CountryID = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;

                objReg.User = Convert.ToString(Session["UserName"]);
                objReg.CreatedBy = Convert.ToString(Session["UserName"]);
                objReg.SendPrefLanguagetoRRBS = Convert.ToString(clientSetting.mvnoSettings.sendPrefLanguagetoRRBS);
                objReg.SMSUPDATE = "1";

                objInsertRes = crmService.InsertPospaidSubscriber(objReg);

                if (objInsertRes.ResponseDetails.ResponseCode == "0" || objInsertRes.ResponseDetails.ResponseCode == "50" || objInsertRes.ResponseDetails.ResponseCode == "51" || objInsertRes.ResponseDetails.ResponseCode == "52" || objInsertRes.ResponseDetails.ResponseCode == "53" || objInsertRes.ResponseDetails.ResponseCode == "54" || objInsertRes.ResponseDetails.ResponseCode == "55" || objInsertRes.ResponseDetails.ResponseCode == "56" || objInsertRes.ResponseDetails.ResponseCode == "57" || objInsertRes.ResponseDetails.ResponseCode == "58")
                {
                    #region Change Uploaded File Path

                    string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                    string sourcePath = string.Empty;
                    // if (Server.MapPath("~/App_Data/UploadFile") != null)
                    //{
                    //    sourcePath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                    //}
                    if (clientSetting.mvnoSettings.internalUploadFile != null)
                    {
                        sourcePath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    }
                    string targetPath = clientSetting.postSettings.registrationDocpath;

                    if (objReg.mode.ToUpper() == "INSERT")
                    {
                        AccountNo = !string.IsNullOrEmpty(objInsertRes.RegisterInsertPostpaid[0].AccountNo) ? objInsertRes.RegisterInsertPostpaid[0].AccountNo : string.Empty;
                        TransactionNumber = !string.IsNullOrEmpty(objInsertRes.RegisterInsertPostpaid[0].TransactionNumber) ? objInsertRes.RegisterInsertPostpaid[0].TransactionNumber : string.Empty;
                        ReferenceNumber = !string.IsNullOrEmpty(objInsertRes.RegisterInsertPostpaid[0].ReferenceNumber) ? objInsertRes.RegisterInsertPostpaid[0].ReferenceNumber : string.Empty;
                        PaymentMessage = !string.IsNullOrEmpty(objInsertRes.RegisterInsertPostpaid[0].PaymentMessage) ? objInsertRes.RegisterInsertPostpaid[0].PaymentMessage : string.Empty;
                    }
                    else
                    {
                        AccountNo = objReg.Accountid;
                    }

                    #region File Upload
                    for (int i = 0; i < 3; i++)     // Max 3 files upload
                    {
                        string[] fileformat = clientSetting.mvnoSettings.ticketAttachFileformat.Split(',');
                        for (int j = 0; j < fileformat.Length; j++)
                        {
                            fileformat[j] = fileformat[j].Replace(".", string.Empty).Trim();
                            System.IO.File.Delete(targetPath + "\\" + AccountNo + "_Doc" + (i + 1) + "." + fileformat[j]);
                        }
                    }

                    //if (Server.MapPath("~/App_Data/UploadFile") != null)
                    if (clientSetting.mvnoSettings.internalUploadFile != null)
                    {
                        if (Directory.GetFiles(sourcePath).Count() > 0)
                        {

                            string[] fileEntries2 = Directory.GetFiles(sourcePath);
                            string[] filenameList = new string[3];
                            string Deletefilename = string.Empty;
                            string Externsion, filename = string.Empty;
                            for (int i = 0; i < fileEntries2.Count(); i++)
                            {
                                Externsion = fileEntries2[i].Substring(fileEntries2[i].LastIndexOf("\\") + 1);
                                string[] splitExternsion = Externsion.Split('.');
                                filename = AccountNo + "_Doc" + (i + 1) + "." + splitExternsion[1];
                                System.IO.File.Move(sourcePath + "\\" + Externsion, targetPath + "\\" + filename);
                                filenameList[i] = filename.ToString();
                                Deletefilename = AccountNo + "_Doc";
                            }
                            MovefileFTP(targetPath + "\\", filenameList, Deletefilename, objReg, objInsertRes.ResponseDetails.ResponseDesc);
                        }
                    }
                    if (clientSetting.mvnoSettings.internalPdfDownload != null)
                    {
                        string delPath = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);

                        if (Directory.Exists(delPath))
                        {
                            System.IO.DirectoryInfo di = new DirectoryInfo(delPath);
                            foreach (FileInfo file in di.GetFiles())
                            {
                                file.Delete();
                            }
                            // Directory.Delete(delPath, true);
                        }
                    }
                    Session["FilePath"] = string.Empty;
                    #endregion

                    #endregion
                }

                switch (objInsertRes.ResponseDetails.ResponseCode)
                {
                    #region switch cases
                    case "0":
                        if (objReg.mode == "UPDATE")
                        {
                            objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EUpdat_AUT_0;
                        }
                        else
                        {
                            objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg0;
                        }
                        break;
                    case "1":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg1;
                        break;
                    case "2":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg2;
                        break;
                    case "3":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg3;
                        break;
                    case "4":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg4;
                        break;
                    case "5":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg5;
                        break;
                    case "6":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg6;
                        break;
                    case "7":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg7;
                        break;
                    case "8":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg8;
                        break;
                    case "9":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg9;
                        break;
                    case "10":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg10;
                        break;
                    case "25":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_ITA_25;
                        break;
                    case "50":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_ITA_50;
                        break;
                    case "51":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_ITA_51;
                        break;
                    case "52":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_ITA_52;
                        objInsertRes.ResponseDetails.ResponseCode = "0";
                        break;
                    case "53":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_ITA_53;
                        break;
                    case "54":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_ITA_54;
                        objInsertRes.ResponseDetails.ResponseCode = "0";
                        break;
                    case "55":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_ITA_55;
                        break;
                    case "56":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_ITA_56;
                        break;
                    case "57":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_ITA_57;
                        break;
                    case "58":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_ITA_58;
                        break;
                    case "60":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_ITA_60;
                        break;
                    case "110":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_GBR110;
                        objInsertRes.ResponseDetails.ResponseCode = "0";
                        break;
                    case "111":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_GBR111;
                        objInsertRes.ResponseDetails.ResponseCode = "0";
                        break;
                    case "112":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_GBR112;
                        objInsertRes.ResponseDetails.ResponseCode = "0";
                        break;
                    case "113":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_GBR_113;
                        objInsertRes.ResponseDetails.ResponseCode = "0";
                        break;
                    case "114":
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.EReg_GBR_114;
                        objInsertRes.ResponseDetails.ResponseCode = "0";
                        break;
                    default:
                        objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.Common_2;
                        break;
                    #endregion
                }

                if (!string.IsNullOrEmpty(PaymentMessage))
                {
                    objInsertRes.ResponseDetails.ResponseDesc += " " + Resources.RegistrationResources.paymentMsg + " " + PaymentMessage;
                }
                if (!string.IsNullOrEmpty(TransactionNumber))
                {
                    objInsertRes.ResponseDetails.ResponseDesc += " " + Resources.RegistrationResources.transactionNo + " " + TransactionNumber;
                }
                if (!string.IsNullOrEmpty(ReferenceNumber))
                {
                    objInsertRes.ResponseDetails.ResponseDesc += " " + Resources.RegistrationResources.ReferenceNo + " " + ReferenceNumber;
                }
            }
            catch (Exception ex)
            {
                objInsertRes.ResponseDetails.ResponseCode = "999";
                objInsertRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.Common_2;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objInsertRes);
        }
        #endregion

        #region ITA Prepaid Insert/Update
        public JsonResult SaveResgistration(string Registration)
        {
            RegisterSubscriberRes objRes = new RegisterSubscriberRes();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, Registration);
                CRM.Models.Registration objReg = JsonConvert.DeserializeObject<CRM.Models.Registration>(Registration);
                objReg.CountryDateFormat = clientSetting.mvnoSettings.dateTimeFormat;
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                objReg.AttachedBy = Convert.ToString(Session["UserName"]);
                StringBuilder sb = new StringBuilder();
                objReg.Language = Thread.CurrentThread.CurrentUICulture.Name.ToUpper();
                objRes = crmService.InsertSubscriber(objReg);

                List<string> checkValues = new List<string> { "13", "4", "3", "2", "5", "1", "10", "6", "7" };

                if ((!checkValues.Contains(objRes.objResponse.ResponseCode)) && objReg.Mode != "VALIDATE")
                {

                    #region Change Uploaded File Path

                    string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                    string sourcePath = string.Empty;
                    //if (Server.MapPath("~/App_Data/UploadFile") != null)
                    //{
                    //    sourcePath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                    //}
                    if (clientSetting.mvnoSettings.internalUploadFile != null)
                    {
                        sourcePath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    }

                    string targetPath = clientSetting.preSettings.registrationDocpath;

                    string AccountNo = string.Empty;
                    if (objRes.RegisterInsertDetails != null)
                        AccountNo = objRes.RegisterInsertDetails.AccId;

                    #region insert fileupload

                    if (objReg.Mode == "INSERT")
                    {

                        for (int i = 0; i < 3; i++)        // Max 3 files upload
                        {
                            string[] fileformat = clientSetting.preSettings.itaDocumentFormat.Split(',');
                            for (int j = 0; j < fileformat.Length; j++)
                            {
                                fileformat[j] = fileformat[j].Replace(".", string.Empty).Trim();
                                System.IO.File.Delete(targetPath + "\\" + AccountNo + "_Doc" + (i + 1) + "." + fileformat[j]);
                            }
                        }
                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        //if (Server.MapPath("~/App_Data/UploadFile") != null)
                        {
                            if (Directory.GetFiles(sourcePath).Count() > 0)
                            {
                                string[] fileEntries2 = Directory.GetFiles(sourcePath);
                                string[] filenameList = new string[3];
                                string Externsion, filename = string.Empty;
                                string Deletefilename = string.Empty;
                                for (int i = 0; i < fileEntries2.Count(); i++)
                                {
                                    Externsion = fileEntries2[i].Substring(fileEntries2[i].LastIndexOf("\\") + 1);
                                    string[] splitExternsion = Externsion.Split('.');
                                    filename = AccountNo + "_Doc" + (i + 1) + "." + splitExternsion[1];
                                    System.IO.File.Move(sourcePath + "\\" + Externsion, targetPath + "\\" + filename);
                                    filenameList[i] = filename.ToString();
                                    Deletefilename = AccountNo + "_Doc";


                                    if (clientSetting.preSettings.enableSFTP.ToUpper() == "FALSE" && clientSetting.mvnoSettings.enableFTPS.ToUpper() == "FALSE")
                                    {

                                        #region Create path

                                        string TargetDocpath = clientSetting.mvnoSettings.sftpFilePath;
                                        if (!Directory.Exists(TargetDocpath))
                                        {
                                            Directory.CreateDirectory(TargetDocpath);
                                        }

                                        #endregion
                                        System.IO.File.Copy(clientSetting.preSettings.registrationDocpath + "\\" + filename, clientSetting.mvnoSettings.sftpFilePath + "\\" + filename, true);
                                    }
                                    if (clientSetting.mvnoSettings.enableFTPS.ToUpper() == "TRUE")
                                    {
                                        FileUploadFTPSServer(targetPath + "\\", filename, objReg);
                                    }
                                }
                                //  MovefileFTP(targetPath + "\\", filenameList, Deletefilename, objReg, objRes.objResponse.ResponseDesc);

                                if (clientSetting.preSettings.enableSFTP.ToUpper() == "TRUE" && clientSetting.mvnoSettings.enableFTPS.ToUpper() == "FALSE")
                                {
                                    MovefileFTP(targetPath + "\\", filenameList, Deletefilename, objReg, "");
                                    //FileUploadFTPSServer(targetPath + "\\", filenameList, objReg);
                                }
                            }
                        }
                        if (clientSetting.mvnoSettings.internalPdfDownload != null)
                        {
                            string delPath = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                            if (Directory.Exists(delPath))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(delPath);
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                            }
                        }
                        Session["FilePath"] = string.Empty;
                    #endregion

                    }

                    else if (objReg.Mode == "UPDATE")
                    {
                        int filecount;
                        // if (objRes.RegisterInsertDetails != null)                        
                        filecount = Convert.ToInt32(objRes.RegisterInsertDetails.RealDummy);

                        for (int i = 0; i < 3; i++)        // Max 3 files upload
                        {
                            string[] fileformat = clientSetting.preSettings.itaDocumentFormat.Split(',');
                            for (int j = 0; j < fileformat.Length; j++)
                            {
                                fileformat[j] = fileformat[j].Replace(".", string.Empty).Trim();
                                i = i + filecount;
                                System.IO.File.Delete(targetPath + "\\" + AccountNo + "_Doc" + (i + 1) + "." + fileformat[j]);
                                i = i - filecount;
                            }
                        }
                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        // if (Server.MapPath("~/App_Data/UploadFile") != null)
                        {
                            if (Directory.GetFiles(sourcePath).Count() > 0)
                            {
                                string[] fileEntries2 = Directory.GetFiles(sourcePath);
                                string[] filenameList = new string[3];
                                string Externsion, filename = string.Empty;
                                string Deletefilename = string.Empty;
                                for (int i = 0; i < fileEntries2.Count(); i++)
                                {
                                    Externsion = fileEntries2[i].Substring(fileEntries2[i].LastIndexOf("\\") + 1);
                                    string[] splitExternsion = Externsion.Split('.');
                                    i = i + Convert.ToInt32(objRes.RegisterInsertDetails.RealDummy);
                                    filename = AccountNo + "_Doc" + (i + 1) + "." + splitExternsion[1];
                                    System.IO.File.Move(sourcePath + "\\" + Externsion, targetPath + "\\" + filename);
                                    i = i - Convert.ToInt32(objRes.RegisterInsertDetails.RealDummy);
                                    filenameList[i] = filename.ToString();
                                    Deletefilename = AccountNo + "_Doc";

                                    if (clientSetting.preSettings.enableSFTP.ToUpper() == "FALSE" && clientSetting.mvnoSettings.enableFTPS.ToUpper() == "FALSE")
                                    {

                                        #region Create path

                                        string TargetDocpath = clientSetting.mvnoSettings.sftpFilePath;
                                        if (!Directory.Exists(TargetDocpath))
                                        {
                                            Directory.CreateDirectory(TargetDocpath);
                                        }

                                        #endregion
                                        System.IO.File.Copy(clientSetting.preSettings.registrationDocpath + "\\" + filename, clientSetting.mvnoSettings.sftpFilePath + "\\" + filename, true);
                                    }
                                    if (clientSetting.mvnoSettings.enableFTPS.ToUpper() == "TRUE")
                                    {
                                        FileUploadFTPSServer(targetPath + "\\", filename, objReg);
                                    }
                                }
                                //  MovefileFTP(targetPath + "\\", filenameList, Deletefilename, objReg, objRes.objResponse.ResponseDesc);

                                if (clientSetting.preSettings.enableSFTP.ToUpper() == "TRUE" && clientSetting.mvnoSettings.enableFTPS.ToUpper() == "FALSE")
                                {
                                    MovefileFTP(targetPath + "\\", filenameList, Deletefilename, objReg, "");
                                    //FileUploadFTPSServer(targetPath + "\\", filenameList, objReg);
                                }
                            }
                        }
                        if (clientSetting.mvnoSettings.internalPdfDownload != null)
                        {
                            string delPath = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                            if (Directory.Exists(delPath))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(delPath);
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                            }
                        }
                        Session["FilePath"] = string.Empty;

                    }

                    #endregion
                }


                if (objReg.Mode.ToUpper() == "UPDATE" && objRes.objResponse.ResponseCode == "0")
                {
                    Session["SubscriberTitle"] = objReg.Title;
                    Session["SubscriberName"] = objReg.FirstName + "|" + objReg.LastName;
                    Session["DOB"] = objReg.DateOfBirth;

                }

                if (objReg.Mode.ToUpper() != "UPDATE" && objReg.Mode.ToUpper() != "VALIDATE")
                {
                    switch (objRes.objResponse.ResponseCode)
                    {
                        #region switch cases
                        case "0":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg0;
                            break;
                        case "1":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg4;
                            break;
                        case "2":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg2;
                            break;
                        case "3":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg3;
                            break;
                        case "4":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg1;
                            break;
                        case "5":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg5;
                            break;
                        case "6":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg6;
                            break;
                        case "7":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg7;
                            break;
                        case "8":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg8;
                            break;
                        case "9":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg9;
                            break;
                        case "10":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg10;
                            break;
                        case "110":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg_GBR110;
                            objRes.objResponse.ResponseCode = "0";
                            break;
                        case "111":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg_GBR111;
                            objRes.objResponse.ResponseCode = "0";
                            break;
                        case "112":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg_GBR112;
                            objRes.objResponse.ResponseCode = "0";
                            break;
                        case "100":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg_GBR100;
                            objRes.objResponse.ResponseCode = "0";
                            break;
                        case "113":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg_GBR_113;
                            objRes.objResponse.ResponseCode = "0";
                            break;
                        case "114":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EReg_GBR_114;
                            objRes.objResponse.ResponseCode = "0";
                            break;
                        default:
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.Common_2;
                            break;

                        #endregion
                    }
                }
                else
                {
                    switch (objRes.objResponse.ResponseCode)
                    {
                        case "0":
                            objRes.objResponse.ResponseDesc = Resources.ErrorResources.EUpdat_AUT_0;
                            break;
                        default:
                            objRes.objResponse.ResponseDesc = objRes.objResponse.ResponseDesc;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objRes);
        }
        #endregion

        #region GetITATax
        public JsonResult GetITATax(string PlanCodeRequest)
        {
            ITAPlanCodeResponse objRes = new ITAPlanCodeResponse();
            try
            {
                ITAPlanCodeRequest objPlanCodeRequest = JsonConvert.DeserializeObject<ITAPlanCodeRequest>(PlanCodeRequest);
                objPlanCodeRequest.CountryCode = clientSetting.countryCode;
                objPlanCodeRequest.BrandCode = clientSetting.brandCode;
                objPlanCodeRequest.LanguageCode = clientSetting.langCode;
                objRes = crmService.LoadTax(objPlanCodeRequest);


                ///FRR--3083
                if (objRes != null && objRes.reponseDetails != null && objRes.reponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetITATax_" + objRes.reponseDetails.ResponseCode);
                    objRes.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.reponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083


                //objRes.objITAPlanListResponse = crmService.CRMPostpaidMappedBundles(objPlanCodeRequest.PlanName);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objRes);
        }
        #endregion

        #region ITA EDIT (Prepaid/Postpaid)
        public ActionResult SubscriberRegistrationEdit_ITA(string RegisterMsisdn, string Mode)
        {
            Session["PDFfileName1"] = null;
            int iDocCount = 0;
            string ErrorMsg = "";
            if (!_runningFromNUnit)
            {
                ErrorMsg = Resources.ErrorResources.EReg_ITA_doc3;
            }
            CRM.Models.Registration objRegistration = new CRM.Models.Registration();
            CustomerDetailITAResponse objGetViewResp = new CustomerDetailITAResponse();
            CustomerDetailsITARequest objGetViewRequest = new CustomerDetailsITARequest();
            RegisterInsertITARequest objReq = new RegisterInsertITARequest();
            Service.ContactAddress objAddres = new Service.ContactAddress();
            Service.IDProof idPf = new Service.IDProof();
            objRegistration.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";

            PostCustomerDetailITAResponse objPostGetViewResp = new PostCustomerDetailITAResponse();
            PostCustomerDetailsITARequest objPostGetViewReq = new PostCustomerDetailsITARequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                if ((!string.IsNullOrEmpty(Mode)) && Mode.ToUpper().Trim() == "UPDATE")
                {
                    objGetViewRequest.CountryCode = clientSetting.countryCode;
                    objGetViewRequest.BrandCode = clientSetting.brandCode;
                    objGetViewRequest.LanguageCode = clientSetting.langCode;
                    objGetViewRequest.MSISDN = RegisterMsisdn;
                    objGetViewRequest.simNUmber = string.Empty;
                    //Postpaid 
                    objPostGetViewReq.CountryCode = clientSetting.countryCode;
                    objPostGetViewReq.BrandCode = clientSetting.brandCode;
                    objPostGetViewReq.LanguageCode = clientSetting.langCode;
                    objPostGetViewReq.MSISDN = RegisterMsisdn;
                    objPostGetViewReq.Customerid = string.Empty;
                    objPostGetViewReq.Status = string.Empty;

                    #region Create Directory path
                    CreateUploadedDir(objRegistration.IsPostpaid);
                    #endregion

                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        #region Prepaid
                        if (objRegistration.IsPostpaid != null && objRegistration.IsPostpaid == "1")  // Prepaid
                        {
                            objGetViewResp = serviceCRM.CRMGetSubscriberITA(objGetViewRequest);
                            XmlDocument XMLSer = Common.CreateXML(objGetViewResp);
                            string XML = XMLSer.InnerXml;
                            objAddres.PostCode = objGetViewResp.subscriberdetail.postCode;
                            objAddres.City = objGetViewResp.subscriberdetail.City;
                            objAddres.State = objGetViewResp.subscriberdetail.State;
                            objAddres.HouseNo = objGetViewResp.subscriberdetail.houseNo;
                            objAddres.Line2 = objGetViewResp.subscriberdetail.Street;
                            //HouseName
                            objAddres.HouseName = objGetViewResp.subscriberdetail.HouseName;
                            XML = Common.ReplaceMultiplestr(XML, "CustomerDetailITAResponse,<subscriberdetail>,</subscriberdetail>,dateOfBirth,birthPlace,Nationality,landLineNo,<responseDetails>,</responseDetails>", "Registration,,,DateOfBirth,PlaceofBirth,Nationality,ContactNumber,<ResponseDetails>,</ResponseDetails>");

                            objRegistration = (CRM.Models.Registration)Common.Deserialize(XML, objRegistration);
                            idPf.IDType = objGetViewResp.subscriberdetail.Idoptions;
                            idPf.IDNumber = objGetViewResp.subscriberdetail.IdNo;
                            string date = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                            if (objGetViewResp.subscriberdetail.Idvaliddate != "0")
                            {
                                idPf.Valitidy = objGetViewResp.subscriberdetail.idvalidityDate.ToString();
                            }

                            idPf.IDIssuedBy = objGetViewResp.subscriberdetail.Issuer;
                            idPf.IssuedDate = objGetViewResp.subscriberdetail.issueDate;
                            objRegistration.IDProof = idPf;
                            objRegistration.smsmak = Convert.ToBoolean(objGetViewResp.subscriberdetail.SMSMARK);
                            objRegistration.RetID = objGetViewResp.subscriberdetail.RetailerID;
                            objRegistration.chkTerms = Convert.ToBoolean(objGetViewResp.subscriberdetail.ChkTerms);
                            objRegistration.ContactNumber = objGetViewResp.subscriberdetail.landLineNo != string.Empty ? objGetViewResp.subscriberdetail.landLineNo : objGetViewResp.subscriberdetail.landLineNo;
                            objRegistration.SIMNo = objGetViewResp.subscriberdetail.SIMNo;

                            objRegistration.Others = objRegistration.Title + "," + objRegistration.Gender.ToUpper() + "," + objRegistration.PrefLanguage + "," + objRegistration.IDProof.IDType + "," + objRegistration.IDProof.IDNumber + "," + objRegistration.Nationality + "," + objRegistration.Rdb;
                            objRegistration.DateOfBirth = objGetViewResp.subscriberdetail.dateOfBirth;
                            objRegistration.LastName = objGetViewResp.subscriberdetail.SurName;

                            if (objGetViewResp.subscriberdetail.Issuedd != 0)
                            {
                                date = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                                date = date.Replace("DD", (objGetViewResp.subscriberdetail.Issuedd.ToString().Length > 1 ? objGetViewResp.subscriberdetail.Issuedd.ToString() : "0" + objGetViewResp.subscriberdetail.Issuedd.ToString())).Replace("MM", (objGetViewResp.subscriberdetail.Issuemm.ToString().Length > 1 ? objGetViewResp.subscriberdetail.Issuemm.ToString() : "0" + objGetViewResp.subscriberdetail.Issuemm.ToString())).Replace("YYYY", objGetViewResp.subscriberdetail.Issueyy.ToString());
                                objRegistration.IDProof.IssuedDate = date;
                            }
                            objRegistration.DocFormat = clientSetting.preSettings.itaDocumentFormat.ToUpper();
                            objRegistration.Filesize = clientSetting.preSettings.itaAttachFileSize;
                            objRegistration.ResponseDetails.ResponseCode = objGetViewResp.responseDetails.ResponseCode;
                            objRegistration.ResponseDetails.ResponseDesc = objGetViewResp.responseDetails.ResponseDesc;
                            objRegistration.IsPostpaid = "1";
                            objRegistration.accountNumber = objGetViewResp.subscriberdetail.accountNumber;

                            objRegistration.Filesize = clientSetting.preSettings.itaAttachFileSize;
                            iDocCount = (clientSetting.preSettings.itaSignedDoc != null && Convert.ToString(clientSetting.preSettings.itaSignedDoc).ToUpper() == "TRUE") ? 1 : 0;
                            if (!_runningFromNUnit)
                            {

                                if (clientSetting.preSettings.isITAAttachmentMand.ToUpper() == "TRUE")
                                {
                                    if (clientSetting.preSettings.itaMandatoryDoc != null && Convert.ToInt16(clientSetting.preSettings.itaMandatoryDoc) > 0)
                                    {
                                        if (iDocCount == 1)
                                            ErrorMsg = string.Format(Resources.ErrorResources.EReg_ITA_doc2, Convert.ToInt16(clientSetting.preSettings.itaMandatoryDoc));
                                        else
                                            ErrorMsg = string.Format(Resources.ErrorResources.EReg_ITA_doc1, Convert.ToInt16(clientSetting.preSettings.itaMandatoryDoc));

                                        iDocCount = iDocCount + Convert.ToInt16(clientSetting.preSettings.itaMandatoryDoc);
                                    }
                                    objRegistration.DocDetails = iDocCount.ToString() + "|" + ErrorMsg;
                                }
                                else
                                {
                                    objRegistration.DocDetails = "0|";
                                }
                            }
                        }
                        #endregion

                        #region Postpaid

                        if (objRegistration.IsPostpaid != null && objRegistration.IsPostpaid == "0")  // Postpaid
                        {
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "MSISDN->" + RegisterMsisdn);
                            objPostGetViewResp = serviceCRM.PostCRMGetSubscriberITA(objPostGetViewReq);
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "View Response->" + Convert.ToString(objPostGetViewResp));
                            if (objPostGetViewResp.ResponseDetails.ResponseCode != "15")
                            {

                                if (objPostGetViewResp.CustomerDetailITA.ChkAge.ToLower() == "false")
                                    objPostGetViewResp.CustomerDetailITA.ChkAge = "false";
                                else if (objPostGetViewResp.CustomerDetailITA.ChkAge.ToLower() == "true")
                                    objPostGetViewResp.CustomerDetailITA.ChkAge = "true";
                                else
                                    objPostGetViewResp.CustomerDetailITA.ChkAge = "false";

                                XmlDocument XMLSer = Common.CreateXML(objPostGetViewResp);
                                string XML = XMLSer.InnerXml;
                                objAddres.PostCode = objPostGetViewResp.CustomerDetailITA.PostCode;
                                objAddres.City = objPostGetViewResp.CustomerDetailITA.CityName;
                                objAddres.State = objPostGetViewResp.CustomerDetailITA.State;
                                objAddres.Locality = objPostGetViewResp.CustomerDetailITA.County;
                                objAddres.HouseNo = objPostGetViewResp.CustomerDetailITA.HouseNo;
                                objAddres.Line2 = objPostGetViewResp.CustomerDetailITA.Street;
                                objAddres.HouseName = objPostGetViewResp.CustomerDetailITA.AppartmentName; // house name is saved to appartment name in DB.

                                XML = Common.ReplaceMultiplestr(XML, "PostCustomerDetailITAResponse,<CustomerDetailITA>,</CustomerDetailITA>,dateOfBirth,birthPlace,Nationality,landLineNo,<responseDetails>,</responseDetails>", "Registration,,,DateOfBirth,PlaceofBirth,Nationality,ContactNumber,<ResponseDetails>,</ResponseDetails>");
                                objRegistration = (CRM.Models.Registration)Common.Deserialize(XML, objRegistration);

                                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "DeSerialize End");
                                objRegistration.objPostCustomerDetailITAResponse = objPostGetViewResp;
                                idPf.IDType = objPostGetViewResp.CustomerDetailITA.IdOptions;
                                idPf.IDNumber = objPostGetViewResp.CustomerDetailITA.IdNo;
                                idPf.Valitidy = objPostGetViewResp.CustomerDetailITA.IDValidateFullDate;
                                idPf.IDIssuedBy = objPostGetViewResp.CustomerDetailITA.Issuer;
                                idPf.IssuedDate = objPostGetViewResp.CustomerDetailITA.IssueDate;
                                objRegistration.IDProof = idPf;
                                if (!string.IsNullOrEmpty(objPostGetViewResp.CustomerDetailITA.SMSMARK))
                                {
                                    if (objPostGetViewResp.CustomerDetailITA.SMSMARK == "1")
                                        objRegistration.smsmak = true;
                                    else
                                        objRegistration.smsmak = false;
                                }
                                else
                                    objRegistration.smsmak = false;

                                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "After getting SMS mark value");
                                objRegistration.RetID = objPostGetViewResp.CustomerDetailITA.RetailerID;
                                objRegistration.chkTerms = Convert.ToBoolean(objPostGetViewResp.CustomerDetailITA.ChkTerms);
                                objRegistration.ContactNumber = objPostGetViewResp.CustomerDetailITA.LandlineNo != string.Empty ? objPostGetViewResp.CustomerDetailITA.LandlineNo : objPostGetViewResp.CustomerDetailITA.LandlineNo;
                                objRegistration.SIMNo = objPostGetViewResp.CustomerDetailITA.SIMNo.Trim();
                                objRegistration.Chkphoto = objPostGetViewResp.CustomerDetailITA.Chkphoto;
                                objRegistration.Others = objRegistration.Title + "," + objRegistration.Gender.ToUpper() + "," + objRegistration.PrefLanguage + "," + objRegistration.IDProof.IDType + "," + objRegistration.IDProof.IDNumber + "," + objRegistration.Nationality + "," + objRegistration.SecretQuestion + "," + objPostGetViewResp.CustomerDetailITA.Native;
                                objRegistration.DateOfBirth = objPostGetViewResp.CustomerDetailITA.DateOfBirth;
                                objRegistration.IDProof.IssuedDate = objPostGetViewResp.CustomerDetailITA.IssueFullDate;
                                objRegistration.LastName = objPostGetViewResp.CustomerDetailITA.LastName;

                                objRegistration.DocFormat = clientSetting.postSettings.itaDocumentFormat.ToUpper();
                                objRegistration.Filesize = clientSetting.postSettings.itaAttachFileSize;

                                CRMBase objBase = new CRMBase();
                                objBase.CountryCode = clientSetting.countryCode;
                                objBase.BrandCode = clientSetting.brandCode;
                                objBase.LanguageCode = clientSetting.langCode;
                                objRegistration.objITAPlanListResponse = crmService.LoadPlan(objBase);

                                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Get Bill Cycle Date from PBS");
                                GetPlansfromPBS objres = crmNewService.CRMGetPlansfromPBS(objBase);
                                objRegistration.drpdownBillCycle = objres.Billcycledatelist;
                                objRegistration.drpCorporate = objres.Corporatelist;
                                TempData["Corporate"] = objres.Corporatelist;

                                // objRegistration.
                                objRegistration.objPostCustomerDetailITAResponse.CustomerDetailITA.VAT_Reg_No = objPostGetViewResp.CustomerDetailITA.VAT_Reg_No;
                                objRegistration.objPostCustomerDetailITAResponse.CustomerDetailITA.AccountNumber = objPostGetViewResp.CustomerDetailITA.AccountNumber;
                                objRegistration.PUK = objPostGetViewResp.CustomerDetailITA.PUKCode;
                                objRegistration.objPostCustomerDetailITAResponse.CustomerDetailITA.BillCycleDate = objPostGetViewResp.CustomerDetailITA.BillCycleDate;
                                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "MSISDN->" + Convert.ToString(objPostGetViewResp.CustomerDetailITA.BillCycleDate));

                                objRegistration.DocFormat = clientSetting.postSettings.itaDocumentFormat.ToUpper();
                                objRegistration.Filesize = clientSetting.postSettings.itaAttachFileSize;
                                iDocCount = (clientSetting.postSettings.itaSignedDoc != null && Convert.ToString(clientSetting.postSettings.itaSignedDoc).ToUpper() == "TRUE") ? 1 : 0;

                                if (clientSetting.postSettings.isITAAttachmentMand.ToUpper() == "TRUE")
                                {

                                    if (iDocCount == 1)
                                        ErrorMsg = string.Format(Resources.ErrorResources.EReg_ITA_doc2, Convert.ToInt16(clientSetting.postSettings.itaMandatoryDoc));
                                    else
                                        ErrorMsg = string.Format(Resources.ErrorResources.EReg_ITA_doc1, Convert.ToInt16(clientSetting.postSettings.itaMandatoryDoc));

                                    iDocCount = iDocCount + Convert.ToInt16(clientSetting.postSettings.itaMandatoryDoc);

                                    objRegistration.DocDetails = iDocCount.ToString() + "|" + ErrorMsg;
                                }
                                else
                                {
                                    objRegistration.DocDetails = "-1|";
                                }
                            }
                            else
                            {
                                objRegistration.objITAPlanListResponse.reponseDetails.ResponseCode = objPostGetViewResp.ResponseDetails.ResponseCode;
                                objRegistration.objITAPlanListResponse.reponseDetails.ResponseDesc = objPostGetViewResp.ResponseDetails.ResponseDesc;
                            }
                            objRegistration.ResponseDetails.ResponseCode = objPostGetViewResp.ResponseDetails.ResponseCode;
                            objRegistration.ResponseDetails.ResponseDesc = objPostGetViewResp.ResponseDetails.ResponseDesc;
                            objRegistration.IsPostpaid = "0";
                        }

                        #endregion

                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "out side of Post and pre");

                        objRegistration.CommAddress = objAddres;
                        objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,6,7,8", objRegistration.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "Tbl_Nationality")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "Tbl_Nationality_EU")).ToList();

                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "after getting drop down values from DB");

                        objRegistration.Mode = "UPDATE";
                        string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                        objRegistration.CountryDateFormat = Datestring;
                        objRegistration.DateCalender = Datestring.ToLower().Replace("yyyy", "yy");
                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);

                        objRegistration.MSISDN = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;

                        #region Change Uploaded File Path
                        string sourcePath = "";
                        string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                        string targetPath = string.Empty;
                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            targetPath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                        }

                        //if (Server.MapPath("~/App_Data/UploadFile") != null)
                        //{
                        //    targetPath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                        //}
                        string filename, filePath = string.Empty;
                        string[] GetFileName = new string[3];
                        objRegistration.PostFileDetails.Clear();
                        if (objRegistration.PostFileDetails.Count() > 0)
                        {
                            #region Delete file from path
                            DeleteUploadedFile();
                            #endregion
                            for (int i = 0; i < objRegistration.PostFileDetails.Count(); i++)
                            {
                                filePath = objRegistration.PostFileDetails[i].Filepath;
                                if (!string.IsNullOrEmpty(filePath))
                                {
                                    sourcePath = objRegistration.PostFileDetails[i].Filepath;
                                }
                                else
                                {
                                    if (objRegistration.IsPostpaid != null && objRegistration.IsPostpaid == "1")  // Prepaid
                                        sourcePath = clientSetting.preSettings.registrationDocpath;
                                    if (objRegistration.IsPostpaid != null && objRegistration.IsPostpaid == "0")  // Postpaid
                                        sourcePath = clientSetting.postSettings.registrationDocpath;
                                }
                                if (clientSetting.mvnoSettings.internalUploadFile != null)
                                // if (Server.MapPath("~/App_Data/UploadFile") != null)
                                {
                                    if (!Directory.Exists(targetPath))
                                    {
                                        Directory.CreateDirectory(targetPath);
                                    }
                                    filename = objRegistration.PostFileDetails[i].Filename;
                                    if (System.IO.File.Exists(sourcePath + "\\" + filename))
                                    {
                                        System.IO.File.Copy(sourcePath + "\\" + filename, targetPath + "\\" + filename, true);
                                        GetFileName[i] = filename;
                                    }
                                }
                            }
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "GetfileFTP start");


                            if (objRegistration.PostFileDetails != null)
                            {
                                for (int i = 0; i < objRegistration.PostFileDetails.Count(); i++)
                                {
                                    GetFileName[i] = objRegistration.PostFileDetails[i].Filename;
                                }
                                GetfileFTP(targetPath + "\\", GetFileName, objRegistration, objRegistration.ResponseDetails.ResponseDesc);
                            }
                        }
                        #endregion
                    
                }

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,7,8", objRegistration.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "Tbl_Nationality")).ToList();
            }
            finally
            {
                serviceCRM = null;
            }
            return View("SubscriberRegistration_ITA", objRegistration);
        }
        #endregion

        #region ITA VIEW (Prepaid/Postpaid)
        public ActionResult SubscriberRegistrationPPView_ITA(string MSISDN)
        {
            return View();
        }

        public JsonResult GetSubscriberRegistrationPP_ITA(string Registration)
        {
            PostCustomerDetailITAResponse objRes = new PostCustomerDetailITAResponse();
            PostCustomerDetailsITARequest objReq = new PostCustomerDetailsITARequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objReq.MSISDN = Session["MobileNumber"].ToString();
                    objReq.CountryCode = clientSetting.countryCode;
                    objReq.BrandCode = clientSetting.brandCode;
                    objReq.LanguageCode = clientSetting.langCode;

                    #region File Upload

                    string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                    #region Create path

                    string registrationDocpath = clientSetting.preSettings.registrationDocpath;
                    if (!Directory.Exists(registrationDocpath))
                    {
                        Directory.CreateDirectory(registrationDocpath);
                    }
                    if (clientSetting.mvnoSettings.internalUploadFile != null)
                    //  if (Server.MapPath("~/App_Data/UploadFile") != null)
                    {
                        // string UploadFile = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                        string UploadFile = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                        if (!Directory.Exists(UploadFile))
                        {
                            Directory.CreateDirectory(UploadFile);
                        }
                    }

                    if (clientSetting.mvnoSettings.internalPdfDownload != null)
                    {
                        string PdfDownload = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                        if (!Directory.Exists(PdfDownload))
                        {
                            Directory.CreateDirectory(PdfDownload);
                        }
                    }
                    #endregion

                    #endregion


                    objRes = serviceCRM.CRMGetCustomerDetailsITA(objReq);
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.CustomerDetailITA);
                    CRMBase cbase = new CRMBase();
                    cbase.CountryCode = clientSetting.countryCode;
                    cbase.BrandCode = clientSetting.brandCode;
                    cbase.LanguageCode = clientSetting.langCode;

                    #region Change Uploaded File Path
                    string sourcePath = "";
                    FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                    string targetPath = string.Empty;

                    if (clientSetting.mvnoSettings.internalUploadFile != null)
                    {
                        targetPath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    }
                    //if (Server.MapPath("~/App_Data/UploadFile") != null)
                    //{
                    //    targetPath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                    //}
                    string filename, filePath = string.Empty;
                    string[] GetFileName = new string[3];

                    if (objRes.PostFileDetails.Count() > 0)
                    {
                        #region Delete file from path
                        DeleteUploadedFile();
                        #endregion
                        for (int i = 0; i < objRes.PostFileDetails.Count(); i++)
                        {
                            filePath = objRes.PostFileDetails[i].Filepath;
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                sourcePath = objRes.PostFileDetails[i].Filepath;
                            }
                            else
                            {
                                sourcePath = clientSetting.preSettings.registrationDocpath;
                            }
                            if (clientSetting.mvnoSettings.internalUploadFile != null)
                            //if (Server.MapPath("~/App_Data/UploadFile") != null)
                            {
                                if (!Directory.Exists(targetPath))
                                {
                                    Directory.CreateDirectory(targetPath);
                                }
                                filename = objRes.PostFileDetails[i].Filename;
                                if (System.IO.File.Exists(sourcePath + "\\" + filename))
                                {
                                    System.IO.File.Copy(sourcePath + "\\" + filename, targetPath + "\\" + filename, true);
                                    GetFileName[i] = filename;
                                }
                            }
                        }
                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "GetfileFTP start");


                        if (objRes.PostFileDetails != null)
                        {
                            for (int i = 0; i < objRes.PostFileDetails.Count(); i++)
                            {
                                GetFileName[i] = objRes.PostFileDetails[i].Filename;
                            }
                            GetfileFTP(targetPath + "\\", GetFileName, objReq, "");

                            string FolderNameitaly = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                            string pathitaly = string.Empty;
                            if (clientSetting.mvnoSettings.internalUploadFile != null)
                            {
                                pathitaly = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                            }
                            //if (Server.MapPath("~/App_Data/UploadFile") != null)
                            //{
                            //    pathitaly = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                            //}
                            string extractPath = pathitaly + '\\' + Session["MobileNumber"] + ".zip";
                            using (var zip = new Ionic.Zip.ZipFile())
                            {
                                zip.AddDirectory(targetPath);
                                zip.Save(extractPath);
                            }

                        }
                    }
                    #endregion



                    GetPlansfromPBS objResPBS = crmNewService.CRMGetPlansfromPBS(cbase);

                    if (objRes.CustomerDetailITA.BillCycleDate != "" && objRes.CustomerDetailITA.BillCycleDate != null)
                    {
                        if (objResPBS.Billcycledatelist.FindAll(b => b.ID == objRes.CustomerDetailITA.BillCycleDate).Count() > 0)
                        {
                            objRes.CustomerDetailITA.BillCycleDate = objResPBS.Billcycledatelist.FindAll(b => b.ID == objRes.CustomerDetailITA.BillCycleDate)[0].Text;
                        }
                    }
                    else
                    {
                        objRes.CustomerDetailITA.BillCycleDate = "";
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
            return Json(objRes);
        }

        public ActionResult SubscriberRegistrationView_ITA(string RegisterMsisdn, string Mode)
        {
            if (Convert.ToString(Session["isPrePaid"]).Equals("1"))
            {
                CRM.Models.Registration_ITA objViewReg = new CRM.Models.Registration_ITA();
                CustomerDetailsITARequest objGetViewRequest = new CustomerDetailsITARequest();
                CustomerDetailITAResponse objgetViewResp = new CustomerDetailITAResponse();
                //s
                ServiceInvokeCRM serviceCRM;
                try
                {
                    objGetViewRequest.CountryCode = clientSetting.countryCode;
                    objGetViewRequest.BrandCode = clientSetting.brandCode;
                    objGetViewRequest.LanguageCode = clientSetting.langCode;
                    objGetViewRequest.MSISDN = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        objgetViewResp = serviceCRM.CRMGetSubscriberITA(objGetViewRequest);
                    
                    if (objgetViewResp.responseDetails.ResponseCode == "0")
                    {
                        objViewReg.ResponseCode = objgetViewResp.responseDetails.ResponseCode;
                        objViewReg.ResponseDesc = objgetViewResp.responseDetails.ResponseDesc;
                        objViewReg.ITAGetSubscribReq = objgetViewResp.subscriberdetail;
                    }
                    else
                    {
                        objViewReg.ResponseCode = objgetViewResp.responseDetails.ResponseCode;
                        objViewReg.ResponseDesc = objgetViewResp.responseDetails.ResponseDesc;
                    }

                }
                catch (Exception Ex)
                {

                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, Ex);
                    objViewReg.ResponseCode = "2";
                    objViewReg.ResponseDesc = Ex.ToString();
                }
                finally
                {
                    serviceCRM = null;
                }
                return View("SubscriberRegistrationView_ITA", objViewReg);
            }
            else
            {
                return View("SubscriberRegistrationPPView_ITA");

            }
        }

        #endregion ITA VIEW (Prepaid/Postpaid)

        #region DeleteUploadedFile
        public void DeleteUploadedFile()
        {
            string regFilepath = "";
            string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
            if (clientSetting.mvnoSettings.internalUploadFile != null)
            // if (Server.MapPath("~/App_Data/UploadFile") != null)
            {
                regFilepath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                if (Directory.Exists(regFilepath))
                {
                    System.IO.DirectoryInfo di = new DirectoryInfo(regFilepath);

                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                }
            }
            if (clientSetting.mvnoSettings.internalPdfDownload != null)
            {
                var filepath2 = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                if (Directory.Exists(filepath2))
                {
                    System.IO.DirectoryInfo di = new DirectoryInfo(filepath2);

                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                }
            }
        }
        #endregion DeleteUploadedFile

        #region CreateUploadedDir
        public void CreateUploadedDir(string isPrePostpaid)
        {
            string registrationDocpath = "";
            if (isPrePostpaid != "" && isPrePostpaid == "1")   // Prepaid
            {
                registrationDocpath = clientSetting.preSettings.registrationDocpath;
            }
            if (isPrePostpaid != "" && isPrePostpaid == "0") // Postpaid
            {
                registrationDocpath = clientSetting.postSettings.registrationDocpath;
            }

            if (!Directory.Exists(registrationDocpath))
            {
                Directory.CreateDirectory(registrationDocpath);
            }
            if (clientSetting.mvnoSettings.internalUploadFile != null)
            // if (Server.MapPath("~/App_Data/UploadFile") != null)
            {
                string UploadFile = Path.Combine(clientSetting.mvnoSettings.internalUploadFile);
                //string UploadFile = Path.Combine(Server.MapPath("~/App_Data/UploadFile"));
                if (!Directory.Exists(UploadFile))
                {
                    Directory.CreateDirectory(UploadFile);
                }
            }

            if (clientSetting.mvnoSettings.internalPdfDownload != null)
            {
                string PdfDownload = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload);
                if (!Directory.Exists(PdfDownload))
                {
                    Directory.CreateDirectory(PdfDownload);
                }
            }
        }
        #endregion CreateUploadedDir

        #region GetIccidITA
        [HttpPost]
        public JsonResult GetIccidITA(string RegisterMsisdn)
        {
            PreGetIcccidReq objReg = JsonConvert.DeserializeObject<PreGetIcccidReq>(RegisterMsisdn);
            PreGetIcccidRes objResp = new PreGetIcccidRes();
            try
            {
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;

                objResp = crmNewService.CRMGetICCIDITA(objReg);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objResp.responseDetails.ResponseCode = "2";
                objResp.responseDetails.ResponseDesc = ex.ToString();
            }
            return Json(objResp);
        }
        #endregion

        #region Check Subscriber Details
        public JsonResult CheckSubscriberDts_ITA(GetSubscriberDetailsITAREQ objReq)
        {
            GetSubscriberDetailsITAResponse objRes = new GetSubscriberDetailsITAResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objReq.CountryCode = clientSetting.countryCode;
                    objReq.BrandCode = clientSetting.brandCode;
                    objReq.LanguageCode = clientSetting.langCode;
                    //objReq.DOB = Utility.FormatDateTimeToService(objReq.DOB, clientSetting.mvnoSettings.dateTimeFormat); 

                    objRes = serviceCRM.ITAGetSubDetails(objReq);

                    if (objRes.GetSubscriberDetailsita != null)
                    {
                        if (objRes.ResponseDetails.ResponseCode.Equals("0") && objRes.ResponseDetails.ResponseCode != null)
                        {
                            objRes.GetSubscriberDetailsita.dateOfBirth = Utility.GetDateconvertion(objRes.GetSubscriberDetailsita.dateOfBirth, clientSetting.mvnoSettings.dateTimeFormat, true, clientSetting.mvnoSettings.dateTimeFormat);
                            objRes.GetSubscriberDetailsita.idvalidityDate = Utility.GetDateconvertion(objRes.GetSubscriberDetailsita.idvalidityDate, clientSetting.mvnoSettings.dateTimeFormat, true, clientSetting.mvnoSettings.dateTimeFormat);
                            objRes.GetSubscriberDetailsita.issueDate = Utility.GetDateconvertion(objRes.GetSubscriberDetailsita.issueDate, clientSetting.mvnoSettings.dateTimeFormat, true, clientSetting.mvnoSettings.dateTimeFormat);
                        }
                    }
                    else
                    {
                        objRes.ResponseDetails.ResponseDesc = Resources.TopupResources.NoRecordsFound;
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
            return Json(objRes);
        }
        #endregion
        #endregion

        #region AUT
        public ActionResult SubscriberRegistrationEdit_AUT(string RegisterMsisdn, string Mode)
        {
            CRM.Models.Registration_AUT objReg_AUT = new CRM.Models.Registration_AUT();
            AUTGetSubscriberReq objGetViewReq = new AUTGetSubscriberReq();
            AUTGetSubscriberResp objGetViewResp = new AUTGetSubscriberResp();
            AutRegisterReq objRq = new AutRegisterReq();
            try
            {
                string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                objReg_AUT.CountryDateFormat = Datestring;
                objReg_AUT.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg_AUT.strDropdown = Utility.GetDropdownMasterFromDB("1,4,8,10,39", objReg_AUT.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg_AUT.IsPostpaid, "TblCountry")).ToList();

                if ((!string.IsNullOrEmpty(Mode)) && Mode.ToUpper().Trim() == "UPDATE")
                {
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
                    objGetViewReq.CountryCode = clientSetting.countryCode;
                    objGetViewReq.BrandCode = clientSetting.brandCode;
                    objGetViewReq.LanguageCode = clientSetting.langCode;
                    objGetViewReq.MSISDN = !string.IsNullOrEmpty(RegisterMsisdn) ? RegisterMsisdn : string.Empty;
                    objGetViewResp = crmService.viewAUT(objGetViewReq);
                    objReg_AUT.strMode = "UPDATE";
                    objReg_AUT.AutRegister_Req = objGetViewResp.AUTGetSubscriber;
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View("SubscriberRegistration_AUT", objReg_AUT);
        }

        public ActionResult SubscriberRegistration_AUT(string RegisterMsisdn, string Mode)
        {
            CRM.Models.Registration_AUT objReg_AUT = new CRM.Models.Registration_AUT();
            AUTGetSubscriberReq objGetViewReq = new AUTGetSubscriberReq();
            AUTGetSubscriberResp objGetViewResp = new AUTGetSubscriberResp();
            AutRegisterReq objRq = new AutRegisterReq();
            try
            {
                string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                objReg_AUT.CountryDateFormat = Datestring;
                objReg_AUT.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg_AUT.strDropdown = Utility.GetDropdownMasterFromDB("1,4,8,10,39", objReg_AUT.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg_AUT.IsPostpaid, "TblCountry")).ToList();
                AutRegisterReq objReq = new AutRegisterReq();
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
                objReg_AUT.MSISDN = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                objReg_AUT.strMode = "INSERT";
                objReg_AUT.AutRegister_Req = objReq;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objReg_AUT);
        }

        public JsonResult SaveResgistration_AUT(string Registration)
        {
            CRMResponse objRes = new CRMResponse();
            try
            {
                AutRegisterReq objReg = JsonConvert.DeserializeObject<AutRegisterReq>(Registration);
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                objReg.SendPrefLanguagetoRRBS = clientSetting.mvnoSettings.sendPrefLanguagetoRRBS;
                string DOB = Utility.GetDateconvertion(objReg.DateofBirth, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                string[] DOBSplit = DOB.Split('/');
                objReg.Birthdd = DOBSplit[0];
                objReg.Birthmm = DOBSplit[1];
                objReg.Birthyy = DOBSplit[2];
                objReg.chkEmail = "0";
                objReg.isPost = "0";
                objReg.IsGAF = clientSetting.mvnoSettings.useGAF;
                objReg.Poterminal = string.Empty;
                objReg.CSAgent = Convert.ToString(Session["UserName"]);
                objRes = crmService.InsertAUT(objReg);
                switch (objRes.ResponseCode)
                {
                    case "0":
                        if (objReg.Mode == "UPDATE")
                        {
                            objRes.ResponseDesc = Resources.ErrorResources.EUpdat_AUT_0;
                        }
                        else
                        {
                            objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_0;
                        }
                        break;
                    case "1":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_1;
                        break;
                    case "3":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_3;
                        break;
                    case "4":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_4;
                        break;
                    case "7":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_7;
                        break;
                    case "8":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_8;
                        break;
                    case "9":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_9;
                        break;
                    case "503":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_503;
                        break;
                    case "100":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_100;
                        break;
                    case "110":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_110;
                        break;
                    case "111":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_111;
                        break;
                    case "112":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_112;
                        break;
                    case "113":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_113;
                        break;
                    case "114":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_AUT_114;
                        break;

                    default:
                        objRes.ResponseDesc = Resources.ErrorResources.Common_2;
                        break;
                }

                if (objReg.Mode == "UPDATE" && (objRes.ResponseCode == "0" || objRes.ResponseCode == "100" || objRes.ResponseCode == "110" || objRes.ResponseCode == "111" || objRes.ResponseCode == "112" || objRes.ResponseCode == "113" || objRes.ResponseCode == "114"))
                {
                    Session["SubscriberTitle"] = objReg.Title;
                    Session["SubscriberName"] = objReg.FirstName + "|" + objReg.Lastname;
                    Session["DOB"] = DOB;
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objRes);
        }

        #region ViewAUT
        public ActionResult SubscriberRegistrationView_AUT(string RegisterMsisdn, string Mode)
        {
            CRM.Models.Registration_AUT objRegistration = new CRM.Models.Registration_AUT();
            AUTGetSubscriberReq objGetViewReq = new AUTGetSubscriberReq();
            AUTGetSubscriberResp objGetViewResp = new AUTGetSubscriberResp();
            AutRegisterReq objRq = new AutRegisterReq();
            try
            {
                objGetViewReq.CountryCode = clientSetting.countryCode;
                objGetViewReq.BrandCode = clientSetting.brandCode;
                objGetViewReq.LanguageCode = clientSetting.langCode;
                objGetViewReq.MSISDN = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                objGetViewResp = crmService.viewAUT(objGetViewReq);
                if (objGetViewResp.reponseDetails.ResponseCode == "0")
                {
                    objRegistration.AutRegister_Req = objGetViewResp.AUTGetSubscriber;
                }
                objRegistration.ResponseCode = objGetViewResp.reponseDetails.ResponseCode;
                objRegistration.ResponseDesc = objGetViewResp.reponseDetails.ResponseDesc;

            }
            catch (Exception Ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, Ex);
            }
            return View(objRegistration);
        }
        #endregion

        #endregion AUT

        #region GBR
        public ActionResult SubscriberRegistration_GBR(string RegisterMsisdn, string Mode)
        {
            CRM.Models.Registration_GBR objRegistration = new CRM.Models.Registration_GBR();
            ServiceCRM.GBRRegistration objReq = new ServiceCRM.GBRRegistration();
            ServiceCRM.ContactAddress commAdd = new ServiceCRM.ContactAddress();
            try
            {
                if (clientSetting.brandCode.ToUpper() == clientSetting.preSettings.ToggleBrandCode)
                {
                    return RedirectToAction("SubscriberRegistration_Toggle", "Registration", new { RegisterMsisdn = RegisterMsisdn, Mode = Mode });
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
                string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                objRegistration.CountryDateFormat = Datestring;
                objRegistration.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,10", objRegistration.IsPostpaid, "drop_master");
                objRegistration.MSISDN = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                objRegistration.DateCalender = Datestring.ToLower().Replace("yyyy", "yy");
                objRegistration.strMode = "INSERT";
                objReq.CommAddress = commAdd;
                objRegistration.GBRGetSubscribReq = objReq;
                string filepath = clientSetting.preSettings.registrationDocpath;
                if (Directory.Exists(filepath))
                {
                    Directory.Delete(filepath, true);
                }
                ViewBag.Title = @Resources.RegistrationResources.GBRRegistration;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objRegistration);
        }

        public JsonResult SaveResgistration_GBR(string Registration)
        {
            CRMResponse objRes = new CRMResponse();
            ServiceCRM.GBRRegistration objReq = new ServiceCRM.GBRRegistration();
            string Strmsisdn = Convert.ToString(Session["MobileNumber"]);
            try
            {
                Service.GBRRegistration objReg = JsonConvert.DeserializeObject<Service.GBRRegistration>(Registration);
                objReg.CountryDateFormat = clientSetting.mvnoSettings.dateTimeFormat;
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                StringBuilder sb = new StringBuilder();
                objReg.DateOfBirth = Utility.GetDateconvertion(objReg.DateOfBirth, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                objReg.SecretQuestion = string.Empty;
                objReg.SecretAnswer = string.Empty;
                objReg.CSAgent = Convert.ToString(Session["UserName"]);

                if (string.IsNullOrEmpty(Strmsisdn))
                {
                    objReg.ModeReg = "INSERT";
                }
                else
                {
                    objReg.ModeReg = "UPDATE";
                }
                objRes = crmService.InsertGBR(objReg);
                switch (objRes.ResponseCode)
                {
                    case "0":
                        if (objReg.ModeReg == "UPDATE")
                        {
                            objRes.ResponseDesc = Resources.ErrorResources.EUpdat_AUT_0;
                        }
                        else
                        {
                            objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR0;
                        }
                        break;
                    case "1":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR1;
                        break;
                    case "2":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR1;
                        break;
                    case "3":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR3;
                        break;
                    case "4":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR4;
                        break;
                    case "7":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR7;
                        break;

                    case "8":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR_8;
                        break;
                    case "40":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR40;
                        break;
                    case "110":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR110;
                        objRes.ResponseCode = "0";
                        break;
                    case "111":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR111;
                        objRes.ResponseCode = "0";
                        break;
                    case "112":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR112;
                        objRes.ResponseCode = "0";
                        break;
                    case "100":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR100;
                        objRes.ResponseCode = "0";
                        break;
                    case "113":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR_113;
                        objRes.ResponseCode = "0";
                        break;
                    case "114":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR_114;
                        objRes.ResponseCode = "0";
                        break;
                    case "503":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR_503;
                        break;
                    default:
                        objRes.ResponseDesc = Resources.ErrorResources.Common_2;
                        break;
                }
                if (objReg.ModeReg == "UPDATE" && (objRes.ResponseCode == "0" || objRes.ResponseCode == "100" || objRes.ResponseCode == "110" || objRes.ResponseCode == "111" || objRes.ResponseCode == "112" || objRes.ResponseCode == "113" || objRes.ResponseCode == "114"))
                {
                    Session["SubscriberTitle"] = objReg.Title;
                    Session["SubscriberName"] = objReg.FirstName + "|" + objReg.LastName;
                }

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objRes);
        }

        #region VIEWGBR
        public ActionResult SubscriberRegistrationView_GBR(string RegisterMsisdn, string Mode)
        {

            CRM.Models.Registration_GBR objRegistration = new CRM.Models.Registration_GBR();
            GBRGetSubscriberRequest objGetViewReq = new GBRGetSubscriberRequest();
            GBRGetSubscriberResponse objGetViewResp = new GBRGetSubscriberResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                if (clientSetting.brandCode.ToUpper() == clientSetting.preSettings.ToggleBrandCode)
                {
                    return RedirectToAction("SubscriberRegistrationView_Toggle_GBR", "Registration", new { RegisterMsisdn = RegisterMsisdn, Mode = Mode });
                }
                objGetViewReq.CountryCode = clientSetting.countryCode;
                objGetViewReq.BrandCode = clientSetting.brandCode;
                objGetViewReq.LanguageCode = clientSetting.langCode;
                objGetViewReq.MSISDN = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objGetViewResp = serviceCRM.ViewSubscribeGBR(objGetViewReq);
                

                if (objGetViewResp.reponseDetails.ResponseCode == "0")
                {
                    objRegistration.GBRGetSubscribReq = objGetViewResp.GBRGetSubscriber;
                    objRegistration.ResponseCode = objGetViewResp.reponseDetails.ResponseCode;
                    objRegistration.ResponseDesc = objGetViewResp.reponseDetails.ResponseDesc;
                }

            }
            catch (Exception Ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, Ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return View(objRegistration);
        }
        #endregion

        #region  Edit_GBR
        public ActionResult SubscriberRegistrationEdit_GBR(string RegisterMsisdn, string Mode)
        {
            CRM.Models.Registration_GBR objRegistration = new CRM.Models.Registration_GBR();
            GBRGetSubscriberResponse objGetViewResp = new GBRGetSubscriberResponse();
            GBRGetSubscriberRequest objGetViewReq = new GBRGetSubscriberRequest();
            //CRM.Models.Registration objRegistration = new CRM.Models.Registration();
            //s
            ServiceInvokeCRM serviceCRM;
            ViewBag.Title = @Resources.RegistrationResources.GBRRegistration;
            try
            {
                if (clientSetting.brandCode.ToUpper() == clientSetting.preSettings.ToggleBrandCode)
                {

                    return RedirectToAction("SubscriberRegistrationEdit_Toggle_GBR", "Registration", new { RegisterMsisdn = RegisterMsisdn, Mode = Mode });

                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
                string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                objRegistration.CountryDateFormat = Datestring;
                objRegistration.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,10", objRegistration.IsPostpaid, "drop_master");
                objRegistration.MSISDN = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                objRegistration.DateCalender = Datestring.ToLower().Replace("yyyy", "yy");
                if ((!string.IsNullOrEmpty(Mode)) && Mode.ToUpper().Trim() == "UPDATE")
                {
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
                    objGetViewReq.CountryCode = clientSetting.countryCode;
                    objGetViewReq.BrandCode = clientSetting.brandCode;
                    objGetViewReq.LanguageCode = clientSetting.langCode;
                    objGetViewReq.MSISDN = !string.IsNullOrEmpty(RegisterMsisdn) ? RegisterMsisdn : string.Empty;
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        objGetViewResp = serviceCRM.ViewSubscribeGBR(objGetViewReq);
                        objRegistration.strMode = "UPDATE";
                        objRegistration.GBRGetSubscribReq = objGetViewResp.GBRGetSubscriber;
                    
                }
                ViewBag.Title = @Resources.RegistrationResources.GBREditReg;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return View("SubscriberRegistration_GBR", objRegistration);
        }
        #endregion

        #endregion

        #region USA Registration
        public ActionResult SubscriberRegistration_USA(string RegisterMSISDN, string Mode)
        {
            Models.Registration_USA objReg = new Models.Registration_USA();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMSISDN);
                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.dd_City = Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "tbl_state");
                objReg.strDropdown = (Utility.GetDropdownMasterFromDB("1,4,8,10,39", objReg.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "TblCountry"))).OrderBy(o => o.Value).ToList();


                objReg.CountryDateFormat = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                #region USARegisterReq Model binding
                USARegisterReq USARegReq_normal = new USARegisterReq();
                USARegReq_normal.Msisdn = RegisterMSISDN != null ? RegisterMSISDN : string.Empty;
                objReg.USARegReq_normal = USARegReq_normal;
                objReg.simbox1_simple = clientSetting.mvnoSettings.simNumberPrefix;

                #endregion
                objReg.Mode = "INSERT";

            }
            catch (Exception ex) { CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex); }
            return View(objReg);
        }

        public ActionResult SubscriberRegistrationEdit_USA(string RegisterMSISDN, string Mode)
        {
            Models.Registration_USA objReg = new Models.Registration_USA();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMSISDN);
                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                List<Dropdown> objLstDropdown = new List<Dropdown>();
                objReg.dd_City = Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "tbl_state");
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,4,8,10,39", objReg.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "TblCountry")).ToList();
                ///edit
                if (Mode != null && Mode.Trim().ToUpper() == "UPDATE")
                {
                    #region Get CustomerDetails
                    CustomerDetailsUSAReq custObjReq = new CustomerDetailsUSAReq();
                    CustomerDetailsUSAResp custObjResp = new CustomerDetailsUSAResp();
                    custObjReq.BrandCode = clientSetting.brandCode;
                    custObjReq.CountryCode = clientSetting.countryCode;
                    custObjReq.LanguageCode = clientSetting.langCode;
                    custObjReq.MSISDN = RegisterMSISDN != null ? RegisterMSISDN : string.Empty;
                    #endregion
                    custObjResp = crmService.GetRegUSA_CustDetails(custObjReq);
                    objReg.USARegReq_normal = custObjResp.CustomerDetailssUSA;
                    objReg.Mode = Mode.Trim().ToUpper();
                    if (objReg.dd_City.FindAll(a => a.Value == objReg.USARegReq_normal.State).Count() == 0)
                    {
                        ServiceCRM.DropdownMaster objDrop = new DropdownMaster();
                        objDrop.Value = objReg.USARegReq_normal.State;
                        objDrop.ID = objReg.USARegReq_normal.State;
                        objReg.dd_City.Add(objDrop);
                    }
                }
            }
            catch (Exception ex) { CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex); }
            return View("SubscriberRegistration_USA", objReg);
        }

        public JsonResult SaveRegistration_USA(string Registration)
        {
            CRMResponse objRes = new CRMResponse();
            try
            {

                USARegisterReq objUSAReg = JsonConvert.DeserializeObject<USARegisterReq>(Registration);
                objUSAReg.CountryCode = clientSetting.countryCode;
                objUSAReg.BrandCode = clientSetting.brandCode;
                objUSAReg.LanguageCode = clientSetting.langCode;
                objUSAReg.CSAgent = Convert.ToString(Session["UserName"]);
                if (objUSAReg.DateOfBirth != string.Empty)
                {
                    string DOB = Utility.GetDateconvertion(objUSAReg.DateOfBirth, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    string[] DOBSplit = DOB.Split('/');
                    objUSAReg.BirthDD = DOBSplit[0];
                    objUSAReg.BirthMM = DOBSplit[1];
                    objUSAReg.BirthYYYY = DOBSplit[2];

                }
                else
                {
                    objUSAReg.BirthDD = string.Empty;
                    objUSAReg.BirthMM = string.Empty;
                    objUSAReg.BirthYYYY = string.Empty;
                }
                if (objUSAReg.Mode == "INSERT")
                {
                    objRes = crmService.InsertUSA_NormalReg(objUSAReg);
                }
                else if (objUSAReg.Mode == "UPDATE")
                {
                    objRes = crmService.InsertUSA_NormalReg(objUSAReg);
                    if (objRes.ResponseCode == "0")
                    {
                        objRes.ResponseCode = "00";
                    }
                }
                if (objRes != null && objRes.ResponseCode != null)
                {
                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_USA_" + objRes.ResponseCode);
                        objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;
                    }
                    if (objRes.ResponseCode == "00" && objUSAReg.Mode == "UPDATE")
                    {
                        Session["SubscriberTitle"] = objUSAReg.Title;
                        Session["SubscriberName"] = objUSAReg.FirstName + "|" + objUSAReg.LastName;
                        Session["DOB"] = objUSAReg.DateOfBirth;
                    }
                }
                else
                {
                    objRes.ResponseDesc = Resources.ErrorResources.Common_2;
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.ResponseCode = "2000";
                objRes.ResponseDesc = ex.ToString();
            }
            return Json(objRes);
        }

        public JsonResult DynamicMSISDNReg_USA(string Registration)
        {
            DynSIMAllocResp objRes = new DynSIMAllocResp();
            try
            {
                DynSIMAllocReq objUSAReg = JsonConvert.DeserializeObject<DynSIMAllocReq>(Registration);
                objUSAReg.CountryCode = clientSetting.countryCode;
                objUSAReg.BrandCode = clientSetting.brandCode;
                objUSAReg.LanguageCode = clientSetting.langCode.ToString();
                objUSAReg.SubmittedBy = Convert.ToString(Session["UserName"]);
                objUSAReg.UserName = Convert.ToString(Session["UserName"]);
                objUSAReg.Description = "Process Initiated";
                string[] ErrorCode = { "0", "150", "152", "151", "152", "153", "154", "155", "157", "158", "170", "180", "181", "182", "190", "200","797","798" };
                objRes = crmService.InsertUSA_SimpleReg(objUSAReg);
                if (objRes != null && objRes.ResponseDetails != null)
                {
                    StringBuilder errorInsertMsg = new StringBuilder();
                    string[] errorCode = objRes.ResponseDetails.ResponseCode.Split(',');
                    foreach (string str in errorCode)
                    {
                        if (Resources.ErrorResources.ResourceManager.GetString("EReg_USA_DNA_" + str) != string.Empty)
                            //errorInsertMsg.Append((!string.IsNullOrEmpty(errorInsertMsg.ToString()) ? " " + Resources.ErrorResources.EReg_USA_DNA_and + " " : "") + Resources.ErrorResources.ResourceManager.GetString("EReg_USA_DNA_" + str));
                            //errorInsertMsg.Append(!string.IsNullOrEmpty(errorInsertMsg.ToString()) ? " " + Resources.ErrorResources.EReg_USA_DNA_and + " " + Resources.ErrorResources.ResourceManager.GetString("EReg_USA_DNA_" + str) : Resources.ErrorResources.ResourceManager.GetString("EReg_USA_DNA_" + str));
                            errorInsertMsg.Append(string.IsNullOrEmpty(errorInsertMsg.ToString()) ? Resources.ErrorResources.ResourceManager.GetString("EReg_USA_DNA_" + str) : (" "+Resources.ErrorResources.EReg_USA_DNA_and +" " + Resources.ErrorResources.ResourceManager.GetString("EReg_USA_DNA_" + str)));
                        else
                            errorInsertMsg.Append(objRes.ResponseDetails.ResponseDesc);
                        if (ErrorCode.Contains(str))
                        {
                            objRes.ResponseDetails.ResponseCode = "0";
                        }
                        if (str == "202")
                        {
                            objRes.ResponseDetails.ResponseCode = "202";
                        }
                    }
                    objRes.ResponseDetails.ResponseDesc = errorInsertMsg.ToString();

                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objRes);
        }

        public ActionResult SubscriberRegistrationView_USA(string RegisterMSISDN, string regType)
        {
            Models.Registration_USA objReg = new Models.Registration_USA();

            #region View_USA Simple Registration
            //if (regType.Trim().ToUpper() == "SIMPLE")
            //{
            //   
            //    DynamicSIMReq dynSimReq = new DynamicSIMReq();
            //    DynamicSIMResp dynSimResp = new DynamicSIMResp();
            //    dynSimReq.BrandCode = clientSetting.brandCode;
            //    dynSimReq.CountryCode = clientSetting.countryCode;
            //    dynSimReq.LanguageCode = clientSetting.langCode.ToString();

            //    dynSimResp = crmService.GetUSA_SimpleReg(dynSimReq);
            //    objReg.USARegResp_simple = dynSimResp;
            //    objReg.viewType = regType.Trim().ToUpper();
            //   
            //}
            //else
            //{
            #endregion
            #region View_USA Nomal Registration
            CustomerDetailsUSAReq custObjReq = new CustomerDetailsUSAReq();
            CustomerDetailsUSAResp custObjResp = new CustomerDetailsUSAResp();
            custObjReq.BrandCode = clientSetting.brandCode;
            custObjReq.CountryCode = clientSetting.countryCode;
            custObjReq.LanguageCode = clientSetting.langCode;
            custObjReq.MSISDN = RegisterMSISDN;

            custObjResp = crmService.GetRegUSA_CustDetails(custObjReq);
            objReg.USARegReq_normal = custObjResp.CustomerDetailssUSA;
            objReg.viewType = "NORMAL";
            objReg.responseDetails = custObjResp.ResponseDetails;
            #endregion
            //}
            return View(objReg);
        }

        public JsonResult ReservePlanBZipcode(DynamicSIMReq dynSimReq)
        {
            DynamicSIMResp dynSimResp = new DynamicSIMResp();
            try
            {
                dynSimReq.BrandCode = clientSetting.brandCode;
                dynSimReq.CountryCode = clientSetting.countryCode;
                dynSimReq.LanguageCode = clientSetting.langCode.ToString();
                dynSimResp = crmService.GetUSA_SimpleReg(dynSimReq);

                dynSimResp.reponseDetails.ResponseCode = dynSimResp.reponseDetails.ResponseCode == "1" ? "300" : dynSimResp.reponseDetails.ResponseCode;
                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_USA_" + dynSimResp.reponseDetails.ResponseCode);
                dynSimResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? dynSimResp.reponseDetails.ResponseDesc : errorInsertMsg;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                dynSimReq = null;
            }
            return Json(dynSimResp);
        }

        public JsonResult LoadWholesalePlan()
        {
            CRMBase objReq = new CRMBase();
            objReq.CountryCode = clientSetting.countryCode;
            objReq.BrandCode = clientSetting.brandCode;
            objReq.LanguageCode = clientSetting.langCode.ToString();
            return Json(crmNewService.LoadWholeSalePlan(objReq));
        }

        public JsonResult ReservePlan()
        {
            FetchBundleDetailsRequest objReq = new FetchBundleDetailsRequest();
            objReq.CountryCode = clientSetting.countryCode;
            objReq.BrandCode = clientSetting.brandCode;
            objReq.LanguageCode = clientSetting.langCode.ToString();
            objReq.planId = string.Empty;
            return Json(crmNewService.CRMFetchBundleDetails(objReq));
        }
        #endregion

        #region File Upload
        [HttpPost]
        public JsonResult FileUploadone(string strDoc)
        {
            return Json(FileUpload("Doc1"));
        }
        [HttpPost]
        public JsonResult FileUploadTwo()
        {
            return Json(FileUpload("Doc2"));
        }
        [HttpPost]
        public JsonResult FileUploadThree()
        {
            return Json(FileUpload("Doc3"));
        }

        public AttachmentResponse FileUpload(string strDoc)
        {
            try
            {
                ArrayList obj = new ArrayList();
                string FileName = string.Empty;
                string FileNameWoEx = string.Empty;
                AttachmentResponse objres = new AttachmentResponse();
                //string filepath = clientSetting.preSettings.registrationDocpath;               
                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;

                if (clientSetting.mvnoSettings.internalUploadFile != null)
                // if (Server.MapPath("~/App_Data/UploadFile") != null)
                {
                    var filepath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    //var filepath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                    if (!Directory.Exists(filepath))
                    {
                        Directory.CreateDirectory(filepath);
                    }
                    string[] fileEntries = Directory.GetFiles(filepath);
                    foreach (string fileName in fileEntries)
                    {
                        FileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                        FileNameWoEx = FileName.Substring(0, FileName.LastIndexOf('.'));
                        obj.Add(FileNameWoEx.Substring(FileNameWoEx.Length - 2));
                    }
                    Attachement objAttach = null;
                    List<Attachement> objLisAttach = new List<Attachement>();
                    int FileCount = 1;
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (file != null && file.ContentLength > 0)
                        {
                            FileCount = obj.Contains("_1") && !obj.Contains("_2") ? 2 : obj.Contains("_1") && obj.Contains("_2") && !obj.Contains("_3") ? 3 : 1;
                            obj.Add("_" + FileCount);
                            var fileName = Path.GetFileName(file.FileName);
                            var Chkpath = Path.Combine(filepath, fileName.Substring(0, fileName.LastIndexOf('.')) + "_Chkpath" + Path.GetExtension(file.FileName));
                            var path = Path.Combine(filepath, fileName.Substring(0, fileName.LastIndexOf('.')) + "_" + FileCount.ToString() + Path.GetExtension(file.FileName));
                            if (!System.IO.File.Exists(Chkpath.Replace("_Chkpath", "_1")) && !System.IO.File.Exists(Chkpath.Replace("_Chkpath", "_2")) && !System.IO.File.Exists(Chkpath.Replace("_Chkpath", "_3")))
                            {
                                file.SaveAs(path);
                                objres.ResponseCode = "0";
                            }
                            else
                            {
                                objres.ResponseCode = "1";
                                objres.ResponseDesc = "File Already added";
                                return objres;
                            }
                            //file.SaveAs(path);
                        }

                    }
                    fileEntries = Directory.GetFiles(filepath);
                    foreach (string fileName in fileEntries)
                    {
                        FileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                        FileNameWoEx = FileName.Substring(0, FileName.LastIndexOf('.'));
                        //if (FileNameWoEx.EndsWith("_1") || FileNameWoEx.EndsWith("_2") || FileNameWoEx.EndsWith("_3"))// Removing other doc
                        //{
                        objAttach = new Attachement();
                        objAttach.Name = FileName;
                        objAttach.Externsion = fileName.Substring(fileName.LastIndexOf(".") + 1);
                        objAttach.FilePath = fileName;
                        objLisAttach.Add(objAttach);
                        //}
                    }
                    objres.objLstAttachement = objLisAttach;
                    return objres;
                }
                //return Json(objLisAttach);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return null;
        }

        [HttpPost]
        public JsonResult FileUpload1(string strDoc)
        {
            return Json(FileUploadcommon("Doc1"));
        }
        [HttpPost]
        public JsonResult FileUpload2()
        {
            return Json(FileUploadcommon("Doc2"));
        }
        [HttpPost]
        public JsonResult FileUpload3()
        {
            return Json(FileUploadcommon("Doc3"));
        }

        [HttpPost]
        public JsonResult FileUpload4()
        {
            return Json(FileUploadcommon("Doc4"));
        }

        [HttpPost]
        public JsonResult FileUpload5()
        {
            return Json(FileUploadcommon("Doc5"));
        }

        [HttpPost]
        public JsonResult FileUpload6()
        {
            return Json(FileUploadcommon("Doc6"));
        }

        public AttachmentResponse FileUploadcommon(string strDoc)
        {
            try
            {
                ArrayList obj = new ArrayList();
                string FileName = string.Empty;
                string FileNameWoEx = string.Empty;
                string fileadded = string.Empty;
                AttachmentResponse objres = new AttachmentResponse();
                //string filepath = clientSetting.preSettings.registrationDocpath;               
                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;

                if (clientSetting.mvnoSettings.internalUploadFile != null)
                // if (Server.MapPath("~/App_Data/UploadFile") != null)
                {
                    var filepath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    // var filepath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                    if (!Directory.Exists(filepath))
                    {
                        Directory.CreateDirectory(filepath);
                    }
                    string[] fileEntries = Directory.GetFiles(filepath);
                    foreach (string fileName in fileEntries)
                    {
                        FileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                        FileNameWoEx = FileName.Substring(0, FileName.LastIndexOf('.'));
                        obj.Add(FileNameWoEx.Substring(FileNameWoEx.Length - 2));
                    }
                    Attachement objAttach = null;
                    List<Attachement> objLisAttach = new List<Attachement>();
                    int FileCount = 1;
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (file != null && file.ContentLength > 0)
                        {
                            FileCount = obj.Contains("_1") && !obj.Contains("_2") ? 2 : obj.Contains("_1") && obj.Contains("_2") && !obj.Contains("_3") ? 3 : 1;
                            obj.Add("_" + FileCount);
                            var fileName = Path.GetFileName(file.FileName);
                            var Chkpath = Path.Combine(filepath, fileName.Substring(0, fileName.LastIndexOf('.')) + "_Chkpath" + Path.GetExtension(file.FileName));
                            var path = Path.Combine(filepath, fileName.Substring(0, fileName.LastIndexOf('.')) + "_" + FileCount.ToString() + Path.GetExtension(file.FileName));
                            fileadded = path;
                            if (!System.IO.File.Exists(Chkpath.Replace("_Chkpath", "_1")) && !System.IO.File.Exists(Chkpath.Replace("_Chkpath", "_2")) && !System.IO.File.Exists(Chkpath.Replace("_Chkpath", "_3")))
                            {
                                file.SaveAs(path);
                                objres.ResponseCode = "0";


                            }
                            else
                            {
                                objres.ResponseCode = "1";
                                objres.ResponseDesc = "File Already added";
                                return objres;
                            }
                            //file.SaveAs(path);
                        }

                    }
                    fileEntries = Directory.GetFiles(filepath);
                    foreach (string fileName in fileEntries)
                    {
                        if (fileadded.Contains(fileName))
                        {

                            FileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                            FileNameWoEx = FileName.Substring(0, FileName.LastIndexOf('.'));

                            objAttach = new Attachement();
                            objAttach.Name = FileName;
                            objAttach.Externsion = fileName.Substring(fileName.LastIndexOf(".") + 1);
                            objAttach.FilePath = fileName;
                            objLisAttach.Add(objAttach);
                        }



                    }
                    objres.objLstAttachement = objLisAttach;
                    return objres;
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return null;
        }

        public FileResult Downloadfilefrompath(string path, string FileName)
        {
            try
            {
                //if (Session["FilePath"].ToString() != "")
                //{
                if (clientSetting.countryCode.ToUpper() == "AUS")
                {
                    #region AUS
                    path = "";
                    path = clientSetting.preSettings.cposFilePath;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    #endregion
                }
                else
                {

                    if (path != "ITALYVIEW" && FileName != "ITALYVIEWAttachment")
                    {
                        FileName = path.Substring(path.LastIndexOf("\\") + 1);
                        string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                        //if (Server.MapPath("~/App_Data/UploadFile") != null)
                        //{
                        //    path = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                        //}

                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            path = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                        }
                    }
                    else
                    {
                        FileName = Session["MobileNumber"] + ".zip";
                        string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                        //if (Server.MapPath("~/App_Data/UploadFile") != null)
                        //{
                        //    path = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                        //}
                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            path = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                        }

                    }


                }

                if (path != null)
                {
                    path = path + '\\' + FileName;
                    if (System.IO.File.Exists(path))
                    {
                        var mimeType = ReturnExtension(FileName);
                        var fileDownloadName = FileName;
                        return File(path, mimeType, fileDownloadName);
                    }
                    else
                    {
                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "File is not exits in given path");
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return null;
            }
        }

        public JsonResult DeleteFile(string path)
        {
            try
            {
                string FileName = string.Empty;
                string FileNameWoEx = string.Empty;
                List<Attachement> objLisAttach = new List<Attachement>();
                System.IO.File.Delete(path);
                Attachement objAttach = null;
                string[] fileEntries = Directory.GetFiles(path.Substring(0, path.LastIndexOf("\\")));
                foreach (string fileName in fileEntries)
                {
                    FileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                    FileNameWoEx = FileName.Substring(0, FileName.LastIndexOf('.'));
                    if (FileNameWoEx.EndsWith("_1") || FileNameWoEx.EndsWith("_2") || FileNameWoEx.EndsWith("_3"))// Removing other doc
                    {
                        objAttach = new Attachement();
                        objAttach.Name = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                        objAttach.Externsion = fileName.Substring(fileName.LastIndexOf(".") + 1);
                        objAttach.FilePath = fileName;
                        objLisAttach.Add(objAttach);
                    }
                }
                return Json(objLisAttach);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return null;
        }

        public JsonResult EditDeleteFile(string path, string ID, string FileID)
        {
            try
            {
                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string ManualPath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    //if (Server.MapPath("~/App_Data/UploadFile") != null)
                    //{
                    //    string ManualPath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                    ManualPath = ManualPath + "\\" + path;

                    string FileName2 = string.Empty;
                    string FileNameWoEx2 = string.Empty;
                    List<Attachement> objLisAttach2 = new List<Attachement>();
                    System.IO.File.Delete(ManualPath);
                    Attachement objAttach2 = null;
                    string[] fileEntries2 = Directory.GetFiles(ManualPath.Substring(0, ManualPath.LastIndexOf("\\")));
                    foreach (string fileName2 in fileEntries2)
                    {
                        FileName2 = fileName2.Substring(fileName2.LastIndexOf("\\") + 1);
                        FileNameWoEx2 = FileName2.Substring(0, FileName2.LastIndexOf('.'));
                        if (FileNameWoEx2.EndsWith("_1") || FileNameWoEx2.EndsWith("_2") || FileNameWoEx2.EndsWith("_3"))// Removing other doc
                        {
                            objAttach2 = new Attachement();
                            objAttach2.Name = fileName2.Substring(fileName2.LastIndexOf("\\") + 1);
                            objAttach2.Externsion = fileName2.Substring(fileName2.LastIndexOf(".") + 1);
                            objAttach2.FilePath = fileName2;
                            objLisAttach2.Add(objAttach2);
                        }
                    }

                    return Json(objLisAttach2);
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return null;
        }

        public JsonResult RemoveAllFiles()
        {
            try
            {
                // string path = clientSetting.preSettings.registrationDocpath;
                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    var path = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string path = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);

                    List<Attachement> objLisAttach = new List<Attachement>();
                    if (Directory.Exists(path))
                    {
                        System.IO.DirectoryInfo di = new DirectoryInfo(path);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                    }

                    return Json(objLisAttach);
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return null;
        }

        #endregion
        //FRR 4788
        [HttpPost]
        public JsonResult BundleInfo(string bundleCode, string msisdn, string bundleName)
        {
            BundleInfoResponse bundleInfoResp = new BundleInfoResponse();
            BundleInfoRequest bundleInfoReq = new BundleInfoRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                bundleInfoReq.CountryCode = clientSetting.countryCode;
                bundleInfoReq.BrandCode = clientSetting.brandCode;
                bundleInfoReq.LanguageCode = clientSetting.langCode;

                bundleInfoReq.MSISDN = msisdn;
                bundleInfoReq.bundleCode = bundleCode +"|"+ bundleName;
                

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    bundleInfoResp = serviceCRM.BundleInfoCRMPP(bundleInfoReq);
                   // bundleInfoResp = serviceCRM.BundleAllowancePostpaid(bundleInfoReq);
                
                if (bundleInfoResp != null && bundleInfoResp.bundleProp != null && bundleInfoResp.bundleProp.Expirydate != null)
                {
                    bundleInfoResp.bundleProp.Expirydate = Utility.GetDateconvertion(bundleInfoResp.bundleProp.Expirydate, "DD-MM-YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);
                }
                if (bundleInfoResp != null && bundleInfoResp.responseDetails != null && bundleInfoResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BundleInfo_" + bundleInfoResp.responseDetails.ResponseCode);
                    bundleInfoResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? bundleInfoResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                return Json(bundleInfoResp.bundleProp);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(bundleInfoResp.bundleProp);
            }
            finally
            {
                //bundleInfoResp = null;
                bundleInfoReq = null;
                serviceCRM = null;
            }

        }
        private string ReturnExtension(string Filename)
        {
            string fileExtension = Filename.Substring(Filename.LastIndexOf('.'));
            switch (fileExtension)
            {
                case ".htm":
                case ".html":
                case ".log":
                    return "text/HTML";
                case ".txt":
                    return "text/plain";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".doc":
                    return "application/msword";
                case ".tiff":
                case ".tif":
                    return "image/tiff";
                case ".asf":
                    return "video/x-ms-asf";
                case ".avi":
                    return "video/avi";
                case ".zip":
                    return "application/zip";
                case ".xls":
                case ".csv":
                    return "application/vnd.ms-excel";
                case ".gif":
                    return "image/gif";
                case ".jpg":
                case "jpeg":
                    return "image/jpeg";
                case ".bmp":
                    return "image/bmp";
                case ".wav":
                    return "audio/wav";
                case ".mp3":
                    return "audio/mpeg3";
                case ".mpg":
                case "mpeg":
                    return "video/mpeg";
                case ".rtf":
                    return "application/rtf";
                case ".asp":
                    return "text/asp";
                case ".pdf":
                    return "application/pdf";
                case ".fdf":
                    return "application/vnd.fdf";
                case ".ppt":
                    return "application/mspowerpoint";
                case ".dwg":
                    return "image/vnd.dwg";
                case ".msg":
                    return "application/msoutlook";
                case ".xml":
                case ".sdxl":
                    return "application/xml";
                case ".xdp":
                    return "application/vnd.adobe.xdp+xml";
                default:
                    return "application/octet-stream";
            }

        }

        public JsonResult GetBunldelist(string PlanCodeRequest)
        {
            PlanMappedBundlesResponse objRes = new PlanMappedBundlesResponse();
            try
            {
                PlanMappedBundles objPlanCodeRequest = JsonConvert.DeserializeObject<PlanMappedBundles>(PlanCodeRequest);
                objPlanCodeRequest.CountryCode = clientSetting.countryCode;
                objPlanCodeRequest.BrandCode = clientSetting.brandCode;
                objPlanCodeRequest.LanguageCode = clientSetting.langCode;
                // objRes = crmService.LoadTax(objPlanCodeRequest);
                objRes = crmNewService.CRMPostpaidMappedBundles(objPlanCodeRequest);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objRes);
        }

        #region PDF
        public FileResult DownloadPDF()
        {
            //string path = clientSetting.preSettings.registrationDocpath;
            string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
            string path = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
            string FileName = Convert.ToString(Session["PDFfileName1"]);
            var mimeType = ReturnExtension(FileName);
            var fileDownloadName = FileName;
            return File(path + "\\" + FileName, mimeType, fileDownloadName);
        }

        [ValidateInput(false)]
        public JsonResult GenPdf(string HTML, string LastName, string ICCID, string Mode)
        {
            try
            {
                Document document = new Document(PageSize.A4, 50, 50, 20, 20);
                StyleSheet styles = new StyleSheet();

                styles.LoadTagStyle(HtmlTags.TABLE, "width", "90%");
                styles.LoadTagStyle(HtmlTags.TABLE, "align", "left");
                styles.LoadTagStyle(HtmlTags.TABLE, "cellpadding", "0");
                styles.LoadTagStyle(HtmlTags.TABLE, "cellspacing", "0");
                string PDFfileName1 = DateTime.Now.ToString("ddMMyyyy_hh_mm_ss_fff") + "_" + ICCID + "_" + LastName + ".pdf";
                Session["PDFfileName1"] = PDFfileName1;

                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                var filepath = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                string PDFfileName = filepath + "\\" + PDFfileName1;
                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }
                PdfWriter.GetInstance(document, new FileStream(PDFfileName, FileMode.Create));
                document.Open();

                #region Image


                if (clientSetting.countryCode == "ITA")
                {
                    string logoPath = @"~\Library\DefaultTheme\Images\PdfLogo" + clientSetting.mvnoSettings.pdfLogoCountrycode + ".png";
                    iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(Server.MapPath(logoPath));
                    pdfImage.ScaleToFit(100, 80);
                    pdfImage.Alignment = iTextSharp.text.Image.UNDERLYING;
                    pdfImage.SetAbsolutePosition(400, 770);
                    document.Add(pdfImage);
                }


                #region TUN
                if (clientSetting.countryCode == "TUN")
                {
                    if (clientSetting.mvnoSettings.enablePDFLogo.ToUpper() == "TRUE")
                    {
                        string logoPath = @"~\Library\DefaultTheme\Images\PdfLogo" + clientSetting.mvnoSettings.pdfLogoCountrycode + ".png";
                        iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(Server.MapPath(logoPath));
                        pdfImage.ScaleToFit(100, 80);
                        pdfImage.Alignment = iTextSharp.text.Image.UNDERLYING;
                        pdfImage.SetAbsolutePosition(400, 770);
                        document.Add(pdfImage);
                    }
                    iTextSharp.text.Image pdfLineImage = iTextSharp.text.Image.GetInstance(Server.MapPath(@"~\Library\DefaultTheme\Images\line.png"));
                    pdfLineImage.ScaleToFit(500, 10);
                    pdfLineImage.Alignment = iTextSharp.text.Image.UNDERLYING;
                    pdfLineImage.SetAbsolutePosition(50, 710);
                    document.Add(pdfLineImage);

                    //iTextSharp.text.Image pdfLineImage2 = iTextSharp.text.Image.GetInstance(Server.MapPath(@"~\Library\DefaultTheme\Images\line.png"));
                    //pdfLineImage2.ScaleToFit(500, 10);
                    //pdfLineImage2.Alignment = iTextSharp.text.Image.UNDERLYING;
                    //pdfLineImage2.SetAbsolutePosition(50, 220);
                    //document.Add(pdfLineImage2);
                }
                #endregion

                #region ESP
                if (clientSetting.countryCode == "ESP")
                {
                    if (clientSetting.mvnoSettings.enablePDFLogo.ToUpper() == "TRUE")
                    {
                        string logoPath = @"~\Library\DefaultTheme\Images\PdfLogo" + clientSetting.mvnoSettings.pdfLogoCountrycode + ".png";
                        iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(Server.MapPath(logoPath));
                        int imgwidth = Convert.ToInt32(clientSetting.preSettings.lOGO_SIZE);// Int32.Parse(clientSetting.preSettings.lOGO_SIZE);
                        imgwidth = imgwidth > 300 ? 300 : imgwidth;
                        imgwidth = imgwidth < 100 ? 100 : imgwidth;
                        pdfImage.ScaleToFit(imgwidth, 80);

                        pdfImage.Alignment = iTextSharp.text.Image.UNDERLYING;

                        string alignment = clientSetting.preSettings.lOGO_POSITION.ToLower();

                        if (alignment == "left")
                            pdfImage.SetAbsolutePosition(50, 770);
                        else if (alignment == "center")
                            pdfImage.SetAbsolutePosition(250, 770);
                        else
                            pdfImage.SetAbsolutePosition(430, 770);
                        document.Add(pdfImage);
                    }
                    iTextSharp.text.Image pdfLineImage = iTextSharp.text.Image.GetInstance(Server.MapPath(@"~\Library\DefaultTheme\Images\line.png"));
                    pdfLineImage.ScaleToFit(500, 10);
                    pdfLineImage.Alignment = iTextSharp.text.Image.UNDERLYING;
                    pdfLineImage.SetAbsolutePosition(50, 750);
                    document.Add(pdfLineImage);
                }
                #endregion

                #region Common Postpaid
                if (Mode == "Common Postpaid")
                {
                    if (clientSetting.mvnoSettings.enablePDFLogo.ToUpper() == "TRUE")
                    {
                        string logoPath = @"~\Library\DefaultTheme\Images\PdfLogoCommonPostpaid.png";
                        iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(Server.MapPath(logoPath));
                        pdfImage.ScaleToFit(100, 80);
                        pdfImage.Alignment = iTextSharp.text.Image.UNDERLYING;
                        pdfImage.SetAbsolutePosition(400, 770);
                        document.Add(pdfImage);
                    }
                    //iTextSharp.text.Image pdfLineImage = iTextSharp.text.Image.GetInstance(Server.MapPath(@"~\Library\DefaultTheme\Images\line.png"));
                    //pdfLineImage.ScaleToFit(500, 10);
                    //pdfLineImage.Alignment = iTextSharp.text.Image.UNDERLYING;
                    //pdfLineImage.SetAbsolutePosition(50, 750);
                    //document.Add(pdfLineImage);
                }
                #endregion

                #region SWI
                if (clientSetting.countryCode == "SWI")
                {
                    if (clientSetting.mvnoSettings.enablePDFLogo.ToUpper() == "TRUE")
                    {
                        string logoPath = @"~\Library\DefaultTheme\Images\PdfLogo" + clientSetting.mvnoSettings.pdfLogoCountrycode + ".png";
                        iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(Server.MapPath(logoPath));
                        pdfImage.ScaleToFit(100, 80);
                        pdfImage.Alignment = iTextSharp.text.Image.UNDERLYING;
                        pdfImage.SetAbsolutePosition(50, 765);
                        document.Add(pdfImage);
                    }
                    iTextSharp.text.Image pdfLineImage = iTextSharp.text.Image.GetInstance(Server.MapPath(@"~\Library\DefaultTheme\Images\line.png"));
                    pdfLineImage.ScaleToFit(500, 10);
                    pdfLineImage.Alignment = iTextSharp.text.Image.UNDERLYING;
                    pdfLineImage.SetAbsolutePosition(50, 750);
                    document.Add(pdfLineImage);
                }
                #endregion

                #endregion

                HTML = HTML.Replace(@"\n", string.Empty);
                HTML = HTML.Replace(@"\", string.Empty);
                ArrayList htmlArrList = HTMLWorker.ParseToList(new StringReader(HTML.Substring(1, HTML.Length - 2)), styles);
                foreach (IElement strLn in htmlArrList)
                {
                    document.Add(strLn);
                }
                document.Close();

                #region Change Login Language
                if (Mode == "Eng PDF")
                {
                    string cultureCode = Session["UserLanguage"].ToString();
                    HttpCookie cookieCultureLanguage = new HttpCookie("UserLanguage") { Value = cultureCode };
                    Response.Cookies.Set(cookieCultureLanguage);
                    Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(cultureCode);
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(cultureCode);
                }
                #endregion


                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Generate PDF");
                return Json("Success");
            }
            catch (Exception exx)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exx);
                Console.Error.WriteLine(exx.StackTrace);
                Console.Error.WriteLine(exx.Message);
                return Json("Failed");
            }
        }
        #endregion


        #region FRR-5022

        public JsonResult pdfcreation(MNPPDFRequest pdfreq)
        {
            string pdfFilePath = string.Empty;
            StringBuilder sbpdf = new StringBuilder();
            string contents = string.Empty;
            string FolderName = string.Empty;
            string path = string.Empty;
            pdfdetails pdfdetails = new pdfdetails();
            ServiceInvokeCRM serviceCRM;
            try
            {

                pdfreq.CountryCode = clientSetting.countryCode;
                pdfreq.LanguageCode = clientSetting.langCode;
                pdfreq.BrandCode = clientSetting.brandCode;

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Registration controller - CRMPDFDetails  Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                pdfdetails = serviceCRM.CRMPDFDetails(pdfreq);

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "pdfcreation-method start");
                if (pdfreq.LanguageCode.ToUpper() == "EN")
                {
                     path = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Customer/ENG_MNPFORM.html");
                }
                else
                {
                     path = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Customer/MNPFORM.html");
                }
                System.IO.StreamReader reader = null;
                FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                pdfFilePath = clientSetting.mvnoSettings.internalPdfDownload + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss_fff") +FolderName + pdfreq.ICCID+".pdf";
                if (System.IO.File.Exists(path))
                {
                    reader = new System.IO.StreamReader(path);
                    contents = reader.ReadToEnd();
                    reader.Close();
                    sbpdf.Append(contents);
                }
                else
                {
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "File not available");

                }
                contents = contents.Replace("  #", "#");
                contents = contents.Replace("\r\n", "");
                contents = contents.Replace(" <td>", ""); 
                contents = contents.Replace("#Firstname#", pdfreq.firstname);
                contents = contents.Replace("#currentoperator#", pdfreq.currentoperator);
                contents = contents.Replace("#Fiscalcode#", pdfreq.Fiscalcode);
                contents = contents.Replace("#surname#", pdfreq.surname);
                contents = contents.Replace("#placeofbirth#", pdfreq.PlaceofBirth);
                contents = contents.Replace("#ICCID#", pdfreq.ICCID);
                contents = contents.Replace("#piccid#", pdfreq.Piccid);
                contents = contents.Replace("#Street#", pdfdetails.Street);
                contents = contents.Replace("#province#", pdfdetails.Province);
                contents = contents.Replace("#postalcode#", pdfdetails.Postcode);




                string str1 = contents.Replace("\r\n", "");

                Encoding Utf8 = Encoding.UTF8;
                byte[] utf8Bytes_1 = Utf8.GetBytes(str1); // Unicode -> UTF-8
                string Decodestr1 = Utf8.GetString(utf8Bytes_1); // Correctly decode as UTF-8 

                SelectPdf.HtmlToPdf converter = new SelectPdf.HtmlToPdf();
                SelectPdf.PdfDocument doc = null;
                SelectPdf.GlobalProperties.HtmlEngineFullPath = System.Web.HttpContext.Current.Server.MapPath("~/bin/Select.Html.dep");
                doc = converter.ConvertHtmlString(contents.ToString(), "");
                doc.Save(pdfFilePath);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Pdf Generate successfully");

                return Json("Success");
            }
            catch(Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json("Failure");
            }
            finally
            {
                pdfFilePath = null;
                contents = null;
                sbpdf = null;
               
            }
           
        }

        #endregion

        public string RenderViewAsString(string viewName, object model)
        {
            // create a string writer to receive the HTML code
            StringWriter stringWriter = new StringWriter();

            // get the view to render
            ViewEngineResult viewResult = ViewEngines.Engines.FindView(ControllerContext, viewName, null);
            // create a context to render a view based on a model
            ViewContext viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    new ViewDataDictionary(model),
                    new TempDataDictionary(),
                    stringWriter
                    );

            // render the view to a HTML code
            viewResult.View.Render(viewContext, stringWriter);

            // return the HTML code
            return stringWriter.ToString();
        }

        #region FRR-4788
        public ViewResult PostpaidPlanPurchase(string msisdn, string planname, string bundlecode, string activationamount, string monthlyrental, string terminationfees, string customerid, string mode, string contaction, string apmamount)
        {
            PayGFamilyBundle ObjResp = new PayGFamilyBundle();
            Dropdown ObjDrop = new Dropdown();
            string DirectDebitPayemtMode = string.Empty;
            try
            {
                ObjResp.FamilyAccID = planname;
                ObjResp.FamilyStatus = activationamount;
                ObjResp.EmailLanguage = bundlecode;
                ObjResp.FamilyAutoRenewalCount = monthlyrental;
                ObjResp.ReferenceNumber = terminationfees;
                ObjResp.LoginMsisdnIndicator = msisdn;
                ObjResp.TotalAmount = customerid;
                ObjResp.NextRenewalCost = apmamount;
                ObjResp.VatPerc = mode;
                DataSet ds = Utility.BindXmlFile("~/App_Data/CountryListSepaCheckout.xml");
                ObjResp.objcountrydd = new Models.CountryDropdown();
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
                ObjResp.objcountrydd.CountryDD = objLstDrop;

                ViewBag.IsRedirect = "1";
                if (mode != "" && contaction == "PostpaidPlanPurchaseAPM")
                {
                    ViewBag.IsRedirect = "2";
                }
                //4834
                if (mode != "" && mode == "NEW_AUTORENEWAL")
                {
                    ViewBag.IsRedirect = "3";
                }
                else if ((planname == "" || planname == null) && contaction != "PostpaidPlanPurchaseAPM")
                {
                    ViewBag.IsRedirect = "0";                  
                }                 
                return View(ObjResp);
                
            }
            catch (Exception ex)
            {
                return View("PostpaidPlanPurchase");
            }
        }    
        //public ViewResult PostpaidPlanPurchase(string isredirect)
        //{
        //    try
        //    {
        //        ViewBag.IsRedirect = "0";          
        //        return View("PostpaidPlanPurchase");
        //    }
        //    catch (Exception ex)
        //    {

        //        return View("PostpaidPlanPurchase");
        //    }
        //}

        public JsonResult PostpaidPlanPurchaseCRMEncodeDecode(string getpostpaidplanpurchase)
        {
            PostpaidPlanPurchaseResponse ObjResp = new PostpaidPlanPurchaseResponse();
            PostpaidPlanPurchaseRequest ObjReq = JsonConvert.DeserializeObject<PostpaidPlanPurchaseRequest>(getpostpaidplanpurchase);
             EncodeDecode objReq = new EncodeDecode();  
             objReq.CountryCode = clientSetting.countryCode;
             objReq.BrandCode = clientSetting.brandCode;
             objReq.LanguageCode = clientSetting.langCode;
             string ErrorCode = string.Empty;
             string ErrorMsg = string.Empty;
             string TransactioID = string.Empty;
             string Orderid = string.Empty;
             EncodeDecoderes objRes = new EncodeDecoderes();
            try
            {
                objReq.returnUrl = ObjReq.IssuerId;
                objRes = crmNewService.GetDecodeValue(objReq);
                ErrorCode = objRes.hashval.FindAll(a => a.ID == "ResponseCode").Count > 0 ? objRes.hashval.FindAll(a => a.ID == "ResponseCode")[0].Values : objRes.responseDetails.ResponseCode;
                ErrorMsg = objRes.hashval.FindAll(a => a.ID == "ResponseMessage").Count > 0 ? objRes.hashval.FindAll(a => a.ID == "ResponseMessage")[0].Values : objRes.responseDetails.ResponseDesc;
                TransactioID = objRes.hashval.FindAll(a => a.ID == "TransId").Count > 0 ? objRes.hashval.FindAll(a => a.ID == "TransId")[0].Values : string.Empty;
                Orderid = objRes.hashval.FindAll(a => a.ID == "PaymentOrderID").Count > 0 ? objRes.hashval.FindAll(a => a.ID == "PaymentOrderID")[0].Values : string.Empty;
                ObjResp.RescodeAPM = ErrorCode;
                ObjResp.ResmsgAPM = ErrorMsg;
                ObjResp.TranIdAPM = TransactioID;
                ObjResp.OrderIdAPM = Orderid;                
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "Registration - PostpaidPlanPurchase - " + this.ControllerContext, ex);
            }
            finally
            {
                objRes = null;
       
            }
            return new JsonResult() { Data = ObjResp, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult GetDDCardDetails()
        {
            PayGFamilyBundle ObjResp = new PayGFamilyBundle();
            ObjResp.lstDirectdebitCardDetails = GetDebitCardDetails();
            return Json(ObjResp);

        }
        public Dictionary<string, string> GetDebitCardDetails()
        {

            PayGFamilyBundle ObjResp = new PayGFamilyBundle();
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
                        ObjResp.lstDirectdebitCardDetails.Add(manageCardResp.debitcardDetails[i].cardId + ","
                            + manageCardResp.debitcardDetails[i].cardNumber +","
                             + manageCardResp.debitcardDetails[i].nameOnCard+","
                            + manageCardResp.debitcardDetails[i].Country + "," 
                            + manageCardResp.debitcardDetails[i].addressline1 + "," 
                            + manageCardResp.debitcardDetails[i].addressline2 + "," 
                            + manageCardResp.debitcardDetails[i].postcode + ","
                            + manageCardResp.debitcardDetails[i].email + ","
                            + manageCardResp.debitcardDetails[i].city , manageCardResp.debitcardDetails[i].cardNumber);
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

        public JsonResult GetDDCardDetailsSEPA()
        {
            PayGFamilyBundle ObjResp = new PayGFamilyBundle();
            ObjResp.lstDirectdebitCardDetails = GetDebitCardDetailsSEPA();
            return Json(ObjResp);

        }


        //4785
        public Dictionary<string, string> GetDebitCardDetailsSEPA()
        {

            PayGFamilyBundle ObjResp = new PayGFamilyBundle();
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
                        ObjResp.lstDirectdebitCardDetails.Add(manageCardResp.debitcardDetails[i].cardId + "|"
                            +manageCardResp.debitcardDetails[i].cardNumber + "|"
                             + manageCardResp.debitcardDetails[i].nameOnCard + "|"
                            + manageCardResp.debitcardDetails[i].Country + "|"
                            + manageCardResp.debitcardDetails[i].addressline1 + "|"
                            + manageCardResp.debitcardDetails[i].postcode + "|"
                            + manageCardResp.debitcardDetails[i].email + "|"
                            + manageCardResp.debitcardDetails[i].city + "|" +manageCardResp.debitcardDetails[i].CountryCode, manageCardResp.debitcardDetails[i].cardNumber);
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

        public JsonResult PostpaidPlanPurchaseCRM(string getpostpaidplanpurchase)
        {
            PostpaidPlanPurchaseResponse ObjResp = new PostpaidPlanPurchaseResponse();
            PostpaidPlanPurchaseRequest ObjReq = JsonConvert.DeserializeObject<PostpaidPlanPurchaseRequest>(getpostpaidplanpurchase);
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            string returnurl;
            string keyframe="?Keyframe=";
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "RegistrationController - PostpaidPlanPurchase Start");
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                if (ObjReq.PaymentMode == "APM")
                {
                    returnurl = Request.UrlReferrer.ToString().TrimEnd('?') + keyframe + Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + "NEW_REGN" + "|" + ObjReq.MSISDN + "|" + "PostpaidPlanPurchaseAPM" + "|" + ObjReq.PostpaidPlanName + "|" + ObjReq.BillingCycle + "|" + ObjReq.Amount + "|" + ObjReq.PromoCode;
                    ObjReq.TypeOfPayment = returnurl;
                    if (!string.IsNullOrEmpty(ObjReq.Firstname))
                        ObjReq.FirstName = ObjReq.Firstname;
                    if (!string.IsNullOrEmpty(ObjReq.Lastname))
                        ObjReq.LastName = ObjReq.Lastname;
                }
                // 4781
                else if (ObjReq.Mode == "Trustlypay" && ObjReq.Mode1 == "Trustlypay")
                {
                    returnurl = Request.UrlReferrer.ToString().TrimEnd('?') + keyframe + Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + "TRUST_PAYDirect" + "|" + ObjReq.MSISDN + "|" + "ChooseAccountTrustlypay" + "|" + ObjReq.PostpaidPlanName + "|" + ObjReq.BillingCycle + "|" + ObjReq.Amount + "|" + ObjReq.PromoCode;
                    ObjReq.TypeOfPayment = returnurl;
                }
                else if(ObjReq.Mode == "Trustlypay")
                {
                    returnurl = Request.UrlReferrer.ToString().TrimEnd('?') + keyframe + Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + "TRUST_PAY" + "|" + ObjReq.MSISDN + "|" + "SelectAccountTrustlypay" + "|" + ObjReq.PostpaidPlanName + "|" + ObjReq.BillingCycle + "|" + ObjReq.Amount + "|" + ObjReq.PromoCode;
                    ObjReq.TypeOfPayment = returnurl;
                }
                else { }

                // 4834
                if(clientSetting.preSettings.EnableTrustlyPay.ToUpper() == "TRUE")
                {
                    if (ObjReq.Mode == "PAYMENT" &&  ObjReq.status != "1")
                    {
                        if (ObjReq.Address != null && !string.IsNullOrEmpty(ObjReq.Address.Country))
                            ObjReq.Address.Country = clientSetting.mvnoSettings.countryName.Trim();

                        if (ObjReq.PaymentMode == "CC")
                        {
                            returnurl = Request.UrlReferrer.ToString().TrimEnd('?') + keyframe + Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + "NEW_AUTORENEWAL" + "|" + ObjReq.MSISDN + "|" + "PostpaidPlanPurchaseDirectDebit" + "|" + ObjReq.PostpaidPlanName + "|" + ObjReq.BillingCycle + "|" + ObjReq.Amount + "|" + ObjReq.PromoCode + "|" + ObjReq.PaymentMode + "|" + ObjReq.CountrySEPA + "|" +  ObjReq.Cardetails.CardNumber  + "|" + ObjReq.Cardetails.NameOnCard + "|" + ObjReq.Cardetails.NameOnCard + "|" + ObjReq.Cardetails.CardType + "|" + ObjReq.Cardetails.ExpiryDate + "|" + ObjReq.Cardetails.IssueDate + "|" + ObjReq.Cardetails.CVV + "|" + ObjReq.Cardetails.CardID + "|" + ObjReq.Cardetails.ConsentDate + "|" 
                             + ObjReq.Address.PostCode + "|" + ObjReq.Address.Street + "|" + ObjReq.Address.City + "|" + ObjReq.Address.Country+ "|" + ObjReq.Address.HouseNo + "|" + ObjReq.Address.ApartmentNo + "|" + ObjReq.Address.Floor + "|" 
                             + ObjReq.Mode + "|" + ObjReq.EmailID + "|" + ObjReq.Firstname + "|" + ObjReq.Lastname + "|" + ObjReq.Mode + "|" + ObjReq.Pincode + "|" 
                             + ObjReq.MSISDN + "|" + ObjReq.FeatureName + "|" + ObjReq.Address01 + "|" + ObjReq.BankCode;
                            ObjReq.DirectDebitReturnUrl = returnurl;
                        }
                        else
                        {
                            returnurl = Request.UrlReferrer.ToString().TrimEnd('?') + keyframe + Convert.ToString(Session["UserName"]) + "|" + Convert.ToString(Session["Password"]) + "|" + "NEW_AUTORENEWAL" + "|" + ObjReq.MSISDN + "|" + "PostpaidPlanPurchaseDirectDebit" + "|" + ObjReq.PostpaidPlanName + "|" + ObjReq.BillingCycle + "|" + ObjReq.Amount + "|" + ObjReq.PromoCode + "|" + ObjReq.PaymentMode + "|" + ObjReq.CountrySEPA + "|" + ObjReq.APMId + "|" + ObjReq.SelectAPM + "|" + ObjReq.DNIIdentificationNumber + "|" + ObjReq.DNIType + "|" + ObjReq.PayerState + "|" + ObjReq.PayerDOB + "|" + ObjReq.PayerPostCode + "|" + ObjReq.APMBankSwiftCode + "|" + ObjReq.AccountHolderAPM + "|"
                              + ObjReq.Pincode + "|" + ObjReq.Address02 + "|" + ObjReq.Towercity + "|" +  "" + "|" + ObjReq.Address01 + "|" + ObjReq.Address02 + "|" + "" + "|"
                              + ObjReq.Mode + "|" + ObjReq.EmailID + "|" + ObjReq.Firstname + "|" + ObjReq.Lastname + "|" + ObjReq.Mode + "|" + ObjReq.Pincode + "|"
                              + ObjReq.MSISDN + "|" + ObjReq.FeatureName + "|" + ObjReq.Address01 + "|" + ObjReq.BankCode;
                            ObjReq.DirectDebitReturnUrl = returnurl;
                        }
                    }
                  
                    
                }

                ObjResp = serviceCRM.CRMPostpaidPlanPurchase(ObjReq);              

                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("PostpaidPlanPurchase" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Registration - PostpaidPlanPurchase End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), "Registration - PostpaidPlanPurchase - " + this.ControllerContext, ex);
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

        public JsonResult GetTaxVAT(TAXVAT objTAXVAT)
        {
            TAXVATRes objTAXVATRes = new TAXVATRes();
            try
            {

                objTAXVAT.isTaxEnabled = clientSetting.mvnoSettings.eShopShowTax.Trim().ToUpper() == "TRUE" ? true : false;
                objTAXVAT.isVATEnabled = clientSetting.mvnoSettings.eShopShowVat.Trim().ToUpper() == "TRUE" ? true : false;
                objTAXVAT.CountryCode = clientSetting.countryCode;
                objTAXVAT.BrandCode = clientSetting.brandCode;
                objTAXVAT.LanguageCode = clientSetting.langCode;
                objTAXVAT.EshopSuccessCodeTAX = clientSetting.mvnoSettings.eShopVatSuccessCode.ToString();
                objTAXVATRes = crmService.LoadVATTAXPOSTPAID(objTAXVAT);


                ///FRR--3083
                if (objTAXVATRes != null && objTAXVATRes.Response != null && objTAXVATRes.Response.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("GetTaxVAT_" + objTAXVATRes.Response.ResponseCode);
                    objTAXVATRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objTAXVATRes.Response.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objTAXVATRes);
        }

        #region Netherland Registration

        public ActionResult SubscriberRegistration_NLD(string RegisterMsisdn)
        {
            if (Convert.ToString(Session["isPrePaid"]) == "1")
            {
                Registration_NLD NLDobjres = new Registration_NLD();
                try
                {

                    NLDobjres.NLDdrpdown = Utility.GetDropdownMasterFromDB("1,4,10", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, "1", "TblCountry")).ToList();
                    //  NLDobjres.NLDdrpdown = Utility.GetDropdownMasterFromDB("1,4,10", "1", "drop_master");
                    ViewBag.Title = "Subscriber Registration";
                    NLDobjres.Mode = "INSERT";
                    // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                    Session["PAType"] = null;
                    if (clientSetting.brandCode.ToUpper().Trim() == clientSetting.preSettings.ToggleBrandCode.ToUpper().Trim())
                    {
                        return RedirectToAction("SubscriberRegistration_NLD_TGL");
                    }
                }
                catch (Exception ex)
                {
                    NLDobjres.NLDdrpdown = Utility.GetDropdownMasterFromDB("1,4,10", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, "1", "TblCountry")).ToList();
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                }
                return View("SubscriberRegistration_NLD", NLDobjres);
            }
            else
            {
                Session["isPrePaid"] = "0";
                Registration_NLD NLDobjres = new Registration_NLD();
                try
                {
                    CRMBase cbase = new CRMBase();
                    cbase.CountryCode = clientSetting.countryCode;
                    cbase.BrandCode = clientSetting.brandCode;
                    cbase.LanguageCode = clientSetting.langCode;
                    GetPlansfromPBS objres = crmNewService.CRMGetPlansfromPBS(cbase);

                    NLDobjres.NLDdrpdown = Utility.GetDropdownMasterFromDB("1,4,10,8,20", "0", "drop_master").Concat(Utility.GetDropdownMasterFromDB("", "0", "TblCountry")).ToList();

                    ViewBag.Title = "Subscriber Registration";
                    NLDobjres.Mode = "INSERT";
                    NLDobjres.NLDdrpdownBillCycle = objres.Billcycledatelist;
                    NLDobjres.NLDdrpdownWholesaler = objres.Wholesaleplanlist; ;
                    NLDobjres.Corporate = objres.Corporatelist;
                    TempData["Corporate"] = objres.Corporatelist;

                    Session["PAType"] = null;
                }
                catch (Exception ex)
                {
                    NLDobjres.NLDdrpdown = Utility.GetDropdownMasterFromDB("1,4,10,8,20", "0", "drop_master");
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                }
                return View("SubscriberRegistrationPP_NLD", NLDobjres);
            }
        }
        public ActionResult SubscriberRegistrationEdit_NLD(string RegisterMsisdn)
        {

            Registration_NLD NLDobjres = new Registration_NLD();
            if (Convert.ToString(Session["isPrePaid"]) == "1")
            {

                try
                {//Prepaid Edit

                    ViewBag.Title = "Subscriber Edit";
                    //NLDobjres.NLDdrpdown = Utility.GetDropdownMasterFromDB("1,4,10", "1", "drop_master");
                    NLDobjres.NLDdrpdown = Utility.GetDropdownMasterFromDB("1,4,10", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, "1", "TblCountry")).ToList();
                    NLDobjres.Mode = "UPDATE";

                    // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                    Session["PAType"] = null;
                    if (clientSetting.brandCode.ToUpper().Trim() == clientSetting.preSettings.ToggleBrandCode.ToUpper().Trim())
                    {

                        return View("SubscriberRegistration_NLD_TGL", NLDobjres);
                    }
                }
                catch (Exception ex)
                {
                    //NLDobjres.NLDdrpdown = Utility.GetDropdownMasterFromDB("1,4,10", "1", "drop_master");
                    NLDobjres.NLDdrpdown = Utility.GetDropdownMasterFromDB("1,4,10", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, "1", "TblCountry")).ToList();
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                }
                return View("SubscriberRegistration_NLD", NLDobjres);


            }
            else
            {
                //Postpaid Edit

                try
                {
                    Session["isPrePaid"] = "0";
                    CRMBase cbase = new CRMBase();
                    cbase.CountryCode = clientSetting.countryCode;
                    cbase.BrandCode = clientSetting.brandCode;
                    cbase.LanguageCode = clientSetting.langCode;
                    GetPlansfromPBS objres = crmNewService.CRMGetPlansfromPBS(cbase);

                    NLDobjres.NLDdrpdown = Utility.GetDropdownMasterFromDB("1,4,10", "1", "drop_master");


                    ViewBag.Title = "Subscriber Registration";
                    NLDobjres.Mode = "UPDATE";
                    NLDobjres.NLDdrpdownBillCycle = objres.Billcycledatelist;
                    NLDobjres.NLDdrpdownWholesaler = objres.Wholesaleplanlist; ;
                    TempData["Corporate"] = objres.Corporatelist;
                    // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                    Session["PAType"] = null;
                }
                catch (Exception ex)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                }
                return View("SubscriberRegistrationPP_NLD", NLDobjres);
            }
        }


        public ActionResult SubscriberRegistration_NLD_TGL(string RegisterMsisdn)
        {

            Registration_NLD NLDobjres = new Registration_NLD();
            try
            {

                NLDobjres.NLDdrpdown = Utility.GetDropdownMasterFromDB("1,4,10", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, "1", "TblCountry")).ToList();

                ViewBag.Title = "Subscriber Registration";
                NLDobjres.Mode = "INSERT";
                // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                Session["PAType"] = null;
            }
            catch (Exception ex)
            {
                NLDobjres.NLDdrpdown = Utility.GetDropdownMasterFromDB("1,4,10", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, "1", "TblCountry")).ToList();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View("SubscriberRegistration_NLD_TGL", NLDobjres);


        }

        #endregion

        #region Netherland Postpaid


        public ActionResult SubscriberRegistrationPP_NLD(string RegisterMsisdn)
        {
            Registration_NLD NLDobjres = new Registration_NLD();
            try
            {
                CRMBase cbase = new CRMBase();
                cbase.CountryCode = clientSetting.countryCode;
                cbase.BrandCode = clientSetting.brandCode;
                cbase.LanguageCode = clientSetting.langCode;
                GetPlansfromPBS objres = crmNewService.CRMGetPlansfromPBS(cbase);
                NLDobjres.NLDdrpdown = Utility.GetDropdownMasterFromDB("1,4,8,20", "2", "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, "2", "TblCountry")).ToList();


                ViewBag.Title = "Subscriber Registration";
                NLDobjres.Mode = "INSERT";
                NLDobjres.NLDdrpdownBillCycle = objres.Billcycledatelist;
                NLDobjres.NLDdrpdownWholesaler = objres.Wholesaleplanlist; ;
                NLDobjres.Corporate = objres.Corporatelist;
                TempData["Corporate"] = objres.Corporatelist;
                //TempData.Keep("Corporate");
                // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                Session["PAType"] = null;
            }
            catch (Exception ex)
            {
                NLDobjres.NLDdrpdown = Utility.GetDropdownMasterFromDB("1,4,8,20", "2", "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, "2", "TblCountry")).ToList();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View("SubscriberRegistrationPP_NLD", NLDobjres);
        }




        public JsonResult InsertPostpaidRegistration_NLD(string Registration)
        {
            RegisterInsertPostpaidResponse objRes = new RegisterInsertPostpaidResponse();
            try
            {
                RegisterInsertPostpaidRequest objReg = JsonConvert.DeserializeObject<RegisterInsertPostpaidRequest>(Registration);
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;


                objReg.User = Convert.ToString(Session["UserName"]);
                //objReg.PrefLanguage = Convert.ToString(clientSetting.mvnoSettings.sendPrefLanguagetoRRBS);
                objReg.SMSUPDATE = "1";
                objRes = crmNewService.CRMRegisterPostpaidNLD(objReg);
                if (objReg.mode != "UPDATE")
                {

                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Reg_NLDPostpaid_" + objRes.ResponseDetails.ResponseCode);
                    objRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                    if (objRes.ResponseDetails.ResponseCode == "51" || objRes.ResponseDetails.ResponseCode == "52" || objRes.ResponseDetails.ResponseCode == "53" || objRes.ResponseDetails.ResponseCode == "54" || objRes.ResponseDetails.ResponseCode == "55" || objRes.ResponseDetails.ResponseCode == "56" || objRes.ResponseDetails.ResponseCode == "57" || objRes.ResponseDetails.ResponseCode == "58")
                    {
                        objRes.ResponseDetails.ResponseCode = "0";
                    }
                }
                else
                {

                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ERegupd_NLD_" + objRes.ResponseDetails.ResponseCode);
                    objRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objRes);
        }

        #endregion

        #region Poland Registration

        public ActionResult SubscriberRegistration_POL(string RegisterMsisdn)
        {
            Registration_POL POLobjres = new Registration_POL();
            try
            {
                POLobjres.strDropdown = Utility.GetDropdownMasterFromDB("1,4", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB("", "1", "TblCountry")).ToList(); ;
                POLobjres.countrycode = clientSetting.mvnoSettings.countryCode;
                POLobjres.Mode = "Insert";
                POLobjres.MSISDN = RegisterMsisdn;

                POLobjres.DocFormat = clientSetting.mvnoSettings.documentFormat.ToUpper();
                // objReg.Filesize = clientSetting.preSettings.itaAttachFileSize;
                POLobjres.Filesize = clientSetting.mvnoSettings.attachFileSize;

                POLobjres.DocDetails = "1" + "|" + Resources.RegistrationResources.UploadSingleFile;


                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;


                string registrationDocpath = clientSetting.preSettings.registrationDocpath;
                if (!Directory.Exists(registrationDocpath))
                {
                    Directory.CreateDirectory(registrationDocpath);
                }
                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string UploadFile = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    //if (Server.MapPath("~/App_Data/UploadFile") != null)
                    //{
                    //    string UploadFile = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                    if (!Directory.Exists(UploadFile))
                    {
                        Directory.CreateDirectory(UploadFile);
                    }
                }

                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    string PdfDownload = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (!Directory.Exists(PdfDownload))
                    {
                        Directory.CreateDirectory(PdfDownload);
                    }
                }



                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    var filepath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string filepath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    if (Directory.Exists(filepath))
                    {
                        System.IO.DirectoryInfo di = new DirectoryInfo(filepath);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                    }
                }
                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    var filepath2 = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (Directory.Exists(filepath2))
                    {
                        System.IO.DirectoryInfo di = new DirectoryInfo(filepath2);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                    }
                }
                // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                Session["PAType"] = null;
                ViewBag.Title = "Subscriber Registration";
            }
            catch (Exception ex)
            {
                POLobjres.strDropdown = Utility.GetDropdownMasterFromDB("1,4", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB("", "1", "TblCountry")).ToList();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View("SubscriberRegistration_POL", POLobjres);
        }


        public ActionResult SubscriberRegistrationEdit_POL(string RegisterMsisdn)
        {
            Registration_POL POLobjres = new Registration_POL();
            try
            {

                POLobjres.strDropdown = Utility.GetDropdownMasterFromDB("1,4", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB("", "1", "TblCountry")).ToList();
                POLobjres.countrycode = clientSetting.mvnoSettings.countryCode.ToString();
                POLobjres.Mode = "Edit";
                Session["PAType"] = null;
                ViewBag.Title = "Subscriber Edit";
            }
            catch (Exception ex)
            {
                POLobjres.strDropdown = Utility.GetDropdownMasterFromDB("1,4", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB("", "1", "TblCountry")).ToList(); ;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View("SubscriberRegistration_POL", POLobjres);
        }



        public ActionResult SubscriberRegistrationView_POL()
        {
            Registration_POL POLobjres = new Registration_POL();
            try
            {
                POLobjres.countrycode = clientSetting.mvnoSettings.countryCode;
                Session["PAType"] = null;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View("subscriberview_POL", POLobjres);
        }
        #endregion

        public string GetAboutUs()
        {
            string strques = string.Empty;
            Dictionary<string, string> lstTopupamount = new Dictionary<string, string>();
            string SelectTitles = SIMResources.ResourceManager.GetString("Select");
            try
            {
                List<DropdownMaster> objAction = Utility.GetDropdownMasterFromDB("1,4,13", "1", "drop_master");
                strques = strques + "<option title='" + SelectTitles + "' value=''>" + SelectTitles + "</option>";
                for (int i = 0; i < objAction.Count; i++)
                {
                    strques = strques + "<option title='" + objAction[i].Value + "' value='" + objAction[i].ID + "'>" + objAction[i].Value + "</option>";
                }

            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            return strques;
        }


        #region GOPIKUMAR REGISTRATION REGION
        //----------------------C.GOPIKUMAR (EmpID-2296) and Vignesh (EmpID-3538)-----------------------------------

        #region GERMANY REGISTRATION

        public ActionResult SubscriberRegistration_GER(string RegisterMsisdn)
        {
            CRM.Models.Registration objReg = new CRM.Models.Registration();
            try
            {
                Session["MobileNumber"] = string.Empty;
                objReg.consecutiveName = clientSetting.mvnoSettings.consecutiveName;
                objReg.MSISDN = RegisterMsisdn;

                objReg.DocFormat = clientSetting.mvnoSettings.documentFormat.ToUpper();
                objReg.Filesize = clientSetting.mvnoSettings.attachFileSize;
                objReg.DocDetails = "1" + "|" + Resources.RegistrationResources.UploadSingleFile;

                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,5,6,7,8,10,31", objReg.IsPostpaid, "drop_master");
                #region File Upload

                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;

                #region Create path

                string registrationDocpath = clientSetting.preSettings.registrationDocpath;
                if (!Directory.Exists(registrationDocpath))
                {
                    Directory.CreateDirectory(registrationDocpath);
                }
                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string UploadFile = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    if (!Directory.Exists(UploadFile))
                    {
                        Directory.CreateDirectory(UploadFile);
                    }
                }
                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    string PdfDownload = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (!Directory.Exists(PdfDownload))
                    {
                        Directory.CreateDirectory(PdfDownload);
                    }
                }

                #endregion

                #region Delete file from path

                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string filepath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    if (Directory.Exists(filepath))
                    {
                        System.IO.DirectoryInfo di = new DirectoryInfo(filepath);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                    }
                }

                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    var filepath2 = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (Directory.Exists(filepath2))
                    {
                        System.IO.DirectoryInfo di = new DirectoryInfo(filepath2);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                    }
                }

                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objReg);
        }

        [HttpPost]
        public JsonResult SaveRegistration_GER(string Registration)
        {
            GERRegistration objReg = JsonConvert.DeserializeObject<GERRegistration>(Registration);
            CRMResponse objRes = new CRMResponse();
            string dob = objReg.BirthDD;
            string strAccountId = string.Empty;
            string strRefNo = string.Empty;
            try
            {
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;

                objReg.IsGafVerified = clientSetting.mvnoSettings.useGAF.ToLower();

                if (objReg.BirthDD != string.Empty)
                {
                    string strDOB = Utility.GetDateconvertion(objReg.BirthDD, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    string[] strSplit = strDOB.Split('/');
                    objReg.BirthDD = strSplit[0].ToString();
                    objReg.BirthMM = strSplit[1].ToString();
                    objReg.BirthYYYY = strSplit[2].ToString();
                }
                else
                {
                    objReg.BirthDD = string.Empty;
                    objReg.BirthMM = string.Empty;
                    objReg.BirthYYYY = string.Empty;
                }
                objReg.CSAgent = Session["UserName"].ToString();



                if (clientSetting.preSettings.enableSFTP.ToUpper() == "FALSE" && clientSetting.mvnoSettings.enableFTPS.ToUpper() == "FALSE" && clientSetting.preSettings.GER_EnableFileUpload.ToLower() == "true")
                {
                    if (Directory.Exists(clientSetting.mvnoSettings.sftpFilePath))
                    {

                    }
                    else
                    {
                        objRes.ResponseCode = "500";
                        objRes.ResponseDesc = "Upload Path not exists";
                        return Json(objRes);
                    }
                }

                objRes = crmService.CRMRegisterSubscriberGER(objReg);

                if (objRes != null)
                {
                    if (objRes.ResponseDesc.Contains("|"))
                    {
                        List<string> lstResp = new List<string>(objRes.ResponseDesc.Split('|'));
                        strAccountId = lstResp[1];
                        objRes.ResponseDesc = lstResp[0];
                        strRefNo = lstResp[2];
                    }

                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_GER_" + objRes.ResponseCode);
                        objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;
                    }

                    if (!string.IsNullOrEmpty(objRes.ResponseCode) && (objRes.ResponseCode == "0" || objRes.ResponseCode == "100" || objRes.ResponseCode == "110" || objRes.ResponseCode == "111"
                                 || objRes.ResponseCode == "112" || objRes.ResponseCode == "113" || objRes.ResponseCode == "114" || objRes.ResponseCode == "120" || objRes.ResponseCode == "121" || objRes.ResponseCode == "122"))
                    {
                        if (objReg.Mode == "UPDATE")
                        {
                            if (!_runningFromNUnit)
                            {
                                objRes.ResponseDesc = @Resources.RegistrationResources.UpdatedSuccessfully;
                            }
                            Session["SubscriberTitle"] = objReg.Title;
                            Session["SubscriberName"] = objReg.FirstName + "|" + objReg.LastName;
                            Session["DOB"] = dob;
                        }

                        if (objReg.Mode == "INSERT" || objReg.Mode == "UPDATE")
                        {
                            if (objReg.Mode == "INSERT")
                            {
                                objRes.ResponseDesc = objRes.ResponseDesc + "," + @Resources.RegistrationResources.ReferenceNumber + ":" + strRefNo;
                            }

                            if (clientSetting.preSettings.GER_EnableFileUpload.ToLower() == "true")
                            {
                                string FileName = "";
                                if ((!string.IsNullOrEmpty(objReg.document1)) || (!string.IsNullOrEmpty(objReg.document2)) || (!string.IsNullOrEmpty(objReg.document3)) || (!string.IsNullOrEmpty(objReg.document4)) || (!string.IsNullOrEmpty(objReg.document5)) || (!string.IsNullOrEmpty(objReg.document6)))
                                {
                                    string Externsion = string.Empty;
                                    string SourceDocpath = clientSetting.preSettings.registrationDocpath;
                                    if (!Directory.Exists(SourceDocpath))
                                    {
                                        Directory.CreateDirectory(SourceDocpath);
                                    }

                                    //FileName = ObjRes.AccountID + "_Doc1." + objReg.Doc1.Split('.')[1];
                                    fileuploadGER(FileName, objReg, strAccountId);
                                }
                            }
                        }
                    }
                }
                else
                {
                    objRes = new CRMResponse();
                    objRes.ResponseDesc = Resources.ErrorResources.Common_2;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.ResponseDesc);
            }
            catch (Exception ex)
            {
                objRes = new CRMResponse();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.ResponseCode = "2";
                objRes.ResponseDesc = ex.ToString();
            }

            return Json(objRes);
        }

        public void fileuploadGER(string FileName, GERRegistration objReg, string AccountNo)
        {

            string sourcePath = string.Empty;
            string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
            //if (Server.MapPath("~/App_Data/UploadFile") != null)
            //{
            //    sourcePath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
            if (clientSetting.mvnoSettings.internalUploadFile != null)
            {
                sourcePath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);

            }

            string targetPath = clientSetting.preSettings.registrationDocpath;
            string Externsion = string.Empty;

            string filename = string.Empty;

            #region Move
            //if (Server.MapPath("~/App_Data/UploadFile") != null)
            //{
            if (clientSetting.mvnoSettings.internalUploadFile != null)
            {
                if (Directory.GetFiles(sourcePath).Count() > 0)
                {
                    string[] fileEntries2 = Directory.GetFiles(sourcePath);
                    string[] filenameList = new string[6];

                    string Deletefilename = string.Empty;
                    for (int i = 0; i < fileEntries2.Count(); i++)
                    {
                        Externsion = fileEntries2[i].Substring(fileEntries2[i].LastIndexOf("\\") + 1);
                        string[] splitExternsion = Externsion.Split('.');

                        if (Externsion == objReg.document1)
                        {
                            filename = AccountNo + "_1" + "." + splitExternsion[1];
                        }
                        else if (Externsion == objReg.document2)
                        {
                            filename = AccountNo + "_2" + "." + splitExternsion[1];
                        }
                        else if (Externsion == objReg.document3)
                        {
                            filename = AccountNo + "_3" + "." + splitExternsion[1];
                        }
                        else if (Externsion == objReg.document4)
                        {
                            filename = AccountNo + "_4" + "." + splitExternsion[1];
                        }
                        else if (Externsion == objReg.document5)
                        {
                            filename = AccountNo + "_5" + "." + splitExternsion[1];
                        }
                        else if (Externsion == objReg.document6)
                        {
                            filename = AccountNo + "_6" + "." + splitExternsion[1];
                        }

                        //filename = AccountNo + "_" + splitExternsion[0].Substring(splitExternsion[0].Length - 1) + "." + splitExternsion[1];

                        #region Delete
                        try
                        {
                            for (int k = 0; k < 6; k++)
                            {
                                string[] fileformat = clientSetting.mvnoSettings.documentFormat.Split(',');
                                for (int j = 0; j < fileformat.Length; j++)
                                {
                                    fileformat[j] = fileformat[j].Replace(".", string.Empty).Trim();
                                    System.IO.File.Delete(targetPath + "\\" + filename);
                                }
                            }
                        }
                        catch
                        {

                        }
                        #endregion

                        System.IO.File.Copy(sourcePath + "\\" + Externsion, targetPath + "\\" + filename);
                        filenameList[i] = filename.ToString();
                        Deletefilename = filename;
                        if (clientSetting.preSettings.enableSFTP.ToUpper() == "FALSE" && clientSetting.mvnoSettings.enableFTPS.ToUpper() == "FALSE")
                        {

                            #region Create path

                            string TargetDocpath = clientSetting.mvnoSettings.sftpFilePath;
                            if (!Directory.Exists(TargetDocpath))
                            {
                                Directory.CreateDirectory(TargetDocpath);
                            }

                            #endregion
                            System.IO.File.Copy(clientSetting.preSettings.registrationDocpath + "\\" + filename, clientSetting.mvnoSettings.sftpFilePath + "\\" + filename, true);
                        }
                        if (clientSetting.mvnoSettings.enableFTPS.ToUpper() == "TRUE")
                        {
                            FileUploadFTPSServer(targetPath + "\\", filename, objReg);
                        }

                    }

                    if (clientSetting.preSettings.enableSFTP.ToUpper() == "TRUE" && clientSetting.mvnoSettings.enableFTPS.ToUpper() == "FALSE")
                    {
                        MovefileFTP(targetPath + "\\", filenameList, Deletefilename, objReg, "");
                        //FileUploadFTPSServer(targetPath + "\\", filenameList, objReg);
                    }


                    for (int i = 0; i < filenameList.Count(); i++)
                    {
                        if (!string.IsNullOrEmpty(filenameList[i]))
                        {
                            System.IO.File.Delete(clientSetting.preSettings.registrationDocpath + "\\" + filenameList[i]);
                        }
                    }
                }
            }

            try
            {
                //Delete file in appdata
                string[] fileEntries3 = Directory.GetFiles(sourcePath);
                for (int i = 0; i < fileEntries3.Count(); i++)
                {

                    Externsion = fileEntries3[i].Substring(fileEntries3[i].LastIndexOf("\\") + 1);
                    System.IO.File.Delete(sourcePath + "\\" + Externsion);
                    if (clientSetting.mvnoSettings.internalPdfDownload != null)
                    {
                        string delPath = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                        if (Directory.Exists(delPath))
                        {
                            System.IO.DirectoryInfo di = new DirectoryInfo(delPath);
                            foreach (FileInfo file in di.GetFiles())
                            {
                                file.Delete();
                            }
                            //   Directory.Delete(delPath, true);
                        }
                    }
                }
            }
            catch
            {

            }


            #endregion
        }


        public ActionResult SubscriberRegistrationEdit_GER(string RegisterMsisdn)
        {
            CRM.Models.Registration_GER objReg = new CRM.Models.Registration_GER();
            CustomerDetailsGERReq CustReq = new CustomerDetailsGERReq();
            CustomerDetailGERResp CustResp = new CustomerDetailGERResp();
            objReg.Filename = new List<FileDetail>();
            try
            {
                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                CustReq.MSISDN = Session["MobileNumber"].ToString();

                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;

                #region Create path

                string registrationDocpath = clientSetting.preSettings.registrationDocpath;
                if (!Directory.Exists(registrationDocpath))
                {
                    Directory.CreateDirectory(registrationDocpath);
                }
                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    string UploadFile = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string UploadFile = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);

                    if (!Directory.Exists(UploadFile))
                    {
                        Directory.CreateDirectory(UploadFile);
                    }
                }

                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    string PdfDownload = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (!Directory.Exists(PdfDownload))
                    {
                        Directory.CreateDirectory(PdfDownload);
                    }
                }
                #endregion

                CustResp = crmService.CRMGetCustomerDetailsGER(CustReq);

                if (CustResp.ResponseDetails != null)
                {
                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Edit_GER_" + CustResp.ResponseDetails.ResponseCode);
                        CustResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CustResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }

                    if (CustResp.ResponseDetails.ResponseCode == "0")
                    {
                        objReg.ResponseCode = CustResp.ResponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetails.ResponseDesc;
                        objReg.CustDtls = CustResp.CustomerDetails;
                        objReg.Filename = CustResp.Filename;

                        #region Delete file from path

                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            var filepath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                            if (Directory.Exists(filepath))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(filepath);
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                            }
                        }

                        if (clientSetting.mvnoSettings.internalPdfDownload != null)
                        {
                            var filepath2 = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                            if (Directory.Exists(filepath2))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(filepath2);
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                            }
                        }

                        #endregion

                        #region Change Uploaded File Path

                        string sourcePath = ""; string[] GetFileName = new string[6];

                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            string targetPath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                            if (objReg.Filename.Count() > 0)
                            {
                                for (int i = 0; i < objReg.Filename.Count(); i++)
                                {
                                    sourcePath = clientSetting.preSettings.registrationDocpath;

                                    if (!Directory.Exists(targetPath))
                                    {
                                        Directory.CreateDirectory(targetPath);
                                    }
                                    if (System.IO.File.Exists(sourcePath + "\\" + objReg.Filename[i].Filename))
                                    {
                                        System.IO.File.Copy(sourcePath + "\\" + objReg.Filename[i].Filename, targetPath + "\\" + objReg.Filename[i].Filename, true);
                                        string filename = objReg.Filename[i].Filename;
                                        GetFileName[i] = filename;
                                    }
                                }
                                if (objReg.Filename != null)
                                {
                                    for (int i = 0; i < objReg.Filename.Count(); i++)
                                    {
                                        GetFileName[i] = objReg.Filename[i].Filename;
                                    }
                                    GetfileFTP(targetPath + "\\", GetFileName, CustReq, CustResp.ResponseDetails.ResponseDesc);
                                }
                            }
                        }

                        #endregion

                    }
                    else
                    {
                        objReg.ResponseCode = CustResp.ResponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetails.ResponseDesc;
                    }
                }
                else
                {
                    objReg = new Registration_GER();
                    objReg.ResponseCode = "1";
                    objReg.ResponseDescription = "No Record";
                }
                objReg.DocFormat = clientSetting.mvnoSettings.documentFormat.ToUpper();
                objReg.Filesize = clientSetting.mvnoSettings.attachFileSize;
                objReg.DocDetails = "1" + "|" + Resources.RegistrationResources.UploadSingleFile;

                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,4,31", objReg.IsPostpaid, "drop_master");
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objReg.ResponseDescription);
            }
            catch (Exception ex)
            {
                objReg = new Registration_GER();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
            }
            return View("SubscriberEdit_GER", objReg);
        }


        public ActionResult SubscriberRegistrationView_GER(string RegisterMsisdn)
        {
            CRM.Models.Registration_GER objReg = new CRM.Models.Registration_GER();
            CustomerDetailsGERReq CustReq = new CustomerDetailsGERReq();
            CustomerDetailGERResp CustResp = new CustomerDetailGERResp();
            try
            {
                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                CustReq.MSISDN = Session["MobileNumber"].ToString();
                CustResp = crmService.CRMGetCustomerDetailsGER(CustReq);
                if (CustResp.ResponseDetails != null)
                {
                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Edit_GER_" + CustResp.ResponseDetails.ResponseCode);
                        CustResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CustResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    if (CustResp.ResponseDetails.ResponseCode == "0")
                    {
                        objReg.ResponseCode = CustResp.ResponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetails.ResponseDesc;
                        objReg.CustDtls = CustResp.CustomerDetails;
                    }
                    else
                    {
                        objReg.ResponseCode = CustResp.ResponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetails.ResponseDesc;
                    }
                }
                else
                {
                    objReg = new Registration_GER();
                    objReg.ResponseCode = "1";
                    objReg.ResponseDescription = "No Record";
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objReg.ResponseDescription);
            }
            catch (Exception ex)
            {
                objReg = new Registration_GER();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
            }
            return View("SubscriberView_GER", objReg);
        }

        public JsonResult LoadCityGER(string PostalCode)
        {
            List<DropdownMaster> objLstDropdown = new List<DropdownMaster>();
            try
            {
                if (Session["objLstDropdown"] != null)
                    objLstDropdown = (List<DropdownMaster>)Session["objLstDropdown"];
                else
                    objLstDropdown = Utility.GetDropdownMasterFromDB("", Convert.ToString(Session["isPrePaid"]), "tbl_city");
                Session["objLstDropdown"] = objLstDropdown;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objLstDropdown.FindAll(a => a.Master_id == PostalCode));
        }

        #endregion

        #region SPAIN REGISTRATION

        public ActionResult SubscriberRegistration_ESP(string RegisterMsisdn)
        {
            CRM.Models.Registration objReg = new CRM.Models.Registration();
            try
            {
                Session["MobileNumber"] = string.Empty;
                objReg.MSISDN = RegisterMsisdn;
                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";

                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,4,13,27,100", objReg.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "Tbl_Nationality")).ToList();
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objReg);

        }

        [HttpPost]
        public JsonResult SaveRegistration_ESP(string Registration)
        {
            ESPRegistration objReg = JsonConvert.DeserializeObject<ESPRegistration>(Registration);
            ESPRegisterResp objRes = new ESPRegisterResp();
            string dob = objReg.BirthDD;
            try
            {
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                if (objReg.BirthDD != null && objReg.BirthDD != string.Empty)
                {
                    string strDOB = Utility.GetDateconvertion(objReg.BirthDD, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    string[] strSplit = strDOB.Split('/');
                    objReg.BirthDD = strSplit[0].ToString();
                    objReg.BirthMM = strSplit[1].ToString();
                    objReg.BirthYYYY = strSplit[2].ToString();
                }
                else
                {
                    objReg.BirthDD = string.Empty;
                    objReg.BirthMM = string.Empty;
                    objReg.BirthYYYY = string.Empty;
                }
                objReg.CSAgent = Session["UserName"].ToString();
                objRes = crmService.CRMRegisterSubscriberESP(objReg);
                if (objRes.ResponseDetails != null)
                {
                    if (!_runningFromNUnit)
                    {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_ESP_" + objRes.ResponseDetails.ResponseCode);
                    objRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    if (objRes.ResponseDetails.ResponseCode == "0")
                    {
                        if (objReg.Mode == "UPDATE")
                        {
                            if (!_runningFromNUnit)
                            {
                            objRes.ResponseDetails.ResponseDesc = @Resources.RegistrationResources.UpdatedSuccessfully;
                            }
                            Session["SubscriberTitle"] = objReg.Title;
                            Session["SubscriberName"] = objReg.FirstName + "|" + objReg.SurName;
                            Session["DOB"] = dob;
                        }
                    }
                }
                else
                {
                    objRes.ResponseDetails = new CRMResponse();
                    objRes.ResponseDetails.ResponseDesc = Resources.ErrorResources.Common_2;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.ResponseDetails.ResponseDesc);
            }
            catch (Exception ex)
            {
                objRes.ResponseDetails = new CRMResponse();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.ResponseDetails.ResponseCode = "2";
                objRes.ResponseDetails.ResponseDesc = ex.ToString();
            }
            return Json(objRes);
        }


        public ActionResult SubscriberRegistrationEdit_ESP(string RegisterMsisdn)
        {
            CRM.Models.Registration_ESP objReg = new CRM.Models.Registration_ESP();
            ESPGetReq CustReq = new ESPGetReq();
            ESPGetResp CustResp = new ESPGetResp();
            try
            {
                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                CustReq.MSISDN = Session["MobileNumber"].ToString();
                CustReq.SimNumber = Session["ICCID"].ToString();
                CustResp = crmService.CRMGetSubscriberESP(CustReq);
                if (CustResp.reponseDetails != null)
                {
                    if (!_runningFromNUnit)
                    {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Edit_ESP_" + CustResp.reponseDetails.ResponseCode);
                    CustResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CustResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    if (CustResp.reponseDetails.ResponseCode == "0")
                    {
                        objReg.ResponseCode = CustResp.reponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.reponseDetails.ResponseDesc;
                        objReg.CustDtls = CustResp;
                    }
                    else
                    {
                        objReg.ResponseCode = CustResp.reponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.reponseDetails.ResponseDesc;
                    }
                }
                else
                {
                    objReg = new Registration_ESP();
                    objReg.ResponseCode = "1";
                    objReg.ResponseDescription = "No Record";
                }
                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";



                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,4,13,100", objReg.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "Tbl_Nationality")).ToList();
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objReg.ResponseDescription);
            }
            catch (Exception ex)
            {
                objReg = new Registration_ESP();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
            }
            return View("SubscriberEdit_ESP", objReg);

        }

        public ActionResult SubscriberRegistrationView_ESP(string RegisterMsisdn)
        {
            CRM.Models.Registration_ESP objReg = new CRM.Models.Registration_ESP();
            ESPGetReq CustReq = new ESPGetReq();
            ESPGetResp CustResp = new ESPGetResp();
            try
            {
                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                CustReq.MSISDN = Session["MobileNumber"].ToString();
                CustReq.SimNumber = Session["ICCID"].ToString();
                CustResp = crmService.CRMGetSubscriberESP(CustReq);
                if (CustResp.reponseDetails != null)
                {
                    if (!_runningFromNUnit)
                    {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Edit_ESP_" + CustResp.reponseDetails.ResponseCode);
                    CustResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CustResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    if (CustResp.reponseDetails.ResponseCode == "0")
                    {
                        objReg.ResponseCode = CustResp.reponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.reponseDetails.ResponseDesc;
                        objReg.CustDtls = CustResp;
                    }
                    else
                    {
                        objReg.ResponseCode = CustResp.reponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.reponseDetails.ResponseDesc;
                    }
                }
                else
                {
                    objReg = new Registration_ESP();
                    objReg.ResponseCode = "1";
                    objReg.ResponseDescription = "No Record";
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objReg.ResponseDescription);
            }
            catch (Exception ex)
            {
                objReg = new Registration_ESP();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
            }
            return View("SubscriberView_ESP", objReg);

        }

        #endregion

        #region FRANCE REGISTRATION

        public ActionResult SubscriberRegistration_FRA(string RegisterMsisdn)
        {
            CRM.Models.Registration objReg = new CRM.Models.Registration();
            try
            {
                Session["MobileNumber"] = string.Empty;
                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,5,6,7,8,10,11,12,13,14", objReg.IsPostpaid, "drop_master");

                #region File Upload

                #region File Upload Design

                //objReg.DocFormat = clientSetting.preSettings.itaDocumentFormat.ToUpper();
                objReg.DocFormat = clientSetting.mvnoSettings.documentFormat.ToUpper();
                // objReg.Filesize = clientSetting.preSettings.itaAttachFileSize;
                objReg.Filesize = clientSetting.mvnoSettings.attachFileSize;
                if (!_runningFromNUnit)
                {
                objReg.DocDetails = "1" + "|" + Resources.RegistrationResources.UploadSingleFile;
                }
                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,5,6,7,8,10,11,12,13,14", objReg.IsPostpaid, "drop_master");
                //return View(objReg);

                #endregion

                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                #region Create path

                string registrationDocpath = clientSetting.preSettings.registrationDocpath;
                if (!Directory.Exists(registrationDocpath))
                {
                    Directory.CreateDirectory(registrationDocpath);
                }
                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    string UploadFile = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string UploadFile = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    if (!Directory.Exists(UploadFile))
                    {
                        Directory.CreateDirectory(UploadFile);
                    }
                }
                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    string PdfDownload = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (!Directory.Exists(PdfDownload))
                    {
                        Directory.CreateDirectory(PdfDownload);
                    }
                }

                #endregion

                #region Delete file from path

                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    var filepath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string filepath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    if (Directory.Exists(filepath))
                    {
                        System.IO.DirectoryInfo di = new DirectoryInfo(filepath);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                    }
                }

                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    var filepath2 = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (Directory.Exists(filepath2))
                    {
                        System.IO.DirectoryInfo di = new DirectoryInfo(filepath2);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                    }
                }

                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objReg);
        }

        [HttpPost]
        public JsonResult SaveRegistration_FRA(string Registration)
        {
            FRARegistration objReg = JsonConvert.DeserializeObject<FRARegistration>(Registration);
            FranceRegisterRes objRes = new FranceRegisterRes();
            string dob = objReg.BirthDD;
            try
            {
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                #region Date Convertion
                if (objReg.BirthDD != string.Empty)
                {
                    string strDOB = Utility.GetDateconvertion(objReg.BirthDD, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    if (strDOB != string.Empty)
                    {
                        string[] strSplit = strDOB.Split('/');
                        objReg.BirthDD = strSplit[0].ToString();
                        objReg.BirthMM = strSplit[1].ToString();
                        objReg.BirthYYYY = strSplit[2].ToString();
                    }
                    else
                    {
                        objReg.BirthDD = string.Empty;
                        objReg.BirthMM = string.Empty;
                        objReg.BirthYYYY = string.Empty;
                    }
                }
                else
                {
                    objReg.BirthDD = string.Empty;
                    objReg.BirthMM = string.Empty;
                    objReg.BirthYYYY = string.Empty;
                }
                #endregion
                objReg.RequestedBy = Session["UserName"].ToString();
                objReg.UserName = Session["UserName"].ToString();

                #region UPDATE

                if (objReg.Mode == "UPDATE")
                {
                    string[] oldSplit = objReg.filedelId.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    objReg.filedelId = "";
                    for (int i = 0; i < oldSplit.Length; i++)
                    {
                        string[] oldNameID = oldSplit[i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        if (objReg.filedelId == "")
                        {
                            objReg.filedelId = oldNameID[1];
                        }
                        else
                        {
                            objReg.filedelId = objReg.filedelId + "," + oldNameID[1];
                        }

                    }
                }

                #endregion

                objRes = crmService.CRMRegisterSubscriberFRA(objReg);

                if (objRes.reponseDetails != null)
                {
                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_FRA_" + objRes.reponseDetails.ResponseCode);
                        objRes.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    if (objRes.reponseDetails.ResponseCode == "0")
                    {
                        if (objReg.Mode == "UPDATE")
                        {
                            if (!_runningFromNUnit)
                            {
                                objRes.reponseDetails.ResponseDesc = @Resources.RegistrationResources.UpdatedSuccessfully;
                            }
                            Session["SubscriberTitle"] = objReg.Title;
                            Session["SubscriberName"] = objReg.FirstName + "|" + objReg.LastName;
                            Session["DOB"] = dob;
                        }

                    }

                    if ((objRes.reponseDetails.ResponseCode == "0" || objRes.reponseDetails.ResponseCode == "100" || objRes.reponseDetails.ResponseCode == "110" || objRes.reponseDetails.ResponseCode == "111" || objRes.reponseDetails.ResponseCode == "112" || objRes.reponseDetails.ResponseCode == "115" || objRes.reponseDetails.ResponseCode == "113" || objRes.reponseDetails.ResponseCode == "120" || objRes.reponseDetails.ResponseCode == "121" || objRes.reponseDetails.ResponseCode == "122") && ((clientSetting.mvnoSettings.isAttachmentMand.Trim().ToUpper() == "TRUE" && objReg.Mode == "INSERT") || (clientSetting.mvnoSettings.isAttachmentMandEdit.Trim().ToUpper() == "TRUE" && objReg.Mode == "UPDATE")))
                    {

                        #region Change Uploaded File Path

                        string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                        string sourcePath = string.Empty;
                        //if (Server.MapPath("~/App_Data/UploadFile") != null)
                        //{
                        //    sourcePath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            sourcePath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                        }
                        string targetPath = clientSetting.preSettings.registrationDocpath;

                        string AccountNo = string.Empty;
                        if (objReg.Mode == "INSERT")
                        {
                            AccountNo = objRes.RegisterFRA[0].AccountNum;
                        }
                        else
                        {
                            AccountNo = objReg.AccountNumber;
                        }

                        #region Delete
                        for (int i = 0; i < 3; i++)
                        {
                            string[] fileformat = clientSetting.mvnoSettings.documentFormat.Split(',');
                            for (int j = 0; j < fileformat.Length; j++)
                            {
                                fileformat[j] = fileformat[j].Replace(".", string.Empty).Trim();
                                System.IO.File.Delete(targetPath + "\\" + AccountNo + "_Doc" + (i + 1) + "." + fileformat[j]);
                            }
                        }
                        #endregion

                        #region Move
                        //if (Server.MapPath("~/App_Data/UploadFile") != null)
                        //{

                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "before count");
                            if (Directory.GetFiles(sourcePath).Count() > 0)
                            {
                                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "greater than count");
                                string[] fileEntries2 = Directory.GetFiles(sourcePath);
                                string[] filenameList = new string[3];
                                string filename = string.Empty;
                                string Deletefilename = string.Empty;
                                for (int i = 0; i < fileEntries2.Count(); i++)
                                {
                                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "for loop");
                                    string Externsion = fileEntries2[i].Substring(fileEntries2[i].LastIndexOf("\\") + 1);
                                    string[] splitExternsion = Externsion.Split('.');
                                    filename = AccountNo + "_Doc" + (i + 1) + "." + splitExternsion[1];
                                    System.IO.File.Move(sourcePath + "\\" + Externsion, targetPath + "\\" + filename);
                                    filenameList[i] = filename.ToString();
                                    Deletefilename = AccountNo + "_Doc";


                                    if (clientSetting.preSettings.enableSFTP.ToUpper() == "FALSE" && clientSetting.mvnoSettings.enableFTPS.ToUpper() == "FALSE")
                                    {
                                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "enableSFTP-FALSE and enableFTPS-False");

                                        #region Create path

                                        string TargetDocpath = clientSetting.mvnoSettings.sftpFilePath;
                                        if (!Directory.Exists(TargetDocpath))
                                        {
                                            Directory.CreateDirectory(TargetDocpath);
                                        }

                                        #endregion
                                        System.IO.File.Copy(clientSetting.preSettings.registrationDocpath + "\\" + filename, clientSetting.mvnoSettings.sftpFilePath + "\\" + filename, true);
                                    }
                                    if (clientSetting.mvnoSettings.enableFTPS.ToUpper() == "TRUE" && clientSetting.preSettings.enableSFTP.ToUpper() == "FALSE")
                                    {
                                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "enableSFTP-FALSE and enableFTPS-TRUE");
                                        FileUploadFTPSServer(targetPath + "\\", filename, objReg);
                                    }

                                }
                                if (clientSetting.preSettings.enableSFTP.ToUpper() == "TRUE" && clientSetting.mvnoSettings.enableFTPS.ToUpper() == "FALSE")
                                {
                                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "enableSFTP-TRUE and enableFTPS-FALSE");
                                    MovefileFTP(targetPath + "\\", filenameList, Deletefilename, objReg, objRes.reponseDetails.ResponseDesc);
                                }
                            }
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "count completed");
                        }
                        #endregion


                        if (clientSetting.mvnoSettings.internalPdfDownload != null)
                        {
                            string delPath = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                            if (Directory.Exists(delPath))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(delPath);
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                                //   Directory.Delete(delPath, true);
                            }
                        }
                        Session["FilePath"] = string.Empty;

                        #endregion
                    }



                }
                else
                {
                    objRes.reponseDetails = new FranCRMResponse();
                    objRes.reponseDetails.ResponseDesc = Resources.ErrorResources.Common_2;
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.reponseDetails.ResponseDesc);
            }
            catch (Exception ex)
            {
                objRes.reponseDetails = new FranCRMResponse();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.reponseDetails.ResponseCode = "2";
                objRes.reponseDetails.ResponseDesc = ex.ToString();
            }
            return Json(objRes);
        }

        [HttpPost]
        public JsonResult CRMValidateSubscriber_FRA(string Registration)
        {
            FranceValidateReq objReg = JsonConvert.DeserializeObject<FranceValidateReq>(Registration);
            FranceValidateRes objRes = new FranceValidateRes();
            try
            {
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                objRes = crmService.CRMFranceValidate(objReg);
                if (objRes.reponseDetails.ResponseCode != null && objRes.reponseDetails.ResponseCode == "0")
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_FRA_" + objRes.reponseDetails.ResponseCode);
                    objRes.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.reponseDetails.ResponseDesc : errorInsertMsg;

                    List<ValidateMSISDNFRA> objListValidateMSISDNFRA = new List<ValidateMSISDNFRA>();
                    for (int i = 0; i < objRes.ValidateMSISDN.Count(); i++)
                    {
                        ValidateMSISDNFRA objValidateMSISDNFRA = new ValidateMSISDNFRA();
                        objValidateMSISDNFRA.ErrCode = objRes.ValidateMSISDN[i].ErrCode;

                        string errorMsg = Resources.ErrorResources.ResourceManager.GetString("ERegFraVal_" + objValidateMSISDNFRA.ErrCode);
                        objValidateMSISDNFRA.ErrMsg = string.IsNullOrEmpty(errorMsg) ? objRes.ValidateMSISDN[i].ErrMsg : errorMsg;

                        // objValidateMSISDNFRA.ErrMsg = objRes.ValidateMSISDN[i].ErrMsg;
                        objValidateMSISDNFRA.MSISDN = objRes.ValidateMSISDN[i].MSISDN;

                        objValidateMSISDNFRA.Accountid = objRes.ValidateMSISDN[i].Accountid;
                        objValidateMSISDNFRA.Fulliccid = objRes.ValidateMSISDN[i].Fulliccid;
                        objValidateMSISDNFRA.RIO = objRes.ValidateMSISDN[i].RIO;
                        objValidateMSISDNFRA.TrfClassName = objRes.ValidateMSISDN[i].TrfClassName;
                        objValidateMSISDNFRA.pukcode = objRes.ValidateMSISDN[i].pukcode;
                        objValidateMSISDNFRA.IMSI = objRes.ValidateMSISDN[i].IMSI;

                        objListValidateMSISDNFRA.Add(objValidateMSISDNFRA);
                    }
                    objRes.ValidateMSISDN = objListValidateMSISDNFRA;
                    objRes.reponseDetails.ResponseCode = objRes.reponseDetails.ResponseCode;
                }
                else
                {

                    objRes.reponseDetails = new FranCRMResponse();
                    objRes.reponseDetails.ResponseCode = objRes.reponseDetails.ResponseCode;
                    objRes.reponseDetails.ResponseDesc = objRes.reponseDetails.ResponseDesc;

                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.reponseDetails.ResponseDesc);
            }
            catch (Exception ex)
            {
                objRes.reponseDetails = new FranCRMResponse();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.reponseDetails.ResponseCode = "2";
                objRes.reponseDetails.ResponseDesc = ex.ToString();
            }
            return Json(objRes);
        }


        public ActionResult SubscriberRegistrationEdit_FRA(string RegisterMsisdn)
        {
            CRM.Models.Registration_FRA objReg = new CRM.Models.Registration_FRA();
            CustomerDetailsFRAReq CustReq = new CustomerDetailsFRAReq();
            CustomerDetailFRAResp CustResp = new CustomerDetailFRAResp();
            try
            {
                #region File Upload

                #region File Upload Design

                objReg.DocFormat = clientSetting.preSettings.itaDocumentFormat.ToUpper();
                objReg.Filesize = clientSetting.preSettings.itaAttachFileSize;
                if (!_runningFromNUnit)
                {
                objReg.DocDetails = "1" + "|" + Resources.RegistrationResources.UploadSingleFile;
                }

                #endregion

                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                #region Create path

                string registrationDocpath = clientSetting.preSettings.registrationDocpath;
                if (!Directory.Exists(registrationDocpath))
                {
                    Directory.CreateDirectory(registrationDocpath);
                }
                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    string UploadFile = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string UploadFile = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);

                    if (!Directory.Exists(UploadFile))
                    {
                        Directory.CreateDirectory(UploadFile);
                    }
                }

                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    string PdfDownload = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (!Directory.Exists(PdfDownload))
                    {
                        Directory.CreateDirectory(PdfDownload);
                    }
                }
                #endregion

                #endregion

                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                CustReq.MSISDN = Session["MobileNumber"].ToString();
                CustResp = crmService.CRMGetCustomerDetailsFRA(CustReq);
                if (CustResp.ResponseDetail != null)
                {
                    if (!_runningFromNUnit)
                    {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Edit_FRA_" + CustResp.ResponseDetail.ResponseCode);
                    CustResp.ResponseDetail.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CustResp.ResponseDetail.ResponseDesc : errorInsertMsg;
                    }
                    if (CustResp.ResponseDetail.ResponseCode == "0")
                    {
                        objReg.ResponseCode = CustResp.ResponseDetail.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetail.ResponseDesc;
                        objReg.CustDtls = CustResp.CustomerDetail;
                        objReg.Filename = CustResp.FileName;

                        #region File Upload

                        // string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;

                        #region Delete file from path
                        //if (Server.MapPath("~/App_Data/UploadFile") != null)
                        //{
                        //    var filepath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            var filepath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                            if (Directory.Exists(filepath))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(filepath);
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                                // Directory.Delete(filepath, true);
                            }
                        }

                        if (clientSetting.mvnoSettings.internalPdfDownload != null)
                        {
                            var filepath2 = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                            if (Directory.Exists(filepath2))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(filepath2);
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                                // Directory.Delete(filepath2, true);
                            }
                        }

                        #endregion

                        #region Change Uploaded File Path
                        string sourcePath = ""; string[] GetFileName = new string[3];
                        //if (Server.MapPath("~/App_Data/UploadFile") != null)
                        //{
                        // string targetPath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            string targetPath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                            if (objReg.Filename.Count() > 0)
                            {
                                for (int i = 0; i < objReg.Filename.Count(); i++)
                                {
                                    if (objReg.Filename[i].Filepath != null && objReg.Filename[i].Filepath != "")
                                    {
                                        sourcePath = objReg.Filename[i].Filepath;
                                    }
                                    else
                                    {
                                        sourcePath = clientSetting.preSettings.registrationDocpath;
                                    }
                                    if (!Directory.Exists(targetPath))
                                    {
                                        Directory.CreateDirectory(targetPath);
                                    }
                                    if (System.IO.File.Exists(sourcePath + "\\" + objReg.Filename[i].Filename))
                                    {
                                        System.IO.File.Copy(sourcePath + "\\" + objReg.Filename[i].Filename, targetPath + "\\" + objReg.Filename[i].Filename, true);
                                        string filename = objReg.Filename[i].Filename;
                                        GetFileName[i] = filename;
                                    }
                                }
                                if (objReg.Filename != null)
                                {
                                    for (int i = 0; i < objReg.Filename.Count(); i++)
                                    {
                                        GetFileName[i] = objReg.Filename[i].Filename;
                                    }
                                    GetfileFTP(targetPath + "\\", GetFileName, CustReq, CustResp.ResponseDetail.ResponseDesc);
                                }
                            }
                        }

                        #endregion

                        #endregion

                    }
                    else
                    {
                        objReg.ResponseCode = CustResp.ResponseDetail.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetail.ResponseDesc;
                    }
                }
                else
                {
                    objReg = new Registration_FRA();
                    objReg.ResponseCode = "1";
                    objReg.ResponseDescription = "No Record";
                }
                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,4,14", objReg.IsPostpaid, "drop_master");
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objReg.ResponseDescription);
            }
            catch (Exception ex)
            {
                objReg = new Registration_FRA();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
            }
            return View(objReg);
        }


        public ActionResult SubscriberRegistrationView_FRA(string RegisterMsisdn)
        {
            CRM.Models.Registration_FRA objReg = new CRM.Models.Registration_FRA();
            CustomerDetailsFRAReq CustReq = new CustomerDetailsFRAReq();
            CustomerDetailFRAResp CustResp = new CustomerDetailFRAResp();
            try
            {
                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                //  Session["MobileNumber"] = "345511177250";
                CustReq.MSISDN = Session["MobileNumber"].ToString();
                CustResp = crmService.CRMGetCustomerDetailsFRA(CustReq);
                if (CustResp.ResponseDetail != null)
                {
                    if (!_runningFromNUnit)
                    {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Edit_FRA_" + CustResp.ResponseDetail.ResponseCode);
                    CustResp.ResponseDetail.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CustResp.ResponseDetail.ResponseDesc : errorInsertMsg;
                    }
                    if (CustResp.ResponseDetail.ResponseCode == "0")
                    {
                        objReg.ResponseCode = CustResp.ResponseDetail.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetail.ResponseDesc;
                        objReg.CustDtls = CustResp.CustomerDetail;
                    }
                    else
                    {
                        objReg.ResponseCode = CustResp.ResponseDetail.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetail.ResponseDesc;
                    }
                }
                else
                {
                    objReg = new Registration_FRA();
                    objReg.ResponseCode = "1";
                    objReg.ResponseDescription = "No Record";
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objReg.ResponseDescription);
            }
            catch (Exception ex)
            {
                objReg = new Registration_FRA();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
            }
            return View(objReg);
        }


        #endregion

        #region DENMARK REGISTRATION

        public ActionResult SubscriberRegistration_DEN(string RegisterMsisdn)
        {
            CRM.Models.Registration_DEN objReg = new CRM.Models.Registration_DEN();
            try
            {
                Session["MobileNumber"] = string.Empty;
                objReg.MSISDN = RegisterMsisdn;
                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,2,4,17,18", objReg.IsPostpaid, "drop_master");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objReg);
        }

        [HttpPost]
        public JsonResult SaveRegistration_DEN(string Registration)
        {
            DENRegisterSubscriber objReg = JsonConvert.DeserializeObject<DENRegisterSubscriber>(Registration);
            CRMResponse objRes = new CRMResponse();
            string dob = objReg.DateOfBirth;
            try
            {
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                if (objReg.DateOfBirth != string.Empty)
                {
                    //string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_DEN_" + objRes.ResponseCode);
                    //objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;
                    string strDOB = Utility.GetDateconvertion(objReg.DateOfBirth, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    string[] strSplit = strDOB.Split('/');
                    objReg.BirthDD = strSplit[0].ToString();
                    objReg.BirthMM = strSplit[1].ToString();
                    objReg.BirthYYYY = strSplit[2].ToString();


                }
                else
                {
                    objReg.BirthDD = string.Empty;
                    objReg.BirthMM = string.Empty;
                    objReg.BirthYYYY = string.Empty;
                }

                objRes = crmNewService.CRMRegisterSubscriberDEN(objReg);

                if (objRes != null)
                {
                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_DEN_" + objRes.ResponseCode);
                        objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;
                    }
                    if (objRes.ResponseCode == "0")
                    {
                        if (objReg.Mode == "UPDATE")
                        {
                            if (!_runningFromNUnit)
                            {
                                objRes.ResponseDesc = @Resources.RegistrationResources.UpdatedSuccessfully;
                            }
                            Session["SubscriberTitle"] = objReg.Title;
                            Session["SubscriberName"] = objReg.FirstName + "|" + objReg.LastName;
                            Session["DOB"] = dob;
                        }
                    }
                }
                else
                {
                    objRes = new CRMResponse();
                    objRes.ResponseDesc = Resources.ErrorResources.Common_2;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.ResponseDesc);
            }
            catch (Exception ex)
            {
                objRes = new CRMResponse();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.ResponseCode = "2";
                objRes.ResponseDesc = ex.ToString();
            }

            return Json(objRes);
        }

        public ActionResult SubscriberRegistrationEdit_DEN(string RegisterMsisdn)
        {
            CRM.Models.Registration_DEN objReg = new CRM.Models.Registration_DEN();
            DENGetSubscriberRequest CustReq = new DENGetSubscriberRequest();
            DENGetSubscriberResponse CustResp = new DENGetSubscriberResponse();
            try
            {
                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                //Session["MobileNumber"] = "4500753354";
                CustReq.MSISDN = Session["MobileNumber"].ToString();
                CustResp = crmNewService.CRMGetSubscriberDEN(CustReq);

                if (CustResp.reponseDetails != null)
                {
                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Edit_DEN_" + CustResp.reponseDetails.ResponseCode);
                        CustResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CustResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }

                    if (CustResp.reponseDetails.ResponseCode == "0")
                    {
                        objReg.ResponseCode = CustResp.reponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.reponseDetails.ResponseDesc;
                        objReg.DENGetSubscriber = CustResp.DENGetSubscriber;
                    }
                    else
                    {
                        objReg.ResponseCode = CustResp.reponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.reponseDetails.ResponseDesc;
                    }
                }
                else
                {
                    objReg = new Registration_DEN();
                    objReg.ResponseCode = "1";
                    objReg.ResponseDescription = "No Record";
                }

                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,2,4,17,18", objReg.IsPostpaid, "drop_master");
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objReg.ResponseDescription);
            }
            catch (Exception ex)
            {
                objReg = new Registration_DEN();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
            }
            return View("SubscriberRegistration_DEN", objReg);
        }

        public ActionResult SubscriberRegistrationView_DEN(string RegisterMsisdn)
        {
            CRM.Models.Registration_DEN objReg = new CRM.Models.Registration_DEN();
            DENGetSubscriberRequest CustReq = new DENGetSubscriberRequest();
            DENGetSubscriberResponse CustResp = new DENGetSubscriberResponse();
            try
            {
                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                //Session["MobileNumber"] = "4500753354";
                CustReq.MSISDN = Session["MobileNumber"].ToString();
                CustResp = crmNewService.CRMGetSubscriberDEN(CustReq);

                if (CustResp.reponseDetails != null)
                {
                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Edit_DEN_" + CustResp.reponseDetails.ResponseCode);
                        CustResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CustResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    if (CustResp.reponseDetails.ResponseCode == "0")
                    {
                        objReg.ResponseCode = CustResp.reponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.reponseDetails.ResponseDesc;
                        objReg.DENGetSubscriber = CustResp.DENGetSubscriber;
                    }
                    else
                    {
                        objReg.ResponseCode = CustResp.reponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.reponseDetails.ResponseDesc;
                    }
                }
                else
                {
                    objReg = new Registration_DEN();
                    objReg.ResponseCode = "1";
                    objReg.ResponseDescription = "No Record";
                }

                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,2,4,15,16", Convert.ToString(Session["isPrePaid"]), "drop_master");
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objReg.ResponseDescription);
            }
            catch (Exception ex)
            {
                objReg = new Registration_DEN();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
            }
            return View(objReg);
        }

        #endregion

        #region SWISS REGISTRATION

        public ActionResult SubscriberRegistration_SWI(string RegisterMsisdn)
        {
            CRM.Models.Registration_SWI objReg = new CRM.Models.Registration_SWI();
            try
            {
                Session["MobileNumber"] = string.Empty;
                objReg.MSISDN = RegisterMsisdn;
                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,2,14,4", objReg.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "Tbl_Nationality")).ToList();

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objReg);
        }

        [HttpPost]
        public JsonResult GetIccid(string RegisterMsisdn)
        {
            CRM.Models.Registration_SWI objReg = new CRM.Models.Registration_SWI();
            CustomerDetailsSWIRequest CustReq = JsonConvert.DeserializeObject<CustomerDetailsSWIRequest>(RegisterMsisdn);
            CustomerDetailSWIResponse CustResp = new CustomerDetailSWIResponse();
            try
            {
                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                CustResp = crmNewService.CRMGetCustomerDetailsSWI(CustReq);
            }
            catch (Exception ex)
            {
                objReg = new Registration_SWI();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
            }
            return Json(CustResp);
        }
        [HttpPost]
        public JsonResult SaveRegistration_SWI(string Registration)
        {
            RegisterDetailsSWI objReg = JsonConvert.DeserializeObject<RegisterDetailsSWI>(Registration);
            CRMResponse objRes = new CRMResponse();
            string dob = objReg.Dateofbirth;
            try
            {
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                if (objReg.Dateofbirth != string.Empty)
                {
                    string strDOB = Utility.GetDateconvertion(objReg.Dateofbirth, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    string[] strSplit = strDOB.Split('/');
                    objReg.birthdd = strSplit[0].ToString();
                    objReg.birthmm = strSplit[1].ToString();
                    objReg.birthyy = strSplit[2].ToString();
                }
                else
                {
                    objReg.birthdd = string.Empty;
                    objReg.birthmm = string.Empty;
                    objReg.birthyy = string.Empty;
                }

                objRes = crmNewService.CRMRegisterSubscriberSWI(objReg);

                if (objRes != null)
                {
                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_SWI_" + objRes.ResponseCode);
                        objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;
                    }
                    if (objRes.ResponseCode == "0")
                    {
                        if (objReg.Mode == "UPDATE")
                        {
                            if (!_runningFromNUnit)
                            {
                                objRes.ResponseDesc = @Resources.RegistrationResources.UpdatedSuccessfully;
                            }
                            Session["SubscriberTitle"] = objReg.Title;
                            Session["SubscriberName"] = objReg.Firstname + "|" + objReg.Lastname;
                            Session["DOB"] = dob;
                        }
                    }
                }
                else
                {
                    objRes = new CRMResponse();
                    objRes.ResponseDesc = Resources.ErrorResources.Common_2;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.ResponseDesc);
            }
            catch (Exception ex)
            {
                objRes = new CRMResponse();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.ResponseCode = "2";
                objRes.ResponseDesc = ex.ToString();
            }

            return Json(objRes);
        }

        public ActionResult SubscriberRegistrationEdit_SWI(string RegisterMsisdn)
        {
            CRM.Models.Registration_SWI objReg = new CRM.Models.Registration_SWI();
            CustomerDetailsSWIRequest CustReq = new CustomerDetailsSWIRequest();
            CustomerDetailSWIResponse CustResp = new CustomerDetailSWIResponse();
            try
            {
                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                //Session["MobileNumber"] = "41999910990";
                CustReq.MSISDN = Session["MobileNumber"].ToString();
                CustReq.Mode = "";
                CustResp = crmNewService.CRMGetCustomerDetailsSWI(CustReq);

                if (CustResp.ResponseDetails != null)
                {
                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Edit_SWI_" + CustResp.ResponseDetails.ResponseCode);
                        CustResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CustResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }

                    if (CustResp.ResponseDetails.ResponseCode == "0")
                    {
                        objReg.ResponseCode = CustResp.ResponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetails.ResponseDesc;
                        objReg.CustomerDetailSWI = CustResp.CustomerDetailSWI;
                    }
                    else
                    {
                        objReg.ResponseCode = CustResp.ResponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetails.ResponseDesc;
                    }
                }
                else
                {
                    objReg = new Registration_SWI();
                    objReg.ResponseCode = "1";
                    objReg.ResponseDescription = "No Record";
                }

                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";

                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,2,14,4", objReg.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "Tbl_Nationality")).ToList();


                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objReg.ResponseDescription);
            }
            catch (Exception ex)
            {
                objReg = new Registration_SWI();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
            }
            return View("SubscriberRegistration_SWI", objReg);
        }

        public ActionResult GetPreRegisterDetails(string Register)
        {
            CRM.Models.Registration_SWI objReg = new CRM.Models.Registration_SWI();
            CustomerDetailsSWIRequest CustReq = JsonConvert.DeserializeObject<CustomerDetailsSWIRequest>(Register);
            CustomerDetailSWIResponse CustResp = new CustomerDetailSWIResponse();
            try
            {
                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                if (CustReq.Birthdd != string.Empty)
                {
                    string strDOB = Utility.GetDateconvertion(CustReq.Birthdd, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    string[] strSplit = strDOB.Split('/');
                    CustReq.Birthdd = strSplit[0].ToString();
                    CustReq.Birthmm = strSplit[1].ToString();
                    CustReq.Birthyy = strSplit[2].ToString();
                }
                CustReq.Mode = "";
                CustResp = crmNewService.CRMGetCustomerDetailsSWI(CustReq);

                if (CustResp != null && CustResp.ResponseDetails != null)
                {
                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_SWI_" + CustResp.ResponseDetails.ResponseCode);
                        CustResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CustResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    if (CustResp.ResponseDetails.ResponseCode == "0")
                    {
                        objReg.ResponseCode = CustResp.ResponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetails.ResponseDesc;
                        objReg.CustomerDetailSWI = CustResp.CustomerDetailSWI;
                    }
                    else
                    {
                        objReg.ResponseCode = CustResp.ResponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetails.ResponseDesc;
                    }
                }
                else
                {
                    objReg = new Registration_SWI();
                    objReg.ResponseCode = "1";
                    objReg.ResponseDescription = Resources.RegistrationResources.PreRegNoRecord;
                }

                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21", objReg.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "TblCountry")).ToList();


                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objReg.ResponseDescription);
            }
            catch (Exception ex)
            {
                objReg = new Registration_SWI();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
            }
            return Json(objReg);
        }

        public ActionResult SubscriberRegistrationView_SWI(string RegisterMsisdn)
        {
            CRM.Models.Registration_SWI objReg = new CRM.Models.Registration_SWI();
            CustomerDetailsSWIRequest CustReq = new CustomerDetailsSWIRequest();
            CustomerDetailSWIResponse CustResp = new CustomerDetailSWIResponse();
            try
            {
                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                //Session["MobileNumber"] = "41999910990";
                CustReq.MSISDN = Session["MobileNumber"].ToString();
                CustResp = crmNewService.CRMGetCustomerDetailsSWI(CustReq);

                if (CustResp.ResponseDetails != null)
                {
                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Edit_SWI_" + CustResp.ResponseDetails.ResponseCode);
                        CustResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CustResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }

                    if (CustResp.ResponseDetails.ResponseCode == "0")
                    {
                        objReg.ResponseCode = CustResp.ResponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetails.ResponseDesc;
                        objReg.CustomerDetailSWI = CustResp.CustomerDetailSWI;
                    }
                    else
                    {
                        objReg.ResponseCode = CustResp.ResponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetails.ResponseDesc;
                    }
                }
                else
                {
                    objReg = new Registration_SWI();
                    objReg.ResponseCode = "1";
                    objReg.ResponseDescription = "No Record";
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objReg.ResponseDescription);
            }
            catch (Exception ex)
            {
                objReg = new Registration_SWI();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
            }
            return View(objReg);
        }

        #endregion

        #region TUNISIA REGISTRATION

        //----------------------C.GOPIKUMAR (EmpID-2296) and Vignesh (EmpID-3538)-----------------------------------

        public ActionResult SubscriberRegistration_TUN(string RegisterMsisdn)
        {
            CRM.Models.Registration_TUN objReg = new CRM.Models.Registration_TUN();
            try
            {
                Session["MobileNumber"] = string.Empty;
                objReg.MSISDN = RegisterMsisdn;

                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("4,14", objReg.IsPostpaid, "drop_master");
                #region FILE UPLOAD

                #region File Upload

                objReg.Mode = "INSERT";
                objReg.DocFormat = clientSetting.mvnoSettings.documentFormat.ToUpper();

                int iDocCount = (clientSetting.mvnoSettings.signedDoc != null && Convert.ToString(clientSetting.mvnoSettings.signedDoc).ToUpper() == "TRUE") ? 1 : 0;


                if (!_runningFromNUnit)
                {
                    string ErrorMsg = Resources.ErrorResources.EReg_ITA_doc3;
                    if ((objReg.IsPostpaid == "1" && clientSetting.mvnoSettings.isAttachmentMand.ToUpper() == "TRUE") || (objReg.IsPostpaid == "0" && clientSetting.mvnoSettings.isAttachmentMand.ToUpper() == "TRUE"))
                    {
                        if (clientSetting.mvnoSettings.mandatoryDoc != null && Convert.ToInt16(clientSetting.mvnoSettings.mandatoryDoc) > 0)
                        {
                            if (iDocCount == 1)
                                ErrorMsg = string.Format(Resources.ErrorResources.EReg_ITA_doc2, Convert.ToInt16(clientSetting.mvnoSettings.mandatoryDoc));
                            else
                                ErrorMsg = string.Format(Resources.ErrorResources.EReg_ITA_doc1, Convert.ToInt16(clientSetting.mvnoSettings.mandatoryDoc));

                            iDocCount = iDocCount + Convert.ToInt16(clientSetting.mvnoSettings.mandatoryDoc);
                        }
                        objReg.DocDetails = iDocCount.ToString() + "|" + ErrorMsg;
                    }
                    else
                    {
                        objReg.DocDetails = "0|";
                    }
                }

                #endregion

                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                #region Create path

                string registrationDocpath = clientSetting.preSettings.registrationDocpath;
                if (!Directory.Exists(registrationDocpath))
                {
                    Directory.CreateDirectory(registrationDocpath);
                }
                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    string UploadFile = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string UploadFile = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    if (!Directory.Exists(UploadFile))
                    {
                        Directory.CreateDirectory(UploadFile);
                    }
                }
                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    string PdfDownload = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (!Directory.Exists(PdfDownload))
                    {
                        Directory.CreateDirectory(PdfDownload);
                    }
                }

                #endregion


                #region Delete file from path

                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    var filepath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    var filepath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    if (Directory.Exists(filepath))
                    {
                        System.IO.DirectoryInfo di = new DirectoryInfo(filepath);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                    }
                }
                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    var filepath2 = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (Directory.Exists(filepath2))
                    {
                        System.IO.DirectoryInfo di = new DirectoryInfo(filepath2);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                    }
                }


                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objReg);
        }

        [HttpPost]
        public JsonResult SaveRegistration_TUN(string Registration)
        {
            TUNRegisterSubscriber objReg = JsonConvert.DeserializeObject<TUNRegisterSubscriber>(Registration);
            TUNRegisterSubscriberResponse objRes = new TUNRegisterSubscriberResponse();

            //StringBuilder sb = new StringBuilder();

            string dob = objReg.DateOfBirth;
            try
            {
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                if (objReg.DateOfBirth != string.Empty)
                {
                    string strDOB = Utility.GetDateconvertion(objReg.DateOfBirth, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    if (strDOB != "")
                    {
                        string[] strSplit = strDOB.Split('/');
                        objReg.BirthDD = strSplit[0].ToString();
                        objReg.BirthMM = strSplit[1].ToString();
                        objReg.BirthYYYY = strSplit[2].ToString();
                    }
                    else
                    {
                        objReg.BirthDD = string.Empty;
                        objReg.BirthMM = string.Empty;
                        objReg.BirthYYYY = string.Empty;
                    }
                }
                else
                {
                    objReg.BirthDD = string.Empty;
                    objReg.BirthMM = string.Empty;
                    objReg.BirthYYYY = string.Empty;
                }
                if (objReg.Mode == "UPDATE")
                {
                    string[] oldSplit = objReg.OldDocumentID.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    objReg.OldDocumentID = "";
                    for (int i = 0; i < oldSplit.Length; i++)
                    {
                        string[] oldNameID = oldSplit[i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        if (objReg.OldDocumentID == "")
                        {
                            objReg.OldDocumentID = oldNameID[1];
                        }
                        else
                        {
                            objReg.OldDocumentID = objReg.OldDocumentID + "," + oldNameID[1];
                        }

                    }
                }



                //objReg.Filepath = sb.ToString();
                objRes = crmNewService.CRMRegisterSubscriberTUN(objReg);



                if (objRes != null)
                {
                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_TUN_" + objRes.reponseDetails.ResponseCode);
                        objRes.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.reponseDetails.ResponseDesc : errorInsertMsg;

                    }

                    if (objRes.reponseDetails.ResponseCode == "0" || objRes.reponseDetails.ResponseCode == "100" || objRes.reponseDetails.ResponseCode == "110" || objRes.reponseDetails.ResponseCode == "111" || objRes.reponseDetails.ResponseCode == "112" || objRes.reponseDetails.ResponseCode == "113" || objRes.reponseDetails.ResponseCode == "114" || objRes.reponseDetails.ResponseCode == "120" || objRes.reponseDetails.ResponseCode == "121" || objRes.reponseDetails.ResponseCode == "122")
                    {
                        //string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_TUN_" + objRes.reponseDetails.ResponseCode);
                        //objRes.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.reponseDetails.ResponseDesc : errorInsertMsg;

                        #region Change Uploaded File Path
                        string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                        string sourcePath = string.Empty;
                        //if (Server.MapPath("~/App_Data/UploadFile") != null)
                        //{
                        //    sourcePath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            sourcePath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                        }
                        string targetPath = clientSetting.preSettings.registrationDocpath;

                        string AccountNo = string.Empty;
                        if (objReg.Mode == "INSERT")
                        {
                            AccountNo = objRes.RegisterInsertDetails.AccId;
                            if (!_runningFromNUnit)
                            {
                                if (objRes.RIO != null && objRes.RIO != "")
                                {

                                    objRes.reponseDetails.ResponseDesc = objRes.reponseDetails.ResponseDesc + " " + @Resources.ErrorResources.RIOCodeSuccessMsg + objRes.RIO;
                                }
                                else
                                {

                                    objRes.reponseDetails.ResponseDesc = objRes.reponseDetails.ResponseDesc + " " + @Resources.ErrorResources.RIOCodeFailureMsg;
                                }
                            }
                        }
                        else
                        {
                            AccountNo = objReg.AccountNumber;
                        }

                        #region Delete File in Target place
                        for (int i = 0; i < 3; i++)
                        {
                            string[] fileformat = clientSetting.mvnoSettings.ticketAttachFileformat.Split(',');
                            for (int j = 0; j < fileformat.Length; j++)
                            {
                                fileformat[j] = fileformat[j].Replace(".", string.Empty).Trim();
                                System.IO.File.Delete(targetPath + "\\" + AccountNo + "_Doc" + (i + 1) + "." + fileformat[j]);
                            }
                        }

                        #endregion

                        #region Move File Target Place
                        //if (Server.MapPath("~/App_Data/UploadFile") != null)
                        //{

                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            if (Directory.GetFiles(sourcePath).Count() > 0)
                            {
                                string[] fileEntries2 = Directory.GetFiles(sourcePath);
                                string[] filenameList = new string[3];
                                string filename = string.Empty;
                                string Deletefilename = string.Empty;
                                for (int i = 0; i < fileEntries2.Count(); i++)
                                {
                                    string Externsion = fileEntries2[i].Substring(fileEntries2[i].LastIndexOf("\\") + 1);
                                    string[] splitExternsion = Externsion.Split('.');
                                    System.IO.File.Move(sourcePath + "\\" + Externsion, targetPath + "\\" + AccountNo + "_Doc" + (i + 1) + "." + splitExternsion[1]);
                                    filename = AccountNo + "_Doc" + (i + 1) + "." + splitExternsion[1];
                                    filenameList[i] = filename.ToString();
                                    Deletefilename = AccountNo + "_Doc";
                                }
                                MovefileFTP(targetPath + "\\", filenameList, Deletefilename, objReg, objRes.reponseDetails.ResponseDesc);
                            }
                        }
                        #endregion
                        string delFolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                        if (clientSetting.mvnoSettings.internalPdfDownload != null)
                        {
                            string delPath = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                            if (Directory.Exists(delPath))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(delPath);
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                                // Directory.Delete(delPath, true);
                            }
                        }

                        #endregion

                        if (objRes.reponseDetails.ResponseCode == "0")
                        {
                            if (objReg.Mode == "UPDATE")
                            {
                                if (!_runningFromNUnit)
                                {
                                    objRes.reponseDetails.ResponseDesc = @Resources.RegistrationResources.UpdatedSuccessfully;
                                }
                                Session["SubscriberTitle"] = objReg.Title;
                                Session["SubscriberName"] = objReg.FirstName + "|" + objReg.LastName;
                                Session["DOB"] = dob;
                            }



                        }
                    }
                    else
                    {
                        // objRes = new TUNRegisterSubscriberResponse();
                        if (objRes.reponseDetails != null)
                            objRes.reponseDetails.ResponseDesc = objRes.reponseDetails.ResponseDesc;
                        else
                        {
                            objRes.reponseDetails = new CRMResponse();
                            objRes.reponseDetails.ResponseDesc = Resources.ErrorResources.Common_2;
                        }
                    }
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.reponseDetails.ResponseDesc);
                }
            }
            catch (Exception ex)
            {
                objRes = new TUNRegisterSubscriberResponse();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.reponseDetails = new CRMResponse();
                objRes.reponseDetails.ResponseCode = "2";
                objRes.reponseDetails.ResponseDesc = ex.ToString();
            }

            return Json(objRes);
        }

        public ActionResult SubscriberRegistrationEdit_TUN(string RegisterMsisdn)
        {
            CRM.Models.Registration_TUN objReg = new CRM.Models.Registration_TUN();
            TUNRegisterSubscriber CustReq = new TUNRegisterSubscriber();
            TUNRegisterSubscriberResponse CustResp = new TUNRegisterSubscriberResponse();
            try
            {

                #region Design related Coding

                #region Dropdown List

                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("4,14", objReg.IsPostpaid, "drop_master");

                #endregion

                #region File Upload
                objReg.Mode = "INSERT";
                objReg.DocFormat = clientSetting.mvnoSettings.documentFormat.ToUpper();

                //int iDocCount = (clientSetting.preSettings.itaSignedDoc != null && Convert.ToString(clientSetting.preSettings.itaSignedDoc).ToUpper() == "TRUE") ? 1 : 0;
                int iDocCount = (clientSetting.mvnoSettings.signedDoc != null && Convert.ToString(clientSetting.mvnoSettings.signedDoc).ToUpper() == "TRUE") ? 1 : 0;

                if (!_runningFromNUnit)
                {
                    string ErrorMsg = Resources.ErrorResources.EReg_ITA_doc3;
                    if ((objReg.IsPostpaid == "1" && clientSetting.mvnoSettings.isAttachmentMand.ToUpper() == "TRUE") || (objReg.IsPostpaid == "0" && clientSetting.mvnoSettings.isAttachmentMand.ToUpper() == "TRUE"))
                    {
                        if (clientSetting.mvnoSettings.mandatoryDoc != null && Convert.ToInt16(clientSetting.mvnoSettings.mandatoryDoc) > 0)
                        {
                            if (iDocCount == 1)
                                ErrorMsg = string.Format(Resources.ErrorResources.EReg_ITA_doc2, Convert.ToInt16(clientSetting.mvnoSettings.mandatoryDoc));
                            else
                                ErrorMsg = string.Format(Resources.ErrorResources.EReg_ITA_doc1, Convert.ToInt16(clientSetting.mvnoSettings.mandatoryDoc));

                            iDocCount = iDocCount + Convert.ToInt16(clientSetting.mvnoSettings.mandatoryDoc);
                        }
                        objReg.DocDetails = iDocCount.ToString() + "|" + ErrorMsg;
                    }
                    else
                    {
                        objReg.DocDetails = "0|";
                    }
                }

                #endregion

                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                #region Create path

                string registrationDocpath = clientSetting.preSettings.registrationDocpath;
                if (!Directory.Exists(registrationDocpath))
                {
                    Directory.CreateDirectory(registrationDocpath);
                }
                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    string UploadFile = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string UploadFile = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    if (!Directory.Exists(UploadFile))
                    {
                        Directory.CreateDirectory(UploadFile);
                    }
                }
                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    string PdfDownload = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (!Directory.Exists(PdfDownload))
                    {
                        Directory.CreateDirectory(PdfDownload);
                    }
                }
                #endregion

                #endregion

                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                // Session["MobileNumber"] = "2169564500004";
                CustReq.Msisdn = Session["MobileNumber"].ToString();
                CustReq.Mode = "VIEW";

                CustResp = crmNewService.CRMRegisterSubscriberTUN(CustReq);

                if (CustResp.reponseDetails != null)
                {
                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Edit_TUN_" + CustResp.reponseDetails.ResponseCode);
                        CustResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CustResp.reponseDetails.ResponseDesc : errorInsertMsg;

                    }
                    if (CustResp.reponseDetails.ResponseCode == "0")
                    {
                        objReg.ResponseCode = CustResp.reponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.reponseDetails.ResponseDesc;
                        objReg.TUNGetSubscriber = CustResp.TUNGetSubscriber;
                        objReg.FileName = CustResp.FileName;
                        objReg.TUNGetSubscriber.ConfirmEMail = CustResp.TUNGetSubscriber.EmailAddress;

                        #region File Upload

                        // string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;

                        #region Delete file from path
                        //if (Server.MapPath("~/App_Data/UploadFile") != null)
                        //{
                        //    var filepath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            var filepath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                            if (Directory.Exists(filepath))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(filepath);
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                                //  Directory.Delete(filepath, true);
                            }
                        }
                        if (clientSetting.mvnoSettings.internalPdfDownload != null)
                        {
                            var filepath2 = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                            if (Directory.Exists(filepath2))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(filepath2);
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                                // Directory.Delete(filepath2, true);
                            }
                        }


                        #endregion

                        #region Change Uploaded File Path
                        string sourcePath = "";
                        string[] GetFileName = new string[3];
                        //if (Server.MapPath("~/App_Data/UploadFile") != null)
                        //{
                        //    string targetPath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            string targetPath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                            objReg.FileName.Clear();
                            if (objReg.FileName.Count() > 0)
                            {
                                for (int i = 0; i < objReg.FileName.Count(); i++)
                                {
                                    //-------------CBOS Upload File-------------------Gopi2296-------------------Start------------------
                                    if (objReg.FileName[i].Filename.Contains("/"))
                                    {
                                        string fName = string.Empty;
                                        fName = objReg.FileName[i].Filename.Substring(objReg.FileName[i].Filename.LastIndexOf("/") + 1);
                                        objReg.FileName[i].Filename = fName;
                                    }
                                    //-------------CBOS Upload File-------------------Gopi2296--------------------End-------------------
                                    if (objReg.FileName[i].Filepath != "")
                                    {
                                        sourcePath = objReg.FileName[i].Filepath;
                                    }
                                    else
                                    {
                                        sourcePath = clientSetting.preSettings.registrationDocpath;
                                    }
                                    if (!Directory.Exists(targetPath))
                                    {
                                        Directory.CreateDirectory(targetPath);
                                    }
                                    if (System.IO.File.Exists(sourcePath + "\\" + objReg.FileName[i].Filename))
                                    {
                                        System.IO.File.Copy(sourcePath + "\\" + objReg.FileName[i].Filename, targetPath + "\\" + objReg.FileName[i].Filename, true);
                                        string filename = objReg.FileName[i].Filename;
                                        GetFileName[i] = filename;
                                    }

                                }
                                if (objReg.FileName != null)
                                {
                                    for (int i = 0; i < objReg.FileName.Count(); i++)
                                    {
                                        //-------------CBOS Upload File-------------------Gopi2296-------------------Start------------------
                                        if (objReg.FileName[i].Filename.Contains("/"))
                                        {
                                            string fName = string.Empty;
                                            fName = objReg.FileName[i].Filename.Substring(objReg.FileName[i].Filename.LastIndexOf("/") + 1);
                                            GetFileName[i] = fName;
                                        }
                                        //-------------CBOS Upload File-------------------Gopi2296--------------------End-------------------
                                        GetFileName[i] = objReg.FileName[i].Filename;
                                    }
                                    GetfileFTP(targetPath + "\\", GetFileName, CustReq, CustResp.reponseDetails.ResponseDesc);
                                }
                            }
                        }

                        #endregion

                        #endregion

                    }
                    else
                    {
                        objReg.ResponseCode = CustResp.reponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.reponseDetails.ResponseDesc;
                    }
                }
                else
                {
                    objReg = new Registration_TUN();
                    objReg.ResponseCode = "1";
                    objReg.ResponseDescription = "No Record";
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objReg.ResponseDescription);
            }
            catch (Exception ex)
            {
                objReg = new Registration_TUN();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
                #region Dropdown List

                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,4,13", objReg.IsPostpaid, "drop_master");

                #endregion
            }
            return View("SubscriberRegistration_TUN", objReg);
        }

        public ActionResult SubscriberRegistrationView_TUN(string RegisterMsisdn)
        {
            CRM.Models.Registration_TUN objReg = new CRM.Models.Registration_TUN();
            TUNRegisterSubscriber CustReq = new TUNRegisterSubscriber();
            TUNRegisterSubscriberResponse CustResp = new TUNRegisterSubscriberResponse();
            try
            {
                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                //Session["MobileNumber"] = "41999910990";
                CustReq.Msisdn = Session["MobileNumber"].ToString();
                CustReq.Mode = "VIEW";
                CustResp = crmNewService.CRMRegisterSubscriberTUN(CustReq);

                if (CustResp.reponseDetails != null)
                {
                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Edit_TUN_" + CustResp.reponseDetails.ResponseCode);
                        CustResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CustResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    if (CustResp.reponseDetails.ResponseCode == "0")
                    {
                        objReg.ResponseCode = CustResp.reponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.reponseDetails.ResponseDesc;
                        objReg.TUNGetSubscriber = CustResp.TUNGetSubscriber;

                        objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                        objReg.strDropdown = Utility.GetIVRLanguage();
                        if (objReg.TUNGetSubscriber.PrefLanguage != "" && objReg.TUNGetSubscriber.PrefLanguage != null && objReg.TUNGetSubscriber.PrefLanguage != "0")
                        {
                            DropdownMaster dropdown = objReg.strDropdown.FirstOrDefault(b => b.ID == objReg.TUNGetSubscriber.PrefLanguage && b.Master_id == "4");
                            objReg.TUNGetSubscriber.PrefLanguage = dropdown != null ? dropdown.Value : "";
                        }
                        else
                        {
                            objReg.TUNGetSubscriber.PrefLanguage = "";
                        }




                    }
                    else
                    {
                        objReg.ResponseCode = CustResp.reponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.reponseDetails.ResponseDesc;
                    }
                }
                else
                {
                    objReg = new Registration_TUN();
                    objReg.ResponseCode = "1";
                    objReg.ResponseDescription = "No Record";
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objReg.ResponseDescription);
            }
            catch (Exception ex)
            {
                objReg = new Registration_TUN();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
            }
            return View(objReg);
        }

        public ActionResult GenPDF_TUN(string Mode)
        {
            if (Mode == "Eng PDF")
            {
                string cultureCode = "en";
                HttpCookie cookieCultureLanguage = new HttpCookie("UserLanguage") { Value = cultureCode };
                Response.Cookies.Set(cookieCultureLanguage);
                Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(cultureCode);
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(cultureCode);
            }
            return PartialView();
        }

        #endregion

        #region COMMON POSTPAID REGISTRATION

        public ActionResult SubscriberRegistration_AllPostpaid(string RegisterMsisdn)
        {
            CRM.Models.Registration_AllPostpaid objReg = new CRM.Models.Registration_AllPostpaid();
            try
            {

                #region LOAD CORPORATE

                CRMBase cbase = new CRMBase();
                cbase.CountryCode = clientSetting.countryCode;
                cbase.BrandCode = clientSetting.brandCode;
                cbase.LanguageCode = clientSetting.langCode;
                GetPlansfromPBS objres = crmNewService.CRMGetPlansfromPBS(cbase);
                objReg.drpdownBillCycle = objres.Billcycledatelist;
                objReg.drpdownWholesaler = objres.Wholesaleplanlist;
                TempData["Corporate"] = objres.Corporatelist;

                #endregion

                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                #region Create path

                string registrationDocpath = clientSetting.postSettings.registrationDocpath;
                if (!Directory.Exists(registrationDocpath))
                {
                    Directory.CreateDirectory(registrationDocpath);
                }
                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    string UploadFile = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string UploadFile = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    if (!Directory.Exists(UploadFile))
                    {
                        Directory.CreateDirectory(UploadFile);
                    }
                }
                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    string PdfDownload = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (!Directory.Exists(PdfDownload))
                    {
                        Directory.CreateDirectory(PdfDownload);
                    }
                }

                #endregion

                #region Delete file from path
                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    var filepath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    var filepath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    if (Directory.Exists(filepath))
                    {
                        // System.IO.File.Delete(filepath);
                        System.IO.DirectoryInfo di = new DirectoryInfo(filepath);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                    }
                }
                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    var filepath2 = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (Directory.Exists(filepath2))
                    {
                        System.IO.DirectoryInfo di1 = new DirectoryInfo(filepath2);

                        foreach (FileInfo file in di1.GetFiles())
                        {
                            file.Delete();
                        }
                    }
                }

                #endregion

                Session["MobileNumber"] = string.Empty;
                objReg.MSISDN = RegisterMsisdn;
                Session["isPrePaid"] = "0";
                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "0";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("31,1,100,2,28,11,13,4,16,20,8,29,35,10", objReg.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "Tbl_Nationality")).ToList();
                objReg.drpcorporate = objres.Corporatelist;
                objReg.DocFormat = clientSetting.mvnoSettings.documentFormat.ToUpper();

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objReg);
        }

        public JsonResult GET_CountryBasedDetils(string strPlanType)
        {
            CRM.Models.Registration_AllPostpaid objReg = new CRM.Models.Registration_AllPostpaid();
            CommonPostpaidRequest objReq = JsonConvert.DeserializeObject<CommonPostpaidRequest>(strPlanType);
            CommonPostpaidResponse objResp = new CommonPostpaidResponse();
            try
            {
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objResp = crmNewService.CRMCommonPostpaid(objReq);
                //objReg.commonpostpaidlist = objResp.commonpostpaidlist;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objResp);
        }

        [HttpPost]
        public JsonResult SaveRegistration_AllPostpaid(string Registration)
        {
            RegisterInsertPostpaidRequest objReg = JsonConvert.DeserializeObject<RegisterInsertPostpaidRequest>(Registration);
            RegisterInsertPostpaidResponse objRes = new RegisterInsertPostpaidResponse();
            StringBuilder sb = new StringBuilder();

            objReg.IsCommonReg = "1";

            string dob = objReg.DateOfBirth;
            try
            {
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                #region Date Convertion
                if (objReg.DateOfBirth != string.Empty)
                {
                    string strDOB = Utility.GetDateconvertion(objReg.DateOfBirth, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    if (strDOB != "")
                    {
                        string[] strSplit = strDOB.Split('/');
                        objReg.Birthdd = Convert.ToInt16(strSplit[0].ToString());
                        objReg.Birthmm = Convert.ToInt16(strSplit[1].ToString());
                        objReg.Birthyy = Convert.ToInt16(strSplit[2].ToString());
                    }
                    else
                    {
                        objReg.Birthdd = 0;
                        objReg.Birthmm = 0;
                        objReg.Birthyy = 0;
                    }
                }
                else
                {
                    objReg.Birthdd = 0;
                    objReg.Birthmm = 0;
                    objReg.Birthyy = 0;
                }
                if (objReg.IssueDate != string.Empty)
                {
                    string strDOB = Utility.GetDateconvertion(objReg.IssueDate, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    if (strDOB != "")
                    {
                        string[] strSplit = strDOB.Split('/');
                        objReg.Issuedd = Convert.ToString(strSplit[0]);
                        objReg.Issuemm = Convert.ToString(strSplit[1]);
                        objReg.Issueyy = Convert.ToString(strSplit[2]);
                    }
                    else
                    {
                        objReg.Issuedd = "";
                        objReg.Issuemm = "";
                        objReg.Issueyy = "";
                    }
                }
                else
                {
                    objReg.Issuedd = "";
                    objReg.Issuemm = "";
                    objReg.Issueyy = "";
                }
                if (objReg.IDValidateFullDate != string.Empty)
                {
                    string strDOB = Utility.GetDateconvertion(objReg.IDValidateFullDate, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    if (strDOB != "")
                    {
                        string[] strSplit = strDOB.Split('/');
                        objReg.Idvaliddate = Convert.ToInt16(strSplit[0].ToString());
                        objReg.Idvalidmonth = Convert.ToInt16(strSplit[1].ToString());
                        objReg.Idvalidyear = Convert.ToInt16(strSplit[2].ToString());
                    }
                    else
                    {
                        objReg.Idvaliddate = 0;
                        objReg.Idvalidmonth = 0;
                        objReg.Idvalidyear = 0;
                    }
                }
                else
                {
                    objReg.Idvaliddate = 0;
                    objReg.Idvalidmonth = 0;
                    objReg.Idvalidyear = 0;
                }
                #endregion
                if (objReg.mode == "UPDATE")
                {
                    string[] oldSplit = objReg.OldDocumentID.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    objReg.OldDocumentID = "";
                    for (int i = 0; i < oldSplit.Length; i++)
                    {
                        string[] oldNameID = oldSplit[i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        if (objReg.OldDocumentID == "")
                        {
                            objReg.OldDocumentID = oldNameID[1];
                        }
                        else
                        {
                            objReg.OldDocumentID = objReg.OldDocumentID + "," + oldNameID[1];
                        }

                    }
                }

                if (objReg.ExpiryDate != null)
                {
                    if (objReg.ExpiryDate.Length > 6)
                    {
                        objReg.ExpiryDate = objReg.ExpiryDate.Replace(" / ", string.Empty);
                    }
                }

                #region Document Rename

                if (objReg.Doc1 == "Filename")
                {
                    objReg.Doc1 = "";
                }
                if (objReg.Doc2 == "Filename")
                {
                    objReg.Doc2 = "";
                }
                if (objReg.Doc3 == "Filename")
                {
                    objReg.Doc3 = "";
                }

                #endregion

                objRes = crmNewService.CRMCommonpostpaidRegister(objReg);

                if (objRes != null && objRes.ResponseDetails != null && objRes.ResponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Reg_CommonPostpaid_" + objRes.ResponseDetails.ResponseCode);
                    objRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                    if (objRes.ResponseDetails.ResponseCode == "0" || objRes.ResponseDetails.ResponseCode == "50" || objRes.ResponseDetails.ResponseCode == "51" || objRes.ResponseDetails.ResponseCode == "52" || objRes.ResponseDetails.ResponseCode == "53" || objRes.ResponseDetails.ResponseCode == "54" || objRes.ResponseDetails.ResponseCode == "55" || objRes.ResponseDetails.ResponseCode == "56" || objRes.ResponseDetails.ResponseCode == "57" || objRes.ResponseDetails.ResponseCode == "58" || objRes.ResponseDetails.ResponseCode == "60")
                    {
                        if (objReg.mode != "VALIDATE")
                        {
                            if (objReg.mode == "UPDATE")
                            {
                                //objRes.ResponseDetails.ResponseDesc = @Resources.RegistrationResources.UpdatedSuccessfully;
                                string errorUpdatetMsg = Resources.ErrorResources.ResourceManager.GetString("Upd_CommonPostpaid_" + objRes.ResponseDetails.ResponseCode);
                                objRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorUpdatetMsg) ? objRes.ResponseDetails.ResponseDesc : errorUpdatetMsg;
                                Session["SubscriberTitle"] = objReg.Title;
                                Session["SubscriberName"] = objReg.FirstName + "|" + objReg.LastName;
                                Session["DOB"] = dob;
                            }

                            #region Change Uploaded File Path

                            string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                            string sourcePath = string.Empty;
                            //if (Server.MapPath("~/App_Data/UploadFile") != null)
                            //{
                            //    sourcePath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                            if (clientSetting.mvnoSettings.internalUploadFile != null)
                            {
                                sourcePath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                            }
                            string targetPath = clientSetting.postSettings.registrationDocpath;

                            string AccountNo = string.Empty;
                            if (objReg.mode == "INSERT")
                            {
                                AccountNo = objRes.RegisterInsertPostpaid[0].AccountNo;
                            }
                            else
                            {
                                AccountNo = objRes.RegisterInsertPostpaid[0].AccountNo;
                            }
                            #region delete
                            for (int i = 0; i < 3; i++)
                            {
                                string[] fileformat = clientSetting.mvnoSettings.ticketAttachFileformat.Split(',');
                                for (int j = 0; j < fileformat.Length; j++)
                                {
                                    fileformat[j] = fileformat[j].Replace(".", string.Empty).Trim();
                                    System.IO.File.Delete(targetPath + "\\" + AccountNo + "_Doc" + (i + 1) + "." + fileformat[j]);
                                }
                            }
                            #endregion

                            #region Move
                            // if (Server.MapPath("~/App_Data/UploadFile") != null)

                            if (clientSetting.mvnoSettings.internalUploadFile != null)
                            {
                                if (Directory.GetFiles(sourcePath).Count() > 0)
                                {
                                    string[] fileEntries2 = Directory.GetFiles(sourcePath);
                                    string[] filenameList = new string[3];
                                    string Deletefilename = string.Empty;
                                    string filename = string.Empty;
                                    for (int i = 0; i < fileEntries2.Count(); i++)
                                    {
                                        string Externsion = fileEntries2[i].Substring(fileEntries2[i].LastIndexOf("\\") + 1);
                                        string[] splitExternsion = Externsion.Split('.');
                                        System.IO.File.Move(sourcePath + "\\" + Externsion, targetPath + "\\" + AccountNo + "_Doc" + (i + 1) + "." + splitExternsion[1]);
                                        Deletefilename = AccountNo + "_Doc";
                                        filename = AccountNo + "_Doc" + (i + 1) + "." + splitExternsion[1];
                                        filenameList[i] = filename.ToString();
                                    }

                                    MovefileFTP(targetPath + "\\", filenameList, Deletefilename, objReg, objRes.ResponseDetails.ResponseDesc);
                                }
                            }
                            #endregion
                            string delFolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                            if (clientSetting.mvnoSettings.internalPdfDownload != null)
                            {
                                string delPath = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                                if (Directory.Exists(delPath))
                                {
                                    System.IO.DirectoryInfo di = new DirectoryInfo(delPath);
                                    foreach (FileInfo file in di.GetFiles())
                                    {
                                        file.Delete();
                                    }
                                }
                            }

                            #endregion
                        }

                    }
                    else
                    {
                        //objRes = new RegisterInsertPostpaidResponse();
                        if (objRes.ResponseDetails != null)
                            objRes.ResponseDetails.ResponseDesc = objRes.ResponseDetails.ResponseDesc;


                    }
                    if (objRes.ResponseDetails != null)
                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.ResponseDetails.ResponseDesc);
                }
            }
            catch (Exception ex)
            {
                objRes = new RegisterInsertPostpaidResponse();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.ResponseDetails = new CRMResponse();
                objRes.ResponseDetails.ResponseCode = "2";
                objRes.ResponseDetails.ResponseDesc = ex.ToString();
            }

            return Json(objRes);
        }

        public ActionResult SubscriberRegistrationEdit_AllPostpaid(string RegisterMsisdn)
        {
            CRM.Models.Registration_AllPostpaid objReg = new CRM.Models.Registration_AllPostpaid();
            CommonpostpaidGetRequest CustReq = new CommonpostpaidGetRequest();
            CommonpostpaidGetResponse CustResp = new CommonpostpaidGetResponse();
            try
            {
                #region Create path

                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;

                string registrationDocpath = clientSetting.postSettings.registrationDocpath;
                if (!Directory.Exists(registrationDocpath))
                {
                    Directory.CreateDirectory(registrationDocpath);
                }
                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    string UploadFile = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string UploadFile = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                    if (!Directory.Exists(UploadFile))
                    {
                        Directory.CreateDirectory(UploadFile);
                    }
                }
                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    string PdfDownload = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (!Directory.Exists(PdfDownload))
                    {
                        Directory.CreateDirectory(PdfDownload);
                    }
                }
                #endregion

                #region LOAD CORPORATE

                CRMBase cbase = new CRMBase();
                cbase.CountryCode = clientSetting.countryCode;
                cbase.BrandCode = clientSetting.brandCode;
                cbase.LanguageCode = clientSetting.langCode;
                GetPlansfromPBS objres = crmNewService.CRMGetPlansfromPBS(cbase);
                objReg.drpdownBillCycle = objres.Billcycledatelist;
                objReg.drpdownWholesaler = objres.Wholesaleplanlist;
                TempData["Corporate"] = objres.Corporatelist;

                #endregion
                objReg.drpcorporate = objres.Corporatelist;
                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                //Session["MobileNumber"] = "447798100044";
                CustReq.MSISDN = Session["MobileNumber"].ToString();
                CustResp = crmNewService.CRMCommonpostpaidGetcustomer(CustReq);
                objReg.DocFormat = clientSetting.mvnoSettings.documentFormat.ToUpper();

                if (CustResp.ResponseDetails != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Edit_CommonPostpaid_" + CustResp.ResponseDetails.ResponseCode);
                    CustResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CustResp.ResponseDetails.ResponseDesc : errorInsertMsg;

                    if (CustResp.ResponseDetails.ResponseCode == "0")
                    {
                        objReg.ResponseCode = CustResp.ResponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetails.ResponseDesc;
                        objReg.CustomerDetails = CustResp.CustomerDetails;
                        objReg.Corporate = CustResp.Corporate;
                        objReg.ResponseDetails = CustResp.ResponseDetails;
                        objReg.PostFileDetails = CustResp.PostFileDetails;

                        #region convert date


                        if (objReg.CustomerDetails.DateOfBirth != string.Empty)
                        {
                            string strDOB = Utility.GetDateconvertion(objReg.CustomerDetails.DateOfBirth, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                            if (strDOB == "")
                            {
                                objReg.CustomerDetails.DateOfBirth = "";

                            }
                        }


                        if (objReg.CustomerDetails.IDValidateFullDate != string.Empty)
                        {
                            string strDOB = Utility.GetDateconvertion(objReg.CustomerDetails.IDValidateFullDate, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                            if (strDOB == "")
                            {
                                objReg.CustomerDetails.IDValidateFullDate = "";

                            }
                        }


                        if (objReg.CustomerDetails.IssueFullDate != string.Empty)
                        {
                            string strDOB = Utility.GetDateconvertion(objReg.CustomerDetails.IssueFullDate, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                            if (strDOB == "")
                            {
                                objReg.CustomerDetails.IssueFullDate = "";

                            }
                        }




                        #endregion

                        #region Delete file from path
                        //if (Server.MapPath("~/App_Data/UploadFile") != null)
                        //{
                        //    var filepath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            var filepath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                            if (Directory.Exists(filepath))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(filepath);
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                            }
                        }
                        if (clientSetting.mvnoSettings.internalPdfDownload != null)
                        {
                            var filepath2 = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                            if (Directory.Exists(filepath2))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(filepath2);
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                            }
                        }

                        #endregion

                        #region Change Uploaded File Path
                        string sourcePath = string.Empty;
                        string targetPath = string.Empty;
                        string[] GetFileName = new string[3];
                        if (objReg.PostFileDetails.Count() > 0)
                        {
                            for (int i = 0; i < objReg.PostFileDetails.Count(); i++)
                            {
                                if (objReg.PostFileDetails[i].Filepath != "")
                                {
                                    sourcePath = objReg.PostFileDetails[i].Filepath;
                                }
                                else
                                {
                                    sourcePath = clientSetting.postSettings.registrationDocpath;
                                }
                                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                                //{
                                //    targetPath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                                if (clientSetting.mvnoSettings.internalUploadFile != null)
                                {
                                    targetPath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                                    if (!Directory.Exists(targetPath))
                                    {
                                        Directory.CreateDirectory(targetPath);
                                    }
                                    if (System.IO.File.Exists(sourcePath + "\\" + objReg.PostFileDetails[i].Filename))
                                    {
                                        System.IO.File.Copy(sourcePath + "\\" + objReg.PostFileDetails[i].Filename, targetPath + "\\" + objReg.PostFileDetails[i].Filename, true);
                                    }
                                }
                                GetFileName[i] = objReg.PostFileDetails[i].Filename;
                            }
                            if (objReg.PostFileDetails != null)
                            {
                                for (int i = 0; i < objReg.PostFileDetails.Count(); i++)
                                {
                                    GetFileName[i] = objReg.PostFileDetails[i].Filename;
                                }
                                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                                //{

                                if (clientSetting.mvnoSettings.internalUploadFile != null)
                                {
                                    GetfileFTP(targetPath + "\\", GetFileName, CustReq, CustResp.ResponseDetails.ResponseDesc);
                                }
                            }

                        }

                        #endregion

                    }
                    else
                    {
                        objReg.ResponseCode = CustResp.ResponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetails.ResponseDesc;
                    }
                }
                else
                {
                    objReg = new Registration_AllPostpaid();
                    objReg.ResponseCode = "1";
                    objReg.ResponseDescription = "No Record";
                }

                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35", objReg.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "Tbl_Nationality")).ToList();

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objReg.ResponseDescription);
            }
            catch (Exception ex)
            {
                objReg = new Registration_AllPostpaid();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);

                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
            }
            return View("SubscriberRegistration_AllPostpaid", objReg);
        }

        public ActionResult SubscriberRegistrationView_AllPostpaid(string RegisterMsisdn)
        {
            CRM.Models.Registration_AllPostpaid objReg = new CRM.Models.Registration_AllPostpaid();
            CommonpostpaidGetRequest CustReq = new CommonpostpaidGetRequest();
            CommonpostpaidGetResponse CustResp = new CommonpostpaidGetResponse();
            try
            {
                #region LOAD CORPORATE

                CRMBase cbase = new CRMBase();
                cbase.CountryCode = clientSetting.countryCode;
                cbase.BrandCode = clientSetting.brandCode;
                cbase.LanguageCode = clientSetting.langCode;
                GetPlansfromPBS objres = crmNewService.CRMGetPlansfromPBS(cbase);
                objReg.drpdownBillCycle = objres.Billcycledatelist;
                objReg.drpdownWholesaler = objres.Wholesaleplanlist;
                TempData["Corporate"] = objres.Corporatelist;

                #endregion

                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35", objReg.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, objReg.IsPostpaid, "Tbl_Nationality")).ToList();
                objReg.drpcorporate = objres.Corporatelist;

                CustReq.CountryCode = clientSetting.countryCode;
                CustReq.BrandCode = clientSetting.brandCode;
                CustReq.LanguageCode = clientSetting.langCode;
                CustReq.MSISDN = Session["MobileNumber"].ToString();

                CustResp = crmNewService.CRMCommonpostpaidGetcustomer(CustReq);

                if (CustResp.ResponseDetails != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Edit_CommonPostpaid_" + CustResp.ResponseDetails.ResponseCode);
                    CustResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? CustResp.ResponseDetails.ResponseDesc : errorInsertMsg;

                    if (CustResp.ResponseDetails.ResponseCode == "0")
                    {
                        objReg.ResponseCode = CustResp.ResponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetails.ResponseDesc;
                        objReg.CustomerDetails = CustResp.CustomerDetails;
                        objReg.Corporate = CustResp.Corporate;
                        objReg.ResponseDetails = CustResp.ResponseDetails;
                        objReg.vasBundle = CustResp.vasBundle;
                        objReg.normalBundle = CustResp.normalBundle;
                        objReg.Handset = CustResp.Handset;




                        if (CustResp.CustomerDetails.CorporateID != "" && CustResp.CustomerDetails.CorporateID != null)
                        {
                            if (objres.Corporatelist.FindAll(b => b.ID == CustResp.CustomerDetails.CorporateID).Count() > 0)
                            {
                                CustResp.CustomerDetails.CorporateID = objres.Corporatelist.FindAll(b => b.ID == CustResp.CustomerDetails.CorporateID)[0].Text;
                            }
                        }
                        else
                        {
                            CustResp.CustomerDetails.CorporateID = "";
                        }


                        if (CustResp.CustomerDetails.SimType != "" && CustResp.CustomerDetails.SimType != null && CustResp.CustomerDetails.SimType != "0")
                        {

                            DropdownMaster dropdownALLPostpaid = objReg.strDropdown.FirstOrDefault(b => b.ID == CustResp.CustomerDetails.SimType && b.Master_id == "11");
                            CustResp.CustomerDetails.SimType = dropdownALLPostpaid.Value;
                        }
                        else
                        {
                            CustResp.CustomerDetails.SimType = "";
                        }

                        if (CustResp.CustomerDetails.BillCycleDate != "" && CustResp.CustomerDetails.BillCycleDate != null)
                        {
                            if (objReg.drpdownBillCycle.FindAll(b => b.ID == CustResp.CustomerDetails.BillCycleDate).Count() > 0)
                            {
                                CustResp.CustomerDetails.BillCycleDate = objReg.drpdownBillCycle.FindAll(b => b.ID == CustResp.CustomerDetails.BillCycleDate)[0].Text;
                            }
                        }
                        else
                        {
                            CustResp.CustomerDetails.BillCycleDate = "";
                        }

                        if (CustResp.CustomerDetails.NDDPreference != "" && CustResp.CustomerDetails.NDDPreference != null && CustResp.CustomerDetails.NDDPreference != "0")
                        {

                            DropdownMaster dropdownALLPostpaid = objReg.strDropdown.FirstOrDefault(b => b.ID == CustResp.CustomerDetails.NDDPreference && b.Master_id == "16");
                            CustResp.CustomerDetails.NDDPreference = dropdownALLPostpaid.Value;
                        }
                        else
                        {
                            CustResp.CustomerDetails.NDDPreference = "";
                        }

                        if (CustResp.CustomerDetails.Credit_Checkstatus != "" && CustResp.CustomerDetails.Credit_Checkstatus != null && CustResp.CustomerDetails.Credit_Checkstatus != "0")
                        {

                            DropdownMaster dropdownALLPostpaid = objReg.strDropdown.FirstOrDefault(b => b.ID == CustResp.CustomerDetails.Credit_Checkstatus && b.Master_id == "29");
                            CustResp.CustomerDetails.Credit_Checkstatus = dropdownALLPostpaid.Value;
                        }
                        else
                        {
                            CustResp.CustomerDetails.Credit_Checkstatus = "";
                        }



                        #region convert date

                        if (CustResp.CustomerDetails.DateOfBirth != null && CustResp.CustomerDetails.DateOfBirth != string.Empty)
                        {
                            string strDOB = Utility.GetDateconvertion(CustResp.CustomerDetails.DateOfBirth, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                            if (strDOB == "")
                            {
                                CustResp.CustomerDetails.DateOfBirth = "";

                            }
                        }



                        if (CustResp.CustomerDetails.IDValidateFullDate != null && CustResp.CustomerDetails.IDValidateFullDate != string.Empty)
                        {
                            string strDOB = Utility.GetDateconvertion(CustResp.CustomerDetails.IDValidateFullDate, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                            if (strDOB == "")
                            {
                                CustResp.CustomerDetails.IDValidateFullDate = "";

                            }
                        }


                        if (CustResp.CustomerDetails.IssueFullDate != null && CustResp.CustomerDetails.IssueFullDate != string.Empty)
                        {
                            string strDOB = Utility.GetDateconvertion(CustResp.CustomerDetails.IssueFullDate, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                            if (strDOB == "")
                            {
                                CustResp.CustomerDetails.IssueFullDate = "";

                            }
                        }
                        #endregion

                    }
                    else
                    {
                        objReg.ResponseCode = CustResp.ResponseDetails.ResponseCode;
                        objReg.ResponseDescription = CustResp.ResponseDetails.ResponseDesc;
                    }
                }
                else
                {
                    objReg = new Registration_AllPostpaid();
                    objReg.ResponseCode = "1";
                    objReg.ResponseDescription = "No Record";
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objReg.ResponseDescription);
            }
            catch (Exception ex)
            {
                objReg = new Registration_AllPostpaid();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDescription = ex.ToString();
            }
            return View(objReg);
        }

        [HttpPost]
        public JsonResult CRMValidateSubscriber_AllPostpaid(string Registration)
        {
            RegisterInsertPostpaidRequest objReg = JsonConvert.DeserializeObject<RegisterInsertPostpaidRequest>(Registration);
            RegisterInsertPostpaidResponse objRes = new RegisterInsertPostpaidResponse();
            try
            {
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                objRes = crmNewService.CRMCommonpostpaidRegister(objReg);

                if (objRes != null && objRes.ResponseDetails != null && objRes.ResponseDetails.ResponseCode != null)
                {

                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Reg_CommonPostpaid_" + objRes.ResponseDetails.ResponseCode);
                    objRes.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
            }
            catch (Exception ex)
            {
                objRes = new RegisterInsertPostpaidResponse();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.ResponseDetails = new CRMResponse();
                objRes.ResponseDetails.ResponseCode = "2";
                objRes.ResponseDetails.ResponseDesc = ex.ToString();
            }

            return Json(objRes);
        }


        public JsonResult GET_MSISDNNumber(string strMSISDN)
        {
            CommonpostpaidGetAvailMsisdnReq objReq = new CommonpostpaidGetAvailMsisdnReq();
            CommonpostpaidGetAvailMsisdnRes objResp = new CommonpostpaidGetAvailMsisdnRes();
            try
            {
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                objReq.MSISDN = strMSISDN;
                objResp = crmNewService.CRMCommonpostpaidGetAvailMsisdn(objReq);

                if (objResp != null && objResp.ResponseDetails != null && objResp.ResponseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Reg_CommonPostpaidGET_MSISDN_" + objResp.ResponseDetails.ResponseCode);
                    objResp.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objResp.ResponseDetails.ResponseDesc : errorInsertMsg;
                }

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objResp);
        }



        #endregion

        #endregion




        #region NLD Registration

        public JsonResult NLDRegistration(string Registration)
        {
            NLDRegistrationRes ObjRes = new NLDRegistrationRes();

            try
            {
                NLDRegistrationReq objReg = JsonConvert.DeserializeObject<NLDRegistrationReq>(Registration);
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                objReg.CSAgent = Session["UserName"].ToString();
                objReg.Language = clientSetting.langCode;

                ObjRes = crmService.InsertNLD(objReg);
                if (objReg.Mode == "INSERT")
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_NLD_" + ObjRes.ResponseCode);
                    ObjRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDesc : errorInsertMsg;

                }//for insertion
                else
                {
                    if (ObjRes != null && objReg.Mode.ToUpper() == "UPDATE")
                    {
                        Session["SubscriberTitle"] = objReg.Title;
                        Session["SubscriberName"] = objReg.FirstName + "|" + objReg.Lastname;
                        string dateformat = clientSetting.mvnoSettings.dateTimeFormat.Trim().ToUpper();
                        Session["DOB"] = dateformat.Replace("MM", objReg.Birthmm).Replace("DD", objReg.Birthdd).Replace("YYYY", objReg.Birthyy);
                    }

                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ERegupd_NLD_" + ObjRes.ResponseCode);
                    ObjRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDesc : errorInsertMsg;
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(ObjRes);
        }

        public JsonResult CRMGetSubscriberNLD(string NLDGet)
        {
            NLDGetSubscriberRes ObjRes = new NLDGetSubscriberRes();
            try
            {
                NLDGetSubscriberReq ObjReq = JsonConvert.DeserializeObject<NLDGetSubscriberReq>(NLDGet);
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode.ToString();
                ObjRes = crmService.CRMGetSubscriberNLD(ObjReq);

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(ObjRes);
        }
        public ActionResult SubscriberRegistrationView_NLD()
        {

            NLDRegistrationGetModel sampleobject = new NLDRegistrationGetModel();
            if (Convert.ToString(Session["isPrePaid"]) == "1")
            {//Prepaid Get NLD
                NLDGetSubscriberRes ObjRes = new NLDGetSubscriberRes();
                try
                {
                    NLDGetSubscriberReq ObjReq = new NLDGetSubscriberReq();
                    ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    ObjReq.CountryCode = clientSetting.countryCode;
                    ObjReq.BrandCode = clientSetting.brandCode;
                    ObjReq.LanguageCode = clientSetting.langCode.ToString();
                    ObjRes = crmService.CRMGetSubscriberNLD(ObjReq);
                    sampleobject.Prepaid = ObjRes;
                    if (clientSetting.brandCode.ToUpper().Trim() == clientSetting.preSettings.ToggleBrandCode.ToUpper().Trim())
                    {

                        return View("SubscriberView_NLD_TGL", sampleobject);
                    }

                }
                catch (Exception ex)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                }
                return View("SubscriberView_NLD", sampleobject);
            }
            else
            {//Postpaid NLD GET
                NLDpostpaidGetResponse ObjRes = new NLDpostpaidGetResponse();
                Registration_NLD NLDobjres = new Registration_NLD();
                //Registration_NLD ObjRes = new Registration_NLD();
                try
                {
                    NLDpostpaidGetRequest ObjReq = new NLDpostpaidGetRequest();
                    ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                    ObjReq.CountryCode = clientSetting.countryCode;
                    ObjReq.BrandCode = clientSetting.brandCode;
                    ObjReq.LanguageCode = clientSetting.langCode.ToString();
                    CRMBase cbase = new CRMBase();
                    cbase.CountryCode = clientSetting.countryCode;
                    cbase.BrandCode = clientSetting.brandCode;
                    cbase.LanguageCode = clientSetting.langCode;
                    GetPlansfromPBS objres = crmNewService.CRMGetPlansfromPBS(cbase);
                    NLDobjres.NLDdrpdownBillCycle = objres.Billcycledatelist;
                    List<DropdownMaster> objLstDropdown = new List<DropdownMaster>();
                    objLstDropdown = Utility.GetDropdownMasterFromDB("11", "2", "drop_master");

                    ObjRes = crmNewService.CRMpostpaidGetNLD(ObjReq);

                    if (ObjRes.CustomerDetails.SimType != "" && ObjRes.CustomerDetails.SimType != null && ObjRes.CustomerDetails.SimType != "0")
                    {
                        DropdownMaster dropdownALLPostpaid = objLstDropdown.FirstOrDefault(b => b.ID == ObjRes.CustomerDetails.SimType && b.Master_id == "11");
                        ObjRes.CustomerDetails.SimType = dropdownALLPostpaid.Value;

                    }
                    else
                    {
                        ObjRes.CustomerDetails.SimType = "";
                    }
                    if (ObjRes.CustomerDetails.BillCycleDate != "" && ObjRes.CustomerDetails.BillCycleDate != null)
                    {
                        if (NLDobjres.NLDdrpdownBillCycle.FindAll(b => b.ID == ObjRes.CustomerDetails.BillCycleDate).Count() > 0)
                        {
                            ObjRes.CustomerDetails.BillCycleDate = NLDobjres.NLDdrpdownBillCycle.FindAll(b => b.ID == ObjRes.CustomerDetails.BillCycleDate)[0].Text;
                        }
                    }
                    else
                    {
                        ObjRes.CustomerDetails.BillCycleDate = "";
                    }

                    sampleobject.Postpaid = ObjRes;
                }
                catch (Exception ex)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                }
                return View("SubscriberView_NLD", sampleobject);
            }


        }
        public JsonResult CRMGetSubscriberPPNLD(string MSISDN)
        {
            NLDpostpaidGetResponse ObjRes = new NLDpostpaidGetResponse();
            //Registration_NLD ObjRes = new Registration_NLD();
            try
            {
                NLDpostpaidGetRequest ObjReq = JsonConvert.DeserializeObject<NLDpostpaidGetRequest>(MSISDN);
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode.ToString();
                ObjRes = crmNewService.CRMpostpaidGetNLD(ObjReq);

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(ObjRes);
        }



        #endregion

        public JsonResult CRMRegisterSubscriberPoland(string POLRegistration)
        {
            PolandRegisterResponse ObjRes = new PolandRegisterResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                PolandMultiRegisterRequest objReg = JsonConvert.DeserializeObject<PolandMultiRegisterRequest>(POLRegistration);
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                


                    ObjRes = serviceCRM.CRMRegisterSubscriberPoland(objReg);
                
                if (ObjRes != null && objReg.Mode.ToUpper() == "UPDATE" && objReg.MobilePurpose.ToUpper() == "PERSONAL")
                {


                    Session["SubscriberName"] = objReg.FirstName + "|" + objReg.LastName;
                    string dateformat = clientSetting.mvnoSettings.dateTimeFormat.Trim().ToUpper();
                    Session["DOB"] = dateformat.Replace("MM", objReg.BirthMM).Replace("DD", objReg.BirthDD).Replace("YYYY", objReg.BirthYYYY);
                }

                foreach (var response in ObjRes.RegisterPOL)
                {

                    string Errordescription = response.ErrMsg;
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_POL_" + response.ErrNo);
                    response.ErrMsg = string.IsNullOrEmpty(errorInsertMsg) ? response.ErrMsg : errorInsertMsg;

                    if (response.ErrNo == "0" && clientSetting.mvnoSettings.sendEmail != "1" && clientSetting.mvnoSettings.sendSMS != "1")
                    {
                        response.ErrMsg = Resources.ErrorResources.EReg_POL_00;

                    }
                    else if (response.ErrNo == "0" && clientSetting.mvnoSettings.sendEmail == "1" && clientSetting.mvnoSettings.sendSMS != "1")
                    {
                        response.ErrMsg = Resources.ErrorResources.EReg_POL_01;
                    }
                    else if (response.ErrNo == "0" && clientSetting.mvnoSettings.sendEmail != "1" && clientSetting.mvnoSettings.sendSMS == "1")
                    {
                        response.ErrMsg = Resources.ErrorResources.EReg_POL_02;
                    }

                }


                if (objReg.Mode.ToUpper() == "UPDATE")
                {
                    if (ObjRes.reponseDetails.ResponseCode == "0")
                    {
                        ObjRes.reponseDetails.ResponseDesc = Resources.ErrorResources.ResourceManager.GetString("EReg_POLUpd_" + ObjRes.reponseDetails.ResponseCode);
                    }
                }
                else
                {//Registration case


                    if ((clientSetting.preSettings.polEnableFileUpload.ToUpper() == "TRUE" || clientSetting.preSettings.polEnableFileUpload.Trim() == "1") && (objReg.FileName != null && objReg.FileName != ""))
                    {

                        #region Change Uploaded File Path

                        string SourceDocpath = clientSetting.preSettings.registrationDocpath;
                        if (!Directory.Exists(SourceDocpath))
                        {
                            Directory.CreateDirectory(SourceDocpath);
                        }

                        string sourcePath = string.Empty;
                        string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                        //if (Server.MapPath("~/App_Data/UploadFile") != null)
                        //{
                        //    sourcePath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);

                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            sourcePath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);
                        }
                        string targetPath = clientSetting.preSettings.registrationDocpath;

                        string Externsion = string.Empty;
                        var x = ObjRes.RegisterPOL.Count;

                        foreach (var response in ObjRes.RegisterPOL)
                        {

                            string FileName = response.DocName;

                            if (response.ErrNo == "0" || response.ErrNo == "100" || response.ErrNo == "110" || response.ErrNo == "111" || response.ErrNo == "112" || response.ErrNo == "113" || response.ErrNo == "114" || response.ErrNo == "121" || response.ErrNo == "122" || response.ErrNo == "123")
                            {
                                #region Delete
                                for (int i = 0; i < 3; i++)
                                {
                                    string[] fileformat = clientSetting.mvnoSettings.ticketAttachFileformat.Split(',');
                                    for (int j = 0; j < fileformat.Length; j++)
                                    {
                                        fileformat[j] = fileformat[j].Replace(".", string.Empty).Trim();
                                        System.IO.File.Delete(targetPath + "\\" + FileName);
                                    }
                                }
                                #endregion

                                #region Move
                                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                                //{
                                if (clientSetting.mvnoSettings.internalUploadFile != null)
                                {

                                    if (Directory.GetFiles(sourcePath).Count() > 0)
                                    {
                                        string[] fileEntries2 = Directory.GetFiles(sourcePath);
                                        string[] filenameList = new string[3];
                                        string filename = string.Empty;
                                        string Deletefilename = string.Empty;
                                        for (int i = 0; i < fileEntries2.Count(); i++)
                                        {
                                            Externsion = fileEntries2[i].Substring(fileEntries2[i].LastIndexOf("\\") + 1);
                                            string[] splitExternsion = Externsion.Split('.');
                                            //filename = AccountNo + "_Doc" + (i + 1) + "." + splitExternsion[1];
                                            System.IO.File.Copy(sourcePath + "\\" + Externsion, targetPath + "\\" + FileName);
                                            filenameList[i] = filename.ToString();
                                            Deletefilename = FileName;
                                            if (clientSetting.preSettings.enableSFTP.ToUpper() == "FALSE" && clientSetting.mvnoSettings.enableFTPS.ToUpper() == "FALSE")
                                            {

                                                #region Create path

                                                string TargetDocpath = clientSetting.mvnoSettings.sftpFilePath;
                                                if (!Directory.Exists(TargetDocpath))
                                                {
                                                    Directory.CreateDirectory(TargetDocpath);
                                                }

                                                #endregion
                                                System.IO.File.Copy(clientSetting.preSettings.registrationDocpath + "\\" + FileName, clientSetting.mvnoSettings.sftpFilePath + "\\" + FileName, true);
                                            }
                                            if (clientSetting.mvnoSettings.enableFTPS.ToUpper() == "TRUE")
                                            {
                                                FileUploadFTPSServer(targetPath + "\\", FileName, objReg);
                                            }

                                        }

                                        if (clientSetting.preSettings.enableSFTP.ToUpper() == "TRUE" && clientSetting.mvnoSettings.enableFTPS.ToUpper() == "FALSE")
                                        {
                                            MovefileFTP(targetPath + "\\", filenameList, Deletefilename, objReg, "");
                                            //FileUploadFTPSServer(targetPath + "\\", filenameList, objReg);
                                        }


                                    }

                                    System.IO.File.Delete(clientSetting.preSettings.registrationDocpath + "\\" + FileName);
                                }
                                #endregion
                            }
                        }
                        System.IO.File.Delete(sourcePath + "\\" + Externsion);
                        if (clientSetting.mvnoSettings.internalPdfDownload != null)
                        {
                            string delPath = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                            if (Directory.Exists(delPath))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(delPath);
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                                //   Directory.Delete(delPath, true);
                            }
                        }
                        Session["FilePath"] = string.Empty;

                        #endregion
                    }

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
            return Json(ObjRes);
        }

        public JsonResult CRMGetSubscriberPOL(string POLGet)
        {
            POLGetSubscriberRes ObjRes = new POLGetSubscriberRes();
            try
            {
                POLGetSubscriberReq ObjReq = JsonConvert.DeserializeObject<POLGetSubscriberReq>(POLGet);
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode.ToString();
                ObjRes = crmService.CRMGetSubscriberPOL(ObjReq);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(ObjRes);
        }

        #region FRR 4858

  

        public JsonResult SimRegwithFamilyPlanUSA(string Registration)
        {
            SimRegwithFamilyPlanUSAResponse ObjRes = new SimRegwithFamilyPlanUSAResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                SimRegwithFamilyPlanUSARequest objReg = JsonConvert.DeserializeObject<SimRegwithFamilyPlanUSARequest>(Registration);
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.SimRegwithFamilyPlanUSA(objReg);
                

                if (objReg.Mode == "Reg")
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SimRegwithFamilyPlan_USA_" + ObjRes.responseDetails.ResponseDesc);
                    ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;
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
            return Json(ObjRes);
        }



        #endregion
        public JsonResult CRMPolandValidation(string PolanValidatelist)
        {
            PolandValidateResponse ObjRes = new PolandValidateResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                PolanValidateRequest ObjReq = JsonConvert.DeserializeObject<PolanValidateRequest>(PolanValidatelist);
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode.ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMPolandValidation(ObjReq);


                

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return Json(ObjRes);
        }
        public JsonResult CRMPolandOTP(string PolandOTP)
        {
            PolandOTPRes ObjRes = new PolandOTPRes();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                PolandOTPReq ObjReq = JsonConvert.DeserializeObject<PolandOTPReq>(PolandOTP);
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode.ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMPolandOTP(ObjReq);

                    //work

                    //ObjReq.Mode!="OTP"
                    if (ObjRes.reponseDetails != null)
                    {
                        string Errordescription = ObjRes.reponseDetails.ResponseDesc;
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString(ObjReq.Mode != "OTP" ? "POL_LINK_" : "POL_OTP_" + ObjRes.reponseDetails.ResponseCode);
                        ObjRes.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.reponseDetails.ResponseDesc : errorInsertMsg;
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
            return Json(ObjRes);
        }
        #region SWEDEN Registration


        public ActionResult SubscriberRegistration_SWE(string RegisterMsisdn)
        {
            Registration_SWE SWEobjres = new Registration_SWE();
            try
            {
                SWEobjres.CountryCode = clientSetting.mvnoSettings.countryCode;
                SWEobjres.Mode = "Insert";
                SWEobjres.Msisdn = RegisterMsisdn;
                // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                Session["PAType"] = null;
                SWEobjres.SWEdrpdown = Utility.GetDropdownMasterFromDB("1,2,4,10", "1", "drop_master");
                ViewBag.Title = "Subscriber Registration";
            }
            catch (Exception ex)
            {
                SWEobjres.SWEdrpdown = Utility.GetDropdownMasterFromDB("1,2,4,10", "1", "drop_master");
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View("SubscriberRegistration_SWE", SWEobjres);
        }
        public ActionResult SubscriberRegistrationEdit_SWE(string RegisterMsisdn)
        {
            Registration_SWE SWEobjres = new Registration_SWE();
            SWEGetSubscriberResponse ObjRes = new SWEGetSubscriberResponse();
            SWEGetSubscriberRequest ObjReq = new SWEGetSubscriberRequest();
            ObjReq.MSISDN = RegisterMsisdn;
            ObjReq.CountryCode = clientSetting.countryCode;
            ObjReq.BrandCode = clientSetting.brandCode;
            ObjReq.LanguageCode = clientSetting.langCode.ToString();
            //s
            ServiceInvokeCRM serviceCRM;

            try
            {

                ViewBag.Title = "Subscriber Edit";
                SWEobjres.SWEdrpdown = Utility.GetDropdownMasterFromDB("1,2,4,10", "1", "drop_master");
                SWEobjres.CountryCode = clientSetting.mvnoSettings.countryCode;
                SWEobjres.Mode = "Edit";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMGetSubscriberSWE(ObjReq);

                
                SWEobjres.SWERegisterSubscriber = ObjRes.SWEGetSubscriber;
                SWEobjres.reponseDetails = ObjRes.reponseDetails;
                //SWEobjres.FirstName = ObjRes.SWEGetSubscriber.FirstName;
                //SWEobjres.Gender = ObjRes.SWEGetSubscriber.Gender;
                //SWEobjres.AccountNumber = ObjRes.SWEGetSubscriber.AccountNumber;
                //SWEobjres.apartmentNo = ObjRes.SWEGetSubscriber.apartmentNo;
                //SWEobjres.

                // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                Session["PAType"] = null;
            }
            catch (Exception ex)
            {
                SWEobjres.SWEdrpdown = Utility.GetDropdownMasterFromDB("1,2,4,10", "1", "drop_master");
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return View("SubscriberRegistration_SWE", SWEobjres);
        }
        public ActionResult SubscriberRegistrationView_SWE()
        {
            SWEGetSubscriberResponse ObjRes = new SWEGetSubscriberResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                SWEGetSubscriberRequest ObjReq = new SWEGetSubscriberRequest();
                ObjReq.MSISDN = Convert.ToString(Session["MobileNumber"]);
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode.ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMGetSubscriberSWE(ObjReq);
                


            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return View("SubscriberRegistrationView_SWE", ObjRes);
        }


        public JsonResult SWERegistration(string Registration)
        {
            ServiceCRM.CRMResponse ObjRes = new ServiceCRM.CRMResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                SWERegisterSubscriber objReg = JsonConvert.DeserializeObject<SWERegisterSubscriber>(Registration);
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                objReg.CSAgent = Session["UserName"].ToString();
                objReg.Language = clientSetting.langCode;

                //  ObjRes = crmService.InsertSWE(objReg);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMRegisterSubscriberSWE(objReg);
                


                if (objReg.Mode == "INSERT")
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_SWE_" + ObjRes.ResponseCode);
                    ObjRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDesc : errorInsertMsg;

                }//for insertion
                else
                {
                    if (ObjRes != null && objReg.Mode.ToUpper() == "UPDATE")
                    {
                        Session["SubscriberTitle"] = objReg.Title;
                        Session["SubscriberName"] = objReg.FirstName + "|" + objReg.LastName;
                        string dateformat = clientSetting.mvnoSettings.dateTimeFormat.Trim().ToUpper();
                        Session["DOB"] = dateformat.Replace("MM", objReg.Birthmm).Replace("DD", objReg.Birthdd).Replace("YYYY", objReg.Birthyy);
                    }

                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ERegupd_SWE_" + ObjRes.ResponseCode);
                    ObjRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDesc : errorInsertMsg;
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
            return Json(ObjRes);
        }


        public JsonResult CRMGetSubscriberSWE(string SWEGet)
        {
            SWEGetSubscriberResponse ObjRes = new SWEGetSubscriberResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                SWEGetSubscriberRequest ObjReq = JsonConvert.DeserializeObject<SWEGetSubscriberRequest>(SWEGet);
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode.ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMGetSubscriberSWE(ObjReq);
                


            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return Json(ObjRes);
        }
        #endregion

        #region Australia
        public JsonResult LoadCityFromAccess(string StateCode)
        {
            List<DropdownMaster> objLstDropdown = new List<DropdownMaster>();
            try
            {
                objLstDropdown = Utility.GetDropdownMasterFromDB(StateCode, "1", "tbl_city");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objLstDropdown);
        }

        public JsonResult LoadStateFromAccess()
        {
            List<DropdownMaster> objLstDropdown = new List<DropdownMaster>();
            try
            {

                objLstDropdown = Utility.GetDropdownMasterFromDB(string.Empty, "1", "tbl_state");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objLstDropdown);
        }

        public List<CRM.Models.Dropdown> AUSBindDropDownIDType()
        {
            List<Dropdown> objLstDropdown = new List<Dropdown>();
            string filename = Server.MapPath("~/App_Data/AUS/AUSDocumentIDType_" + clientSetting.langCode + ".xml");
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlnode;
            string IDtypeText = string.Empty;
            string IDTypeValue = string.Empty;
            try
            {
                xmldoc.LoadXml(System.IO.File.ReadAllText(filename));
                xmlnode = xmldoc.SelectNodes("/IDDOCUMENTTYPES/IDTYPE");
                foreach (XmlNode node in xmlnode)
                {
                    if (node["UIDISPLAY"].InnerText.ToString().ToUpper().Equals("TRUE"))
                    {
                        IDtypeText = IDtypeText + "|" + node["IDNAME"].InnerText.ToString();
                        IDTypeValue = IDTypeValue + "|" + node["ID"].InnerText.ToString();
                    }
                }

                string[] strENID = IDTypeValue.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                string[] strOtherID = IDtypeText.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                Dropdown objDrop = new Dropdown();
                for (int iTitCount = 0; iTitCount <= strOtherID.Length - 1; iTitCount++)
                {
                    objDrop = new Dropdown();
                    objDrop.ID = strENID[iTitCount].ToString();
                    objDrop.Value = strOtherID[iTitCount].ToString();
                    objDrop.Master_id = "1";
                    objLstDropdown.Add(objDrop);
                }


                string filePath = Server.MapPath("~/App_Data/AUS/AUSPassportCountrycodeStateCardtype_" + clientSetting.langCode + ".xml");
                DataSet ds = new DataSet();
                ds.ReadXml(filePath);
                DataTable dt = ds.Tables["COUNTRY"];
                foreach (DataRow row in dt.Rows)
                {
                    objDrop = new Dropdown();
                    objDrop.ID = row["CODE"].ToString();
                    objDrop.Value = row["NAME"].ToString();
                    objDrop.Master_id = "2";
                    objLstDropdown.Add(objDrop);
                }
                dt = new DataTable();
                dt = ds.Tables["STATE"];
                foreach (DataRow row in dt.Rows)
                {
                    objDrop = new Dropdown();
                    objDrop.ID = row["STATE_Text"].ToString();
                    objDrop.Value = row["STATE_Text"].ToString();
                    objDrop.Master_id = "3";
                    objLstDropdown.Add(objDrop);
                }
                dt = new DataTable();
                dt = ds.Tables["CARD"];
                foreach (DataRow row in dt.Rows)
                {
                    objDrop = new Dropdown();
                    objDrop.ID = row["TYPE"].ToString();
                    objDrop.Value = row["NAME"].ToString();
                    objDrop.Master_id = "4";
                    objLstDropdown.Add(objDrop);
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return objLstDropdown;
        }

        public JsonResult SaveResgistration_AUS(string Registration)
        {
            CRMResponse objRes = new CRMResponse();
            try
            {
                AUSRegisterSubscriberReq objReg = JsonConvert.DeserializeObject<AUSRegisterSubscriberReq>(Registration);
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                objReg.UserName = Session["UserName"].ToString();
                objReg.RegisterBy = Session["UserName"].ToString();
                StringBuilder sb = new StringBuilder();
                objRes = crmService.InsertAustralia(objReg);
                string[] errorVer = null;
                if ((!string.IsNullOrEmpty(objRes.ResponseDesc) ? objRes.ResponseDesc.Split('|').Count() : 0) > 1)
                    errorVer = objRes.ResponseDesc.Split('|');
                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_AUS_" + objRes.ResponseCode);
                objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;
                if (objReg.Mode.ToUpper() == "UPDATE" && objRes.ResponseCode == "0")
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_AUS_00");
                    objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;

                }
                if (objRes.ResponseCode == "11")
                {
                    Session["SIMSTATUS"] = "1";
                }
                else if (objRes.ResponseCode == "24" || objRes.ResponseCode == "120" || objRes.ResponseCode == "121" || objRes.ResponseCode == "122")
                {
                    Session["SIMSTATUS"] = "0";
                }
                if (errorVer != null && errorVer.Count() > 1)
                {
                    objRes.ResponseDesc = objRes.ResponseDesc + "|" + errorVer[1] + "|" + (errorVer.Count() > 2 ? errorVer[2] : "");
                }
                if (objReg.Mode.ToUpper() == "UPDATE" && (objRes.ResponseCode == "0" || objRes.ResponseCode == "100" || objRes.ResponseCode == "110" || objRes.ResponseCode == "111" || objRes.ResponseCode == "112"))
                {
                    Session["SubscriberTitle"] = objReg.Title;
                    Session["SubscriberName"] = objReg.FirstName + "|" + objReg.LastName;
                    string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                    Datestring = Datestring.Replace("DD", objReg.BirthDD).Replace("MM", objReg.BirthMM).Replace("YYYY", objReg.BirthYYYY);
                    Session["DOB"] = Datestring;
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objRes);
        }

        public List<SPDETAILS> CRMMNPServiceProvider()
        {
            CRMBase objBase = new CRMBase();
            ServiceProviderResponse objRes = new ServiceProviderResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objBase.CountryCode = clientSetting.countryCode;
                objBase.BrandCode = clientSetting.brandCode;
                objBase.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.CRMMNPServiceProvider(objBase);
                
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return objRes.ServiceProvider;

        }

        public ActionResult SubscriberRegistrationEdit_AUS(string RegisterMSISDN)
        {
            AUSRegisterSubscriberModelReq objRegistration = new AUSRegisterSubscriberModelReq();
            try
            {
                RegisterMSISDN = Session["MobileNumber"].ToString();
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMSISDN);
                string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                AUSSubscriberReq objReq = new AUSSubscriberReq();
                objReq.MSISDN = RegisterMSISDN;
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode.ToString();
                AUSGetSubscriberRes objCRMRes = new AUSGetSubscriberRes();
                objCRMRes = crmService.GetAustralia(objReq);
                XmlDocument XMLSer = Common.CreateXML(objCRMRes.AUSGetSubscriber);
                XMLSer.InnerXml = XMLSer.InnerXml.Replace("AUSRegisterSubscriber", "AUSRegisterSubscriberModelReq");
                objRegistration = (CRM.Models.AUSRegisterSubscriberModelReq)Common.Deserialize(XMLSer.InnerXml, objRegistration);
                objRegistration.CountryDateFormat = Datestring;
                objRegistration.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,2,4,51", objRegistration.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "TblCountry")).ToList();
                objRegistration.City = Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "tbl_state");
                string contactNo = string.IsNullOrEmpty(objRegistration.ContactNumber) ? "1" : "2";
                objRegistration.ContactNumber = !string.IsNullOrEmpty(objRegistration.MobileNo) ? objRegistration.MobileNo : objRegistration.ContactNumber;
                objRegistration.MobileNo = RegisterMSISDN != null ? RegisterMSISDN : string.Empty;
                objRegistration.DateCalender = Datestring.ToLower().Replace("yyyy", "yy");
                objRegistration.GovernID = AUSBindDropDownIDType();
                objRegistration.SPDETAILS = CRMMNPServiceProvider();
                objRegistration.Mode = "UPDATE";
                objRegistration.Others = objRegistration.Title + "," + objRegistration.Gender + "," + objRegistration.State + "," + objRegistration.Cityname + "," + objRegistration.StreetType + "," + objRegistration.Docid + "," + objRegistration.PassportCountryCode + "," + objRegistration.PrefLanguage + "," + objRegistration.StateofIssue + "," + objRegistration.DOCMedicareLine2Name + "," + objRegistration.Country + "," + objRegistration.CardType + "," + objRegistration.ChkTerms + "," + objRegistration.DirectoryServices + "," + objRegistration.SMSMARK + "," + objRegistration.ISGAFVerified + "," + objRegistration.Docgender + "," + contactNo + "," + objRegistration.RetailerID;
                objRegistration.PageTitle = Resources.RegistrationResources.lblEditSubscriber;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View("SubscriberRegistration_AUS", objRegistration);
        }

        public ActionResult SubscriberRegistrationView_AUS(string MSISDN)
        {
            ViewBag.CountryCode = clientSetting.countryCode;
            return View();
        }

        public JsonResult GetSubscriberRegistration_AUS(string Registration)
        {
            Service.AUSGetSubscriberResp objRes = new Service.AUSGetSubscriberResp();
            try
            {
                Service.AUSGetSubscriberReq objReg = JsonConvert.DeserializeObject<Service.AUSGetSubscriberReq>(Registration);
                objReg.MSISDN = Session["MobileNumber"].ToString();
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                objRes = crmService.GetAUSRegistration(objReg);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRes.reponseDetails);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objRes);
        }

        public JsonResult RedirectDirection(string Registration)
        {
            TopupRequestNoResponse ObjRes = new TopupRequestNoResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                TopupRequestNoRequest objTopupNoreq = new TopupRequestNoRequest();
                AUSRegisterSubscriberModelReq objReg = JsonConvert.DeserializeObject<AUSRegisterSubscriberModelReq>(Registration);
                Session["objRegAUS"] = objReg;
                objTopupNoreq.BrandCode = clientSetting.brandCode;
                objTopupNoreq.CountryCode = clientSetting.countryCode;
                objTopupNoreq.LanguageCode = clientSetting.langCode;
                objTopupNoreq.Requestid = "CCV";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMGetTopupRequestNumber(objTopupNoreq);
                
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return Json(ObjRes);
        }

        public ActionResult SubscriberRegistration_AUS(string RegisterMsisdn, string PaymentResponse)
        {
            AUSRegisterSubscriberModelReq objRegistration = new AUSRegisterSubscriberModelReq();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
                objRegistration.MobileNo = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                if (!string.IsNullOrEmpty(PaymentResponse))
                {
                    string Eshopurl = PaymentResponse;
                    EncodeDecode objReq = new EncodeDecode();
                    objReq.returnUrl = PaymentResponse;
                    objReq.CountryCode = clientSetting.countryCode;
                    objReq.BrandCode = clientSetting.brandCode;
                    objReq.LanguageCode = clientSetting.langCode;
                    EncodeDecoderes objRes = new EncodeDecoderes();
                    string ErrorCode = string.Empty;
                    string ErrorMsg = string.Empty;
                    string TransactioID = string.Empty;
                    if (Session["objRegAUS"] != null)
                        objRegistration = (CRM.Models.AUSRegisterSubscriberModelReq)Session["objRegAUS"];
                    if (PaymentResponse != "CANCELREQUEST")
                    {
                        objRes = crmNewService.GetDecodeValue(objReq);
                        ErrorCode = objRes.hashval.FindAll(a => a.ID == "ResponseCode").Count > 0 ? objRes.hashval.FindAll(a => a.ID == "ResponseCode")[0].Values : objRes.responseDetails.ResponseCode;
                        ErrorMsg = objRes.hashval.FindAll(a => a.ID == "ResponseMessage").Count > 0 ? objRes.hashval.FindAll(a => a.ID == "ResponseMessage")[0].Values : objRes.responseDetails.ResponseDesc;
                        TransactioID = objRes.hashval.FindAll(a => a.ID == "TransactionID").Count > 0 ? objRes.hashval.FindAll(a => a.ID == "TransactionID")[0].Values : string.Empty;
                        objRegistration.objITAPlanListResponse = new ITAPlanListResponse();
                        objRegistration.objITAPlanListResponse.reponseDetails = new CRMResponse();
                        if (ErrorCode != "0")
                        {
                            objRegistration.objITAPlanListResponse.reponseDetails.ResponseCode = ErrorCode;
                            objRegistration.objITAPlanListResponse.reponseDetails.ResponseDesc = ErrorMsg;
                        }
                    }
                    else
                    {
                        objRegistration.objITAPlanListResponse = new ITAPlanListResponse();
                        objRegistration.objITAPlanListResponse.reponseDetails = new CRMResponse();
                        ErrorCode = "0";
                        objRegistration.objITAPlanListResponse.reponseDetails.ResponseCode = "0";
                        objRegistration.objITAPlanListResponse.reponseDetails.ResponseDesc = "";
                        objRegistration.Docid = Resources.RegistrationResources.Select;
                    }
                    objRegistration.Others = objRegistration.Title + "," + objRegistration.Gender + "," + objRegistration.State + "," + objRegistration.Cityname + "," + objRegistration.StreetType + "," + (ErrorCode == "0" ? objRegistration.Docid : "0") + "," + objRegistration.PassportCountryCode + "," + objRegistration.PrefLanguage + "," + objRegistration.StateofIssue + "," + objRegistration.DOCMedicareLine2Name + "," + objRegistration.Country + "," + objRegistration.CardType + "," + objRegistration.ChkTerms + "," + objRegistration.DirectoryServices + "," + objRegistration.SMSMARK + "," + objRegistration.ISGAFVerified + "," + TransactioID + "," + (string.IsNullOrEmpty(objRegistration.ContactNumber) ? "2" : "1");
                    objRegistration.ContactNumber = string.IsNullOrEmpty(objRegistration.ContactNumber) ? objRegistration.MobileNo : objRegistration.ContactNumber;
                    objRegistration.MobileNo = objRegistration.Msisdn;

                }
                else
                {
                    Session["objRegAUS"] = null;
                }

                string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                objRegistration.CountryDateFormat = Datestring;
                objRegistration.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objRegistration.City = Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "tbl_state");
                objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,2,4,51", objRegistration.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "TblCountry")).ToList();
                objRegistration.DateCalender = Datestring.ToLower().Replace("yyyy", "yy");
                objRegistration.GovernID = AUSBindDropDownIDType();
                objRegistration.SPDETAILS = CRMMNPServiceProvider();
                objRegistration.PageTitle = Resources.RegistrationResources.lblRegistrationSubscriber;
                objRegistration.Mode = "INSERT";
                string filepath = clientSetting.preSettings.registrationDocpath;
                if (Directory.Exists(filepath))
                {
                    Directory.Delete(filepath, true);
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objRegistration);
        }

        public ActionResult SubscriberRegistration_AUS_Payment(string PaymentResponse)
        {
            AUSRegisterSubscriberModelReq objRegistration = new AUSRegisterSubscriberModelReq();
            try
            {
                string Eshopurl = PaymentResponse;
                EncodeDecode objReq = new EncodeDecode();
                objReq.returnUrl = PaymentResponse;
                objReq.CountryCode = clientSetting.countryCode;
                objReq.BrandCode = clientSetting.brandCode;
                objReq.LanguageCode = clientSetting.langCode;
                EncodeDecoderes objRes = new EncodeDecoderes();
                string ErrorCode = string.Empty;
                string ErrorMsg = string.Empty;
                string TransactioID = string.Empty;
                if (Session["objRegAUS"] != null)
                    objRegistration = (CRM.Models.AUSRegisterSubscriberModelReq)Session["objRegAUS"];
                if (PaymentResponse != "CANCELREQUEST")
                {
                    objRes = crmNewService.GetDecodeValue(objReq);
                    ErrorCode = objRes.hashval.FindAll(a => a.ID == "ResponseCode").Count > 0 ? objRes.hashval.FindAll(a => a.ID == "ResponseCode")[0].Values : objRes.responseDetails.ResponseCode;
                    ErrorMsg = objRes.hashval.FindAll(a => a.ID == "ResponseMessage").Count > 0 ? objRes.hashval.FindAll(a => a.ID == "ResponseMessage")[0].Values : objRes.responseDetails.ResponseDesc;
                    TransactioID = objRes.hashval.FindAll(a => a.ID == "TransId").Count > 0 ? objRes.hashval.FindAll(a => a.ID == "TransId")[0].Values : string.Empty;
                    objRegistration.objITAPlanListResponse = new ITAPlanListResponse();
                    objRegistration.objITAPlanListResponse.reponseDetails = new CRMResponse();
                    if (ErrorCode != "0")
                    {
                        objRegistration.objITAPlanListResponse.reponseDetails.ResponseCode = ErrorCode;
                        objRegistration.objITAPlanListResponse.reponseDetails.ResponseDesc = ErrorMsg;
                    }
                }
                else
                {
                    objRegistration.objITAPlanListResponse = new ITAPlanListResponse();
                    objRegistration.objITAPlanListResponse.reponseDetails = new CRMResponse();
                    ErrorCode = "0";
                    objRegistration.objITAPlanListResponse.reponseDetails.ResponseCode = "0";
                    objRegistration.objITAPlanListResponse.reponseDetails.ResponseDesc = "";
                    objRegistration.Docid = Resources.RegistrationResources.Select;
                }
                objRegistration.Others = objRegistration.Title + "," + objRegistration.Gender + "," + objRegistration.State + "," + objRegistration.Cityname + "," + objRegistration.StreetType + "," + (ErrorCode == "0" ? objRegistration.Docid : "0") + "," + objRegistration.PassportCountryCode + "," + objRegistration.PrefLanguage + "," + objRegistration.StateofIssue + "," + objRegistration.DOCMedicareLine2Name + "," + objRegistration.Country + "," + objRegistration.CardType + "," + objRegistration.ChkTerms + "," + objRegistration.DirectoryServices + "," + objRegistration.SMSMARK + "," + objRegistration.ISGAFVerified + "," + TransactioID + "," + ((string.IsNullOrEmpty(objRegistration.ContactNumber) ? "2" : "1") + "," + objRegistration.objITAPlanListResponse.reponseDetails.ResponseDesc);
                objRegistration.ContactNumber = string.IsNullOrEmpty(objRegistration.ContactNumber) ? objRegistration.MobileNo : objRegistration.ContactNumber;
                objRegistration.MobileNo = objRegistration.Msisdn;
                string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                objRegistration.CountryDateFormat = Datestring;
                objRegistration.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objRegistration.City = Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "tbl_state");
                objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,2,4,51", objRegistration.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "TblCountry")).ToList();
                objRegistration.DateCalender = Datestring.ToLower().Replace("yyyy", "yy");
                objRegistration.GovernID = AUSBindDropDownIDType();
                objRegistration.SPDETAILS = CRMMNPServiceProvider();
                objRegistration.PageTitle = Resources.RegistrationResources.lblRegistrationSubscriber;
                // objRegistration.Mode = "INSERT";
                string filepath = clientSetting.preSettings.registrationDocpath;
                if (Directory.Exists(filepath))
                {
                    Directory.Delete(filepath, true);
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objRegistration);
        }

        #endregion

        #region Check For Valid Email ID's

        public JsonResult CheckforRestrictedMailIDs(string EmailID)
        {
            CRMResponse objRes = new CRMResponse();
            try
            {
                if (clientSetting.preSettings.restrictedMailIdCheck.ToUpper().Equals("TRUE"))
                {
                    string Restricted_MailIDs = clientSetting.preSettings.restrictedMailIds.ToLower();
                    string[] Split_Restricted_MailIDs = Restricted_MailIDs.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (Array.IndexOf(Split_Restricted_MailIDs, EmailID.ToLower()) == -1)
                    {
                        objRes.ResponseCode = "0";
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_REmailID0;
                    }
                    else
                    {
                        objRes.ResponseCode = "1";
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_REmailID1;
                    }
                }
                else
                {
                    objRes.ResponseCode = "0";
                    objRes.ResponseDesc = Resources.ErrorResources.EReg_REmailID0;
                }
            }
            catch (Exception ex)
            {
                objRes.ResponseCode = "0";
                objRes.ResponseDesc = Resources.ErrorResources.EReg_REmailID0;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objRes);
        }

        #endregion

        #region Belgium  Registration
        //public ActionResult SubscriberRegistration_BEL(string RegisterMSISDN, string Mode)
        //{
        //    Models.Registration_BEL objReg = new Models.Registration_BEL();
        //    try
        //    {
        //        CRMLogger.WriteMessage(Session["UserName"].ToString(), this.ControllerContext, RegisterMSISDN);
        //        objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
        //        objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,2,4,10", objReg.IsPostpaid, "drop_master");

        //        ///insert
        //        objReg.CountryDateFormat = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
        //        #region BELRegister Model binding
        //        BELRegisterSubscriber BELRegSubc = new BELRegisterSubscriber();
        //        BELRegSubc.Msisdn = RegisterMSISDN != null ? RegisterMSISDN : string.Empty;
        //        objReg.BELRegSubscriber = BELRegSubc;
        //        #endregion
        //        objReg.Mode = "INSERT";

        //    }
        //    catch (Exception ex) { CRMLogger.WriteException(Session["UserName"].ToString(), this.ControllerContext, ex); }
        //    return View(objReg);
        //}

        public ActionResult SubscriberRegistrationEdit_BEL(string RegisterMSISDN, string Mode)
        {
            Models.Registration_BEL objReg = new Models.Registration_BEL();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMSISDN);
                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                List<Dropdown> objLstDropdown = new List<Dropdown>();
                //objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,2,4,10", objReg.IsPostpaid, "drop_master");
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,5,6,7,8,10,67", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, "0", "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, "0", "Tbl_Nationality")).ToList();
                ///update
                objReg.CountryDateFormat = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                #region BELRegister Model binding
                BELRegisterSubscriber BELRegSubc = new BELRegisterSubscriber();
                BELRegSubc.Msisdn = RegisterMSISDN != null ? RegisterMSISDN : string.Empty;
                objReg.BELRegSubscriber = BELRegSubc;
                #endregion

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    #region View Belgium Registration
                    CustomerDetailsBELRequest custObjReq = new CustomerDetailsBELRequest();
                    CustomerDetailBELResponse custObjResp = new CustomerDetailBELResponse();
                    custObjReq.BrandCode = clientSetting.brandCode;
                    custObjReq.CountryCode = clientSetting.countryCode;
                    custObjReq.LanguageCode = clientSetting.langCode;
                    custObjReq.MSISDN = RegisterMSISDN;
                    custObjResp = serviceCRM.CRMGetSubscriberBEL(custObjReq);
                    #endregion
                    objReg.CountryName = clientSetting.mvnoSettings.countryName.Trim();
                    objReg.BELRegSubscriber = custObjResp.registerDetailsBEL;
                    objReg.responseDetails = custObjResp.responseDetails;
                    if (!string.IsNullOrEmpty(Mode))
                    {
                        objReg.Mode = Mode.Trim().ToUpper();
                    }
                
            }
            catch (Exception ex)
            {
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,5,6,7,8,10,67", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, "0", "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, "0", "Tbl_Nationality")).ToList();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return View("SubscriberRegistration_BEL", objReg);
        }

        public JsonResult SaveRegistration_BEL(string Registration)
        {
            CRMResponse objRes = new CRMResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                BELRegisterSubscriber objBELReg = JsonConvert.DeserializeObject<BELRegisterSubscriber>(Registration);
                objBELReg.CountryCode = clientSetting.countryCode;
                objBELReg.BrandCode = clientSetting.brandCode;
                objBELReg.LanguageCode = clientSetting.langCode;
                objBELReg.CSAgent = Convert.ToString(Session["UserName"]);

                if (objBELReg.DOB != string.Empty)
                {
                    string DOB = Utility.GetDateconvertion(objBELReg.DOB, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    string[] DOBSplit = DOB.Split('/');
                    objBELReg.BirthDD = DOBSplit[0];
                    objBELReg.BirthMM = DOBSplit[1];
                    objBELReg.BirthYYYY = DOBSplit[2];
                }
                else
                {
                    objBELReg.BirthDD = string.Empty;
                    objBELReg.BirthMM = string.Empty;
                    objBELReg.BirthYYYY = string.Empty;
                }
                if (objBELReg.Mode == "INSERT")
                {
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        objRes = serviceCRM.CRMRegisterSubscriberBEL(objBELReg);
                    
                }
                else if (objBELReg.Mode == "UPDATE")
                {
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        objRes = serviceCRM.CRMRegisterSubscriberBEL(objBELReg);
                        if (objRes.ResponseCode == "0")
                        {
                            objRes.ResponseCode = "00";
                        }
                    
                }
                if (objRes != null && objRes.ResponseCode != null)
                {
                    if (!string.IsNullOrEmpty(objRes.ResponseDesc))
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_BEL_" + objRes.ResponseCode);
                        objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;
                    }
                    else
                    {
                        objRes.ResponseDesc = Resources.ErrorResources.Common_2;
                    }
                    if (objRes.ResponseCode == "00" && objBELReg.Mode == "UPDATE")
                    {
                        Session["SubscriberTitle"] = objBELReg.Title;
                        Session["SubscriberName"] = objBELReg.FirstName + "|" + objBELReg.LastName;
                        Session["DOB"] = objBELReg.DOB;
                    }
                }
                else
                {
                    objRes.ResponseDesc = Resources.ErrorResources.Common_2;
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.ResponseCode = "2000";
                objRes.ResponseDesc = ex.ToString();
            }
            finally
            {
                serviceCRM = null;
            }
            return Json(objRes);
        }

        public ActionResult SubscriberRegistrationView_BEL(string RegisterMSISDN, string regType)
        {
            Models.Registration_BEL objReg = new Models.Registration_BEL();
            using (ServiceInvokeCRM serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl))
            {
                #region View Belgium Registration
                CustomerDetailsBELRequest custObjReq = new CustomerDetailsBELRequest();
                CustomerDetailBELResponse custObjResp = new CustomerDetailBELResponse();
                custObjReq.BrandCode = clientSetting.brandCode;
                custObjReq.CountryCode = clientSetting.countryCode;
                custObjReq.LanguageCode = clientSetting.langCode;
                custObjReq.MSISDN = RegisterMSISDN;
                custObjResp = serviceCRM.CRMGetSubscriberBEL(custObjReq);
                #endregion
                objReg.CountryName = clientSetting.mvnoSettings.countryName.Trim();
                objReg.BELRegSubscriber = custObjResp.registerDetailsBEL;
                objReg.responseDetails = custObjResp.responseDetails;
            }
            return View(objReg);
        }
        #endregion

        #region IRELAND
        public ActionResult SubscriberRegistration_IRL(string RegisterMsisdn)
        {
            Registration_IRL IRLobjres = new Registration_IRL();
            try
            {
                IRLobjres.strDropdown = Utility.GetDropdownMasterFromDB("1,4,15,16", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB("tbl_county")).ToList();
                IRLobjres.countrycode = clientSetting.mvnoSettings.countryCode;
                IRLobjres.MSISDN = RegisterMsisdn;
                IRLobjres.Mode = "Insert";
                // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                Session["PAType"] = null;
            }
            catch (Exception ex)
            {
                IRLobjres.strDropdown = Utility.GetDropdownMasterFromDB("1,4,15,16", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB("tbl_county")).ToList();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View("SubscriberRegistration_IRL", IRLobjres);
        }

        public ActionResult SubscriberRegistrationEdit_IRL(string RegisterMsisdn)
        {
            Registration_IRL IRLobjres = new Registration_IRL();
            try
            {
                IRLobjres.strDropdown = Utility.GetDropdownMasterFromDB("1,4,15,16", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB("tbl_county")).ToList();
                IRLobjres.countrycode = clientSetting.mvnoSettings.countryCode.ToString();
                IRLobjres.Mode = "Edit";
                Session["PAType"] = null;
                ViewBag.Title = "Subscriber Edit";
            }
            catch (Exception ex)
            {
                IRLobjres.strDropdown = Utility.GetDropdownMasterFromDB("1,4,15,16", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB("tbl_county")).ToList();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View("SubscriberRegistration_IRL", IRLobjres);
        }

        public ActionResult SubscriberRegistrationView_IRL()
        {
            Registration_IRL IRLobjres = new Registration_IRL();
            try
            {
                IRLobjres.countrycode = clientSetting.mvnoSettings.countryCode;
                Session["PAType"] = null;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View("subscriberview_IRL", IRLobjres);
        }

        public JsonResult InserIRLRegistration(string IRLRegistration)
        {
            ServiceCRM.IRLRegisterSubscriberRes ObjRes = new ServiceCRM.IRLRegisterSubscriberRes();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                ServiceCRM.IRLRegisterSubscriberReq objReg = JsonConvert.DeserializeObject<ServiceCRM.IRLRegisterSubscriberReq>(IRLRegistration);

                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMRegisterSubscriberIRL(objReg);
                

                if (ObjRes != null && objReg.Mode.ToUpper() == "UPDATE" && objReg.mobilePurpose.ToUpper() == "PERSONAL")
                {


                    Session["SubscriberTitle"] = objReg.Title;
                    Session["SubscriberName"] = objReg.FirstName + "|" + objReg.LastName;
                    string dateformat = clientSetting.mvnoSettings.dateTimeFormat.Trim().ToUpper();
                    Session["DOB"] = dateformat.Replace("MM", objReg.BirthMM).Replace("DD", objReg.BirthDD).Replace("YYYY", objReg.BirthYYYY);
                }

                string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_IRL_" + ObjRes.ResponseCode);
                ObjRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.ResponseDesc : errorInsertMsg;

                if (objReg.Mode.ToUpper() == "UPDATE")
                {
                    //if (ObjRes.ResponseCode == "0")
                    //{
                    // ObjRes.ResponseDesc = Resources.ErrorResources.ResourceManager.GetString("ERegupd_IRL_" + ObjRes.ResponseCode);
                    string errorUpdateMsg = Resources.ErrorResources.ResourceManager.GetString("ERegupd_IRL_" + ObjRes.ResponseCode);
                    ObjRes.ResponseDesc = string.IsNullOrEmpty(errorUpdateMsg) ? ObjRes.ResponseDesc : errorUpdateMsg;
                    // }
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
            return Json(ObjRes);
        }

        public JsonResult CRMGetSubscriberIRL(string POLGet)
        {
            IRLGetSubscriberResponse ObjRes = new IRLGetSubscriberResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                IRLGetSubscriberRequest ObjReq = JsonConvert.DeserializeObject<IRLGetSubscriberRequest>(POLGet);
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode.ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                



                    ObjRes = serviceCRM.CRMGetSubscriberIRL(ObjReq);


                
                if (ObjRes.IRLGetSubscriber.NDDPreference != "" && ObjRes.IRLGetSubscriber.NDDPreference != null && ObjRes.IRLGetSubscriber.NDDPreference != "0")
                {
                    List<DropdownMaster> Ndddrp = Utility.GetDropdownMasterFromDB("16", "2", "drop_master");
                    if (Ndddrp.Where(m => m.ID.Contains(ObjRes.IRLGetSubscriber.NDDPreference)).Count() > 0)
                        ObjRes.IRLGetSubscriber.NDDPreference = Ndddrp.FindAll(m => m.ID.Contains(ObjRes.IRLGetSubscriber.NDDPreference))[0].Value;
                }
                else
                {
                    ObjRes.IRLGetSubscriber.NDDPreference = "";
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
            return Json(ObjRes);
        }
        #endregion

        #region Roumania  Registration
        public ActionResult SubscriberRegistration_ROU(string RegisterMSISDN, string Mode)
        {
            Models.Registration_ROU objReg = new Models.Registration_ROU();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMSISDN);
                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,4,10", objReg.IsPostpaid, "drop_master");

                ///insert
                objReg.CountryDateFormat = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                #region BELRegister Model binding
                ROURegisterSubscriber ROURegSubc = new ROURegisterSubscriber();
                ROURegSubc.Msisdn = RegisterMSISDN != null ? RegisterMSISDN : string.Empty;
                objReg.ROURegSubscriber = ROURegSubc;
                #endregion
                objReg.Mode = "INSERT";

            }
            catch (Exception ex) { CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex); }
            return View(objReg);
        }

        public ActionResult SubscriberRegistrationEdit_ROU(string RegisterMSISDN, string Mode)
        {
            Models.Registration_ROU objReg = new Models.Registration_ROU();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMSISDN);
                objReg.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objReg.strDropdown = Utility.GetDropdownMasterFromDB("1,4,10", objReg.IsPostpaid, "drop_master");

                ///update
                objReg.CountryDateFormat = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                #region ROURegister Model binding
                ROURegisterSubscriber ROURegSubc = new ROURegisterSubscriber();
                ROURegSubc.Msisdn = RegisterMSISDN != null ? RegisterMSISDN : string.Empty;
                objReg.ROURegSubscriber = ROURegSubc;
                #endregion

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    #region View ROUgium Registration
                    ROUGetSubscriberRequest custObjReq = new ROUGetSubscriberRequest();
                    ROUGetSubscriberResponse custObjResp = new ROUGetSubscriberResponse();
                    custObjReq.BrandCode = clientSetting.brandCode;
                    custObjReq.CountryCode = clientSetting.countryCode;
                    custObjReq.LanguageCode = clientSetting.langCode;
                    custObjReq.MSISDN = RegisterMSISDN;
                    custObjResp = serviceCRM.CRMGetSubscriberROU(custObjReq);
                    if (objReg.strDropdown.FindAll(a => a.Master_id == "4" && a.Value == custObjResp.ROUGetSubscriber.PrefLang).Count > 0)
                        custObjResp.ROUGetSubscriber.PrefLangId = Convert.ToInt32(objReg.strDropdown.FindAll(a => a.Master_id == "4" && a.Value == custObjResp.ROUGetSubscriber.PrefLang)[0].ID);
                    #endregion
                    objReg.CountryName = clientSetting.mvnoSettings.countryName.Trim();
                    objReg.ROURegSubscriber = custObjResp.ROUGetSubscriber;
                    objReg.responseDetails = custObjResp.reponseDetails;

                    objReg.Mode = Mode.Trim().ToUpper();
                
                }
            catch (Exception ex) { CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex); }
            finally
            {
                serviceCRM = null;
            }
            return View("SubscriberRegistration_ROU", objReg);
        }

        public JsonResult SaveRegistration_ROU(string Registration)
        {
            CRMResponse objRes = new CRMResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                ROURegisterSubscriber objBELReg = JsonConvert.DeserializeObject<ROURegisterSubscriber>(Registration);
                objBELReg.CountryCode = clientSetting.countryCode;
                objBELReg.BrandCode = clientSetting.brandCode;
                objBELReg.LanguageCode = clientSetting.langCode;
                objBELReg.CSAgent = Convert.ToString(Session["UserName"]);
                if (objBELReg.DateOfBirth != string.Empty)
                {
                    string DOB = Utility.GetDateconvertion(objBELReg.DateOfBirth, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    string[] DOBSplit = DOB.Split('/');
                    objBELReg.BirthDD = DOBSplit[0];
                    objBELReg.BirthMM = DOBSplit[1];
                    objBELReg.BirthYYYY = DOBSplit[2];

                }
                else
                {
                    objBELReg.BirthDD = string.Empty;
                    objBELReg.BirthMM = string.Empty;
                    objBELReg.BirthYYYY = string.Empty;
                }
                if (objBELReg.Mode == "INSERT")
                {
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        objRes = serviceCRM.CRMRegisterSubscriberROU(objBELReg);
                    
                }
                else if (objBELReg.Mode == "UPDATE")
                {
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        objRes = serviceCRM.CRMRegisterSubscriberROU(objBELReg);
                        if (objRes.ResponseCode == "0")
                        {
                            objRes.ResponseCode = "00";
                        }
                    
                }
                else if (objBELReg.Mode == "EXISTING")
                {
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        objRes = serviceCRM.CRMRegisterSubscriberROU(objBELReg);
                    
                }
                if (objRes != null && objRes.ResponseCode != null)
                {

                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_ROU_" + objRes.ResponseCode);
                        objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;
                    }
                    if (objRes.ResponseCode == "00" && objBELReg.Mode == "UPDATE")
                    {
                        Session["SubscriberTitle"] = objBELReg.Title;
                        Session["SubscriberName"] = objBELReg.FirstName + "|" + objBELReg.LastName;
                        Session["DOB"] = objBELReg.DateOfBirth;
                    }
                }
                else
                {
                    objRes.ResponseDesc = Resources.ErrorResources.Common_2;
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.ResponseCode = "2000";
                objRes.ResponseDesc = ex.ToString();
            }
            finally
            {
                serviceCRM = null;
            }
            return Json(objRes);
        }

        public ActionResult SubscriberRegistrationView_ROU(string RegisterMSISDN, string regType)
        {
            Models.Registration_ROU objReg = new Models.Registration_ROU();
            using (ServiceInvokeCRM serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl))
            {
                #region View Belgium Registration
                ROUGetSubscriberRequest custObjReq = new ROUGetSubscriberRequest();
                ROUGetSubscriberResponse custObjResp = new ROUGetSubscriberResponse();
                custObjReq.BrandCode = clientSetting.brandCode;
                custObjReq.CountryCode = clientSetting.countryCode;
                custObjReq.LanguageCode = clientSetting.langCode;
                custObjReq.MSISDN = RegisterMSISDN;
                custObjResp = serviceCRM.CRMGetSubscriberROU(custObjReq);
                #endregion
                objReg.CountryName = clientSetting.mvnoSettings.countryName.Trim();
                objReg.ROURegSubscriber = custObjResp.ROUGetSubscriber;
                objReg.responseDetails = custObjResp.reponseDetails;
            }
            return View(objReg);
        }
        #endregion

        #region Hong Kong

        public ActionResult SubscriberRegistration_HKG(string RegisterMsisdn, string Mode)
        {
            CRM.Models.HKGRegisterSubscriberReq objRegistration = new CRM.Models.HKGRegisterSubscriberReq();
            HKGRegisterSubscriber HKGRegisterSubscriber = new HKGRegisterSubscriber();
            objRegistration.HKGRegSubscriber = HKGRegisterSubscriber;
            try
            {
                //Session["isPrePaid"] = "1";
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
                string dateString = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                objRegistration.CountryDateFormat = dateString;
                objRegistration.HKGRegSubscriber.Msisdn = RegisterMsisdn;
                objRegistration.HKGRegSubscriber.Mode = "INSERT";
                objRegistration.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,10", objRegistration.IsPostpaid, "drop_master");
                ViewBag.Title = @Resources.RegistrationResources.HKGRegistration;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objRegistration);
        }

        public ActionResult SubscriberRegistrationEdit_HKG(string RegisterMsisdn, string Mode)
        {
            CRM.Models.HKGRegisterSubscriberReq objRegistration = new CRM.Models.HKGRegisterSubscriberReq();
            HKGRegisterSubscriber HKGRegisterSubscriber = new HKGRegisterSubscriber();
            HKGGetSubscriberReqest objGetViewRequest = new HKGGetSubscriberReqest();
            HKGGetSubscriberResponse objGetViewResp = new HKGGetSubscriberResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            objRegistration.HKGRegSubscriber = HKGRegisterSubscriber;

            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);

            try
            {
                if (Mode.ToUpper().Equals("UPDATE") && !string.IsNullOrEmpty(Mode))
                {
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);

                    objGetViewRequest.CountryCode = clientSetting.countryCode;
                    objGetViewRequest.BrandCode = clientSetting.brandCode;
                    objGetViewRequest.LanguageCode = clientSetting.langCode;
                    objGetViewRequest.MSISDN = RegisterMsisdn;

                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        objGetViewResp = serviceCRM.CRMGetSubscriberHKG(objGetViewRequest);

                        objRegistration.ResponseCode = objGetViewResp.reponseDetails.ResponseCode;
                        objRegistration.ResponseDesc = objGetViewResp.reponseDetails.ResponseDesc;
                        if (objRegistration.ResponseCode.Equals("0"))
                        {
                            objRegistration.HKGRegSubscriber = objGetViewResp.getcustomer;
                            string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                            objRegistration.CountryDateFormat = Datestring;
                            objRegistration.HKGRegSubscriber.Mode = Mode;
                            objRegistration.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                            objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,10", objRegistration.IsPostpaid, "drop_master");
                            ViewBag.Title = @Resources.RegistrationResources.HKGEditReg;
                        }
                        else
                        {
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRegistration.ResponseCode + " " + objRegistration.ResponseDesc);
                        }
                    

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
            return View("SubscriberRegistration_HKG", objRegistration);
        }

        public ActionResult SaveResgistration_HKG(string Registration)
        {
            CRMResponse objRes = new CRMResponse();
            try
            {
                CRM.Models.HKGRegisterSubscriberReq objReg = new CRM.Models.HKGRegisterSubscriberReq();
                HKGRegisterSubscriber HKGRegisterSubscriber = new HKGRegisterSubscriber();
                objReg.HKGRegSubscriber = HKGRegisterSubscriber;

                objReg.HKGRegSubscriber = JsonConvert.DeserializeObject<HKGRegisterSubscriber>(Registration);
                objReg.HKGRegSubscriber.CountryCode = clientSetting.countryCode;
                objReg.HKGRegSubscriber.BrandCode = clientSetting.brandCode;
                objReg.HKGRegSubscriber.LanguageCode = clientSetting.langCode;
                objReg.HKGRegSubscriber.CSAgent = Convert.ToString(Session["UserName"]);

                StringBuilder sb = new StringBuilder();
                objRes = crmNewService.CRMRegisterSubscriberHKG(objReg.HKGRegSubscriber);

                if (!string.IsNullOrEmpty(objRes.ResponseCode))
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_HKG" + objRes.ResponseCode);
                    objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;
                }

                switch (objRes.ResponseCode)
                {
                    case "0":
                        objRes.ResponseDesc = objReg.HKGRegSubscriber.Mode.ToUpper() == "UPDATE" ? Resources.ErrorResources.EReg_HKG00 : Resources.ErrorResources.EReg_HKG0;
                        break;

                    case "110":
                        objRes.ResponseCode = "0";
                        break;
                    case "111":
                        objRes.ResponseCode = "0";
                        break;
                    case "100":
                        objRes.ResponseCode = "0";
                        break;
                    case "112":
                        objRes.ResponseCode = "0";
                        break;
                    case "113":
                        objRes.ResponseCode = "0";
                        break;
                    case "114":
                        objRes.ResponseCode = "0";
                        break;
                }

                if (objReg.HKGRegSubscriber.Mode.ToLower() == "update" && objRes.ResponseCode == "0")
                {
                    Session["SubscriberTitle"] = objReg.HKGRegSubscriber.Title;
                    Session["SubscriberName"] = objReg.HKGRegSubscriber.FirstName + "|" + objReg.HKGRegSubscriber.LastName;
                    string dateformat = clientSetting.mvnoSettings.dateTimeFormat.Trim().ToUpper();
                    Session["DOB"] = dateformat.Replace("MM", objReg.HKGRegSubscriber.BirthMM).Replace("DD", objReg.HKGRegSubscriber.BirthDD).Replace("YYYY", objReg.HKGRegSubscriber.BirthYYYY);
                }


                //if (objReg.HKGRegSubscriber.Mode.ToUpper() == "UPDATE" && (objRes.ResponseCode == "0" || objRes.ResponseCode == "100" || objRes.ResponseCode == "110" || objRes.ResponseCode == "111" || objRes.ResponseCode == "112"))
                //{

                //}
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objRes);
        }

        public ActionResult SubscriberRegistrationView_HKG(string RegisterMsisdn, string Mode)
        {
            CRM.Models.HKGRegisterSubscriberReq objReg = new CRM.Models.HKGRegisterSubscriberReq();
            HKGRegisterSubscriber objViewReg = new HKGRegisterSubscriber();
            objReg.HKGRegSubscriber = objViewReg;
            //s
            ServiceInvokeCRM serviceCRM;
            HKGGetSubscriberReqest objGetViewRequest = new HKGGetSubscriberReqest();
            HKGGetSubscriberResponse objgetViewResp = new HKGGetSubscriberResponse();
            try
            {
                objGetViewRequest.CountryCode = clientSetting.countryCode;
                objGetViewRequest.BrandCode = clientSetting.brandCode;
                objGetViewRequest.LanguageCode = clientSetting.langCode;
                objGetViewRequest.MSISDN = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objgetViewResp = serviceCRM.CRMGetSubscriberHKG(objGetViewRequest);
                
                if (objgetViewResp.reponseDetails.ResponseCode == "0")
                {
                    objReg.ResponseCode = objgetViewResp.reponseDetails.ResponseCode;
                    objReg.ResponseDesc = objgetViewResp.reponseDetails.ResponseDesc;
                    objReg.HKGRegSubscriber = objgetViewResp.getcustomer;
                }
                else
                {
                    objReg.ResponseCode = objgetViewResp.reponseDetails.ResponseCode;
                    objReg.ResponseDesc = objgetViewResp.reponseDetails.ResponseDesc;
                }

            }
            catch (Exception Ex)
            {

                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, Ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDesc = Ex.ToString();
            }
            finally
            {
                serviceCRM = null;
            }
            return View("SubscriberRegistrationView_HKG", objReg);
        }
        #endregion

        #region PRT
        public ActionResult SubscriberRegistration_PRT(string RegisterMsisdn, string Mode)
        {
            CRM.Models.PRTRegisterSubscriberReq objRegistration = new CRM.Models.PRTRegisterSubscriberReq();
            PRTRegisterSubscriber PRTRegisterSubscriber = new PRTRegisterSubscriber();
            objRegistration.PRTRegSubscriber = PRTRegisterSubscriber;

            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
                string dateString = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                objRegistration.CountryDateFormat = dateString;
                objRegistration.PRTRegSubscriber.MSISDN = RegisterMsisdn;
                objRegistration.PRTRegSubscriber.mode = "INSERT";
                objRegistration.IsPostpaid = Session["IsPrePaid"] != null ? Convert.ToString(Session["IsPrePaid"]) : "1";

                objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,8,10,39", objRegistration.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "TblCountry")).ToList();
                ViewBag.Title = @Resources.RegistrationResources.PRTRegistration;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objRegistration);
        }

        public ActionResult SaveResgistration_PRT(string Registration)
        {
            CRMResponse objRes = new CRMResponse();
            try
            {
                CRM.Models.PRTRegisterSubscriberReq objReg = new CRM.Models.PRTRegisterSubscriberReq();
                PRTRegisterSubscriber PRTRegisterSubscriber = new PRTRegisterSubscriber();
                objReg.PRTRegSubscriber = PRTRegisterSubscriber;

                objReg.PRTRegSubscriber = JsonConvert.DeserializeObject<PRTRegisterSubscriber>(Registration);
                objReg.PRTRegSubscriber.CountryCode = clientSetting.countryCode;
                objReg.PRTRegSubscriber.BrandCode = clientSetting.brandCode;
                objReg.PRTRegSubscriber.LanguageCode = clientSetting.langCode;
                objReg.PRTRegSubscriber.userName = objReg.CSAgent = Convert.ToString(Session["UserName"]);

                StringBuilder sb = new StringBuilder();
                objRes = crmNewService.CRMRegisterSubscriberPRT(objReg.PRTRegSubscriber);
                if (!string.IsNullOrEmpty(objRes.ResponseCode))
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_PRT" + objRes.ResponseCode);
                    objRes.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.ResponseDesc : errorInsertMsg;
                }

                switch (objRes.ResponseCode)
                {
                    case "0":
                        objRes.ResponseDesc = objReg.PRTRegSubscriber.mode.ToUpper() == "UPDATE" ? Resources.ErrorResources.EReg_PRT00 : Resources.ErrorResources.EReg_PRT0;
                        break;

                    case "110":
                        objRes.ResponseCode = "0";
                        break;
                    case "111":
                        objRes.ResponseCode = "0";
                        break;
                    case "100":
                        objRes.ResponseCode = "0";
                        break;
                    case "112":
                        objRes.ResponseCode = "0";
                        break;
                    case "113":
                        objRes.ResponseCode = "0";
                        break;
                    case "114":
                        objRes.ResponseCode = "0";
                        break;

                }

                if (objReg.PRTRegSubscriber.mode.ToLower() == "update" && objRes.ResponseCode == "0")
                {
                    Session["SubscriberTitle"] = objReg.PRTRegSubscriber.Title;
                    Session["SubscriberName"] = objReg.PRTRegSubscriber.firstName + "|" + objReg.PRTRegSubscriber.lastName;
                    string dateformat = clientSetting.mvnoSettings.dateTimeFormat.Trim().ToUpper();
                    Session["DOB"] = dateformat.Replace("MM", objReg.PRTRegSubscriber.birthMM).Replace("DD", objReg.PRTRegSubscriber.birthDD).Replace("YYYY", objReg.PRTRegSubscriber.birthYYYY);
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objRes);
        }

        public ActionResult SubscriberRegistrationView_PRT(string RegisterMsisdn, string Mode)
        {
            CRM.Models.PRTRegisterSubscriberReq objReg = new CRM.Models.PRTRegisterSubscriberReq();
            PRTRegisterSubscriber objViewReg = new PRTRegisterSubscriber();
            objReg.PRTRegSubscriber = objViewReg;
            //s
            ServiceInvokeCRM serviceCRM;
            CustomerDetailsPRTRequest objGetViewRequest = new CustomerDetailsPRTRequest();
            CustomerDetailPRTResponse objgetViewResp = new CustomerDetailPRTResponse();
            try
            {
                objGetViewRequest.CountryCode = clientSetting.countryCode;
                objGetViewRequest.BrandCode = clientSetting.brandCode;
                objGetViewRequest.LanguageCode = clientSetting.langCode;
                objGetViewRequest.MSISDN = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objgetViewResp = serviceCRM.CRMGetSubscriberPRT(objGetViewRequest);
                
                if (objgetViewResp.ResponseDetails.ResponseCode == "0")
                {
                    objReg.ResponseCode = objgetViewResp.ResponseDetails.ResponseCode;
                    objReg.ResponseDesc = objgetViewResp.ResponseDetails.ResponseDesc;
                    objReg.PRTRegSubscriber = objgetViewResp.registerDetailsPRT;
                    string strPostCode = objReg.PRTRegSubscriber.postCode;
                    string[] postCode = new string[2];
                    if (!string.IsNullOrEmpty(strPostCode))
                    {
                        postCode = strPostCode.Split('-');
                        objReg.PRTRegSubscriber.postCode = postCode[0];
                        objReg.TeritorialCode = postCode[1];
                    }
                }
                else
                {
                    objReg.ResponseCode = objgetViewResp.ResponseDetails.ResponseCode;
                    objReg.ResponseDesc = objgetViewResp.ResponseDetails.ResponseDesc;
                }

            }
            catch (Exception Ex)
            {

                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, Ex);
                objReg.ResponseCode = "2";
                objReg.ResponseDesc = Ex.ToString();
            }
            finally
            {
                serviceCRM = null;
            }
            return View("SubscriberRegistrationView_PRT", objReg);
        }

        public ActionResult SubscriberRegistrationEdit_PRT(string RegisterMsisdn, string Mode)
        {
            PRTRegisterSubscriberReq objRegistration = new PRTRegisterSubscriberReq();
            PRTRegisterSubscriber PRTRegSubscriber = new PRTRegisterSubscriber();
            CustomerDetailsPRTRequest objGetRequest = new CustomerDetailsPRTRequest();
            CustomerDetailPRTResponse objGetResponse = new CustomerDetailPRTResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            objRegistration.PRTRegSubscriber = PRTRegSubscriber;
            ViewBag.Title = @Resources.RegistrationResources.PRTRegistration;

            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
            try
            {
                if (!string.IsNullOrEmpty(Mode) && Mode.ToUpper().Equals("UPDATE"))
                {
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
                    objGetRequest.CountryCode = clientSetting.countryCode;
                    objGetRequest.BrandCode = clientSetting.brandCode;
                    objGetRequest.LanguageCode = clientSetting.langCode;
                    objGetRequest.MSISDN = RegisterMsisdn;

                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        objGetResponse = serviceCRM.CRMGetSubscriberPRT(objGetRequest);

                        objRegistration.ResponseCode = objGetResponse.ResponseDetails.ResponseCode;
                        objRegistration.ResponseDesc = objGetResponse.ResponseDetails.ResponseDesc;

                        if (objRegistration.ResponseCode.Equals("0"))
                        {
                            objRegistration.PRTRegSubscriber = objGetResponse.registerDetailsPRT;
                            string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();

                            objRegistration.CountryDateFormat = Datestring;
                            objRegistration.PRTRegSubscriber.mode = Mode;
                            string strPostCode = objRegistration.PRTRegSubscriber.postCode;
                            string[] postCode = new string[2];
                            if (!string.IsNullOrEmpty(strPostCode))
                            {
                                postCode = strPostCode.Split('-');
                                objRegistration.PRTRegSubscriber.postCode = postCode[0];
                                objRegistration.TeritorialCode = postCode[1];
                            }
                            objRegistration.PRTRegSubscriber.ICCID = objRegistration.PRTRegSubscriber.ICCID.Substring(objRegistration.PRTRegSubscriber.ICCID.Length - 4);
                            objRegistration.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                            objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,8,10,39", objRegistration.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "TblCountry")).ToList();
                            ViewBag.Title = @Resources.RegistrationResources.PRTEditReg;
                        }
                        else
                        {
                            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, objRegistration.ResponseCode + " " + objRegistration.ResponseDesc);
                        }
                    
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally {
                serviceCRM = null;
            }
            return View("SubscriberRegistration_PRT", objRegistration);
        }

        #endregion PRT

        #region NORWAY
        public ActionResult SubscriberRegistration_NOR(string RegisterMsisdn, string Mode)
        {
            Session["MobileNumber"] = string.Empty;
            NorRegister_Req CustomerDetails = new NorRegister_Req();
            RegisterDetailsNOR objGetViewReq = new RegisterDetailsNOR();
            CRMResponse objGetViewResp = new CRMResponse();
            List<FileDetail> FD = new List<FileDetail>();

            try
            {
                string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                CustomerDetails.CountryDateFormat = Datestring;
                objGetViewReq.Country = clientSetting.mvnoSettings.countryName.ToUpper();
                CustomerDetails.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                CustomerDetails.strDropdown = Utility.GetDropdownMasterFromDB("1,31,32", CustomerDetails.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, CustomerDetails.IsPostpaid, "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, CustomerDetails.IsPostpaid, "Tbl_Nationality")).ToList();
                CustomerDetails.DocFormat = clientSetting.mvnoSettings.documentFormat.ToUpper();
                CustomerDetails.DocDetails = "1" + "|" + Resources.RegistrationResources.UploadSingleFile;
                CustomerDetails.RegisterDetailsNOR = new RegisterDetailsNOR();
                CustomerDetails.RegisterDetailsNOR.Msisdn = !string.IsNullOrEmpty(RegisterMsisdn) ? RegisterMsisdn.Substring(clientSetting.mvnoSettings.countryPrefix.Length) : string.Empty;
                CustomerDetails.FileName = FD;
                CustomerDetails.strMode = "INSERT";
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View("SubscriberRegistration_NOR", CustomerDetails);
        }
        public ActionResult SubscriberRegistrationEdit_NOR(string RegisterMsisdn, string Mode)
        {

            CustomerDetailsNORRequest objGetViewReq = new CustomerDetailsNORRequest();
            CustomerDetailNORResponse objGetViewResp = new CustomerDetailNORResponse();
            NorRegister_Req objRq = new NorRegister_Req();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                #region File Upload

                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                #region Create path

                string registrationDocpath = clientSetting.preSettings.registrationDocpath;
                if (!Directory.Exists(registrationDocpath))
                {
                    Directory.CreateDirectory(registrationDocpath);
                }
                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    string UploadFile = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string UploadFile = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);

                    if (!Directory.Exists(UploadFile))
                    {
                        Directory.CreateDirectory(UploadFile);
                    }
                }

                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    string PdfDownload = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (!Directory.Exists(PdfDownload))
                    {
                        Directory.CreateDirectory(PdfDownload);
                    }
                }
                #endregion

                #endregion

                if ((!string.IsNullOrEmpty(Mode)) && Mode.ToUpper().Trim() == "UPDATE")
                {
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
                    objGetViewReq.CountryCode = clientSetting.countryCode;
                    objGetViewReq.BrandCode = clientSetting.brandCode;
                    objGetViewReq.LanguageCode = clientSetting.langCode;
                    objGetViewReq.MSISDN = !string.IsNullOrEmpty(RegisterMsisdn) ? RegisterMsisdn : string.Empty;
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        objGetViewResp = serviceCRM.CRMGetCustomerDetailsNOR(objGetViewReq);

                        objRq.RegisterDetailsNOR = objGetViewResp.RegisterDetailsNOR;
                        objRq.FileName = objGetViewResp.FileName;
                        string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                        objRq.CountryDateFormat = Datestring;
                        objRq.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                        objRq.strDropdown = Utility.GetDropdownMasterFromDB("1,31,32", objRq.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRq.IsPostpaid, "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRq.IsPostpaid, "Tbl_Nationality")).ToList();
                        objRq.strMode = "UPDATE";
                        objRq.DocFormat = clientSetting.mvnoSettings.documentFormat.ToUpper();
                        if (!_runningFromNUnit)
                        {
                            objRq.DocDetails = "1" + "|" + Resources.RegistrationResources.UploadSingleFile;
                        }
                        objRq.RegisterDetailsNOR.Msisdn = objRq.RegisterDetailsNOR.Msisdn.Substring(clientSetting.mvnoSettings.countryPrefix.Length);
                        objRq.RegisterDetailsNOR.BrandCode = objRq.RegisterDetailsNOR.BrandCode == null ? string.Empty : objRq.RegisterDetailsNOR.BrandCode;
                        objRq.RegisterDetailsNOR.LanguageCode = objRq.RegisterDetailsNOR.LanguageCode == null ? string.Empty : objRq.RegisterDetailsNOR.LanguageCode;
                        objRq.RegisterDetailsNOR.CountryCode = objRq.RegisterDetailsNOR.CountryCode == null ? string.Empty : objRq.RegisterDetailsNOR.CountryCode;
                        objRq.RegisterDetailsNOR.RegisterdBy = objRq.RegisterDetailsNOR.RegisterdBy == null ? string.Empty : objRq.RegisterDetailsNOR.RegisterdBy;
                        objRq.RegisterDetailsNOR.SMSUPDATE = objRq.RegisterDetailsNOR.SMSUPDATE == null ? string.Empty : objRq.RegisterDetailsNOR.SMSUPDATE;
                        objRq.RegisterDetailsNOR.Mode = objRq.RegisterDetailsNOR.Mode == null ? string.Empty : objRq.RegisterDetailsNOR.Mode;
                        objRq.RegisterDetailsNOR.Username = objRq.RegisterDetailsNOR.Username == null ? string.Empty : objRq.RegisterDetailsNOR.Username;
                        objRq.RegisterDetailsNOR.UpdateFor = objRq.RegisterDetailsNOR.UpdateFor == null ? string.Empty : objRq.RegisterDetailsNOR.UpdateFor;
                        objRq.RegisterDetailsNOR.UpdateMode = objRq.RegisterDetailsNOR.UpdateMode == null ? string.Empty : objRq.RegisterDetailsNOR.UpdateMode;
                        objRq.RegisterDetailsNOR.rdPerYes = objRq.RegisterDetailsNOR.rdPerYes == null ? string.Empty : objRq.RegisterDetailsNOR.rdPerYes;
                        objRq.RegisterDetailsNOR.fileName = objRq.RegisterDetailsNOR.fileName == null ? string.Empty : objRq.RegisterDetailsNOR.fileName;
                        objRq.RegisterDetailsNOR.IsGAF = objRq.RegisterDetailsNOR.IsGAF == null ? string.Empty : objRq.RegisterDetailsNOR.IsGAF;
                        objRq.RegisterDetailsNOR.DocFormat = objRq.RegisterDetailsNOR.DocFormat == null ? string.Empty : objRq.RegisterDetailsNOR.DocFormat;
                        objRq.RegisterDetailsNOR.Filesize = objRq.RegisterDetailsNOR.Filesize == null ? string.Empty : objRq.RegisterDetailsNOR.Filesize;
                        objRq.RegisterDetailsNOR.houseName = objRq.RegisterDetailsNOR.houseName == null ? string.Empty : objRq.RegisterDetailsNOR.houseName;
                        objRq.RegisterDetailsNOR.AccountNo = objRq.RegisterDetailsNOR.AccountNo == null ? string.Empty : objRq.RegisterDetailsNOR.AccountNo;
                        if (!_runningFromNUnit)
                        {
                            objRq.FileName[0].SerialNo = objRq.FileName[0].SerialNo == null ? string.Empty : objRq.FileName[0].SerialNo;
                            objRq.FileName[0].CustomerID = objRq.FileName[0].CustomerID == null ? string.Empty : objRq.FileName[0].CustomerID;
                            objRq.FileName[0].Filepath = objRq.FileName[0].Filepath.Replace("\\", "$$");
                            objRq.RegisterDetailsNOR.fileName = objRq.FileName[0].Filename == null ? string.Empty : objRq.FileName[0].Filename;
                            Session["GetFileNameNor"] = objRq.FileName[0].Filename;
                        }
                        objRq.LanguageCode = objRq.LanguageCode == null ? string.Empty : objRq.LanguageCode;
                        objRq.BrandCode = objRq.BrandCode == null ? string.Empty : objRq.BrandCode;
                        objRq.CountryCode = objRq.CountryCode == null ? string.Empty : objRq.CountryCode;
                        objRq.ResponseCode = objRq.ResponseCode == null ? string.Empty : objRq.ResponseCode;
                        objRq.ResponseDesc = objRq.ResponseDesc == null ? string.Empty : objRq.ResponseDesc;
                        objRq.RegisterDetailsNOR.Pukcode = objRq.RegisterDetailsNOR.Pukcode == "0" ? string.Empty : objRq.RegisterDetailsNOR.Pukcode;
                        Session["FilePathNor"] = null;


                        #region File Upload

                        // string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;

                        #region Delete file from path
                        //if (Server.MapPath("~/App_Data/UploadFile") != null)
                        //{
                        //    var filepath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            var filepath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);

                            if (Directory.Exists(filepath))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(filepath);
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                                // Directory.Delete(filepath, true);
                            }
                        }

                        if (clientSetting.mvnoSettings.internalPdfDownload != null)
                        {
                            var filepath2 = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                            if (Directory.Exists(filepath2))
                            {
                                System.IO.DirectoryInfo di = new DirectoryInfo(filepath2);
                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                                // Directory.Delete(filepath2, true);
                            }
                        }

                        #endregion

                        #region Change Uploaded File Path
                        string sourcePath = ""; string[] GetFileName = new string[3];
                        // if (Server.MapPath("~/App_Data/UploadFile") != null)
                        // {
                        //   string targetPath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                        if (clientSetting.mvnoSettings.internalUploadFile != null)
                        {
                            string targetPath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);

                            if (objRq.FileName.Count() > 0)
                            {
                                for (int i = 0; i < objRq.FileName.Count(); i++)
                                {
                                    if (objRq.FileName[i].Filepath != null && objRq.FileName[i].Filepath != "")
                                    {
                                        sourcePath = objRq.FileName[i].Filepath;
                                    }
                                    else
                                    {
                                        sourcePath = clientSetting.preSettings.registrationDocpath;
                                    }
                                    if (!Directory.Exists(targetPath))
                                    {
                                        Directory.CreateDirectory(targetPath);
                                    }
                                    if (System.IO.File.Exists(sourcePath + "\\" + objRq.FileName[i].Filename))
                                    {
                                        System.IO.File.Copy(sourcePath + "\\" + objRq.FileName[i].Filename, targetPath + "\\" + objRq.FileName[i].Filename, true);
                                        string filename = objRq.FileName[i].Filename;
                                        GetFileName[i] = filename;
                                    }
                                }
                                if (objRq.FileName != null)
                                {
                                    for (int i = 0; i < objRq.FileName.Count(); i++)
                                    {
                                        GetFileName[i] = objRq.FileName[i].Filename;
                                    }
                                    GetfileFTP(targetPath + "\\", GetFileName, objGetViewReq, "");
                                }
                            }
                        }

                        #endregion

                        #endregion



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
            return View("SubscriberRegistration_NOR", objRq);
        }
        public JsonResult SaveResgistration_NOR(RegisterDetailsNOR objReg)
        {

            CRMResponse objRes = new CRMResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                objReg.Language = clientSetting.langCode;
                //  objReg.Country = clientSetting.mvnoSettings.countryName;
                objReg.IsGAF = clientSetting.mvnoSettings.useGAF;
                objReg.RegisterdBy = Session["UserName"].ToString();
                objReg.Contactno = string.IsNullOrEmpty(objReg.Contactno) ? string.Empty : objReg.Contactno;
                string origFileName = objReg.fileName;



                if (objReg.Dateofbirth != string.Empty)
                {
                    string strDOB = Utility.GetDateconvertion(objReg.Dateofbirth, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                    string[] strSplit = strDOB.Split('/');
                    objReg.Birthdd = strSplit[0].ToString();
                    objReg.Birthmm = strSplit[1].ToString();
                    objReg.Birthyy = strSplit[2].ToString();
                }
                else
                {
                    objReg.Birthdd = string.Empty;
                    objReg.Birthmm = string.Empty;
                    objReg.Birthyy = string.Empty;
                }




                if (Session["GetFileNameNor"] != null && objReg.Mode == "UPDATE")
                {
                    //if (Session["GetFileNameNor"].ToString() == objReg.fileName)
                    //{
                    if (objReg.fileName != null && objReg.fileName != "")
                        objReg.fileName = objReg.fileName.Replace(objReg.AccountNo + "_1", "");
                    else
                        objReg.fileName = "";

                    //}
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objRes = serviceCRM.CRMRegisterSubscriberNOR(objReg);
                
                if (!_runningFromNUnit)
                {

                    switch (objRes.ResponseCode)
                    {
                        case "0":
                            //if (objReg.IDType == "Personal Number" && !string.IsNullOrEmpty(objReg.fileName))
                            if (objReg.rdPerYes == "NO" && !string.IsNullOrEmpty(objReg.fileName))
                            {
                                objRes = SaveDoc(origFileName, objRes.ResponseDesc, objReg.IDType, objReg.Msisdn, objReg.RetailerID, objReg.Mode);
                            }
                            else if (objReg.Mode == "UPDATE")
                            {
                                objRes.ResponseDesc = Resources.ErrorResources.NOR_Reg_76;


                                Session["SubscriberTitle"] = objReg.Title;
                                Session["SubscriberName"] = objReg.Firstname + "|" + objReg.Lastname;
                                Session["DOB"] = clientSetting.mvnoSettings.dateTimeFormat.ToUpper().Replace("DD", objReg.Birthdd).Replace("MM", objReg.Birthmm).Replace("YYYY", objReg.Birthyy);

                            }
                            else
                            {
                                objRes.ResponseDesc = Resources.ErrorResources.NOR_Reg_75;
                            }
                            break;
                        case "1":
                            objRes.ResponseDesc = Resources.ErrorResources.NOR_Reg_66;
                            break;
                        case "3":
                            objRes.ResponseDesc = Resources.ErrorResources.NOR_Reg_67;
                            break;
                        case "4":
                            objRes.ResponseDesc = Resources.ErrorResources.NOR_Reg_69;
                            break;
                        case "7":
                            objRes.ResponseDesc = Resources.ErrorResources.NOR_Reg_71;
                            break;
                        case "8 ":
                            objRes.ResponseDesc = Resources.ErrorResources.NOR_Reg_72;
                            break;
                        case "9":
                            objRes.ResponseDesc = Resources.ErrorResources.NOR_Reg_73;
                            break;
                        case "500":
                            objRes.ResponseDesc = Resources.ErrorResources.NOR_Reg_500;
                            break;
                        default:
                            objRes.ResponseDesc = objRes.ResponseDesc;
                            break;
                    }
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
            return Json(objRes);
        }
        [HttpPost]
        public JsonResult FileUploadForNOR()
        {
            return Json(FileUploadNor("Doc"));
        }
        public AttachmentResponse FileUploadNor(string strDoc)
        {
            AttachmentResponse objres = new AttachmentResponse();
            string filepath = clientSetting.preSettings.registrationDocpath;
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            var file = Request.Files[0];
            var fileName = Path.GetFileName(file.FileName);
            var path = Path.Combine(filepath, fileName);
            Session["FileName"] = path;
            try
            {
                file.SaveAs(path);
                objres.ResponseCode = "0";
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objres.ResponseCode = "1";
            }
            return objres;
        }
        public CRMResponse SaveDoc(string path, string generatedFileName, string docType, string MSISDN, string retailorID, string mode)
        {
            CRMResponse crmResp = new CRMResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                string[] splitFileName = generatedFileName.Split(new string[] { "$$$" }, StringSplitOptions.RemoveEmptyEntries);
                string fileName = splitFileName[splitFileName.Length - 1];
                string response = splitFileName[0];
                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    var filepath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    var filepath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);

                    string targetPath = filepath.ToString();
                    string destFile = System.IO.Path.Combine(targetPath + "\\", fileName);
                    if (!System.IO.Directory.Exists(targetPath))
                    {
                        System.IO.Directory.CreateDirectory(targetPath);
                    }

                    System.IO.File.Move(targetPath + "\\" + path, targetPath + "\\" + fileName);

                    SFTPFileUploadRequest sftpFileReq = new SFTPFileUploadRequest();
                    string[] splitGeneratedFileName = fileName.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                    sftpFileReq.accountNumber = splitGeneratedFileName[0];
                    sftpFileReq.attachedBy = Session["UserName"].ToString();
                    sftpFileReq.CountryCode = clientSetting.countryCode;
                    sftpFileReq.BrandCode = clientSetting.brandCode;
                    sftpFileReq.LanguageCode = clientSetting.langCode;
                    sftpFileReq.docType = docType;
                    sftpFileReq.fileName = fileName;
                    sftpFileReq.destPath = string.Concat(targetPath + "\\", fileName);
                    sftpFileReq.MSISDN = MSISDN;
                    sftpFileReq.retailerID = retailorID;
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        crmResp = serviceCRM.CRMSFTPFileUpload(sftpFileReq);

                        if (crmResp.ResponseCode == "0")
                        {
                            FileInfo FileDelPath1 = new FileInfo(sftpFileReq.destPath);
                            if (FileDelPath1.Exists)
                            {
                                System.IO.File.Delete(sftpFileReq.destPath);
                            }
                        }
                    
                    if (crmResp.ResponseCode == "0")
                    {
                        crmResp.ResponseDesc = response + Resources.ErrorResources.NOR_Reg_78;
                    }
                    else
                    {
                        crmResp.ResponseDesc = response + crmResp.ResponseDesc;
                    }
                    return crmResp;
                }
                else
                {
                    crmResp.ResponseDesc = generatedFileName;
                    return crmResp;
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                crmResp.ResponseDesc = generatedFileName + ex.ToString();
                return crmResp;
            }
            finally
            {
                serviceCRM = null;
            }
        }
        public ActionResult SubscriberRegistrationView_NOR(string RegisterMsisdn, string Mode)
        {
            CustomerDetailsNORRequest objGetViewReq = new CustomerDetailsNORRequest();
            CustomerDetailNORResponse objGetViewResp = new CustomerDetailNORResponse();
            NorRegister_Req objRq = new NorRegister_Req();
            //s
            ServiceInvokeCRM serviceCRM;

            try
            {
                objGetViewReq.CountryCode = clientSetting.countryCode;
                objGetViewReq.BrandCode = clientSetting.brandCode;
                objGetViewReq.LanguageCode = clientSetting.langCode;
                objGetViewReq.MSISDN = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objGetViewResp = serviceCRM.CRMGetCustomerDetailsNOR(objGetViewReq);
                

                objRq.ResponseCode = objGetViewResp.ResponseDetails.ResponseCode;
                objRq.ResponseDesc = objGetViewResp.ResponseDetails.ResponseDesc;
                objRq.RegisterDetailsNOR = objGetViewResp.RegisterDetailsNOR;
                objRq.FileName = objGetViewResp.FileName;


            }
            catch (Exception Ex)
            {

                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, Ex);
                objRq.ResponseCode = "2";
                objRq.ResponseDesc = Ex.ToString();
            }
            finally
            {
                serviceCRM = null;
            }
            return View("SubscriberView_NOR", objRq);
        }
        #endregion

        public JsonResult Rename(string name, string Value)
        {
            try
            {
                string path = clientSetting.preSettings.registrationDocpath;
                string[] format = name.Split('.');
                List<Attachement> objLisAttach = new List<Attachement>();
                if (Directory.Exists(path))
                {
                    //Directory.Delete(path, true);
                    // File.Move(string.Empty, string.Empty);
                    //  string oldFileName = path + \\ "MyOldFile.txt";


                    // System.IO.File.Move(oldFileName, newFileName);
                    System.IO.File.Move(path + "\\" + name, path + "\\" + Value + "." + format[1]);


                }
                return Json(objLisAttach);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return null;
        }


        #region FTPS

        public bool FileUploadFTPSServer(string LocalFilepath, string FileName, CRMBase Input)
        {

            try
            {

                var request = (FtpWebRequest)WebRequest.Create(clientSetting.mvnoSettings.fTPSHost + "/" + FileName);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(clientSetting.mvnoSettings.fTPSUserName, clientSetting.mvnoSettings.fTPSPassword);
                CRMLogger.WriteMessage("","", "ftp WebRequest.Create");
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.EnableSsl = true;
                CRMLogger.WriteMessage("", "", "certificate start");
                ServicePointManager.ServerCertificateValidationCallback =
                    delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                    {
                        CRMLogger.WriteMessage("", "", "Certificate failure");
                        return true;

                    };
                CRMLogger.WriteMessage("", "", "certificate end");
                CRMLogger.WriteMessage("", "", "Entering into file upload");
                using (var fileStream = System.IO.File.OpenRead(LocalFilepath + "/" + FileName))
                {
                    CRMLogger.WriteMessage("", "","Path :" + LocalFilepath + "/" + FileName);
                    CRMLogger.WriteMessage("", "", "Reading data from file in path");
                    using (var requestStream = request.GetRequestStream())
                    {
                        CRMLogger.WriteMessage("", "", "Copying data from file start");
                        fileStream.CopyTo(requestStream);
                        CRMLogger.WriteMessage("", "", "Copying data from file end");
                        requestStream.Close();
                        CRMLogger.WriteMessage("", "", "file uploaded");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return false;

            }
        }
        public bool FileDownloadFTPSServer(string LocalFilepath, string FileName, CRMBase Input)
        {
            FtpWebRequest reqFTP = null;
            FtpWebResponse response = null;
            try
            {
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(clientSetting.mvnoSettings.fTPSHost + "/" + FileName));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UsePassive = true;
                reqFTP.UseBinary = true;
                reqFTP.KeepAlive = false;
                reqFTP.EnableSsl = true;

                ServicePointManager.ServerCertificateValidationCallback =
                    delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                    {
                        return true;
                    };
                reqFTP.Credentials = new NetworkCredential(clientSetting.mvnoSettings.fTPSUserName, clientSetting.mvnoSettings.fTPSPassword);
                response = (FtpWebResponse)reqFTP.GetResponse();
                using (Stream ftpStream = response.GetResponseStream())
                {
                    long cl = response.ContentLength;
                    int bufferSize = 2048;
                    int readCount;
                    byte[] buffer = new byte[bufferSize];
                    using (FileStream outputStream = new FileStream(LocalFilepath + "\\" + FileName, FileMode.Create))
                    {
                        readCount = ftpStream.Read(buffer, 0, bufferSize);
                        while (readCount > 0)
                        {
                            outputStream.Write(buffer, 0, readCount);
                            readCount = ftpStream.Read(buffer, 0, bufferSize);
                        }
                    }
                }
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return false;
            }
            finally
            {
                reqFTP = null;
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }
        #endregion

        #region FRR-5022

        public bool fileuploadftps(string LocalFilepath, string Strfilename, CRMBase Input)
        {
            NetworkCredential Credentials;
            try
            {

                if (clientSetting.mvnoSettings.enableFileupload.ToUpper() != "TRUE")
                {

                    var request = (FtpWebRequest)WebRequest.Create(clientSetting.mvnoSettings.fTPSHost + "/" + Strfilename);
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(clientSetting.mvnoSettings.fTPSUserName, clientSetting.mvnoSettings.fTPSPassword);
                    CRMLogger.WriteMessage("", "", "ftp WebRequest.Create");
                    request.UsePassive = true;
                    request.UseBinary = true;
                    request.KeepAlive = false;
                    request.EnableSsl = true;
                    CRMLogger.WriteMessage("", "", "certificate start");
                    ServicePointManager.ServerCertificateValidationCallback =
                        delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                        {
                            CRMLogger.WriteMessage("", "", "Certificate failure");
                            return true;

                        };
                    CRMLogger.WriteMessage("", "", "certificate end");
                    CRMLogger.WriteMessage("", "", "Entering into file upload");
                    using (var fileStream = System.IO.File.OpenRead(LocalFilepath + "/" + Strfilename))
                    {
                        CRMLogger.WriteMessage("", "", "Path :" + LocalFilepath + "/" + Strfilename);
                        CRMLogger.WriteMessage("", "", "Reading data from file in path");
                        using (var requestStream = request.GetRequestStream())
                        {
                            CRMLogger.WriteMessage("", "", "Copying data from file start");
                            fileStream.CopyTo(requestStream);
                            CRMLogger.WriteMessage("", "", "Copying data from file end");
                            requestStream.Close();
                            CRMLogger.WriteMessage("", "", "file uploaded");
                        }
                    }
                    return true;
                }
                else
                {
                    CRMLogger.WriteMessage("", "", "UNC Fileupload Start");
                    Credentials = new NetworkCredential(clientSetting.mvnoSettings.UNCUserName, clientSetting.mvnoSettings.UNCPassword, clientSetting.mvnoSettings.UNCDomain);
                    using (new NetworkConnection(@clientSetting.mvnoSettings.UNCFilePath, Credentials))
                    {

                    string TargetDocpath = clientSetting.mvnoSettings.UNCFilePath;
                    if (!Directory.Exists(TargetDocpath))
                    {
                        Directory.CreateDirectory(TargetDocpath);
                    }
                    System.IO.File.Copy(LocalFilepath + "\\" + Strfilename, clientSetting.mvnoSettings.UNCFilePath + "\\" + Strfilename, true);
                    return true;
                        
                    } 
                    CRMLogger.WriteMessage("", "", "Fileuploaded");
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return false;

            }
        }




        public FileResult FileDownloadFTPS(string LocalFilepath, string FileName, CRMBase Input)
        {
            FtpWebRequest reqFTP;
            var mimeType = "";
            var fileDownloadName = "";
            NetworkCredential Credentials;
            string path = "";
            byte[] theFolders;
            string fileNameDisplayedToUser = string.Empty;
            CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), Convert.ToString(Session["UserName"]), "First Time");
            try
            {
                CRMLogger.WriteMessage("", "", "FileDownloadFTPS-METHOD START");

                if (clientSetting.mvnoSettings.enableFileupload.ToUpper() != "TRUE")
                {
                
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(clientSetting.mvnoSettings.fTPSHost + "/" + FileName));
                    reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                    reqFTP.UsePassive = true;
                    reqFTP.UseBinary = true;
                    reqFTP.KeepAlive = false;
                    reqFTP.EnableSsl = true;

                    ServicePointManager.ServerCertificateValidationCallback =
                        delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                        {
                            return true;
                        };
                    reqFTP.Credentials = new NetworkCredential(clientSetting.mvnoSettings.fTPSUserName, clientSetting.mvnoSettings.fTPSPassword);
                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();

                    mimeType = ReturnExtension(FileName);
                    fileDownloadName = FileName;
                    return File(response.GetResponseStream(), mimeType, fileDownloadName);

                
                }
                else
                {
                    CRMLogger.WriteMessage("", "", "UNC Download start");
                
                    Credentials = new NetworkCredential(clientSetting.mvnoSettings.UNCUserName, clientSetting.mvnoSettings.UNCPassword, clientSetting.mvnoSettings.UNCDomain);
                    using (new NetworkConnection(@clientSetting.mvnoSettings.UNCFilePath, Credentials))
                    {
                        path = clientSetting.mvnoSettings.UNCFilePath;
                        path = path.Replace("\\", "//");
                        theFolders = System.IO.File.ReadAllBytes(path + '/' + FileName);
                        return File(theFolders, System.Net.Mime.MediaTypeNames.Application.Octet, FileName);
                    }
                }


            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                theFolders = System.Text.Encoding.ASCII.GetBytes("Internal Error");
                return File(theFolders, "application/pdf", "UnableToDownload.pdf");
            }

        }

        #endregion

        #region FTP PAth Move and Get

        public bool MovefileFTP(string localFile, string[] strFileName, string Deletefilename, CRMBase Input, string Description)
        {
            string host = string.Empty;
            try
            {
                if (clientSetting.mvnoSettings.sftpUserName != "" && clientSetting.mvnoSettings.sftpPassword != "")
                {
                    string FolderName = SettingsCRM.countryCode;
                    host = clientSetting.mvnoSettings.sftpServerIp;
                    host = host.Replace("/", string.Empty);
                    Sftp sftp = new Sftp(host, clientSetting.mvnoSettings.sftpUserName, clientSetting.mvnoSettings.sftpPassword);
                    try
                    {
                        string remoteFile_SFTP = "";
                        sftp.Connect(Convert.ToInt32(clientSetting.mvnoSettings.sftpPort.Trim()));
                        string registrationDocpath = clientSetting.mvnoSettings.sftpFilePath;

                        #region Delete
                        //JSch jsch = new JSch();
                        //Session session = jsch.getSession(clientSetting.mvnoSettings.sftpUserName, host, 22);
                        //Channel channel = session.openChannel("sftp");
                        //ChannelSftp ChannelSftp = (ChannelSftp)channel;
                        //string[] fileformat = clientSetting.mvnoSettings.ticketAttachFileformat.Split(',');
                        //for (int k = 0; k < 3; k++)
                        //{
                        //    for (int j = 0; j < fileformat.Length; j++)
                        //    {
                        //        fileformat[j] = fileformat[j].Replace(".", string.Empty).Trim();
                        //        ChannelSftp.rm(registrationDocpath + "/" + Deletefilename + (k + 1) + "." + fileformat[j]);
                        //    }
                        //}

                        #endregion

                        #region Move
                        if (strFileName.Count() > 0)
                        {
                            for (int i = 0; i < strFileName.Count(); i++)
                            {
                                if (strFileName[i] != null)
                                {
                                    if (strFileName[i] != "")
                                    {
                                        string strlocalFile = string.Empty;
                                        remoteFile_SFTP = registrationDocpath + "/" + strFileName[i].Trim();
                                        strlocalFile = localFile + strFileName[i].Trim();
                                        sftp.Put(strlocalFile, remoteFile_SFTP);
                                        if (clientSetting.countryCode.ToUpper() != "POL")
                                        {
                                            if ((System.IO.File.Exists(strlocalFile)))
                                            {
                                                System.IO.File.Delete(strlocalFile);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Put File Success");
                    }
                    catch (Exception ex)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                        return false;
                    }
                    finally
                    {
                        sftp.Close();
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return false;
            }
            return true;
        }

        public bool GetfileFTP(string targetPath, string[] strFileName, CRMBase Input, string Description)
        {
            string host = string.Empty;
            try
            {
                string TargetDocpath = clientSetting.mvnoSettings.sftpFilePath;
                // TargetDocpath = clientSetting.mvnoSettings.enableFTPS.ToUpper() == "TRUE" ? clientSetting.mvnoSettings.fTPSHost : clientSetting.mvnoSettings.sftpFilePath;
                if (!Directory.Exists(TargetDocpath))
                {
                    Directory.CreateDirectory(TargetDocpath);
                }

                #region Create path
                for (int i = 0; i < strFileName.Count(); i++)
                {
                    if (strFileName[i] != null && strFileName[i] != "")
                    {
                        if (clientSetting.preSettings.enableSFTP.ToUpper() == "FALSE" && clientSetting.mvnoSettings.enableFTPS.ToUpper() == "FALSE")
                        {
                            TargetDocpath = "";
                            TargetDocpath = clientSetting.mvnoSettings.sftpFilePath + "\\" + strFileName[i].Trim();
                            if (System.IO.File.Exists(TargetDocpath))
                            {
                                System.IO.File.Copy(TargetDocpath, targetPath + "/" + strFileName[i].Trim());
                            }
                        }
                        if (clientSetting.mvnoSettings.enableFTPS.ToUpper() == "TRUE")
                        {
                            // FileUploadFTPSServer(targetPath + "\\", strFileName[i].Trim(), Input);
                            FileDownloadFTPSServer(targetPath + "\\", strFileName[i].Trim(), Input);
                        }
                    }
                }
                #endregion

                if (clientSetting.preSettings.enableSFTP.ToUpper() == "TRUE" && clientSetting.mvnoSettings.enableFTPS.ToUpper() == "FALSE")
                {
                    #region SFTP
                    host = clientSetting.mvnoSettings.sftpServerIp;
                    host = host.Replace("/", string.Empty);
                    Sftp sftp = new Sftp(host, clientSetting.mvnoSettings.sftpUserName, clientSetting.mvnoSettings.sftpPassword);
                    try
                    {
                        string remoteFile_SFTP = "";
                        sftp.Connect(Convert.ToInt32(clientSetting.mvnoSettings.sftpPort.Trim()));

                        string registrationDocpath = clientSetting.mvnoSettings.sftpFilePath;
                        for (int i = 0; i < strFileName.Count(); i++)
                        {
                            if (strFileName[i] != null && strFileName[i] != "")
                            {
                                remoteFile_SFTP = registrationDocpath + "/" + strFileName[i].Trim();
                                if (!System.IO.File.Exists(remoteFile_SFTP))
                                {
                                    sftp.Get(remoteFile_SFTP, targetPath + "/" + strFileName[i].Trim());
                                }
                            }
                        }
                        CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Get File Success");
                    }
                    catch (Exception ex)
                    {
                        CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                        return false;
                    }
                    finally
                    {
                        sftp.Close();
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return false;
            }
            return true;
        }

        #endregion





        [HttpPost]
        public JsonResult CRMValidateSubscriber_Prepaid(string Registration)
        {


            ValidatePreSubscriberReq objReg = JsonConvert.DeserializeObject<ValidatePreSubscriberReq>(Registration);
            ValidatePreSubscriberRes objRes = new ValidatePreSubscriberRes();

            try
            {
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                objRes = crmNewService.CRMValidatePreSubscriber(objReg);


                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {

                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ValidateSubscriber_" + objRes.responseDetails.ResponseCode);
                    objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
                }
            }
            catch (Exception ex)
            {
                objRes = new ValidatePreSubscriberRes();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.responseDetails = new CRMResponse();
                objRes.responseDetails.ResponseCode = "2";
                objRes.responseDetails.ResponseDesc = ex.ToString();
            }

            return Json(objRes);
        }

        #region Macedonia

        public ActionResult SubscriberRegistration_MKD(string RegisterMsisdn)
        {
            Registration_MKD IRLobjres = new Registration_MKD();
            try
            {
                IRLobjres.MKDCity = Utility.GetDropdownMasterFromDB("", Convert.ToString(Session["isPrePaid"]), "tbl_city");
                //IRLobjres.MKDCity = Utility.DataTableToDictionary(Utility.ExecuteAccessDataTable(Utility.crmMDBConn, "SELECT Value,text FROM MKD_Muncipality where CityId = 5"), "Value", "text");
                IRLobjres.strDropdown = Utility.GetDropdownMasterFromDB("1,4,36,37,38,39,40", Convert.ToString(Session["isPrePaid"]), "drop_master");
                IRLobjres.Mode = "Insert";
                Session["PAType"] = null;
                IRLobjres.GetMacedonia = null;
            }

            catch (Exception ex)
            {
                //List<DropdownIRL> NLDDrpdown = Utility.ExecuteToListIRL(Utility.crmMDBConn, "select table_master_id,value,text," + clientSetting.langCode + " from prepaid_dropdown_master where table_master_id in (1,2,3,4,5,6,7,8,10,15,16)");
                //IRLobjres.strDropdown = NLDDrpdown;
                //CRMLogger.WriteException(Session["UserName"].ToString(), this.ControllerContext, ex);
            }
            return View("SubscriberRegistration_MKD", IRLobjres);
        }

        [HttpPost]
        public JsonResult CRMMACRegisterSubscriber(string Registration)
        {
            MKDRegisterSubscriberReq objReg = JsonConvert.DeserializeObject<MKDRegisterSubscriberReq>(Registration);
            MKDRegisterSubscriberRes objRes = new MKDRegisterSubscriberRes();
            try
            {
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                string DOB = Utility.GetDateconvertion(objReg.DateOfBirth, clientSetting.mvnoSettings.dateTimeFormat, false, "DD/MM/YYYY");
                if (DOB != "")
                {
                    string[] DOBSplit = DOB.Split('/');
                    if (DOBSplit.Length >= 2)
                    {

                        objReg.Birthdd = DOBSplit[0];
                        objReg.Birthmm = DOBSplit[1];
                        objReg.Birthyy = DOBSplit[2];
                    }
                }
                objReg.DateOfBirth = "";
                objRes = crmNewService.CRMMACRegisterSubscriber(objReg);
                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {
                    if (!_runningFromNUnit)
                    {
                        if (objReg.Mode == "INSERT")
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_MKD_" + objRes.responseDetails.ResponseCode);
                            objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                        else
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ERegEdit_MKD_" + objRes.responseDetails.ResponseCode);
                            objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                objRes = new MKDRegisterSubscriberRes();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                objRes.responseDetails = new CRMResponse();
                objRes.responseDetails.ResponseCode = "2";
                objRes.responseDetails.ResponseDesc = ex.ToString();
            }

            return Json(objRes);
        }

        public ActionResult SubscriberRegistrationView_MKD()
        {
            MKDRegisterSubscriberReq objReg = new MKDRegisterSubscriberReq();
            MKDRegisterSubscriberRes objRes = new MKDRegisterSubscriberRes();

            try
            {

                objRes = CRMGetSubscriberMKD();
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objRes);
        }


        public MKDRegisterSubscriberRes CRMGetSubscriberMKD()
        {
            MKDRegisterSubscriberRes objRes = new MKDRegisterSubscriberRes();
            MKDRegisterSubscriberReq ObjReq = new MKDRegisterSubscriberReq();
            try
            {

                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode;
                ObjReq.Mode = "GET";
                ObjReq.MSISDN = Session["MobileNumber"].ToString();
                objRes = crmNewService.CRMMACRegisterSubscriber(ObjReq);
                if (objRes != null && objRes.responseDetails != null && objRes.responseDetails.ResponseCode != null)
                {
                    if (!_runningFromNUnit)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("ValidateSubscriber_" + objRes.responseDetails.ResponseCode);
                        objRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? objRes.responseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return objRes;
        }

        public ActionResult SubscriberRegistrationEdit_MKD(string RegisterMsisdn)
        {
            Registration_MKD IRLobjres = new Registration_MKD();
            try
            {
                IRLobjres.strDropdown = Utility.GetDropdownMasterFromDB("1,4,36,37,38,39,40", Convert.ToString(Session["isPrePaid"]), "drop_master");
                IRLobjres.MKDCity = Utility.GetDropdownMasterFromDB("", Convert.ToString(Session["isPrePaid"]), "tbl_city");
                IRLobjres.Mode = "Edit";
                Session["PAType"] = null;
                ViewBag.Title = "Subscriber Edit";
                IRLobjres.GetMacedonia = CRMGetSubscriberMKD();


                //IRLobjres.MKDCity = Utility.DataTableToDictionary(Utility.ExecuteAccessDataTable(Utility.crmMDBConn, "exec Select_MKDCity"), "Value", "text");
                if (IRLobjres.GetMacedonia.GetMacedonia.Municipality != null)
                {
                    IRLobjres.MKDMuncipality = Utility.DataTableToDictionary(Utility.GetDropdownMasterFromDB(IRLobjres.GetMacedonia.GetMacedonia.CityName.Trim(), Convert.ToString(Session["isPrePaid"]), "tbl_muncipality"));
                }
                if (IRLobjres.GetMacedonia.GetMacedonia.place != null)
                {
                    IRLobjres.MKDplace = Utility.DataTableToDictionary(Utility.GetDropdownMasterFromDB(IRLobjres.GetMacedonia.GetMacedonia.Municipality.Trim(), Convert.ToString(Session["isPrePaid"]), "tbl_place"));
                }

            }
            catch (Exception ex)
            {
                IRLobjres.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,5,6,7,8,10,15,16", Convert.ToString(Session["isPrePaid"]), "drop_master");
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View("SubscriberRegistration_MKD", IRLobjres);
        }


        public string GetLoadMuncipality(string MuncipalityID)
        {
            Registration_MKD IRLobjres = new Registration_MKD();
            string strques = string.Empty;
            string SelectTitles = SIMResources.ResourceManager.GetString("Select");
            try
            {
                IRLobjres.MKDCity = Utility.GetDropdownMasterFromDB(MuncipalityID, Convert.ToString(Session["isPrePaid"]), "tbl_muncipality");

                strques = strques + "<option title='" + SelectTitles + "' value=''>" + SelectTitles + "</option>";
                foreach (DropdownMaster mkdmuncipality in IRLobjres.MKDCity)
                {
                    strques = strques + "<option title='" + mkdmuncipality.Value + "' value='" + mkdmuncipality.ID + "'>" + mkdmuncipality.Value + "</option>";
                }

            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            return strques;
        }


        public string GetLoadPlace(string PlaceID)
        {
            Registration_MKD IRLobjres = new Registration_MKD();
            string strques = string.Empty;
            string SelectTitles = SIMResources.ResourceManager.GetString("Select");
            try
            {
                IRLobjres.MKDCity = Utility.GetDropdownMasterFromDB(PlaceID, Convert.ToString(Session["isPrePaid"]), "tbl_place");

                strques = strques + "<option title='" + SelectTitles + "' value=''>" + SelectTitles + "</option>";
                foreach (DropdownMaster mkdmuncipality in IRLobjres.MKDCity)
                {
                    strques = strques + "<option title='" + mkdmuncipality.Value + "' value='" + mkdmuncipality.ID + "'>" + mkdmuncipality.Value + "</option>";
                }

            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            return strques;
        }

        #endregion

        #region Toggle Reg

        public ActionResult SubscriberRegistration_Toggle(string RegisterMsisdn, string Mode)
        {
            CRM.Models.Registration_GBR objRegistration = new CRM.Models.Registration_GBR();
            ServiceCRM.GBRRegistration objReq = new ServiceCRM.GBRRegistration();
            ServiceCRM.ContactAddress commAdd = new ServiceCRM.ContactAddress();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
                string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                objRegistration.CountryDateFormat = Datestring;
                objRegistration.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                //objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,10", objRegistration.IsPostpaid, "drop_master"); 
                objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,10", objRegistration.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "TblCountry")).ToList();
                objRegistration.MSISDN = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                objRegistration.DateCalender = Datestring.ToLower().Replace("yyyy", "yy");
                objRegistration.strMode = "INSERT";
                objReq.CommAddress = commAdd;
                objRegistration.GBRGetSubscribReq = objReq;
                string filepath = clientSetting.preSettings.registrationDocpath;
                if (Directory.Exists(filepath))
                {
                    Directory.Delete(filepath, true);
                }
                ViewBag.Title = @Resources.RegistrationResources.GBRRegistration;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View(objRegistration);
        }

        public JsonResult SaveResgistration_Toggle_GBR(string Registration)
        {
            CRMResponse objRes = new CRMResponse();
            ServiceCRM.GBRRegistration objReq = new ServiceCRM.GBRRegistration();
            string Strmsisdn = Convert.ToString(Session["MobileNumber"]);
            try
            {
                Service.GBRRegistration objReg = JsonConvert.DeserializeObject<Service.GBRRegistration>(Registration);
                objReg.CountryDateFormat = clientSetting.mvnoSettings.dateTimeFormat;
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                StringBuilder sb = new StringBuilder();
                objReg.DateOfBirth = Utility.GetDateconvertion(objReg.DateOfBirth, "DD/MM/YYYY", true, clientSetting.mvnoSettings.dateTimeFormat);
                objReg.SecretQuestion = string.Empty;
                objReg.SecretAnswer = string.Empty;
                objReg.CSAgent = Convert.ToString(Session["UserName"]);

                if (string.IsNullOrEmpty(Strmsisdn))
                {
                    objReg.ModeReg = "INSERT";
                }
                else
                {
                    objReg.ModeReg = "UPDATE";
                }
                objRes = crmService.InsertGBR(objReg);
                switch (objRes.ResponseCode)
                {
                    case "0":
                        if (objReg.ModeReg == "UPDATE")
                        {
                            objRes.ResponseDesc = Resources.ErrorResources.EUpdat_AUT_0;
                        }
                        else
                        {
                            objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR0;
                        }
                        break;
                    case "1":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR1;
                        break;
                    case "2":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR1;
                        break;
                    case "3":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR3;
                        break;
                    case "4":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR4;
                        break;
                    case "7":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR7;
                        break;
                    case "40":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR40;
                        break;
                    case "110":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR110;
                        objRes.ResponseCode = "0";
                        break;
                    case "111":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR111;
                        objRes.ResponseCode = "0";
                        break;
                    case "112":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR112;
                        objRes.ResponseCode = "0";
                        break;
                    case "100":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR100;
                        objRes.ResponseCode = "0";
                        break;
                    case "113":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR_113;
                        objRes.ResponseCode = "0";
                        break;
                    case "114":
                        objRes.ResponseDesc = Resources.ErrorResources.EReg_GBR_114;
                        objRes.ResponseCode = "0";
                        break;
                    default:
                        objRes.ResponseDesc = Resources.ErrorResources.Common_2;
                        break;
                }
                if (objReg.ModeReg == "UPDATE" && (objRes.ResponseCode == "0" || objRes.ResponseCode == "100" || objRes.ResponseCode == "110" || objRes.ResponseCode == "111" || objRes.ResponseCode == "112" || objRes.ResponseCode == "113" || objRes.ResponseCode == "114"))
                {
                    Session["SubscriberTitle"] = objReg.Title;
                    Session["SubscriberName"] = objReg.FirstName + "|" + objReg.LastName;
                }

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return Json(objRes);
        }
        #endregion


        #region  Edit_Toggle_GBR
        public ActionResult SubscriberRegistrationEdit_Toggle_GBR(string RegisterMsisdn, string Mode)
        {
            CRM.Models.Registration_GBR objRegistration = new CRM.Models.Registration_GBR();
            GBRGetSubscriberResponse objGetViewResp = new GBRGetSubscriberResponse();
            GBRGetSubscriberRequest objGetViewReq = new GBRGetSubscriberRequest();
            ViewBag.Title = @Resources.RegistrationResources.GBRRegistration;
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
                string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                objRegistration.CountryDateFormat = Datestring;
                objRegistration.IsPostpaid = Session["isPrePaid"] != null ? Convert.ToString(Session["isPrePaid"]) : "1";
                //  objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,10", objRegistration.IsPostpaid, "drop_master");
                objRegistration.strDropdown = Utility.GetDropdownMasterFromDB("1,10", objRegistration.IsPostpaid, "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, objRegistration.IsPostpaid, "TblCountry")).ToList();
                objRegistration.MSISDN = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                objRegistration.DateCalender = Datestring.ToLower().Replace("yyyy", "yy");
                if ((!string.IsNullOrEmpty(Mode)) && Mode.ToUpper().Trim() == "UPDATE")
                {
                    CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, RegisterMsisdn);
                    objGetViewReq.CountryCode = clientSetting.countryCode;
                    objGetViewReq.BrandCode = clientSetting.brandCode;
                    objGetViewReq.LanguageCode = clientSetting.langCode;
                    objGetViewReq.MSISDN = !string.IsNullOrEmpty(RegisterMsisdn) ? RegisterMsisdn : string.Empty;
                    serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                    
                        objGetViewResp = serviceCRM.ViewSubscribeGBR(objGetViewReq);
                        objRegistration.strMode = "UPDATE";
                        objRegistration.GBRGetSubscribReq = objGetViewResp.GBRGetSubscriber;
                    
                }
                ViewBag.Title = @Resources.RegistrationResources.GBREditReg;
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return View("SubscriberRegistration_Toggle", objRegistration);
        }
        #endregion


        #region VIEWGBR
        public ActionResult SubscriberRegistrationView_Toggle_GBR(string RegisterMsisdn, string Mode)
        {
            CRM.Models.Registration_GBR objRegistration = new CRM.Models.Registration_GBR();
            GBRGetSubscriberRequest objGetViewReq = new GBRGetSubscriberRequest();
            GBRGetSubscriberResponse objGetViewResp = new GBRGetSubscriberResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {

                objGetViewReq.CountryCode = clientSetting.countryCode;
                objGetViewReq.BrandCode = clientSetting.brandCode;
                objGetViewReq.LanguageCode = clientSetting.langCode;
                objGetViewReq.MSISDN = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    objGetViewResp = serviceCRM.ViewSubscribeGBR(objGetViewReq);
                

                if (objGetViewResp.reponseDetails.ResponseCode == "0")
                {
                    objRegistration.GBRGetSubscribReq = objGetViewResp.GBRGetSubscriber;
                    objRegistration.ResponseCode = objGetViewResp.reponseDetails.ResponseCode;
                    objRegistration.ResponseDesc = objGetViewResp.reponseDetails.ResponseDesc;
                }

            }
            catch (Exception Ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, Ex);
            }
            finally
            {
                serviceCRM = null;

            }
            return View(objRegistration);
        }
        #endregion

        #region Belgium PhaseII
        public JsonResult CRMBelgiumValidation(string BELValidatelist)
        {
            BELValidateRes ObjRes = new BELValidateRes();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                BELValidateRequest ObjReq = JsonConvert.DeserializeObject<BELValidateRequest>(BELValidatelist);
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode.ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMBELValidation(ObjReq);
                    if (ObjRes != null && ObjRes.responseDetails != null && (ObjRes.responseDetails.ResponseCode == "0" || ObjRes.responseDetails.ResponseCode == "6"))
                    {
                        Session["MobileNumber"] = ObjRes.MSISDN;
                        Session["AccountNumber"] = ObjRes.AccountID;
                    }
                    if (ObjRes != null && ObjRes.responseDetails != null)
                    {
                        if (!string.IsNullOrEmpty(ObjRes.responseDetails.ResponseDesc))
                        {
                            string Errordescription = ObjRes.responseDetails.ResponseDesc;
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_BELVal_" + ObjRes.responseDetails.ResponseCode);
                            ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;
                        }
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
            return Json(ObjRes);
        }

        public ActionResult SubscriberRegistration_BEL(string RegisterMsisdn)
        {
            //Registration_BELGIUM BELobjres = new Registration_BELGIUM();
            Models.Registration_BEL BELobjres = new Models.Registration_BEL();
            try
            {
                //BELobjres.strDropdown = Utility.GetDropdownMasterFromDB("1,4", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB("", "1", "TblCountry")).ToList(); ;

                BELobjres.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,5,6,7,8,10,67", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, "0", "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, "0", "Tbl_Nationality")).ToList();

                BELRegisterSubscriber BELRegSubc = new BELRegisterSubscriber();
                BELRegSubc.Msisdn = RegisterMsisdn != null ? RegisterMsisdn : string.Empty;
                BELobjres.BELRegSubscriber = BELRegSubc;
                BELobjres.Mode = "INSERT";

                BELobjres.DocFormat = clientSetting.mvnoSettings.documentFormat.ToUpper();
                BELobjres.Filesize = clientSetting.mvnoSettings.attachFileSize;

                string Datestring = clientSetting.mvnoSettings.dateTimeFormat.ToUpper();
                BELobjres.CountryDateFormat = Datestring;

                int iDocCount = (clientSetting.mvnoSettings.signedDoc != null && Convert.ToString(clientSetting.mvnoSettings.signedDoc).ToUpper() == "TRUE") ? 1 : 0;
                string ErrorMsg = Resources.ErrorResources.EReg_ITA_doc3;

                if ((clientSetting.mvnoSettings.isAttachmentMand.ToUpper() == "TRUE") || (clientSetting.mvnoSettings.isAttachmentMand.ToUpper() == "TRUE"))
                {
                    if (clientSetting.mvnoSettings.mandatoryDoc != null && Convert.ToInt16(clientSetting.mvnoSettings.mandatoryDoc) > 0)
                    {
                        if (iDocCount == 1)
                            ErrorMsg = string.Format(Resources.ErrorResources.EReg_ITA_doc2, Convert.ToInt16(clientSetting.mvnoSettings.mandatoryDoc));
                        else
                            ErrorMsg = string.Format(Resources.ErrorResources.EReg_ITA_doc1, Convert.ToInt16(clientSetting.mvnoSettings.mandatoryDoc));

                        iDocCount = iDocCount + Convert.ToInt16(clientSetting.mvnoSettings.mandatoryDoc);
                    }
                    BELobjres.DocDetails = iDocCount.ToString() + "|" + ErrorMsg;
                }
                else
                {
                    BELobjres.DocDetails = "0|";
                }


                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;


                string registrationDocpath = clientSetting.preSettings.registrationDocpath;
                if (!Directory.Exists(registrationDocpath))
                {
                    Directory.CreateDirectory(registrationDocpath);
                }
                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    string UploadFile = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    string UploadFile = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);

                    if (!Directory.Exists(UploadFile))
                    {
                        Directory.CreateDirectory(UploadFile);
                    }
                }

                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    string PdfDownload = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (!Directory.Exists(PdfDownload))
                    {
                        Directory.CreateDirectory(PdfDownload);
                    }
                }



                //if (Server.MapPath("~/App_Data/UploadFile") != null)
                //{
                //    var filepath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
                if (clientSetting.mvnoSettings.internalUploadFile != null)
                {
                    var filepath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);

                    if (Directory.Exists(filepath))
                    {
                        System.IO.DirectoryInfo di = new DirectoryInfo(filepath);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                    }
                }
                if (clientSetting.mvnoSettings.internalPdfDownload != null)
                {
                    var filepath2 = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                    if (Directory.Exists(filepath2))
                    {
                        System.IO.DirectoryInfo di = new DirectoryInfo(filepath2);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                    }
                }
                // set Session["PAType"] to null - purpose-not to redirect PendingApproval often
                Session["PAType"] = null;
                ViewBag.Title = "Subscriber Registration";
            }
            catch (Exception ex)
            {
                BELobjres.strDropdown = Utility.GetDropdownMasterFromDB("1,2,3,4,5,6,7,8,10,67", "1", "drop_master").Concat(Utility.GetDropdownMasterFromDB(string.Empty, "0", "TblCountry")).ToList().Concat(Utility.GetDropdownMasterFromDB(string.Empty, "0", "Tbl_Nationality")).ToList();
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            return View("SubscriberRegistration_BEL", BELobjres);
        }

        public JsonResult CRMRegisterSubscriberBelgium(string Registration)
        {
            BELValidateRes ObjRes = new BELValidateRes();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                BELRegisterSubscriber objReg = JsonConvert.DeserializeObject<BELRegisterSubscriber>(Registration);
                objReg.CountryCode = clientSetting.countryCode;
                objReg.BrandCode = clientSetting.brandCode;
                objReg.LanguageCode = clientSetting.langCode;
                objReg.CSAgent = Convert.ToString(Session["UserName"]);
                objReg.AccountNumber = Convert.ToString(Session["AccountNumber"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMRegisterSubscriberBelgium(objReg);
                


                if (objReg.Mode.ToUpper() == "UPDATE")
                {
                    if (ObjRes != null && ObjRes.responseDetails != null)
                    {
                        if (!string.IsNullOrEmpty(ObjRes.responseDetails.ResponseDesc))
                        {
                            if (ObjRes.responseDetails.ResponseCode == "0")
                            {
                                ObjRes.responseDetails.ResponseDesc = Resources.ErrorResources.ResourceManager.GetString("EReg_BELUpd_" + ObjRes.responseDetails.ResponseCode);
                            }
                            else
                            {
                                ObjRes.responseDetails.ResponseDesc = ObjRes.responseDetails.ResponseDesc;
                            }
                        }
                    }
                }
                else
                {//Registration case

                    if (ObjRes != null && ObjRes.responseDetails != null)
                    {
                        if (!string.IsNullOrEmpty(ObjRes.responseDetails.ResponseDesc))
                        {
                            // ObjRes.responseDetails.ResponseDesc = Resources.ErrorResources.ResourceManager.GetString("EReg_BEL_" + ObjRes.responseDetails.ResponseCode);
                            string Errordescription = ObjRes.responseDetails.ResponseDesc;
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("EReg_BEL_" + ObjRes.responseDetails.ResponseCode);
                            ObjRes.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.responseDetails.ResponseDesc : errorInsertMsg;
                        }
                    }

                    //if ((clientSetting.preSettings.belEnableFileUpload.ToUpper() == "TRUE" || clientSetting.preSettings.belEnableFileUpload.Trim() == "1") && (objReg.Doc1 != null && objReg.Doc1 != ""))
                    //{

                    #region Change Uploaded File Path

                    if (ObjRes != null && ObjRes.responseDetails != null)
                    {
                        if (ObjRes.responseDetails.ResponseCode == "0" || ObjRes.responseDetails.ResponseCode == "100" || ObjRes.responseDetails.ResponseCode == "110" || ObjRes.responseDetails.ResponseCode == "111" || ObjRes.responseDetails.ResponseCode == "112" || ObjRes.responseDetails.ResponseCode == "113" ||
                            ObjRes.responseDetails.ResponseCode == "114" || ObjRes.responseDetails.ResponseCode == "99" || ObjRes.responseDetails.ResponseCode == "64" || ObjRes.responseDetails.ResponseCode == "66" || ObjRes.responseDetails.ResponseCode == "67")
                        {

                            string FileName = "";
                            if ((!string.IsNullOrEmpty(objReg.Doc1)) || (!string.IsNullOrEmpty(objReg.Doc2)) || (!string.IsNullOrEmpty(objReg.Doc3)))
                            {
                                string Externsion = string.Empty;
                                string SourceDocpath = clientSetting.preSettings.registrationDocpath;
                                if (!Directory.Exists(SourceDocpath))
                                {
                                    Directory.CreateDirectory(SourceDocpath);
                                }

                                //FileName = ObjRes.AccountID + "_Doc1." + objReg.Doc1.Split('.')[1];
                                fileupload(FileName, objReg, ObjRes.AccountID);
                            }
                        }
                    }
                    //if (!string.IsNullOrEmpty(objReg.Doc2))
                    //{
                    //    FileName = ObjRes.AccountID + "_Doc2." + objReg.Doc2.Split('.')[1];
                    //    fileupload(FileName, objReg, sourcePath, ObjRes.AccountID);
                    //}
                    //if (!string.IsNullOrEmpty(objReg.Doc3))
                    //{
                    //    FileName = ObjRes.AccountID + "_Doc3." + objReg.Doc3.Split('.')[1];
                    //    fileupload(FileName, objReg, sourcePath, ObjRes.AccountID);
                    //}                       
                    Session["FilePath"] = string.Empty;

                    #endregion
                    //}

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
            return Json(ObjRes);
        }

        public void fileupload(string FileName, BELRegisterSubscriber objReg, string AccountNo)
        {

            string sourcePath = string.Empty;
            string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
            //if (Server.MapPath("~/App_Data/UploadFile") != null)
            //{
            //    sourcePath = Path.Combine(Server.MapPath("~/App_Data/UploadFile"), FolderName);
            if (clientSetting.mvnoSettings.internalUploadFile != null)
            {
                sourcePath = Path.Combine(clientSetting.mvnoSettings.internalUploadFile, FolderName);

            }

            string targetPath = clientSetting.preSettings.registrationDocpath;
            string Externsion = string.Empty;

            string filename = string.Empty;

            #region Move
            //if (Server.MapPath("~/App_Data/UploadFile") != null)
            //{
            if (clientSetting.mvnoSettings.internalUploadFile != null)
            {
                if (Directory.GetFiles(sourcePath).Count() > 0)
                {
                    string[] fileEntries2 = Directory.GetFiles(sourcePath);
                    string[] filenameList = new string[3];

                    string Deletefilename = string.Empty;
                    for (int i = 0; i < fileEntries2.Count(); i++)
                    {
                        Externsion = fileEntries2[i].Substring(fileEntries2[i].LastIndexOf("\\") + 1);
                        string[] splitExternsion = Externsion.Split('.');
                        filename = AccountNo + "_" + (i + 1) + "." + splitExternsion[1];

                        #region Delete
                        try
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                string[] fileformat = clientSetting.mvnoSettings.documentFormat.Split(',');
                                for (int j = 0; j < fileformat.Length; j++)
                                {
                                    fileformat[j] = fileformat[j].Replace(".", string.Empty).Trim();
                                    System.IO.File.Delete(targetPath + "\\" + filename);
                                }
                            }
                        }
                        catch
                        {

                        }
                        #endregion

                        System.IO.File.Copy(sourcePath + "\\" + Externsion, targetPath + "\\" + filename);
                        filenameList[i] = filename.ToString();
                        Deletefilename = filename;
                        if (clientSetting.preSettings.enableSFTP.ToUpper() == "FALSE" && clientSetting.mvnoSettings.enableFTPS.ToUpper() == "FALSE")
                        {

                            #region Create path

                            string TargetDocpath = clientSetting.mvnoSettings.sftpFilePath;
                            if (!Directory.Exists(TargetDocpath))
                            {
                                Directory.CreateDirectory(TargetDocpath);
                            }

                            #endregion
                            System.IO.File.Copy(clientSetting.preSettings.registrationDocpath + "\\" + filename, clientSetting.mvnoSettings.sftpFilePath + "\\" + filename, true);
                        }
                        if (clientSetting.mvnoSettings.enableFTPS.ToUpper() == "TRUE")
                        {
                            FileUploadFTPSServer(targetPath + "\\", filename, objReg);
                        }

                    }

                    if (clientSetting.preSettings.enableSFTP.ToUpper() == "TRUE" && clientSetting.mvnoSettings.enableFTPS.ToUpper() == "FALSE")
                    {
                        MovefileFTP(targetPath + "\\", filenameList, Deletefilename, objReg, "");
                        //FileUploadFTPSServer(targetPath + "\\", filenameList, objReg);
                    }


                }

                System.IO.File.Delete(clientSetting.preSettings.registrationDocpath + "\\" + filename);
            }

            try
            {
                //Delete file in appdata
                string[] fileEntries3 = Directory.GetFiles(sourcePath);
                for (int i = 0; i < fileEntries3.Count(); i++)
                {

                    Externsion = fileEntries3[i].Substring(fileEntries3[i].LastIndexOf("\\") + 1);
                    System.IO.File.Delete(sourcePath + "\\" + Externsion);
                    if (clientSetting.mvnoSettings.internalPdfDownload != null)
                    {
                        string delPath = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                        if (Directory.Exists(delPath))
                        {
                            System.IO.DirectoryInfo di = new DirectoryInfo(delPath);
                            foreach (FileInfo file in di.GetFiles())
                            {
                                file.Delete();
                            }
                            //   Directory.Delete(delPath, true);
                        }
                    }
                }
            }
            catch
            {

            }


            #endregion
        }

        public JsonResult CRMBelgiumOTP(string BelgiumOTP)
        {
            BelgiumOTPRes ObjRes = new BelgiumOTPRes();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                BelgiumOTPReq ObjReq = JsonConvert.DeserializeObject<BelgiumOTPReq>(BelgiumOTP);
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode.ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMBelgiumOTP(ObjReq);

                    //ObjReq.Mode!="OTP"
                    if (ObjRes.reponseDetails != null)
                    {
                        string Errordescription = ObjRes.reponseDetails.ResponseDesc;
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString(ObjReq.Mode != "OTP" ? "BEL_LINK_" : "BEL_OTP_" + ObjRes.reponseDetails.ResponseCode);
                        ObjRes.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjRes.reponseDetails.ResponseDesc : errorInsertMsg;
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
            return Json(ObjRes);
        }

        public JsonResult CRMGetSubscriberBEL(string BELGet)
        {
            CustomerDetailBELResponse ObjRes = new CustomerDetailBELResponse();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                CustomerDetailsBELRequest ObjReq = JsonConvert.DeserializeObject<CustomerDetailsBELRequest>(BELGet);
                ObjReq.CountryCode = clientSetting.countryCode;
                ObjReq.BrandCode = clientSetting.brandCode;
                ObjReq.LanguageCode = clientSetting.langCode.ToString();
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjRes = serviceCRM.CRMGetSubscriberBEL(ObjReq);
                

            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
            finally
            {
                serviceCRM = null;
            }
            return Json(ObjRes);
        }
        #endregion

        #region Registration History FRR 3959
        public ActionResult RegistrationHistory_GER(string RegisterMsisdn)
        {
            Session["RegHistory"] = "History";
            return View("SubscriberRegistrationHistory_GER");
        }

        public JsonResult LoadRegistrationHistory(string reqDetails)
        {
            RegHistoryResponse ObjResp = new RegHistoryResponse();
            RegHistoryReq ObjReq = JsonConvert.DeserializeObject<RegHistoryReq>(reqDetails);
            string strInputDate = clientSetting.mvnoSettings.dateTimeFormat;
            //s
            ServiceInvokeCRM serviceCRM;
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
                    ObjReq.fromDate = strYear + "/" + strMonth + "/" + strDate;
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
                    ObjReq.toDate = strYear + "/" + strMonth + "/" + strDate;
                }
                #endregion

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjResp = serviceCRM.CRMRegistrationHistory(ObjReq);
                

                //FRR--3083
                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("LoadRegHistory_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }

                try
                {
                    ObjResp.RegReport.Where(a => a.DOB != string.Empty).ToList().ForEach(b => b.DOB = Utility.FormatDateTime(b.DOB, clientSetting.mvnoSettings.dateTimeFormat));
                    ObjResp.RegReport.Where(a => a.registereddate != string.Empty).ToList().ForEach(b => b.registereddate = Utility.FormatDateTime(b.registereddate, clientSetting.mvnoSettings.dateTimeFormat));
                    ObjResp.RegReport.Where(a => a.idIssueDate != string.Empty).ToList().ForEach(b => b.idIssueDate = Utility.FormatDateTime(b.idIssueDate, clientSetting.mvnoSettings.dateTimeFormat));
                    ObjResp.RegReport.Where(a => a.DATE_OF_EXPIRY != string.Empty).ToList().ForEach(b => b.DATE_OF_EXPIRY = Utility.FormatDateTime(b.DATE_OF_EXPIRY, clientSetting.mvnoSettings.dateTimeFormat));
                    ObjResp.RegReport.Where(a => a.updateddt != string.Empty).ToList().ForEach(b => b.updateddt = Utility.FormatDateTime(b.updateddt, clientSetting.mvnoSettings.dateTimeFormat));                    
                }
                catch
                {

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

        



        #region  --- DownLoadEposAppDocument --- FRR 4521 --- 

        public ActionResult DownLoadEposAppDocument(string filepath, string filename, string fileuploadtype)
        {
            NetworkCredential Credentials;
            string path = "";
            byte[] theFolders;
            string fileNameDisplayedToUser = string.Empty;
            try
            {
                if (fileuploadtype == "1")
                {
                    var ftpRequest = (FtpWebRequest)FtpWebRequest.Create(clientSetting.mvnoSettings.FTPFilepath + "/" + filepath + "/" + filename);
                    ftpRequest.Credentials = new NetworkCredential(clientSetting.mvnoSettings.FTPUserName, clientSetting.mvnoSettings.FTPPassword);
                    ftpRequest.UseBinary = true;
                    ftpRequest.UsePassive = true;
                    ftpRequest.KeepAlive = true;
                    ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                    // FIX in  PID 50227

                    ftpRequest.EnableSsl = true;

                    ServicePointManager.ServerCertificateValidationCallback =
                        delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                        {
                            return true;
                        };
                    //

                    var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                    var ftpStream = ftpResponse.GetResponseStream();
                    const string contentType = "application/pdf";
                    fileNameDisplayedToUser = filename;
                    return File(ftpStream, contentType, fileNameDisplayedToUser);
                }
                else
                {
                    Credentials = new NetworkCredential(clientSetting.mvnoSettings.UNCUserName, clientSetting.mvnoSettings.UNCPassword, clientSetting.mvnoSettings.UNCDomain);
                    using (new NetworkConnection(@clientSetting.mvnoSettings.UNCFilePath, Credentials))
                    {
                        path = clientSetting.mvnoSettings.UNCFilePath;
                        path = path.Replace("\\", "//");
                        theFolders = System.IO.File.ReadAllBytes(path + '/' + filepath + '/' + filename);
                        return File(theFolders, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
                    }
                }
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                theFolders = System.Text.Encoding.ASCII.GetBytes("Internal Error");
                return File(theFolders, "application/pdf", "UnableToDownload.pdf");
            }
            finally
            {
                filename = null;
                filepath = null;
                path = null;
                Credentials = null;
                theFolders = null;
                fileNameDisplayedToUser = string.Empty;
            }
        }
        #endregion


        #region FRR 4788
        public ActionResult DownLoadPostpaidinvoice(string filepath, string filename)
        {
            NetworkCredential Credentials;
            string path = "";
            byte[] theFolders;
            string fileNameDisplayedToUser = string.Empty;
            try
            {
                if (clientSetting.mvnoSettings.enableFileupload.ToUpper() != "TRUE")
                {

                var ftpRequest = (FtpWebRequest)FtpWebRequest.Create(filepath + "/" + filename);
                ftpRequest.Credentials = new NetworkCredential(clientSetting.mvnoSettings.FTPUserName, clientSetting.mvnoSettings.FTPPassword);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                // FIX in  PID 50227

                ftpRequest.EnableSsl = true;

                ServicePointManager.ServerCertificateValidationCallback =
                    delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                    {
                        return true;
                    };
                //

                var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                var ftpStream = ftpResponse.GetResponseStream();
                const string contentType = "application/pdf";
                fileNameDisplayedToUser = filename;
                return File(ftpStream, contentType, fileNameDisplayedToUser);
                }
                else
                {

                    Credentials = new NetworkCredential(clientSetting.mvnoSettings.UNCUserName, clientSetting.mvnoSettings.UNCPassword, clientSetting.mvnoSettings.UNCDomain);
                    using (new NetworkConnection(@clientSetting.mvnoSettings.UNCFilePath, Credentials))
                    {
                        path = clientSetting.mvnoSettings.UNCFilePath;
                        path = path.Replace("\\", "//");
                        path = path + "//" + filename;
                        theFolders = System.IO.File.ReadAllBytes(path);
                        return File(theFolders, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
                    }
                }



            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                theFolders = System.Text.Encoding.ASCII.GetBytes("Internal Error");
                return File(theFolders, "application/pdf", "UnableToDownload.pdf");

            }
            finally
            {
                filename = null;
                filepath = null;
                path = null;
                Credentials = null;
                theFolders = null;
                fileNameDisplayedToUser = string.Empty;
            }

        }
        #endregion


    


        #region -- NetworkConnection check For UNC Download path More Info Please review above code --- FRR 4521
        public class NetworkConnection : IDisposable
        {
            string name;

            public NetworkConnection(string networkName, NetworkCredential credentials)
            {
                name = networkName;
                

                var netResource = new NetResource()
                {
                    Scope = ResourceScope.GlobalNetwork,
                    ResourceType = ResourceType.Disk,
                    DisplayType = ResourceDisplaytype.Share,
                    RemoteName = networkName
                };
                var userName = string.IsNullOrEmpty(credentials.Domain)
               ? credentials.UserName
               : string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);
                var result = WNetAddConnection2(
                   netResource,
                   credentials.Password,
                   userName,
                   0);

                if (result != 0)
                {
                    throw new Win32Exception(result);
                }
            }
            ~NetworkConnection()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            protected virtual void Dispose(bool disposing)
            {
                WNetCancelConnection2(name, 0, true);
            }

            [DllImport("mpr.dll")]
            private static extern int WNetAddConnection2(NetResource netResource,
                string password, string username, int flags);

            [DllImport("mpr.dll")]
            private static extern int WNetCancelConnection2(string name, int flags,
                bool force);

        }

        [StructLayout(LayoutKind.Sequential)]
        public class NetResource
        {
            public ResourceScope Scope;
            public ResourceType ResourceType;
            public ResourceDisplaytype DisplayType;
            public int Usage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }
        public enum ResourceScope : int
        {
            Connected = 1,
            GlobalNetwork,
            Remembered,
            Recent,
            Context
        };
        public enum ResourceType : int
        {
            Any = 0,
            Disk = 1,
            Print = 2,
            Reserved = 8,
        }
        public enum ResourceDisplaytype : int
        {
            Generic = 0x0,
            Domain = 0x01,
            Server = 0x02,
            Share = 0x03,
            File = 0x04,
            Group = 0x05,
            Network = 0x06,
            Root = 0x07,
            Shareadmin = 0x08,
            Directory = 0x09,
            Tree = 0x0a,
            Ndscontainer = 0x0b
        }

        #endregion





        #region frr 4994 download pdf
        [ValidateInput(false)]
        public JsonResult SendEmailPostpaidcustomer(string HTML)
        {

            try
            {
                Document document = new Document(PageSize.A4, 50, 50, 20, 20);
                StyleSheet styles = new StyleSheet();
                styles.LoadTagStyle(HtmlTags.TABLE, "width", "90%");
                styles.LoadTagStyle(HtmlTags.TABLE, "align", "left");
                styles.LoadTagStyle(HtmlTags.TABLE, "cellpadding", "0");
                styles.LoadTagStyle(HtmlTags.TABLE, "cellspacing", "0");
                //string PDFfileName1 = DateTime.Now.ToString("ddMMyyyy_hh_mm_ss_fff") + ".pdf";
                //Session["PDFfileName1"] = PDFfileName1;
                //string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                //var filepath = "C:\\Resources\\superadmin_GBR";
                //string PDFfileName = filepath + "\\" + PDFfileName1;

                string PDFfileName1 = DateTime.Now.ToString("ddMMyyyy_hh_mm_ss_fff") + "_" + ".pdf";
                Session["PDFfileName1"] = PDFfileName1;

                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                var filepath = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                string PDFfileName = filepath + "\\" + PDFfileName1;
                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }
                PdfWriter.GetInstance(document, new FileStream(PDFfileName, FileMode.Create));
                document.Open();
                HTML = HTML.Replace(@"\n", string.Empty);
                HTML = HTML.Replace(@"\", string.Empty);



                ArrayList htmlArrList = HTMLWorker.ParseToList(new StringReader(HTML.Substring(1, HTML.Length - 2)), styles);

                foreach (IElement strLn in htmlArrList)
                {
                    document.Add(strLn);
                }
                document.Close();

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Generate PDF");
                return Json("Success");

            }
            catch (Exception exx)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exx);
                Console.Error.WriteLine(exx.StackTrace);
                Console.Error.WriteLine(exx.Message);
                return Json("Failed");
            }
        }

        public ActionResult GetPdfIn()
        {
            byte[] theFolders;
            try
            {
                string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                var path = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                path = path.Replace("\\", "//");
                path = path + "//" + Session["PDFfileName1"].ToString();


               // string PDFfileName1 = DateTime.Now.ToString("ddMMyyyy_hh_mm_ss_fff") + "_" + ".pdf";
                //Session["PDFfileName1"] = PDFfileName1;

                //string FolderName = Session["UserName"].ToString() + "_" + SettingsCRM.countryCode;
                //var filepath = Path.Combine(clientSetting.mvnoSettings.internalPdfDownload, FolderName);
                //string PDFfileName = filepath + "\\" + PDFfileName1;




                theFolders = System.IO.File.ReadAllBytes(path);


                if (Directory.Exists(path))
                {
                    System.IO.File.Delete(Path.Combine(path, Session["PDFfileName1"].ToString()));
                }
                return File(theFolders, "application/pdf", DateTime.Now.ToString("ddMMyyyy_hh_mm_ss_fff") + "_" + ".pdf");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                theFolders = System.Text.Encoding.ASCII.GetBytes("Internal Error");
                return File(theFolders, "application/pdf", "UnableToDownload.pdf");
            }

        }


        #endregion
    }
}


