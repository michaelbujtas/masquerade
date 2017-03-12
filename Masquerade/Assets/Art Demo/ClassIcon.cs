using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class ClassIcon : MonoBehaviour {

	public Sprite KingSprite;
	public Sprite QueenSprite;
	public Sprite NobleSprite;
	public Sprite SoldierSprite;
	public Sprite AssassinSprite;
	public Sprite CommonSprite;

	public CardClass CurrentClass = 0;
	public bool DebugSwap = false;
	Image image;

	void Awake()
	{
		image = GetComponent<Image>();
		Swap(CurrentClass);
	}

	void Update()
	{
		if (DebugSwap)
		{
			DebugSwap = false;
			Swap(CurrentClass);
		}
	}

	public bool Swap(CardClass newClass)
	{
		CurrentClass = newClass;
		image.color = Color.white;

		switch(CurrentClass)
		{
			case CardClass.KING:
				image.sprite = KingSprite;
				return true;
			case CardClass.QUEEN:
				image.sprite = QueenSprite;
				return true;
			case CardClass.NOBLE:
				image.sprite = NobleSprite;
				return true;
			case CardClass.SOLDIER:
				image.sprite = SoldierSprite;
				return true;
			case CardClass.ASSASSIN:
				image.sprite = AssassinSprite;
				return true;
			case CardClass.COMMONER:
				image.sprite = CommonSprite;
				return true;

		}

		CustomConsole.LogWarning("I guess you added a class to the enum but didn't add it to classIcon? Fix that.");
		return false;

	}

	public void Clear()
	{
		image.sprite = null;
		image.color = Color.clear;
	}
}
