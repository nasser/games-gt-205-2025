using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using pixel_lab;
using Window = pixel_lab.Window;

var w = new Window();

// load model from file and extract attributes and index buffers
// index buffers are something we didn't get to in class. they're a list of indexes that control the order that the gpu
// reads from your other attributes (position and normal in this example). without an index buffer, the gpu reads from
// your attributes in order, e.g. reading positions[0] and normals[0] first, then positions[1] and normals[1] next, then 
// positions[2] and normals[2], and so on. with an index buffer you can specify a different order to read from your
// attributes. this makes it easy to reuse values without specifying them more than once. we use it here because most
// 3D models coming out of 3D modeling software like blender will be set up to use index buffers.
var model = SharpGLTF.Schema2.ModelRoot.Load(Watchers.FindFile("helios_vaporwave_bust.glb"));
var positions = model.LogicalMeshes[0].Primitives[0].GetVertexAccessor("POSITION").AsVector3Array().ToArray();
var normals = model.LogicalMeshes[0].Primitives[0].GetVertexAccessor("NORMAL").AsVector3Array().ToArray();
var indexes = model.LogicalMeshes[0].Primitives[0].GetIndices().ToArray();

var p = new Pipeline();

p.ShaderFiles("vertex.glsl", "fragment.glsl");

// set up attributes and indexes in pipeline
// position and normal are regular attributes like we've seen all semester. Indexes is the index buffer. it is treated
// special so has different syntax here.
p.Attribute("position", positions, size: 3);
p.Attribute("normal", normals, size: 3);
p.Indexes = indexes;

// draw count is the number of indexes -- not the number of positions/normals! remember, an index buffer lets us repeat
// data, so the number of positions/normals should be less than the number of indexes
p.PrimitiveType = PrimitiveType.Triangles;
p.DrawCount = indexes.Length;

// set the light position and color. you can change these in the update loop to animate them.
p.Uniform("lightPos", new Vector3(8, 8, 8));
p.Uniform("lightColor", new Vector3(1, 1, 1));

// set the "camera" matrix (outside the update loop because it does not change)
p.Uniform("view",
    Matrix4.LookAt(new Vector3(15, 2, 15), new Vector3(0, 0, 0), new Vector3(0, 1, 0)));

w.Render += t =>
{
    // rotate and scale model to be upright, then rotate in Y over time
    p.Uniform("model",
        Matrix4.CreateRotationX(-MathF.PI / 2)
        * Matrix4.CreateTranslation(0, -10, 0)
        * Matrix4.CreateScale(0.25f)
        * Matrix4.CreateRotationY(MathF.PI / 2 * t));

    // compute projection matrix on the fly in case the window resizes
    p.Uniform("projection",
        Matrix4.CreatePerspectiveFieldOfView(
            MathF.PI / 8f,
            (float)w.ClientSize.X / w.ClientSize.Y,
            0.1f,
            1000f));
    p.Draw();
};

w.Run();