using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using Kululu.BL.Entities;

namespace kululu.DAL.Mapping
{
    internal class GenreMapping : ClassMap<Genre>
    {
        internal GenreMapping()
        {
            Table("Genres");

            Id(o => o.Id)
            .GeneratedBy.Identity();

            Map(o => o.Name)
               .Not.Nullable();

            HasMany(o => o.SubGenres)
                .LazyLoad()
                .AsSet()
                .Inverse();
            
            HasMany(o => o.CorrespondingTags)
                .LazyLoad()
                .AsSet()
                .Inverse();
        }
    }
}