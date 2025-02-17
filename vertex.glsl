#version 330 core
in vec2 quad;
in vec2 uv;
in float seed;

uniform float time;

out vec4 color_out;
out vec2 uv_out;

void main()
{
    float life = mod(time + seed * 80.0, 1.0);
    float y = seed * life - 1.0;
    float scale = 1.0 - (y * y) * 0.75;
    float x = sin(seed * 8.0 + time * life * 0.05) * 0.25;
    vec2 position = vec2(x, y);
    vec2 transformedPos = quad * scale + position;
    gl_Position = vec4(transformedPos, 0.0, 1.0);
    float color_t = pow(2.0, 10.0 * life - 10.0);
    vec4 from_color = vec4(0.65 * seed, 0.4 * seed, 0.2 * seed, 1.0);
    vec4 to_color = vec4(1.0, 1.0, 1.0, 0.0);
    color_out = mix(from_color, to_color, color_t);
    uv_out = uv;
}