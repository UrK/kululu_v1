using FluentNHibernate.Mapping;
using Kululu.Entities;

namespace Kululu.Mappings
{
    public sealed class PlaylistRatingMapping : ClassMap<PlaylistSongRating>
    {
        public PlaylistRatingMapping()
        {
            Table("playlist_rating");

            Id(o => o.Id).GeneratedBy.Identity();
            
            Map(o => o.FBPostId).Nullable();
            Map(o => o.NumOfComments).Nullable();

            Map(o => o.FacebookAddedDate).Not.Nullable();

            References(o => o.Creator)
                .Not.Nullable()
                .LazyLoad();

            Map(o => o.CreationTime).Not.Nullable();

            Map(o => o.FBMessage)
                .Nullable()
                .Length(500);

            Map(o => o.FBDescription)
                .Nullable()
                .Length(500);

            Map(o => o.Origin);

            Map(o => o.IsAddedByAdmin)
                .Not.Nullable();

            Map(o => o.AdminRating)
                .Not.Nullable();

            HasMany(o => o.Details)
                   .AsBag()
                   .Inverse()
                   .KeyColumn("SongsStatistics_id")
                   .Cascade.AllDeleteOrphan()
                   .LazyLoad();

            References(o => o.Playlist)
             .Nullable()
             .Not.LazyLoad();

            References(o => o.Song)
             .Nullable()
             .Cascade.SaveUpdate()
             .LazyLoad();

            this.DefaultMap();
            
        }
    }
}