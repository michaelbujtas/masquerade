using UnityEngine;
using System.Collections.Generic;
using System;
using BeardedManStudios.Network;

public class CustomConsole : SimpleNetworkedMonoBehavior {

	static CustomConsole singleton = null;


	public static void Log(object value, ConsoleMessageType messageType = ConsoleMessageType.DEFAULT)
	{
		Log(value, new Color(1, 1, 1), messageType);
	}

	public static void Log(object value, Color color, ConsoleMessageType messageType = ConsoleMessageType.DEFAULT)
	{
		singleton.onLog(value.ToString(), color, messageType);
	}

	public static void LogWarning(object value)
	{
		Log(value, new Color(1, 1, 0), ConsoleMessageType.WARNING);
	}

	public static void LogError(object value)
	{
		Log(value, new Color(1, 0, 0), ConsoleMessageType.ERROR);
	}

	public static void LogSpam(object value)
	{
		Log(value, new Color(1, 0, 1), ConsoleMessageType.SPAM);
	}

	public static void LogNetworked(object value, Color color, ConsoleMessageType messageType = ConsoleMessageType.DEFAULT)
	{
		if(singleton.OwningNetWorker.Connected)
		{
			singleton.RPC("LogRPC", singleton.OwningNetWorker, NetworkReceivers.All, value.ToString(), color, messageType);
		}
		else
		{
			Log(value, color, messageType);
		}
	}

	public static void LogNetworked(object value)
	{
		LogNetworked(value, singleton.OwningNetWorker.IsServer ? new Color(0, 1, 1) : new Color(0, 1, 0), ConsoleMessageType.NETWORKED);
	}

	[BRPC]
	void LogRPC(string value, Color color, ConsoleMessageType messageType)
	{
		Log(value, color, messageType);
	}

	public static void Watch(object value)
	{
		singleton.onWatch(value);
	}


	public static void Unwatch(object value)
	{
		singleton.onUnwatch(value);
	}

	public Action<string, Color, ConsoleMessageType> onLog;
	public Action<object> onWatch;
	public Action<object> onUnwatch;

	public List<LogEntry> MasterLog = new List<LogEntry>();
	public List<object> MasterWatch = new List<object>();
	public int maxLogSize = 10000;
	public int dumpSize = 1000;


	void Awake () {

		onLog = (value, color, messageType) =>
		{
			MasterLog.Add(new LogEntry(value, color, messageType));
		};

		onWatch = (value) =>
		{
			if (!MasterWatch.Contains(value))
				MasterWatch.Add(value);
		};

		onUnwatch = (value) =>
		{
			while (MasterWatch.Contains(value))
				MasterWatch.Remove(value);
		};



		if (singleton == null)
		{
            singleton = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			LogWarning("There's already a singleton instance of CustomConsole. Making more will do weird crap, probably.");
        }




	}
	
	// Update is called once per frame
	void Update () {
	
		while(MasterLog.Count > maxLogSize)
		{
			MasterLog.RemoveRange(0, dumpSize);
			LogWarning("Master log is too big. Dropping the oldest " + dumpSize + " entries.");
		}
	}
}
