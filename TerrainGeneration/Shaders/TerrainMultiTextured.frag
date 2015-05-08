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
	//normalize positions to [0, 1]
	float posFactor = (Position.y - MinTerrainHeight) / (MaxTerrainHeight - MinTerrainHeight);

	//flat surfaces are dirt-like, more rock-like as slope increases (slope factor)
	vec3 slopeColor = slope * rockSlope + (1 - slope) * dirtSlope;
	
	//grass near on bottom, snow near top (position factor)
	vec3 locColor = posFactor * snowSlope + (1 - posFactor) * grassSlope;

	//quadratic bias towards extremes (top and bottom have more weight)
	//that is, tops/bottoms rely more heavily on position weight than slope weight
	float extremeX = abs(.5 - posFactor) * 2;
	float extremeFactor = (extremeX + 1) * (extremeX - 1) + 1;
	color = extremeFactor * locColor + (1 - extremeFactor) * slopeColor;
	
	//low flat areas have grass
	if (slope < .3 && posFactor < .4){
		//normalize
		float slopeFactor = abs(slope - .3) / .3;
		color = slopeFactor * grassSlope + (1 - slopeFactor) * color;
	}

	//high flat areas have snow
	else if (slope < .3 && posFactor >= .8){
		//normalize
		float slopeFactor = abs(slope - .3) / .3;
		color = slopeFactor * snowSlope + (1 - slopeFactor) * color;
	}

	//middle ground
	else if (slope < .3){
		//normalize
		float slopeFactor = abs(slope - .3) / .3;
		float posFactor2 = (posFactor - .4)/ .4;

		color = slopeFactor * (posFactor2 * snowSlope + (1-posFactor2) * grassSlope) + (1 - slopeFactor) * color;
	}

	//factor in phong
	color = color * intensity;
	
	//vec3 textureVec = texture(snowSampler, uv).rgb;
}