using System;
using System.Collections.Generic;
using System.Linq;

namespace QSharp.Scheme.Classical.Hash
{
    /// <summary>
    ///  Split ordered hash base class that leaves bucket implementation to the derived
    /// </summary>
    /// <typeparam name="T">The type of the data kept in the hash</typeparam>
    public abstract class SoHashBase<T>
    {
        #region Enumerations

        /// <summary>
        ///  How to deal with duplication on adding
        /// </summary>
        public enum AddStrategy
        {
            ReplaceExisting,
            ReturnFalseOnExisting,
            AddDuplicate,
        }

        #endregion

        #region Nested classes

        /// <summary>
        ///  bucket table interface to be implemented by concrete so-hash table bucket implementation
        /// </summary>
        protected interface IBucketTable
        {
            #region Properties

            /// <summary>
            ///  returns the node at the specified index to the table
            /// </summary>
            /// <param name="index">The index</param>
            /// <returns>The node; null if not initialized</returns>
            BaseNode this[int index] { get; set; }

            #endregion
        }

        /// <summary>
        ///  The base node, also holds all the data for a dummy node
        /// </summary>
        protected class BaseNode
        {
            #region Fields

            /// <summary>
            ///  SO-key for the node (bit-reversal) plus 1 if non-dummy
            /// </summary>
            public uint Key;

            /// <summary>
            ///  A reference that points to the next node
            /// </summary>
            public BaseNode Next;

            #endregion
        }

        /// <summary>
        ///  The derived node that holds all the data for a non-dummy node
        /// </summary>
        protected class Node : BaseNode
        {
            #region Fields

            /// <summary>
            ///  The data associated with the node
            /// </summary>
            public T Value;

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>
        ///  The number of bits needed to represent an index of a bucket;
        ///  Unlike TableSize this is maintained by this base class, although they
        ///  should always be correspondent
        /// </summary>
        /// <remarks>
        ///  Therefore the table size has to initially set to 2
        /// </remarks>
        protected int TableIndexBits = 1;

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates and initializes the split ordered hash table (base class part)
        /// </summary>
        protected SoHashBase()
        {
            Count = 0;
        }

        #endregion

        #region Properties

        /// <summary>
        ///  The total number of items (key-value pairs)
        /// </summary>
        public uint Count { get; protected set; }

        /// <summary>
        ///  The size of the bucket table; it's provided by the implementor however it should always hold that
        ///  it starts at 2 and doubles only after a CAS
        /// </summary>
        /// <remarks>
        ///  Any bucket in a table within this range should either be null (noninitialised) or with a dummy node
        /// </remarks>
        protected abstract int TableSize { get; }
        
        /// <summary>
        ///  The bucket table for the hash algorithm to access
        /// </summary>
        protected abstract IBucketTable Buckets { get; }

        #endregion

        #region Methods

        /// <summary>
        ///  Determines if an expansion is needed and performs it if so. 
        ///  The implementation must internally call Double() to perform a CAS based expansion if the
        ///  expansion is decided to be performed and at the end of a TableSize is by design doubled
        /// </summary>
        protected abstract void ExpandIfNeeded();

        /// <summary>
        ///  Adds a node to the specified bucket as part of the CAS expanding process;
        ///  The adding is always starting from the previous table end and performed in order 
        /// </summary>
        /// <param name="indexBucket">The location of the bucket (in some implementation might be ignored</param>
        /// <param name="node">The node to add</param>
        protected abstract void AddBucket(int indexBucket, BaseNode node);

        /// <summary>
        ///  Sets buckets to initial state after clear up the contents of the hash
        /// </summary>
        protected abstract void ResetBuckets();

