using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Products;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using RazorEngine.Compilation.ImpromptuInterface.InvokeExt;
using DotNetNuke.Entities.Users;

namespace OpenStore.Providers.OS_CartReview
{
    public class AjaxProvider : AjaxInterface
    {
        public override string Ajaxkey { get; set; }

        public override string ProcessCommand(string paramCmd, HttpContext context, string editlang = "")
        {
            if (!CheckRights())
            {
                return "Security Error.";
            }

            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var lang = NBrightBuyUtils.SetContextLangauge(ajaxInfo); // Ajax breaks context with DNN, so reset the context language to match the client.
            var objCtrl = new NBrightBuyController();

            var strOut = "OS_CartReview Ajax Error";

            // NOTE: The paramCmd MUST start with the plugin ref. in lowercase. (links ajax provider to cmd)
            switch (paramCmd)
            {
                case "os_cartreview_test":
                    strOut = "<root>" + UserController.Instance.GetCurrentUserInfo().Username + "</root>";
                    break;
                case "os_cartreview_getdata":
                    strOut = GetData(context);
                    break;
                case "os_cartreview_deleterecord":
                    strOut = DeleteData(context);
                    break;
                case "os_cartreview_savedata":
                    strOut = SaveData(context);
                    break;
                case "os_cartreview_selectlang":
                    strOut = SaveData(context);
                    break;
            }

            return strOut;

        }

        public override void Validate()
        {
        }

        #region "Methods"

        private String GetData(HttpContext context, bool clearCache = false)
        {

            var objCtrl = new NBrightBuyController();
            var strOut = "";
            //get uploaded params
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);

            var itemid = ajaxInfo.GetXmlProperty("genxml/hidden/itemid");
            var typeCode = ajaxInfo.GetXmlProperty("genxml/hidden/typecode");
            var newitem = ajaxInfo.GetXmlProperty("genxml/hidden/newitem");
            var selecteditemid = ajaxInfo.GetXmlProperty("genxml/hidden/selecteditemid");
            var moduleid = ajaxInfo.GetXmlProperty("genxml/hidden/moduleid");
            var editlang = ajaxInfo.GetXmlProperty("genxml/hidden/editlang");
            if (editlang == "") editlang = Utils.GetCurrentCulture();

            if (!Utils.IsNumeric(moduleid)) moduleid = "-2"; // use moduleid -2 for razor

            if (clearCache) NBrightBuyUtils.RemoveModCache(Convert.ToInt32(moduleid));

            if (newitem == "new") selecteditemid = AddNew(moduleid, typeCode);

            var templateControl = "/DesktopModules/NBright/OS_CartReview";

