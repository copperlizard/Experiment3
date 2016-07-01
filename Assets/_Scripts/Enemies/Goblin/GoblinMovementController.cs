using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GoblinStateInfo))]
[RequireComponent(typeof(Rigidbody))]
public class GoblinMovementController : MonoBehaviour
{
    public float m_groundCheckDist = 0.2f;

    private GoblinStateInfo m_goblinState;

    private Rigidbody m_goblinRigidbody;

    private Vector3 m_groundNormal;

    private float m_sprintInputModifier = 2.0f;

    // Use this for initialization
    void Start ()
    {
        m_goblinState = GetComponent<GoblinStateInfo>();
        m_goblinRigidbody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void FixedUpdate ()
    {
        m_goblinState.m_grounded = CheckGround() ^ (m_goblinState.m_swept || m_goblinState.m_gravLocked);
    }

    public void Move(float v, float h)
    {
        Vector3 move = new Vector3(h, 0.0f, v).normalized;

        Debug.DrawLine(transform.position, transform.position + move, Color.yellow);

        // Apply move speed modifier
        if (m_goblinState.m_sprinting)
        {
            move *= m_sprintInputModifier;
        }        

        if (m_goblinState.m_grounded)
        {
            NormalMove(move);
        }
        else
        {
            AirMove(move);
        }

    }

    private bool CheckGround ()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + (Vector3.up * m_groundCheckDist * 0.5f), Vector3.down, out hit, m_groundCheckDist, ~LayerMask.GetMask("Goblin", "PlayerBubble")))
        {
            m_groundNormal = hit.normal;
            return true;
        }

        return false;
    }

    private void NormalMove (Vector3 move)
    {
        if (move.magnitude > 0.0f)
        {
            // Look "forward"
            Quaternion lookRot = Quaternion.LookRotation(move);
            m_goblinState.m_turnTarAng = lookRot.eulerAngles.y;

            // Account for "hills"
            move = Vector3.ProjectOnPlane(move, m_groundNormal);
        }
        else
        {
            //Stop rotating
            m_goblinState.m_turnTarAng = transform.rotation.eulerAngles.y;
        }

        m_goblinState.m_forwardAmount = move.magnitude;
        m_goblinState.m_sidewaysAmount = 0.0f;
    }

    private void AirMove (Vector3 move)
    {
        //m_goblinRigidbody.AddForce(move * 100.0f);
    }
}
