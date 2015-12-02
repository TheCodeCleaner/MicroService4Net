using MicroService4Net.Network;

namespace MicroService4Net.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var microService = new MicroService();
            microService.Run(args);
        }
    }
}
