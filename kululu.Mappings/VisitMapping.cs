using FluentNHibernate.Mapping;
using Kululu.Entities;

namespace Kululu.Mappings
{
    public class VisitMapping : ClassMap<Visit>
    {
        public VisitMapping()
        {
            Table("visits");

            Id(o => o.Id);
            Map(o => o.VisitDate);

            References(o => o.Playlist).Cascade.Delete();
            References(o => o.User);
        }
    }
}
