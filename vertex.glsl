#version 330 core
in vec3 color;
in vec2 position;
out vec3 vertColor;

void main() {
    gl_Position = vec4(position, 0.0, 1.0);
    vertColor = color;
}
