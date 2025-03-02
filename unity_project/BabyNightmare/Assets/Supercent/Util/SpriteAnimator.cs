using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Supercent.Util
{
    public class SpriteAnimator : SpriteImport
    {
        public bool m_SetNativeSize;
        public bool m_Loop;
        public bool playOnAwake;
        public Image m_Image;
        public SpriteRenderer m_Renderer;
        public float m_Interval;

        private void OnEnable()
        {
            if (playOnAwake)
            {
                Play(m_Loop);
            }
        }

        public void Play(bool inLoop)
        {
            StopCoroutine("Co_Sprite");
            StartCoroutine("Co_Sprite", inLoop);
        }

        IEnumerator Co_Sprite(bool inLoop)
        {
            //		float p=1;
            while (true)
            {
                for (int i = 0; i < m_Sprites.Length; i++)
                {
                    if (m_Image != null)
                    {
                        m_Image.sprite = m_Sprites[i];
                        if (m_SetNativeSize)
                            m_Image.SetNativeSize();
                    }
                    if (m_Renderer != null)
                    {
                        m_Renderer.sprite = m_Sprites[i];
                    }

                    var secDone = Time.timeAsDouble + m_Interval;
                    while (Time.timeAsDouble < secDone)
                        yield return null;
                }
                if (!inLoop)
                {
                    break;
                }
            }
        }
    }
}