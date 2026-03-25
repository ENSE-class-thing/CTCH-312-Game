using UnityEngine;
using UnityEngine.AI;

public class Enemy_AI_Script : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Detection")]
    public float viewDistance = 50f;
    public float viewAngle = 45f;
    public float detectionTimer = 0f;
    public float waitTimer = 3f;

    [Header("Vision")]
    public LayerMask visionIgnoreLayer;

    [Header("AI Behavior")]
    public float repathInterval = 0.5f;
    public float doorEvaluationInterval = 1.5f;

    private float repathTimer = 0f;
    private float doorEvalTimer = 0f;

    private bool playerIsVisible = false;
    private bool playerIsTracked = false;
    private bool patrolling = true;

    private Transform player;
    private Renderer senseRenderer;
    private DoorInteractionHinge[] allDoors;

    // Added: track door AI is currently targeting
    private DoorInteractionHinge targetDoor = null;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 10f;
        agent.acceleration = 50f;
        agent.angularSpeed = 720f;

        player = GameObject.FindGameObjectWithTag("Target").transform;

        GameObject sense = GameObject.FindWithTag("Sense");
        senseRenderer = sense.GetComponent<Renderer>();

        allDoors = FindObjectsOfType<DoorInteractionHinge>();
    }

    void Update()
    {
        HandleVision();
        HandleState();
        HandleMovement();
        TryOpenDoorAhead(); // reactive
    }

    // =========================
    // 👁️ VISION SYSTEM
    // =========================
    void HandleVision()
    {
        playerIsVisible = CanSeePlayer(transform, player);

        if (playerIsVisible)
        {
            playerIsTracked = true;
            detectionTimer = 5f + waitTimer;
            senseRenderer.material.color = Color.red;
        }

        detectionTimer -= Time.deltaTime;

        if (detectionTimer < waitTimer && detectionTimer > 0f)
        {
            playerIsTracked = false;
            patrolling = false;
            agent.SetDestination(transform.position);
            senseRenderer.material.color = Color.orange;
        }
        else if (detectionTimer <= 0f)
        {
            patrolling = true;
            senseRenderer.material.color = Color.yellow;
        }
    }

    // =========================
    // 🧠 STATE LOGIC
    // =========================
    void HandleState()
    {
        if (playerIsTracked)
        {
            patrolling = false;

            repathTimer -= Time.deltaTime;
            doorEvalTimer -= Time.deltaTime;

            if (repathTimer <= 0f)
            {
                // If targeting a door, go to it first
                if (targetDoor != null)
                    agent.SetDestination(targetDoor.transform.position);
                else
                    agent.SetDestination(player.position);

                repathTimer = repathInterval;
            }

            if (doorEvalTimer <= 0f)
            {
                EvaluateDoorVsPath();
                doorEvalTimer = doorEvaluationInterval;
            }
        }
    }

    // =========================
    // 🚶 MOVEMENT
    // =========================
    void HandleMovement()
    {
        if (patrolling && !playerIsTracked)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);

            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
        }

        // If path is blocked → force door interaction
        if (agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            TryOpenDoorAhead();
        }
    }

    // =========================
    // 🚪 REACTIVE DOOR OPEN
    // =========================
    void TryOpenDoorAhead()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, transform.forward, out hit, 2f))
        {
            DoorInteractionHinge door = hit.collider.GetComponentInParent<DoorInteractionHinge>();

            if (door != null && door.CanInteract())
            {
                door.Interact(null);

                // If this was the AI's target door, clear it
                if (targetDoor == door)
                    targetDoor = null;

                agent.SetDestination(player.position);
            }
        }
    }

    // =========================
    // 🚀 SMART DOOR DECISION + DOOR ANTICIPATION
    // =========================
    void EvaluateDoorVsPath()
    {
         float normalPath = GetPathLength(player.position);

  //  // Disable carving on all doors temporarily
  //  foreach (var door in allDoors)
   // {
    //    if (door != null && door.Obstacle != null)
    //        door.Obstacle.enabled = false;
   // }

    // Calculate path as if doors are open
    float pathWithDoorsOpen = GetPathLength(player.position);

    // Restore obstacles to their real state
   // foreach (var door in allDoors)
   // {
     //   if (door != null && door.Obstacle != null)
   //        door.Obstacle.enabled = !door.IsOpen();
   // }

        // If doors help shorten path → pick closest unopened door along path
        if (pathWithDoorsOpen + 1f < normalPath) // small tolerance
        {
            DoorInteractionHinge closestDoor = null;
            float closestDist = Mathf.Infinity;

            foreach (var door in allDoors)
            {
                if (!door.IsOpen())
                {
                    float dist = Vector3.Distance(transform.position, door.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestDoor = door;
                    }
                }
            }

            if (closestDoor != null)
            {
                targetDoor = closestDoor;
                agent.SetDestination(targetDoor.transform.position);
                return;
            }
        }

        // No door shortcut, reset targetDoor and go to player
        targetDoor = null;
        agent.SetDestination(player.position);
    }

    void TryOpenBestDoor()
    {
        float closest = Mathf.Infinity;
        DoorInteractionHinge bestDoor = null;

        foreach (var door in allDoors)
        {
            float dist = Vector3.Distance(transform.position, door.transform.position);

            if (dist < closest)
            {
                closest = dist;
                bestDoor = door;
            }
        }

        if (bestDoor != null && bestDoor.CanInteract())
        {
            bestDoor.Interact(null);
            agent.SetDestination(player.position);
        }
    }

    // =========================
    // 📏 PATH LENGTH CALC
    // =========================
    float GetPathLength(Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(target, path);

        if (path.corners.Length < 2)
            return Mathf.Infinity;

        float length = 0f;

        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        return length;
    }

    // =========================
    // 👁️ LINE OF SIGHT
    // =========================
    public bool CanSeePlayer(Transform enemy, Transform player)
    {
        Vector3 dirToPlayer = (player.position - enemy.position).normalized;

        if (Vector3.Distance(enemy.position, player.position) > viewDistance)
            return false;

        float angle = Vector3.Angle(enemy.forward, dirToPlayer);
        if (angle > viewAngle)
            return false;

        // Debug visuals
        Vector3 left = Quaternion.Euler(0, -viewAngle, 0) * transform.forward * viewDistance;
        Vector3 right = Quaternion.Euler(0, viewAngle, 0) * transform.forward * viewDistance;

        Debug.DrawLine(transform.position, transform.position + left, Color.red);
        Debug.DrawLine(transform.position, transform.position + right, Color.red);
        Debug.DrawLine(transform.position, transform.position + transform.forward * viewDistance, Color.green);

        int mask = ~(1 << LayerMask.NameToLayer("VisionIgnore"));

        Vector3 origin = enemy.position + Vector3.up * 1.5f;

        if (Physics.Raycast(origin, dirToPlayer, out RaycastHit hit, viewDistance, mask))
        {
            if (hit.transform.CompareTag("Target"))
                return true;
        }

        return false;
    }
    
}