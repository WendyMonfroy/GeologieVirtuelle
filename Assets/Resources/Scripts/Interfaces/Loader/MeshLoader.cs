using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;


public class MeshLoader : MonoBehaviour
{
    private FileStream _file;
    // path for the terrain file and texture
    private String _path; 
    private String _texPath; 

    // paring file variables
    private List<string> _verts;
    private List<string> _norms;
    private List<string> _tex;
    private List<string> _facets;

    // mesh creation variables
    private GameObject finalTerrain;
    private List<GameObject> Terrains;
    private List<Mesh> _meshes;
    private Texture2D _texture;
    // autonomous mesh creation with lists
    private List<Dictionary<string, HashSet<string>>> _dicoVertexUVs;
    private List<Dictionary<string, int>> _dicoIndexes;
    private List<List<Vector3>> _vertexArrayList;
    private List<List<Vector3>> _normalArrayList;
    private List<List<Vector2>> _uvArrayList;
    private List<List<int>> _triangleArrayList;

    // mesh limits
    private Vector3 _vertexCoordinates;
    private float xmin, xmax, ymin, ymax, zmin, zmax;
    private Vector3 _meshCenter;

    private int _subdivision;
    private int _subdivisionVert;

    private Vector3 meshScale1o100;
    private Vector3 meshPositionOffset;


    // Start is called before the first frame update
    void Start()
    {
        meshScale1o100 = new Vector3(0.01f, 0.01f, 0.01f);
        meshPositionOffset = new Vector3(1f, 0.9f, 0f);
        // custom event listener
        EventManager.StartListening("LoadChosenMesh", CheckAndLoad);
        EventManager.StartListening("SendSignalToStartSimulation", SendMesh);
        EventManager.StartListening("LoadMeshAndMeasures", LoadMeshAndMeasures);
        EventManager.StartListening("DestroyReaminingMeshes", DestroyReaminingMeshes);

        // getting terrain back when ending simulation
        EventManager.StartListening("TerrainFromSimulationToLobby", GetTerrainBack);
    }

    // store a vertex in the proper mesh
    void StoreInMesh( Dictionary<string, HashSet<string>> dicoVertexUV,  Dictionary<string, int> dicoIndex,  List<Vector3> meshVertices,  List<Vector2> meshUVs,  List<int> meshTriangles, List<Vector3> facetVertices, List<Vector2> facetUVs)
    {
        string key;
        string val;
        string combinedKey;
        int[] index = new int[3];

        // process facet
        for (int i = 0; i < 3; i++)
        {
            key = facetVertices[i].x.ToString() + ' ' + facetVertices[i].y.ToString() + ' ' + facetVertices[i].z.ToString();
            val = facetUVs[i].x.ToString() + ' ' + facetUVs[i].y.ToString();
            combinedKey = key + ' ' + val;

            if (!dicoVertexUV.ContainsKey(key))
            {
                // this vertex is not processed yet
                dicoVertexUV.Add(key, new HashSet<string>());
            }
            if (!dicoVertexUV[key].Contains(val))
            {
                dicoVertexUV[key].Add(val);
                dicoIndex.Add(combinedKey, meshVertices.Count);

                // add vertex and its UV to the mesh lists
                meshVertices.Add(new Vector3(-facetVertices[i].x, facetVertices[i].y, facetVertices[i].z));
                meshUVs.Add(new Vector2(facetUVs[i].x, facetUVs[i].y));
                
            }
            index[i] = dicoIndex[combinedKey];
        }

        // store vertices in the mesh facet list
        // facet vertices order changed to conpensate the -x operation
        meshTriangles.Add(index[2]);
        meshTriangles.Add(index[1]);
        meshTriangles.Add(index[0]);
    }

