using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack }

    [Header("Detection")]
    public float viewRadius = 12f;
    public float attackRange = 8f;
    public LayerMask playerMask;
    public LayerMask obstacleMask;

    [Header("Patrol")]
    public float patrolRadius = 10f;
    public float patrolDelay = 3f;

    NavMeshAgent agent;
    Transform player;
    EnemyShooter shooter;

    State state;
    float patrolTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        shooter = GetComponent<EnemyShooter>();
        state = State.Patrol;
    }

    void Update()
    {
        switch (state)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase: Chase(); break;
            case State.Attack: Attack(); break;
        }
    }

    void Patrol()
    {
        patrolTimer -= Time.deltaTime;

        if (!agent.hasPath || patrolTimer <= 0)
        {
            Vector3 random = Random.insideUnitSphere * patrolRadius;
            random += transform.position;

            if (NavMesh.SamplePosition(random, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                patrolTimer = patrolDelay;
            }
        }

        LookForPlayer();
    }

    void LookForPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewRadius, playerMask);

        foreach (var col in hits)
        {
            Transform t = col.transform;
            Vector3 dir = (t.position - transform.position).normalized;

            if (!Physics.Raycast(transform.position + Vector3.up, dir, viewRadius, obstacleMask))
            {
                player = t;
                state = State.Chase;
                return;
            }
        }
    }

    void Chase()
    {
        if (!player)
        {
            state = State.Patrol;
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            agent.isStopped = true;
            state = State.Attack;
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);
        FaceTarget(player);
    }

    void Attack()
    {
        if (!player)
        {
            state = State.Patrol;
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > attackRange)
        {
            agent.isStopped = false;
            state = State.Chase;
            return;
        }

        FaceTarget(player);
        shooter.ShootAt(player);
    }

    void FaceTarget(Transform target)
    {
        Vector3 dir = (target.position - transform.position);
        dir.y = 0;
        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 8f);
    }
}