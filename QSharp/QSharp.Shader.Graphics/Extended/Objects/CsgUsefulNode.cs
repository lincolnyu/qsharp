using QSharp.Shader.Graphics.Csg;
using QSharp.Shader.Graphics.Extended.Wireframe;

namespace QSharp.Shader.Graphics.Extended.Objects
{
    public class CsgUsefulNode : CsgRayTraceOpticalNode, IWireframedShape
    {
        #region Constructor
        
        public CsgUsefulNode(ICsgShape left, ICsgShape right, Operation operation)
            : base(left, right, operation)
        {
        }

        #endregion

        #region Methods

        #region Implementation of IWireframedShape

        public void DrawWireframe(ScreenPlotter screen, IWireframeStyle style)
        {
            IWireframedShape left = Left as IWireframedShape;
            if (left != null)
            {
                left.DrawWireframe(screen, style);
            }
            IWireframedShape right = Right as IWireframedShape;
            if (right != null)
            {
                right.DrawWireframe(screen, style);
            }
        }

        #endregion

        #endregion

        public override RayTracing.IRayTraceSurface OuterSurface
        {
            get { return null; }
        }

        public override RayTracing.IRayTraceSurface InnerSurface
        {
            get { return null; }
        }
    }
}
