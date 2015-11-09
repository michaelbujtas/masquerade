using UnityEngine;
using System.Collections;
using BeardedManStudios.Network;


public class ClientChoice : SimpleNetworkedMonoBehavior {

	public SimpleConnect Networking;

	public GameObject RequestButton, YesButton, NoButton;
	bool? result = null; //Server

	[BRPC]
	public void AskForChoiceRPC() //Called by Server on Client
	{
		YesButton.SetActive(true);
		NoButton.SetActive(true);
	}
	
	[BRPC]
	public void AnswerChoiceRPC(bool answer) //Called by Client on Server
	{
		result = answer;
	}


	public void YesButtonClickUI() //Client
	{
		RPC("AnswerChoiceRPC", NetworkReceivers.Server, true);
		YesButton.SetActive(false);
		NoButton.SetActive(false);

	}

	public void NoButtonClickUI() //Client
	{
		RPC("AnswerChoiceRPC", NetworkReceivers.Server, false);
		YesButton.SetActive(false);
		NoButton.SetActive(false);

	}

	public void RequestButtonClickUI() //Server
	{
		//This is where we start our coroutine
		StartCoroutine(UpdateRequestCOR());
	}
	

	IEnumerator UpdateRequestCOR() //Server
	{
		RPC("AskForChoiceRPC", NetworkReceivers.Others);
		result = null;
		while(result == null)
			yield return null;
	
		Debug.Log("Client answered " + result);
	}
	

	void Start()
	{
		YesButton.SetActive(false);
		NoButton.SetActive(false);



	}

	void Update()
	{
		RequestButton.SetActive(Networking.WeAreHost);
	}


}
