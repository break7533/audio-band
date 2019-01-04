﻿using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using ServiceContracts;

namespace AudioBand.AudioSource
{
    internal class AudioSourceProxy : IAudioSource
    {
        private readonly ILogger _logger;
        private IAudioSourceHost _host;
        private DuplexChannelFactory<IAudioSourceHost> _channelFactory;

        public AudioSourceProxy(Uri hostUri)
        {
            _logger = LogManager.GetLogger($"AudioSourceProxy@{hostUri}");

            var callback = new AudioSourceHostCallback();
            callback.SettingChanged += (o, e) => SettingChanged?.Invoke(this, e);
            callback.TrackInfoChanged += (o, e) => TrackInfoChanged?.Invoke(this, e);
            callback.TrackPlaying += (o, e) => TrackPlaying?.Invoke(this, e);
            callback.TrackPaused += (o, e) => TrackPaused?.Invoke(this, e);
            callback.TrackProgressChanged += (o, e) => TrackProgressChanged?.Invoke(this, e);

            var callbackInstance = new InstanceContext(callback);
            _channelFactory = new DuplexChannelFactory<IAudioSourceHost>(callbackInstance, new NetNamedPipeBinding(), new EndpointAddress(hostUri));
            _channelFactory.Faulted += ChannelFactoryFaulted;
            _host = _channelFactory.CreateChannel();
        }

        private void ChannelFactoryFaulted(object sender, EventArgs e)
        {
            _logger.Error("Communication faulted");
            Errored?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Errored;

        public event EventHandler<SettingChangedEventArgs> SettingChanged;

        public event EventHandler<TrackInfoChangedEventArgs> TrackInfoChanged;

        public event EventHandler TrackPlaying;

        public event EventHandler TrackPaused;

        public event EventHandler<TimeSpan> TrackProgressChanged;

        public Uri Uri { get; set; }

        public string Name
        {
            get
            {
                CheckChannel();
                return _host.GetName();
            }
        }

        public IAudioSourceLogger Logger { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Close()
        {
            _channelFactory.Close();
        }

        public async Task ActivateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckChannel();
            await _host.ActivateAsync().ConfigureAwait(false);
        }

        public async Task DeactivateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckChannel();
            await _host.DeactivateAsync().ConfigureAwait(false);
        }

        public async Task NextTrackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckChannel();
            await _host.NextTrackAsync().ConfigureAwait(false);
        }

        public async Task PauseTrackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckChannel();
            await _host.PauseTrackAsync().ConfigureAwait(false);
        }

        public async Task PlayTrackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckChannel();
            await _host.PlayTrackAsync().ConfigureAwait(false);
        }

        public async Task PreviousTrackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckChannel();
            await _host.PreviousTrackAsync().ConfigureAwait(false);
        }

        private void CheckChannel()
        {
            if (_channelFactory.State == CommunicationState.Faulted)
            {
                _logger.Warn("Channel in faulted state. creating new channel");
                _host = _channelFactory.CreateChannel();
            }
        }
    }
}
