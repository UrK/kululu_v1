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
    public partial class PlaylistController {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected PlaylistController(Dummy d) { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToAction(ActionResult result) {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoute(callInfo.RouteValueDictionary);
        }

        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult GetVideoInfo() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.GetVideoInfo);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult GetSongInfo() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.GetSongInfo);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.PartialViewResult Dialog() {
            return new T4MVC_PartialViewResult(Area, Name, ActionNames.Dialog);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.PartialViewResult Chart() {
            return new T4MVC_PartialViewResult(Area, Name, ActionNames.Chart);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult GetSong() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.GetSong);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult GetSongs() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.GetSongs);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult GetAllRatingsOfSong() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.GetAllRatingsOfSong);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult QuerySongs() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.QuerySongs);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult GetNumOfVotes() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.GetNumOfVotes);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult AddSong() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.AddSong);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult RateSong() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.RateSong);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult DeleteSong() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.DeleteSong);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult DetachSong() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.DetachSong);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult GetDetails() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.GetDetails);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult UpdateSong() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.UpdateSong);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.JsonResult GetActivityStream() {
            return new T4MVC_JsonResult(Area, Name, ActionNames.GetActivityStream);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult Create() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.Create);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult CanvasCreate() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.CanvasCreate);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult Edit() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.Edit);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public PlaylistController Actions { get { return MVC.Playlist; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "Playlist";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass {
            public readonly string GetVideoInfo = "GetVideoInfo";
            public readonly string GetSongInfo = "GetSongInfo";
            public readonly string Wall = "Wall";
            public readonly string Dialog = "Dialog";
            public readonly string Chart = "Chart";
            public readonly string Player = "Player";
            public readonly string GetSong = "GetSong";
            public readonly string GetSongs = "GetSongs";
            public readonly string GetAllRatingsOfSong = "GetAllRatingsOfSong";
            public readonly string QuerySongs = "QuerySongs";
            public readonly string GetNumOfVotes = "GetNumOfVotes";
            public readonly string AddSong = "AddSong";
            public readonly string RateSong = "RateSong";
            public readonly string DeleteSong = "DeleteSong";
            public readonly string DetachSong = "DetachSong";
            public readonly string GetDetails = "GetDetails";
            public readonly string UpdateSong = "UpdateSong";
            public readonly string GetActivityStream = "GetActivityStream";
            public readonly string Create = "Create";
            public readonly string CanvasCreate = "CanvasCreate";
            public readonly string Edit = "Edit";
        }


        static readonly ViewNames s_views = new ViewNames();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ViewNames Views { get { return s_views; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ViewNames {
            public readonly string _ChartView = "~/Views/Playlist/_ChartView.cshtml";
            public readonly string _Dialogs = "~/Views/Playlist/_Dialogs.cshtml";
            public readonly string _DialoguesTemplates = "~/Views/Playlist/_DialoguesTemplates.cshtml";
            public readonly string _Header = "~/Views/Playlist/_Header.cshtml";
            public readonly string _Leaderboard = "~/Views/Playlist/_Leaderboard.cshtml";
            public readonly string _Nav = "~/Views/Playlist/_Nav.cshtml";
            public readonly string _Player = "~/Views/Playlist/_Player.cshtml";
            public readonly string _playlistInfo = "~/Views/Playlist/_playlistInfo.cshtml";
            public readonly string _Wall = "~/Views/Playlist/_Wall.cshtml";
            public readonly string Playlist = "~/Views/Playlist/Playlist.cshtml";
            public readonly string Playlist_he = "~/Views/Playlist/Playlist.he.resx";
            public readonly string Playlist_resx = "~/Views/Playlist/Playlist.resx";
            public readonly string SongInfo = "~/Views/Playlist/SongInfo.cshtml";
            public readonly string VideoInfo = "~/Views/Playlist/VideoInfo.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public class T4MVC_PlaylistController: Kululu.Web.Controllers.PlaylistController {
        public T4MVC_PlaylistController() : base(Dummy.Instance) { }

        public override System.Web.Mvc.ActionResult GetVideoInfo(string id) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.GetVideoInfo);
            callInfo.RouteValueDictionary.Add("id", id);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult GetSongInfo(long id) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.GetSongInfo);
            callInfo.RouteValueDictionary.Add("id", id);
            return callInfo;
        }

        public override System.Web.Mvc.PartialViewResult Wall() {
            var callInfo = new T4MVC_PartialViewResult(Area, Name, ActionNames.Wall);
            return callInfo;
        }

        public override System.Web.Mvc.PartialViewResult Dialog(string dialogName) {
            var callInfo = new T4MVC_PartialViewResult(Area, Name, ActionNames.Dialog);
            callInfo.RouteValueDictionary.Add("dialogName", dialogName);
            return callInfo;
        }

        public override System.Web.Mvc.PartialViewResult Chart(long id) {
            var callInfo = new T4MVC_PartialViewResult(Area, Name, ActionNames.Chart);
            callInfo.RouteValueDictionary.Add("id", id);
            return callInfo;
        }

        public override System.Web.Mvc.PartialViewResult Player() {
            var callInfo = new T4MVC_PartialViewResult(Area, Name, ActionNames.Player);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult GetSong(long playlistSongRatingId, long playlistId) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.GetSong);
            callInfo.RouteValueDictionary.Add("playlistSongRatingId", playlistSongRatingId);
            callInfo.RouteValueDictionary.Add("playlistId", playlistId);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult GetSongs(long localBussinesId, long? playlistId, short startCount, short increment, Kululu.Entities.Common.SortOptions sort, bool descending, long referringPlaylistSongRatingId) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.GetSongs);
            callInfo.RouteValueDictionary.Add("localBussinesId", localBussinesId);
            callInfo.RouteValueDictionary.Add("playlistId", playlistId);
            callInfo.RouteValueDictionary.Add("startCount", startCount);
            callInfo.RouteValueDictionary.Add("increment", increment);
            callInfo.RouteValueDictionary.Add("sort", sort);
            callInfo.RouteValueDictionary.Add("descending", descending);
            callInfo.RouteValueDictionary.Add("referringPlaylistSongRatingId", referringPlaylistSongRatingId);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult GetAllRatingsOfSong(long playlistId, long playlistSongRatingId) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.GetAllRatingsOfSong);
            callInfo.RouteValueDictionary.Add("playlistId", playlistId);
            callInfo.RouteValueDictionary.Add("playlistSongRatingId", playlistSongRatingId);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult QuerySongs(long playlistId, string queryStr) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.QuerySongs);
            callInfo.RouteValueDictionary.Add("playlistId", playlistId);
            callInfo.RouteValueDictionary.Add("queryStr", queryStr);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult GetNumOfVotes(long playlistId) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.GetNumOfVotes);
            callInfo.RouteValueDictionary.Add("playlistId", playlistId);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult AddSong(long playlistId, string songName, string videoId, short newRatingValue, string songImage, string songArtist, double? songDuration) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.AddSong);
            callInfo.RouteValueDictionary.Add("playlistId", playlistId);
            callInfo.RouteValueDictionary.Add("songName", songName);
            callInfo.RouteValueDictionary.Add("videoId", videoId);
            callInfo.RouteValueDictionary.Add("newRatingValue", newRatingValue);
            callInfo.RouteValueDictionary.Add("songImage", songImage);
            callInfo.RouteValueDictionary.Add("songArtist", songArtist);
            callInfo.RouteValueDictionary.Add("songDuration", songDuration);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult RateSong(long playlistId, long playlistSongRatingId, short rating) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.RateSong);
            callInfo.RouteValueDictionary.Add("playlistId", playlistId);
            callInfo.RouteValueDictionary.Add("playlistSongRatingId", playlistSongRatingId);
            callInfo.RouteValueDictionary.Add("rating", rating);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult DeleteSong(long playlistId, long playlistSongRatingId) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.DeleteSong);
            callInfo.RouteValueDictionary.Add("playlistId", playlistId);
            callInfo.RouteValueDictionary.Add("playlistSongRatingId", playlistSongRatingId);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult DetachSong(Kululu.Entities.Playlist playlist, long playlistSongRatingId) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.DetachSong);
            callInfo.RouteValueDictionary.Add("playlist", playlist);
            callInfo.RouteValueDictionary.Add("playlistSongRatingId", playlistSongRatingId);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult GetDetails(long playlistId) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.GetDetails);
            callInfo.RouteValueDictionary.Add("playlistId", playlistId);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult UpdateSong(long playlistId, long playlistSongRatingId, string songName, string artistName, string imageUrl) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.UpdateSong);
            callInfo.RouteValueDictionary.Add("playlistId", playlistId);
            callInfo.RouteValueDictionary.Add("playlistSongRatingId", playlistSongRatingId);
            callInfo.RouteValueDictionary.Add("songName", songName);
            callInfo.RouteValueDictionary.Add("artistName", artistName);
            callInfo.RouteValueDictionary.Add("imageUrl", imageUrl);
            return callInfo;
        }

        public override System.Web.Mvc.JsonResult GetActivityStream(long playlistId, Kululu.Entities.ActivityType type, long? userId, System.DateTime? startStreamDate, int numOfActivities) {
            var callInfo = new T4MVC_JsonResult(Area, Name, ActionNames.GetActivityStream);
            callInfo.RouteValueDictionary.Add("playlistId", playlistId);
            callInfo.RouteValueDictionary.Add("type", type);
            callInfo.RouteValueDictionary.Add("userId", userId);
            callInfo.RouteValueDictionary.Add("startStreamDate", startStreamDate);
            callInfo.RouteValueDictionary.Add("numOfActivities", numOfActivities);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult Create(long fanPageId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Create);
            callInfo.RouteValueDictionary.Add("fanPageId", fanPageId);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult CanvasCreate(Kululu.Web.Models.PlaylistWithUserDTO pdto) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.CanvasCreate);
            callInfo.RouteValueDictionary.Add("pdto", pdto);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult Create(Kululu.Web.Models.PlaylistWithUserDTO pdto) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Create);
            callInfo.RouteValueDictionary.Add("pdto", pdto);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult Edit(long id) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Edit);
            callInfo.RouteValueDictionary.Add("id", id);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult Edit(Kululu.Web.Models.PlaylistWithUserDTO editInfo) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Edit);
            callInfo.RouteValueDictionary.Add("editInfo", editInfo);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591
