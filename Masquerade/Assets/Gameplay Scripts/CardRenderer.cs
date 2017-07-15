using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

//using AdvancedInspector;

//[AdvancedInspector]
public class CardRenderer : MonoBehaviour
{


	//[Inspect]
	public GameObject Visuals;

	//[Inspect]
	public TextMeshProUGUI AttackText;
	//[Inspect]
	public TextMeshProUGUI DefenseText;
	//[Inspect]
	public TextMeshProUGUI NameText;
	//[Inspect]
	public Image Background;
	//[Inspect]
	public Image Art;
	//[Inspect]
	public ClassIcon ClassSeal;
	//[Inspect]
	public EffectIcon Nameplate;
	//[Inspect]
	public Image SelectionHighlight;
	//[Inspect]
	public Image CardBack;


	const int upArrowIndex = 0;
	const int downArrowIndex = 2;
	const int shieldIndex = 1;
	const int swordIndex = 3;

	public RectTransform rectTransform
	{
		get;
		private set;
	}


	//[Inspect]
	public bool DummyRenderer;
	bool dummyFacing = false;
	bool dummyTapped = false;

	//[Inspect]
	public Card Card
	{
		get
		{
			if(Index < 200)
				return CardIndex.GetCard(Index);
			return null;
		}
	}

	//[Inspect]
	public byte Index = CardIndex.EMPTY_SLOT;

	//[Inspect]
	CardIndex CardIndex;



	public void LinkCard(Card card)
	{
		card.LinkRenderer(this);
	}


	void Awake() {
		CardIndex = FindObjectOfType<CardIndex>();

		rectTransform = GetComponent<RectTransform>();

		OnSettled += () => {
			UseSpeed = BaselineSpeed;

		};

		MoveTarget = rectTransform.anchoredPosition;


	}


	public void Update()
	{
		if (!DummyRenderer)
		{
			RefreshCardImage();
			MoveUpdate();
		}


		}


    public void SetFacing(bool isFaceUp)
    {
		dummyFacing = isFaceUp;
        if (isFaceUp)
        {
            Background.color = Color.white;
			Art.color = Color.white;

        }
        else
		{
            Background.color = Color.gray;
			Art.color = Color.gray;
		}

    }

	string FormatCombatValue(int value, FaceUpBonus bonus, int buffs, bool noBonuses)
	{
		string retVal = value.ToString();

		
		if (bonus == FaceUpBonus.FACE_UP)
		{
			if (dummyFacing)
			{
				if(noBonuses)
					retVal = "<color=#006600>" + retVal + "</color>";
				else
					retVal = "<color=#00ff00>" + retVal + "</color>";
			}
			else
			{
				if (noBonuses)
					retVal = "<color=#660000>" + retVal + "</color>";
				else
					retVal = "<color=#ff0000>" + retVal + "</color>";
			}
		}
		else if (bonus == FaceUpBonus.FACE_DOWN)
		{
			if (dummyFacing)
			{
				if (noBonuses)
					retVal = "<color=#660000>" + retVal + "</color>";
				else
					retVal = "<color=#ff0000>" + retVal + "</color>";
			}
			else
			{
				if (noBonuses)
					retVal = "<color=#006600>" + retVal + "</color>";
				else
					retVal = "<color=#00ff00>" + retVal + "</color>";
			}
		}
		else if (buffs != 0)
		{ 
				retVal = "<color=#ff00ff>" + retVal + "</color>";
		}

        return retVal;
    }

    public void RefreshCardImage(bool? forceFaceUp = null)
    {
		if (Index == CardIndex.EMPTY_SLOT)
		{
			Visuals.SetActive(false);
			CardBack.enabled = true;
		}
		else
		{
			Visuals.SetActive(true);

			CardBack.enabled = false;
			if (CardIndex.PLAYER_1_FACEDOWN <= Index && Index <= CardIndex.PLAYER_4_FACEDOWN)
			{
				//This is us drawing a card back.
				SetFacing(false);
				Background.color = Color.black;
				Art.sprite = null;
				Art.color = Color.clear;
				ClassSeal.Clear();

				Nameplate.Swap(true);

				CardBack.enabled = true;
				AttackText.text = "?";
				DefenseText.text = "?";
				NameText.text = "?" + Index + "?";
			}
			else
			{
				//This is a card you can see, whether it's facedown or faceup.
				Card Card = CardIndex.GetCard(Index);

				if (Card != null)
				{
					CardBack.enabled = false;
					if (forceFaceUp == null)
						SetFacing(Card.IsFaceUp);
					else
						SetFacing((bool)forceFaceUp);

					AttackText.text = FormatCombatValue(Card.GetCombatAttack(dummyFacing), Card.AttackBonus, Card.TotalAttackBuff, Card.HasKeyword(Keyword.NO_BONUSES));
					DefenseText.text = FormatCombatValue(Card.GetCombatDefense(dummyFacing), Card.DefenseBonus, Card.TotalDefenseBuff, Card.HasKeyword(Keyword.NO_BONUSES));
					ClassSeal.Swap(Card.CardClass);
					Nameplate.Swap(Card.Logic != null);
					Art.sprite = Card.Art;
					NameText.text = Card.CardName;

					bool useTapped;
					if(DummyRenderer)
						useTapped = dummyTapped;
					else
						useTapped = Card.IsTapped;

					if (useTapped)
					{
						//NameText.text = "<i>" + Card.CardName + "</i>";
						Visuals.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, transform.parent.GetComponent<RectTransform>().rotation.eulerAngles.z + 270);
					}
					else
					{
						Visuals.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, transform.parent.GetComponent<RectTransform>().rotation.eulerAngles.z);
						//NameText.text = Card.CardName;
					}
					name = NameText.text;
				}
				else
				{
					//This is essentially an error message
					CardBack.enabled = false;
					Art.color = Color.magenta;
					Art.sprite = null;
					ClassSeal.Clear();
					Nameplate.Clear();
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

	public void Destroy()
	{
		Card.Renderer = null;
		Destroy(this.gameObject);
	}


	//Animation Stuff
	float BaselineSpeed = 500;
	float UseSpeed = 500;
	Vector2 MoveTarget;
	public void SetMoveTarget(Vector2 target)
	{
		MoveTarget = target;
	}
	public void SetMoveSpeed(float speed)
	{
		UseSpeed = speed;
	}
	public void SetBaselineSpeed(float speed)
	{
		BaselineSpeed = speed;
	}
	public void MoveUpdate()
	{
		Vector2 totalDelta = MoveTarget - rectTransform.anchoredPosition;
		Vector2 moveDelta = totalDelta.normalized * UseSpeed * Time.deltaTime;
		if(moveDelta.SqrMagnitude() >= totalDelta.SqrMagnitude())
		{
			rectTransform.anchoredPosition = MoveTarget;
			OnSettled();
		}
		else
		{

			rectTransform.anchoredPosition += moveDelta;
		}

	}

	public System.Action OnSettled;


}
