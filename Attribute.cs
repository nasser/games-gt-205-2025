using OpenTK.Graphics.OpenGL4;

namespace pixel_lab;

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