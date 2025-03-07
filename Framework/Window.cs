using System.Diagnostics;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace pixel_lab;

public class Window : GameWindow
{
    public static Window Instance { get; private set; } = null!;
    public event Action<float>? Render;
    float _elapsed;
    private int _fbo;
    private int _screenProgram;
    private int _fboTexture;
    private Vector2i? _resolution;
    public float? AspectRatio { get; set; }

    public int Fbo => _fbo;

    public Vector2i Resolution
    {
        get => _resolution ?? ClientSize;
        set
        {
            _resolution = value;
            (_fbo, _fboTexture) = Common.InitializeFboAndTexture(_resolution.Value.X, _resolution.Value.Y);
        }
    }

    public Window() : base(new(), new())
    {
        Instance = this;
        Resolution = ClientSize;
        _screenProgram = InitializeScreenShader();
        GL.Enable(EnableCap.Blend);
    }

    public void ClearColor(Color color)
    {
        GL.ClearColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    }

    public void ClearColor(float r, float g, float b, float a = 1.0f)
    {
        GL.ClearColor(r, g, b, a);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        var sw = Stopwatch.StartNew();
        base.OnRenderFrame(args);
        _elapsed += (float)args.Time;
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        Render?.Invoke(_elapsed);
        DrawToScreen();
        UniformTexture.ResetTextureUnitBindings();
        SwapBuffers();
        var elapsedSeconds = sw.Elapsed.TotalSeconds;
        var fps = 1.0f / elapsedSeconds;
        this.Title = $"FPS: {Math.Round(fps)}";
    }

    private void DrawToScreen()
    {
        var windowWidth = Window.Instance.ClientSize.X;
        var windowHeight = Window.Instance.ClientSize.Y;
        var viewportWidth = windowWidth;
        var viewportHeight = windowHeight;
        var viewportX = 0;
        var viewportY = 0;

        if (AspectRatio.HasValue && AspectRatio.Value > 0)
        {

            if (windowWidth < windowHeight)
            {
                viewportWidth = windowWidth;
                viewportHeight = (int)(viewportWidth / AspectRatio.Value);
                viewportY = (windowHeight - viewportHeight) / 2; // Center vertically
            }
            else
            {
                viewportHeight = windowHeight;
                viewportWidth = (int)(viewportHeight / AspectRatio.Value);
                viewportX = (windowWidth - viewportWidth) / 2; // Center horizontally
            }
        }

        // Bind default framebuffer (screen)
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    
        // Set viewport with adjusted aspect ratio
        GL.Viewport(viewportX, viewportY, viewportWidth, viewportHeight);

        // Render the screen quad
        GL.UseProgram(_screenProgram);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _fboTexture);
        GL.Uniform1(1, 0);
        GL.Uniform2(0, (float)windowWidth, (float)windowHeight);
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
    }


    private int InitializeScreenShader()
    {
        return Common.LinkProgram(
            """
            #version 330 core
            void main() {
                vec2 positions[4] = vec2[4](
                    vec2( 1.0, -1.0),
                    vec2(-1.0, -1.0),
                    vec2( 1.0,  1.0),
                    vec2(-1.0,  1.0)
                );
                gl_Position = vec4(positions[gl_VertexID], 0.0, 1.0);
            }
            """,
            """
            #version 330 core
            out vec4 FragColor;
            uniform vec2 resolution;
            uniform sampler2D screenTexture;

            void main() {
                vec2 uv = gl_FragCoord.xy / resolution;
                FragColor = texture(screenTexture, uv);
                // FragColor = vec4(uv, 0.0, 1.0);
            }
            """);
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        if (KeyboardState.IsKeyDown(Keys.Escape)) Close();
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        WriteDeviceInfo();
    }

    private void WriteDeviceInfo()
    {
        Console.WriteLine($"Vendor:    {GL.GetString(StringName.Vendor)}");
        Console.WriteLine($"Renderer:  {GL.GetString(StringName.Renderer)}");
        Console.WriteLine($"OpenGL:    {GL.GetString(StringName.Version)}");
        Console.WriteLine($"GLSL:      {GL.GetString(StringName.ShadingLanguageVersion)}");
        Console.WriteLine($"Textures:  {GL.GetInteger(GetPName.MaxTextureImageUnits)}");
    }
}