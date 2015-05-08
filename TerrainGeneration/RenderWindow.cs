using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK.Input;

namespace TerrainGeneration
{
    /// <summary>
    /// A window class for displaying render results
    /// </summary>
    public class RenderWindow : GameWindow
    {
        public Renderer Renderer { get; protected set; }
        public Scene Scene { get; protected set; }
        public CameraController CameraController { get; protected set; }
        public bool bUseTextured = true;
        public bool bUseMultiTextured = true;

        public RenderWindow() : base(800, 600, new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(8), 24, 0, 4))
        {
            Title = "Terrain Generation Project";
            Icon = Properties.Resources.ProgramIcon;
        }

        protected virtual Renderer CreateRenderer()
        {
            return new Renderer();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Debug.WriteLine("Initializing Renderer...");
            Renderer = CreateRenderer();
            Renderer.Initialize(this);

            Debug.WriteLine("Creating Scene...");
            CreateScene();
        }

        protected override void OnResize(EventArgs e)
        {
            Renderer.OnResize(ClientSize);

            base.OnResize(e);
        }

        protected virtual void CreateScene()
        {
            // Create our scene
            Scene = new Scene();
            var cellSize = new Vector3(4f, 1f, 4f);

            // Load our shader
            Debug.WriteLine("Loading Materials...");
            ShaderProgram terrainShader = -1;
            Material terrainMaterial = null;

            // Load materials
            if (bUseMultiTextured)
            {
                // Load textures
                Texture grassTexture = ResourceLoader.LoadTextureFromFile("Textures\\Grass.jpg");
                Scene.Resources.Add(grassTexture);

                Texture snowTexture = ResourceLoader.LoadTextureFromFile("Textures\\Snow.jpg");
                Scene.Resources.Add(snowTexture);

                Texture dirtTexture = ResourceLoader.LoadTextureFromFile("Textures\\Dirt.jpg");
                Scene.Resources.Add(dirtTexture);

                Texture rockTexture = ResourceLoader.LoadTextureFromFile("Textures\\Rock.jpg");
                Scene.Resources.Add(rockTexture);

                // Load textured material
                terrainShader = ResourceLoader.LoadProgramFromFile("Shaders\\TerrainMultiTextured.vert", "Shaders\\TerrainMultiTextured.frag");
                terrainMaterial = new TerrainMultiTextureMaterial(terrainShader, grassTexture, snowTexture, dirtTexture, rockTexture)
                {
                    UVScale = new Vector2(1f / 64f, 1f / 64f)
                };
            }
            else if (bUseTextured)
            {
                // Load textures
                Texture grassTexture = ResourceLoader.LoadTextureFromFile("Textures\\Grass.jpg");
                Scene.Resources.Add(grassTexture);

                // Load textured material
                terrainShader = ResourceLoader.LoadProgramFromFile("Shaders\\TerrainTextured.vert", "Shaders\\TerrainTextured.frag");
                terrainMaterial = new TerrainTextureMaterial(terrainShader, grassTexture)
                {
                    UVScale = new Vector2(1f / 64f, 1f / 64f)
                };
            }
            else
            {
                // Load default material
                terrainShader = ResourceLoader.LoadProgramFromFile("Shaders\\Terrain.vert", "Shaders\\Terrain.frag");
                terrainMaterial = new DefaultMaterial(terrainShader);
            }
                               
            // Create our terrain entity
            Debug.WriteLine("Creating Mesh Data...");
            var heightMap = DiamondSquare.GenerateRandom(1.5f, 70f, 7);
            var terrainData = heightMap.ToTerrainData(cellSize);
            var terrainMesh = terrainData.CreateMesh();
            var terrainEntity = new Entity()
            {
                EntityMesh = terrainMesh,
                Transform = Matrix4.Identity,
                EntityMaterial = terrainMaterial
            };

            // Add resources to scene (auto-cleanup)
            Scene.Resources.Add(terrainMesh);
            Scene.Resources.Add(terrainShader);
            Scene.Entities.Add(terrainEntity); 
   
            // Position the camera correctly
            CameraController = new RotationCameraController(Renderer.Camera, Keyboard, Mouse)
            {
                CameraCenter = new Vector3(cellSize.X * (float)heightMap.Width / 2f, 0f, cellSize.Z * (float)heightMap.Width / 2f),
                Phi = (float)Math.PI / 4f,
                Radius = cellSize.X * (float)heightMap.Width / 2f + cellSize.Z * (float)heightMap.Width / 2f
            };
            CameraController.UpdateCameraParams();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Keyboard[Key.Escape])
                Close();

            if (CameraController != null)
                CameraController.UpdateCamera(e);

            Renderer.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // Render the scene
            Renderer.Render(e, Scene);

            SwapBuffers();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (CameraController != null)
                CameraController.Dispose();

            // Dispose all remaining resources
            Debug.WriteLine("Disposing Resources...");
            Scene.Dispose();
            Renderer.Dispose();
            Renderer = null;

            base.OnClosed(e);
        }
    }
}
