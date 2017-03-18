using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger  {
	
	public Trigger(CardLogic source)
	{
		Source = source;
	}

	public Trigger(CardLogic source, System.Action resolution)
	{
		Source = source;
		Resolution = resolution;

	}

	public CardLogic Source
	{
		get;
		private set;
	}

	public System.Action Resolution;
	public System.Action PostResolution;

	public void Resolve(System.Action postResolution)
	{
		PostResolution = postResolution;
		Resolution();
	}
}
