using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DevConsole;

namespace DevConsole{
	[System.Serializable]
	public class Console : MonoBehaviour {
		
		//=============================
		//VARS
		//=============================
		[SerializeField]
		float fadeSpeed = 500f;							//Pixels/sec at which console fades in & out
		const int TEXT_AREA_OFFSET = 7;					//Margin for the text at the bottom of the console
		bool helpEnabled = true;						//Is the help for autocompletion on?
		int numHelpCommandsToShow = 5;					//Max candidates to show on autocompletion
		float helpWindowWidth = 200;					//The width of the autocompletion window
		const int WARNING_THRESHOLD = 15000;			//Number of characters in consoleText at which a warning appears
		const int DANGER_THRESHOLD = 16000;				//Number of characters in consoleText at which a danger appears	
		const int AUTOCLEAR_THRESHOLD = 18000;			//At this many characters, a DC_CLEAR will be done automatically
		

		List<CommandBase> consoleCommands;					//Whole list of commands available				
		List<string> candidates = new List<string>();	//Commands that match existing text
		int selectedCandidate = 0;						//Index of candidate selected
		List<string> history = new List<string>();		//A history of texts sent into the console
		int selectedHistory = 0;						//Current index in the history
		List<KeyValuePair<string, string>> buffer = 
			new List<KeyValuePair<string, string>>();	//Messages buffer. Stored in order to print them on the next OnGUI update

		[SerializeField]
		KeyCode consoleKey = KeyCode.Backslash;			//Key to open/close Console
		[SerializeField]
		GUISkin skin;									//GUISkin to use
		[SerializeField]
		Font font;										//Font
		[SerializeField]
		bool dontDestroyOnLoad;							//Sets whether it destroys on load or not

		static Console _singleton;						//Singleton
		static Console Singleton{						
			get{
				if (_singleton==null)
					_singleton = GameObject.FindObjectOfType<Console>();
				return _singleton;
			}
		}					

		bool opening;									//Can write already?
		bool closed = true;								//Is the Console closed?
		bool showHelp = true;							//Should Help Window show?
		bool inHistory = false;							//Are we browsing the history?
		bool showTimeStamp = false;						//Should time stamps be shown?
		
		float numLinesThreshold;						//Max numes allowed in the console
		float maxConsoleHeight; 						//Screen.height/3
		
		float currentConsoleHeight;						//Current Y position in pixels
		Vector2 consoleScroll = Vector2.zero;			
		Vector2 helpWindowScroll = Vector2.zero;

		string consoleText = string.Empty;				//Text in the whole console log
		string inputText = string.Empty;				//Text in the input TextField
		string lastText = string.Empty;					//Used for history mostly
		int numLines;									//Number of '\n' in consoleText
		bool _verbose = true;							//Used to check for extra logs
		public static bool verbose{get{return Singleton._verbose;}set{Singleton._verbose = value;}}
		
		//=============================
		//AWAKE
		//=============================
		void Awake(){
			if (Singleton != this){
				Debug.LogWarning("There can only be one Console per project");
				Destroy(this); 
				return;
			}
			if (dontDestroyOnLoad)
				DontDestroyOnLoad(gameObject);
		}

