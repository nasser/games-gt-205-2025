#version 330 core
in vec3 color_out;
out vec4 FragColor;

void main() {
    FragColor = vec4(color_out, 1.0);
}