#version 450 core

@program vertex

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in float aBrightness;

out vec2 vTexCoord;
out float vBrightness;

uniform ivec3 uOffset;
uniform mat4 mProjView;

void main() {
    vTexCoord = aTexCoord;
    vBrightness = aBrightness;
    gl_Position = mProjView * vec4(aPos + uOffset, 1f);
}

@program fragment

in vec2 vTexCoord;
in float vBrightness;

out vec4 pixelColor;

uniform sampler2D texture0;

void main() {
    pixelColor = texture2D(texture0, vTexCoord);
    pixelColor.xyz *= vBrightness;
}