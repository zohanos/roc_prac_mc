using Godot;
using System;


public struct Options
{
    bool FaceCulling;
    int RenderDistance;
    bool Multithreading;

    public Options(bool fc, int rd, bool mt) 
    { 
        FaceCulling = fc;
        RenderDistance = rd;
        Multithreading = mt;
    }

    public bool GetFaceCulling() { return FaceCulling; }
    public int GetRenderDistance() {  return RenderDistance; }
    public bool GetMultithreading() { return Multithreading; }

}
