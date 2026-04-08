using UnityEngine;
using UnityEngine.AI;
using System;
using DG.Tweening;

public class Enemy_AI_Script : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;

    [Header("Patrol Routes")]
    [SerializeField] private Transform[] patrolRoute1;
    [SerializeField] private Transform[] patrolRoute2;
    [SerializeField] private Transform[] patrolRoute3;
    [SerializeField] private AudioClip attack;

    private bool endingOne = false;
    private bool endingTwo = false;


    private Transform[] currentPatrolPoints;
    private int currentPatrolIndex = 0;
    private int currentRouteIndex = -1; //Track which route is active (-1 = none)

    [Header("Detection")]
    public float viewDistance = 50f;
    public float viewAngle = 45f;
    public float detectionTimer = 0f;
    public float waitTimer = 3f;

    [Header("Vision")]
    public LayerMask visionIgnoreLayer;

    [Header("AI Behavior")]
    public float repathInterval = 1f;
    public float doorEvaluationInterval = 1.5f;

    private float repathTimer = 0f;
    private float doorEvalTimer = 0f;

    private bool playerIsVisible = false;
    private bool playerIsTracked = false;
    private bool patrolling = true;

    private Transform player;
    private Renderer senseRenderer;
    private DoorInteractionHinge[] allDoors;
    private DoorInteractionHinge targetDoor = null;

    [Header("Sound Detection")]
    public float soundInvestigationTime = 10f;
    private bool investigatingSound = false;
    private Vector3 soundTarget;
    private float soundTimer = 0f;

    [SerializeField] private float debugSpeed;

    [SerializeField] private float attackDistance = 2f;

    public static event Action<Transform> OnPlayerKilled;
    public static event Action OnBadEnding;
    public static event Action OnGoodEnding;
    [SerializeField] private AltarInteraction Altar;
    private bool hasKilledPlayer = false;

    [SerializeField] private Animator animator;
    [SerializeField] private PaperInteraction badEnding;



    private void OnEnable()
    {
        KeyInteraction.OnDropSound += GoToSound;
        PaperInteraction.OnDropSound += GoToSound;
        BookInteraction.OnDropSound += GoToSound;
        PadlockScript.OnDropSound += GoToSound;
        AltarInteraction.OnAltarActivated += goodEnding;
    }

    private void OnDisable()
    {
        KeyInteraction.OnDropSound -= GoToSound;
        PaperInteraction.OnDropSound -= GoToSound;
        BookInteraction.OnDropSound -= GoToSound;
        PadlockScript.OnDropSound -= GoToSound;
        AltarInteraction.OnAltarActivated -= goodEnding;
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 7f;
        agent.acceleration = 50f;
        agent.angularSpeed = 720f;

        player = GameObject.FindGameObjectWithTag("Target").transform;

        GameObject sense = GameObject.FindWithTag("Sense");
        if (sense != null)
            senseRenderer = sense.GetComponent<Renderer>();

        allDoors = FindObjectsOfType<DoorInteractionHinge>();

        //Start with default patrol route
        currentRouteIndex = -1;
        currentPatrolPoints = patrolRoute1;

    }

    void Update()
    {
        HandleVision();
        if(!hasKilledPlayer)
        {
            HandleState();
        } 
        HandleMovement();
        TryOpenDoorAhead();
        UpdatePatrolRouteBasedOnRoom();
        HandleAttack();



        //For seeing the enemy'ys speed live in the inspector
        debugSpeed = agent.speed;

        if (badEnding.IsPickedUp && !hasKilledPlayer)
        {
            StartCoroutine(DelayedAttackCoroutine());
        }
    }

    void HandleVision()
    {
        
        if(IsAttacking()) //Same as in the HandleState function
        {
            return;
        }
        playerIsVisible = CanSeePlayer(transform, player);

        if (playerIsVisible)
        {
            playerIsTracked = true;
            agent.speed = 16f;
            detectionTimer = 5f + waitTimer;
            if (senseRenderer != null) senseRenderer.material.color = Color.red;
        }

        detectionTimer -= Time.deltaTime;

        if (detectionTimer < waitTimer && detectionTimer > 0f)
        {
            playerIsTracked = false;
            patrolling = false;
            agent.speed = 7f;
            agent.SetDestination(transform.position);
            if (senseRenderer != null) senseRenderer.material.color = Color.orange;
        }
        else if (detectionTimer <= 0f)
        {
            patrolling = true;
            agent.speed = 7f;
            if (senseRenderer != null) senseRenderer.material.color = Color.yellow;
        }
    }

    bool IsAttacking()
    {
        return hasKilledPlayer;
    }

    void HandleState()
    {
        //Delete this if this breaks endings of animations elsewhere
        if(IsAttacking())
        {
            return;
        }

        if (patrolling && !hasKilledPlayer)
        {
                animator.SetTrigger("Normal");
                animator.SetBool("Normal", !playerIsTracked && !hasKilledPlayer);
        }

        if (playerIsTracked)
        {
            investigatingSound = false;
            patrolling = false;

            //Animation handling
            if (animator != null)
            {
                animator.SetTrigger("Chase");
                animator.SetBool("isChasing", playerIsTracked && !hasKilledPlayer);
            }

            repathTimer -= Time.deltaTime;
            doorEvalTimer -= Time.deltaTime;

            if (repathTimer <= 0f)
            {
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
            if (!playerIsVisible)
            {
                agent.speed = 12f;
            }
            
            return;
        }

        if (investigatingSound)
        {
            soundTimer -= Time.deltaTime;
            agent.SetDestination(soundTarget);

            //Animation handling
            if (animator != null && !hasKilledPlayer)
            {
                animator.SetTrigger("Normal");
                animator.SetBool("Normal", playerIsTracked && !hasKilledPlayer);
            }

            if (soundTimer <= 0f || agent.remainingDistance < 1f)
            {
                investigatingSound = false;
                patrolling = true;
            }
            agent.speed = 12f;
            return;
        }

        patrolling = true;
    }

    void HandleMovement()
    {
        if (patrolling && !playerIsTracked && !investigatingSound && currentPatrolPoints.Length > 0)
        {
            agent.speed = 7f;
            agent.SetDestination(currentPatrolPoints[currentPatrolIndex].position);

            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % currentPatrolPoints.Length;
            }
        }

        if (agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            TryOpenDoorAhead();
        }
    }

    void UpdatePatrolRouteBasedOnRoom()
    {
        Vector3 p = player.position;

        int newRoute = currentRouteIndex;

        //Room 1: x [-44.5,0], z [-44.5,44.5]
        if (p.x >= -44.5f && p.x <= 0f && p.z >= -44.5f && p.z <= 44.5f)
            newRoute = 0;
        //Room 2: x [-224,-44.5], z [-44.5,44.5]
        else if (p.x >= -224f && p.x < -44.5f && p.z >= -44.5f && p.z <= 44.5f)
            newRoute = 1;
        //Room 3: x [-224,-30], z [-134.5,-44.5]
        else if (p.x >= -224f && p.x <= -30f && p.z >= -134.5f && p.z <= -44.5f)
            newRoute = 2;

        if (newRoute != currentRouteIndex)
            SwitchPatrolRoute(newRoute);
    }

    void SwitchPatrolRoute(int routeIndex)
    {
        currentRouteIndex = routeIndex;
        currentPatrolIndex = 0;

        switch (routeIndex)
        {
            case 0: currentPatrolPoints = patrolRoute1; break;
            case 1: currentPatrolPoints = patrolRoute2; break;
            case 2: currentPatrolPoints = patrolRoute3; break;
        }
    }

    void TryOpenDoorAhead()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, transform.forward, out hit, 2f))
        {
            DoorInteractionHinge door = hit.collider.GetComponentInParent<DoorInteractionHinge>();

            if (door != null && door.CanInteract())
            {
                door.Interact(null);

                if (targetDoor == door)
                    targetDoor = null;

                agent.SetDestination(player.position);
            }
        }
    }

    void EvaluateDoorVsPath()
    {
        float normalPath = GetPathLength(player.position);
        float pathWithDoorsOpen = GetPathLength(player.position);

        if (pathWithDoorsOpen + 1f < normalPath)
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

        targetDoor = null;
        agent.SetDestination(player.position);
    }

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

    public void GoToSound(Vector3 soundPosition)
    {
        if (playerIsTracked) return;

        investigatingSound = true;
        soundTarget = soundPosition;
        soundTimer = soundInvestigationTime;
        agent.speed = 12f;
        agent.SetDestination(soundTarget);
    }

    public bool CanSeePlayer(Transform enemy, Transform player)
    {
        Vector3 dirToPlayer = (player.position - enemy.position).normalized;

        if (Vector3.Distance(enemy.position, player.position) > viewDistance)
            return false;

        float angle = Vector3.Angle(enemy.forward, dirToPlayer);
        if (angle > viewAngle)
            return false;

        Vector3 origin = enemy.position + Vector3.up * 1.5f;

        int mask = ~(1 << LayerMask.NameToLayer("VisionIgnore"));

        if (Physics.Raycast(origin, dirToPlayer, out RaycastHit hit, viewDistance, mask))
        {
            if (hit.transform.CompareTag("Target"))
                return true;
        }

        return false;
    }

    void HandleAttack()
    {
        if (player == null || hasKilledPlayer) return;

        float dist = Vector3.Distance(transform.position, player.position);

        //Check distance
        if (dist <= attackDistance)
        {
            //Raycast to ensure no walls in between
            Vector3 dir = (player.position - transform.position).normalized;

            if (Physics.Raycast(transform.position + Vector3.up * 1.5f, dir, out RaycastHit hit, attackDistance))
            {
                if (hit.transform.CompareTag("Target"))
                {
                    //Face the player
                    Vector3 lookPos = player.position - transform.position;
                    lookPos.y = 0;
                    transform.rotation = Quaternion.LookRotation(lookPos);

                    //Stop moving
                    agent.isStopped = true;
                    agent.speed = 0f;
                    //Play attack animation
                    if (animator != null){
                        animator.SetBool("isNormal", false);
                        hasKilledPlayer = true;
                        animator.SetTrigger("Attack");

                    }

                    //Play damage sound effect
                    SoundEffectsManager.instance?.PlaySoundEffectsClip(attack, transform, 2f);
                    //Fire death event ONCE
                    hasKilledPlayer = true;
                    OnPlayerKilled?.Invoke(transform);

                    StartCoroutine(DelayedGameOverCoroutine());

                    return;
                }
            }
        }

        //Not attacking
        agent.isStopped = false;

        if (animator != null)
            animator.SetBool("isAttacking", false);
    }

    private System.Collections.IEnumerator DelayedAttackCoroutine()
    {
        OnBadEnding?.Invoke(); //notify subscribers
        yield return new WaitForSeconds(2f);
        BadEndingScript();
    }

    private System.Collections.IEnumerator DelayedGameOverCoroutine()
    {
        yield return new WaitForSeconds(1.75f);
        SceneLoader.Instance.LoadGameOver(EndingType.PlayerKilled);
    }

    private System.Collections.IEnumerator DelayedBadEndingCoroutine()
    {
        yield return new WaitForSeconds(1.75f);
        SceneLoader.Instance.LoadGameOver(EndingType.BadEnding);
    }

    private System.Collections.IEnumerator DelayedGoodEndingCoroutine()
    {
        yield return new WaitForSeconds(4f);
        Transform temp = agent.transform;
        temp.DOLocalMove(new Vector3(temp.localPosition.x, temp.localPosition.y - 25f, temp.localPosition.z), 5f);
        yield return new WaitForSeconds(5f);
        SceneLoader.Instance.LoadGameOver(EndingType.GoodEnding);
    }

    private void BadEndingScript()
    {
        if(hasKilledPlayer)
            return;
        Debug.Log("Bad ending triggered!");
        Vector3 playerpos = player.position;
        agent.Warp(new Vector3(playerpos.x - 5f, playerpos.y, playerpos.z));

         //Face the player
                    Vector3 lookPos = player.position - transform.position;
                    lookPos.y = 0;
                    transform.rotation = Quaternion.LookRotation(lookPos);

                    //Stop moving
                    agent.isStopped = true;
                    agent.speed = 0f;
                    //Play attack animation
                    if (animator != null){
                        animator.SetBool("isNormal", false);
                        hasKilledPlayer = true;
                        animator.SetTrigger("Attack");

                    }
                    //Play damage sound effect
                    SoundEffectsManager.instance?.PlaySoundEffectsClip(attack, transform, 2f);
                    //Fire death event ONCE
                    hasKilledPlayer = true;
                    OnPlayerKilled?.Invoke(transform);
                    endingOne = true;
                    StartCoroutine(DelayedBadEndingCoroutine());
                    return;

    }

    void goodEnding(AltarInteraction altar)
    {
        Debug.Log("Good Ending Triggered!");
        hasKilledPlayer = true; //Blocks other endings from triggering
        Vector3 playerpos = player.position;
        Vector3 altarPos = new Vector3(-112.4f, 10f, -79.5f);
        agent.Warp(altarPos);

        //Face the player
                    Vector3 lookPos = player.position - transform.position;
                    lookPos.y = 0;
                    transform.rotation = Quaternion.LookRotation(lookPos);

                    //Stop moving
                    agent.isStopped = true;
                    agent.speed = 0f;

        StartCoroutine(DelayedGoodEndingCoroutine());


    
        
    }


}