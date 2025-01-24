using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace pixel_lab;

public class Pipeline
{
    private int? _vao;
    private int? _program;
    private int? _lastGoodProgram;

    private string? _vertexShaderSource;
    private string? _fragmentShaderSource;

    private readonly Dictionary<string, Uniform> _uniforms = new();
    private readonly Dictionary<string, Attribute> _attributes = new();

    public PrimitiveType PrimitiveType { get; set; } = PrimitiveType.Triangles;

    public int DrawStart { get; set; }

    public int DrawCount { get; set; }

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

        foreach (var uniform in _uniforms.Values)
            uniform.Upload(_program.Value);
        GL.DrawArrays(PrimitiveType, DrawStart, DrawCount);
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