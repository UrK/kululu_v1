using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using Kululu.BL.Entities;

namespace kululu.DAL.Mapping
{
    internal class FbUserMapping : ClassMap<FbUser>
    {
        internal FbUserMapping()
        {
            Table("Users");

            Id(o => o.FbId)
              .GeneratedBy.Assigned();

            Map(o => o.Status)
                .Not.Nullable();
            
            Map(o => o.JoinDate, "Join_Date")
                .Not.Nullable();
        }
    }
}