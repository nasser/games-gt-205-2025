using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using pixel_lab;

var w = new Window();

float[] vertices =
[
    -3, -1,
    1, -1,
    1, 3
];

var p = new Pipeline();

p.ShaderFiles("vertex.glsl", "fragment.glsl");

p.Attribute("position", vertices, size: 2);

p.PrimitiveType = PrimitiveType.Triangles;
p.DrawCount = vertices.Length / 2;
// p.Resolution = new Vector2i(16, 16);

w.Render += t => { p.Draw(); };

w.Run();