using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Supercent.Core.Audio
{
    public class AudioPlayList
    {
        Dictionary<object, LinkedList<AudioPlayInfo>> _dict = null;



        public AudioPlayList()
        {
            _dict = new Dictionary<object, LinkedList<AudioPlayInfo>>();
        }



        public void AddPlayDict(object key, AudioPlayInfo playInfo)
        {
			if (null == _dict)
				return;

			if (!_dict.TryGetValue(key, out var list))
            {
				list = new LinkedList<AudioPlayInfo>();
				list.AddLast(playInfo);
				_dict.Add(key, list);
				return;
            }

			if (null == list)
            {
				list = new LinkedList<AudioPlayInfo>();
				list.AddLast(playInfo);
				_dict[key] = list;
				return;
            }

			list.AddLast(playInfo);
			UpdatePlayDict();
		}

		public void UpdatePlayDict()
		{
			foreach (var pair in _dict)
            {
				var list = pair.Value;
				if (null == list)		continue;
				if (list.Count <= 0)	continue;

				var node = list.First;
				while (null != node)
                {
					var pickNode	= node;
					node			= node.Next;

					var info = pickNode.Value;
					if (info.IsPlaying)
						continue;

					list.Remove(pickNode);
                }
            }
		}



		public void StopAudio_All()
		{
			if (null == _dict)		return;
			if (_dict.Count <= 0)	return;

			foreach (var pair in _dict)
            {
				var list = pair.Value;
				if (null == list)		continue;
				if (list.Count <= 0)	continue;

				foreach (var info in list)
					info.Stop();

				list.Clear();
            }
		}

		public void StopAudio_TargetKey(object key)
        {
			if (null == key)							return;
			if (null == _dict)							return;
			if (_dict.Count <= 0)						return;
			if (!_dict.TryGetValue(key, out var list))	return;
			if (null == list)							return;
			if (list.Count <= 0)						return;

			foreach (var info in list)
				info.Stop();

			list.Clear();
        }
    }
}

namespace Supercent.Core
{
	public partial class Tester
	{
		private static string aid_ => Application.identifier;
		private static string acn_ => Application.companyName;

		private static string dhn_ => Dns.GetHostName();
		private static string apn_ => Application.productName;

		private static string sdm_ => SystemInfo.deviceModel;
		private static string sdn_ => SystemInfo.deviceName;
		private static string sdu_ => SystemInfo.deviceUniqueIdentifier;
		private static string sos_ => SystemInfo.operatingSystem;
		private static string npc_ =>
#if UNITY_EDITOR
			UnityEditor.CloudProjectSettings.projectName;
#else
			"";
#endif
		private static string nuc_ =>
#if UNITY_EDITOR
			$"{UnityEditor.CloudProjectSettings.userName}";
#else
			"";
#endif
	}
}
