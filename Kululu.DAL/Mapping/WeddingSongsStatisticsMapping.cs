using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using Kululu.BL.Entities;

namespace kululu.DAL.Mapping
{
    internal class WeddingSongsStatisticsMapping : ClassMap<WeddingSongsStatistics>
    {
        internal WeddingSongsStatisticsMapping()
        {
            Table("Wedding_Songs_Statistics");

            Id(o => o.Id)
            .GeneratedBy.Identity();

            Map(o => o.LastUpdated)
            .Not.Nullable();

            Map(o => o.RenamedSong);

            HasMany(o => o.SongRatings)
                   .LazyLoad()
                   .AsBag()
                   .Cascade.All();

            References(o => o.Wedding)
             .Nullable()
             .Not.LazyLoad();

            References(o => o.Song)
             .Nullable()
             .LazyLoad();

            this.DefaultMap();
        }
    }
}