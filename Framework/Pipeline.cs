using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace pixel_lab;

public record RenderTarget(int FrameBuffer, int Texture)
{
    public void Clear()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBuffer);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }
}

public class Pipeline
{
    public record BlendingFactors
    {
        public BlendingFactor SourceFactor;
        public BlendingFactor DestinationFactor;
    }

    private int? _vao;
    private int? _ebo;
    private int? _program;
    private int? _lastGoodProgram;

    private string? _vertexShaderSource;
    private string? _fragmentShaderSource;

    private readonly Dictionary<string, Uniform> _uniforms = new();
    private readonly Dictionary<string, Attribute> _attributes = new();
    private static int _textureUnit = 0;

    public PrimitiveType PrimitiveType { get; set; } = PrimitiveType.Triangles;

    // finalColor = (sourceColor × srcFactor) + (destinationColor × dstFactor)
    public BlendingFactors BlendingFunction { get; set; } = new()
        { SourceFactor = BlendingFactor.SrcAlpha, DestinationFactor = BlendingFactor.OneMinusSrcAlpha };

    public int DrawStart { get; set; }

    public int DrawCount { get; set; }

    public int InstanceCount { get; set; } = 1;

    public void Draw()
    {
        Draw(Window.Instance.Fbo);
    }

    public void Draw(int framebuffer)
    {
        if (_program == null)
        {
            if (_vertexShaderSource == null || _fragmentShaderSource == null)
                throw new InvalidOperationException("Shader sources not set.");
            try
            {
                _program = Common.LinkProgram(_vertexShaderSource, _fragmentShaderSource);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                if (_lastGoodProgram == null)
                    throw new InvalidOperationException("Shader compilation failed.");
                _program = _lastGoodProgram.Value;
            }

            GL.UseProgram(_program.Value);
            _lastGoodProgram = _program.Value;
        }

        if (_vao == null)
        {
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao.Value);
            foreach (var attribute in _attributes.Values)
                attribute.Bind(_program.Value, _vao.Value);
        }

        GL.UseProgram(_program.Value);

        foreach (var uniform in _uniforms.Values)
            uniform.Upload(_program.Value);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
        GL.BindVertexArray(_vao.Value);
        GL.UseProgram(_program.Value);
        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        GL.Viewport(0, 0, Window.Instance.Resolution.X, Window.Instance.Resolution.Y);
         
        GL.BlendFunc(BlendingFunction.SourceFactor, BlendingFunction.DestinationFactor);

