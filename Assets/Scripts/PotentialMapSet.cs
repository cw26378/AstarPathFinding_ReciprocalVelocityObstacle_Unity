using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PotentialMapSet : MonoBehaviour
{
    private int largePenalty = 10000;
    private int repulse = 100;
    float stepPerGrid;

    // GetPathFromFieldMap returns the PathNode list from field map
    // Input:
    //      Vector3 startPosition
    //      Vector3 target or rally point
    //      Vector2[,] 2D field map of field or unit vector
    // Output:
    //      List of nodes forming the path from start to target

    public List<Vector3> GetPathFromFieldMap(Vector3 startPosition, Vector3 target, Vector2[,] fieldMap)
    {
        if(fieldMap == null)
        {
            throw new System.Exception("fieldMap is not loaded correctly");
            //return null;
        }
        List<Vector3> resultPathNode = new List<Vector3>();
        int iterationN = 0;
        int fieldMapWidth = fieldMap.GetLength(1);
        int fieldMapDepth = fieldMap.GetLength(0);

        Vector3 currentNodePosition = GetClosestNodePosition(stepPerGrid, fieldMapWidth, fieldMapDepth, startPosition);
        Vector3 nextNodePosition;

        while ((Mathf.Abs((currentNodePosition - target).x) >= stepPerGrid/2 || Mathf.Abs((currentNodePosition - target).y) >= stepPerGrid / 2) && iterationN < 1000) // add the upper iteration N to prevent getting stuck...
        {


            // get the current node index from position

            int currentIndex_w = (int)((currentNodePosition.x) / stepPerGrid + fieldMapWidth / 2);
            int currentIndex_d = (int)((-1 * currentNodePosition.z) / stepPerGrid + fieldMapDepth/ 2);

            resultPathNode.Add(currentNodePosition);
            iterationN++;

            // get the next node index based on field map
            // next node can be obtained in two ways: 1) caculate next index and then get the node position OR 2) calculate the moving vector and then find closest node position

            // approach 1)
            // int nextIndex_w = currentIndex_w + (int)((fieldMap[currentIndex_d, currentIndex_w].x) / 0.5001f);
            // int nextIndex_d = currentIndex_d + (int)((fieldMap[currentIndex_d, currentIndex_w].y) / 0.5001f);
            // nextNodePosition = new Vector3((nextIndex_w - fieldMapWidth/2 + 0.5f) * stepPerGrid, 0.0f, -(nextIndex_d - fieldMapDepth/2 + 0.5f) * stepPerGrid);

            //Debug.Log("Calculate nextNode Position from index change:" + nextNodePosition.ToString());

            // approach 2)
            Vector3 moveToPosition;
            // Debug.Log("the current NodePosition:" + currentNodePosition.ToString());
            // Debug.Log("the current index:" +" w: "+ currentIndex_w.ToString() + " d: " + currentIndex_d.ToString());
            if(!(currentIndex_d >= 0 && currentIndex_d < fieldMapDepth && currentIndex_w >= 0 && currentIndex_w < fieldMapWidth))
            {
                Debug.Log("Node index out of range.");
                return null;
            }
            moveToPosition.x = currentNodePosition.x + fieldMap[currentIndex_d, currentIndex_w].x * stepPerGrid;
            moveToPosition.y = currentNodePosition.y;
            moveToPosition.z = currentNodePosition.z - fieldMap[currentIndex_d, currentIndex_w].y * stepPerGrid; // note the "-" sign for vertical direction
            nextNodePosition = GetClosestNodePosition(stepPerGrid, fieldMapWidth, fieldMapDepth, moveToPosition);
            currentNodePosition = nextNodePosition;
            //Debug.Log("Calculate nextNode Position from moveToPosition's closest node" + nextNodePosition.ToString());

            //check if the nextNode is walkable
            int nextIndex_w = currentIndex_w + (int)((fieldMap[currentIndex_d, currentIndex_w].x) / 0.5001f);
            int nextIndex_d = currentIndex_d + (int)((fieldMap[currentIndex_d, currentIndex_w].y) / 0.5001f);

            if (fieldMap[nextIndex_d, nextIndex_w] == Vector2.zero)
            {
                // if for some reason agent came to a non-walkable grid, reverse the motion vector based on current and one before current grid in the resultPathNode list
                moveToPosition.x = 2 * resultPathNode[Mathf.Max(0, resultPathNode.Count - 2)].x - currentNodePosition.x;
                moveToPosition.y = currentNodePosition.y;
                moveToPosition.z = 2 * resultPathNode[Mathf.Max(0, resultPathNode.Count - 2)].z - currentNodePosition.z;
                nextNodePosition = GetClosestNodePosition(stepPerGrid, fieldMapWidth, fieldMapDepth, moveToPosition);
                currentNodePosition = nextNodePosition;
                Debug.Log("agent stuck in local trap...");

            }
 

        }
        return resultPathNode;
    }

    Vector3 GetClosestNodePosition(float stepSize, int width, int depth, Vector3 position)
    {
        // get the current node index from position
        int Index_w = (int)((position.x) / stepSize + width / 2);
        int Index_d = (int)(-(position.z) / stepSize + depth / 2);
        Vector3 result = new Vector3((Index_w - width / 2 + 0.5f) * stepSize, 0.0f, -(Index_d - depth / 2 + 0.5f) * stepSize);
        return result; 

    }



    // PadMap is used to pad the potential map in order to make the field map calculation easier
    // input: 
    //      int[,] potentialMap N * N
    // output
    //      int[,] paddedMap (N+1) * (N+1)

    public int[,] PadMap(int[,] potentialMap)
    {   

        int[,] paddedMap = new int[potentialMap.GetLength(0) + 2, potentialMap.GetLength(1) + 2];

        int paddedRowNum = paddedMap.GetLength(0);
        int paddedColNum = paddedMap.GetLength(1);

        for (int i = 0; i < paddedRowNum; i++)
        {   
            // for each row set the value of first col and last col
            paddedMap[i, 0] = largePenalty;
            paddedMap[i, paddedColNum-1] = largePenalty;
        }

        for (int j = 0; j < paddedColNum; j++)
        {
            // for each row set the value of first row and last row
            paddedMap[0, j] = largePenalty;
            paddedMap[paddedRowNum-1, j] = largePenalty;
        }

        // put the original potentialMap in! how to do slice in c#?

        for (int i = 1; i < paddedRowNum - 1; i++)
        {
            for (int j = 1; j < paddedColNum - 1; j++)
            {
                paddedMap[i, j] = potentialMap[i - 1, j - 1];
            }
        }

        return paddedMap;
    }

    // GetFieldMap calculates the vector field map based on the potential
    // The non-walkable area will have artificial repulse 
    // Input:
    //      int[,] paddedPotentialMap
    // Output:
    //      Vector2[,] resultFieldMap

    public Vector2[,] GetFieldMap(int[,] paddedPotentialMap)
    {
        //input should be ALREADY padded, otherwise boundary will not be treated. 

        int potentialRowNum = paddedPotentialMap.GetLength(0);
        int potentialColNum = paddedPotentialMap.GetLength(1);
        //Vector2[,] paddedMap = new Vector2[potentialRowNum+2, potentialColNum+2];
        Vector2[,] resultFieldMap = new Vector2[potentialRowNum-2, potentialColNum-2];
        int leftPotential;
        int rightPotential;
        int upPotential;
        int downPotential;
        for (int i = 1; i < potentialRowNum-1; i++)
        {
            for (int j = 1; j < potentialColNum-1; j++)
            {
                if (paddedPotentialMap[i, j] != largePenalty && paddedPotentialMap[i, j] != 0) // current potential map point is walkable and is not the target
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
                else
                {
                    // resultFieldMap[i - 1, j - 1] = Vector2.zero; // NOT working for local optimal trap...

                    // if current point is not walkable, move towards the smallest potential point
                    int NonWalkable_d = i;
                    int NonWalkable_w = j;


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

                }

            }
        }

        return resultFieldMap;
    }

    // GetPotentialMap calculate the potential map based on generated grid from A* and target position
    // Input:
    //      Vector3 target
    //      Pathfing.GridGraph
    // Output:
    //      int[,] resultMap

    public int[,] GetPotentialMap(Vector3 target, Pathfinding.GridGraph gridGraph)
    {
        int[,] resultMap = new int[gridGraph.Depth, gridGraph.Width];
        for (int i = 0; i < resultMap.GetLength(0); i++)
        {
            for (int j = 0; j < resultMap.GetLength(1); j++)
            {
                resultMap[i, j] = largePenalty;
            }
        }

        int totalAvailable = 0;

        stepPerGrid = (gridGraph.nodeSize);

        float target_w = ((Vector3) gridGraph.GetNearest(target).node.position).x; // x-coordinate
        float target_d = ((Vector3) gridGraph.GetNearest(target).node.position).z;   // y-coordinate

        int targetIndex_w = (int)((target_w) / stepPerGrid + gridGraph.width / 2);
        int targetIndex_d = (int)((-1 * target_d) / stepPerGrid + gridGraph.depth / 2);

        //Debug.Log("X in unit of grid size = " + ((target_w) / stepPerGrid).ToString() + ", and actual grid index = " + ((target_w) / stepPerGrid + gridGraph.width / 2).ToString());
        //Debug.Log("Calculated targetIndex:" + "row: " + targetIndex_d.ToString() + ", column: " + targetIndex_w);

        if (targetIndex_w >= gridGraph.width || targetIndex_w < 0 || targetIndex_d >= gridGraph.depth || targetIndex_d < 0)
        {
            Debug.Log("The target index is out of range of the map...");
            return null;
        }
        // depth -> row, width -> col
        int current_potential = 0;
        resultMap[targetIndex_d, targetIndex_w] = current_potential;

        //searched HashSet 
        HashSet<int> nodeIndicesSearched = new HashSet<int>();
        nodeIndicesSearched.Add(gridGraph.GetNearest(target).node.NodeIndex);

        //Queue<GraphNode> nodesQueue = new Queue<GraphNode>();
        //nodesQueue.Enqueue(gridGraph.GetNearest(target).node);


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
        //Debug.Log("If everything goes as expected, the size of nodeIndicesSearched should be slightly bigger than the walkable total area: " + nodeIndicesSearched.Count);
        //Debug.Log("Total available points should be this number (check the A* grid info)" + totalAvailable);

        return resultMap;
    }
}
