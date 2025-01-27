using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace pixel_lab;

public class Pipeline
{
    private int? _vao;
    private int? _fbo;
    private int? _fboTexture;
    private int? _program;
    private int? _screenProgram;
    private int? _lastGoodProgram;

    private string? _vertexShaderSource;
    private string? _fragmentShaderSource;

    private readonly Dictionary<string, Uniform> _uniforms = new();
    private readonly Dictionary<string, Attribute> _attributes = new();
    private Vector2i _resolution = Window.Instance.ClientSize;

    public PrimitiveType PrimitiveType { get; set; } = PrimitiveType.Triangles;

    public int DrawStart { get; set; }

    public int DrawCount { get; set; }

    public Vector2i Resolution
    {
        get => _resolution;
        set
        {
            _fbo = null;
            _resolution = value;
        }
    }

    public void Draw()
    {
        if (_program == null)
        {
            if (_vertexShaderSource == null || _fragmentShaderSource == null)
                throw new InvalidOperationException("Shader sources not set.");
            try
            {
                _program = LinkProgram(_vertexShaderSource, _fragmentShaderSource);
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

        var fboSize = Resolution;

        // TODO not ideal
        if (_fbo == null)
            (_fbo, _fboTexture) = InitializeFboAndTexture(fboSize.X, fboSize.Y);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo.Value);
        GL.BindVertexArray(_vao.Value);
        GL.UseProgram(_program.Value);
        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.Viewport(0, 0, fboSize.X, fboSize.Y);
        GL.DrawArrays(PrimitiveType, DrawStart, DrawCount);
        
        DrawToScreen();
    }

    private void DrawToScreen()
    {
        _screenProgram ??= InitializeScreenShader();
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Viewport(0, 0, Window.Instance.ClientSize.X, Window.Instance.ClientSize.Y);
        GL.UseProgram(_screenProgram.Value);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _fboTexture!.Value);
        GL.Uniform1(1, 0);
        GL.Uniform2(0, (float)Window.Instance.ClientSize.X, (float)Window.Instance.ClientSize.Y);
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
    }

    private int InitializeScreenShader()
    {
        return LinkProgram(
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

    (int fbo, int texture) InitializeFboAndTexture(int width = 1, int height = 1)
    {
        var fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        
        var texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture, 0);
        
        if( GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            throw new InvalidOperationException("Framebuffer not complete.");

        return (fbo, texture);
    }
    
    #region Shader

    private int CompileShader(ShaderType type, string source)
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

    private int LinkProgram(int vertexShader, int fragmentShader)
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

    private int LinkProgram(string vertexShaderSource, string fragmentShaderSource)
    {
        var vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
        var fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);
        return LinkProgram(vertexShader, fragmentShader);
    }

    public void ShaderFiles(string vertexShaderPath, string fragmentShaderPath)
    {
        vertexShaderPath = Watchers.FindFild(vertexShaderPath);
        fragmentShaderPath = Watchers.FindFild(fragmentShaderPath);
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
    
    #region Attributes

    public Attribute Attribute(
        string name,
        float[] data,
        int? size = null,
        VertexAttribPointerType? type = null,
        bool normalized = false,
        int stride = 0,
        int offset = 0,
        BufferUsageHint usage = BufferUsageHint.StaticDraw)
    {
        var attribute = Attribute(name, GL.GenBuffer(), size, type, normalized, stride, offset);
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
        int offset = 0)
    {
        _attributes[name] = new Attribute
        {
            Name = name,
            Vbo = vbo,
            Size = size,
            Type = type,
            Normalized = normalized,
            Stride = stride,
            Offset = offset
        };
        return _attributes[name];
    }

    public void UpdateAttribute(string name, float[] data) =>
        _attributes[name].Update(data);
    
    #endregion
    
    #region Uniforms

    public void Uniform(string name, float value) =>
        _uniforms[name] = new UniformFloat { Name = name, Value = value };

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