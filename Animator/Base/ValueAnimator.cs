using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BX
{
    /// <summary>
    /// Animates a value without holding grudges to the animated values or it's initial values.
    /// <br>Only sets the values of an animation while <see cref="IsPlaying"/>.</br>
    /// </summary>
    /// <typeparam name="TValue">The type of value to animate. This is often preferred to be a value of a target component.</typeparam>
    public abstract class ValueAnimator<TValue> : ValueAnimatorBase
    {
        /// <summary>
        /// A value animation sequence used to animate values.
        /// </summary>
        [Serializable]
        public class ValueAnim : Sequence
        {
            public override int FrameCount => valueFrames == null ? 0 : valueFrames.Length;

            /// <summary>
            /// Animation frames of given animator.
            /// </summary>
            public TValue[] valueFrames;
            /// <summary>
            /// Get a value at index shorthand.
            /// </summary>
            public TValue this[int index]
            {
                get => valueFrames[index];
                set => valueFrames[index] = value;
            }

            public override void Clear()
            {
                valueFrames = new TValue[0];
            }

            public override void Reverse()
            {
                Array.Reverse(valueFrames);
            }
        }

        // -- Settings
        [Header(":: Animation")]
        [SerializeField] protected int m_CurrentAnimIndex = 0;
        /// <summary>
        /// Index of the current animation assigned to be played.
        /// </summary>
        public override int CurrentAnimIndex
        {
            get { return animations.Length <= 0 ? -1 : Mathf.Clamp(m_CurrentAnimIndex, 0, animations.Length - 1); }
            set { m_CurrentAnimIndex = Mathf.Clamp(value, 0, animations.Length - 1); }
        }
        public override int AnimationCount => animations.Length;
        /// <summary>
        /// The current animation assigned to be played.
        /// </summary>
        public ValueAnim CurrentAnimation
        {
            get
            {
                // Index property being lower than 0 = No animations
                if (CurrentAnimIndex < 0)
                {
                    return null;
                }

                return animations[CurrentAnimIndex];
            }
        }
        /// <summary>
        /// List of the contained animations.
        /// </summary>
        public ValueAnim[] animations = new ValueAnim[1];
        /// <summary>
        /// Plays animation on <c>Start()</c>.
        /// </summary>
        public bool playOnStart = false;
        /// <summary>
        /// Determines which update will the animation will be played on.
        /// </summary>
        public TickMode animUpdateMode = TickMode.Update;
        /// <summary>
        /// <br>Animates the sprite independent of the <see cref="Time.timeScale"/>, using the unscaled delta times.</br>
        /// </summary>
        public bool ignoreTimeScale = false;

        // -- State
        /// <summary>
        /// Whether if the animation is playing.
        /// </summary>
        public bool IsPlaying { get; private set; } = false;
        /// <summary>
        /// Returns whether if '<see cref="GatherInitialValue"/>' was called once atleast.
        /// <br>Used to validate <see cref="initialValue"/> if there's none.</br>
        /// </summary>
        public bool IsInitialized { get; private set; } = false;
        /// <summary>
        /// Current frame of the animation.
        /// </summary>
        public int CurrentFrame { get; private set; } = 0;
        /// <summary>
        /// Current animation timer.
        /// <br>Tick this if you want to completely change the <see cref="UpdateAnimator(float)"/> behaviour.</br>
        /// </summary>
        protected float m_timer;
        /// <summary>
        /// The starting value for the animation.
        /// <br>The <see cref="Value"/> is set to this when the animation is <see cref="Stop"/></br>
        /// </summary> 
        public TValue initialValue;

        /// <summary>
        /// Returns whether if the initial value is null.
        /// <br>This is only needed internally as unity objects don't work with normal null comparison,
        /// but the other scripts accessing this class can use the <typeparamref name="TValue"/>'s equality comparer.</br>
        /// </summary>
        protected virtual bool InitialValueIsNull
        {
            get
            {
                if (initialValue is Object initialUnityObject)
                {
                    return initialUnityObject == null;
                }

                // Normal object comparison
                return initialValue == null;
            }
        }

        // -- Abstract Class
        /// <summary>
        /// The value that is going to be animated by the inheriting type <see cref="ValueAnim"/>.
        /// </summary>
        public abstract TValue Value { get; protected set; }

        private void Start()
        {
            GatherInitialValue();

            if (animations.Length < 0) // Array to play is invalid.
            {
                Debug.LogWarning($"[ValueAnimator::Start] Cannot start animation : there is no animations on object '{name}'.");
                enabled = false;
            }
            else if (playOnStart) // Array to play is valid, allow play on start.
            {
                Play();
            }
        }
        /// <summary>
        /// Gathers the <see cref="initialValue"/> variable from existing <see cref="Value"/>.
        /// </summary>
        public void GatherInitialValue()
        {
            initialValue = Value;
            IsInitialized = true;
        }

        protected virtual void Update()
        {
            if (!IsPlaying || animUpdateMode != TickMode.Update)
            {
                return;
            }

            UpdateAnimator(ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
        }
        protected virtual void FixedUpdate()
        {
            if (!IsPlaying || animUpdateMode != TickMode.FixedUpdate)
            {
                return;
            }

            UpdateAnimator(ignoreTimeScale ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime);
        }
        protected virtual void LateUpdate()
        {
            if (!IsPlaying || animUpdateMode != TickMode.LateUpdate)
            {
                return;
            }

            UpdateAnimator(ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
        }

        protected bool finished = false;
        /// <summary>
        /// Updates the animator itself depending on the settings.
        /// </summary>
        protected virtual void UpdateAnimator(float deltaTime)
        {
            if (CurrentAnimation == null)
            {
                Debug.LogError($"[ValueAnimator::UpdateAnimator] Current animation on \"{name}\" is null, stopping.");
                Stop();
                return;
            }

            m_timer += deltaTime;

            var frameMS = CurrentAnimation.frameMS;
            var loop = CurrentAnimation.loop;

            if (frameMS <= 0f)
            {
                return;
            }

            // Lower timer + increment animation
            ValueAnim anim = CurrentAnimation;
            while (m_timer >= frameMS)
            {
                m_timer -= frameMS;

                // CurrentFrame starts from 0.
                // We also want to show the first frame.                
                Value = anim[CurrentFrame];

                if (!anim.iterateReverse)
                {
                    if (loop)
                    {
                        // Animation is forever looping
                        if (anim.loopMode == LoopMode.PingPong)
                        {
                            anim.iterateReverse = CurrentFrame == anim.valueFrames.Length - 2; // next frame
                        }

                        CurrentFrame = (CurrentFrame + 1) % anim.valueFrames.Length;
                    }
                    else
                    {
                        // Check the last frame
                        int currentFrameSet = CurrentFrame + 1;
                        CurrentFrame = Mathf.Clamp(currentFrameSet, 0, anim.valueFrames.Length - 1);

                        // Do a psuedo-stop if the last frame
                        if (CurrentFrame != currentFrameSet)
                        {
                            // Don't call 'Stop()' here to keep the sprite on the last one.
                            // Calling stop will reset to the initial frame.
                            IsPlaying = false;
                            finished = true;
                            return;
                        }
                    }
                }
                else
                {
                    if (loop)
                    {
                        // Animation is forever looping
                        // Two-way wrap
                        if (anim.loopMode == LoopMode.PingPong)
                        {
                            anim.iterateReverse = CurrentFrame != 1; // next frame
                        }
                        CurrentFrame = (((CurrentFrame - 1) % anim.valueFrames.Length) + anim.valueFrames.Length) % anim.valueFrames.Length;
                    }
                    else
                    {
                        int currentFrameSet = CurrentFrame - 1;
                        CurrentFrame = Mathf.Clamp(currentFrameSet, 0, anim.valueFrames.Length - 1);

                        if (CurrentFrame != currentFrameSet)
                        {
                            IsPlaying = false;
                            finished = true;
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get an animation sequence index from ID.
        /// <br>Returns -1 if the animation with <paramref name="id"/> doesn't exist.</br>
        /// </summary>
        public int GetIndexFromID(string id)
        {
            for (int i = 0; i < animations.Length; i++)
            {
                if (animations[i].name.Equals(id, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }
        /// <summary>
        /// Get whether if the <paramref name="id"/> exists in the list of animations to play.
        /// </summary>
        public bool IDExists(string id)
        {
            return GetIndexFromID(id) >= 0;
        }
        /// <summary>
        /// Plays the <see cref="CurrentAnimation"/>.
        /// </summary>
        public override void Play()
        {
            if (IsPlaying)
            {
                Stop();
            }
            else if (!finished)
            {
                // This means that we aren't stuck on the animation value & not playing
                // We can safely gather the initial value without it being a initial value of the given animation
                GatherInitialValue();
            }

            CurrentFrame = 0;
            IsPlaying = true;
            finished = false;

            // Immediately set the value to the first
            Value = CurrentAnimation[CurrentFrame];
        }
        /// <summary>
        /// Plays the <see cref="ValueAnim"/> in <see cref="animations"/> with matching id.
        /// </summary>
        /// <param name="id">ID of the animation to play. This should exist in the list.</param>
        /// <exception cref="ArgumentException"></exception>
        public override void Play(string id)
        {
            if (IsPlaying)
            {
                // Update
                Stop();
            }
            else if (!finished)
            {
                GatherInitialValue();
            }

            // Find sequentially as animations are not sorted.
            int animIndex = GetIndexFromID(id);

            if (animIndex < 0)
            {
                throw new ArgumentException($"[ValueAnimator::Play] Failed to play animation with id '{id}'.", nameof(id));
            }
            m_CurrentAnimIndex = animIndex;

            CurrentFrame = 0;
            IsPlaying = true;
            finished = false;

            // Immediately set the value to the first
            Value = CurrentAnimation[CurrentFrame];
        }
        /// <summary>
        /// Stops the animation while keeping <see cref="CurrentFrame"/> in it's place.
        /// </summary>
        public override void Pause()
        {
            IsPlaying = false;
        }
        /// <summary>
        /// Stops the animation &amp; resets everything.
        /// </summary>
        public override void Stop()
        {
            // Reset state
            IsPlaying = false;
            finished = false;
            CurrentFrame = 0;
            CurrentAnimation.iterateReverse = false;

            // Set value to initial (if initialized)
            if (InitialValueIsNull)
            {
                if (!IsInitialized)
                {
                    GatherInitialValue();
                }
            }

            Value = initialValue;
        }
    }
}
