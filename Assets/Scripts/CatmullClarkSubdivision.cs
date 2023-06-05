using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullClarkSubdivision : MonoBehaviour
{
   public MeshFilter originalMeshFilter;
    public MeshFilter subdividedMeshFilter;

    private Mesh _originalMesh;
    private Mesh _subdividedMesh;
    private Dictionary<(int, int), int[]> _edgesTriangleCouple;

    void Start()
    {
        _originalMesh = originalMeshFilter.mesh;
        _subdividedMesh = new Mesh
        {
            name = "SubdividedMesh"
        };
        _edgesTriangleCouple = new Dictionary<(int, int), int[]>();
        Subdivide();
    }

    private void Subdivide()
    {
        var originalVertices = _originalMesh.vertices;
        var originalTriangles = _originalMesh.triangles;
        var faceCenters = new Dictionary<int, Vector3>(_originalMesh.triangles.Length);
        var edgePoints = new Dictionary<Vector3, int[]>();
        var newVertices = new Vector3[originalVertices.Length];
        for (var i = 0; i < originalTriangles.Length; i += 3)
        {
            var v1 = originalTriangles[i];
            var v2 = originalTriangles[i+1];
            var v3 = originalTriangles[i+2];
            faceCenters.Add(i,(originalVertices[v1] + originalVertices[v2] + originalVertices[v3])/3f);

            var edge1 = (v1, v2);
            var edge2 = (v2, v3);
            var edge3 = (v3, v1);
            
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
                if (t != triangles[1] && t != triangles[2]) continue;
                localFacePoint[i++] = vec;
                if (i == 2)
                    break;
            }
            edgePoints.Add(originalVertices[item1] + originalVertices[item2] + localFacePoint[0] + localFacePoint[1] / 4f, triangles);
        }

        for(var i = 0; i < originalVertices.Length; i++)
        {
            var averagePoints = new List<Vector3>();
            var averageFaces = new List<int>();
            foreach (var ((vertex1, vertex2), triangles) in _edgesTriangleCouple)
            {
                if (originalVertices[i] == originalVertices[vertex1] || 
                    originalVertices[i] == originalVertices[vertex2])
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
        }
        
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