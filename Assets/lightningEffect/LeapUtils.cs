using UnityEngine;
using Leap;
using Leap.Unity;

public class LeapUtils {
    public static Vector3 ToVector3(Vector v) {
        return new Vector3(v.x, v.y, v.z);
    }
    public static Quaternion ToQuaternion(LeapQuaternion q) {
        return new Quaternion(q.x, q.y, q.z, q.w);
    }

    public static Vector3 Deg2RadAtVector3(Vector3 degree) {
        return new Vector3(
            degree.x * Mathf.Deg2Rad,
            degree.y * Mathf.Deg2Rad,
            degree.z * Mathf.Deg2Rad
        );
    }
}
