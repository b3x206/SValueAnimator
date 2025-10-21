using UnityEngine;
using UnityEngine.UI;

namespace BX
{
    /// <summary>
    /// Animates an <see cref="Image"/>'s sprite property.
    /// </summary>
    public sealed class ImageAnimator : ValueAnimator<Sprite>
    {
        [Space]
        public Image targetImage;
        public override Sprite Value
        {
            get
            {
                if (targetImage == null)
                {
                    TryGetComponent(out targetImage);
                }

                return targetImage.sprite;
            }
            protected set
            {
                if (targetImage == null)
                {
                    TryGetComponent(out targetImage);
                }

                targetImage.sprite = value;
            }
        }
    }
}