        /// <summary>
        ///  Adds a key value pair to the hash table
        /// </summary>
        /// <param name="key">The numeric key to the value</param>
        /// <param name="value">The value associated with the key</param>
        /// <param name="addStrategy">How to deal with duplication</param>
        /// <returns>true if the pair is added</returns>
        public bool AddKeyValuePair(uint key, T value, AddStrategy addStrategy = AddStrategy.ReplaceExisting)
        {
            var soDummyKey = Reverse(key);
            var soKey = soDummyKey | 0x1;
            var node = new Node
                {
                    Key = soKey,
                    Value = value,
                };

            // lock for synchronising writers (add, remove and clear)
            // tests suggest that the locks have to be at minimum enclosing the following code piece(s)
            // to make the method reentrant in parallel computing circumstances
            lock (this)
            {
                var indexBucket = (int) (key & ((uint) TableSize - 1));
                var cp = Buckets[indexBucket] ?? InitializeBucket(indexBucket);
                // move to the one after the insertion point
                for (; cp.Next != null && cp.Next.Key < soKey; cp = cp.Next)
                {
                }

                if (cp.Next != null && cp.Next.Key == soKey)
                {
                    switch (addStrategy)
                    {
                        case AddStrategy.ReplaceExisting:
                            ((Node)cp.Next).Value = value;
                            return true;
                        case AddStrategy.ReturnFalseOnExisting:
                            return false;
                    }
                }

                node.Next = cp.Next;
                cp.Next = node;

                Count++;

                ExpandIfNeeded();
            }
            return true;
        }

        /// <summary>
        ///  Removes all the contents of the hash and reinitializes it
        /// </summary>
        public void Clear()
        {
            // lock for synchronising writers (add, remove and clear)
            // tests suggest that the locks have to be at minimum enclosing the following code piece(s)
            // to make the method reentrant in parallel computing circumstances
            lock (this)
            {
                var cp = Buckets[0];
                if (cp == null) return;

                for (; cp.Next != null; cp.Next = cp.Next.Next)
                {
                    if (cp.Next is Node && ((Node) cp.Next).Value is IDisposable)
                    {
                        ((IDisposable) ((Node) cp.Next).Value).Dispose();
                    }
                }
                ResetBuckets();
                TableIndexBits = 1;
                Count = 0;
            }
        }

        /// <summary>
        ///  Finds the first value with the specified key
        /// </summary>
        /// <param name="key">The key the value to find with</param>
        /// <param name="value">The value</param>
        /// <returns>true if found</returns>
        public bool FindFirst(uint key, out T value)
        {
            var node = FindFirst(key);
            if (node == null)
            {
                value = default(T);
                return false;
            }
            value = node.Value;
            return true;
        }

        /// <summary>
        ///  Finds all values with the specified key
        /// </summary>
        /// <param name="key">The key the values to find with</param>
        /// <returns>All the values</returns>
        public IEnumerable<T> Find(uint key)
        {
            var all = FindAll(key);
            return all.Select(n => n.Value);
        }

        /// <summary>
        ///  Finds the first non-dummy node with the key
        /// </summary>
        /// <param name="key">The key the node to find with</param>
        /// <returns>The node or null if not found</returns>
        protected Node FindFirst(uint key)
        {
            var indexBucket = (int)(key & ((uint)TableSize - 1));
            var soDummyKey = Reverse(key);
            var soKey = soDummyKey | 0x1;

            var cp = Buckets[indexBucket];
            if (cp == null) return null;
            for (; cp != null && cp.Key < soKey; cp = cp.Next)
            {
            }

            if (cp != null && cp.Key == soKey)
            {
                return (Node)cp;
            }

            return null;
        }

        /// <summary>
        ///  Returns all non-dummy nodes with the specified key
        /// </summary>
        /// <param name="key">The normal key to the nodes</param>
        /// <returns>All the nodes</returns>
        protected IEnumerable<Node> FindAll(uint key)
        {
            var indexBucket = (int)(key & ((uint)TableSize - 1));
            var soDummyKey = Reverse(key);
            var soKey = soDummyKey | 0x1;
            
            var cp = Buckets[indexBucket];
            if (cp == null)
            {
                yield break;
            }
            
            for (; cp != null && cp.Key < soKey; cp = cp.Next)
            {
            }

            for (; cp != null && cp.Key == soKey; cp = cp.Next)
            {
                yield return (Node)cp;
            }
        }

