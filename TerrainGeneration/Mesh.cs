using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace TerrainGeneration
{
    /// <summary>
    /// A class which represents a mesh object composed of multiple vertex buffers and an optional index buffer
    /// </summary>
    public class Mesh : IDisposable
    {
        public VertexBuffer[] VertexAttributeBuffers;
        public IndexBuffer IndexBuffer;
        public bool IsIndexed;
        public PrimitiveType PrimitiveType;
        public int PrimitiveCount;

        public void Enable()
        {
            for (int i = 0, length = VertexAttributeBuffers.Length; i < length; ++i)
            {
                GL.EnableVertexAttribArray(i);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexAttributeBuffers[i].Handle);
                GL.VertexAttribPointer(i, VertexAttributeBuffers[i].ComponentsPerAttribute,
                    VertexAttributeBuffers[i].AttributeType, VertexAttributeBuffers[i].ShouldNormalize,
                    VertexAttributeBuffers[i].Stride, VertexAttributeBuffers[i].Offset);
            }

            if (IsIndexed)
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer.Handle);
        }

        public void Draw()
        {
            if (IsIndexed)
                GL.DrawElements(PrimitiveType, PrimitiveCount, IndexBuffer.Type, 0);
            else
                GL.DrawArrays(PrimitiveType, 0, PrimitiveCount);
        }

        public void Disable()
        {
            for (int i = 0, length = VertexAttributeBuffers.Length; i < length; ++i)
                GL.DisableVertexAttribArray(i);
        }
    
        public void Dispose()
        {
            foreach (var vertBuffer in VertexAttributeBuffers)
                vertBuffer.Dispose();

            if (IsIndexed)
                IndexBuffer.Dispose();
        }

        public static Mesh CreateTestTriangle()
        {
            var vertexArray = new[]
            {
                new Vector3(-1.0f, -1.0f, 0.0f),
                new Vector3(1.0f, -1.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f)
            };

            var vertBuffer = new VertexBuffer(GL.GenBuffer(), 3, VertexAttribPointerType.Float);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertBuffer.Handle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * vertexArray.Length), vertexArray, BufferUsageHint.StaticDraw);

            return new Mesh()
            {
                IsIndexed = false,
                PrimitiveCount = vertexArray.Length,
                PrimitiveType = PrimitiveType.Triangles,
                VertexAttributeBuffers = new[] { vertBuffer }
            };
        }
    }
}
