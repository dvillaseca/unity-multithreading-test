using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ValueEditor : MonoBehaviour
{
	private IEnumerator Start()
	{
		while (!AtractorMulti.Instance)
			yield return null;
		var field = GetComponentsInChildren<InputField>();
		field[0].text = AtractorMulti.Instance.massRange.x.ToString();
		field[1].text = AtractorMulti.Instance.massRange.y.ToString();
		field[2].text = AtractorMulti.Instance.spawnRadius.ToString();
		field[3].text = AtractorMulti.Instance.emitCount.ToString();
		field[4].text = AtractorMulti.Instance.density.ToString();
	}
	public void OnMinMassEdit(InputField input)
	{
		string s = input.text;
		float v;
		if (float.TryParse(s, out v))
		{
			AtractorMulti.Instance.massRange.x = v;
		}
	}
	public void OnMaxMassEdit(InputField input)
	{
		string s = input.text;
		float v;
		if (float.TryParse(s, out v))
		{
			AtractorMulti.Instance.massRange.y = v;
		}
	}
	public void OnSpawnEdit(InputField input)
	{
		string s = input.text;
		float v;
		if (float.TryParse(s, out v))
		{
			AtractorMulti.Instance.spawnRadius = v;
		}
	}
	public void OnGravityEdit(InputField input)
	{
		string s = input.text;
		int v;
		if (int.TryParse(s, out v))
		{
			AtractorMulti.Instance.emitCount = v;
		}
	}
	public void OnDensityEdit(InputField input)
	{
		string s = input.text;
		float v;
		if (float.TryParse(s, out v))
		{
			AtractorMulti.Instance.density = v;
		}
	}
}