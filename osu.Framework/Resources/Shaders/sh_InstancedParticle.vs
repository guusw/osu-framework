#version 130
#extension GL_EXT_draw_instanced : enable
#extension GL_EXT_gpu_shader4 : enable

attribute vec2 m_Position;
attribute vec4 m_Colour;
attribute vec2 m_TexCoord;

varying vec3 v_Position;
varying vec4 v_Colour;
varying vec2 v_TexCoord;

uniform mat4 g_ViewMatrix;
uniform mat4 g_InverseViewMatrix;
uniform mat4 g_ProjMatrix;
uniform samplerBuffer g_ParticleBuffer;

// Number of vec4's per particle
#define PARTICLE_BUFFER_STRIDE 3

void main(void)
{
	vec4 row1 = texelFetch(g_ParticleBuffer, gl_InstanceID*PARTICLE_BUFFER_STRIDE+1);
	// Scale source (xy = size)
	vec2 scaled = m_Position * row1.xy;
	// Rotate source (z = cos, w = sin)
	vec2 rotated = vec2(
		scaled.x * row1.z - scaled.y * row1.w,
		scaled.y * row1.z + scaled.x * row1.w);
	
	// Perform billboarding
	vec4 row0 = texelFetch(g_ParticleBuffer, gl_InstanceID*PARTICLE_BUFFER_STRIDE);
	v_Position = row0.xyz + 
		g_InverseViewMatrix[0].xyz * rotated.x +
		g_InverseViewMatrix[1].xyz * rotated.y;
	
	gl_Position = g_ProjMatrix * g_ViewMatrix * vec4(v_Position, 1.0);
	v_Colour = texelFetch(g_ParticleBuffer, gl_InstanceID*PARTICLE_BUFFER_STRIDE+2); // row2
	v_TexCoord = m_TexCoord;
}