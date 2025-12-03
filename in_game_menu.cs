using Godot;
using System;

public partial class in_game_menu : Control
{
	public bool visible = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Escape"))
		{
			if (!visible) 
			{
				visible	= true;
				this.Show();
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
			else
			{
				visible = false;
				this.Hide();
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}

		}
	}

	public void on_back_button_pressed()
	{
		visible = false;
		this.Hide();
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public void on_exit_to_main_menu_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://main_menu.tscn");
	}
}
