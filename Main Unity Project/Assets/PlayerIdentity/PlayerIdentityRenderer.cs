using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class PlayerIdentityRenderer : MonoBehaviour {

	public TextMeshProUGUI Name;

	public PlayerIdentity Identity;


	void Update () {
		if (Identity != null)
			Name.text = Identity.Name;
		else
			Name.text = "";
	}
}
