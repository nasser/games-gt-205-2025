#version 330 core
in vec2 position;
in float colorFactor;
out vec3 color_out;

uniform vec3 fromColor;
uniform vec3 toColor;

void main() {
    gl_Position = vec4(position, 0.0, 1.0);
    color_out = fromColor * colorFactor + toColor * (1.0 - colorFactor); 
}
