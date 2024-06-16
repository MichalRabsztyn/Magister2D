using TMPro;
using UnityEngine;

public class Academy : MonoBehaviour
{
    [SerializeField] private GameObject _environmentPrefab;
    [SerializeField] private int _numberOfInstances = 1;
    [SerializeField] private float _environmentSpacingX = 50.0f;
    [SerializeField] private float _environmentSpacingY = 50.0f;

    [SerializeField] private TextMeshProUGUI _successText;
    [SerializeField] private TextMeshProUGUI _failText;

    private int _successScore = 0;
    private int _failScore = 0;

    private void Awake()
    {
        if (!_environmentPrefab || _numberOfInstances <= 0)
            return;

        int gridSizeX = Mathf.CeilToInt(Mathf.Sqrt(_numberOfInstances));
        int gridSizeY = Mathf.CeilToInt((float)_numberOfInstances / gridSizeX);

        Vector3 spawnPosition = transform.position;

        for (int i = 0; i < _numberOfInstances; i++)
        {
            float xOffset = (_environmentSpacingX * (i % gridSizeX));
            float yOffset = (_environmentSpacingY * (i / gridSizeX));

            Vector3 finalSpawnPosition = spawnPosition + new Vector3(xOffset, yOffset, 0f);

            GameObject Enviro = Instantiate(_environmentPrefab, finalSpawnPosition, Quaternion.identity, transform);
            foreach (Transform child in Enviro.transform)
            {
                Camera cameraComponent = child.GetComponentInChildren<Camera>();
                if (cameraComponent != null)
                {
                    cameraComponent.enabled = false;
                }
                AudioListener audioListener = child.GetComponentInChildren<AudioListener>();
                if (audioListener != null)
                {
                    audioListener.enabled = false;
                }
            }
        }
    }

    public void UpdateSuccess()
    {
        _successScore++;
        _successText.text = _successScore.ToString();
    }

    public void UpdateFail()
    {
        _failScore++;
        _failText.text = _failScore.ToString();
    }
}
