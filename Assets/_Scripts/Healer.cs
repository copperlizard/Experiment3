using UnityEngine;
using System.Collections;

public class Healer : MonoBehaviour
{
    public GameObject m_goblin;

    private GoblinStateInfo m_goblinState;

	// Use this for initialization
	void Start ()
    {
        m_goblinState = m_goblin.GetComponent<GoblinStateInfo>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (m_goblinState.m_health < 1.0f)
        {
            m_goblinState.m_health = 1.0f;
        }
	}

    void FixedUpdate()
    {
        transform.Rotate(1.0f, 1.0f, 1.0f);
    }
}
