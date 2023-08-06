using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rigidBody;
    [SerializeField] private Animator _anim;

    [Header("Movement Variables")]
    [SerializeField] private float _movementAcceleration;
    [SerializeField] private float _maxMoveSpeed;
    [SerializeField] private float _groundLinearDrag;
    private float _currentSpeed;
    private float _horizontalDirection;
    private float _verticalDirection;
    private bool _changingDirection => (_rigidBody.velocity.x > 0f && _horizontalDirection < 0f) || (_rigidBody.velocity.x < 0f && _horizontalDirection > 0f);
    private bool _facingRight = true;
    private bool _recentlyFlipped = true;

    [Header("Jump Variables")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _airLinearDrag = 2.5f;
    [SerializeField] private float _fallMultiplier = 8f;
    [SerializeField] private float _lowJumpFallMultiplier = 5f;
    [SerializeField] private float _hangTime = .1f;
    [SerializeField] private float _jumpBufferLength = .1f;
    [SerializeField] private int _extraJumps = 1;
    private float _initialGravityScale;
    private float _hangTimeCounter;
    private float _jumpBufferCounter;
    private int _extraJumpsValue;
    private bool _isJumping = false;
    private bool _canJump => _jumpBufferCounter > 0f && (_hangTimeCounter > 0f || _extraJumpsValue > 0 || _onWall);

    [Header("Glide Variables")]
    [SerializeField] private float _glidePower = -1f;
    private bool _canGlide => Mathf.Abs(_rigidBody.velocity.y) > 0f;

    [Header("Wall Interaction Variables")]
    [SerializeField] private float _wallSliderMod = .01f;
    [SerializeField] private float _wallRunMod = .25f;
    private bool _wallSlide => _onWall && !_onGround && !_wallRun && _rigidBody.velocity.y < 0f;
    private bool _wallRun => _onWall && PowerUps.wallRun && Mathf.Abs(_horizontalDirection) > 0f;


    [Header("Layer Masks")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Ground Collision Variables")]
    [SerializeField] private float _groundRaycastLength;
    [SerializeField] private Vector3 _groundRaycastOffSet;
    private bool _onGround;

    [Header("Wall Collision Variables")]
    [SerializeField] private float _wallRaycastLength;
    [SerializeField] private float _wallRaycastApproach;
    private bool _onWall;
    private bool _onRightWall;
    private bool _onLeftWall;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _initialGravityScale = _rigidBody.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        _horizontalDirection = GetInput().x;
        _verticalDirection = GetInput().y;
        if (Input.GetButtonDown("Jump")) _jumpBufferCounter = _jumpBufferLength;
        else _jumpBufferCounter -= Time.deltaTime;
        Animation();

        //flips are flipping cool!
        if (!_onWall)
        {
            if (_horizontalDirection > 0f && !_facingRight || _horizontalDirection < 0f && _facingRight)
            {
                Flip();
            }
        }
        else if (_onWall && !_onGround)
        {
            if (_onRightWall && !_onLeftWall && _recentlyFlipped)
            {
                Flip();
            }
            else if (!_onRightWall && _onLeftWall && !_recentlyFlipped)
            {
                Flip();
            }
        }

    }
    private void FixedUpdate()
    {
        CheckCollisions();
        MoveCharacter();
        ApplyGroundLinearDrag();
        if (_canJump)
        {
            if(_onWall && !_onGround)
            {
                WallJump();
            }
            else
            {
                Jump(Vector2.up);
            }
        }
        if (_canGlide) Glide();
        if(!_isJumping)
        {
            if (_wallSlide) WallSlide();
            if (_wallRun) WallRun();
        }
        if(_onGround)
        {
            ApplyGroundLinearDrag();
            if (PowerUps.doubleJump)
                _extraJumpsValue = _extraJumps;
            else _extraJumpsValue = 0;
            _hangTimeCounter = _hangTime;
            _rigidBody.gravityScale = _initialGravityScale;
        }
        else
        {
            ApplyAirLinearDrag();
            FallMultiplier();
            _hangTimeCounter -= Time.fixedDeltaTime;
            if (!_onWall || _rigidBody.velocity.y < 0f || _wallRun) _isJumping = false;
        }

    }

    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void Jump(Vector2 direction)
    {
        if (!_onGround && !_onWall)
            _extraJumpsValue--;

        ApplyAirLinearDrag();
        _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, 0f);
        _rigidBody.AddForce(direction * _jumpForce, ForceMode2D.Impulse);
        _hangTimeCounter = 0f;
        _jumpBufferCounter = 0f;
        _isJumping = true;
    }
   
    private void WallJump()
    {
        Vector2 jumpDirection = _onRightWall ? Vector2.left : Vector2.right;
        Jump(Vector2.up + jumpDirection);
    }

    private void Glide()
    {
        if(!_onGround && PowerUps.glidePower && Input.GetButton("Jump") && _rigidBody.velocity.y <= 0f)
        {
            _rigidBody.gravityScale = 0f;
            _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, _glidePower);
            _hangTimeCounter = 0f;
            _jumpBufferCounter = 0f;

        }
        else
        {
            _rigidBody.gravityScale = _initialGravityScale;
        }
    }

    private void WallSlide()
    {
        _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, -_maxMoveSpeed * _wallSliderMod);
    }

    private void WallRun()
    {
        if(_onRightWall && _horizontalDirection > 0f)
        {
            _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, _horizontalDirection * _movementAcceleration * _wallRunMod);
        }
        else if(!_onRightWall && _horizontalDirection < 0f)
        {
            _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, -_horizontalDirection * _movementAcceleration * _wallRunMod);
        }
    }

    private void FallMultiplier()
    {
        if(_rigidBody.velocity.y < 0)
        {
            _rigidBody.gravityScale = _fallMultiplier;
        }
        else if(_rigidBody.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            _rigidBody.gravityScale = _lowJumpFallMultiplier;
        }
        else
        {
            _rigidBody.gravityScale = 1f;
        }
    }

    private void MoveCharacter()
    {
        _rigidBody.AddForce(new Vector2(_horizontalDirection, 0f) * _movementAcceleration);
        if (Mathf.Abs(_rigidBody.velocity.x) > _maxMoveSpeed)
        {
            _rigidBody.velocity = new Vector2(Mathf.Sign(_rigidBody.velocity.x) * _maxMoveSpeed, _rigidBody.velocity.y);

        }

    }

    private void ApplyGroundLinearDrag()
    {
        if(Mathf.Abs(_horizontalDirection) < .04f || _changingDirection)
        {
            _rigidBody.drag = _groundLinearDrag;
        }
        else
        {
            _rigidBody.drag = 0f;
        }
    }

    private void ApplyAirLinearDrag()
    {
        _rigidBody.drag = _airLinearDrag;
    }

    private void CheckCollisions()
    {
        _onGround = Physics2D.Raycast(transform.position + _groundRaycastOffSet, Vector2.down, _groundRaycastLength, groundLayer);
        _onWall = Physics2D.Raycast(transform.position, Vector2.right, _wallRaycastLength, wallLayer) ||
            Physics2D.Raycast(transform.position, Vector2.left, _wallRaycastLength, wallLayer);
        _onRightWall = Physics2D.Raycast(transform.position, Vector2.right, _wallRaycastLength, wallLayer);
        _onLeftWall = Physics2D.Raycast(transform.position, Vector2.left, _wallRaycastLength, wallLayer);
    }

    void Animation()
    {
        if(_onGround)
        {
            _anim.SetBool("isGrounded", true);
            _anim.SetFloat("horizontalDirection", Mathf.Abs(_horizontalDirection));
        }
        else
        {
            _anim.SetBool("isGrounded", false);
        }
        if(_isJumping)
        {
            _anim.SetBool("isJumping", true);
            _anim.SetFloat("verticalDirection", 0f);
        }
        else
        {
            _anim.SetBool("isJumping", false);
            if(_wallRun)
            {
                _anim.SetFloat("verticalDirection", Mathf.Abs(_horizontalDirection));
            }
        }
    }

    private void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
        _facingRight = !_facingRight;
        _recentlyFlipped = !_recentlyFlipped;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        //Ground Detection gizmos
        Gizmos.DrawLine(transform.position + _groundRaycastOffSet, transform.position + _groundRaycastOffSet + Vector3.down * _groundRaycastLength);
        Gizmos.DrawLine(transform.position - _groundRaycastOffSet, transform.position - _groundRaycastOffSet + Vector3.down * _groundRaycastLength);
        
        //Wall Detection gizmos
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * _wallRaycastLength);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * _wallRaycastLength);
    }
}
