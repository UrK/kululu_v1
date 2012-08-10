using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Kululu.Entities;

namespace Kululu.Web.Models
{
    public class SongWithUserRatingDTO
    {
        #region PlalystSongRatingInfo
        [UIHint("TODO: Use Automapper to map object")]
        public virtual long SongId { get; set; }
        public virtual string SongName { get; set; }
        public virtual string SongArtistName { get; set; }
        public virtual double? SongDuration { get; set; }
        public virtual string SongVideoID { get; set; }
        public virtual string SongImageUrl { get; set; }

        public virtual string FBOnlyPostId
        {
            get
            {
                if (FBPostId != null)
                {
                    var content = FBPostId.Split('_');
                    return content[content.Length-1];
                }
                return string.Empty;
            }
        }

        public virtual string FBOnlyPageId
        {
            get
            {
                if (FBPostId != null)
                {
                    var content = FBPostId.Split('_');
                    return content[0];
                }
                return string.Empty;
            }
        }

        public virtual string FBPostId { get; set; }
        public virtual string Message { get; set; }
        public virtual string Description { get; set; }
        public virtual int NumOfComments { get; set; }
        public virtual long RatingId { get; set; }
        #endregion

        /// <summary>
        /// date of current song creation for current playlist
        /// </summary>
        public virtual DateTime CreationDate { get; set; }
        public double SummedPositiveRating { get; set; }
        public double SummedNegativeRating { get; set; }
        public bool  IsAddedByAdmin { get; set; }

        #region Current User Info
        public virtual bool CreatedByCurrentUser { get; set; }
        //TODO: rating won't stay long on song dto, as it needs to come with a second request
        //retrieving both rating and song will take a long time. That's why need to devide it up
        public short Rating { get; set; }

        #endregion

        public MemberDTO Member { get; set; }
    }

    /// <summary>
    /// factory creating song suitable for JSON formatting
    /// </summary>
    public class SongWithUserRatingDtoFactory
    {
        static bool isCreated;
        static SongWithUserRatingDtoFactory()
        {
            CreateMaps();
        }

        public static void CreateMaps()
        {
            if (isCreated)
                return;

            MemberDTOFactory.CreateMaps();

            Mapper.CreateMap<Song, SongWithUserRatingDTO>()
                .ForMember(dest => dest.SongArtistName, opt => opt.MapFrom(src => src.ArtistName))
                .ForMember(dest => dest.SongDuration, opt => opt.MapFrom(src => src.Duration))
                .ForMember(dest => dest.SongId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SongName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.SongImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.SongVideoID, opt => opt.MapFrom(src => src.VideoID));

            Mapper.CreateMap<PlaylistSongRating, SongWithUserRatingDTO>()
                 .ForMember(dest => dest.SongId, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.FacebookAddedDate ))
                 .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.FBMessage))
                 .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.FBDescription))
                 .ForMember(dest => dest.FBPostId, opt => opt.MapFrom(src => src.FBPostId))
                 .ForMember(dest => dest.Member, opt => opt.MapFrom(src => src.Creator));

            isCreated = true;
        }

        public static SongWithUserRatingDTO Map(Song song, long ratingId)
        {
            var retVal = Mapper.Map<Song, SongWithUserRatingDTO>(song);
            retVal.RatingId = ratingId;
            return retVal;
        }

        public static SongWithUserRatingDTO Map(PlaylistSongRating rat, bool isAdmin = false)
        {
            var ratVal = Mapper.Map<PlaylistSongRating, SongWithUserRatingDTO>(rat);
            ratVal.Member = MemberDTOFactory.Map<MemberDTO>(rat.Creator, rat, isAdmin);
            return ratVal;
        }
    }
}