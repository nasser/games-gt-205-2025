#version 330 core
in vec2 quad;
in vec2 uv;
in vec2 position;
in vec3 color;
in float rotation;
in float scale;

uniform vec2 resolution;
uniform float time;

out vec3 color_out;
out vec2 uv_out;

void main()
{
    vec2 adjustment = resolution.y > resolution.x ? vec2(1.0, resolution.x / resolution.y) : vec2(resolution.y / resolution.x, 1.0);

    vec2 bounce = vec2(cos(time + rotation + position.x), sin(time + rotation));

    mat2 rotationMatrix = mat2(
        cos(rotation), -sin(rotation),
        sin(rotation),  cos(rotation)
    );

    vec2 transformedPos = (rotationMatrix * quad * adjustment) * scale + (position + bounce) * adjustment;

    gl_Position = vec4(transformedPos, 0.0, 1.0);
    color_out = color;
    uv_out = uv;
}