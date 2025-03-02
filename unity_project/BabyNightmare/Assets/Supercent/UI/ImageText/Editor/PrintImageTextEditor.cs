using System.Collections;
using System.Collections.Generic;
using Supercent.Util;
using UnityEditor;
using UnityEngine;

namespace Supercent.UI
{
    [CustomEditor(typeof(PrintImageText))]
    public class PrintImageTextEditor : BehaviourBaseEditor
    {
        private void OnEnable() {
            
            PrintImageText my = (PrintImageText)target;

            var canvas = my.GetComponentInParent<Canvas>();

            if(null == canvas)
            {
                Debug.LogError("There is no Canvas, please set 'Canvas'");

                DestroyImmediate(my);

                return;
            }

        }   
    }
}

