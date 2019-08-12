using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float walkSpeed = 15f;
    private Rigidbody _rigidBody;
    private Vector3 _verticalMovement;

    void Start()
    {
        _rigidBody = GetComponentInChildren<Rigidbody>();
        CreateHurtBoxes();
    }

    void Update()
    {
        ProcessInput();
        ProcessMovement();
        ProcessGravity();
    }

    void ProcessInput()
    {
        // TODO: 
        // 1. Add crossplatform input. For now, use plain keyboard presser for testing.
        // 2. Make movement smarter and consistent. Dont use default unity options.
        _verticalMovement = new Vector3( 0, 0, 0);
        if (GetXAxisInput() > 0)
        {
            _verticalMovement = new Vector3( walkSpeed, 0, 0);
        }
        if (GetXAxisInput() < 0)
        {
            _verticalMovement = new Vector3(-walkSpeed, 0, 0);
        }

    }

    private void CreateHurtBoxes()
    {
        throw new NotImplementedException();
    }

    private void ProcessMovement()
    {
        float delta = Time.deltaTime;
        Vector3 localPos = transform.localPosition;
        Vector3 processedMovementVector = new Vector3(
            _verticalMovement.x + (_verticalMovement.x * delta),
            localPos.y,
            localPos.z
        );

        transform.localPosition += _verticalMovement;
    }
    private void ProcessGravity()
    {
        throw new NotImplementedException();
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
}
