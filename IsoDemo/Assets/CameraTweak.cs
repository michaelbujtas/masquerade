using UnityEngine;
using System.Collections;

public class CameraTweak : MonoBehaviour {

	public float orbitRadius = 10;
	public Vector3 orbitPoint;

	public float up = 35;
	public float around = 45;
	public float step = 5;

	float timeout = 0;
	float orbitTime = 0.5f;
	// Use this for initialization
	void Start () {



		


	
	}
	
	// Update is called once per frame
	void Update () {

		if (timeout > 0)
		{
			timeout -= Time.deltaTime;
		}
		else
		{
			if (Input.GetKey(KeyCode.UpArrow))
			{
				up += step;
				resetTime();
			}
			else if (Input.GetKey(KeyCode.DownArrow))
			{
				up -= step;
				resetTime();
			}
			else if (Input.GetKey(KeyCode.LeftArrow))
			{
				around -= step;
				resetTime();
			}
			else if (Input.GetKey(KeyCode.RightArrow))
			{
				around += step;
				resetTime();
			}

			transform.rotation = Quaternion.Euler(up, around, 0);

			Vector3 flatBack = new Vector3(transform.forward.x * -1, 0, transform.forward.z * -1);
			transform.position = orbitPoint + flatBack.normalized * orbitRadius;
		}
	}

	void resetTime()
	{
		timeout = orbitTime * step / 360f;
	}	

}
