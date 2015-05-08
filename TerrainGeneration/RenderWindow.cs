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
        public Vector3 CellSize { get; set; }
        public bool Fullscreen { get; set; }
        public int ChuckSize { get; set; }

        /// <summary>
        /// Generate the default terrain
        /// </summary>
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
                    Iterations = 7,
                    CellSize = new Vector3(4f, 1f, 4f),
                    Fullscreen = false,
                    ChuckSize = 128
                };
            }
        }

        /// <summary>
        /// Generate a larger terrain
        /// </summary>
        public static ApplicationOptions LargeTerrain
        {
            get
            {
                return new ApplicationOptions()
                {
                    TerrainRenderMode = RenderMode.Multitextured,
                    UVScale = new Vector2(1f / 64f, 1f / 64f),
                    ErrorConstant = 0.5f,
                    MaxSeedHeight = 100f,
                    Iterations = 8,
                    CellSize = new Vector3(2f, 1f, 2f),
                    Fullscreen = false,
                    ChuckSize = 128
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
        public ApplicationOptions Options = ApplicationOptions.LargeTerrain;

        public RenderWindow()
            : base(800, 600, new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(8), 24, 0, 2))
        {
            Title = "Terrain Generation Project";
            Icon = Properties.Resources.ProgramIcon;

            if (Options.Fullscreen)
                WindowState = OpenTK.WindowState.Fullscreen;
        }

        protected virtual Renderer CreateRenderer()
        {
            return new Renderer();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Keyboard.KeyDown += OnKeyDown;

            // Create the renderer
            Debug.WriteLine("Initializing Renderer...");
            Renderer = CreateRenderer();
            Renderer.Initialize(this);

            // Create the scene to be rendered
            Debug.WriteLine("Creating Scene...");
            CreateScene();
        }

        protected void OnKeyDown(object obj, KeyboardKeyEventArgs args)
        {
            // Toggle wireframe
            if (args.Key == Key.Tilde)
                if (Renderer != null)
                    Renderer.UseWireframe = !Renderer.UseWireframe;
        }

        protected override void OnResize(EventArgs e)
        {
            // Let the renderer know the screen size changed
            Renderer.OnResize(ClientSize);

            base.OnResize(e);
        }

        /// <summary>
        /// Load the material and textures for the terrain entities
        /// </summary>
        /// <param name="terrainData">The terrain data</param>
        /// <returns>A material for the terrain</returns>
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

        protected virtual void CreateTerrainChunks(TerrainData terrainData, Material terrainMaterial)
        {
            // Generate terrain chunks
            var terrainChunks = terrainData.CreateMeshChunks(Options.ChuckSize, MeshCreationOptions.Default);

            foreach (var chunk in terrainChunks)
            {
                // Create our terrain entity
                var terrainChunkEntity = new Entity()
                {
                    EntityMesh = chunk,
                    Transform = Matrix4.Identity,
                    EntityMaterial = terrainMaterial
                };

                // Add resources to scene (auto-cleanup)
                Scene.Resources.Add(chunk);
                Scene.Entities.Add(terrainChunkEntity);
            }
        }

        /// <summary>
        /// Creates a scene to be rendered
        /// </summary>
        protected virtual void CreateScene()
        {
            // Create our scene
            Scene = new Scene();
            var cellSize = Options.CellSize;

            // Generate mesh data
            Debug.WriteLine("Creating Height Data...");
            var heightMap = DiamondSquare.GenerateRandom(Options.ErrorConstant, Options.MaxSeedHeight, Options.Iterations);
            var terrainData = heightMap.ToTerrainData(cellSize);
            
            Debug.WriteLine("Loading Materials...");
            var terrainMaterial = LoadTerrainMaterial(terrainData);

            Debug.WriteLine("Creating Mesh Data...");
            CreateTerrainChunks(terrainData, terrainMaterial);

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
            // Close the application if the user presses escape
            if (Keyboard[Key.Escape])
                Close();

            // Update the camera
            if (CameraController != null)
                CameraController.UpdateCamera(e);

            // Update the renderer
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
