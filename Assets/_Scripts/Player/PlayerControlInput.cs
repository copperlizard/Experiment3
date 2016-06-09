using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerStateInfo))]
[RequireComponent(typeof(PlayerMovementController))]
public class PlayerControlInput : MonoBehaviour
{
    private PlayerStateInfo m_playerState;
    private PlayerMovementController m_playerMover;

    private float m_v, m_h;

    private bool m_weaponModeLock = false;

	// Use this for initialization
	void Start ()
    {
        m_playerState = GetComponent<PlayerStateInfo>();
        m_playerMover = GetComponent<PlayerMovementController>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        m_v = Input.GetAxis("Vertical");
        m_h = Input.GetAxis("Horizontal");

        m_playerState.m_firing = Input.GetButton("Fire1");
        m_playerState.m_aiming = Input.GetButton("Fire2");

        m_playerState.m_crouching = Input.GetButton("Crouch");
        m_playerState.m_jumping = Input.GetButton("Jump");
        m_playerState.m_sprinting = Input.GetButton("Sprint");

        if (!m_weaponModeLock && Input.GetButton("WeaponMode"))
        {
            m_weaponModeLock = true;
            m_playerState.m_armed = !m_playerState.m_armed;
        }
        else if (!Input.GetButton("WeaponMode"))
        {
            m_weaponModeLock = false;
        }

        m_playerMover.Move(m_v, m_h);
	}
}
