using UnityEngine;
using System.Collections;

public class ShowHideMenu : MonoBehaviour {
	public GameObject visuals;
	public KeyCode key;
	public bool on;

	// Use this for initialization
	void Start () {
		Toggle(on);
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(key))
		{
			Toggle();
		}
	}

	public void Toggle(bool? value = null)
	{
		if (value == null)
			on = !on;
		else
			on = (bool)value;

		visuals.SetActive(on);
	}
}
