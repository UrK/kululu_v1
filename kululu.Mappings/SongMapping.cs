using FluentNHibernate.Mapping;
using Kululu.Entities;

namespace Kululu.Mappings
{
    public sealed class SongMapping : ClassMap<Song>
    {
        public SongMapping()
        {
            Table("songs");

            Id(o => o.Id, "Id")
                .GeneratedBy.Identity();

            Map(o => o.Name)
                .Not.Nullable();

            Map(o => o.VideoID)
                .Not.Nullable()
                .Unique();

            Map(o => o.ArtistName)
                .Nullable();

            Map(o => o.Duration)
                .Nullable();
            
            Map(o => o.LastUpdated)
                .Not.Nullable();

            Map(o => o.ImageUrl)
                .Nullable();
        }
    }
}