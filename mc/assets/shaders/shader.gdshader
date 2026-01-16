shader_type spatial;

uniform sampler2DArray texture_array : source_color, filter_nearest_mipmap;


void vertex() {
    
}

void fragment() {
    // UV contains tiling coordinates (x, y)
    // UV2.x contains layer index
    vec4 tex_color = texture(texture_array, vec3(fract(UV), UV2.x));
    ALBEDO = tex_color.rgb;
}