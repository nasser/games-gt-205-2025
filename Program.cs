using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using pixel_lab;
using Window = pixel_lab.Window;

var w = new Window();

float[] positions =
[
    // Front face
    -0.5f, -0.5f, 0.5f, 0.5f, -0.5f, 0.5f, 0.25f, 0.5f, 0.25f,
    -0.5f, -0.5f, 0.5f, 0.25f, 0.5f, 0.25f, -0.25f, 0.5f, 0.25f,

    // Right face
    0.5f, -0.5f, 0.5f, 0.5f, -0.5f, -0.5f, 0.25f, 0.5f, -0.25f,
    0.5f, -0.5f, 0.5f, 0.25f, 0.5f, -0.25f, 0.25f, 0.5f, 0.25f,

    // Back face
    0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.25f, 0.5f, -0.25f,
    0.5f, -0.5f, -0.5f, -0.25f, 0.5f, -0.25f, 0.25f, 0.5f, -0.25f,

    // Left face
    -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, 0.5f, -0.25f, 0.5f, 0.25f,
    -0.5f, -0.5f, -0.5f, -0.25f, 0.5f, 0.25f, -0.25f, 0.5f, -0.25f,

    // Top face
    -0.25f, 0.5f, 0.25f, 0.25f, 0.5f, 0.25f, 0.25f, 0.5f, -0.25f,
    -0.25f, 0.5f, 0.25f, 0.25f, 0.5f, -0.25f, -0.25f, 0.5f, -0.25f,

    // Bottom face
    -0.5f, -0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f, 0.5f,
    -0.5f, -0.5f, -0.5f, 0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f
];

var p = new Pipeline();

p.ShaderFiles("vertex.glsl", "fragment.glsl");

p.Attribute("position", positions, size: 3);

p.PrimitiveType = PrimitiveType.LineLoop;
p.DrawCount = positions.Length / 3;

var cameraPosition = new Vector3(0, 0, 0);
var cameraRotation = new Vector3(0, 0, 0);

var speed = 0.01f;

w.Render += t =>
{
    p.Uniform("model", Matrix4.CreateRotationY(0) * Matrix4.CreateTranslation(0, 0, 0));
    // p.Uniform("projection", Matrix4.CreatePerspectiveFieldOfView(MathF.PI / 3, (float)w.ClientSize.X / w.ClientSize.Y, 0.1f, 100f));
    if(w.KeyboardState.IsKeyDown(Keys.W))
        cameraPosition += new Vector3(0, 0, -1) * speed;
    if(w.KeyboardState.IsKeyDown(Keys.S))
        cameraPosition += new Vector3(0, 0, 1) * speed;
    if(w.KeyboardState.IsKeyDown(Keys.A))
        cameraPosition += new Vector3(-1, 0, 0) * speed;
    if(w.KeyboardState.IsKeyDown(Keys.D))
        cameraPosition += new Vector3(1, 0, 0) * speed;
    cameraRotation = new Vector3((float)w.MousePosition.X / w.ClientSize.X, 0, 0);
    p.Uniform("view", (Matrix4.CreateRotationY(-cameraRotation.X) * Matrix4.CreateTranslation(cameraPosition)).Inverted());
    // p.Uniform("view", Matrix4.LookAt(new Vector3(MathF.Cos(t)* 15, 2, MathF.Sin(t) * 2), new Vector3(0, 0, 0), new Vector3(0, 1, 0)));
    // p.Uniform("projection", Matrix4.CreatePerspectiveFieldOfView(MathF.PI / 3, (float)w.ClientSize.X / w.ClientSize.Y, 0.1f, 100f));
    p.Uniform("projection", Matrix4.CreateOrthographic(2f, 2f, 0.1f, 20f));
    p.Draw();
};

w.Run();