        /// <summary>
        ///  Deletes all key value pairs with the specified key
        /// </summary>
        /// <param name="key">The normal key to the target to find</param>
        /// <returns>The number of pairs deleted</returns>
        public int DeleteKey(uint key)
        {
            return DeleteKeyValuePairs(key, obj => true);
        }

        /// <summary>
        ///   Deletes key value pairs with the specified key and that can pass the predicate test
        /// </summary>
        /// <param name="key">The normal key to the target to find</param>
        /// <param name="isTarget">A predicate that determine if the target with the specified key is accepted</param>
        /// <returns>The number of pairs deleted</returns>
        public int DeleteKeyValuePairs(uint key, Predicate<T> isTarget)
        {
            var soDummyKey = Reverse(key);
            var soKey = soDummyKey | 0x1;

            // lock for synchronising writers (add, remove and clear)
            // tests suggest that the locks have to be at minimum enclosing the following code piece(s)
            // to make the method reentrant in parallel computing circumstances
            lock (this)
            {
                var indexBucket = (int) (key & ((uint) TableSize - 1));
                var cp = Buckets[indexBucket];
                if (cp == null) return 0;

                var numDeleted = 0;

                for (; cp.Next != null && cp.Next.Key < soKey; cp = cp.Next)
                {
                }

                for (; cp.Next != null && cp.Next.Key == soKey;)
                {
                    var probe = (Node) cp.Next; // by definition with a non-dummy key it's a Node
                    if (isTarget(probe.Value))
                    {
                        cp.Next = probe.Next;
                        var valueToDispose = probe.Value as IDisposable;
                        if (valueToDispose != null)
                        {
                            valueToDispose.Dispose();
                        }
                        numDeleted++;
                        Count--;
                    }
                    else
                    {
                        cp = cp.Next;
                    }

                }
                return numDeleted;
            }
        }

        /// <summary>
        ///  Doubles the bucket table
        /// </summary>
        protected void Double()
        {
            // expands the bucket list
            var oldSize = TableSize;
            for (var i = 0; i < oldSize; i++)
            {
                var cp = Buckets[i];
                if (cp == null)
                {   // parent uninitialized
                    AddBucket(oldSize + i, null); 
                    continue; 
                }
                var msb = cp.Key >> (32 - TableIndexBits);
                var testbit = 1 << (31 - TableIndexBits);
                for (; cp.Next != null; cp = cp.Next)
                {
                    if ((cp.Next.Key & testbit) != 0 || cp.Next.Key >> (32 - TableIndexBits) != msb)
                    {
                        break;
                    }
                }
                if (cp.Next != null && cp.Next.Key >> (32 - TableIndexBits) == msb)
                {
                    var dummyNode = new BaseNode
                    {
                        Key = (cp.Next.Key >> (31 - TableIndexBits)) << (31 - TableIndexBits),
                        Next = null
                    };

                    AddBucket(oldSize + i, dummyNode);

                    // this order to ensure readers are unaffected
                    dummyNode.Next = cp.Next;
                    cp.Next = dummyNode;
                }
                else
                {
                    AddBucket(oldSize + i, null);
                }
            }

            TableIndexBits++;
        }

        /// <summary>
        ///  Initialise a non-initialized (with a null pointer) bucket
        /// </summary>
        /// <param name="indexBucket">The index of the bucket</param>
        /// <returns>A dummy node the bucket now points to</returns>
        protected BaseNode InitializeBucket(int indexBucket)
        {
            var soDummyKey = Reverse((uint)indexBucket);
            BaseNode dummyNode;
            if (indexBucket == 0)
            {
                // NOTE the bucket 0 is always by itself or recursively initialised before any other buckets
                dummyNode = new BaseNode
                {
                    Key = soDummyKey,
                    Next = null
                };
            }
            else
            {
                var parentBucket = GetParent(indexBucket);
                var parent = Buckets[parentBucket] ?? InitializeBucket(parentBucket);
                dummyNode = new BaseNode
                {
                    Key = soDummyKey,
                };
                ListInsert(dummyNode, parent);
            }
            Buckets[indexBucket] = dummyNode;
            return dummyNode;
        }

