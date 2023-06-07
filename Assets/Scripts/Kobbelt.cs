using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Kobbelt : MonoBehaviour
{
    public GameObject[] objects;
    private bool[] subDivLevels;
    private int currentMesh = 0;
    public Dropdown dropdown;

    // Start is called before the first frame update
    void Start()
    {
        subDivLevels = new bool[objects.Length];

        for(int i = 0; i < objects.Length; i++)
        {
            subDivLevels[i] = false;
        }
    }

    public void Subdiv()
    {
        MeshFilter meshFilter = objects[currentMesh].GetComponent<MeshFilter>();

        List<int> triangles;
        List<Vector3> vertices = new List<Vector3>(meshFilter.mesh.vertices);

        List<Vector3> newVertices = new List<Vector3>();

        if(subDivLevels[currentMesh] == true)
        {
            int[] temp = meshFilter.mesh.triangles;
            Array.Resize(ref temp, meshFilter.mesh.triangles.Length/2);
            triangles = new List<int>(temp);
        }else{
            triangles = new List<int>(meshFilter.mesh.triangles);
        }

        RemoveDuplicate(ref vertices, ref triangles);

        HashSet<int>[] sums = new HashSet<int>[vertices.Count];

        int[] triCenter = new int[triangles.Count/3];
        List<int>[] triAdjacent = new List<int>[triangles.Count/3];

        for(int i = 0; i < sums.Length; i++)
        {
            sums[i] = new HashSet<int>();
            newVertices.Add(vertices[i]);
        }

        for(int i = 0; i < triCenter.Length; i++)
        {
            triAdjacent[i] = new List<int>();
        }

        //Compute center, map triangles adjacency
        for(int i = 0; i < triangles.Count; i+=3)
        {

            for(int triIndex = 0; triIndex < 3; triIndex++)
            {
                for(int triIndex2 = 0; triIndex2 < 3; triIndex2++)
                {
                    if(triIndex != triIndex2)
                    {
                        sums[triangles[i + triIndex]].Add(triangles[i + triIndex2]);
                    }
                }
            }

            newVertices.Add((vertices[triangles[i]] + vertices[triangles[i+1]] + vertices[triangles[i+2]])/3.0f);
            triCenter[i/3] = newVertices.Count-1;

            for(int i2 = i+3; i2 < triangles.Count; i2+=3)
            {
                int similar = 0;

                for(int cpt = 0; cpt < 3; cpt++)
                {
                    for(int cpt2 = 0; cpt2 < 3; cpt2++)
                    {
                        if(triangles[i+cpt] == triangles[i2+cpt2])
                        {
                            similar++;
                        }
                    }
                }

                if(similar == 2)
                {
                    triAdjacent[i/3].Add(i2/3);
                }
            }
        }

        List<int> newTri = new List<int>();

        for(int i = 0; i < triAdjacent.Length; i++)
        {
            for(int cpt = 0; cpt < triAdjacent[i].Count; cpt++)
            {
                int[] hold = new int[2];
                int holder = 0;

                for(int i2 = 0; i2 < 3; i2++)
                {
                    for(int cpt2 = 0; cpt2 < 3; cpt2++)
                    {
                        if(triangles[i*3 + i2] == triangles[triAdjacent[i][cpt] * 3 + cpt2])
                        {
                            hold[holder] = triangles[i*3 + i2];
                            holder++;
                        }
                    }
                }   

                newTri.Add(triCenter[i]);
                newTri.Add(triCenter[triAdjacent[i][cpt]]);
                newTri.Add(hold[0]);

                // sums[hold[0]].Add(triCenter[i]);
                // sums[hold[0]].Add(triCenter[triAdjacent[i][cpt]]);

                newTri.Add(triCenter[i]);
                newTri.Add(triCenter[triAdjacent[i][cpt]]);
                newTri.Add(hold[1]);

                // sums[hold[1]].Add(triCenter[i]);
                // sums[hold[1]].Add(triCenter[triAdjacent[i][cpt]]);
            }
        }

        for(int i = 0; i < sums.Length; i++)
        {
            if(sums[i].Count == 0) continue;

            Vector3 sum = Vector3.zero;

            foreach(int index in sums[i])
            {
                if(index < vertices.Count)
                {
                    sum += vertices[index];
                }else{
                    sum += newVertices[index];
                }
                
            }

            float beta = (4f - 2f * Mathf.Cos(2f * Mathf.PI / sums[i].Count)) / 9f;

            //Debug.Log(sum * 1000 + " " + sums[i].Count);

            newVertices[i] = vertices[i] * (1 - beta) + beta / sums[i].Count * sum;

            //Debug.Log(vertices[i] * 1000);
        }

        int[] triArray = newTri.ToArray();

        DoubleFaceIndices(ref triArray);

        meshFilter.mesh.SetVertices(newVertices.ToArray());
        meshFilter.mesh.SetIndices(triArray, MeshTopology.Triangles, 0);
    }

    public void RemoveDuplicate(ref List<Vector3> vertices, ref List<int> triangles)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            for (int cpt = i + 1; cpt < vertices.Count; cpt++)
            {
                if (vertices[i] == vertices[cpt])
                {
                    for (int i2 = 0; i2 < triangles.Count; i2++)
                    {
                        if (triangles[i2] == cpt)
                        {
                            triangles[i2] = i;
                        }
                        else if (triangles[i2] > cpt)
                        {
                            //triangles[i2]--;
                        }
                    }

                    //vertices.RemoveAt(cpt);
                }
            }
        }
    }

    public void DoubleFaceIndices(ref int[] indices)
    {
        int n = 3;

        if ((indices.Length % n) != 0)
        {
            return;
        }

        int indicesLength = indices.Length;

        Array.Resize(ref indices, indices.Length * 2);

        for (int i = 0; i < indicesLength; i += n)
        {
            for (int cpt = 0; cpt < n; cpt++)
            {
                indices[indicesLength + i + cpt] = indices[i + n - 1 - cpt];
            }
        }
    }

    public void DropdownValueChanged()
    {
        objects[currentMesh].SetActive(false);
        currentMesh = dropdown.value;
        objects[currentMesh].SetActive(true);
    }
}
