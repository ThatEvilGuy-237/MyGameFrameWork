#version 330 core

in vec2 texCoords; // Texture coordinates passed from the vertex shader
out vec4 FragColor;

uniform sampler2D texture1; // The texture to sample
uniform vec4 u_Color; // Color to use if texture is transparent

void main()
{
    // Sample the texture at texCoords
    vec4 texColor = texture(texture1, texCoords);
    
    // If texture is fully transparent (alpha == 0), use u_Color
    if (texColor.a == 0.0)
    {
        FragColor = u_Color;
    }
    else
    {
        FragColor = texColor;
    }
}
