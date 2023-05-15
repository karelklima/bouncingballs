using System;
using System.IO;
using System.Runtime.InteropServices;

using NAudio;
using NAudio.Wave;

namespace BouncingBalls
{
    /// <summary>
    /// Represents a wave out device
    /// </summary>
    public class SoundOut
    {
        private IntPtr hWaveOut;
        private WaveOutBuffer Buffer;
        private IWaveProvider waveStream;

        private PlaybackState playbackState;
        private WaveInterop.WaveCallback callback;

        // Added by Karel Klima - stereo volume support
        private int stereoVolume;

        private WaveCallbackInfo callbackInfo;
        private object waveOutLock;

        /// <summary>
        /// Indicates playback has stopped automatically
        /// </summary>
        public event EventHandler PlaybackStopped;

        /// <summary>
        /// Retrieves the capabilities of a waveOut device
        /// </summary>
        /// <param name="devNumber">Device to test</param>
        /// <returns>The WaveOut device capabilities</returns>
        public static WaveOutCapabilities GetCapabilities(int devNumber)
        {
            WaveOutCapabilities caps = new WaveOutCapabilities();
            int structSize = Marshal.SizeOf(caps);
            MmException.Try(WaveInterop.waveOutGetDevCaps(devNumber, out caps, structSize), "waveOutGetDevCaps");
            return caps;
        }

        /// <summary>
        /// Returns the number of Wave Out devices available in the system
        /// </summary>
        public static Int32 DeviceCount
        {
            get
            {
                return WaveInterop.waveOutGetNumDevs();
            }
        }

        /// <summary>
        /// Gets or sets the desired latency in milliseconds
        /// Should be set before a call to Init
        /// </summary>
        public int DesiredLatency { get; set; }

        /// <summary>
        /// Gets or sets the number of buffers used
        /// Should be set before a call to Init
        /// </summary>
        public int NumberOfBuffers { get; set; }

        /// <summary>
        /// Gets or sets the device number
        /// Should be set before a call to Init
        /// This must be between 0 and <see>DeviceCount</see> - 1.
        /// </summary>
        public int DeviceNumber { get; set; }


        /// <summary>
        /// Opens a WaveOut device
        /// </summary>
        public SoundOut(WaveCallbackInfo callbackInfo)
        {
            // set default values up
            this.DeviceNumber = 0;
            this.DesiredLatency = 300;
            this.NumberOfBuffers = 3;

            this.callback = new WaveInterop.WaveCallback(Callback);
            this.waveOutLock = new object();
            this.callbackInfo = callbackInfo;
            callbackInfo.Connect(this.callback);
        }

        /// <summary>
        /// Initialises the WaveOut device
        /// </summary>
        /// <param name="waveProvider">WaveProvider to play</param>
        public void Init(IWaveProvider waveProvider, int bufferSize)
        {
            this.waveStream = waveProvider;
            
            MmResult result;
            lock (waveOutLock)
            {
                result = callbackInfo.WaveOutOpen(out hWaveOut, DeviceNumber, waveStream.WaveFormat, callback);
            }
            MmException.Try(result, "waveOutOpen");

            playbackState = PlaybackState.Stopped;

            this.Buffer = new WaveOutBuffer(hWaveOut, bufferSize, waveStream, waveOutLock);
            
        }

        /// <summary>
        /// Start playing the audio from the WaveStream
        /// </summary>
        public void Play()
        {
            if (playbackState == PlaybackState.Stopped)
            {
                playbackState = PlaybackState.Playing;
                if (this.Buffer.OnDone())
                {
                    playbackState = PlaybackState.Stopped;
                }
            }
        }

        /// <summary>
        /// Stop and reset the WaveOut device
        /// </summary>
        public void Stop()
        {
            if (playbackState != PlaybackState.Stopped)
            {
                MmResult result;
                lock (waveOutLock)
                {
                    result = WaveInterop.waveOutReset(hWaveOut);
                }
                if (result != MmResult.NoError)
                {
                    throw new MmException(result, "waveOutReset");
                }
                playbackState = PlaybackState.Stopped;
            }
        }

        /// <summary>
        /// Playback State
        /// </summary>
        public PlaybackState PlaybackState
        {
            get { return playbackState; }
        }

        // STEREO VOLUME SUPPORT - added by Karel Klima
        public int StereoVolume
        {
            get
            {
                return stereoVolume;
            }
            set
            {
                stereoVolume = value;
                MmResult result;
                lock (waveOutLock)
                {
                    result = WaveInterop.waveOutSetVolume(hWaveOut, stereoVolume);
                }
                MmException.Try(result, "waveOutSetVolume");
            }
        }

        #region Dispose Pattern

        /// <summary>
        /// Closes this WaveOut device
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>
        /// Closes the WaveOut device and disposes of buffers
        /// </summary>
        /// <param name="disposing">True if called from <see>Dispose</see></param>
        protected void Dispose(bool disposing)
        {
            Stop();

            if (disposing)
            {
                if (this.Buffer != null)
                {
                    this.Buffer.Dispose();
                    this.Buffer = null;
                }
            }

            lock (waveOutLock)
            {
                WaveInterop.waveOutClose(hWaveOut);
            }
            if (disposing)
            {
                callbackInfo.Disconnect();
            }
        }

        /// <summary>
        /// Finalizer. Only called when user forgets to call <see>Dispose</see>
        /// </summary>
        ~SoundOut()
        {
            System.Diagnostics.Debug.Assert(false, "WaveOut device was not closed");
            Dispose(false);
        }

        #endregion

        // made non-static so that playing can be stopped here
        private void Callback(IntPtr hWaveOut, WaveInterop.WaveMessage uMsg, Int32 dwUser, WaveHeader wavhdr, int dwReserved)
        {
            if (uMsg == WaveInterop.WaveMessage.WaveOutDone)
            {
                GCHandle hBuffer = (GCHandle)wavhdr.userData;
                WaveOutBuffer buffer = (WaveOutBuffer)hBuffer.Target;
                // check that we're not here through pressing stop
                if (PlaybackState == PlaybackState.Playing)
                {
                    if (!buffer.OnDone())
                    {
                        playbackState = PlaybackState.Stopped;
                        RaisePlaybackStoppedEvent();
                    }
                }

                // n.b. this was wrapped in an exception handler, but bug should be fixed now
            }
        }

        private void RaisePlaybackStoppedEvent()
        {
            if (PlaybackStopped != null)
            {
                PlaybackStopped(this, EventArgs.Empty);
            }
        }
    }
}
