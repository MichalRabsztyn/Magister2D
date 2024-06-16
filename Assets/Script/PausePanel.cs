using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;

public class PausePanel : MonoBehaviour
{
	public Canvas _panel;
	private void Awake()
	{
		_panel.enabled = false;
	}

	public void OnButtonClick()
	{
		_panel.enabled = false;
		GetComponent<MLAgentPlayerController>().ResumePlay();
	}

	public void ShowPanel()
	{
		_panel.enabled = true;
		GetComponent<MLAgentPlayerController>().PausePlay();
	}
}
