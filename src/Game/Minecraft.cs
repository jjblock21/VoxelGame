using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelGame.Engine;
using VoxelGame.Framework.Helpers;
using VoxelGame.Engine.Voxels;
using VoxelGame.Engine.Rendering;
using VoxelGame.Engine.Rendering.Text;
using VoxelGame.Framework;
using VoxelGame.Game.Blocks;
using VoxelGame.Game.Blocks.Models;

namespace VoxelGame.Game
{
    public class Minecraft
    {
        private SpectatorCamera? _camera;
        private FrameCounter _frameCounter;
        private Texture2D _hudTexture;

        private static Minecraft? _instance;
        public static Minecraft Instance => _instance!; // Instance should not be null here.

        public World? CurrentWorld { get; private set; }

        public SpriteBatch? SpriteBatch { get; private set; }
        public WorldRenderer? WorldRenderer { get; private set; }

        public McWindow Window { get; }
        public BlockRegistry BlockRegistry { get; }
        public TextureAtlas TextureAtlas { get; }
        public TextRenderer? TextRenderer { get; private set; }

        public Minecraft(McWindow window)
        {
            _instance = this;
            Window = window;

            Input.OnKeyPressed += Input_OnKeyPressed;
            Input.OnKeyReleased += Input_OnKeyReleased;
            Input.OnMouseClicked += Input_OnMouseClicked;

            BlockRegistry = new BlockRegistry(3);
            TextureAtlas = new TextureAtlas(2, 2, 16, 16);

            _frameCounter = new FrameCounter(1f);
        }

        private void Input_OnMouseClicked(MouseButton button, KeyModifiers modifiers)
        {
            switch (button)
            {
                case MouseButton.Left:
                    if (Raycast.Perform(_camera!.Translation, _camera!.Forward, 5, out Raycast.Result result))
                    {
                        CurrentWorld?.TryPlaceBlock(result.Location, BlockType.Air);
                    }
                    break;
                case MouseButton.Right:
                    if (Raycast.Perform(_camera!.Translation, _camera!.Forward, 5, out Raycast.Result result2))
                    {
                        Vector3i location = result2.Location + World.DirToVector(result2.FaceDirection);
                        CurrentWorld?.TryPlaceBlock(location, _placeBlock);
                    }
                    break;
            }
        }

        private bool wireframe = false;
        private bool updateWireframe = false;

        private void Input_OnKeyReleased(Keys key, int code, KeyModifiers modifiers)
        {
            if (key == Keys.F4)
            {
                wireframe = false;
                updateWireframe = true;
            }
        }

        private bool mouseLocked = true;
        private BlockType _placeBlock = BlockType.Earth;

        private void Input_OnKeyPressed(Keys key, int code, KeyModifiers modifiers)
        {
            if (key == Keys.Escape)
            {
                mouseLocked = !mouseLocked;
                Input.SetCursorLocked(mouseLocked);
            }
            if (key == Keys.F4)
            {
                wireframe = true;
                updateWireframe = true;
            }
            if (key == Keys.F)
            {
                _placeBlock++;
                if (_placeBlock > BlockType.Wood)
                    _placeBlock = BlockType.Stone;
            }
        }


        // Initialization which requires GL commands.
        public void Init()
        {
            ErrorHandler.Section("Init OpenGL");

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.CullFace(CullFaceMode.Front);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _camera = new SpectatorCamera(Window.Size, 70, new Vector3(0, 0, -3), 0, 0);
        }

