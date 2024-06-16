using UnityEngine;

public class Exit : MonoBehaviour
{
    public void ExitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif //UNITY_EDITOR
    }
}
