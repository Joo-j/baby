using UnityEngine;

namespace Supercent.Core.Audio
{
    public struct AudioPlayInfo
    {
		AudioSource _source;
		AudioClip	_clip;
		float		_finishAt;



		[System.Obsolete("해당 프로퍼티는 빠른 시일 내에 제거될 예정입니다.")]
		public AudioSource Source => _source;
		public bool IsPlaying
        {
			get
            {
				if (null == _source)		return false;
				if (null == _clip)			return false;
				if (_source.clip != _clip)	return false;
				if (!_source.isPlaying)		return false;
				if (_finishAt < Time.time)	return false;

				return true;
            }	
        }



		public AudioPlayInfo(AudioSource source, float delay)
        {
			_source		= source;
			_clip		= source.clip;

			if (source.loop)	_finishAt = float.MaxValue;
			else				_finishAt = Time.time + _clip.length + delay;
		}

		public void Stop()
        {
			if (!IsPlaying)
				return;

			_source.Stop();
			_source.clip = null;
        }
    }
}