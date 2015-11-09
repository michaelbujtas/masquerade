using UnityEngine;
using System.Collections;
using BeardedManStudios.Network;
using UnityEngine.UI;

public class SimpleChat : SimpleNetworkedMonoBehavior
{
	public Text textBox;
	public InputField input;

	string nick = string.Empty;
	[BRPC]
	void ShowString(string chat)
	{
		textBox.text += chat;
		Debug.Log(chat);
	}
	

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnChatEnteredUI()
	{
		
		string text = input.text;

		if (text != string.Empty)
		{
			if(text.StartsWith("/"))
			{
				string[] splits = text.Split(' ');
				if (text.StartsWith("/nick"))
				{
					if (splits.Length > 1)
						nick = text.Split(' ')[1];
					else
						nick = string.Empty;
				}
			}
			else
			{
				string chatMessage = ">";
				if (!nick.Equals(string.Empty))
					chatMessage += nick + ": ";
				chatMessage += text;
				chatMessage += "/n";
				RPC("ShowString", NetworkReceivers.AllBuffered, chatMessage);

			}
			input.text = string.Empty;
			input.ActivateInputField();
		}
	}
}
