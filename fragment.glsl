#version 330 core
in vec2 uv_out;
in vec3 color_out;
out vec4 FragColor;

// sampler2D is the texture uniform type
uniform sampler2D star;

void main() {
    // we lookup the color from the texture based on the blended uv coordinate we get from the vertex shader / rasterizer
    vec4 textureColor = texture(star, uv_out);
    // we tint it by multiplying by the tint color
    FragColor = vec4(textureColor.rgb * color_out, textureColor.a);
}