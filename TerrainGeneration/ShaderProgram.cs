using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace TerrainGeneration
{
    /// <summary>
    /// A struct for an OpenGL texture
    /// </summary>
    public struct Texture : IDisposable
    {
        public int Handle;

        public static implicit operator int(Texture program)
        {
            return program.Handle;
        }

        public static implicit operator Texture(int handle)
        {
            return new Texture()
            {
                Handle = handle
            };
        }

        public void Dispose()
        {
            GL.DeleteTexture(Handle);
        }
    }

    /// <summary>
    /// A struct for an OpenGL shader program
    /// </summary>
    public struct ShaderProgram : IDisposable
    {
        public int Handle;

        public static implicit operator int(ShaderProgram program)
        {
            return program.Handle;
        }

        public static implicit operator ShaderProgram(int handle)
        {
            return new ShaderProgram()
            {
                Handle = handle
            };
        }

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }
    }
}
