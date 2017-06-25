using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenAnimation : Animation {
	Vector2 Origin, Destination;
	public float Duration = 1;
	public float Speed = 1;
	public bool SpeedMode = false;
	float Age = 0;

	public bool AimAtTarget;
	public Vector3 Rotation;

	new RectTransform transform;


	System.Action callback;


	void Start () {
	}

	void Update () {
		Age += Time.deltaTime;

		if (transform != null)
		{
			transform.eulerAngles += Time.deltaTime * Rotation * Duration;

			if (SpeedMode)
			{
				float distance = Vector3.Distance(transform.position, new Vector3(Destination.x, Destination.y, transform.position.z));

				if (distance == 0)
				{
					if (callback != null)
						callback();
					Destroy(this.gameObject);
				}
				else if (distance < Speed * Time.deltaTime)
				{
					transform.position = new Vector3(Destination.x, Destination.y, transform.position.z);
				}
				else
				{

					transform.position += new Vector3(Destination.x - Origin.x, Destination.y - Origin.y, 0).normalized * Speed * Time.deltaTime;
				}
			}
			else
			{

				transform.position = Vector2.Lerp(Origin, Destination, Age / Duration);

				if (Age > Duration)
				{
					if (callback != null)
						callback();
					Destroy(this.gameObject);
				}
			}

		}

			
	}

	public void Setup(Vector2 origin, Vector2 destination, float duration, System.Action callback = null)
	{
		transform = GetComponent<RectTransform>();

		Origin = origin;
		transform.position = origin;
		Destination = destination;
		Duration *= duration;
		this.callback = callback;

		if (AimAtTarget)
			transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(destination.y - origin.y, destination.x - origin.x) * Mathf.Rad2Deg);
	}
}
