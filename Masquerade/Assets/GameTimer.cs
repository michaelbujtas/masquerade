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


	public bool MainTimerDone = false;

	[Inspect]
	public NamedValueField TimerText;

	public delegate void MainTimerDelegate();

	MainTimerDelegate mainTimerDelegate;

	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() {


		if (MainTimer > 0 && !MainTimerDone)
		{
			MainTimer -= Time.deltaTime;
			if (MainTimer <= 0)
			{
				MainTimer = 0;
				MainTimerDone = true;
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

		MainTimerDone = false;
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

		if (MainTimer == 0)
			onEnd();

		return onEnd;
	}

	public void RemoveMainTimerDelegate(MainTimerDelegate del)
	{
		mainTimerDelegate -= del;
	}

	[BRPC]
	public void SyncMainTimerDisplay(float duration)
	{
		MainTimerDone = false;
		MainTimer = duration;
	}

}
