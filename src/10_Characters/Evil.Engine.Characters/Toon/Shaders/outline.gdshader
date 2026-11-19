shader_type spatial;
render_mode unshaded, cull_front, shadows_disabled;

uniform vec3 line_color = vec3(0.05, 0.02, 0.04);
uniform float width = 0.003;
uniform float least_pixels = 1.0;
uniform vec3 wash_color = vec3(0.3);
uniform float wash = 0.0;

void vertex() {
	if (IN_SHADOW_PASS) {
		POSITION = vec4(2.0, 2.0, 2.0, 1.0);
	} else {
		vec3 place = (MODELVIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
		vec3 outward = normalize(MODELVIEW_NORMAL_MATRIX * NORMAL);
		float size = length(MODEL_MATRIX[0].xyz);
		float pixel = 2.0 * abs(place.z) / (abs(PROJECTION_MATRIX[1][1]) * VIEWPORT_SIZE.y);
		place += outward * max(width * size, least_pixels * pixel) * COLOR.a;
		POSITION = PROJECTION_MATRIX * vec4(place, 1.0);
	}
}

void fragment() {
	ALBEDO = mix(line_color, wash_color, wash * 0.5);
}
