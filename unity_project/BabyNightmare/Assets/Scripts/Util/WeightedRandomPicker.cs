using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.Util
{
    public class WeightedRandomPicker<T>
    {
        //------------------------------------------------------------------------------
        // static variables
        //------------------------------------------------------------------------------  

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------  
        private Dictionary<T, int> _dict = null;
        private int _sumOfWeights = 0;

        //------------------------------------------------------------------------------
        // functions - constructor
        //------------------------------------------------------------------------------  
        public WeightedRandomPicker()
        {
            _dict = new Dictionary<T, int>();
        }

        //------------------------------------------------------------------------------
        // function - public
        //------------------------------------------------------------------------------  
        public void Add(T item, int weight)
        {
            if (_dict.ContainsKey(item))
            {
                Debug.LogError($"item({item})은 이미 포함되어있습니다.");
                return;
            }
            if (0 > weight)
            {
                Debug.LogError($"가중치({weight})를 0보다 작게 둘 수 없습니다.");
                return;
            }

            _dict.Add(item, weight);

            _sumOfWeights += weight;
        }
        public void ModifyWeight(T item, int weight)
        {
            if (!_dict.ContainsKey(item))
            {
                Debug.LogError($"item({item})은 포함되어있지 않습니다.");
                return;
            }
            if (0 > weight)
            {
                Debug.LogError($"가중치({weight})를 0보다 작게 수정할 수 없습니다.");
                return;
            }

            _sumOfWeights -= _dict[item];

            _dict[item] = weight;

            _sumOfWeights += weight;
        }
        public void Remove(T item)
        {
            if (!_dict.ContainsKey(item))
            {
                Debug.LogError($"item({item})은 포함되어 있지 않습니다.");
                return;
            }

            _sumOfWeights -= _dict[item];
            _dict.Remove(item);
        }
        public void Clear()
        {
            _dict.Clear();
            _sumOfWeights = 0;
        }

        public T RandomPick()
        {
            var rand = UnityEngine.Random.Range(0, _sumOfWeights);
            return RandomPick(rand);
        }
        public T RandomPick(int rand)
        {
            //Clamp
            if (0 > rand) rand = 0;
            if (_sumOfWeights <= rand) rand = _sumOfWeights - 1;

            var current = 0;
            foreach (var pair in _dict)
            {
                current += pair.Value;
                if (rand < current)
                    return pair.Key;
            }

            Debug.LogError($"실패하였습니다. rand({rand}), current({current})");
            return default;
        }
    }
}