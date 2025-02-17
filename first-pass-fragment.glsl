#version 330 core
uniform sampler2D particleTexture;
in vec4 color_out;
in vec2 uv_out;
out vec4 FragColor;

void main() {
    vec4 textureColor = texture(particleTexture, uv_out);
    FragColor = vec4(color_out.rgb * textureColor.rgb, color_out.a * textureColor.a * 0.025);
}