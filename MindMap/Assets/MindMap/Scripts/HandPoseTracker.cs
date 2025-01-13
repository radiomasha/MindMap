using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

public class HandPoseTracker : MonoBehaviour
{
    [SerializeField] private HandRef lefthandRef;
    [SerializeField] private HandRef righthandRef;
    [SerializeField] private HandRef swipeRef;
    [SerializeField] private HandRef rightPinch;
    [SerializeField] private HandRef leftPinch;

    [SerializeField] private SkinnedMeshRenderer leftHandRenderer;
    [SerializeField] private SkinnedMeshRenderer rightHandRenderer;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color poseDetectedColor;

    public bool leftHandPose = false;
    public bool rightHandPose = false;
    public bool swipePose = false;
    public bool rightPinchPose = false;
    public bool leftPinchPose = false;

    private Pose leftIndexPose;
    private Pose rightIndexPose;
    private Pose leftThumbPose;
    private Pose rightThumbPose;
    
    public Pose LeftIndexPose
    {
        get => leftIndexPose; 
    }

    public Pose RightIndexPose
    {
        get => rightIndexPose;
    }
    public Pose LeftThumbPose
    {
        get => leftThumbPose; 
    }
    public Pose RightThumbPose
    {
        get => rightThumbPose; 
    }
    private Material leftHandMaterial;
    private Material rightHandMaterial;

    private void Awake()
    {
        leftHandMaterial = leftHandRenderer.sharedMaterials[0];
        rightHandMaterial = rightHandRenderer.sharedMaterials[0];
    }

    public void IsLeftHandPoseDetected(bool handPose)
    {
        leftHandPose = handPose;
        if(leftHandPose)
            leftHandMaterial.SetColor("_OutlineColor", poseDetectedColor);
        else 
            leftHandMaterial.SetColor("_OutlineColor", defaultColor);
    }
    public void IsRightHandPoseDetected(bool handPose)
    {
        rightHandPose = handPose;
        if(rightHandPose)
            rightHandMaterial.SetColor("_OutlineColor", poseDetectedColor);
        else 
            rightHandMaterial.SetColor("_OutlineColor", defaultColor);
    }

    public void IsSwipePoseDetected(bool handPose)
    {
        rightHandPose = handPose;
        if(rightHandPose)
            rightHandMaterial.SetColor("_OutlineColor", poseDetectedColor);
        else 
            rightHandMaterial.SetColor("_OutlineColor", defaultColor);
    }

    public void IsRightPinchPoseDetected(bool handPose)
    {
        rightHandPose = handPose;
        if(rightHandPose)
            rightHandMaterial.SetColor("_OutlineColor", poseDetectedColor);
        else 
            rightHandMaterial.SetColor("_OutlineColor", defaultColor);
    }

    public void IsLeftPinchPoseDetected(bool handPose)
    {
        leftHandPose = handPose;
        if(leftHandPose)
            leftHandMaterial.SetColor("_OutlineColor", poseDetectedColor);
        else 
            leftHandMaterial.SetColor("_OutlineColor", defaultColor);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ExplodedPoseDetection();
        SwipePoseDetection();
        PinchPoseDetection();

    }

    private void ExplodedPoseDetection()
    {
        if (!leftHandPose || !rightHandPose) return;
        lefthandRef.GetJointPose(HandJointId.HandIndexTip, out leftIndexPose);
        righthandRef.GetJointPose(HandJointId.HandIndexTip, out rightIndexPose);
        //UIDebugger.Log("Hand Pose Detected");
    }

    private void SwipePoseDetection()
    {
        if (!swipePose) return;
        UIDebugger.Log("Swipe Pose Detected");
    }
    private void PinchPoseDetection()
    {
        if (rightPinchPose || leftPinchPose)
        {
            UIDebugger.Log("Pinch Pose Detected");
            lefthandRef.GetJointPose(HandJointId.HandIndexTip, out leftIndexPose);
            righthandRef.GetJointPose(HandJointId.HandIndexTip, out rightIndexPose);
            lefthandRef.GetJointPose(HandJointId.HandThumbTip, out leftThumbPose);
            righthandRef.GetJointPose(HandJointId.HandThumbTip, out rightThumbPose);
        }
   
    }
    private void OnDisable()
    {
        leftHandMaterial.SetColor("_OutlineColor", defaultColor);
        rightHandMaterial.SetColor("_OutlineColor", defaultColor);
    }
    
}
