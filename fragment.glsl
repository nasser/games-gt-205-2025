#version 330 core
in vec4 color_out;
in float edge_out;
out vec4 FragColor;

void main() {
    float t = pow(1.0 - 2.0 * abs(edge_out - 0.5), 0.5);
    float a = color_out.a * t;
    FragColor = vec4(color_out.r * a, color_out.g * a, color_out.b * a, 1.0);
}