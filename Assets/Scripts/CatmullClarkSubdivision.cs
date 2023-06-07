using System;
using System.Collections.Generic;
using UnityEngine;

public class FaceSubdivision
{
    public int TriangleIndex;
    public Vector3 Center;
    public readonly Edge[] Edges;
    public readonly Vector3[] EdgePoints;

    public FaceSubdivision(int index, Vector3 center, Edge[] edges)
    {
        TriangleIndex = index;
        Center = center;
        Edges = edges;
        EdgePoints = new Vector3[3];

        foreach (var edge in Edges)
        {
            edge.AddTriangle(this);
        }
    }

    public int FindIndexOfEdge(Edge other)
    {
        for(var i = 0; i < Edges.Length; i++)
        {
            if (Edges[i].Equals(other)) 
                return i;
        }
        return -1;
    }
}

public class Edge
{
    public readonly int Vertex1;
    public readonly int Vertex2;
    public readonly FaceSubdivision[] Triangles;
    private int _trianglesCount;

    public Edge(int vertex1, int vertex2)
    {
        Vertex1 = vertex1;
        Vertex2 = vertex2;
        Triangles = new FaceSubdivision[2];
        _trianglesCount = 0;
    }

    public bool Equals(Edge other)
    {
        return Vertex1 == other.Vertex1 && Vertex2 == other.Vertex2 || Vertex1 == other.Vertex2 && Vertex2 == other.Vertex1;
    }

    public void AddTriangle(FaceSubdivision triangle)
    {
        Triangles[_trianglesCount++] = triangle;
    }
}

public class CatmullClarkSubdivision : MonoBehaviour
{
    [SerializeField] private MeshFilter originalMeshFilter;
    [SerializeField] private int currentSubdivision;
    [SerializeField] private int subdivisionLimit = 3;

    private Mesh _originalMesh;
    private List<Vector3>_subdividedVerticesList;
    private List<int> _subdividedTrianglesList;
    
    private Vector3[] _subdividedVertices;
    private int[] _subdividedTriangles;

    private List<FaceSubdivision> _faceSubdivisions;
    private List<Edge> _edges;
    
    //private List<GameObject> _pointsList;

    private int _subdivisionCount;

    private List<(int[], Vector3[])> _subdivisionSaves; 

    void Start()
    {
        _originalMesh = originalMeshFilter.mesh;
        _subdividedVerticesList = new List<Vector3>(_originalMesh.vertices.Length);
        _subdividedTrianglesList = new List<int>(_originalMesh.triangles.Length);
        _faceSubdivisions = new List<FaceSubdivision>(_originalMesh.triangles.Length/3);
        _edges = new List<Edge>();
        //_pointsList = new List<GameObject>();
        _subdivisionCount = 0;
        _subdivisionSaves = new List<(int[], Vector3[])>();
        var triangles = new int[_originalMesh.triangles.Length];
        _originalMesh.triangles.CopyTo(triangles, 0);
        var vertices = new Vector3[_originalMesh.vertices.Length];
        _originalMesh.vertices.CopyTo(vertices, 0);
        _subdivisionSaves.Add((triangles, vertices));
        _subdivisionCount++;
    }

    public void HigherSubdivision()
    {
        if (currentSubdivision >= subdivisionLimit) return;
        currentSubdivision++;
        if(currentSubdivision >= _subdivisionCount)
            Subdivide();
        else
        {
            _originalMesh = new Mesh
            {
                vertices = _subdivisionSaves[currentSubdivision].Item2,
                triangles = _subdivisionSaves[currentSubdivision].Item1
            };
            originalMeshFilter.mesh = _originalMesh;
            originalMeshFilter.mesh.RecalculateNormals();
        }
    }

    public void LowerSubdivision()
    {
        if (currentSubdivision <= 0) return;
        currentSubdivision--;
        _originalMesh = new Mesh
        {
            vertices = _subdivisionSaves[currentSubdivision].Item2,
            triangles = _subdivisionSaves[currentSubdivision].Item1
        };
        originalMeshFilter.mesh = _originalMesh;
        originalMeshFilter.mesh.RecalculateNormals();
    }

