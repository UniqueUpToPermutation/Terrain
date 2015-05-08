#version 330 core

in vec3 Position;
in vec3 Normal;
out vec3 color;

uniform float MinTerrainHeight;
uniform float MaxTerrainHeight;

uniform vec3 LightDirection = normalize(vec3(-1.0, -1.0, -1.0));
uniform vec2 UVScale = vec2(1.0, 1.0);
uniform sampler2D grassSampler;
uniform sampler2D snowSampler;
uniform sampler2D dirtSampler;
uniform sampler2D rockSampler;

void main()
{	
	vec2 uv = vec2(Position.x * UVScale.x, Position.z * UVScale.y);
	float intensity = clamp(-dot(LightDirection, normalize(Normal)), 0.0, 1.0);
	
	vec3 grassSlope = texture(grassSampler, uv).rgb;
	vec3 snowSlope = texture(snowSampler, uv).rgb;
	vec3 dirtSlope = texture(dirtSampler, uv).rgb;
	vec3 rockSlope = texture(rockSampler, uv).rgb;

	vec3 newNormal = normalize(Normal);
	float slope = 1.0 - newNormal.y;

	//flat surfaces are dirt-like, more rock-like as slope increases
	vec3 slopeColor = slope * rockSlope + (1 - slope) * dirtSlope;
	
	//normalize positions to [0, 1]
	float posFactor = (Position.y - MinTerrainHeight) / (MaxTerrainHeight - MinTerrainHeight);
		//snow peak
	
	//grass on bottom, snow on top
	vec3 locColor = posFactor * snowSlope + (1 - posFactor) * grassSlope;

	//extreme at tops and bottoms
	float extremeX = abs(.5 - posFactor) * 2;
	float extremeFactor = (extremeX + 1) * (extremeX - 1) + 1;
	color = extremeFactor * locColor + (1 - extremeFactor) * slopeColor;
	
	//factor in phong
	color = color * intensity;
	
	//vec3 textureVec = texture(snowSampler, uv).rgb;
}