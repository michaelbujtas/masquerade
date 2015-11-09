using UnityEngine;
using System.Collections;

using UnityEngine.UI;

using AdvancedInspector;

[AdvancedInspector]
public class CardRenderer : MonoBehaviour
{
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
	public bool NoSelector;

	[Inspect]
	public Card Card;

	public void LinkCard(Card card)
	{
		card.LinkRenderer(this);
	}

	public RectTransform rectTransform
	{
		get { return (RectTransform)transform;  }
	}

	// Use this for initialization
	void Start () {

		ImportantObjectReference reference = FindObjectOfType<ImportantObjectReference>();

		Menu = reference.CardOptionsMenu;
        Selector = reference.CardSelector;


        if (Selector != null && !NoSelector)
            Selector.CardsInPlay.Add(this);

    }
	


    public void SetFacing(bool facing)
    {
        if (facing)
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
            retVal += "↑";
        if (bonus == FaceUpBonus.FACE_DOWN)
            retVal += "↓";

        return retVal;
    }


    public void RefreshCardImage()
    {
		if (Card != null)
		{
			SetFacing(Card.IsFaceUp);
			AttackText.text = FormatCombatValue(Card.Attack, Card.AttackBonus);
			DefenseText.text = FormatCombatValue(Card.Defense, Card.DefenseBonus);
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
		if(!NoSelector && Selector != null)
			Selector.CardsInPlay.Remove(this);
		Destroy(this.gameObject);
	}

}
