using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLogicFactory {

	public static CardLogic GetCard(string name)
	{
		switch(name.ToLower())
		{
			case "debug dude":
				return new OlafTheGlutton();
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
			case "sorcerer theodoric whitemane":
				return new SorcerorWhitemane();
			case "lance wymund":
				return new LanceWymund();
			case "barbarian bodyguard ogg":
				return new BodyguardOgg();
			case "novice ralf hicket":
				return new HalfRicket();
			case "blacksmith roger norman":
				return new BlacksmithRogerNorman();
			case "alchemist leon brewster":
				return new AlchemistLeonBrewster();
			case "dante anderson the stud":
				return new DanteTheStud();
			case "sir arthur mourne":
				return new ArthurMourne();
			case "rollin hodgkin":
				return new RollinHodgkin();


		}

		return null;
	}
}
