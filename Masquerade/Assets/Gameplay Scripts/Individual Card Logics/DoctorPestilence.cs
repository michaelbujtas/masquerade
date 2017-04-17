using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoctorPestilence : CardLogic, IStartPhase, IHasKeywords
{

	List<Keyword> keywords = new List<Keyword>() { Keyword.SKIP_DRAW_PHASE };
	List<Keyword> IHasKeywords.GetKeywords()
	{
		return keywords;
	}

	void IStartPhase.OnStartPhase(MasqueradePlayer turn, System.Action callback)
	{

		callback();

	}
}
