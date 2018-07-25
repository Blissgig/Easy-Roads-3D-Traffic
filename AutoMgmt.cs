using System.Collections.Generic;
using UnityEngine;
using EasyRoads3Dv3;
using System.Linq;
using System;

public class AutoMgmt : MonoBehaviour
{
    public float autoSpeed = 40f;
    public float rotationSpeed = 0.5f;
    public ERRoad currentRoad;
    public GameObject hoverAuto;
    public float minRight = 2.4f;
    public float maxRight = 6.8f;

    private float reachDistance = 1.0f;
    protected int CurrentWayPointID = 0;
    private List<Vector3> followPath = new List<Vector3>();
    private System.Random random = new System.Random(System.DateTime.Now.Millisecond);
    private bool startAtStart = false;


    //Searchlight
    public Light searchLight;
    public bool isSearchLightOn = false;
    //private Vector3 searchLightRotation;
    private float maxLightRotation = 90;
    //private float minLightRotation = 0;

    //Additional Props
    public List<GameObject> props;
    

    private void Start ()
    {
        if (currentRoad == null)
        {
            Debug.Log("Error: AutoMgmt is set active before a ERRoad object has been added.");
        }
        else
        {
            GetNextPath();
        }
    }

    private void Update ()
    {
        try
        {
            float distance = Vector3.Distance(followPath[CurrentWayPointID], transform.position);
            transform.position = Vector3.MoveTowards(transform.position, followPath[CurrentWayPointID], Time.deltaTime * autoSpeed);  //works, no bobbing

            var toTarget = followPath[CurrentWayPointID] - transform.position;
            if (toTarget != Vector3.zero)  //to avoid the "Look rotation viewing vector is zero" exception
            {               
                //Rotate the Base of the auto
                var rotation = Quaternion.LookRotation(toTarget); 
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
            }

            if (distance <= reachDistance)
            {
                CurrentWayPointID++;
            }

            if (CurrentWayPointID >= followPath.Count)
            {
                GetNextPath();
            }

            //Spotlight
            //Some of my assets have search lights.   Do your own thing, or just delete this.
            if (isSearchLightOn)
            {
                searchLight.transform.rotation = Quaternion.Euler(maxLightRotation * Mathf.Sin(Time.time * rotationSpeed), maxLightRotation * Mathf.Sin(Time.time * rotationSpeed), 0f); //working
            }
        }
        catch 
        {  }
    }

    private void GetNextPath()
    {
        try
        {
            ERConnection erConnection;

            if (startAtStart)
            {
                erConnection = currentRoad.GetConnectionAtStart();
            }
            else
            {
                erConnection = currentRoad.GetConnectionAtEnd();
            }

            //This happens if the road is a dead-end (and??)
            if (erConnection == null)
            {
                if (startAtStart)
                {
                    erConnection = currentRoad.GetConnectionAtEnd();
                }
                else
                {
                    erConnection = currentRoad.GetConnectionAtStart();
                }
            }

            List<ERConnectionData> erConnectionData = erConnection.GetConnectionData().ToList();

            //If there is more than one connection, delete the current connection from the list (don't want to go back the way we came)
            if (erConnectionData.Count() > 1)
            {
                for (int i = 0; i < erConnectionData.Count(); i++)
                {
                    if (erConnectionData[i].road.GetName() == currentRoad.GetName())
                    {
                        erConnectionData.RemoveAt(i);
                        break;
                    }
                }
            }

            //Get the next road
            int iNext = random.Next(0, erConnectionData.Count);
            currentRoad = erConnectionData[iNext].road;
            
            Vector3[] centerPoints = currentRoad.GetSplinePointsCenter();
            float distanceStart = Vector3.Distance(centerPoints[0], transform.position);
            float distanceEnd = Vector3.Distance(centerPoints[centerPoints.Count() - 1], transform.position);
            
            if (distanceStart > distanceEnd)
            {
                Array.Reverse(centerPoints);
                startAtStart = true;
            }
            else
            {
                startAtStart = false;
            }

            //Add a marker part way into the intersection to add as a way-point between the two roads
            //This is so the autos don't cut across the curb and/or into buildings
            if (followPath.Count > 0)
            {
                var intersectionPoint = IntersectionVector(erConnection.gameObject.transform.position, followPath[followPath.Count - 1]);
                followPath.Clear();
                followPath.Add(intersectionPoint);
            }
            else
            {
                followPath.Clear();
            }

            //Reset
            CurrentWayPointID = 0;

            //No need for ALL markers to be used, MINOR memory savings
            //For some reason the value at 0 in the ERRoad's center points is sometimes 0,0,0.  So ignoring that one.
            for (int iPath = 1; iPath < centerPoints.Count(); iPath += 4)
            {
                followPath.Add(centerPoints[iPath]);
            }

            //Cleanup, just in case
            erConnectionData.Clear();
        }
        catch (System.Exception ex)
        {
            Debug.Log("GetNextPath ex: " + ex.Message);
        }
    }
    
    //This function is to find the center of the intersection and then make a point x% before that value.
    //This improves the auto's turn radius so the autos don't drive over sidewalks
    private Vector3 IntersectionVector(Vector3 start, Vector3 end)
    {
        Vector3 result = start + (end - start) * 0.15f;

        return result;
    }
}
