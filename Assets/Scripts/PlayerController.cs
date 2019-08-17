using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float playerGravity = 2f;
    [SerializeField] float maxFallSpeed = 5f;
    [SerializeField] float playerJumpForce = 9f;
    [SerializeField] public CharacterState playerState = new CharacterState();
    private Rigidbody _rigidBody;
    private Collider _mainCollider;
    private SimpleCollider _groundCollider;
    private float _verticalMovement;
    private float _horizontalMovement;
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
        _rigidBody = GetComponent<Rigidbody>();
        _mainCollider = GetComponent<Collider>();
        _groundCollider = transform.GetChild(0).transform.GetChild(0).GetComponent<SimpleCollider>();

        playerState.SetMovementState(MovementState.Idle);
        playerState.SetActiveState(ActiveState.Inactive);

        InitializeFramedata();
    }

    void Update()
    {
        ProcessMovement();
    }

    private void ProcessMovement()
    {
        ProcessWalk();
        ProcessJump();
        ProcessFall();

        float delta = Time.deltaTime;
        Vector3 localPos = transform.localPosition;

        Vector3 processedMovementVector = new Vector3(
            localPos.x + (_verticalMovement * delta),
            localPos.y + (_horizontalMovement * delta),
            localPos.z
        );
        transform.localPosition = processedMovementVector;
    }

    private void ProcessWalk()
    {
        float xAxisInput = GetXAxisInput();
        MovementState movementState = playerState.GetCurrentMovementState();
        ActiveState activeState = playerState.GetCurrentActiveState();
        int movementFrames = playerState.GetCurMovementStateFrame();

        _verticalMovement = 0f;

        if (movementState != MovementState.Land)
        {
            if (xAxisInput < -0.1f || xAxisInput > 0.1f)
            {
                // Set X speed
                if (xAxisInput > 0)
                {     
                    _verticalMovement = walkSpeed;
                }
                else if (xAxisInput < 0)
                {
                    _verticalMovement = -walkSpeed;
                }
                // Set MovementState to Walk if it is not enabled yet and process it.
                if (movementState != MovementState.Walk && movementState == MovementState.Idle)
                {
                    playerState.SetMovementState(MovementState.Walk);
                }
                // 
                if (movementState != MovementState.Walk && movementState == MovementState.Idle)
                {
                    playerState.SetMovementState(MovementState.Walk);
                }
                
                if (movementState == MovementState.Walk)
                {
                    ProcessMovementStateFrames(MovementState.Walk, _frameData_Walk);
                }
            }
            else
            {
                // Prevent user "tap" left or right and continue walk animation unnecessarily. Just go idle state again.
                if (movementState == MovementState.Walk && movementFrames < 4) // 4 frames should do the trick
                {
                    playerState.SetMovementState(MovementState.Idle);
                }
                else if (movementState == MovementState.Walk)
                {
                    ProcessMovementStateFrames(MovementState.Walk, _frameData_Walk, MovementState.Idle);
                }
            }
        }
    }

    private void ProcessJump()
    {
        MovementState movementState = playerState.GetCurrentMovementState();
        // If player has pressed jump and is not on jump&fall state, enable jump state
        if (GetJumpInput() &&
            movementState != MovementState.Jump &&
            movementState != MovementState.Fall)
        {

            playerState.SetMovementState(MovementState.Jump);

            // Raise player little bit upwards to get off the floor
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                transform.localPosition.y + 0.2f,
                transform.localPosition.z
            );
        }
        // Go upwards and play the animation on loop until fall animation starts
        else if (movementState == MovementState.Jump)
        {
            _horizontalMovement = playerJumpForce;
            ProcessMovementStateFrames(MovementState.Jump, _frameData_Jump, MovementState.Fall);
        }
    }

    private void ProcessFall()
    {
        MovementState movementState = playerState.GetCurrentMovementState();
        ActiveState activeState = playerState.GetCurrentActiveState();
        float delta = Time.deltaTime;
        
        // Land on the ground animation
        if (movementState == MovementState.Land)
        {
            _horizontalMovement = 0f;
            ProcessMovementStateFrames(MovementState.Land, _frameData_Land, MovementState.Idle);
        }

        if (movementState == MovementState.Fall)
        {
            if (_groundCollider.isTouching() &&
                _groundCollider.whatTagCollisionHas() == "Stage")
            {
                playerState.SetMovementState(MovementState.Land);
                _horizontalMovement = 0f;
                // TODO: make this check landing height by platform, not default -1 !!
                transform.localPosition = new Vector3(
                    transform.localPosition.x,
                    -1.000f,
                    transform.localPosition.z
                );
            }
            // Fall towards the ground. Gravity pulls character down but locks to maxFallSpeed
            _horizontalMovement -= playerGravity;
            _horizontalMovement = Mathf.Clamp(_horizontalMovement, -maxFallSpeed, playerJumpForce);
            // Loop fall animation
            ProcessMovementStateFrames(MovementState.Fall, _frameData_Fall);
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
    /*
     * 
     */
    private void ProcessMovementStateFrames(MovementState mState, int[] fData, MovementState nextState = MovementState.Undefined)
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
                activeState == ActiveState.Active &&
                nextState != MovementState.Undefined)
        {
            playerState.SetMovementState(nextState);
        }
    }

    private float GetXAxisInput()
    {
        float inputDir = 0f;
        if (Input.GetKey(KeyCode.D))
        {
           inputDir += 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
           inputDir += -1f;
        }
        return inputDir;
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
