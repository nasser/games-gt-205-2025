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
List<float> lifespan = [];
List<float> initialPositions = [];
List<float> positions = [];
List<float> scales = [];
List<float> initialColors = [];
List<float> colors = [];

Random rand = new Random();

for (int i = 0; i < 1000; ++i)
{
    lifespan.Add(rand.NextSingle() * 2f);
    var y = -1f + rand.NextSingle() * 0.1f;
    var y1 = y + 1;
    var x = y1 * y1 * 2.5f * (rand.NextSingle() * 2.0f - 1.0f);
    initialPositions.Add(x);
    initialPositions.Add(y);
    positions.Add(0);
    positions.Add(0);
    scales.Add(rand.NextSingle() * 0.25f);
    initialColors.Add(rand.NextSingle());
    initialColors.Add(rand.NextSingle() * 0.5f);
    initialColors.Add(rand.NextSingle() * 0.25f);
    colors.Add(0);
    colors.Add(0);
    colors.Add(0);
}

var p = new Pipeline();

var t = Pipeline.Texture("particles/smoke_07.png");
p.Uniform("particleTexture", t);

p.ShaderFiles("vertex.glsl", "fragment.glsl");

p.Attribute("quad", quads, size: 2);
p.Attribute("uv", uvs, size: 2);
p.Attribute("position", positions.ToArray(), size: 2, divisor: 1);
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
    // p.Uniform("time", t);
    for (int i = 0; i < positions.Count; i += 2)
    {
        var particleIndex = i / 2;
        var life = (t + lifespan[particleIndex]) % 1;
        var y = initialPositions[i + 1] + life;
        var y1 = y + 1;
        var x = initialPositions[i] + (y1 * y1) * 2.5f * (initialPositions[i] * 20.0f);
        positions[i] = x;
        positions[i + 1] = y;
        scales[particleIndex] = (2f - life) * 0.25f;
        var colorT = MathHelper.MapRange(y, -1f, 1, 0, 1);
        colors[particleIndex * 3] = MathHelper.Lerp(initialColors[particleIndex * 3], 1f, colorT);
        colors[particleIndex * 3 + 1] = MathHelper.Lerp(initialColors[particleIndex * 3 + 1], 1f, colorT);
        colors[particleIndex * 3 + 2] = MathHelper.Lerp(initialColors[particleIndex * 3 + 2], 1f, colorT);
    }

    p.UpdateAttribute("position", positions.ToArray());
    p.UpdateAttribute("scale", scales.ToArray());
    p.UpdateAttribute("color", colors.ToArray());
    p.Draw();
};

w.Run();