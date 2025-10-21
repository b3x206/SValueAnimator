using UnityEngine;

namespace BX
{
    /// <summary>
    /// Animates a <see cref="SpriteRenderer"/>'s sprite property.
    /// </summary>
    public sealed class SpriteRendererAnimator : ValueAnimator<Sprite>
    {
        [Space]
        public SpriteRenderer targetRenderer;
        public override Sprite Value
        {
            get
            {
                if (targetRenderer == null)
                {
                    TryGetComponent(out targetRenderer);
                }

                return targetRenderer.sprite;
            }
            protected set
            {
                if (targetRenderer == null)
                {
                    TryGetComponent(out targetRenderer);
                }

                targetRenderer.sprite = value;
            }
        }
    }
}
