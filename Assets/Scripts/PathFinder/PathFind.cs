using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.PathFinder
{
    public class PathFind : MonoBehaviour
    {
        private const int MaxSearchTime = 100;
        private static PathFindAgent _mShareAgent;
        private static PathFind _mInstance;
        private static GameObject _mHoldGo;

        /// <summary>
        /// 一次性返回搜索结果,用于小图搜索
        /// </summary>
        public static PathFindResult FindPath(int starTileX, int starTileY, int endTileX, int endTileY, PathFindMapAgent findEngine)
        {
            if (_mShareAgent == null)
            {
                _mShareAgent = new PathFindAgent();
            }

            _mShareAgent.Reset(findEngine, starTileX, starTileY, endTileX, endTileY);
            PathFindResult result = null;
            var isFinish = false;
            var searchTime = 0;
            while (!isFinish)
            {
                _mShareAgent.TickSearch(out isFinish, out result);
                searchTime++;
                if (searchTime >= MaxSearchTime && !isFinish)
                {
                    isFinish = true;
                    result = new PathFindResult {IsHavePath = false};
                    Debug.LogError("Reach CEPathFind max loop");
                    _mShareAgent.DebugOutput();
                }
            }

            return result;
        }

        /// <summary>
        /// 异步搜索,需要等待回调
        /// </summary>
        public static void FindPathAsync(int starTileX, int starTileY,
            int endTileX, int endTileY,
            PathFindMapAgent findEngine,
            Action<PathFindResult> finishCallback)
        {
            if (_mInstance == null)
            {
                _mHoldGo = new GameObject("CEPathFind");
                DontDestroyOnLoad(_mHoldGo);
                _mInstance = _mHoldGo.AddComponent<PathFind>();
            }

            var agentProxy = new CePathFindAgentProxy {Agent = new PathFindAgent()};
            agentProxy.Agent.Reset(findEngine, starTileX, starTileY, endTileX, endTileY);
            agentProxy.Callback = finishCallback;

            _mInstance.AddAgentProxy(agentProxy);
        }


        private readonly List<CePathFindAgentProxy> _mAllAgentProxyList = new List<CePathFindAgentProxy>();

        // Update is called once per frame
        private void Update()
        {
            if (_mAllAgentProxyList.Count <= 0) return;

            _mAllAgentProxyList.ForEach(proxy =>
            {
                bool isFinish;
                PathFindResult result;
                proxy.Agent.TickSearch(out isFinish, out result);
                proxy.SearchTime++;

                if (!isFinish && proxy.SearchTime >= MaxSearchTime)
                {
                    isFinish = true;
                    result = new PathFindResult {IsHavePath = false};
                    Debug.LogError("Reach CEPathFind max loop");
                    _mShareAgent.DebugOutput();
                }

                if (!isFinish) return;
                proxy.Callback(result);
                proxy.IsFinish = true;
            });

            _mAllAgentProxyList.RemoveAll(proxy => proxy.IsFinish);
        }

        private void AddAgentProxy(CePathFindAgentProxy proxy)
        {
            _mAllAgentProxyList.Add(proxy);
        }

        private class CePathFindAgentProxy
        {
            public bool IsFinish;
            public PathFindAgent Agent;
            public Action<PathFindResult> Callback;
            public int SearchTime;
        }
    }
}