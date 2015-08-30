using System;
using System.ServiceModel;
using System.Threading;

namespace lageant.Utils
{
    [ServiceContract]
    public interface IApplicationInstanceMonitor<in T>
    {
        [OperationContract(IsOneWay = true)]
        void NotifyNewInstance(T message);
    }

    public class NewInstanceCreatedEventArgs<T> : EventArgs
    {
        public NewInstanceCreatedEventArgs(T message)
        {
            Message = message;
        }

        public T Message { get; private set; }
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public sealed class ApplicationInstanceMonitor<T> : IApplicationInstanceMonitor<T>, IDisposable
    {
        #region Disposal

        public void Dispose()
        {
            if (_processLock != null)
                _processLock.Close();

            if (_ipcServer != null)
                _ipcServer.Close();

            if (_channelFactory != null)
                _channelFactory.Close();

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Events

        public event EventHandler<NewInstanceCreatedEventArgs<T>> NewInstanceCreated;

        #endregion

        #region Fields

        private readonly string _mutexName;
        private Mutex _processLock;

        private readonly Uri _ipcUri;
        private readonly NetNamedPipeBinding _binding;
        private ServiceHost _ipcServer;
        private ChannelFactory<IApplicationInstanceMonitor<T>> _channelFactory;
        private IApplicationInstanceMonitor<T> _ipcChannel;

        #endregion

        #region Constructors

        public ApplicationInstanceMonitor() :
            this(typeof (ApplicationInstanceMonitor<>).Assembly.FullName)
        {
        }

        public ApplicationInstanceMonitor(string mutexName) : this(mutexName, mutexName)
        {
        }

        public ApplicationInstanceMonitor(string mutexName, string ipcUriPath)
        {
            _mutexName = mutexName;
            var builder = new UriBuilder {Scheme = Uri.UriSchemeNetPipe, Host = "localhost", Path = ipcUriPath};
            _ipcUri = builder.Uri;
            _binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.Transport);
        }

        #endregion

        #region Methods

        public bool Assert()
        {
            if (_processLock != null)
                throw new InvalidOperationException("Assert() has already been called.");

            bool created;
            _processLock = new Mutex(true, _mutexName, out created);

            if (created)
                StartIpcServer();
            else
                ConnectToIpcServer();

            return created;
        }

        private void StartIpcServer()
        {
            try
            {
                _ipcServer = new ServiceHost(this, _ipcUri);
                _ipcServer.AddServiceEndpoint(typeof (IApplicationInstanceMonitor<T>), _binding, _ipcUri);
                _ipcServer.Open();

                _ipcChannel = this;
            }
            catch (Exception)
            {
            }
        }

        private void ConnectToIpcServer()
        {
            _channelFactory = new ChannelFactory<IApplicationInstanceMonitor<T>>(_binding, new EndpointAddress(_ipcUri));
            _ipcChannel = _channelFactory.CreateChannel();
        }

        public void NotifyNewInstance(T message)
        {
            // Client side

            if (_ipcChannel == null)
                throw new InvalidOperationException("Not connected to IPC Server.");

            _ipcChannel.NotifyNewInstance(message);
        }

        void IApplicationInstanceMonitor<T>.NotifyNewInstance(T message)
        {
            // Server side

            if (NewInstanceCreated != null)
                NewInstanceCreated(this, new NewInstanceCreatedEventArgs<T>(message));
        }

        #endregion
    }
}