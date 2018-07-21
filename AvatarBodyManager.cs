using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.XR;

namespace CustomAvatar
{
    public class AvatarBodyManager : MonoBehaviour
    {
        private GameObject Head;
        private GameObject Body;
        private GameObject LeftHand;
        private GameObject RightHand;

        public bool manual;

        private Vector3 OldPosition = Vector3.zero;

        private void Awake()
        {
            this.gameObject.transform.localPosition = Vector3.zero;
            this.gameObject.transform.localRotation = Quaternion.identity;
            this.gameObject.transform.localScale = Vector3.one;

            Head = this.gameObject.transform.Find("Head").gameObject;
            Body = this.gameObject.transform.Find("Body").gameObject;
            LeftHand = this.gameObject.transform.Find("LeftHand").gameObject;
            RightHand = this.gameObject.transform.Find("RightHand").gameObject;
        }

        private void Update()
        {
            Body.transform.position = Head.transform.position - (Head.transform.up * 0.1f);

            Vector3 vel = new Vector3(Body.transform.localPosition.x - OldPosition.x, 0.0f, Body.transform.localPosition.z - OldPosition.z);

            Quaternion rot = Quaternion.Euler(0.0f, Head.transform.localEulerAngles.y, 0.0f);
            Vector3 tiltAxis = Vector3.Cross(this.gameObject.transform.up, vel);
            Body.transform.localRotation = Quaternion.Lerp(Body.transform.localRotation,
                                                           Quaternion.AngleAxis(vel.magnitude * 1250.0f, tiltAxis) * rot,
                                                           Time.deltaTime * 10.0f);

            OldPosition = Body.transform.localPosition;

            if (!manual)
            {
                SetHeadPosRot(InputTracking.GetLocalPosition(XRNode.Head), InputTracking.GetLocalRotation(XRNode.Head));
                SetLeftHandPosRot(InputTracking.GetLocalPosition(XRNode.LeftHand), InputTracking.GetLocalRotation(XRNode.LeftHand));
                SetRightHandPosRot(InputTracking.GetLocalPosition(XRNode.RightHand), InputTracking.GetLocalRotation(XRNode.RightHand));
            }
        }

        public void SetHeadPosRot(Vector3 newPosition, Quaternion newRotation)
        {
            Head.transform.localPosition = newPosition;
            Head.transform.localRotation = newRotation;
        }

        public void SetLeftHandPosRot(Vector3 newPosition, Quaternion newRotation)
        {
            LeftHand.transform.localPosition = newPosition;
            LeftHand.transform.localRotation = newRotation;
            PersistentSingleton<VRPlatformHelper>.instance.AdjustPlatformSpecificControllerTransform(LeftHand.transform);
        }

        public void SetRightHandPosRot(Vector3 newPosition, Quaternion newRotation)
        {
            RightHand.transform.localPosition = newPosition;
            RightHand.transform.localRotation = newRotation;
            PersistentSingleton<VRPlatformHelper>.instance.AdjustPlatformSpecificControllerTransform(RightHand.transform);
        }

        public Transform GetHeadTransform()
        {
            return Head.transform;
        }

        public Transform GetLeftHandTransform()
        {
            return LeftHand.transform;
        }

        public Transform GetRightHandTransform()
        {
            return RightHand.transform;
        }
    }
}
