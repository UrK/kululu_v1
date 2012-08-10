using Kululu.Entities;
using FluentNHibernate.Mapping;

namespace Kululu.Mappings
{
    public class LocalBusinessMapping : ClassMap<LocalBusiness>
    {
        public LocalBusinessMapping()
        {
            Table("local_business");

            Id(o => o.Id);

            Map(o => o.AddressCity)
                .Nullable();
            Map(o => o.AddressStreet)
                .Nullable();
            Map(o => o.CreatedDate);
            Map(o => o.Category);
            Map(o => o.FanPageId);
            Map(o => o.FBFanPageAccessToken);

            Map(o => o.FacebookUrl)
                .Length(500)
                .Nullable();

            Map(o => o.ImageUrl)
                .Nullable();
            Map(o => o.LastModified);
            Map(o => o.Name);

            Map(o => o.YoutubeChannel);
            Map(o => o.IsAutomaticallyGenerated);
            Map(o => o.Approved);
            Map(o => o.Contact);

            Map(o => o.IsLikeDemanded);
            Map(o => o.PublishAdminContentToWall);
            Map(o => o.PublishUserContentToWall);
            Map(o => o.UserPostToWallTypeValue);
            
            References(o => o.DefaultPlaylist)
                .LazyLoad();

            References(o => o.ImportPlaylist)
                .Nullable()
                .LazyLoad();

            HasMany(o => o.Playlists)
                .Inverse()
                .Cascade.AllDeleteOrphan();

            HasManyToMany(o => o.Owners)
              .AsBag()
              .Cascade.All()
              .Not.LazyLoad();

            Map(o => o.EmailOnAdminPost)
                .Nullable();

            Map(o => o.LikePageImage)
                .Nullable();
        }
    }
}
