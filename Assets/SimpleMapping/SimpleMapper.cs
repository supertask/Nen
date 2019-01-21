using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMapper : MonoBehaviour
{
    private GameObject originBox;
    private GameObject mapped;
    private GameObject selecting;
    private GameObject field;

    // Start is called before the first frame update
    void Start()
    {
        this.originBox = GameObject.Find("OriginCube");
        this.mapped = GameObject.Find("Mapped");
        this.selecting = GameObject.Find("Selecting");
        this.field = GameObject.Find("Field");
        this.Load();
    }

    /*
     * セーブデータを読み込む
     */
    void Load()
    {
        if (PlayerPrefs.HasKey("mappedPosList"))
        {
            Vector3[] posList = PlayerPrefsX.GetVector3Array("mappedPosList");
            Quaternion[] rotList = PlayerPrefsX.GetQuaternionArray("mappedRotList");
            Vector3[] scaleList = PlayerPrefsX.GetVector3Array("mappedScaleList");
            for(int i = 0; i < posList.Length; i++)
            {
                GameObject obj = Object.Instantiate(this.originBox) as GameObject;
                obj.transform.position = posList[i];
                obj.transform.rotation = rotList[i];
                obj.transform.localScale = scaleList[i];
                obj.transform.parent = this.mapped.transform;
                obj.GetComponent<Renderer>().enabled = true;
            }
        }
        else {
            //セーブデータがない場合，何もしない
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.B)) {
            this.mapSelectedObjects(); //選択したオブジェクトをマッピングする

            //boxを作成
            Vector3 p = this.originBox.transform.position;
            GameObject box = Object.Instantiate(originBox) as GameObject; //コピー
            box.transform.position = p;
            box.GetComponent<Renderer>().enabled = true;
            box.transform.parent = this.selecting.transform;

            //
        } else if (Input.GetKeyUp(KeyCode.E)) {
            //マッピング終了
            this.mapSelectedObjects(); //選択したオブジェクトをマッピングする
            this.Save();
        }
    }

    /*
     * 選択したオブジェクトをマップ済みオブジェクトへ登録する
     */
    private void mapSelectedObjects() {
        foreach(Transform child in this.selecting.transform) {
            child.parent = this.mapped.transform;
        }
    }

    /*
     * データをPlayerPrefsXに保存する
     */
    public void Save()
    {
        List<Vector3> savingPosList = new List<Vector3>() { };
        List<Quaternion> savingRotList = new List<Quaternion>() { };
        List<Vector3> savingScaleList = new List<Vector3>() { };
        foreach(Transform transform in this.mapped.transform) {
            savingPosList.Add(transform.position);
            savingRotList.Add(transform.rotation);
            savingScaleList.Add(transform.localScale);
        }
        PlayerPrefsX.SetVector3Array("mappedPosList", savingPosList.ToArray());
        PlayerPrefsX.SetQuaternionArray("mappedRotList", savingRotList.ToArray());
        PlayerPrefsX.SetVector3Array("mappedScaleList", savingScaleList.ToArray());
    }

    void OnApplicationQuit()
    {
    }
}
