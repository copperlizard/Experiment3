using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerStateInfo))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovementController : MonoBehaviour
{
    public GameObject m_cam;

    public TerrainManager m_terrainManager;

    public float m_groundCheckDist = 0.1f, m_headCheckDist = 1.7f, m_headCheckGroundOffset = 0.1f;

    [System.Serializable]
    public struct SFX
    {
        [HideInInspector]
        public AudioSource m_footSoundEffectsSource;
        public AudioClip m_miscSurface, m_grassSurface, m_mudSurface, m_rockSurface;
    }
    public SFX m_footSoundEffects;

    private PlayerStateInfo m_playerState;

    private RaycastHit m_groundHit;

    private Rigidbody m_playerRigidbody;

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

        m_playerRigidbody = GetComponent<Rigidbody>();

        if (m_terrainManager == null)
        {
            m_terrainManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<TerrainManager>();
        }
        
        m_footSoundEffects.m_footSoundEffectsSource = GetComponent<AudioSource>();        
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void FixedUpdate ()
    {
        m_playerState.m_grounded = CheckGround() ^ m_playerState.m_gravLocked;

        if (m_playerState.m_grounded)
        {
            if (m_groundHit.collider.gameObject.name == "Terrain")
            {
                int surface = m_terrainManager.GetActiveTerrainTextureId(m_groundHit.point);

                switch (surface)
                {
                    case 0:
                        m_footSoundEffects.m_footSoundEffectsSource.clip = m_footSoundEffects.m_mudSurface;
                        break;
                    case 1:
                        m_footSoundEffects.m_footSoundEffectsSource.clip = m_footSoundEffects.m_rockSurface;
                        break;
                    case 3:
                        m_footSoundEffects.m_footSoundEffectsSource.clip = m_footSoundEffects.m_grassSurface;
                        break;
                    default:
                        m_footSoundEffects.m_footSoundEffectsSource.clip = m_footSoundEffects.m_miscSurface;
                        break;
                }
            }   
            else
            {
                m_footSoundEffects.m_footSoundEffectsSource.clip = m_footSoundEffects.m_miscSurface;
            }         
        }

        m_playerState.m_crouching = CheckHead();

        AudioAlertEnemies();
    }

    public void Move (float v, float h)
    {
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

        if (!m_playerState.m_grounded)
        {
            AirMove(move);
        }
    }

    private bool CheckGround ()
    {
        /*   
#if UNITY_EDITOR
        Debug.DrawLine(transform.position + (Vector3.up * m_groundCheckDist * 0.5f), (transform.position + (Vector3.up * m_groundCheckDist * 0.5f)) + Vector3.down * m_groundCheckDist);
#endif
        */
       
        if (Physics.Raycast(transform.position + (Vector3.up * m_groundCheckDist * 0.5f), Vector3.down, out m_groundHit, m_groundCheckDist, ~LayerMask.GetMask("Player", "PlayerBubble")))
        {
            m_groundNormal = m_groundHit.normal;
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
            
            if (Physics.Raycast(startPos, Vector3.up, out m_groundHit, m_headCheckDist, ~LayerMask.GetMask("Player", "PlayerBubble", "Ignore Raycast", "Enemy")))
            {                
                m_playerState.m_sprinting = false;
                m_playerState.m_jumping = false;

                //Hit head

                Debug.Log("hit head on " + m_groundHit.transform.name);

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

    private void AudioAlertEnemies ()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, m_playerRigidbody.velocity.magnitude * 1.5f);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].tag == "Goblin")
            {
                GoblinStateInfo thisGoblin = hits[i].GetComponent<GoblinStateInfo>();

                if (thisGoblin != null)
                {
                    thisGoblin.m_alert = true;
                    thisGoblin.m_sprinting = true;
                }
            }
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

    private void AirMove (Vector3 move)
    {
        // Rotate input to match camera
        move = Quaternion.Euler(0.0f, m_cam.transform.eulerAngles.y, 0.0f) * move;

        m_playerRigidbody.AddForce(move * 100.0f);
    }
}
