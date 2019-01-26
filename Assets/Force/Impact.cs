using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impact : MonoBehaviour
{
    public float coefficient;
    public Vector3 velocity;

    void OnTriggerStay(Collider other) {
        Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();
        if (otherRigidbody == null) return;

        Vector3 relativeVelocity = this.velocity - otherRigidbody.velocity;
        otherRigidbody.AddForce(coefficient * relativeVelocity);
    }
}
