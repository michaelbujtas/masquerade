using UnityEngine;
using System.Collections;

using UnityEngine.UI;

using AdvancedInspector;

[AdvancedInspector]
public class CardRenderer : MonoBehaviour
{
	[Inspect]
	public GameObject Visuals;

	[Inspect]
	public Text AttackText;
	[Inspect]
	public Text DefenseText;
	[Inspect]
	public Text NameText;
	[Inspect]
	public Image Background;
	[Inspect]
	public Image SelectionHighlight;

	CardOptionsMenu Menu;
	CardSelector Selector;

	[Inspect]
	public bool DummyRenderer;
	bool dummyFacing = false;

	[Inspect]
	public Card Card
	{
		get
		{
			if(Index < 200)
				return CardIndex.GetCard(Index);
			return null;
		}
		set
		{
			throw new System.Exception("You're using something incompatible with the newly-broken Card Renderer rewrite. Sorry sorry big sorry.");
		}
	}

	[Inspect]
	public byte Index = CardIndex.EMPTY_SLOT;

	[Inspect]
	CardIndex CardIndex;



	public void LinkCard(Card card)
	{
		card.LinkRenderer(this);
	}

	public RectTransform rectTransform
	{
		get { return (RectTransform)transform; }
	}

	// Use this for initialization
	void Awake() {
		CardIndex = FindObjectOfType<CardIndex>();

		/*ImportantObjectReference reference = FindObjectOfType<ImportantObjectReference>();

		Menu = reference.CardOptionsMenu;
        Selector = reference.CardSelector;


        if (Selector != null && !NoSelector)
            Selector.CardsInPlay.Add(this);*/


	}


	public void Update()
	{
		if(!DummyRenderer)
			RefreshCardImage();
	}


    public void SetFacing(bool isFaceUp)
    {
		dummyFacing = isFaceUp;
        if (isFaceUp)
        {
            AttackText.color = Color.black;
            DefenseText.color = Color.black;
            NameText.color = Color.black;
            Background.color = Color.white;
        }
        else
        {

            AttackText.color = Color.red;
            DefenseText.color = Color.red;
            NameText.color = Color.red;
            Background.color = Color.gray;
        }

    }

	string FormatCombatValue(int value, FaceUpBonus bonus)
	{
		string retVal = value.ToString();

		if (bonus == FaceUpBonus.FACE_UP)
		{
			if (dummyFacing)
				retVal = "<color=#00ff00>" + retVal + "</color>";
			else
				retVal += "↑";
		}

		if (bonus == FaceUpBonus.FACE_DOWN)
		{
			if (!dummyFacing)
				retVal = "<color=#00ff00>" + retVal + "</color>";
			else
				retVal += "↓";
		}

        return retVal;
    }


    public void RefreshCardImage(bool? forceFaceUp = null)
    {
		if (Index == CardIndex.EMPTY_SLOT)
		{
			Visuals.SetActive(false);
		}
		else
		{
			Visuals.SetActive(true);

			if (CardIndex.PLAYER_1_FACEDOWN <= Index && Index <= CardIndex.PLAYER_4_FACEDOWN)
			{
				SetFacing(false);
				Background.color = Color.black;
				AttackText.text = "¿";
				DefenseText.text = "?";
				NameText.text = "¿" + Index + "?";
			}
			else
			{

				Card Card = CardIndex.GetCard(Index);

				if (Card != null)
				{
					if (forceFaceUp == null)
						SetFacing(Card.IsFaceUp);
					else
						SetFacing((bool)forceFaceUp);

					AttackText.text = FormatCombatValue(Card.GetCombatAttack(dummyFacing), Card.AttackBonus);
					DefenseText.text = FormatCombatValue(Card.GetCombatDefense(dummyFacing), Card.DefenseBonus);
					
					if(Card.IsTapped)
						NameText.text = "<i>" + Card.CardName + "</i>";
					else
						NameText.text = Card.CardName;
					name = NameText.text;
				}
				else
				{
					SetFacing(true);
					AttackText.text = "";
					DefenseText.text = "";
					NameText.text = "NULL";
				}
			}

		}
        
    }

    public void Highlight(Color color)
    {
        SelectionHighlight.color = color;
    }

    public void ShowFullMenu()
    {
        if (Selector != null)
        {
            if (Selector.wantsASelection)
            {
                Selector.OnRendererClick(this);
            }
            else
            {
                //Menu.SetTarget(this);
                //Menu.ShowMenu();
            }
        }
    }

	public void Destroy()
	{
		Card.Renderer = null;
		if(!DummyRenderer && Selector != null)
			Selector.CardsInPlay.Remove(this);
		Destroy(this.gameObject);
	}

}
