using UnityEngine;
using UnityEngine.AI;

public class enemy : MonoBehaviour
{
    private NavMeshAgent _enemyAgent;

    public enum EnemyState
    {
        Patrolling,
        Waiting,
        Chasing,
        Searching,
        Attacking
    }

    public EnemyState currentState;

    Transform _player;
    Vector3 _playerLastPositionKnown;

    // PATROL
    [SerializeField] private Transform[] _patrolPoints;
    private int _currentPatrolIndex;

    // DETECTION
    [SerializeField] private float _detectionRange = 7;
    [SerializeField] private float _detectionAngle = 90;

    // ATTACKING
    [SerializeField] private float _attackRange = 2f;

    // SEARCH
    private float _searchTimer;
    [SerializeField] private float _searchWaitTime = 15;
    [SerializeField] private float _searchRadius = 10;

    // WAITING
    [SerializeField] private float _waitingTime = 5;
    private float _waitingTimer;

    void Awake()
    {
        _enemyAgent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindWithTag("Player").transform;
    }

    void Start()
    {
        currentState = EnemyState.Patrolling;
        _currentPatrolIndex = 0;
        SetPatrolPoint();
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                Patrol();
                break;

            case EnemyState.Waiting:
                Waiting();
                break;

            case EnemyState.Chasing:
                Chase();
                break;

            case EnemyState.Searching:
                Search();
                break;

            case EnemyState.Attacking:
                Attack();
                break;
        }
    }

    // ESTADOS

    void Patrol()
    {
        if (OnRange())
        {
            currentState = EnemyState.Chasing;
            return;
        }

        if (_enemyAgent.remainingDistance < 0.5f)
        {
            currentState = EnemyState.Waiting;
            _waitingTimer = 0;
        }
    }

    void Waiting()
    {
        _waitingTimer += Time.deltaTime;

        if (_waitingTimer >= _waitingTime)
        {
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
            SetPatrolPoint();
            currentState = EnemyState.Patrolling;
        }
    }

    void Chase()
    {
        _enemyAgent.SetDestination(_player.position);
        _playerLastPositionKnown = _player.position;

        float distance = Vector3.Distance(transform.position, _player.position);

        if (distance <= _attackRange)
        {
            currentState = EnemyState.Attacking;
        }
        else if (!OnRange())
        {
            currentState = EnemyState.Searching;
            _searchTimer = 0;
        }
    }

    void Attack()
    {
        Debug.Log("Atacando al jugador");

        currentState = EnemyState.Chasing;
    }

    void Search()
    {
        if (OnRange())
        {
            currentState = EnemyState.Chasing;
            return;
        }

        _searchTimer += Time.deltaTime;

        if (_searchTimer >= _searchWaitTime)
        {
            currentState = EnemyState.Patrolling;
            SetPatrolPoint();
        }
        else if (_enemyAgent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint;
            if (RandomSearchPoint(_playerLastPositionKnown, _searchRadius, out randomPoint))
            {
                _enemyAgent.SetDestination(randomPoint);
            }
        }
    }

    

    void SetPatrolPoint()
    {
        _enemyAgent.SetDestination(_patrolPoints[_currentPatrolIndex].position);
    }

    bool RandomSearchPoint(Vector3 center, float radius, out Vector3 point)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * radius;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 4, NavMesh.AllAreas))
        {
            point = hit.position;
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    bool OnRange()
    {
        Vector3 directionToPlayer = _player.position - transform.position;
        float distance = directionToPlayer.magnitude;

        if (distance > _detectionRange)
            return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > _detectionAngle * 0.5f)
            return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, distance))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }
}
