using Microsoft.Win32;

namespace MicroService4Net
{
    internal class RegistryManipulator
    {
        private readonly string _serviceRegistryPath;
        private const string Imagepath = "ImagePath";
        private const string MinusService = " -service";

        public RegistryManipulator(string serviceName)
        {
            _serviceRegistryPath = @"SYSTEM\CurrentControlSet\services\" + serviceName;
        }

        internal void RemoveMinusServiceFromRegistry()
        {
            var key = Registry.LocalMachine.OpenSubKey(_serviceRegistryPath, true);
            var path = key.GetValue(Imagepath).ToString().Replace(MinusService, "");
            key.SetValue(Imagepath,path);
            key.Close();
        }

        internal void AddMinusServiceToRegistry()
        {
            var key = Registry.LocalMachine.OpenSubKey(_serviceRegistryPath, true);
            var path = key.GetValue(Imagepath) + MinusService;
            key.SetValue(Imagepath,path);
            key.Close();
        }

    }
}
