using UnityEngine;
using UnityEngine.AI;

public class enemy : MonoBehaviour
{

    private NavMeshAgent _enemyAgent;
    public enum EnemyState
    {
        Patrolling,
        Chasing,
        Searching,
    }

    public EnemyState currentState;
    
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
