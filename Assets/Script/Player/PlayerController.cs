using System;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public struct FrameInput
{
	public bool JumpDown;
	public bool JumpHeld;
	public Vector2 Move;
}

public interface IPlayerController
{
	public event Action<bool, float> GroundedChanged;

	public event Action Jumped;
	public Vector2 FrameInput { get; }

	public void Died();
	public void GotToEnd();
}

public class PlayerController : MonoBehaviour, IPlayerController
{
	[SerializeField] private ScriptableStats _stats;
	[SerializeField] private TextMeshProUGUI _timerText;
	[SerializeField] private TextMeshProUGUI _levelNameText;

	private Rigidbody2D _rb;
	private CapsuleCollider2D _col;
	private UIManager _uiManager;
	private FrameInput _frameInput;
	private Vector2 _frameVelocity;
	private bool _cachedQueryStartInColliders;

	private bool _allowUpdate = true;
	private bool _playerMoved = false;

	private Vector3 _startPosition;

	private Vector2 _savedVelocity;
	private float _savedGravityScale;

	#region Interface

		public Vector2 FrameInput => _frameInput.Move;
		public event Action<bool, float> GroundedChanged;
		public event Action Jumped;

		#endregion

	private float _time;
	private float _points;

	private int _levelIndex;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();
		_col = GetComponent<CapsuleCollider2D>();
		_uiManager = GetComponent<UIManager>();

		_cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
		_startPosition = transform.localPosition;
		_timerText.SetText("00:00:00");
	}

	private void Start()
	{
		_rb.velocity = Vector2.zero;
		transform.localPosition = _startPosition;
		_grounded = true;

		_levelIndex = SceneManager.GetActiveScene().buildIndex - 1;
		_levelNameText.SetText("Poziom " + _levelIndex);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			_uiManager?.PauseGame();
			return;
		}

		GatherInput();
	}

	private void PlayerReset(bool bSuccess)
	{
		_rb.velocity = Vector2.zero;
		transform.localPosition = _startPosition;
		_grounded = true;
		_frameVelocity = Vector2.zero;
		_bufferedJumpUsable = false;
		_points = 0;
		_playerMoved = false;

		_timerText.SetText("00:00:00");
	}

	public void PausePlay()
	{
		_allowUpdate = false;
		_savedVelocity = _rb.velocity;
		_rb.velocity = new Vector2(0.0f, 0.0f);
		_savedGravityScale = _rb.gravityScale;
		_rb.gravityScale = 0.0f;
	}

	public void ResumePlay()
	{
		_allowUpdate = true;
		_rb.velocity = _savedVelocity;
		_savedVelocity = new Vector2(0.0f, 0.0f);
		_rb.gravityScale = _savedGravityScale;
		_savedGravityScale = 0.0f;
	}

	private void GatherInput()
	{
		_frameInput = new FrameInput
		{
			JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
			JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
			Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
		};

		if (_stats.SnapInput)
		{
			_frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
			_frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
		}

		if (_frameInput.JumpDown)
		{
			_jumpToConsume = true;
			_timeJumpWasPressed = _time;
		}

		if (_frameInput.Move.x != 0 || _frameInput.Move.y != 0) _playerMoved = true;
	}

	private void FixedUpdate()
	{
		_time += Time.deltaTime;

		if (!_allowUpdate) return;

		if (_playerMoved)
		{
			_points += Time.deltaTime;
			System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(_points);
			string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
											 timeSpan.Minutes,
											 timeSpan.Seconds,
											 timeSpan.Milliseconds / 10);
			_timerText?.SetText(formattedTime);
		}

		CheckCollisions();

		HandleJump();
		HandleDirection();
		HandleGravity();

		ApplyMovement();
	}

	#region Collisions

		private float _frameLeftGrounded = float.MinValue;
		private bool _grounded;

		private void CheckCollisions()
		{
			Physics2D.queriesStartInColliders = false;

			bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
			bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

			if (ceilingHit)
			{
				_frameVelocity.y = Mathf.Min(0, _frameVelocity.y);
			}

			if (!_grounded && groundHit)
			{
				_grounded = true;
				_bufferedJumpUsable = true;
				_endedJumpEarly = false;
				GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
			}
			else if (_grounded && !groundHit)
			{
				_grounded = false;
				_frameLeftGrounded = _time;
				GroundedChanged?.Invoke(false, 0);
			}

			Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
		}

		#endregion

	#region Jumping

		private bool _jumpToConsume;
		private bool _bufferedJumpUsable;
		private bool _endedJumpEarly;
		private float _timeJumpWasPressed;

		private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;

		private void HandleJump()
		{
			if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;

			if (!_jumpToConsume && !HasBufferedJump) return;

			if (_grounded && _playerMoved) ExecuteJump();

			_jumpToConsume = false;
		}

		private void ExecuteJump()
		{
			_endedJumpEarly = false;
			_timeJumpWasPressed = 0;
			_bufferedJumpUsable = false;
			_frameVelocity.y = _stats.JumpPower;
			Jumped?.Invoke();
		}

		#endregion

	#region Horizontal

		private void HandleDirection()
		{
			if (_frameInput.Move.x == 0)
			{
				var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
				_frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
			}
			else
			{
				_frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
			}
		}

		#endregion

	#region Gravity

		private void HandleGravity()
		{
			if (_grounded && _frameVelocity.y <= 0f)
			{
				_frameVelocity.y = _stats.GroundingForce;
			}
			else
			{
				var inAirGravity = _stats.FallAcceleration;
				if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
				_frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
			}
		}

		#endregion

	private void ApplyMovement() => _rb.velocity = _frameVelocity;

	public void GotToEnd()
	{
		GetComponent<ScoreDatabase>()?.SaveScore(_levelIndex, (int)(_points * 100.0f));

		_uiManager.ShowSummary(true);
	}

	public void Died()
	{
		GetComponent<ScoreDatabase>()?.SaveScore(_levelIndex, 0);

		_uiManager.ShowSummary(false);
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
	}
#endif
}