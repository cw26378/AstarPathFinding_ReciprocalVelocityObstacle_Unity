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
3. Implementation of the vector field/potential method; provide the option of using either A-star or Potential field depending on the 

relative size of group and map
4. How to use ECS (Entity-Component System)

### Update on 08/03/2018

add a documentation for illustraing the framework the project.
# Documentation on the framework of path finding project

## Outline
1. Setup and preparation
2. Path finding based on A* algorithm implemented with reciprocal velocity obstacle (RVO) 
3. Path finding based on the potential field method

## 1. Setup and preparation

A* PathFinding Project (https://arongranberg.com/astar/) is used for the A* approach, and RVO (http://gamma.cs.unc.edu/RVO/) is used for local collision avoidance during motion. This path finding project is implemented based on a terrain world, where non-walkable area has height = 50 while walkable area has height = 0. 
Once a UNITY terrain is drawn, a mesh of square grid is baked using the A* package. `GameObject A*` contains the `Astar Path` script dealing with the setting of grid. In addition to the A* algorithm, a potential field approach is also implemented (see script `PotentialMapSet` or Part-3 of this README for further detail).

A canvas with three buttons are added for simple user interface. Basically it allows to add units on click when the game starts, and it also makes it possible to activate rally once the units addition is done. Agents are added according to script `CreateAgent` attached to `GameObject AI`, of which all kinds of agents are children. The transition between adding units and rally is controlled by boolean variable `SpawnIsDone` in script `UI_ButtonControl` of the `GameObject _manager`.
   
Agents are defined by prefabs, currently either `RVOAgent` or `AirAgent`, each having a script attached. In the stage of deploying or spawning of agents, it is needed that each unit will push away other units closerby when the local point of mouse click gets crowded. This is done in the function `CheckPushAway()` in RVO_Agent:



      void CheckPushAway()
      {
         foreach(var agent in GameObject.FindGameObjectWithTag("RVO_sim").GetComponent<RVO_Simulator>().rvoGameObjs)
         {
            float agentDist = Vector3.Distance(agent.transform.position, transform.position);
            if (agentDist > 0 &&  agentDist < (characterController.radius + agent.GetComponent < CharacterController>().radius) * 1.25f)
            {
               Vector3 dirMove = (transform.position - agent.transform.position).normalized;
               if (AstarPath.active.GetNearest(transform.position + dirMove * Time.deltaTime).node.Walkable == true)
               {
                  transform.Translate(dirMove * Time.deltaTime);
               }
            }
         }
         if (agentIndex != -1)
         {
            GameObject.FindGameObjectWithTag("RVO_sim").GetComponent<RVO_Simulator>().agentPositions[agentIndex] =   
            new RVO.Vector2(transform.position.x, transform.position.z);
         }
      }


Basically the CheckPushAway function check the distance between each agents and push one self away from any agent close by along the opposite of the agent to to agent direcetion, as long as the agent is not entering non-walkable regime. The effect of pushing away each other can be clearly seen at the instantiation. 

## 2. Path finding based on A* algorithm implemented with reciprocal velocity obstacle (RVO) 

A* path finding is done following the documentation and examples from the package website. For each (ground) agent, a path from current position to the clicked point, which is represented by a list of grid nodes based on A* heuristic function, is caculated whenever a mouse click occurs. The destination point is obtained from shedding a ray of light from camera to the mousePosition and returning a hit point on the terrain world.

And the `RVO simulator` takes each agent together with its path as an input added to the list of agents and paths. In each time frame, the Simulator will run local collision avoidance `Simulator.Instance.doStep()` after each agent's position and preferred velocity are updated based on A* paths as is written in `FixedUpdate()` of RVO_Simulator. The idea of RVO is that agents will form velocity obstacle cones in the velocity space based on the relative velocities with other agents, and a new velocity resulted from the current preferred velocity and a velocity outside the RVOs will be given to each agent in 
order to prevent collisions.

## 3. Path finding based on the potential field method
A* path finding works great when the total number of agents is small and the path is not too long. But it will become time consuming if the number of agent increases or the path length is long, since the complexity is exponential ~ `O((number of next states)^(pathlength))`. Therefore a potential based search is implemented and used when the path is expected to be long and/or there are a large number of units. 

The decision of using A star search or potential field is determined in the RVO_Agent acript by comparing the log of grid node size with the log of expected A* time complexity `log (N * 3^(PathLengthEstimated)`, in which N is the number of agents, 3 is available next step state numbers, PathNumberEstimated is estimation of path length based on the distance to target point and grid size:

`if(Mathf.Log(simulator.agentPositions.Count) + 3 * Mathf.Log(pathLengthEstimated) > Mathf.Log(AstarPath.active.data.gridGraph.Depth * AstarPath.active.data.gridGraph.Width))`

The potential field search is implemented in script `PotentialFieldSet`. Functions related to potential field method are called in the RVOAgent script.The major steps and work flow are explained as following:

### 1. Generate potential map
The first step is to generate potential map, done by function `GetPotentialMap(target, gridgraph)`. `target` is obtained from 
mousePosition, and `gridgraph` is obtained from the A* grid. In `GeneratePotentialMap`, it is needed to find the grid node that contains the target position. `gridgraph.GetNearest()` method is used to get target node index. This node is conisdered the "source" of an attracting potential in the current map, and thus the potential value of this node is set to zero. Beginning from this source point, the potential map is calculated using standard breadth first search. 

Each node will expand to the neighboring nodes (except the one already visited) and potential value of new nodes will be previous potential value + 1:

        
         
         
          //searched HashSet 
          HashSet<int> nodeIndicesSearched = new HashSet<int>();
          nodeIndicesSearched.Add(gridGraph.GetNearest(target).node.NodeIndex);

          Queue<List<float>> coordinateQueue = new Queue<List<float>>();
          coordinateQueue.Enqueue(new List<float> {target_w, target_d });


          List<List<int>> grid_Direct = new List<List<int>>();
          grid_Direct.Add(new List<int> { -1, 0 }); // w-1 : left
          grid_Direct.Add(new List<int> {  1, 0 }); // w+1 : right
          grid_Direct.Add(new List<int> { 0, -1 }); // d-1 : up
          grid_Direct.Add(new List<int> { 0,  1 }); // d+1 : down

          while (coordinateQueue.Count != 0)
          {
              var currentCoord = coordinateQueue.Dequeue();
              float currentCoordinate_w = currentCoord[0];
              float currentCoordinate_d = currentCoord[1];

              int currentIndex_w = (int)((currentCoordinate_w) / stepPerGrid + gridGraph.width / 2);
              int currentIndex_d = (int)((-1 * currentCoordinate_d) / stepPerGrid + gridGraph.depth / 2);

              current_potential = resultMap[currentIndex_d, currentIndex_w];
              //Debug.Log("current location's potential = " + current_potential.ToString());

              foreach( var dir in grid_Direct)
              {
                  int newIndex_w = currentIndex_w + dir[0];
                  int newIndex_d = currentIndex_d + dir[1];

                  if (newIndex_w >= 0 && newIndex_w < gridGraph.Width && newIndex_d >= 0 && newIndex_d < gridGraph.Depth)
                  {
                      float newCoordinate_w = currentCoordinate_w + stepPerGrid * dir[0];
                      float newCoordinate_d = currentCoordinate_d - stepPerGrid * dir[1]; //note the minus sign for y direction

                      GraphNode newNode = gridGraph.GetNearest(new Vector3(newCoordinate_w, 0, newCoordinate_d)).node;
                      int newNodeIndex = newNode.NodeIndex;

                      if (!nodeIndicesSearched.Contains(newNodeIndex))
                      {
                          if(newNode.Walkable)
                          {
                              // if new grid is not visited before and is walkable
                              resultMap[newIndex_d, newIndex_w] = current_potential + 1;

                              // put the newly calculated point into queue
                              coordinateQueue.Enqueue(new List<float> { newCoordinate_w, newCoordinate_d });
                              totalAvailable += 1;
                          }

                      nodeIndicesSearched.Add(newNodeIndex);

                      }
                  }
              }
          }


Note that although 4 directions will be checked for each node expansion, only newly visited nodes will change the potential value. A hash set `nodeIndicesSearched` stores all the visited nodes indices. In the end, all walkable nodes will be traversed, and the non-walkable nodes will remain at the initial value `largePenalty`, which is considerably larger than normal potential values (currently set to 10000).

### 2. Generate field map
The next step is to generate field map from the potential map, and is done in the function ` public Vector2[,] GetFieldMap(int[,] paddedPotentialMap)`. The input is a padded potential map (2D int array), and returns a 2D Vector2 array. 

The original potential map resulted from  `GetPotentialMap` is padded by one unit along the boundaries (done in `public int[,] PadMap(int[,] potentialMap)`), dimension going from `N * N` to `(N+2) * (N+2)`, the padding is to make the vector derivative of potential field around the edges easier to be treated.

Based on the padded potential map, the field is calculated for each node of the original potential map node:

For each (i,j) of potentialMap, if the node is walkable:

      
      if (paddedPotentialMap[i, j] != largePenalty && paddedPotentialMap[i, j] != 0) 
       {
            //partial derivative along x(column) and y(row) direction
           //need to consider the boundary points separately... is there a smarter way?

           leftPotential = paddedPotentialMap[i, j - 1];
           rightPotential = paddedPotentialMap[i, j + 1];
           upPotential = paddedPotentialMap[i - 1, j];
           downPotential = paddedPotentialMap[i + 1, j];

           // if any point of the four neighbours is non-walkable, use current potential[i,j] + artificial repulse
           if (leftPotential == largePenalty)
           {
               leftPotential = paddedPotentialMap[i, j] + repulse;
           }

           if (rightPotential == largePenalty)
           {
               rightPotential = paddedPotentialMap[i, j] + repulse;
           }

           if (upPotential == largePenalty)
           {
               upPotential = paddedPotentialMap[i, j] + repulse;
           }

           if (downPotential == largePenalty)
           {
               downPotential = paddedPotentialMap[i, j] + repulse;
           }

           int delta_x = leftPotential - rightPotential;
           int delta_y = upPotential - downPotential; // delta_y > 0 means pointind down

           resultFieldMap[i - 1, j - 1] = new Vector2(delta_x, delta_y).normalized;

      }
      
      

But the scenario of treating non-walkable area is a little bit tricky. Initially the field map of non-walkable is set to zero, which clearly cannot work because the agent will not move anywhere once located in the non-walkable point after update. I end up with the following mechanism to deal with agent into the non-walkable area: for a non-walkable point, search all eight neighbors of the point and find the minimal potential of from these eight nodes, and let the vector field point from the current non-walkable node to that node with minimal potential field. The code snippet is the following:

         



            List<List<int>> grid_Proximity = new List<List<int>>();
            grid_Proximity.Add(new List<int> { -1, 0 }); // w-1 : left
            grid_Proximity.Add(new List<int> { 1, 0 }); // w+1 : right
            grid_Proximity.Add(new List<int> { 0, -1 }); // d-1 : up
            grid_Proximity.Add(new List<int> { 0, 1 }); // d+1 : down
            grid_Proximity.Add(new List<int> { -1, 1 }); // w-1, d+1: lower left
            grid_Proximity.Add(new List<int> { 1, 1 }); // w+1 , d+1: lower right
            grid_Proximity.Add(new List<int> { -1, -1 }); // w-1, d-1 : upper left
            grid_Proximity.Add(new List<int> { 1, -1 }); // w+1, d-1 : upper right

            int minPotentialProximity = paddedPotentialMap[i, j];
            List<int> minPotentialIndex = new List<int>{0, 0};


            // find the proximity point with smallest potential
            for (int k = 0; k < grid_Proximity.Count; k++)
            {
               int checkPotential_w = NonWalkable_w + grid_Proximity[k][0];
               int checkPotential_d = NonWalkable_d + grid_Proximity[k][1];
               int checkPotential = paddedPotentialMap[checkPotential_d, checkPotential_w];

               if (checkPotential < minPotentialProximity)
               {
                   minPotentialProximity = checkPotential;
                   minPotentialIndex[1] = grid_Proximity[k][1];
                   minPotentialIndex[0] = grid_Proximity[k][0];
               }

            }
            resultFieldMap[i - 1, j - 1] = new Vector2(minPotentialIndex[0], minPotentialIndex[1]).normalized;
               

### 3. Get path from FieldMap
The final step is to get the path as a list of node coordinates, as is realized in 
`public List<Vector3> GetPathFromFieldMap(Vector3 startPosition, Vector3 target, Vector2[,] fieldMap)`. 

For each agent's position as a starting point `startPosition` and destination `target`, the next node position is obtained based on the field map `fieldmap`:


              
      moveToPosition.x = currentNodePosition.x + fieldMap[currentIndex_d, currentIndex_w].x * stepPerGrid;
      moveToPosition.y = currentNodePosition.y;
      // note the "-" sign for vertical direction
      moveToPosition.z = currentNodePosition.z - fieldMap[currentIndex_d, currentIndex_w].y * stepPerGrid; 
      nextNodePosition = GetClosestNodePosition(stepPerGrid, fieldMapWidth, fieldMapDepth, moveToPosition);
      currentNodePosition = nextNodePosition;
`
         

The above is executed until the agent position and target are within the size of one node.

In the `RVO_Agent` script, the lists of path nodes feeding into RVO simualtor either come from A star PathFinding package, or from potential vector field map. Once such list of path nodes are obtained and given to RVO simulator, the agent motion will follow the same control.  




