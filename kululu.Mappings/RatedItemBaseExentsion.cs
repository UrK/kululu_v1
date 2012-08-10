using FluentNHibernate.Mapping;
using Kululu.Entities;

namespace Kululu.Mappings
{
    public static class RatedItemBaseExentsion
    {
        public static void DefaultMap<T>(this ClassMap<T> DDL) where T : AggregatedRating
        {
            DDL.Map(o => o.SummedNegativeRating).Column("SummedNegativeRating");
            DDL.Map(o => o.SummedPositiveRating).Column("SummedPositiveRating");
        }
    }
}