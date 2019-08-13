﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_StateDisplay : MonoBehaviour
{
    [SerializeField] PlayerController playerCtrl;
    private TextMesh _textMesh;
    private CharacterState _cState;

    void Start()
    {
        _textMesh = gameObject.GetComponent<TextMesh>();
        _cState = playerCtrl.playerState;
    }

    void Update()
    {
        string activeStateName = System.Enum.GetName(typeof(ActiveState),_cState.GetCurrentActiveState());
        string movementStateName = System.Enum.GetName(typeof(MovementState),_cState.GetCurrentMovementState());
        string curActiveFrame = _cState.GetCurActiveStateFrame().ToString();
        string curMovementFrame = _cState.GetCurMovementStateFrame().ToString();
        _textMesh.text =
            "MovementState: " + movementStateName + "\n" +
            "MovementFrame: " + curMovementFrame + "\n" +
            "ActiveState: " + movementStateName + "\n" +
            "ActiveFrame: " + curActiveFrame + "\n"
            
        ;
    }
}