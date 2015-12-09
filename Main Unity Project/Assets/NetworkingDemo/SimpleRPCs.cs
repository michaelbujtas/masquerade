using UnityEngine;
using System.Collections;
using BeardedManStudios.Network;
using UnityEngine.UI;
using DevConsole;



public class SimpleRPCs : SimpleNetworkedMonoBehavior {
	public Color myColor;
	public Button button;


	[BRPC]
	void SetColor(Color color)
	{
		button.image.color = color;
	}

	[BRPC]
	void SendGarbage(byte first, byte second, byte third, byte fourth)
	{
		Console.Log(first, Color.magenta);
		Console.Log(second, Color.magenta);
		Console.Log(third, Color.magenta);
		Console.Log(fourth, Color.magenta);
	}

	// Use this for initialization
	void Start () {
		myColor = new Color(Random.value, Random.value, Random.value, 1);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/*public void OnButtonClick()
	{
		RPC("SetColor", myColor);
	}*/

	public void OnButtonClick()
	{
		byte first = 3;
		byte second = 7;
		byte third = 9;
		byte fourth = 205;

		Console.Log(first, Color.cyan);
		Console.Log(second, Color.cyan);
		Console.Log(third, Color.cyan);
		Console.Log(fourth, Color.cyan);
		RPC("SendGarbage", NetworkReceivers.Server, first, second, third, fourth);
	}
}
