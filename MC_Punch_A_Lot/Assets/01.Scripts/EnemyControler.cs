using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyControler : MonoBehaviour
{
    [Header("Enemy Metrics")]
    [SerializeField][Range(1f, 100f)] float m_maxTravelDistance;
    [SerializeField][Range(1f, 5f)] float m_allowedRemainingDistance;
    [SerializeField] float m_punchForce;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator enemyAnimator;

    [Header("Colliders")]
    [SerializeField] Collider interactionTrigger;
    [SerializeField] Collider masterCollider;

    [Header("Ragdoll References")]
    [SerializeField] Transform hipTransform;
    [SerializeField] Rigidbody masterBone;
    [SerializeField] Rigidbody[] bodyBones;



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

        if (State == EnemyState.ALIVE && agent.enabled)
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
        Set_Radgoll_State(false);
        m_currentState = EnemyState.ALIVE;
        agent.SetDestination(Vector3Extention.RandomVec3(m_maxTravelDistance, true, Axis.Y));
    }

    public void Get_Punched(Vector3 _direction)
    {
        agent.enabled = false;
        enemyAnimator.enabled = false;
        Set_Radgoll_State(true);
        Debug.DrawLine(masterBone.position, masterBone.position + _direction, Color.red, 10f);
        _direction.y = 1f;
        masterBone.AddForce(_direction * m_punchForce, ForceMode.Impulse);
        StartCoroutine(Switch_State(EnemyState.DEAD));
    }

    IEnumerator Switch_State(EnemyState _nextState)
    {
        //Generaly used to let the physics calculations cool down on the limbs
        yield return new WaitForSeconds(1.5f);

        m_currentState = _nextState;
    }

    public void Get_Collected()
    {
        Set_Radgoll_State(false);
        masterBone.transform.localPosition = Vector3.zero;
        masterCollider.enabled = false;
        transform.rotation = Quaternion.identity;   //TEST
    }

    private void Set_Radgoll_State(bool _state)
    {
        //if true:  kinematic off; gravity on
        //else:     kinematic on; gravity off
        masterBone.isKinematic = !_state;
        for (int i = 0; i < bodyBones.Length; i++)
        {
            bodyBones[i].useGravity = _state;
            bodyBones[i].isKinematic = !_state;
        }
    }

    private void ShutDown_Interation() { interactionTrigger.enabled = false; }

    public void Place_On_Pile(Vector3 _pos)
    {
        transform.position = _pos;
        ShutDown_Interation();
    }


    private void OnTriggerEnter(Collider _other)
    {
        if (_other.CompareTag(Tags.DROP_BOX))
            Debug.Log("<b>MONEEEEEY!!</b>");
    }

    private void OnCollisionEnter(Collision _other)
    {
        if (_other.transform.CompareTag(Tags.GROUND))
            interactionTrigger.enabled = true;
    }
}
