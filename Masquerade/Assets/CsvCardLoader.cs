using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CsvHelper;
using System.IO;
using AdvancedInspector;


public static class CsvCardLoader{


	static string ArtFolder = "Full Size Character Portraits\\";
	static Dictionary<string, List<record>> allRecords = new Dictionary<string, List<record>>();

	public static void OpenCSV(string path, bool openPath)
	{
		allRecords = new Dictionary<string, List<record>>();
		TextReader textReader;
		if (openPath)
			textReader = File.OpenText(path);
		else
			textReader = new StreamReader(path);

		var csv = new CsvReader(textReader);

		var records = csv.GetRecords<record>();

		foreach (record r in records)
		{
			if (!allRecords.ContainsKey(r.Set))
				allRecords[r.Set] = new List<record>();

			allRecords[r.Set].Add(r);
		}
	}

	public static GameObject GenerateCardObjects(string setName)
	{
		GameObject setObject = new GameObject(setName);

		if(allRecords.ContainsKey(setName))
		{

			foreach (record r in allRecords[setName])
			{
				Card card = setObject.AddComponent<Card>();
				card.CardName = r.Name;
			
				switch(r.Class.ToLower())
				{
					case "noble":
						card.CardClass = CardClass.NOBLE;
						break;
					case "assassin":
						card.CardClass = CardClass.ASSASSIN;
						break;
					case "commoner":
						card.CardClass = CardClass.COMMONER;
						break;
					case "soldier":
						card.CardClass = CardClass.SOLDIER;
						break;
					case "queen":
						card.CardClass = CardClass.QUEEN;
						break;
					case "king":
						card.CardClass = CardClass.KING;
						break;
				}

				card.Attack = r.Attack;
				card.AttackBonus = getBonusFromString(r.AttackBonus);

				card.Defense = r.Defense;
				card.DefenseBonus = getBonusFromString(r.DefenseBonus);

				card.RulesText = r.RulesText;
				card.FlavorText = r.FlavorText;

				card.Art = Resources.Load<Sprite>(ArtFolder + r.ArtPath);
				
				card.Logic = CardLogicFactory.GetCard(r.Name);

				}
		}
		else
		{
			Debug.LogError("No set called " + setName);
		}

		return setObject;

	}

	static FaceUpBonus getBonusFromString(string s)
	{
		switch(s.ToLower())
		{
			case "up":
				return FaceUpBonus.FACE_UP;
			case "down":
				return FaceUpBonus.FACE_DOWN;
		}
		return FaceUpBonus.NONE;
	}
	private class record
	{
		public string Name { get; set; }
		public string Class {get; set;}
		public int Attack { get; set; }
		public string AttackBonus { get; set; }
		public int Defense { get; set; }
		public string DefenseBonus { get; set; }
		public string RulesText { get; set; }
		public string FlavorText { get; set; }
		public string Set { get; set; }
		public string ArtPath { get; set; }
	}
}
