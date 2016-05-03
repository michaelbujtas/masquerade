using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CardRenderer))]
public class CollectionCard : MonoBehaviour {

	CardRenderer cardRenderer;

	CardIndex cardIndex;


	void Awake () {
		cardRenderer = GetComponent<CardRenderer>();
		cardIndex = FindObjectOfType<CardIndex>();
	
	}
	

	public void Flip()
	{
		cardIndex.GetCard(cardRenderer.Index).IsFaceUp = !cardIndex.GetCard(cardRenderer.Index).IsFaceUp;
	}
}
