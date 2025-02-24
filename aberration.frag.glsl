#version 330 core
// adapted from https://gist.github.com/ryonakae/7f45edb449f016214354e8acf374db2e

// added our own uniforms and out
out vec4 FragColor;
uniform float time;
uniform vec2 resolution;
uniform sampler2D renderedScene;

// varying vec2 vTextureCoord;
// uniform sampler2D uSampler;

// uniform vec2 uResolution; // app.screen
// uniform vec2 uMouse; // -1.0 ~ 1.0
uniform vec2 uRed;
uniform vec2 uBlue;

void main(void) {
  vec2 uv = gl_FragCoord.xy / resolution;
  vec2 pixelCoord = uv * resolution;
  vec2 p = (pixelCoord * 2.0 - resolution) / min(resolution.x, resolution.y);

  FragColor.r = texture2D(renderedScene, uv - uRed * p).r;
  FragColor.g = texture2D(renderedScene, uv).g;
  FragColor.b = texture2D(renderedScene, uv - uBlue * p).b;
  FragColor.a = texture2D(renderedScene, uv).a;
}