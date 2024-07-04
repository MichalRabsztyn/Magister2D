using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayLevel : MonoBehaviour
{
	public void OpenLevel(int levelID)
	{
		SceneManager.LoadScene(levelID);
	}
}
