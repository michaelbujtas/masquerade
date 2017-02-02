using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AdvancedInspector;
using TMPro;
using System;
using BeardedManStudios.Network;

public class GameTimer : SimpleNetworkedMonoBehavior {
	[Inspect]
	public float GracePeriod = 1;

	[Inspect]
	public float MainTimer;


	bool timeDone = false;

	[Inspect]
	public NamedValueField TimerText;

	public delegate void MainTimerDelegate();

	MainTimerDelegate mainTimerDelegate;

	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() {


		if (MainTimer > 0 && !timeDone)
		{
			MainTimer -= Time.deltaTime;
			if (MainTimer <= 0)
			{
				MainTimer = 0;
				timeDone = true;
				if (mainTimerDelegate != null)
					mainTimerDelegate();
			}
		}

		if (MainTimer < 10)
		{
			if (MainTimer < 1)
			{
				TimerText.SetColors(Color.red, Color.white);
			}
			else
			{
				TimerText.SetColors(Color.yellow, Color.white);
			}
		}
		else
		{
			TimerText.SetColors(Color.white, Color.white);
		}

		TimerText.SetValue(MainTimer);



	}

	public void SetMainTimer(float value, MainTimerDelegate onEnd)
	{

		timeDone = false;
		if (OwningNetWorker.IsServer)
		{
			
			foreach(NetworkingPlayer p in OwningNetWorker.Players)
				AuthoritativeRPC("SyncMainTimerDisplay", OwningNetWorker, p, false, value);
		}
		MainTimer = value + GracePeriod;
		mainTimerDelegate = onEnd;

	}

	public MainTimerDelegate AddMainTimerDelegate(MainTimerDelegate onEnd)
	{
		mainTimerDelegate += onEnd;
		return onEnd;
	}

	public void RemoveMainTimerDelegate(MainTimerDelegate del)
	{
		mainTimerDelegate -= del;
	}

	[BRPC]
	public void SyncMainTimerDisplay(float duration)
	{
		timeDone = false;
		MainTimer = duration;
	}

}
