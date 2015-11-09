using UnityEngine;
using System.Collections.Generic;

public class CardFactory : MonoBehaviour {
	public GameObject CardPrefab;
	public GameObject Container;

	List<Vector2> usedSlots = new List<Vector2>();
	public CardRenderer MakeCard(Card card)
	{
		GameObject newCardObject = Instantiate(CardPrefab);

		CardRenderer newCard = newCardObject.GetComponent<CardRenderer>();

		newCard.LinkCard(card);

		newCard.RefreshCardImage();

		newCard.rectTransform.parent = Container.transform;
		newCard.rectTransform.localScale = new Vector3(1, 1, 1);
		//((RectTransform)newCard.transform).rotation = Quaternion.Euler(-270, 0, 0);

		Vector2 newPos;
		do
		{
			newPos = new Vector2(Random.Range(-3, 5) * 85, Random.Range(-1, 1) * 115);
		} while (usedSlots.Contains(newPos));

		usedSlots.Add(newPos);
		newCard.rectTransform.anchoredPosition = newPos;

		newCard.rectTransform.SetAsFirstSibling();
        return newCard;

	}

}
