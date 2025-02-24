using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using pixel_lab;
using Window = pixel_lab.Window;

var w = new Window();

float[] positions =
[
    // Front face
    -0.5f, -0.5f, 0.5f, 0.5f, -0.5f, 0.5f, 0.25f, 0.5f, 0.25f,
    -0.5f, -0.5f, 0.5f, 0.25f, 0.5f, 0.25f, -0.25f, 0.5f, 0.25f,

    // Right face
    0.5f, -0.5f, 0.5f, 0.5f, -0.5f, -0.5f, 0.25f, 0.5f, -0.25f,
    0.5f, -0.5f, 0.5f, 0.25f, 0.5f, -0.25f, 0.25f, 0.5f, 0.25f,

    // Back face
    0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.25f, 0.5f, -0.25f,
    0.5f, -0.5f, -0.5f, -0.25f, 0.5f, -0.25f, 0.25f, 0.5f, -0.25f,

    // Left face
    -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, 0.5f, -0.25f, 0.5f, 0.25f,
    -0.5f, -0.5f, -0.5f, -0.25f, 0.5f, 0.25f, -0.25f, 0.5f, -0.25f,

    // Top face
    -0.25f, 0.5f, 0.25f, 0.25f, 0.5f, 0.25f, 0.25f, 0.5f, -0.25f,
    -0.25f, 0.5f, 0.25f, 0.25f, 0.5f, -0.25f, -0.25f, 0.5f, -0.25f,

    // Bottom face
    -0.5f, -0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f, 0.5f,
    -0.5f, -0.5f, -0.5f, 0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f
];

var p = new Pipeline();

p.ShaderFiles("vertex.glsl", "fragment.glsl");

p.Attribute("position", positions, size: 3);

p.PrimitiveType = PrimitiveType.LineLoop;
p.DrawCount = positions.Length / 3;

var pos = Vector3.Zero;
var rot = Vector3.Zero;

w.Render += t =>
{
    p.Uniform("model", Matrix4.CreateRotationY(t));
    var yaw = w.MousePosition.X / w.ClientSize.X * MathF.PI * 2f;
    var pitch = w.MousePosition.Y / w.ClientSize.Y * MathF.PI * 2f;
    // float radius = 5f;
    // p.Uniform("view", Matrix4.LookAt(new
    //         Vector3(
    //             MathF.Cos(pitch) * MathF.Cos(yaw) * radius,
    //             MathF.Sin(pitch) * radius,
    //             MathF.Cos(pitch) * MathF.Sin(yaw) * radius
    //         ),
    //     Vector3.Zero,
    //     Vector3.UnitY));
    pos += new Vector3(
        w.IsKeyDown(Keys.A) ? -0.05f : w.IsKeyDown(Keys.D) ? 0.05f : 0,
        0,
        w.IsKeyDown(Keys.W) ? -0.05f : w.IsKeyDown(Keys.S) ? 0.05f : 0
    );
    rot.Y = w.MousePosition.X / w.ClientSize.X * MathF.PI * 2f;
    rot.X = -w.MousePosition.Y / w.ClientSize.Y * MathF.PI * 2f;
    var camera = Matrix4.CreateRotationX(rot.X) * Matrix4.CreateRotationY(rot.Y) * Matrix4.CreateTranslation(pos);
    p.Uniform("view", camera.Inverted());
    p.Uniform("projection", Matrix4.CreatePerspectiveFieldOfView(MathF.PI / 2, 1, 0.1f, 100f));
    p.Draw();
};

w.Run();