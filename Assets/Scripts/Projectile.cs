using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float launchForce;
    public float aimFudge;
    public float timeToLive;

    private float expireTime;
    private Rigidbody2D rb;

    void FixedUpdate()
    {
        if (Time.fixedTime > expireTime)
            // TODO: consider object pools
            Destroy(gameObject);
    }
    public void InitializeMovement(Vector2 velocity, Vector2 aimVector)
    {
        this.rb = GetComponent<Rigidbody2D>();
        Vector2 fudgeVector = Random.insideUnitCircle * aimFudge;
        Vector2 forceVector = (aimVector + fudgeVector).normalized * launchForce;
        rb.velocity = velocity;
        rb.AddForce(forceVector);

        expireTime = Time.fixedTime + timeToLive;
    }
}
