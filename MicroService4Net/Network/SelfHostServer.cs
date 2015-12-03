using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;

namespace MicroService4Net.Network
{
    public class SelfHostServer : IDisposable
    {
        #region Fields

        private readonly StartOptions _options;
        private IDisposable _serverDisposable;

        #endregion

        #region C'tor

        public SelfHostServer(string uri, bool callControllersStaticConstractorsOnInit = true)
        {
            _options = new StartOptions(uri);

            if (callControllersStaticConstractorsOnInit)
                CallControllersStaticConstractors();
        }

        #endregion

        #region Public

        public void Connect(Action<HttpConfiguration> configure, bool useCors)
        {
            try
            {
                _serverDisposable = WebApp.Start(_options, appBuilder => BuildApp(appBuilder, configure, useCors));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void BuildApp(IAppBuilder appBuilder, Action<HttpConfiguration> configure, bool useCors)
        {
            var config = new HttpConfiguration();

            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());

            config.MapHttpAttributeRoutes();

            if ( configure != null)
                configure(config);

            if (useCors)
                appBuilder.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            
            appBuilder.UseWebApi(config);
        }

        public async void Dispose()
        {
            _serverDisposable.Dispose();
        }

        #endregion

        #region Private

        private void CallControllersStaticConstractors()
        {
            foreach (
                var type in
                    Assembly.GetEntryAssembly().DefinedTypes.Where(type => type.IsSubclassOf(typeof (ApiController))))
                InvokeStaticConstractor(type);
        }

        private static void InvokeStaticConstractor(Type type)
        {
            Activator.CreateInstance(type);
        }

        #endregion
    }
}
