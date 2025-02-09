#version 330 core
in vec2 position;
in vec3 color;
out vec3 color_out;

void main() {
    gl_Position = vec4(position, 0.0, 1.0);
    color_out = color; 
}
