using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace TerrainGeneration
{
    /// <summary>
    /// A data structure for an OpenGL vetex buffer
    /// </summary>
    public struct VertexBuffer : IDisposable
    {
        public int Handle;
        public int Stride;
        public int ComponentsPerAttribute;
        public bool ShouldNormalize;
        public int Offset;
        public VertexAttribPointerType AttributeType;

        public VertexBuffer(int handle, int componentsPerAttribute, VertexAttribPointerType attributeType)
        {
            Handle = handle;
            ComponentsPerAttribute = componentsPerAttribute;
            AttributeType = attributeType;
            ShouldNormalize = false;
            Stride = 0;
            Offset = 0;
        }

        public VertexBuffer(int handle, int componentsPerAttribute, VertexAttribPointerType attributeType, int stride, bool shouldNormalize, int offset)
            : this(handle, componentsPerAttribute, attributeType)
        {
            Stride = stride;
            ShouldNormalize = shouldNormalize;
            Offset = offset;
        }

        public void Dispose()
        {
            GL.DeleteBuffer(Handle);
        }
    }
}
