using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Godot.HttpRequest;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class Renderer : Node
{
	Texture2DArray textureArray = new Texture2DArray();
	BlockDataLib _blockLib = new BlockDataLib();
	public int triangles = 0;
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
		var uvs = new List<Vector2>(); // Using Vector3 for Texture2DArray index
		var indices = new List<int>();
		var uv2s = new List<Vector2>();

		for (int x = 0; x < chunk.chunkDimms.X; x++)
		{
			for (int y = 0; y < chunk.chunkDimms.Y; y++)
			{
				for (int z = 0; z < chunk.chunkDimms.Z; z++)
				{
					int blockId = chunk.chunkData[x + 1, y, z + 1];
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

	private void AddBlock(int x, int y, int z, int id, List<Vector3> verts, List<Vector3> norms, List<Vector2> uvs, List<int> indices, Chunk chunk, List<Vector2> uv2s)
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
				uvs.Add(new Vector2(faceTex.UV0.X, faceTex.UV0.Y));
				uvs.Add(new Vector2(faceTex.UV1.X, faceTex.UV1.Y));
				uvs.Add(new Vector2(faceTex.UV2.X, faceTex.UV2.Y));
				uvs.Add(new Vector2(faceTex.UV3.X, faceTex.UV3.Y));

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

			if (neighborPos.Y < 0 || neighborPos.Y >= chunk.chunkDimms.Y)
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

		MeshData? result = await Task.Run<MeshData?>(() =>
		{
			var verts = new List<Vector3>();
			var indices = new List<int>();
			var norms = new List<Vector3>();
			var uvs = new List<Vector2>();
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
			//World.SetDebugInfo(triangles);

			// Safety check for CollisionShape
			if (chunk.collisionShape != null)
			{
				var shape = new ConcavePolygonShape3D();
				shape.Data = result.Value.CollisionFaces;
				chunk.collisionShape.Shape = shape;
			}
		}
	}

	public void GreedyMesh(Chunk chunk)
	{
		MeshData? result = GreedyMeshData(chunk);
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
			//World.SetDebugInfo(triangles);

			// Safety check for CollisionShape
			if (chunk.collisionShape != null)
			{
				var shape = new ConcavePolygonShape3D();
				shape.Data = result.Value.CollisionFaces;
				chunk.collisionShape.Shape = shape;
			}
		}
	}

	public async void GreedyMeshMT(Chunk chunk)
	{
		MeshData? result = await Task.Run<MeshData?>(() => GreedyMeshData(chunk));


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
			//World.SetDebugInfo(triangles);

			// Safety check for CollisionShape
			if (chunk.collisionShape != null)
			{
				var shape = new ConcavePolygonShape3D();
				shape.Data = result.Value.CollisionFaces;
				chunk.collisionShape.Shape = shape;
			}
		}
	}







	public MeshData? GreedyMeshData(Chunk chunk)
	{
		var vertices = new List<Vector3>();
		var normals = new List<Vector3>();
		var uvs = new List<Vector2>();
		var uvs2 = new List<Vector2>(); // Added for the Layer Index
		var indices = new List<int>();

		Vector3I dimms = chunk.chunkDimms;
		Vector2I pos = chunk.chunkPosition;

		for (int d = 0; d < 6; d++)
		{
			Direction dir = (Direction)d;
			GreedyMeshFace(chunk, dir, vertices, normals, uvs, uvs2, indices);
		}

		if (vertices.Count == 0) return null;

		var mesh = new ArrayMesh();
		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);


		Vector3[] colFaces = new Vector3[indices.Count];
		for (int i = 0; i < indices.Count; i++)
		{
			colFaces[i] = vertices[indices[i]];

		}

		return new MeshData
		{
			Vertices = vertices.ToArray(),
			Indices = indices.ToArray(),
			Normals = normals.ToArray(),
			Uvs = uvs.ToArray(),
			Uv2s = uvs2.ToArray(),
			CollisionFaces = colFaces
		};
	}
	private void GreedyMeshFace(Chunk chunk, Direction dir, List<Vector3> verts, List<Vector3> norms, List<Vector2> uvs, List<Vector2> uvs2, List<int> inds)
	{
		Vector3I dim = chunk.chunkDimms;
		int axis = GetAxisForDirection(dir);
		int u = (axis + 1) % 3;
		int v = (axis + 2) % 3;

		int[] x = new int[3];
		int[] q = new int[3];
		q[axis] = 1;

		int[] mask = new int[dim[u] * dim[v]];

		for (x[axis] = -1; x[axis] < dim[axis];)
		{
			int n = 0;
			for (x[v] = 0; x[v] < dim[v]; x[v]++)
			{
				for (x[u] = 0; x[u] < dim[u]; x[u]++)
				{
					int id1 = chunk.GetBlockID(x[0], x[1], x[2]);
					int id2 = chunk.GetBlockID(x[0] + q[0], x[1] + q[1], x[2] + q[2]);

					bool isPositive = (dir == Direction.Back || dir == Direction.Right || dir == Direction.Up);
					if (isPositive) mask[n++] = (id1 != 0 && id2 == 0) ? id1 : 0;
					else mask[n++] = (id1 == 0 && id2 != 0) ? id2 : 0;
				}
			}

			x[axis]++;
			n = 0;
			for (int j = 0; j < dim[v]; j++)
			{
				for (int i = 0; i < dim[u];)
				{
					int currentID = mask[n + i];
					if (currentID != 0)
					{
						int width, height;
						for (width = 1; i + width < dim[u] && mask[n + i + width] == currentID; width++) { }
						bool done = false;
						for (height = 1; j + height < dim[v]; height++)
						{
							for (int k = 0; k < width; k++)
							{
								if (mask[n + i + k + height * dim[u]] != currentID) { done = true; break; }
							}
							if (done) break;
						}

						AddGreedyQuad(verts, norms, uvs, uvs2, inds, dir, currentID, x[axis], x, u, v, i, j, width, height);

						for (int l = 0; l < height; l++)
							for (int k = 0; k < width; k++)
								mask[n + i + k + l * dim[u]] = 0;
						i += width;
					}
					else i++;
				}
				n += dim[u];
			}
		}
	}

	private void AddGreedyQuad(List<Vector3> verts, List<Vector3> norms, List<Vector2> uvs, List<Vector2> uvs2, List<int> inds,
		Direction dir, int id, int axisPos, int[] x, int u, int v, int i, int j, int width, int height)
	{
		int vCount = verts.Count;
		Vector3 normal = GetDirectionVector(dir);

		Vector3[] corners = new Vector3[4];

		// The 'axis' coordinate is fixed for all 4 corners of the quad
		// The 'u' and 'v' coordinates vary based on width/height
		for (int k = 0; k < 4; k++) corners[k] = Vector3.Zero;

		int axis = GetAxisForDirection(dir);
		// Assign fixed axis position
		for (int k = 0; k < 4; k++) SetCoord(ref corners[k], axis, axisPos);

		// Assign U and V offsets
		// Corner 0: (u, v)
		SetCoord(ref corners[0], u, i);
		SetCoord(ref corners[0], v, j);

		// Corner 1: (u+w, v)
		SetCoord(ref corners[1], u, i + width);
		SetCoord(ref corners[1], v, j);

		// Corner 2: (u+w, v+h)
		SetCoord(ref corners[2], u, i + width);
		SetCoord(ref corners[2], v, j + height);

		// Corner 3: (u, v+h)
		SetCoord(ref corners[3], u, i);
		SetCoord(ref corners[3], v, j + height);

		foreach (var vec in corners) verts.Add(vec-new Vector3(0.5f, 0.5f, 0.5f));
		for (int k = 0; k < 4; k++) norms.Add(normal);


		// UV: Width and Height for tiling
		uvs.Add(new Vector2(0, 0));
		uvs.Add(new Vector2(width, 0));
		uvs.Add(new Vector2(width, height));
		uvs.Add(new Vector2(0, height));

		// UV2: Layer Index (Z-coord)
		float layer = id - 1;
		for (int k = 0; k < 4; k++) uvs2.Add(new Vector2(layer, 0));

		bool isPositive = (dir == Direction.Back || dir == Direction.Right || dir == Direction.Up);
		if (!isPositive)
		{
			inds.Add(vCount + 0); inds.Add(vCount + 1); inds.Add(vCount + 2);
			inds.Add(vCount + 0); inds.Add(vCount + 2); inds.Add(vCount + 3);
		}
		else
		{
			inds.Add(vCount + 0); inds.Add(vCount + 2); inds.Add(vCount + 1);
			inds.Add(vCount + 0); inds.Add(vCount + 3); inds.Add(vCount + 2);
		}
	}


	private void SetCoord(ref Vector3 vec, int index, float value)
	{
		if (index == 0) vec.X = value;
		else if (index == 1) vec.Y = value;
		else vec.Z = value;
	}
	private int GetAxisForDirection(Direction dir) => dir switch
	{
		Direction.Left or Direction.Right => 0,
		Direction.Down or Direction.Up => 1,
		Direction.Forward or Direction.Back => 2,
		_ => 0
	};
}
public struct MeshData
{
	public Vector3[] Vertices;
	public Vector3[] Normals;
	public Vector2[] Uvs;
	public int[] Indices;
	public Vector2[] Uv2s;
	public Vector3[] CollisionFaces; // For fast collision setup
}
