using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class ForceController : MonoBehaviour
{
    public GameObject leapProviderObj;
    LeapServiceProvider m_Provider;
    private int mode;
    private int NONE = 0;
    private int FORCE = 1;
    private int PINCH_MOVE = 2;
    private int HIDE = 3;
    private GameObject selecting;
    private bool[] isOpenHands;
    private int LEFT = 0;
    private int RIGHT = 1;
    private GameObject insideBuilding;
    private GameObject outsideBuilding;
    private GameObject garden;
    private bool usedImpact;
    public float GRAVITY_COEF = 0.00000000006671999f;    // 万有引力係数
    public float BLACK_HOLE_MASS = 1f * 0.4f; //ブラックホールの質量

    void Start() { this.Init(); }

    private void Init() {
        this.m_Provider = this.leapProviderObj.GetComponent<LeapServiceProvider>();
        this.mode = PINCH_MOVE;
        this.isOpenHands = new bool[2] { false, false };
        this.insideBuilding = GameObject.Find("Building/Inside");
        this.outsideBuilding = GameObject.Find("Building/Outside");
        this.garden = GameObject.Find("Garden");
        this.usedImpact = false;
    }


    void Update()
    {
        if (this.mode == FORCE) {
            //フォースモード
            Frame frame = this.m_Provider.CurrentFrame;
            Hand[] hands = this.GetCorrectHands(frame);
            if (hands[LEFT] != null) {
                if (this.isOpenHands[LEFT] && !this.IsOpenFiveFingers(hands[LEFT])) {
                    this.isOpenHands[LEFT] = false; //closed hand
                }
                else if (!this.isOpenHands[LEFT] && this.IsOpenFiveFingers(hands[LEFT])) {
                    this.isOpenHands[LEFT] = true; //opened hand
                    Debug.Log("left hand opened");
                    this.Impact(hands[LEFT], this.insideBuilding);
                }
            }
            if (hands[RIGHT] != null) {
                if (this.isOpenHands[RIGHT] && !this.IsOpenFiveFingers(hands[RIGHT])) {
                    this.isOpenHands[RIGHT] = false; //closed hand
                }
                else if (!this.isOpenHands[RIGHT] && this.IsOpenFiveFingers(hands[RIGHT])) {
                    this.isOpenHands[RIGHT] = true; //opened hand
                    Debug.Log("right hand opened");
                }
            }

            //Do a KazaAna effect while a right hand is opend
            if (this.isOpenHands[RIGHT] && hands[RIGHT] != null) {
                this.KazaAna(hands[RIGHT], this.insideBuilding);
            }
        }

        if (Input.GetKeyUp(KeyCode.F)) {
            this.mode = FORCE;
            ObjSelector.MapObjects(); //選択したオブジェクトを固定する
        }
        else if (Input.GetKeyUp(KeyCode.H)) {
            //ビルの外壁とガーデンを消す
            this.mode = HIDE;
            this.HideObjectsRecursively(this.outsideBuilding);
            this.garden.SetActive(false);
        }
        else if (Input.GetKeyUp(KeyCode.G)) {
            this.AddRigidbody(this.insideBuilding);
        }
        else if (Input.GetKeyUp(KeyCode.P)) {
            this.mode = PINCH_MOVE;
            ObjSelector.SelectMappedObjects(); //固定されたオブジェクトを選択しピンチ可能に
        }
    }

    private void Impact(Hand hand, GameObject obj) {
        /*
        Vector3 src = this.GetVector3(hand.PalmPosition);
        Vector3 target = this.GetVector3(hand.PalmVelocity);
        Debug.DrawLine(src, target * 10, Color.red);
        Debug.Log(target);
        */
        //this.usedImpact = true;
    }

    /*
     * Kaza Ana Effect at Inuyasha(Anime)
     * It is a black hole effect
     */
    private void KazaAna(Hand hand, GameObject obj)
    {
        foreach (Transform objTrans in obj.GetComponentsInChildren<Transform>())
        {
            Rigidbody rigid = objTrans.gameObject.GetComponent<Rigidbody>();
            if (rigid == null) { continue; }

            Vector3 handPos = this.GetVector3(hand.PalmPosition);
            Vector3 direction = objTrans.position - handPos;
            float R = direction.magnitude;
            if (R < 0.001f) { objTrans.gameObject.SetActive(false); }

            rigid.SetDensity(0.7f); // wood density
            float objMass = rigid.mass;
            float gravity = GRAVITY_COEF * objMass * BLACK_HOLE_MASS / (R * R);
            rigid.AddForce(-gravity * direction, ForceMode.Force);
        }
    }

    private void HideObjectsRecursively(GameObject obj) {
        foreach (Transform t in obj.GetComponentsInChildren<Transform>())
        {
            MeshRenderer mr = t.gameObject.GetComponent<MeshRenderer>();
            if (mr != null) { mr.enabled = false; }
        }
    }


    /*
     * 
     * Leap Motionのframe.Handsのindexはleft, rightに対応していないため
     */
    private Hand[] GetCorrectHands(Frame frame)
    {
        Hand[] hands = new Hand[2];
        if (frame.Hands.Count >= 2) {
            hands[LEFT] = frame.Hands[0].IsLeft ? frame.Hands[0] : frame.Hands[1];
            hands[RIGHT] = frame.Hands[1].IsRight ? frame.Hands[1] : frame.Hands[0];
        }
        else if (frame.Hands.Count == 1) {
            hands[LEFT] = frame.Hands[0].IsLeft ? frame.Hands[0] : null;
            hands[RIGHT] = frame.Hands[0].IsRight ? frame.Hands[0] : null;
        }
        else { hands[LEFT] = hands[RIGHT] = null; }
        return hands;
    }

    /*
     * すべての指が開いているかを返す
     */
    private bool IsOpenFiveFingers(Hand hand)
    {
        foreach (Finger f in hand.Fingers) {
            if (! f.IsExtended) { return false; }
        }
        return true;
    }

    private Vector3 GetVector3(Vector v) {
        return new Vector3(v.x, v.y, v.z);
    }

    
    private void AddRigidbody(GameObject parent)
    {
        foreach (Transform t  in parent.GetComponentsInChildren<Transform>()) {
            if (t.gameObject.name == parent.name) { continue; }

            if (t.gameObject.GetComponent<Collider>() == null) {
                t.gameObject.AddComponent<MeshCollider>().convex = true;
            }
            if (t.gameObject.GetComponent<Rigidbody>() == null) {
                Rigidbody r = t.gameObject.AddComponent<Rigidbody>();
                r.useGravity = false;
                //r.velocity = Vector3.zero;
                //r.useGravity = true;
            }
        }
    }
}
