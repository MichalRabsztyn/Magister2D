using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        MLAgentPlayerController sideScrollerAgent = other.GetComponent<MLAgentPlayerController>();
        if (sideScrollerAgent != null)
        {
            sideScrollerAgent.GotToEnd();
        }
    }
}
