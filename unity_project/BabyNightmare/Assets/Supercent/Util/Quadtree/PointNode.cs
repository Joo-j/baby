using System.Collections.Generic;
using UnityEngine;

namespace Supercent.Util.Quadtree
{
    public class PointNode<T>
    {
        private class Data
        {
            public T       Target;
            public Vector2 Pos;
        }

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private int _depthSelf  = -1;
        private int _depthLimit = -1;

        private Vector2 _boundLeftBottom     = Vector2.zero;
        private Vector2 _boundRightTop = Vector2.zero;

        private Dictionary<int, Data> _dataSet = null;

        private List<PointNode<T>> _childNodes = null;

        private const int CHILD_NODE_SIZE = 4;

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public Vector2 Center { get; private set; } = Vector2.zero;
        public Vector3 Size   { get; private set; } = Vector2.zero;

        public bool IsLastDepth => _depthSelf == _depthLimit;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        /// <summary>
        /// 생성자
        /// </summary>
        public PointNode(Vector2 center, Vector2 size, int depth, int depthLimit)
        {
            Center = center;
            Size   = size;

            var half = size * 0.5f;
            _boundLeftBottom = center - half;
            _boundRightTop   = center + half;

            _depthSelf  = depth;
            _depthLimit = depthLimit;

            _dataSet?.Clear();
            _dataSet = new Dictionary<int, Data>();

            CreateChildNodes();
        }

        /// <summary>
        /// 정보 추가
        /// </summary>
        public void Add(T t, Vector2 pos)
        {
            // 마지막 깊이라면 정보 추가
            if (IsLastDepth)
            {
                _dataSet[t.GetHashCode()] = new Data() 
                {
                    Target = t,
                    Pos    = pos,
                };
                return;
            }

            // 아니라면 자식 노드에 추가
            for (int i = 0, size = _childNodes.Count; i < size; ++i)
            {
                if (_childNodes[i].InBound(pos))
                {
                    _childNodes[i].Add(t, pos);
                    return;
                }
            }

            Debug.LogError($"[Supercent.Util.Quadtree.Node.Add] 정보를 추가하지 못했습니다. pos: {pos}");
        }

        /// <summary>
        /// 정보 제거
        /// </summary>
        public void Remove(T t)
        {
            if (IsLastDepth)
            {
                var hash = t.GetHashCode();
                if (_dataSet.ContainsKey(hash))
                    _dataSet.Remove(hash);

                return;
            }

            for (int i = 0, size = _childNodes.Count; i < size; ++i)
                _childNodes[i].Remove(t);
        }

        /// <summary>
        /// 클리어
        /// </summary>
        public void Clear()
        {
            if (null != _childNodes)
            {
                for (int i = 0, size = _childNodes.Count; i < size; ++i)
                {
                    _childNodes[i].Clear();
                    _childNodes[i] = null;
                }

                _childNodes.Clear();
                _childNodes = null;
            }

            _dataSet?.Clear();
            _dataSet = null;
        }

        /// <summary>
        /// 이 노드의 영역에 속해있는지 확인
        /// </summary>
        public bool InBound(Vector2 pos)
        {
            return _boundLeftBottom.x <= pos.x && pos.x <= _boundRightTop.x
                && _boundLeftBottom.y <= pos.y && pos.y <= _boundRightTop.y;
        }

        /// <summary>
        /// 가장 가까운 정보를 가져오기
        /// </summary>
        public bool TryGetNearby(Vector2 pos, out T value)
        {
            if (IsLastDepth)
            {
                value = default(T);

                if (0 == _dataSet.Count)
                    return false;

                var iter = _dataSet.GetEnumerator();
                var near = float.MaxValue;
                var temp = 0f;

                while (iter.MoveNext())
                {
                    temp = (iter.Current.Value.Pos - pos).sqrMagnitude;
                    if (temp < near)
                    {
                        near  = temp;
                        value = iter.Current.Value.Target;
                    }
                }

                return true;
            }

            for (int i = 0, size = _childNodes.Count; i < size; ++i)
            {
                if (_childNodes[i].InBound(pos))
                    return _childNodes[i].TryGetNearby(pos, out value);
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// 자식 노드 생성
        /// </summary>
        private void CreateChildNodes()
        {
            if (_depthLimit <= _depthSelf)
                return;

            var halfSize        = Size * 0.5f;
            var quarterPositive = Size * 0.25f;
            var quarterNegative = -quarterPositive;
            var depthOfChild    = _depthSelf + 1;

            _childNodes?.Clear();
            _childNodes = new List<PointNode<T>>()
            {
                new PointNode<T>(Center + new Vector2(quarterNegative.x, quarterNegative.y), halfSize, depthOfChild, _depthLimit),
                new PointNode<T>(Center + new Vector2(quarterPositive.x, quarterNegative.y), halfSize, depthOfChild, _depthLimit),
                new PointNode<T>(Center + new Vector2(quarterNegative.x, quarterPositive.y), halfSize, depthOfChild, _depthLimit),
                new PointNode<T>(Center + new Vector2(quarterPositive.x, quarterPositive.y), halfSize, depthOfChild, _depthLimit),
            };
        }
    }
}