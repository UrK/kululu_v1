using FluentNHibernate.Mapping;
using Kululu.Entities;

namespace Kululu.Mappings
{
    public class EmailsMapping : ClassMap<Emails>
    {
        public EmailsMapping()
        {
            Table("emails");

            Id(o => o.Email);
        }
    }
}
