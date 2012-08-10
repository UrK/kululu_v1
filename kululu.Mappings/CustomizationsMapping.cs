using FluentNHibernate.Mapping;
using Kululu.Entities;

namespace Kululu.Mappings
{
    public class CustomizationsMapping : ClassMap<Customizations>
    {
        public CustomizationsMapping()
        {
            Table("customization");

            Id(o => o.Id);

            Map(o => o.Locale);

            References(o => o.LocalBusiness)
                .Nullable()
                .LazyLoad();

            References(o => o.Playlist)
                .Nullable()
                .LazyLoad();
        }
    }
}
