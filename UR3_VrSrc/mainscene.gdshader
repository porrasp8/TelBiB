shader_type spatial;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec3 color = vec3(COLOR(0), COLOR(1), COLOR(2));
	COLOR = vec4(color,1.0);
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
