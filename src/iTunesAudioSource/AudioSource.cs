﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using AudioBand.AudioSource;
using Timer = System.Timers.Timer;

namespace iTunesAudioSource
{
    public class AudioSource : IAudioSource
    {
        private Timer _checkiTunesTimer;
        private string _currentTrack;
        private bool _isPlaying;
        private ITunesControls _itunesControls = new ITunesControls();

        public AudioSource()
        {
            _checkiTunesTimer = new Timer(100)
            {
                Enabled = false,
                AutoReset = false
            };

            _checkiTunesTimer.Elapsed += CheckItunes;
        }

        public event EventHandler<TrackInfoChangedEventArgs> TrackInfoChanged;

        public event EventHandler TrackPlaying;

        public event EventHandler TrackPaused;

        public event EventHandler<TimeSpan> TrackProgressChanged;

#pragma warning disable 00067 // Event is not used
        public event EventHandler<SettingChangedEventArgs> SettingChanged;
#pragma warning restore 00067 // Event is not used

        public string Name => "iTunes";

        public IAudioSourceLogger Logger { get; set; }

        public Task ActivateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _itunesControls.Start();
            _checkiTunesTimer.Start();
            return Task.CompletedTask;
        }

        public Task DeactivateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _checkiTunesTimer.Stop();
            _itunesControls.Stop();

            _isPlaying = false;
            _currentTrack = null;

            return Task.CompletedTask;
        }

        public Task NextTrackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _itunesControls.Next();
            return Task.CompletedTask;
        }

        public Task PauseTrackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _itunesControls.Pause();
            return Task.CompletedTask;
        }

        public Task PlayTrackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _itunesControls.Play();
            return Task.CompletedTask;
        }

        public Task PreviousTrackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _itunesControls.Previous();
            return Task.CompletedTask;
        }

        private void NotifyTrackChange(Track track)
        {
            var trackInfo = new TrackInfoChangedEventArgs
            {
                Artist = track.Artist,
                Album = track.Album,
                AlbumArt = track.Artwork,
                TrackLength = track.Length,
                TrackName = track.Name,
            };

            TrackInfoChanged?.Invoke(this, trackInfo);
        }

        private bool IsNewTrack(Track track)
        {
            var trackname = track.Artist + track.Name;
            if (trackname == _currentTrack)
            {
                return false;
            }

            _currentTrack = trackname;
            return true;
        }

        private void NotifyPlayerState()
        {
            var playing = _itunesControls.IsPlaying;
            if (_isPlaying == playing)
            {
                return;
            }

            if (playing)
            {
                TrackPlaying?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                TrackPaused.Invoke(this, EventArgs.Empty);
            }

            _isPlaying = playing;
        }

        private void CheckItunes(object sender, ElapsedEventArgs eventArgs)
        {
            try
            {
                var track = _itunesControls.CurrentTrack;
                if (track == null)
                {
                    return;
                }

                NotifyPlayerState();
                if (IsNewTrack(track))
                {
                    NotifyTrackChange(track);
                }

                TrackProgressChanged?.Invoke(this, _itunesControls.Progress);
            }
            catch (Exception e)
            {
                Logger.Debug(e);
            }
            finally
            {
                _checkiTunesTimer.Enabled = true;
            }
        }
    }
}
