using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] Transform pivotPoint;

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            RotateAroundPivot(1);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            RotateAroundPivot(-1);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            RotateAroundPivot(-1, true);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            RotateAroundPivot(1, true);
        }
    }
    
    void RotateAroundPivot(int value, bool vertical = false)
    {
        if (!vertical)
        {
            transform.RotateAround(pivotPoint.position, new Vector3(0, value, 0), speed * Time.deltaTime);
        }
        else
        {
            transform.RotateAround(pivotPoint.position, new Vector3(0, 0, value), speed * Time.deltaTime);
        }

    }
}
