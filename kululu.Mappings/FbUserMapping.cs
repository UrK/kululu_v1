using FluentNHibernate.Mapping;
using Kululu.Entities;

namespace Kululu.Mappings
{
    public class FbUserMapping : ClassMap<FbUser>
    {
        public FbUserMapping()
        {
            Table("users");

            Id(o => o.Id)
              .GeneratedBy.Assigned()
              .Column("FbId");

            Map(o => o.Status)
                .Not.Nullable();
            
            Map(o => o.JoinDate, "Join_Date")
                .Not.Nullable();

            //TODO: remove this. we don't need duplicate properties - FullName and Name
            Map(o => o.Name)
                .Nullable();

            Map(o => o.FullName).Not.Nullable();

            Map(o => o.ProfileImageUrl)
               .Nullable();

            Map(o => o.RelationshipStatus)
               .Nullable();
            
            Map(o => o.Gender)
                .Nullable();

            Map(o => o.LinkToProfile)
                .Nullable();

            Map(o => o.Email).Nullable();
            
            References(o => o.SignificantOther)
                .Nullable();

            HasMany(o => o.PlaylistsInfo)
                .Cascade.All()
                .Inverse()
                .Table("User_Playlist_Info");

            HasManyToMany(o => o.Businesses)
                .Inverse()
                .AsBag()
                .Cascade.All()
                .LazyLoad();

            Map(o => o.Locale).Nullable();
        }
    }
}