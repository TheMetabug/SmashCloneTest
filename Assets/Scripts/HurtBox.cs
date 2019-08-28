using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    private int _playerID;
    void Start()
    {
        _playerID = transform.parent.parent.GetComponent<PlayerController>().playerID;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "AttackHitBox")
        {
            gameObject.SendMessageUpwards("CheckHurtBoxCollision", other.GetComponent<HitboxObject>());
        }
    }

}
