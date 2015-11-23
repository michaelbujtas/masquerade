using UnityEngine;
using System.Collections.Generic;
using DevConsole;

public class IndexHand : MonoBehaviour {

	public List<CardRenderer> Renderers;

	public void Awake()
	{
		Renderers.AddRange(GetComponentsInChildren<CardRenderer>());
	}


	public void SetIndex(byte slot, byte index)
	{
		Renderers[slot].Index = index;
		Renderers[slot].RefreshCardImage();
	}

	public byte FirstOpenSlot
	{
		get
		{
			for (byte i = 0; i < Renderers.Count; i++)
				if (Renderers[i].Index == CardIndex.EMPTY_SLOT)
					return i;
			Console.LogError("ArgumentOutOfRangeException: No open slots in IndexHand.");
			throw new System.NotFiniteNumberException();
		}
	}
}
