using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Log : MonoBehaviour
{
	Text log;
	AtractorMulti atr;
	void Awake()
	{
		log = GetComponent<Text>();
		atr = FindObjectOfType<AtractorMulti>();
		Application.targetFrameRate = 60;
	}
	IEnumerator Start()
	{
		var build = new System.Text.StringBuilder();
		while (true)
		{
			build.Append("current: ");
			build.Append(atr.count);
		//	build.Append("\nto emit: ");
			//build.Append(atr.emitCount);
			build.Append("\nfps: ");
			build.Append((int)(1f / Time.deltaTime));
			build.Append("\nmode: ");
			build.Append(atr.emitOption.ToString());
			log.text = build.ToString();
			build.Length = 0;
			yield return new WaitForSeconds(0.5f);
		}
	}
	private void Update()
	{
		//atr.emitCount += (int)(5f * Input.GetAxis("Horizontal"));
		//atr.startVel += (0.1f * Input.GetAxis("Vertical"));ç
		if (Input.GetKeyDown(KeyCode.O))
		{
			atr.emitOption += 1;
			atr.emitOption = (EmitOption)(((int)atr.emitOption) % System.Enum.GetNames(typeof(EmitOption)).Length);
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			atr.paused = !atr.paused;
		}
		if (Input.GetKeyDown(KeyCode.E))
		{
			atr.emit = true;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			atr.restart = true;
		}
		if (Input.GetKeyDown(KeyCode.T))
		{
			atr.useTrails = !atr.useTrails;
		}
		if (Input.GetKeyDown(KeyCode.F))
		{
			Application.targetFrameRate = Application.targetFrameRate == 0 ? 60 : 0;
		}
	}
}