﻿namespace Macabre2D.Framework.Audio {

    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Content;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// A single audio clip.
    /// </summary>
    [DataContract]
    public sealed class AudioClip : IDisposable, IIdentifiable {
        private bool _disposedValue = false;

        [DataMember]
        private Guid _id = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioClip"/> class.
        /// </summary>
        public AudioClip() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioClip"/> class.
        /// </summary>
        /// <param name="contentPath">The content path.</param>
        /// <param name="contentManager">The content manager.</param>
        /// <param name="volume">The volume.</param>
        /// <param name="pan">The pan.</param>
        /// <param name="pitch">The pitch.</param>
        public AudioClip(string contentPath, ContentManager contentManager, float volume, float pan, float pitch) {
            this.ContentPath = contentPath;
            this.LoadSoundEffect(contentManager, volume, pan, pitch);
        }

        /// <summary>
        /// Gets or sets the content path.
        /// </summary>
        /// <value>The content path.</value>
        [DataMember]
        public string ContentPath { get; set; }

        /// <inheritdoc/>
        public Guid Id {
            get {
                return this._id;
            }
        }

        /// <summary>
        /// Gets or sets the sound effect instance.
        /// </summary>
        /// <value>The sound effect instance.</value>
        internal SoundEffectInstance SoundEffectInstance { get; set; }

        /// <inheritdoc/>
        public void Dispose() {
            this.Dispose(true);
        }

        /// <summary>
        /// Loads the sound effect.
        /// </summary>
        /// <param name="contentManager">The content manager.</param>
        /// <param name="volume">The volume.</param>
        /// <param name="pan">The pan.</param>
        /// <param name="pitch">The pitch.</param>
        public void LoadSoundEffect(ContentManager contentManager, float volume, float pan, float pitch) {
            var soundEffect = contentManager.Load<SoundEffect>(this.ContentPath);

            if (soundEffect != null) {
                this.SoundEffectInstance = soundEffect.CreateInstance();
                this.SoundEffectInstance.Volume = volume;
                this.SoundEffectInstance.Pan = pan;
                this.SoundEffectInstance.Pitch = pitch;
            }
        }

        private void Dispose(bool disposing) {
            if (!this._disposedValue) {
                if (disposing) {
                    this.SoundEffectInstance.Dispose();
                }

                this._disposedValue = true;
            }
        }
    }
}