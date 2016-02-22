using UnityEngine;
using System.Collections.Generic;

public class IndexDiscardPile : MonoBehaviour {

	public CardRenderer Renderer;

	public List<byte> Contents = new List<byte>();

	public void Start()
	{
		Renderer.Index = CardIndex.EMPTY_SLOT;
		Renderer.RefreshCardImage();
	}

	public void AddIndex(byte index)
	{
		Contents.Add(index);

		Renderer.Index = index;
		Renderer.RefreshCardImage();
	}

	public void RemoveIndex(byte index)
	{
		Contents.Remove(index);

		if(Contents.Count > 0)
		{
			Renderer.Index = Contents[Contents.Count];
		}
		else
		{
			Renderer.Index = CardIndex.EMPTY_SLOT;
		}

		Renderer.RefreshCardImage();
	}

	public void Clear()
	{
		Contents.Clear();
		Renderer.Index = CardIndex.EMPTY_SLOT;
		Renderer.RefreshCardImage();
	}
}
