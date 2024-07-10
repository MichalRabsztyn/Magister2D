using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
	[SerializeField] private GameObject _pauseScreen;
	[SerializeField] private GameObject _summaryScreen;
	[SerializeField] private TextMeshProUGUI _summaryTitleText;

	private PlayerController _playerController;

	private void Start()
	{
		_pauseScreen.SetActive(false);
		_summaryScreen.SetActive(false);

		_playerController = GetComponent<PlayerController>();
	}

	public void PauseGame()
	{
		if (_summaryScreen.activeSelf)
			return;

		_pauseScreen.SetActive(true);
		Time.timeScale = 0;

		_playerController?.PausePlay();
	}

	public void ResumeGame()
	{
		if (_summaryScreen.activeSelf)
			return;

		_pauseScreen.SetActive(false);
		Time.timeScale = 1;

		_playerController?.ResumePlay();
	}

	public void ShowSummary(bool bSuccess)
	{
		if (_pauseScreen.activeSelf)
			return;

		_summaryScreen.SetActive(true);
		Time.timeScale = 0;

		_summaryTitleText.text = bSuccess ? "Wygrana!" : "Spróbuj ponownie!";
	}

	public void HideSummary()
	{
		if (_pauseScreen.activeSelf)
			return;

		_summaryScreen.SetActive(false);
		Time.timeScale = 1;
	}

	public void RestartGame()
	{
		Time.timeScale = 1;
		UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public void NextLevel()
	{
		Time.timeScale = 1;
		int LevelIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
		int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
		if (LevelIndex >= sceneCount - 1)
		{
			LevelIndex = 0;
		}
		else
		{
			LevelIndex++;
		}

		UnityEngine.SceneManagement.SceneManager.LoadScene(LevelIndex);
	}

	public void MainMenu()
	{
		Time.timeScale = 1;
		UnityEngine.SceneManagement.SceneManager.LoadScene(0);
	}
}
