using Kululu.Entities;
using FluentNHibernate.Mapping;

namespace Kululu.Mappings
{
    public sealed class PlaylistMapping : ClassMap<Playlist>
    {
        public PlaylistMapping()
        {
            Table("playlist");

            Id(o => o.Id)
            .GeneratedBy.Identity();

            Map(o => o.Name)
            .Not.Nullable().Index("idx_Wedding_Name");

            Map(o => o.IsActive);

            Map(o => o.Image);

            Map(o => o.Description);

            Map(o => o.CreationDate)
                .Not.Nullable();
            
            Map(o => o.NumOfSongsLimit);

            Map(o => o.NumberOfSongs)
            .Default("0");

            Map(o => o.NumberOfVotes)
                .Default("0");

            Map(o => o.LastModifiedDate);

            Map(o => o.LastPlayedDate)
                .Nullable();

            Map(o => o.NextPlayDate)
                .Nullable();

            Map(o => o.IncrementUpdate);

            Map(o => o.NextUpdate);

            Map(o => o.UpdateDescriptionFormat);

            Map(o => o.IsPushesToWall);

            References(o => o.LocalBusiness)
                .Cascade
                .Delete();

            HasMany(o => o.Ratings)
                .AsBag()
                .Cascade.All()
                .LazyLoad();

            HasMany(o => o.UserInfo)
                .Cascade.All()
                //.Inverse()
                .Table("User_Playlist_Info");

            HasMany(o => o.Visits)
                .AsBag()
                .Cascade.All()
                .LazyLoad();

            Map(o => o.IsSongsLimitDaily);
        }
    }
}