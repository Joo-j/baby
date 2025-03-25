using System.Collections.Generic;
using UnityEngine;

namespace Supercent.Core.Audio
{
    public class ClipInfo
    {
        string                  _path       = null;
        AudioClip               _clip       = null;
        uint                    _priority   = 0;
        LinkedList<AudioSource> _chain      = null;



        public string       Path            => _path;
        public AudioClip    Clip            => _clip;
        public uint         Priority        => _priority;
        public bool         HasValidChain
        {
            get
            {
                UpdateChain();

                if (null == _chain)     return false;
                if (_chain.Count <= 0)  return false;

                return true;
            }
        }



        public ClipInfo(string path, AudioClip clip)
        {
            _path       = path;
            _clip       = clip;
            _priority   = 0;
            _chain     = new LinkedList<AudioSource>();
        }

        public void Finalized()
        {
            _clip?.UnloadAudioData();
            _clip = null;

            _priority = 0;

            _chain?.Clear();
            _chain = null;
        }



        public void Chain(AudioSource source)
        {
            if (null == source)
            {
                Debug.LogWarning($"[ClipInfo - Chain] AudioSource is null !");
                return;
            }

            if (null == _chain)
            {
                Debug.LogWarning($"[ClipInfo - Chain] Linked is null !");
                return;
            }

            UpdateChain();
            _chain.AddLast(source);

            ++_priority;
        }

        public void UpdateChain()
        {
            if (null == _chain)     return;
            if (_chain.Count <= 0)  return;

            var node = _chain.First;
            while (null != node)
            {
                var pickNode    = node;
                node            = node.Next;

                var source = pickNode.Value;
                if (null == source || !source.isPlaying || source.clip != _clip)
                {
                    _chain.Remove(pickNode);
                    continue;
                }
            }
        }
    }
}
