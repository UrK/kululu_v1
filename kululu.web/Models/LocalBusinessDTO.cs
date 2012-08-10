using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using AutoMapper;
using Dror.Common.Utils;
using Kululu.Entities;
using Kululu.Entities.Common;

namespace Kululu.Web.Models
{
    public class LocalBusinessDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string FacebookUrl { get; set; }
        public long FanPageId { get; set; }

        public bool PublishUserContentToWall { get; set; }
        public bool PublishAdminContentToWall { get; set; }
        public bool IsLikeDemanded { get; set; }
        public FbPostToWallType UserPostToWallType { get; set; }

        public PlaylistDTO DefaultPlaylist { get; set; }
        public PlaylistDTO ImportPlaylist { get; set; }

        public IList<PlaylistDTO> Playlists { get; set; }
        public IList<UserDTO> Owners { get; set; }

        /// <summary>
        ///  email address of the admin who accepts posting emails
        /// </summary>
        public string EmailOnAdminPost { get; set; }

        /// <summary>
        /// language selected for this local business
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// get the map of languages with corresponding names
        /// </summary>
        public IDictionary<string, string> LanguagesMap
        {
            get
            {
                var dict = new Dictionary<string, string>();
                var allVals = Enum.GetValues(typeof (MvcApplication.Culture));
                foreach (var v in allVals)
                {
                    var name =
                        Enum.GetName(typeof (MvcApplication.Culture), v);
                    var cult =
                        CultureInfo.GetCultureInfo(v.ToString()).NativeName;
                    dict.Add(cult, name);
                }
                return dict;
            }
        }
    }

    /// <summary>
    /// factory creating localbusiness suitable for JSON formatting
    /// </summary>
    public class LocalBusinessDTOFactory
    {
        static bool isCreated;

        static LocalBusinessDTOFactory()
        {
            CreateMaps();
        }

        public static void CreateMaps()
        {
            if (isCreated)
                return;

            CreateMap<LocalBusinessDTO>();
            
            isCreated = true;
        }

        public static void CreateMap<TDestination>()
            where TDestination : LocalBusinessDTO
        {
            Mapper.CreateMap<LocalBusiness, TDestination>();
        }

        /// <summary>
        /// map local business into its DTO representation
        /// </summary>
        ///
        /// <param name="localBusiness">
        /// local business to map
        /// </param>
        ///
        /// <param name="appPath">
        /// absolute URL to the application
        /// </param>
        ///
        /// <param name="locale">
        /// required locale for resulting LocalBusinessDTO
        /// </param>
        ///
        ///<returns>
        /// mapped DTO object
        /// </returns>
        public static TDestination Map<TDestination>(
            LocalBusiness localBusiness,
            string appPath,
            string locale)
            where TDestination: LocalBusinessDTO
        {
            UserDTOFactory.CreateMaps();
            PlaylistDTOFactory.Map(localBusiness.DefaultPlaylist);
            PlaylistDTOFactory.Map(localBusiness.Playlists);
            MemberDTOFactory.Map(localBusiness.Owners);
            
            var retVal = Mapper.Map<LocalBusiness, TDestination>(localBusiness);
            retVal.ImageUrl = UrlUtils.BuildApplicationImageUrl(
                retVal.ImageUrl,
                appPath,
                LocalBusiness.DEFAULT_IMAGE_URL);
            retVal.Locale = locale;

            return retVal;
        }
    }
}