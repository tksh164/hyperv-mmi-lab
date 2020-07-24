using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Management.Infrastructure;

namespace vmkvp
{
    internal class HvKvpExchangeComponent
    {
        public CimMetadata CimMetadata { get; protected set; }
        public List<HvKvpExchangeDataItem> GuestExchangeItems { get; protected set; }
        public List<HvKvpExchangeDataItem> GuestIntrinsicExchangeItems { get; protected set; }

        public HvKvpExchangeComponent(CimInstance instance)
        {
            CimMetadata = new CimMetadata(instance);
            GuestExchangeItems = new List<HvKvpExchangeDataItem>();
            GuestIntrinsicExchangeItems = new List<HvKvpExchangeDataItem>();

            var guestExchangeItems = (string[])instance.CimInstanceProperties["GuestExchangeItems"].Value;
            foreach (var itemXml in guestExchangeItems)
            {
                GuestExchangeItems.Add(DeserializeGuestExchangeItemXml(itemXml));
            }

            var guestIntrinsicExchangeItems = (string[])instance.CimInstanceProperties["GuestIntrinsicExchangeItems"].Value;
            foreach (var itemXml in guestIntrinsicExchangeItems)
            {
                GuestIntrinsicExchangeItems.Add(DeserializeGuestExchangeItemXml(itemXml));
            }
        }

        private HvKvpExchangeDataItem DeserializeGuestExchangeItemXml(string xmlText)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlText ?? string.Empty), false))
            {
                var serializer = new XmlSerializer(typeof(KvpExchangeDataItemInstance));
                var rawItem = (KvpExchangeDataItemInstance)serializer.Deserialize(stream);
                return new HvKvpExchangeDataItem(rawItem);
            }
        }
    }

    internal class HvKvpExchangeDataItem
    {
        public string Name { get; protected set; }
        public string Data { get; protected set; }
        public uint Source { get; protected set; }
        public string Caption { get; protected set; }
        public string Description { get; protected set; }
        public string ElementName { get; protected set; }
        public string InstanceId { get; protected set; }

        public HvKvpExchangeDataItem(KvpExchangeDataItemInstance rawItem)
        {
            foreach (var p in rawItem.Properties)
            {
                if (p.Name.Equals("Name", StringComparison.Ordinal))
                {
                    Name = p.Value;
                }
                else if (p.Name.Equals("Data", StringComparison.Ordinal))
                {
                    Data = p.Value;
                }
                else if (p.Name.Equals("Source", StringComparison.Ordinal))
                {
                    Source = uint.Parse(p.Value);
                }
                else if (p.Name.Equals("Caption", StringComparison.Ordinal))
                {
                    Caption = p.Value;
                }
                else if (p.Name.Equals("Description", StringComparison.Ordinal))
                {
                    Description = p.Value;
                }
                else if (p.Name.Equals("ElementName", StringComparison.Ordinal))
                {
                    ElementName = p.Value;
                }
                else if (p.Name.Equals("InstanceID", StringComparison.Ordinal))
                {
                    InstanceId = p.Value;
                }
                else
                {
                    throw new NotImplementedException(string.Format("Unexpected property '{0}' was detected.", p.Name));
                }
            }
        }
    }

    [XmlRoot("INSTANCE")]
    public class KvpExchangeDataItemInstance
    {
        [XmlAttribute("CLASSNAME")]
        public string ClassName { get; set; }

        [XmlElement("PROPERTY")]
        public KvpExchangeDataItemInstanceProperty[] Properties { get; set; }
    }

    public class KvpExchangeDataItemInstanceProperty
    {
        [XmlAttribute("NAME")]
        public string Name { get; set; }

        [XmlAttribute("TYPE")]
        public string Type { get; set; }

        [XmlElement("VALUE")]
        public string Value { get; set; }
    }
}
