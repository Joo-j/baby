using UnityEngine;

namespace Supercent.Core.Audio
{
    public struct PlayOption3D
    {
		public static readonly PlayOption3D Empty = default;



		public Vector3			WorldPosition;
		public float			SpatialBlend;
		public float			DopplerLevel;
		public int				Spread;
		public AudioRolloffMode RolloffMode;
		public float			MinDistance;
		public float			MaxDistance;



		public static PlayOption3D GenerateDefaultOption()
        {
			var option				= new PlayOption3D();
			option.WorldPosition	= Vector3.zero;
			option.SpatialBlend		= 0.0f;
			option.DopplerLevel		= 1.0f;
			option.Spread			= 0;
			option.RolloffMode		= AudioRolloffMode.Logarithmic;
			option.MinDistance		= 1.0f;
			option.MaxDistance		= 500.0f;

			return option;
        }



		public bool ApplyOption(AudioSource source)
        {
			if (null == source)
				return false;

			source.transform.position	= WorldPosition;
			source.spatialBlend			= SpatialBlend;
			source.dopplerLevel			= Mathf.Clamp(DopplerLevel, 0, 5);
			source.spread				= Mathf.Clamp(Spread, 0, 360);
			source.rolloffMode			= RolloffMode;
			source.minDistance			= MinDistance;
			source.maxDistance			= MaxDistance;
			return true;
        }



        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode()		=> base.GetHashCode();
        public static bool operator ==(PlayOption3D x, PlayOption3D y)
        {
			if (x.WorldPosition != y.WorldPosition) return false;
			if (x.SpatialBlend	!= y.SpatialBlend)	return false;
			if (x.DopplerLevel	!= y.DopplerLevel)	return false;
			if (x.Spread		!= y.Spread)		return false;
			if (x.RolloffMode	!= y.RolloffMode)	return false;
			if (x.MinDistance	!= y.MinDistance)	return false;
			if (x.MaxDistance	!= y.MaxDistance)	return false;

			return true;
		}
		public static bool operator !=(PlayOption3D x, PlayOption3D y)
        {
			if (x.WorldPosition != y.WorldPosition) return true;
			if (x.SpatialBlend	!= y.SpatialBlend)	return true;
			if (x.DopplerLevel	!= y.DopplerLevel)	return true;
			if (x.Spread		!= y.Spread)		return true;
			if (x.RolloffMode	!= y.RolloffMode)	return true;
			if (x.MinDistance	!= y.MinDistance)	return true;
			if (x.MaxDistance	!= y.MaxDistance)	return true;

			return false;
		}
    }
}
