using Godot;
using System;
using System.Collections.Generic;

public partial class World : Node3D
{
	public bool firstgen = true;
	[Export] public Control LoadingScreen;
	[Export] public ProgressBar ProgressBar;
	[Export] public Label DebugInfoLabel;
	public static Debuginfo debuginfo;
	private static Options options = new Options(true, 5, true, false, false);
	[Export] public Node3D player; 
	private static Vector2I _lastPlayerChunkPos;
	private const int ChunkSize = 32; 
	PackedScene chunkScene = GD.Load<PackedScene>("res://scenes/chunk.tscn");
	public static Dictionary<Vector2I, Chunk> world = new Dictionary<Vector2I, Chunk>();
	public static Dictionary<Vector2I, bool> notRenderedChunks = new Dictionary<Vector2I, bool>();
	public static int pb;
	public static bool loadingScreenVisibility = false;


	public override void _Ready()
	{
		//debuginfo.triangles = 0;
		world.Clear();
		ConfigFile config = new ConfigFile();
		Error err = config.Load("res://assets/options.cfg");


		if (err != Error.Ok)
		{
			GD.Print("Nastavení nenalezeno, vracím výchozí.");
			options = new Options(true, 4, true, false, false);
		}
		else
		{
			bool fc = (bool)config.GetValue("Graphics", "FaceCulling", true);
			int rd = (int)config.GetValue("Graphics", "RenderDistance", 4);
			bool mt = (bool)config.GetValue("System", "Multithreading", true);
			bool gm = (bool)config.GetValue("Graphics", "GreedyMeshing", false);
			bool wf = (bool)config.GetValue("Graphics", "ShowWireframe", false);

			options = new Options(fc, rd, mt, gm, wf);
		}




		SetTextureArray();
		_lastPlayerChunkPos = GetChunkPos(player.GlobalPosition);
		UpdateChunks();

	}
	public override void _Process(double delta)
	{

		if (LoadingScreen.Visible != loadingScreenVisibility)
		{
			LoadingScreen.Hide();
			firstgen = false;
		}
		if (ProgressBar.Value != pb)
		{
			ProgressBar.Value = pb;
		}
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
		Viewport viewport = GetViewport();

		// Toggle between Disabled (Normal) and Wireframe
		if (options.GetWireframe())
		{
			viewport.DebugDraw = Viewport.DebugDrawEnum.Wireframe;
		}
		else
		{
			viewport.DebugDraw = Viewport.DebugDrawEnum.Disabled;
		}
		// Identify chunks to unload
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
			if (GodotObject.IsInstanceValid(chunk))
			{
				chunk.QueueFree(); // Removes from scene and memory
			}
		}

		
		// Identify and load new chunks
		for (int x = -renderDistance; x <= renderDistance; x++)
		{
			for (int y = -renderDistance; y <= renderDistance; y++)
			{
				Vector2I relativePos = new Vector2I(x, y);
				Vector2I worldPos = _lastPlayerChunkPos + relativePos;

				if (!world.ContainsKey(worldPos))
				{
					if (firstgen) { notRenderedChunks[worldPos] = false; }
					CreateChunk(worldPos);
				}

			}
		}

		if(notRenderedChunks.Count > renderDistance * 2 * 2 & firstgen)
		{
			LoadingScreen.Show();
			loadingScreenVisibility = true;
			pb = 0;
			ProgressBar.MaxValue = notRenderedChunks.Count;
			ProgressBar.Step = 1;
			ProgressBar.Value = 0;
		}
	}

	private void CreateChunk(Vector2I pos)
	{
		Chunk chunk = (Chunk)chunkScene.Instantiate();
		AddChild(chunk);

		chunk.Position = new Vector3(pos.X * ChunkSize, 0, pos.Y * ChunkSize);
		world[pos] = chunk;

		chunk.SetChunkPosition(pos);
		switch (options.GetMultithreading())
		{
			case true:
				chunk.GenerateChunkMT();
				break;
			case false:
				chunk.GenerateChunk();
				break;
		}

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

	public static Vector2I GetPlayerChunkPos() { return _lastPlayerChunkPos; }

	public static void OptionsChanged()
	{

		ConfigFile config = new ConfigFile();
		Error err = config.Load("res://assets/options.cfg");


		if (err != Error.Ok)
		{
			GD.Print("Nastavení nenalezeno, vracím výchozí.");
			options = new Options(true, 4, true, false, false);
		}
		else
		{
			bool fc = (bool)config.GetValue("Graphics", "FaceCulling", true);
			int rd = (int)config.GetValue("Graphics", "RenderDistance", 4);
			bool mt = (bool)config.GetValue("System", "Multithreading", true);
			bool gm = (bool)config.GetValue("Graphics", "GreedyMeshing", false);
			bool wf = (bool)config.GetValue("Graphics", "ShowWireframe", false);

			options = new Options(fc, rd, mt, gm, wf);
		}
	}

	public static void RemoveFromNotRendered(Vector2I pos)
	{
		pb = pb + 1;
		notRenderedChunks.Remove(pos);
		if (notRenderedChunks.Count == 0)
		{
			loadingScreenVisibility = false;
		}
	}

	public static void SetblockInChunk(Vector3I pos, int blockID)
	{

		//Calculate Local and Chunk coordinates
		int localX = pos.X & 31;
		int localY = pos.Y;
		int localZ = pos.Z & 31;

		int chunkX = pos.X >> 5;
		int chunkY = pos.Z >> 5;

		world[new Vector2I(chunkX, chunkY)].SetBlock(localX, localY, localZ, blockID);


		// Check for Border Cases and update neighbors
		// Check X Borders
		if (localX == 0)
			UpdateNeighborPadding(chunkX - 1, chunkY, 32, localY, localZ, blockID);
		else if (localX == 31)
			UpdateNeighborPadding(chunkX + 1, chunkY, -1, localY, localZ, blockID);

		// Check Z Borders
		if (localZ == 0)
			UpdateNeighborPadding(chunkX, chunkY - 1, localX, localY, 32, blockID);
		else if (localZ == 31)
			UpdateNeighborPadding(chunkX, chunkY + 1, localX, localY, -1, blockID);
	}

	private static void UpdateNeighborPadding(int cX, int cZ, int lX, int lY, int lZ, int blockID)
	{
		Vector2I neighborCoord = new Vector2I(cX, cZ);
		if (world.ContainsKey(neighborCoord))
		{
			// We call SetBlock on the neighbor. 
			// Because your SetBlock adds +1 to indices, 
			// passing -1 results in index 0 (the padding).
			// Passing 32 results in index 33 (the padding on the opposite side).
			world[neighborCoord].SetBlock(lX, lY, lZ, blockID);
		}
	}

	/*public static void SetDebugInfo(int x)
	{
		debuginfo.triangles += x;
	}*/
}



public struct Debuginfo
{
	public int triangles;


	public override string ToString()
	{
		return $"Total triangles rendered: {triangles}";
	}
}
