using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using Kululu.BL.Entities;

namespace kululu.DAL.Mapping
{
    internal static class RatedItemBaseExentsion
    {
        internal static void DefaultMap<T>(this ClassMap<T> DDL) where T : RatedItem
        {
            DDL.Map(o => o.SummedNegativeRating).Column("SummedNegativeRating");
            DDL.Map(o => o.SummedPositiveRating).Column("SummedPositiveRating");
        }
    }
}