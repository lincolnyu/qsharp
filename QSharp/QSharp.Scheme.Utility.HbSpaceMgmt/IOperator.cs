namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    public interface IOperator
    {
        #region Methods

        IPosition Add(IPosition lhs, ISize rhs);
        
        IPosition Subtract(IPosition lhs, ISize rhs);

        ISize Add(ISize lhs, ISize rhs);

        ISize Subtract(ISize lhs, ISize rhs);

        #endregion
    }
}