		void OnEnable(){
			if (consoleCommands == null)
				consoleCommands = new List<CommandBase>();
			AddCommands(
				new Command("DC_HELP",HelpCommand,"Shows a list of all Commands available"),
				new Command("DC_INFO",ShowInfo, "Shows info about the DevConsole"),
				new Command("DC_CLEAR",Clear, "Clears the console"),
				new Command<string>("DC_CHANGE_KEY",ChangeKey, ChangeKeyHelp),
				new Command<bool>("DC_SHOW_TIMESTAMP",ShowTimeStamp, "Establishes whether or not to show the time stamp for each command"),
				new Command<bool>("DC_SHOW_VERBOSE", ShowVerbose, "Sets true or false the property Verbose"),
				new Command<bool>("showDebugLog", ShowLog, "Establishes whether or not to show Unity Debug Log"),
				new Command<object>("debugLog", Debug.Log, "Logs some text to the Unity Console"),
				new Command<object>("debugWarning", Debug.LogWarning, "Logs some warning text to the Unity Console"),
				new Command<object>("debugError", Debug.LogError, "Logs some error text to the Unity Console"),
				new Command("exit",Application.Quit,"Exits the game"),
				new Command("quit",Application.Quit,"Exits the game"),
				new Command<int>("Application.LoadLevelI",Application.LoadLevel, "Loads a level based on an integer value"),
				new Command<string>("Application.LoadLevelS", Application.LoadLevel, "Loads a level based on an string value")
			);
		}
		void Prueba(string s){
			Debug.Log (s);
		}
		//=============================
		//GUI
		//=============================
		void OnGUI(){
			if (skin != null && GUI.skin != skin)
				GUI.skin = skin;
			if (consoleKey == KeyCode.None)
				return;

			Event current = Event.current;
			GUI.skin.textArea.richText = true;
			if (font != null)
				GUI.skin.font = font;
			
			//Open/Close Console
			if (current.type == EventType.KeyDown && current.keyCode == consoleKey){
				opening = !opening;
				GUIUtility.keyboardControl = 0;
				StartCoroutine(FadeInOut(opening));
			}
			
			//Local declarations
			bool moving = !((currentConsoleHeight == maxConsoleHeight) || (currentConsoleHeight == 0));
			float lineHeight = GUI.skin.textArea.lineHeight;
			float height = lineHeight*numLines;
			float scrollHeight = height>currentConsoleHeight?height:currentConsoleHeight;

			if (!closed){
				//Treat buffer
				for (int i = 0; i< buffer.Count; i++)
					BasePrintOnGUI(buffer[i].Key, buffer[i].Value);
				buffer.Clear();

				if (!moving)
					GUI.FocusControl("TextField");
				//KEYS
				if (current.type == EventType.keyDown){
					if (!string.IsNullOrEmpty(inputText)){
						switch(current.keyCode){
						case KeyCode.Return:
							PrintInput(inputText);
							break;
						case KeyCode.Tab:
							if (candidates.Count !=0){
								inputText = candidates[selectedCandidate];
								showHelp = false;
								SetCursorPos(inputText, inputText.Length);
								candidates.Clear();
							}
							break;
						case KeyCode.Escape:
							showHelp = false;
							candidates.Clear();
							break;
						case KeyCode.F1:
							showHelp = true;
							break;
						}
					}
					switch(current.keyCode){
					case KeyCode.UpArrow:
						if ((inHistory || inputText == string.Empty) && history.Count != 0){
							selectedHistory = Mathf.Clamp(selectedHistory+(inHistory?1:0), 0, history.Count-1);
							inputText = history[selectedHistory];
							showHelp = false;
							inHistory = true;
							lastText = inputText;
						}
						else if (inputText != string.Empty && !inHistory){
							selectedCandidate = Mathf.Clamp(--selectedCandidate,0, candidates.Count-1);
							if (selectedCandidate*lineHeight <= helpWindowScroll.y ||
							    selectedCandidate*lineHeight > helpWindowScroll.y + lineHeight*(numHelpCommandsToShow-1))	 
								helpWindowScroll = new Vector2(0,selectedCandidate*lineHeight-1*lineHeight);
						}
						SetCursorPos(inputText, inputText.Length);
						break;
					case KeyCode.DownArrow:
						if ((inHistory || inputText == string.Empty) && history.Count != 0){
							selectedHistory = Mathf.Clamp(selectedHistory-(inHistory?1:0), 0, history.Count-1);
							inputText = history[selectedHistory];
							showHelp = false;
							inHistory = true;
							lastText = inputText;
						}
						else if (inputText != string.Empty && !inHistory){
							selectedCandidate = Mathf.Clamp(++selectedCandidate,0, candidates.Count-1);
							if (selectedCandidate*lineHeight > helpWindowScroll.y + lineHeight*(numHelpCommandsToShow-2) ||
							    selectedCandidate*lineHeight < helpWindowScroll.y)
								helpWindowScroll = new Vector2(0, selectedCandidate*lineHeight-((numHelpCommandsToShow-2)*lineHeight));
						}
						SetCursorPos(inputText, inputText.Length);
						break;
					}
				}
				
				if (lastText != inputText){
					inHistory = false;
					lastText = string.Empty;
				}
				//CONSOLE PAINTING
				GUI.Box(new Rect(0,0,Screen.width, currentConsoleHeight), new GUIContent());
				GUI.SetNextControlName("TextField");
				inputText = GUI.TextField(new Rect(0, currentConsoleHeight + 0, Screen.width, 25), inputText);
				GUI.skin.textArea.normal.background = null;
				GUI.skin.textArea.hover.background = null;
				consoleScroll = GUI.BeginScrollView(new Rect(0,0,Screen.width, currentConsoleHeight),consoleScroll,
				                                    new Rect(0, 0, Screen.width-20, scrollHeight));
				GUI.TextArea(new Rect(0, -5+currentConsoleHeight-0-(numLines==0?-0+lineHeight:height)+
				                      (numLines>=numLinesThreshold-1?lineHeight*(numLines-numLinesThreshold):0), 
				                      Screen.width,TEXT_AREA_OFFSET+(numLines==0?lineHeight:height)), consoleText);
				GUI.EndScrollView();
				if (inputText == string.Empty)
					showHelp= true;
			}

			//HELP WINDOW
			if (showHelp && helpEnabled && inputText.Trim ()!=string.Empty){
				ShowHelp();
				if (candidates.Count != 0){
					string help = string.Empty;
					foreach(string s in candidates)
						help+= (candidates[selectedCandidate]==s?"<color=yellow>"+s+"</color>":s)+'\n';
					GUI.skin.textArea.normal.background = GUI.skin.textField.normal.background;
					GUI.skin.textArea.hover.background = GUI.skin.textField.hover.background;
					if (candidates.Count > numHelpCommandsToShow){
						helpWindowScroll = GUI.BeginScrollView(new Rect(0,currentConsoleHeight-numHelpCommandsToShow*lineHeight-TEXT_AREA_OFFSET,
	                                                helpWindowWidth,5+lineHeight*numHelpCommandsToShow), helpWindowScroll,
						                                       new Rect(0, 0,helpWindowWidth-20,TEXT_AREA_OFFSET + candidates.Count*lineHeight));
						GUI.TextArea(new Rect(0, 0, helpWindowWidth, TEXT_AREA_OFFSET + candidates.Count*lineHeight),help);
						GUI.EndScrollView();
					}
					else
						GUI.TextArea(new Rect(0,currentConsoleHeight-TEXT_AREA_OFFSET-
					                      (candidates.Count > numHelpCommandsToShow?numHelpCommandsToShow*lineHeight:
                               				lineHeight*candidates.Count),helpWindowWidth,
			                     		 (candidates.Count > numHelpCommandsToShow?numHelpCommandsToShow*lineHeight:
					 						lineHeight*candidates.Count)+TEXT_AREA_OFFSET),help);
				}
			}
			
		}
		
