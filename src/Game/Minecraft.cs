﻿using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelGame.Engine;
using VoxelGame.Engine.Voxels;
using VoxelGame.Engine.Rendering;
using VoxelGame.Engine.Rendering.Text;
using VoxelGame.Framework;
using VoxelGame.Game.Blocks;
using VoxelGame.Game.Blocks.Models;
using VoxelGame.Engine.Voxels.Helpers;
using VoxelGame.Engine.Voxels.Blocks;
using System.Threading;
using VoxelGame.Game.Level;
using VoxelGame.Engine.Voxels.Chunks;

namespace VoxelGame.Game
{
    /*
     * Note: Memory consumption rises dramatically when loading new chunks, GC probably can't keep up. (Don't know why, I'm not a genius)
     * This probably wasn't the best choice of language, but I'm already knee deep in this shit, so I'm just gonna continue.
     */

    public class Minecraft
    {
        private SpectatorCamera? _camera;
        private Texture2D _hudTexture;

        private static Minecraft? _instance;
        public static Minecraft Instance => _instance!; // Instance should not be null here.

        private Session? _session;
        public Session Session => _session!; // This is done to have the property not be nullable

        public World? CurrentWorld { get; private set; }

        public SpriteBatch? SpriteBatch { get; private set; }

        public McWindow Window { get; }
        public BlockRegistry BlockRegistry { get; }
        public TextureAtlas TextureAtlas { get; }
        public TextRenderer? TextRenderer { get; private set; }

        public Minecraft(McWindow window)
        {
            _instance = this;
            Window = window;

            Input.OnKeyPressed += Input_OnKeyPressed;
            Input.OnMouseClicked += Input_OnMouseClicked;

            BlockRegistry = new BlockRegistry(4);
            TextureAtlas = new TextureAtlas(2, 2, 16, 16);
        }

        private void Input_OnMouseClicked(MouseButton button, KeyModifiers modifiers)
        {
            switch (button)
            {
                case MouseButton.Left:
                    BreakBlock();
                    break;
                case MouseButton.Right:
                    PlaceBlock();
                    break;
            }
        }

        private bool mouseLocked = true;
        private bool wireframe = false;
        private BlockType _blockInHand = BlockType.Earth;

        private void Input_OnKeyPressed(Keys key, int code, KeyModifiers modifiers)
        {
            switch (key)
            {
                case Keys.Escape:
                    mouseLocked = !mouseLocked;
                    Input.SetCursorLocked(mouseLocked);
                    break;

                case Keys.F3:
                    wireframe = !wireframe;
                    Window.SetWireframe(wireframe);
                    break;

                case Keys.F:
                    _blockInHand++;
                    if (_blockInHand > BlockType.Wood)
                        _blockInHand = BlockType.Stone;
                    break;

                // Code for debugging chunks with unexpected or illegal state.
                case Keys.F5:
                    Vector3i chunkIndex = ConvertH.PosToChunkIndex(_camera!.Translation);
                    Chunk? chunk = Session.ChunkManager.GetChunk(chunkIndex);
                    McWindow.Logger.Debug("ljdkfhad"); // Breakpoint would be placed on this line.
                    break;
            }
        }

        private void BreakBlock()
        {
            if (Raycast.Perform(_camera!.Translation, _camera!.Forward, 5, out Raycast.Result result))
                Session.ChunkManager.TrySetBlock(result.Location, BlockType.Air);
        }

        private void PlaceBlock()
        {
            if (Raycast.Perform(_camera!.Translation, _camera!.Forward, 5, out Raycast.Result result))
            {
                Vector3i location = result.Location + ConvertH.DirToVector(result.FaceDirection);
                Session.ChunkManager.TrySetBlock(location, _blockInHand);
            }
        }

        // Initialization which requires GL commands.
        public void Init()
        {
            // Set the render thread priority to the highest.
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            ErrorHandler.Section("Init OpenGL");

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.CullFace(CullFaceMode.Front);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GLHelper.ClearColor(50, 50, 50);

            _camera = new SpectatorCamera(Window.Size, 70, new Vector3(0, 0, -3), 0, 0);
        }

