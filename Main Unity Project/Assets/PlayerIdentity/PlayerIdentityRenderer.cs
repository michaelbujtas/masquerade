using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerIdentityRenderer : MonoBehaviour {

	public Text Name;

	public PlayerIdentity Identity;


	void Update () {
		if (Identity != null)
			Name.text = Identity.Name;
		else
			Name.text = "";
	}
}
