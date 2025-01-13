using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
using Oculus.Interaction.Input;

public class CreateNewBranch : MonoBehaviour
{
    [SerializeField] private Material _lineMaterial;
    [SerializeField] private GameObject _prefab;
    private Transform _prefabTransform;
    private XRHandSubsystem _subsystem;
    private bool _isPinching = false;
    private Vector3 _startPosition;
    private LineRenderer _currentLineRenderer;
    private List<LineRenderer> _lineRenderers = new List<LineRenderer>();

    // Start is called before the first frame update
    void Start()
    {
        _subsystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();
        if (_subsystem == null)
        {
            UIDebugger.Log("XR Hand Subsystem Not Found");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_subsystem == null) return;
        XRHand righthand = _subsystem.rightHand;
        if (righthand.isTracked)
        {
            bool isPinching = IsPinching(righthand, out Vector3 pinchPosition);
            if (isPinching && !_isPinching && IsPinchingAnchor(pinchPosition))
            {
                    _isPinching = true;
                    _startPosition = pinchPosition;
                    CreateNewLine(pinchPosition);
            }
            else if (_isPinching && isPinching)
            {
                UpdateLine(pinchPosition);

            }
            else if (_isPinching && !isPinching)
            {
                _isPinching = false;
                FinalizeLine();
            }
        }
        
        
    }
    
    private void CreateNewLine(Vector3 startPosition)
    {
        GameObject newLine = new GameObject("LineRenderer");
        _currentLineRenderer= newLine.AddComponent<LineRenderer>();
        newLine.transform.SetParent(transform);
        _currentLineRenderer.positionCount = 2;
        _currentLineRenderer.startWidth = 0.01f;
        _currentLineRenderer.endWidth = 0.01f;
        _currentLineRenderer.material=_lineMaterial?? new Material(Shader.Find("Unlit/Color"));
        _currentLineRenderer.material.color = Color.green;
        _currentLineRenderer.SetPosition(0, _startPosition);
        _currentLineRenderer.SetPosition(1, _startPosition);
        _lineRenderers.Add(_currentLineRenderer);
    }

    private void UpdateLine(Vector3 currentPosition)
    {
        if (_currentLineRenderer != null)
        {
            _currentLineRenderer.SetPosition(1, currentPosition);
            //_prefabTransform.position = _currentLineRenderer.GetPosition(1);
           
        }
    }

    private void FinalizeLine()
    {
        float distance = Vector3.Distance(_lineRenderers[_lineRenderers.Count-1].GetPosition(0),_lineRenderers[_lineRenderers.Count-1].GetPosition(1));
        if (distance > 0.2f)
        {
            Instantiate(_prefab, _lineRenderers[_lineRenderers.Count-1].GetPosition(1), Quaternion.identity);
            _currentLineRenderer = null; 
        }
        else
        {
            Destroy(_lineRenderers[_lineRenderers.Count-1]);
        }
        
    }
    private bool IsPinching(XRHand hand, out Vector3 pinchPosition)
    {
        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var indexTip= hand.GetJoint(XRHandJointID.IndexTip);
        if (thumbTip.TryGetPose(out Pose thumbTipPose) && indexTip.TryGetPose(out Pose indexTipPose))
        {
            float distance = Vector3.Distance(thumbTipPose.position, indexTipPose.position);
            if (distance < 0.02f)
            {
                pinchPosition = (thumbTipPose.position+indexTipPose.position)/2.0f;
                return true; 
            }
        }
        pinchPosition = Vector3.zero;
        return false;
    }

    private bool IsPinchingAnchor(Vector3 pinchPosition)
    {
        Ray ray = new Ray(pinchPosition, Vector3.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.05f))
        {
            if (hit.collider.gameObject == gameObject)
            {
                UIDebugger.Log("Pinching Anchor");
                return true; 
            }
        }
        return false;
    }
}
