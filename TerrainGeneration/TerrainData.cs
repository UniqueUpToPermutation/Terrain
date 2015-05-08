using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TerrainGeneration
{
    /// <summary>
    /// Options for the creation of a mesh from terrain data
    /// </summary>
    public struct MeshCreationOptions
    {
        /// <summary>
        /// Should the created mesh have normal data?
        /// </summary>
        public bool bCreateNormals;

        /// <summary>
        /// The bounds of where to create the mesh
        /// </summary>
        public Nullable<Rectangle> MeshBounds;

        public static MeshCreationOptions Default
        {
            get
            {
                return new MeshCreationOptions()
                {
                    bCreateNormals = true,
                    MeshBounds = null
                };
            }
        }
    }

    /// <summary>
    /// A class which is used to store raw terrain data, can be converted into a mesh
    /// </summary>
    public class TerrainData
    {
        public Vector3[] VertexPositions { get; protected set; }
        public int DataSizeX { get; protected set; }
        public int DataSizeZ { get; protected set; }

        public float MinHeight
        {
            get
            {
                return VertexPositions.Min(v => v.Y);
            }
        }

        public float MaxHeight
        {
            get
            {
                return VertexPositions.Max(v => v.Y);
            }
        }

        protected Vector3[] normalComputionArray;

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

        public Vector3 GetFaceNormal(int x, int z)
        {
            if (x >= 0 && x < DataSizeX - 1 && z >= 0 && z < DataSizeZ - 1)
            {
                var v12 = this[x, z + 1] - this[x, z];
                var v13 = this[x + 1, z] - this[x, z];
                var v24 = this[x + 1, z + 1] - this[x, z + 1];
                var v34 = this[x + 1, z + 1] - this[x + 1, z];

                // Hopefully this gives us an up facing normal
                return Vector3.Cross(v12, v13) + Vector3.Cross(v12, v24) + Vector3.Cross(v34, v13) + Vector3.Cross(v34, v24);
            }
            else
                return Vector3.Zero;
        }

        public Vector3 GetVertexNormal(int x, int z)
        {
            var normal = Vector3.Zero;
            var vertexPos = this[x, z];
            var zDif = Vector3.Zero;
            var xDif = Vector3.Zero;

            if (x > 0 && z > 0)
            {
                xDif = vertexPos - this[x - 1, z];
                zDif = vertexPos - this[x, z - 1];
                normal += Vector3.Normalize(Vector3.Cross(zDif, xDif));
            }

            if (x < DataSizeX - 1 && z > 0)
            {
                xDif = this[x + 1, z] - vertexPos; 
                zDif = vertexPos - this[x, z - 1];
                normal += Vector3.Normalize(Vector3.Cross(zDif, xDif));
            }

            if (x > 0 && z < DataSizeZ - 1)
            {
                xDif = vertexPos - this[x - 1, z];
                zDif = this[x, z + 1] - vertexPos;
                normal += Vector3.Normalize(Vector3.Cross(zDif, xDif));
            }

            if (x < DataSizeX - 1 && z < DataSizeZ - 1)
            {
                xDif = this[x + 1, z] - vertexPos; 
                zDif = this[x, z + 1] - vertexPos;
                normal += Vector3.Normalize(Vector3.Cross(zDif, xDif));
            }

            return normal;
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
            return CreateMesh(MeshCreationOptions.Default);
        }

        /// <summary>
        /// Create a mesh from terrain data
        /// </summary>
        /// <param name="options">Options for mesh creation</param>
        /// <returns></returns>
        public Mesh CreateMesh(MeshCreationOptions options)
        {
            var beginX = (options.MeshBounds.HasValue ? options.MeshBounds.Value.X : 0);
            var beginZ = (options.MeshBounds.HasValue ? options.MeshBounds.Value.Y : 0);
            var countX = (options.MeshBounds.HasValue ? options.MeshBounds.Value.Width : DataSizeX);
            var countZ = (options.MeshBounds.HasValue ? options.MeshBounds.Value.Height : DataSizeZ);

            var positionVertexBuffer = new VertexBuffer(GL.GenBuffer(), 3, VertexAttribPointerType.Float);
            var positionIndexBuffer = new IndexBuffer(GL.GenBuffer(), DrawElementsType.UnsignedShort);
            var vertexAttributeBuffers = new List<VertexBuffer>();

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
            var indicies = (from z in Enumerable.Range(beginZ, countZ - 1)
                           from x in Enumerable.Range(beginX, countX - 1)
                           from offset in vertexOffsets
                           select (short)GetIndex(x + offset[0], z + offset[1])).ToArray();

            // Select a small section of the vertex position array if necessary
            var vertexBufferData = VertexPositions;
            if (options.MeshBounds.HasValue)
            {
                vertexBufferData = (from z in Enumerable.Range(beginZ, countZ)
                                   from x in Enumerable.Range(beginX, countX)
                                   select this[x, z]).ToArray();
            }

            // Load buffer data
            GL.BindBuffer(BufferTarget.ArrayBuffer, positionVertexBuffer.Handle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * VertexPositions.Length), VertexPositions, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, positionIndexBuffer.Handle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(short) * indicies.Length), indicies, BufferUsageHint.StaticDraw);

            // Add this to the attribute buffers
            vertexAttributeBuffers.Add(positionVertexBuffer);

            // Create normal data?
            if (options.bCreateNormals)
            {
                var normalsData = new Vector3[DataSizeX * DataSizeZ];

                // Compute normals by weighted average of nieghboring faces
                for (int z = 0; z < DataSizeZ; ++z)
                    for (int x = 0; x < DataSizeX; ++x)
                    {
                        var normal = GetVertexNormal(x, z);
                        normal.Normalize();
                        normalsData[x + z * DataSizeX] = normal;
                    }

                // Load data into an OpenGL buffer
                var normalVertexBuffer = new VertexBuffer(GL.GenBuffer(), 3, VertexAttribPointerType.Float);
                GL.BindBuffer(BufferTarget.ArrayBuffer, normalVertexBuffer.Handle);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * normalsData.Length), normalsData, BufferUsageHint.StaticDraw);

                // Add the normals buffer to the attribute buffers
                vertexAttributeBuffers.Add(normalVertexBuffer);
            }

            // Create the mesh
            return new Mesh()
            {
                IsIndexed = true,
                IndexBuffer = positionIndexBuffer,
                PrimitiveCount = indicies.Length,
                PrimitiveType = PrimitiveType.Triangles,
                VertexAttributeBuffers = vertexAttributeBuffers.ToArray()
            };
        }
    }
}