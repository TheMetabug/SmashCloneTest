using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

/*
* FRAMEDATA - Framedata will be fetch from JSON file or something similiar in the future.
* These are now placeholders, but they are "data" how much startup, active, final active frames are in "state".
* Example: Land = [5, 5, 15]
*/
public struct FrameData
{
    public int[] land;
    public int[] jump;
    public int[] doubleJump;
    public Attack[] attacks; // This will contain all of player attacks
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

/*
 * ATTACK - Contains data of SINGLE attack. Attack can have multiple hitboxes too.
 */
public struct Attack
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

/*
 * HITBOX - Contains data of SINGLE hitbox. Pretty self explatanory.
 * Note: Hitboxes are meant to "spawn" at correct location when called. Hitboxes are sphere kind
 *       of hitboxes what causes hits to check if connects to enemy hitboxes.
 */
public struct HitBox
{
    public int id;
    public Vector3 position;
    public float radius;
    public float damage;
    public float launchPower;
    public float hitBoxDuration;
    public Vector3 launchDirection;
    public HitBox(int _id, Vector3 _position, float _radius, float _damage, float _launchPower, float _hitBoxDuration, Vector3 _launchDirection)
    {
        id = _id;
        position = _position;
        radius = _radius;
        damage = _damage;
        launchPower = _launchPower;
        hitBoxDuration = _hitBoxDuration;
        launchDirection = _launchDirection;
        
    }
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float playerGravity = 2f;
    [SerializeField] float maxFallSpeed = 5f;
    [SerializeField] float playerJumpForce = 9f;
    [SerializeField] public GameObject hitBoxObject;
    public CharacterState playerState = new CharacterState();
    // [SerializeField] public CharacterState playerAttackState = new CharacterState();
    private Rigidbody _rigidBody;
    private Collider _mainCollider;
    private SimpleCollider _groundCollider;
    private FrameData _frameData;
    private float _verticalMovement;
    private float _horizontalMovement;
    private bool _isFacingRight = true;
    private bool _isJumpActive = false;
    private bool _isOnGround = false;
    private bool _hasLanded = false;
    private int _curAttackId = -1;

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

        // Ground jab
        if (attackInput &&
           (movementState == MovementState.Idle &&
            movementState != MovementState.Attack))
        {
            playerState.SetMovementState(MovementState.Attack, _frameData.attacks[0].name);
            _curAttackId = 0;
        }
        // Second jab (when jab 1 is in progress and you input attack input)
        if (attackInput &&
            activeState == ActiveState.Active && playerState.GetStateExtraInfo() == "jab1" && 
            movementState != MovementState.Attack)
        {
            playerState.SetMovementState(MovementState.Attack, _frameData.attacks[1].name);
            _curAttackId = 1;
        }
        // Nair (Neutral air input)
        if (attackInput && (movementState == MovementState.Jump && movementState != MovementState.Attack) || 
            attackInput && (movementState == MovementState.Fall && movementState != MovementState.Attack)) 
        {
            playerState.SetMovementState(MovementState.Attack, _frameData.attacks[2].name);
            _curAttackId = 2;
        }

        // 
        if (movementState == MovementState.Attack)
        {
            ProcessAttack(_frameData.attacks[_curAttackId]);
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
                    _isFacingRight = true;
                }
                else if (xAxisInput < 0)
                {
                    _verticalMovement = -walkSpeed;
                    _isFacingRight = false;
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
        if (isJumpInput && _hasLanded && 
           (movementState != MovementState.Jump &&
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

            _hasLanded = false;
            StartCoroutine("JumpForce");
        }
        // Go upwards and play the animation on loop until fall animation starts
        else if (movementState == MovementState.Jump)
        {
            ProcessMovementStateFrames(MovementState.Jump, _frameData.jump, MovementState.Fall);
        }

        if (_isJumpActive)
        {
            _horizontalMovement = playerJumpForce;
        }
    }

    /*
     * Give player the force when jump has been inputted. Player can change states while going "upwards" so that's why
     * this is processed by coroutine. Example: press jump and then attack to use "nair" attack.
     */
    IEnumerator JumpForce()
    {
        _isJumpActive = true;
        int jumpStartFrame = Time.frameCount;
        int curJumpFrame = 0;
        int frameDataIndex = _frameData.jump.Length - 1;
        while (_isJumpActive)
        {
            curJumpFrame = Time.frameCount - jumpStartFrame;
            // get LAST framedata frame number from jump. Then the "jump" has ended.
            if (curJumpFrame >= _frameData.jump[frameDataIndex])
            {
                _isJumpActive = false;
            }
            yield return null;
        }
        yield return null;
    }

    private void ProcessFall()
    {
        MovementState movementState = playerState.GetCurrentMovementState();
        ActiveState activeState = playerState.GetCurrentActiveState();
        float delta = Time.deltaTime;

        if (_groundCollider.isTouching() &&
            _groundCollider.whatTagCollisionHas() == "Stage")
        {
            _isOnGround = true;
        }
        else
        {
            _isOnGround = false;
        }

        if (!_isJumpActive && !_isOnGround && !_hasLanded)
        {
            // Fall towards the ground. Gravity pulls character down but locks to maxFallSpeed
            _horizontalMovement -= playerGravity;
            _horizontalMovement = Mathf.Clamp(_horizontalMovement, -maxFallSpeed, playerJumpForce);
        }
        else if (!_isJumpActive && _isOnGround && !_hasLanded)
        {
            playerState.SetMovementState(MovementState.Land);
            _horizontalMovement = 0f;
            // TODO: make this check landing height by platform, not default -1 !!
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                -1.000f,
                transform.localPosition.z
            );
            _hasLanded = true;
        }


        if (movementState == MovementState.Land)
        {
            ProcessMovementStateFrames(MovementState.Land, _frameData.land, MovementState.Idle);
        }

        if (movementState == MovementState.Fall)
        {
            ProcessMovementStateFrames(MovementState.Fall, _frameData.fall);
        }

    }

