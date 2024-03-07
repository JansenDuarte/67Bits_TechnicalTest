using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyControler : MonoBehaviour
{
    [Header("Enemy Metrics")]
    [SerializeField][Range(1f, 100f)] float m_maxTravelDistance;
    [SerializeField][Range(1f, 5f)] float m_allowedRemainingDistance;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator enemyAnimator;



    EnemyState m_currentState;
    /// <summary>
    /// Enemy's behaviour state
    /// </summary>
    public EnemyState State { get { return m_currentState; } }


    private void Awake()
    {
        //DEBUG later, change this so the enemy is only alive after being spawned
        m_currentState = EnemyState.ALIVE;
    }

    private void Start()
    {
        //DEBUG
        Get_Spawned();
    }


    private void Update()
    {
        //TODO make a state machine to set up the enemy movement

        if (State == EnemyState.ALIVE)
        {
            if (agent.remainingDistance < m_allowedRemainingDistance)
            {
                Debug.Log("Changed target");
                agent.SetDestination(Vector3Extention.RandomVec3(m_maxTravelDistance, true, Axis.Y));
            }
        }
    }

    public void Get_Spawned()
    {
        agent.enabled = true;
        enemyAnimator.enabled = true;
        m_currentState = EnemyState.ALIVE;
        agent.SetDestination(Vector3Extention.RandomVec3(m_maxTravelDistance, true, Axis.Y));
    }

    public void Get_Punched()
    {
        //TODO activate ragdoll
        agent.enabled = false;
        enemyAnimator.enabled = false;
        m_currentState = EnemyState.DEAD;
    }




    private void OnTriggerEnter(Collider _other)
    {
        if (_other.CompareTag(Tags.DROP_BOX))
            Debug.Log("<b>MONEEEEEY!!</b>");
    }
}
