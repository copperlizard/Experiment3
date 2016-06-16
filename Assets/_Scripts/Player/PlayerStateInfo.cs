using UnityEngine;
using System.Collections;

public class PlayerStateInfo : MonoBehaviour
{
    [HideInInspector]
    public float m_forwardAmount, m_sidewaysAmount, m_turnTarAng, m_arrowCharge;

    [HideInInspector]
    public int m_arrowMode, m_magicMode;

    [HideInInspector]
    public bool m_grounded, m_armed, m_aiming, m_firing, m_jumping, m_crouching, m_sprinting;
}
