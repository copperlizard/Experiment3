using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStatusPanel : MonoBehaviour
{
    public GameObject m_player, m_spellsPanel, m_arrowsPanel;

    public Slider m_healthSlider, m_manaSlider;

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
        m_healthSlider.value = m_playerState.m_health;
        m_manaSlider.value = m_playerState.m_mana;

        if (m_playerState.m_armed)
        {
            if (m_spellsPanel.activeInHierarchy)
            {
                m_spellsPanel.SetActive(false);
            }

            if (!m_arrowsPanel.activeInHierarchy)
            {
                m_arrowsPanel.SetActive(true);
            }
        }
        else
        {
            if (!m_spellsPanel.activeInHierarchy)
            {
                m_spellsPanel.SetActive(true);
            }

            if (m_arrowsPanel.activeInHierarchy)
            {
                m_arrowsPanel.SetActive(false);
            }
        }
	
	}
}
