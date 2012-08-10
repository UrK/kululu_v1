using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using Kululu.BL.Entities;

namespace kululu.DAL.Mapping
{
    internal class RequestsMapping : ClassMap<Requests>
    {
        internal RequestsMapping()
        {
            Table("Request_Type");

            Id(o => o.Id)
              .GeneratedBy.Assigned();

            Map(o => o.IssuedDate)
              .Not.Nullable();

            References(o => o.Requestor)
                        .Not.Nullable()
                        .LazyLoad();

            References(o => o.Approver)
                        .Not.Nullable()
                        .LazyLoad();

            References(o => o.Type)
                        .Not.Nullable();
        }
    }
}