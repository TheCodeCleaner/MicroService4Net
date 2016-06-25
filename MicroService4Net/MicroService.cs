using System;
using System.Reflection;
using System.ServiceProcess;
using System.Web.Http;
using MicroService4Net.Network;
using MicroService4Net.ServiceInternals;

namespace MicroService4Net
{
    public class MicroService
    {
        #region Events

        public event Action OnServiceStarted;
        public event Action OnServiceStopped;

        #endregion

        #region Fields

        private readonly string _serviceDisplayName;
        private readonly string _ipAddress;
        private readonly int _port;
        private readonly WindowsServiceManager _serviceManager;
        private readonly RegistryManipulator _registryManipulator;
        private SelfHostServer _selfHostServer;
        private readonly Action<HttpConfiguration> _configure;
        private readonly bool _useCors;
        private readonly Uri _uri;
        private readonly bool _isValidUri = false;

        #endregion

        #region C'tor

        /// <summary>
        /// Microservice constructor
        /// </summary>
        /// <param name="ipAddress">Valid IP addres (ie. localhost, *, 192.168.0.1, etc.)</param>
        /// <param name="port"></param>
        /// <param name="serviceDisplayName"></param>
        /// <param name="serviceName"></param>
        /// <param name="configure"></param>
        /// <param name="useCors"></param>
        public MicroService(string ipAddress = "*", int port = 8080, string serviceDisplayName = null, string serviceName = null,
            Action<HttpConfiguration> configure = null, bool useCors = true)
        {
            _ipAddress = ipAddress;
            _port = port;
            _configure = configure;
            _useCors = useCors;

            // NOTE: * in IP address binds to all IP addresses and it is not valid URI
            _isValidUri = Uri.TryCreate($"http://{_ipAddress}:{_port}", UriKind.RelativeOrAbsolute, out _uri);

            var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
            _serviceDisplayName = serviceDisplayName ?? assemblyName;
            serviceName = serviceName ?? assemblyName;

            _serviceManager = new WindowsServiceManager(_serviceDisplayName);
            _registryManipulator = new RegistryManipulator(serviceName);

            InternalService.OsStarted += () => Start(_configure, _useCors);
            InternalService.OsStopped += Stop;
            ProjectInstaller.InitInstaller(_serviceDisplayName, serviceName);

        }

        #endregion

        #region Public

        public void Run(string[] args)
        {
            if (args.Length == 0)
            {
                RunConsole();
                return;
            }

            switch (args[0])
            {
                case "-service":
                    RunService();
                    break;
                case "-install":
                    InstallService();
                    break;
                case "-uninstall":
                    UnInstallService();
                    break;
                default:
                    throw new Exception(args[0] + " is not a valid command!");
            }
        }

        #endregion

        #region Private

        private void RunConsole()
        {
            Start(_configure, _useCors);
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
            Stop();
        }

        private static void RunService()
        {
            ServiceBase[] servicesToRun = { new InternalService() };
            ServiceBase.Run(servicesToRun);
        }

        private void InstallService()
        {
            _serviceManager.Install();
            _registryManipulator.AddMinusServiceToRegistry();
            _serviceManager.Start();
        }

        private void UnInstallService()
        {
            _serviceManager.Stop();
            _registryManipulator.RemoveMinusServiceFromRegistry();
            _serviceManager.UnInstall();
        }

        private void Stop()
        {
            _selfHostServer.Dispose();
            if (OnServiceStopped != null)
                OnServiceStopped.Invoke();
        }

        private void Start(Action<HttpConfiguration> configure, bool useCors)
        {
            if (_isValidUri)
                _selfHostServer = new SelfHostServer(_uri);
            else
                _selfHostServer = new SelfHostServer(_ipAddress, _port);

            _selfHostServer.Connect(configure, useCors);
            Console.WriteLine($"Service {_serviceDisplayName} started on {_ipAddress}:{_port}");
            if (OnServiceStarted != null)
                OnServiceStarted.Invoke();
        }

        #endregion
    }
}
