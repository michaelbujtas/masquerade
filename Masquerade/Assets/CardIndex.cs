using UnityEngine;
using System.Collections.Generic;
using AdvancedInspector;

[AdvancedInspector]
public class CardIndex : MonoBehaviour {

	public const byte PLAYER_1_FACEDOWN = 201;
	public const byte PLAYER_2_FACEDOWN = 202;
	public const byte PLAYER_3_FACEDOWN = 203;
	public const byte PLAYER_4_FACEDOWN = 204;
	public const byte EMPTY_SLOT = 205;


	[Inspect]
	public string Path = "Assets\\Resources\\CardList.csv";

	[Inspect]
	public List<string> SetNames = new List<string>();


	/*[Inspect]
	public List<GameObject> Sets = new List<GameObject>();*/

	[Inspect]
	Dictionary<byte, Card> Cards = new Dictionary<byte, Card>();



	public byte[] AllCardIndices
	{
		get
		{
			byte[] retVal = new byte[Cards.Keys.Count];
			Cards.Keys.CopyTo(retVal, 0);
			return retVal;
		}

	}

	public Card GetCard(byte index)
	{
		if (!Cards.ContainsKey(index))
		{
			CustomConsole.LogWarning("There is no card at that index, which probably means it's too high.");
			return null;
		}
		else
		{

			if (index <= 200)
			{
				return Cards[index];
			}
			else
			{

				CustomConsole.LogWarning("Don't ask about indices over 200. They don't correspond to actual cards.");
				return null;
			}
		}
	}

	public void Awake () {
		byte index = 0;


		List<GameObject> generated = new List<GameObject>();

		for (int i = 0; i < SetNames.Count; i++)
		{
			generated.Add( GenerateCardsFromFile(Path, SetNames[i], ref index) );
		}
		
		/*ScrapeCardsFromGameObject(gameObject, index);

		foreach(GameObject s in Sets)
		{
			index = ScrapeCardsFromGameObject(s, index);
		}*/
	
	}

	GameObject GenerateCardsFromFile(string path, string set, ref byte startingIndex)
	{
		CsvCardLoader.OpenPath(path);
		GameObject cardObject = CsvCardLoader.GenerateCardObjects(set);
		cardObject.transform.parent = transform;
		startingIndex = ScrapeCardsFromGameObject(cardObject, startingIndex);
		return cardObject;
	}

	public byte ScrapeCardsFromGameObject(GameObject source, byte startingIndex)
	{
		byte index = startingIndex;
		foreach (Card c in source.GetComponents<Card>())
		{
			Cards[index] = c;
			c.Index = index;
			index++;
			if (index > 200)
			{
				CustomConsole.LogError("Hard cap of 200 cards in a deck. Tough.");

				break;
			}
		}

		return index;
	}

	public byte ScrapeCardsFromSet(List<Card> set, byte startingIndex)
	{
		byte index = startingIndex;
		foreach (Card c in set)
		{
			Cards[index] = c;
			c.Index = index;
			index++;
			if (index > 200)
			{
				CustomConsole.LogError("Hard cap of 200 cards in a deck. Tough.");

				break;
			}
		}

		return index;
	}
}
