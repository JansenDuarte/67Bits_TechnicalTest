using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;




[DisallowMultipleComponent]
public class EnemyControler : MonoBehaviour
{
    /*      EnemyControler        */
    /*
            Created by Jansen Duarte    
    
            Controls ragdoll state, navigation and spawning of money.
    */


    private void Start() { Get_Spawned(); }


    private void Update()
    {
        if (m_currentState == EnemyState.ALIVE && agent.enabled)
        {
            if (agent.remainingDistance < m_allowedRemainingDistance)
                agent.SetDestination(Vector3Extention.RandomVec3(m_maxTravelDistance, true, Axis.Y));
        }
        else if(m_currentState == EnemyState.PUNCHED)
        {
            //test collision like this, because the NavMeshAgent overrides the internal collisions
            if (Physics.Raycast(masterBone.transform.position, Vector3.down, out RaycastHit hit, 0.5f))
            {
                if (hit.transform.CompareTag(Tags.GROUND))
                {
                    interactionTrigger.enabled = true;
                    m_currentState = EnemyState.DEAD;
                }
            }
        }
        else if (m_currentState == EnemyState.DEAD)
        {
            //DEBUG this line is helpful in the editor
            Debug.DrawLine(masterBone.transform.position, masterBone.transform.position + Vector3.down, Color.blue);

            //test collision like this, because the NavMeshAgent overrides the internal collisions
            if (Physics.Raycast(masterBone.transform.position, Vector3.down, out RaycastHit hit, 0.5f))
            {
                if (hit.transform.CompareTag(Tags.GROUND))
                    interactionTrigger.enabled = true;
                if (hit.transform.CompareTag(Tags.DROP_BOX))
                {
                    GameObject go = Instantiate(
                        moneyPrefab,
                        Vector3Extention.AddScalar_OnAxis(hit.transform.position, 3f, Axis.Y),
                        Quaternion.identity);
                    go.GetComponent<MoneyBehaviour>().Spawn(transform.position);
                    StartCoroutine(Wait_For_Respawn());
                }
            }
        }
    }


    #region PRIVATE_METHODS
    private void Get_Spawned()
    {
        interactionTrigger.enabled = true;
        transform.position = Vector3Extention.RandomVec3(m_maxTravelDistance, true, Axis.Y);
        agent.enabled = true;
        enemyAnimator.enabled = true;
        enemyAnimator.SetFloat("Speed", 1f);
        Set_Radgoll_State(false);
        m_currentState = EnemyState.ALIVE;
        agent.SetDestination(Vector3Extention.RandomVec3(m_maxTravelDistance, true, Axis.Y));
    }

    private void Set_Radgoll_State(bool _state) { masterBone.isKinematic = !_state; }

    private void ShutDown_Interation() { interactionTrigger.enabled = false; }

    private IEnumerator Wait_For_Respawn()
    {
        Set_Radgoll_State(false);
        transform.position = Vector3.down * 10f;

        yield return new WaitForSeconds(1f);

        Get_Spawned();
    }
    #endregion //PRIVATE_METHODS


    #region PUBLIC_METHODS
    public void Get_Punched(Vector3 _direction)
    {
        GameManager.Instance.Play_HurtSFX();
        agent.enabled = false;
        enemyAnimator.enabled = false;
        Set_Radgoll_State(true);
        Debug.DrawLine(masterBone.position, masterBone.position + _direction, Color.red, 10f);
        _direction.y = 1f;
        masterBone.AddForce(_direction * m_punchForce, ForceMode.Impulse);
        m_currentState = EnemyState.PUNCHED;
    }

    public void Get_Thrown(Vector3 _velocity)
    {
        Set_Radgoll_State(true);
        masterBone.AddForce(_velocity * -MathConstants.g, ForceMode.VelocityChange);
        Debug.DrawLine(masterBone.position, masterBone.position + _velocity, Color.green, 10f);
        transform.SetParent(null);
    }

    public void Fall()
    {
        Set_Radgoll_State(true);
        transform.SetParent(null);
    }

    public void Get_Collected() { Set_Radgoll_State(false); }

    public void Place_On_Pile(Vector3 _pos)
    {
        transform.position = _pos;
        masterBone.transform.localPosition = Vector3.zero;
        transform.rotation = Quaternion.identity;
        ShutDown_Interation();
    }
    #endregion //PUBLIC_METHODS






    #region VARIABLES
    #region SERIALIZED_MEMBERS
    [Header("Enemy Metrics")]
    [SerializeField][Range(1f, 100f)] float m_maxTravelDistance;
    [SerializeField][Range(1f, 5f)] float m_allowedRemainingDistance;
    [SerializeField] float m_punchForce;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator enemyAnimator;

    [Header("Colliders")]
    [SerializeField] Collider interactionTrigger;

    [Header("Ragdoll References")]
    [SerializeField] Transform hipTransform;
    [SerializeField] Rigidbody masterBone;
    [SerializeField] Rigidbody[] bodyBones;

    [Header("Money Spawn Reference")]
    [SerializeField] GameObject moneyPrefab;
    #endregion //SERIALIZED_MEMBERS


    #region PROPERTIES
    EnemyState m_currentState = EnemyState.ALIVE;
    /// <summary>
    /// Enemy's behaviour state
    /// </summary>
    public EnemyState State { get { return m_currentState; } }
    #endregion //PROPERTIES
    #endregion //VARIABLES
}
