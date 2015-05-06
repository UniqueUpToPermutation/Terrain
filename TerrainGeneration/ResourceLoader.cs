using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TerrainGeneration
{
    /// <summary>
    /// A resource loader class for loading textures and shader programs
    /// </summary>
    public static class ResourceLoader
    {
        public static int LoadTextureFromFile(string filename)
        {
            // Validate name
            if (String.IsNullOrEmpty(filename))
                throw new ArgumentException(filename);

            // Create a new texture2D and bind it
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            // Load bitmap data
            Bitmap bmp = new Bitmap(filename);
            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Copy data
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            bmp.UnlockBits(bmp_data);

            // Generate mipmaps
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return id;
        }

        public static int LoadProgramFromFile(string vertexShaderSource, string fragmentShaderSource)
        {
            // Based off code from here: http://www.opengl-tutorial.org/
            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

            string strVertex;
            string strFragment;

            // Read files
            try
            {
                StreamReader file = new StreamReader(vertexShaderSource);
                strVertex = file.ReadToEnd();
                file.Close();

                file = new StreamReader(fragmentShaderSource);
                strFragment = file.ReadToEnd();
                file.Close();
            }
            catch (Exception e)
            {
                throw e;
            }

            string log;
            int status_code;

            // Compile vertex shader
            Debug.WriteLine("Compiling " + vertexShaderSource + "...");
            GL.ShaderSource(vertexShaderHandle, strVertex);
            GL.CompileShader(vertexShaderHandle);
            GL.GetShaderInfoLog(vertexShaderHandle, out log);
            GL.GetShader(vertexShaderHandle, ShaderParameter.CompileStatus, out status_code);
            Debug.Write(log);

            if (status_code != 1)
                return 0;

            // Compile vertex shader
            Debug.WriteLine("Compiling " + fragmentShaderSource + "...");
            GL.ShaderSource(fragmentShaderHandle, strFragment);
            GL.CompileShader(fragmentShaderHandle);
            GL.GetShaderInfoLog(fragmentShaderHandle, out log);
            GL.GetShader(fragmentShaderHandle, ShaderParameter.CompileStatus, out status_code);
            Debug.Write(log);

            if (status_code != 1)
                return 0;

            // Create the shader program
            int shaderProgram = GL.CreateProgram();

            // Attach shaders
            GL.AttachShader(shaderProgram, vertexShaderHandle);
            GL.AttachShader(shaderProgram, fragmentShaderHandle);

            // Link the program
            Debug.WriteLine("Linking " + vertexShaderSource + " and " + fragmentShaderSource + "...");
            GL.LinkProgram(shaderProgram);

            // Output program log info
            GL.GetProgramInfoLog(shaderProgram, out log);
            Debug.Write(log);

            // Delete used shaders
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);

            return shaderProgram;
        }
    }
}
