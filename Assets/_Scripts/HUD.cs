using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD : MonoBehaviour
{
    public GameObject m_player;

    public Image m_targetingReticule;

    private PlayerStateInfo m_playerState;

	// Use this for initialization
	void Start ()
    {
	    if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }

        m_playerState = m_player.GetComponent<PlayerStateInfo>();
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
}
