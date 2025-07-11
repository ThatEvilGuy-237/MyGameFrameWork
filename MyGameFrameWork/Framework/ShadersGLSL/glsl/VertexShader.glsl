#version 330 core

layout(location = 0) in vec3 aPosition; // Vertex position
out vec2 texCoords; // Output texture coordinates

uniform vec2 ViewportSize;
uniform vec4 ObjectRectSize; // The portion of the texture to use (x, y, width, height)
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

    // Use SourceRect to adjust texCoords for texture mapping
    texCoords = vec2(
        (aPosition.x - ObjectRectSize.x) / ObjectRectSize.z,  // Normalize x based on SourceRect
         (aPosition.y - ObjectRectSize.y) / ObjectRectSize.w   // Normalize y based on SourceRect
    );
}
