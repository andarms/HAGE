#version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec4 aBoneIds;
layout(location = 3) in vec4 aBoneWeights;

#define MAX_BONES 100

uniform mat4 uProjection;
uniform mat4 uView;
uniform mat4 uModel;
uniform bool uSkinned;
uniform mat4 uBones[MAX_BONES];

out vec2 TexCoord;

void main()
{
  vec4 localPos = vec4(aPos, 1.0);

  if (uSkinned)
  {
    mat4 skinMatrix =
      aBoneWeights.x * uBones[int(aBoneIds.x)] +
      aBoneWeights.y * uBones[int(aBoneIds.y)] +
      aBoneWeights.z * uBones[int(aBoneIds.z)] +
      aBoneWeights.w * uBones[int(aBoneIds.w)];
    localPos = skinMatrix * localPos;
  }

  gl_Position = uProjection * uView * uModel * localPos;
  TexCoord = aTexCoord;
}
