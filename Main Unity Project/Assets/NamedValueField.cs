using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class NamedValueField : MonoBehaviour {

	public GameObject Visuals;
	public Text Name, Value;
	public Image Background;
	public Image SelectionHighlight;
	public string FloatFormatter = "G3";


	public void SetName(string name)
	{
		Name.text = name;
	}

	public void SetValue(int value)
	{
		Value.text = value.ToString();
	}

	public void SetValue(float value)
	{
		Value.text = value.ToString(FloatFormatter);
	}

	public void SetColors(Color text, Color background)
	{
		Name.color = text;
		Value.color = text;
		Background.color = background;
	}

	public void SetHighlight(Color highlight)
	{
		SelectionHighlight.color = highlight;
	}

	public void ClearHighlight()
	{
		SelectionHighlight.color = Color.clear;
	}

}
