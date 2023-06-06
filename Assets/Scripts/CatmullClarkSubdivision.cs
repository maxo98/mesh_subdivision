using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

class FaceSubdivision
{
    public int TriangleIndex;
    public Vector3 Center;
    public (int, int)[] Edges;
    public int[] EdgePoints;

    public FaceSubdivision()
    {
        TriangleIndex = -1;
        Center = new Vector3();
        Edges = new (int, int)[3];
        EdgePoints = new int[3];
    }

    public FaceSubdivision(int index, Vector3 center, (int, int)[] edges, int[] edgePoins)
    {
        TriangleIndex = index;
        Center = center;
        Edges = edges;
        EdgePoints = edgePoins;
    }
}

public class CatmullClarkSubdivision : MonoBehaviour
{
    public MeshFilter originalMeshFilter;

    private Mesh _originalMesh;

    private List<Vector3>_subdividedVerticesList;
    private List<int> _subdividedTrianglesList;
    
    private Vector3[] _subdividedVertices;
    private int[] _subdividedTriangles;

    private List<FaceSubdivision> _faceSubdivisions;
    
    private Dictionary<(int, int), int[]> _edgesTriangleCouple;
    //private List<GameObject> _pointsList;

    void Start()
    {
        _originalMesh = originalMeshFilter.mesh;
        _edgesTriangleCouple = new Dictionary<(int, int), int[]>();
        _subdividedVerticesList = new List<Vector3>(_originalMesh.vertices.Length);
        _subdividedTrianglesList = new List<int>(_originalMesh.triangles.Length);
        _faceSubdivisions = new List<FaceSubdivision>(_originalMesh.triangles.Length/3);

        //_pointsList = new List<GameObject>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Subdivide();
        }
    }

    private void Subdivide()
    {
        Debug.Log("start subdivid : " + DateTime.Now);
        _edgesTriangleCouple.Clear();
        _subdividedVerticesList.Clear();
        _subdividedTrianglesList.Clear();
        var originalVertices = _originalMesh.vertices;
        var originalTriangles = _originalMesh.triangles;
        
        var faceCenters = new Dictionary<int, Vector3>(_originalMesh.triangles.Length);
        var edgePoints = new Dictionary<Vector3, (int, int)>();
        var newVertices = new List<Vector3>(originalVertices.Length);
        
        for (var i = 0; i < originalTriangles.Length; i += 3)
        {
            var v1 = originalTriangles[i];
            var v2 = originalTriangles[i+1];
            var v3 = originalTriangles[i+2];
            var faceCenter = (originalVertices[v1] + originalVertices[v2] + originalVertices[v3]) / 3f;
            faceCenters.Add(i,faceCenter);

            var edge1 = (v1, v2);
            var edge2 = (v2, v3);
            var edge3 = (v3, v1);
            
            //_faceSubdivisions.Add(new FaceSubdivision(i, faceCenter, ));
            
            AddToDictionary(edge1, i);
            AddToDictionary(edge2, i);
            AddToDictionary(edge3, i);
        }

        
        foreach (var ((item1, item2), triangles) in _edgesTriangleCouple)
        {
            var localFacePoint = new Vector3[2];
            var i = 0;
            foreach (var (t, vec) in faceCenters)
            {
                if (t != triangles[0] && t != triangles[1]) continue;
                localFacePoint[i++] = vec;
                if (i == 2)
                    break;
            }
            edgePoints.Add((originalVertices[item1] + originalVertices[item2] + localFacePoint[0] + localFacePoint[1]) / 4f, (item1, item2));
        }

        foreach (var t1 in originalVertices)
        {
            var averagePoints = new List<Vector3>();
            var averageFaces = new List<int>();
            foreach (var ((vertex1, vertex2), triangles) in _edgesTriangleCouple)
            {
                if (t1 == originalVertices[vertex1] || 
                    t1 == originalVertices[vertex2])
                {
                    averagePoints.Add((originalVertices[vertex1] + originalVertices[vertex2]) / 2);
                    foreach (var t in triangles)
                    {
                        if(!averageFaces.Contains(t))
                            averageFaces.Add(t);
                    }
                }
            }

            var averageMidPoint = new Vector3();
            foreach (var vec in averagePoints)
            {
                averageMidPoint += vec;
            }
            averageMidPoint /= averagePoints.Count;

            var averageMidFace = new Vector3();
            foreach (var face in averageFaces)
            {
                averageMidFace += faceCenters[face];
            }
            averageMidFace /= averageFaces.Count;
            
            newVertices.Add(1f / averagePoints.Count * averageMidFace + 2f / averagePoints.Count * averageMidPoint +
                            (averagePoints.Count - 3f) / averagePoints.Count * t1);
        }


        foreach (var (t, center) in faceCenters)
        {
            var v1 = originalTriangles[t];
            var v2 = originalTriangles[t+1];
            var v3 = originalTriangles[t+2];

            
            foreach (var ((vertex1, vertex2), triangles) in _edgesTriangleCouple)
            {
                if (!triangles.Contains(t)) continue;
                foreach (var (vertexEdgePoint, edges) in edgePoints)
                {
                    if (edges != (vertex1, vertex2)) continue;
                    _subdividedVerticesList.Add(center);
                    _subdividedVerticesList.Add(vertexEdgePoint);
                    _subdividedVerticesList.Add(newVertices[vertex1]);
                    _subdividedVerticesList.Add(newVertices[vertex2]);
                    _subdividedTrianglesList.Add(_subdividedVerticesList.IndexOf(center));
                    _subdividedTrianglesList.Add(_subdividedVerticesList.IndexOf(vertexEdgePoint));
                    _subdividedTrianglesList.Add(_subdividedVerticesList.IndexOf(newVertices[vertex1]));
                    _subdividedTrianglesList.Add(_subdividedVerticesList.IndexOf(center));
                    _subdividedTrianglesList.Add(_subdividedVerticesList.IndexOf(vertexEdgePoint));
                    _subdividedTrianglesList.Add(_subdividedVerticesList.IndexOf(newVertices[vertex2]));
                    break;
                }
            }
        }
        
        _subdividedVertices = _subdividedVerticesList.ToArray();
        _subdividedTriangles = _subdividedTrianglesList.ToArray();
        LoopSub.DoubleFaceIndices(ref _subdividedTriangles);
        _originalMesh.vertices = _subdividedVertices;
        _originalMesh.triangles = _subdividedTriangles;
        originalMeshFilter.mesh = _originalMesh;
        
        Debug.Log("end subdivid : " + DateTime.Now);
    }

    private void AddToDictionary((int, int) edge, int triangle)
    {
        foreach (var ((item1, item2), triangles) in _edgesTriangleCouple)
        {
            if (_originalMesh.vertices[item1] == _originalMesh.vertices[edge.Item1] &&
                _originalMesh.vertices[item2] == _originalMesh.vertices[edge.Item2] ||
                _originalMesh.vertices[item2] == _originalMesh.vertices[edge.Item1] &&
                _originalMesh.vertices[item1] == _originalMesh.vertices[edge.Item2])
            {
                triangles[1] = triangle;
                return;
            }
        }
        _edgesTriangleCouple.Add(edge, new []{triangle, -1});
    }
}

// foreach (var go in _pointsList)
// {
//     Destroy(go);
// }
// _pointsList.Clear();

// foreach (var vertex in newVertices)
// {
//     var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//     go.transform.position = vertex;
//     go.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
//     go.GetComponent<MeshRenderer>().material.color  = Color.red;
//     _pointsList.Add(go);
// }
//
// foreach (var vertex in edgePoints)
// {
//     var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//     go.transform.position = vertex.Key;
//     go.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
//     go.GetComponent<MeshRenderer>().material.color  = Color.blue;
//     _pointsList.Add(go);
// }
//
// foreach (var vertex in faceCenters)
// {
//     var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//     go.transform.position = vertex.Value;
//     go.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
//     go.GetComponent<MeshRenderer>().material.color = Color.green;
//     _pointsList.Add(go);
// }