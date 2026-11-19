shader_type spatial;
render_mode unshaded, cull_disabled;

uniform vec3 base_color = vec3(1.0);
uniform vec3 inside_color = vec3(0.0);
uniform float inside_amount = 0.0;
uniform sampler2D paint_a : source_color, repeat_disable, filter_linear_mipmap;
uniform vec4 paint_a_region = vec4(0.0, 0.0, 1.0, 1.0);
uniform vec4 paint_a_weight = vec4(0.0);
uniform float paint_a_amount = 0.0;
uniform sampler2D paint_b : source_color, repeat_disable, filter_linear_mipmap;
uniform vec4 paint_b_region = vec4(0.0, 0.0, 1.0, 1.0);
uniform vec4 paint_b_weight = vec4(0.0);
uniform float paint_b_amount = 0.0;
uniform vec3 wash_color = vec3(0.3);
uniform float wash = 0.0;

varying vec4 weights;

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

void fragment() {
	vec3 colour = base_color;
	vec4 first = painted(paint_a, paint_a_region, UV);
	colour = mix(colour, first.rgb, first.a * weighed(paint_a_weight) * paint_a_amount);
	vec4 second = painted(paint_b, paint_b_region, UV);
	colour = mix(colour, second.rgb, second.a * weighed(paint_b_weight) * paint_b_amount);
	colour = mix(colour, inside_color, FRONT_FACING ? 0.0 : inside_amount);
	colour = mix(colour, wash_color, wash);
#if CURRENT_RENDERER == RENDERER_COMPATIBILITY
	ALBEDO = mix(colour * 12.92, 1.055 * pow(max(colour, vec3(0.0)), vec3(1.0 / 2.4)) - 0.055, step(vec3(0.0031308), colour));
#else
	ALBEDO = colour;
#endif
}
