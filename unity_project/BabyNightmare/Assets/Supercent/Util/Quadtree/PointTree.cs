using UnityEngine;

namespace Supercent.Util.Quadtree
{
    public class PointTree<T>
    {
        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private PointNode<T> _rootNode = null;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public PointTree(Vector2 center, Vector2 size, int depthLimit)
        {
            _rootNode = new PointNode<T>(center, size, 1, depthLimit);
        }

        public void Add(T t, Vector2 pos)
        {
            _rootNode.Add(t, pos);
        }

        public void Remove(T t)
        {
            _rootNode.Remove(t);
        }

        public void Clear()
        {
            _rootNode.Clear();
        }

        public bool TryGetNearby(Vector2 pos, out T value)
        {
            return _rootNode.TryGetNearby(pos, out value);
        }
    }
}