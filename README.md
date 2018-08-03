# AstarPathFinding_ReciprocalVelocityObstacle_Unity
### Multiple agents path finding using A* algorithm, combined with RVO for local collision avoidance.

### Update on 07/31/2018
Action items completed
1. Unit drop and rally at clicked on location is done, with local crowd pushaway implemented at instantiation.
2. Potential field map approach is implemented and used depending on the number of units and the length of path finding.
3. Multiple ways are used to deal with potential map's local optimal issue.

### Update after communication on 07/15/2018
Action items in the following:
1. Add UI for unit drop and rally point select
2. Size depedent grid/mesh in A-star graph or node
3. Implementation of the vector field/potential method; provide the option of using either A-star or Potential field depending on the relative size of group and map
4. How to use ECS (Entity-Component System)

# Documentation on the framework of path finding project

## Outline
1. Setup and preparation
2. Path finding based on A* algorithm implemented with reciprocal velocity obstacle (RVO) 
3. Path finding based on the potential field method

## 1. Setup and preparation

A* PathFinding Project (https://arongranberg.com/astar/) is used for the A* approach, and RVO (http://gamma.cs.unc.edu/RVO/) is used for local collision avoidance during motion. 
This path finding project is implemented based on a terrain world, where non-walkable area has height = 50 while walkable area has height = 0. Once a terrain is drawn, a mesh of square grid is baked using the A* package.
GameObject 'A*' containes the 'Astar Path' script dealing with the setting of grid. In addition to the A* algorithm, a potential field approach is also implemented (see script 'PotentialMapSet' or Part-3 of this README for further detail).

A canvas with three buttons are added for simple user interface. Basically it allows to add units on click when the game starts, and it also makes it possible to activate rally once the units addition is done. Agents are added according to script 'CreateAgent' attached to GameObject 'AI', of which all kinds of agents are children. The transition between adding units and rally is controlled by boolean variable 'SpawnIsDone' in script 'UI_ButtonControl' of the GameObject _manager.
   
Agents are defined by prefabs, currently either "RVOAgent" or "AirAgent", each having a script attached. In the stage of deploying or spawning of agents, it is needed that each unit will push away other units closerby when the local point of mouse click gets crowded. This is done in the function "CheckPushAway()" in RVO_Agent. Basically the CheckPushAway function check the distance between each agents and push one self away from any agent close by along the opposite of the agent to to agent direcetion, as long as the agent is not entering non-walkable regime. The effect of pushing away each other can be clearly seen at the instantiation. 


## 2. Path finding based on A* algorithm implemented with reciprocal velocity obstacle (RVO) 

A* path finding is done following the documentation and examples from the package website. For each (ground) agent, a path from current position to the clicked point, which is represented by a list of grid nodes based on A* heuristic function, is caculated whenever a mouse click occurs. The destination point is obtained from shedding a ray of light from camera to the mousePosition and returning a hit point on the terrain world.
And the 'RVO simulator' takes each agent together with its path as an input added to the list of agents and paths. In each time frame, the Simulator will run local collision avoidance Simulator.Instance.doStep() after each agent's position and preferred velocity are updated based on A* paths as is written in FixedUpdate() of RVO_Simulator. The idea of RVO is that agents will form velocity obstacle cones in the velocity space based on the relative velocities with other agents, and a new velocity resulted from the current preferred velocity and a velocity outside the RVOs will be given to each agent in order to prevent collisions.

## 3. Path finding based on the potential field method

A* path finding works great when the total number of agents is small and the path is not too long. But it will become time consuming if the number of agent increases or the path length is long, since the complexity is exponential ~ O((number of next states)^(pathlength)). Therefore a potential based search is implemented and used when the path is expected to be long and/or there are a large number of units. The decision of using A star search or potential field is determined in the RVO_Agent acript by comparing the log of grid node size with the log of expected A* time complexity `log (N * 3^(PathLengthEstimated)`, in which N is the number of agents, 3 is available next step state numbers, PathNumberEstimated is estimation of path length based on the distance to target point and grid size:
`if(Mathf.Log(simulator.agentPositions.Count) + 3 * Mathf.Log(pathLengthEstimated) > Mathf.Log(AstarPath.active.data.gridGraph.Depth * AstarPath.active.data.gridGraph.Width))`

The potential field search is implemented in script `PotentialFieldSet`. Functions related to potential field method are called in the RVOAgent script.The major steps and work flow are explained as following:

1. The first step is generate potential map, done by function `GetPotentialMap(target, gridgraph)`. `target` is obtained from mousePosition, and gridgraph is obtained from the A* grid.
In `GeneratePotentialMap`, the first critical thing to do is to find the grid node that contains the target position. `gridgraph.GetNearest()` method is used to get target node index. This node is conisdered the "source" of potential in the current map, and thus the potential value of this node is set to zero. Beginning from this source point, the potential map is calculated using standard breadth first search. Each node will expand to the neighboring nodes (except the one already visited) and potential value of new nodes will be previous potential value + 1. Note that although 4 directions will be checked for each node expansion, only newly visited nodes will change the potential value. A hash set `nodeIndicesSearched` stores all the visited nodes indices. In the end, all walkable nodes will be traversed, and the non-walkable nodes will remain at the initial value `largePenalty`, which is considerably larger than normal potential values.

2.



