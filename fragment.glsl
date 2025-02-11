#version 330 core
uniform float time;
uniform vec2 resolution;
out vec4 FragColor;

void main() {
    float radius = 0.25;
    
    // gl_FragCoord is the built-in OpenGL variable that stores the coordinate of the fragment in screen space
    // calculate position in NDC coordinates
    vec2 position = gl_FragCoord.xy / resolution;
    
    // length is a built-in function that returns the length of a vector
    float distance = length(position - vec2(0.5, 0.5));

    // if the distance is greater than the radius, set the color to yellow, otherwise set it to red
    if(distance > radius) {
        FragColor = vec4(1.0, 1.0, 0.0, 1.0);
    } else {
        FragColor = vec4(1.0, 0.0, 0.0, 1.0);
    }
}