using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Supercent.Core.Audio
{
	public class AudioManager : MonoBehaviour
	{
		static AudioManager _instance = null;



		AudioListener		_listener							= null;
		AudioSource			_bgm								= null;
		List<AudioSource>	_sfxPool							= null;
		CacheAudio			_cachedAudio						= null;
		AudioPlayList		_playList							= null;
		Coroutine			_coroutinePlayBGMList				= null;
		Coroutine			_coroutineListenerAutoPositioning	= null;
		int					_lastPlayedIndex					= -1;
		
		static AudioManager Instance
		{
			get
			{
				if (null != _instance)
					return _instance;

				Init();
				return _instance;
			}
		}

		public static bool IsMuteBGM
		{
			set
			{
				PlayerPrefs.SetInt("IsMuteBGM", value ? 1 : 0);
				Instance.SetMute(true, value);
			}
			get => PlayerPrefs.GetInt("IsMuteBGM", 0) == 1;
		}

		public static bool IsMuteSFX
		{
			set
			{
				PlayerPrefs.SetInt("IsMuteSFX", value ? 1 : 0);
				Instance.SetMute(false, value);
			}
			get => PlayerPrefs.GetInt("IsMuteSFX", 0) == 1;
		}

		public static bool IsPlayingBGM
        {
            get
            {
				var source = Instance._bgm;
				if (null == source)
					return false;

				return source.isPlaying;
            }
        }

		public static void Init(int sfxPoolingCount = 64, int autoClearCondition = 64, int autoClearTargetCount = 32)
		{
			if (null != _instance)
				return;
			
			_instance = new GameObject("AudioManager").AddComponent<AudioManager>();
			_instance.OnInit(sfxPoolingCount, autoClearCondition, autoClearTargetCount);
			DontDestroyOnLoad(_instance.gameObject);
		}
		void OnInit(int sfxPoolingCount, int autoClearCondition, int autoClearTargetCount)
        {
			_listener					= new GameObject("Listener").AddComponent<AudioListener>();
			_listener.transform.parent	= transform;

			_bgm					= new GameObject("BGM").AddComponent<AudioSource>();
			_bgm.transform.parent	= transform;
			_bgm.spatialBlend		= 0.0f;
			_bgm.priority			= 200;
			_bgm.playOnAwake		= false;

			var sfxRoot		= new GameObject("SFXPool").transform;
			_sfxPool		= new List<AudioSource>();
			sfxRoot.parent	= transform;
			for (int n = 0, cnt = sfxPoolingCount; n < cnt; ++n)
            {
				var sfx					= new GameObject($"SFX_{n:00}").AddComponent<AudioSource>();
				sfx.transform.parent	= sfxRoot;
				sfx.playOnAwake			= false;

				_sfxPool.Add(sfx);
            }

			_cachedAudio		= new CacheAudio(autoClearCondition, autoClearTargetCount);
			_playList			= new AudioPlayList();
			_lastPlayedIndex	= -1;

			if (IsMuteBGM) SetMute(true, true);
			if (IsMuteSFX) SetMute(false, true);
		}

		public static bool CacheAudio(string resourcePath, bool preloadData) => Instance._cachedAudio?.Cache(resourcePath, preloadData) ?? false;

		public static void ClearAudio_All()
		{
			StopAudio_All();
			Instance._cachedAudio?.ClearAudio_All();
		}

		public static void ClearAudio_BasedOnPriority(int targetRemaingCount) => Instance._cachedAudio?.ClearAudio_BasedOnPriority(targetRemaingCount);

		public static AudioPlayInfo PlayBGM(string resourcePath, float volume = 1.0f, float pitch = 1.0f, bool loop = true, float delay = 0.0f, bool force = false)
        {
			return Instance.OnPlayBGM(resourcePath, volume, pitch, loop, delay, force);
        }
		AudioPlayInfo OnPlayBGM(string resourcePath, float volume, float pitch, bool loop, float delay, bool force)
        {
			OnStopBGMList();

			var clipInfo = _cachedAudio.GetClipInfo(resourcePath, false);
			if (null == clipInfo)
				return default;

			var playSource = OnPlayBGM_Internal(clipInfo.Clip, volume, pitch, loop, delay, force);;
			if (null == playSource)
				return default;

			clipInfo.Chain(playSource);
			return GeneratePlayInfo(null, playSource, delay);
        }
		public static AudioPlayInfo PlayBGM(AudioClip clip, float volume = 1.0f, float pitch = 1.0f, bool loop = true, float delay = 0.0f, bool force = false)
        {
			return Instance.OnPlayBGM(clip, volume, pitch, loop, delay, force);
        }
		AudioPlayInfo OnPlayBGM(AudioClip clip, float volume, float pitch, bool loop, float delay, bool force)
        {
			OnStopBGMList();

			var playSource = OnPlayBGM_Internal(clip, volume, pitch, loop, delay, force);;
			if (null == playSource)
				return default;

			return GeneratePlayInfo(null, playSource, delay);
        }
		AudioSource OnPlayBGM_Internal(AudioClip clip, float volume, float pitch, bool loop, float delay, bool force)
		{
			if (null == clip)
				return null;

			var source = GetSource(true);
			if (null == source)
				return null;

			source.volume	= volume;
			source.pitch	= pitch;
			source.loop		= loop;

			if (source.isPlaying)
			{
				if (source.clip != clip || force)
				{
					source.Stop();
					source.clip = clip;
				}
				else
					return null;
			}
			else
				source.clip = clip;


			if (0.0f < delay)	source.PlayDelayed(delay);
			else				source.Play();

			return source;
		}
		AudioPlayInfo GeneratePlayInfo(object key, AudioSource source, float delay)
		{
			var playInfo = new AudioPlayInfo(source, delay);
			if (null != _playList)
			{
				if (null == key)	_playList.AddPlayDict(typeof(AudioManager), playInfo);
				else				_playList.AddPlayDict(key, playInfo);
			}

			return playInfo;
		}


		public static void StopBGMList() => Instance.OnStopBGMList();

		void OnStopBGMList()
        {
			if (null != _coroutinePlayBGMList)
			{
				StopCoroutine(_coroutinePlayBGMList);
				_coroutinePlayBGMList = null;
			}

			if (null != _bgm)
				_bgm.Stop();
		}

		public static AudioPlayInfo PlayBGMList(List<string> resourcePath, float volume = 1.0f, float pitch = 1.0f, bool loop = true)
		{
			return Instance.OnPlayBGMList(resourcePath, volume, pitch, loop);
		}
		AudioPlayInfo OnPlayBGMList(List<string> resourcePath, float volume, float pitch, bool loop)
        {
			if (null != _coroutinePlayBGMList)
				StopCoroutine(_coroutinePlayBGMList);

			_coroutinePlayBGMList = StartCoroutine(CoroutinePlayBGMList(resourcePath, volume, pitch, loop));

			var playInfo = new AudioPlayInfo(_bgm, 0);
			if (null != _playList)
				_playList.AddPlayDict(typeof(AudioManager), playInfo);

			return playInfo;
        }
		IEnumerator CoroutinePlayBGMList(List<string> resourcePath, float volume, float pitch, bool loop)
        {
			var list = new List<ClipInfo>();
			for (int n = 0, cnt = resourcePath.Count; n < cnt; ++n)
            {
				var path = resourcePath[n];
				if (string.IsNullOrEmpty(path))
					continue;

				var clipInfo = _cachedAudio.GetClipInfo(path, false);
				if (null == clipInfo)
					continue;

				list.Add(clipInfo);
			}

			if (list.Count <= 0)
            {
				_coroutinePlayBGMList = null;
				yield break;
            }

			var index 		= 0;
			var missCount 	= 0;
			while (index < list.Count)
            {
				var info	= list[index];
				var source	= OnPlayBGM_Internal(info.Clip, volume, pitch, false, 0.0f, true);

				++index;
				if (loop)
                {
					if (list.Count <= index)
					{
						index 		= 0;
						missCount 	= 0;
					}
                }

				if (null == source)
				{
					++missCount;
					if (list.Count <= missCount)
						break;

					continue;
				}

				while (source.isPlaying)
					yield return null;

				yield return null;
			}

			_coroutinePlayBGMList = null;
        }




		public static AudioPlayInfo PlaySFX(string resourcePath, float volume = 1.0f, float pitch = 1.0f, bool loop = false, float delay = 0.0f, PlayOption3D option3D = default)
        {
			return Instance.OnPlaySFX(null, resourcePath, volume, pitch, loop, delay, option3D);
		}

		public static AudioPlayInfo PlaySFX(object key, string resourcePath, float volume = 1.0f, float pitch = 1.0f, bool loop = false, float delay = 0.0f, PlayOption3D option3D = default)
		{
			return Instance.OnPlaySFX(key, resourcePath, volume, pitch, loop, delay, option3D);
		}
		AudioPlayInfo OnPlaySFX(object key, string resourcePath, float volume, float pitch, bool loop, float delay, PlayOption3D option3D)
        {
			var clipInfo = _cachedAudio.GetClipInfo(resourcePath, false);
			if (null == clipInfo)
				return default;

			var playSource = OnPlaySFX_Internal(clipInfo.Clip, volume, pitch, loop, delay, option3D);
			if (null == playSource)
				return default;

			clipInfo.Chain(playSource);
			return GeneratePlayInfo(key, playSource, delay);
        }
		public static AudioPlayInfo PlaySFX(AudioClip clip, float volume = 1.0f, float pitch = 1.0f, bool loop = false, float delay = 0.0f, PlayOption3D option3D = default)
		{
			return Instance.OnPlaySFX(null, clip, volume, pitch, loop, delay, option3D);
		}
		public static AudioPlayInfo PlaySFX(object key, AudioClip clip, float volume = 1.0f, float pitch = 1.0f, bool loop = false, float delay = 0.0f, PlayOption3D option3D = default)
		{
			return Instance.OnPlaySFX(key, clip, volume, pitch, loop, delay, option3D);
		}
		AudioPlayInfo OnPlaySFX(object key, AudioClip clip, float volume, float pitch, bool loop, float delay, PlayOption3D option3D)
        {
			var playSource = OnPlaySFX_Internal(clip, volume, pitch, loop, delay, option3D);
			if (null == playSource)
				return default;

			return GeneratePlayInfo(key, playSource, delay);
        }
		AudioSource OnPlaySFX_Internal(AudioClip clip, float volume, float pitch, bool loop, float delay, PlayOption3D option3D)
		{
			if (null == clip)
				return null;

			var source = GetSource(false);
			if (null == source)
				return null;

			source.clip		= clip;
			source.volume	= volume;
			source.pitch	= pitch;
			source.loop		= loop;

			if (option3D == PlayOption3D.Empty)
				option3D = PlayOption3D.GenerateDefaultOption();

			option3D.ApplyOption(source);

			if (0.0f < delay)	source.PlayDelayed(delay);
			else				source.Play();

			return source;
		}



		AudioSource GetSource(bool isBGM)
        {
			if (isBGM)
				return _bgm;

			var poolCount	= _sfxPool.Count;
			var tryCount	= poolCount;
			var pickIndex	= _lastPlayedIndex;
			while (0 < tryCount)
            {
				--tryCount;
				++pickIndex;

					 if (pickIndex < 0)				pickIndex = 0;
				else if (poolCount <= pickIndex)	pickIndex = 0;

				var source = _sfxPool[pickIndex];
				if (null == source)		continue;
				if (source.isPlaying)	continue;

				_lastPlayedIndex = pickIndex;
				return source;
            }

			Debug.LogWarning($"[AudioManager - GetSource] Need more SFX's Source !");
			return null;
        }



		public static void StopAudio_All()							=> Instance._playList?.StopAudio_All();
		public static void StopAudio_TargetKey(object key)			=> Instance._playList?.StopAudio_TargetKey(key);
		public static void StopAudio_PlayInfo(AudioPlayInfo info) 	=> info.Stop();



		void SetMute(bool isBGM, bool mute)
        {
			if (isBGM)
            {
				if (null == _bgm)
					return;

				_bgm.mute = mute;
            }
			else
            {
				if (null == _sfxPool)		return;
				if (_sfxPool.Count <= 0)	return;

				for (int n = 0, cnt = _sfxPool.Count; n < cnt; ++n)
                {
					var source = _sfxPool[n];
					if (null == source)
						continue;

					source.mute = mute;
                }
            }
        }



		public static void SetListener_Position(Vector3 worldPosition) => Instance.OnSetListener_Position(worldPosition);
		void OnSetListener_Position(Vector3 worldPosition)
        {
			if (null == _listener)
				return;

			_listener.transform.position = worldPosition;
        }

		public static void SetListener_AutoPositioning(Transform target) => Instance.OnSetListener_AutoPositioning(target);
		void OnSetListener_AutoPositioning(Transform target)
        {
			if (null != _coroutineListenerAutoPositioning)
				StopCoroutine(_coroutineListenerAutoPositioning);

			_coroutineListenerAutoPositioning = StartCoroutine(CoroutineListener_AutoPositioning(target));
        }
		IEnumerator CoroutineListener_AutoPositioning(Transform target)
        {
			if (null == _listener)
            {
				Debug.LogWarning($"[AudioManager - CoroutineListener_AutoPositioning] Listener is null !");
				_coroutineListenerAutoPositioning = null;
				yield break;
			}

			if (null == target)
			{
				Debug.LogWarning($"[AudioManager - CoroutineListener_AutoPositioning] Target is null !");
				_coroutineListenerAutoPositioning = null;
				yield break;
			}


			var listenerTransform = _listener.transform;
			while (null != target && null != listenerTransform)
            {
				listenerTransform.position = target.position;
				yield return null;
            }

			_coroutineListenerAutoPositioning = null;
        }

		public static void SetSfxMixer(AudioMixerGroup mixerGroup)
		{
			if (_instance == null)
			{
				Debug.LogWarning($"[AudioManager - {nameof(SetSfxMixer)}] _instance is null !");
				return ;
			}

			foreach(var source  in _instance._sfxPool)
			{
				source.outputAudioMixerGroup = mixerGroup;
			}
		}
	}
}
