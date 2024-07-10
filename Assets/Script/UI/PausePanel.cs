using UnityEngine;

public class PausePanel : MonoBehaviour
{
	private Canvas _panel;

	private void Awake()
	{
		_panel.enabled = false;
	}

	public void OnResumeButtonClick()
	{
		_panel.enabled = false;
		GetComponent<PlayerController>()?.ResumePlay();
	}

	public void ShowPanel()
	{
		_panel.enabled = true;
		GetComponent<PlayerController>()?.PausePlay();
	}
}
