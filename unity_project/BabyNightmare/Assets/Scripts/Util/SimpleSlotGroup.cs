using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BabyNightmare.Util
{

    public class SimpleSlotGroup : MonoBehaviour
    {
        [SerializeField] private RectTransform _rtf;
        [SerializeField] private GridLayoutGroup _glg;
        [SerializeField] private SimpleSlot _simpleSlotResource;

        private SimpleSlot[,] _slotArr = null;

        public List<SimpleSlot> SlotList { get; private set; }
        public int Row { get; private set; }
        public int Column { get; private set; }


        public void Build(int row, int column)
        {
            Clear();
            Row = row;
            Column = column;

            _glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _glg.constraintCount = row;

            _slotArr = new SimpleSlot[row, column];
            SlotList = new List<SimpleSlot>();
            for (var i = 0; i < row; i++)
            {
                for (var j = 0; j < column; j++)
                {
                    var slot = Instantiate(_simpleSlotResource, transform);
                    slot.Enabled = false;
                    _slotArr[i, j] = slot;
                    SlotList.Add(slot);
                }
            }
        }

        private void Clear()
        {
            if (null == _slotArr)
                return;

            var row = _slotArr.GetLength(0);
            var column = _slotArr.GetLength(1);
            for (var i = 0; i < row; i++)
            {
                for (var j = 0; j < column; j++)
                {
                    Destroy(_slotArr[i, j].gameObject);
                }
            }
        }

        public void EnableSlot(int row, int column, bool enable)
        {
            _slotArr[row, column].Enabled = enable;
        }

        public void OcuppySlot(int row, int column, MonoBehaviour owner)
        {
            _slotArr[row, column].Owner = owner;
        }
    }
}