using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Shader.Graphics.Base.Exceptions;
using QSharp.Shader.Graphics.Base.Geometry;
using QSharp.Shader.Graphics.Base.Objects;
using QSharp.Shader.Graphics.Base.Optics;
using QSharp.Shader.Graphics.RayTracing;
using QSharp.Shader.Graphics.Csg;

namespace QSharpTest.Shader.Graphics
{
    [TestClass]
    public class CsgIntsTest
    {
        /*
         * <code title="tester objects">
         */

        #region Nested types

        public interface IInsideChecker
        {
            bool IsIn(float pos);
        }

        public class TestNode : CsgRayTraceOpticalNode, IInsideChecker
        {
            public TestNode(ICsgShape left, ICsgShape right, Operation oper)
                : base(left, right, oper)
            {
            }

            public TestNode()
                : base(null, null, Operation.Union) /* put in default values */
            {
            }

            public override IRayTraceSurface InnerSurface 
            { 
                get 
                {
                    throw new NotImplementedException();
                }  
            }
            
            public override IRayTraceSurface OuterSurface 
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public new ICsgShape Left
            {
                get
                {
                    return base.Left;
                }
                set
                {
                    base.Left = value;
                }
            }

            public new ICsgShape Right
            {
                get
                {
                    return base.Right;
                }
                set
                {
                    base.Right = value;
                }
            }

            public new Operation Oper
            {
                get
                {
                    return base.Oper;
                }
                set
                {
                    base.Oper = value;
                }
            }

            public bool IsIn(float pos)
            {
                var iicLeft = Left as IInsideChecker;
                var iicRight = Right as IInsideChecker;
                var inLeft = iicLeft != null && iicLeft.IsIn(pos);
                var inRight = iicRight != null && iicRight.IsIn(pos);
                switch (Oper)
                {
                    case Operation.Union:
                        return inLeft || inRight;
                    case Operation.Intersection:
                        return inLeft && inRight;
                    case Operation.Subtraction:
                        return inLeft && !inRight;
                    default:
                        throw new GraphicsException("Unknown operation");
                }
            }

            public override string ToString()
            {
                var left = Left.ToString();
                var right = Right.ToString();
                if (Left is TestNode)
                {
                    left = '(' + left + ')';
                }
                if (Right is TestNode)
                {
                    right = '(' + right + ')';
                }
                string strOper;
                switch (Oper)
                {
                    case Operation.Union:
                        strOper = "U";
                        break;
                    case Operation.Intersection:
                        strOper = "^";
                        break;
                    case Operation.Subtraction:
                        strOper = "-";
                        break;
                    default:
                        throw new GraphicsException("Unknown operator");
                }
                string result = left + strOper + right;
                return result;
            }
        }

        public class TestLeaf : ICsgLeaf, CsgRayTrace.IIntersectable, IOptical, IInsideChecker
        {
            public float StartingPos
            {
                get; protected set;
            }

            public float EndingPos
            {
                get;
                protected set;
            }

            public class IntersectState : CsgRayTrace.IIntersectState
            {
                public bool Selected { get; set; }

                public float Distance { get; set; }

                protected Intersection Intersection { get; set; }

                public int Stage { get; set; }
                
                public IRayTraceSurface Surface { get; set; }
                
                public IntersectState()
                {
                    Selected = false;
                    Stage = 0;
                }

                public Intersection.Direction Direction
                {
                    get
                    {
                        return Intersection.AccessDirection;
                    }
                    set
                    {
                        throw new GraphicsException("Not supposed to be set");
                    }
                }

                public UndirectionalIntersection UndirectionalIntersection
                {
                    get
                    {
                        return Intersection;
                    }
                    set
                    {
                        throw new GraphicsException("Not supposed to be set");
                    }
                }

                public void SetIntersection(Intersection intersection)
                {
                    Intersection = intersection;
                }
            }

            public TestLeaf(float startingPos, float endingPos)
            {
                StartingPos = startingPos;
                EndingPos = endingPos;
            }