		//=============================
		//OTHERS
		//=============================
		IEnumerator FadeInOut(bool opening){
			maxConsoleHeight = Screen.height/3;
			numLinesThreshold = maxConsoleHeight/GUI.skin.textArea.lineHeight;
			closed = false;
			do{
				if (opening)
					currentConsoleHeight = Mathf.Min(currentConsoleHeight+fadeSpeed*Time.deltaTime, maxConsoleHeight);
				else
					currentConsoleHeight = Mathf.Max(currentConsoleHeight-fadeSpeed*Time.deltaTime,0);
				if (currentConsoleHeight == 0 || currentConsoleHeight == maxConsoleHeight)
					opening = !opening;
				yield return null;
			}while (opening == this.opening);
			if (currentConsoleHeight == 0)
				closed = true;
			if (closed)
				inputText =string.Empty;
		}
		
		void ShowHelp(){
			string aux = string.Empty;
			if (candidates.Count != 0 && selectedCandidate >= 0 && candidates.Count > selectedCandidate)
				aux = candidates[selectedCandidate];
			candidates.Clear();
			for (int i = 0; i < consoleCommands.Count; i++){
				if(consoleCommands[i].name.ToUpper().StartsWith(inputText.ToUpper()))
					candidates.Add(consoleCommands[i].name);
			}
			if (aux == string.Empty){
				selectedCandidate = 0;
				return;
			}
			for(int i = 0; i < candidates.Count; i++){
				if (candidates[i] == aux){
					selectedCandidate = i;
					return;
				}
			}
			selectedCandidate = 0;
		}

