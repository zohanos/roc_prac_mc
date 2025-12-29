using Godot;
using System;

public partial class MainMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void on_start_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/world.tscn");
	}
	public void on_options_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/options_menu.tscn");
	}
	public void on_end_button_pressed()
	{
		GetTree().Quit();
		
	}
}
