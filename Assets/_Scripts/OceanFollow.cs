using UnityEngine;
using System.Collections;

public class OceanFollow : MonoBehaviour
{
    public GameObject m_player;

	// Use this for initialization
	void Start ()
    {
	    if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.position = new Vector3(m_player.transform.position.x, transform.position.y, m_player.transform.position.z);
	}
}
