using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCollider : MonoBehaviour
{
    private string _tagWhatColliderIsTouching = "";

    void OnTriggerStay(Collider other)
    {
        _tagWhatColliderIsTouching = other.tag;
    }
    void OnTriggerExit(Collider other)
    {
        _tagWhatColliderIsTouching = "";
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
}
