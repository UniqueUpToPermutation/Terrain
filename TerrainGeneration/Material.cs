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

        public Material(ShaderProgram shader)
        {
            Shader = shader;
            LocateTransformUniforms();
        }

        public void LocateTransformUniforms()
        {
            WorldUniform = GL.GetUniformLocation(Shader, "WorldTransform");
            ViewProjectionUniform = GL.GetUniformLocation(Shader, "ViewProjectionTransform");

            if (WorldUniform < 0)
                Debug.WriteLine("Could not find WorldTransform uniform!");
            if (ViewProjectionUniform < 0)
                Debug.WriteLine("Could not find ViewProjection uniform!");
        }
    }

    public class DefaultMaterial : Material
    {
        public DefaultMaterial(ShaderProgram program) : base(program)
        {
        }

        public override void Apply()
        {
        }

        public override void SetParameter(string parameterName, object value)
        {
        }
    }

    public class TerrainTextureMaterial : Material
    {
        public Vector2 UVScale = Vector2.One;
        protected int uvScaleUniform;

        public TerrainTextureMaterial(ShaderProgram program, Texture texture1, Texture texture2, Texture texture3, Texture texture4) : base(program)
        {
            Textures.Add(texture1);
            Textures.Add(texture2);
            Textures.Add(texture3);
            Textures.Add(texture4);

            uvScaleUniform = GL.GetUniformLocation(program, "UVScale");
            if (uvScaleUniform < 0)
                Debug.WriteLine("Could not find UVScale uniform!");
        }

        public override void Apply()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Textures[0]);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, Textures[1]);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, Textures[2]);

            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, Textures[3]);

            GL.Uniform2(uvScaleUniform, ref UVScale);
        }

        public override void SetParameter(string parameterName, object value)
        {
            if (parameterName == "UVScale")
                UVScale = (Vector2)value;
        }
    }
}
