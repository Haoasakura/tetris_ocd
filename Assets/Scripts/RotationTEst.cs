using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTEst : MonoBehaviour
{

    public Transform customPivot;
    public Vector3 customP;


void Update()
{
    transform.GetChild(0).RotateAround(customPivot.position, Vector3.forward, 20 * Time.deltaTime);
    transform.GetChild(1).RotateAround(customPivot.position, Vector3.forward, 20 * Time.deltaTime);
    transform.GetChild(2).RotateAround(customPivot.position, Vector3.forward, 20 * Time.deltaTime);
    transform.GetChild(3).RotateAround(customPivot.position, Vector3.forward, 20 * Time.deltaTime);
}
}
