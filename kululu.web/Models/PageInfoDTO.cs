using System.Collections.Generic;
using System.Threading;
using Kululu.Entities;

namespace Kululu.Web.Models
{
    /// <summary>
    /// data object used to pass full information to the client
    /// </summary>
    public class PageInfoDTO : LocalBusinessDTO
    {
        /// <summary>
        /// information about the current user
        /// </summary>
        public UserDTO User { get; set; }
        public long ReferringPlaylisingSongRating { get; set; }
        public IDictionary<string, string> Strings { get; set; }
        public bool IsCanvas { get; set; }
        /// <summary>
        /// locale used to render the page
        /// </summary>
        public string DisplayLocale { get; set; }

        /// <summary>
        /// is currently used locale RTL?
        /// </summary>
        public bool IsRightToLeft { get; set; }
    }

    public class PageInfoDTOFactory
    {
        static PageInfoDTOFactory()
        {
            LocalBusinessDTOFactory.CreateMap<PageInfoDTO>();
            PlaylistDTOFactory.CreateMaps();
        }

        public static PageInfoDTO Map(LocalBusiness localBusiness,
            UserDTO user, string appPath, string locale)
        {
            var pageDTO = LocalBusinessDTOFactory.Map<PageInfoDTO>(
                localBusiness, appPath, locale);
            pageDTO.User = user;
            pageDTO.DisplayLocale = Thread.CurrentThread.CurrentUICulture.Name;
            pageDTO.IsRightToLeft =
                Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft;
            return pageDTO;
        }
    }
}