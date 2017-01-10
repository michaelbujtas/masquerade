using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class SaveInputField : MonoBehaviour {

	public string Key;

	InputField inputField;
	void Awake()
	{
		inputField = GetComponent<InputField>();

		inputField.onEndEdit = new InputField.SubmitEvent();
		inputField.onEndEdit.AddListener(Save);

		if(PlayerPrefs.HasKey(Key))
		{
			inputField.text = PlayerPrefs.GetString(Key);
		}
	}

	void Save(string value)
	{
		PlayerPrefs.SetString(Key, value);
		PlayerPrefs.Save();
	}
}

