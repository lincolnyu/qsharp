using System.Collections.Generic;

namespace QSharp.Signal.Visual.Image.Transform2D
{
    public interface IAccessor2D
    {
        double this[int y, int x] { get; set; }
        
        int Height { get; }

        int Width { get; }
    }
}
