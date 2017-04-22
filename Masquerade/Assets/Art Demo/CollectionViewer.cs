using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
public class CollectionViewer : MonoBehaviour {

	public CardIndex cardIndex;
	public CardRenderer rendererPrefab;
	public GridLayoutGroup layoutGroup;

	List<CardRenderer> allRenderers = new List<CardRenderer>();

	public bool loadAllCardsOnStart = false;





	public void SetValues(List<byte> values)
	{
		//Prune the list down to the size we need
		if (allRenderers.Count > values.Count)
		{
			for (int i = values.Count; i < allRenderers.Count; i++)
			{
				Destroy(allRenderers[i].gameObject);
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
		retval.transform.parent = layoutGroup.transform;
		allRenderers.Add(retval);
		retval.Background.GetComponent<Button>().onClick.AddListener(
			delegate
			{
				OnAnyCardClicked((byte)retval.Index);
			});
		return retval;
	}

	void Start()
	{
		if (cardIndex == null)	
			cardIndex = FindObjectOfType<CardIndex>();

		if (loadAllCardsOnStart)
		{
			List<byte> indices = new List<byte>();
			indices.AddRange(cardIndex.AllCardIndices);

			List<byte> sortedList = indices.OrderBy(o => cardIndex.GetCard(o).CardName).ToList();

			SetValues(sortedList);
		}
	}

	public delegate void OnCardClickDelegate(byte index);
	public OnCardClickDelegate OnAnyCardClickedDelegate;
	void OnAnyCardClicked(byte index)
	{
		OnAnyCardClickedDelegate(index);
	}
}