            public CsgRayTrace.IIntersectState IntersectNext(CsgRayTrace.IIntersectState state, Ray ray)
            {
                IntersectState local;
                var fromLeft = ray.Direction.X > 0;
                var firstPos = fromLeft ? StartingPos : EndingPos;
                var secondPos = fromLeft ? EndingPos : StartingPos;

                if (state == null)
                {
                    local = new IntersectState();
                }
                else
                {
                    local = state as IntersectState;
                }

                if (local != null && local.Stage == 0)
                {
                    if (fromLeft)
                    {
                        if (ray.Source.X <= firstPos)
                        {
                            local.Stage = 1;    /* first hit occurs at the left boundary */
                        }
                        else if (ray.Source.X <= secondPos)
                        {
                            local.Stage = 2;    /* first hit occurs at the right boundary */
                        }
                        else
                        {
                            local.Stage = 3;
                        }
                    }
                    else
                    {
                        if (ray.Source.X >= firstPos)
                        {
                            local.Stage = 1;    /* first hit occurs at the left boundary */
                        }
                        else if (ray.Source.X >= secondPos)
                        {
                            local.Stage = 2;    /* first hit occurs at the right boundary */
                        }
                        else
                        {
                            local.Stage = 3;
                        }
                    }
                }

                if (local == null)
                {
                    return null;
                }
                switch (local.Stage)
                {
                    case 1:
                        local.SetIntersection(new Intersection(
                                                  new Vector4f(firstPos, 0, 0), null,
                                                  Intersection.Direction.ToInterior));
                        local.Surface = null;
                        local.Distance = fromLeft ? firstPos - ray.Source.X : ray.Source.X - firstPos;
                        local.Stage++;
                        break;
                    case 2:
                        local.SetIntersection(new Intersection(
                                                  new Vector4f(secondPos, 0, 0), null,
                                                  Intersection.Direction.ToExterior));
                        local.Surface = null;
                        local.Distance = fromLeft ? secondPos - ray.Source.X : ray.Source.X - secondPos;
                        local.Stage++;
                        break;
                    default:
                        local.SetIntersection(null);
                        local.Surface = null;
                        return CsgRayTrace.TerminalState.EnterInstance;
                }
                return local;
            }

            public bool IsIn(float pos)
            {
                return pos >= StartingPos && pos <= EndingPos;
            }

            public override string ToString()
            {
                var strStarting = string.Format("{0:#.00}", StartingPos);
                var strEnding = string.Format("{0:#.00}", EndingPos);
                return "(" + strStarting + ", " + strEnding + ")";
            }

        }   /* class TestLeaf */

        /*
         * </code>
         */

        /* 
         * <code title="verification tools"> 
         */
        public struct Boundary : IComparable<Boundary>
        {
            public float Pos;
            public Intersection.Direction Dir;

            public Boundary(float pos, Intersection.Direction dir)
            {
                Pos = pos;
                Dir = dir;
            }

            public int CompareTo(Boundary that)
            {
                return Pos.CompareTo(that.Pos);
            }
        }

        #endregion

        #region Methods

        public static List<float> AddCandidates(List<float> list, ICsgShape shape)
        {
            if (shape is TestNode)
            {
                var node = shape as TestNode;
                list = AddCandidates(list, node.Left);
                list = AddCandidates(list, node.Right);
            }
            else if (shape is TestLeaf)
            {
                var leaf = shape as TestLeaf;
                var i = list.BinarySearch(leaf.StartingPos);
                if (i < 0)
                {
                    i = -i - 1;
                    list.Insert(i, leaf.StartingPos);
                }
                i = list.BinarySearch(leaf.EndingPos);
                if (i < 0)
                {
                    i = -i - 1;
                    list.Insert(i, leaf.EndingPos);
                }
            }
            else
            {
                throw new GraphicsException("Error node");
            }
            return list;
        }

