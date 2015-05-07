using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

namespace TerrainGeneration
{
    public class Renderer : IDisposable
    {
        public Camera Camera { get; set; }

        protected Size clientSize;
        protected int vertexArray;

        public void Initialize(RenderWindow window)
        {
            clientSize = window.ClientSize;

            // Create camera
            Camera = new TerrainGeneration.Camera();

            // Set our clear color
            GL.ClearColor(Color.CornflowerBlue);

            // Create our vertex array and bind it
            vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(vertexArray);

            // Enable required featuers
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.AlphaTest);
            GL.Enable(EnableCap.Blend);

            // Cull back faces
            GL.Enable(EnableCap.CullFace);
        }

        public void OnResize(Size ClientSize)
        {
            // Window has been resized, change viewport size
            clientSize = ClientSize;
            GL.Viewport(ClientSize);
        }

        public void OnUpdateFrame(FrameEventArgs e)
        {

        }

        public void Render(FrameEventArgs e, Scene scene)
        {
            // Clear the framebuffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 Projection;
            Matrix4 View;

            // Get view and projection matrices
            Camera.GetProjection(out Projection, clientSize.Width, clientSize.Height);
            Camera.GetView(out View);

            // Compound the view and projection matrices
            Matrix4 ViewProj = View * Projection;

            // Render all entities in the scene
            if (scene != null)
            {
                // Group by shader
                var shaderIterator = scene.Entities.GroupBy(t => t.EntityMaterial.Shader);

                foreach (var shaderGroup in shaderIterator)
                {
                    // Change shader
                    GL.UseProgram(shaderGroup.Key);

                    // Group by material
                    var materialIterator = shaderGroup.GroupBy(t => t.EntityMaterial);

                    foreach (var materialGroup in materialIterator)
                    {
                        // Change material if necessary
                        materialGroup.Key.Apply();

                        // Set View Projection Transform
                        GL.UniformMatrix4(materialGroup.Key.ViewProjectionUniform, false, ref ViewProj);

                        // Group by mesh
                        var meshIterator = materialGroup.GroupBy(t => t.EntityMesh);

                        foreach (var meshGroup in meshIterator)
                        {
                            // Enable mesh
                            meshGroup.Key.Enable();

                            foreach (var entity in meshGroup)
                            {
                                // Actually render
                                RenderEntity(entity, ref ViewProj);
                            }

                            // Disable mesh
                            meshGroup.Key.Disable();
                        }
                    }
                }
            }
        }

        public void RenderEntity(Entity entity, ref Matrix4 ViewProj)
        {
            // Set transforms
            GL.UniformMatrix4(entity.EntityMaterial.WorldUniform, false, ref entity.Transform);

            // Draw the mesh
            entity.EntityMesh.Draw();
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(vertexArray);
        }
    }
}
