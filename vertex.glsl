#version 330 core
in vec3 aPosition;
uniform float time;

void main()
{
    gl_Position = vec4(aPosition / 1.0 * sin(time), 1.0);
}