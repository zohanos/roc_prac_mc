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
		RenderChunk();
	}

	public void RenderChunk()
	{
		Renderer r = new Renderer();
		r.UpdateMesh(this);
	}

}
