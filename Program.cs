using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using pixel_lab;

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

w.Render += t => { p.Draw(); };

w.Run();