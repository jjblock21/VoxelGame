#version 450 core

@program vertex

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in uint aColor;

out vec2 vTexCoord;
out vec4 vColor;

void main() {
	gl_Position = vec4(aPos, 0, 1);
	vTexCoord = aTexCoord;

	// Unpack the color into a float vector.
	vColor = unpackUnorm4x8(aColor).rgba;
}

@program fragment

in vec2 vTexCoord;
in vec4 vColor;

out vec4 pixelColor;
uniform sampler2D texture0;

void main() {
	// Render texture like texture with overlayed solid color.
	vec4 tex = texture2D(texture0, vTexCoord);
	// Just the SrcAlpha, OneMinusSrcAlpha blend function.
	pixelColor = tex * (1 - vColor.a) + vColor * vColor.a;
}