using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Pathfinding : MonoBehaviour
{
    private GridForPathFinding gridForPathFinding;

    private void Awake()
    {
        gridForPathFinding = GameObject.FindObjectOfType<GridForPathFinding>();
    }

    public void FindPath(PathRequest request, Action<PathResult> callback)
    {
        Node startNode = gridForPathFinding.NodeFromWorldPosition(request.pathStart);
        Node targetNode = gridForPathFinding.NodeFromWorldPosition(request.pathEnd);
        
        Vector3[] waypoints = new Vector3[] { };
        bool pathFindingSuccess = false;
        if (targetNode.walkable && startNode.walkable && startNode != targetNode)
        {
            Heap<Node> openSet = new Heap<Node>(gridForPathFinding.gridSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            
            openSet.Add(startNode);
            
            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirstItem();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathFindingSuccess = true;
                    break;
                }

                foreach (Node neighbourNode in gridForPathFinding.GetNeighboursOfNode(currentNode))
                {
                    if (!neighbourNode.walkable || closedSet.Contains(neighbourNode))
                    {
                        continue;
                    }

                    float CostOfGoingToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbourNode) + neighbourNode.penalty;
                    if (CostOfGoingToNeighbour < currentNode.gCost || !openSet.Contains(neighbourNode))
                    {
                        neighbourNode.gCost = CostOfGoingToNeighbour;
                        neighbourNode.hCost = GetDistance(neighbourNode, targetNode);
                        neighbourNode.parentNode = currentNode;
                        if (!openSet.Contains(neighbourNode))
                        {
                            openSet.Add(neighbourNode);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbourNode);
                        }
                    }
                }
            }
        }
        if (pathFindingSuccess)
        {
            waypoints = ReTrace(startNode, targetNode);
            pathFindingSuccess = waypoints.Length > 0;
        }
        callback(new PathResult(waypoints, pathFindingSuccess, request));
    }

    public Vector3[] ReTrace(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node currentNode = end;
        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        path.Clear();
        return waypoints;
    }

    public Vector3[] SimplifyPath(List<Node> nodes)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;
        for (int i = 1; i < nodes.Count; i++)
        {
            Vector2 directionNew = new Vector2(nodes[i].gridX - nodes[i - 1].gridX, nodes[i].gridY - nodes[i - 1].gridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(nodes[i].worldPosition);
            }
            
            directionOld = directionNew;
        }

        Vector3[] waypointsArray = waypoints.ToArray();
        waypoints.Clear();
        return waypointsArray;
    }

    public float GetDistance(Node nodeA, Node nodeB)
    {
        Vector3 distance = nodeA.worldPosition - nodeB.worldPosition;
        float distanceX = Mathf.Abs(distance.x);
        float distanceY = Mathf.Abs(distance.z);

        if (distanceX > distanceY)
        {
            return distanceY * 14f + ((distanceX - distanceY) * 10f);
        }
        return distanceX * 14f + ((distanceY - distanceX) * 10f);
    }
}
