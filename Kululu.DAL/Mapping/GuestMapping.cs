using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using Kululu.BL.Entities;

namespace kululu.DAL.Mapping
{
    internal class GuestMapping : ClassMap<Guest>
    {
        internal GuestMapping()
        {
            Table("Guest");

            Id(o => o.Id)
              .GeneratedBy.Assigned();

            References(o=>o.Wedding)
                       .Not.Nullable()
                       .LazyLoad();

            References(o => o.User)
                       .Not.Nullable()
                       .LazyLoad();

            Map(o => o.Approved, "Approved")
                .Not.Nullable();

            Map(o => o.RequestedDate, "Requested_Date")
                .Not.Nullable();

            Map(o => o.ApprovedDate, "Approved_Date")
                .Not.Nullable();
        }
    }
}