using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
            [DirectionSelected.Front] = new List<GameObject>(),
            [DirectionSelected.Back] = new List<GameObject>(),
            [DirectionSelected.Left] = new List<GameObject>(),
            [DirectionSelected.Right] = new List<GameObject>()
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

        InstantiatePoint(hit.point, defaultPointMaterial, _listPointsByFace[_idDirectionSelected]);
    }
    
    public void OnClickMoveCameraRightSide()
    {
        mainCamera.transform.position = new Vector3(20, 0, 0);
        mainCamera.transform.rotation = Quaternion.Euler(0, -90, 0);
        _idDirectionSelected = DirectionSelected.Right;
    }

    public void OnClickMoveCameraLeftSide()
    {
        mainCamera.transform.position = new Vector3(-20, 0, 0);
        mainCamera.transform.rotation = Quaternion.Euler(0, 90, 0);
        _idDirectionSelected = DirectionSelected.Left;
    }

    public void OnClickMoveCameraFrontSide()
    {
        mainCamera.transform.position = new Vector3(0, 0, -20);
        mainCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
        _idDirectionSelected = DirectionSelected.Front;
    }

    public void OnClickMoveCameraBackSide()
    {
        mainCamera.transform.position = new Vector3(0, 0, 20);
        mainCamera.transform.rotation = Quaternion.Euler(0, 180, 0);
        _idDirectionSelected = DirectionSelected.Back;
    }
}
