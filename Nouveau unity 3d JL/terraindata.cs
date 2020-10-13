using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
//using battleia;

public class terraindata : MonoBehaviour
{
    public GameObject mur;
    public GameObject terrain;
    public GameObject robot;
    public GameObject energie;
    public GameObject centreterrain;
    public int lenght = 10;
    public int width = 10;
    public int[] mapinfo;
    private bool[] isempty;
    private GameObject[] typeobjs;
    // Start is called before the first frame update
    void Start()
    {
        isempty = new bool[lenght*width];
        mapinfo = new int[lenght*width];
        typeobjs = new GameObject[lenght*width];
        centreterrain.transform.position = new Vector3(lenght / 2, 0, width / 3);

        Debug.Log("nouveauterrain");
        GenerateTerrain();
        for (int i = 0; i > lenght * width; i++)
        {
            float j = UnityEngine.Random.Range(0, 6);
            if (j == 5)
            {
                Debug.Log("new obj");
                mapinfo[i] = UnityEngine.Random.Range(1, 5);
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        //GameObject newmur = Instantiate(mur, new Vector3(10,0,10), Quaternion.identity) as GameObject;
        //Debug.Log("nouveauterrain1");
        updateTerrain();
    }
    void GenerateTerrain()
    {
        for (int x = 0; x < lenght; x++)
        {
            for (int z = 0; z < width; z++)
            {

                // Vector3 positionmur = new Vector3(x, 0, z);
                Vector3 positionterrain = new Vector3(x, 0, z);
                //Vector3 rot = new Vector3(0, 0, 0);
                //GameObject newmur = Instantiate(mur, positionmur, Quaternion.identity) as GameObject;
                GameObject newterrain = Instantiate(terrain, positionterrain, Quaternion.identity) as GameObject;
            }
        }
        for (int x = -1; x < lenght + 1; x++)
        {
            for (int z = -1; z < width + 1; z++)
            {
                if (x == -1 || x == lenght || z == -1 || z == width)
                {


                    Vector3 positionmur = new Vector3(x, 1, z);
                    //Vector3 positionterrain = new Vector3(x - 10, 0, z - 10);
                    //Vector3 rot = new Vector3(0, 0, 0);
                    GameObject newmur = Instantiate(mur, positionmur, Quaternion.identity) as GameObject;
                    //GameObject newterrain = Instantiate(terrain, positionterrain, Quaternion.identity) as GameObject;
                }
            }
        }




    }
    void updateTerrain()
    {
        
        for (int i = 0; i <= mapinfo.Length; i++)
        {
            if (mapinfo[i] == 0 || mapinfo[i]>4)
            {
                isempty[i] = false;
                if(typeobjs[i] != null)
                {
                    DestroyImmediate(typeobjs[i]);
                }
            }
            if (mapinfo[i] == 4&& !isempty[i])
            {
                float posobjx = i / lenght;
                posobjx = Mathf.Floor(posobjx);
                int posobjz = (i % width);

                Vector3 positionobj = new Vector3(posobjx, 1, posobjz);

                GameObject bot = Instantiate(robot, positionobj, Quaternion.identity) as GameObject;
                typeobjs[i] = bot;
                isempty[i] = true;

            }
            if (mapinfo[i] == 2 && !isempty[i])
            {
                float posobjx = i / lenght;
                posobjx = Mathf.Floor(posobjx);
                int posobjz = (i % width);

                Vector3 positionobj = new Vector3(posobjx, 1, posobjz);

                GameObject newmur = Instantiate(mur, positionobj, Quaternion.identity) as GameObject;
                typeobjs[i] = newmur;
                isempty[i] = true;
            }
            if (mapinfo[i] == 3 && !isempty[i])
            {

                float posobjx = i / lenght;
                posobjx = Mathf.Floor(posobjx);
                int posobjz = (i % width);
                Debug.Log(posobjx);
                Debug.Log(posobjz);
                Vector3 positionobj = new Vector3(posobjx, 1, posobjz);

                GameObject ene = Instantiate(energie, positionobj, Quaternion.identity) as GameObject;
                typeobjs[i] = ene;
                isempty[i] = true;
            }
            

        }
    }
}