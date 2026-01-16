using Godot;
using System;


[GlobalClass]
public partial class TerrainGenerator : Node
{
	public int baseHeight = 40;
	public int sealevel = 85;
	public int snowline = 130;
	public int maxCaveHeight = 100;
	private FastNoiseLite baseNoise = new FastNoiseLite();
	private FastNoiseLite helpNoise = new FastNoiseLite();
	private FastNoiseLite desertNoise = new FastNoiseLite();
	private FastNoiseLite cheeseCaveNoise = new FastNoiseLite();
    private FastNoiseLite PlantsNoise = new FastNoiseLite(); //cacti and trees
	private int seed = 1;

	public void SetSeed(int s)
	{
		seed = s;
	}

	public int[,,] GenerateTerrainShape(Vector2I chunkPos)
	{
        baseNoise.Seed = seed;
        baseNoise.Frequency = 0.005f;
        baseNoise.FractalOctaves = 2;
        baseNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

        //breaks the terrain, so its not too smooth
        helpNoise.Seed = seed + 1;
        helpNoise.Frequency = 0.015f;
        helpNoise.FractalOctaves = 4;


        desertNoise.Seed = seed + 2;
        desertNoise.Frequency = 0.001f;


        cheeseCaveNoise.Seed = seed + 3;
        cheeseCaveNoise.Frequency = 0.005f;

        PlantsNoise.Seed = seed + 4;
        PlantsNoise.Frequency = 0.005f;
        PlantsNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Cellular;
        PlantsNoise.CellularReturnType = FastNoiseLite.CellularReturnTypeEnum.CellValue;


        int[,,] chunkData = new int[34, 256, 34];
		

		for (int x = 0; x < 34; x++) 
		{
			for (int y = 0; y < 34; y++)
			{
				float plants = PlantsNoise.GetNoise2D(x + chunkPos.X * 32, y + chunkPos.Y * 32);

                float height = GetHeightAt(x + chunkPos.X * 32, y + chunkPos.Y * 32);





				for (int i = 0; i < 256; i++) 
				{
					
					if (i < maxCaveHeight && IsThereCave(x + chunkPos.X * 32, i, y + chunkPos.Y * 32) && !IsThereWater(i, height, sealevel)) //carves caves
					{
						chunkData[x, i, y] = 0;
					}
					else
					{
						if (i < height && height > sealevel && height < snowline + (helpNoise.GetNoise2D(x, y) * 5))
						{
							if (height - i < 4) //places top layer of blocks
							{
								if (IsThereDesert(x + chunkPos.X * 32, y + chunkPos.Y * 32))  //checks if there is a desert
								{
									chunkData[x, i, y] = 4;		//places sand
									if (height - i < 1 && plants > 0)
									{

										float rnd = GetWhiteNoise(x + chunkPos.X * 32, y + chunkPos.Y * 32, 1);
										//places cacti
                                        if (rnd < 0.01)
										{
											int cac = i + 1;
                                            chunkData[x, cac, y] = 7;
                                            if (rnd < 0.007)
                                            {
                                                cac += 1;
                                                chunkData[x, cac, y] = 7;
                                                if (rnd < 0.0025)
                                                {
                                                    cac += 1;
                                                    chunkData[x, cac, y] = 7;

                                                }
                                            }
                                        }
									}
								}
								else  //not in desert
								{
									if (height - i <= 1)  //places grass on top
									{
										chunkData[x, i, y] = 2;
									}
									else    //places dirt under grass
									{
										chunkData[x, i, y] = 3;
									}
								}
							}
							else  //places stone
							{
								chunkData[x, i, y] = 1;
							}

						}
						else if (i < height && height > sealevel && height >= snowline + (helpNoise.GetNoise2D(x,y) * 5))  
						{
							if (height - i < 1)
							{
								chunkData[x, i, y] = 5;     //places snow
                            }
							else
							{
								chunkData[x, i, y] = 1;     //places stone
                            }
						}
						else if (i < height && height < sealevel) //places sand under water
						{
							chunkData[x, i, y] = 4;
						}
						else if (i > height && i <= sealevel)  //places water
						{
							chunkData[x, i, y] = 6;
						}
						else
						{

							if (chunkData[x, i, y] != 7)
							{
								chunkData[x, i, y] = 0;
							}
						}
					}
				}
			}
		}
		return chunkData;
	}

	public float GetHeightAt(float x, float z)
	{
		

		float height = baseHeight + 30 * Mathf.Pow(2, Mathf.Pow(baseNoise.GetNoise2D(x, z) + 1, 2)) + 10 * helpNoise.GetNoise2D(x, z);



		return height;
	}


	public bool IsThereDesert(float x, float z)
	{
		return desertNoise.GetNoise2D(x, z) > 0;
	}

	public bool IsThereCave(float x, float y, float z)
	{
		return cheeseCaveNoise.GetNoise3D(x, y, z) > 0.5;
	}

	public bool IsThereWater(float x, float y, float z) { return x > y && x <= z; }


	
    public static float GetWhiteNoise(int x, int y, int seed) //Got this from gemini. Seems like it works.
    {

        uint BIT_NOISE1 = 0xB5297A4D;
		uint BIT_NOISE2 = 0x68E10B4C;
		uint BIT_NOISE3 = 0x1B56C4E9;
		// 1. Flatten x, y, and seed into a single 1D index
		// Using a large prime for 'y' helps prevent vertical patterns
		uint n = (uint)(x + (y * 198491317) + (seed * 1234567));

        // 2. Scramble the bits (The "Mangler")
        n ^= (n >> 8);
        n += BIT_NOISE1;
        n ^= (n << 8);
        n += BIT_NOISE2;
        n ^= (n >> 8);
        n *= BIT_NOISE3;
        n ^= (n >> 8);

        // 3. Convert to a 0.0 - 1.0 float
        return (float)n / uint.MaxValue;
    }
}
