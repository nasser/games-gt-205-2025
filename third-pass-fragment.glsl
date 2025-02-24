#version 330 core
out vec4 FragColor;
uniform float time;
uniform vec2 resolution;
uniform sampler2D renderedScene;

void main() {
    vec2 uv = gl_FragCoord.xy / resolution;
    FragColor = texture(renderedScene, uv);
}