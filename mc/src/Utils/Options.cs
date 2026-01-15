using Godot;
using System;


public struct Options
{
	bool FaceCulling;
	int RenderDistance;
	bool Multithreading;
	bool GreedyMeshing;
	bool ShowWireframe;

	public Options(bool fc, int rd, bool mt, bool gm, bool wf) 
	{ 
		FaceCulling = fc;
		RenderDistance = rd;
		Multithreading = mt;
		GreedyMeshing = gm;
		ShowWireframe = wf;
	}

	public bool GetFaceCulling() { return FaceCulling; }
	public int GetRenderDistance() {  return RenderDistance; }
	public bool GetMultithreading() { return Multithreading; }
	public bool GetGreedyMeshing() { return GreedyMeshing; }
	public bool GetWireframe() { return ShowWireframe; }

	public void SetFaceCulling(bool fc) { FaceCulling = fc; }
	public void SetRenderDistance(int rd) { RenderDistance = rd; }
	public void SetMultithreading(bool mt) { Multithreading = mt; }
	public void SetGreedyMeshing(bool gm) { GreedyMeshing = gm; }
	public void SetWireframe(bool wf) { ShowWireframe = wf; }
}
