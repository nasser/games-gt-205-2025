using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace pixel_lab;

public class Window : GameWindow
{
    public static Window Instance { get; private set; } = null!;
    public event Action<float>? Render;
    float _elapsed;

    public Window() : base(new(), new())
    {
        Instance = this;
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
        base.OnRenderFrame(args);
        _elapsed += (float)args.Time;
        GL.Clear(ClearBufferMask.ColorBufferBit);
        Render?.Invoke(_elapsed);
        SwapBuffers();
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
        Console.WriteLine($"Vendor:   {GL.GetString(StringName.Vendor)}");
        Console.WriteLine($"Renderer: {GL.GetString(StringName.Renderer)}");
        Console.WriteLine($"OpenGL:   {GL.GetString(StringName.Version)}");
        Console.WriteLine($"GLSL:     {GL.GetString(StringName.ShadingLanguageVersion)}");
    }
}