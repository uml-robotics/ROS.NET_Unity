//Based originally on UnityStandardAssets.Utility.SimpleMouseRotator
using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class OrbitRotator : MonoBehaviour
{

    // A mouselook behaviour with constraints which operate relative to
    // this gameobject's initial rotation.
    // Only rotates around local X and Y.
    // Works in local coordinates, so if this object is parented
    // to another moving gameobject, its local constraints will
    // operate correctly
    // (Think: looking out the side window of a car, or a gun turret
    // on a moving spaceship with a limited angular range)
    // to have no constraints on an axis, set the rotationRange to 360 or greater.
    public Vector2 rotationRange = new Vector3(90, 90);
    public float rotationSpeed = 5;
    public float translationSpeed = 20;
    public float dampingTime = 0.0f;
    public bool autoZeroVerticalOnMobile = true;
    public bool autoZeroHorizontalOnMobile = false;
    public bool relative = false;
    private bool ignoreEvents;
    public Component Pivot;
    public Component Mast;
        
    private Camera cam {
        get
        {
            return gameObject.GetComponentInChildren<Camera>();
        }
    }
    private Vector3? m_OnClickPointerOrigin;
    private Vector3 m_OnClickTFOrigin;

    

    private Vector3 m_TargetAngles;
    private Vector3 m_FollowAngles;
    private Vector3 m_FollowVelocity;
    private Quaternion m_OriginalRotation;
    private Quaternion m_OriginalOrbitRotation;

    private void Start()
    {
        m_OriginalRotation = Pivot.transform.localRotation;
        m_OriginalOrbitRotation =  transform.localRotation;
    }

    void OnApplicationFocus(bool focusStatus)
    {
        ignoreEvents = !focusStatus;
    }
    private void Update()
    {
        if (ignoreEvents) return;
        HandlePivot();
        HandleZoom();
        HandleTranslate();
    }

    private void HandleZoom()
    {
        //hot fix: This will prevent scroll-wheel from moving cam while not on screen
        //does not work when on a diff screen while using synergy
        if (Input.mousePosition.x < 0 || Input.mousePosition.x > Screen.width || Input.mousePosition.y < 0 || Input.mousePosition.y > Screen.height)
            return;

        Vector2 scrollDelta = Input.mouseScrollDelta;
        if (!scrollDelta.Equals(Vector2.zero))
        {
            Mast.transform.localPosition = new Vector3(Mast.transform.localPosition.x, Mast.transform.localPosition.y, Mast.transform.localPosition.z + scrollDelta.y * 2f);
        }
        else
        {
            if (!CrossPlatformInputManager.GetButton("Fire2"))
            {
                return;
            }
            float inputV = CrossPlatformInputManager.GetAxis("Mouse Y");
            Mast.transform.localPosition = new Vector3(Mast.transform.localPosition.x, Mast.transform.localPosition.y, Mast.transform.localPosition.z + inputV);
        }
    }

    private void HandleTranslate()
    {
        if (!CrossPlatformInputManager.GetButton("Fire3"))
        {
            m_OnClickPointerOrigin = null;
            return;
        }

        if (m_OnClickPointerOrigin == null)
        {
            m_OnClickPointerOrigin = cam.ScreenToViewportPoint(CrossPlatformInputManager.mousePosition);
            m_OnClickTFOrigin = transform.position;
        }
        Vector3 delta_click = m_OnClickPointerOrigin.Value - cam.ScreenToViewportPoint(CrossPlatformInputManager.mousePosition);
        transform.position = transform.TransformVector(new Vector3((delta_click.x * translationSpeed), 0f, (delta_click.y * translationSpeed))) + m_OnClickTFOrigin;
    
        //float inputH = CrossPlatformInputManager.GetAxis("Mouse X");
        //float inputV = CrossPlatformInputManager.GetAxis("Mouse Y");
    }

    private void HandlePivot()
    {
        if (!CrossPlatformInputManager.GetButton("Fire1"))
        {
            return;
        }

        // we make initial calculations from the original local rotation
        Pivot.transform.localRotation = m_OriginalRotation;

        // read input from mouse or mobile controls
        float inputH;
        float inputV;
        if (relative)
        {
            inputH = CrossPlatformInputManager.GetAxis("Mouse X");
            inputV = CrossPlatformInputManager.GetAxis("Mouse Y");

            // wrap values to avoid springing quickly the wrong way from positive to negative
            if (m_TargetAngles.y > 180)
            {
                m_TargetAngles.y -= 360;
                m_FollowAngles.y -= 360;
            }
            if (m_TargetAngles.x > 180)
            {
                m_TargetAngles.x -= 360;
                m_FollowAngles.x -= 360;
            }
            if (m_TargetAngles.y < -180)
            {
                m_TargetAngles.y += 360;
                m_FollowAngles.y += 360;
            }
            if (m_TargetAngles.x < -180)
            {
                m_TargetAngles.x += 360;
                m_FollowAngles.x += 360;
            }

#if MOBILE_INPUT
        // on mobile, sometimes we want input mapped directly to tilt value,
        // so it springs back automatically when the look input is released.
		if (autoZeroHorizontalOnMobile) {
			m_TargetAngles.y = Mathf.Lerp (-rotationRange.y * 0.5f, rotationRange.y * 0.5f, inputH * .5f + .5f);
		} else {
			m_TargetAngles.y += inputH * rotationSpeed;
		}
		if (autoZeroVerticalOnMobile) {
			m_TargetAngles.x = Mathf.Lerp (-rotationRange.x * 0.5f, rotationRange.x * 0.5f, inputV * .5f + .5f);
		} else {
			m_TargetAngles.x += inputV * rotationSpeed;
		}
#else
            // with mouse input, we have direct control with no springback required.
            m_TargetAngles.y += inputH * rotationSpeed;
            m_TargetAngles.x += inputV * rotationSpeed;
#endif

            // clamp values to allowed range
            m_TargetAngles.y = Mathf.Clamp(m_TargetAngles.y, -rotationRange.y * 0.5f, rotationRange.y * 0.5f);
            m_TargetAngles.x = Mathf.Clamp(m_TargetAngles.x, -rotationRange.x * 0.5f, rotationRange.x * 0.5f);
        }
        else
        {
            inputH = Input.mousePosition.x;
            inputV = Input.mousePosition.y;

            // set values to allowed range
            m_TargetAngles.y = Mathf.Lerp(-rotationRange.y * 0.5f, rotationRange.y * 0.5f, inputH / Screen.width);
            m_TargetAngles.x = Mathf.Lerp(-rotationRange.x * 0.5f, rotationRange.x * 0.5f, inputV / Screen.height);
        }

        // smoothly interpolate current values to target angles
        m_FollowAngles = Vector3.SmoothDamp(m_FollowAngles, m_TargetAngles, ref m_FollowVelocity, dampingTime);

        // update the actual gameobject's rotation
        Pivot.transform.localRotation = m_OriginalRotation * Quaternion.Euler(-m_FollowAngles.x, 0, 0);
        transform.localRotation = m_OriginalOrbitRotation * Quaternion.Euler(0,m_FollowAngles.y,0);
    }
}
