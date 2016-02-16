using UnityEngine;
using System.Collections.Generic;

public class IndexDeck : MonoBehaviour {


	public List<byte> Indices = new List<byte>();


	// Use this for initialization
	void Start () {
		Indices.AddRange(FindObjectOfType<CardIndex>().AllCardIndices);
		Shuffle();
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void Shuffle()
	{
		List<byte> newOrder = new List<byte>();
		while (Indices.Count > 0)
		{
			int randomIndex = Random.Range(0, Indices.Count);
			newOrder.Add(Indices[randomIndex]);
			Indices.RemoveAt(randomIndex);
		}
		Indices = newOrder;
	}

	public byte Draw()
	{
		byte retVal = Indices[0];
		Indices.RemoveAt(0);
		return retVal;
	}

	public int CardsRemaining
	{
		get { return Indices.Count; }
	}
}