		#region Tools
		void SetCursorPos(string text, int pos){
			TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor),GUIUtility.keyboardControl);
			te.content.text = text;
			te.cursorIndex = pos;
			te.selectIndex = pos;
		}

		public static string ColorToHex(Color color){
			string hex ="0123456789ABCDEF";
			int r = (int)(color.r*255);
			int g = (int)(color.g*255);
			int b = (int)(color.b*255);

			return	hex[(int)(Mathf.Floor(r/16))].ToString()+hex[(int)(Mathf.Round(r%16))].ToString()+
				hex[(int)(Mathf.Floor(g/16))].ToString()+hex[(int)(Mathf.Round(g%16))].ToString()+
				hex[(int)(Mathf.Floor(b/16))].ToString()+hex[(int)(Mathf.Round(b%16))].ToString();
		}

		#endregion
		//=============================
		//PRINTS
		//=============================
		#region Logs
		/// <summary>
		/// Logs a white text to the console.
		/// </summary>
		/// <param name="text">Text to be sent.</param>
		public static void Log(string text){
			Singleton.BasePrint(text);
		}
		public static void Log(object obj){
			Log (obj.ToString());
		}
		/// <summary>
		/// Logs a ligh blue text to the console.
		/// </summary>
		/// <param name="text">Text to be sent.</param>
		public static void LogInfo(string text){
			Singleton.BasePrint(text, Color.cyan);
		}
		public static void LogInfo(object obj){
			LogInfo (obj.ToString());
		}
		/// <summary>
		/// Logs a yellow text to the console.
		/// </summary>
		/// <param name="text">Text to be sent.</param>
		public static void LogWarning(string text){
			Singleton.BasePrint(text, Color.yellow);
		}
		public static void LogWarning(object obj){
			LogWarning (obj.ToString());
		}
		/// <summary>
		/// Logs a red text to the console.
		/// </summary>
		/// <param name="text">Text to be sent.</param>
		public static void LogError(string text){
			Singleton.BasePrint(text, Color.red);
		}
		public static void LogError(object obj){
			LogError (obj.ToString());
		}
		/// <summary>
		/// Logs a text to the console with the specified color.
		/// </summary>
		/// <param name="text">Text to be sent.</param>
		/// <param name="color">Color provided in HTML format.</param>
		public static void Log(string text, string color){
			Singleton.BasePrint(text, color);
		}
		public static void Log(object obj, string color){
			Log(obj.ToString(), color);
		}
		/// <summary>
		/// Logs a text to the console with the specified color.
		/// </summary>
		/// <param name="text">Text to be sent.</param>
		/// <param name="color">Color to be used.</param>
		public static void Log(string text, Color color){
			Singleton.BasePrint(text,color);
		}
		public static void Log(object obj, Color color){
			Log(obj.ToString(), color);
		}
		#endregion
		#region Prints
		void BasePrint(string text){
			BasePrint(text, ColorToHex(Color.white));
		}
		void BasePrint(string text, Color color){
			BasePrint(text, ColorToHex(color));	
		}
		void BasePrint(string text, string color){
			buffer.Add(new KeyValuePair<string, string>(text, color));
		}
		//In case print is called from another thread, the action is cached in a buffer and processed in the next OnGUI update
		void BasePrintOnGUI(string text, string color){
			text = "> " + text;
			int numLineJumps = 1;
			GUIContent lastLine = new GUIContent((showTimeStamp?"["+System.DateTime.Now.ToShortTimeString()+"]  ":string.Empty));
			for (int i = 0; i < text.Length; i++){
				if (text[i] == '\n')
					numLineJumps++;
				else
					lastLine.text+=text[i];
				if (GUI.skin.textArea.CalcSize(lastLine).x>Screen.width-20){
					text = text.Insert(i, "\n");
					i--;
					lastLine.text = (showTimeStamp?"["+System.DateTime.Now.ToShortTimeString()+"]  ":string.Empty);
				}
			}
			text += '\n';
			numLines+= numLineJumps;
			if (numLines >=numLinesThreshold-1)
				consoleScroll = new Vector2(0,consoleScroll.y+int.MaxValue);
			AddText(text, color);
			if (consoleText.Length>=AUTOCLEAR_THRESHOLD){
				Clear();
				AddText("Buffer cleared automatically\n", ColorToHex(Color.yellow));
			}
			else if (consoleText.Length>=DANGER_THRESHOLD)
				AddText("Buffer size too large. You should clear the console\n", ColorToHex(Color.red));
			else if (consoleText.Length>=WARNING_THRESHOLD)
				AddText("Buffer size too large. You should clear the console\n", ColorToHex(Color.yellow));
		}
		void AddText(string text, string color){
			consoleText += (showTimeStamp?"["+System.DateTime.Now.ToShortTimeString()+"]  ":string.Empty)+ "<color=#"+color+">"+text+"</color>";
		}
		
		void PrintInput(string input){
			inputText = string.Empty;
			if ((history.Count == 0 || history[0] != input) && input.Trim() != string.Empty)
				history.Insert(0,input);
			selectedHistory = 0;
			BasePrint(input);
			ExecuteCommandInternal(input);
		}

		void LogCallback(string log, string stackTrace, LogType type){
			Color color;
			if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
				color = Color.red;
			else if (type == LogType.Warning)
				color = Color.yellow;
			else// if (type == LogType.Log)
				color = Color.cyan;
			BasePrint(log, color);
			BasePrint(stackTrace, color);

			int length =(int) GUI.skin.textArea.CalcSize(new GUIContent(log)).x;
			while (length>= Screen.width){
				length-=Screen.width;
				numLines++;
			}
			numLines++;
		}
		#endregion
		//===========================
		//COMMANDS
		//===========================
		#region Manage Commands
		public static void ExecuteCommand(string command){
			Singleton.ExecuteCommandInternal(command);
		}
		public static void ExecuteCommand(string command, string args){
			Singleton.ExecuteCommandInternal(command+" "+args);
		}
		void ExecuteCommandInternal(string command){
			for (int i = 0; i < consoleCommands.Count; i++){
				if (command.ToUpper().StartsWith(consoleCommands[i].name.ToUpper())){
					if (command.ToUpper() == consoleCommands[i].name.ToUpper() + "?")
						consoleCommands[i].ShowHelp();
					else
						consoleCommands[i].Execute(command.Substring(
							consoleCommands[i].name.Length+(command.Contains(" ")?1:0)));
				}
			}
		}
		public static void AddCommands(params CommandBase[] cs){
			foreach(CommandBase c in cs)
				AddCommand(c);
		}
		public static void AddCommand(CommandBase c){
			if (!CommandExists(c.name))
				Singleton.consoleCommands.Add (c);
		}
		static bool CommandExists(string commandName){
			foreach(CommandBase c in Singleton.consoleCommands){
				if (c.name.ToUpper() == commandName.ToUpper()){
					LogError("The command " + commandName + " already exists");
					return true;
				}
			}
			return false;
		}
		public static void RemoveCommand(string commandName){
			foreach(CommandBase c in Singleton.consoleCommands){
				if (c.name == commandName){
					Singleton.consoleCommands.Remove(c);
					Log("Command " + commandName + " removed successfully", Color.green);
					return;
				}
			}
			LogWarning("The command " + commandName + " could not be found");
		}
		#endregion
		#region Predefined Commands
		void HelpCommand(){ 
			string text = string.Empty;
			for (int i = 0; i < consoleCommands.Count; i++){
				text+=consoleCommands[i].name+(consoleCommands[i].helpText == null?"":": "+consoleCommands[i].helpText);
				text+='\n';
			}
			LogInfo("\n"+text);
		}
		void Clear(){
			Singleton.consoleText = string.Empty;
			Singleton.numLines = 0;
		}
		void ShowVerbose(bool show){
			verbose = show;
			Log("Changed Succesful", Color.green);
		}
		void ChangeKey(string key){
			//If not a number
			int n;
			if (!int.TryParse(key, out n)){
				try{
					Singleton.consoleKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), key, true);
					Log("Change successful", Color.green);
				}catch{
					LogError("The entered value is not a valid KeyCode value");
				}
			}
			else{
				string[] keyCodes = System.Enum.GetNames(typeof(KeyCode));
				if (n>=0 || n<keyCodes.Length){
					try{
						Singleton.consoleKey = (KeyCode)System.Enum.Parse(typeof(KeyCode),keyCodes[n], true);
						Log("Change successful", Color.green);
					}catch{
						LogError("The entered value is not a valid KeyCode value");
					}
				}
				else
					LogError("The entered value is not a valid KeyCode value");
			}
		}
		void ChangeKeyHelp(){
			string[] keyCodes = System.Enum.GetNames(typeof(KeyCode));
			string help = "\nSPECIAL KEYS 1: ";
			int lineLength = 0;
			for(int i = 0; i < keyCodes.Length; i++){
				string text = string.Empty;
				if (i == 22){
					text = "\n\nNUMERIC KEYS: ";
					lineLength = 0;
				}
				else if (i == 32){
					text = "\n\nSPECIAL KEYS 2: ";
					lineLength = 0;
				}
				else if (i == 45){
					text = "\n\nALPHA KEYS: ";
					lineLength = 0;
				}
				else if (i == 71){
					text = "\n\nKEYPAD KEYS: ";
					lineLength = 0;
				}
				else if (i == 89){
					text = "\n\nSPECIAL KEYS 3: ";
					lineLength = 0;
				}
				else if (i == 98){
					text = "\n\nF KEYS: ";
					lineLength = 0;
				}
				else if (i == 113){
					text = "\n\nSPECIAL KEYS 4: ";
					lineLength = 0;
				}
				else if (i == 134){
					text = "\n\nMOUSE: ";
					lineLength = 0;
				}
				else if (i == 141){
					text = "\n\nJOYSTICK KEYS: ";
					lineLength = 0;
				}
				text+= keyCodes[i]+"["+i+"]"+(i!=keyCodes.Length-1?",":"");
				lineLength+=text.Length;
				help+=text;
				if (lineLength>= 65){
					help+='\n';
					lineLength = 0;
				}
			}
			LogInfo("Command Info: " + help);
		}

		void ShowLog(bool value){
			if (value)
				Application.logMessageReceived+=LogCallback;
			else
				Application.logMessageReceived-=LogCallback; 
			Log("Change successful", Color.green);
		}
		void ShowTimeStamp(bool value){
			showTimeStamp = value;
			Log("Change successful", Color.green);
		}
		void ShowInfo(){
			string text = "DevConsole by CobsTech \n" +
				"Version 2.0\n" +
				"Contact/Support: antoniocogo@gmail.com\n+" +
				"More updates soon";
			Console.LogInfo (text);
		}
		#endregion
	}
}