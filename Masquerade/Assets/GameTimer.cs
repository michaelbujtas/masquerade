using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AdvancedInspector;
using TMPro;
using System;
using BeardedManStudios.Network;

public class GameTimer : SimpleNetworkedMonoBehavior {

	[Inspect]
	public float MainTimer;

	[Inspect]
	public long TargetTicks;

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

		long TicksLeft = TargetTicks - DateTime.UtcNow.Ticks;

		if(TicksLeft > 0)
		{
			MainTimer = (float)TicksLeft / 10000000;
		}
		else
		{
			MainTimer = 0;
			if(!timeDone)
			{
				timeDone = true;
				if (mainTimerDelegate != null)
					mainTimerDelegate();
			}
		}


		/*if (MainTimer > 0)
		{
			MainTimer -= Time.deltaTime;
			if (MainTimer <= 0)
			{
				MainTimer = 0;
				if (mainTimerDelegate != null)
					mainTimerDelegate();
			}
		}*/

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
		TargetTicks = DateTime.UtcNow.AddSeconds(value).Ticks;
		if (OwningNetWorker.IsServer)
		{
			foreach(NetworkingPlayer p in OwningNetWorker.Players)
				AuthoritativeRPC("SyncMainTimerDisplay", OwningNetWorker, p, false, TargetTicks);
		}
		MainTimer = value;
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
	public void SyncMainTimerDisplay(long targetTicks)
	{
		TargetTicks = targetTicks;
	}

}
