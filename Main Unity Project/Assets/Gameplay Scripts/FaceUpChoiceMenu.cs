using UnityEngine;
using System.Collections;

public class FaceUpChoiceMenu : MonoBehaviour {

	public CardRenderer FaceUpCard;
	public CardRenderer FaceDownCard;




	Card referenceCard;
	public void SetTarget(Card newCard)
	{
		referenceCard = newCard;

		FaceUpCard.Card = newCard;
		FaceUpCard.RefreshCardImage();
		FaceUpCard.SetFacing(true);


		FaceDownCard.Card = newCard;
		FaceDownCard.RefreshCardImage();
		FaceDownCard.SetFacing(false);
	}

	public void FaceUpButton()
	{
		currentRequest.Fill(true);
		currentRequest = null;
		HideMenu();
	}

	public void FaceDownButton()
	{
		currentRequest.Fill(false);
		currentRequest = null;
		HideMenu();
	}


	SingleFlipRequest currentRequest;
	public void Handle(SingleFlipRequest request)
	{
		currentRequest = request;
		SetTarget(request.card);
		ShowMenu();

	}

	public void HideMenu()
	{
		gameObject.SetActive(false);
	}

	public void ShowMenu()
	{
		gameObject.SetActive(true);
	}
}
