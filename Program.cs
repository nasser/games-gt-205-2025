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
List<float> seeds = [];

Random rand = new Random();

for (int i = 0; i < 1000; ++i) 
    seeds.Add(rand.NextSingle());

var p = new Pipeline();

var t = Pipeline.Texture("particles/smoke_07.png");
p.Uniform("particleTexture", t);

p.ShaderFiles("vertex.glsl", "fragment.glsl");

p.Attribute("quad", quads, size: 2);
p.Attribute("uv", uvs, size: 2);
p.Attribute("seed", seeds.ToArray(), size: 1, divisor: 1);

p.PrimitiveType = PrimitiveType.TriangleStrip;
p.DrawCount = quads.Length / 2;
p.InstanceCount = seeds.Count;
p.BlendingFunction.SourceFactor = BlendingFactor.SrcAlpha;
p.BlendingFunction.DestinationFactor = BlendingFactor.One;

p.Uniform("base_scale", 0.75f); // 0 - 1
p.Uniform("spread", 0.125f); // 0 - 1
p.Uniform("wiggle", 0.05f); // 0 - 1
p.Uniform("from_color", new Vector3(0.65f, 0.4f, 0.2f)); // 0 - 1
p.Uniform("to_color", new Vector3(1.0f, 1.0f, 1.0f)); // 0 - 1
p.Uniform("smoke_ramp", 10f); // > 0
p.Uniform("mouse_effect_size", 1f); // > 0

w.Render += t =>
{
    p.Uniform("resolution", w.ClientSize);
    p.Uniform("mouse", w.MousePosition);
    p.Uniform("time", t);
    p.Draw();
};

w.Run();