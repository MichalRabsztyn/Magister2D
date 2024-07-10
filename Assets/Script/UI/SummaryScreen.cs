using UnityEngine;
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
		GetComponent<PlayerController>()?.PausePlay();
		_summaryTitleText.text = bSuccess ? "Wygrana!" : "Spr�buj ponownie!";
	}

	public void OnTryAgainButtonClick()
	{
		_summaryScreen.enabled = false;
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
