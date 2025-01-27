#version 330 core
uniform float time;
out vec4 FragColor;

void main() {
    float radius = 100.0;
    
    // gl_FragCoord is the built-in OpenGL variable that stores the coordinate of the fragment in screen space 
    vec2 position = gl_FragCoord.xy;

    // length is a built-in function that returns the length of a vector
    float distance = length(position);

    // if the distance is greater than the radius, set the color to yellow, otherwise set it to red
    if(distance > radius) {
        FragColor = vec4(1.0, 1.0, 0.0, 1.0);
    } else {
        FragColor = vec4(1.0, 0.0, 0.0, 1.0);
    }
}