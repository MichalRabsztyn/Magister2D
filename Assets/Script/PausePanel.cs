using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;
using TMPro;

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
		GetComponent<MLAgentPlayerController>()?.ResumePlay();
		GetComponent<PlayerController>()?.ResumePlay();
	}

	public void ShowPanel()
	{
		_panel.enabled = true;
		GetComponent<MLAgentPlayerController>()?.PausePlay();
		GetComponent<PlayerController>()?.PausePlay();
	}
}
