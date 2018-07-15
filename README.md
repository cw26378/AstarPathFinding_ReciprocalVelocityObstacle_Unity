# AstarPathFinding_ReciprocalVelocityObstacle_Unity

## Implementation Summary
	
	This is an implementation of go to clicked position in a Unity Terrain.

	The path finding is done using Astar PathFinding Project (https://arongranberg.com/astar/).
	
	The local avoidance is done based on the RVO2 library from the UNC gamma group (http://gamma.cs.unc.edu/RVO2/). 
	
	Only land units use collision avoidance. Airbourne units can just move to the target point.

