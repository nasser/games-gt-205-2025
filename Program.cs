using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using pixel_lab;

var w = new Window();

List<float> positions = [];

var p = new Pipeline();

p.ShaderFiles("vertex.glsl", "fragment.glsl");

p.Attribute("position", positions.ToArray(), size: 2);

p.PrimitiveType = PrimitiveType.TriangleStrip;
p.DrawCount = positions.Count / 2;

// track the last position of the mouse so we can get the direction and offset between frames 
var lastMousePosition = Vector2.Zero;

// the width of the trail in NDC units
const float widthNdc = 0.05f;

// the minimum distance in pixels the mouse has to travel before we register a new vertex. this is what was missing in
// class! without this logic, the mouse holding still will register vertexes in the same spot creating geometry that is
// infinitely thin and will cause problems. 
const int minimumDistancePixels = 8;

var fromColor = new Vector3(1f, 0f, 0f);
var toColor = new Vector3(0f, 1f, 0f);

p.Uniform("fromColor", fromColor);
p.Uniform("toColor", toColor);

var maxTrailLength = 100;

w.Render += t =>
{
    p.Uniform("time", t);
    
    // get the offset and distance in pixels first, so we can see if we moved far enough to register a new vertex
    // var currentMousePosition = w.MousePosition;
    var currentMousePosition = Vector2.Lerp(w.MousePosition, lastMousePosition, 0.755f);
    var pixelOffset = currentMousePosition - lastMousePosition;
    var pixelDistance = pixelOffset.Length;

    if (pixelDistance > minimumDistancePixels)
    {
        // yes -- we've moved at least minimumDistancePixels since the last vertex, time to register a new one!

        // calculate the current and previous mouse coordinates in NDC (the function is at the bottom of the file, same
        // as it was in class)
        var currentMousePositionNdc = ScreenPositionToNdc(currentMousePosition, w);
        var lastMousePositionNdc = ScreenPositionToNdc(lastMousePosition, w);

        // get the offset, the vector or "arrow" that describes the movement of the mouse from the last position to the
        // current one. to calculate the direction we normalize the offset, so the direction is the arrow between the
        // last position and the current one but with its length set to 1.  
        var offsetNdc = lastMousePositionNdc - currentMousePositionNdc;
        var directionNdc = offsetNdc.Normalized();

        // in 2D a simple trick to get a 90-degree "rotation" of a vector is to negate one of its components. 3D
        // requires different approaches! keep in mind that there are always two 90-degree rotations of a vector, to the
        // "left" and to the "right"

        // direction to the 'a' point, a 90-degree rotation of the direction vector
        var directionToA = new Vector2(directionNdc.Y, -directionNdc.X);
        // direction to the 'b' point, a 90-degree rotation of the direction vector in the opposite direction
        var directionToB = new Vector2(-directionNdc.Y, directionNdc.X);
        
        // the actual 'a' point we care about, calculated by starting at currentMousePositionNdc and "moving" along directionToA a distance of widthNdc  
        var a = currentMousePositionNdc + directionToA * widthNdc;
        // the actual 'b' point we care about, calculated by starting at currentMousePositionNdc and "moving" along directionToB a distance of widthNdc
        var b = currentMousePositionNdc + directionToB * widthNdc;

        // add a and b to the positions list
        positions.Add(a.X);
        positions.Add(a.Y);
        positions.Add(b.X);
        positions.Add(b.Y);

        if (positions.Count / 2 >= maxTrailLength)
        {
            positions.RemoveRange(0, 4);
        }

        // update the existing attribute with new data
        p.UpdateAttribute("position", positions.ToArray());

        // update the draw count to tell the GPU to use the new values we just uploaded. we only need to do this if the
        // size of the data changes.
        p.DrawCount = positions.Count / 2;
        p.Uniform("count", (float)p.DrawCount);
        
        // only update 'lastMousePosition' if we register a new vertex 
        lastMousePosition = currentMousePosition;
    }

    // always draw, even if the mouse hasn't moved far enough to register a new vertex
    p.Draw();
};

w.Run();

Vector2 ScreenPositionToNdc(Vector2 screenPosition, Window window) =>
    new(
        screenPosition.X / window.ClientSize.X * 2f - 1f,
        -1 * (screenPosition.Y / window.ClientSize.Y * 2f - 1f));