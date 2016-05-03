using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class CollectionViewer : MonoBehaviour {

	public CardIndex cardIndex;
	public CardRenderer rendererPrefab;
	List<CardRenderer> allRenderers = new List<CardRenderer>();

	public void GenerateCardsForValues(List<byte> values)
	{
		//Prune the list down to the size we need
		if (allRenderers.Count > values.Count)
		{
			for (int i = values.Count; i < allRenderers.Count; i++)
			{
				Destroy(allRenderers[i]);
			}
			allRenderers.RemoveRange(values.Count, allRenderers.Count - values.Count);
		}

		//Set it to our values
		for(int i = 0; i < values.Count; i++)
		{
			if (i < allRenderers.Count)
				allRenderers[i].Index = values[i];
			else
				AddCard().Index = values[i]; //Expand it if we have to
		}
	}

	CardRenderer AddCard()
	{
		CardRenderer retval = Instantiate(rendererPrefab);
		retval.transform.parent = transform;
		allRenderers.Add(retval);
		return retval;
	}

	void Start()
	{
		List<byte> indices = new List<byte>();
		indices.AddRange(cardIndex.AllCardIndices);

		List<byte> sortedList = indices.OrderBy(o => cardIndex.GetCard(o).CardName).ToList();

		GenerateCardsForValues(sortedList);
	}
}
