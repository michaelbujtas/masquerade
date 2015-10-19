using UnityEngine;
using System.Collections;
using AdvancedInspector;
using DevConsole;

[AdvancedInspector]
public class Card : MonoBehaviour {


	[Inspect]
    public string CardName;
	[Inspect]
	public int Attack;
	[Inspect]
	public FaceUpBonus AttackBonus;
	[Inspect]
	public int Defense;
	[Inspect]
	public FaceUpBonus DefenseBonus;

	bool isFaceUp = true;
	public bool IsFaceUp
	{
		get { return isFaceUp; }
		set { isFaceUp = value; }
	}
	[Inspect, CreateDerived]
	public CardLogic[] logic = new CardLogic[1];

	public CardLogic Logic
	{
		get
		{

			if (logic != null)
			{
				if (logic.Length > 1)
					Console.LogError(CardName + "has too many logics. I'm gonna use the first one, but you need to get your shit together.");
				if (logic.Length == 0)
					return null;
				return logic[0];
			}
			return null;
		}
		set
		{
			logic[0] = value;
		}
	}

	public bool CanAttack
	{
		get { return Attack > 0; }
	}

	public bool CanFlipDown
	{
		get{ return isFaceUp == true; }
	}
	public bool CanFlipUp
	{
		get{ return isFaceUp == false; }
	}
	public bool CanActivateAbility
	{
		get { return Logic != null && Logic is IActivatedAbility; }
	}

	


	public CardRenderer Renderer;
	public void LinkRenderer(CardRenderer renderer)
	{
		Renderer = renderer;
		Renderer.Card = this;
	}

	public void LinkLogic(CardLogic logic)
	{
		Logic = logic;
		Logic.Card = this;
	}

	MasqueradeEngine engine;
	public MasqueradeEngine Engine
	{
		get
		{
			if (engine == null)
				engine = FindObjectOfType<MasqueradeEngine>();
			return engine;
		}
	}

	public void Start()
	{
		if (Logic != null)
			Logic.Card = this;

	}


	public void Copy(Card newCard)
    {
        isFaceUp = newCard.isFaceUp;
        Attack = newCard.Attack;
        Defense = newCard.Defense;
        AttackBonus = newCard.AttackBonus;
        DefenseBonus = newCard.DefenseBonus;
        CardName = newCard.CardName;


		if (newCard.Logic != null)
			Logic = newCard.Logic.Instantiate(gameObject, this) as CardLogic;
		/*

		if (newCard.Logic != null)
			//Logic = newCard.Logic;
			newCard.Logic.CopyTo(this);
			*/
    }


    public int GetCombatAttack()
    {
        if (isFaceUp && AttackBonus == FaceUpBonus.FACE_UP ||
           !isFaceUp && AttackBonus == FaceUpBonus.FACE_DOWN)
            return Attack * 2;

        return Attack;
    }

    public int GetCombatDefense()
    {
        if (isFaceUp && DefenseBonus == FaceUpBonus.FACE_UP ||
           !isFaceUp && DefenseBonus == FaceUpBonus.FACE_DOWN)
            return Defense * 2;

        return Defense;
    }
}
