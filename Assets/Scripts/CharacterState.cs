using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This state is kind of "sub" state for movement or other states in future.
 * Their purpose is to tell what "state" is main state going right now.
 */
public enum ActiveState {
    Inactive = 0,
    Start,
    Warmup,
    Active,
    Stopping,
    Stop,
}

/*
 * This state registers movement related states.
 */
public enum MovementState
{
    Idle    = 0,
    Walk,
    Run,
    Jump,
    Fall,
    Land,
    Attack,
    Hit,
    Undefined,
}

/*
 * Simple state change functions to keep character "state" in check
 * You can set and get states currently activated to a character.
 * It also registers the frame when it is entered a movement state and can return
 * how many frames has current state been active.
 * Currently there is only ACTIVE and MOVEMENT states.
 */
public class CharacterState
{
    private MovementState currentMovementState = MovementState.Idle;
    private MovementState lastMovementState = MovementState.Idle;
    private ActiveState currentActiveState;
    private int mStateActivationFrame = 0;
    private int aStateActivationFrame = 0;
    private string extraInfo = "";

    public MovementState GetCurrentMovementState()
    {
        return currentMovementState;
    }

    public ActiveState GetCurrentActiveState()
    {
        return currentActiveState;
    }

    public MovementState GetLastMovementState()
    {
        return lastMovementState;
    }

    public void SetMovementState(MovementState value, string exInfo = "")
    {
        if (value != MovementState.Attack)
        {
            lastMovementState = currentMovementState;
        }

        currentMovementState = value;
        extraInfo = exInfo;
        SetActiveState(ActiveState.Start);
        mStateActivationFrame = Time.frameCount;
    }

    public void SetActiveState(ActiveState value)
    {
        currentActiveState = value;
        aStateActivationFrame = Time.frameCount;
    }

    public int GetCurActiveStateFrame()
    {
        return Mathf.Clamp(Time.frameCount - aStateActivationFrame, 0, 100);
    }

    public int GetCurMovementStateFrame()
    {
        return Mathf.Clamp(Time.frameCount - mStateActivationFrame, 0, 100);
    }
    public string GetStateExtraInfo()
    {
        return extraInfo;
    }
}
