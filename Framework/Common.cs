using OpenTK.Graphics.OpenGL4;

namespace pixel_lab;

public static class Common
{
    public static int CompileShader(ShaderType type, string source)
    {
        var shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out var status);
        if (status != 1)
        {
            var infoLog = GL.GetShaderInfoLog(shader);
            throw new InvalidOperationException($"[{type}] {infoLog}");
        }

        return shader;
    }

    public static int LinkProgram(int vertexShader, int fragmentShader)
    {
        var program = GL.CreateProgram();
        GL.AttachShader(program, vertexShader);
        GL.AttachShader(program, fragmentShader);
        GL.LinkProgram(program);
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var status);
        if (status != 1)
        {
            var infoLog = GL.GetProgramInfoLog(program);
            throw new InvalidOperationException(infoLog);
        }

        return program;
    }

    public static int LinkProgram(string vertexShaderSource, string fragmentShaderSource)
    {
        var vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
        var fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);
        return LinkProgram(vertexShader, fragmentShader);
    }

    public static (int fbo, int texture) InitializeFboAndTexture(int width = 1, int height = 1,
        TextureMinFilter minFilter = TextureMinFilter.Nearest,
        TextureMagFilter magFilter = TextureMagFilter.Nearest,
        TextureWrapMode wrapS = TextureWrapMode.Repeat,
        TextureWrapMode wrapT = TextureWrapMode.Repeat)
    {
        var fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

        var texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb,
            PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapS);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapT);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, texture, 0);

        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            throw new InvalidOperationException("Framebuffer not complete.");

        return (fbo, texture);
    }
}