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
    public partial class FacebookController {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected FacebookController(Dummy d) { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToAction(ActionResult result) {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoute(callInfo.RouteValueDictionary);
        }

        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult SongInfo() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.SongInfo);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult SendFeedback() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.SendFeedback);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult ArePageAdmins() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.ArePageAdmins);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public FacebookController Actions { get { return MVC.Facebook; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "Facebook";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass {
            public readonly string SongInfo = "SongInfo";
            public readonly string SendFeedback = "SendFeedback";
            public readonly string ArePageAdmins = "ArePageAdmins";
        }


        static readonly ViewNames s_views = new ViewNames();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ViewNames Views { get { return s_views; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ViewNames {
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public class T4MVC_FacebookController: Kululu.Web.Controllers.FacebookController {
        public T4MVC_FacebookController() : base(Dummy.Instance) { }

        public override System.Web.Mvc.ActionResult SongInfo(long id) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.SongInfo);
            callInfo.RouteValueDictionary.Add("id", id);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult SendFeedback(string feedback, long playlistId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.SendFeedback);
            callInfo.RouteValueDictionary.Add("feedback", feedback);
            callInfo.RouteValueDictionary.Add("playlistId", playlistId);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult ArePageAdmins(long localbusinessId, string userIds) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.ArePageAdmins);
            callInfo.RouteValueDictionary.Add("localbusinessId", localbusinessId);
            callInfo.RouteValueDictionary.Add("userIds", userIds);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591