#version 330 core
in vec3 position;
in vec3 normal;

out vec3 normal_out;
out vec3 position_out;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

void main()
{
    position_out = vec3(model * vec4(position, 1.0));
    mat3 normalMatrix = transpose(inverse(mat3(model)));
    normal_out = normalize(normalMatrix * normal);
    gl_Position = projection * view * vec4(position_out, 1.0);
}
