using UnityEngine;
using System.Collections;

public class Hoverboard : MonoBehaviour
{
    public GameObject m_player, m_cam;
    public GameManager m_gameManager;

    public float m_hoverForce, m_hoverDistance, m_stationaryTurnSpeed = 180.0f, m_movingTurnSpeed = 360.0f;

    private Rigidbody m_boardRB, m_playerRB;
    private PlayerStateInfo m_playerState;
    private PlayerMovementController m_playerMover;

    private Renderer m_boardRenderer;

    private float m_v, m_h, m_turnTarAng, m_turn;

    private bool m_playerOnBoard = false, m_playerInteracting = false, m_playerSurfing = false, m_surfLocked = false, m_playerDismounting = false;

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

        m_boardRenderer = GetComponentInChildren<Renderer>();
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
            Vector3 front = m_boardRenderer.bounds.center + new Vector3(0.0f, 0.1f, m_boardRenderer.bounds.extents.z);
            Vector3 back = m_boardRenderer.bounds.center - new Vector3(0.0f, -0.1f, m_boardRenderer.bounds.extents.z);
            Vector3 left = m_boardRenderer.bounds.center - new Vector3(m_boardRenderer.bounds.extents.x, -0.1f, 0.0f);
            Vector3 right = m_boardRenderer.bounds.center + new Vector3(m_boardRenderer.bounds.extents.x, 0.1f, 0.0f);

            RaycastHit hit;
            if (Physics.Raycast(front, -transform.up, out hit, m_hoverDistance + 0.1f, ~LayerMask.GetMask("Player", "PlayerBubble"), QueryTriggerInteraction.Ignore))
            {
                HoverForce(hit.point, front);
            }

            if (Physics.Raycast(back, -transform.up, out hit, m_hoverDistance + 0.1f, ~LayerMask.GetMask("Player", "PlayerBubble"), QueryTriggerInteraction.Ignore))
            {
                HoverForce(hit.point, back);
            }

            if (Physics.Raycast(left, -transform.up, out hit, m_hoverDistance + 0.1f, ~LayerMask.GetMask("Player", "PlayerBubble"), QueryTriggerInteraction.Ignore))
            {
                HoverForce(hit.point, left);
            }

            if (Physics.Raycast(right, -transform.up, out hit, m_hoverDistance + 0.1f, ~LayerMask.GetMask("Player", "PlayerBubble"), QueryTriggerInteraction.Ignore))
            {
                HoverForce(hit.point, right);
            }

            //Board drive
            Vector3 move = new Vector3(m_h, 0.0f, m_v).normalized;            

            if (move.magnitude > 0.0f)
            {
                // Rotate input to match camera
                move = Quaternion.Euler(0.0f, m_cam.transform.eulerAngles.y, 0.0f) * move;

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

                //m_boardRB.AddForce(move.normalized * 500.0f); //LERP TO MOVE VELOCITY INSTEAD
            }

            //Stick player to board
            m_player.transform.position = transform.position + new Vector3(0.0f, 0.11f, 0.0f);
            //m_player.transform.position = transform.position + new Vector3(0.0f, m_boardRenderer.bounds.size.y, 0.0f);
            m_player.transform.rotation = transform.rotation;
        }
    }

    private void HoverForce (Vector3 hitPoint, Vector3 hoverPos)
    {
        Debug.DrawLine(hoverPos, hitPoint, Color.blue);

        float distance, hoverForceFactor, localHoverForce;
        Vector3 forceLine = hoverPos - hitPoint;

        distance = forceLine.magnitude;
        hoverForceFactor = 1.0f - (distance / m_hoverDistance);

        localHoverForce = m_hoverForce * Mathf.SmoothStep(0.0f, 1.0f, hoverForceFactor);

        //Debug.Log("localHoverForce == " + localHoverForce.ToString());

        m_boardRB.AddForceAtPosition(forceLine.normalized * localHoverForce, hoverPos, ForceMode.Acceleration);
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
