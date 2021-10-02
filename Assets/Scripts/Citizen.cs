using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pathfinding;

public enum CitizenState
{
    WALK = 0,
    RUN = 1,
    PROTEST = 2,
    STAGGER = 3,
    DEAD = 4
}

public class Citizen : MonoBehaviour
{
    public CitizenState state;
    public float wanderDistance;
    public Vector2 targetPosition;
    public float nextWaypointDistance;
    public Path path;
    public float walkSpeed;
    public float runSpeed;
    public float staggerTime;
    public float deadTime;
    public float speedRandomness;
    public Sprite[] stateSprites;
    public float vehicleCollisionSurvivalVelocity;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Seeker seeker;
    private bool validPath;
    private int currentWaypoint;
    private float stateEndTime;
    // Start is called before the first frame update
    void Start()
    {
        transform.name += Time.fixedTime;
        state = CitizenState.WALK;
        walkSpeed = walkSpeed * Random.Range(1 - speedRandomness, 1 + speedRandomness);
        runSpeed = runSpeed * Random.Range(1 - speedRandomness, 1 + speedRandomness);
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        validPath = false;
    }

    private void Update()
    {
        sr.sprite = stateSprites[(int)state];
        if (rb.velocity.x > 0)
        {
            sr.flipX = true;
        }
        else if (rb.velocity.x < 0)
        {
            sr.flipX = false;
        }
    }

    void FixedUpdate()
    {
        // get a new path if we don't have one, but aren't looking for one
        switch (state)
        {
            case CitizenState.WALK:
                DoWalkMovement();
                break;
            case CitizenState.PROTEST:
                DoProtestMovement();
                break;
            case CitizenState.STAGGER:
                if (Time.fixedTime >= stateEndTime)
                    state = CitizenState.WALK;
                break;
            case CitizenState.DEAD:
                if (Time.fixedTime >= stateEndTime)
                    Destroy(gameObject);
                break;
            default:
                break;
        }

    }
    private void DoProtestMovement()
    {
        if (!validPath & seeker.IsDone())
        {
            // TODO: change target detection for protest
            targetPosition = (Vector2)transform.position + (Random.insideUnitCircle * wanderDistance);
            seeker.StartPath(transform.position, targetPosition, OnPathComplete);
        }
        else if (path != null)
        {
            float distanceToWaypoint;
            while (true)
            {
                distanceToWaypoint = Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]);
                if (distanceToWaypoint < nextWaypointDistance)
                {
                    if (currentWaypoint + 1 < path.vectorPath.Count)
                    {
                        currentWaypoint++;
                    }
                    else
                    {
                        validPath = false;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            var speedFactor = validPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;
            Vector2 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
            Vector2 velocity = dir * runSpeed * speedFactor;
            rb.velocity = velocity;
        }
    }

    private void DoWalkMovement()
    {
        if (!validPath & seeker.IsDone())
        {
            targetPosition = (Vector2)transform.position + (Random.insideUnitCircle * wanderDistance);
            seeker.StartPath(transform.position, targetPosition, OnPathComplete);
        }
        else if (path != null)
        {
            float distanceToWaypoint;
            while (true)
            {
                distanceToWaypoint = Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]);
                if (distanceToWaypoint < nextWaypointDistance)
                {
                    if (currentWaypoint + 1 < path.vectorPath.Count)
                    {
                        currentWaypoint++;
                    }
                    else
                    {
                        validPath = false;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            var speedFactor = validPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;
            Vector2 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
            Vector2 velocity = dir * walkSpeed * speedFactor;
            rb.velocity = velocity;
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            validPath = true;
            currentWaypoint = 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerProjectile"))
        {
            SetStagger();
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.rigidbody.velocity.magnitude <= vehicleCollisionSurvivalVelocity)
            {
                SetStagger();
            } else
            {
                SetDead();
            }
        }
    }

    private void SetStagger()
    {
        state = CitizenState.STAGGER;
        validPath = false;
        stateEndTime = Time.fixedTime + staggerTime;
    }

    private void SetDead()
    {
        state = CitizenState.DEAD;
        validPath = false;
        stateEndTime = Time.fixedTime + deadTime;
        // disable colliders
        GetComponent<Collider2D>().enabled = false;
    }
}
