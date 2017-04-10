using System;
#if !OldDotNet
using ICloneable = QSharp.Shared.ICloneable;
#endif

namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    public interface IPosition : IEncodable, IComparable<IPosition>, ICloneable
    {
        #region Methods

        IPosition Add(ISize size);

        IPosition Subtract(ISize size);

        #endregion
    }

}
