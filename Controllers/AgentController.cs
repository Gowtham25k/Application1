using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CRM.Models;
using ServiceCRM;

namespace CRM.Controllers
{
    [ValidateState]
    public class AgentController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();

        public ViewResult MyProfile()
        {
            AgentProfile agentProfile = new AgentProfile();
            CRMUserDetailsRequest crmUserDetReq = new CRMUserDetailsRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                agentProfile.agentAccess = Session["UserGroup"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    

                    crmUserDetReq.CountryCode = clientSetting.countryCode;
                    crmUserDetReq.BrandCode = clientSetting.brandCode;
                    crmUserDetReq.LanguageCode = clientSetting.langCode;
                    crmUserDetReq.Username = Session["UserName"].ToString();

                    CRMUserDetailsResponse crmUserDetResp = serviceCRM.CRMProfileUserDetails(crmUserDetReq);

                    if (crmUserDetResp.userDetails != null && crmUserDetResp.userDetails.Count > 0)
                    {
                        agentProfile.agentUserName = crmUserDetResp.userDetails[0].UserName;
                        agentProfile.agentFullName = crmUserDetResp.userDetails[0].FirstName;
                        agentProfile.agentPhone = crmUserDetResp.userDetails[0].Phone;
                        agentProfile.agentEMail = crmUserDetResp.userDetails[0].Email;
                        agentProfile.agentStatus = crmUserDetResp.userDetails[0].status == "1" ? true : false;
                        agentProfile.agentEMail = crmUserDetResp.userDetails[0].Email;
                        //agentProfile.agentLastLogin = Utility.FormatDateTime(crmUserDetResp.userDetails[0].lastLogin, clientSetting.mvnoSettings.dateTimeFormat);
                        agentProfile.agentLastLogin = Utility.GetDateconvertion(crmUserDetResp.userDetails[0].lastLogin, "DD/MM/YYYY", false,clientSetting.mvnoSettings.dateTimeFormat);
                    }
                
                return View(agentProfile);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return View(agentProfile);
            }
            finally
            {
                crmUserDetReq = null;
                serviceCRM = null;
                //agentProfile = null;
            }

            
        }

        public JsonResult ChangePassword(string oldPassword, string newPassword, string UserName)
        {
            UserRegisterResponse userRegisterResponse = new UserRegisterResponse();
            UserRegisterRequest userRegisterReq = new UserRegisterRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                userRegisterReq.BrandCode = clientSetting.brandCode;
                userRegisterReq.CountryCode = clientSetting.countryCode;
                userRegisterReq.LanguageCode = clientSetting.langCode;
                if (!string.IsNullOrEmpty(Convert.ToString(Session["UserName"])))
                    userRegisterReq.UserName = Session["UserName"].ToString();
                else
                    userRegisterReq.UserName = UserName;
                userRegisterReq.userId = Session["UserID"].ToString();
                userRegisterReq.Password = newPassword;
                userRegisterReq.oldPassword = oldPassword;
                userRegisterReq.PasswordUpd = "YES";
                userRegisterReq.Mode = "MOD";

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    userRegisterResponse = serviceCRM.CRMUserRegister(userRegisterReq);

                    if (userRegisterResponse != null && userRegisterResponse.reponseDetails != null && userRegisterResponse.reponseDetails.ResponseCode != null)
                    {
                        string errorMsg = Resources.ErrorResources.ResourceManager.GetString("AgentProfile_" + userRegisterResponse.reponseDetails.ResponseCode);
                        userRegisterResponse.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorMsg) ? userRegisterResponse.reponseDetails.ResponseDesc : errorMsg;
                    }
                    if (userRegisterResponse != null && userRegisterResponse.reponseDetails.ResponseCode == "0" && string.IsNullOrEmpty(Convert.ToString(Session["UserName"])))
                    {
                        Session["UserName"] = UserName;
                    }
                
                return Json(userRegisterResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(userRegisterResponse, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                userRegisterReq = null;
                serviceCRM = null;
                //userRegisterResponse = null;
            }
            
        }

        [HttpPost]
        public JsonResult MyProfileImage()
        {
            SelectListItem mpItem = new SelectListItem();
            try
            {
                string imgExtn = string.Empty;
                string dirPath = SettingsCRM.myProfileImgPath;

                if (Request.Files.Count > 0)
                {
                    HttpPostedFileBase imgFile = Request.Files[0];
                    imgExtn = Path.GetExtension(imgFile.FileName);
                }
                if (Request.Files.Count > 0)
                {
                    HttpPostedFileBase imgFile = Request.Files[0];
                    if (imgFile != null && imgFile.ContentLength > 0)
                    {
                        Directory.GetFiles(dirPath, Session["UserName"].ToString() + ".*", SearchOption.TopDirectoryOnly).ToList().ForEach(f => System.IO.File.Delete(f));
                        Directory.GetFiles(Server.MapPath("~/Content/MyProfile/"), Session["UserName"].ToString() + ".*", SearchOption.TopDirectoryOnly).ToList().ForEach(f => System.IO.File.Delete(f));

                        imgFile.SaveAs(dirPath + Session["UserName"] + imgExtn);
                        imgFile.SaveAs(Server.MapPath("~/Content/MyProfile/" + Session["UserName"] + imgExtn));
                        mpItem.Value = "0";
                        mpItem.Text = "Success";
                    }
                    else
                    {
                        mpItem.Value = "1";
                        mpItem.Text = "Failed";
                    }
                }
                else
                {
                    System.IO.File.Delete(Directory.GetFiles(dirPath, Session["UserName"] + ".*")[0]);
                    System.IO.File.Delete(Directory.GetFiles(Server.MapPath("~/Content/MyProfile/"), Session["UserName"] + ".*")[0]);
                    mpItem.Value = "1";
                    mpItem.Text = "Deleted";
                }
                return Json(mpItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception eX)
            {
                mpItem.Value = "1";
                mpItem.Text = eX.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(mpItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //mpItem = null;
            }
            
        }

        [HttpGet]
        public ViewResult Reference()
        {
            try
            {

            }
            catch
            {

            }
            return View();
        }

        [HttpPost]
        public string RenderReference(string refFileName)
        {
            StringBuilder sbRefHtml = new StringBuilder();
            try
            {
                sbRefHtml.Append(System.IO.File.ReadAllText(clientSetting.mvnoSettings.referenceHtmlPath + refFileName + ".html"));
                return sbRefHtml.ToString();
            }
            catch
            {
                return sbRefHtml.ToString();
            }
            finally
            {
                //sbRefHtml = null;
            }
            
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
                if (Date.Length != 2)
                {
                    Date = "0" + Date;
                }
                Month = SplitDOB[Convert.ToInt16(Indexsp[1])].ToString();
                if (Month.Length != 2)
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
            return DateValue;
        }

    }
}