        public static List<Boundary> GetBoundaries(ICsgShape root, bool fromLeft)
        {
            var boundaries = new List<Boundary>();
            var candidates = new List<float>();

            candidates = AddCandidates(candidates, root);

            if (candidates.Count == 0)
            {
                return boundaries;
            }

            var before = candidates[0] - 1;
            var insideChecker = root as IInsideChecker;
            var beforeIn = insideChecker != null && insideChecker.IsIn(before);

            int i;
            for (i = 0; i < candidates.Count; i++)
            {
                float after;
                if (i < candidates.Count - 1)
                {
                    after = (candidates[i] + candidates[i + 1]) * 0.5f;
                }
                else
                {
                    after = candidates[i] + 1;
                }

                var checker = root as IInsideChecker;
                var afterIn = checker != null && checker.IsIn(after);

                if (!beforeIn && afterIn)
                {   // enter
                    boundaries.Add(new Boundary(candidates[i], Intersection.Direction.ToInterior));
                }
                else if (beforeIn && !afterIn)
                {   // leave
                    boundaries.Add(new Boundary(candidates[i], Intersection.Direction.ToExterior));
                }

                beforeIn = afterIn;
            }

            if (!fromLeft)
            {
                var newBoundaries = new List<Boundary>();
                for (i = boundaries.Count - 1; i >= 0; i--)
                {
                    var boundary = new Boundary(boundaries[i].Pos,
                        (boundaries[i].Dir == Intersection.Direction.ToInterior) ?
                        Intersection.Direction.ToExterior : Intersection.Direction.ToInterior);
                    newBoundaries.Add(boundary);
                }
                boundaries = newBoundaries;
            }

            return boundaries;
        }

        public static int GetFirstBoundaryForRay(Ray ray, List<Boundary> boundaries)
        {
            if (ray.Direction.X > 0)
            {
                var index = boundaries.BinarySearch(new Boundary(ray.Source.X, 
                    Intersection.Direction.ToInterior /* dummy */));

                if (index < 0)
                {
                    index = -index - 1;
                }

                return index;
            }
            else
            {
                var index = boundaries.BinarySearch(new Boundary(ray.Source.X,
                    Intersection.Direction.ToInterior /* dummy */));

                if (index < 0)
                {
                    index = -index;
                }

                return index;
            }

        }

        public static Ray GetRayForBoundary(List<Boundary> boundaries, int index, 
            bool onBoundary, bool fromLeft)
        {

            var rayDir = fromLeft ? new Vector4f(1f, 0f, 0f) : new Vector4f(-1f, 0f, 0f);
            float x;
            if (onBoundary)
            {
                x = boundaries[index].Pos;
            }
            else if (index > 0)
            {
                x = (boundaries[index].Pos + boundaries[index - 1].Pos) * 0.5f;
            }
            else if (fromLeft)
            {
                x = boundaries[0].Pos - 1f;
            }
            else
            {
                x = boundaries[0].Pos + 1f;
            }
            return new Ray(new Vector4f(x, 0f, 0f), rayDir);
        }

        /* 
         * </code> 
         */

        /*
         * <code title="test generator">
         */

        public class TestGenerator
        {
            #region Fields

            public float RegionLow;
            public float RegionHigh;
            public int MinOpers;
            public int MaxOpers;
            public int Balance;
            public int Adherent;

            private readonly Random _rand = new Random(2);
            private readonly int _numOpers;
            private readonly int _numShapes;
            private readonly int _regionGran;

            private readonly List<float> _visited = new List<float>();

            #endregion

            /// <summary>
            ///  Instantiates a TestGenerator
            /// </summary>
            /// <param name="regionLow">lowest point</param>
            /// <param name="regionHigh">highest point</param>
            /// <param name="minOpers">minimum number of operators</param>
            /// <param name="maxOpers">maximum number of operators</param>
            /// <param name="balance">The balance between left and right in percentage</param>
            /// <param name="adherent">in percentage</param>
            public TestGenerator(float regionLow, float regionHigh, 
                int minOpers, int maxOpers, int balance, int adherent)
            {
                RegionLow = regionLow;
                RegionHigh = regionHigh;
                MinOpers = minOpers;
                MaxOpers = maxOpers;
                Balance = balance;
                Adherent = adherent;

                _numOpers = _rand.Next(MinOpers, MaxOpers + 1);
                _numShapes = _numOpers + 1;

                _regionGran = _numShapes * 8;
            }

