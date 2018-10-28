using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Lightning
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Lightning))]
    public class LightningEditor : Editor
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /*
        void OnSceneGUI()
        {
            var instance = (Lightning)target;

            EditorGUI.BeginChangeCheck();

            var p0 = instance.emitter;
            var p1 = instance.receiver;

            if (instance.emitterTransform == null)
                p0 = Handles.PositionHandle(p0, Quaternion.identity);

            if (instance.receiverTransform == null)
                p1 = Handles.PositionHandle(p1, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Move Position");

                instance.emitter = p0;
                instance.receiver = p1;

                EditorUtility.SetDirty(target);
            }
        }
        */
    }
}
