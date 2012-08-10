using Kululu.Entities;
using AutoMapper;
using Kululu.Web.Models.Common;
using System.Linq;
using Kululu.Web.Startup;

namespace Kululu.Web.Models
{
    public class UserDTO : MemberDTO
    {
        public string role { get; set; }

        public int numOfSongsLeft { get; set; }

        public int numOfVotes { get; set; }

        public bool IsConnected
        {
            get
            {
                return FBID != -1;
            }
        }

        /// <summary>
        /// is the user admin of current fan page?
        /// </summary>
        public bool IsPageAdmin { get; set; }

        public bool HasLikedPage { get; set; }

        /// <summary>
        /// time offset of the current user as received from Facebook relative
        /// to UTC
        /// </summary>
        public int TimeOffset { get; set; }
    }

    public class UserDTOFactory
    {
        static bool isCreated;

        static UserDTOFactory()
        {
            CreateMaps();
        }

        public static void CreateMaps()
        {
            if (isCreated)
                return;

            MemberDTOFactory.CreateMap<UserDTO>();  

            isCreated = true;
        }

        /// <summary>
        /// is the current usere admin of relevant fan page?
        /// </summary>
        ///
        /// <param name="user">
        /// user to be mapped
        /// </param>
        ///
        /// <param name="playlist">
        /// playlist with which current operation is executed
        /// </param>
        ///
        /// <param name="privileges">
        /// additional information: user privileges to be set in returned
        /// object
        /// </param>
        ///
        /// <param name="loggedInUserInfo">
        /// cookie information about currently logged in user
        /// </param>
        ///
        /// <returns>
        /// mapped user object with additional information
        /// </returns>
        public static UserDTO Map(FbUser user,
            FbCookieInfo loggedInUserInfo,
            Playlist playlist,
            UserPrivileges privileges = UserPrivileges.None)
        {
            if (user == null)
            {
                return null;
            }

            var userDTO = MemberDTOFactory.Map<UserDTO>(user, playlist);

            userDTO.role = privileges.ToString();
            userDTO.IsPageAdmin = loggedInUserInfo.IsAdmin;
            userDTO.HasLikedPage = loggedInUserInfo.HasLikedPage;
            
            if (playlist == null)
            {
                return userDTO;
            }

            if (loggedInUserInfo.IsAdmin)
            {
                userDTO.name = playlist.LocalBusiness.Name;
                userDTO.FBID = playlist.LocalBusiness.FanPageId;
            }

            // TODO: test this assumption: We're assuming that since a user
            // doesn't have a score, it means he didn't even add a song
            // since he didn't add a song, that's why we can set numOfSongsAdded to playlist's max
            if (userDTO.score == 0)
            {
                userDTO.numOfSongsLeft = playlist.NumOfSongsLimit;
            }
            else
            {
                var userInfo = user.PlaylistsInfo.FirstOrDefault(p => p.Playlist == playlist);
                //TODO: remove redundant NumOfVotes column
                userDTO.numOfVotes = userInfo.NumOfVotes;
                userDTO.numOfSongsLeft = playlist.GetNumOfSongsLeft(user);
            }
            
            return userDTO;
        }

        public static UserDTO Map(PlaylistSongRating playlistSongRating, FbCookieInfo cookieInfo)
        {
            return Mapper.Map<PlaylistSongRating, UserDTO>(playlistSongRating);
        }

        /// <summary>
        /// map user DTO from his/her user points
        /// </summary>
        ///
        /// <param name="usrInfo">
        /// user score data
        /// </param>
        ///
        /// <returns>
        /// mapped user object
        /// </returns>
        public static UserDTO Map(UserPlaylistInfo usrInfo, FbCookieInfo cookieInfo)
        {
            if (usrInfo ==  null)
            {
                return null;
            }
            var userDto = Mapper.Map<UserPlaylistInfo, UserDTO>(usrInfo);
            userDto.role = null;
            userDto.IsPageAdmin = false;
            return userDto;
        }
    }
}