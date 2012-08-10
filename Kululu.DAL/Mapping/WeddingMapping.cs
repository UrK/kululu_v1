using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kululu.BL.Entities;
using FluentNHibernate.Mapping;

namespace kululu.DAL.Mapping
{
    internal class WeddingMapping : ClassMap<Wedding>
    {
        internal WeddingMapping()
        {
            Table("Weddings");

            Id(o => o.Id)
            .GeneratedBy.Identity();

            Map(o => o.WeddingName)
            .Not.Nullable().Index("idx_Wedding_Name");

            Map(o => o.Image);

            Map(o => o.Description);

            Map(o => o.WeddingDate);

            Map(o => o.CreationDate)
              .Not.Nullable();

            HasMany(o => o.WeddingSongsStats)
                .AsBag()
                .Cascade.Delete();

            HasMany(o => o.Guests)
                    .AsBag()
                    .LazyLoad()
                    .Cascade.All();

            References(o => o.Groom)
             .Nullable()
             .LazyLoad()
             .Cascade.SaveUpdate();

            References(o => o.Bride)
               .Nullable()
               .LazyLoad()
               .Cascade.SaveUpdate();
        }
    }
}