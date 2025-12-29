using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class Renderer : Node
{
	Texture2DArray textureArray = new Texture2DArray();
	BlockDataLib _blockLib = new BlockDataLib();
	public void SetTextureArray()
	{
		TextureManager tm = new TextureManager();
		BlockDataLib lib = new BlockDataLib();
		string[] names = lib.GetNames();
		List<string> tempPaths = new List<string>();
		foreach (string name in names) 
		{
			if (name != "Air")
			{
				tempPaths.Add($"res://assets/textures/blocks/{name.ToLower()}.png");
			}
		}
		string[] paths = tempPaths.ToArray();

		textureArray = tm.CreateArray(paths);

	}

	public void UpdateMesh(Chunk chunk)
	{
		var vertices = new List<Vector3>();
		var normals = new List<Vector3>();
		var uvs = new List<Vector3>(); // Using Vector3 for Texture2DArray index
		var indices = new List<int>();
		var uv2s = new List<Vector2>();

		for (int x = 0; x < chunk.chunkDimms.X; x++)
		{
			for (int y = 0; y < chunk.chunkDimms.Y ; y++)
			{
				for (int z = 0; z < chunk.chunkDimms.Z; z++)
				{
					int blockId = chunk.chunkData[x+1, y, z+1];
					if (blockId == 0) continue; // Skip Air

					AddBlock(x, y, z, blockId, vertices, normals, uvs, indices, chunk, uv2s);
				}
			}
		}

		if (vertices.Count == 0) return;

		// Create ArrayMesh
		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);
		arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
		arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
		arrays[(int)Mesh.ArrayType.TexUV] = uvs.ToArray(); // Godot 4 supports Vector3 here
		arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();
		arrays[(int)Mesh.ArrayType.TexUV2] = uv2s.ToArray();

		var newMesh = new ArrayMesh();
		newMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
		chunk.meshInstance.Mesh = newMesh;

		// Update Collision
		chunk.collisionShape.Shape = newMesh.CreateTrimeshShape();

	}

	private void AddBlock(int x, int y, int z, int id, List<Vector3> verts, List<Vector3> norms, List<Vector3> uvs, List<int> indices, Chunk chunk, List<Vector2> uv2s)
	{
		
		Vector3 blockPos = new Vector3(x, y, z);
		CubeTexture cubeTex = _blockLib.GetTextureFromID(id);

		for (int i = 0; i < 6; i++)
		{
			Direction dir = (Direction)i;
			if (IsFaceVisible(x + 1, y, z + 1, dir, chunk))
			{
				int vCount = verts.Count;
				QuadData faceData = VoxelData.CubeFaces[i];
				QuadTexture faceTex = cubeTex.GetFace(dir);

				// Add Vertices
				verts.Add(faceData.Vertex0 + blockPos);
				verts.Add(faceData.Vertex1 + blockPos);
				verts.Add(faceData.Vertex2 + blockPos);
				verts.Add(faceData.Vertex3 + blockPos);

				// Add Normals
				for (int j = 0; j < 4; j++) norms.Add(faceData.Normal);

				// Add UVs (Vector3: X, Y, LayerIndex)
				uvs.Add(faceTex.UV0);
				uvs.Add(faceTex.UV1);
				uvs.Add(faceTex.UV2);
				uvs.Add(faceTex.UV3);

				uv2s.Add(new Vector2(faceTex.UV1.Z, 0));
				uv2s.Add(new Vector2(faceTex.UV1.Z, 0));
				uv2s.Add(new Vector2(faceTex.UV1.Z, 0));
				uv2s.Add(new Vector2(faceTex.UV1.Z, 0));

				
				// Add Indices (Two triangles per face)
				indices.Add(vCount + 0);
				indices.Add(vCount + 1);
				indices.Add(vCount + 2);
				indices.Add(vCount + 3);
				indices.Add(vCount + 0);
				indices.Add(vCount + 2);
			}
		}
	}

	private bool IsFaceVisible(int x, int y, int z, Direction dir, Chunk chunk)
	{
		Options options = World.GetOptions();
		if (options.GetFaceCulling())
		{
			Vector3I neighborPos = new Vector3I(x, y, z) + GetDirectionVector(dir);

			if (/*neighborPos.X < 0 || neighborPos.X >= chunk.chunkDimms.X ||*/
				neighborPos.Y < 0 || neighborPos.Y >= chunk.chunkDimms.Y //||
				/*neighborPos.Z < 0 || neighborPos.Z >= chunk.chunkDimms.Z*/)
			{
				return true;
			}

			int neighborId = chunk.chunkData[neighborPos.X, neighborPos.Y, neighborPos.Z];
			return _blockLib.GetBlockTransparencyFromID(neighborId);
		}
		else
		{
			return true;
		}
	}

	private Vector3I GetDirectionVector(Direction dir) => dir switch
	{
		Direction.Forward => new Vector3I(0, 0, -1),
		Direction.Back => new Vector3I(0, 0, 1),
		Direction.Left => new Vector3I(-1, 0, 0),
		Direction.Right => new Vector3I(1, 0, 0),
		Direction.Down => new Vector3I(0, -1, 0),
		Direction.Up => new Vector3I(0, 1, 0),
		_ => Vector3I.Zero
	};

	public async void UpdateMeshMT(Chunk chunk)
	{
		/*var meshData = await Task.Run(() =>
		{
			var vertices = new List<Vector3>();
			var normals = new List<Vector3>();
			var uvs = new List<Vector3>();
			var indices = new List<int>();
			var uv2s = new List<Vector2>();

			for (int x = 0; x < chunk.chunkDimms.X; x++)
			{
				for (int y = 0; y < chunk.chunkDimms.Y; y++)
				{
					for (int z = 0; z < chunk.chunkDimms.Z; z++)
					{
						int blockId = chunk.chunkData[x + 1, y, z + 1];
						if (blockId == 0) continue;

						AddBlock(x, y, z, blockId, vertices, normals, uvs, indices, chunk, uv2s);
					}
				}
			}

			if (vertices.Count == 0) return null;

			//Prepare the data array
			var arrays = new Godot.Collections.Array();
			arrays.Resize((int)Mesh.ArrayType.Max);
			arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
			arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
			arrays[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
			arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();
			arrays[(int)Mesh.ArrayType.TexUV2] = uv2s.ToArray();

			return arrays;
		});*/


		MeshData? result = await Task.Run<MeshData?>(() =>
		{
			var verts = new List<Vector3>();
			var indices = new List<int>();
			var norms = new List<Vector3>();
			var uvs = new List<Vector3>();
			var uv2s = new List<Vector2>();

			for (int x = 0; x < chunk.chunkDimms.X; x++)
			{
				for (int y = 0; y < chunk.chunkDimms.Y; y++)
				{
					for (int z = 0; z < chunk.chunkDimms.Z; z++)
					{
						int blockId = chunk.chunkData[x + 1, y, z + 1];
						if (blockId == 0) continue;

						AddBlock(x, y, z, blockId, verts, norms, uvs, indices, chunk, uv2s);
					}
				}
			}

			if (verts.Count == 0) return null;

			Vector3[] colFaces = new Vector3[indices.Count];
			for (int i = 0; i < indices.Count; i++)
			{
				colFaces[i] = verts[indices[i]];
			}

			return new MeshData
			{
				Vertices = verts.ToArray(),
				Indices = indices.ToArray(),
				Normals = norms.ToArray(),
				Uvs = uvs.ToArray(),
				Uv2s = uv2s.ToArray(),
				CollisionFaces = colFaces
			};
		});



		if (!GodotObject.IsInstanceValid(chunk) || !GodotObject.IsInstanceValid(chunk.meshInstance))
		{
			GD.Print("Chunk was disposed before mesh could be applied. Aborting.");
			return;
		}


		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);
		arrays[(int)Mesh.ArrayType.Vertex] = result.Value.Vertices;
		arrays[(int)Mesh.ArrayType.Index] = result.Value.Indices;
		arrays[(int)Mesh.ArrayType.Normal] = result.Value.Normals;
		arrays[(int)Mesh.ArrayType.TexUV] = result.Value.Uvs;
		arrays[(int)Mesh.ArrayType.TexUV2] = result.Value.Uv2s;

		//Back on the Main Thread: Apply the data to the Godot Nodes
		if (result != null)
		{
			var newMesh = new ArrayMesh();
			newMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

			chunk.meshInstance.Mesh = newMesh;

			// Safety check for CollisionShape
			if (chunk.collisionShape != null )
			{
				var shape = new ConcavePolygonShape3D();
				shape.Data = result.Value.CollisionFaces;
				chunk.collisionShape.Shape = shape;
			}
		}
	}
}
public struct MeshData
{
	public Vector3[] Vertices;
	public Vector3[] Normals;
	public Vector3[] Uvs;
	public int[] Indices;
	public Vector2[] Uv2s;
	public Vector3[] CollisionFaces; // For fast collision setup
}
