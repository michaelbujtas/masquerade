using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;


public class NamedValueField : MonoBehaviour {

	public GameObject Visuals;
	public TextMeshProUGUI Name, Value;
	public Image Background;
	public Image SelectionHighlight;
	public string FloatFormatter = "G3";


	public void SetName(string name)
	{
		if(Name != null)
			Name.text = name;
	}

	public void SetValue(int value)
	{
		if (Value != null)
			Value.text = value.ToString();
	}

	public void SetValue(float value)
	{
		if (Value != null)

			Value.text = value.ToString(FloatFormatter);
	}

	public void SetColors(Color text, Color background)
	{
		if (Name != null)
			Name.color = text;
		if (Value != null)
			Value.color = text;

		if (Background != null)
			Background.color = background;
	}

	public void SetHighlight(Color highlight)
	{
		if (SelectionHighlight != null)
			SelectionHighlight.color = highlight;
	}

	public void ClearHighlight()
	{
		if (SelectionHighlight != null)
			SelectionHighlight.color = Color.clear;
	}

}
