using System;
using System.ComponentModel;
using UnityEngine;

namespace BX
{
    /// <summary>
    /// Contains a base for a value animator.
    /// <br>Used for matching the inspector and containing the playing methods.</br>
    /// </summary>
    public abstract class ValueAnimatorBase : MonoBehaviour
    {
        /// <summary>
        /// Corresponds to any of the unity callbacks that are 
        /// called each frame or each physics process tick.
        /// </summary>
        public enum TickMode
        {
            Update, FixedUpdate, LateUpdate
        }

        public enum LoopMode
        {
            /// <summary>
            /// Starts value from the first.
            /// </summary>
            Reset,
            /// <summary>
            /// Reverse iterates on the next loop.
            /// </summary>
            PingPong
        }

        /// <summary>
        /// Defines a sequence of values.
        /// </summary>
        [Serializable]
        public abstract class Sequence
        {
            /// <summary>
            /// Whether to iterate this sequence in reverse.
            /// <br>Used with <see cref="LoopMode.PingPong"/></br>
            /// </summary>
            [NonSerialized]
            [EditorBrowsable(EditorBrowsableState.Never)]
            protected internal bool iterateReverse = false;

            /// <summary>
            /// Name of the animation.
            /// </summary>
            public string name = "None";
            /// <summary>
            /// Milliseconds to wait to update the frame.
            /// <br>This also changes the speed.</br>
            /// </summary>
            public float frameMS = 0.040f; // Default value is 25 fps
            /// <summary>
            /// Whether if the animation should loop.
            /// </summary>
            public bool loop = false;
            /// <summary>
            /// Loop mode to pick.
            /// </summary>
            public LoopMode loopMode = LoopMode.PingPong;

            /// <summary>
            /// Size of the frames of this sequence.
            /// </summary>
            public abstract int FrameCount { get; }
            /// <summary>
            /// Duration of this sequence.
            /// </summary>
            public virtual float Duration => FrameCount * frameMS;

            /// <summary>
            /// Clears all sprites in sequence.
            /// </summary>
            public abstract void Clear();

            /// <summary>
            /// Reverses all the sprites in sequence.
            /// </summary>
            public abstract void Reverse();
        }

        /// <summary>
        /// Currently selected animation's index.
        /// </summary>
        public abstract int CurrentAnimIndex { get; set; }
        /// <summary>
        /// Size of the animations.
        /// </summary>
        public abstract int AnimationCount { get; }

        /// <summary>
        /// Plays the current contained animation.
        /// </summary>
        public abstract void Play();
        /// <summary>
        /// Plays the animation with given <paramref name="id"/>.
        /// </summary>
        public abstract void Play(string id);
        /// <summary>
        /// Pauses the currently playing animation.
        /// </summary>
        public abstract void Pause();
        /// <summary>
        /// Stops the currently playing animation.
        /// </summary>
        public abstract void Stop();
    }
}
