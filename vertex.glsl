#version 330 core
in vec2 position;
out vec4 color_out;
out float edge_out;
out float length_out;

uniform vec3 fromColor;
uniform vec3 toColor;
uniform float time;
uniform float count;

void main() {
    float colorFactor = float(gl_VertexID) / count;
    float edgeFactor = gl_VertexID % 2 == 0 ? 1.0 : 0.0;
    gl_Position = vec4(position, 0.0, 1.0);
    float t = pow(abs(mod(time + colorFactor, 1.0) - 0.5), 0.5);
    color_out = vec4(fromColor * t + toColor * (1.0 - t), colorFactor);
    edge_out = edgeFactor;
    length_out = colorFactor;
}
