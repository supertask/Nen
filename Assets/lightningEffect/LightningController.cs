using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

namespace Lightning
{
    public class LightningController : MonoBehaviour
    {
        LeapServiceProvider m_Provider;
        Vector3 baseEulerAngle;
        public GameObject originLightningObj;
        public GameObject leapProviderObj;

        // Use this for initialization
        void Start()
        {
            this.m_Provider = this.leapProviderObj.GetComponent<LeapServiceProvider>();
            this.baseEulerAngle = new Vector3(270, 90, 0); //基底となるオイラー角（左手の指先が上，親指が右の状態）
        }


        private GameObject CloneLightning(int fingerIndex,
                Vector3 emitterPos, Vector3 receiverPos, Vector3 fingerNormal) {
            GameObject lightningObj = GameObject.Find("LightningFinger" + fingerIndex);
            if (lightningObj == null) {
                //Lightningオブジェクトとマテリアルをそれぞれ複製
                lightningObj = Object.Instantiate(originLightningObj) as GameObject;
                Material mat = Material.Instantiate(originLightningObj.GetComponent<Lightning>()._material);
                lightningObj.name = "LightningFinger" + fingerIndex;
                lightningObj.GetComponent<Lightning>()._material = mat;
            }
            Lightning light = lightningObj.GetComponent<Lightning>();
            light.transform.position = Vector3.Lerp(emitterPos, receiverPos, 0.5f);
            light.emitter = emitterPos;
            light.receiver = receiverPos;
            light._Seed = 2 * fingerIndex * fingerIndex + 3 * fingerIndex + 8;
            light._material.SetVector("_FingerNormal",
                fingerNormal);

            return lightningObj;
        }
        private void DeleteLightning(int fingerIndex) {
            GameObject.Destroy(GameObject.Find("LightningFinger" + fingerIndex));
        }

        // Update is called once per frame
        void Update()
        {
            Frame frame = this.m_Provider.CurrentFrame;
            if (frame.Hands.Count < 2) {
                for (int i = 0; i < 5; i++) { this.DeleteLightning(i); }
                return;
            }

            Hand leftHand = frame.Hands[0].IsLeft ? frame.Hands[0] : frame.Hands[1];
            Hand rightHand = frame.Hands[1].IsRight ? frame.Hands[1] : frame.Hands[0];
            Vector3 handCenter = Vector3.Lerp(LeapUtils.ToVector3(rightHand.PalmPosition),
                    LeapUtils.ToVector3(leftHand.PalmPosition), 0.5f);

            for (int i = 0; i < 5; i++) {
                if (leftHand.Fingers[i].IsExtended && rightHand.Fingers[i].IsExtended) {
                    //光らせる
                    Vector3 emitterPos = LeapUtils.ToVector3(leftHand.Fingers[i].TipPosition);
                    Vector3 receiverPos = LeapUtils.ToVector3(rightHand.Fingers[i].TipPosition);
                    Vector3 fingerCenter = Vector3.Lerp(emitterPos, receiverPos, 0.5f);
                    Vector3 fingerNormal = (fingerCenter - handCenter).normalized;

                    Debug.DrawLine(fingerCenter, fingerCenter + 20 * fingerNormal, Color.white);
                    this.CloneLightning(i, emitterPos, receiverPos, fingerNormal);
                }
                else {
                    //光を消す
                    this.DeleteLightning(i);
                }
            }

            /*
            foreach (Hand hand in frame.Hands) {
                Vector3 leapPosition = LeapUtils.ToVector3(hand.PalmPosition);
                Vector3 leapVelocity = LeapUtils.ToVector3(hand.PalmVelocity);
                //Debug.Log("actual pos: " + leapPosition);
                //Debug.Log("actual velocity: " + leapVelocity.normalized);

                foreach(Finger finger in hand.Fingers) {
                    //Debug.DrawRay(finger.TipPosition, finger.Direction, Color.red);
                    //Debug.Log("pos: " + finger.TipPosition);
                    Debug.Log("dir: " + finger.Direction);
                }
            }
            */
        }
    }
}