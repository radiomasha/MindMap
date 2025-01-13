using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnWithGesture : MonoBehaviour
{
    public static SpawnCustomAnchor instance;
    [SerializeField] private GameObject customAnchorPrefab;
    [SerializeField] private GameObject customPreview;
    private List<OVRSpatialAnchor> _anchorInstances = new();
    private HashSet<Guid> _anchorUuids = new();
    private Action<bool, OVRSpatialAnchor.UnboundAnchor> _onLocalized;
    private HandPoseTracker handPoseTracker;

    private float indexFingerDistance;
    private bool _isAnchorCreated = false;
    // Start is called before the first frame update
    void Start()
    {
        handPoseTracker = FindObjectOfType<HandPoseTracker>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!handPoseTracker.leftHandPose || !handPoseTracker.rightHandPose)
        {
            return;
        }
        indexFingerDistance = Vector3.Distance(handPoseTracker.LeftIndexPose.position, handPoseTracker.RightIndexPose.position);
        Vector3 center = (handPoseTracker.LeftIndexPose.position + handPoseTracker.RightIndexPose.position) /2f;
        if (indexFingerDistance >= 0.3f&&!_isAnchorCreated)
        {
            var go = Instantiate(customAnchorPrefab, center, Quaternion.identity); // Anchor A
            SetupAnchorAsync(go.AddComponent<OVRSpatialAnchor>(), saveAnchor: true);
            _isAnchorCreated = true;
            UIDebugger.Log("Anchor Created");
        }

        if (handPoseTracker.swipePose)
        {
            foreach (var anchor in _anchorInstances)
            {
                Destroy(anchor.gameObject);
            }
            _anchorInstances.Clear();
            UIDebugger.Log("Anchor Deleted");
            EraseAllAnchors();
            _isAnchorCreated = false;
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
    private void OnLocalized(bool success, OVRSpatialAnchor.UnboundAnchor unboundAnchor)
    {
        var pose = unboundAnchor.Pose;
        var go = Instantiate(customAnchorPrefab, pose.position, pose.rotation);
        var anchor = go.AddComponent<OVRSpatialAnchor>();

        unboundAnchor.BindTo(anchor);

        // Add the anchor to the running total
        _anchorInstances.Add(anchor);
    }
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
