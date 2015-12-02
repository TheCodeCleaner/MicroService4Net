using System;
using System.Reflection;
using System.ServiceProcess;
using MicroService4Net.Network;
using MicroService4Net.ServiceInternals;
using Owin;

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
        private readonly int _port;
        private readonly WindowsServiceManager _serviceManager;
        private readonly RegistryManipulator _registryManipulator;
        private SelfHostServer _selfHostServer;
        private Action<IAppBuilder> _buildApp;

        #endregion

        #region C'tor

        public MicroService(int port = 8080, string serviceDisplayName = null, string serviceName = null, Action<IAppBuilder> buildApp = null)
        {
            _port = port;
            _buildApp = buildApp;

            var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
            _serviceDisplayName = serviceDisplayName ?? assemblyName;
            serviceName = serviceName ?? assemblyName;

            _serviceManager = new WindowsServiceManager(_serviceDisplayName);
            _registryManipulator = new RegistryManipulator(serviceName);

            InternalService.OsStarted += () => Start(buildApp);
            InternalService.OsStopped += Stop;
            ProjectInstaller.InitInstaller(_serviceDisplayName,serviceName);

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
            Start(_buildApp);
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
            Stop();
        }

        private static void RunService()
        {
            ServiceBase[] servicesToRun = {new InternalService()};
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
            if ( OnServiceStopped != null)
                OnServiceStopped.Invoke();
        }

        private void Start(Action<IAppBuilder> buildApp = null)
        {
            _selfHostServer = new SelfHostServer("http://localhost:" + _port);

            _selfHostServer.Connect(buildApp);
            Console.WriteLine("Service {0} started on port {1}", _serviceDisplayName,_port);
            if ( OnServiceStarted != null)
                OnServiceStarted.Invoke();
        }

        #endregion
    }
}
