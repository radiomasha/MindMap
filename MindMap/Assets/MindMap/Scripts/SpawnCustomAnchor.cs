using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
using Meta.XR.BuildingBlocks;
using System.Collections.Generic;

public class SpawnCustomAnchor : MonoBehaviour
{
    public static SpawnCustomAnchor instance;
    [SerializeField] private GameObject customAnchorPrefab;
    [SerializeField] private GameObject customPreview;
    public bool _isAnchorCreated = false;
    public List<OVRSpatialAnchor> _anchorInstances = new();
    private HashSet<Guid> _anchorUuids = new();
    private Action<bool, OVRSpatialAnchor.UnboundAnchor> _onLocalized;
    private XRHandSubsystem _subsystem;
    private bool _isLeftMakingFist = false;
    private bool _isRightPinching = false;
   
    private Vector3 _lastPinchPosition;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            _onLocalized = OnLocalized;
        }
        else
        {
            Destroy(this);
        }
    }
    private void Start()
    {
        _subsystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();
      
        if (_subsystem == null)
        {
            UIDebugger.Log("Can't find XRHandSubsystem");
        }
    }

    private void Update()
    {
        CheckGestures();

    }

    private void CheckGestures()
    {
        if (_subsystem == null) return;
        XRHand leftHand = _subsystem.leftHand;
        XRHand rightHand = _subsystem.rightHand;
        if (leftHand.isTracked && rightHand.isTracked)
        {
            _isLeftMakingFist = IsHandMakingFist(leftHand);
            bool rightPinching = IsPinching(rightHand, out Vector3 pinchPosition);
            if (_isLeftMakingFist && rightPinching&&!_isAnchorCreated)
            {
                _lastPinchPosition= pinchPosition;
                _isRightPinching = true;
            }
            else if (_isRightPinching && !rightPinching&&_isLeftMakingFist&&!_isAnchorCreated)
            {
                var go = Instantiate(customAnchorPrefab, _lastPinchPosition, Quaternion.identity); // Anchor A
                SetupAnchorAsync(go.AddComponent<OVRSpatialAnchor>(), saveAnchor: true);
                _isRightPinching = false;
                _isAnchorCreated = true;
                UIDebugger.Log("Anchor Created");
            }
            //else if (_isLeftMakingFist && IsHandMakingFist(rightHand)&&!_isAnchorCreated)
            //{
            //    LoadAllAnchors();
           // }
            else if (IsHandPinching(rightHand) && IsHandPinching(leftHand) && _isAnchorCreated)
            {
                //foreach (var anchor in _anchorInstances)
                //{
                    Destroy(_anchorInstances[_anchorInstances.Count - 1]);
                //}
                //_anchorInstances.Clear();
                UIDebugger.Log("Anchor Deleted");
                //EraseAllAnchors();
                _isAnchorCreated = false;
                
            }
        }
    }
    private async void SetupAnchorAsync(OVRSpatialAnchor anchor, bool saveAnchor)
    {
        if (!await anchor.WhenLocalizedAsync())
        {
            UIDebugger.Log("Can't locate anchor");
            Destroy(anchor.gameObject);
            return;
        }
        _anchorInstances.Add(anchor);
        if(saveAnchor&&(await anchor.SaveAnchorAsync()).Success)
        {
            _anchorUuids.Add(anchor.Uuid);
        }
    }
    private bool IsHandMakingFist(XRHand hand)
    {
        int curledFingersCount = 0;
        foreach (HandFinger finger in (HandFinger[])System.Enum.GetValues(typeof(HandFinger)))
        {
            if(finger == HandFinger.Thumb)continue;
            if (IsFingerCurled(hand, finger))
            {
                curledFingersCount++;
            }
        }
        return curledFingersCount >= 3;
    }

    private bool IsFingerCurled(XRHand hand, HandFinger finger)
    {
        XRHandJointID jointID = GetJointID(finger);
        var joint = hand.GetJoint(jointID);
        if (joint!= null && joint.TryGetPose(out Pose pose))
        {
            Vector3 fingerTip = pose.position;
            XRHandJoint palmJoint = hand.GetJoint(XRHandJointID.Palm);
            if (palmJoint != null && palmJoint.TryGetPose(out var palmPose))
            {
                Vector3 palmcenter = palmPose.position;
                float distance = Vector3.Distance(fingerTip, palmcenter);
                return distance < 0.03f;
            }
        }
        return false;
    }
    private XRHandJointID GetJointID(HandFinger finger)
    {
        switch (finger)
        {
            case HandFinger.Index:
                return XRHandJointID.IndexTip;
            case HandFinger.Middle:
                return XRHandJointID.MiddleTip;
            case HandFinger.Ring:
                return XRHandJointID.RingTip;
            case HandFinger.Pinky:
                return XRHandJointID.LittleTip;
            default:
                return XRHandJointID.ThumbTip;
        }
    }

    private bool IsPinching(XRHand hand,out Vector3 pinchPosition)
    {
        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);
        if (thumbTip.TryGetPose(out Pose thumbPose) && indexTip.TryGetPose(out Pose indexPose))
        {
            float distance = Vector3.Distance(thumbPose.position, indexPose.position);
            if (distance < 0.03f) 
            {
                pinchPosition = (thumbPose.position + indexPose.position) / 2; 
                return true;
            }
        }
        pinchPosition = Vector3.zero;
        return false;
    }
    private bool IsHandPinching(XRHand hand)
    {
        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);
        if (thumbTip.TryGetPose(out Pose thumbPose) && indexTip.TryGetPose(out Pose indexPose))
        {
            float distance = Vector3.Distance(thumbPose.position, indexPose.position);
            if (distance < 0.02f) 
            {
                return true;
            }
        }
        return false;
    }
    private async void LoadAllAnchors()
    {
        // Load and localize
        var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
        var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(_anchorUuids, unboundAnchors);

        if (result.Success)
        {
            foreach (var anchor in unboundAnchors)
            {
                anchor.LocalizeAsync().ContinueWith(_onLocalized, anchor);
            }
        }
        else
        {
            Debug.LogError($"Load anchors failed with {result.Status}.");
        }
    }

    private void OnLocalized(bool success, OVRSpatialAnchor.UnboundAnchor unboundAnchor)
    {
        var pose = unboundAnchor.Pose;
        var go = Instantiate(customAnchorPrefab, pose.position, pose.rotation);
        var anchor = go.AddComponent<OVRSpatialAnchor>();

        unboundAnchor.BindTo(anchor);

        // Add the anchor to the running total
        _anchorInstances.Add(anchor);
    }

    /******************* Erase Anchor Methods *****************/
    // in the headset, but don't destroy them. They should remain displayed.
    public async void EraseAllAnchors()
    {
        var result = await OVRSpatialAnchor.EraseAnchorsAsync(anchors: null, uuids: _anchorUuids);
        if (result.Success)
        {
            _anchorUuids.Clear();

            Debug.Log($"Anchors erased.");
        }
        else
        {
            Debug.LogError($"Anchors NOT erased {result.Status}");
        }
    }
}
