using UnityEngine;
using System.Collections;

public class FarOceanIllusion : MonoBehaviour
{
    public GameObject m_player;

    public Camera m_cam;

    private Vector3 m_offset;

	// Use this for initialization
	void Start ()
    {
	    if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }

        m_offset = transform.position - m_player.transform.position; 
        
        if (m_cam == null)
        {
            m_cam = Camera.main;
        }      
    }
	
	// Update is called once per frame
	void Update ()
    {
        transform.position = m_player.transform.position + m_offset;

        //transform.localRotation = Quaternion.Euler(0.0f, m_cam.transform.localRotation.eulerAngles.y, 0.0f);
	}
}
