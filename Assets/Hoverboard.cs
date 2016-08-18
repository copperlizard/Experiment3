using UnityEngine;
using System.Collections;

public class Hoverboard : MonoBehaviour
{
    public GameObject m_player, m_cam;
    public GameManager m_gameManager;

    public float m_hoverForce, m_hoverDistance, m_stationaryTurnSpeed = 180.0f, m_movingTurnSpeed = 360.0f, m_boardJumpForce = 50.0f, m_boardJumpChargeRate = 0.1f;

    private Rigidbody m_boardRB, m_playerRB;
    private PlayerStateInfo m_playerState;
    private PlayerMovementController m_playerMover;

    private MeshFilter  m_boardMeshFilter;
    private Mesh m_boardMesh;

    private Vector3 m_meshExtents;

    private float m_v, m_h, m_turnTarAng, m_turn, m_boardJumpCharge;

    private bool m_playerOnBoard = false, m_playerInteracting = false, m_playerSurfing = false, m_surfLocked = false, 
        m_playerDismounting = false, m_playerFalling = false, m_boardJumping = false, m_boardJumpLock = false,
        m_boardGrounded = true;

	// Use this for initialization
	void Start ()
    {
	    if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }

        if (m_cam == null)
        {
            m_cam = Camera.main.gameObject;
        }

        if (m_gameManager == null)
        {
            m_gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        }

        m_boardRB = GetComponent<Rigidbody>();
        m_playerRB = m_player.GetComponent<Rigidbody>();
        m_playerState = m_player.GetComponent<PlayerStateInfo>();
        m_playerMover = m_player.GetComponent<PlayerMovementController>();

        m_boardMeshFilter = GetComponentInChildren<MeshFilter>();
        m_boardMesh = m_boardMeshFilter.mesh;        
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (m_gameManager.m_paused)
        {
            return;
        }

        if (m_playerSurfing)
        {
            m_v = Input.GetAxis("Vertical");
            m_h = Input.GetAxis("Horizontal");

            m_boardJumping = Input.GetButton("Jump");
        }

