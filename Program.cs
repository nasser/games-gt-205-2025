using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using pixel_lab;
using Window = pixel_lab.Window;

var w = new Window();

var model = SharpGLTF.Schema2.ModelRoot.Load(Watchers.FindFile("helios_vaporwave_bust.glb"));
var positions = model.LogicalMeshes[0].Primitives[0].GetVertexAccessor("POSITION").AsVector3Array().ToArray();
var normals = model.LogicalMeshes[0].Primitives[0].GetVertexAccessor("NORMAL").AsVector3Array().ToArray();
var indices = model.LogicalMeshes[0].Primitives[0].GetIndices().ToArray();

var p = new Pipeline();

p.ShaderFiles("vertex.glsl", "fragment.glsl");
p.Attribute("position", positions, size: 3);
p.Attribute("normal", normals, size: 3);
p.Indexes = indices;

p.PrimitiveType = PrimitiveType.Triangles;
p.DrawCount = indices.Length;

p.Uniform("lightPos", new Vector3(8, 8, 8));
p.Uniform("lightColor", new Vector3(1, 1, 1));

p.Uniform("view",
    Matrix4.LookAt(new Vector3(15, 2, 15), new Vector3(0, 0, 0), new Vector3(0, 1, 0)));

w.Render += t =>
{
    p.Uniform("model",
        Matrix4.CreateRotationX(-MathF.PI / 2)
        * Matrix4.CreateTranslation(0, -10, 0)
        * Matrix4.CreateScale(0.25f)
        * Matrix4.CreateRotationY(MathF.PI / 2 * t));

    p.Uniform("projection",
        Matrix4.CreatePerspectiveFieldOfView(
            MathF.PI / 8f,
            (float)w.ClientSize.X / w.ClientSize.Y,
            0.1f,
            1000f));
    p.Draw();
};

w.Run();