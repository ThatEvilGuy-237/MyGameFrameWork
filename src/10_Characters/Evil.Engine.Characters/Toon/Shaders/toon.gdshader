shader_type spatial;
render_mode cull_disabled, ambient_light_disabled, specular_disabled;

uniform vec3 lit_color = vec3(1.0);
uniform vec3 shade_color = vec3(0.5);
uniform float threshold = 0.5;
uniform vec3 inside_color = vec3(0.0);
uniform float inside_amount = 0.0;
uniform vec3 rim_color = vec3(1.0);
uniform float rim_facing = -1.0;
uniform sampler2D paint_a : source_color, repeat_disable, filter_linear_mipmap;
uniform vec4 paint_a_region = vec4(0.0, 0.0, 1.0, 1.0);
uniform vec3 paint_a_shade_tint = vec3(1.0);
uniform vec4 paint_a_weight = vec4(0.0);
uniform float paint_a_amount = 0.0;
uniform float paint_a_lit_only = 0.0;
uniform sampler2D paint_b : source_color, repeat_disable, filter_linear_mipmap;
uniform vec4 paint_b_region = vec4(0.0, 0.0, 1.0, 1.0);
uniform vec3 paint_b_shade_tint = vec3(1.0);
uniform vec4 paint_b_weight = vec4(0.0);
uniform float paint_b_amount = 0.0;
uniform float paint_b_lit_only = 0.0;
uniform vec3 wash_color = vec3(0.3);
uniform float wash = 0.0;

varying vec4 weights;
varying vec3 raise;

void vertex() {
	weights = COLOR;
}

vec4 painted(sampler2D picture, vec4 region, vec2 uv) {
	vec2 inside = (uv - region.xy) / (region.zw - region.xy);
	float within = step(0.0, inside.x) * step(inside.x, 1.0) * step(0.0, inside.y) * step(inside.y, 1.0);
	vec4 colour = texture(picture, inside);
	return vec4(colour.rgb, colour.a * within);
}

float weighed(vec4 choice) {
	return mix(1.0, dot(choice, weights), step(0.5, choice.r + choice.g + choice.b + choice.a));
}

vec3 encoded(vec3 linear) {
	return mix(linear * 12.92, 1.055 * pow(max(linear, vec3(0.0)), vec3(1.0 / 2.4)) - 0.055, step(vec3(0.0031308), linear));
}

vec3 decoded(vec3 shown) {
	return mix(shown / 12.92, pow((shown + 0.055) / 1.055, vec3(2.4)), step(vec3(0.04045), shown));
}

void fragment() {
	vec3 lit = lit_color;
	vec3 shade = shade_color;
	lit = mix(lit, rim_color, step(dot(NORMAL, VIEW), rim_facing));
	vec4 first = painted(paint_a, paint_a_region, UV);
	float first_amount = first.a * weighed(paint_a_weight) * paint_a_amount;
	lit = mix(lit, first.rgb, first_amount);
	shade = mix(shade, first.rgb * paint_a_shade_tint, first_amount * (1.0 - paint_a_lit_only));
	vec4 second = painted(paint_b, paint_b_region, UV);
	float second_amount = second.a * weighed(paint_b_weight) * paint_b_amount;
	lit = mix(lit, second.rgb, second_amount);
	shade = mix(shade, second.rgb * paint_b_shade_tint, second_amount * (1.0 - paint_b_lit_only));
	float inner = FRONT_FACING ? 0.0 : inside_amount;
	lit = mix(mix(lit, inside_color, inner), wash_color, wash);
	shade = mix(mix(shade, inside_color, inner), wash_color, wash);
	ALBEDO = vec3(1.0);
	ROUGHNESS = 1.0;
#if CURRENT_RENDERER == RENDERER_COMPATIBILITY
	EMISSION = encoded(shade);
	raise = decoded(max(encoded(lit) - encoded(shade), vec3(0.0)));
#else
	EMISSION = shade;
	raise = lit - shade;
#endif
}

void light() {
	if (LIGHT_IS_DIRECTIONAL) {
		float facing = step(threshold, dot(NORMAL, LIGHT)) * step(0.5, ATTENUATION);
		DIFFUSE_LIGHT += facing * raise;
	}
}
