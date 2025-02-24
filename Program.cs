﻿using OpenTK.Graphics.OpenGL4;
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

var firstPass = new Pipeline();

var t = Pipeline.Texture("particles/smoke_07.png");
firstPass.Uniform("particleTexture", t);

firstPass.ShaderFiles("first-pass-vertex.glsl", "first-pass-fragment.glsl");

firstPass.Attribute("quad", quads, size: 2);
firstPass.Attribute("uv", uvs, size: 2);
firstPass.Attribute("seed", seeds.ToArray(), size: 1, divisor: 1);

firstPass.PrimitiveType = PrimitiveType.TriangleStrip;
firstPass.DrawCount = quads.Length / 2;
firstPass.InstanceCount = seeds.Count;
firstPass.BlendingFunction.SourceFactor = BlendingFactor.SrcAlpha;
firstPass.BlendingFunction.DestinationFactor = BlendingFactor.One;

firstPass.Uniform("base_scale", 0.75f); // 0 - 1
firstPass.Uniform("spread", 0.125f); // 0 - 1
firstPass.Uniform("wiggle", 0.05f); // 0 - 1
firstPass.Uniform("from_color", new Vector3(0.65f, 0.4f, 0.2f)); // 0 - 1
firstPass.Uniform("to_color", new Vector3(1.0f, 1.0f, 1.0f)); // 0 - 1
firstPass.Uniform("smoke_ramp", 10f); // > 0
firstPass.Uniform("mouse_effect_size", 1f); // > 0

var target = Pipeline.RenderTarget(
    width: w.ClientSize.X,
    height: w.ClientSize.Y,
    wrapS: TextureWrapMode.ClampToEdge,
    wrapT: TextureWrapMode.ClampToEdge);

var target2 = Pipeline.RenderTarget(
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
var secondPass = new Pipeline();
secondPass.ShaderFiles("second-pass-vertex.glsl", "second-pass-fragment.glsl");
secondPass.Attribute("position", fullScreenTriangle, size: 2);
secondPass.PrimitiveType = PrimitiveType.Triangles;
secondPass.DrawCount = fullScreenTriangle.Length / 2;

var thirdPass = new Pipeline();
thirdPass.ShaderFiles("second-pass-vertex.glsl", "third-pass-fragment.glsl");
thirdPass.Attribute("position", fullScreenTriangle, size: 2);
thirdPass.PrimitiveType = PrimitiveType.Triangles;
thirdPass.DrawCount = fullScreenTriangle.Length / 2;

w.Render += t =>
{
    firstPass.Uniform("resolution", w.ClientSize);
    firstPass.Uniform("mouse", w.MousePosition);
    firstPass.Uniform("time", t);
    target.Clear();
    firstPass.Draw(target.FrameBuffer);

    secondPass.Uniform("resolution", w.ClientSize);
    secondPass.Uniform("time", t);
    secondPass.Uniform("renderedScene", target.Texture);
    secondPass.Draw(target2.FrameBuffer);

    thirdPass.Uniform("resolution", w.ClientSize);
    thirdPass.Uniform("time", t);
    thirdPass.Uniform("renderedScene", target2.Texture);

    // combines "curvature", "lineWidth", "lineContrast", and "verticalLine"
    // https://github.com/pixijs/filters/blob/8276b6f9baf0685b46bae1c731cd8d0388067371/src/crt/CRTFilter.ts#L180-L208
    thirdPass.Uniform("uLine", new Vector4(
        1f, // curvature
        1f, // lineWidth
        0.25f, // lineContrast
        0f // verticalLine
    ));

    // combines "noise" and "noiseSize" 
    // https://github.com/pixijs/filters/blob/8276b6f9baf0685b46bae1c731cd8d0388067371/src/crt/CRTFilter.ts#L210-L222
    thirdPass.Uniform("uNoise", new Vector2(
        0.3f,
        0f
    ));

    // combines "vignette", "vignettingAlpha" and "vignettingBlur"
    // https://github.com/pixijs/filters/blob/8276b6f9baf0685b46bae1c731cd8d0388067371/src/crt/CRTFilter.ts#L224-L243
    thirdPass.Uniform("uVignette", new Vector3(
        0.3f,
        1f,
        0.3f
    ));

    // https://github.com/pixijs/filters/blob/8276b6f9baf0685b46bae1c731cd8d0388067371/src/crt/CRTFilter.ts#L103-L107
    thirdPass.Uniform("uSeed", 0f);
    thirdPass.Uniform("uDimensions", new Vector2(
        w.ClientSize.X,
        w.ClientSize.Y
    ));
    
    // ??
    thirdPass.Uniform("uInputSize", new Vector4(
        w.ClientSize.X,
        w.ClientSize.Y,
        0f, // ??
        0f // ??
    ));
    thirdPass.Draw();
};

w.Run();