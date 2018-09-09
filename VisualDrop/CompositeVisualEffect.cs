using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDrop
{
    [Serializable]
    public class CompositeVisualEffect : IVisualEffect
    {
        private readonly IVisualEffect[] _composedEffects;

        public CompositeVisualEffect(int numberOfEffects)
        {
            _composedEffects = new IVisualEffect[numberOfEffects];
        }

        public void SetEffect(int index, IVisualEffect effect)
        {
            _composedEffects[index] = effect;
        }

        public byte[] GetEffect()
        {
            if (_composedEffects == null) return null;
            var effects = new List<byte[]>();
            var largestEffectSize = 0;
            foreach (var composedEffect in _composedEffects)
            {
                if (composedEffect == null) continue;
                var effect = composedEffect.GetEffect();
                if (largestEffectSize < effect.Length) largestEffectSize = effect.Length;
                effects.Add(effect);
            }
            var composedGraphic = new byte[largestEffectSize];

            foreach (var effect in effects)
            {
                for (int i = 0; i < effect.Length; i++)
                {
                    var a = composedGraphic[i];
                    var b = effect[i];
                    composedGraphic[i] = Math.Max(a, b);
                }
            }

            return composedGraphic;
        }
    }
}
