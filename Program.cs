using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using pixel_lab;

var w = new Window();

// quads lays out two triangles centered at (0, 0) as the basis geometry of each particle
// uvs associates each corner with a UV coordinate to perform texture lookup 
float[] quads =
[
    -0.25f, -0.25f,
    -0.25f, 0.25f,
    0.25f, -0.25f,
    0.25f, 0.25f,
];
float[] uvs =
[
    0f, 0f,
    0f, 1f,
    1f, 0f,
    1f, 1f,
];

// positions will move each particle to a place on the screen
// colors will tint the texture
List<float> positions = [];
List<float> colors = [];

// random initial positions and colors
var random = new Random();
for (int i = 0; i < 10000; i++)
{
    positions.Add((random.NextSingle() - 0.5f) * 2f);
    positions.Add((random.NextSingle() - 0.5f) * 2f);
    colors.Add(random.NextSingle());
    colors.Add(random.NextSingle());
    colors.Add(random.NextSingle());
}

var p = new Pipeline();

p.ShaderFiles("vertex.glsl", "fragment.glsl");

// we want to REUSE quads and uvs for each instance, so we DO NOT provide a divisor
// they will draw "normally" as we have seen so far, for each instance
p.Attribute("quad", quads, size: 2);
p.Attribute("uv", uvs, size: 2);

// we want to keep position and color attributes CONSTANT for each instance, so we DO provide a divisor of 1
// the values will be the same for each vertex (combination of quad and uv) in each instance
p.Attribute("position", positions.ToArray(), size: 2, divisor: 1);
p.Attribute("color", colors.ToArray(), size: 3, divisor: 1);

// load a texture and associate it with a uniform
var texture = Pipeline.Texture("particles/star_06.png");
p.Uniform("star", texture);

p.PrimitiveType = PrimitiveType.TriangleStrip;

// DrawCount should be the number of vertexes in the base geometry -- so we base it on quads  
p.DrawCount = quads.Length / 2;
// InstanceCount should be the number of COPIES of the base geometry with different attributes (taken from position and color) -- so we base it on positions
p.InstanceCount = positions.Count / 2;

w.Render += t =>
{
    // update positions on the CPU each frames
    for (int i = 0; i < positions.Count; i += 2)
    {
        var tt = (float)i / positions.Count;
        positions[i] = MathF.Sin(t + i * 0.3f) * tt;
        positions[i+1] = MathF.Cos(t + i* 0.3f) * tt;
    }
    p.UpdateAttribute("position", positions.ToArray());
    p.Draw();
};

w.Run();
