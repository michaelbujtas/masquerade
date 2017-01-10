using UnityEngine;
using System.Collections;
using BeardedManStudios.Network;
using UnityEngine.UI;

public class SimpleChat : SimpleNetworkedMonoBehavior
{
	public Text textBox;
	public InputField input;
	public PlayerIdentitySettings identitySettings;


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
						identitySettings.SetName(text.Split(' ')[1]);
					else
						identitySettings.SetName("");
				}
			}
			else
			{
				string chatMessage = ">";
				if (identitySettings.MyIdentity != null)
				{
					if (!identitySettings.MyIdentity.Name.Equals(string.Empty))
						chatMessage += identitySettings.MyIdentity.Name + ": ";
				}
				else
				{

					if (!identitySettings.Name.text.Equals(string.Empty))
						chatMessage += identitySettings.Name.text + ": ";
				}
				chatMessage += text;
				chatMessage += "\n";
				RPC("ShowString", NetworkReceivers.AllBuffered, chatMessage);

			}
			input.text = string.Empty;
			input.ActivateInputField();
		}
	}
}
