using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kululu.BL.Entities;
using FluentNHibernate.Mapping;

namespace kululu.DAL.Mapping
{
    internal class SubGenreMapping : ClassMap<SubGenre>
    {
        internal SubGenreMapping()
        {
            Table("Sub_Genres");
            
            Id(o => o.Id)
            .GeneratedBy.Identity();

            Map(o => o.Name)
                .Not.Nullable();
        }
    }
}