using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyRoads3Dv3;
using System;

public class TrafficMgmt : MonoBehaviour {

    public float minHeight = 1;
    public float maxHeight = 2;
    public int autoCount = 1;
    public List<GameObject> autos = new List<GameObject>();

    private System.Random random = new System.Random(System.DateTime.Now.Millisecond);
    private ERRoad[] roads;
    private const int AUTO_MIN_DISTANCE = 400;

    void Start ()
    {
        try
        {
            int iRandom = 0;
            ERRoadNetwork roadNetwork = new ERRoadNetwork();
            roads = roadNetwork.GetRoads();
            var activeMgmt = transform.GetComponent<ActiveMgmt>();


            for (int i = 0; i < autoCount; i++)
            {
                ERRoad road = GetRoad();
                Vector3[] centerPoints = road.GetSplinePointsCenter();  

                //Somehow some roads come back with ZERO points
                if (centerPoints.Length > 0)
                {
                    iRandom = random.Next(0, autos.Count);
                    var autoPrefab = autos[iRandom];
                    var auto = Instantiate(autoPrefab);
                    AutoMgmt autoMgmt = auto.GetComponent<AutoMgmt>();
                    float xPosition = UnityEngine.Random.Range(autoMgmt.minRight, autoMgmt.maxRight);
                    float yPosition = UnityEngine.Random.Range(minHeight, maxHeight); 
                    float zPosition = auto.transform.position.z;
                    autoMgmt.currentRoad = road;
                    autoMgmt.autoSpeed = random.Next(20, 40);
                    autoMgmt.rotationSpeed = random.Next(3, 4);
                    autoMgmt.hoverAuto.transform.position = new Vector3(xPosition, yPosition, zPosition);
                    auto.transform.position = new Vector3(centerPoints[1].x, centerPoints[1].y, centerPoints[1].z);
                    
                    
                    if (auto.GetComponent<AutoMgmt>().searchLight == null)
                    {
                        //Not all drones have search lights, best to just turn off the option once
                        auto.GetComponent<AutoMgmt>().isSearchLightOn = false;
                    }
                    else
                    {
                        iRandom = random.Next(0, 10);
                        if (iRandom > 6)
                        {
                            auto.GetComponent<AutoMgmt>().isSearchLightOn = true;
                        }
                        else
                        {
                            auto.GetComponent<AutoMgmt>().isSearchLightOn = false;
                        }
                    }

                    //Optional Props
                    foreach (GameObject prop in auto.GetComponent<AutoMgmt>().props)
                    {
                        iRandom = random.Next(0, 10);
                        if (iRandom > 5)
                        {
                            prop.SetActive(true);
                        }
                    }

                    //Insure this is set AFTER the road and other values are set.  This value is set OFF in the prefab
                    autoMgmt.enabled = true;  
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log("Traffic Mgmt Start ex: " + ex.Message);
        }
    }

    private ERRoad[] ArrayRemove(ERRoad[] currentArray, int RemoveAt)
    {
        ERRoad[] newArray = new ERRoad[currentArray.Length - 1];

        int i = 0;
        int j = 0;
        while (i < currentArray.Length)
        {
            if (i != RemoveAt)
            {
                newArray[j] = currentArray[i];
                j++;
            }

            i++;
        }

        return newArray;
    }

    private ERRoad GetRoad()
    {
        ERRoad road = roads[0]; //Just to insure that SOMETHING goes back

        try
        {
            int iRandom = 0;
            ERConnection erConnection = null;
            

            while (erConnection == null)
            {
                iRandom = random.Next(0, roads.Length);
                road = roads[iRandom];
                roads = ArrayRemove(roads, iRandom);  //remove road from list, to avoid adding autos/drones to the same road

                erConnection = road.GetConnectionAtEnd();
            }
        }
        catch (Exception)
        {
            throw;
        }

        return road;
    }
}
