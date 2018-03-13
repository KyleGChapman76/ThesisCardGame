using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeTotalRenderer : MonoBehaviour
{
	public Text localLifeTotalText;

	public int highHealthLowerBound;
	public Color highHealthColor;

	public int mediumHealthLowerBound;
	public Color mediumHealthColor;

	public Color lowHealthColor;

	public void RenderLifeTotal(int lifeTotal)
	{
		if (localLifeTotalText == null)
		{
			Debug.LogError("Can't render life total cause don't have reference to life total text.");
			return;
		}
		else
		{
			//Debug.Log("Rendering " + lifeTotal.ToString() + " lifetotal.");
		}

		localLifeTotalText.text = lifeTotal.ToString();

		if (lifeTotal > highHealthLowerBound)
		{
			localLifeTotalText.color = highHealthColor;
		}
		else if (lifeTotal > mediumHealthLowerBound)
		{
			localLifeTotalText.color = mediumHealthColor;
		}
		else
		{
			localLifeTotalText.color = lowHealthColor;
		}
    }
}
