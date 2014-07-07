using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSharp.Shader.Graphics.Csg
{
    /// <summary>
    ///  CSG node
    /// </summary>
    public class CsgNode : ICsgShape
    {
        #region Nested types
        
        /// <summary>
        ///  a collection of CSG node operations
        /// </summary>
        public enum Operation
        {
            Union,
            Intersection,
            Subtraction
        }

        #endregion

        #region Properties

        /// <summary>
        ///  node on the left side of the operation
        /// </summary>
        protected ICsgShape Left { set; get; }

        /// <summary>
        ///  node on the right side of the operation
        /// </summary>
        protected ICsgShape Right { set; get; }

        /// <summary>
        ///  operation on the nodes to form the current node
        /// </summary>
        protected Operation Oper { set; get; }

        #endregion

        /// <summary>
        ///  constructor that initializes the object with necessary information 
        /// </summary>
        /// <param name="left">left node</param>
        /// <param name="right">right node</param>
        /// <param name="oper">operation on the two nodes above</param>
        public CsgNode(ICsgShape left, ICsgShape right, Operation oper)
        {
            Left = left;
            Right = right;
            Oper = oper;
        }
    }
}
