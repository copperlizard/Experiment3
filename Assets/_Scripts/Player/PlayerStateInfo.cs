using UnityEngine;
using System.Collections;

public class PlayerStateInfo : MonoBehaviour
{
    [HideInInspector]
    public float m_forwardAmount, m_sidewaysAmount, m_turnTarAng, m_arrowCharge, m_health = 1.0f, m_mana = 1.0f;

    [HideInInspector]
    public int m_arrowMode = 0, m_magicMode = 0;

    [HideInInspector]
    public bool m_grounded, m_armed, m_aiming, m_firing, m_jumping, m_crouching, m_sprinting, m_gravLocked = false, m_surfing = false, m_interacting = false;
}
