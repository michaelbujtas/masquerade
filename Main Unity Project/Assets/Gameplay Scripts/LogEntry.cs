using UnityEngine;

public class LogEntry
{
	public string Entry;
	public Color Color = Color.white;
	public ConsoleMessageType MessageType = ConsoleMessageType.DEFAULT;

	public LogEntry(string entry, Color color, ConsoleMessageType messageType)
	{
		Entry = entry;
		Color = color;
		MessageType = messageType;
	}

	public LogEntry()
	{
		Entry = "";
		Color = new Color(1, 1, 1);
		MessageType = ConsoleMessageType.DEFAULT;
	}

}