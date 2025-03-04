#version 330 core
in vec3 normal_out;
in vec3 position_out;

out vec4 FragColor;

uniform vec3 lightPos;
uniform vec3 lightColor;

void main()
{
    vec3 normal = normalize(normal_out);
    
    vec3 lightDir = normalize(lightPos - position_out);
    
    float diff = max(dot(normal, lightDir), 0.0);
    
    vec3 diffuse = diff * lightColor;
    
    FragColor = vec4(diffuse, 1.0);
}
