using UnityEngine;
using System.Collections;

public class DrawnMagic : MonoBehaviour
{
    public ParticleSystem m_balls, m_glow;

    private ParticleSystem.EmissionModule m_ballsEmitter, m_glowEmitter;

    //private ParticleSystem.MinMaxCurve m_ballsEmitterRate, m_glowEmitterRate;

    private PlayerStateInfo m_playerState;

	// Use this for initialization
	void Start ()
    {
        m_playerState = GetComponentInParent<PlayerStateInfo>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (m_playerState.m_sprinting)
        {
            m_balls.startLifetime = Mathf.Lerp(m_balls.startLifetime, 1.0f, 0.3f);
            m_glow.startSize = Mathf.Lerp(m_glow.startSize, 0.25f, 0.3f);

            //m_ballsEmitterRate.constantMax = Mathf.Lerp(m_ballsEmitterRate.constantMax, 0.00f, 0.1f);
            //m_ballsEmitter.rate = m_ballsEmitterRate;
        }
        else if (!m_playerState.m_sprinting)
        {
            m_balls.startLifetime = Mathf.Lerp(m_balls.startLifetime, 5.0f, 0.3f);
            m_glow.startSize = Mathf.Lerp(m_glow.startSize, 0.5f, 0.3f);

            //m_ballsEmitterRate.constantMax = Mathf.Lerp(m_ballsEmitterRate.constantMax, 50.0f, 0.1f);
            //m_ballsEmitter.rate = m_ballsEmitterRate;
        }
	}
}
