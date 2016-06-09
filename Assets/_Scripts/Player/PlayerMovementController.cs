using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerStateInfo))]
public class PlayerMovementController : MonoBehaviour
{
    public GameObject m_cam;

    public float m_groundCheckDist = 0.1f, m_headCheckDist = 1.7f, m_headCheckGroundOffset = 0.1f;

    private PlayerStateInfo m_playerState;

    private Vector3 m_groundNormal;

    private float m_sprintInputModifier = 2.0f;

	// Use this for initialization
	void Start ()
    {
        if (m_cam == null)
        {
            m_cam = Camera.main.gameObject;
        }

        m_playerState = GetComponent<PlayerStateInfo>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void Move (float v, float h)
    {
        m_playerState.m_grounded = CheckGround();
        m_playerState.m_crouching = CheckHead();

        // Airborne move
        if (!m_playerState.m_grounded)
        {
            v = 0.0f;
            h = 0.0f;
            return;
        }

        Vector3 move = new Vector3(h, 0.0f, v).normalized;
        
        // Apply move speed modifier
        if (m_playerState.m_sprinting)
        {
            move *= m_sprintInputModifier;
        }

        if (!m_playerState.m_aiming)
        {
            NormalMove(move);
        }
        else
        {
            AimMove(move);
        }
    }

    private bool CheckGround ()
    {
        /*   
#if UNITY_EDITOR
        Debug.DrawLine(transform.position + (Vector3.up * m_groundCheckDist * 0.5f), (transform.position + (Vector3.up * m_groundCheckDist * 0.5f)) + Vector3.down * m_groundCheckDist);
#endif
        */

        RaycastHit hit;
        if (Physics.Raycast(transform.position + (Vector3.up * m_groundCheckDist * 0.5f), Vector3.down, out hit, m_groundCheckDist))
        {
            m_groundNormal = hit.normal;
            return true;
        }

        return false;
    }

    private bool CheckHead ()
    {
        if (!m_playerState.m_crouching)
        {
            Vector3 startPos = transform.position;
            startPos.y += m_headCheckGroundOffset;
            
#if UNITY_EDITOR
            Debug.DrawLine(startPos, startPos + Vector3.up * m_headCheckDist, Color.green);
#endif            

            if (Physics.Raycast(startPos, Vector3.up, m_headCheckDist, ~LayerMask.GetMask("Player", "Ignore Raycast")))
            {                
                m_playerState.m_sprinting = false;
                m_playerState.m_jumping = false;

                //Hit head
                return true;
            }

            //Clear
            return false;
        }
        else
        {
            //Player crouching
            return true;
        }
    }

    private void NormalMove (Vector3 move)
    {
        if (move.magnitude > 0.0f)
        {
            // Rotate input to match camera
            move = Quaternion.Euler(0.0f, m_cam.transform.eulerAngles.y, 0.0f) * move;

            // Look "forward"
            Quaternion lookRot = Quaternion.LookRotation(move);
            m_playerState.m_turnTarAng = lookRot.eulerAngles.y;

            // Account for "hills"
            move = Vector3.ProjectOnPlane(move, m_groundNormal);
        }
        else
        {
            //Stop rotating
            m_playerState.m_turnTarAng = transform.rotation.eulerAngles.y;
        }

        m_playerState.m_forwardAmount = move.magnitude;
        m_playerState.m_sidewaysAmount = 0.0f;
    }

    private void AimMove (Vector3 move)
    {
        // Rotate input to match camera
        move = Quaternion.Euler(0.0f, m_cam.transform.eulerAngles.y, 0.0f) * move;

        // Rotate player to match input        
        m_playerState.m_turnTarAng = m_cam.transform.eulerAngles.y;

        // Account for "hills"
        move = Vector3.ProjectOnPlane(move, m_groundNormal);

        Vector3 localMove = transform.InverseTransformVector(move);
        m_playerState.m_forwardAmount = localMove.z;
        m_playerState.m_sidewaysAmount = localMove.x;
    }
}
