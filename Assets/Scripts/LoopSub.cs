using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class LoopSub : MonoBehaviour
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

        if(subDivLevels[currentMesh] == true)
        {
            int[] temp = meshFilter.mesh.triangles;
            Array.Resize(ref temp, meshFilter.mesh.triangles.Length/2);
            triangles = new List<int>(temp);
        }else{
            triangles = new List<int>(meshFilter.mesh.triangles);
        }
        

        RemoveDuplicate(ref vertices, ref triangles);

        Dictionary<(int, int), int> edgeToVert = new Dictionary<(int, int), int>();
        List<Vector3> newVert = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        List<Vector3>[] sums = new List<Vector3>[vertices.Count];
        int[] neighbors = new int[vertices.Count];

        for(int i = 0; i < vertices.Count; i++)
        {
            neighbors[i] = 0;
            sums[i] = new List<Vector3>();
            newVert.Add(Vector3.zero);
        }

        //Odd vertices
        for(int i = 0; i < triangles.Count; i+=3)
        {
            if(triangles[i] < triangles[i+1] && edgeToVert.ContainsKey((triangles[i], triangles[i+1])) == false)
            {
                
                int a = triangles[i];
                int b = triangles[i+1];
                int c = triangles[i+2];
                int d;

                if(FindTriangle(out d, ref triangles, (triangles[i], triangles[i+1]), i) == true)
                {
                    newVert.Add(3.0f/8.0f * (vertices[a] + vertices[b]) + 1.0f/8.0f * (vertices[c] + vertices[d]));
                    
                }else{
                    newVert.Add(0.5f * (vertices[a] + vertices[b]));
                }

                edgeToVert[(triangles[i], triangles[i+1])] = newVert.Count-1;

                sums[triangles[i]].Add(newVert[newVert.Count-1]);
                sums[triangles[i+1]].Add(newVert[newVert.Count-1]);

            }else if(triangles[i] > triangles[i+1] && edgeToVert.ContainsKey((triangles[i+1], triangles[i])) == false)
            {

                int a = triangles[i];
                int b = triangles[i+1];
                int c = triangles[i+2];
                int d;

                if(FindTriangle(out d, ref triangles, (triangles[i+1], triangles[i]), i) == true)
                {
                    newVert.Add(3.0f/8.0f * (vertices[a] + vertices[b]) + 1.0f/8.0f * (vertices[c] + vertices[d]));

                }else{
                    newVert.Add(0.5f * (vertices[a] + vertices[b]));
                }

                edgeToVert[(triangles[i+1], triangles[i])] = newVert.Count-1;

                sums[triangles[i]].Add(newVert[newVert.Count-1]);
                sums[triangles[i+1]].Add(newVert[newVert.Count-1]);
            }

            if(triangles[i+1] < triangles[i+2] && edgeToVert.ContainsKey((triangles[i+1], triangles[i+2])) == false)
            {
                
                int a = triangles[i+1];
                int b = triangles[i+2];
                int c = triangles[i];
                int d;

                if(FindTriangle(out d, ref triangles, (triangles[i+1], triangles[i+2]), i) == true)
                {
                    newVert.Add(3.0f/8.0f * (vertices[a] + vertices[b]) + 1.0f/8.0f * (vertices[c] + vertices[d]));

                }else{
                    newVert.Add(0.5f * (vertices[a] + vertices[b]));
                }

                edgeToVert[(triangles[i+1], triangles[i+2])] = newVert.Count-1;

                sums[triangles[i+2]].Add(newVert[newVert.Count-1]);
                sums[triangles[i+1]].Add(newVert[newVert.Count-1]);

            }else if(triangles[i+1] > triangles[i+2] && edgeToVert.ContainsKey((triangles[i+2], triangles[i+1])) == false)
            {
                int a = triangles[i+1];
                int b = triangles[i+2];
                int c = triangles[i];
                int d;

                if(FindTriangle(out d, ref triangles, (triangles[i+2], triangles[i+1]), i) == true)
                {
                    newVert.Add(3.0f/8.0f * (vertices[a] + vertices[b]) + 1.0f/8.0f * (vertices[c] + vertices[d]));

                }else{
                    newVert.Add(0.5f * (vertices[a] + vertices[b]));
                }

                edgeToVert[(triangles[i+2], triangles[i+1])] = newVert.Count-1;

                sums[triangles[i+2]].Add(newVert[newVert.Count-1]);
                sums[triangles[i+1]].Add(newVert[newVert.Count-1]);
            }

            if(triangles[i+2] < triangles[i] && edgeToVert.ContainsKey((triangles[i+2], triangles[i])) == false)
            {

                int a = triangles[i+2];
                int b = triangles[i];
                int c = triangles[i+1];
                int d;

                if(FindTriangle(out d, ref triangles, (triangles[i+2], triangles[i]), i) == true)
                {
                    newVert.Add(3.0f/8.0f * (vertices[a] + vertices[b]) + 1.0f/8.0f * (vertices[c] + vertices[d]));

                }else{
                    newVert.Add(0.5f * (vertices[a] + vertices[b]));
                }

                edgeToVert[(triangles[i+2], triangles[i])] = newVert.Count-1;

                sums[triangles[i]].Add(newVert[newVert.Count-1]);
                sums[triangles[i+2]].Add(newVert[newVert.Count-1]);

            }else if(triangles[i+2] > triangles[i] && edgeToVert.ContainsKey((triangles[i], triangles[i+2])) == false)
            {

                int a = triangles[i+2];
                int b = triangles[i];
                int c = triangles[i+1];
                int d;

                if(FindTriangle(out d, ref triangles, (triangles[i], triangles[i+2]), i) == true)
                {
                    newVert.Add(3.0f/8.0f * (vertices[a] + vertices[b]) + 1.0f/8.0f * (vertices[c] + vertices[d]));

                }else{
                    newVert.Add(0.5f * (vertices[a] + vertices[b]));
                }

                edgeToVert[(triangles[i], triangles[i+2])] = newVert.Count-1;

                sums[triangles[i]].Add(newVert[newVert.Count-1]);
                sums[triangles[i+2]].Add(newVert[newVert.Count-1]);
            }
        }

        //Even vertices
        for(int i = 0; i < vertices.Count; i++)
        {
            float n = 0;
            Vector3 a = Vector3.zero;
            Vector3 b = Vector3.zero;

            Vector3 sum = Vector3.zero;

            for(int cpt = 0; cpt < sums[i].Count; cpt++)
            {
                sum += sums[i][cpt];
            }

            n = sums[i].Count;
            

            if(n > 2)
            {
                float beta = 0;
            
                if(n > 3)
                {
                    beta = (1 / n) * (5.0f/8.0f - Mathf.Pow(3.0f/8.0f + 1.0f/4.0f * Mathf.Cos((2.0f * Mathf.PI)/n ), 2));
                }else{
                    beta = 3.0f / 16.0f;
                }

                Debug.Log((1 - n * beta) + " " + vertices[i]);

                newVert[i] = vertices[i] * (1 - n * beta) + sum * beta;
            }else{

                newVert[i] = 1.0f/8.0f*(a + b) + 3.0f/4.0f*(vertices[i]);
            }
        }

        //Recreate triangles
        for(int i = 0; i < triangles.Count; i+=3)
        {
            int[] tri = new int[3];
            tri[0] = triangles[i];
            tri[1] = triangles[i+1];
            tri[2] = triangles[i+2];
            int[] newTri = new int[3];

            for(int cpt = 0; cpt < 3; cpt++)
            {
                int b = (cpt == 2 ? 0 : cpt+1);

                if(tri[cpt] < triangles[i + b])
                {
                    newTri[cpt] = edgeToVert[(tri[cpt], triangles[i + b])];
                }else{
                    newTri[cpt] = edgeToVert[(triangles[i + b], tri[cpt])];
                }
            }

            newTriangles.Add(tri[0]);
            newTriangles.Add(newTri[0]);
            newTriangles.Add(newTri[2]);

            newTriangles.Add(tri[1]);
            newTriangles.Add(newTri[0]);
            newTriangles.Add(newTri[1]);

            newTriangles.Add(tri[2]);
            newTriangles.Add(newTri[1]);
            newTriangles.Add(newTri[2]);

            newTriangles.Add(newTri[0]);
            newTriangles.Add(newTri[1]);
            newTriangles.Add(newTri[2]);
        }

        int[] triArray = newTriangles.ToArray();

        DoubleFaceIndices(ref triArray);

        meshFilter.mesh.SetVertices(newVert.ToArray());
        meshFilter.mesh.SetIndices(triArray, MeshTopology.Triangles, 0);

        subDivLevels[currentMesh] = true;
    }

    bool FindTriangle(out int i, ref List<int> triangles, in (int, int) edge, int current)
    {
        for(i = 0; i < triangles.Count; i+=3)
        {
            if(i == current) continue;

            if((triangles[i], triangles[i+1]) == edge || (triangles[i+1], triangles[i]) == edge)
            {
                i = triangles[i + 2];
                return true;
            }

            if((triangles[i+1], triangles[i+2]) == edge || (triangles[i+2], triangles[i+1]) == edge)
            {
                i = triangles[i];
                return true;
            }

            if((triangles[i+2], triangles[i]) == edge || (triangles[i], triangles[i+2]) == edge)
            {
                i = triangles[i + 1];
                return true;
            }
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void DoubleFaceIndices(ref int[] indices)
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

    public void DropdownValueChanged()
    {
        objects[currentMesh].SetActive(false);
        currentMesh = dropdown.value;
        objects[currentMesh].SetActive(true);
    }
    
}
