using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using Kululu.BL.Entities;

namespace kululu.DAL.Mapping
{
    internal class SongRatingMapping : ClassMap<SongRating>
    {
        internal SongRatingMapping()
        {
            Table("Songs_Ratings");

            Id(o => o.Id)
            .GeneratedBy.Identity();

            Map(o => o.Rating)
                .Not.Nullable();

            References(o => o.Song)
             .Nullable()
             .LazyLoad();


            References(o => o.VotingUser)
             .Nullable()
             .LazyLoad();

            References(o => o.Wedding)
             .Nullable()
             .LazyLoad();

            References(o => o.WeddingSongsStatistics)
            .Nullable()
            .LazyLoad();
        }
    }
}