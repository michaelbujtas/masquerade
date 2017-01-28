using UnityEngine;
using System.Collections;
using UnityEngine.UI;


[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class AnchorNudger : MonoBehaviour {


	public bool zeroOffset = false;
	public bool centerHorizontally = false;
	public bool centerVertically = false;
	public bool flipHorizontally = false;
	public bool setAspect = false;
	public float toRatio = 1;

	public float panSpeed = 1;
	public Vector2 pan;

	[System.NonSerialized]
	private RectTransform m_Rect;

	private RectTransform rectTransform
	{
		get
		{
			if (m_Rect == null)
				m_Rect = GetComponent<RectTransform>();
			return m_Rect;
		}
	}

	// Use this for initialization
	void Start () {

#if !UNITY_EDITOR
		enabled = false;
#endif
	}
	
	// Update is called once per frame
	void Update () {
		//rectTransform.anchorMin = new Vector2(.1f, .337f);
		if (centerVertically)
		{
			float anchorHeight = rectTransform.anchorMax.x - rectTransform.anchorMin.x;
			rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, 0.5f - (anchorHeight / 2));
			rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x , 0.5f + (anchorHeight / 2));


		}
		if (centerHorizontally)
		{
			float anchorWidth = rectTransform.anchorMax.x - rectTransform.anchorMin.x;
			rectTransform.anchorMin = new Vector2(0.5f - (anchorWidth / 2), rectTransform.anchorMin.y);
			rectTransform.anchorMax = new Vector2(0.5f + (anchorWidth / 2), rectTransform.anchorMax.y);


		}
		if (flipHorizontally)
		{
			flipHorizontally = false;
			
			Vector2 oldMin = rectTransform.anchorMin;
			Vector2 oldMax = rectTransform.anchorMax;

			rectTransform.anchorMin = new Vector2(1 - oldMax.x, oldMin.y);
			rectTransform.anchorMax = new Vector2(1 - oldMin.x, oldMax.y);


		}
		if(setAspect)
		{
			setAspect = false;
			float anchorWidth = rectTransform.anchorMax.x - rectTransform.anchorMin.x;
			float anchorHeight = anchorWidth * toRatio;
			float currentCenter = (rectTransform.anchorMax.y + rectTransform.anchorMin.y) / 2;
			rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, currentCenter - (anchorHeight / 2));
			rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, currentCenter + (anchorHeight / 2));
		}
		if(zeroOffset)
		{
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.offsetMin = Vector2.zero;

		}
		if(pan != Vector2.zero)
		{
			Vector2 adjust = pan * (panSpeed / 100);

			rectTransform.anchorMin += adjust;
			rectTransform.anchorMax += adjust;

			pan = Vector2.zero;
		}

	}

	private Vector2 GetParentSize()
	{
		RectTransform parent = rectTransform.parent as RectTransform;
		if (!parent)
			return Vector2.zero;
		return parent.rect.size;
	}
}
