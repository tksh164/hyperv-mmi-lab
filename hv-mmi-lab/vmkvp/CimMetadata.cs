using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Management.Infrastructure;

namespace vmkvp
{
    internal class CimMetadata
    {
        public string ServerName { get; protected set; }
        public string Namespace { get; protected set; }
        public string ObjectPath { get; protected set; }

        public CimMetadata(CimInstance instance)
        {
            ServerName = instance.CimSystemProperties.ServerName;
            Namespace = instance.CimSystemProperties.Namespace;
            ObjectPath = GetCimInstanceObjectPath(instance);
        }

        private static string GetCimInstanceObjectPath(CimInstance instance)
        {
            var keyProperties = instance.CimClass.CimClassProperties.Where((property) => {
                return property.Qualifiers.FirstOrDefault((qualifier) => {
                    return qualifier.Name.Equals("Key", StringComparison.OrdinalIgnoreCase);
                }) != null;
            });

            List<string> keyPropertyValuePairs = new List<string>();
            foreach (var kp in keyProperties)
            {
                var propertyName = kp.Name;
                var propertyValue = instance.CimInstanceProperties[propertyName].Value.ToString();
                keyPropertyValuePairs.Add(string.Format(@"{0}=""{1}""", propertyName, propertyValue));
            }

            var className = instance.CimSystemProperties.ClassName;
            var keyPropertyValuePairsPart = string.Join(',', keyPropertyValuePairs);
            return string.Format("{0}.{1}", className, keyPropertyValuePairsPart);
        }
    }
}
