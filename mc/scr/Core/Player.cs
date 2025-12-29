using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export] public Node3D Head { get; set; }
	[Export] public Camera3D Camera { get; set; }
	[Export] public RayCast3D RayCast { get; set; }
	[Export] public MeshInstance3D BlockHighlight { get; set; }

	[Export] private float _mouseSensitivity = 0.1f;
	[Export] private float _movementSpeed = 4f;
	[Export] private float _jumpVelocity = 9f;
	private bool flight = false;

	private float _cameraXRotation;

	private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public static Player Instance { get; private set; }
	public override void _Ready()
	{
		Instance = this;

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion)
		{
			var mouseMotion = @event as InputEventMouseMotion;
			var deltaX = mouseMotion.Relative.Y * _mouseSensitivity;
			var deltaY = mouseMotion.Relative.X * _mouseSensitivity;

			Head.RotateY(Mathf.DegToRad(-deltaY));
			if (_cameraXRotation + deltaX > -90 && _cameraXRotation + deltaX < 90)
			{
				Camera.RotateX(Mathf.DegToRad(-deltaX));
				_cameraXRotation += deltaX;
			}
		}
	}

	public override void _Process(double delta)
	{


	}

	public override void _PhysicsProcess(double delta)
	{
		var velocity = Velocity;

		if (Input.IsActionJustPressed("EnableFlight"))
		{
			flight = !flight;
		}

		if (Input.IsActionPressed("Sprint"))
		{
			_movementSpeed = 6f;
		}
		else if (flight)
		{
			_movementSpeed = 100f;
		}
		else
		{
			_movementSpeed = 4f;
		}

		if (!IsOnFloor() && !flight)
		{
			velocity.Y -= _gravity * (float)delta;
		}

		if (Input.IsActionJustPressed("Jump") && IsOnFloor() && !flight)
		{
			velocity.Y = _jumpVelocity;
		}

		if (Input.IsActionPressed("Jump") && flight)
		{
			velocity.Y = _jumpVelocity;
		}
		else if (!Input.IsActionPressed("Jump") && flight)
		{
			velocity.Y = 0;
		}

		var inputDirection = Input.GetVector("Left", "Right", "Back", "Forward");

		var direction = Vector3.Zero;

		direction += inputDirection.X * Head.GlobalBasis.X;

		direction += inputDirection.Y * -Head.GlobalBasis.Z;

		velocity.X = direction.X * _movementSpeed;
		velocity.Z = direction.Z * _movementSpeed;

		Velocity = velocity;
		MoveAndSlide();
	}
}
