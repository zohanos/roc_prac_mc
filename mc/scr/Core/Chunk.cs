using Godot;
using System;
using System.Threading.Tasks;

public partial class Chunk : StaticBody3D
{
	[Export] public MeshInstance3D meshInstance;
	[Export] public CollisionShape3D collisionShape;
	public Vector2I chunkPosition;
	public Vector3I chunkDimms = new Vector3I(32, 256, 32);
	public int[,,] chunkData;
	public bool rendered = false;

	public Chunk()
	{

	}
	public void SetChunkPosition(Vector2I chunkPosition)
	{
		this.chunkPosition = chunkPosition;
	}
	public void GenerateChunk()
	{
		
		var tg = new TerrainGenerator();
		chunkData = tg.GenerateTerrainShape(chunkPosition);
		RenderChunk();
	}

	public async void GenerateChunkMT()
	{
		chunkData = await Task.Run(() => {
			var tg = new TerrainGenerator();
			return tg.GenerateTerrainShape(chunkPosition);
		});
		RenderChunkMT();
	}

	public void RenderChunk()
	{
		Renderer r = new Renderer();
		r.UpdateMesh(this);
		World.RemoveFromNotRendered(chunkPosition);
	}

	public void RenderChunkMT()
	{
		Renderer r = new Renderer();
		r.UpdateMeshMT(this);
		World.RemoveFromNotRendered(chunkPosition);
	}


	public void SetBlock(int x, int y, int z, int blockid)
	{
		//at borders neighbour chunk dosnt render needed side
		chunkData[x+1, y, z+1] = blockid;
		RenderChunkMT();
	}

}