    /*
     * This function will load JSON file for a character it controls (TODO: make it load JSON) 
     */
    private void InitializeFramedata()
    {
        // Ignore JSON loading now, just put something so we can test the feature!
        HitBox[] hitboxes = new HitBox[3]{
                   // ID            Position            Radius  DMG LaunchPow   HBdur   LaunchDir   
            new HitBox(0, new Vector3(0.5f, 0f, 0f),    0.25f,  2f,     2f,     0.075f, new Vector3(1f, 0f, 0f)), // jab 1
            new HitBox(1, new Vector3(0.65f, 0f, 0f),   0.5f,   2f,     2f,     0.075f, new Vector3(1f, 0f, 0f)), // jab 2
            new HitBox(2, new Vector3(0.25f, 0f, 0f),   0.35f,  2f,     2f,     0.25f,  new Vector3(1f, 0f, 0f))  // nair
        };

        Attack[] attacks = new Attack[3]{
        //            ID     NAME               S   A   ST  END
            new Attack(0,   "jab1",   new int[4]{2, 5,  12, 15},    hitboxes),
            new Attack(1,   "jab2",   new int[4]{4, 9,  14, 20},    hitboxes),
            new Attack(2,   "nAir",   new int[4]{7, 10, 20, 24},    hitboxes),
        };

        _frameData = new FrameData(
            new int[4]{2,5,8,15}, // land
            new int[4]{2,5,8,15}, // jump
            new int[4]{2,5,8,15}, // doubleJump
            attacks,            // attacks
            new int[4]{2,5,8,15}, // walk
            new int[4]{2,5,8,15}, // fall
            new int[4]{2,5,8,15}  // hit
        );

    }

    /*
     * 
     */
    private void ProcessMovementStateFrames(MovementState mState, int[] fData, MovementState nextState = MovementState.Undefined)
    {   
        int warmupFrame = fData[0];
        int activeFrame = fData[1];
        int stoppingFrame = fData[2];
        int stopFrame = fData[3];

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
            ClearHitBoxes(); // Just in case if there is hitboxes activate, clear them.
            playerState.SetActiveState(ActiveState.Warmup);
        }
        else if (currentActiveFrame >= activeFrame &&
                activeState == ActiveState.Warmup)
        {
            playerState.SetActiveState(ActiveState.Active);
        }
        else if (currentActiveFrame >= stoppingFrame &&
                activeState == ActiveState.Active)
        {
            playerState.SetActiveState(ActiveState.Stopping);
        }
        else if (currentActiveFrame >= stopFrame &&
                activeState == ActiveState.Stopping &&
                nextState != MovementState.Undefined)
        {
            playerState.SetMovementState(nextState);
        }
    }

    /*
     * 
     */
    private void ProcessAttack(Attack attack)
    {   
        int warmupFrame = attack.frameData[0];
        int activeFrame = attack.frameData[1];
        int stoppingFrame = attack.frameData[2];
        int stopFrame = attack.frameData[3];

        int currentActiveFrame = playerState.GetCurMovementStateFrame();
        ActiveState activeState = playerState.GetCurrentActiveState();

        // START from inactive (this is rare case. But done just in case)
        if (currentActiveFrame >= 0 &&
            activeState == ActiveState.Inactive)
        {
            playerState.SetActiveState(ActiveState.Start);
        }
        
        // START to WARMUP transition
        if (currentActiveFrame >= warmupFrame &&
                activeState == ActiveState.Start)
        {
            playerState.SetActiveState(ActiveState.Warmup);
        }
        // WARMUP to ACTIVE transition. Also activate hitbox.
        else if (currentActiveFrame >= activeFrame &&
                activeState == ActiveState.Warmup)
        {
            CreateAndActivateHitBox(attack);
            playerState.SetActiveState(ActiveState.Active);
        }
        // ACTIVE to STOPPING transition.
        else if (currentActiveFrame >= stoppingFrame &&
                activeState == ActiveState.Active)
        {
            playerState.SetActiveState(ActiveState.Stopping);
        }
        // STOPPING to STOP transition
        else if (currentActiveFrame >= stopFrame &&
                activeState == ActiveState.Stopping)
        {
            MovementState lastState = playerState.GetLastMovementState(); 
            if (lastState == MovementState.Attack)
            {
                lastState = MovementState.Idle;
            }

            playerState.SetMovementState(lastState);
        }
    }

    private void CreateAndActivateHitBox(Attack _attack)
    {
        Vector3 locPos = transform.localPosition;
        // Create hitbox (this will be activated in "active")
        GameObject hBox = Instantiate(
            hitBoxObject,
            locPos,
            transform.rotation,
            transform
        );
        hBox.GetComponent<HitboxObject>().ActivateHitbox(_attack.hitBox[_attack.id], _isFacingRight);
        GameObject.Destroy(hBox, _attack.hitBox[_attack.id].hitBoxDuration);
    }

    /*
     * This clears hitboxes. Actually wont "delete" them but deactivates them instead. Every hitbox is marked for deletion with a timer.
     * So if we just deactivate 
     */
    private void ClearHitBoxes()
    {
        int childCount = transform.childCount;
        for (int i = 0; i <= childCount - 1; i++)
        {
            GameObject gObj = transform.GetChild(i).gameObject;
            if (gObj.tag == "AttackHitBox")
            {
                gObj.SetActive(false);
            }
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
        if (CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            return true;
        }
        return false;
    }
}
