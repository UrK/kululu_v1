using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using Kululu.BL.Entities;

namespace kululu.DAL.Mapping
{
    internal class TagMapping : ClassMap<Tag>
    {
        internal TagMapping()
        {
            Table("Tags");
            
            Id(o => o.Id)
            .GeneratedBy.Identity();

            Map(o => o.Name)
                .Not.Nullable();
        }
    }
}