    void RenderMeshes(int meshnumber,Transform parentObjectTransform, ref List<GameObject> gameObjects, ref List<Mesh> _meshes, ref List<List<Vector3>> _vertices, ref List<List<Vector2>> _uvs, ref List<List<int>> _triangles, Texture2D texture)
    {
        for (int j = 0; j < meshnumber; j++)
        {
            // initialize terrain game object
            gameObjects.Add(new GameObject());
            gameObjects[j].AddComponent<MeshFilter>();
            gameObjects[j].AddComponent<MeshRenderer>();

            // set parent and transform
            gameObjects[j].transform.SetParent(parentObjectTransform);
            gameObjects[j].transform.localScale = meshScale1o100;
            gameObjects[j].transform.Translate(meshPositionOffset);
            gameObjects[j].transform.tag = "Terrain";

            // set mesh elements
            _meshes.Add(gameObjects[j].GetComponent<MeshFilter>().mesh);
            _meshes[j].SetVertices(_vertices[j]);
            _meshes[j].SetTriangles(_triangles[j], 0, true, 0);
            _meshes[j].SetUVs(0, _uvs[j]);

            gameObjects[j].GetComponent<MeshFilter>().mesh = _meshes[j];
            gameObjects[j].GetComponent<MeshFilter>().mesh.RecalculateNormals();
            gameObjects[j].GetComponent<MeshRenderer>().material.mainTexture = texture;
            gameObjects[j].GetComponent<MeshRenderer>().material.shader = UnityEngine.Shader.Find("Unlit/Texture");
            gameObjects[j].AddComponent<MeshCollider>();
            gameObjects[j].AddComponent<Rigidbody>();
            gameObjects[j].GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    // main method
    public void LoadMesh(EventParam pathParam)
    {
        // file and texture files path to load
        string[] pathSplited = pathParam.getStringParam().Trim().Split('.'); // keep the file path apart from its extention
        _path = pathSplited[0] + ".obj"; // fetch the .obj file
        _texPath = pathSplited[0] + ".jpg"; // fetch the corresponding .jpg file

        // text lists for file parsing
        _verts = new List<string>();
        _norms = new List<string>();
        _tex = new List<string>();
        _facets = new List<string>();

        xmin = 0;
        xmax = 0;
        ymin = 0;
        ymax = 0;
        zmin = 0;
        zmax = 0;

        // number of subdivisions
        _subdivision = 20;
        _subdivisionVert = 4;
        int meshnumber = _subdivision * _subdivision * _subdivisionVert;


        // lists initialisation
        _dicoVertexUVs = new List<Dictionary<string, HashSet<string>>>();
        _dicoIndexes = new List<Dictionary<string, int>>();
        _vertexArrayList = new List<List<Vector3>>();
        _uvArrayList = new List<List<Vector2>>();
        _triangleArrayList = new List<List<int>>();
        for (int i = 0; i < meshnumber; i++)
        {
            _dicoVertexUVs.Add(new Dictionary<string, HashSet<string>>());
            _dicoIndexes.Add(new Dictionary<string, int>());
            _vertexArrayList.Add(new List<Vector3>());
            _uvArrayList.Add(new List<Vector2>());
            _triangleArrayList.Add(new List<int>());
        }


        // file parsing
        if (!File.Exists(_path))
        {
            Debug.Log("fichier innexistant");
        }
        else
        {
            _file = File.OpenRead(_path);

            // file reading
            foreach (string _line in File.ReadLines(_path))
            {
                string[] text = _line.Trim().Split(' ');

                // put line in the right list
                switch (text[0])
                {
                    case "v":
                        // bounds calculation
                        _vertexCoordinates = new Vector3(float.Parse(text[1], CultureInfo.InvariantCulture), float.Parse(text[2], CultureInfo.InvariantCulture), float.Parse(text[3], CultureInfo.InvariantCulture));
                        if (_vertexCoordinates.x < xmin)
                        {
                            xmin = _vertexCoordinates.x;
                        }
                        else if (_vertexCoordinates.x > xmax)
                        {
                            xmax = _vertexCoordinates.x;
                        }
                        if (_vertexCoordinates.y < ymin)
                        {
                            ymin = _vertexCoordinates.y;
                        }
                        else if (_vertexCoordinates.y > ymax)
                        {
                            ymax = _vertexCoordinates.y;
                        }
                        if (_vertexCoordinates.z < zmin)
                        {
                            zmin = _vertexCoordinates.z;
                        }
                        else if (_vertexCoordinates.z > zmax)
                        {
                            zmax = _vertexCoordinates.z;
                        }

                        _verts.Add(text[1] + ' ' + text[2] + ' ' + text[3]);
                        break;

                    case "vt":
                        _tex.Add(text[1] + ' ' + text[2]);
                        break;

                    case "vn":
                        _norms.Add(text[1] + ' ' + text[2] + ' ' + text[3]);
                        break;

                    case "f":
                        _facets.Add(text[1] + ' ' + text[2] + ' ' + text[3]);
                        break;

                    default:
                        break;
                }
            }

            // calculate mesh center
            _meshCenter = new Vector3((xmin + xmax) / 2, (ymin + ymax) / 2, (zmin + zmax) / 2);

            // facet processing
            string[] _fLine;
            int _facetIndex;
            for (_facetIndex = 0; _facetIndex < _facets.Count; _facetIndex++)
            {
                _fLine = _facets[_facetIndex].Trim().Split(' '); // line format: "v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3"

                string[] s1 = _fLine[0].Trim().Split('/');
                string[] s2 = _fLine[1].Trim().Split('/');
                string[] s3 = _fLine[2].Trim().Split('/');

                // store the indexes of facet's vertices
                // -1 to all indexes because .obj format starts index from 1 instead of 0
                List<Vector3Int> f = new List<Vector3Int>();
                f.Add(new Vector3Int(Int32.Parse(s1[0]) - 1, Int32.Parse(s1[1]) - 1, Int32.Parse(s1[2]) - 1)); // 1st vertex [v, vt, vn]
                f.Add(new Vector3Int(Int32.Parse(s2[0]) - 1, Int32.Parse(s2[1]) - 1, Int32.Parse(s2[2]) - 1)); // 2nd vertex [v, vt, vn]
                f.Add(new Vector3Int(Int32.Parse(s3[0]) - 1, Int32.Parse(s3[1]) - 1, Int32.Parse(s3[2]) - 1)); // 3rd vertex [v, vt, vn]

                // variables pour parser 
                string[] _res;
                List<Vector3> coordSommets = new List<Vector3>();
                List<Vector2> coordTexture = new List<Vector2>();

                _res = _verts[f[0].x].Trim().Split(' ');
                coordSommets.Add(new Vector3(float.Parse(_res[0], CultureInfo.InvariantCulture), float.Parse(_res[1], CultureInfo.InvariantCulture), float.Parse(_res[2], CultureInfo.InvariantCulture)));
                _res = _verts[f[1].x].Trim().Split(' ');
                coordSommets.Add(new Vector3(float.Parse(_res[0], CultureInfo.InvariantCulture), float.Parse(_res[1], CultureInfo.InvariantCulture), float.Parse(_res[2], CultureInfo.InvariantCulture)));
                _res = _verts[f[2].x].Trim().Split(' ');
                coordSommets.Add(new Vector3(float.Parse(_res[0], CultureInfo.InvariantCulture), float.Parse(_res[1], CultureInfo.InvariantCulture), float.Parse(_res[2], CultureInfo.InvariantCulture)));

                // calculate facet center to set it to the proper submesh
                _vertexCoordinates = (coordSommets[0] + coordSommets[1] + coordSommets[2]) / 3;

                _res = _tex[f[0].y].Trim().Split(' ');
                coordTexture.Add(new Vector2(float.Parse(_res[0], CultureInfo.InvariantCulture), float.Parse(_res[1], CultureInfo.InvariantCulture)));
                _res = _tex[f[1].y].Trim().Split(' ');
                coordTexture.Add(new Vector2(float.Parse(_res[0], CultureInfo.InvariantCulture), float.Parse(_res[1], CultureInfo.InvariantCulture)));
                _res = _tex[f[2].y].Trim().Split(' ');
                coordTexture.Add(new Vector2(float.Parse(_res[0], CultureInfo.InvariantCulture), float.Parse(_res[1], CultureInfo.InvariantCulture)));

                int facetSubMeshIndex = 0;

                // set submesh index according to its x position
                if (_vertexCoordinates.x < (xmin + xmax) / 20)
                {
                    facetSubMeshIndex += 0;
                }
                else if ((_vertexCoordinates.x >= (xmin + xmax) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 2) / 20))
                {
                    facetSubMeshIndex += 1;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 2) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 3) / 20))
                {
                    facetSubMeshIndex += 2;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 3) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 4) / 20))
                {
                    facetSubMeshIndex += 3;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 4) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 5) / 20))
                {
                    facetSubMeshIndex += 4;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 5) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 6) / 20))
                {
                    facetSubMeshIndex += 5;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 6) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 7) / 20))
                {
                    facetSubMeshIndex += 6;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 7) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 8) / 20))
                {
                    facetSubMeshIndex += 7;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 8) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 9) / 20))
                {
                    facetSubMeshIndex += 8;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 9) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 10) / 20))
                {
                    facetSubMeshIndex += 9;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 10) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 11) / 20))
                {
                    facetSubMeshIndex += 10;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 11) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 12) / 20))
                {
                    facetSubMeshIndex += 11;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 12) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 13) / 20))
                {
                    facetSubMeshIndex += 12;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 13) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 14) / 20))
                {
                    facetSubMeshIndex += 13;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 14) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 15) / 20))
                {
                    facetSubMeshIndex += 14;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 15) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 16) / 20))
                {
                    facetSubMeshIndex += 15;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 16) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 17) / 20))
                {
                    facetSubMeshIndex += 16;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 17) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 18) / 20))
                {
                    facetSubMeshIndex += 17;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 18) / 20) && (_vertexCoordinates.x < ((xmin + xmax) * 19) / 20))
                {
                    facetSubMeshIndex += 18;
                }
                else if ((_vertexCoordinates.x >= ((xmin + xmax) * 19) / 20))
                {
                    facetSubMeshIndex += 19;
                }

                // set submesh index according to its z position
                if (_vertexCoordinates.z < (zmin + zmax) / 20)
                {
                    facetSubMeshIndex += 0;
                }
                else if ((_vertexCoordinates.z >= (zmin + zmax) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 2) / 20))
                {
                    facetSubMeshIndex += 1 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 2) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 3) / 20))
                {
                    facetSubMeshIndex += 2 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 3) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 4) / 20))
                {
                    facetSubMeshIndex += 3 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 4) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 5) / 20))
                {
                    facetSubMeshIndex += 4 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 5) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 6) / 20))
                {
                    facetSubMeshIndex += 5 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 6) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 7) / 20))
                {
                    facetSubMeshIndex += 6 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 7) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 8) / 20))
                {
                    facetSubMeshIndex += 7 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 8) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 9) / 20))
                {
                    facetSubMeshIndex += 8 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 9) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 10) / 20))
                {
                    facetSubMeshIndex += 9 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 10) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 11) / 20))
                {
                    facetSubMeshIndex += 10 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 11) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 12) / 20))
                {
                    facetSubMeshIndex += 11 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 12) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 13) / 20))
                {
                    facetSubMeshIndex += 12 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 13) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 14) / 20))
                {
                    facetSubMeshIndex += 13 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 14) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 15) / 20))
                {
                    facetSubMeshIndex += 14 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 15) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 16) / 20))
                {
                    facetSubMeshIndex += 15 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 16) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 17) / 20))
                {
                    facetSubMeshIndex += 16 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 17) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 18) / 20))
                {
                    facetSubMeshIndex += 17 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 18) / 20) && (_vertexCoordinates.z < ((zmin + zmax) * 19) / 20))
                {
                    facetSubMeshIndex += 18 * _subdivision;
                }
                else if ((_vertexCoordinates.z >= ((zmin + zmax) * 19) / 20))
                {
                    facetSubMeshIndex += 19 * _subdivision;
                }

                // set submesh index according to its y position (vertical)
                if (_vertexCoordinates.y < (ymin + ymax) / 4)
                {
                    facetSubMeshIndex += 0 * _subdivision * _subdivision;
                }
                else if ((_vertexCoordinates.y >= (ymin + ymax) / 4) && (_vertexCoordinates.y < ((ymin + ymax) * 2) / 4))
                {
                    facetSubMeshIndex += 1 * _subdivision * _subdivision;
                }
                else if ((_vertexCoordinates.y >= ((ymin + ymax) * 2) / 4) && (_vertexCoordinates.y < ((ymin + ymax) * 3) / 4))
                {
                    facetSubMeshIndex += 2 * _subdivision * _subdivision;
                }
                /*
                else if ((_vertexCoordinates.y >= ((ymin + ymax) * 3) / 10) && (_vertexCoordinates.y < ((ymin + ymax) * 4) / 10))
                {
                    facetSubMeshIndex += 3 * _subdivision * _subdivision;
                }
                else if ((_vertexCoordinates.y >= ((ymin + ymax) * 4) / 10) && (_vertexCoordinates.y < ((ymin + ymax) * 5) / 10))
                {
                    facetSubMeshIndex += 4 * _subdivision * _subdivision;
                }
                else if ((_vertexCoordinates.y >= ((ymin + ymax) * 5) / 10) && (_vertexCoordinates.y < ((ymin + ymax) * 6) / 10))
                {
                    facetSubMeshIndex += 5 * _subdivision * _subdivision;
                }
                else if ((_vertexCoordinates.y >= ((ymin + ymax) * 6) / 10) && (_vertexCoordinates.y < ((ymin + ymax) * 7) / 10))
                {
                    facetSubMeshIndex += 6 * _subdivision * _subdivision;
                }
                else if ((_vertexCoordinates.y >= ((ymin + ymax) * 7) / 10) && (_vertexCoordinates.y < ((ymin + ymax) * 8) / 10))
                {
                    facetSubMeshIndex += 7 * _subdivision * _subdivision;
                }
                else if ((_vertexCoordinates.y >= ((ymin + ymax) * 8) / 10) && (_vertexCoordinates.y < ((ymin + ymax) * 9) / 10))
                {
                    facetSubMeshIndex += 8 * _subdivision * _subdivision;
                }*/
                else if ((_vertexCoordinates.y >= ((ymin + ymax) * 3) / 4))
                {
                    facetSubMeshIndex += 3 * _subdivision * _subdivision;
                }

                // use quadtree structure to improve ?

                // fill the proper submesh 
                StoreInMesh(_dicoVertexUVs[facetSubMeshIndex], _dicoIndexes[facetSubMeshIndex], _vertexArrayList[facetSubMeshIndex], _uvArrayList[facetSubMeshIndex], _triangleArrayList[facetSubMeshIndex], coordSommets, coordTexture);
            }
            // end of facet processing

            // submeshes creation

            // load texture
            byte[] _imgBytes = File.ReadAllBytes(_texPath);
            _texture = new Texture2D(2, 2);
            _texture.LoadImage(_imgBytes);

            // GameObjects creation an rendering
            finalTerrain = new GameObject();
            finalTerrain.transform.SetParent(gameObject.transform);
            finalTerrain.name = new FileInfo(_path).Name.Trim().Split('.')[0];

            Terrains = new List<GameObject>();
            _meshes = new List<Mesh>();
            RenderMeshes(meshnumber,finalTerrain.transform, ref Terrains, ref _meshes, ref _vertexArrayList, ref _uvArrayList, ref _triangleArrayList, _texture);
            // reset hint message to void when the terrain is fully loaded
            EventManager.TriggerEvent("LobbyHintMessage", new EventParam("Vous pouvez sélectionner un point de départ sur le terrain à votre droite."));
            // trigger event to enable terrain buttons when loading is over
        }
    }

    // check if the terrain given as param is already loaded or not
    void CheckAndLoad(EventParam param)
    {
        string fileName = new FileInfo(param.getStringParam()).Name.Trim().Split('.')[0];
        int match = -1;

        // reset the hint message
        EventManager.TriggerEvent("LobbyHintMessage", new EventParam(""));

        for (int i=0; i<gameObject.transform.childCount; i++)
        {
            if (fileName == gameObject.transform.GetChild(i).name)
                match = i;
        }

        if (match != -1)
        {
            // the terrain has not been loaded yet
            finalTerrain.SetActive(false);
            finalTerrain = gameObject.transform.GetChild(match).gameObject;
            finalTerrain.SetActive(true);
        }
        else
        {
            if (finalTerrain != null)
                finalTerrain.SetActive(false);
            
            // set hint message to display the loading status
            EventManager.TriggerEvent("LobbyHintMessage", new EventParam("Le terrain choisi est en cours de chargement"));
            // trigger event to disable other terrain buttons while loading
            LoadMesh(param); // startCoroutine
        }
        
    }

    void SendMesh(EventParam param)
    {
        if (finalTerrain != null)
        {
            // start simulation with the last selected terrain
            EventManager.TriggerEvent("StartSimulation", new EventParam(finalTerrain));
        }
        else
        {
            // no terrain loaded: send an error message to display
            EventManager.TriggerEvent("LobbyErrorMessage", new EventParam("Veuillez sélectionner un terrain avant de lancer la simulation"));
        }
    }

    void LoadMeshAndMeasures(EventParam param)
    {
        Debug.Log("on est là");
        if (finalTerrain != null)
        {
            SendMesh(null);
            EventManager.TriggerEvent("RestoreMeasures", new EventParam(finalTerrain.name));
        }
        else
        {
            // no terrain loaded: send an error message to display
            EventManager.TriggerEvent("LobbyErrorMessage", new EventParam("Veuillez sélectionner un terrain avant de charger des mesures"));
        }
    }

    void DestroyReaminingMeshes(EventParam nullParam)
    {
        // this method is called when strating the simulation to free memory space
        int childCount = gameObject.transform.childCount;
        for (int i=0; i<childCount; i++)
        {
            Destroy(gameObject.transform.GetChild(0).gameObject);
        }
    }

    void GetTerrainBack(EventParam gameObjectParam)
    {
        // set terrain parent to MeshLoader game object
        gameObjectParam.getGameObjectParam().transform.SetParent(gameObject.transform);
        gameObjectParam.getGameObjectParam().transform.localScale = Vector3.one;

        // set scale for each submesh
        int terrainCount = gameObjectParam.getGameObjectParam().transform.childCount;
        for (int i = 0; i<terrainCount; i++)
        {
            gameObjectParam.getGameObjectParam().transform.GetChild(i).localScale = meshScale1o100;
            gameObjectParam.getGameObjectParam().transform.GetChild(i).position = meshPositionOffset;
            gameObjectParam.getGameObjectParam().transform.GetChild(i).tag = "Terrain"; // enable spawn point selection
        }
    }
}
