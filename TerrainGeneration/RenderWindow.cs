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
    /// A enumeration of different render modes
    /// </summary>
    public enum RenderMode
    {
        NoTexture,
        Textured,
        Multitextured
    }

    /// <summary>
    /// Options for the application
    /// </summary>
    public class ApplicationOptions
    {
        public RenderMode TerrainRenderMode { get; set; }
        public Vector2 UVScale { get; set; }
        public float ErrorConstant { get; set; }
        public float MaxSeedHeight { get; set; }
        public int Iterations { get; set; }

        public static ApplicationOptions Default
        {
            get
            {
                return new ApplicationOptions()
                {
                    TerrainRenderMode = RenderMode.Multitextured,
                    UVScale = new Vector2(1f / 64f, 1f / 64f),
                    ErrorConstant = 1.5f,
                    MaxSeedHeight = 70f,
                    Iterations = 7
                };
            }
        }
    }

    /// <summary>
    /// A window class for displaying render results
    /// </summary>
    public class RenderWindow : GameWindow
    {
        public Renderer Renderer { get; protected set; }
        public Scene Scene { get; protected set; }
        public CameraController CameraController { get; protected set; }
        public ApplicationOptions Options = ApplicationOptions.Default;

        public RenderWindow()
            : base(800, 600, new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(8), 24, 0, 2))
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

        protected virtual Material LoadTerrainMaterial(TerrainData terrainData)
        {
            // Load our shader
            ShaderProgram terrainShader = -1;
            Material terrainMaterial = null;

            // Load materials
            switch (Options.TerrainRenderMode)
            {
                case RenderMode.Multitextured:
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

                        var textureArray = new[] { grassTexture, snowTexture, dirtTexture, rockTexture };
                        var samplerUniforms = new[] { "grassSampler", "snowSampler", "dirtSampler", "rockSampler" };

                        terrainMaterial = new TerrainMultiTextureMaterial(terrainShader, samplerUniforms, textureArray)
                        {
                            UVScale = Options.UVScale,
                            MaxTerrainHeight = terrainData.MaxHeight,
                            MinTerrainHeight = terrainData.MinHeight
                        };
                    }
                    break;

                case RenderMode.Textured:
                    {
                        // Load textures
                        Texture grassTexture = ResourceLoader.LoadTextureFromFile("Textures\\Grass.jpg");
                        Scene.Resources.Add(grassTexture);

                        // Load textured material
                        terrainShader = ResourceLoader.LoadProgramFromFile("Shaders\\TerrainTextured.vert", "Shaders\\TerrainTextured.frag");
                        terrainMaterial = new TerrainTextureMaterial(terrainShader, grassTexture)
                        {
                            UVScale = Options.UVScale
                        };
                    }
                    break;

                case RenderMode.NoTexture:
                    {
                        // Load default material
                        terrainShader = ResourceLoader.LoadProgramFromFile("Shaders\\Terrain.vert", "Shaders\\Terrain.frag");
                        terrainMaterial = new DefaultMaterial(terrainShader);
                    }
                    break;
            }

            // Add resources to scene (auto-cleanup)
            Scene.Resources.Add(terrainShader);

            return terrainMaterial;
        }

        protected virtual void CreateScene()
        {
            // Create our scene
            Scene = new Scene();
            var cellSize = new Vector3(4f, 1f, 4f);

            // Generate mesh data
            Debug.WriteLine("Creating Mesh Data...");
            var heightMap = DiamondSquare.GenerateRandom(Options.ErrorConstant, Options.MaxSeedHeight, Options.Iterations);
            var terrainData = heightMap.ToTerrainData(cellSize);
            var terrainMesh = terrainData.CreateMesh();

            Debug.WriteLine("Loading Materials...");
            var terrainMaterial = LoadTerrainMaterial(terrainData);

            // Create our terrain entity
            var terrainEntity = new Entity()
            {
                EntityMesh = terrainMesh,
                Transform = Matrix4.Identity,
                EntityMaterial = terrainMaterial
            };

            // Add resources to scene (auto-cleanup)
            Scene.Resources.Add(terrainMesh);
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
