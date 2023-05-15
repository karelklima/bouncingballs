using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;

using NAudio.Wave;

namespace BouncingBalls
{
    class SoundEngine
    {
        const int MAX_PITCH = 100;
        const int MIN_PITCH = 50;

        private int SoundPower = 2000000000;

        private WaveFormat WaveFormat;
        private double BufferDuration = 0.5;
        
        private int NumberOfSamples;
        private List<short[]> Samples;

        private LinkedList<SoundOut> WavesOut;

        private WaveCallbackInfo WaveCallbackInfo;

        public SoundEngine()
        {
            this.WaveFormat = new WaveFormat(10000, 16, 2);
            
            this.NumberOfSamples = Convert.ToInt32(this.BufferDuration * this.WaveFormat.SampleRate);
            this.Samples = new List<short[]>();
            this.GenerateSampleData();

            this.WavesOut = new LinkedList<SoundOut>();

            this.WaveCallbackInfo = WaveCallbackInfo.NewWindow();
        }

        public void Destroy()
        {
            SoundOut waveOut = new SoundOut(this.WaveCallbackInfo);
            waveOut.StereoVolume = this.GetVolume((float)0.5, (float)0.5); // reset volume
            this.WavesOut.AddFirst(waveOut);

            while (this.WavesOut.Count > 0)
            {
                SoundOut SoundOut = this.WavesOut.Last.Value;
                this.WavesOut.RemoveLast();
                SoundOut.Dispose();
                SoundOut = null;
            }
        }

        public void Play(double Pitch, int Amplitude, float Pan)
        {

            if (Pitch < MIN_PITCH) Pitch = MIN_PITCH;
            if (Pitch > MAX_PITCH) Pitch = MAX_PITCH;

            SoundProvider SoundProvider = new SoundProvider(this.WaveFormat, this.Samples[(int)Pitch - MIN_PITCH]);

            SoundOut waveOut = new SoundOut(this.WaveCallbackInfo);

            waveOut.StereoVolume = this.GetVolume((float)1 - Pan, Pan);

            waveOut.Init(SoundProvider, 20000);
            
            waveOut.Play();

            this.WavesOut.AddFirst(waveOut);
            
            
        }

        private int GetVolume(float Left, float Right)
        {
            Left = Math.Min(Math.Max(Left, 0.0f), 1.0f);
            Right = Math.Min(Math.Max(Right, 0.0f), 1.0f);
            
            return (int)(Left * 0xFFFF) + ((int)(Right * 0xFFFF) << 16);
        }

        private void GenerateSampleData()
        {
            double Frequency, Amplitude;
            for (int i = MIN_PITCH; i <= MAX_PITCH; i++)
            {
                Frequency = (double)440 * Math.Pow((double)2, (double)(i - 69) / (double)12);
                Amplitude = Math.Pow(Math.Sqrt(this.SoundPower / Frequency), 1.1);
                this.Samples.Add(this.GenerateSampleData(Frequency, Convert.ToInt32(Amplitude)));
            }
        }

        private short[] GenerateSampleData(double Frequency, int Amplitude)
        {
            short[] SampleData = new short[this.NumberOfSamples];

            double Angle = (Math.PI * 2 * Frequency) / (this.WaveFormat.SampleRate * this.WaveFormat.Channels);

            for (int i = 0; i < this.NumberOfSamples; i++)
            {
                // Generate a sine wave in both channels.
                SampleData[i] = Convert.ToInt16(Amplitude * Math.Sin(Angle * i) * (1 - Math.Pow((double)i / (double)NumberOfSamples, 2)));
            }

            return SampleData;
        }

        public void DisposeOldBuffers()
        {
            while (this.WavesOut.Last != null && this.WavesOut.Last.Value.PlaybackState == PlaybackState.Stopped)
            {
                SoundOut WaveOut = this.WavesOut.Last.Value;
                this.WavesOut.RemoveLast();
                WaveOut.Dispose();
                WaveOut = null;
            }
        }

    }
}
