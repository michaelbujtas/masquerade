using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class CardSelector : MonoBehaviour {

    public bool wantsASelection;
    public Button cancelButton;

    public List<CardRenderer> CardsInPlay = new List<CardRenderer>();


    public void OnRendererClick(CardRenderer renderer)
    {
        if(currentTargets.Contains(renderer))
		{
			CardSelectorRequest request = currentRequest;
			Clear();
			request.Fill(renderer.Card);
        }
    }

    public void OnCancelClick()
    {
		currentRequest.Cancel();
		Clear();
    }

	void Clear()
	{
		currentRequest = null;
		wantsASelection = false;
		currentTargets = null;

		cancelButton.gameObject.SetActive(false);

		foreach (CardRenderer r in CardsInPlay)
		{
			r.Highlight(Color.clear);
		}
	}

    CardSelectorRequest currentRequest;
    List<CardRenderer> currentTargets;
    public void Handle(CardSelectorRequest request)
    {
        wantsASelection = true;
        currentRequest = request;
        currentTargets = request.GetList(CardsInPlay);

        if (currentRequest.CanBeCancelled || currentTargets.Count == 0)
            cancelButton.gameObject.SetActive(true);
        else
            cancelButton.gameObject.SetActive(false);

        foreach(CardRenderer r in CardsInPlay)
        {
			if (currentTargets.Contains(r))
				if (request is SelectDefenderRequest)
					r.Highlight(Color.red);
				else
					r.Highlight(Color.green);
			else
				r.Highlight(Color.clear);
        }
        
    }
}
