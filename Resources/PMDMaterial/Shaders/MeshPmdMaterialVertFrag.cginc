/*
 * MMD Shader for Unity
 *
 * Copyright 2012 Masataka SUMI, Takahiro INOUE
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 */
float4 _Color;
float _Opacity;
float4 _OutlineColor;
float _OutlineWidth;

struct v2f
{
	float4 pos : SV_POSITION;
	float2 uv  : TEXCOORD0;
};

v2f vert( appdata_base v )
{
	v2f o;
	float4 pos = mul( UNITY_MATRIX_MVP, v.vertex );
	float width = 0.01 * _OutlineWidth;
	float4 edge_pos = v.vertex + pos.w * width * float4( v.normal, 0.0 );
	o.pos = mul( UNITY_MATRIX_MVP, edge_pos );

	return o;
}
half4 frag( v2f i ) : COLOR
{
	return half4( _OutlineColor.rgb, _Opacity );
}
