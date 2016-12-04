// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.Timing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace osu.Framework.Audio.Track
{
    public abstract class AudioTrack : AdjustableAudioComponent, IAdjustableClock, IHasCompletedState
    {
        protected List<Dsp> Dsps = new List<Dsp>();

        protected object DspProcessorLock = new object();

        /// <summary>
        /// Is this track capable of producing audio?
        /// </summary>
        public virtual bool IsDummyDevice => true;

        public bool Looping { get; set; }

        /// <summary>
        /// The speed of track playback. Does not affect pitch, but will reduce playback quality due to skipped frames.
        /// </summary>
        public readonly BindableDouble Tempo = new BindableDouble(1);

        protected AudioTrack()
        {
            Tempo.ValueChanged += InvalidateState;
        }

        /// <summary>
        /// Reset this track to a logical default state.
        /// </summary>
        public virtual void Reset()
        {
            Frequency.Value = 1;

            Stop();
            Seek(0);
        }

        /// <summary>
        /// Current position in milliseconds.
        /// </summary>
        public abstract double CurrentTime { get; }

        /// <summary>
        /// Lenth of the track in milliseconds.
        /// </summary>
        public double Length { get; protected set; }

        /// <summary>
        /// Sample rate of the stream
        /// </summary>
        public double SampleRate { get; protected set; }

        public virtual int? Bitrate => null;

        /// <summary>
        /// Seek to a new position.
        /// </summary>
        /// <param name="seek">New position in milliseconds</param>
        /// <returns>Whether the seek was successful.</returns>
        public abstract bool Seek(double seek);

        public abstract void Start();

        public abstract void Stop();

        public void AddDsp(Dsp dsp)
        {
            if(Dsps.Contains(dsp)) throw new InvalidOperationException("DSP added twice");
            if(dsp.Track != null)
                throw new InvalidOperationException("DSP added to more than 1 track at the same time");

            Dsps.Add(dsp);
            dsp.Track = this;
        }

        public void RemoveDsp(Dsp dsp)
        {
            if(!Dsps.Contains(dsp)) throw new InvalidOperationException("DSP not contained on this track");

            lock(DspProcessorLock)
            {
                OnDspRemoved(dsp);
                Dsps.Remove(dsp);
                dsp.Track = null;
            }
        }

        public void ClearDsps()
        {
            foreach(var dsp in Dsps)
            {
                OnDspRemoved(dsp);
                dsp.Track = null;
            }

            Dsps.Clear();
        }

        public abstract bool IsRunning { get; }

        /// <summary>
        /// Overall playback rate (1 is 100%, -1 is reversed at 100%).
        /// </summary>
        public virtual double Rate => Frequency * Tempo;

        public bool IsReversed => Rate < 0;

        public override void Update()
        {
            base.Update();
            if(Looping && !IsRunning && Length == CurrentTime)
            {
                Reset();
                Start();
            }
        }

        public virtual bool HasCompleted => IsDisposed;

        protected virtual void OnDspAdded(Dsp newDsp)
        {
        }

        protected virtual void OnDspRemoved(Dsp newDsp)
        {
        }
    }
}