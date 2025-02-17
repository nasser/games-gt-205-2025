#version 330 core
in vec2 quad;
in vec2 uv;
in float seed;

uniform vec2 resolution;
uniform float time;

out vec3 color_out;
out vec2 uv_out;

void main()
{
    float scale = seed + 0.5;
    vec2 position = vec2(sin(time + seed), cos(time + seed));
    vec2 transformedPos = quad * scale + position;
    gl_Position = vec4(transformedPos, 0.0, 1.0);
    color_out = vec3(seed * 0.75, seed * 0.5, seed * 0.125);
    uv_out = uv;
}