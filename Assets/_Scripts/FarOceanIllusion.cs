using UnityEngine;
using System.Collections;

public class FarOceanIllusion : MonoBehaviour
{
    public GameObject m_player;

    private Vector3 m_offset;

	// Use this for initialization
	void Start ()
    {
	    if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }

        m_offset = transform.position - m_player.transform.position;       
    }
	
	// Update is called once per frame
	void Update ()
    {
        transform.position = m_player.transform.position + m_offset;
	}
}