        //Get on/off board
        if (m_playerOnBoard)
        {
            if (!m_surfLocked)
            {
                if (Input.GetButton("Interact"))
                {
                    //Debug.Log("interact/disinteract with board!");

                    if (m_playerSurfing && !m_playerDismounting)
                    {
                        StartCoroutine(PlayerDismount());                        
                    }

                    m_playerInteracting = !m_playerInteracting;
                    m_playerState.m_interacting = m_playerInteracting; //set player state
                    m_surfLocked = true;

                    StartCoroutine(PositionPlayer());
                }
            }
            else
            {
                if (!Input.GetButton("Interact"))
                {
                    m_surfLocked = false;
                }
            }
        }        
	}

    void FixedUpdate ()
    {
        //Control board
        if (m_playerSurfing)
        {
            //Board Hover
            Vector3 center = transform.position + m_boardMesh.bounds.center + new Vector3(0.0f, 0.5f, 0.0f);
            Vector3 front = transform.position + m_boardMesh.bounds.center + transform.rotation * new Vector3(0.0f, 0.5f, m_boardMesh.bounds.extents.z * m_boardMeshFilter.transform.localScale.z);
            Vector3 back = transform.position + m_boardMesh.bounds.center - transform.rotation * new Vector3(0.0f, -0.5f, m_boardMesh.bounds.extents.z * m_boardMeshFilter.transform.localScale.z);
            Vector3 left = transform.position + m_boardMesh.bounds.center - transform.rotation * new Vector3(m_boardMesh.bounds.extents.x * m_boardMeshFilter.transform.localScale.x, -0.5f, 0.0f);
            Vector3 right = transform.position + m_boardMesh.bounds.center + transform.rotation * new Vector3(m_boardMesh.bounds.extents.x * m_boardMeshFilter.transform.localScale.x, 0.5f, 0.0f);

            bool noGround = true;

            RaycastHit hit;
            if (Physics.Raycast(center, -transform.up, out hit, m_hoverDistance + 0.5f, ~LayerMask.GetMask("Player", "PlayerBubble"), QueryTriggerInteraction.Ignore))
            {
                noGround = false;
                HoverForce(hit.point, center);
            }

            if (Physics.Raycast(front, -transform.up, out hit, m_hoverDistance + 0.5f, ~LayerMask.GetMask("Player", "PlayerBubble"), QueryTriggerInteraction.Ignore))
            {
                noGround = false;
                HoverForce(hit.point, front);
            }

            if (Physics.Raycast(back, -transform.up, out hit, m_hoverDistance + 0.5f, ~LayerMask.GetMask("Player", "PlayerBubble"), QueryTriggerInteraction.Ignore))
            {
                noGround = false;
                HoverForce(hit.point, back);
            }

            if (Physics.Raycast(left, -transform.up, out hit, m_hoverDistance + 0.5f, ~LayerMask.GetMask("Player", "PlayerBubble"), QueryTriggerInteraction.Ignore))
            {
                noGround = false;
                HoverForce(hit.point, left);
            }

            if (Physics.Raycast(right, -transform.up, out hit, m_hoverDistance + 0.5f, ~LayerMask.GetMask("Player", "PlayerBubble"), QueryTriggerInteraction.Ignore))
            {
                noGround = false;
                HoverForce(hit.point, right);
            }

            if (noGround)
            {
                Debug.Log("no ground!");
                m_boardGrounded = false;
            }
            else
            {
                m_boardGrounded = true;
            }

            //Board drive
            Vector3 move = new Vector3(m_h, 0.0f, m_v).normalized;            

            if (move.magnitude > 0.0f)
            {
                // Rotate input to match camera
                move = Quaternion.Euler(0.0f, m_cam.transform.eulerAngles.y, 0.0f) * move;

                //Project to board plane
                move = Vector3.ProjectOnPlane(move, transform.up);

                // Look "forward"
                Quaternion lookRot = Quaternion.LookRotation(move);
                m_turnTarAng = lookRot.eulerAngles.y;

                m_turn = m_turnTarAng - transform.rotation.eulerAngles.y;

                if (Mathf.Abs(m_turn) > 180.0f)
                {
                    if (transform.rotation.eulerAngles.y < m_turnTarAng)
                    {
                        m_turn = m_turnTarAng - (transform.rotation.eulerAngles.y + 360.0f);
                    }
                    else
                    {
                        m_turn = (m_turnTarAng + 360.0f) - transform.rotation.eulerAngles.y;
                    }
                }
                m_turn /= 180.0f;

                RotateBoard(m_turn);

                float presY = m_boardRB.velocity.y;                
                m_boardRB.velocity = Vector3.Lerp(m_boardRB.velocity, move.normalized * 20.0f, 0.1f);
                m_boardRB.velocity = new Vector3(m_boardRB.velocity.x, presY, m_boardRB.velocity.z);
            }

            if (m_boardJumping)
            {
                BoardJump();
            }

            //Stick player to board
            m_player.transform.position = transform.position + new Vector3(0.0f, 0.11f, 0.0f);
            //m_player.transform.position = transform.position + new Vector3(0.0f, m_boardRenderer.bounds.size.y, 0.0f);
            m_player.transform.rotation = transform.rotation;

            //Player fall off
            if (Vector3.Dot(transform.up, Vector3.up) <= 0.0f && !m_playerFalling)
            {
                StartCoroutine(PlayerFallOff());
            }
        }
    }

    private void HoverForce (Vector3 hitPoint, Vector3 hoverPos)
    {
        Debug.DrawLine(hoverPos, hitPoint, Color.blue);

        float distance, hoverForceFactor, localHoverForce;
        Vector3 forceLine = hoverPos - hitPoint;

        distance = forceLine.magnitude;
        hoverForceFactor = 1.0f - (distance / (m_hoverDistance + 0.5f));

        localHoverForce = m_hoverForce * Mathf.SmoothStep(0.0f, 1.0f, hoverForceFactor);

        //Debug.Log("localHoverForce == " + localHoverForce.ToString());

        Debug.DrawLine(hoverPos, hoverPos + forceLine.normalized * localHoverForce, Color.red);

        m_boardRB.AddForceAtPosition(transform.up * localHoverForce, hoverPos);
    }

    void RotateBoard(float ang)
    {
        float turnSpeed = Mathf.Lerp(m_stationaryTurnSpeed, m_movingTurnSpeed, m_playerState.m_forwardAmount);

        if (m_playerState.m_aiming)
        {
            turnSpeed *= 4.0f;
            m_turn *= 4.0f;
        }

        transform.Rotate(0, ang * turnSpeed * Time.deltaTime, 0);
    }

    void BoardJump()
    {
        if (!m_boardJumpLock)
        {
            m_boardJumpLock = true;
            StartCoroutine(ChargeBoardJump());
        }
    }

    IEnumerator ChargeBoardJump()
    {
        while (m_boardJumping)
        {
            m_boardJumpCharge = Mathf.SmoothStep(m_boardJumpCharge, 1.0f, m_boardJumpChargeRate);
            //m_jumpSoundEffectSource.pitch = m_jumpCharge;

            //m_jumpEffectEmissionRate.constantMax = 50.0f * m_jumpCharge;
            //m_jumpEffectEmission.rate = m_jumpEffectEmissionRate;

            Debug.Log("m_jumpCharge == " + m_boardJumpCharge.ToString());

            yield return null;
        }

        if (m_boardGrounded)
        {
            m_boardGrounded = false;
            m_boardRB.AddForce(Vector3.up * (m_boardJumpForce * Mathf.Clamp(m_boardJumpChargeRate, 0.2f, 1.0f)), ForceMode.VelocityChange);
        }

        m_boardJumpCharge = 0.0f;
        //m_jumpSoundEffectSource.pitch = m_jumpCharge;        
        //m_jumpEffectEmissionRate.constantMax = 0.0f;
        //m_jumpEffectEmission.rate = m_jumpEffectEmissionRate;
        //m_jumpEffect.Stop();

        m_boardJumpLock = false;
        yield return null;
    }

    IEnumerator PositionPlayer ()
    {
        while (!m_playerSurfing && m_playerInteracting)
        {
            //position player here
            Vector2 toPos = new Vector2(transform.position.x, transform.position.z) - new Vector2(m_player.transform.position.x, m_player.transform.position.z);
            //Debug.DrawLine(new Vector3(m_player.transform.position.x, 2.0f, m_player.transform.position.z), new Vector3(m_player.transform.position.x, 2.0f, m_player.transform.position.z) + new Vector3(toPos.x, 0.0f, toPos.y), Color.blue);

            Vector2 move = toPos.normalized;

            m_playerMover.MoveToInteract(move.y, move.x, transform.rotation.eulerAngles.y);

            m_player.transform.position = Vector3.Lerp(m_player.transform.position, transform.position, 0.1f);

            //Positioned
            if (toPos.magnitude < 0.05f)
            {
                m_playerState.m_surfing = true;
                m_playerSurfing = true;
                m_player.transform.parent = transform;
                m_playerRB.isKinematic = true;
                m_playerRB.useGravity = false;
                m_playerRB.constraints = RigidbodyConstraints.None;      
            }
            
            yield return null;
        }

        yield return null;
    }

    IEnumerator PlayerDismount ()
    {
        m_playerDismounting = true;

        while (Vector3.Dot(m_player.transform.up, Vector3.up) <= 0.99f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0.0f, transform.rotation.eulerAngles.y, 0.0f), 0.1f);
            yield return null;
        }

        m_playerSurfing = false;
        m_playerState.m_surfing = false;
        m_player.transform.parent = null;
        m_playerRB.isKinematic = false;
        m_playerRB.useGravity = true;
        m_playerRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        m_playerDismounting = false;

        m_player.transform.rotation = Quaternion.Euler(0.0f, m_player.transform.rotation.eulerAngles.y, 0.0f);

        yield return null;
    }

    IEnumerator PlayerFallOff ()
    {
        m_playerFalling = true;

        m_playerSurfing = false;
        m_playerState.m_interacting = false;
        m_playerState.m_surfing = false;
        m_player.transform.parent = null;
        m_playerRB.isKinematic = false;
        m_playerRB.useGravity = true;
        

        while (Vector3.Dot(m_player.transform.up, Vector3.up) <= 0.90f)
        {
            Debug.Log("player falling upright!");

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0.0f, transform.rotation.eulerAngles.y, 0.0f), 0.1f); //board falls right way up
            m_player.transform.rotation = Quaternion.Lerp(m_player.transform.rotation, Quaternion.Euler(0.0f, m_player.transform.rotation.eulerAngles.y, 0.0f), 0.1f); //player falls right way up
            yield return null;
        }

        Debug.Log("player is upright enough!");

        m_playerRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        m_player.transform.rotation = Quaternion.Euler(0.0f, m_player.transform.rotation.eulerAngles.y, 0.0f);

        m_playerFalling = false;

        yield return null;
    }

    void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Player")
        {
            m_playerOnBoard = true;
        }
    }

    void OnTriggerExit (Collider other)
    {
        if (other.tag == "Player")
        {
            m_playerOnBoard = false;
        }
    }
}
