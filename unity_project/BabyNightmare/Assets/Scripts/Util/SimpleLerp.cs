using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Supercent.Util;
using TMPro;
using BabyNightmare.HUD;

namespace BabyNightmare.Util
{
    public static class SimpleLerp
    {
        public static IEnumerator Co_Bounce(RectTransform rtf, Vector2 startPos, Vector2 targetPos, float duration, AnimationCurve curve, Action halfCallback = null)
        {
            var elapsed = 0f;
            duration = duration * 0.5f;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;

                var factor = curve.Evaluate(elapsed / duration);
                rtf.anchoredPosition = Vector2.Lerp(startPos, targetPos, factor);
            }

            halfCallback?.Invoke();

            while (elapsed < duration)
            {
                yield return null;
                elapsed -= Time.deltaTime;

                var factor = curve.Evaluate(elapsed / duration);
                rtf.anchoredPosition = Vector2.Lerp(startPos, targetPos, factor);
            }

            rtf.anchoredPosition = startPos;
        }

        public static IEnumerator Co_Bounce_Horizontal(Transform tf, float duration, AnimationCurve curve)
        {
            var startScale = tf.localScale;
            var targetScale = new Vector3(1.7f, 0.6f, 1f);
            var time1 = duration * 0.6f;
            var time2 = duration * 0.4f;

            yield return Co_LerpScale(tf, startScale, targetScale, curve, time1);
            yield return Co_LerpScale(tf, targetScale, startScale, curve, time2);
        }

        public static IEnumerator Co_BounceScale(Transform tf, Vector3 targetScale, AnimationCurve curve, float duration, bool loop = false)
        {
            var startScale = tf.localScale;
            var halfTime = duration / 2;

            while (true)
            {
                yield return Co_LerpScale(tf, startScale, targetScale, curve, halfTime);
                yield return Co_LerpScale(tf, targetScale, startScale, curve, halfTime);

                if (false == loop)
                {
                    tf.localScale = startScale;
                    yield break;
                }
            }
        }

        public static IEnumerator Co_BounceScale(Transform tf, Vector3 targetScale, AnimationCurve curve, float duration, Action callback)
        {
            var startScale = tf.localScale;
            var halfTime = duration / 2;

            yield return Co_LerpScale(tf, startScale, targetScale, curve, halfTime);
            yield return Co_LerpScale(tf, targetScale, startScale, curve, halfTime);

            tf.localScale = startScale;

            callback?.Invoke();
        }

        public static IEnumerator Co_BounceRotation(Transform tf, Quaternion targetRot, float duration, AnimationCurve curve, Action callback = null)
        {
            var halfTime = duration / 2;

            var startRot = Quaternion.Euler(tf.eulerAngles);

            yield return Co_LerpRotation(tf, startRot, targetRot, halfTime, curve);
            yield return Co_LerpRotation(tf, targetRot, startRot, halfTime, curve);

            callback?.Invoke();
        }

        public static IEnumerator Co_BounceRotation_Loop(Transform tf, Quaternion startRot, Quaternion targetRot, float duration, AnimationCurve curve)
        {
            var halfTime = duration / 2;

            while (true)
            {
                yield return Co_LerpRotation(tf, startRot, targetRot, halfTime, curve);
                yield return Co_LerpRotation(tf, targetRot, startRot, halfTime, curve);
            }
        }

        public static IEnumerator Co_LerpScale_Loop(Transform tf, Vector3 startScale, Vector3 targetScale, AnimationCurve curve, float lerpTime)
        {
            var elapsed = 0f;
            while (elapsed < lerpTime)
            {
                yield return null;
                elapsed += Time.deltaTime;
                var factor = curve.Evaluate(elapsed / lerpTime);
                tf.localScale = Vector3.Lerp(startScale, targetScale, factor);
            }

            while (0 < elapsed)
            {
                yield return null;
                elapsed -= Time.deltaTime;
                var factor = curve.Evaluate(elapsed / lerpTime);
                tf.localScale = Vector3.Lerp(startScale, targetScale, factor);
            }

            tf.localScale = startScale;

            yield return Co_LerpScale(tf, startScale, targetScale, curve, lerpTime);
        }

        public static IEnumerator Co_LerpScale(Transform tf, AnimationCurve curve, float lerpTime, Action callback = null)
        {
            var elapsed = 0f;
            while (elapsed < lerpTime)
            {
                yield return null;
                elapsed += Time.deltaTime;
                var factor = curve.Evaluate(elapsed / lerpTime);
                tf.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, factor);
            }

