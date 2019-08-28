using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCollider : MonoBehaviour
{
    private string _tagWhatColliderIsTouching = "";
    private Collider _otherCollision;
    private int _playerID;

    void Start()
    {
        _playerID = transform.parent.parent.GetComponent<PlayerController>().playerID;
    }

    void OnTriggerStay(Collider other)
    {
        _tagWhatColliderIsTouching = other.tag;
        _otherCollision = other;
    }
    void OnTriggerExit(Collider other)
    {
        ResetValues();
    }

    public bool isTouching()
    {
        if (_tagWhatColliderIsTouching != "")
            return true;
        return false;
    }

    public string whatTagCollisionHas()
    {
        return _tagWhatColliderIsTouching;
    }
    public Collider GetCollisionObject()
    {
        if (_tagWhatColliderIsTouching != "")
            return _otherCollision;
        return null;
    }

    private void ResetValues() 
    {
        _tagWhatColliderIsTouching = "";
        _otherCollision = null;
    }
}
