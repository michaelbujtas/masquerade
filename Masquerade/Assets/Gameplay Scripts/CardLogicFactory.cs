using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLogicFactory {

	public static CardLogic GetCard(string name)
	{
		switch(name.ToLower())
		{
			/*case "debug dude":
				return new WarrinOldman();
			case "debug dude2":
				return new WarrinOldman();
			case "debug dude3":
				return new WarrinOldman();
			case "debug dude4":
				return new WarrinOldman();*/
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
			case "queen beatrice dacre":
				return new QueenBea();
			case "the avatar of festivals":
				return new AvatarOfFestivals();
			case "advisor simon ernest":
				return new SimonErnest();
			case "sergeant warrin oldman":
				return new WarrinOldman();
			case "mistress eva":
				return new MistressEva();
			case "lord snivellous feck":
				return new Feck();
			case "guardsman terric illbert":
				return new GuardsmanTerricIllbert();
			case "gossip lady anneliese":
				return new GossipLady();
			case "princess isolda drago":
				return new IsoldaDrago();
			case "baron radolf maynard":
				return new BaronRadolf();
			case "nickie \"the shredder\" dickens":
				return new NickieTheShredder();
			case "ourri the nobleslayer":
				return new OurriTheNobleslayer();
			case "raz the shiv":
				return new Poisoner();
			case "poisoner roger baldwin":
				return new Poisoner();
			case "dancer isabella pascoe":
				return new DancerIsabella();
			case "court jester foolset":
				return new CourtJesterFoolset();
			case "job the butler":
				return new JobTheButler();
			case "mary the mute":
				return new MaryTheMute();
			case "lady beatrice serell":
				return new LadyBeatriceSerell();
			case "gravekeeper anathema":
				return new GravekeeperAnathema();
			case "bishop cornellous shepard":
				return new Bishop();
			case "doctor pestilence":
				return new DoctorPestilence();
		}

		return null;
	}
}
