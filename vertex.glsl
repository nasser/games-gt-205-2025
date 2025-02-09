#version 330 core
in vec2 position;
in float colorFactor;
out vec4 color_out;

uniform vec3 fromColor;
uniform vec3 toColor;
uniform float time;

void main() {
    gl_Position = vec4(position, 0.0, 1.0);
    float t = pow(abs(mod(time + colorFactor, 1.0) - 0.5), 0.5);
    color_out = vec4(fromColor * t + toColor * (1.0 - t), colorFactor); 
}
