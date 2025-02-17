#version 330 core
uniform sampler2D particleTexture;
in vec3 color_out;
in vec2 uv_out;
out vec4 FragColor;

void main() {
    vec4 textureColor = texture(particleTexture, uv_out);
    FragColor = vec4(color_out * textureColor.rgb, textureColor.a * 0.05);
}