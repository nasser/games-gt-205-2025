#version 330 core
uniform float time;
uniform vec2 resolution;
out vec4 FragColor;

void main() {
    float radius = 0.5;
    
    // gl_FragCoord is the built-in OpenGL variable that stores the coordinate of the fragment in screen space 
    vec2 position = gl_FragCoord.xy / resolution;

    // length is a built-in function that returns the length of a vector

    // in the corner
    //float distance = length(position);
    // centered 
    // float distance = length(position - vec2(0.5, 0.5));
    // wavey
    //float distance = length(position - vec2(0.5 + sin(time + position.y * 8.0) * 0.125, 0.5));
    // ???
    float distance = length(position - vec2(0.5 + sin(time + position.x * 8.0) * 0.125 * position.y * 7.0, 0.5));

    // if the distance is greater than the radius, set the color to yellow, otherwise set it to red
    if(distance > radius) {
        FragColor = vec4(1.0, 1.0, 0.0, 1.0);
    } else {
        FragColor = vec4(1.0, 0.0, 0.0, 1.0);
    }
}