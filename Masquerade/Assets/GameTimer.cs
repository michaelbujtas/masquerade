using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using BeardedManStudios.Network;

public class GameTimer : SimpleNetworkedMonoBehavior {

	public float GracePeriod = 1;

	public float MainTimer;


	public bool MainTimerDone = false;

	public bool MainTimerPaused = false;

	public NamedValueField TimerText;
	TimerDelegate mainTimerDelegate;

	public float SubTimer = 0;
	public bool SubTimerDone = true;

	TimerDelegate subTimerDelegate;


	public NamedValueField SubTimerText;

	public delegate void TimerDelegate();


	void Update() {

		//Main Timer
		if (MainTimer > 0 && !MainTimerDone && !MainTimerPaused)
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

		//Sub-Timer		
		if (SubTimer > 0 && !SubTimerDone)
		{
			SubTimer -= Time.deltaTime;
			if (SubTimer <= 0)
			{
				SubTimer = 0;
				SubTimerDone = true;
				if (subTimerDelegate != null)
					subTimerDelegate();
			}
		}

		if(SubTimerDone)
		{
			SubTimerText.Visuals.SetActive(false);
		}
		else
		{
			SubTimerText.Visuals.SetActive(true);
			if (SubTimer < 10)
			{
				if (SubTimer < 1)
				{
					SubTimerText.SetColors(Color.red, Color.white);
				}
				else
				{
					SubTimerText.SetColors(Color.yellow, Color.white);
				}
			}
			else
			{
				SubTimerText.SetColors(Color.green, Color.white);
			}

			SubTimerText.SetValue(SubTimer);
		}


	}

	public void SetMainTimer(float value, TimerDelegate onEnd)
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

	public TimerDelegate AddMainTimerDelegate(TimerDelegate onEnd)
	{
		mainTimerDelegate += onEnd;

		if (MainTimer == 0)
			onEnd();

		return onEnd;
	}

	public void RemoveMainTimerDelegate(TimerDelegate del)
	{
		mainTimerDelegate -= del;
	}

	public void PauseMainTimer()
	{
		if(!MainTimerPaused)
		{
			MainTimerPaused = true;
			if (OwningNetWorker.IsServer)
			{
				foreach (NetworkingPlayer p in OwningNetWorker.Players)
					AuthoritativeRPC("SyncMainTimerPause", OwningNetWorker, p, false, MainTimerPaused);
			}
		}
	}

	public void ResumeMainTimer()
	{
		if(MainTimerPaused)
		{
			MainTimerPaused = false;
			if (OwningNetWorker.IsServer)
			{
				foreach (NetworkingPlayer p in OwningNetWorker.Players)
					AuthoritativeRPC("SyncMainTimerPause", OwningNetWorker, p, false, MainTimerPaused);
			}
		}
	}

	[BRPC]
	public void SyncMainTimerPause(bool paused)
	{
		MainTimerPaused = paused;
	}

	public TimerDelegate StartSubTimer(float value, TimerDelegate onEnd)
	{
		SubTimerDone = false;
		if (OwningNetWorker.IsServer)
		{
			foreach (NetworkingPlayer p in OwningNetWorker.Players)
				AuthoritativeRPC("SyncSubTimerDisplay", OwningNetWorker, p, false, value);
		}
		SubTimer = value + GracePeriod;
		subTimerDelegate = onEnd;
		return onEnd;
	}

	public TimerDelegate AddSubTimerDelegate(TimerDelegate onEnd)
	{
		subTimerDelegate += onEnd;

		if (SubTimer == 0)
			onEnd();

		return onEnd;
	}

	public void RemoveSubTimerDelegate(TimerDelegate del)
	{
		subTimerDelegate -= del;
	}

	[BRPC]
	public void SyncMainTimerDisplay(float duration)
	{
		MainTimerDone = false;
		MainTimer = duration;
	}

	[BRPC]
	public void SyncSubTimerDisplay(float duration)
	{
		SubTimer = duration;
		SubTimerDone = false;
		if (duration == 0)
		{
			SubTimerDone = true;
			if(subTimerDelegate != null)
				subTimerDelegate();
		}
	}


	public void CancelSubTimer()
	{
		if(SubTimer > 0)
		{
			SubTimer = 0;
			SubTimerDone = true;
			if(subTimerDelegate != null)
				subTimerDelegate();

			if (OwningNetWorker.IsServer)
			{
				foreach (NetworkingPlayer p in OwningNetWorker.Players)
					AuthoritativeRPC("SyncSubTimerDisplay", OwningNetWorker, p, false, SubTimer);
			}
		}
	}
}
