using UnityEngine;
using BeardedManStudios.Network;
using UnityEngine.UI;

public class NetworkedSlider : NetworkedMonoBehavior
{
	public int number = 53;
	private Vector3 direction = new Vector3(15.3f, 3.74f, 9.5f);

	void Awake()
	{
		//AddNetworkVariable(GetNum, SetNum);
		//AddNetworkVariable(GetDirection, SetDirection);
	}

	private int GetNum()
	{
		return number;
	}

	private void SetNum(int newVal)
	{
		number = newVal;
	}

	private Vector3 GetDirection()
	{
		return direction;
	}

	private void SetDirection(Vector3 newDir)
	{
		direction = newDir;
	}
}