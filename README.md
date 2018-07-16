# Easy-Roads-3D-Traffic
Simple automated traffic using Easy Roads 3D as the path information

I have been looking at multiple traffic ai assets and since my needs were simple, as well as having issues having other assets match the Easy Roads road path.

Easy Roads 3D has a series of Vector3 points, these points are in the center of the road (these are the ones I use) as well as along the right and left hand side of the road.

There are two scripts: 
* TrafficMgmt.cs - This adds traffic to the game/experience.  This script is attached, in my case to the camera, and is run only at Startup.
* AutoMgmt.cs    - This script is attached to each automobile and it sets the current path.   At each intersection a new road is selected.
