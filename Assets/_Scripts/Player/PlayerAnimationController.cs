using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerAnimationController : MonoBehaviour
{
    public Camera m_cam;

    public ObjectPool m_firedArrows;

    public GameObject m_bow, m_magic, m_drawnArrow, m_drawingArrow;

    public Transform m_leftHandDrawnArrowTransform;    
    
    public float m_jumpForce = 1.0f, m_animSpeedMultiplier = 1.0f, m_MoveSpeedMultiplier = 1.0f, m_crouchSpeedModifier = 1.0f,
        m_sprintSpeedModifier = 1.0f, m_runCycleLegOffset = 0.2f, m_stationaryTurnSpeed = 180.0f, m_movingTurnSpeed = 360.0f;

    private OrbitCam m_orbitCam;
    private PlayerStateInfo m_playerState;

    private Animator m_playerAnimator;
    private Transform m_spineTransform, m_leftShoulderTransform, m_rightShoulderTransform;

    private Rigidbody m_playerRigidBody;

    private float m_turn;

    private bool m_fireLock = false;

    // Use this for initialization
    void Start ()
    {
        if (m_cam == null)
        {
            m_cam = Camera.main;
        }
        m_orbitCam = m_cam.GetComponent<OrbitCam>();

        m_playerState = GetComponent<PlayerStateInfo>();

        m_playerAnimator = GetComponent<Animator>();
        m_spineTransform = m_playerAnimator.GetBoneTransform(HumanBodyBones.Spine);
        m_leftShoulderTransform = m_playerAnimator.GetBoneTransform(HumanBodyBones.LeftShoulder);
        m_rightShoulderTransform = m_playerAnimator.GetBoneTransform(HumanBodyBones.RightShoulder);

        if (m_leftShoulderTransform == null)
        {
            Debug.Log("no left shoulder");
        }
        if (m_rightShoulderTransform == null)
        {
            Debug.Log("no right shoulder");
        }

        m_playerRigidBody = GetComponent<Rigidbody>();

        m_drawnArrow.SetActive(false);
        m_drawingArrow.SetActive(false);
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateAnimator();

        if (m_playerState.m_armed)
        {
            if (!m_bow.activeInHierarchy)
            {
                m_bow.SetActive(true);
            }

            if (m_magic.activeInHierarchy)
            {
                m_magic.SetActive(false);
            }

            if (!m_playerState.m_aiming && m_drawnArrow.activeInHierarchy)
            {
                ArrowCanceled();
            }
        }
        else if (!m_playerState.m_armed)
        {
            if (m_bow.activeInHierarchy)
            {
                m_bow.SetActive(false);
            }

            if (m_drawnArrow.activeInHierarchy)
            {
                ArrowCanceled();
            }

            if (m_playerState.m_aiming)
            {
                if (!m_magic.activeInHierarchy)
                {
                    m_magic.SetActive(true);
                }
            }
            else
            {
                if (m_magic.activeInHierarchy)
                {
                    m_magic.SetActive(false);
                }
            }
        }        
	}

    private void UpdateAnimator ()
    {
        m_turn = m_playerState.m_turnTarAng - transform.rotation.eulerAngles.y;

        if (Mathf.Abs(m_turn) > 180.0f)
        {
            if (transform.rotation.eulerAngles.y < m_playerState.m_turnTarAng)
            {
                m_turn = m_playerState.m_turnTarAng - (transform.rotation.eulerAngles.y + 360.0f);
            }
            else
            {
                m_turn = (m_playerState.m_turnTarAng + 360.0f) - transform.rotation.eulerAngles.y;
            }
        }
        m_turn /= 180.0f;

        RotatePlayer(m_turn);

        PlayerJump();

        // update the animator parameters
        m_playerAnimator.SetLayerWeight(1, (m_playerState.m_aiming) ? Mathf.Lerp(m_playerAnimator.GetLayerWeight(1), 1.0f, 0.1f) : Mathf.Lerp(m_playerAnimator.GetLayerWeight(1), 0.0f, 0.1f)); //set aiming layer weight
        m_playerAnimator.SetFloat("Forward", m_playerState.m_forwardAmount, 0.1f, Time.deltaTime);
        m_playerAnimator.SetFloat("Sideways", m_playerState.m_sidewaysAmount, 0.1f, Time.deltaTime);
        m_playerAnimator.SetFloat("Turn", m_turn, 0.1f, Time.deltaTime);
        m_playerAnimator.SetBool("OnGround", m_playerState.m_grounded);
        m_playerAnimator.SetBool("Aiming", m_playerState.m_aiming);
        m_playerAnimator.SetBool("Crouch", m_playerState.m_crouching);
        m_playerAnimator.SetBool("Armed", m_playerState.m_armed);

        if (!m_playerState.m_grounded)
        {
            m_playerAnimator.SetFloat("Jump", m_playerRigidBody.velocity.y);
        }

        // check if sliding
        AnimatorStateInfo curAnimState = m_playerAnimator.GetCurrentAnimatorStateInfo(0);
        bool isSliding = curAnimState.IsName("Sliding");

        // calculate which leg is behind, so as to leave that leg trailing in the jump animation
        // (This code is reliant on the specific run cycle offset in our animations,
        // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
        float runCycle =
            Mathf.Repeat(
                m_playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_runCycleLegOffset, 1);
        float jumpLeg = (runCycle < 0.5f ? 1 : -1) * m_playerState.m_forwardAmount;
        if (m_playerState.m_grounded && !isSliding)
        {
            m_playerAnimator.SetFloat("JumpLeg", jumpLeg);
        }

        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        if (m_playerState.m_grounded && (m_playerState.m_forwardAmount != 0 || m_playerState.m_sidewaysAmount != 0))
        {
            m_playerAnimator.speed = m_animSpeedMultiplier;
        }
        else
        {
            // don't use that while airborne or turning in place
            m_playerAnimator.speed = 1;
        }

        //Manual state changes...

        AnimatorStateInfo torsoStateInfo = m_playerAnimator.GetCurrentAnimatorStateInfo(1);
        
        if (m_playerState.m_firing && !m_fireLock && torsoStateInfo.IsName("ArmedDraw") && torsoStateInfo.normalizedTime >= 1)
        {
            //Debug.Log("charging/firing arrow!");

            m_fireLock = true;
            StartCoroutine(ChargeArrow());
        }
    }

    IEnumerator ChargeArrow ()
    {
        
        m_playerAnimator.Play("ArmedCharging", 1);

        AnimatorStateInfo stateInfo = m_playerAnimator.GetCurrentAnimatorStateInfo(1);
        while (stateInfo.IsName("ArmedCharging"))
        {   
            stateInfo = m_playerAnimator.GetCurrentAnimatorStateInfo(1);

            m_playerState.m_arrowCharge = (stateInfo.normalizedTime > 1.0f) ? 1.0f : stateInfo.normalizedTime;

            yield return null;
        }        

        while (m_playerState.m_firing)
        {
            yield return null;
        }

        m_playerAnimator.Play("ArmedFire", 1);
        
        m_fireLock = false;
    }

    private void OnAnimatorMove ()
    {
        if (m_playerState.m_grounded && Time.deltaTime > 0)
        {
            Vector3 v = (m_playerAnimator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

            //Debug.Log("v == " + v.ToString());

            // Apply move speed modifier
            if (m_playerState.m_crouching)
            {
                v *= m_crouchSpeedModifier;
            }
            else if (m_playerState.m_sprinting)
            {
                v *= m_sprintSpeedModifier;
            }

            // we preserve the existing y part of the current velocity.
            v.y = m_playerRigidBody.velocity.y;
            m_playerRigidBody.velocity = v;
        }
    }

    private void OnAnimatorIK (int layer)
    {        
        if (m_playerState.m_aiming && m_playerState.m_armed)
        {
            //Vector3 tar = m_cam.transform.position + m_cam.transform.forward * 10.0f;
            Vector3 tar = m_orbitCam.m_hit.point;
            Vector3 toTar = tar - m_rightShoulderTransform.position;

            Debug.DrawLine(m_rightShoulderTransform.position, m_rightShoulderTransform.position + toTar, Color.red);
            Debug.DrawLine(m_cam.transform.position, m_cam.transform.position + m_cam.transform.forward * 10.0f, Color.yellow);

            toTar = m_spineTransform.InverseTransformDirection(toTar);
            
            Quaternion deltaRot = Quaternion.FromToRotation(m_spineTransform.InverseTransformDirection(m_leftHandDrawnArrowTransform.forward), toTar);

            AnimatorStateInfo stateInfo = m_playerAnimator.GetCurrentAnimatorStateInfo(1);

            if ((stateInfo.IsName("ArmedDraw") && stateInfo.normalizedTime < 1.0f) || stateInfo.IsName("ArmedFire"))
            {
                Vector3 shoulders = m_leftShoulderTransform.position - m_rightShoulderTransform.position;
                shoulders = m_spineTransform.InverseTransformDirection(shoulders);
                deltaRot = Quaternion.FromToRotation(shoulders, toTar) * Quaternion.Euler(0.0f, 3.0f, 11.0f);
            }

            m_spineTransform.localRotation *= deltaRot;

            m_playerAnimator.SetBoneLocalRotation(HumanBodyBones.Spine, m_spineTransform.localRotation);
        }
        else if (m_playerState.m_aiming)
        {
            m_playerAnimator.SetLookAtPosition(m_orbitCam.m_hit.point);
            m_playerAnimator.SetLookAtWeight(1.0f);
        }
        else
        {
            m_playerAnimator.SetLookAtWeight(0.0f);
        }    
    }

    void RotatePlayer (float ang)
    {
        float turnSpeed = Mathf.Lerp(m_stationaryTurnSpeed, m_movingTurnSpeed, m_playerState.m_forwardAmount);

        if (m_playerState.m_aiming)
        {
            turnSpeed *= 4.0f;
            m_turn *= 4.0f;
        }

        transform.Rotate(0, ang * turnSpeed * Time.deltaTime, 0);
    }

    void PlayerJump()
    {
        if (m_playerState.m_jumping && m_playerState.m_grounded)
        {
            m_playerState.m_grounded = false;
            m_playerState.m_jumping = false;
            m_playerRigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.VelocityChange);
        }
    }

    void DrawingArrow ()
    {
        m_drawingArrow.SetActive(true);
    }

    void ArrowDrawn ()
    {
        m_drawingArrow.SetActive(false);
        m_drawnArrow.SetActive(true);
    }

    void ArrowFired ()
    {
        m_drawnArrow.SetActive(false);

        GameObject arrow = m_firedArrows.GetObject();
        Rigidbody arrowRB = arrow.GetComponent<Rigidbody>();

        arrow.transform.position = m_drawnArrow.transform.position;
        arrow.transform.rotation = m_drawnArrow.transform.rotation;

        arrow.SetActive(true);

        arrowRB.WakeUp();
        arrowRB.velocity = arrow.transform.up * (30.0f + 30.0f * m_playerState.m_arrowCharge);

        m_playerState.m_arrowCharge = 0.0f;
    }

    void ArrowCanceled ()
    {
        m_drawnArrow.SetActive(false);
    }
}
