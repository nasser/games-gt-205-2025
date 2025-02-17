#version 330 core
in vec2 quad;
in vec2 uv;
in vec2 position;
in vec3 color;
in float scale;

uniform vec2 resolution;
uniform float time;

out vec3 color_out;
out vec2 uv_out;

void main()
{
    vec2 transformedPos = quad * scale + position;
    gl_Position = vec4(transformedPos, 0.0, 1.0);
    color_out = color;
    uv_out = uv;
}