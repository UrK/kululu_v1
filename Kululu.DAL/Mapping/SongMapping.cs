using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using Kululu.BL.Entities;

namespace kululu.DAL.Mapping
{
    internal class SongMapping : ClassMap<Song>
    {
        internal SongMapping()
        {
            Table("Songs");

            Id(o => o.Id, "Id")
            .GeneratedBy.Identity();

            Map(o => o.Name)
                .Not.Nullable();

            Map(o => o.VideoID)
                .Not.Nullable();

            Map(o => o.LastUpdated)
                .Not.Nullable();
            
            //HasMany(o=>o.CorrespondingGeneres)
            //    .LazyLoad()
            //    .AsSet()
            //    .Inverse();

            this.DefaultMap();
        }
    }
}