using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class SelectDefenderRequest : CardSelectorRequest
{
    MasqueradeEngine engine;
    Card attacker;

    public SelectDefenderRequest(MasqueradeEngine engine, Card attacker)
    {
        this.engine = engine;
        this.attacker = attacker;
    }

    public override List<CardRenderer> GetList(List<CardRenderer> allRenderers)
    {
        var retVal =
            from renderer in allRenderers
            where renderer.Card != attacker && renderer.Card.IsFaceUp
            select renderer;

        if(retVal.Count<CardRenderer>() == 0)
        {
            retVal =
            from renderer in allRenderers
            where renderer.Card != attacker
            select renderer;
        }

        return retVal.ToList();
                          
    }

    public override void Fill(Card card)
    {
        engine.Attack(attacker, card);
    }

    public override void Cancel()
    {
        Debug.Log("Attack cancelled.");
    }

}
