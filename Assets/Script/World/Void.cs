using UnityEngine;

public class Void : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			Component[] components = other.GetComponents<Component>();
			foreach (Component component in components)
			{
				if (component is IPlayerController myInterface)
				{
					myInterface.Died();
					return;
				}
			}
		}
	}
}
