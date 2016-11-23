attribute vec3 m_Position;
attribute vec4 m_Colour;
attribute vec2 m_TexCoord;

varying vec3 v_Position;
varying vec4 v_Colour;
varying vec2 v_TexCoord;

uniform mat4 g_WorldMatrix;
uniform mat4 g_ViewMatrix;
uniform mat4 g_ProjMatrix;

void main(void)
{
	gl_Position = g_ProjMatrix * g_ViewMatrix * g_WorldMatrix * vec4(m_Position.xyz, 1.0);
	v_Position = m_Position;
	v_Colour = m_Colour;
	v_TexCoord = m_TexCoord;
}