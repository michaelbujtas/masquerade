﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLogicFactory {

	public static CardLogic GetCard(string name)
	{
		switch(name.ToLower())
		{
			case "court assassin timm":
				return new Timm();
			case "king alric dacre":
				return new KingAlric();
			case "sir olaf the glutton":
				return new OlafTheGlutton();
			case "the black widow":
				return new BlackWidow();
			case "wench helga":
				return new BlackWidow();
		}

		return null;
	}
}