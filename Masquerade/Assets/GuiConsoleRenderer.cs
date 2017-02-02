using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GuiConsoleRenderer : MonoBehaviour {
	public Text output;
	public Scrollbar scrollBar;

	public int totalLines = 50;


	CustomConsole console;
	// Use this for initialization
	void Start () {
		console = FindObjectOfType<CustomConsole>();
		
		console.onLog += (entry, color, messageType) =>
		{
			string newText = "";
			for (int i = totalLines; i > 0; i--)
			{

				int nextLine = console.MasterLog.Count - i;

				if (nextLine >= 0 && nextLine < console.MasterLog.Count)
					newText += FormatEntry(console.MasterLog[nextLine]);
			}
			output.text = newText;
		};


	}
	
	// Update is called once per frame
	void Update () {
	
	}
	

	

	string FormatEntry(LogEntry entry)
	{

		string colorString = ColorToHex(entry.Color);

		return "<color=#" + colorString + ">" + entry.Entry + "</color>\n";
	}

	void OnDrag(UnityEngine.EventSystems.PointerEventData data)
	{

	}

	public static string ColorToHex(Color color)
	{
		string hex = "0123456789ABCDEF";
		int r = (int)(color.r * 255);
		int g = (int)(color.g * 255);
		int b = (int)(color.b * 255);

		return hex[(int)(Mathf.Floor(r / 16))].ToString() + hex[(int)(Mathf.Round(r % 16))].ToString() +
			hex[(int)(Mathf.Floor(g / 16))].ToString() + hex[(int)(Mathf.Round(g % 16))].ToString() +
			hex[(int)(Mathf.Floor(b / 16))].ToString() + hex[(int)(Mathf.Round(b % 16))].ToString();
	}
}
