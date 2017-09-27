using System;
using System.Linq;
#if !OldRt
using System.Reflection;
#endif

namespace QSharp.Scheme.Classical.Trees
{
    /// <summary>
    ///  A B-tree object that works on the data it involves
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BTree<T> : BTreeBase<T>
    {
        #region Constructors

        /// <summary>
        ///  Instantiates a B-tree object with creator as per the type of the data to include
        /// </summary>
        /// <param name="comparison">Compares items and determines their orders in the tree</param>
        /// <param name="order">The b-tree order</param>
        public BTree(Comparison<T> comparison, int order) : base(comparison)
        {
            var tt = typeof(T);

            // TODO the two below might be different
#if OldRt || NETSTANDARD
            var interfaces = tt.GetTypeInfo().ImplementedInterfaces;
#else
            var interfaces = tt.GetInterfaces();
#endif

            var isDisposable = interfaces.Any(intf => intf == typeof(IDisposable));

            if (isDisposable)
            {
                Creator = new DisposableCreator();
            }
            else
            {
                Creator = new NondisposableCreator();
            }

            Order = order;
        }

#endregion
    }
}
