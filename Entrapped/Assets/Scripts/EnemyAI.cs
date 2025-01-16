using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance;

    public Transform target;

    public float speed = 200f;

    public float nextWaypointDistance = 3f;

    public Transform EnemyGFX;

    int currentWaypoint = 0;

    bool reachedEndOfPath = false;

    Path path;

    Seeker seeker;

    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        InvokeRepeating("UpdatePath", 0f, 1f);
    }

    // Check if the target (player) is alive
    private bool isTargetAlive()
    {
        if (target == null)
            return false;

        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        return playerHealth != null && playerHealth.health > 0;
    }

    void UpdatePath()
    {
        // Only try to calculate a path if the target is alive
        if (target != null && seeker.IsDone() && isTargetAlive())
            seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Stop AI if the player is dead or target is null
        if (path == null || !isTargetAlive() || target == null)
            return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        if (force.x >= 0.1f)
        {
            EnemyGFX.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (force.x >= -0.1f)
        {
            EnemyGFX.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}

