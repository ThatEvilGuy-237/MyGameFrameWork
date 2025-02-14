#version 330 core
in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D texture1;
uniform vec4 u_Color;

void main()
{
    vec4 texColor = texture(texture1, TexCoord);
    
    // If texture is fully transparent (not loaded), use u_Color
    if (texColor.a == 0.0)
    {
        FragColor = u_Color;
    }
    else
    {
        FragColor = texColor * u_Color;
    }
}
