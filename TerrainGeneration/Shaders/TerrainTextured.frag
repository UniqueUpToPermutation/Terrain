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
{
	//comment
	vec2 uv = vec2(Position.x * UVScale.x, Position.z * UVScale.y);
	float intensity = clamp(-dot(LightDirection, normalize(Normal)), 0.0, 1.0);

	vec3 textureVec = texture(snowSampler, uv).rgb;
	if (intensity < .5){
		textureVec = texture(grassSampler, uv).rgb + 5 * texture(grassSampler, uv).rgb;
	}

	color = textureVec * intensity;
}