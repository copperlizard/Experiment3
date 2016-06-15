using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class FiredArrow : MonoBehaviour
{
    public GameObject m_arrow;

    private Rigidbody m_rigidBody;

	// Use this for initialization
	void Start ()
    {
        m_rigidBody = GetComponent<Rigidbody>();	
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (m_rigidBody.velocity.magnitude > 0.0f)
        {
            transform.rotation = Quaternion.LookRotation(m_rigidBody.velocity) * Quaternion.Euler(90.0f, 0.0f, 0.0f);
        }   
	}
}
