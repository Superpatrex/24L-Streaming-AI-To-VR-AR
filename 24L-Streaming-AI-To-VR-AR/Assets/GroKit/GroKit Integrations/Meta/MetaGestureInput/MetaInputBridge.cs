using Core3lb;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;
using System;
using UnityEngine;

[Core3lbClass]
public class MetaInputBridge : MonoBehaviour
{
    [SerializeField, Interface(typeof(IHand))]
    [CoreReadOnly]
    public MonoBehaviour _hand;
    public IHand Hand;

    [CoreHeader("Required Assignments")]
    [CoreRequired]
    public ShapeRecognizerActiveState fistShape;
    [CoreRequired]
    public ShapeRecognizerActiveState lShapeGesture;
    [CoreRequired]
    public ShapeRecognizerActiveState pointThumbIn;
    [CoreRequired]
    public ShapeRecognizerActiveState openHandShape;
    [CoreRequired]
    public HandRef myHandRef;
    [CoreHeader("Gestures")]
    public bool isTracking;
    public bool _pinchIndex;
    public bool _pinchMiddle;
    public bool _fistGesture;
    public bool _pointThumbInGesture;
    public bool _LGesture;
    public bool _handSpread;
    public bool _GrabCombinedGesture;

    public event Action WhenSelected = delegate { };
    public event Action WhenUnselected = delegate { };


    [CoreToggleHeader("Delay Gestures on Tracking Lost")]
    public bool useStickyGestures;
    [CoreShowIf("useStickyGestures")]
    public float gestureTimingDelay = .3f;
    float timer = 0.0f;

    protected bool _started = false;

    public void Init()
    {
        Hand = _hand as IHand;
        //Debug.LogError(Hand + name);
        this.BeginStart(ref _started);
        this.AssertField(Hand, nameof(Hand));
        this.EndStart(ref _started);
        if (Hand == null)
        {
            Debug.LogError("Hand Tracking is Not Configured Correctly");
        }
        myHandRef.InjectAllHandRef(Hand);
    }

    protected virtual void OnEnable()
    {
        if (_started)
        {
            Hand.WhenHandUpdated += HandleHandUpdated;
        }
    }

    protected virtual void OnDisable()
    {
        if (_started)
        {
            Hand.WhenHandUpdated -= HandleHandUpdated;
        }
    }

    public Vector3 getBone()
    {
        Pose pointerFingerBone;
        Hand.GetJointPose(HandJointId.HandIndexTip, out pointerFingerBone);
        return pointerFingerBone.forward;
    }

    public bool IsTrackingValid()
    {
        OVRPlugin.HandState state = new OVRPlugin.HandState();

        OVRPlugin.Hand queryHand = OVRPlugin.Hand.HandRight;
        if (Hand.Handedness == Handedness.Left)
        {
            queryHand = OVRPlugin.Hand.HandLeft;
        }
        OVRPlugin.GetHandState(OVRPlugin.Step.Render, queryHand, ref state);
        return (state.Status & OVRPlugin.HandStatus.HandTracked) != 0;
    }

    private void HandleHandUpdated()
    {
        if(OVRPlugin.GetHandTrackingEnabled() == false)
        {
            return;
        }
        isTracking = IsTrackingValid();
        if (!isTracking)
        {
            //Reseting for Tracking Lost Disabled for now
            if(useStickyGestures)
            {
                timer += Time.deltaTime;
                if (timer >= gestureTimingDelay)
                {
                    _pinchIndex = false;
                    _pinchMiddle = false;
                    _fistGesture = false;
                    _pointThumbInGesture = false;
                    _LGesture = false;
                    _handSpread = false;
                    _GrabCombinedGesture = false;
                    return;
                }
            }
          
        }
        else
        {
            timer = 0;
        }
        _pinchIndex = Hand.GetIndexFingerIsPinching();
        _pinchMiddle = Hand.GetFingerIsPinching(HandFinger.Middle);
        _fistGesture = fistShape.Active;
        _pointThumbInGesture = lShapeGesture.Active;
        _LGesture = pointThumbIn.Active;
        _handSpread = openHandShape.Active;
        _GrabCombinedGesture = _fistGesture || _pinchIndex;
    }


    public bool GetConfidence()
    {
        if (Hand.GetFingerIsHighConfidence(HandFinger.Index) &&
           Hand.GetFingerIsHighConfidence(HandFinger.Middle) &&
           Hand.GetFingerIsHighConfidence(HandFinger.Ring) &&
           Hand.GetFingerIsHighConfidence(HandFinger.Pinky) &&
           Hand.GetFingerIsHighConfidence(HandFinger.Thumb))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
