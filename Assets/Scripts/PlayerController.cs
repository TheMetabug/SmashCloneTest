using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float playerGravity = 9f;
    [SerializeField] float playerJumpForce = 9f;
    [SerializeField] public CharacterState playerState = new CharacterState();
    private Rigidbody _rigidBody;
    private Vector3 _verticalMovement;
    private Vector3 _horizontalMovement;
    private bool _isOnFloor = false;
    private bool _isJumping = false;
    private bool _isFalling = false;

    void Start()
    {
        _rigidBody = GetComponentInChildren<Rigidbody>();
        playerState.SetMovementState(MovementState.Idle);
        playerState.SetActiveState(ActiveState.Inactive);
    }

    void Update()
    {
        ProcessInput();
        ProcessMovement();
    }

    void ProcessInput()
    {
        // TODO: 
        // 1. Add crossplatform input. For now, use plain keyboard presser for testing.
        // 2. Make movement smarter and consistent. Dont use default unity options.


        _verticalMovement = new Vector3( 0, 0, 0);
        float xAxisInput = GetXAxisInput();

        if (xAxisInput > 0)
        {
            _verticalMovement = new Vector3( walkSpeed, 0, 0);
        }
        else if (xAxisInput < 0)
        {
            _verticalMovement = new Vector3(-walkSpeed, 0, 0);
        }

        if (GetJumpInput() && !_isJumping && _isOnFloor)
        {
            _isJumping = true;
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                transform.localPosition.y + 0.5f,
                transform.localPosition.z
            );
        }
        else if (!GetJumpInput() && _isJumping)
        {
            _isJumping = false;
        }

    }

    private void ProcessMovement()
    {
        float delta = Time.deltaTime;
        Vector3 localPos = transform.localPosition;

        ProcessJump(delta);
        ProcessGravity(delta);

        Vector3 processedMovementVector = new Vector3(
            localPos.x + (_verticalMovement.x * delta),
            localPos.y + (_horizontalMovement.y * delta),
            localPos.z
        );
        transform.localPosition = processedMovementVector;
    }

    private void ProcessJump(float delta)
    {
        if (_isJumping)
        {
            _horizontalMovement.y = playerJumpForce;
        }
    }

    private void ProcessGravity(float delta)
    {
        if (_isOnFloor)
        {
            _horizontalMovement.y = 0f;
        }
        else
        {
            _horizontalMovement.y -= playerGravity;
        }
    }

    private float GetXAxisInput()
    {
        if (Input.GetKey(KeyCode.D))
        {
           return 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
           return -1f;
        }
        return 0f;
    }

    private bool GetJumpInput()
    {
        if (CrossPlatformInputManager.GetButton("Jump"))
        {
            return true;
        }
        return false;
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "Stage")
        {
            _isOnFloor = false;
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.tag == "Stage")
        {
            _isOnFloor = true;
        }
    }
}
