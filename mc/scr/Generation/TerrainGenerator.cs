using Godot;
using System;


[GlobalClass]
public partial class TerrainGenerator : Node
{
	public int baseHeight = 40;
	public int sealevel = 85;
	public int snowline = 130;
	public int maxCaveHeight = 100;
	private FastNoiseLite _biomeNoise = new FastNoiseLite();
	private FastNoiseLite _mountainNoise = new FastNoiseLite();
	private FastNoiseLite _desertNoise = new FastNoiseLite();
	private FastNoiseLite _cheeseCaveNoise = new FastNoiseLite();

	
	public void SetupNoise(int seed)
	{
		// Biome Selector: Big, sweeping shapes
		_biomeNoise.Seed = seed;
		_biomeNoise.Frequency = 0.005f;
		_biomeNoise.FractalOctaves = 2;
		_biomeNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

		// Mountains: High detail, sharp
		_mountainNoise.Seed = seed + 1;
		_mountainNoise.Frequency = 0.015f;
		_mountainNoise.FractalOctaves = 4;

		// Plains: Very flat, subtle bumps
		_desertNoise.Seed = seed + 2;
		_desertNoise.Frequency = 0.001f;

		_cheeseCaveNoise.Seed = seed + 3;
		_cheeseCaveNoise.Frequency = 0.005f;


	}
	public int[,,] GenerateTerrainShape(Vector2I chunkPos)
	{
		int[,,] chunkData = new int[34, 256, 34];
		SetupNoise(1);
		

		for (int x = 0; x < 34; x++) 
		{
			for (int y = 0; y < 34; y++)
			{
				float height = GetHeightAt(x + chunkPos.X * 32, y + chunkPos.Y * 32);
				for (int i = 0; i < 256; i++) 
				{
					if (i < maxCaveHeight && IsThereCave(x + chunkPos.X * 32, i, y + chunkPos.Y * 32))
					{
						chunkData[x, i, y] = 0;
					}
					else
					{
						if (i < height && height > sealevel && height < snowline)
						{
							if (IsThereDesert(x + chunkPos.X * 32, y + chunkPos.Y * 32))
							{
								chunkData[x, i, y] = 2;
							}
							else
							{
								chunkData[x, i, y] = 4;
							}

						}
						else if (i < height && height > sealevel && height >= snowline)
						{

							chunkData[x, i, y] = 5;
						}
						else if (i < height && height < sealevel)
						{
							chunkData[x, i, y] = 4;
						}
						else if (i > height && i <= sealevel)
						{
							chunkData[x, i, y] = 6;
						}
						else
						{
							chunkData[x, i, y] = 0;
						}
					}
				}
			}
		}
		GD.Print($"Generated Chunk on {chunkPos.X},{chunkPos.Y}");
		return chunkData;
	}

	public float GetHeightAt(float x, float z)
	{
		

		float height = baseHeight + 30 * Mathf.Pow(2, Mathf.Pow(_biomeNoise.GetNoise2D(x, z) + 1, 2)) + 10 * _mountainNoise.GetNoise2D(x, z);



		return height;
	}

	public bool IsThereDesert(float x, float z)
	{
		return _desertNoise.GetNoise2D(x, z) > 0;
	}

	public bool IsThereCave(float x, float y, float z)//vymrdany
	{
		return _cheeseCaveNoise.GetNoise3D(x, y, z) > 0.5;
	}
}
