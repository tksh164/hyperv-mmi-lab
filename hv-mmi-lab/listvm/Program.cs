using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;

namespace listvm
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintUsage();
                return;
            }

            var hvHostMachine = args[0];
            var vms = GetVMs(hvHostMachine);
            PrintVMList(vms);
        }

        private const string VirtualizationNamespace = @"root\virtualization\v2";

        private static IReadOnlyCollection<HvVirtualMachine> GetVMs(string hvHostMachine)
        {
            var vms = new List<HvVirtualMachine>();

            var options = new CimSessionOptions();
            using (var session = CimSession.Create(hvHostMachine, options))
            {
                const string query = "SELECT * FROM Msvm_ComputerSystem WHERE Caption = 'Virtual Machine'";
                var cimOpsOptions = new CimOperationOptions();
                var computerSystems = session.QueryInstances(VirtualizationNamespace, "WQL", query, cimOpsOptions);

                foreach (var instance in computerSystems)
                {
                    vms.Add(new HvVirtualMachine(instance));
                    instance.Dispose();
                }
            }

            return vms.AsReadOnly();
        }

        private static void PrintVMList(IReadOnlyCollection<HvVirtualMachine> vms)
        {
            foreach (var vm in vms)
            {
                Console.WriteLine("DisplayName: " + vm.DisplayName);
                Console.WriteLine("VMGuid: " + vm.VMGuid);
                Console.WriteLine("InstallDate: " + vm.InstallDate);
                Console.WriteLine("TimeOfLastConfigurationChange: " + vm.TimeOfLastConfigurationChange);
                Console.WriteLine("TimeOfLastStateChange: " + vm.TimeOfLastStateChange);
                Console.WriteLine();
            }
        }

        private static void PrintUsage()
        {
            var exeName = Assembly.GetExecutingAssembly().GetName().Name;
            Console.WriteLine(@"Usage: {0} hyperv_host", exeName);
            Console.WriteLine();
            Console.WriteLine("  Example: {0} localhost", exeName);
            Console.WriteLine("  Example: {0} hvhost1", exeName);
            Console.WriteLine("  Example: {0} hvhost1.internal.example.com", exeName);
        }
    }
}
