using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace MicroService4Net.Network
{
    public class SelfHostServer : IDisposable
    {
        #region Fields

        private readonly HttpSelfHostServer _server;

        #endregion

        #region C'tor

        public SelfHostServer(string uri, bool callControllersStaticConstractorsOnInit = true)
        {
            var config = new HttpSelfHostConfiguration(uri);

            config.MapHttpAttributeRoutes();

            config.EnableCors();
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());

            _server = new HttpSelfHostServer(config);

            if (callControllersStaticConstractorsOnInit)
                CallControllersStaticConstractors();
        }



        #endregion

        #region Public

        public void Connect()
        {
            try
            {
                _server.OpenAsync().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async void Dispose()
        {
            await _server.CloseAsync();
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

        private void InvokeStaticConstractor(TypeInfo type)
        {
            Activator.CreateInstance(type);
        }

        #endregion
    }
}
