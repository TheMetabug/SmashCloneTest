using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    private string _tagWhatColliderIsTouching = "";
    private Collider _otherCollision;
    private int _playerID;

    void Start()
    {
        _playerID = transform.parent.parent.GetComponent<PlayerController>().playerID;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag + " : " + _playerID + " " + "HurtBox_" + _playerID.ToString());
        gameObject.SendMessageUpwards("CheckHurtBoxCollision", this);
    }

}