            tf.localScale = Vector3.one;
            callback?.Invoke();
        }

        public static IEnumerator Co_LerpScale(Transform tf, Vector3 startScale, Vector3 targetScale, AnimationCurve curve, float lerpTime, Action callback = null)
        {
            var elapsed = 0f;
            while (elapsed < lerpTime)
            {
                yield return null;
                elapsed += Time.deltaTime;
                var factor = curve.Evaluate(elapsed / lerpTime);
                tf.localScale = Vector3.Lerp(startScale, targetScale, factor);
            }

            tf.localScale = targetScale;
            callback?.Invoke();
        }

        public static IEnumerator Co_LerpSize(RectTransform rtf, Vector2 startSize, Vector2 targetSize, AnimationCurve curve, float lerpTime, Action callback = null)
        {
            var elapsed = 0f;
            while (elapsed < lerpTime)
            {
                yield return null;
                elapsed += Time.deltaTime;
                var factor = curve.Evaluate(elapsed / lerpTime);
                rtf.sizeDelta = Vector2.Lerp(startSize, targetSize, factor);
            }

            rtf.sizeDelta = targetSize;
            callback?.Invoke();
        }

        public static IEnumerator Co_LerpPosition(Transform tf, Vector3 startPos, Vector3 targetPos, AnimationCurve curve, float duration, bool yoyo)
        {
            while (true)
            {
                yield return Co_LerpPosition(tf, startPos, targetPos, curve, duration, null);
                yield return Co_LerpPosition(tf, targetPos, startPos, curve, duration, null);

                if (false == yoyo)
                {
                    tf.position = targetPos;
                    yield break;
                }
            }
        }

        public static IEnumerator Co_LerpPosition(Transform tf, Vector3 startPos, Vector3 targetPos, AnimationCurve curve, float duration, Action doneCallback)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;

                var factor = curve.Evaluate(elapsed / duration);
                tf.position = Vector3.Lerp(startPos, targetPos, factor);
            }

            tf.position = targetPos;
            doneCallback?.Invoke();
        }

        public static IEnumerator Co_LerpAnchoredPosition_Loop(RectTransform rtf, Vector2 startPos, Vector2 targetPos, AnimationCurve curve, float duration)
        {
            while (true)
            {
                yield return Co_LerpAnchoredPosition(rtf, startPos, targetPos, curve, duration);
                yield return Co_LerpAnchoredPosition(rtf, targetPos, startPos, curve, duration);
            }
        }

        public static IEnumerator Co_LerpAnchoredPosition(RectTransform rtf, Vector2 startPos, Vector2 targetPos, AnimationCurve curve, float duration, Action callback = null)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;

                var factor = curve.Evaluate(elapsed / duration);
                rtf.anchoredPosition = Vector2.Lerp(startPos, targetPos, factor);
            }

            rtf.anchoredPosition = targetPos;

            callback?.Invoke();
        }

        public static IEnumerator Co_LerpAnchoredPosition(RectTransform rtf, Vector2 startPos, Vector2 targetPos, AnimationCurve curve, float delay, float duration, Action callback)
        {
            yield return CoroutineUtil.WaitForSeconds(delay);
            yield return Co_LerpAnchoredPosition(rtf, startPos, targetPos, curve, duration, callback);
        }

        public static IEnumerator Co_LerpAnchoredPosition_Bezier(RectTransform rtf, Vector2 startPos, Vector2 midPos, Vector2 targetPos, float duration, AnimationCurve curve, Action callback = null)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;

                var factor = curve.Evaluate(elapsed / duration);
                rtf.anchoredPosition = VectorExtensions.CalcBezier(startPos, midPos, targetPos, factor);
            }

            rtf.anchoredPosition = targetPos;
            callback?.Invoke();
        }

        public static IEnumerator Co_LerpPoision_Bezier(Transform tf, Vector3 startPos, Vector3 midPos, Vector3 targetPos, float duration, AnimationCurve curve, Action callback = null)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;

                var factor = curve.Evaluate(elapsed / duration);
                tf.position = VectorExtensions.CalcBezier(startPos, midPos, targetPos, factor); ;
            }

            tf.position = targetPos;
            callback?.Invoke();
        }

        public static IEnumerator Co_LerpRotation(Transform tf, Vector3 startRot, Vector3 targetRot, float duration, AnimationCurve curve)
        {
            yield return Co_LerpRotation(tf, Quaternion.Euler(startRot), Quaternion.Euler(targetRot), duration, curve);
        }

        public static IEnumerator Co_LerpRotation(Transform tf, Quaternion startRot, Quaternion targetRot, float duration, AnimationCurve curve)
        {
            var elapsed = 0f;

            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;
                var factor = curve.Evaluate(elapsed / duration);
                tf.rotation = Quaternion.Lerp(startRot, targetRot, factor);
            }

            tf.rotation = targetRot;
        }

        public static IEnumerator Co_LerpCurrency(TextMeshProUGUI tmp, int startNum, int targetNum, float duration, AnimationCurve curve)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;

                var factor = curve.Evaluate(elapsed / duration);
                var value = (int)Mathf.Lerp(startNum, targetNum, factor);
                tmp.text = $"x{CurrencyUtil.GetUnit(value)}";
            }

            tmp.text = $"x{CurrencyUtil.GetUnit(targetNum)}";
        }

        public static IEnumerator Co_LerpColor(Image image, Color startColor, Color targetColor, float duration, AnimationCurve curve, Action doneCallback = null)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;

                var factor = curve.Evaluate(elapsed / duration);
                image.color = Color.Lerp(startColor, targetColor, factor);
            }

            image.color = targetColor;
            doneCallback?.Invoke();
        }
        public static IEnumerator Co_LerpColor(TextMeshProUGUI tmp, Color startColor, Color targetColor, float duration, AnimationCurve curve, Action doneCallback = null)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;

                var factor = curve.Evaluate(elapsed / duration);
                tmp.color = Color.Lerp(startColor, targetColor, factor);
            }

            tmp.color = targetColor;
            doneCallback?.Invoke();
        }


        public static IEnumerator Co_LerpNumber(TextMeshProUGUI tmp, int startNum, int targetNum, float duration, AnimationCurve curve)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;

                var factor = curve.Evaluate(elapsed / duration);
                var value = (int)Mathf.Lerp(startNum, targetNum, factor);
                tmp.text = $"{value}";
            }

            tmp.text = $"{targetNum}";
        }

        public static IEnumerator Co_LerpAlpha(CanvasGroup canvasGroup, float startAlpha, float targetAlpha, float duration, AnimationCurve curve, Action doneCallback = null)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;
                var factor = curve.Evaluate(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, factor);
            }

            canvasGroup.alpha = targetAlpha;
            doneCallback?.Invoke();
        }

        public static IEnumerator Co_Invoke(float duration, Action doneCallback)
        {
            yield return CoroutineUtil.WaitForSeconds(duration);

            doneCallback?.Invoke();
        }
    }
}