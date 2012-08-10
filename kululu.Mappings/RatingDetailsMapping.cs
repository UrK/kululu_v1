using FluentNHibernate.Mapping;
using Kululu.Entities;

namespace Kululu.Mappings
{
    public class RatingDetailsMapping : ClassMap<RatingDetails>
    {
        public RatingDetailsMapping()
        {
            Table("rating_details");

            Id(o => o.Id)
            .GeneratedBy.Identity();

            Map(o => o.Rating)
                .Not.Nullable();

            Map(o => o.LastUpdated)
                .Not.Nullable();

            References(o => o.VotingUser)
             .Nullable()
             .Cascade.SaveUpdate()
             .LazyLoad()
             .UniqueKey("Unq_UsrVote");

            References(o => o.PlaylistSongRating, "songsStatistics_Id")
                .Cascade.SaveUpdate()
                .LazyLoad()
                .UniqueKey("Unq_UsrVote");
        }
    }
}