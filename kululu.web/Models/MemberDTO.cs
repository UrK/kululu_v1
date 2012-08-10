using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Kululu.Entities;
using Kululu.Entities.Common;

namespace Kululu.Web.Models
{
    public class MemberDTO
    {
        public long FBID { get; set; }
        public int score { get; set; }
        public int summedScore { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public UserStatus Status { get; set; }
        public string Locale { get; set; }
        public bool IsRtl { get; set; }
        
        //TODO: make this generic, handle case where there's no value
        public string Gender
        {
            get
            {
                return "male";
            }
        }
    }

    public class MemberDTOFactory
    {
        static bool isCreated;
        static MemberDTOFactory()
        {
            CreateMaps();
        }

        public static void CreateMaps()
        {
            if (isCreated)
                return;
            
            CreateMap<MemberDTO>();
            isCreated = true;
        }

         public static void CreateMap<TDestination>()
            where TDestination : MemberDTO
        {
            Mapper.CreateMap<FbUser, TDestination>()
                .ForMember(dest => dest.FBID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.email, opt => opt.MapFrom(src => src.Email));
     
             Mapper.CreateMap<UserPlaylistInfo, UserDTO>()
             .ForMember(d => d.FBID, opt => opt.MapFrom(s => s.User.Id))
             .ForMember(d => d.score, opt => opt.MapFrom(s => s.Points))
             .ForMember(d => d.name, opt => opt.MapFrom(s => s.User.FullName))
             .ForMember(d => d.email, opt => opt.MapFrom(s => s.User.Email))
             .ForMember(d => d.Status, opt => opt.MapFrom(s => s.User.Status));
        
             Mapper.CreateMap<PlaylistSongRating, UserDTO>()
               .ForMember(d => d.FBID, opt => opt.MapFrom(s => s.IsAddedByAdmin
                   ? s.Playlist.LocalBusiness.FanPageId
                   : s.Creator.Id))
               .ForMember(d => d.name, opt => opt.MapFrom(s => s.IsAddedByAdmin
                   ? s.Playlist.LocalBusiness.Name
                   : s.Creator.Name));
         }

         public static TDestination Map<TDestination>(FbUser user, Playlist playlist, bool isAdmin = false)
            where TDestination : MemberDTO
         {
             var memberDTO = Mapper.Map<FbUser, TDestination>(user);

             if (playlist == null)
             {
                 return memberDTO;
             }

             var userInfo = user.PlaylistsInfo.FirstOrDefault(p => p.Playlist == playlist);

             var summedUserPts =
             user.PlaylistsInfo
                 .Where(userPlaylistInfo => userPlaylistInfo.Playlist.LocalBusiness == playlist.LocalBusiness)
                 .Sum(info => info.Points);

             if (userInfo == null)
             {
                 memberDTO.score = 0;
                 memberDTO.summedScore = summedUserPts;
             }
             else
             {
                 memberDTO.score = userInfo.Points;
                 memberDTO.summedScore = summedUserPts;
             }
             if (isAdmin)
             {
                 memberDTO.FBID = playlist.LocalBusiness.FanPageId;
                 memberDTO.name = playlist.LocalBusiness.Name;
             }

             return memberDTO;
         }

         public static TDestination Map<TDestination>(FbUser user, PlaylistSongRating playlistSongRating, bool isAdmin = false)
             where TDestination : MemberDTO
         {
             var memberDTO = Map<TDestination>(user, playlistSongRating.Playlist, isAdmin);
             return memberDTO;
         }

         /// <summary>
         /// use in Leaderboard 
         /// </summary>
         /// <param name="users"></param>
         /// <param name="scores"></param>
         /// <returns></returns>
         public static IList<MemberDTO> Map(IList<FbUser> users, IList<int> scores = null)
         {
             var retVal = Mapper.Map<List<FbUser>, List<MemberDTO>>(
                 users as List<FbUser>);
             int index = 0;
             retVal.ForEach(u =>
             {
                 u.score =
                     scores != null && scores.Count > index
                     ? scores[index]
                     : 0;
                 index++;
             });
             return retVal;
         }

         /// <summary>
         /// map list user DTOs from the list of user points
         /// </summary>
         ///
         /// <param name="usrsInfo">
         /// list of user points structures
         /// </param>
         ///
         /// <returns>
         /// list of user objects
         /// </returns>
         /// <remarks>Use in localbusiness settings index </remarks>
         public static IList<MemberDTO> Map(IList<UserPlaylistInfo> usrsInfo)
         {
             var retVal = Mapper.Map<List<UserPlaylistInfo>, List<MemberDTO>>(
                 usrsInfo as List<UserPlaylistInfo>);
             
             return retVal;
         }
    }
}