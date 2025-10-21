# (simple) ValueAnimator

Small utility code for setting a sequence of values to given component.

Useful for simple sprite animations without having to use Unity Engine's mecanim system.

It can technically animate any value, though it is primitive and doesn't support interpolation / most things.

## Usage

You can install through UPM (not recommended as this package lacks asmdef files) <sup>(Window &gt; Package Manager &gt; Click `+` icon on top left &gt; Add from Git URL)</sup><br/>
Or through the [Unity Package](https://github.com/b3x206/SValueAnimator/releases) release.

Add one the premade components (either `BX.SpriteRendererAnimator` or `BX.ImageAnimator`) given to your to any GameObject, the component will look like this: <br/>
![Component Image](https://github.com/b3x206/SValueAnimator/blob/resource/resource/component-view.png?raw=true)

* `Current Animation Index` = This sets which animation to play within the `Animations` list.
* `Animations` = List of sequenced values to set for sprite.
  * `Total Duration` = The length the sequence will take to iterate over every frame.
  * `Clear Frames` = Removes all values in the `Value Frames`
  * `Reverse Frames` = Reverses all values in the `Value Frames` <br/>
    You can optionally use the ReorderableList view to manage frames as well.
  * `Name` = The `id` parameter that `BX.ValueAnimator<TValue>.Play(string id)` uses. Put names to your animations.
  * `Frame MS` = How long a frame will take?
  * `Loop` = Whether to loop this animation. If this is enabled, the animation will not stop.
    * `Loop Mode` = When the `Loop` is enabled, this picks the looping mode. Ping Pong goes in reverse for the next iteration while Reset wraps around. This is useful to reduce the amount of frames.
  * `Value Frames` = References of "frame" in this animation sequence. Each value is iterated and waited for `Frame MS`.
* `Play On Start` = Calls `BX.ValueAnimator<TValue>.Play()` with the `Current Animation Index` for starting.
* `Anim Update Mode` = Select which Unity Engine callback to update frames.
* `Ignore Time Scale` = Ignores `Time.timeScale` and uses unscaled deltatime.
* `Initial Value` = When the animation is stopped, this will be the value set for that state. You can set this to another value or leave it null.
* `Target Renderer` = Target object to change it's value according to the sequence.

Here's how SpriteRenderer based animator looks like : <br/>
![SpriteRenderer](https://github.com/b3x206/SValueAnimator/blob/resource/resource/demo1.gif?raw=true)

---

To implement a custom component, all you have to do is create a MonoBehaviour script, then inherit from the `BX.ValueAnimator<TValue>` class with the target values you would like to cycle:
```cs
using BX;
using UnityEngine;

public class MaterialSwitchAnimator : ValueAnimator<Material>
{
    [Space]
    public Renderer targetRenderer;

    public override Material Value
    {
        get
        {
            if (targetRenderer == null)
            {
                TryGetComponent(out targetRenderer);
            }

            return targetRenderer.material;
        }
        protected set
        {
            if (targetRenderer == null)
            {
                TryGetComponent(out targetRenderer);
            }

            targetRenderer.material = value;
        }
    }
}
```
and that's it. Now you can switch materials like you switch `Sprite` or something.

To play a sequence with name:
```cs
using BX;
using UnityEngine;

public class SampleScript : MonoBehaviour
{
    public SpriteRendererAnimator anim;
    
    private void Start()
    {
        anim.Play("Foo");
    }
}
```

---

The ValueAnimator only works in Play Mode, for the time being.

You can use this code to create a base for a more complex but simpler animator component, or you can use it for simple animations like in the demo.

If you want a more capable animation system (that is _generally code driven however_), [check out BXSTween instead.](https://github.com/b3x206/BXSTween)
