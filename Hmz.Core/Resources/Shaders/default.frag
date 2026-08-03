#version 330 core
out vec4 FragColor;
uniform vec4 uColor;
uniform sampler2D uTexture;
uniform bool uTextured;
uniform bool uUseVertexColor;
in vec2 TexCoord;
in vec4 VertexColor;

void main()
{
  vec4 baseColor = uUseVertexColor ? VertexColor : uColor;
  FragColor = uTextured ? texture(uTexture, TexCoord) * baseColor : baseColor;
}
