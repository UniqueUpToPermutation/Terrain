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

            // Wireframe
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
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
                foreach (var entity in scene.Entities)
                    RenderEntity(entity, ref ViewProj);
            }
        }

        public void RenderEntity(Entity entity, ref Matrix4 ViewProj)
        {
            // We'll do the slow way for now, clean this up later
            GL.UseProgram(entity.EntityProgram);

            var worldParam = GL.GetUniformLocation(entity.EntityProgram, "WorldTransform");
            var viewProjParam = GL.GetUniformLocation(entity.EntityProgram, "ViewProjectionTransform");
            
            // Set transforms
            GL.UniformMatrix4(worldParam, false, ref entity.Transform);
            GL.UniformMatrix4(viewProjParam, false, ref ViewProj);

            // Set InputColor parameter to red
            /* var inputColorParam = GL.GetUniformLocation(entity.EntityProgram, "InputColor");
            GL.Uniform3(inputColorParam, Vector3.UnitX); */

            // Draw the mesh
            entity.EntityMesh.Enable();
            entity.EntityMesh.Draw();
            entity.EntityMesh.Disable();
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(vertexArray);
        }
    }
}
