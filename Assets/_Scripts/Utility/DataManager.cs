using UnityEngine;
using System.Collections;

public class DataManager : MonoBehaviour
{
    public static DataManager m_this;

    private GameObject m_player;

    private PlayerStateInfo m_savedPlayerState;

    private bool m_playerStateLoaded = false;

	// Use this for initialization
	void Awake ()
    {
	    if (!m_this)
        {
            DontDestroyOnLoad(gameObject);
            m_this = this;

            m_savedPlayerState = gameObject.AddComponent<PlayerStateInfo>();
        }
        else if (m_this != this)
        {
            Destroy(gameObject);
        }

        m_player = GameObject.FindGameObjectWithTag("Player");
	}

    public void SavePlayerState ()
    {
        if (!m_playerStateLoaded)
        {
            LoadPlayerState();
        }

        if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }

        PlayerStateInfo toSave = m_player.GetComponent<PlayerStateInfo>();

        toSave.m_aiming = m_savedPlayerState.m_aiming;
        toSave.m_armed = m_savedPlayerState.m_armed;
        toSave.m_arrowCharge = m_savedPlayerState.m_arrowCharge;
        toSave.m_arrowMode = m_savedPlayerState.m_arrowMode;
        toSave.m_crouching = m_savedPlayerState.m_crouching;
        toSave.m_firing = m_savedPlayerState.m_firing;
        toSave.m_forwardAmount = m_savedPlayerState.m_forwardAmount;
        toSave.m_gravLocked = false;
        toSave.m_grounded = m_savedPlayerState.m_grounded;
        toSave.m_health = m_savedPlayerState.m_health;
        toSave.m_jumping = m_savedPlayerState.m_jumping;
        toSave.m_magicMode = m_savedPlayerState.m_magicMode;
        toSave.m_mana = m_savedPlayerState.m_mana;
        toSave.m_sidewaysAmount = m_savedPlayerState.m_sidewaysAmount;
        toSave.m_sprinting = m_savedPlayerState.m_sprinting;
        toSave.m_surfing = false;
        toSave.m_interacting = false;
        toSave.m_turnTarAng = m_savedPlayerState.m_turnTarAng;
    }

    public void LoadPlayerState ()
    {
        if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }

        PlayerStateInfo toLoad = m_player.GetComponent<PlayerStateInfo>();

        m_savedPlayerState.m_aiming = toLoad.m_aiming;
        m_savedPlayerState.m_armed = toLoad.m_armed;
        m_savedPlayerState.m_arrowCharge = toLoad.m_arrowCharge;
        m_savedPlayerState.m_arrowMode = toLoad.m_arrowMode;
        m_savedPlayerState.m_crouching = toLoad.m_crouching;
        m_savedPlayerState.m_firing = toLoad.m_firing;
        m_savedPlayerState.m_forwardAmount = toLoad.m_forwardAmount;
        m_savedPlayerState.m_gravLocked = toLoad.m_gravLocked;
        m_savedPlayerState.m_grounded = toLoad.m_grounded;
        m_savedPlayerState.m_health = toLoad.m_health;
        m_savedPlayerState.m_jumping = toLoad.m_jumping;
        m_savedPlayerState.m_magicMode = toLoad.m_magicMode;
        m_savedPlayerState.m_mana = toLoad.m_mana;
        m_savedPlayerState.m_sidewaysAmount = toLoad.m_sidewaysAmount;
        m_savedPlayerState.m_sprinting = toLoad.m_sprinting;
        m_savedPlayerState.m_surfing = toLoad.m_surfing;
        m_savedPlayerState.m_interacting = toLoad.m_interacting;
        m_savedPlayerState.m_turnTarAng = toLoad.m_turnTarAng;        

        if (toLoad != null)
        {
            m_playerStateLoaded = true;
        }
    }

    void OnLevelWasLoaded ()
    {
        m_player = GameObject.FindGameObjectWithTag("Player");
        
        if (m_playerStateLoaded)
        {
            SavePlayerState();
        }
    }
}
