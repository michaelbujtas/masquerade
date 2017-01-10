using UnityEngine;
using System.Collections.Generic;

public static class HelperFunctions{

	public static int MathMod(int a, int b)
	{
		return ((a % b) + b) % b;
	}

	public static List<t> Shuffle<t>(List<t> list)
	{
		List<t> newList = new List<t>();

		while(list.Count > 0)
		{
			int randomIndex = Random.Range(0, list.Count);
			newList.Add(list[randomIndex]);
			list.RemoveAt(randomIndex);
		}

		list = newList;
		return newList;
	}
}
