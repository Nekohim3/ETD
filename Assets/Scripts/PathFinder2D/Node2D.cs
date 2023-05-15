using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.PathFinder2D
{
    [System.Serializable]
    public class Node2D
    {
        public int     gCost, hCost;
        public bool    obstacle;
        public bool    collision;
        public Vector3 worldPosition;

        public int    GridX, GridY;
        public Node2D parent;


        public Node2D(bool _obstacle, Vector3 _worldPos, int _gridX, int _gridY)
        {
            obstacle      = _obstacle;
            worldPosition = _worldPos;
            GridX         = _gridX;
            GridY         = _gridY;
        }

        public int FCost => gCost + hCost;

        public void SetObstacle(bool isOb)
        {
            obstacle = isOb;
        }

        public void SetCollision(bool isCol)
        {
            collision = isCol;
        }
    }
}

