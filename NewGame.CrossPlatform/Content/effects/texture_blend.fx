texture blend_texture;
sampler s0;
sampler blend_sampler = sampler_state{Texture = (blend_texture);};

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float4 color = tex2D(s0, coords);
	//float4 blend_line_color = tex2D(blend_sampler, coords - coords);
	float4 blend_color = tex2D(blend_sampler, coords);
	
	if (color.a == 0){
		return color;
	}
	//if (color.a > 0 && color.r == 0 && color.g == 0 && color.b == 0){
	//	return blend_line_color;
	//}
	return blend_color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
