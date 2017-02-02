using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AdvancedInspector;
using BeardedManStudios.Network;

[AdvancedInspector]
public class WhatsWrongWithAdvancedInspector : SimpleNetworkedMonoBehavior {

	[Inspect]
	int first;

	[Inspect]
	public int second;

	int third;

	public int fourth;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
