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
    public Vector3 restartPosition;

    public GameObject projectilePrefab;
    public int speedmph;
    public float maxPickupVelocity;
    private Camera mainCamera;
    private Rigidbody2D rb;
    private float currentSpeed;
    private Transform reticleTransform;
    private Transform waterCannonTransform;
    private Transform projectileParent;
    private GameController gc;
    private AudioSource cannonAudioSource;
    private AudioSource engineAudioSource;

    // Start is called before the first frame update
    void Start()
    {
        this.rb = GetComponent<Rigidbody2D>();
        gc = FindObjectOfType<GameController>();
        rb.centerOfMass = centerOfMass;
        this.mainCamera = Camera.main;
        reticleTransform = GameObject.Find("Reticle").transform;
        waterCannonTransform = GameObject.Find("WaterCannon").transform;
        projectileParent = GameObject.Find("ProjectileParent").transform;
        cannonAudioSource = waterCannonTransform.GetComponent<AudioSource>();
        engineAudioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        if (gc.gameState != GameState.RUNNING)
        {
            return;
        }
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

    public void Reset()
    {
        transform.position = restartPosition;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        // move main camera - this is dumb
        mainCamera.transform.position = transform.position + new Vector3(0, 0, -10);

        if (gc.gameState != GameState.RUNNING)
        {
            return;
        }

        // calculate aimVector based on mouse position - this needs to happen after camera move
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        aimVector = (mousePosition - (Vector2)transform.position).normalized;

        // move reticle
        reticleTransform.position = aimVector * reticleDistance;
        reticleTransform.position = new Vector3(reticleTransform.position.x, reticleTransform.position.y, -190);

        // aim cannon
        float angle = Mathf.Atan2(aimVector.y, aimVector.x) * Mathf.Rad2Deg;
        waterCannonTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        if (Input.GetButton("Fire1"))
        {
            if (!cannonAudioSource.isPlaying)
            { 
                cannonAudioSource.Play();
            }
            GameObject projectile = GameObject.Instantiate(
                projectilePrefab, waterCannonTransform.GetChild(0).position, Quaternion.identity, projectileParent);
            projectile.GetComponent<Projectile>().InitializeMovement(rb.velocity, aimVector);
        } else
        {
            if (cannonAudioSource.isPlaying)
            {
                cannonAudioSource.Pause();
            }
        }

        engineAudioSource.pitch = Mathf.Lerp(0.75f, 6f, speedmph / 60.0f);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((collision.tag == "TransportPU" || collision.tag == "TransportDO") && rb.velocity.magnitude <= maxPickupVelocity)
        {
            Destroy(collision.gameObject);
            gc.AdvanceTransportState();
        } else if (rb.velocity.magnitude >= maxPickupVelocity)
        {
            // Debug.Log("Too fast!");
        }
    }
}
