using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct BodyPose
{
    public Transform m_hips, m_leftUpperLeg, m_rightUpperLeg, m_leftLowerLeg,
        m_rightLowerLeg, m_leftFoot, m_rightFoot, m_spine,
        m_chest, m_neck, m_head, m_leftShoulder,
        m_rightShoulder, m_leftUpperArm, m_rightUpperArm, m_leftLowerArm,
        m_rightLowerArm, m_leftHand, m_rightHand, m_leftToes,
        m_rightToes, m_leftEye, m_rightEye, m_jaw; 
}

public class GameManager : MonoBehaviour
{
    public GameObject m_player, m_pauseMenu;

    public List<GameObject> m_goblins = new List<GameObject>();

    public ObjectPool m_goblinBodies;
    
    [HideInInspector]
    public bool m_paused = false;

    private List<GoblinStateInfo> m_goblinStates = new List<GoblinStateInfo>();

    private PlayerStateInfo m_playerState;

    private bool m_pauseLock = false;

    // Use this for initialization
    void Start ()
    {
        if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }
        
        m_playerState = m_player.GetComponent<PlayerStateInfo>();

        for (int i = 0; i < m_goblins.Count; i++)
        {
            m_goblinStates.Add(m_goblins[i].GetComponent<GoblinStateInfo>());
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!m_pauseLock && Input.GetButton("Pause"))
        {
            m_pauseLock = true;
            m_paused = !m_paused;
        }
        else if (!Input.GetButton("Pause"))
        {
            m_pauseLock = false;
        }

        if (m_paused && !m_pauseMenu.activeInHierarchy)
        {
            m_pauseMenu.SetActive(true);
            Time.timeScale = 0.0f;

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else if (!m_paused && m_pauseMenu.activeInHierarchy)
        {
            m_pauseMenu.SetActive(false);
            Time.timeScale = 1.0f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (!m_paused)
        {
            for (int i = 0; i < m_goblinStates.Count; i++)
            {
                if (m_goblinStates[i].m_health <= 0.0f)
                {                    
                    //Goblin died
                    m_goblinStates[i].m_health = 1.0f;
                    DropBody(m_goblinStates[i].gameObject);
                }
            }
        }
    }

    public void PauseButton ()
    {
        m_paused = !m_paused;        
    }

    public void GameQuit ()
    {
        if (Application.isPlaying)
        {
            Application.Quit();
        }
    }

    private void DropBody (GameObject goblin)
    {
        Debug.Log("dropping body for " + goblin.name);

        Animator goblinAnimator = goblin.GetComponent<Animator>();

        //BodyPose goblinBodyPose = GetBodyPose(goblinAnimator);

        goblin.SetActive(false);

        GameObject deadBody = m_goblinBodies.GetObject();

        deadBody.transform.position = goblin.transform.position;
        deadBody.transform.rotation = goblin.transform.rotation;

        Animator deadBodyAnimator = deadBody.GetComponent<Animator>();

        //SetBodyPose(deadBodyAnimator, goblinBodyPose);

        deadBody.SetActive(true);
    }

    private BodyPose GetBodyPose (Animator animator)
    {
        BodyPose pose;

        pose.m_chest = animator.GetBoneTransform(HumanBodyBones.Chest);
        pose.m_head = animator.GetBoneTransform(HumanBodyBones.Head);

        if (pose.m_head == null)
        {
            Debug.Log("WTF HEAD!!!");
        }

        pose.m_hips = animator.GetBoneTransform(HumanBodyBones.Hips);

        if (pose.m_hips == null)
        {
            Debug.Log("WTF HIPS!!!");
        }

        pose.m_jaw = animator.GetBoneTransform(HumanBodyBones.Jaw);
        pose.m_leftEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
        pose.m_leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        pose.m_leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        pose.m_leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        pose.m_leftLowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        pose.m_leftShoulder = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
        pose.m_leftToes = animator.GetBoneTransform(HumanBodyBones.LeftToes);
        pose.m_leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        pose.m_leftUpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        pose.m_neck = animator.GetBoneTransform(HumanBodyBones.Neck);
        pose.m_rightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);
        pose.m_rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        pose.m_rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        pose.m_rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        pose.m_rightLowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        pose.m_rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
        pose.m_rightToes = animator.GetBoneTransform(HumanBodyBones.RightToes);
        pose.m_rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        pose.m_rightUpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        pose.m_spine = animator.GetBoneTransform(HumanBodyBones.Spine);

        return pose;
    }

    private void SetBodyPose (Animator animator, BodyPose pose)
    {
        if (pose.m_chest != null)
        {
            animator.SetBoneLocalRotation(HumanBodyBones.Chest, pose.m_chest.localRotation);
        }
        
        if (pose.m_head != null)
        {
            animator.SetBoneLocalRotation(HumanBodyBones.Head, pose.m_head.localRotation);
        } 
        
        animator.SetBoneLocalRotation(HumanBodyBones.Hips, pose.m_hips.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.Jaw, pose.m_jaw.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.LeftEye, pose.m_leftEye.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.LeftFoot, pose.m_leftFoot.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.LeftHand, pose.m_leftHand.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.LeftLowerArm, pose.m_leftLowerArm.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.LeftLowerLeg, pose.m_leftLowerLeg.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.LeftShoulder, pose.m_leftShoulder.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.LeftToes, pose.m_leftToes.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.LeftUpperArm, pose.m_leftUpperArm.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.LeftUpperLeg, pose.m_leftUpperLeg.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.Neck, pose.m_neck.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.RightEye, pose.m_rightEye.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.RightFoot, pose.m_rightFoot.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.RightHand, pose.m_rightHand.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.RightLowerArm, pose.m_rightLowerArm.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.RightLowerLeg, pose.m_rightLowerLeg.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.RightShoulder, pose.m_rightShoulder.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.RightToes, pose.m_rightToes.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.RightUpperArm, pose.m_rightUpperArm.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.RightUpperLeg, pose.m_rightUpperLeg.localRotation);
        animator.SetBoneLocalRotation(HumanBodyBones.Spine, pose.m_spine.localRotation);
    }
}