            if (Utils.IsNumeric(selecteditemid))
            {
                // do edit field data if a itemid has been selected
                var obj = objCtrl.Get(Convert.ToInt32(selecteditemid), "", editlang);
                var cartobj = new CartData(PortalSettings.Current.PortalId, "", obj.ItemID.ToString(""));
                strOut = NBrightBuyUtils.RazorTemplRender("datafields.cshtml", Convert.ToInt32(moduleid), itemid + editlang + selecteditemid, cartobj, templateControl, "config", editlang, StoreSettings.Current.Settings());
            }
            else
            {
                var filter = "";
                var searchText = ajaxInfo.GetXmlProperty("genxml/hidden/searchtext");
                if (searchText != "")
                {
                    filter += " and (    (([xmldata].value('(genxml/billaddress/genxml/textbox/firstname)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                    filter += " or (([xmldata].value('(genxml/billaddress/genxml/textbox/lastname)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                    filter += " or (([xmldata].value('(genxml/billaddress/genxml/textbox/unit)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                    filter += " or (([xmldata].value('(genxml/billaddress/genxml/textbox/street)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                    filter += " or (([xmldata].value('(genxml/billaddress/genxml/textbox/postalcode)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                    filter += " or (([xmldata].value('(genxml/billaddress/genxml/textbox/email)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                    filter += " or (([xmldata].value('(genxml/shipaddress/genxml/textbox/firstname)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                    filter += " or (([xmldata].value('(genxml/shipaddress/genxml/textbox/lastname)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                    filter += " or (([xmldata].value('(genxml/shipaddress/genxml/textbox/unit)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                    filter += " or (([xmldata].value('(genxml/shipaddress/genxml/textbox/street)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                    filter += " or (([xmldata].value('(genxml/shipaddress/genxml/textbox/postalcode)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                    filter += " or (([xmldata].value('(genxml/shipaddress/genxml/textbox/email)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                    filter += " or (([xmldata].value('(genxml/productrefs)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))";
                    filter += " or (([xmldata].value('(genxml/ordernumber)[1]', 'nvarchar(max)') like '%" + searchText + "%' collate sql_latin1_general_cp1_ci_ai ))  ) ";
                }

                var pagenumber = ajaxInfo.GetXmlPropertyInt("genxml/hidden/pagenumber");
                var pagesize = ajaxInfo.GetXmlPropertyInt("genxml/hidden/pagesize");

                if (pagenumber == 0) pagenumber = 1;
                if (pagesize == 0) pagesize = 20;

                // get only entity type required
                var recordcount = objCtrl.GetListCount(PortalSettings.Current.PortalId, -1, typeCode, filter);

                // Return list of items
                var l = objCtrl.GetList(PortalSettings.Current.PortalId, Convert.ToInt32(moduleid), typeCode, filter, " order by NB1.ModifiedDate DESC ", 0, pagenumber, pagesize, recordcount, editlang);
                strOut = NBrightBuyUtils.RazorTemplRenderList("datalist.cshtml", Convert.ToInt32(moduleid), editlang + pagenumber, l, templateControl, "config", editlang, StoreSettings.Current.Settings());

                if (recordcount > pagesize)
                {
                    var pg = new NBrightCore.controls.PagingCtrl();
                    strOut += pg.RenderPager(recordcount, pagesize, pagenumber);
                }
            }

            return strOut;

        }

        private String AddNew(String moduleid, String typeCode)
        {
            if (!Utils.IsNumeric(moduleid)) moduleid = "-2"; // -2 for razor

            var objCtrl = new NBrightBuyController();
            var nbi = new NBrightInfo(true);
            nbi.PortalId = PortalSettings.Current.PortalId;
            nbi.TypeCode = typeCode;
            nbi.ModuleId = Convert.ToInt32(moduleid);
            nbi.ItemID = -1;
            var itemId = objCtrl.Update(nbi);
            nbi.ItemID = itemId;

            foreach (var lang in DnnUtils.GetCultureCodeList(PortalSettings.Current.PortalId))
            {
                var nbi2 = new NBrightInfo(true);
                nbi2.PortalId = PortalSettings.Current.PortalId;
                nbi2.TypeCode = typeCode + "LANG";
                nbi2.ModuleId = Convert.ToInt32(moduleid);
                nbi2.ItemID = -1;
                nbi2.Lang = lang;
                nbi2.ParentItemId = itemId;
                nbi2.GUIDKey = "";
                nbi2.ItemID = objCtrl.Update(nbi2);
            }

            NBrightBuyUtils.RemoveModCache(nbi.ModuleId);

            return nbi.ItemID.ToString("");
        }

        private String SaveData(HttpContext context)
        {
            var objCtrl = new NBrightBuyController();

            //get uploaded params
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);

            var itemid = ajaxInfo.GetXmlProperty("genxml/hidden/itemid");
            var editlang = ajaxInfo.GetXmlProperty("genxml/hidden/editlang");
            if (editlang == "") editlang = Utils.GetCurrentCulture();

            if (Utils.IsNumeric(itemid))
            {
                // get DB record
                var nbi = objCtrl.Get(Convert.ToInt32(itemid));
                if (nbi != null)
                {
                    var typecode = nbi.TypeCode;

                    // get data passed back by ajax
                    var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
                    // update record with ajax data
                    nbi.UpdateAjax(strIn);
                    nbi.GUIDKey = nbi.GetXmlProperty("genxml/textbox/ref");
                    objCtrl.Update(nbi);

                    // do langauge record
                    var nbi2 = objCtrl.GetDataLang(Convert.ToInt32(itemid), editlang);
                    nbi2.UpdateAjax(strIn);
                    objCtrl.Update(nbi2);

                    DataCache.ClearCache(); // clear ALL cache.

                }
            }
            return "";
        }

        private String DeleteData(HttpContext context)
        {
            var objCtrl = new NBrightBuyController();

            //get uploaded params
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var itemid = ajaxInfo.GetXmlProperty("genxml/hidden/itemid");
            if (Utils.IsNumeric(itemid))
            {
                // delete DB record
                objCtrl.Delete(Convert.ToInt32(itemid));

                NBrightBuyUtils.RemoveModCache(-2);
            }
            return "";
        }

        #endregion


        private Boolean CheckRights()
        {
            if (UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) || UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.EditorRole) || UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators"))
            {
                return true;
            }
            return false;
        }


    }
}
