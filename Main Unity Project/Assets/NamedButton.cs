using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NamedButton : MonoBehaviour {
	public GameObject Visuals;
	public Text Name;
	public Image Background;
	public Image SelectionHighlight;
	public Button Button;


	public void SetName(string name)
	{
		Name.text = name;
	}

	public void SetColors(Color text, Color background)
	{
		Name.color = text;
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