    private void Subdivide()
    {
        Debug.Log("start subdivide : " + DateTime.Now + " n°" + _subdivisionCount);
        _edges.Clear();
        _faceSubdivisions.Clear();
        _subdividedVerticesList.Clear();
        _subdividedTrianglesList.Clear();
        
        var originalVertices = _originalMesh.vertices;
        int[] originalTriangles;
        if (_subdivisionCount > 1)
        {
            originalTriangles = new int[_originalMesh.triangles.Length / 2];
            Array.Copy(_originalMesh.triangles, originalTriangles, _originalMesh.triangles.Length / 2);
        }
        else
        {
            originalTriangles = _originalMesh.triangles;
        }

        var newVertices = new List<Vector3>(originalVertices.Length);
        
        for (var i = 0; i < originalTriangles.Length; i += 3)
        {
            var v1 = originalTriangles[i];
            var v2 = originalTriangles[i+1];
            var v3 = originalTriangles[i+2];
            var faceCenter = (originalVertices[v1] + originalVertices[v2] + originalVertices[v3]) / 3f;

            var edge1 = (v1, v2);
            var edge2 = (v2, v3);
            var edge3 = (v3, v1);
            var index1 = AddEdges(edge1);
            var index2 = AddEdges(edge2);
            var index3 = AddEdges(edge3);
            var edges = new[] {_edges[index1], _edges[index2], _edges[index3]};
            _faceSubdivisions.Add(new FaceSubdivision(i, faceCenter, edges));
        }

        foreach (var edge in _edges)
        {
            var edgePoint = (originalVertices[edge.Vertex1] + originalVertices[edge.Vertex2] + edge.Triangles[0].Center +
                            edge.Triangles[1].Center) / 4f;
            var index1 = edge.Triangles[0].FindIndexOfEdge(edge);
            var index2 = edge.Triangles[1].FindIndexOfEdge(edge);
            if (index1 == -1 || index2 == -1)
                throw new IndexOutOfRangeException();
            edge.Triangles[0].EdgePoints[index1] = edgePoint;
            edge.Triangles[1].EdgePoints[index2] = edgePoint;
        }

        foreach (var vertex in originalVertices)
        {
            var averagePoints = new List<Vector3>();
            var averageFaces = new List<FaceSubdivision>();
            foreach (var edge in _edges)
            {
                if (originalVertices[edge.Vertex1] != vertex && originalVertices[edge.Vertex2] != vertex) continue;
                averagePoints.Add((originalVertices[edge.Vertex1] + originalVertices[edge.Vertex2]) / 2);
                foreach (var t in edge.Triangles)
                {
                    if(!averageFaces.Contains(t))
                        averageFaces.Add(t);
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
                averageMidFace += face.Center;
            }
            averageMidFace /= averageFaces.Count;
            
            newVertices.Add(1f / averagePoints.Count * averageMidFace + 2f / averagePoints.Count * averageMidPoint +
                            (averagePoints.Count - 3f) / averagePoints.Count * vertex);
        }



        foreach (var triangle in _faceSubdivisions)
        {
            _subdividedVerticesList.Add(triangle.Center);
            var centerIndex = _subdividedVerticesList.IndexOf(triangle.Center);
            for (var i = 0; i < triangle.Edges.Length; i++)
            {
                _subdividedVerticesList.Add(triangle.EdgePoints[i]);
                _subdividedVerticesList.Add(newVertices[triangle.Edges[i].Vertex1]);
                _subdividedVerticesList.Add(newVertices[triangle.Edges[i].Vertex2]);
                _subdividedTrianglesList.Add(centerIndex);
                _subdividedTrianglesList.Add(_subdividedVerticesList.IndexOf(triangle.EdgePoints[i]));
                _subdividedTrianglesList.Add(_subdividedVerticesList.IndexOf(newVertices[triangle.Edges[i].Vertex1]));
                _subdividedTrianglesList.Add(centerIndex);
                _subdividedTrianglesList.Add(_subdividedVerticesList.IndexOf(triangle.EdgePoints[i]));
                _subdividedTrianglesList.Add(_subdividedVerticesList.IndexOf(newVertices[triangle.Edges[i].Vertex2]));
            }
        }
        _subdividedVertices = _subdividedVerticesList.ToArray();
        _subdividedTriangles = _subdividedTrianglesList.ToArray();
        LoopSub.DoubleFaceIndices(ref _subdividedTriangles);
        _originalMesh.vertices = _subdividedVertices;
        _originalMesh.triangles = _subdividedTriangles;
        originalMeshFilter.mesh = _originalMesh;
        originalMeshFilter.mesh.RecalculateNormals();
        originalMeshFilter.mesh.RecalculateTangents();
        var triangles = new int[_originalMesh.triangles.Length];
        _originalMesh.triangles.CopyTo(triangles, 0);
        var vertices = new Vector3[_originalMesh.vertices.Length];
        _originalMesh.vertices.CopyTo(vertices, 0);
        _subdivisionSaves.Add((triangles, vertices));
        _subdivisionCount++;
        Debug.Log("end subdivide : " + DateTime.Now);
    }

    private int AddEdges((int, int) newEdge)
    {
        foreach (var edge in _edges)
        {
            if (!Compare(edge, newEdge)) continue;
            return _edges.IndexOf(edge);
        }
        var e = new Edge(newEdge.Item1, newEdge.Item2);
        _edges.Add(e);
        return _edges.IndexOf(e);
    }

    private bool Compare(Edge edge1, (int, int) edge2)
    {
        var (item1, item2) = edge2;
        return _originalMesh.vertices[edge1.Vertex1] == _originalMesh.vertices[item1] &&
               _originalMesh.vertices[edge1.Vertex2] == _originalMesh.vertices[item2] ||
               _originalMesh.vertices[edge1.Vertex1] == _originalMesh.vertices[item2] &&
               _originalMesh.vertices[edge1.Vertex2] == _originalMesh.vertices[item1];
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
//     go.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
//     go.GetComponent<MeshRenderer>().material.color  = Color.red;
//     _pointsList.Add(go);
// }
//
// foreach (var face in _faceSubdivisions)
// {
//     var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//     go.transform.position = face.Center;
//     go.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
//     go.GetComponent<MeshRenderer>().material.color  = Color.green;
//     _pointsList.Add(go);
//     foreach (var edgePoint in face.EdgePoints)
//     {
//         var go2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//         go2.transform.position = edgePoint;
//         go2.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
//         go2.GetComponent<MeshRenderer>().material.color  = Color.blue;
//         _pointsList.Add(go);
//     }
// }