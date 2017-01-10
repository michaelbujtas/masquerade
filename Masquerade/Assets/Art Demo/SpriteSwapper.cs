using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteSwapper : MonoBehaviour {

	public Sprite[] Sprites;

	public int Value = 0;
	public bool DebugSwap = false;
	Image image;

	void Start ()
	{
		image = GetComponent<Image>();
		Swap(Value);
	}
	
	void Update ()
	{
		if(DebugSwap)
		{
			DebugSwap = false;
			Swap(Value);
		}
	}

	public bool Swap(int value)
	{
		if (Value < Sprites.Length && Sprites[Value] != null)
		{

			Value = value;
			image.sprite = Sprites[Value];
			return true;
		}
		return false;

	}
}
