using UnityEngine;
using System.Collections;
public class ConsoleCooker : MonoBehaviour {

	int frame = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < 10; i++)
			CustomConsole.Log(frame + " " + i);
		frame++;
	
	}
}
