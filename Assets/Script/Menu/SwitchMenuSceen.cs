using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchMenuSceen : MonoBehaviour
{
    [SerializeField]
    private GameObject _mainMenuView;
    [SerializeField]
    private GameObject _levelsView;
    [SerializeField]
    private GameObject _scoreView;

    private void Awake()
    {
        SwitchToMainMenu();
    }

    public void SwitchToMainMenu()
    {
        _mainMenuView.SetActive(true);
        _levelsView.SetActive(false);
        _scoreView.SetActive(false);
    }

    public void SwitchToLevels()
    {
        _mainMenuView.SetActive(false);
        _levelsView.SetActive(true);
        _scoreView.SetActive(false);
    }

    public void SwitchToScore()
    {
        _mainMenuView.SetActive(false);
        _levelsView.SetActive(false);
        _scoreView.SetActive(true);

        FindObjectOfType<ScoreDatabase>()?.ReadScore();
    }
}
