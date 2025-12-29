using Godot;
using System;
using System.Collections.Generic;

public partial class World : Node3D
{
	private static Options options = new Options(true, 5, true);
	[Export] public Node3D player; 
	private Vector2I _lastPlayerChunkPos;
	private const int ChunkSize = 32; 
	PackedScene chunkScene = GD.Load<PackedScene>("res://scenes/chunk.tscn");
	private Dictionary<Vector2I, Chunk> world = new Dictionary<Vector2I, Chunk>();

	public override void _Ready()
	{
		
		SetTextureArray();
		_lastPlayerChunkPos = GetChunkPos(player.GlobalPosition);
		UpdateChunks();

	}
	public override void _Process(double delta)
	{
		Vector2I currentChunkPos = GetChunkPos(player.GlobalPosition);

		if (currentChunkPos != _lastPlayerChunkPos)
		{
			_lastPlayerChunkPos = currentChunkPos;
			UpdateChunks();
		}
	}

	private Vector2I GetChunkPos(Vector3 pos)
	{
		return new Vector2I(
			Mathf.FloorToInt(pos.X / ChunkSize),
			Mathf.FloorToInt(pos.Z / ChunkSize)
		);
	}

	private void UpdateChunks()
	{
		int renderDistance = options.GetRenderDistance();

		// 1. Identify chunks to unload
		List<Vector2I> toRemove = new List<Vector2I>();
		foreach (var pos in world.Keys)
		{
			if (Math.Abs(pos.X - _lastPlayerChunkPos.X) > renderDistance ||
				Math.Abs(pos.Y - _lastPlayerChunkPos.Y) > renderDistance)
			{
				toRemove.Add(pos);
			}
		}

		// Unload them
		foreach (var pos in toRemove)
		{
			Chunk chunk = world[pos];
			world.Remove(pos);
			chunk.QueueFree(); // Removes from scene and memory
		}

		// 2. Identify and load new chunks
		for (int x = -renderDistance; x <= renderDistance; x++)
		{
			for (int y = -renderDistance; y <= renderDistance; y++)
			{
				Vector2I relativePos = new Vector2I(x, y);
				Vector2I worldPos = _lastPlayerChunkPos + relativePos;

				if (!world.ContainsKey(worldPos))
				{
					CreateChunk(worldPos);
				}
			}
		}
	}

	private void CreateChunk(Vector2I pos)
	{
		Chunk chunk = (Chunk)chunkScene.Instantiate();
		AddChild(chunk);
		chunk.Position = new Vector3(pos.X * ChunkSize, 0, pos.Y * ChunkSize);
		world[pos] = chunk;

		chunk.SetChunkPosition(pos);
		switch (true)
		{
			case true:
				chunk.GenerateChunkMT();
				break;
			case false:
				chunk.GenerateChunk();
				break;
		}

		//chunk.GenerateChunk();
	}

	public void SetTextureArray()
	{
		Texture2DArray textureArray = new Texture2DArray();
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

		Error err = ResourceSaver.Save(textureArray, "res://assets/resources/block_textures.res");

		if (err == Error.Ok)
			GD.Print("TextureArray saved successfully!");
		else
			GD.Print("Failed to save TextureArray: " + err);
	}

	public static Options GetOptions() {  return options; }
}
