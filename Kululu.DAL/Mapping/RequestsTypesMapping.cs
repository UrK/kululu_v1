using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using Kululu.BL.Entities;

namespace kululu.DAL.Mapping
{
    internal class RequestsTypesMapping : ClassMap<RequestsTypes>
    {
        internal RequestsTypesMapping()
        {
            Table("Requests_Types");

            Id(o => o.Id)
              .GeneratedBy.Assigned();

            Map(o => o.Description, "Desription")
                .Not.Nullable();
        }
    }
}