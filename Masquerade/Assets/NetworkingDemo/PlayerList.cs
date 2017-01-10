using UnityEngine;
using System.Collections.Generic;
using BeardedManStudios.Network;

using UnityEngine.UI;
public class PlayerList : MonoBehaviour {

	public SimpleConnect connect;
	public Text text;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (connect.IsConnected)
		{
			List<NetworkingPlayer> players = connect.NetWorker.Players;
			string newText = string.Empty;
			foreach (NetworkingPlayer p in players)
			{
				newText += p.Name + " (" + p.Ip + ")\n";
			}


			text.text = newText;
		}
	}
}
