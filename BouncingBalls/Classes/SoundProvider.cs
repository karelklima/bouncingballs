using System;
using System.Collections.Generic;

using NAudio.Wave;

namespace BouncingBalls
{
    class SoundProvider : IWaveProvider
    {
        private short[] _SoundBuffer;

        private bool Accessed = false;

        private WaveFormat _WaveFormat;
        public WaveFormat WaveFormat { get { return this._WaveFormat; } }

        public SoundProvider(WaveFormat WaveFormat, short[] SoundBuffer)
        {
            this._WaveFormat = WaveFormat;
            this._SoundBuffer = SoundBuffer;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            WaveBuffer waveBuffer = new WaveBuffer(buffer);
            int samplesRequired = count / 2;
            int samplesRead = Read(waveBuffer.ShortBuffer, offset / 2, samplesRequired);
            return samplesRead * 2;
        }

        public int Read(short[] buffer, int offset, int sampleCount)
        {

            if (this.Accessed) return 0; else this.Accessed = true;

            for (int index = 0; index < sampleCount / 2; index++)
            {
                buffer[offset + 2 * index] = (short)this._SoundBuffer[offset + index];
                buffer[offset + 2 * index + 1] = (short)this._SoundBuffer[offset + index];
            }

            
            return sampleCount;
        }

    }
}
