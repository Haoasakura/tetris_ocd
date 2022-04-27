using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyIfEmpty : MonoBehaviour
{
     void Update()
    {
        if (transform.childCount < 1) {
            Destroy(gameObject);
        }
    }
}
