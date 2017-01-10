using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class ToggleGroupButton : MonoBehaviour {

	public bool On;

	public List<GameObject> Controlled = new List<GameObject>();
	public List<GameObject> ReverseControlled = new List<GameObject>();

	// Use this for initialization
	void Start () {
		Toggle();
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Toggle()
	{
		foreach (GameObject o in Controlled)
			o.SetActive(On);
		foreach (GameObject o in ReverseControlled)
			o.SetActive(!On);

	}

	public void OnButtonClickUI()
	{
		On = !On; 
        Toggle();
	}
}
