using System.Collections.Generic;
using UnityEngine;

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
    
    private DirectionSelected _idDirectionSelected;
    private Dictionary<DirectionSelected, List<GameObject>> _listPointsByFace;

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
    }

    void Update()
    {
        ManagerClicks();
    }

    private void ManagerClicks()
    {
        if (Input.GetMouseButtonDown(0))
            OnLeftClick();
    }
    
    private void OnLeftClick()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        var layerMask = LayerMask.GetMask("PlanClickable");

        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore) || _listPointsByFace[_idDirectionSelected].Count >= numberOfPoints)
            return;

        var go = Instantiate(pointToDraw, hit.point, Quaternion.identity);
        _listPointsByFace[_idDirectionSelected].Add(go);
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
