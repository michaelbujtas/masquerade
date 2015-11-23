using UnityEngine;
using System.Collections;

public class HeadlessConsole : MonoBehaviour {

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

	Windows.ConsoleWindow console = new Windows.ConsoleWindow();
	Windows.ConsoleInput input = new Windows.ConsoleInput();

	string strInput;
	void Awake()
	{
		if(SystemInfo.graphicsDeviceID == 0)
		{
			DontDestroyOnLoad(gameObject);

			console.Initialize();
			console.SetTitle("Rust Server");

			input.OnInputText += OnInputText;

			Application.RegisterLogCallback(HandleLog);

			Debug.Log("Console Started");
		}
	}

	void OnInputText(string obj)
	{
		//ConsoleSystem.Run(obj, true);
	}


	void HandleLog(string message, string stackTrace, LogType type)
	{
		if (type == LogType.Warning)
			System.Console.ForegroundColor = System.ConsoleColor.Yellow;
		else if (type == LogType.Error)
			System.Console.ForegroundColor = System.ConsoleColor.Red;
		else
			System.Console.ForegroundColor = System.ConsoleColor.White;

		// We're half way through typing something, so clear this line ..
		if (System.Console.CursorLeft != 0)
			input.ClearLine();

		System.Console.WriteLine(message);

		// If we were typing something re-add it.
		input.RedrawInputLine();
	}

	void Update()
	{
		input.Update();
	}

	//
	// It's important to call console.ShutDown in OnDestroy
	// because compiling will error out in the editor if you don't
	// because we redirected output. This sets it back to normal.
	//
	void OnDestroy()
	{
		console.Shutdown();
	}

#endif
}
