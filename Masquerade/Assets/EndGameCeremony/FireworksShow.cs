using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireworksShow : MonoBehaviour {

	public GameObject FireworkPrefab;

	public float MinTimePerWave = 1;
	public float MaxTimePerWave = 5;

	public int MinShotsPerWave = 3;
	public int MaxShotsPerWave = 10;


	float TimeLeftInWave = 0;
	float ShotsLeftInWave = 0;


	// Use this for initialization
	void Start () {
		ShotsLeftInWave = Random.Range(MinShotsPerWave, MaxShotsPerWave + 1);
		
	}
	
	// Update is called once per frame
	void Update () {
		TimeLeftInWave -= Time.deltaTime;

		if(TimeLeftInWave < 0)
		{
			if (ShotsLeftInWave > 0)
			{
				GameObject shot = Instantiate(FireworkPrefab, transform.position, Quaternion.identity);
				ShotsLeftInWave--;
			}
			else
			{
				TimeLeftInWave = Random.Range(MinTimePerWave, MaxTimePerWave);
				ShotsLeftInWave = Random.Range(MinShotsPerWave, MaxShotsPerWave + 1);
			}
		}

		
	}

	void SpawnWave()
	{

	}
}
