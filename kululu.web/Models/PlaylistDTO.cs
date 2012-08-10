using System;
using System.Collections.Generic;
using AutoMapper;
using Kululu.Entities;
using System.ComponentModel.DataAnnotations;
using Kululu.Web.Attributes;
using Dror.Common.Data.Contracts;
using System.Linq;

namespace Kululu.Web.Models
{
    public class PlaylistDTO
    {
        public long Id { get; set; }
        public long LocalBusinessId { get; set; }

        [Required(ErrorMessage = "מי יצר את הפלייליסט")]
        public long FanPageId { get; set; }

        public string CreatorFullName { get; set; }

        public bool IsDefaultPlaylist { get; set; }
        
        [Required(ErrorMessage = "שם הפלייליסט לא יכול להיות ריק.")]
        public string Name { get; set; }

        [PropertyFalseAttribute("IsDefaultPlaylist", "לא ניתן להפוך פלייליסט דיפולטי ללא פעיל")]
        public bool IsActive { get; set; }
        
        public string Description { get; set; }

        [Required(ErrorMessage = "מה מגבלת השירים?")]
        public int NumOfSongsLimit { get; set; }
        
        public DateTime LastModifiedDate { get; set; }
        public DateTime CreationDate { get; set; }

        public DateTime? NextPlayDate { get; set; }
        public bool IncludeMarchDates
        {
            get
            {
                return NextPlayDate.HasValue;
            }
        }

        public long NextPlayDateBinary { get; set; }
        public double TimeStampBinary { get; set; }

        public string Image { get; set; }
        public int NumberOfVotes { get; set; }
        public int NumberOfSongs { get; set; }

        /// <summary>
        /// is the limit of the songs reset daily?
        /// </summary>
        public bool IsSongsLimitDaily { get; set; }
        
        /// <summary>
        /// maximal number of votes for a single song in this playlist
        /// </summary>
        public int MaxVotes;

        //public bool IsPulledFromWall { get; set; }
        public bool IsPushesToWall { get; set; }

        /// <summary>
        /// are songs added by the user posted to the wall of the fan page?
        /// </summary>
        public virtual bool PublishAdminContentToWall { get; set; }

        /// <summary>
        /// are songs added by the user posted to the wall of the same user?
        /// </summary>
        public virtual bool PublishUserContentToWall { get; set; }
    }

    /// <summary>
    /// factory creating playlist suitable for JSON formatting
    /// </summary>
    public class PlaylistDTOFactory
    {
        static bool isCreated;
        static PlaylistDTOFactory()
        {
            CreateMaps();
        }

        public static void CreateMaps()
        {
            if (isCreated)
                return;

            Mapper.CreateMap<Playlist, PlaylistDTO>()
                .ForMember(dest => dest.CreatorFullName, opt => opt.MapFrom(src => (src.LocalBusiness.Owners.Count > 0) ? src.LocalBusiness.Owners[0].FullName : String.Empty))
                .ForMember(dest => dest.IsDefaultPlaylist, opt => opt.MapFrom(src => src.LocalBusiness.DefaultPlaylist == src))
                .ForMember(dest => dest.FanPageId, opt => opt.MapFrom(src => src.LocalBusiness.FanPageId))
                .ForMember(dest => dest.NextPlayDateBinary, opt => opt.MapFrom(src => src.NextPlayDate.HasValue ?
                    (src.NextPlayDate.Value - new DateTime(1970, 1, 1)).TotalMilliseconds : -1))
                .ForMember(dest => dest.TimeStampBinary,
                    opt => opt.MapFrom(src => (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds));

            Mapper.CreateMap<PlaylistDTO, Playlist>();
                
            isCreated = true;
        }

        /// <summary>
        /// map playlist object into playlist DTO passed to the client
        /// </summary>
        ///
        /// <param name="playlist">
        /// playlist to convert
        /// </param>
        ///
        /// <param name="uniqueVotersCount">
        /// number of unique voters for this playlist
        /// </param>
        ///
        /// <returns>
        /// DTO of the playlist
        /// </returns>
        public static PlaylistDTO Map(Playlist playlist)
        {
            var playlistDTO = Mapper.Map<Playlist, PlaylistDTO>(playlist);
            playlistDTO.PublishAdminContentToWall =
                (playlist.LocalBusiness != null) &&
                playlist.LocalBusiness.PublishAdminContentToWall;
            playlistDTO.PublishUserContentToWall =
                (playlist.LocalBusiness != null) &&
                    playlist.LocalBusiness.PublishUserContentToWall;
            return playlistDTO;
        }

        public static IList<PlaylistDTO> Map(IList<Playlist> playlists)
        {
            return Mapper.Map<IList<Playlist>, IList<PlaylistDTO>>(playlists);
        }

        public static Playlist ReverseMap(PlaylistDTO pdto)
        {
            return Mapper.Map<PlaylistDTO, Playlist>(pdto);
        }

    }
}