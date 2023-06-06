using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

public class CurveCornerCuttingAlgo : MonoBehaviour
{
    private enum DirectionSelected
    {
        Front = 0,
        Back = 1,
        Left = 2,
        Right = 3
    }
    
    [SerializeField] private int numberOfPoints;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject pointToDraw;
    [SerializeField] private Material defaultPointMaterial;
    [SerializeField] private Material segmentationPointMaterial;
    [SerializeField] private float u;
    [SerializeField] private float v;
    
    private DirectionSelected _idDirectionSelected;
    private Dictionary<DirectionSelected, List<GameObject>> _listPointsByFace;
    private Dictionary<DirectionSelected, List<GameObject>> _listPointsSegmented;
    
    private List<Vector3> _matrixA;
    private List<Vector3> _matrixB;
    private List<Vector3> _matrixC;
    
    private Mesh _reworkedObject;
    
    void Start()
    {
        _idDirectionSelected = DirectionSelected.Front;
        _listPointsByFace = new Dictionary<DirectionSelected, List<GameObject>>
        {
            [DirectionSelected.Front] = new List<GameObject>(numberOfPoints),
            [DirectionSelected.Back] = new List<GameObject>(numberOfPoints),
            [DirectionSelected.Left] = new List<GameObject>(numberOfPoints),
            [DirectionSelected.Right] = new List<GameObject>(numberOfPoints)
        };
        
        _listPointsSegmented = new Dictionary<DirectionSelected, List<GameObject>>
        {
            [DirectionSelected.Front] = new List<GameObject>(),
            [DirectionSelected.Back] = new List<GameObject>(),
            [DirectionSelected.Left] = new List<GameObject>(),
            [DirectionSelected.Right] = new List<GameObject>()
        };
    }

    void Update()
    {
        ManagerMouse();
        ManagerKeyboard();
    }

    private void DrawPolygon()
    {
        foreach (var pair in _listPointsSegmented)
        {
            for (var i = 0; i < pair.Value.Count - 1; ++i)
            {
                DrawLine(pair.Value[i].transform.position, pair.Value[i  + 1].transform.position);
            }
        }
    }

    private void DrawLine(Vector3 startPosition, Vector3 endPosition)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = startPosition;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = defaultPointMaterial;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.SetPosition(0, startPosition);
        lr.SetPosition(1, endPosition);
    }
    
    private void ManagerKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Segmentation();
            DrawPolygon();
        }
    }
    private void ManagerMouse()
    {
        if (Input.GetMouseButtonDown(0))
            OnLeftClick();
    }

    private void Segmentation()
    {
        foreach (var pair in _listPointsByFace)
        {
            if (pair.Value.Count < 3)
                continue;

            for (var i = 0; i < pair.Value.Count - 1; ++i)
            {
                var pointA = pair.Value[i].transform.position;
                var pointB = pair.Value[i + 1].transform.position;
                var segmentationPointA = pointA + (pointB - pointA) * u;
                var segmentationPointB = pointA + (pointB - pointA) * v;
                InstantiatePoint(segmentationPointA, segmentationPointMaterial,
                    _listPointsSegmented[pair.Key]);
                InstantiatePoint(segmentationPointB, segmentationPointMaterial,
                    _listPointsSegmented[pair.Key]);
            }
        }
    }

    private void InstantiatePoint(Vector3 position, Material materialPoint, List<GameObject> listPoints = null)
    {
        var go = Instantiate(pointToDraw, position, Quaternion.identity);
        go.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        go.GetComponent<MeshRenderer>().material = materialPoint;
        listPoints?.Add(go);
    }
    
    private void OnLeftClick()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        var layerMask = LayerMask.GetMask("PlanClickable");

        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore) || _listPointsByFace[_idDirectionSelected].Count >= numberOfPoints)
            return;

        if ((_idDirectionSelected == DirectionSelected.Right || _idDirectionSelected == DirectionSelected.Left) &&
            _listPointsByFace[_idDirectionSelected].Count == 4)
        {
            if (_idDirectionSelected == DirectionSelected.Right)
                _listPointsByFace[_idDirectionSelected].Add(_listPointsByFace[DirectionSelected.Back][0]);
            else
                _listPointsByFace[_idDirectionSelected].Add(_listPointsByFace[DirectionSelected.Front][0]);
        }
        else 
            InstantiatePoint(hit.point, defaultPointMaterial, _listPointsByFace[_idDirectionSelected]);

        switch (_idDirectionSelected)
        {
            case DirectionSelected.Front when _listPointsByFace[_idDirectionSelected].Count == numberOfPoints:
                _listPointsByFace[DirectionSelected.Right].Add(_listPointsByFace[_idDirectionSelected][numberOfPoints - 1]);
                break;
            case DirectionSelected.Back when _listPointsByFace[_idDirectionSelected].Count == numberOfPoints:
                _listPointsByFace[DirectionSelected.Left].Add(_listPointsByFace[_idDirectionSelected][numberOfPoints - 1]);
                break;
        }
    }

    public void OnClickSwitchCameraFace()
    {
        if (_idDirectionSelected == DirectionSelected.Right)
            return;

        switch (_idDirectionSelected + 1)
        {
            case DirectionSelected.Left:
                OnClickMoveCameraLeftSide();
                break;
            case DirectionSelected.Right:
                OnClickMoveCameraRightSide();
                break;
            case DirectionSelected.Back:
                OnClickMoveCameraBackSide();
                break;
        }
    }
    
    private void OnClickMoveCameraRightSide()
    {
        mainCamera.transform.position = new Vector3(20, 0, 0);
        mainCamera.transform.rotation = Quaternion.Euler(0, -90, 0);
        _idDirectionSelected = DirectionSelected.Right;
    }

    private void OnClickMoveCameraLeftSide()
    {
        mainCamera.transform.position = new Vector3(-20, 0, 0);
        mainCamera.transform.rotation = Quaternion.Euler(0, 90, 0);
        _idDirectionSelected = DirectionSelected.Left;
    }

    private void OnClickMoveCameraFrontSide()
    {
        mainCamera.transform.position = new Vector3(0, 0, -20);
        mainCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
        _idDirectionSelected = DirectionSelected.Front;
    }

    private void OnClickMoveCameraBackSide()
    {
        mainCamera.transform.position = new Vector3(0, 0, 20);
        mainCamera.transform.rotation = Quaternion.Euler(0, 180, 0);
        _idDirectionSelected = DirectionSelected.Back;
    }
}
