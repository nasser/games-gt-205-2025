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
float[] uvs =
[
    0f, 0f,
    0f, 1f,
    1f, 0f,
    1f, 1f,
];
List<float> positions = [];
List<float> rotations = [];
List<float> scales = [];
List<float> colors = [];

Random rand = new Random();

for (int i = 0; i < 10000; ++i)
{
    positions.Add(rand.NextSingle() * 2.0f - 1.0f);
    positions.Add(rand.NextSingle() * 2.0f - 1.0f);
    rotations.Add((float)(rand.NextSingle() * Math.PI));
    scales.Add(0.25f);
    colors.Add(rand.NextSingle());
    colors.Add(rand.NextSingle() * 0.5f);
    colors.Add(rand.NextSingle() * 0.25f);
}

var p = new Pipeline();

var t = Pipeline.Texture("particles/smoke_07.png");
p.Uniform("particleTexture", t);

p.ShaderFiles("vertex.glsl", "fragment.glsl");

p.Attribute("quad", quads, size: 2);
p.Attribute("uv", uvs, size: 2);
p.Attribute("position", positions.ToArray(), size: 2, divisor: 1);
p.Attribute("rotation", rotations.ToArray(), size: 1, divisor: 1);
p.Attribute("scale", scales.ToArray(), size: 1, divisor: 1);
p.Attribute("color", colors.ToArray(), size: 3, divisor: 1);

p.PrimitiveType = PrimitiveType.TriangleStrip;
p.DrawCount = quads.Length / 2;
p.InstanceCount = positions.Count / 2;
p.BlendingFunction.SourceFactor = BlendingFactor.SrcAlpha;
p.BlendingFunction.DestinationFactor = BlendingFactor.One;

w.Render += t =>
{
    p.Uniform("resolution", w.ClientSize);
    p.Uniform("time", t);
    
    for (int i = 0; i < rotations.Count; i++)
    {
        rotations[i] += 0.0025f;
    }
    p.UpdateAttribute("rotation", rotations.ToArray());
    p.Draw();
};

w.Run();