using UnityEngine;

public class Goal : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			Component[] components = other.GetComponents<Component>();
			foreach (Component component in components)
			{
				if (component is IPlayerController PlayerControllerInterface)
				{
					PlayerControllerInterface.GotToEnd();
					return;
				}
			}
		}
	}
}
