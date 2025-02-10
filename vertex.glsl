#version 330 core
// these attributes are loaded once per vertex and REPEAT for each instance
in vec2 quad;
in vec2 uv;

// these attributes are loaded once PER INSTANCE -- note that theres no difference
// to the way we access them in the shader! 
in vec3 color;
in vec2 position;

out vec2 uv_out;
out vec3 color_out;

void main() {
    // remember -- quad represents each corner of the quad and each corner will be moved by *the same position* value
    // because position will be HELD CONSTANT for the entire instance draw 
    gl_Position = vec4(quad + position, 0.0, 1.0);
    uv_out = uv;
    color_out = color; 
}
