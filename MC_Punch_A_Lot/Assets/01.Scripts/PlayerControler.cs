using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Etouch = UnityEngine.InputSystem.EnhancedTouch;//Setting and alias to remove ambiguity between UnityEngine.Touch and EnhancedTouch.Touch




[DisallowMultipleComponent, RequireComponent(typeof(Rigidbody))]
public class PlayerControler : MonoBehaviour
{
    /*      PlayerController        */
    /*
            Created by Jansen Duarte    
    
            Controls inputs, rigidbody movement, enemy pile behaviour,
            camera positioning, money and level.
    */


    #region INIT
    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        Etouch.Touch.onFingerDown += Begin_Touch;
        Etouch.Touch.onFingerMove += Move_Touch;
        Etouch.Touch.onFingerUp += End_Touch;
    }

    private void OnDisable()
    {
        Etouch.Touch.onFingerDown -= Begin_Touch;
        Etouch.Touch.onFingerMove -= Move_Touch;
        Etouch.Touch.onFingerUp -= End_Touch;
        EnhancedTouchSupport.Disable();
    }

    private void Start()
    {
        m_cameraInitialPos = mainCamera.transform.localPosition;

        for (int i = 0; i < m_pileLimit + 1; i++)
        {
            m_pilePositions.Add(new Vector3());
            m_pileRotations.Add(new Vector3());
        }
        StartCoroutine(Pile_Sway());
        StartCoroutine(Camera_Follow());
    }
    #endregion //INIT



    #region UPDATE_LOOP
    private void Update()
    {
        //Control animation
        playerAnimator.SetFloat("Speed", m_playerMovement.sqrMagnitude);
    }

    private void FixedUpdate()
    {
        //Control movement
        m_playerMovement = new Vector3(m_movementAmmount.x, 0f, m_movementAmmount.y);
        m_rigidbody.velocity = m_playerMovement * m_movementSpeed;
        transform.LookAt(transform.position + m_playerMovement * m_movementSpeed, transform.up);
    }

    IEnumerator Camera_Follow()
    {
        Vector3 targetPosition;
        while (true) //this could be a flag for the game being paused
        {
            targetPosition = transform.position + m_cameraInitialPos;
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, m_cameraFollowRate);
            yield return new WaitForFixedUpdate();
        }
    }
    #endregion //UPDATE_LOOP



    #region TOUCH_MANAGEMENT
    private void Begin_Touch(Finger _touch)
    {
        if (m_fingerMovement == null)
        {
            m_fingerMovement = _touch;
            m_movementAmmount = Vector2.zero;
            m_joystick.gameObject.SetActive(true);
            m_joystick.Enable_Joystick(_touch.screenPosition);
        }
    }
    private void Move_Touch(Finger _touch)
    {
        if (m_fingerMovement == _touch)
        {
            m_movementAmmount = m_joystick.Move_Joystick(_touch.currentTouch.screenPosition);
        }
    }
    private void End_Touch(Finger _touch)
    {
        if (m_fingerMovement == _touch)
        {
            m_joystick.Disable_Joystick();
            m_movementAmmount = Vector2.zero;
            m_fingerMovement = null;
        }
    }
    #endregion //TOUCH_MANAGEMENT



    #region LEVELUP_MANAGEMENT
    private IEnumerator Wait_And_LevelUp()
    {
        //TODO make the UI call
        while (m_money >= m_level * 10)
        {
            m_playerHud.Show_LevelUp_Indicator(m_levelUpTimer);
            yield return new WaitForSeconds(m_levelUpTimer);
            Level_Up();
        }
    }

    private void Level_Up()
    {
        m_money -= m_level * 10;

        m_level++;
        m_pileLimit += m_pileIncrease;
        m_dropThreshold += m_thresholdIncrease;

        for (int i = 0; i < m_pileIncrease; i++)
        {
            m_pilePositions.Add(new Vector3());
            m_pileRotations.Add(new Vector3());
        }
        Color newColor = Color.HSVToRGB(m_level / 10f % 1f, 1f, 1f);
        m_renderer.material.color = newColor;

        m_generalHud.Update_Level(m_level);
        m_generalHud.Update_Money(m_money);
    }
    #endregion //LEVELUP_MANAGEMENT



    #region PILE_MANAGEMENT
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
        m_generalHud.Update_PileSize(m_currentPileSize, m_pileLimit);
    }

    private IEnumerator Pile_Sway()
    {
        float sqrAcceleration;
        while (true) //This can be a flag for when the game is paused
        {
            //calculate acceleration between physics iterations
            sqrAcceleration = m_rigidbody.velocity.sqrMagnitude;
            yield return new WaitForFixedUpdate();
            sqrAcceleration = (m_rigidbody.velocity.sqrMagnitude - sqrAcceleration) / Time.fixedDeltaTime;

            Adjust_Pile_Position();

            Adjust_Pile_Rotation();

            Relocate_Pile_Items();

            Check_Falling_Bodies(sqrAcceleration);
        }
    }

    private void Adjust_Pile_Position()
    {
        for (int i = 0; i < m_pilePositions.Count; i++)
        {
            Vector3 pos = m_pilePositions[i];
            pos.y = i / m_pileHightVariation;
            pos.z = -m_playerMovement.sqrMagnitude * (i / m_pileLinearVariation);
            m_pilePositions[i] = pos;
        }
    }

    private void Adjust_Pile_Rotation()
    {
        for (int i = 0; i < m_pileRotations.Count; i++)
        {
            //This makes a cool pile, but it's not perfect
            Vector3 rot = m_pileRotations[i];
            rot.x = -(m_playerMovement.sqrMagnitude * m_pileAngularVariation * i);  //if I flip the sign, it looks like a liquid
            m_pileRotations[i] = rot;
        }
    }

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

    private void Check_Falling_Bodies(float _acceleration)
    {
        //It's realy hard to drop only 1 item
        if (m_currentPileSize < 2)
            return;

        if (_acceleration >= m_dropThreshold / m_currentPileSize)
        {
            for (int i = m_pileItems.Count - 1; i > 1; i--)
            {
                m_pileItems[i].Fall();
                m_pileItems.RemoveAt(i);
                m_currentPileSize--;
            }
        }
    }

    private IEnumerator Throw_Bodies(Vector3 _boxPosition)
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
            m_generalHud.Update_PileSize(m_currentPileSize, m_pileLimit);

            yield return new WaitForSeconds(m_throwDelay);
        }

        co_throw = null;
        yield break;
    }
    #endregion //PILE_MANAGEMENT



    #region COLLISION_DETECTION
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
            co_throw = StartCoroutine(Throw_Bodies(_other.transform.position));

        if (_other.CompareTag(Tags.MONEY))
        {
            m_money += (int)_other.GetComponentInParent<MoneyBehaviour>().Collect();
            m_generalHud.Update_Money(m_money);
        }

        if (_other.CompareTag(Tags.LEVELUP))
        {
            if (m_money >= m_level * 10)  //bein a curve could be more fun
            {
                if (co_levelUp == null)
                    StartCoroutine(Wait_And_LevelUp());
            }
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.CompareTag(Tags.DROP_BOX))
        {
            if (co_throw != null)
                StopCoroutine(co_throw);
        }

        if (_other.CompareTag(Tags.LEVELUP))
        {
            if (co_levelUp != null)
                StopCoroutine(co_levelUp);
        }
    }
    #endregion //COLLISION_DETECTION





    #region VARIABLES

    #region SERIALIZED_MEMBERS
    [Header("Player Metrics")]
    [SerializeField] Rigidbody m_rigidbody;
    [SerializeField, Range(1f, 20f)] float m_movementSpeed;
    [SerializeField] int m_pileLimit = 1;
    [SerializeField] int m_pileIncrease = 1;
    [SerializeField] float m_pileLinearVariation;
    [SerializeField] float m_pileAngularVariation;
    [SerializeField] float m_pileHightVariation;
    [SerializeField] float m_dropThreshold;
    [SerializeField] float m_thresholdIncrease;
    [SerializeField] float m_throwDelay;
    [SerializeField] int m_money = 0;
    [SerializeField] int m_level = 1;
    [SerializeField] float m_levelUpTimer = 1f;



    [Space, Header("Aditional References")]
    [SerializeField] Camera mainCamera;
    [SerializeField, Range(0.1f, 1f)] float m_cameraFollowRate;
    [SerializeField] Animator playerAnimator;
    [SerializeField] Transform pileBase;
    [SerializeField] SkinnedMeshRenderer m_renderer;
    [SerializeField] MobiileJoystick m_joystick;
    [SerializeField] PlayerHUD m_playerHud;
    [SerializeField] GeneralHUD m_generalHud;
    #endregion //SERIALIZED_MEMBERS



    #region PRIVATE_MEMBERS
    Finger m_fingerMovement;
    Vector2 m_movementAmmount;



    Vector3 m_cameraInitialPos;
    Vector3 m_playerMovement;
    int m_currentPileSize = 0;



    List<Vector3> m_pilePositions = new List<Vector3>();
    List<Vector3> m_pileRotations = new List<Vector3>();
    Quaternion rotationHelper = new Quaternion();
    List<EnemyControler> m_pileItems = new List<EnemyControler>();



    Coroutine co_throw = null;
    Coroutine co_levelUp = null;
    #endregion //PRIVATE_MEMBERS

    #endregion //VARIABLES
}