using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;

namespace vmkvp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }
            var hvHostMachine = args[0];
            var vmGuid = new Guid(args[1]);

            var vm = GetVirtualMachine(hvHostMachine, vmGuid);
            var kvpComponents = GetKvpExchangeComponents(vm);
            PrintDataExchageItems(kvpComponents);
        }

        private const string VirtualizationNamespace = @"root\virtualization\v2";

        private static HvVirtualMachine GetVirtualMachine(string hvHostMachine, Guid vmGuid)
        {
            var vms = new List<HvVirtualMachine>();

            var options = new CimSessionOptions();
            using (var session = CimSession.Create(hvHostMachine, options))
            {
                var vmQuery = string.Format("SELECT * FROM Msvm_ComputerSystem WHERE Name = '{0}'", vmGuid.ToString());
                var cimOpsOptions = new CimOperationOptions();
                var computerSystems = session.QueryInstances(VirtualizationNamespace, "WQL", vmQuery, cimOpsOptions);
                foreach (var cs in computerSystems)
                {
                    vms.Add(new HvVirtualMachine(cs));
                    cs.Dispose();
                }
            }

            return vms.FirstOrDefault();
        }

        private static IReadOnlyCollection<HvKvpExchangeComponent> GetKvpExchangeComponents(HvVirtualMachine vm)
        {
            var options = new CimSessionOptions();
            using (var session = CimSession.Create(vm.CimMetadata.ServerName, options))
            {
                var cimOpsOptions = new CimOperationOptions();
                var kvpExchangeComponents = GetRelatedInstance(session, VirtualizationNamespace, vm.CimMetadata.ObjectPath, "Msvm_SystemDevice", "Msvm_KvpExchangeComponent", "PartComponent", "GroupComponent", cimOpsOptions);

                var kvpComponents = new List<HvKvpExchangeComponent>();
                foreach (var c in kvpExchangeComponents)
                {
                    kvpComponents.Add(new HvKvpExchangeComponent(c));
                    c.Dispose();
                }
                return kvpComponents.AsReadOnly();
            }
        }

        private static IEnumerable<CimInstance> GetRelatedInstance(CimSession session, string namespaceName, string sourceInstanceObjectPath, string associationClassName, string resultClassName, string resultRoleName, string roleName, CimOperationOptions options)
        {
            var query = string.Format("ASSOCIATORS OF {{{0}}} WHERE AssocClass = {1} ResultClass = {2} ResultRole = {3} Role = {4}", sourceInstanceObjectPath, associationClassName, resultClassName, resultRoleName, roleName);
            return session.QueryInstances(namespaceName, "WQL", query, options);
        }

        private static void PrintDataExchageItems(IReadOnlyCollection<HvKvpExchangeComponent> kvpComponents)
        {
            foreach (var kvpComponent in kvpComponents)
            {
                Console.WriteLine("==== GuestIntrinsicExchangeItems ====");
                foreach (var item in kvpComponent.GuestIntrinsicExchangeItems)
                {
                    Console.WriteLine("Name:{0}, Source:{1}, Data:{2}, InstanceID:{3}, ElementName:{4}, Caption:{5}, Description:{6}", item.Name, item.Source, item.Data, item.InstanceId, item.ElementName, item.Caption, item.Description);
                }
                Console.WriteLine();
                Console.WriteLine("==== GuestExchangeItems ====");
                foreach (var item in kvpComponent.GuestExchangeItems)
                {
                    Console.WriteLine("Name:{0}, Source:{1}, Data:{2}, InstanceID:{3}, ElementName:{4}, Caption:{5}, Description:{6}", item.Name, item.Source, item.Data, item.InstanceId, item.ElementName, item.Caption, item.Description);
                }
            }
        }

        private static void PrintUsage()
        {
            var exeName = Assembly.GetExecutingAssembly().GetName().Name;
            Console.WriteLine(@"Usage: {0} hyperv_host vm_guid", exeName);
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  {0} localhost 259da607-f607-463a-90a7-ab310b890722", exeName);
            Console.WriteLine("  {0} hvhost1 259da607-f607-463a-90a7-ab310b890722", exeName);
            Console.WriteLine("  {0} hvhost1.internal.example.com 259da607-f607-463a-90a7-ab310b890722", exeName);
        }
    }
}
