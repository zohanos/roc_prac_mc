using Godot;
using System;

public readonly struct VoxelData
{
    public static readonly QuadData[] CubeFaces =
    [
		// --- FRONT (-Z) ---
		new QuadData
        (
            new Vector3(-0.5f,  0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f,  0.5f, -0.5f),
            Vector3.Forward
        ),

		// --- BACK (+Z) ---
		new QuadData
        (
            new Vector3( 0.5f,  0.5f,  0.5f),
            new Vector3( 0.5f, -0.5f,  0.5f),
            new Vector3(-0.5f, -0.5f,  0.5f),
            new Vector3(-0.5f,  0.5f,  0.5f),
            Vector3.Back
        ),

		// --- LEFT (-X) ---
		new QuadData
        (
            new Vector3(-0.5f,  0.5f,  0.5f),
            new Vector3(-0.5f, -0.5f,  0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f,  0.5f, -0.5f),
            Vector3.Left
        ),

		// --- RIGHT (+X) ---
		new QuadData
        (
            new Vector3( 0.5f,  0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f,  0.5f),
            new Vector3( 0.5f,  0.5f,  0.5f),
            Vector3.Right
        ),

		// --- BOTTOM (-Y) ---
		new QuadData
        (
            new Vector3( 0.5f, -0.5f,  0.5f),
            new Vector3( 0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f,  0.5f),
            Vector3.Down
        ),

		// --- TOP (+Y) ---
		new QuadData
        (
            new Vector3( 0.5f,  0.5f, -0.5f),
            new Vector3( 0.5f,  0.5f,  0.5f),
            new Vector3(-0.5f,  0.5f,  0.5f),
            new Vector3(-0.5f,  0.5f, -0.5f),
            Vector3.Up
        ),
    ];
}


public readonly struct QuadData
{
    // Indices: 
    // 0, 1, 2 - Triangle 1
    // 3, 0, 2 - Triangle 2

    // 3 - 0
    // |   |
    // 2 - 1

    public readonly Vector3 Vertex0;
    public readonly Vector3 Vertex1;
    public readonly Vector3 Vertex2;
    public readonly Vector3 Vertex3;
    public readonly Vector3 Normal;

    public QuadData(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Vector3 normal)
    {
        Vertex0 = vertex0;
        Vertex1 = vertex1;
        Vertex2 = vertex2;
        Vertex3 = vertex3;
        Normal = normal;
    }
}


public readonly struct QuadTexture
{
    // Vertices layout:
    // 3(0,0) - 0(1,0)
    //   |       |
    // 2(0,1) - 1(1,1)

    // We use Vector3 so the Z component can store the Layer Index
    public readonly Vector3 UV0;
    public readonly Vector3 UV1;
    public readonly Vector3 UV2;
    public readonly Vector3 UV3;


    /// <param name="layerIndex">The index of the texture in the array.</param>
    public QuadTexture(int layerIndex)
    {
        // Standard normalized UVs (0 to 1)
        // Z component holds the index for the Texture2DArray
        float z = (float)layerIndex;

        UV3 = new Vector3(0f, 0f, z); // Top-Left
        UV0 = new Vector3(1f, 0f, z); // Top-Right
        UV2 = new Vector3(0f, 1f, z); // Bottom-Left
        UV1 = new Vector3(1f, 1f, z); // Bottom-Right
    }
}