using UnityEngine;
using System.Collections;
using AdvancedInspector;
using BeardedManStudios.Network;
using UnityEngine.UI;

public class PlayerIdentitySettings : SimpleNetworkedMonoBehavior {

	public InputField Name;


	public PlayerIdentity MyIdentity = null;

	public void OnButtonClickUI()
	{
		RefreshSettings();
	}

	public void OnSpawnClickUI()
	{
		/*Networking.Instantiate(PlayerIdentityObject, NetworkReceivers.AllBuffered,
			   (newIdentity) => {
				   myIdentity = newIdentity.GetComponent<PlayerIdentity>();
				   CustomConsole.Log("Spawning new PlayerIdentity.");
				   myIdentity.SetName(Name.text);
			   });
		*/

	}

	public void SetName(string name)
	{
		Name.text = name;
		RefreshSettings();
	}

	public void RefreshSettings()
	{
		if(MyIdentity != null)
			MyIdentity.SetName(Name.text);
	}

}