            public float GetBoundary(List<float> visited)
            {
                var r = _rand.Next(100);
                float boundary;
                if (r >= Adherent && visited.Count > 0)
                {
                    r = _rand.Next(0, visited.Count);
                    boundary = visited[r];
                }
                else
                {
                    r = _rand.Next(0, _regionGran + 1);
                    boundary = (RegionHigh - RegionLow) * r /
                        _regionGran  + RegionLow;

                    var i = visited.IndexOf(boundary);
                    if (i < 0)
                    {
                        i = -i - 1;
                        visited.Insert(i, boundary);
                    }
                }
                return boundary;
            }

            public ICsgShape Generate(int totalOpers)
            {
                if (totalOpers == 0)
                {
                    var pos1 = GetBoundary(_visited);
                    float pos2;
                    /*
                     * testing program doesn't handle the
                     * dot object as good as the testee
                     * so wipe out this case
                     */
                    do
                    {
                        pos2 = GetBoundary(_visited);
                    } while (Math.Abs(pos2 - pos1) < float.Epsilon);

                    var startingPos = Math.Min(pos1, pos2);
                    var endingPos = Math.Max(pos1, pos2);
                    return new TestLeaf(startingPos, endingPos);
                }

                totalOpers--; /* 
                               * we've gone thru the root
                               * so deduct one from the total number of
                               * nodes quota for this subtree 
                               */

                var node = new TestNode();

                var iOperType = _rand.Next(3);
                node.Oper = (CsgNode.Operation)iOperType;

                var nLeft = totalOpers * Balance / 100;
                var nRight = totalOpers - nLeft;

                node.Left = Generate(nLeft);
                node.Right = Generate(nRight);

                return node;
            }

            public ICsgShape Create()
            {
                return Generate(_numOpers);
            }
        }

        private static bool Check(List<Boundary> boundaries, CsgRayTrace.IIntersectable root, 
            bool onBoundary, int iBoundary, bool fromLeft)
        {
            var ray = GetRayForBoundary(boundaries, iBoundary, onBoundary, fromLeft);

            var res = true;
            CsgRayTrace.IIntersectState state = null;

            while (true)
            {
                state = root.IntersectNext(state, ray);
                if (!float.IsPositiveInfinity(state.Distance))
                {
                    if (iBoundary >= boundaries.Count)
                    {
                        res = false;
                        break;
                    }

                    var boundary = boundaries[iBoundary];
                    var pos = state.UndirectionalIntersection.Position.X;
                    var dir = state.Direction;

                    if (Math.Abs(pos - boundary.Pos) < float.Epsilon && dir == boundary.Dir)
                    {
                        iBoundary++;
                    }
                    else
                    {
                        res = false;
                        break;
                    }
                }
                else
                {
                    if (iBoundary < boundaries.Count)
                    {
                        res = false;
                    }
                    break;
                }
            }

            return res;
        }

        [TestMethod]
        public void RandomTest()
        {
            var rand = new Random(100);
            var tg = new TestGenerator(-10f, 10f, 4, 16, 50, 25);

            const int numTests = 1000;

            for (var iTest = 0; iTest<numTests; iTest++)
            {
                bool onBoundary;
                bool fromLeft;
                ICsgShape shape;
                List<Boundary> boundaries;
                do
                {
                    onBoundary = rand.Next(2) > 0;
                    fromLeft = rand.Next(2) > 0;

                    shape = tg.Create();

                    boundaries = GetBoundaries(shape, fromLeft);
                } while (boundaries.Count == 0);    // make sure that it is a valid object that has boundaries

                for (var i = 0; i < boundaries.Count; i++)
                {
                    var b = Check(boundaries, shape as CsgRayTrace.IIntersectable, onBoundary, i, fromLeft);

                    Assert.IsTrue(b, string.Format("Check failed for ray-from-left={0}", fromLeft));
                }
            }
        }

