shader_type spatial;

uniform sampler2DArray texture_array : source_color, filter_nearest_mipmap;

varying vec3 v_uv; // We use a vec3 to carry (U, V, Layer)

void vertex() {
    
}

void fragment() {
    // UV contains our tiling coordinates (x, y)
    // UV2.x contains our layer index
    vec4 tex_color = texture(texture_array, vec3(fract(UV), UV2.x));
    ALBEDO = tex_color.rgb;
}