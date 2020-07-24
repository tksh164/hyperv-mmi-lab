using System;
using Microsoft.Management.Infrastructure;

namespace vmkvp
{
    internal class HvVirtualMachine
    {
        public CimMetadata CimMetadata { get; protected set; }
        public Guid VMGuid { get; protected set; }
        public string DisplayName { get; protected set; }
        public DateTime InstallDate { get; protected set; }
        public DateTime TimeOfLastConfigurationChange { get; protected set; }
        public DateTime TimeOfLastStateChange { get; protected set; }

        public HvVirtualMachine(CimInstance instance)
        {
            CimMetadata = new CimMetadata(instance);
            VMGuid = new Guid(instance.CimInstanceProperties["Name"].Value.ToString());
            DisplayName = instance.CimInstanceProperties["ElementName"].Value.ToString();
            InstallDate = (DateTime) instance.CimInstanceProperties["InstallDate"].Value;
            TimeOfLastConfigurationChange = (DateTime) instance.CimInstanceProperties["TimeOfLastConfigurationChange"].Value;
            TimeOfLastStateChange = (DateTime) instance.CimInstanceProperties["TimeOfLastStateChange"].Value;
        }
    }
}
