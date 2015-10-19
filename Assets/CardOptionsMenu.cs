using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardOptionsMenu : MonoBehaviour {

	public CardRenderer cardRenderer;
	public Button attack;
	public Button flipUp;
	public Button flipDown;
	public Button ability;

	public MasqueradeEngine Engine;




	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	ActionRequest currentRequest;
	public void Handle(ActionRequest request)
	{
		currentRequest = request;
		ShowMenu();
		SetTarget(request.Card.Renderer);
	}




    //CardRenderer referenceCardRenderer;
    public void SetTarget(CardRenderer newCard)
    {
        //referenceCardRenderer = newCard;
        cardRenderer.Card = newCard.Card;
		cardRenderer.RefreshCardImage();

		attack.gameObject.SetActive(newCard.Card.CanAttack);
		flipUp.gameObject.SetActive(newCard.Card.CanFlipUp);
		flipDown.gameObject.SetActive(newCard.Card.CanFlipDown);
		ability.gameObject.SetActive(newCard.Card.CanActivateAbility);

	}

    public void AttackButton()
    {
		//Engine.Attack(referenceCardRenderer.Card);
		currentRequest.Fill(CardAction.ATTACK);
        HideMenu();
    }

    public void FlipUpButton()
    {
		//Engine.FlipUp(referenceCardRenderer.Card);
		currentRequest.Fill(CardAction.FLIP);
		HideMenu();
    }
    public void FlipDownButton()
    {
		//Engine.FlipDown(referenceCardRenderer.Card);
		currentRequest.Fill(CardAction.FLIP);
		HideMenu();
    }

    public void AbilityButton()
    {
		//Debug.Log("Abilities are even harder!");
		currentRequest.Fill(CardAction.ACTIVATE);
		HideMenu();
	}
    public void BackButton()
	{
		currentRequest.Fill(CardAction.CANCEL);
		HideMenu();
    }

    public void HideMenu()
    {
		cardRenderer.Card = null;
		currentRequest = null;
		gameObject.SetActive(false);
    }

    public void ShowMenu()
    {
        gameObject.SetActive(true);
    }
}
