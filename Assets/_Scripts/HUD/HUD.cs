using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD : MonoBehaviour
{
    public GameObject m_player;

    public Image m_targetingReticule, m_hitIndicator;

    public float m_hitIndicationTime;

    private PlayerStateInfo m_playerState;

    private AudioSource m_hitSoundSource;

    private bool m_hitIndicatorLock = false;

	// Use this for initialization
	void Start ()
    {
	    if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }

        m_playerState = m_player.GetComponent<PlayerStateInfo>();
        
        m_hitSoundSource = m_hitIndicator.GetComponent<AudioSource>();

        m_hitIndicator.enabled = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (m_playerState.m_aiming && !m_targetingReticule.enabled)
        {
            m_targetingReticule.enabled = true;
        }
        else if (!m_playerState.m_aiming && m_targetingReticule.enabled)
        {
            m_targetingReticule.enabled = false;
        }
	}

    public void IndicateHit ()
    {
        if (!m_hitIndicatorLock)
        {
            m_hitIndicatorLock = true;
            StartCoroutine(HitIndicate());
        }
        
    }

    IEnumerator HitIndicate ()
    {
        m_hitSoundSource.Play();
        
        float startTime = Time.time;
        
        while(startTime + m_hitIndicationTime >= Time.time)
        {
            if (m_playerState.m_aiming && !m_hitIndicator.enabled)
            {
                m_hitIndicator.enabled = true;
            }
            else if (!m_playerState.m_aiming && m_hitIndicator.enabled)
            {
                m_hitIndicator.enabled = false;
            }

            yield return null;
        }
        
        if (m_hitIndicator.enabled)
        {
            m_hitIndicator.enabled = false;
        }
        
        m_hitIndicatorLock = false;
        yield return null;
    }
}
