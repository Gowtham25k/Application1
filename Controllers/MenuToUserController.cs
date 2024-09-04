using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using ServiceCRM;
using CRM.Models;

namespace BeginCRM.Controllers
{
    [ValidateState]
    public class MenuToUserController : Controller
    {
        ClientSetting clientSetting = new ClientSetting();
        public static bool _runningFromNUnit = Service.UnitTestDetector._runningFromNUnit;

        public ActionResult MenuUserMapping()
        {
            return View();
        }

     
        public PartialViewResult MenuCategories()
        {
            MenuCategoryResponse menuCatgResp = new MenuCategoryResponse();
            MenuCategoryRequest menuCatgReq = new MenuCategoryRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                
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
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MenuCategoryGET_" + menuCatgResp.reponseDetails.ResponseCode);
                        menuCatgResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? menuCatgResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Menu Categories Success");
                return PartialView(menuCatgResp);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return PartialView(menuCatgResp);
            }
            finally
            {
               // menuCatgResp = null;
                menuCatgReq = null;
                serviceCRM = null;
            }
            
        }

        [HttpPost]
        public JsonResult UpdateSettingsMenuItems(string userCatgID, string settMenuItemsArray)
        {
            SelectListItem muItem = new SelectListItem();
            CRMResponse menuItemResp = new CRMResponse();
            RoleMgtMasterUpdateRequest menuItemReq = new RoleMgtMasterUpdateRequest();
            List<UpdateSettingsMenu> lstUpdateSettingsMenu = new List<UpdateSettingsMenu>();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                menuItemReq.CountryCode = clientSetting.countryCode;
                menuItemReq.BrandCode = clientSetting.brandCode;
                menuItemReq.LanguageCode = clientSetting.langCode;
                string[] totalMenuItems;
                totalMenuItems = settMenuItemsArray.Split(new[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                string[] splitMenuitems; char[] splitChar = { ',' };
                for (int i = 0; i < totalMenuItems.Count(); i++)
                {
                    UpdateSettingsMenu settingsMenu = new UpdateSettingsMenu();
                    splitMenuitems = totalMenuItems[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    settingsMenu.ROLE_MGMT_ID = splitMenuitems[0];
                    settingsMenu.ROLE_CAT_ID = userCatgID;
                    settingsMenu.EDIT_RIGHTS = splitMenuitems[1];
                    settingsMenu.ADMIN_RIGHTS = splitMenuitems[3];
                    lstUpdateSettingsMenu.Add(settingsMenu);
                }
                menuItemReq.settingMenuUpdate = lstUpdateSettingsMenu;

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    menuItemResp = serviceCRM.CRMUpdateRoleMgtMaster(menuItemReq);
                    ///FRR--3083
                    if (menuItemResp != null && menuItemResp.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UpdateSettingsMenuItems_" + menuItemResp.ResponseCode);
                        menuItemResp.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? menuItemResp.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                
                muItem.Value = menuItemResp.ResponseCode;
                muItem.Text = menuItemResp.ResponseDesc;

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Menu Items Success");
                return Json(muItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(muItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
               // muItem = null;
                menuItemResp = null;
                menuItemReq = null;
                lstUpdateSettingsMenu = null;
                serviceCRM = null;
            }
            
        }

        [HttpGet]
        public PartialViewResult MenuItems()
        {
            MenuItemsResponse menuItemResp = new MenuItemsResponse();
            MenuItemsRequest menuItemReq = new MenuItemsRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                
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
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MenuItemGET_" + menuItemResp.reponseDetails.ResponseCode);
                        menuItemResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? menuItemResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Menu Items Success");
                return PartialView(menuItemResp);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return PartialView(menuItemResp);
            }
            finally
            {
               // menuItemResp = null;
                menuItemReq = null;
                serviceCRM = null;
            }
            
        }

        [HttpGet]
        public JsonResult MenuList(string userCatgID, string catgLinkID)
        {
            List<string> lstMenuLinkID = new List<string>();
            RoleMenuCategoryItemResponse roleMenuCatgItemResp = new RoleMenuCategoryItemResponse();
            RoleMenuCategoryItemRequest roleMenuCatgItemReq = new RoleMenuCategoryItemRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {                
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                                    
                    roleMenuCatgItemReq.CountryCode = clientSetting.countryCode;
                    roleMenuCatgItemReq.BrandCode = clientSetting.brandCode;
                    roleMenuCatgItemReq.LanguageCode = clientSetting.langCode;
                    roleMenuCatgItemReq.roleCatID = userCatgID.Trim();
                    roleMenuCatgItemReq.menuCatID = catgLinkID.Trim();

                    roleMenuCatgItemReq.userName = Session["UserName"].ToString();
                    roleMenuCatgItemReq.SuperadminID = System.Configuration.ConfigurationManager.AppSettings["RootUserIDs"].Trim();
                    roleMenuCatgItemReq.LoginID = Convert.ToString(Session["UserID"]);
                    roleMenuCatgItemResp = serviceCRM.CRMRoleMenuCategoryItem(roleMenuCatgItemReq);
                    if (roleMenuCatgItemResp.reponseDetails.ResponseCode == "0")
                    {
                        roleMenuCatgItemResp.roleMenuCategoryItem.ForEach(m => lstMenuLinkID.Add(m.subCatgID));
                    }
                    else
                    {

                    }
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Menu list Success");
                return Json(lstMenuLinkID, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(lstMenuLinkID, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                roleMenuCatgItemResp = null;
                roleMenuCatgItemReq = null;
                serviceCRM = null;
            }
            
        }

        public JsonResult MenuListNew(string catgLinkID) //gopi2296
        {
            RoleMenuCategoryItemResponse roleMenuCatgItemResp = new RoleMenuCategoryItemResponse();
            RoleMenuCategoryItemRequest roleMenuCatgItemReq = new RoleMenuCategoryItemRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    
                    roleMenuCatgItemReq.CountryCode = clientSetting.countryCode;
                    roleMenuCatgItemReq.BrandCode = clientSetting.brandCode;
                    roleMenuCatgItemReq.LanguageCode = clientSetting.langCode;
                    roleMenuCatgItemReq.menuCatID = catgLinkID.Trim();
                    roleMenuCatgItemReq.SuperadminID = System.Configuration.ConfigurationManager.AppSettings["RootUserIDs"].Trim();
                    roleMenuCatgItemReq.LoginID = Convert.ToString(Session["UserID"]);
                    roleMenuCatgItemResp = serviceCRM.CRMRoleMenuCategoryItem(roleMenuCatgItemReq);
                    if (roleMenuCatgItemResp != null && roleMenuCatgItemResp.roleMenuCategoryItem != null)
                    {
                        List<RoleMenuCategoryItem> lstObj = new List<RoleMenuCategoryItem>();
                        for (int i = 0; i < roleMenuCatgItemResp.roleMenuCategoryItem.Count; i++)
                        {
                            RoleMenuCategoryItem lst = new RoleMenuCategoryItem();

                            lst.adminRights = roleMenuCatgItemResp.roleMenuCategoryItem[i].adminRights;
                            lst.CatID = roleMenuCatgItemResp.roleMenuCategoryItem[i].CatID;
                            lst.createdDate = roleMenuCatgItemResp.roleMenuCategoryItem[i].createdDate;
                            lst.editRights = roleMenuCatgItemResp.roleMenuCategoryItem[i].editRights;
                            lst.modifiedDate = roleMenuCatgItemResp.roleMenuCategoryItem[i].modifiedDate;
                            lst.subCatgID = roleMenuCatgItemResp.roleMenuCategoryItem[i].subCatgID;
                            lst.subCatgStyle = roleMenuCatgItemResp.roleMenuCategoryItem[i].subCatgStyle;
                            lst.subCatName = roleMenuCatgItemResp.roleMenuCategoryItem[i].subCatName;
                            lst.subCatOrder = roleMenuCatgItemResp.roleMenuCategoryItem[i].subCatOrder;
                            lst.subCatType = roleMenuCatgItemResp.roleMenuCategoryItem[i].subCatType;
                            lst.subCatURL = roleMenuCatgItemResp.roleMenuCategoryItem[i].subCatURL;
                            lst.subStatus = roleMenuCatgItemResp.roleMenuCategoryItem[i].subStatus;
                            if (lst.CatID == catgLinkID)
                            {
                                lstObj.Add(lst);
                            }
                        }
                        roleMenuCatgItemResp.roleMenuCategoryItem = null;
                        roleMenuCatgItemResp.roleMenuCategoryItem = lstObj;
                    }
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Menu list Success");
                return Json(roleMenuCatgItemResp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(roleMenuCatgItemResp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
              //  roleMenuCatgItemResp = null;
                roleMenuCatgItemReq = null;
                serviceCRM = null;
            }
        }

        [HttpGet]
        public JsonResult MenuListAll(string catgLinkID)
        {
            List<string> lstMenuLinkID = new List<string>();
            RoleMenuCategoryItemResponse roleMenuCatgItemResp = new RoleMenuCategoryItemResponse();
            RoleMenuCategoryItemRequest roleMenuCatgItemReq = new RoleMenuCategoryItemRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {                
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                                   
                    roleMenuCatgItemReq.CountryCode = clientSetting.countryCode;
                    roleMenuCatgItemReq.BrandCode = clientSetting.brandCode;
                    roleMenuCatgItemReq.LanguageCode = clientSetting.langCode;
                    roleMenuCatgItemReq.menuCatID = catgLinkID.Trim();
                    roleMenuCatgItemReq.roleCatID = Convert.ToString(Session["UserGroupID"]);
                    roleMenuCatgItemReq.userName = "";// Convert.ToString(Session["UserName"]);
                    roleMenuCatgItemReq.SuperadminID = System.Configuration.ConfigurationManager.AppSettings["RootUserIDs"].Trim();
                    roleMenuCatgItemReq.LoginID = Convert.ToString(Session["UserID"]);

                    roleMenuCatgItemResp = serviceCRM.CRMRoleMenuCategoryItem(roleMenuCatgItemReq);

                    if (roleMenuCatgItemResp.reponseDetails.ResponseCode == "0")
                    {
                        roleMenuCatgItemResp.roleMenuCategoryItem.ForEach(m => lstMenuLinkID.Add(m.subCatgID));
                    }
                    else
                    {

                    }
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Menu list Success");
                return Json(lstMenuLinkID, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(lstMenuLinkID, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                roleMenuCatgItemResp = null;
                roleMenuCatgItemReq = null;
                serviceCRM = null;
            }
            
        }

        [HttpGet]
        public PartialViewResult UserCategories()
        {
            RoleCategoryResponse roleCatgResp = new RoleCategoryResponse();
            RoleCategoryRequest roleCatgReq = new RoleCategoryRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
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
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UserCategories_" + roleCatgResp.reponseDetails.ResponseCode);
                        roleCatgResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? roleCatgResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083

                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "User Categories Success");
                return PartialView(roleCatgResp);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return PartialView(roleCatgResp);
            }
            finally
            {
              //  roleCatgResp = null;
                roleCatgReq = null;
                serviceCRM = null;
            }
            
        }


        public JsonResult MenutoUser(string userCatgID, string catgLinkID, string menuLinkIDs)
        {
            SelectListItem muItem = new SelectListItem();
            CRMResponse crmResp = new CRMResponse();
            ROLE_MENU_MAPRequest roleMenuMapReq = new ROLE_MENU_MAPRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {                
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                                    
                    roleMenuMapReq.CountryCode = clientSetting.countryCode;
                    roleMenuMapReq.BrandCode = clientSetting.brandCode;
                    roleMenuMapReq.LanguageCode = clientSetting.langCode;
                    roleMenuMapReq.ROLE_CAT_ID = userCatgID;
                    roleMenuMapReq.SUB_CAT_ID = menuLinkIDs;
                    roleMenuMapReq.MENU_CAT_ID = catgLinkID;

                    crmResp = serviceCRM.CRMRoleMenuMap(roleMenuMapReq);

                    ///FRR--3083
                    if (crmResp != null && crmResp.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MenutoUser_" + crmResp.ResponseCode);
                        crmResp.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? crmResp.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                    muItem.Value = crmResp.ResponseCode;
                    muItem.Text = crmResp.ResponseDesc;
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Menu to user Success");
                return Json(muItem, JsonRequestBehavior.AllowGet);
            }
            catch (Exception eX)
            {
                muItem.Value = "9";
                muItem.Text = eX.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(muItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
               // muItem = null;
                crmResp = null;
                roleMenuMapReq = null;
                serviceCRM = null;
            }
        }

        [HttpGet]
        public PartialViewResult MenuFavorites()
        {
            return PartialView();
        }

        [HttpGet]
        public PartialViewResult FavMenuItems()
        {
            MenuItemsResponse menuItemResp = new MenuItemsResponse();
            MenuItemsRequest menuItemReq = new MenuItemsRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
                
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
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MenuItemGET_" + menuItemResp.reponseDetails.ResponseCode);
                        menuItemResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? menuItemResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083

                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Menu Items Success");
                return PartialView(menuItemResp);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return PartialView(menuItemResp);
            }
            finally
            {
               // menuItemResp = null;
                menuItemReq = null;
                serviceCRM = null;
            }
            
        }

        [HttpGet]
        public PartialViewResult FavMenuCategories()
        {
            MenuCategoryResponse menuCatgResp = new MenuCategoryResponse();
            MenuCategoryRequest menuCatgReq = new MenuCategoryRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                
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
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("MenuCategoryGET_" + menuCatgResp.reponseDetails.ResponseCode);
                        menuCatgResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? menuCatgResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Menu Categories Success");
                return PartialView(menuCatgResp);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return PartialView(menuCatgResp);
            }
            finally
            {
                menuCatgReq = null;
               // menuCatgResp = null;
                serviceCRM = null;
            }
            
        }

        [HttpGet]
        public JsonResult FavMenuList(string userCatgID, string catgLinkID)
        {
            List<string> lstMenuLinkID = new List<string>();
            UserFavoritesMapPResponse userFavMapResp = new UserFavoritesMapPResponse();
            UserFavoritesMapRequest userFavReq = new UserFavoritesMapRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {               

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                                   
                    userFavReq.CountryCode = clientSetting.countryCode;
                    userFavReq.BrandCode = clientSetting.brandCode;
                    userFavReq.LanguageCode = clientSetting.langCode;
                    userFavReq.MENU_CAT_ID = catgLinkID;
                    userFavReq.ROLE_CAT_ID = userCatgID;
                    userFavReq.MODE = "GET";
                    //------------------ gopi2296 V_1.0.23.0----------------------------
                    userFavReq.UserID = Convert.ToString(Session["UserID"]);
                    if (@clientSetting.mvnoSettings.userManagementType.ToUpper() == "ON")
                    {
                        userFavReq.UserRoleID = "";
                    }
                    else
                    {
                        userFavReq.UserRoleID = System.Configuration.ConfigurationManager.AppSettings["RootUserIDs"].Trim();
                    }
                    //------------------------- End ------------------------------------

                    userFavMapResp = serviceCRM.CRMUserFavotitesMap(userFavReq);

                    userFavMapResp.UserFavorites.ForEach(m => lstMenuLinkID.Add(m.subCatID));
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Favorite Menu List Success");
                return Json(lstMenuLinkID, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(lstMenuLinkID, JsonRequestBehavior.AllowGet);
            }
            finally
            {
               // lstMenuLinkID = null;
                userFavMapResp = null;
                userFavReq = null;
                serviceCRM = null;

            }
            
        }

        [HttpGet]
        public PartialViewResult FavUserCategories()
        {
            RoleCategoryResponse roleCatgResp = new RoleCategoryResponse();
            RoleCategoryRequest roleCatgReq = new RoleCategoryRequest();
            ServiceInvokeCRM serviceCRM;
            try
            {
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
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UserCategories_" + roleCatgResp.reponseDetails.ResponseCode);
                        roleCatgResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? roleCatgResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                    ///

                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "User Categories Success");
                return PartialView(roleCatgResp);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return PartialView(roleCatgResp);
            }
            finally
            {
                //roleCatgResp = null;
                roleCatgReq = null;
                serviceCRM = null;
            }
            
        }


        public JsonResult FavMenutoUser(string userCatgID, string menuLinkIDs, string catgLinkID)
        {
            SelectListItem muItem = new SelectListItem();
            UserFavoritesMapPResponse userFavMapResp = new UserFavoritesMapPResponse();
            UserFavoritesMapRequest userFavReq = new UserFavoritesMapRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                

                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                                   
                    userFavReq.CountryCode = clientSetting.countryCode;
                    userFavReq.BrandCode = clientSetting.brandCode;
                    userFavReq.LanguageCode = clientSetting.langCode;
                    userFavReq.ROLE_CAT_ID = userCatgID;
                    userFavReq.MENU_CAT_ID = catgLinkID;
                    userFavReq.SUB_CAT_ID = menuLinkIDs;
                    userFavReq.MODE = "ADD";
                    //------------------ gopi2296 V_1.0.23.0----------------------------
                    userFavReq.UserID = Convert.ToString(Session["UserID"]);
                    if (@clientSetting.mvnoSettings.userManagementType.ToUpper() == "ON")
                    {
                        userFavReq.UserRoleID = "";
                    }
                    else
                    {
                        userFavReq.UserRoleID = System.Configuration.ConfigurationManager.AppSettings["RootUserIDs"].Trim();
                    }
                    //------------------------- End ------------------------------------

                    userFavMapResp = serviceCRM.CRMUserFavotitesMap(userFavReq);

                    ///FRR--3083
                    if (userFavMapResp != null && userFavMapResp.reponseDetails != null && userFavMapResp.reponseDetails.ResponseCode != null)
                    {
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("FavMenutoUser_" + userFavMapResp.reponseDetails.ResponseCode);
                        userFavMapResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? userFavMapResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083
                

                muItem.Value = userFavMapResp.reponseDetails.ResponseCode;
                muItem.Text = userFavMapResp.reponseDetails.ResponseDesc;
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Favorit Menu to user Success");
                return Json(muItem, JsonRequestBehavior.AllowGet);
            }

            catch (Exception eX)
            {
                muItem.Value = "1";
                muItem.Text = eX.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return Json(muItem, JsonRequestBehavior.AllowGet);
            }
            finally
            {
             //   muItem = null;
                userFavMapResp = null;
                userFavReq = null;
                serviceCRM = null;
            }
            
        }

        [HttpPost]
        public JsonResult MenuRoleDetailsCRM(string RoleId)
        {
            RoleDetailsResponse Resp = new RoleDetailsResponse();
            Resp.menuFeatureHeader = new List<MenuFeatureHeader>();
            RoleDetailsRequest req = new RoleDetailsRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    
                    req.CountryCode = clientSetting.countryCode;
                    req.BrandCode = clientSetting.brandCode;
                    req.LanguageCode = clientSetting.langCode;
                    req.RoleId = RoleId.Trim();
                    req.SuperadminID = System.Configuration.ConfigurationManager.AppSettings["RootUserIDs"].Trim();
                    req.LoginID = Convert.ToString(Session["UserID"]);

                    Resp = serviceCRM.RoleDetailsCRM(req);
                    List<string> newHeader = new List<string>();
                    newHeader = Resp.menuFeatureMaster.Select(p => p.SubCatDesc).Distinct().ToList();
                    if (newHeader.Count > 0)
                    {
                        for (int i = 0; i < newHeader.Count; i++)
                        {
                            MenuFeatureHeader lst = new MenuFeatureHeader();
                            lst.FeatureHeader = newHeader[i];
                            lst.FeatureHeaderURL = newHeader[i];
                            Resp.menuFeatureHeader.Add(lst);
                        }
                    }

                

                if (Resp != null)
                {
                    if (Resp.responseDetails != null)
                    {
                        string dummystring = System.Text.RegularExpressions.Regex.Replace( Resp.responseDetails.ResponseDesc, "[^0-9a-zA-Z]+", string.Empty);
                            string ResourceMsg = Resources.SettingResource.ResourceManager.GetString(dummystring);
                            Resp.responseDetails.ResponseDesc = string.IsNullOrEmpty(ResourceMsg) ? Resp.responseDetails.ResponseDesc : ResourceMsg;
                    }
                    if (Resp.menuCategory != null && Resp.menuCategory.Count > 0)
                    {
                        for (int i = 0; i < Resp.menuCategory.Count; i++)
                        {
                            string dummystring = System.Text.RegularExpressions.Regex.Replace( Resp.menuCategory[i].MenuCatDesc, "[^0-9a-zA-Z]+", string.Empty);
                            string ResourceMsg = Resources.SettingResource.ResourceManager.GetString(dummystring);
                            Resp.menuCategory[i].MenuCatDesc = string.IsNullOrEmpty(ResourceMsg) ? Resp.menuCategory[i].MenuCatDesc : ResourceMsg;
                        }
                    }

                    if (Resp.menuSubCategory != null && Resp.menuSubCategory.Count > 0)
                    {
                        for (int i = 0; i < Resp.menuSubCategory.Count; i++)
                        {
                            string dummystring = System.Text.RegularExpressions.Regex.Replace(Resp.menuSubCategory[i].SubCatDesc, "[^0-9a-zA-Z]+", string.Empty);
                            string ResourceMsg = Resources.SettingResource.ResourceManager.GetString(dummystring);
                            Resp.menuSubCategory[i].SubCatDesc = string.IsNullOrEmpty(ResourceMsg) ? Resp.menuSubCategory[i].SubCatDesc : ResourceMsg;
                        }
                    }


                    if (Resp.menuFeature != null && Resp.menuFeature.Count > 0)
                    {
                        for (int i = 0; i < Resp.menuFeature.Count; i++)
                        {
                            string dummystring = System.Text.RegularExpressions.Regex.Replace( Resp.menuFeature[i].Features, "[^0-9a-zA-Z]+", string.Empty);
                            string ResourceMsg = Resources.SettingResource.ResourceManager.GetString("Feature_" + dummystring);
                            Resp.menuFeature[i].SubCatDesc = string.IsNullOrEmpty(ResourceMsg) ? Resp.menuFeature[i].SubCatDesc : ResourceMsg;
                        }
                    }

                    if (Resp.menuFeatureHeader != null && Resp.menuFeatureHeader.Count > 0)
                    {
                        for (int i = 0; i < Resp.menuFeatureHeader.Count; i++)
                        {
                            string dummystring = System.Text.RegularExpressions.Regex.Replace( Resp.menuFeatureHeader[i].FeatureHeader, "[^0-9a-zA-Z]+", string.Empty);
                            string ResourceMsg = Resources.SettingResource.ResourceManager.GetString("FeatureHeader_" + dummystring);
                            Resp.menuFeatureHeader[i].FeatureHeader = string.IsNullOrEmpty(ResourceMsg) ? Resp.menuFeatureHeader[i].FeatureHeader : ResourceMsg;
                        }
                    }
                    if (Resp.menuFeatureMaster != null && Resp.menuFeatureMaster.Count > 0)
                    {
                        for (int i = 0; i < Resp.menuFeatureMaster.Count; i++)
                        {
                            string dummystring = System.Text.RegularExpressions.Regex.Replace( Resp.menuFeatureMaster[i].SubCatDesc, "[^0-9a-zA-Z]+", string.Empty);
                            string ResourceMsg = Resources.SettingResource.ResourceManager.GetString("FeatureHeader_" + dummystring);
                            Resp.menuFeatureMaster[i].SubCatDesc = string.IsNullOrEmpty(ResourceMsg) ? Resp.menuFeatureMaster[i].SubCatDesc : ResourceMsg;

                            string dummystring2 = System.Text.RegularExpressions.Regex.Replace( Resp.menuFeatureMaster[i].Features, "[^0-9a-zA-Z]+", string.Empty);
                            string ResourceMsg2 = Resources.SettingResource.ResourceManager.GetString("Feature_" + dummystring2);
                            Resp.menuFeatureMaster[i].Features = string.IsNullOrEmpty(ResourceMsg2) ? Resp.menuFeatureMaster[i].Features : ResourceMsg2;
                        }
                    }



                    if (Resp.menuSubCategory != null && Resp.menuSubCategory.Count > 0)
                    {
                        for (int i = 0; i < Resp.menuSubCategory.Count; i++)
                        {                            
                            string dummystring = System.Text.RegularExpressions.Regex.Replace(Resp.menuSubCategory[i].SubCatDesc, "[^0-9a-zA-Z]+", string.Empty);
                            string ResourceMsg = Resources.SettingResource.ResourceManager.GetString(dummystring);
                            Resp.menuSubCategory[i].SubCatDesc = string.IsNullOrEmpty(ResourceMsg) ? Resp.menuSubCategory[i].SubCatDesc : ResourceMsg;
                        }
                    }
                }

                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Menu list Success");
                return Json(Resp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(Resp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                //Resp = null;
                req = null;
                serviceCRM = null;
            }
            
        }

        [HttpPost]
        public JsonResult InsertUpdateRoleDetailsCRM(string reqValue)
        {
            InsertRoleDetailsResp Resp = new InsertRoleDetailsResp();
            Resp.responseDetails = new CRMResponse();
            //InsertRoleDetailsResp Resp = new InsertRoleDetailsResp();
            //s
            ServiceInvokeCRM serviceCRM;

            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    InsertRoleDetailsReq req = JsonConvert.DeserializeObject<InsertRoleDetailsReq>(reqValue);
                    req.CountryCode = clientSetting.countryCode;
                    req.BrandCode = clientSetting.brandCode;
                    req.LanguageCode = clientSetting.langCode;
                    req.UserName = Session["UserName"].ToString();

                    Resp = serviceCRM.InsertUpdateRoleDetails(req);
                    Session["MenuAndFeatures"] = Resp.MenuList;
                    Session["UserMenu"] = null;
                    Session["UserFavMenu"] = null;

                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "Menu list Success");
                return Json(Resp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return Json(Resp, JsonRequestBehavior.AllowGet);
            }
            finally
            {
               // Resp = null;
                serviceCRM = null;
            }
            
        }

        public PartialViewResult MenuRoleDetails()
        {
            return PartialView();
        }

        public PartialViewResult MenuRoleDetailsJS()
        {
            return PartialView();
        }

        public PartialViewResult FeaturesDetails()
        {
            return PartialView();
        }

        public PartialViewResult SettingDetails()
        {
            return PartialView();
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
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
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
                            roleCatgReq.ViewRightsOnly = userCatg.ViewRightsOnly;
                            if (clientSetting.countryCode.ToUpper() == "BRA")
                            {
                                roleCatgReq.ShowAnatelId = userCatg.ShowAnatelId;
                                roleCatgReq.GenAnatelId = userCatg.GenAnatelId;
                            }
                            roleCatgReq.RoleCategoryType = userCatg.RoleCategoryType;
                            roleCatgReq.CreatedBy = Session["UserName"].ToString();
                            roleCatgReq.EditRights = userCatg.EditRights;
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
                            roleCatgReq.ViewRightsOnly = userCatg.ViewRightsOnly;
                            if (clientSetting.countryCode.ToUpper() == "BRA")
                            {
                                roleCatgReq.ShowAnatelId = userCatg.ShowAnatelId;
                                roleCatgReq.GenAnatelId = userCatg.GenAnatelId;
                            }
                            roleCatgReq.RoleCategoryType = userCatg.RoleCategoryType;
                            roleCatgReq.CreatedBy = Session["UserName"].ToString();
                            roleCatgReq.EditRights = userCatg.EditRights;
                        }
                    }
                    else
                    {
                        roleCatgReq.MODE = "ADD";

                        roleCatgReq.ROLECATNAME = userCatg.ROLECATNAME;
                        roleCatgReq.STATUS = userCatg.STATUS;
                        roleCatgReq.ENABLEPASSWORDPOLICY = userCatg.ENABLEPASSWORDPOLICY;
                        roleCatgReq.ViewRightsOnly = userCatg.ViewRightsOnly;
                        if (clientSetting.countryCode.ToUpper() == "BRA")
                        {
                            roleCatgReq.ShowAnatelId = userCatg.ShowAnatelId;
                            roleCatgReq.GenAnatelId = userCatg.GenAnatelId;
                        }
                        roleCatgReq.RoleCategoryType = userCatg.RoleCategoryType;
                        roleCatgReq.CreatedBy = Session["UserName"].ToString();
                        roleCatgReq.EditRights = userCatg.EditRights;
                    }
                    roleCatgResp = serviceCRM.CRMRoleCategoryDetails(roleCatgReq);

                    if (!_runningFromNUnit)
                    {
                        ///FRR--3083
                        if (roleCatgResp != null && roleCatgResp.reponseDetails != null && roleCatgResp.reponseDetails.ResponseCode != null)
                        {
                            string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UserCategories_" + roleCatgResp.reponseDetails.ResponseCode);
                            roleCatgResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? roleCatgResp.reponseDetails.ResponseDesc : errorInsertMsg;
                        }
                        ///FRR--3083

                    }
                    modMsg.responseCode = Convert.ToInt32(roleCatgResp.reponseDetails.ResponseCode.Trim());
                    modMsg.responseMessage = roleCatgResp.reponseDetails.ResponseDesc;
                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "User Categories Success");
                return PartialView("MessageModal", modMsg);
            }
            catch (Exception eX)
            {
                modMsg.responseMessage = eX.Message;
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, eX);
                return PartialView("MessageModal", modMsg);
            }
            finally
            {
               // modMsg = null;
                roleCatgResp = null;
                roleCatgReq = null;
                serviceCRM = null;
            }
        }
        
        [HttpGet]
        public PartialViewResult AssignGroup()
        {
            RoleCategoryResponse roleCatgResp = new RoleCategoryResponse();
            RoleCategoryRequest roleCatgReq = new RoleCategoryRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                                    
                    roleCatgReq.CountryCode = clientSetting.countryCode;
                    roleCatgReq.BrandCode = clientSetting.brandCode;
                    roleCatgReq.LanguageCode = clientSetting.langCode;
                    roleCatgReq.MODE = "GetRoleMap";
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
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UserCategories_" + roleCatgResp.reponseDetails.ResponseCode);
                        roleCatgResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? roleCatgResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083

                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "User Categories Success");
                return PartialView(roleCatgResp);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return PartialView(roleCatgResp);
            }
            finally
            {
                //roleCatgResp = null;
                roleCatgReq = null;
                serviceCRM = null;
            }
        }