        [TestMethod]
        public void StaticTest()
        {
            var leafLll = new TestLeaf(-9.3f, -4.16f);
            var leafLlrl = new TestLeaf(-3.83f, 5.84f);
            var leafLlrr = new TestLeaf(-10f, -1.16f);
            var leafLrll = new TestLeaf(-9.3f, 8.5f);
            var leafLrlr = new TestLeaf(2.66f, 8f);
            var leafLrrl = new TestLeaf(-10f, -8.3f);
            var leafLrrr = new TestLeaf(-10f, -1.3f);

            var leafRlll = new TestLeaf(-1.16f, 8f);
            var leafRllr = new TestLeaf(5.3f, 8f);
            var leafRlrl = new TestLeaf(-1.16f, 5.83f);
            var leafRlrr = new TestLeaf(7.66f, 8.5f);
            var leafRrll = new TestLeaf(-9.33f, -8.33f);
            var leafRrlr = new TestLeaf(-8.33f, 5.83f);
            var leafRrrl = new TestLeaf(5.83f, 8.0f);
            var leafRrrr = new TestLeaf(-9.3f, 5.3f);


            var nodeLlr = new TestNode(leafLlrl, leafLlrr, CsgNode.Operation.Intersection);
            var nodeLl = new TestNode(leafLll, nodeLlr, CsgNode.Operation.Subtraction);

            var nodeLrl = new TestNode(leafLrll, leafLrlr, CsgNode.Operation.Intersection);
            var nodeLrr = new TestNode(leafLrrl, leafLrrr, CsgNode.Operation.Intersection);
            var nodeLr = new TestNode(nodeLrl, nodeLrr, CsgNode.Operation.Union);

            var nodeRll = new TestNode(leafRlll, leafRllr, CsgNode.Operation.Intersection);
            var nodeRlr = new TestNode(leafRlrl, leafRlrr, CsgNode.Operation.Subtraction);
            var nodeRl = new TestNode(nodeRll, nodeRlr, CsgNode.Operation.Union);

            var nodeRrl = new TestNode(leafRrll, leafRrlr, CsgNode.Operation.Intersection);
            var nodeRrr = new TestNode(leafRrrl, leafRrrr, CsgNode.Operation.Union);
            var nodeRr = new TestNode(nodeRrl, nodeRrr, CsgNode.Operation.Subtraction);

            var nodeL = new TestNode(nodeLl, nodeLr, CsgNode.Operation.Union);
            var nodeR = new TestNode(nodeRl, nodeRr, CsgNode.Operation.Intersection);

            var root = new TestNode(nodeL, nodeR, CsgNode.Operation.Subtraction);

            CsgRayTrace.IIntersectState state = null;
            var ray = new Ray(new Vector4f(-10f, 0f, 0f), new Vector4f(1f, 0f, 0f));

            var boundaries = GetBoundaries(root, true);
            var iBoundary = GetFirstBoundaryForRay(ray, boundaries);

            for (; ; )
            {
                state = root.IntersectNext(state, ray);
                if (float.IsPositiveInfinity(state.Distance))
                {
                    break;
                }

                if (iBoundary >= boundaries.Count)
                {
                    // all boundaries have been checked
                    break;
                }

                var boundary = boundaries[iBoundary];
                var pos = state.UndirectionalIntersection.Position.X;
                var dir = state.Direction;

                //Console.Write("UndirectionalIntersection = {0}, dir = {1} ", pos, dir);

                if (Math.Abs(pos - boundary.Pos) < float.Epsilon && dir == boundary.Dir)
                {
                    // this boundary is correctly hit
                    iBoundary++;
                }
                else
                {
                    Assert.Fail("Boundary is incorrect.");
                }
            }
        }

        #endregion
    }
}
