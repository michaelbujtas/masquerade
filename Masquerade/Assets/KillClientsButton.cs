using UnityEngine;
using System.Collections;
using BeardedManStudios.Network;


public class KillClientsButton : SimpleNetworkedMonoBehavior {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnKillButtonClickUI()
	{
		RPC("SuicideRPC");
	}

	[BRPC]
	void SuicideRPC()
	{
		Application.Quit();

		#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
		#endif
	}
}
