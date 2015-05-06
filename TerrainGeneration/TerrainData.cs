using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TerrainGeneration
{
    public class TerrainData
    {
        public Vector3[] VertexPositions { get; protected set; }
        public int DataSizeX { get; protected set; }
        public int DataSizeZ { get; protected set; }

        public Vector3 this[int x, int z]
        {
            get
            {
                return VertexPositions[x + z * DataSizeX];
            }
            set
            {
                VertexPositions[x + z * DataSizeX] = value;
            }
        }

        public int GetIndex(int x, int z)
        {
            return x + z * DataSizeX;
        }

        public TerrainData(Vector3[] vertexPositions, int dataSizeX, int dataSizeZ)
        {
            VertexPositions = vertexPositions;
            DataSizeX = dataSizeX;
            DataSizeZ = dataSizeZ;
        }

        public static TerrainData CreateFlatTerrain(int sizeX, int sizeZ, Vector2 cellSize)
        {
            var data = from z in Enumerable.Range(0, sizeX)
                       from x in Enumerable.Range(0, sizeZ)
                       select new Vector3((float)x * cellSize.X, 0.0f, (float)z * cellSize.Y);

            return new TerrainData(data.ToArray(), sizeX, sizeZ);
        }

        public static TerrainData CreateGaussian(int sizeX, int sizeZ, Vector2 cellSize, float magnitude, Vector2 centerPosition, float standardDev)
        {
            var data = from z in Enumerable.Range(0, sizeX)
                       from x in Enumerable.Range(0, sizeZ)
                       let vecPosition = new Vector2((float)x * cellSize.X, (float)z * cellSize.Y)
                       select new Vector3(vecPosition.X, magnitude * (float)Math.Exp(-(centerPosition - vecPosition).LengthSquared / (2.0f * standardDev * standardDev)), vecPosition.Y);
                           
            return new TerrainData(data.ToArray(), sizeX, sizeZ);
        }

        public Mesh CreateMesh()
        {
            var positionVertexBuffer = new VertexBuffer(GL.GenBuffer(), 3, VertexAttribPointerType.Float);
            var positionIndexBuffer = new IndexBuffer(GL.GenBuffer(), DrawElementsType.UnsignedShort);

            var vertexOffsets = new[] 
                { 
                    new[] { 0, 0 }, 
                    new[] { 0, 1 }, 
                    new[] { 1, 0 }, 
                    new[] { 1, 0 }, 
                    new[] { 0, 1 }, 
                    new[] { 1, 1 }
                };

            // Create index array
            var indicies = (from z in Enumerable.Range(0, DataSizeZ - 1)
                           from x in Enumerable.Range(0, DataSizeX - 1)
                           from offset in vertexOffsets
                           select (short)GetIndex(x + offset[0], z + offset[1])).ToArray();

            // Load buffer data
            GL.BindBuffer(BufferTarget.ArrayBuffer, positionVertexBuffer.Handle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * VertexPositions.Length), VertexPositions, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, positionIndexBuffer.Handle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(short) * indicies.Length), indicies, BufferUsageHint.StaticDraw);

            // Create the mesh
            return new Mesh()
            {
                IsIndexed = true,
                IndexBuffer = positionIndexBuffer,
                PrimitiveCount = indicies.Length,
                PrimitiveType = PrimitiveType.Triangles,
                VertexAttributeBuffers = new[] { positionVertexBuffer }
            };
        }
    }
}