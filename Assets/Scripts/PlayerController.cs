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

    /*
     * FRAMEDATA - Framedata will be fetch from JSON file or something similiar in the future.
     * These are now placeholders, but they are "data" how much startup, active, final active frames are in "state".
     * Example: Land = [5, 5, 15] is []
     */
    private int[] _frameData_Land;
    private int[] _frameData_Jump;
    private int[] _frameData_Attack;
    private int[] _frameData_Walk;
    private int[] _frameData_Fall;
    private int[] _frameData_Hit;

    void Start()
    {
        _rigidBody = GetComponentInChildren<Rigidbody>();
        playerState.SetMovementState(MovementState.Idle);
        playerState.SetActiveState(ActiveState.Inactive);
        InitializeFramedata();
    }

    void Update()
    {
        ProcessInput();
        ProcessMovement();
        ProcessStates();
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


        ProcessJump();
        ProcessFall();


    }

    private void ProcessMovement()
    {
        float delta = Time.deltaTime;
        Vector3 localPos = transform.localPosition;

        ProcessGravity(delta);

        Vector3 processedMovementVector = new Vector3(
            localPos.x + (_verticalMovement.x * delta),
            localPos.y + (_horizontalMovement.y * delta),
            localPos.z
        );
        transform.localPosition = processedMovementVector;
    }

    private void ProcessStates()
    {
        // 
    }

    private void ProcessJump()
    {
        // If player has pressed jump and is not on jump state, enable jump state
        if (GetJumpInput() && playerState.GetCurrentMovementState() != MovementState.Jump)
        {

            playerState.SetMovementState(MovementState.Jump);
            playerState.SetActiveState(ActiveState.Start);

            // Raise player little bit upwards to get off the floor
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                transform.localPosition.y + 0.5f,
                transform.localPosition.z
            );
        }
        // 
        else if (playerState.GetCurrentMovementState() == MovementState.Jump)
        {
            _horizontalMovement.y = playerJumpForce;
            ProcessMovementStateFrames(MovementState.Jump, _frameData_Jump, MovementState.Fall);
        }
    }

    private void ProcessFall()
    {
        MovementState movementState = playerState.GetCurrentMovementState();
        ActiveState activeState = playerState.GetCurrentActiveState();

        // If jump input isn't pressed anymore and player is still in the jump state, enable fall state
        if (!GetJumpInput() && movementState == MovementState.Jump && activeState == ActiveState.Stop ||
            !_isOnFloor && movementState != MovementState.Jump)
        {
            playerState.SetMovementState(MovementState.Fall);
        }
        // If floor is touching the player and we are falling, it means we are going to land.
        else if (_isOnFloor && movementState == MovementState.Fall)
        {
            playerState.SetMovementState(MovementState.Land);
        }
        // Call transition from landing to idle
        if (movementState == MovementState.Land)
        {
            ProcessMovementStateFrames(MovementState.Land, _frameData_Land, MovementState.Idle);
        }
    }

    /*
     * This function will load JSON file for a character it controls (TODO: make it load JSON) 
     */
    private void InitializeFramedata()
    {
        // Ignore JSON loading now, just put something so we can test the feature!
        _frameData_Land = new int[3];
        _frameData_Land[0] = 2;
        _frameData_Land[1] = 5;
        _frameData_Land[2] = 15;

        _frameData_Jump = new int[3];
        _frameData_Jump[0] = 2;
        _frameData_Jump[1] = 5;
        _frameData_Jump[2] = 15;

        _frameData_Attack = new int[3];
        _frameData_Attack[0] = 2;
        _frameData_Attack[1] = 5;
        _frameData_Attack[2] = 15;

        _frameData_Walk = new int[3];
        _frameData_Walk[0] = 2;
        _frameData_Walk[1] = 5;
        _frameData_Walk[2] = 15;

        _frameData_Fall = new int[3];
        _frameData_Fall[0] = 2;
        _frameData_Fall[1] = 5;
        _frameData_Fall[2] = 15;

        _frameData_Hit = new int[3];
        _frameData_Hit[0] = 2;
        _frameData_Hit[1] = 5;
        _frameData_Hit[2] = 15;
    }

    private void ProcessMovementStateFrames(MovementState mState, int[] fData, MovementState nextState)
    {   
        int warmupFrame = fData[0];
        int activeFrame = fData[1];
        int stopFrame = fData[2];

        int currentActiveFrame = playerState.GetCurMovementStateFrame();
        ActiveState activeState = playerState.GetCurrentActiveState();

        if (currentActiveFrame >= 0 &&
            activeState == ActiveState.Inactive)
        {
            playerState.SetActiveState(ActiveState.Start);
        }
        
        if (currentActiveFrame >= warmupFrame &&
                activeState == ActiveState.Start)
        {
            playerState.SetActiveState(ActiveState.Warmup);
        }
        else if (currentActiveFrame >= activeFrame &&
                activeState == ActiveState.Warmup)
        {
            playerState.SetActiveState(ActiveState.Active);
        }
        else if (currentActiveFrame >= stopFrame &&
                activeState == ActiveState.Active)
        {
            playerState.SetMovementState(nextState);
        }
    }

    /*
     * Handles the falling, nothing else really. Should this be also when checking states??
     */
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