        [HttpGet]
        public PartialViewResult AssignRoleGroupMappingApproval()
        {
            RoleCategoryResponse roleCatgResp = new RoleCategoryResponse();
            RoleCategoryRequest roleCatgReq = new RoleCategoryRequest();
            //s
            ServiceInvokeCRM serviceCRM;
            try
            {
                serviceCRM = new ServiceInvokeCRM(SettingsCRM.crmServiceUrl);
                
                    roleCatgReq.CountryCode = clientSetting.countryCode;
                    roleCatgReq.BrandCode = clientSetting.brandCode;
                    roleCatgReq.LanguageCode = clientSetting.langCode;
                    roleCatgReq.MODE = "GetRoleForCRDR";
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
                        string errorInsertMsg = Resources.ErrorResources.ResourceManager.GetString("UserCategories_" + roleCatgResp.reponseDetails.ResponseCode);
                        roleCatgResp.reponseDetails.ResponseDesc = string.IsNullOrEmpty(errorInsertMsg) ? roleCatgResp.reponseDetails.ResponseDesc : errorInsertMsg;
                    }
                    ///FRR--3083

                
                CRMLogger.WriteMessage(Convert.ToString(Session["UserName"]), this.ControllerContext, "User Categories Success");
                return PartialView(roleCatgResp);
            }
            catch (Exception exp)
            {
                CRMLogger.WriteException(Convert.ToString(Session["UserName"]), this.ControllerContext, exp);
                return PartialView(roleCatgResp);
            }
            finally
            {
                //roleCatgResp = null;
                roleCatgReq = null;
                serviceCRM= null;
            }
        }

    }
}