        if (_ebo.HasValue)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo.Value);
            GL.DrawElementsInstanced(PrimitiveType, DrawCount, DrawElementsType.UnsignedInt, IntPtr.Zero,
                InstanceCount);
        }
        else
        {
            GL.DrawArraysInstanced(PrimitiveType, DrawStart, DrawCount, InstanceCount);
        }
    }

    public static int Texture(string path,
        TextureMinFilter minFilter = TextureMinFilter.Linear,
        TextureMagFilter magFilter = TextureMagFilter.Linear,
        TextureWrapMode wrapS = TextureWrapMode.Repeat,
        TextureWrapMode wrapT = TextureWrapMode.Repeat)
    {
        var foundPath = Watchers.FindFile(path);
        StbImage.stbi_set_flip_vertically_on_load(1);
        var image = ImageResult.FromStream(File.OpenRead(foundPath), ColorComponents.RedGreenBlueAlpha);

        var handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, handle);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapS);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapT);
        GL.BindTexture(TextureTarget.Texture2D, handle);

        return handle;
    }

    public static RenderTarget RenderTarget(int width = 1, int height = 1,
        TextureMinFilter minFilter = TextureMinFilter.Nearest,
        TextureMagFilter magFilter = TextureMagFilter.Nearest,
        TextureWrapMode wrapS = TextureWrapMode.Repeat,
        TextureWrapMode wrapT = TextureWrapMode.Repeat)
    {
        var (fbo, texture) = Common.InitializeFboAndTexture(width, height, minFilter, magFilter, wrapS, wrapT);
        return new RenderTarget(fbo, texture);
    }


    #region Shader

    public void ShaderFiles(string vertexShaderPath, string fragmentShaderPath)
    {
        vertexShaderPath = Watchers.FindFile(vertexShaderPath);
        fragmentShaderPath = Watchers.FindFile(fragmentShaderPath);
        Watchers.Watch(vertexShaderPath, _ =>
        {
            _vertexShaderSource = File.ReadAllText(vertexShaderPath);
            Warnings.ClearCache();
            _program = null;
        });
        Watchers.Watch(fragmentShaderPath, _ =>
        {
            _fragmentShaderSource = File.ReadAllText(fragmentShaderPath);
            Warnings.ClearCache();
            _program = null;
        });
        _vertexShaderSource = File.ReadAllText(vertexShaderPath);
        _fragmentShaderSource = File.ReadAllText(fragmentShaderPath);
    }

    public void ShaderSource(string vertexShaderSource, string fragmentShaderSource)
    {
        _vertexShaderSource = vertexShaderSource;
        _fragmentShaderSource = fragmentShaderSource;
        _program = null;
    }

    #endregion

    #region indexed rendering

    public uint[] Indexes
    {
        set
        {
            if (!_ebo.HasValue)
                _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo.Value);
            GL.BufferData(BufferTarget.ElementArrayBuffer, value.Length * sizeof(uint), value,
                BufferUsageHint.StaticDraw);
        }
    }
    
    #endregion

    #region Attributes

    public Attribute Attribute(
        string name,
        float[] data,
        int? size = null,
        VertexAttribPointerType? type = null,
        bool normalized = false,
        int stride = 0,
        int offset = 0,
        int divisor = 0,
        BufferUsageHint usage = BufferUsageHint.StaticDraw)
    {
        var attribute = Attribute(name, GL.GenBuffer(), size, type, normalized, stride, offset, divisor);
        attribute.Update(data, usage);
        return attribute;
    }

    public Attribute Attribute(
        string name,
        System.Numerics.Vector3[] data,
        int? size = null,
        VertexAttribPointerType? type = null,
        bool normalized = false,
        int stride = 0,
        int offset = 0,
        int divisor = 0,
        BufferUsageHint usage = BufferUsageHint.StaticDraw)
    {
        var attribute = Attribute(name, GL.GenBuffer(), size, type, normalized, stride, offset, divisor);
        attribute.Update(data, usage);
        return attribute;
    }

    public Attribute Attribute(
        string name,
        int vbo,
        int? size = null,
        VertexAttribPointerType? type = VertexAttribPointerType.Float,
        bool normalized = false,
        int stride = 0,
        int offset = 0,
        int divisor = 0)
    {
        _attributes[name] = new Attribute
        {
            Name = name,
            Vbo = vbo,
            Size = size,
            Type = type,
            Normalized = normalized,
            Stride = stride,
            Offset = offset,
            Divisor = divisor
        };
        return _attributes[name];
    }

    public void UpdateAttribute(string name, float[] data) =>
        _attributes[name].Update(data);

    #endregion

    #region Uniforms

    public void Uniform(string name, float value) =>
        _uniforms[name] = new UniformFloat { Name = name, Value = value };

    public void Uniform(string name, Matrix4 value) =>
        _uniforms[name] = new UniformMatrix4 { Name = name, Value = value };

    public void Uniform(string name, int value) =>
        _uniforms[name] = new UniformTexture { Name = name, Value = value };

    public void Uniform(string name, Vector2 value) =>
        _uniforms[name] = new UniformVector2 { Name = name, Value = value };

    public void Uniform(string name, Vector2i value) =>
        _uniforms[name] = new UniformVector2 { Name = name, Value = value };

    public void Uniform(string name, Vector3 value) =>
        _uniforms[name] = new UniformVector3 { Name = name, Value = value };

    public void Uniform(string name, Vector4 value) =>
        _uniforms[name] = new UniformVector4 { Name = name, Value = value };

    public void Uniform(string name, Color value) =>
        _uniforms[name] = new UniformVector3()
            { Name = name, Value = new Vector3(value.R / 255f, value.G / 255f, value.B / 255f) };

    public void Uniform(string name, Matrix2 value) =>
        _uniforms[name] = new UniformMatrix2 { Name = name, Value = value };


    public void Uniform(string name, Matrix3 value) =>
        _uniforms[name] = new UniformMatrix3 { Name = name, Value = value };

    #endregion
}