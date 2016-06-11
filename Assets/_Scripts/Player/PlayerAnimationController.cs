using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerAnimationController : MonoBehaviour
{
    public Camera m_cam;

    public float m_jumpForce = 1.0f, m_animSpeedMultiplier = 1.0f, m_MoveSpeedMultiplier = 1.0f, m_crouchSpeedModifier = 1.0f,
        m_sprintSpeedModifier = 1.0f, m_runCycleLegOffset = 0.2f, m_stationaryTurnSpeed = 180.0f, m_movingTurnSpeed = 360.0f;

    //private OrbitCam m_orbitCam;
    private PlayerStateInfo m_playerState;

    private Animator m_playerAnimator;
    private Transform m_spineTransform;

    private Rigidbody m_playerRigidBody;

    private float m_turn;

    // Use this for initialization
    void Start ()
    {
        if (m_cam == null)
        {
            m_cam = Camera.main;
        }
        //m_orbitCam = m_cam.GetComponent<OrbitCam>();

        m_playerState = GetComponent<PlayerStateInfo>();

        m_playerAnimator = GetComponent<Animator>();
        m_spineTransform = m_playerAnimator.GetBoneTransform(HumanBodyBones.Spine);

        m_playerRigidBody = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateAnimator();
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
    }

    private void OnAnimatorMove ()
    {
        if (m_playerState.m_grounded && Time.deltaTime > 0)
        {
            Vector3 v = (m_playerAnimator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

            Debug.Log("v == " + v.ToString());

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
        //Aim Torso!!! FORWARD POINTS RIGHT, UP POINTS UP

        //TRY USING SHOULDERS TO DRAW LINE THAT SHOULD BE PARALLEL WITH LINE TO TARGET!??!?!

        if (m_playerState.m_aiming)
        {
            Vector3 tar = m_cam.transform.position + m_cam.transform.forward * 10.0f;
            Vector3 toTar = tar - m_spineTransform.position;
            Vector3 tiltLine = Vector3.ProjectOnPlane(toTar, m_spineTransform.right);
            Vector3 panLine = Vector3.ProjectOnPlane(toTar, m_spineTransform.up);

            //Vector3 tiltLine = Vector3.ProjectOnPlane(toTar, transform.right);
            //Vector3 panLine = Vector3.ProjectOnPlane(toTar, transform.up);

            float pan = Mathf.Clamp(Vector3.Angle(m_spineTransform.forward, panLine), 0.0f, 45.0f);
            float tilt = Mathf.Clamp(Vector3.Angle(m_spineTransform.forward, tiltLine), 0.0f, 45.0f);

            //float pan = Mathf.Clamp(Vector3.Angle(transform.forward, panLine), 0.0f, 45.0f);
            //float tilt = Mathf.Clamp(Vector3.Angle(transform.forward, tiltLine), 0.0f, 45.0f);

            Debug.Log("pan == " + pan.ToString() + " ; tilt == " + tilt.ToString());

            bool panRight = Vector3.Dot(panLine, m_spineTransform.right) > 0.0f;
            bool tiltUp = Vector3.Dot(tiltLine, m_spineTransform.up) > 0.0f;

            //bool panRight = Vector3.Dot(panLine, transform.right) > 0.0f;
            //bool tiltUp = Vector3.Dot(tiltLine, transform.up) > 0.0f;

            Debug.Log("panRight == " + panRight.ToString() + " ; tiltUp == " + tiltUp.ToString());

            //m_spineTransform.Rotate(-tilt * ((tiltUp) ? 1.0f : -1.0f), pan * ((panRight) ? 1.0f : -1.0f), 0.0f);
            //m_spineTransform.Rotate(0.0f, 0.0f, -tilt * ((tiltUp) ? 1.0f : -1.0f));
            //m_playerAnimator.SetBoneLocalRotation(HumanBodyBones.Spine, m_spineTransform.localRotation);

            m_playerAnimator.SetLookAtPosition(tar);
            m_playerAnimator.SetLookAtWeight(1.0f);

#if UNITY_EDITOR
            Debug.DrawLine(m_spineTransform.position, m_spineTransform.position + tiltLine, Color.yellow);
            Debug.DrawLine(m_spineTransform.position, m_spineTransform.position + panLine, Color.green);
#endif
        }
        else
        {
            m_playerAnimator.SetLookAtWeight(0.0f);
        }


#if UNITY_EDITOR
        Debug.DrawLine(m_spineTransform.position, m_spineTransform.position + m_spineTransform.forward);
        Debug.DrawLine(m_spineTransform.position, (m_cam.transform.position + m_cam.transform.forward * 10.0f), Color.red);

        
#endif

        //m_playerAnimator.SetBoneLocalRotation()        
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
}
