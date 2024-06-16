using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
	public Transform platformLocation;
	public Transform[] wayPoints;

	public float speed;
	public float waitTime;
	public float startWaitTime;
	public float stopWaitTime;

	private int _currentWayPointIndex;
	private float _waitTimeCounter;
	private bool _isMoving;

	private void Start()
	{
		_currentWayPointIndex = 0;
		_waitTimeCounter = startWaitTime;
		_isMoving = true;
	}

	private void Update()
	{
		if (_isMoving)
		{
			MovePlatform();
		}
	}

	private void MovePlatform()
	{
		if (wayPoints.Length == 0)
		{
			return;
		}

		Vector3 targetPosition = wayPoints[_currentWayPointIndex].position;
		platformLocation.position = Vector3.MoveTowards(platformLocation.position, targetPosition, speed * Time.deltaTime);

		if (Vector3.Distance(platformLocation.position, targetPosition) <= 0.05f)
		{
			if (_waitTimeCounter <= 0)
			{
				_currentWayPointIndex++;
				_waitTimeCounter = waitTime;
			}
			else
			{
				_waitTimeCounter -= Time.deltaTime;
			}
		}

		if (_currentWayPointIndex == wayPoints.Length)
		{
			_currentWayPointIndex = 0;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Player"))
		{
			collision.transform.SetParent(platformLocation);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject.CompareTag("Player"))
		{
			collision.transform.SetParent(null);
		}
	}

	private void OnDrawGizmos()
	{
		if (wayPoints.Length == 0)
		{
			return;
		}

		for (int i = 0; i < wayPoints.Length; i++)
		{
			if (i == 0)
			{
				Gizmos.DrawLine(transform.position, wayPoints[i].position);
			}
			else
			{
				Gizmos.DrawLine(wayPoints[i - 1].position, wayPoints[i].position);
			}
		}
	}

	public void StartMoving()
	{
		_isMoving = true;
	}

	public void StopMoving()
	{
		_isMoving = false;
	}

	public void ResetPlatform()
	{
		_currentWayPointIndex = 0;
		_waitTimeCounter = startWaitTime;
		_isMoving = true;
	}
}
