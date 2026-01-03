shader_type spatial;
render_mode unshaded;

uniform float thickness : hint_range(0.0, 0.5) = 0.005;
uniform vec3 color : source_color = vec3(0.0, 0.0, 0.0);

varying vec3 local_pos;

void vertex() {
    local_pos = VERTEX; // Get the raw position of the vertex
}

void fragment() {
    // A standard BoxMesh goes from -0.5 to 0.5. 
    // We check how close the current pixel is to any edge.
    vec3 a = abs(local_pos);
    float edge_count = 0.0;
    
    // Check if the pixel is near the edge on at least two axes
    if (a.x > 0.5 - thickness) edge_count += 1.0;
    if (a.y > 0.5 - thickness) edge_count += 1.0;
    if (a.z > 0.5 - thickness) edge_count += 1.0;

    if (edge_count >= 2.0) {
        ALBEDO = color;
    } else {
        discard; // Remove the "face" part
    }
}