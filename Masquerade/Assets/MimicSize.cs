using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class MimicSize : MonoBehaviour {

	public RectTransform target;

	public float scale = 1;
	public bool X;
	public bool Y;
	public bool Position;

	RectTransform rectTransform;

	void Start () {
		rectTransform = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
		rectTransform.sizeDelta = new Vector2(
			X ? target.sizeDelta.x * scale : rectTransform.sizeDelta.x,
			Y ? target.sizeDelta.y * scale : rectTransform.sizeDelta.y);

		if (Position)
			rectTransform.position = target.position;
	}
}
