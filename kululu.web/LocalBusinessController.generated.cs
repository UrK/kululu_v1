// <auto-generated />
// This file was generated by a T4 template.
// Don't change it directly as your change would get overwritten.  Instead, make changes
// to the .tt file (i.e. the T4 template) and save it to regenerate this file.

// Make sure the compiler doesn't complain about missing Xml comments
#pragma warning disable 1591
#region T4MVC

using System;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;
using T4MVC;
namespace Kululu.Web.Controllers {
    public partial class LocalBusinessController {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected LocalBusinessController(Dummy d) { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToAction(ActionResult result) {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoute(callInfo.RouteValueDictionary);
        }

        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult GetInfo() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.GetInfo);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult SetDefaultPlaylist() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.SetDefaultPlaylist);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult GetPlaylistByFanPage() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.GetPlaylistByFanPage);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult GetPlaylists() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.GetPlaylists);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult GetPlaylist() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.GetPlaylist);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult AddOwner() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.AddOwner);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult RemoveOwner() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.RemoveOwner);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult SetSocialSettings() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.SetSocialSettings);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult CheckProperty() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.CheckProperty);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.PartialViewResult GetLeaders() {
            return new T4MVC_PartialViewResult(Area, Name, ActionNames.GetLeaders);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult Settings() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.Settings);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public LocalBusinessController Actions { get { return MVC.LocalBusiness; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "LocalBusiness";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass {
            public readonly string GetInfo = "GetInfo";
            public readonly string Index = "Index";
            public readonly string SetDefaultPlaylist = "SetDefaultPlaylist";
            public readonly string GetPlaylistByFanPage = "GetPlaylistByFanPage";
            public readonly string GetPlaylists = "GetPlaylists";
            public readonly string GetPlaylist = "GetPlaylist";
            public readonly string AddOwner = "AddOwner";
            public readonly string RemoveOwner = "RemoveOwner";
            public readonly string SetSocialSettings = "SetSocialSettings";
            public readonly string CheckProperty = "CheckProperty";
            public readonly string TOS = "TOS";
            public readonly string GetLeaders = "GetLeaders";
            public readonly string PeriodicPost = "PeriodicPost";
            public readonly string Settings = "Settings";
            public readonly string ServerStatusPingdom = "ServerStatusPingdom";
        }


        static readonly ViewNames s_views = new ViewNames();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ViewNames Views { get { return s_views; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ViewNames {
            public readonly string HarvestFacebook = "~/Views/LocalBusiness/HarvestFacebook.cshtml";
            public readonly string ServerStatusPingdom = "~/Views/LocalBusiness/ServerStatusPingdom.cshtml";
            public readonly string TOS = "~/Views/LocalBusiness/TOS.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public class T4MVC_LocalBusinessController: Kululu.Web.Controllers.LocalBusinessController {
        public T4MVC_LocalBusinessController() : base(Dummy.Instance) { }

        public override System.Web.Mvc.ActionResult GetInfo(long id) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.GetInfo);
            callInfo.RouteValueDictionary.Add("id", id);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult Index() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Index);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult Index(short startCount, short increment) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.Index);
            callInfo.RouteValueDictionary.Add("startCount", startCount);
            callInfo.RouteValueDictionary.Add("increment", increment);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult SetDefaultPlaylist(long localBusinessId, long playlistId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.SetDefaultPlaylist);
            callInfo.RouteValueDictionary.Add("localBusinessId", localBusinessId);
            callInfo.RouteValueDictionary.Add("playlistId", playlistId);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult GetPlaylistByFanPage(long fanPageId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.GetPlaylistByFanPage);
            callInfo.RouteValueDictionary.Add("fanPageId", fanPageId);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult GetPlaylists(long id) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.GetPlaylists);
            callInfo.RouteValueDictionary.Add("id", id);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult GetPlaylist(long id) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.GetPlaylist);
            callInfo.RouteValueDictionary.Add("id", id);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult AddOwner(long localBusinessId, long ownerId) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.AddOwner);
            callInfo.RouteValueDictionary.Add("localBusinessId", localBusinessId);
            callInfo.RouteValueDictionary.Add("ownerId", ownerId);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult RemoveOwner(long localBusinessId, long ownerId) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.RemoveOwner);
            callInfo.RouteValueDictionary.Add("localBusinessId", localBusinessId);
            callInfo.RouteValueDictionary.Add("ownerId", ownerId);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult SetSocialSettings(long id, string properties) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.SetSocialSettings);
            callInfo.RouteValueDictionary.Add("id", id);
            callInfo.RouteValueDictionary.Add("properties", properties);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult CheckProperty(long id, string propertyName, bool value) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.CheckProperty);
            callInfo.RouteValueDictionary.Add("id", id);
            callInfo.RouteValueDictionary.Add("propertyName", propertyName);
            callInfo.RouteValueDictionary.Add("value", value);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult TOS() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.TOS);
            return callInfo;
        }

        public override System.Web.Mvc.PartialViewResult GetLeaders(long localBussinesId) {
            var callInfo = new T4MVC_PartialViewResult(Area, Name, ActionNames.GetLeaders);
            callInfo.RouteValueDictionary.Add("localBussinesId", localBussinesId);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult PeriodicPost() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.PeriodicPost);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult Settings(long id) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Settings);
            callInfo.RouteValueDictionary.Add("id", id);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult ServerStatusPingdom() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.ServerStatusPingdom);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591
