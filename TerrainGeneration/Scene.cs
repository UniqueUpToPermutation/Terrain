using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace TerrainGeneration
{
    /// <summary>
    /// Represents a scene which can be rendered by the renderer
    /// </summary>
    public class Scene : IDisposable
    {
        public List<Entity> Entities = new List<Entity>();
        public List<IDisposable> Resources = new List<IDisposable>();

        public void Dispose()
        {
            foreach (var resource in Resources)
                resource.Dispose();
        }
    }

    /// <summary>
    /// An entity in a scene
    /// </summary>
    public class Entity
    {
        public Mesh EntityMesh;
        public int EntityProgram;
        public Matrix4 Transform;
    }
}
