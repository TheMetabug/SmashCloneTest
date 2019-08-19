using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxObject : MonoBehaviour
{
    public void ActivateHitbox(HitBox _hitBox, bool _isFacingRight)
    {
        float diameter = _hitBox.radius*2;
        float radius = _hitBox.radius;

        float xDirMultiplier = 1f;
        if (!_isFacingRight)
        {
            xDirMultiplier = -1f;
        }

        SphereCollider sCollider = GetComponent<SphereCollider>();

        transform.localPosition = new Vector3(_hitBox.position.x * xDirMultiplier, _hitBox.position.y, _hitBox.position.z);
        transform.localScale = new Vector3(diameter, diameter, diameter);
        sCollider.radius = radius;
        sCollider.enabled = true;
    }

}
