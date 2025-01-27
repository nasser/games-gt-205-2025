using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using pixel_lab;

var w = new Window();

float[] vertices = [-1, -1, 1, -1, 0, 0];
float[] colors = [1, 0, 0, 0, 1, 0, 0, 0, 1];

var p = new Pipeline();

p.ShaderFiles("vertex.glsl", "fragment.glsl");

p.Attribute("position", vertices, size: 2);
p.Attribute("color", colors, size: 3);

p.PrimitiveType = PrimitiveType.Triangles;
p.DrawCount = 3;
p.Resolution = new Vector2i(16, 16);

w.Render += t => {
    p.Uniform("time", t);
    p.Draw();
};

w.Run();