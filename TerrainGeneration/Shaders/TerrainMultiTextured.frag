#version 330 core

in vec3 Position;
in vec3 Normal;

out vec3 color;

uniform vec3 LightDirection = normalize(vec3(-1.0, -1.0, -1.0));
uniform vec2 UVScale = vec2(1.0, 1.0);
uniform sampler2D grassSampler;
uniform sampler2D snowSampler;
uniform sampler2D dirtSampler;
uniform sampler2D rockSampler;

void main()
{	/*
	//comment
	vec3 snowSlope = vec3(1, 1, 1);
	vec3 rockSlope = vec3(.5, .5, .5);
	vec3 dirtSlope = vec3(.1, .2, .2);
	vec3 grassSlope = vec3(0.0, 1.0, 0);

	vec3 newNormal = normalize(Normal);
	float slope = 1.0 - newNormal.y;

	//snow peak
	if (Position.y > 90){
		float blendFactor = (Position.y - 90) / 10;
		color = (1-blendFactor) * rockSlope + blendFactor * snowSlope;
	}

	else if (slope  < .2){
		float blendFactor = slope / .2;
		color = (1-blendFactor) * grassSlope + blendFactor * dirtSlope;
	}

	else if((slope < 0.7) && (slope >= 0.2))
    {
        float blendFactor = (slope - 0.2) * (1.0 / (0.7 - 0.2));
        color = (1-blendFactor) * dirtSlope + blendFactor * rockSlope;
    }

	else{
		color = rockSlope;
	}*/
	vec2 uv = vec2(Position.x * UVScale.x, Position.z * UVScale.y);
	float intensity = clamp(-dot(LightDirection, normalize(Normal)), 0.0, 1.0);
	
	//vec3 textureVec = texture(grassSampler, uv).rgb;
	vec3 textureVec = texture(snowSampler, uv).rgb;
	color = textureVec * intensity;
}