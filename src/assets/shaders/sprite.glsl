#version 450 core

@program vertex

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec4 aOverlayColor;

out vec2 vTexCoord;
out vec4 vOverlayColor;

void main() {
	gl_Position = vec4(aPos, 0, 1);
	vTexCoord = aTexCoord;
	vOverlayColor = aOverlayColor;
}

@program fragment

in vec2 vTexCoord;
in vec4 vOverlayColor;

out vec4 pixelColor;
uniform sampler2D texture0;

void main() {
	// Render texture like texture with overlayed solid color.
	vec4 tex = texture2D(texture0, vTexCoord);
	// Just the SrcAlpha, OneMinusSrcAlpha blend function.
	pixelColor = tex * (1 - vOverlayColor.a) + vOverlayColor * vOverlayColor.a;
}