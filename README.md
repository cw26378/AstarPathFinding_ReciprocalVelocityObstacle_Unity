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
