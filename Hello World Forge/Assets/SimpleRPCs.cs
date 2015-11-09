using UnityEngine;
using System.Collections;
using BeardedManStudios.Network;
using UnityEngine.UI;


public class SimpleRPCs : SimpleNetworkedMonoBehavior {
	public Color myColor;
	public Button button;


	[BRPC]
	void SetColor(Color color)
	{
		button.image.color = color;
	}

	// Use this for initialization
	void Start () {
		myColor = new Color(Random.value, Random.value, Random.value, 1);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnButtonClick()
	{
		RPC("SetColor", myColor);
	}
}
