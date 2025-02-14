#version 330 core

layout(location = 0) in vec3 aPosition; // Vertex position
uniform vec2 ViewportSize;
uniform mat4 u_Transformation; // Transformation matrix passed from the CPU

void main()
{
    // Apply the transformation matrix to the vertex position
    vec4 transformedPosition = u_Transformation * vec4(aPosition, 1.0);

    // Convert position to normalized coordinates based on viewport size
    float nx = transformedPosition.x / ViewportSize.x * 2.0 - 1.0;
    float ny = transformedPosition.y / ViewportSize.y * 2.0 - 1.0;

    // Set the final position for rendering
    gl_Position = vec4(nx, -ny, transformedPosition.z, 1.0);
}
