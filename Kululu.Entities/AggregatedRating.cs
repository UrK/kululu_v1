using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dror.Common.Data.Contracts;

namespace Kululu.Entities
{
    public abstract class AggregatedRating : IEntity
    {
        #region Properties
        public virtual double SummedPositiveRating
        {
            get;
            set;
        }

        public virtual double SummedNegativeRating
        {
            get;
            set;
        }

        #endregion

        public virtual void UpdatedAggregatedRating(short oldRating, short newRating)
        {
            switch (newRating)
            {
                case -1:
                    RateNegative(oldRating);
                    break;
                case 0:
                    ResetARating(oldRating);
                    break;
                case 1:
                    RatePositive(oldRating);
                    break;
                default:
                    break;
            }
        }

        public virtual void AddAggregatedRating(int rating)
        {
            switch (rating)
            {
                case -1:
                    AddNegativeRating();
                    break;
                case 1:
                    AddPositiveRating();
                    break;
                default:
                    break;
            }
        }

        private void RateNegative(short oldRating)
        {
            AddNegativeRating();
            if (oldRating != 0)
            {
                SubstractPositiveRating();
            }
        }

        private void RatePositive(short oldRating)
        {
            AddPositiveRating();
            if (oldRating != 0)
            {
                SubstractNegativeRating();
            }
        }

        private void AddPositiveRating()
        {
            this.SummedPositiveRating++;
        }

        private void AddNegativeRating()
        {
            this.SummedNegativeRating++;
        }

        private void SubstractPositiveRating()
        {
            this.SummedPositiveRating--;
        }

        private void SubstractNegativeRating()
        {
            this.SummedNegativeRating--;
        }

        private void ResetARating(int oldRating)
        {
            switch (oldRating)
            {
                case -1:
                    SubstractNegativeRating();
                    break;
                case 0:
                    break;
                case 1:
                    SubstractPositiveRating();
                    break;
                default:
                    break;
            }
        }

        public abstract bool IsOwner(object id);
        
        public virtual event RepositoryTransactionCompletin OnSaving;
        public virtual void RiseSaving()
        {
            if (OnSaving != null)
            {
                OnSaving(); 
            }
        }

        public virtual long Id
        {
            get;
            set;
        }
    }
}