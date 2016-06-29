using UnityEngine;
using System.Collections;

public class GoblinStateInfo : MonoBehaviour
{ 
    [HideInInspector]
    public float m_forwardAmount, m_sidewaysAmount, m_turnTarAng, m_health = 1.0f;
        
    [HideInInspector]
    public bool m_grounded, m_sprinting, m_jumping, m_gravLocked = false, m_swept = false;
}
