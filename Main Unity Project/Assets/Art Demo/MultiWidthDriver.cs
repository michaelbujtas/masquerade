using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class MultiWidthDriver : MonoBehaviour {

	public List<RectTransform> drivenTransforms = new List<RectTransform>();

	public float EdgePadding = 15;
	public float InternalPadding = 10;
	float PaddingBetweenMainAndBackup = 5;

	public float aspectRatio = 4.5f;

	public float height;
	public bool autoSize;

	public float HandHeight
	{
		get
		{
			return CardHeight * 2 + 5;
		}
	}

	public float CardHeight
	{
		get
		{
			return height;
		}
	}

	public float HandWidth
	{
		get
		{
			return height * aspectRatio;
		}
	}

	void AutoSize()
	{
		float shortEdge = Mathf.Min(Screen.height, Screen.width);
		float longEdge = Mathf.Max(Screen.height, Screen.width);

		float shortEdgeTargetHeight = (shortEdge - 2 * EdgePadding) / aspectRatio;
		//float shortEdgeTargetHeight = 10000;
		float middleStackTargetHeight = (Screen.height - 2 * EdgePadding - 2 * PaddingBetweenMainAndBackup - 2 * InternalPadding) / 5;
		float longEdgeTargetHeight = (longEdge - 2 * EdgePadding - 2 * PaddingBetweenMainAndBackup - 2 * InternalPadding) / (4 + aspectRatio);
		///float longEdgeTargetHeight = 10000;

		height = Mathf.Min(Mathf.Min(shortEdgeTargetHeight, middleStackTargetHeight), longEdgeTargetHeight);



	}

	void Update () {
		if(autoSize)
			AutoSize();

		foreach (RectTransform t in drivenTransforms)
		{
			t.sizeDelta = new Vector2(height * aspectRatio, height);
		}
			
	
	}


	//Whatever side is shortest is the side that won't have to worry about it's edges. It correctly assumes it has it's side to itself, and just uses margin padding
	//If it's the height (and it always is), there's another concern -- the middle column is five heights, which is more than a width, so we have to use that instead
	//The other side is a stack of four heights and a width. It uses both the margin padding and the internal padding.

}
