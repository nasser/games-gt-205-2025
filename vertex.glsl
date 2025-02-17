#version 330 core
in vec2 quad;
in vec2 uv;
in float seed;

uniform float time;

uniform float base_scale;
uniform float spread;
uniform float wiggle;
uniform vec3 from_color;
uniform vec3 to_color;
uniform float smoke_ramp;

out vec4 color_out;
out vec2 uv_out;

void main()
{
    float life = mod(time + seed * 80.0, 1.0);
    float y = seed * life - 1.0;
    float scale = 1.0 - (y * y) * base_scale;
    float x = sin(seed * 8.0 + time * life * wiggle) * spread;
    vec2 position = vec2(x, y);
    vec2 transformedPos = quad * scale + position;
    gl_Position = vec4(transformedPos, 0.0, 1.0);
    float color_t = pow(2.0, smoke_ramp * life - smoke_ramp);
    vec4 from_color_a = vec4(from_color * seed, 1.0);
    vec4 to_color_a = vec4(to_color, 0.0);
    color_out = mix(from_color_a, to_color_a, color_t);
    uv_out = uv;
}