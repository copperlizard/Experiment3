using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerStateInfo))]
[RequireComponent(typeof(PlayerMovementController))]
public class PlayerControlInput : MonoBehaviour
{
    public GameManager m_gameManager;

    private PlayerStateInfo m_playerState;
    private PlayerMovementController m_playerMover;

    private float m_v, m_h;

    private bool m_weaponModeLock = false;

	// Use this for initialization
	void Start ()
    {
        m_playerState = GetComponent<PlayerStateInfo>();
        m_playerMover = GetComponent<PlayerMovementController>();

        if (m_gameManager == null)
        {
            m_gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (m_gameManager.m_paused)
        {
            return;
        }

        m_v = Input.GetAxis("Vertical");
        m_h = Input.GetAxis("Horizontal");

        m_playerState.m_firing = Input.GetButton("Fire1");
        m_playerState.m_aiming = Input.GetButton("Fire2");

        m_playerState.m_crouching = Input.GetButton("Crouch");
        m_playerState.m_jumping = Input.GetButton("Jump");
        m_playerState.m_sprinting = Input.GetButton("Sprint");

        //No magic while sprinting
        if (m_playerState.m_sprinting && !m_playerState.m_armed)
        {
            m_playerState.m_firing = false;
        }

        //No sprint crouching with bow
        if (m_playerState.m_armed && m_playerState.m_crouching)
        {
            m_playerState.m_sprinting = false;
        }

        if (!m_weaponModeLock && Input.GetButton("WeaponMode"))
        {
            m_weaponModeLock = true;
            m_playerState.m_armed = !m_playerState.m_armed;
        }
        else if (!Input.GetButton("WeaponMode"))
        {
            m_weaponModeLock = false;
        }

        if (m_playerState.m_armed)
        {
            if (Input.GetButton("WeaponSelect1"))
            {
                m_playerState.m_arrowMode = 0;
            }
            else if (Input.GetButton("WeaponSelect2"))
            {
                m_playerState.m_arrowMode = 1;
            }
            else if (Input.GetButton("WeaponSelect3"))
            {
                m_playerState.m_arrowMode = 2;
            }
        }
        else
        {
            if (Input.GetButton("WeaponSelect1"))
            {
                m_playerState.m_magicMode = 0;
            }
            else if (Input.GetButton("WeaponSelect2"))
            {
                m_playerState.m_magicMode = 1;
            }
            else if (Input.GetButton("WeaponSelect3"))
            {
                m_playerState.m_magicMode = 2;
            }
        }

        m_playerMover.Move(m_v, m_h);
	}
}
