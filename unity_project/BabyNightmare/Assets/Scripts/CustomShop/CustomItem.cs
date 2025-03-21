using System;
using UnityEngine;
using UnityEditor;
using BabyNightmare.StaticData;
using SimpleJSON;

namespace BabyNightmare.CustomShop
{
    public class CustomItem 
    {
        [SerializeField] private Transform _model;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private MeshFilter _shadow;

        // public void Init(FeedItem feedItem, Vector3 worldPosition, Vector3 localScale, Action<FieldItem> onReturn)
        // {
        //     TF.position = worldPosition;
        //     TF.localScale = localScale;

        //     _model.localEulerAngles = feedItem.RandomAngle;
        //     _meshFilter.mesh = feedItem.Mesh;
        //     _renderer.sharedMaterial = feedItem.Material;
        //     RefreshShadow(feedItem.ShadowData);

        //     _onReturn = onReturn;
        // }

        // private void RefreshShadow(FeedShadowData shadowData)
        // {
        //     if (false == shadowData.Use || null == shadowData.Mesh)
        //     {
        //         _shadow.gameObject.SetActive(false);
        //         return;
        //     }

        //     _shadow.mesh = shadowData.Mesh;
        //     _shadow.transform.localPosition = shadowData.Pos;
        //     _shadow.transform.localEulerAngles = shadowData.Angle;
        //     _shadow.transform.localScale = shadowData.Scale;
        //     _shadow.gameObject.SetActive(true);
        // }

        // public void RefreshModel(FeedItem feedItem)
        // {
        //     if (null == feedItem)
        //     {
        //         Debug.LogError($"FeedItem이 null입니다.");
        //         return;
        //     }

        //     _model.localEulerAngles = feedItem.Angle;
        //     _meshFilter.mesh = feedItem.Mesh;
        //     _renderer.sharedMaterial = feedItem.Material;

        //     _shadow.gameObject.SetActive(false);
        // }

        // public void RefreshRotation(FeedItem feedItem, Vector3 angle)
        // {
        //     _model.localEulerAngles = angle;
        //     _meshFilter.mesh = feedItem.Mesh;
        //     _renderer.sharedMaterial = feedItem.Material;
        //     RefreshShadow(feedItem.ShadowData);
        // }

        public void RefreshOutlineWidth(float amount)
        {
            _renderer.material.SetFloat("_OutlineWidth", amount);
        }
    }
}
