using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float maxSpeed;
    public float acceleration;
    public float steering;
    public float reticleDistance;
    public Vector2 aimVector;

    public int speedkmh;

    private Camera mainCamera;
    private Rigidbody2D rb;
    private float currentSpeed;
    private Transform reticleTransform;

    // Start is called before the first frame update
    void Start()
    {
        this.rb = GetComponent<Rigidbody2D>();
        this.mainCamera = Camera.main;
        reticleTransform = GameObject.Find("Reticle").transform;
    }

    private void FixedUpdate()
    {
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
        float driftForce = Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.left)) * 2.0f;
        Vector2 relativeForce = Vector2.right * driftForce;
        Debug.DrawLine(rb.position, rb.GetRelativePoint(relativeForce), Color.green);
        rb.AddForce(rb.GetRelativeVector(relativeForce));

        // force max speed limit

        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        currentSpeed = rb.velocity.magnitude;
        speedkmh = (int)(currentSpeed * 3.6f);




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
    }
}