        // Loading assets and expensive calculations.
        public void Load()
        {
            ErrorHandler.Section("Load and compile shaders");
            int spriteShader = ShaderCompiler.Compile(Resources.ReadText("shaders/sprite.glsl"), "sprite.glsl");
            int worldShader = ShaderCompiler.Compile(Resources.ReadText("shaders/world.glsl"), "world.glsl");

            ErrorHandler.Section("Create SpriteBatch");
            SpriteBatch = new SpriteBatch(spriteShader, 256, Window.Size);

            ErrorHandler.Section("Initialize fonts");

            Texture2D fontTexture = Texture2D.FromImage(Resources.ReadImage("fonts/default.png"));
            GlyphAtlas glyphAtlas = new GlyphAtlas(fontTexture, 5, 7);
            TextRenderer = new TextRenderer(glyphAtlas, SpriteBatch, TextRenderer.Settings.Default);

            ErrorHandler.Section("Load textures");
            _hudTexture = Texture2D.FromImage(Resources.ReadImage("textures/hud.png"));

            InitBlocks();

            // Upload the generated texture atlas to OpenGL.
            Texture2D worldTexture = Texture2D.FromImage(TextureAtlas.Texture);

            ErrorHandler.Section("Init world");

            // Create vertex attrib array.
            VertexArrayObject chunkMeshVao = new VertexArrayObject();
            chunkMeshVao.AddVertexAttrib(0, 3, 0); // Vertex position
            chunkMeshVao.AddVertexAttrib(0, 2, 3 * sizeof(float)); // Texture index
            chunkMeshVao.AddVertexAttrib(0, 1, 5 * sizeof(float)); // Vertex brightness

            _session = new Session(new WorldRenderer(chunkMeshVao, worldShader, worldTexture, _camera!));

            ErrorHandler.Section("World gen");

            CurrentWorld = new World(_session.ChunkManager, _camera!);
            CurrentWorld.GenerateAsync();
            Session.RenderWorld = true;

            ErrorHandler.Section("Game loop");
        }

        private void InitBlocks()
        {
            // Add textures to the texture atlas.
            TextureAtlas.Append(Resources.ReadImage("textures/stone.png"));
            TextureAtlas.Append(Resources.ReadImage("textures/earth.png"));
            TextureAtlas.Append(Resources.ReadImage("textures/wood.png"));
            TextureAtlas.Append(Resources.ReadImage("textures/debug.png"));

            ErrorHandler.Section("Init blocks");

            // Initialize properties for the different types of blocks.
            BlockRegistry[BlockType.Stone] = new BlockEntry(BlockParams.Default, new DefaultBlockModel(0));
            BlockRegistry[BlockType.Earth] = new BlockEntry(BlockParams.Default, new DefaultBlockModel(1));
            BlockRegistry[BlockType.Wood] = new BlockEntry(BlockParams.Default, new DefaultBlockModel(2));
            BlockRegistry[BlockType.Debug] = new BlockEntry(BlockParams.Default, new DefaultBlockModel(3));
        }

        public void Unload()
        {
            ErrorHandler.Section("Unload resources");

            GL.UseProgram(0);
            GLHelper.UnbindMeshBuffers();
            GL.BindTexture(TextureTarget.Texture2D, 0);

            Session.Free();

            ErrorHandler.CheckGLErrors();
        }

        public void OnResize(ResizeEventArgs e)
        {
            _camera!.ChangeViewport(e.Size);
            SpriteBatch!.ChangeViewport(e.Size);
        }

        public void RenderScene(double frameTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Session.Render();
        }

        public void RenderOverlay(double frameTime)
        {
            SpriteBatch!.Begin(_hudTexture);

            // Render crosshair.
            SpriteBatch.Quad((int)(Window.ClientSize.X * 0.5f - 3.5f), (int)(Window.ClientSize.Y * 0.5f - 3.5f), 14, 14, new Vector4i(0, 0, 7, 7));
            // Render fps text background.
            SpriteBatch.Quad(0, Window.Size.Y - 86, Window.Size.X / 2, 86, color: new Argb(0f, 0f, 0f, 0.6f));

            SpriteBatch.Flush();
            TextRenderer!.Begin();

            // This causes a lot of string allocations.
            TextRenderer.DrawText(5, Window.Size.Y - 20, $"Fps:{Window.FrameRate}", 10);
            TextRenderer.DrawText(5, Window.Size.Y - 40, $"Facing:{_camera!.Forward}", 10);
            TextRenderer.DrawText(5, Window.Size.Y - 60, $"Location:{_camera!.Translation}", 10);
            TextRenderer.DrawText(5, Window.Size.Y - 80, $"Chunk:{ConvertH.PosToChunkIndex(_camera!.Translation)}", 10);

            TextRenderer.Flush();
        }

        public void UpdateFrame(double frameTime)
        {
            _camera!.Update(frameTime);
            CurrentWorld?.Update();
            Session.Update();
        }
    }
}
