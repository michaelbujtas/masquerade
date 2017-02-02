using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firework : MonoBehaviour {

	public ParticleSystem rocketParticles;
	public ParticleSystem shellParticles;


	public Vector3 Velocity;
	public Vector3 Gravity;
	public Color Color;
	public float FuseTime;

	float FuseLeft;

	bool exploded = false;

	// Use this for initialization
	void Start ()
	{
		Velocity += Random.insideUnitSphere * 10;
		FuseTime += Random.Range(-1f, 1f);
		FuseLeft = FuseTime;
		transform.position += new Vector3(Random.Range(-60, 60), 0, 0);

		Color = Random.ColorHSV(0, 1, 1, 1, 1, 1);

		rocketParticles.startColor = Color;
		shellParticles.startColor = Color;


	}
	
	// Update is called once per frame
	void Update () {
		FuseLeft -= Time.deltaTime;
		if(FuseLeft > 0)
		{
			Velocity += Gravity * Time.deltaTime;
			transform.position += Velocity * Time.deltaTime;
			rocketParticles.startColor = Color * rocketParticles.colorOverLifetime.color.Evaluate(1 - (FuseLeft / FuseTime));
			
		}
		else
		{
			if (!exploded)
			{
				exploded = true;
				rocketParticles.gameObject.SetActive(false);
				shellParticles.gameObject.SetActive(true);
			}
		}
		if(FuseLeft < -1)
		{
			Destroy(this.gameObject);
		}

		

		
	}
}
