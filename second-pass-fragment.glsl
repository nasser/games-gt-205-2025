#version 330 core
out vec4 FragColor;
uniform float time;
uniform vec2 resolution;
uniform sampler2D renderedScene;

// https://en.wikipedia.org/wiki/Box_blur
vec3 blur(sampler2D tex, vec2 uv, vec2 offset) {
    vec3 sum = vec3(0.0);
    
    for (int x = -1; x <= 1; x++) {
        for (int y = -1; y <= 1; y++) {
            vec2 sampleUV = uv + vec2(x, y) * offset / resolution;
            sum += texture(tex, sampleUV).rgb;
        }
    }

    return sum / 9.0;
}

void main() {
    vec2 uv = gl_FragCoord.xy / resolution;
    vec3 originalFragment = texture(renderedScene, uv).rgb;
    
    // https://stackoverflow.com/questions/596216/formula-to-determine-perceived-brightness-of-rgb-color
    float brightness = 0.2126 * originalFragment.r + 0.7152 * originalFragment.g + 0.0722 * originalFragment.b;
    float f = smoothstep(0.5, 0.0, brightness);

    vec3 bloom = blur(renderedScene, uv, vec2(50.0, 50.0)) * f;

    FragColor = vec4(originalFragment + bloom * 2.7, 1.0);
}