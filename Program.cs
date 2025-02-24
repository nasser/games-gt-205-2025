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

var scenePass = new Pipeline();

var t = Pipeline.Texture("particles/smoke_07.png");
scenePass.Uniform("particleTexture", t);

scenePass.ShaderFiles("scene.vert.glsl", "scene.frag.glsl");

scenePass.Attribute("quad", quads, size: 2);
scenePass.Attribute("uv", uvs, size: 2);
scenePass.Attribute("seed", seeds.ToArray(), size: 1, divisor: 1);

scenePass.PrimitiveType = PrimitiveType.TriangleStrip;
scenePass.DrawCount = quads.Length / 2;
scenePass.InstanceCount = seeds.Count;
scenePass.BlendingFunction.SourceFactor = BlendingFactor.SrcAlpha;
scenePass.BlendingFunction.DestinationFactor = BlendingFactor.One;

scenePass.Uniform("base_scale", 0.75f); // 0 - 1
scenePass.Uniform("spread", 0.125f); // 0 - 1
scenePass.Uniform("wiggle", 0.05f); // 0 - 1
scenePass.Uniform("from_color", new Vector3(0.65f, 0.4f, 0.2f)); // 0 - 1
scenePass.Uniform("to_color", new Vector3(1.0f, 1.0f, 1.0f)); // 0 - 1
scenePass.Uniform("smoke_ramp", 10f); // > 0
scenePass.Uniform("mouse_effect_size", 1f); // > 0

var sceneRenderTarget = Pipeline.RenderTarget(
    width: w.ClientSize.X,
    height: w.ClientSize.Y,
    wrapS: TextureWrapMode.ClampToEdge,
    wrapT: TextureWrapMode.ClampToEdge);

var bloomTarget = Pipeline.RenderTarget(
    width: w.ClientSize.X,
    height: w.ClientSize.Y,
    wrapS: TextureWrapMode.ClampToEdge,
    wrapT: TextureWrapMode.ClampToEdge);

var aberrationTarget = Pipeline.RenderTarget(
    width: w.ClientSize.X,
    height: w.ClientSize.Y,
    wrapS: TextureWrapMode.ClampToEdge,
    wrapT: TextureWrapMode.ClampToEdge);

float[] fullScreenTriangle =
[
    -1f, 3f,
    -1f, -1f,
    3f, -1f
];
var bloomPass = new Pipeline();
bloomPass.ShaderFiles("post-process.vert.glsl", "bloom.frag.glsl");
bloomPass.Attribute("position", fullScreenTriangle, size: 2);
bloomPass.PrimitiveType = PrimitiveType.Triangles;
bloomPass.DrawCount = fullScreenTriangle.Length / 2;

var aberrationPass = new Pipeline();
aberrationPass.ShaderFiles("post-process.vert.glsl", "aberration.frag.glsl");
aberrationPass.Attribute("position", fullScreenTriangle, size: 2);
aberrationPass.PrimitiveType = PrimitiveType.Triangles;
aberrationPass.DrawCount = fullScreenTriangle.Length / 2;
aberrationPass.Uniform("uRed", new Vector2(0.2f, 0.2f));
aberrationPass.Uniform("uGreen", new Vector2(0.2f, 0.2f));
aberrationPass.Uniform("uBlue", new Vector2(0.2f, 0.2f));

var crtPass = new Pipeline();
crtPass.ShaderFiles("post-process.vert.glsl", "crt.frag.glsl");
crtPass.Attribute("position", fullScreenTriangle, size: 2);
crtPass.PrimitiveType = PrimitiveType.Triangles;
crtPass.DrawCount = fullScreenTriangle.Length / 2;

// combines "curvature", "lineWidth", "lineContrast", and "verticalLine"
// https://github.com/pixijs/filters/blob/8276b6f9baf0685b46bae1c731cd8d0388067371/src/crt/CRTFilter.ts#L180-L208
crtPass.Uniform("uLine", new Vector4(1f, 1f, 0.25f, 0f));

// combines "noise" and "noiseSize" 
// https://github.com/pixijs/filters/blob/8276b6f9baf0685b46bae1c731cd8d0388067371/src/crt/CRTFilter.ts#L210-L222
crtPass.Uniform("uNoise", new Vector2(0.3f, 0f));

// combines "vignette", "vignettingAlpha" and "vignettingBlur"
// https://github.com/pixijs/filters/blob/8276b6f9baf0685b46bae1c731cd8d0388067371/src/crt/CRTFilter.ts#L224-L243
crtPass.Uniform("uVignette", new Vector3(0.3f, 1f, 0.3f));

// https://github.com/pixijs/filters/blob/8276b6f9baf0685b46bae1c731cd8d0388067371/src/crt/CRTFilter.ts#L103-L107
crtPass.Uniform("uSeed", 0f);
crtPass.Uniform("uDimensions", new Vector2(w.ClientSize.X, w.ClientSize.Y));
    
// ??
crtPass.Uniform("uInputSize", new Vector4(w.ClientSize.X, w.ClientSize.Y, 0f, 0f));

w.Render += t =>
{
    scenePass.Uniform("resolution", w.ClientSize);
    scenePass.Uniform("mouse", w.MousePosition);
    scenePass.Uniform("time", t);
    sceneRenderTarget.Clear();
    scenePass.Draw(sceneRenderTarget.FrameBuffer);

    bloomPass.Uniform("resolution", w.ClientSize);
    bloomPass.Uniform("time", t);
    bloomPass.Uniform("renderedScene", sceneRenderTarget.Texture);
    bloomPass.Draw(bloomTarget.FrameBuffer);

    aberrationPass.Uniform("resolution", w.ClientSize);
    aberrationPass.Uniform("time", t);
    aberrationPass.Uniform("renderedScene", bloomTarget.Texture);
    aberrationPass.Draw(aberrationTarget.FrameBuffer);

    crtPass.Uniform("resolution", w.ClientSize);
    crtPass.Uniform("time", t);
    crtPass.Uniform("renderedScene", aberrationTarget.Texture);
    
    crtPass.Draw();
};

w.Run();