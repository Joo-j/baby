using System.Collections.Generic;
using UnityEngine;

namespace Supercent.Core.Audio
{
    public class CacheAudio
    {
        Dictionary<string, ClipInfo>	_dict					= null;
		int								_autoClearCondition		= -1;
		int								_autoClearTargetCount	= -1;



        public CacheAudio(int autoClearCondition, int autoClearTargetCount)
        {
            _dict					= new Dictionary<string, ClipInfo>(autoClearCondition);
			_autoClearCondition		= Mathf.Max(16, autoClearCondition);
			_autoClearTargetCount	= Mathf.Max(8, autoClearTargetCount);
        }



        public bool Cache(string resourcePath, bool preloadData)
        {
			if (null == _dict)
				return false;

			var alreadyCached = false;
			if (_dict.TryGetValue(resourcePath, out var cachedClipInfo))
            {
				if (null != cachedClipInfo)
                {
					if (preloadData)
                    {
						var cachedClip = cachedClipInfo.Clip;
						if (null != cachedClip)
							cachedClip.LoadAudioData();
                    }

					return true;
                }

				alreadyCached = true;
            }

			var clip = Resources.Load<AudioClip>(resourcePath);
			if (null == clip)
            {
				Debug.LogWarning($"[CacheAudio - OnPreloadAudio] Clip({resourcePath}) is null !");
				return false;
            }

			if (preloadData)
				clip.LoadAudioData();

			if (_autoClearCondition - 1 <= _dict.Count)
				ClearAudio_BasedOnPriority(_autoClearTargetCount - 1);

			var clipInfo = new ClipInfo(resourcePath, clip);
			if (alreadyCached)	_dict[resourcePath] = clipInfo;
			else				_dict.Add(resourcePath, clipInfo);

			return true;
        }



		public void ClearAudio_All()
        {
			if (null == _dict)		return;
			if (_dict.Count <= 0)	return;

			foreach (var pair in _dict)
            {
				var clipInfo = pair.Value;
				clipInfo?.Finalized();
            }

			_dict.Clear();
        }

		public void ClearAudio_BasedOnPriority(int targetRemaingCount)
        {
			if (null == _dict)						return;
			if (_dict.Count <= targetRemaingCount)	return;

			var list = new List<ClipInfo>(_dict.Values);
			list.Sort(delegate (ClipInfo x, ClipInfo y)
			{
				if (x.HasValidChain != y.HasValidChain)
                {
					if (x.HasValidChain) return -1;
					if (y.HasValidChain) return 1;
                }

				if (x.Priority < y.Priority)	return 1;
				if (x.Priority > y.Priority)	return -1;
												return 0;
			});

			for (int n = list.Count - 1; 0 <= n; --n)
            {
				var clipInfo = list[n];
				if (null == clipInfo)
					continue;

				if (clipInfo.HasValidChain)				break;
				if (_dict.Count <= targetRemaingCount)	break;

				_dict.Remove(clipInfo.Path);
            }
        }



		public ClipInfo GetClipInfo(string resourcePath, bool preloadData)
		{
			if (!Cache(resourcePath, preloadData))
			{
				Debug.LogWarning($"[CacheAudio - GetClipInfo] Clip({resourcePath}) is null !");
				return null;
			}

			return _dict[resourcePath];
		}
    }
}
