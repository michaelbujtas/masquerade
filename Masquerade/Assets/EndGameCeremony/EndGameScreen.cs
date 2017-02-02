using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameScreen : MonoBehaviour {

	public List<GameObject> WinnerObjects = new List<GameObject>();
	public List<GameObject> LoserObjects = new List<GameObject>();


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ShowYouWon(bool didYouWin)
	{
		if(didYouWin)
		{
			for (int i = 0; i < WinnerObjects.Count; i++)
			{
				WinnerObjects[i].SetActive(true);
			}
		}
		else
		{
			for (int i = 0; i < LoserObjects.Count; i++)
			{
				LoserObjects[i].SetActive(true);
			}

		}
	}

	public void Hide()
	{
		for (int i = 0; i < WinnerObjects.Count; i++)
		{
			WinnerObjects[i].SetActive(false);
		}

		for (int i = 0; i < LoserObjects.Count; i++)
		{
			LoserObjects[i].SetActive(false);
		}
	}
}
