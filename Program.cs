using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using pixel_lab;

var w = new Window();

float[] quads =
[
    -0.5f, -0.5f,
    -0.5f, 0.5f,
    0.5f, -0.5f,
    0.5f, 0.5f,
];
List<float> positions = [
    0, 0
];
List<float> rotations = [];
List<float> scales = [];
List<float> colors = [];
// List<float> uvs = [];

var p = new Pipeline();

w.AspectRatio = 2.0f;

p.ShaderFiles("vertex.glsl", "fragment.glsl");

p.Attribute("quad", quads, size: 2);
p.Attribute("position", positions.ToArray(), size: 2, divisor: 1);

p.PrimitiveType = PrimitiveType.TriangleStrip;
p.DrawCount = quads.Length / 2;
p.InstanceCount = positions.Count / 2;

w.Render += t => { p.Draw(); };

w.Run();
