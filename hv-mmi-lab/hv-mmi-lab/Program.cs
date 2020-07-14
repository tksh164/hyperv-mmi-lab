using System;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;

namespace hv_mmi_lab
{
    class Program
    {
        static void Main(string[] args)
        {
            var hvHostMachine = "localhost";
            var options = new CimSessionOptions();
            using (var session = CimSession.Create(hvHostMachine, options))
            {
                const string namespaceName = @"root\virtualization\v2";
                const string query = "SELECT * FROM Msvm_ComputerSystem";
                var cimOpsOptions = new CimOperationOptions();
                var computerSystems = session.QueryInstances(namespaceName, "WQL", query, cimOpsOptions);

                foreach (var instance in computerSystems)
                {
                    Console.WriteLine(instance.CimInstanceProperties["ElementName"].Value.ToString());
                    instance.Dispose();
                }
            }
        }
    }
}
