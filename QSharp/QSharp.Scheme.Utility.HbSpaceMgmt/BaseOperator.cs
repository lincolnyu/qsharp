namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    public abstract class BaseOperator : IPaginator
    {
        #region Methods

        #region IPaginator members

        public virtual IPosition Paginate(IPosition pos, ISize pageSize)
        {
            return pos;
        }

        public virtual ISize Paginate(ISize size, ISize pageSize)
        {
            return size;
        }

        public virtual IPosition Unpaginate(IPosition pos, ISize pageSize)
        {
            return pos;
        }

        public virtual ISize Unpaginate(ISize size, ISize pageSize)
        {
            return size;
        }

        public abstract ISize OnePage { get; }

        #endregion

        public virtual IPosition Add(IPosition lhs, ISize rhs)
        {
            return lhs.Add(rhs);
        }

        public virtual IPosition Subtract(IPosition lhs, ISize rhs)
        {
            return lhs.Subtract(rhs);
        }

        public virtual ISize Add(ISize lhs, ISize rhs)
        {
            return lhs.Add(rhs);
        }

        public virtual ISize Subtract(ISize lhs, ISize rhs)
        {
            return lhs.Subtract(rhs);
        }

        #endregion
    }
}
