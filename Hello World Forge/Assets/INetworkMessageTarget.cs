using UnityEngine;
using System.Collections;

public interface INetworkMessageTarget {

	void OnNetworkConnectedMessage();
	void OnNetworkDisconnectedMessage();
}
