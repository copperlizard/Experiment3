using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GameObject m_player, m_pauseMenu;
    
    [HideInInspector]
    public bool m_paused = false;

    private PlayerStateInfo m_playerState;

    private bool m_pauseLock = false;

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
        if (!m_pauseLock && Input.GetButton("Pause"))
        {
            m_pauseLock = true;
            m_paused = !m_paused;
        }
        else if (!Input.GetButton("Pause"))
        {
            m_pauseLock = false;
        }

        if (m_paused && !m_pauseMenu.activeInHierarchy)
        {
            m_pauseMenu.SetActive(true);
            Time.timeScale = 0.0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }
        else if (!m_paused && m_pauseMenu.activeInHierarchy)
        {
            m_pauseMenu.SetActive(false);
            Time.timeScale = 1.0f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
