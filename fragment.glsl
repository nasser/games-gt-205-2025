#version 330 core
in vec4 color_out;
out vec4 FragColor;

void main() {
    FragColor = vec4(color_out.r * color_out.a, color_out.g * color_out.a, color_out.b * color_out.a, 1.0);
}