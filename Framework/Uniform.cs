using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace pixel_lab;

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