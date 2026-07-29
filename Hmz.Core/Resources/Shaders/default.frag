#version 330 core
out vec4 FragColor;
uniform vec4 uColor;
uniform sampler2D uTexture;
uniform bool uTextured;
in vec2 TexCoord;

void main()
{
  FragColor = uTextured ? texture(uTexture, TexCoord) * uColor : uColor;
}
