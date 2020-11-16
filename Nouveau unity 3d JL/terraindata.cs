using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    private bool isSelected;
    private bool done = false;
    //private int previousw, previousl;
    private GameObject[] murs;
    // Start is called before the first frame update
    void Start()
    {
        
    }
        // Update is called once per frame
    void Update()
    {
        
        while (!terrainwebsocket.isConnect) return;
        while (!terrainwebsocket.receivedmapinfo)
            {
            done = false;
            return;
            
            }
        if(!done)
            {
            done = true;
            width = terrainwebsocket.height;
            lenght = terrainwebsocket.width;


            GenerateTerrain();
            Debug.LogWarning("terminé");
            }

        clearcase();

        moveBot();

        addEnergiy();
        /*previousl = lenght;
        previousw = width;
        //width = gestionui.largeur;
        //lenght = gestionui.longeur;
        width = 30;
        lenght = 24;
        murs = GameObject.FindGameObjectsWithTag("Mur");
        if (!isSelected)
        {
            if(previousl!=lenght || previousw!= width)
            {
                foreach(GameObject mur in murs)
                {
                    DestroyImmediate(mur);

                }
                isempty = new bool[lenght * width];
                mapinfo = new int[lenght * width];
                typeobjs = new GameObject[lenght * width];
                centreterrain.transform.position = new Vector3(lenght / 2 - 0.5f, 1, width / 2 - 0.5f);

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

            isSelected = gestionui.isPressed;

        }

        //GameObject newmur = Instantiate(mur, new Vector3(10,0,10), Quaternion.identity) as GameObject;
        //Debug.Log("nouveauterrain1");
        */
        updateTerrain();
        }
        void GenerateTerrain()
        {
            for (int x = 0; x < lenght; x++)
            {
                for (int z = 0; z < width; z++)
                {

                    // Vector3 positionmur = new Vector3(x, 0, z);
                    Vector3 positionterrain = new Vector3(x, 0.5f, z);
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
                        Vector3 positionmur2 = new Vector3(x, 2, z);
                        //Vector3 positionterrain = new Vector3(x - 10, 0, z - 10);
                        //Vector3 rot = new Vector3(0, 0, 0);
                        GameObject newmur = Instantiate(mur, positionmur, Quaternion.identity) as GameObject;
                        GameObject newmur2 = Instantiate(mur, positionmur2, Quaternion.identity) as GameObject;
                        //GameObject newterrain = Instantiate(terrain, positionterrain, Quaternion.identity) as GameObject;
                    }
                }
            }
        isempty = new bool[lenght * width];
        mapinfo = new int[lenght * width];
        typeobjs = new GameObject[lenght * width];
        centreterrain.transform.position = new Vector3(lenght / 2 - 0.5f, 1, width / 2 - 0.5f);

        Debug.Log("nouveauterrain");



    }
        void updateTerrain()
        {

            for (int i = 0; i <= mapinfo.Length; i++)
            {
                if (mapinfo[i] == 0 || mapinfo[i] > 4)
                {
                    isempty[i] = true;
                    if (typeobjs[i] != null)
                    {
                        DestroyImmediate(typeobjs[i]);
                    }
                }
                if (mapinfo[i] == 4 && isempty[i])
                {
                /*float posobjx = i / width;
                posobjx = Mathf.Floor(posobjx);
                int posobjz = (i % width);

                Vector3 positionobj = new Vector3(posobjx, 1, posobjz);*/
                Vector3 positionobj = new Vector3(terrainwebsocket.bx2, 1, terrainwebsocket.by2);
                GameObject bot = Instantiate(robot, positionobj, Quaternion.identity) as GameObject;
                typeobjs[i] = bot;
                isempty[i] = false;

                
                


                }
                if (mapinfo[i] == 2 && isempty[i])
                {

                    float posobjx = i / width;
                    posobjx = Mathf.Floor(posobjx);

                    int posobjz = (i % width);

                    Vector3 positionobj = new Vector3(posobjx, 0.5f, posobjz);

                    GameObject newmur = Instantiate(mur, positionobj, Quaternion.identity) as GameObject;
                    typeobjs[i] = newmur;
                    isempty[i] = false;
                }
                if (mapinfo[i] == 3 && isempty[i])
                {

                /* float posobjx = i / width;
                 posobjx = Mathf.Floor(posobjx);
                 int posobjz = (i % width);
                 Debug.Log(posobjx);
                 Debug.Log(posobjz);
                 Vector3 positionobj = new Vector3(posobjx, 0.5f, posobjz);

                 GameObject ene = Instantiate(energie, positionobj, Quaternion.identity) as GameObject;
                 typeobjs[i] = ene;
                 isempty[i] = false;*/
                Vector3 positionobj = new Vector3(terrainwebsocket.ex1, 0.5f, terrainwebsocket.ey1);
                GameObject ene = Instantiate(energie, positionobj, Quaternion.identity) as GameObject;
                

                
                isempty[i] = false;
                typeobjs[i] = ene;
            }


            }
        }

    void clearcase()
    {
        int i = (terrainwebsocket.cx1 * width) + terrainwebsocket.cy1;
        mapinfo[i] = 0;
        DestroyImmediate(typeobjs[i]);
        typeobjs[i] = null;
        isempty[i] = true;
    }
    void moveBot()
    {
        //Vector3 positionobj = new Vector3(terrainwebsocket.bx2, 1, terrainwebsocket.by2);
        //GameObject bot = Instantiate(robot, positionobj, Quaternion.identity) as GameObject;
        int i = (terrainwebsocket.by2 * width) + terrainwebsocket.bx2;
        int j = (terrainwebsocket.by1 * width) + terrainwebsocket.bx1;
        
        mapinfo[i] = 4;
        mapinfo[j] = 0;
        //isempty[i] = false;
       // typeobjs[i] = bot;

    }
    void addEnergiy()
    {
        
       
        int i = (terrainwebsocket.ey1 * width) + terrainwebsocket.ex1;

        mapinfo[i] = 3;
        
    }
}
