using System;
using System.ServiceProcess;

namespace MicroService4Net.ServiceInternals
{
    public class InternalService : ServiceBase
    {
        internal static event Action OsStarted;
        internal static event Action OsStopped;

        protected override void OnStart(string[] args)
        {
            OsStarted.Invoke();
        }

        protected override void OnStop()
        {
            OsStopped.Invoke();
        }
    }
}
