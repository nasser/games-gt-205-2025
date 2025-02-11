using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using pixel_lab;

var w = new Window();

List<float> positions = [];

var circleSize = 1f;

for (int i = 0; i <= 32; i++)
{
    positions.Add(0);
    positions.Add(0);
    positions.Add((float)Math.Cos(i * Math.PI * 2f / 32) * circleSize);
    positions.Add((float)Math.Sin(i * Math.PI * 2f / 32) * circleSize);
}

var p = new Pipeline();

p.ShaderFiles("vertex.glsl", "fragment.glsl");

p.Attribute("position", positions.ToArray(), size: 2);

p.PrimitiveType = PrimitiveType.TriangleStrip;
p.DrawCount = positions.Count / 2;
// p.Resolution = new Vector2i(16, 16);

w.Render += t => { p.Draw(); };

w.Run();