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
    [SerializeField] private Material meshPointMaterial;
    [SerializeField] private Material finalPointMaterial;
    [SerializeField] private float u;
    [SerializeField] private float v;
    
    private DirectionSelected _idDirectionSelected;
    private Dictionary<DirectionSelected, List<GameObject>> _listPointsByFace;
    private Dictionary<DirectionSelected, List<GameObject>> _listPointsSegmented;
    
    private List<List<GameObject>> _matrixA;
    private List<List<GameObject>> _matrixB;
    private List<List<GameObject>> _matrixC;
    private List<List<GameObject>> _matrixD;
    
    public GameObject pointA;
    public GameObject pointB;
    public GameObject pointC;
    public GameObject pointD;
    
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

        _matrixA = new List<List<GameObject>>();
        _matrixB = new List<List<GameObject>>();
        _matrixC = new List<List<GameObject>>();
        _matrixD = new List<List<GameObject>>();
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
            BuildGrid(_listPointsSegmented[DirectionSelected.Front], _listPointsSegmented[DirectionSelected.Back], _listPointsSegmented[DirectionSelected.Right].Count, _matrixA);
            BuildGrid(_listPointsSegmented[DirectionSelected.Right], _listPointsSegmented[DirectionSelected.Left], _listPointsSegmented[DirectionSelected.Front].Count, _matrixB);
            BuildGridC();
            BuildCoons();
        }
    }

    public void HidePointsByFace()
    {
        foreach (var points in _listPointsByFace)
        {
            for (var i = 1; i < points.Value.Count - 1; ++i)
            {
                points.Value[i].SetActive(false);
            }
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

            _listPointsSegmented[pair.Key].Add(pair.Value[0]);
            
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

            if (pair.Key == DirectionSelected.Right)
            {
                var pointA = _listPointsByFace[pair.Key][^1].transform.position;
                var pointB = _listPointsByFace[DirectionSelected.Back][0].transform.position;
                var segmentationPointA = pointA + (pointB - pointA) * u;
                var segmentationPointB = pointA + (pointB - pointA) * v;
                InstantiatePoint(segmentationPointA, segmentationPointMaterial,
                    _listPointsSegmented[pair.Key]);
                InstantiatePoint(segmentationPointB, segmentationPointMaterial,
                    _listPointsSegmented[pair.Key]);
                _listPointsSegmented[pair.Key].Add(_listPointsSegmented[DirectionSelected.Back][0]);
            } else
                _listPointsSegmented[pair.Key].Add(pair.Value[^1]);
        }

        _listPointsSegmented[DirectionSelected.Back].Reverse();
        _listPointsSegmented[DirectionSelected.Right].Reverse();
    }

    private void BuildGrid(IReadOnlyList<GameObject> face, IReadOnlyList<GameObject> back, float size, List<List<GameObject>> matrix)
    { 
        var step = 1 / (size - 1);
        
        for (var i = 0; i < face.Count; ++i)
        {
            matrix.Add(new List<GameObject>());
            matrix[^1].Add(face[i]);

            for (var j = 1; j < size - 1; ++j)
            {
                var position = (1.0f - step * j) * face[i].transform.position +
                    (step * j) * back[i].transform.position;
                InstantiatePoint(position, meshPointMaterial, matrix[^1]);
            }
            
            matrix[^1].Add(back[i]);
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
        mainCamera.transform.position = new Vector3(20, 100, 0);
        mainCamera.transform.rotation = Quaternion.Euler(0, -90, 0);
        _idDirectionSelected = DirectionSelected.Right;
    }
    
    private void OnClickMoveCameraLeftSide()
    {
        mainCamera.transform.position = new Vector3(-20, 100, 0);
        mainCamera.transform.rotation = Quaternion.Euler(0, 90, 0);
        _idDirectionSelected = DirectionSelected.Left;
    }
    
    private void OnClickMoveCameraBackSide()
    {
        mainCamera.transform.position = new Vector3(0, 100, 20);
        mainCamera.transform.rotation = Quaternion.Euler(0, 180, 0);
        _idDirectionSelected = DirectionSelected.Back;
    }

    public void MeshA()
    {
        foreach (var go in _matrixA.SelectMany(line => line))
        {
            go.SetActive(!go.activeSelf);
        }
    }
    
    public void MeshB()
    {
        foreach (var go in _matrixB.SelectMany(line => line))
        {
            go.SetActive(!go.activeSelf);
        }
    }

    public void MeshC()
    {
        foreach (var go in _matrixC.SelectMany(line => line))
        {
            go.SetActive(!go.activeSelf);
        }
    }
    
    public void MeshD()
    {
        foreach (var go in _matrixD.SelectMany(line => line))
        {
            go.SetActive(!go.activeSelf);
        }
    }
    
    private void BuildGridC()
    {
        var step = 1 / ((float)_listPointsSegmented[DirectionSelected.Front].Count - 1);

        pointA = _listPointsByFace[DirectionSelected.Front][0];
        pointB = _listPointsByFace[DirectionSelected.Front][^1];
        pointC = _listPointsByFace[DirectionSelected.Back][0];
        pointD = _listPointsByFace[DirectionSelected.Back][^1];
        
        for (var x = 0; x < _listPointsSegmented[DirectionSelected.Right].Count; ++x)
        {
            _matrixC.Add(new List<GameObject>());
            var Xy = (1 - (step * x)) * pointA.transform.position + (step * x) * pointB.transform.position;
            var Yy = (1 - (step * x)) * pointD.transform.position + (step * x) * pointC.transform.position;
            var defaultPointAPosition = _listPointsSegmented[DirectionSelected.Front][x].transform.position;
            var defaultPointBPosition = _listPointsSegmented[DirectionSelected.Back][x].transform.position;
            for (var y = 0; y < _listPointsSegmented[DirectionSelected.Front].Count; ++y)
            {
                var position = (1.0f - (step * y)) * new Vector3(defaultPointAPosition.x, Xy.y, defaultPointAPosition.z) + (step * y) * new Vector3(defaultPointBPosition.x, Yy.y, defaultPointBPosition.z);
                InstantiatePoint(position, defaultPointMaterial, _matrixC[^1]);
            }
        }
    }

    private void BuildCoons()
    {
        for (var x = 0; x < _matrixA.Count; ++x)
        {
            _matrixD.Add(new List<GameObject>());
            
            for (var y = 0; y < _matrixA[x].Count; ++y)
            {
                var pointA = _matrixA[x][y].transform.position;
                var pointB = _matrixB[y][x].transform.position;
                var pointC = _matrixC[x][y].transform.position;

                var finalPointPosition = pointA + pointB - pointC;
                InstantiatePoint(finalPointPosition, finalPointMaterial, _matrixD[^1]);
            }
        }
    }
}