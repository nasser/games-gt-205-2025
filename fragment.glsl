#version 330 core
in vec3 vertColor;
uniform float time;
out vec4 FragColor;

void main() {
    FragColor = vec4(vertColor * abs(sin(time)), 1.0);
}