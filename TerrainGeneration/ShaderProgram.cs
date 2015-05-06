using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace TerrainGeneration
{
    /// <summary>
    /// A class for an OpenGL shader program
    /// </summary>
    class ShaderProgram : IDisposable
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
