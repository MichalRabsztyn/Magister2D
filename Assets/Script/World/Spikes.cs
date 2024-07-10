using UnityEngine;

public class Spikes : MonoBehaviour
{
	void OnTriggerEnter2D(Collider2D other)
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
