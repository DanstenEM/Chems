using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemyShooter shooter;

    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Ranges")]
    [SerializeField] private float detectRange = 20f;
    [SerializeField] private float attackRange = 12f;
    [SerializeField] private float loseRange = 30f;

    [Header("Patrol")]
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float patrolDelay = 3f;

    float patrolTimer;
    Vector3 patrolPoint;
    bool hasPatrolPoint;

    enum State
    {
        Patrol,
        Chase,
        Attack
    }

    State currentState;

    void Awake()
    {
        if (!agent)
            agent = GetComponent<NavMeshAgent>();

        if (!shooter)
            shooter = GetComponent<EnemyShooter>();
    }

    void Update()
    {
        if (!player)
            FindPlayer();

        if (!player)
            return;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                if (dist <= detectRange)
                    currentState = State.Chase;
                break;

            case State.Chase:
                Chase();
                if (dist <= attackRange)
                    currentState = State.Attack;
                else if (dist > loseRange)
                    currentState = State.Patrol;
                break;

            case State.Attack:
                Attack();
                if (dist > attackRange)
                    currentState = State.Chase;
                else if (dist > loseRange)
                    currentState = State.Patrol;
                break;
        }
    }

    void Patrol()
    {
        shooter.enabled = false;

        patrolTimer += Time.deltaTime;

        if (!hasPatrolPoint || patrolTimer >= patrolDelay)
        {
            patrolTimer = 0;
            hasPatrolPoint = true;

            Vector3 random = Random.insideUnitSphere * patrolRadius;
            random += transform.position;

            if (NavMesh.SamplePosition(random, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                patrolPoint = hit.position;
                agent.SetDestination(patrolPoint);
            }
        }
    }

    void Chase()
    {
        shooter.enabled = false;

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void Attack()
    {
        agent.isStopped = true;

        shooter.enabled = true;
        shooter.SetTarget(player);

        // face target smoothly
        Vector3 dir = player.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 6f);
        }
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p)
            player = p.transform;
    }
}