        /// <summary>
        ///  Inserts the node at the appropriate position after 'start'
        /// </summary>
        /// <param name="node">The node to insert</param>
        /// <param name="start">The starting point guaranteed to be before where the node is to be inserted</param>
        /// <returns>Inserted node if insertion is successful or the existing node</returns>
        protected BaseNode ListInsert(BaseNode node, BaseNode start)
        {
            // move to the one after the insertion point
            for (; start.Next != null && start.Next.Key < node.Key; start = start.Next)
            {
            }
            if (start.Next != null && start.Next.Key == node.Key)
            {
                return start.Next;
            }
            node.Next = start.Next;
            start.Next = node;
            
            return node;
        }

        /// <summary>
        ///  Returns the parent bucket of the specified bucket; basically
        ///  it removes the highest signifcant bit from the input
        /// </summary>
        /// <param name="indexBucket">The index of the bucket</param>
        /// <returns>The index of the parent bucket</returns>
        /// <remarks>
        ///  The current tweaked version is from the following source
        ///   http://stackoverflow.com/questions/2589096/find-most-significant-bit-left-most-that-is-set-in-a-bit-array
        ///  Note this class is not supposed to call this method with bucket number 0
        ///  however the implementation is better to be able to cope with that
        /// </remarks>
        protected static int GetParent(int indexBucket)
        {
            int[] bval = {0, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4};
            var x = (uint) indexBucket;
            var r = -1; // this implementation can handle 0 bucket index
            if ((x & 0xFFFF0000) != 0)
            {
                r += 16/1;
                x >>= 16/1;
            }
            if ((x & 0x0000FF00) != 0)
            {
                r += 16/2;
                x >>= 16/2;
            }
            if ((x & 0x000000F0) != 0)
            {
                r += 16/4;
                x >>= 16/4;
            }
            r += bval[x];
            var mask = 1U << r; // C# does cyclic shift; negative value does right shift
            indexBucket = (int) (((uint) indexBucket) & ~mask);
            return indexBucket;
        }

        /// <summary>
        ///  Returns the bit-reversal of the specified key
        /// </summary>
        /// <param name="key">The key to bit-reverse</param>
        /// <returns>The bit-reversal of the key</returns>
        /// <remarks>
        ///  The current implementation is based on the 3-operation approach from
        ///   1. http://graphics.stanford.edu/~seander/bithacks.html#BitReverseObvious
        ///   2. http://stackoverflow.com/questions/1688532/how-to-reverse-bits-of-a-byte
        /// </remarks>
        protected static uint Reverse(uint key)
        {
            var b0 = key & 0xff;
            var b1 = (key >> 8) & 0xff;
            var b2 = (key >> 16) & 0xff;
            var b3 = (key >> 24) & 0xff;

            // reverse the bytes
            if (b0 != 0)
            {
                b0 = (uint) ((b0*0x0202020202UL & 0x010884422010UL)%1023);
            }
            if (b1 != 0)
            {
                b1 = (uint) ((b1*0x0202020202UL & 0x010884422010UL)%1023);
            }
            if (b2 != 0)
            {
                b2 = (uint) ((b2*0x0202020202UL & 0x010884422010UL)%1023);
            }
            if (b3 != 0)
            {
                b3 = (uint) ((b3*0x0202020202UL & 0x010884422010UL)%1023);
            }

            return ((b0 << 24) | (b1 << 16) | (b2 << 8) | b3);
        }

        #endregion
    }
}
