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

        private string _serviceDisplayName;
        private string _ipAddress;
        private int _port;
        private WindowsServiceManager _serviceManager;
        private RegistryManipulator _registryManipulator;
        private SelfHostServer _selfHostServer;
        private Action<HttpConfiguration> _configure;
        private bool _useCors;

        #endregion

        #region C'tor

        public MicroService( int port = 8080, string serviceDisplayName = null, string serviceName = null,
            Action<HttpConfiguration> configure = null, bool useCors = true)
        {
            InitMicroService("*", port, serviceDisplayName, serviceName, configure, useCors);
        }

        /// <param name="ipAddress">Valid IP address (ie. localhost, *, 192.168.0.1, etc.)</param>
        public MicroService(string ipAddress, int port = 8080, string serviceDisplayName = null, string serviceName = null,
            Action<HttpConfiguration> configure = null, bool useCors = true)
        {
            InitMicroService(ipAddress, port, serviceDisplayName, serviceName, configure, useCors);
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

        private void InitMicroService(string ipAddress, int port, string serviceDisplayName, string serviceName,
            Action<HttpConfiguration> configure, bool useCors)
        {
            _ipAddress = ipAddress;
            _port = port;
            _configure = configure;
            _useCors = useCors;

            var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
            _serviceDisplayName = serviceDisplayName ?? assemblyName;
            serviceName = serviceName ?? assemblyName;

            _serviceManager = new WindowsServiceManager(_serviceDisplayName);
            _registryManipulator = new RegistryManipulator(serviceName);

            InternalService.OsStarted += () => Start(_configure, _useCors);
            InternalService.OsStopped += Stop;
            ProjectInstaller.InitInstaller(_serviceDisplayName, serviceName);
        }

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
            OnServiceStopped?.Invoke();
        }

        private void Start(Action<HttpConfiguration> configure, bool useCors)
        {
            _selfHostServer = new SelfHostServer(_ipAddress, _port, true);

            _selfHostServer.Connect(configure, useCors);
            Console.WriteLine($"Service {_serviceDisplayName} started on {_ipAddress}:{_port}");
            OnServiceStarted?.Invoke();
        }

        #endregion
    }
}
