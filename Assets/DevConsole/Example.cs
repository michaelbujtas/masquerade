using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DevConsole;

public class Example:MonoBehaviour {

	void Start(){
		Console.AddCommand(new Command<string>("TIME_TIMESCALE", TimeScale));
		Console.AddCommand(new Command<string>("TIME_SHOWTIME", ShowTime));
		Console.AddCommand(new Command<string>("PHYSICS_GRAVITY_X", XGravity));
		Console.AddCommand(new Command<string>("PHYSICS_GRAVITY_Y", YGravity));
		Console.AddCommand(new Command<string>("PHYSICS_GRAVITY_Z", ZGravity));
		Console.AddCommand(new Command<string>("EXAMPLE_HELP",ExampleCommand, ExampleCommandHelp));
	}
	//=============FUNCTIONS=================
    static void ExampleCommand(string args){
		Console.Log("Type EXAMPLE_HELP? to use this command");
	}
	static void ExampleCommandHelp(){
		string uncoloredText = "The help for this command is shown through a custom function";
		string coloredText = string.Empty;
		for(int i = 0; i < uncoloredText.Length; i++){
			Color c = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
			coloredText+= "<color=#"+Console.ColorToHex(c)+">"+uncoloredText[i]+"</color>";
		}
		Console.Log(coloredText);
	}
	static void TimeScale(string sValue){
		float fValue;
		if (float.TryParse(sValue, out fValue)){
			Time.timeScale = fValue;
			Console.Log("Change successful", Color.green);
		}
		else
			Console.LogError("The entered value is not a valid float value");
	}
	static void ShowTime(string args){
		Console.Log(Time.time.ToString());
	}

	static void XGravity(string sValue){
		float fValue;
		if (float.TryParse(sValue, out fValue)){
			Physics.gravity = new Vector3(fValue,Physics.gravity.y,  Physics.gravity.z);
			Console.Log("Change successful", Color.green);
		}
		else
			Console.LogError("The entered value is not a valid float value");
	}
	static void YGravity(string sValue){
		float fValue;
		if (float.TryParse(sValue, out fValue)){
			Physics.gravity = new Vector3(Physics.gravity.x, fValue, Physics.gravity.z);
			Console.Log("Change successful", Color.green);
		}
		else
			Console.LogError("The entered value is not a valid float value");
	}
	static void ZGravity(string sValue){
		float fValue;
		if (float.TryParse(sValue, out fValue)){
			Physics.gravity = new Vector3(Physics.gravity.x, Physics.gravity.y, fValue);
			Console.Log("Change successful", Color.green);
		}
		else
			Console.LogError("The entered value is not a valid float value");
	}
}