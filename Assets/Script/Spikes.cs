using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;

public class Spikes : MonoBehaviour
{
	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			other.GetComponent<MLAgentPlayerController>()?.Died();
		}
	}
}