        // Loading assets and expensive calculations.
        public void Load()
        {
            ErrorHandler.Section("Set up Vertex Arrays");

            // Create vertex attrib array.
            VertexArrayObject chunkMeshVao = new VertexArrayObject();
            chunkMeshVao.AddVertexAttrib(0, 3, 0); // Vertex position
            chunkMeshVao.AddVertexAttrib(0, 2, 3 * sizeof(float)); // Texture index
            chunkMeshVao.AddVertexAttrib(0, 1, 5 * sizeof(float)); // Vertex brihgness

            ErrorHandler.Section("Load and compile shaders");
            int spriteShader = ShaderCompiler.Compile(Resources.ReadText("shaders/sprite.glsl"), "sprite.glsl");
            int worldShader = ShaderCompiler.Compile(Resources.ReadText("shaders/world.glsl"), "world.glsl");

            ErrorHandler.Section("Create SpriteBatch");
            SpriteBatch = new SpriteBatch(spriteShader, 16, Window.Size);

            ErrorHandler.Section("Intialize fonts");

            Texture2D fontTexture = Texture2D.FromImage(Resources.ReadImage("fonts/default.png"));
            GlyphAtlas glyphAtlas = new GlyphAtlas(fontTexture, 5, 6);
            TextRenderer = new TextRenderer(glyphAtlas, SpriteBatch, TextRenderer.Settings.Default);

            ErrorHandler.Section("Load textures");
            _hudTexture = Texture2D.FromImage(Resources.ReadImage("textures/hud.png"));

            InitBlocks();

            // Upload the gnerated texture atlas to OpenGL.
            Texture2D worldTexture = Texture2D.FromImage(TextureAtlas.Texture);

            ErrorHandler.Section("Init world");

            WorldRenderer = new WorldRenderer(chunkMeshVao, worldShader, worldTexture, _camera!);
            CurrentWorld = new World(WorldRenderer);

            ErrorHandler.Section("World gen");

            // Generate cylinder of chunks.
            const int radius = 8;
            const int height = 4;
            const int radiusSquared = radius * radius;
            VectorUtility.Vec3For(-radius, -height, -radius, radius, height, radius, vec =>
            {
                int ldist = vec.X * vec.X + vec.Z * vec.Z;
                if (ldist < radiusSquared) CurrentWorld.GenChunk(vec);
            });

            ErrorHandler.Section("Game loop");
        }

        private void InitBlocks()
        {
            Image<Rgba32> stoneTexture = Resources.ReadImage("textures/stone.png");
            TextureAtlas.AddTexture(stoneTexture);
            Image<Rgba32> earthTexture = Resources.ReadImage("textures/earth.png");
            TextureAtlas.AddTexture(earthTexture);
            Image<Rgba32> woodTexture = Resources.ReadImage("textures/wood.png");
            TextureAtlas.AddTexture(woodTexture);

            ErrorHandler.Section("Init blocks");

            // Air block doesn't need data.

            SharedBlockData stoneData = new SharedBlockData(BlockParams.Default, new DefaultBlockModel(0));
            BlockRegistry.Register(BlockType.Stone, stoneData);

            SharedBlockData earthData = new SharedBlockData(BlockParams.Default, new DefaultBlockModel(1));
            BlockRegistry.Register(BlockType.Earth, earthData);

            SharedBlockData woodData = new SharedBlockData(BlockParams.Default, new DefaultBlockModel(2));
            BlockRegistry.Register(BlockType.Wood, woodData);
        }

        public void Unload()
        {
            GL.UseProgram(0);

            ErrorHandler.Section("Unload resources");
            GLHelper.UnbindMeshBuffers();
            GL.BindTexture(TextureTarget.Texture2D, 0);
            WorldRenderer!.Free();

            ErrorHandler.CheckGLErrors();
        }

        public void OnResize(ResizeEventArgs e)
        {
            _camera!.ChangeViewport(e.Size);
            SpriteBatch!.ChangeViewport(e.Size);
        }

        public void RenderFrame(double frameTime)
        {
            _frameCounter.Sample(frameTime);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            CurrentWorld?.Render();

            GL.Disable(EnableCap.DepthTest);
            SpriteBatch!.Begin(_hudTexture);

            // Render crosshair.
            SpriteBatch.Quad((int)(Window.ClientSize.X * 0.5f - 3.5f), (int)(Window.ClientSize.Y * 0.5f - 3.5f), 14, 14, new Vector4i(0, 0, 7, 7));

            // Render fps text background.
            SpriteBatch.Quad(0, Window.Size.Y - 22, 100, 22, color: new Vector4(0f, 0f, 0f, 0.6f));

            SpriteBatch.Flush();
            TextRenderer!.Begin();

            TextRenderer.DrawText(6, Window.Size.Y - 16, $"Fps:{_frameCounter.FrameRate}", 10);

            TextRenderer.Flush();
            GL.Enable(EnableCap.DepthTest);

            ErrorHandler.CheckGLErrors();
        }

        public void UpdateFrame(double frameTime)
        {
            if (updateWireframe)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, wireframe ? PolygonMode.Line : PolygonMode.Fill);
                if (wireframe) GL.Disable(EnableCap.Blend); else GL.Enable(EnableCap.Blend);
                updateWireframe = false;
            }
            CurrentWorld?.Update();
            _camera!.Update(frameTime);
        }
    }
}
