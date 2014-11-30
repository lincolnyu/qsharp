using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Shader.Geometry.Euclid2D;
using Vector2D = QSharp.Shader.Geometry.Triangulation.Primitive.Vector2D;

namespace QSharpTest.Shader.Geometry.Triangulation
{
    [TestClass]
    public class DelaunayTest
    {
        public static IEnumerable<Vector2D> GenerateRandomVertices(double minx, double miny, double maxx, 
            double maxy, double epsilon, int count)
        {
            var rand = new Random();
            var list = new List<Vector2D>();
            var ee = epsilon*epsilon;
            for (; list.Count < count; )
            {
                var x = rand.NextDouble()*(maxx - minx) + minx;
                var y = rand.NextDouble()*(maxy - miny) + miny;
                var nv = new Vector2D {X = x, Y = y};

                // check against existing vertices
                var valid = list.All(v => v.GetSquareDistance(nv) > ee);
                if (valid)
                {
                    list.Add(nv);
                }
            }
            return list;
        }

        [TestMethod]
        public static void Test()
        {
            
        }
    }
}
