#version 330 core
out vec4 FragColor;
uniform vec2 resolution;
uniform float time;

void main()
{
    FragColor = vec4(gl_FragCoord.xy / resolution, 1.0 * sin(time * 5.0), 1.0f);
}