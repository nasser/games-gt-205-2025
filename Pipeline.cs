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
            if(_vertexShaderSource == null || _fragmentShaderSource == null)
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

    // public void Uniform(string name, int value) =>
    //     _uniforms[name] = new UniformInt { Name = name, Value = value };
    // public void Uniform(string name, double value) =>
    //     _uniforms[name] = new UniformDouble { Name = name, Value = value };

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
}

abstract class Uniform
{
    public required string Name { get; init; }
    public abstract void Upload(int program);

    protected bool TryGetUniformLocation(int program, out int location)
    {
        location = GL.GetUniformLocation(program, Name);
        if (location == -1)
            Warnings.WriteOnce($"Uniform '{Name}' not found in shader.");
        return location != -1;
    }
}

class UniformFloat : Uniform
{
    public float Value { get; init; }

    public override void Upload(int program)
    {
        if (TryGetUniformLocation(program, out var location))
            GL.Uniform1(location, Value);
    }
}

// class UniformDouble : Uniform
// {
//     public double Value { get; init; }
//
//     public override void Upload(int program)
//     {
//         if (TryGetUniformLocation(program, out var location))
//             GL.Uniform1(location, Value);
//     }
// }
//
// class UniformInt : Uniform
// {
//     public int Value { get; init; }
//
//     public override void Upload(int program)
//     {
//         if (TryGetUniformLocation(program, out var location))
//             GL.Uniform1(location, Value);
//     }
// }

class UniformVector2 : Uniform
{
    public Vector2 Value { get; init; }

    public override void Upload(int program)
    {
        if (TryGetUniformLocation(program, out var location))
            GL.Uniform2(location, Value.X, Value.Y);
    }
}

class UniformVector3 : Uniform
{
    public Vector3 Value { get; init; }

    public override void Upload(int program)
    {
        if (TryGetUniformLocation(program, out var location))
            GL.Uniform3(location, Value.X, Value.Y, Value.Z);
    }
}

class UniformVector4 : Uniform
{
    public Vector4 Value { get; init; }

    public override void Upload(int program)
    {
        if (TryGetUniformLocation(program, out var location))
            GL.Uniform4(location, Value.X, Value.Y, Value.Z, Value.W);
    }
}

class UniformMatrix2 : Uniform
{
    public Matrix2 Value { get; init; }

    public override void Upload(int program)
    {
        if (TryGetUniformLocation(program, out var location))
        {
            var matrix2 = Value;
            GL.UniformMatrix2(location, false, ref matrix2);
        }
    }
}

class UniformMatrix3 : Uniform
{
    public Matrix3 Value { get; init; }

    public override void Upload(int program)
    {
        if (TryGetUniformLocation(program, out var location))
        {
            var matrix3 = Value;
            GL.UniformMatrix3(location, false, ref matrix3);
        }
    }
}

public class Attribute
{
    public required string Name { get; init; }
    public int Vbo { get; init; }
    public int? Size { get; init; }
    public VertexAttribPointerType? Type { get; init; }
    public bool Normalized { get; init; }
    public int Stride { get; init; }
    public int Offset { get; init; }

    public void Bind(int program, int vao)
    {
        var location = GL.GetAttribLocation(program, Name);
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
        GetAttributeInfo(program, location, out var reflectedSize, out var reflectedType);
        GL.VertexAttribPointer(location, Size ?? reflectedSize, Type ?? reflectedType, Normalized, Stride, Offset);
        GL.EnableVertexAttribArray(location);
    }

    public void Update(float[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, usage);
    }

    static void GetAttributeInfo(int program, int location, out int size, out VertexAttribPointerType type)
    {
        GL.GetActiveAttrib(program, location, out _, out var attributeType);
        switch (attributeType)
        {
            case ActiveAttribType.Float:
                size = 1;
                type = VertexAttribPointerType.Float;
                break;
            case ActiveAttribType.FloatVec2:
                size = 2;
                type = VertexAttribPointerType.Float;
                break;
            case ActiveAttribType.FloatVec3:
                size = 3;
                type = VertexAttribPointerType.Float;
                break;
            case ActiveAttribType.FloatVec4:
                size = 4;
                type = VertexAttribPointerType.Float;
                break;
            case ActiveAttribType.Int:
                size = 1;
                type = VertexAttribPointerType.Int;
                break;
            case ActiveAttribType.IntVec2:
                size = 2;
                type = VertexAttribPointerType.Int;
                break;
            case ActiveAttribType.IntVec3:
                size = 3;
                type = VertexAttribPointerType.Int;
                break;
            case ActiveAttribType.IntVec4:
                size = 4;
                type = VertexAttribPointerType.Int;
                break;
            case ActiveAttribType.Double:
                size = 1;
                type = VertexAttribPointerType.Double;
                break;
            case ActiveAttribType.DoubleVec2:
                size = 2;
                type = VertexAttribPointerType.Double;
                break;
            case ActiveAttribType.DoubleVec3:
                size = 3;
                type = VertexAttribPointerType.Double;
                break;
            case ActiveAttribType.DoubleVec4:
                size = 4;
                type = VertexAttribPointerType.Double;
                break;
            default:
                throw new InvalidOperationException($"Attribute type '{attributeType}' not supported.");
        }
    }
}

public static class Warnings
{
    private static readonly HashSet<string> Messages = [];

    public static void WriteOnce(string message)
    {
        if (Messages.Add(message))
            Console.WriteLine(message);
    }

    public static void ClearCache() => 
        Messages.Clear();
}