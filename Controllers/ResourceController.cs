using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CRM.Models;
using ServiceCRM;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;

using Newtonsoft.Json;
using System.Text;
using System.Xml;

namespace BeginCRM.Controllers
{
    [ValidateState]

    public class ResourceController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();
        public static bool _runningFromNUnit = Service.UnitTestDetector._runningFromNUnit;

        public ActionResult MenuUserMapping()
        {
            return RedirectToAction("MenuUserMapping", "MenuToUser");
        }

        public ActionResult MenuFavorites()
        {
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Menu Favorites Success");
                return RedirectToAction("MenuFavorites", "MenuToUser");

            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            return PartialView();
        }

        public ViewResult RoleManagement()
        {
            return View();
        }

        public ViewResult MenuManagement()
        {
            return View();
        }

        public ActionResult GroupMapping()
        {
            return View();
        }

        public ActionResult RoleGroupMappingApproval()
        {
            return View();
        }

        public ActionResult UserManagement()
        {
            return View();
        }

        public PartialViewResult MenuCategories()
        {
            MenuCategoryResponse menuCatgResp = new MenuCategoryResponse();
            MenuCategoryRequest menuCatgReq = new MenuCategoryRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuCategories Start");
                menuCatgReq.CountryCode = clientSetting.countryCode;
                menuCatgReq.BrandCode = clientSetting.brandCode;
                menuCatgReq.LanguageCode = clientSetting.langCode;
                menuCatgReq.MODE = "GET";
                menuCatgReq.SuperadminID = System.Configuration.ConfigurationManager.AppSettings["RootUserIDs"].Trim();
                menuCatgReq.LoginID = Convert.ToString(Session["UserID"]);

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                menuCatgResp = serviceCRM.CRMMenuCategory(menuCatgReq);
                ///FRR--3083
                if (menuCatgResp != null && menuCatgResp.reponseDetails != null && menuCatgResp.reponseDetails.ResponseCode != null)
                {
                    if (!_runningFromNUnit)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MenuCategoryGET_" + menuCatgResp.reponseDetails.ResponseCode);
                        menuCatgResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? menuCatgResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuCategories End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                menuCatgReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return PartialView(menuCatgResp);
        }
        public PartialViewResult SettingsMenuItem(string userCatgID)
        {
            RoleMgtMasterItemResponse menuItemResp = new RoleMgtMasterItemResponse();
            RoleMgtMasterItemRequest menuItemReq = new RoleMgtMasterItemRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - SettingsMenuItem Start");
                menuItemReq.CountryCode = clientSetting.countryCode;
                menuItemReq.BrandCode = clientSetting.brandCode;
                menuItemReq.LanguageCode = clientSetting.langCode;
                menuItemReq.roleID = userCatgID;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                menuItemResp = serviceCRM.CRMRoleMgtMaster(menuItemReq);
                ///FRR--3083
                if (menuItemResp != null && menuItemResp.reponseDetails != null && menuItemResp.reponseDetails.ResponseCode != null)
                {
                    if (!_runningFromNUnit)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SettingsMenuItem_" + menuItemResp.reponseDetails.ResponseCode);
                        menuItemResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? menuItemResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                ///FRR--3083
                if (menuItemResp.settingMenu.Count == 0)
                {
                    menuItemResp = new RoleMgtMasterItemResponse();
                    menuItemReq.roleID = string.Empty;
                    menuItemResp = serviceCRM.CRMRoleMgtMaster(menuItemReq);
                    menuItemResp.settingMenu.ForEach(m => m.editRights = "0");
                    menuItemResp.settingMenu.ForEach(m => m.adminRights = "0");
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - SettingsMenuItem End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                menuItemReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return PartialView(menuItemResp);
        }
        public PartialViewResult MenuItems()
        {
            MenuItemsResponse menuItemResp = new MenuItemsResponse();
            MenuItemsRequest menuItemReq = new MenuItemsRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;

            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuItems Start");
                menuItemReq.CountryCode = clientSetting.countryCode;
                menuItemReq.BrandCode = clientSetting.brandCode;
                menuItemReq.LanguageCode = clientSetting.langCode;
                menuItemReq.MODE = "GET";
                menuItemReq.SuperadminID = System.Configuration.ConfigurationManager.AppSettings["RootUserIDs"].Trim();
                menuItemReq.LoginID = Convert.ToString(Session["UserID"]);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                menuItemResp = serviceCRM.CRMMenuItems(menuItemReq);
                ///FRR--3083
                if (menuItemResp != null && menuItemResp.reponseDetails != null && menuItemResp.reponseDetails.ResponseCode != null)
                {
                    if (!_runningFromNUnit)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MenuItemGET_" + menuItemResp.reponseDetails.ResponseCode);
                        menuItemResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? menuItemResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuItems End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                menuItemReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return PartialView(menuItemResp);
        }
        public JsonResult MenuList(string catgLinkID)
        {
            List<string> lstMenuLinkID = new List<string>();
            RoleMenuCategoryItemResponse roleMenuCatgItemResp = new RoleMenuCategoryItemResponse();
            RoleMenuCategoryItemRequest roleMenuCatgItemReq = new RoleMenuCategoryItemRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuList Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                roleMenuCatgItemReq.CountryCode = clientSetting.countryCode;
                roleMenuCatgItemReq.BrandCode = clientSetting.brandCode;
                roleMenuCatgItemReq.LanguageCode = clientSetting.langCode;
                roleMenuCatgItemReq.menuCatID = catgLinkID.Trim();
                roleMenuCatgItemReq.SuperadminID = System.Configuration.ConfigurationManager.AppSettings["RootUserIDs"].Trim();
                roleMenuCatgItemReq.LoginID = Convert.ToString(Session["UserID"]);
                roleMenuCatgItemResp = serviceCRM.CRMRoleMenuCategoryItem(roleMenuCatgItemReq);
                if (roleMenuCatgItemResp.reponseDetails.ResponseCode == "0")
                {
                    roleMenuCatgItemResp.roleMenuCategoryItem.ForEach(m => lstMenuLinkID.Add(m.subCatgID));
                }
                else { }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuList End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                roleMenuCatgItemReq = null;
                roleMenuCatgItemResp = null;
                serviceCRM = null;
            }
            return Json(lstMenuLinkID, JsonRequestBehavior.AllowGet);
        }
        public JsonResult MenuListNew(string catgLinkID, string mode, string groupedvalues) //gopi2296
        {
            RoleMenuCategoryItemResponse roleMenuCatgItemResp = new RoleMenuCategoryItemResponse();
            RoleMenuCategoryItemRequest roleMenuCatgItemReq = new RoleMenuCategoryItemRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuListNew Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                roleMenuCatgItemReq.CountryCode = clientSetting.countryCode;
                roleMenuCatgItemReq.BrandCode = clientSetting.brandCode;
                roleMenuCatgItemReq.LanguageCode = clientSetting.langCode;
                roleMenuCatgItemReq.menuCatID = catgLinkID.Trim();
                roleMenuCatgItemReq.SuperadminID = System.Configuration.ConfigurationManager.AppSettings["RootUserIDs"].Trim();
                roleMenuCatgItemReq.LoginID = Convert.ToString(Session["UserID"]);
                roleMenuCatgItemReq.Mode = mode; //Menu mapping & Group Mapping based on the mode
                roleMenuCatgItemReq.MappedIDs = groupedvalues; //Menu mapping & Group Mapping based on the mode
                roleMenuCatgItemResp = serviceCRM.CRMRoleMenuCategoryItem(roleMenuCatgItemReq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuListNew End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                roleMenuCatgItemReq = null;
                serviceCRM = null;
            }
            return Json(roleMenuCatgItemResp, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult MenuCategory(menu_Category menuCatg)
        {
            MessageModal modMsg = new MessageModal();
            MenuCategoryResponse menuCatgResp = new MenuCategoryResponse();
            MenuCategoryRequest menuCatgReq = new MenuCategoryRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuCategory Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                menuCatgReq.CountryCode = clientSetting.countryCode;
                menuCatgReq.BrandCode = clientSetting.brandCode;
                menuCatgReq.LanguageCode = clientSetting.langCode;
                menuCatgReq.MENUCATID = menuCatg.MENUCATID;
                if (!string.IsNullOrEmpty(menuCatg.menuMode))
                {
                    if (menuCatg.menuMode.ToUpper() == "MOD")
                    {
                        menuCatgReq.MODE = "MOD";
                        menuCatgReq.MENUCATDESC = menuCatg.MENUCATDESC;
                        menuCatgReq.MENUCATORDER = menuCatg.MENUCATORDER;
                        menuCatgReq.MENUCATSTYLE = menuCatg.MENUCATSTYLE;
                        menuCatgReq.MENUSTATUS = menuCatg.MENUSTATUS;
                    }
                    else if (menuCatg.menuMode.ToUpper() == "DEL")
                    {
                        menuCatgReq.MODE = "DEL";
                        menuCatgReq.MENUCATDESC = menuCatg.MENUCATDESC;
                    }
                    else
                    {
                        menuCatgReq.MODE = "ADD";
                        menuCatgReq.MENUCATDESC = menuCatg.MENUCATDESC;
                        menuCatgReq.MENUCATORDER = menuCatg.MENUCATORDER;
                        menuCatgReq.MENUCATSTYLE = menuCatg.MENUCATSTYLE;
                        menuCatgReq.MENUSTATUS = menuCatg.MENUSTATUS;
                    }
                }
                else
                {
                    menuCatgReq.MODE = "ADD";
                    menuCatgReq.MENUCATDESC = menuCatg.MENUCATDESC;
                    menuCatgReq.MENUCATORDER = menuCatg.MENUCATORDER;
                    menuCatgReq.MENUCATSTYLE = menuCatg.MENUCATSTYLE;
                    menuCatgReq.MENUSTATUS = menuCatg.MENUSTATUS;
                }
                menuCatgResp = serviceCRM.CRMMenuCategory(menuCatgReq);
                ///FRR--3083
                if (menuCatgResp != null && menuCatgResp.reponseDetails != null && menuCatgResp.reponseDetails.ResponseCode != null)
                {
                    if (!_runningFromNUnit)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MenuCategoryGET_" + menuCatgResp.reponseDetails.ResponseCode);
                        menuCatgResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? menuCatgResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                ///FRR--3083
                modMsg.responseCode = Convert.ToInt32(menuCatgResp.reponseDetails.ResponseCode.Trim());
                modMsg.responseMessage = menuCatgResp.reponseDetails.ResponseDesc;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuCategory End");
            }
            catch (Exception eX)
            {
                modMsg.responseCode = 9;
                modMsg.responseMessage = eX.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                serviceCRM = null;
                menuCatgResp = null;
                menuCatgReq = null;
                errorInsertMsg = string.Empty;
            }
            return PartialView("MessageModal", modMsg);
        }
        public PartialViewResult MenuItem(menu_Items menuItem)
        {
            MessageModal modMsg = new MessageModal();
            MenuItemsResponse menuItemsResp = new MenuItemsResponse();
            MenuItemsRequest menuItemReq = new MenuItemsRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuItem Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                menuItemReq.CountryCode = clientSetting.countryCode;
                menuItemReq.BrandCode = clientSetting.brandCode;
                menuItemReq.LanguageCode = clientSetting.langCode;
                menuItemReq.SUBCATID = menuItem.SUBCATID;
                if (!string.IsNullOrEmpty(menuItem.menuMode))
                {
                    if (menuItem.menuMode.ToUpper() == "MOD")
                    {
                        menuItemReq.MODE = "MOD";
                        menuItemReq.SUbCATDESC = menuItem.SUbCATDESC;
                        menuItemReq.SUBCATURL = menuItem.SUBCATURL;
                        menuItemReq.SUBCATTYPE = menuItem.SUBCATTYPE;
                        menuItemReq.SUBCATORDER = menuItem.SUBCATORDER;
                        menuItemReq.SUBCATSTYLE = menuItem.SUBCATSTYLE;
                        menuItemReq.SUBSTAUTS = menuItem.SUBSTAUTS;
                        menuItemReq.userName = ""; Session["UserName"].ToString();
                    }
                    else if (menuItem.menuMode.ToUpper() == "DEL")
                    {
                        menuItemReq.MODE = "DEL";
                        menuItemReq.SUbCATDESC = menuItem.SUbCATDESC;
                    }
                    else
                    {
                        menuItemReq.MODE = "ADD";
                        menuItemReq.SUbCATDESC = menuItem.SUbCATDESC;
                        menuItemReq.SUBCATURL = menuItem.SUBCATURL;
                        menuItemReq.SUBCATTYPE = menuItem.SUBCATTYPE;
                        menuItemReq.SUBCATORDER = menuItem.SUBCATORDER;
                        menuItemReq.SUBCATSTYLE = menuItem.SUBCATSTYLE;
                        menuItemReq.SUBSTAUTS = menuItem.SUBSTAUTS;
                        menuItemReq.userName = ""; Session["UserName"].ToString();
                    }
                }
                else
                {
                    menuItemReq.MODE = "ADD";
                    menuItemReq.SUbCATDESC = menuItem.SUbCATDESC;
                    menuItemReq.SUBCATURL = menuItem.SUBCATURL;
                    menuItemReq.SUBCATTYPE = menuItem.SUBCATTYPE;
                    menuItemReq.SUBCATORDER = menuItem.SUBCATORDER;
                    menuItemReq.SUBCATSTYLE = menuItem.SUBCATSTYLE;
                    menuItemReq.SUBSTAUTS = menuItem.SUBSTAUTS;
                    menuItemReq.userName = "";     //Session["UserName"].ToString();
                    menuItemReq.SuperadminID = System.Configuration.ConfigurationManager.AppSettings["RootUserIDs"].Trim();
                    menuItemReq.LoginID = Convert.ToString(Session["UserID"]);
                }
                menuItemsResp = serviceCRM.CRMMenuItems(menuItemReq);
                ///FRR--3083
                if (menuItemsResp != null && menuItemsResp.reponseDetails != null && menuItemsResp.reponseDetails.ResponseCode != null)
                {
                    if (!_runningFromNUnit)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MenuItemGET_" + menuItemsResp.reponseDetails.ResponseCode);
                        menuItemsResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? menuItemsResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                ///FRR--3083
                modMsg.responseCode = Convert.ToInt32(menuItemsResp.reponseDetails.ResponseCode.Trim());
                modMsg.responseMessage = menuItemsResp.reponseDetails.ResponseDesc;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuItem End");
            }
            catch (Exception eX)
            {
                modMsg.responseCode = 9;
                modMsg.responseMessage = eX.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                serviceCRM = null;
                menuItemsResp = null;
                menuItemReq = null;
                errorInsertMsg = string.Empty;
            }
            return PartialView("MessageModal", modMsg);
        }
        public JsonResult MenuToCategory(string catgLinkID, string menuLinkIDs)
        {
            SelectListItem mcItem = new SelectListItem();
            CRMResponse crmResp = new CRMResponse();
            MenuMappingRequest menuMapReq = new MenuMappingRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuToCategory Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                menuMapReq.CountryCode = clientSetting.countryCode;
                menuMapReq.BrandCode = clientSetting.brandCode;
                menuMapReq.LanguageCode = clientSetting.langCode;
                menuMapReq.MENUCATID = catgLinkID;
                menuMapReq.SUBCATID = menuLinkIDs;
                crmResp = serviceCRM.CRMMenuItemMapping(menuMapReq);
                ///FRR--3083
                if (crmResp != null && crmResp.ResponseCode != null)
                {
                    if (!_runningFromNUnit)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MenuItemMapping_" + crmResp.ResponseCode);
                        crmResp.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? crmResp.ResponseDesc : errorInsertMsg;
                    }
                }
                ///FRR--3083
                mcItem.Value = crmResp.ResponseCode;
                mcItem.Text = crmResp.ResponseDesc;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuToCategory End");
            }
            catch (Exception eX)
            {
                mcItem.Value = "1";
                mcItem.Text = eX.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                serviceCRM = null;
                crmResp = null;
                menuMapReq = null;
                errorInsertMsg = string.Empty;
            }
            return Json(mcItem, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult MenuCategoryDetails(menu_Category menuCatg)
        {
            MenuCatgResource menuCatgResource = new MenuCatgResource();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuCategoryDetails Start");
                menuCatgResource.menuCatg = menuCatg;
                menuCatgResource.lstCatgCssClass.Sort();
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuCategoryDetails End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                // menuCatgResource = null;
            }
            return PartialView("MenuCategory", menuCatgResource);
        }
        public PartialViewResult MenuItemDetails(menu_Items menuItem)
        {
            MenuItemResource menuItemResource = new MenuItemResource();
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuItemDetails Start");
                menuItemResource.menuItem = menuItem;
                menuItemResource.lstMenuCssClass.Sort();
                menuItemResource.lstMenuLinkID.Sort();
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - MenuItemDetails End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                // menuItemResource = null;
            }
            return PartialView("MenuItem", menuItemResource);
        }
        public PartialViewResult UserCategories()
        {
            RoleCategoryResponse roleCatgResp = new RoleCategoryResponse();
            RoleCategoryRequest roleCatgReq = new RoleCategoryRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - UserCategories Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                roleCatgReq.CountryCode = clientSetting.countryCode;
                roleCatgReq.BrandCode = clientSetting.brandCode;
                roleCatgReq.LanguageCode = clientSetting.langCode;
                roleCatgReq.MODE = "GET";
                if (clientSetting.mvnoSettings.userManagementType.ToUpper() == "ON")
                {
                    roleCatgReq.CreatedBy = Session["UserName"].ToString();
                    roleCatgReq.RoleCategoryType = Session["ROLE_CAT_TYPE"].ToString();
                    roleCatgReq.ROLECATNAME = Session["UserGroup"].ToString();
                }
                roleCatgResp = serviceCRM.CRMRoleCategoryDetails(roleCatgReq);
                ///FRR--3083
                if (roleCatgResp != null && roleCatgResp.reponseDetails != null && roleCatgResp.reponseDetails.ResponseCode != null)
                {
                    if (!_runningFromNUnit)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UserCategories_" + roleCatgResp.reponseDetails.ResponseCode);
                        roleCatgResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? roleCatgResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - UserCategories End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                roleCatgReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return PartialView(roleCatgResp);
        }
        public PartialViewResult UserItems()
        {
            UserItems userLists = new UserItems();
            GetUserDB getUserDB = new GetUserDB();
            ServiceInvokeCRM serviceCRM;
            GetCrmUsers getCrmUsers;
            UserItem userItm;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - UserItems Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                getUserDB.CountryCode = clientSetting.countryCode;
                getUserDB.BrandCode = clientSetting.brandCode;
                getUserDB.LanguageCode = clientSetting.langCode;
                getUserDB.Enable = "1";
                getCrmUsers = serviceCRM.GetCrmUser(getUserDB);
                foreach (User user in getCrmUsers.Users)
                {
                    userItm = new UserItem();
                    userItm.userItemID = user.username;
                    userLists.lstUserItem.Add(userItm);
                }
                userLists.lstUserItem = userLists.lstUserItem.OrderBy(m => m.userItemID).ToList();
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - UserItems End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
            finally
            {
                serviceCRM = null;
                getUserDB = null;
                getCrmUsers = null;
                userItm = null;
            }
            return PartialView(userLists);
        }

        public PartialViewResult UserCategoryDetails(RoleCategoryName userCatg)
        {
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "User Category Details Success");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);

            }
            return PartialView("UserCategory", userCatg);
        }

        public ActionResult UserCategory(RoleCategoryName userCatg)
        {
            MessageModal modMsg = new MessageModal();
            RoleCategoryResponse roleCatgResp = new RoleCategoryResponse();
            RoleCategoryRequest roleCatgReq = new RoleCategoryRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - UserCategory Start");
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                roleCatgReq.CountryCode = clientSetting.countryCode;
                roleCatgReq.BrandCode = clientSetting.brandCode;
                roleCatgReq.LanguageCode = clientSetting.langCode;
                roleCatgReq.ROLECATID = userCatg.ROLECATID;
                if (!string.IsNullOrEmpty(userCatg.Mode))
                {
                    if (userCatg.Mode.ToUpper() == "MOD")
                    {
                        roleCatgReq.MODE = "MOD";
                        roleCatgReq.ROLECATNAME = userCatg.ROLECATNAME;
                        roleCatgReq.STATUS = userCatg.STATUS;
                        roleCatgReq.ENABLEPASSWORDPOLICY = userCatg.ENABLEPASSWORDPOLICY;
                        roleCatgReq.LevelAuth = userCatg.LevelAuth;
                        roleCatgReq.ViewRightsOnly = userCatg.ViewRightsOnly;
                        // 4836
                        roleCatgReq.SemiAutoVerification = userCatg.SemiAutoVerification;
                        //6129
                        roleCatgReq.OTPVerificationprocess = userCatg.OTPVerificationprocess;
                        if (clientSetting.countryCode.ToUpper() == "BRA")
                        {
                            roleCatgReq.ShowAnatelId = userCatg.ShowAnatelId;
                            roleCatgReq.GenAnatelId = userCatg.GenAnatelId;
                        }
                        roleCatgReq.RoleCategoryType = userCatg.RoleCategoryType;
                        roleCatgReq.CreatedBy = Session["UserName"].ToString();
                        roleCatgReq.EditRights = userCatg.EditRights;

                        roleCatgReq.ApprovalLimitFeature = userCatg.ApprovalLimitFeature;
                        roleCatgReq.SubmissionLimitFeature = userCatg.SubmissionLimitFeature;
                        roleCatgReq.MaxCreditApprovalLimit = decimal.Parse(string.IsNullOrEmpty(userCatg.MaxCreditApprovalLimit) ? "0" : userCatg.MaxCreditApprovalLimit);
                        if (roleCatgReq.MaxCreditApprovalLimit.ToString().Length > 10)
                            roleCatgReq.MaxCreditApprovalLimit = decimal.Parse(roleCatgReq.MaxCreditApprovalLimit.ToString().Substring(0, 10));
                        roleCatgReq.MaxDebitApprovalLimit = decimal.Parse(string.IsNullOrEmpty(userCatg.MaxDebitApprovalLimit) ? "0" : userCatg.MaxDebitApprovalLimit);
                        if (roleCatgReq.MaxDebitApprovalLimit.ToString().Length > 10)
                            roleCatgReq.MaxDebitApprovalLimit = decimal.Parse(roleCatgReq.MaxDebitApprovalLimit.ToString().Substring(0, 10));
                        roleCatgReq.MaxCreditSubmitLimit = decimal.Parse(string.IsNullOrEmpty(userCatg.MaxCreditSubmitLimit) ? "0" : userCatg.MaxCreditSubmitLimit);
                        if (roleCatgReq.MaxCreditSubmitLimit.ToString().Length > 10)
                            roleCatgReq.MaxCreditSubmitLimit = decimal.Parse(roleCatgReq.MaxCreditSubmitLimit.ToString().Substring(0, 10));
                        roleCatgReq.MaxDebitSubmitLimit = decimal.Parse(string.IsNullOrEmpty(userCatg.MaxDebitSubmitLimit) ? "0" : userCatg.MaxDebitSubmitLimit);
                        if (roleCatgReq.MaxDebitSubmitLimit.ToString().Length > 10)
                            roleCatgReq.MaxDebitSubmitLimit = decimal.Parse(roleCatgReq.MaxDebitSubmitLimit.ToString().Substring(0, 10));

                    }
                    else if (userCatg.Mode.ToUpper() == "DEL")
                    {
                        roleCatgReq.MODE = "DEL";
                        roleCatgReq.ROLECATNAME = userCatg.ROLECATNAME;
                    }
                    else
                    {
                        roleCatgReq.MODE = "ADD";
                        roleCatgReq.ROLECATNAME = userCatg.ROLECATNAME;
                        roleCatgReq.STATUS = userCatg.STATUS;
                        roleCatgReq.ENABLEPASSWORDPOLICY = userCatg.ENABLEPASSWORDPOLICY;
                        roleCatgReq.LevelAuth = userCatg.LevelAuth;
                        roleCatgReq.ViewRightsOnly = userCatg.ViewRightsOnly;
                        // 4836
                        roleCatgReq.SemiAutoVerification = userCatg.SemiAutoVerification;
                        //6129
                        roleCatgReq.OTPVerificationprocess = userCatg.OTPVerificationprocess;
                        if (clientSetting.countryCode.ToUpper() == "BRA")
                        {
                            roleCatgReq.ShowAnatelId = userCatg.ShowAnatelId;
                            roleCatgReq.GenAnatelId = userCatg.GenAnatelId;
                        }
                        roleCatgReq.RoleCategoryType = userCatg.RoleCategoryType;
                        roleCatgReq.CreatedBy = Session["UserName"].ToString();
                        roleCatgReq.EditRights = userCatg.EditRights;

                        roleCatgReq.ApprovalLimitFeature = userCatg.ApprovalLimitFeature;
                        roleCatgReq.SubmissionLimitFeature = userCatg.SubmissionLimitFeature;
                        roleCatgReq.MaxCreditApprovalLimit = decimal.Parse(string.IsNullOrEmpty(userCatg.MaxCreditApprovalLimit) ? "0" : userCatg.MaxCreditApprovalLimit);
                        roleCatgReq.MaxDebitApprovalLimit = decimal.Parse(string.IsNullOrEmpty(userCatg.MaxDebitApprovalLimit) ? "0" : userCatg.MaxDebitApprovalLimit);
                        roleCatgReq.MaxCreditSubmitLimit = decimal.Parse(string.IsNullOrEmpty(userCatg.MaxCreditSubmitLimit) ? "0" : userCatg.MaxCreditSubmitLimit);
                        roleCatgReq.MaxDebitSubmitLimit = decimal.Parse(string.IsNullOrEmpty(userCatg.MaxDebitSubmitLimit) ? "0" : userCatg.MaxDebitSubmitLimit);
                    }
                }
                else
                {
                    roleCatgReq.MODE = "ADD";
                    roleCatgReq.ROLECATNAME = userCatg.ROLECATNAME;
                    roleCatgReq.STATUS = userCatg.STATUS;
                    roleCatgReq.ENABLEPASSWORDPOLICY = userCatg.ENABLEPASSWORDPOLICY;
                    roleCatgReq.LevelAuth = userCatg.LevelAuth;
                    roleCatgReq.ViewRightsOnly = userCatg.ViewRightsOnly;
                    // 4836
                    roleCatgReq.SemiAutoVerification = userCatg.SemiAutoVerification;
                    //6129
                    roleCatgReq.OTPVerificationprocess = userCatg.OTPVerificationprocess;
                    if (clientSetting.countryCode.ToUpper() == "BRA")
                    {
                        roleCatgReq.ShowAnatelId = userCatg.ShowAnatelId;
                        roleCatgReq.GenAnatelId = userCatg.GenAnatelId;
                    }
                    roleCatgReq.RoleCategoryType = userCatg.RoleCategoryType;
                    roleCatgReq.CreatedBy = Session["UserName"].ToString();
                    roleCatgReq.EditRights = userCatg.EditRights;

                    roleCatgReq.ApprovalLimitFeature = userCatg.ApprovalLimitFeature;
                    roleCatgReq.SubmissionLimitFeature = userCatg.SubmissionLimitFeature;
                    roleCatgReq.MaxCreditApprovalLimit = decimal.Parse(string.IsNullOrEmpty(userCatg.MaxCreditApprovalLimit) ? "0" : userCatg.MaxCreditApprovalLimit);
                    roleCatgReq.MaxDebitApprovalLimit = decimal.Parse(string.IsNullOrEmpty(userCatg.MaxDebitApprovalLimit) ? "0" : userCatg.MaxDebitApprovalLimit);
                    roleCatgReq.MaxCreditSubmitLimit = decimal.Parse(string.IsNullOrEmpty(userCatg.MaxCreditSubmitLimit) ? "0" : userCatg.MaxCreditSubmitLimit);
                    roleCatgReq.MaxDebitSubmitLimit = decimal.Parse(string.IsNullOrEmpty(userCatg.MaxDebitSubmitLimit) ? "0" : userCatg.MaxDebitSubmitLimit);
                }
                roleCatgResp = serviceCRM.CRMRoleCategoryDetails(roleCatgReq);
                if (!_runningFromNUnit)
                {
                    ///FRR--3083
                    if (roleCatgResp != null && roleCatgResp.reponseDetails != null && roleCatgResp.reponseDetails.ResponseCode != null)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UserCategories_" + roleCatgResp.reponseDetails.ResponseCode);
                        roleCatgResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? roleCatgResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                }
                modMsg.responseCode = Convert.ToInt32(roleCatgResp.reponseDetails.ResponseCode.Trim());
                modMsg.responseMessage = roleCatgResp.reponseDetails.ResponseDesc;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - UserCategory End");
            }
            catch (Exception eX)
            {
                modMsg.responseMessage = eX.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                serviceCRM = null;
                roleCatgResp = null;
                roleCatgReq = null;
                errorInsertMsg = string.Empty;
            }
            return PartialView("MessageModal", modMsg);
        }
        public UserRegisterResponse CRMUserRegister(UserRegister userDetails, string mode)
        {
            UserRegisterRequest userRegisterReq = new UserRegisterRequest();
            UserRegisterResponse userRegisterResponse = new UserRegisterResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - CRMUserRegister Start");
                userRegisterReq.BrandCode = clientSetting.brandCode;
                userRegisterReq.CountryCode = clientSetting.countryCode;
                userRegisterReq.LanguageCode = clientSetting.langCode;
                userRegisterReq.UserName = userDetails.userName;
                userRegisterReq.userId = userDetails.userID;
                userRegisterReq.FirstName = userDetails.firstName;
                userRegisterReq.LastName = userDetails.lastName;
                userRegisterReq.PhoneNumber = userDetails.phoneNumber;
                userRegisterReq.EmailId = userDetails.emailID;
                userRegisterReq.PasswordUpd = "NO";
                userRegisterReq.Status = userDetails.status;
                userRegisterReq.Mode = mode;
                userRegisterReq.roleCatId = userDetails.roleCatId;
                userRegisterReq.AccBlocked = userDetails.AccBlocked;
                userRegisterReq.RoleCategoryType = userDetails.RoleCategoryType;
                userRegisterReq.AdminID = userDetails.AdminID;
                // if (mode == "ADD" || mode == "DEL")
                userRegisterReq.CreatedBy = Session["UserName"].ToString();
                userRegisterReq.UserAccessId = userDetails.UserAccessId;
                userRegisterReq.UserGroupId = userDetails.UserGroupId;
                userRegisterReq.ATRID = userDetails.ATRID;
                userRegisterReq.ATRName = userDetails.ATRName;
                //4710
                userRegisterReq.EnablePasswordPolicy = userDetails.EnablePasswordPolicy;
                if (clientSetting.mvnoSettings.enterPasswordManually.ToUpper() == "TRUE")
                {
                    userRegisterReq.Password = userDetails.password;
                }
                if (clientSetting.mvnoSettings.createUserInTickettingTool.ToUpper() != "0")
                {
                    userRegisterReq.RestrictUserCreinTicketing = userDetails.RestrictUserCreinTicketing;
                }
                if (clientSetting.mvnoSettings.userManagementType.ToUpper() == "OFF")
                {
                    userRegisterReq.roleCatId = Convert.ToString(Session["UserGroupID"]);
                    if (mode == "ADD" || mode == "MOD" || mode == "DEL")
                    {
                        userRegisterReq.roleCatId = userDetails.roleCatId;
                        userRegisterReq.LoginRole = Convert.ToString(Session["UserGroupID"]);
                    }
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                userRegisterResponse = serviceCRM.CRMUserRegister(userRegisterReq);

                userRegisterResponse.UserRegister = userRegisterResponse.UserRegister.OrderBy(m => m.roleCatId).ToList();
                ViewBag.Status = userRegisterResponse.ATRIDActionGroupList;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - CRMUserRegister End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                userRegisterReq = null;
                serviceCRM = null;
            }
            return userRegisterResponse;
        }
        public JsonResult AddUser(UserRegister userDetails)
        {
            UserRegisterResponse userRegisterResponse = new UserRegisterResponse();
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - AddUser Start");
                if (clientSetting.mvnoSettings.userManagementType.ToUpper() == "ON")
                {
                    userDetails.CreatedBy = Session["UserName"].ToString();
                    userDetails.RoleCategoryType = Session["ROLE_CAT_TYPE"].ToString();
                }
                userRegisterResponse = CRMUserRegister(userDetails, "ADD");
                ///FRR--3083
                if (userRegisterResponse != null && userRegisterResponse.reponseDetails != null && userRegisterResponse.reponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("userRegisterAdd_" + userRegisterResponse.reponseDetails.ResponseCode);
                    userRegisterResponse.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? userRegisterResponse.reponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - AddUser End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                //userRegisterResponse = null;
            }
            return Json(userRegisterResponse, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckAvailability(string userName)
        {
            UserRegisterResponse userRegisterResponse = new UserRegisterResponse();
            UserRegister userdetails = new UserRegister();
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - CheckAvailability Start");
                userdetails.userName = userName;
                userRegisterResponse = CRMUserRegister(userdetails, "USER");
                ///FRR--3083
                if (userRegisterResponse != null && userRegisterResponse.reponseDetails != null && userRegisterResponse.reponseDetails.ResponseCode != null)
                {
                    if (!_runningFromNUnit)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("userRegisterUSER_" + userRegisterResponse.reponseDetails.ResponseCode);
                        userRegisterResponse.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? userRegisterResponse.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - CheckAvailability End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                userdetails = null;
                errorInsertMsg = string.Empty;
            }
            return Json(userRegisterResponse, JsonRequestBehavior.AllowGet);
        }

        #region PID 45999(Ticketting user update)
        public JsonResult GetTickettingUserDetails(string userName)
        {
            UserRegisterResponse userRegisterResponse = new UserRegisterResponse();
            UserRegister userdetails = new UserRegister();
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - GetTickettingUserDetails Start");
                userdetails.userName = userName;
                userRegisterResponse = CRMUserRegister(userdetails, "TICKETTINGUSER");
                ///FRR--3083
                if (userRegisterResponse != null && userRegisterResponse.reponseDetails != null && userRegisterResponse.reponseDetails.ResponseCode != null)
                {
                    if (!_runningFromNUnit)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("TickettingUser_" + userRegisterResponse.reponseDetails.ResponseCode);
                        userRegisterResponse.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? userRegisterResponse.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - GetTickettingUserDetails End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                errorInsertMsg = string.Empty;
                userdetails = null;
            }
            return Json(userRegisterResponse, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateTickettingUser(UserRegister userDetails)
        {
            UserRegisterResponse userRegisterResponse = new UserRegisterResponse();
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - UpdateTickettingUser Start");
                if (clientSetting.mvnoSettings.userManagementType.ToUpper() == "ON")
                {
                    userDetails.CreatedBy = Session["UserName"].ToString();
                    userDetails.RoleCategoryType = Session["ROLE_CAT_TYPE"].ToString();
                }
                userRegisterResponse = CRMUserRegister(userDetails, "UPDATEUSERINTICKETTING");
                ///FRR--3083
                if (userRegisterResponse != null && userRegisterResponse.reponseDetails != null && userRegisterResponse.reponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UpdateTickettingUser_" + userRegisterResponse.reponseDetails.ResponseCode);
                    userRegisterResponse.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? userRegisterResponse.reponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - UpdateTickettingUser End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                errorInsertMsg = string.Empty;
            }
            return Json(userRegisterResponse, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public JsonResult ManageUserList()
        {
            UserRegisterResponse userRegisterResponse = new UserRegisterResponse();
            UserRegister userDetails = new UserRegister();
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - ManageUserList Start");
                if (clientSetting.mvnoSettings.userManagementType.ToUpper() == "ON")
                {
                    userDetails.userName = Session["UserName"].ToString();
                    userDetails.RoleCategoryType = Session["ROLE_CAT_TYPE"].ToString();
                    // userDetails.roleCatName = Session["UserGroup"].ToString();
                }
                userRegisterResponse = CRMUserRegister(userDetails, "GET");
                ///FRR--3083
                if (userRegisterResponse != null && userRegisterResponse.reponseDetails != null && userRegisterResponse.reponseDetails.ResponseCode != null)
                {
                    if (!_runningFromNUnit)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("userRegisterGET_" + userRegisterResponse.reponseDetails.ResponseCode);
                        userRegisterResponse.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? userRegisterResponse.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - ManageUserList End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                errorInsertMsg = string.Empty;
                userDetails = null;
            }
            return new JsonResult() { Data = userRegisterResponse, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult UpdateUserDetails(UserRegister userDet)
        {
            UserRegisterResponse userRegisterResponse = new UserRegisterResponse();
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - UpdateUserDetails Start");
                if (clientSetting.mvnoSettings.userManagementType.ToUpper() == "ON")
                {
                    userDet.RoleCategoryType = Session["ROLE_CAT_TYPE"].ToString();
                    userDet.CreatedBy = Session["UserName"].ToString();
                }
                userRegisterResponse = CRMUserRegister(userDet, "MOD");
                ///FRR--3083
                if (userRegisterResponse != null && userRegisterResponse.reponseDetails != null && userRegisterResponse.reponseDetails.ResponseCode != null)
                {
                    if (!_runningFromNUnit)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("userRegisterMOD_" + userRegisterResponse.reponseDetails.ResponseCode);
                        userRegisterResponse.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? userRegisterResponse.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - UpdateUserDetails End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return new JsonResult() { Data = userRegisterResponse, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            finally
            {
                errorInsertMsg = string.Empty;
            }
            return new JsonResult() { Data = userRegisterResponse, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult GetAllRoles()
        {
            RoleCategoryResponse roleCategoryResponse = new RoleCategoryResponse();
            RoleCategoryRequest roleCategoryReq = new RoleCategoryRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - GetAllRoles Start");
                roleCategoryReq.BrandCode = clientSetting.brandCode;
                roleCategoryReq.CountryCode = clientSetting.countryCode;
                roleCategoryReq.LanguageCode = clientSetting.langCode;
                roleCategoryReq.MODE = "GET";
                if (clientSetting.mvnoSettings.userManagementType.ToUpper() == "ON")
                {
                    roleCategoryReq.CreatedBy = Session["UserName"].ToString();
                    roleCategoryReq.RoleCategoryType = Session["ROLE_CAT_TYPE"].ToString();
                    roleCategoryReq.ROLECATNAME = Session["UserGroup"].ToString();
                }
                else
                {
                    roleCategoryReq.MODE = "GETRoleName";
                    roleCategoryReq.ROLECATID = Convert.ToString(Session["UserGroupID"]);
                }
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                roleCategoryResponse = serviceCRM.CRMRoleCategoryDetails(roleCategoryReq);
                ViewBag.status = roleCategoryResponse.ATRIDLIST;
                ///FRR--3083
                if (roleCategoryResponse != null && roleCategoryResponse.reponseDetails != null && roleCategoryResponse.reponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UserCategories_" + roleCategoryResponse.reponseDetails.ResponseCode);
                    roleCategoryResponse.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? roleCategoryResponse.reponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - GetAllRoles End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                roleCategoryReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(roleCategoryResponse, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteUser(string userID)
        {
            UserRegisterResponse userRegisterResponse = new UserRegisterResponse();
            UserRegister userdetails = new UserRegister();
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DeleteUser Start");
                userdetails.userID = userID;
                if (clientSetting.mvnoSettings.userManagementType.ToUpper() == "ON")
                {
                    userdetails.CreatedBy = Session["UserName"].ToString();
                    userdetails.RoleCategoryType = Session["ROLE_CAT_TYPE"].ToString();
                }
                userRegisterResponse = CRMUserRegister(userdetails, "DEL");
                ///FRR--3083
                if (userRegisterResponse != null && userRegisterResponse.reponseDetails != null && userRegisterResponse.reponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("userRegisterDEL_" + userRegisterResponse.reponseDetails.ResponseCode);
                    userRegisterResponse.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? userRegisterResponse.reponseDetails.ResponseDesc : errorInsertMsg;
                }
                ///FRR--3083
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DeleteUser End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                userdetails = null;
                errorInsertMsg = string.Empty;
            }
            return Json(userRegisterResponse, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult Configuration()
        {
            GetSpecificConfigRes specificConfigRes = new GetSpecificConfigRes();
            GetSpecificConfigReq specificConfigReq = new GetSpecificConfigReq();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - Configuration Start");
                specificConfigReq.BrandCode = SettingsCRM.brandCode;
                specificConfigReq.CountryCode = SettingsCRM.countryCode;
                specificConfigReq.LanguageCode = SettingsCRM.langCode;
                specificConfigReq.RequestType = "Q";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                specificConfigRes = serviceCRM.GetSpecificConfigDetails(specificConfigReq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - Configuration End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                specificConfigReq = null;
                serviceCRM = null;
            }
            return PartialView(specificConfigRes.countryBrandList);
        }
        public JsonResult BrandConfiguration(string countryCode, string brandCode)
        {
            GetSpecificConfigRes specificConfigRes = new GetSpecificConfigRes();
            GetSpecificConfigReq specificConfigReq = new GetSpecificConfigReq();
            bool isConfigReq = true;
            ServiceInvokeCRM serviceCRM;
            ConfigSettings configSettings;
            CRMBase crmBaseReq;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - BrandConfiguration Start");
                Session["ConfigCountryCode"] = countryCode;
                Session["ConfigBrandCode"] = brandCode;
                specificConfigReq.BrandCode = brandCode;
                specificConfigReq.CountryCode = countryCode;
                specificConfigReq.LanguageCode = SettingsCRM.langCode;
                specificConfigReq.RequestType = "G";
                specificConfigReq.ConfigType = "ALL";
                if (ConfigCRM.configSettings.countrySettings != null)
                {
                    if (ConfigCRM.configSettings.countrySettings.Any(c => c.countryCode == countryCode))
                    {
                        if (ConfigCRM.configSettings.countrySettings.Where(c => c.countryCode == countryCode).Any(b => b.brandSettings.Any(d => d.brandCode == brandCode)))
                        {
                            isConfigReq = false;
                        }
                        else
                        {
                            isConfigReq = true;
                        }
                    }
                    else
                    {
                        isConfigReq = true;
                    }
                }
                else
                {
                    isConfigReq = true;
                }

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                if (isConfigReq)
                {
                    configSettings = new ConfigSettings();
                    crmBaseReq = new CRMBase();
                    crmBaseReq.CountryCode = countryCode;
                    crmBaseReq.BrandCode = brandCode;
                    crmBaseReq.LanguageCode = SettingsCRM.langCode;
                    configSettings = serviceCRM.AuthenticateUser(crmBaseReq);
                    if (configSettings.ResponseCode == "0")
                    {
                        if (ConfigCRM.configSettings.countrySettings != null)
                        {
                            if (ConfigCRM.configSettings.countrySettings.Any(c => c.countryCode == countryCode))
                            {
                                if (ConfigCRM.configSettings.countrySettings.Where(c => c.countryCode == countryCode).Any(b => b.brandSettings.Any(d => d.brandCode == brandCode)))
                                {
                                    ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == countryCode).brandSettings.RemoveAll(b => b.brandCode == brandCode);
                                    ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == countryCode).brandSettings.Add(configSettings.countrySettings.FirstOrDefault().brandSettings.FirstOrDefault());
                                }
                                else
                                {
                                    ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == countryCode).brandSettings.Add(configSettings.countrySettings.FirstOrDefault().brandSettings.FirstOrDefault());
                                }
                            }
                            else
                            {
                                ConfigCRM.configSettings.countrySettings.Add(configSettings.countrySettings.FirstOrDefault());
                            }
                        }
                        else
                        {
                            ConfigCRM.configSettings.countrySettings = new List<CountrySettings>();
                            ConfigCRM.configSettings.countrySettings.Add(configSettings.countrySettings.FirstOrDefault());
                        }
                    }
                }
                specificConfigRes = serviceCRM.GetSpecificConfigDetails(specificConfigReq);
                if (specificConfigRes.Response.ResponseCode == "0")
                {
                    Int32 i = 0;
                    specificConfigRes.ConfigKeysandDetails.ForEach(cd => cd.id = (i++).ToString());
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - BrandConfiguration End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                serviceCRM = null;
                specificConfigReq = null;
                configSettings = null;
                crmBaseReq = null;
            }
            return Json(specificConfigRes.ConfigKeysandDetails, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateConfiguration(ConfigKeysandDetails configKeyDetails)
        {
            GetSpecificConfigRes specificConfigRes = new GetSpecificConfigRes();
            GetSpecificConfigReq specificConfigReq = new GetSpecificConfigReq();
            ServiceInvokeCRM serviceCRM;
            PrePaidSettings prePaidSetting;
            PostPaidSettings postPaidSetting;
            SettingsMVNO mvnoSetting;
            CorporateSettings corporateSettings;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - UpdateConfiguration Start");
                specificConfigReq.BrandCode = Convert.ToString(Session["ConfigBrandCode"]);
                specificConfigReq.CountryCode = Convert.ToString(Session["ConfigCountryCode"]);
                specificConfigReq.LanguageCode = SettingsCRM.langCode;
                specificConfigReq.RequestType = "U";
                specificConfigReq.isRequired = SettingsCRM.tableConfigKeys.Contains(configKeyDetails.Key) ? "0" : "1";
                specificConfigReq.configKeyDetails = configKeyDetails;
                specificConfigReq.ModifiedBy = Session["UserName"].ToString();

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                specificConfigRes = serviceCRM.GetSpecificConfigDetails(specificConfigReq);
                if (specificConfigRes != null && specificConfigRes.Response != null && specificConfigRes.Response.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("BrandConfig_" + specificConfigRes.Response.ResponseCode);
                    specificConfigRes.Response.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? specificConfigRes.Response.ResponseDesc : errorInsertMsg;
                }

                if (specificConfigRes.Response.ResponseCode == "0")
                {
                    if (specificConfigReq.isRequired == "1")
                    {
                        if (specificConfigReq.configKeyDetails.SIMType.Trim().ToUpper() == "PREPAID")
                        {
                            prePaidSetting = ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == specificConfigReq.CountryCode).brandSettings.FirstOrDefault(b => b.brandCode == specificConfigReq.BrandCode).preSettings;
                            ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == specificConfigReq.CountryCode).brandSettings.FirstOrDefault(b => b.brandCode == specificConfigReq.BrandCode).preSettings.GetType().GetProperties().FirstOrDefault(p => ((KeyValueAttribute)(p.GetCustomAttributes(typeof(KeyValueAttribute), false).FirstOrDefault())).configKey == specificConfigReq.configKeyDetails.Key).SetValue(prePaidSetting, specificConfigReq.configKeyDetails.Value, null);
                        }
                        else if (specificConfigReq.configKeyDetails.SIMType.Trim().ToUpper() == "POSTPAID")
                        {
                            postPaidSetting = ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == specificConfigReq.CountryCode).brandSettings.FirstOrDefault(b => b.brandCode == specificConfigReq.BrandCode).postSettings;
                            ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == specificConfigReq.CountryCode).brandSettings.FirstOrDefault(b => b.brandCode == specificConfigReq.BrandCode).postSettings.GetType().GetProperties().FirstOrDefault(p => ((KeyValueAttribute)(p.GetCustomAttributes(typeof(KeyValueAttribute), false).FirstOrDefault())).configKey == specificConfigReq.configKeyDetails.Key).SetValue(postPaidSetting, specificConfigReq.configKeyDetails.Value, null);
                        }
                        else if (specificConfigReq.configKeyDetails.SIMType.Trim().ToUpper() == "MVNO")
                        {
                            mvnoSetting = ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == specificConfigReq.CountryCode).brandSettings.FirstOrDefault(b => b.brandCode == specificConfigReq.BrandCode).mvnoSettings;
                            ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == specificConfigReq.CountryCode).brandSettings.FirstOrDefault(b => b.brandCode == specificConfigReq.BrandCode).mvnoSettings.GetType().GetProperties().FirstOrDefault(p => ((KeyValueAttribute)(p.GetCustomAttributes(typeof(KeyValueAttribute), false).FirstOrDefault())).configKey == specificConfigReq.configKeyDetails.Key).SetValue(mvnoSetting, specificConfigReq.configKeyDetails.Value, null);
                        }
                        else if (specificConfigReq.configKeyDetails.SIMType.Trim().ToUpper() == "CORPORATE")
                        {
                            corporateSettings = ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == specificConfigReq.CountryCode).brandSettings.FirstOrDefault(b => b.brandCode == specificConfigReq.BrandCode).corpSettings;
                            ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == specificConfigReq.CountryCode).brandSettings.FirstOrDefault(b => b.brandCode == specificConfigReq.BrandCode).corpSettings.GetType().GetProperties().FirstOrDefault(p => ((KeyValueAttribute)(p.GetCustomAttributes(typeof(KeyValueAttribute), false).FirstOrDefault())).configKey == specificConfigReq.configKeyDetails.Key).SetValue(corporateSettings, specificConfigReq.configKeyDetails.Value, null);
                        }
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - UpdateConfiguration End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == Convert.ToString(Session["ConfigCountryCode"])).brandSettings.ForEach(b => b = b.brandCode == Convert.ToString(Session["ConfigBrandCode"]) ? b = null : b);
            }
            finally
            {
                specificConfigReq = null;
                serviceCRM = null;
                prePaidSetting = null;
                postPaidSetting = null;
                mvnoSetting = null;
                corporateSettings = null;
                errorInsertMsg = string.Empty;
            }
            return Json(specificConfigRes.Response, JsonRequestBehavior.AllowGet);
        }

      
        public ViewResult AuditTrailReport()
        {
            AuditTrailReportResponse auditTrailReportResp = new AuditTrailReportResponse();
            AuditTrailReportRequest auditTrailReportReq = new AuditTrailReportRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - AuditTrailReport start");
                auditTrailReportReq.CountryCode = clientSetting.countryCode;
                auditTrailReportReq.BrandCode = clientSetting.brandCode;
                auditTrailReportReq.LanguageCode = SettingsCRM.langCode;
                auditTrailReportReq.mode = "Q";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                auditTrailReportResp = serviceCRM.AuditTrailReportCRM(auditTrailReportReq);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - AuditTrailReport End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                auditTrailReportReq = null;
                serviceCRM = null;
            }
            return View(auditTrailReportResp.auditTrailQuery);
        }

        [HttpPost]
        public JsonResult AuditTrailReport(AuditTrailReportRequest auditTrailReportReq)
        {
            AuditTrailReportResponse auditTrailReportResp = new AuditTrailReportResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            string errorInsert = string.Empty;
            string[] strSplit;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - Json AuditTrailReport Start");
                auditTrailReportReq.CountryCode = clientSetting.countryCode;
                auditTrailReportReq.BrandCode = clientSetting.brandCode;
                auditTrailReportReq.LanguageCode = SettingsCRM.langCode;
                if (clientSetting.countryCode == "BRA")
                    auditTrailReportReq.mode = "A";
                else
                    auditTrailReportReq.mode = "R";
                auditTrailReportReq.action = "";
                auditTrailReportReq.fromDate = Utility.FormatDateTimeToService(auditTrailReportReq.fromDate, clientSetting.mvnoSettings.dateTimeFormat);
                auditTrailReportReq.toDate = Utility.FormatDateTimeToService(auditTrailReportReq.toDate, clientSetting.mvnoSettings.dateTimeFormat);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                auditTrailReportResp = serviceCRM.AuditTrailReportCRM(auditTrailReportReq);
                if (auditTrailReportResp != null && auditTrailReportResp.auditTrailReport != null)
                {
                    for (int i = 0; i < auditTrailReportResp.auditTrailReport.Count(); i++)
                    {
                        errorInsertMsg = string.Empty;
                        if (auditTrailReportResp.auditTrailReport[i].DescID.Contains("|"))
                        {
                            strSplit = auditTrailReportResp.auditTrailReport[i].DescID.Split('|');
                            foreach (string str in strSplit)
                            {
                                errorInsert = string.Empty;
                                errorInsert = Resources.AuditTrialCRMResources.ResourceManager.GetString(str);
                                errorInsertMsg += string.IsNullOrEmpty(errorInsert) ? str : (errorInsert + " ");
                            }
                        }
                        else
                        {
                            errorInsertMsg = Resources.AuditTrialCRMResources.ResourceManager.GetString(auditTrailReportResp.auditTrailReport[i].DescID);
                        }
                        auditTrailReportResp.auditTrailReport[i].DescID = string.IsNullOrEmpty(errorInsertMsg) ? auditTrailReportResp.auditTrailReport[i].DescID : errorInsertMsg;
                        auditTrailReportResp.auditTrailReport[i].actionDate = Utility.GetDateconvertion(auditTrailReportResp.auditTrailReport[i].actionDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                    }
                }
                try
                {
                    auditTrailReportResp.auditTrailReport.ForEach(atr => atr.actionDate = Utility.FormatDateTime(atr.actionDate, clientSetting.mvnoSettings.dateTimeFormat));
                }
                catch (Exception exDateFormat)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exDateFormat);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - Json AuditTrailReport End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                serviceCRM = null;
                errorInsertMsg = string.Empty;
                errorInsert = string.Empty;
                strSplit = null;
            }
            return new JsonResult() { Data = auditTrailReportResp.auditTrailReport, MaxJsonLength = int.MaxValue };
        }

        [HttpPost]
        public JsonResult AuditTrailReportGet(string auditTrail)
        {
            // GERRegistration objReg = JsonConvert.DeserializeObject<GERRegistration>(Registration);
            AuditTrailReportRequest auditTrailReportReq = JsonConvert.DeserializeObject<AuditTrailReportRequest>(auditTrail);
            AuditTrailReportResponse auditTrailReportResp = new AuditTrailReportResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - AuditTrailReportGet start");
                auditTrailReportReq.CountryCode = clientSetting.countryCode;
                auditTrailReportReq.BrandCode = clientSetting.brandCode;
                auditTrailReportReq.LanguageCode = SettingsCRM.langCode;
                auditTrailReportReq.mode = "A";
                auditTrailReportReq.action = "";
                auditTrailReportReq.fromDate = Utility.FormatDateTimeToService(auditTrailReportReq.fromDate, clientSetting.mvnoSettings.dateTimeFormat);
                auditTrailReportReq.toDate = Utility.FormatDateTimeToService(auditTrailReportReq.toDate, clientSetting.mvnoSettings.dateTimeFormat);
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                auditTrailReportResp = serviceCRM.AuditTrailReportCRM(auditTrailReportReq);
                if (auditTrailReportResp != null && auditTrailReportResp.auditTrailReport != null)
                {
                    for (int i = 0; i < auditTrailReportResp.auditTrailReport.Count(); i++)
                    {
                        errorInsertMsg = Resources.AuditTrialCRMResources.ResourceManager.GetString(auditTrailReportResp.auditTrailReport[i].DescID);
                        auditTrailReportResp.auditTrailReport[i].DescID = string.IsNullOrEmpty(errorInsertMsg) ? auditTrailReportResp.auditTrailReport[i].DescID : errorInsertMsg;
                        auditTrailReportResp.auditTrailReport[i].actionDate = Utility.GetDateconvertion(auditTrailReportResp.auditTrailReport[i].actionDate, "yyyy-mm-dd", false, clientSetting.mvnoSettings.dateTimeFormat);
                    }
                }
                try
                {
                    auditTrailReportResp.auditTrailReport.ForEach(atr => atr.actionDate = Utility.FormatDateTime(atr.actionDate, clientSetting.mvnoSettings.dateTimeFormat));
                }
                catch (Exception exAuditTrailReportGetDateFormat)
                {
                    CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exAuditTrailReportGetDateFormat);
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - AuditTrailReportGet End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                auditTrailReportReq = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return new JsonResult() { Data = auditTrailReportResp.auditTrailReport, MaxJsonLength = int.MaxValue };
        }


        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadAuditTrailReport(string auditTrailReport)
        {
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadAuditTrailReport Start");
                GridView gridView = new GridView();
                List<AuditTrailReport> lstAuditTrailReport = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<AuditTrailReport>>(auditTrailReport);
                gridView.DataSource = lstAuditTrailReport;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "AuditTrailReport", this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadAuditTrailReport End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
        }


        // 4710 

        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadAuditTrailDetails(string auditTrailReport)
        {
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadAuditTrailReport Start");
                GridView gridView = new GridView();
                List<AuditTrailReport> lstAuditTrailReport = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<AuditTrailReport>>(auditTrailReport);

                // List<AuditTrailDetail> lstAuditTrailDetails = new List<AuditTrailDetail>();

                List<AuditTrailDetail> lstAuditTrailDetails = lstAuditTrailReport.Where(m => m.userId != null)
                 .Select(m => new AuditTrailDetail
                  {
                      action = m.action,
                      userId = m.userId,
                      module = m.module,
                      subModule = m.subModule,
                      actionDate = m.actionDate,
                      Group = m.Group
                  }).ToList();

                gridView.DataSource = lstAuditTrailDetails;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "AuditTrailReport", this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadAuditTrailDetails End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
        }



        #region FRR 4928

      
        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadPortoutRefundReport(string TempObj)
        {
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadAuditTrailReport Start");
                GridView gridView = new GridView();
                List<PortoutRefundReport_list> lstPortoutRefundReporReport = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<PortoutRefundReport_list>>(TempObj);

                List<DownloadPortoutRefundReport> lstAuditTrailDetails = lstPortoutRefundReporReport.Where(a => a.Id != null)
                .Select(a => new DownloadPortoutRefundReport
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Msisdn = a.Msisdn,
                    BankAccNO = a.BankAccNO,
                    AccountHolderFirstName = a.AccountHolderFirstName,
                    AccountHolderLastName = a.AccountHolderLastName,
                    DateOfPortOut = a.DateOfPortOut,
                    DateOfRefundRequest = a.DateOfRefundRequest,
                    PrepaidBalance = a.PrepaidBalance,
                    AdminFee = a.AdminFee,
                    EligibileRefundAmt = a.EligibileRefundAmt,
                    SwiftCode = a.SwiftCode,
                    IBAN = a.IBAN,
                    Country = a.Country,
                    Document = a.Document,
                    TransactionID = a.TransactionID,
                    submittedDate = a.submittedDate,
                    submittedBy = a.submittedBy,
                    authorizedBy = a.authorizedBy,
                    authorizedDate = a.authorizedDate,
                    CurrentStatus = a.CurrentStatus,
                    Level1 = a.Level1,
                    Level1ApprovalStatus = a.Level1ApprovalStatus,
                    Level2 = a.Level2,
                    Level2ApprovalStatus = a.Level2ApprovalStatus,
                    Reason = a.Reason


                }).ToList();

                gridView.DataSource = lstAuditTrailDetails;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "PortoutRefundReporReport", this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadPortoutRefundReport End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
        }

        #endregion

        #region FRR 4928



        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadPortoutPendingRefund(string downloadobj)
        {
            List<PendingAction> objlist = null;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadPortoutPendingRefund Start");
                GridView gridView = new GridView();
                PendingAction lstSubscriberStatusReport = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<PendingAction>(downloadobj);
                objlist = new List<PendingAction>();
                objlist.Add(lstSubscriberStatusReport);

                List<DownloadPortoutRefundReport> lstAuditTrailDetails = objlist.Where(a => a.Id != null)
               .Select(a => new DownloadPortoutRefundReport
               {
                   Id = a.Id,
                   FirstName = a.FirstName,
                   LastName = a.LastName,
                   Msisdn = a.Msisdn,
                   BankAccNO = a.BankAccNO,
                   AccountHolderFirstName = a.AccountHolderFirstName,
                   AccountHolderLastName = a.AccountHolderLastName,
                   DateOfPortOut = a.DateOfPortOut,
                   DateOfRefundRequest = a.DateOfRefundRequest,
                   PrepaidBalance = a.PrepaidBalance,
                   AdminFee = a.AdminFee,
                   EligibileRefundAmt = a.EligibileRefundAmt,
                   SwiftCode = a.SwiftCode,
                   IBAN = a.IBAN,
                   Country = a.Country,
                   Document = a.Document,
                   TransactionID = a.TransactionID,
                   submittedDate = a.submittedDate,
                   submittedBy = a.submittedBy,
                   authorizedBy = a.authorizedBy,
                   authorizedDate = a.authorizedDate,
                   CurrentStatus = a.CurrentStatus,
                   Level1 = a.Level1,
                   Level1ApprovalStatus = a.Level1ApprovalStatus,
                   Level2 = a.Level2,
                   Level2ApprovalStatus = a.Level2ApprovalStatus,
                   Reason = a.Reason

               }).ToList();

                gridView.DataSource = lstAuditTrailDetails;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "DownLoadPortoutPendingRefund", this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadPortoutPendingRefund End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
        }

        #endregion

        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadOnlineReport(string OnlineReport)
        {
            try
            {
                //GridView gridView = new GridView();
                //List<OnlineSearchRecord> lstOnlineReport = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<OnlineSearchRecord>>(OnlineReport);
                //gridView.DataSource = lstOnlineReport;
                //gridView.DataBind();
                //Utility.ExportToExcell(gridView, "DownLoadOnlineReport", this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadOnlineReport Start");
                GridView gridView = new GridView();
                List<OnlineSearchRecord> billHisDetails = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<OnlineSearchRecord>>(OnlineReport);
                OnlineSearchRecord billTemp = new OnlineSearchRecord();
                var props = billTemp.GetType().GetProperties();
                foreach (var prop in props)
                {
                    List<string> hideCol = new List<string>(new string[] { "ID", "cardid", "topupmode", "ErrorDesc", "ProcessedDate", "clientID", "PromoCode", "PromoType", "PromoDiscountType", "PromoDiscountAmount", "TAXAMOUNT", "EshopStatus", "Totalamount" });
                    if (!hideCol.Contains(prop.Name))
                    {
                        BoundField bfield = new BoundField();
                        bfield.HeaderText = prop.Name;
                        bfield.DataField = prop.Name;
                        gridView.Columns.Add(bfield);
                    }
                    else { }
                }
                gridView.AutoGenerateColumns = false;
                gridView.DataSource = billHisDetails;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "DownLoadOnlineReport_" + DateTime.Now.ToString("yyyy-MM-ddHHmmss"), this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadOnlineReport End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
        }
        #region 4212
        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadPortinofferReport(string PortinOfferReport)
        {
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadPortinofferReport Start");
                GridView gridView = new GridView();
                List<PortinofferReport> lstPortinofferReport = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<PortinofferReport>>(PortinOfferReport);
                gridView.DataSource = lstPortinofferReport;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "DownLoadPortinofferReport", this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadPortinofferReport End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
        }



        #endregion

        [HttpPost]
        public void DownLoadUserManagement(string userData, string filterdata)
        {
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadUserManagement Start");
                GridView gridView = new GridView();
                //TopupFailure[] topupFailure = new JavaScriptSerializer().Deserialize<TopupFailure[]>(topupData);
                List<UserRegister> UserData = JsonConvert.DeserializeObject<List<UserRegister>>(userData);
                UserRegister uservalues = new UserRegister();
                UserData.ForEach(a =>
                {
                    a.createdDate = Utility.GetDateconvertion(a.createdDate, "MM/DD/YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);
                    a.modifiedDate = Utility.GetDateconvertion(a.modifiedDate, "MM/DD/YYYY", false, clientSetting.mvnoSettings.dateTimeFormat);
                    //PID
                    var str = a.firstName.ToString();
                    if (str.Contains('"'))
                    { 
                    int index = str.IndexOf('"');
                    a.firstName = a.firstName.Remove(index,1).Insert(index, "'");
                }
                });
                var props = uservalues.GetType().GetProperties();
                string[] strList = filterdata.Split(',');
                UserData = UserData.Join(strList, a => a.userID, b => b.ToString(), (a, b) => a).ToList();
                foreach (var prop in props)
                {
                    List<string> hideCol = new List<string>(new string[] { "CreatedBy", "RoleCategoryType", "AdminID", "UserAccessId", "UserGroupId", "RestrictUserCreinTicketing", "userID", "password", "countryCode", "loginAttempts", "isPasswordExpired", "roleCatId", "lastPwdChangeDate" });
                    if (!hideCol.Contains(prop.Name))
                    {
                        BoundField bfield = new BoundField();
                        bfield.HeaderText = prop.Name;
                        bfield.DataField = prop.Name;
                        gridView.Columns.Add(bfield);
                    }
                    else { }
                }
                gridView.AutoGenerateColumns = false;
                gridView.DataSource = UserData;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "UserDetailsDownload_", this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadUserManagement End");
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
            }
        }

        public ActionResult Promotions()
        {
            PromotionsRequest requestObject = new PromotionsRequest();
            PromotionsResponse responseObject = new PromotionsResponse();
            ServiceInvokeCRM serviceCRM;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - Promotions Start");
                requestObject.CountryCode = clientSetting.countryCode;
                requestObject.BrandCode = clientSetting.brandCode;
                requestObject.LanguageCode = SettingsCRM.langCode;
                requestObject.Mode = "GET";
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                responseObject = serviceCRM.CRMPromotions(requestObject);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - Promotions End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                requestObject = null;
                serviceCRM = null;
            }
            return View(responseObject);
        }

        [HttpPost]
        public JsonResult PostPromotion(string jsonRequestObject)
        {
            PromotionsRequest requestObject = new PromotionsRequest();
            PromotionsResponse responseObject = new PromotionsResponse();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - PostPromotion Start");
                requestObject = new JavaScriptSerializer().Deserialize<PromotionsRequest>(jsonRequestObject);
                requestObject.CountryCode = clientSetting.countryCode;
                requestObject.BrandCode = clientSetting.brandCode;
                requestObject.LanguageCode = SettingsCRM.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                responseObject = serviceCRM.CRMPromotions(requestObject);
                if (responseObject != null && responseObject.ResponseDetails != null && responseObject.ResponseDetails.ResponseCode != null)
                {
                    if (!_runningFromNUnit)
                    {
                        errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("Promotions_" + responseObject.ResponseDetails.ResponseCode);
                        responseObject.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? responseObject.ResponseDetails.ResponseDesc : errorInsertMsg;
                    }
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - PostPromotion End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                requestObject = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(responseObject, JsonRequestBehavior.AllowGet);
        }

        #region MDB Table Configuration

        [HttpGet]
        public PartialViewResult TableConfiguration()
        {
            GetSpecificConfigRes specificConfigRes = new GetSpecificConfigRes();
            GetSpecificConfigReq specificConfigReq = new GetSpecificConfigReq();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                specificConfigReq.BrandCode = SettingsCRM.brandCode;
                specificConfigReq.CountryCode = SettingsCRM.countryCode;
                specificConfigReq.LanguageCode = SettingsCRM.langCode;
                specificConfigReq.RequestType = "Q";

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    specificConfigRes = serviceCRM.GetSpecificConfigDetails(specificConfigReq);
                
                return PartialView(specificConfigRes.countryBrandList);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return PartialView(specificConfigRes.countryBrandList);
            }
            finally
            {
                specificConfigReq = null;
                // specificConfigRes = null;
                serviceCRM = null;

            }
        }

        public JsonResult TableConfigurationReport(string countryCode, string brandCode)
        {
            TableConfigurationResponse ObjResp = new TableConfigurationResponse();
            TableConfigurationRequest ObjReq = new TableConfigurationRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                bool isConfigReq = true;

                if (ConfigCRM.configSettings.countrySettings != null)
                {
                    if (ConfigCRM.configSettings.countrySettings.Any(c => c.countryCode == countryCode))
                    {
                        if (ConfigCRM.configSettings.countrySettings.Where(c => c.countryCode == countryCode).Any(b => b.brandSettings.Any(d => d.brandCode == brandCode)))
                        {
                            isConfigReq = false;
                        }
                        else
                        {
                            isConfigReq = true;
                        }
                    }
                    else
                    {
                        isConfigReq = true;
                    }
                }
                else
                {
                    isConfigReq = true;
                }

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    if (isConfigReq)
                    {
                        ConfigSettings configSettings = new ConfigSettings();

                        CRMBase crmBaseReq = new CRMBase();
                        crmBaseReq.CountryCode = countryCode;
                        crmBaseReq.BrandCode = brandCode;
                        crmBaseReq.LanguageCode = SettingsCRM.langCode;

                        configSettings = serviceCRM.AuthenticateUser(crmBaseReq);

                        if (configSettings.ResponseCode == "0")
                        {
                            if (ConfigCRM.configSettings.countrySettings != null)
                            {
                                if (ConfigCRM.configSettings.countrySettings.Any(c => c.countryCode == countryCode))
                                {
                                    if (ConfigCRM.configSettings.countrySettings.Where(c => c.countryCode == countryCode).Any(b => b.brandSettings.Any(d => d.brandCode == brandCode)))
                                    {
                                        ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == countryCode).brandSettings.RemoveAll(b => b.brandCode == brandCode);
                                        ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == countryCode).brandSettings.Add(configSettings.countrySettings.FirstOrDefault().brandSettings.FirstOrDefault());
                                    }
                                    else
                                    {
                                        ConfigCRM.configSettings.countrySettings.FirstOrDefault(c => c.countryCode == countryCode).brandSettings.Add(configSettings.countrySettings.FirstOrDefault().brandSettings.FirstOrDefault());
                                    }
                                }
                                else
                                {
                                    ConfigCRM.configSettings.countrySettings.Add(configSettings.countrySettings.FirstOrDefault());
                                }
                            }
                            else
                            {
                                ConfigCRM.configSettings.countrySettings = new List<CountrySettings>();
                                ConfigCRM.configSettings.countrySettings.Add(configSettings.countrySettings.FirstOrDefault());
                            }
                        }
                    }
                

                ObjReq.CountryCode = countryCode;
                ObjReq.BrandCode = brandCode;
                ObjReq.mode = "Q";
                using (ServiceInvokeCRM crmNewService = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl))
                {
                    ObjResp = crmNewService.TableConfigurationCRM(ObjReq);
                }
                Session["ConfigCountryCode"] = countryCode;
                Session["ConfigBrandCode"] = brandCode;
                return Json(ObjResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(ObjResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //ObjResp = null;
                ObjReq = null;
                serviceCRM = null;
            }



            //return PartialView(objResp);

        }

        public JsonResult GetTableConfiguration(string TableValue)
        {
            TableConfigurationResponse ObjResp = new TableConfigurationResponse();
            //s
            ServiceInvokeCRM crmNewService;
            try
            {
                TableConfigurationRequest ObjReq = JsonConvert.DeserializeObject<TableConfigurationRequest>(TableValue);

                ObjReq.CountryCode = Convert.ToString(Session["ConfigCountryCode"]);
                ObjReq.BrandCode = Convert.ToString(Session["ConfigBrandCode"]);
                ObjReq.LanguageCode = clientSetting.langCode;
                Session["TableTypeID"] = ObjReq.typeId;
                crmNewService = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    ObjResp = crmNewService.TableConfigurationCRM(ObjReq);
                

                if (ObjResp.responseDetails != null && ObjResp.tableProp != null && !string.IsNullOrEmpty(ObjResp.tableProp.tableRows))
                {
                    XmlDocument XDoc = new XmlDocument();
                    XDoc.LoadXml(ObjResp.tableProp.tableRows);
                    ObjResp.tableProp.tableRows = JsonConvert.SerializeXmlNode(XDoc);
                    //Vignesh
                    ObjResp.tableProp.tableRows = ObjResp.tableProp.tableRows.Replace("\"Table\":{", "\"Table\":[{");
                    ObjResp.tableProp.tableRows = ObjResp.tableProp.tableRows.Replace("}}}", "}]}}");
                    //Vignesh

                    if (ObjResp.tableProp.tableColModel.Count > 0)
                    {
                        ObjResp.tableProp.tableColModel[0].key = true;
                    }

                    for (int i = 0; i < ObjResp.tableProp.tableHeaders.Count(); i++)
                    {
                        string errorInsertMsg = Resources.SettingResource.ResourceManager.GetString("MDB_" + ObjReq.typeId + "_" + i);
                        ObjResp.tableProp.tableHeaders[i] = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.tableProp.tableHeaders[i] : errorInsertMsg;

                    }


                }

                if (ObjResp != null && ObjResp.responseDetails != null && ObjResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SQL_MDB_" + ObjResp.responseDetails.ResponseCode);
                    ObjResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? ObjResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                return Json(ObjResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
                return Json(ObjResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                // ObjResp = null;
                crmNewService = null;
            }

        }

        public JsonResult SaveTableConfiguration(FormCollection formCollxn)
        {
            TableConfigurationResponse tblConfigResp = new TableConfigurationResponse();
            TableConfigurationRequest tblConfigReq = new TableConfigurationRequest();
            //s
            ServiceInvokeCRM crmNewService;
            try
            {
                tblConfigReq.mode = (formCollxn["oper"] == "edit") ? "U" : (formCollxn["oper"] == "add") ? "I" : "D";
                tblConfigReq.typeId = Convert.ToString(Session["TableTypeID"]);

                if (tblConfigReq.mode == "D")
                {
                    tblConfigReq.xmlValue = formCollxn["masterID"];
                }
                else
                {

                    int formLength = formCollxn.AllKeys.Length;
                    StringBuilder sbXml = new StringBuilder();
                    sbXml.Append("<B><H>");

                    int len = 0;
                    if (tblConfigReq.mode == "I")
                    {
                        len = 1;
                    }
                    int cI = 1;
                    for (int i = len; i < formLength - 1; i++)
                    {
                        if (formCollxn.Keys[i].ToLower() == "id")
                        {
                            string formVal = formCollxn[i].Contains(",") ? formCollxn[i].Split(',')[0] : formCollxn[i];
                            sbXml.Append("<C" + cI + ">" + formVal + "</C" + cI + ">");
                        }
                        else
                        {
                            sbXml.Append("<C" + cI + ">" + formCollxn[i] + "</C" + cI + ">");
                        }
                        cI++;
                    }
                    sbXml.Append("</H></B>");

                    tblConfigReq.xmlValue = sbXml.ToString();
                }
                tblConfigReq.CountryCode = Convert.ToString(Session["ConfigCountryCode"]);
                tblConfigReq.BrandCode = Convert.ToString(Session["ConfigBrandCode"]);
                tblConfigReq.LanguageCode = clientSetting.langCode;

                crmNewService = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    tblConfigResp = crmNewService.TableConfigurationCRM(tblConfigReq);
                

                if (tblConfigResp != null && tblConfigResp.responseDetails != null && tblConfigResp.responseDetails.ResponseCode != null)
                {
                    string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("SQL_MDB_" + tblConfigResp.responseDetails.ResponseCode);
                    tblConfigResp.responseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? tblConfigResp.responseDetails.ResponseDesc : errorInsertMsg;
                }
                return Json(tblConfigResp.responseDetails);
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(tblConfigResp.responseDetails);
            }
            finally
            {
                //tblConfigResp = null;
                crmNewService = null;
            }
        }

        #endregion

        #region Admin Details

        public JsonResult GetAdminDetails()
        {
            AdminDetailsResponse adminDetailsResponse = new AdminDetailsResponse();
            AdminDetailsRequest adminDetailsRequest = new AdminDetailsRequest();
            ServiceInvokeCRM serviceCRM;
            string errorInsertMsg = string.Empty;
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - GetAdminDetails Start");
                adminDetailsRequest.BrandCode = clientSetting.brandCode;
                adminDetailsRequest.CountryCode = clientSetting.countryCode;
                adminDetailsRequest.LanguageCode = clientSetting.langCode;
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                if (clientSetting.mvnoSettings.userManagementType == "ON")
                {
                    adminDetailsRequest.RoleCategoryType = Session["ROLE_CAT_TYPE"].ToString();
                }
                adminDetailsResponse = serviceCRM.CRMGetAdminDetails(adminDetailsRequest);
                if (adminDetailsResponse != null && adminDetailsResponse.ResponseDetails != null && adminDetailsResponse.ResponseDetails.ResponseCode != null)
                {
                    errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UserCategories_" + adminDetailsResponse.ResponseDetails.ResponseCode);
                    adminDetailsResponse.ResponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? adminDetailsResponse.ResponseDetails.ResponseDesc : errorInsertMsg;
                }
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - GetAdminDetails End");
            }
            catch (Exception eX)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
            }
            finally
            {
                adminDetailsRequest = null;
                serviceCRM = null;
                errorInsertMsg = string.Empty;
            }
            return Json(adminDetailsResponse, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region 4268
        [HttpPost]
        [ValidateInput(false)]
        public void DownLoadSubscriberStatusReport(string SubscriberStatusReport)
        {
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadSubscriberStatusReport Start");
                GridView gridView = new GridView();
                List<SubscriberStatusReport> lstSubscriberStatusReport = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<SubscriberStatusReport>>(SubscriberStatusReport);
                gridView.DataSource = lstSubscriberStatusReport;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "DownLoadSubscriberStatusReport", this.HttpContext.Response);
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DownLoadSubscriberStatusReport End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
        }



        #endregion

      



        #region FRR-4345 SWAP IMSI SEARCH DOWNLOAD

        [HttpPost]
        [ValidateInput(false)]
        public void DOWNLOADIMSISEARCH(string Details)
        {
            try
            {
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "ResourceController - DOWNLOADIMSISEARCH Start");
                GridView gridView = new GridView();
                List<swapimsidetails> lstSwapImsiReport = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue }.Deserialize<List<swapimsidetails>>(Details);
                gridView.DataSource = lstSwapImsiReport;
                gridView.DataBind();
                Utility.ExportToExcell(gridView, "SEARCHSIMDOWNLOAD", this.HttpContext.Response);
                CRMLogger.WriteMessage(Session["UserName"].ToString(), this.ControllerContext, "ResourceController - DOWNLOADIMSISEARCH End");
            }
            catch (Exception ex)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, ex);
            }
        }

        #endregion

    }
}
