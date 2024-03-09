using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerControler : MonoBehaviour
{
    [Header("Player Metrics")]
    [SerializeField] Rigidbody m_rigidbody;
    [SerializeField, Range(1f, 20f)] float m_movementSpeed;
    [SerializeField, Range(1f, 20f)] float m_rotationSpeed;
    [SerializeField] int m_pileLimit = 1;
    [SerializeField] int m_pileIncrease = 1;
    [SerializeField] float m_pileLinearVariation;
    [SerializeField] float m_pileAngularVariation;
    [SerializeField] float m_pileHightVariation;

    [SerializeField] float m_throwDelay;

    public int money = 0;

    [Header("Aditional References")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Animator playerAnimator;
    [SerializeField] Transform pileBase;
    [SerializeField] SkinnedMeshRenderer m_renderer;


    Vector3 m_cameraInitialPos;
    Vector3 m_playerMovement;
    int m_currentPileSize = 0;

    int m_level = 1;

    List<Vector3> m_pilePositions = new List<Vector3>();
    List<Vector3> m_pileRotations = new List<Vector3>();
    List<EnemyControler> m_pileItems = new List<EnemyControler>();

    Coroutine co_throw = null;



    private void Start()
    {
        InitialSetup();
    }

    private void Update()
    {
        TakeIn_Inputs();

        Control_Animation();
    }

    private void TakeIn_Inputs()
    {
        //DEBUG
#if UNITY_EDITOR_WIN
        //Input of the forward direction
        m_playerMovement.z = Input.GetAxis("Vertical");
        //Input of the rotation
        m_playerMovement.x = Input.GetAxis("Horizontal");
#endif
    }

    private void Control_Animation()
    {
        //FIXME the floor to int is creating problems in the animation
        playerAnimator.SetInteger("Speed", Mathf.FloorToInt(m_playerMovement.sqrMagnitude));
    }

    private void FixedUpdate()
    {
        Control_Inputs();

        Camera_Follow();
    }







    private void Control_Inputs()
    {
        //TODO control/place virtual joystick

        //DEBUG
#if UNITY_EDITOR
        m_rigidbody.velocity = m_playerMovement.z * m_movementSpeed * transform.forward;
        transform.Rotate(transform.up, m_playerMovement.x * m_rotationSpeed);
#endif
    }


    private void Camera_Follow()
    {
        //TODO implement camera folow and reposition

        //DEBUG
        mainCamera.transform.position = transform.position + m_cameraInitialPos;
    }




    private void Level_Up()
    {
        m_level++;
        m_pileLimit += m_pileIncrease;

        for (int i = 0; i < m_pileIncrease; i++)
        {
            m_pilePositions.Add(new Vector3());
            m_pileRotations.Add(new Vector3());
        }
        Color newColor = Color.HSVToRGB((m_level / 10f) % 1f, 1f, 1f);
        m_renderer.material.color = newColor;
    }


    private void PileUp_Enemies(EnemyControler _enemy)
    {
        //Being careful to translate the position to world space,
        //otherwise there is an ugly flicker on the bodies
        _enemy.Place_On_Pile(pileBase.TransformPoint(m_pilePositions[m_currentPileSize]));

        if (m_currentPileSize + 1 > m_pileLimit)
            return;

        //get collected after the check of the list size
        //because this makes a cool interaction with the pile and the bodies that don't fit
        _enemy.Get_Collected();
        _enemy.transform.SetParent(pileBase);

        m_pileItems.Add(_enemy);
        m_currentPileSize++;
    }



    private void OnTriggerEnter(Collider _other)
    {
        if (_other.CompareTag(Tags.ENEMY))
        {
            EnemyControler ec = _other.GetComponentInParent<EnemyControler>();
            if (ec.State == EnemyState.DEAD)
                PileUp_Enemies(ec);
            if (ec.State == EnemyState.ALIVE)
                ec.Get_Punched(Vector3.Normalize(ec.transform.position - transform.position));
        }

        if (_other.CompareTag(Tags.DROP_BOX))
        {
            co_throw = StartCoroutine(Throw_Bodies(_other.transform.position));
        }

        if (_other.CompareTag(Tags.MONEY))
        {
            MoneyBehaviour mb = _other.GetComponentInParent<MoneyBehaviour>();
            money += (int)mb.Collect();
        }

        if(_other.CompareTag(Tags.LEVELUP))
        {
            //TODO make the calculation or the necessary money to levelup
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.CompareTag(Tags.DROP_BOX))
        {
            if (co_throw != null)
                StopCoroutine(co_throw);
        }
    }

    IEnumerator Throw_Bodies(Vector3 _boxPosition)
    {
        while (m_pileItems.ElementAtOrDefault(m_pileItems.Count - 1) != null)
        {
            EnemyControler ec = m_pileItems[m_pileItems.Count - 1];
            Vector3 deltaPos = _boxPosition - ec.transform.position;

            Vector3 velocity = Vector3.zero;
            velocity.x = deltaPos.x * MathConstants.COS_60;
            velocity.z = deltaPos.z * MathConstants.COS_60;
            velocity.y = (deltaPos.y - MathConstants.g / 2f) / MathConstants.SIN_60;

            ec.Get_Thrown(velocity);
            m_pileItems.Remove(ec);
            m_currentPileSize--;

            yield return new WaitForSeconds(m_throwDelay);
        }

        co_throw = null;
        yield break;
    }


    private void InitialSetup()
    {
        m_cameraInitialPos = mainCamera.transform.localPosition;

        for (int i = 0; i < m_pileLimit + 1; i++)
        {
            m_pilePositions.Add(new Vector3());
            m_pileRotations.Add(new Vector3());
        }
        StartCoroutine(Pile_Sway());


        //FIXME this message can be replaced in an editor only script
        if (m_rigidbody == null)
        {
            if (TryGetComponent<Rigidbody>(out m_rigidbody) == true)
                Debug.LogWarning("<b>PlayerControler</b> - \'m_rigidbody\' was not set. This might impact future performance");
            else
                Debug.LogError("<b>PlayerControler</b> - \'m_rigidbody\' was not present in the player GameObject.");
        }

        if (mainCamera == null)
        {
            mainCamera = FindAnyObjectByType<Camera>();
            Debug.LogWarning("<b>PlayerControler</b> - \'mainCamera\' was not set. This might impact future performance");
        }

        if (playerAnimator == null)
        {
            if (TryGetComponent<Animator>(out playerAnimator) == true)
                Debug.LogWarning("<b>PlayerControler</b> - \'playerAnimator\' was not set. This might impact future performance");
            else
                Debug.LogError("<b>PlayerControler</b> - \'playerAnimator\' was not present in the player GameObject.");
        }
    }

    IEnumerator Pile_Sway()
    {
        while (true) //This can be a flag for when the game is paused
        {
            Adjust_Pile_Position();

            Adjust_Pile_Rotation();

            Relocate_Pile_Items();

            Check_Falling_Bodies();

            yield return new WaitForEndOfFrame();
        }
    }

    private Quaternion rotationHelper = new Quaternion();

    private void Relocate_Pile_Items()
    {
        //Add positions
        for (int i = 0; i < m_pileItems.Count; i++)
        {
            if (m_pileItems[i] != null)
            {
                m_pileItems[i].transform.localPosition = m_pilePositions[i];
                rotationHelper.eulerAngles = m_pileRotations[i];
                m_pileItems[i].transform.SetLocalPositionAndRotation(m_pilePositions[i], rotationHelper);
            }
        }
    }

    private void Adjust_Pile_Position()
    {
        for (int i = 0; i < m_pilePositions.Count; i++)
        {
            m_pilePositions[i] = (pileBase.localPosition - m_playerMovement) * (i / m_pileLinearVariation);
            Vector3 v = m_pilePositions[i];
            v.y = i / m_pileHightVariation;
            v.x = 0f;   //the x sway creates a weird artifact
            m_pilePositions[i] = v;
        }
    }

    private void Adjust_Pile_Rotation()
    {
        for (int i = 0; i < m_pileRotations.Count; i++)
        {
            //This makes a cool pile, but it's not perfect
            Vector3 v = m_pileRotations[i];
            v.y = m_playerMovement.x * m_pileAngularVariation * i;
            v.x = -(m_playerMovement.z * m_pileAngularVariation * i);  //if I flip the sign, it looks like a liquid
            m_pileRotations[i] = v;
        }
    }

    private void Check_Falling_Bodies()
    {
        //TODO duh!
    }

}