#version 330 core

layout(location = 0) in vec3 vertexPosition;

uniform mat4 WorldTransform;
uniform mat4 ViewProjectionTransform;

void main()
{
	gl_Position = ViewProjectionTransform * (WorldTransform * vec4(vertexPosition, 1.0));
}