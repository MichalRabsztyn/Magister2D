using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.SceneManagement;

public class SummaryScreen : MonoBehaviour
{
	[SerializeField] private Canvas _summaryScreen;
	[SerializeField] private TextMeshProUGUI _summaryTitleText;
	private void Awake()
	{
		_summaryScreen.enabled = false;
	}

	public void ShowSummaryScreen(bool bSuccess)
	{
		_summaryScreen.enabled = true;
		GetComponent<MLAgentPlayerController>()?.PausePlay();
		GetComponent<PlayerController>()?.PausePlay();
		_summaryTitleText.text = bSuccess ? "Wygrana!" : "Spróbuj ponownie!";
	}

	public void OnTryAgainButtonClick()
	{
		_summaryScreen.enabled = false;
		GetComponent<MLAgentPlayerController>()?.ResumePlay();
		GetComponent<PlayerController>()?.ResumePlay();
	}

	public void OnNextLevelButtonClick()
	{
		_summaryScreen.enabled = false;
		int LevelIndex = SceneManager.GetActiveScene().buildIndex;
		int sceneCount = SceneManager.sceneCountInBuildSettings;
		if (LevelIndex >= sceneCount - 1)
		{
			LevelIndex = 0;
		}
		else
		{
			LevelIndex++;
		}

		SceneManager.LoadScene(LevelIndex);
	}

	public void OnMainMenuButtonClick()
	{
		_summaryScreen.enabled = false;
		SceneManager.LoadScene(0);
	}
}
