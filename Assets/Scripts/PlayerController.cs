using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

/*
* FRAMEDATA - Framedata will be fetch from JSON file or something similiar in the future.
* These are now placeholders, but they are "data" how much startup, active, final active frames are in "state".
* Example: Land = [5, 5, 15] is []
*/
struct FrameData
{
    public int[] land;
    public int[] jump;
    public int[] doubleJump;
    public Attack[] attacks;
    public int[] walk;
    public int[] fall;
    public int[] hit;
    public FrameData(int[] _land, int[] _jump, int[] _doubleJump, Attack[] _attacks, int[] _walk, int[] _fall, int[] _hit)
    {
        land = _land;
        jump = _jump;
        doubleJump = _doubleJump;
        attacks = _attacks;
        walk = _walk;
        fall = _fall;
        hit = _hit;
    }
}

struct Attack
{
    public string name;
    public int id;
    public int[] frameData;
    public HitBox[] hitBox;
    public Attack(int _id, string _name, int[] _frameData, HitBox[] _hitBox)
    {
        id = _id;
        name = _name;
        frameData = _frameData;
        hitBox = _hitBox;
    }
}

struct HitBox
{
    public int id;
    public Vector3 position;
    public float radius;
    public float damage;
    public float launchPower;
    public Vector3 launchDirection;
    public HitBox(int _id, Vector3 _position, float _radius, float _damage, float _launchPower, Vector3 _launchDirection)
    {
        id = _id;
        position = _position;
        radius = _radius;
        damage = _damage;
        launchPower = _launchPower;
        launchDirection = _launchDirection;
    }
}

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
    private FrameData _frameData;

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
        ProcessAttack();
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

    private void ProcessAttack()
    {   
        bool attackInput = GetAttackInput();
        MovementState movementState = playerState.GetCurrentMovementState();
        ActiveState activeState = playerState.GetCurrentActiveState();
        int movementFrames = playerState.GetCurMovementStateFrame();

        // Ground attack
        if (attackInput && (movementState != MovementState.Jump &&
            movementState != MovementState.Fall &&
            movementState != MovementState.Land &&
            movementState != MovementState.Attack))
        {
            playerState.SetMovementState(MovementState.Attack);
        }

        if (movementState == MovementState.Attack)
        {
            ProcessMovementStateFrames(MovementState.Attack, _frameData.attacks[0].frameData, MovementState.Idle);
        }
    }

    private void ProcessWalk()
    {
        float xAxisInput = GetXAxisInput();
        MovementState movementState = playerState.GetCurrentMovementState();
        ActiveState activeState = playerState.GetCurrentActiveState();
        int movementFrames = playerState.GetCurMovementStateFrame();

        _verticalMovement = 0f;

        if (movementState != MovementState.Land && movementState != MovementState.Attack)
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
                    ProcessMovementStateFrames(MovementState.Walk, _frameData.walk);
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
                    ProcessMovementStateFrames(MovementState.Walk, _frameData.walk, MovementState.Idle);
                }
            }
        }
    }

    private void ProcessJump()
    {
        MovementState movementState = playerState.GetCurrentMovementState();
        bool isJumpInput = GetJumpInput();
        // If player has pressed jump and is not on unwanted state, enable jump state
        if (isJumpInput && (movementState != MovementState.Jump &&
            movementState != MovementState.Fall &&
            movementState != MovementState.Land &&
            movementState != MovementState.Attack))
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
            ProcessMovementStateFrames(MovementState.Jump, _frameData.jump, MovementState.Fall);
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
            ProcessMovementStateFrames(MovementState.Land, _frameData.land, MovementState.Idle);
        }

        if (movementState == MovementState.Fall ||
            !_groundCollider.isTouching() && movementState == MovementState.Attack ||
            !_groundCollider.isTouching() && movementState == MovementState.Idle)
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
            ProcessMovementStateFrames(MovementState.Fall, _frameData.fall);
        }

    }

    /*
     * This function will load JSON file for a character it controls (TODO: make it load JSON) 
     */
    private void InitializeFramedata()
    {
        // Ignore JSON loading now, just put something so we can test the feature!
        HitBox[] hitboxes = new HitBox[1]{
            new HitBox(0, new Vector3(), 1f, 2f, 2f, new Vector3(1f, 0f, 0f))
        };

        Attack[] attacks = new Attack[1]{
            new Attack(0, "jab1", new int[3]{2,5,15}, hitboxes)
        };

        _frameData = new FrameData(
            new int[3]{2,5,15}, // land
            new int[3]{2,5,15}, // jump
            new int[3]{2,5,15}, // doubleJump
            attacks,            // attacks
            new int[3]{2,5,15}, // walk
            new int[3]{2,5,15}, // fall
            new int[3]{2,5,15}  // hit
        );

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
    
    private bool GetAttackInput()
    {
        if (CrossPlatformInputManager.GetButton("Fire1"))
        {
            return true;
        }
        return false;
    }
}
