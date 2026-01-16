using Godot;
using System;

public partial class InGameOptions : Control
{
	public bool visible = false;
	[Export] public Button fcButton;
	[Export] public Button rdButton;
	[Export] public Button mtButton;
	[Export] public Button gmButton;
	[Export] public Button wfButton;
	[Export] public HSlider rdSlider;

	public Options options = new Options(true, 5, true, false, false);
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ConfigFile config = new ConfigFile();
		Error err = config.Load("user://options.cfg");


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

		rdSlider.Value = options.GetRenderDistance();
		fcButton.Text = $"FaceCulling - {options.GetFaceCulling()}";
		rdButton.Text = $"RenderDistance = {(int)rdSlider.Value}";
		mtButton.Text = $"Multithreading - {options.GetMultithreading()}";
		gmButton.Text = $"Greedy Meshing - {options.GetGreedyMeshing()}";
		wfButton.Text = $"Show wireframe - {options.GetWireframe()}";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Escape"))
		{
			if (visible)
			{
				visible = false;
				this.Hide();
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}

		}
	}

	public void _on_back_button_pressed()
	{
		ConfigFile config = new ConfigFile();

		// Nastavení hodnot (Sekce, Klíč, Hodnota)
		config.SetValue("Graphics", "FaceCulling", options.GetFaceCulling());
		config.SetValue("Graphics", "RenderDistance", options.GetRenderDistance());
		config.SetValue("System", "Multithreading", options.GetMultithreading());
		config.SetValue("Graphics", "GreedyMeshing", options.GetGreedyMeshing());
		config.SetValue("Graphics", "ShowWireframe", options.GetWireframe());

		// Uložení na disk
		Error err = config.Save("user://options.cfg");
		if (err != Error.Ok)
		{
			GD.PrintErr("Nepodařilo se uložit nastavení: " + err);
		}
		visible = false;
		this.Hide();
		Input.MouseMode = Input.MouseModeEnum.Captured;

		World.OptionsChanged();
	}

	public void _on_FaceCullingButton_pressed()
	{
		if (options.GetFaceCulling())
		{
			fcButton.Text = "FaceCulling - false";
			options.SetFaceCulling(false);
		}
		else
		{
			fcButton.Text = "FaceCulling - true";
			options.SetFaceCulling(true);
		}
	}

	public void _on_RenderDistanceSlider_changed(bool value_changed)
	{
		rdButton.Text = $"RenderDistance = {(int)rdSlider.Value}";
		options.SetRenderDistance((int)rdSlider.Value);
	}

	public void _on_MultiThreadingButton_pressed()
	{
		if (options.GetMultithreading())
		{
			mtButton.Text = "Multithreading - false";
			options.SetMultithreading(false);
		}
		else
		{
			mtButton.Text = "Multithreading - true";
			options.SetMultithreading(true);
		}
	}

	public void _on_greedy_meshing_button_pressed()
	{
		if (options.GetGreedyMeshing())
		{
			gmButton.Text = "Greedy meshing - false";
			options.SetGreedyMeshing(false);
		}
		else
		{
			gmButton.Text = "Greedy meshing - true";
			options.SetGreedyMeshing(true);
		}
	}

	public void _on_show_wireframe_button_pressed()
	{
		if (options.GetWireframe())
		{
			wfButton.Text = "Show wireframe - false";
			options.SetWireframe(false);
		}
		else
		{
			wfButton.Text = "Show wireframe - true";
			options.SetWireframe(true);
		}
	}
}
