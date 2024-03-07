using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerControler : MonoBehaviour
{
    [Header("Player Metrics")]
    [SerializeField] Rigidbody m_rigidbody;
    [SerializeField][Range(1f, 20f)] float m_movementSpeed;
    [SerializeField][Range(1f, 20f)] float m_rotationSpeed;

    [Header("Aditional References")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Animator playerAnimator;


    Vector3 m_cameraInitialPos;
    Vector3 m_playerMovement;



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
        //TODO increase level, change color and increase number of bodies you can carry
    }


    private void PileUp_Enemies(Transform _enemy)
    {
        //TODO
        //Move enemy to the pile

        //Verify if there is room in the pile

        //Reparent enemy if there is room
    }



    private void OnTriggerEnter(Collider _other)
    {
        //TODO make the different collisions with the objects and enemies
        if(_other.CompareTag(Tags.ENEMY))
        {
            EnemyControler ec = _other.GetComponentInParent<EnemyControler>();
            if(ec.State == EnemyState.DEAD)
                PileUp_Enemies(ec.transform);
            if(ec.State == EnemyState.ALIVE)
                ec.Get_Punched();
        }
    }



    private void InitialSetup()
    {
        m_cameraInitialPos = mainCamera.transform.localPosition;


        //FIXME this message can be replaced in an editor only script
        if(m_rigidbody == null)
        {
            if(TryGetComponent<Rigidbody>(out m_rigidbody) == true)
                Debug.LogWarning("<b>PlayerControler</b> - \'m_rigidbody\' was not set. This might impact future performance");
            else
                Debug.LogError("<b>PlayerControler</b> - \'m_rigidbody\' was not present in the player GameObject.");
        }

        if(mainCamera == null)
        {
            mainCamera = FindAnyObjectByType<Camera>();
            Debug.LogWarning("<b>PlayerControler</b> - \'mainCamera\' was not set. This might impact future performance");
        }

        if(playerAnimator == null)
        {
            if(TryGetComponent<Animator>(out playerAnimator) == true)
                Debug.LogWarning("<b>PlayerControler</b> - \'playerAnimator\' was not set. This might impact future performance");
            else
                Debug.LogError("<b>PlayerControler</b> - \'playerAnimator\' was not present in the player GameObject.");
        }
    }

}