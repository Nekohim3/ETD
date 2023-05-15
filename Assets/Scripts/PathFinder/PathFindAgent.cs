using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.PathFinder
{
    public sealed class PathFindAgent
    {
        private static readonly Stack MNodePool = new Stack();

        /// <summary>
        /// 每次Tick调用时候,搜索Node节点的个数
        /// </summary>
        private const int EachTickSearchNodeNum = 50;

        private PathFindMapAgent _mMapAgent;

//        private readonly List<CEPathFindNode> mOpenList;
        private readonly PriorityQueue<PathFindNode> _mOpenList;
        private readonly List<PathFindNode> _mCloseList;

        private PathFindNode _mStarNode;
        private PathFindNode _mEndNode;

        private PathFindNode _mCurrentNode;

        public PathFindAgent()
        {
            _mOpenList = new PriorityQueue<PathFindNode>(new PathFindNode());
            _mCloseList = new List<PathFindNode>();
        }

        public void Reset(PathFindMapAgent mapAgent, int startTileX, int startTileY, int endTileX, int endTileY)
        {
            _mMapAgent = mapAgent;

            RecycleNodes();

            _mStarNode = GetNewNode();
            _mStarNode.X = startTileX;
            _mStarNode.Y = startTileY;
            _mStarNode.IsWalkable = _mMapAgent.IsTileWalkable(_mStarNode.X, _mStarNode.Y);

            _mEndNode = GetNewNode();
            _mEndNode.X = endTileX;
            _mEndNode.Y = endTileY;
            _mEndNode.IsWalkable = _mMapAgent.IsTileWalkable(_mEndNode.X, _mEndNode.Y);

            _mCurrentNode = _mStarNode;
            _mCurrentNode.IsWalkable = _mMapAgent.IsTileWalkable(_mCurrentNode.X, _mCurrentNode.Y);
        }


        public void TickSearch(out bool isFinish, out PathFindResult result)
        {
            //起始节点和结束节点本身就无法走
            if (!_mStarNode.IsWalkable || !_mEndNode.IsWalkable)
            {
                isFinish = true;
                result = new PathFindResult {IsHavePath = false};
                RecycleNodes();
                return;
            }


            for (var i = 0; i < EachTickSearchNodeNum; i++)
            {
                if (_mCurrentNode.X == _mEndNode.X && _mCurrentNode.Y == _mEndNode.Y)
                {
                    isFinish = true;
                    result = GetPathFindResult(_mCurrentNode);
                    RecycleNodes();
                    return;
                }

                _mCloseList.Add(_mCurrentNode);

                CheckCurrentSearchAroundTile();

                if (_mOpenList.Count == 0)
                {
                    //没有Open节点了,全部搜索过,但未找到路径
                    isFinish = true;
                    result = new PathFindResult {IsHavePath = false};
                    RecycleNodes();
                    return;
                }

//                mOpenList.Sort(SortListByScore);
//                mCurrentNode = mOpenList[0];
//                mOpenList.RemoveAt(0);
                _mCurrentNode = _mOpenList.Remove();
            }

            isFinish = false;
            result = null;
            return;
        }

        public void DebugOutput()
        {
            Debug.Log($"Open node length :{_mOpenList.Count} Close node length :{_mCloseList.Count}");
        }

        private void CheckCurrentSearchAroundTile()
        {
            //上下左右
            DoCheckTile(_mCurrentNode.X + 1, _mCurrentNode.Y);
            DoCheckTile(_mCurrentNode.X - 1, _mCurrentNode.Y);
            DoCheckTile(_mCurrentNode.X, _mCurrentNode.Y + 1);
            DoCheckTile(_mCurrentNode.X, _mCurrentNode.Y - 1);

            if (_mMapAgent.GetTileSearchType() == PathFindMapAgent.TileSearchType.EightDirection)
            {
                //右上
                DoCheckTile(_mCurrentNode.X + 1, _mCurrentNode.Y + 1);
                //右下
                DoCheckTile(_mCurrentNode.X + 1, _mCurrentNode.Y - 1);
                //左下
                DoCheckTile(_mCurrentNode.X - 1, _mCurrentNode.Y - 1);
                //左上
                DoCheckTile(_mCurrentNode.X - 1, _mCurrentNode.Y + 1);
            }
            else if (_mMapAgent.GetTileSearchType() == PathFindMapAgent.TileSearchType.EightDirectionFixCorner)
            {
                var upTileWalkable = _mMapAgent.IsTileWalkable(_mCurrentNode.X, _mCurrentNode.Y + 1);
                var downTileWalkable = _mMapAgent.IsTileWalkable(_mCurrentNode.X, _mCurrentNode.Y - 1);
                var rightTileWalkable = _mMapAgent.IsTileWalkable(_mCurrentNode.X + 1, _mCurrentNode.Y);
                var leftTileWalkable = _mMapAgent.IsTileWalkable(_mCurrentNode.X - 1, _mCurrentNode.Y);

                if (upTileWalkable && rightTileWalkable)
                {
                    //右上
                    DoCheckTile(_mCurrentNode.X + 1, _mCurrentNode.Y + 1);
                }

                if (downTileWalkable && rightTileWalkable)
                {
                    //右下
                    DoCheckTile(_mCurrentNode.X + 1, _mCurrentNode.Y - 1);
                }

                if (downTileWalkable && leftTileWalkable)
                {
                    //左下
                    DoCheckTile(_mCurrentNode.X - 1, _mCurrentNode.Y - 1);
                }

                if (upTileWalkable && leftTileWalkable)
                {
                    //左上
                    DoCheckTile(_mCurrentNode.X - 1, _mCurrentNode.Y + 1);
                }
            }
        }

        private void DoCheckTile(int tileX, int tileY)
        {
            //如果当前节点已经在Open和Close列表中则忽略
            if (IsTileInNode(tileX, tileY, _mOpenList) || IsTileInNode(tileX, tileY, _mCloseList))
            {
                return;
            }

            var node = GetNewNode();
            SetNodeProperty(node, tileX, tileY);
            node.Parent = _mCurrentNode;
            if (node.IsWalkable)
            {
                _mOpenList.Add(node);
            }
            else
            {
                _mCloseList.Add(node);
            }
        }

        private void SetNodeProperty(PathFindNode node, int tileX, int tileY)
        {
            node.X = tileX;
            node.Y = tileY;
            _mMapAgent.GetTileProperty(tileX, tileY, _mStarNode, _mEndNode, out node.IsWalkable, out node.Score);
        }

        private static bool IsTileInNode(int tileX, int tileY, ICollection<PathFindNode> list)
        {
            foreach (var node in list)
            {
                if (node.X == tileX && node.Y == tileY)
                {
                    return true;
                }
            }

            return false;
//            return _list.Contains(node => node.x == _tileX && node.y == _tileY);
        }

        /// <summary>
        /// 节点评分,分数越小越优先遍历
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static int SortListByScore(PathFindNode a, PathFindNode b)
        {
            if (a.Score == b.Score)
            {
                return 0;
            }

            return a.Score > b.Score ? 1 : -1;
        }


        private static PathFindNode GetNewNode()
        {
            PathFindNode node;
            if (MNodePool.Count > 0)
            {
                node = (PathFindNode) MNodePool.Pop();
                node.Reset();
            }
            else
            {
                node = new PathFindNode();
            }

            return node;
        }


        private static PathFindResult GetPathFindResult(PathFindNode endNode)
        {
            var result = new PathFindResult();
            var maxNum = 1;
            var node = endNode;
            while (node.Parent != null)
            {
                node = node.Parent;
                maxNum++;
            }

            result.IsHavePath = true;
            result.Paths = new Vector2Int[maxNum];

            node = endNode;
            while (node != null)
            {
                result.Paths[maxNum - 1] = new Vector2Int(node.X, node.Y);
                node = node.Parent;
                maxNum--;
            }

            return result;
        }


        private void RecycleNodes()
        {
            foreach (var openNode in _mOpenList)
            {
                MNodePool.Push(openNode);
            }

//            mOpenList.ForEach(node => mNodePool.Push(node));
            _mCloseList.ForEach(node => MNodePool.Push(node));
            _mOpenList.Clear();
            _mCloseList.Clear();
        }
    }
}