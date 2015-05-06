using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace TerrainGeneration
{
    /// <summary>
    /// A data structure for an OpenGL index buffer
    /// </summary>
    public struct IndexBuffer : IDisposable
    {
        public int Handle;
        public DrawElementsType Type;
    
        public IndexBuffer(int handle, DrawElementsType type)
        {
            Handle = handle;
            Type = type;
        }

        public void Dispose()
        {
            GL.DeleteBuffer(Handle);
        }
    }
}
