using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace TerrainGeneration
{
    public abstract class Material
    {
        public ShaderProgram Shader { get; set; }
        public int WorldUniform { get; protected set; }
        public int ViewProjectionUniform { get; protected set; }
        public List<Texture> Textures = new List<Texture>();

        public abstract void Apply();
        public abstract void SetParameter(string parameterName, object value);
    }

    public class DefaultMaterial : Material
    {
        public DefaultMaterial(ShaderProgram program)
        {
            Shader = program;
            WorldUniform = GL.GetUniformLocation(program, "WorldTransform");
            ViewProjectionUniform = GL.GetUniformLocation(program, "ViewProjectionTransform");

            if (WorldUniform < 0)
                Debug.WriteLine("Could not find WorldTransform uniform!");
            if (ViewProjectionUniform < 0)
                Debug.WriteLine("Could not find ViewProjection uniform!");
        }

        public override void Apply()
        {
        }

        public override void SetParameter(string parameterName, object value)
        {
        }
    }
}
