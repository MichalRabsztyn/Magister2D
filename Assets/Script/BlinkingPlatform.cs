using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingPlatform : MonoBehaviour
{
	private Renderer _renderer;
	private Collider2D _collider;

	[SerializeField] private float _blinkInterval = 3.0f;

	private void Start()
	{
		_renderer = GetComponent<Renderer>();
		_collider = GetComponent<Collider2D>();
		StartCoroutine(Blink());
	}

	private IEnumerator Blink()
	{
		while (true)
		{
			_renderer.enabled = true;
			_collider.enabled = true;
			yield return new WaitForSeconds(_blinkInterval);
			_renderer.enabled = false;
			_collider.enabled = false;
			yield return new WaitForSeconds(_blinkInterval);
		}
	}
}
