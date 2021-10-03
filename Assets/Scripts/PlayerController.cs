using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float maxSpeed;
    public float acceleration;
    public float steering;
    public float driftFactor;
    public float reticleDistance;
    public Vector2 aimVector;
    public Vector2 centerOfMass;

    public GameObject projectilePrefab;
    public int speedmph;

    private Camera mainCamera;
    private Rigidbody2D rb;
    private float currentSpeed;
    private Transform reticleTransform;
    private Transform waterCannonTransform;
    private Transform projectileParent;
    // Start is called before the first frame update
    void Start()
    {
        this.rb = GetComponent<Rigidbody2D>();
        rb.centerOfMass = centerOfMass;
        this.mainCamera = Camera.main;
        reticleTransform = GameObject.Find("Reticle").transform;
        waterCannonTransform = GameObject.Find("WaterCannon").transform;
        projectileParent = GameObject.Find("ProjectileParent").transform;
    }

    private void FixedUpdate()
    {
        // just for development - set centerOfMass every tick for tweaking
        rb.centerOfMass = centerOfMass;

        // Get input
        float h = -Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Calculate speed from input and acceleration
        Vector2 speed = transform.up * (v * acceleration);
        rb.AddForce(speed);

        // Create car rotation
        float direction = Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.up));
        if (direction >= 0.0f)
        {
            rb.rotation += h * steering * (rb.velocity.magnitude / maxSpeed);
        }
        else
        {
            rb.rotation -= h * steering * (rb.velocity.magnitude / maxSpeed);
        }

        // Change velocity based on rotation
        float driftForce = Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.left)) * driftFactor;
        Vector2 relativeForce = Vector2.right * driftForce;
        Debug.DrawLine(rb.position, rb.GetRelativePoint(relativeForce), Color.green);
        rb.AddForce(rb.GetRelativeVector(relativeForce));

        // force max speed limit

        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        currentSpeed = rb.velocity.magnitude;
        speedmph = (int)(currentSpeed * 2.237f);
    }

    // Update is called once per frame
    void Update()
    {
        // move main camera - this is dumb
        mainCamera.transform.position = transform.position + new Vector3(0, 0, -10);

        // calculate aimVector based on mouse position - this needs to happen after camera move
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        aimVector = (mousePosition - (Vector2)transform.position).normalized;

        // move reticle
        reticleTransform.position = (Vector2)mainCamera.transform.position + (aimVector * reticleDistance);

        // aim cannon
        float angle = Mathf.Atan2(aimVector.y, aimVector.x) * Mathf.Rad2Deg;
        waterCannonTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        if (Input.GetButton("Fire1"))
        {
            GameObject projectile = GameObject.Instantiate(
                projectilePrefab, waterCannonTransform.GetChild(0).position, Quaternion.identity, projectileParent);
            projectile.GetComponent<Projectile>().InitializeMovement(rb.velocity, aimVector);
        }
    }
}
