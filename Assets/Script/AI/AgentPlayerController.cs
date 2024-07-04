using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using TMPro;
using Unity.MLAgents.Integrations.Match3;

namespace TarodevController
{
	public class MLAgentPlayerController : Agent, IPlayerController
	{
		[SerializeField] private ScriptableStats _stats;
		[SerializeField] private GameObject _goalGameObject;
		[SerializeField] private bool _isBot = true;
		[SerializeField] private int _level = 1;
		[SerializeField] private TextMeshProUGUI _timerText;
		private Rigidbody2D _rb;
		private CapsuleCollider2D _col;
		private PausePanel _pausePanel;
		private FrameInput _frameInput;
		private Vector2 _frameVelocity;
		private bool _cachedQueryStartInColliders;

		private ActionSegment<int> _lastDiscreteAction;
		private bool _wasJumping = false;
		private bool _allowUpdate = true;
		private bool _playerMoved = false;

		private Academy _academy;
		private Vector3 _startPosition;

		private Vector2 _savedVelocity;
		private float _savedGravityScale;

		#region Interface

		public Vector2 FrameInput => _frameInput.Move;
		public event Action<bool, float> GroundedChanged;
		public event Action Jumped;

		#endregion

		private float _time;

		private void Awake()
		{
			_rb = GetComponent<Rigidbody2D>();
			_col = GetComponent<CapsuleCollider2D>();
			_pausePanel = GetComponent<PausePanel>();

			_cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
			_startPosition = transform.localPosition;
			_timerText?.SetText("0");
		}

		private void Start()
		{
			_academy = GameObject.FindObjectOfType<Academy>();
			_bufferedJumpUsable = false;

			_frameInput = new FrameInput
			{
				JumpDown = false,
				JumpHeld = false,
				Move = new Vector2(0.0f, 0.0f)
			};
			_lastDiscreteAction = new ActionSegment<int>();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				_pausePanel?.ShowPanel();
			}
		}

		public override void OnEpisodeBegin()
		{
			_rb.velocity = Vector2.zero;
			transform.localPosition = _startPosition;
			_grounded = true;
		}

		private void PlayerReset(bool bSuccess)
		{
			_rb.velocity = Vector2.zero;
			transform.localPosition = _startPosition;
			_grounded = true;
			_wasJumping = false;
			_frameVelocity = Vector2.zero;
			_bufferedJumpUsable = false;

			_timerText.SetText("0");
			GetComponent<SummaryScreen>()?.ShowSummaryScreen(bSuccess);
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

		public override void CollectObservations(VectorSensor sensor)
		{
			if (!_isBot) return;

			sensor?.AddObservation(transform.localPosition);
			sensor?.AddObservation(_rb.velocity);
			sensor?.AddObservation(_grounded);
			sensor?.AddObservation(Vector3.Distance(_goalGameObject.transform.position, transform.position));
		}

		public override void OnActionReceived(ActionBuffers actions)
		{
			_lastDiscreteAction = actions.DiscreteActions;

			AddReward(-1f / MaxStep);
			AddReward(Vector3.Distance(_goalGameObject.transform.position, transform.position) / MaxStep);
		}

		private void GatherInput()
		{
			if (_lastDiscreteAction.Length > 1)
			{
				if (!_playerMoved && _lastDiscreteAction[0] == 0 && _lastDiscreteAction[1] == 0)
				{
					_frameInput = new FrameInput
					{
						JumpDown = false,
						JumpHeld = false,
						Move = new Vector2(0.0f, 0.0f)
					};
				}
				else
				{
					_frameInput = new FrameInput
					{
						JumpDown = !_wasJumping && _lastDiscreteAction[2] != 0,
						JumpHeld = _wasJumping && _lastDiscreteAction[2] != 0,
						Move = new Vector2(_lastDiscreteAction[0] - 1, _lastDiscreteAction[1] - 1)
					};
				}

				_wasJumping = _lastDiscreteAction.Length > 1 ? _lastDiscreteAction[2] != 0 : false;

				if (_frameInput.Move != Vector2.zero)
				{
					_playerMoved = true;
				}
			}
			else
			{
				_frameInput = new FrameInput
				{
					JumpDown = false,
					JumpHeld = false,
					Move = new Vector2(0.0f, 0.0f)
				};

				_wasJumping = false;
			}

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
		}

		private void FixedUpdate()
		{
			if (!_allowUpdate) return;

			if (_playerMoved)
			{
				_time += Time.deltaTime;
				_timerText?.SetText(((int)(_time * 100.0f)).ToString());
			}

			GatherInput();
			CheckCollisions();

			HandleJump();
			HandleDirection();
			HandleGravity();

			ApplyMovement();

			if (StepCount == MaxStep)
			{
				_academy?.UpdateFail();
			}
		}

		#region Collisions

		private float _frameLeftGrounded = float.MinValue;
		private bool _grounded;

		private void CheckCollisions()
		{
			Physics2D.queriesStartInColliders = false;

			// Ground and Ceiling
			bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
			bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

			// Hit a Ceiling
			if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

			// Landed on the Ground
			if (!_grounded && groundHit)
			{
				_grounded = true;
				_coyoteUsable = true;
				_bufferedJumpUsable = true;
				_endedJumpEarly = false;
				GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
			}
			// Left the Ground
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
		private bool _coyoteUsable;
		private float _timeJumpWasPressed;

		private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
		private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

		private void HandleJump()
		{
			if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;

			if (!_jumpToConsume && !HasBufferedJump) return;

			if (_grounded || CanUseCoyote) ExecuteJump();

			_jumpToConsume = false;
		}

		private void ExecuteJump()
		{
			_endedJumpEarly = false;
			_timeJumpWasPressed = 0;
			_bufferedJumpUsable = false;
			_coyoteUsable = false;
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

		public override void Heuristic(in ActionBuffers actionsOut)
		{
			var discreteActionsOut = actionsOut.DiscreteActions;
			discreteActionsOut[0] = Input.GetAxisRaw("Horizontal") < 0 ? 0 : (Input.GetAxisRaw("Horizontal") > 0 ? 2 : 1);
			discreteActionsOut[1] = Input.GetAxisRaw("Vertical") < 0 ? 0 : (Input.GetAxisRaw("Vertical") > 0 ? 2 : 1);
			discreteActionsOut[2] = Input.GetKey(KeyCode.Space) ? 1 : 0;

			_isBot = false;
		}
		public void GotToEnd()
		{
			if (_isBot)
			{
				_academy?.UpdateSuccess();
				AddReward(5.0f);
				_time = 0;
				_playerMoved = false;
				EndEpisode();
			}
			else
			{
				GetComponent<ScoreDatabase>()?.SaveScore(_level, (int)(_time*100.0f));
				_time = 0;
				_playerMoved = false;
				PlayerReset(true);
			}
		}

		public void Died()
		{
			_time = 0;
			_playerMoved = false;

			if (_isBot)
			{
				_academy?.UpdateFail();
				AddReward(-1.0f);
				EndEpisode();
			}
			else
			{
				GetComponent<ScoreDatabase>()?.SaveScore(_level, (int)(_time * 100.0f));
				PlayerReset(false);
			}
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
		}
#endif
		public int GetLevel()
		{
			return _level;
		}
	}
}