using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using AdvancedInspector;

[AdvancedInspector]
public class EffectIcon : MonoBehaviour {

	[Inspect]
	public Sprite NormalSprite;
	[Inspect]
	public Sprite EffectSprite;
	Image image;

	public bool CurrentValue = false;

	void Awake()
	{
		image = GetComponent<Image>();
		Swap(CurrentValue);
	}

	public void Swap(bool newValue)
	{
		CurrentValue = newValue;
		image.color = Color.white;

		if (CurrentValue)
			image.sprite = EffectSprite;
		else
			image.sprite = NormalSprite;
	}

	public void Clear()
	{
		image.sprite = null;
		image.color = Color.clear;
	}
}
