using Godot;
using Godot.Collections;
using System;

public partial class TextureManager : Node
{

	public Texture2DArray CreateArray(string[] paths)
	{
		var images = new Array<Image>();

		foreach (string path in paths)
		{
            Texture2D tex = ResourceLoader.Load<Texture2D>(path);
            var img = tex.GetImage();
            // Ensure the format is consistent (e.g., RGBA8)
            img.Convert(Image.Format.Rgba8);
			img.GenerateMipmaps();
			images.Add(img);
		}

		var texArray = new Texture2DArray();
		// This initializes the array with the list of images
		texArray.CreateFromImages(images);
		
		return texArray;
	}
}
