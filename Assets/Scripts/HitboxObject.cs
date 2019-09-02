using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxObject : MonoBehaviour
{
    public int playerID = -1;
    public HitBox hitBox;

    public void ActivateHitbox(HitBox _hitBox, bool _isFacingRight, int _playerID = -1)
    {
        playerID = _playerID;
        hitBox = _hitBox;
        float diameter = hitBox.radius*2;
        // float radius = hitBox.radius;

        float xDirMultiplier = 1f;
        if (!_isFacingRight)
        {
            xDirMultiplier = -1f;
            hitBox.launchDirection.x *= -1f;
        }

        SphereCollider sCollider = GetComponent<SphereCollider>();

        transform.localPosition = new Vector3(hitBox.position.x * xDirMultiplier, hitBox.position.y, hitBox.position.z);
        transform.localScale = new Vector3(diameter, diameter, diameter);
        sCollider.radius = diameter;
        // gameObject.tag = "HitBox_P" + playerID.ToString();
        // Debug.Log(LayerMask.GetMask("HitBox_P" + playerID.ToString()));
        // gameObject.layer = LayerMask.GetMask("HitBox_P" + playerID.ToString());
        
        sCollider.enabled = true;
    }

    public int GetHitboxPlayerId()
    {
        return playerID;
    }

    public HitBox GetHitboxProperties()
    {
        return hitBox;
    }

    void OnTriggerEnter(Collider other)
    {
        // 
    }
}
