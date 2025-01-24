using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using pixel_lab;

var w = new Window();

float[] vertices =
[
    -0.99f, -0.99f, 0.0f,
    0.99f, -0.99f, 0.0f,
    0.0f, 0.99f, 0.0f
];

var p = new Pipeline();

p.ShaderFiles("vertex.glsl", "fragment.glsl");

p.Attribute("aPosition", vertices);

p.PrimitiveType = PrimitiveType.Triangles;
p.DrawCount = 3;

w.OnRender(t =>
{
    // Console.WriteLine($"{w.Location} {w.ClientSize}");
    p.Uniform("time", t);
    p.Uniform("resolution", new Vector2(w.ClientSize.X, w.ClientSize.Y));
    p.Draw();
});

// TODO load gltf
// TODO load textures
// TODO render to texture
// TODO control resolution
