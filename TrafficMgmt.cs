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
            var roadNetwork = new ERRoadNetwork();
            roads = roadNetwork.GetRoads();

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
                    var autoMgmt = auto.GetComponent<AutoMgmt>();
                    float yPosition = UnityEngine.Random.Range(minHeight, maxHeight);
                    float xPosition = UnityEngine.Random.Range(autoMgmt.minRight, autoMgmt.maxRight);  
                    
                    autoMgmt.enabled = false;
                    autoMgmt.currentRoad = road;
                    autoMgmt.autoSpeed = random.Next(20, 40);
                    autoMgmt.rotationSpeed = random.Next(3, 4);
                    autoMgmt.hoverAuto.transform.localPosition = new Vector3(xPosition, yPosition, 0);
                    auto.transform.position = new Vector3(centerPoints[1].x, centerPoints[1].y, centerPoints[1].z);
                    autoMgmt.enabled = true;  //Insure this is set AFTER the road and other values are set.  This value is set OFF in the prefab
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
