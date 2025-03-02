using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Supercent.UI
{
     public class PunctuationControlModule : MonoBehaviour
     {
          private RectTransform _rtf = null;

          private float Y_POS_OFFSET = 0.25f;

          // Start is called before the first frame update
          void Start()
          {
               
          }

          public void Init()
          {
               _rtf = GetComponent<RectTransform>();
          }

          public void Release()
          {
               _rtf = null;
          }

          public void SetPivot(Vector2 pivot)
          {
               _rtf.pivot = pivot;
          }

          public void SetSize(Vector2 size)
          {
               _rtf.sizeDelta = size;

               _rtf.anchoredPosition = Vector2.zero;
          }

          private void SetProperYPos()
          {
               var height = _rtf.sizeDelta.y;

               var yPos = -height * Y_POS_OFFSET;
               
               _rtf.anchoredPosition = Vector3.up * yPos;

          }
     }
}
