using Godot;
using System;
using static Godot.TextServer;


public readonly struct BlockDataLib
 {
	readonly string[] Names = [
		"Air",
		"Stone",
		"Grass",
		"Dirt",
		"Sand",
		"Snow",
		"Water",
		"Cactus",
		];

	readonly CubeTexture[] Textures = {
		new CubeTexture(),
		new CubeTexture(new QuadTexture(0)),
		new CubeTexture(new QuadTexture(1)),
		new CubeTexture(new QuadTexture(2)),
		new CubeTexture(new QuadTexture(3)),
		new CubeTexture(new QuadTexture(4)),
		new CubeTexture(new QuadTexture(5)),
		new CubeTexture(new QuadTexture(6)),
	};

	readonly int[] IDs = [
		0,
		1,
		2,
		3,
		4, 
		5,
		6,
		7,
		];

	readonly bool[] Transparent = { 
		true, 
		false,
		false,
		false,
		false,
		false,
		false,
		false,
	};
	
	public string[] GetNames () { return Names; }
	public CubeTexture GetTextureFromID( int id)
	{
		return Textures[id];
	}
	
	public string GetBlockNameFromID( int id)
	{
		return Names[id];
	}

	public bool GetBlockTransparencyFromID(int id)
	{
		return Transparent[id];
	}
	
	
	
	public BlockDataLib()
	{
	}
}

public readonly struct CubeTexture
{
	public readonly QuadTexture Front;
	public readonly QuadTexture Back;
	public readonly QuadTexture Left;
	public readonly QuadTexture Right;
	public readonly QuadTexture Bottom;
	public readonly QuadTexture Top;

	public CubeTexture(QuadTexture t)
	{
		Front = t;
		Back = t;
		Left = t;
		Right = t;
		Bottom = t;
		Top = t;
	}

	public CubeTexture(QuadTexture top, QuadTexture bottom, QuadTexture sides)
	{
		Front = sides;
		Back = sides;
		Left = sides;
		Right = sides;
		Bottom = bottom;
		Top = top;
	}

	public CubeTexture(QuadTexture top_bottom, QuadTexture sides)
	{
		Front = sides;
		Back = sides;
		Left = sides;
		Right = sides;
		Bottom = top_bottom;
		Top = top_bottom;
	}


	public CubeTexture(QuadTexture front, QuadTexture back, QuadTexture left, QuadTexture right, QuadTexture bottom, QuadTexture top)
	{
		Front = front;
		Back = back;
		Left = left;
		Right = right;
		Bottom = bottom;
		Top = top;
	}

	public QuadTexture GetFace(Direction direction) => direction switch
	{
		Direction.Forward => Front,
		Direction.Back => Back,
		Direction.Left => Left,
		Direction.Right => Right,
		Direction.Down => Bottom,
		Direction.Up => Top,
		_ => throw new ArgumentOutOfRangeException(),
	};
}

public enum Direction
{
	Forward = 0,
	Back = 1,
	Left = 2,
	Right = 3,
	Down = 4,
	Up = 5,
}
