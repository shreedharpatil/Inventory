using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace roslyncompiler
{
    public class EndPointMap
    {
        public string Type { get; set; }

        public string EndPoint { get; set; }
    }

    public class EndPoint
    {
        public string Operation { get; set; }

        public string EndPointUrl { get; set; }
    }

    public class MappingGeneration
    {
        public static IList<EndPointMap> EndPointsMapper;
        public static void Generate(string webconfigPath =null)
        {
            string configFilePath = 
                webconfigPath != null ? webconfigPath :
            @"";
            var xml = XDocument.Load(configFilePath);
            EndPointsMapper = new List<EndPointMap>();
            var client = xml.Root.Descendants("client");
            string endpointType;
            string endpoint;
            foreach (var element in client.Elements())
            {
                if(element.HasAttributes)
                {
                    endpointType = element.Attribute("contract").Value;
                    endpoint = element.Attribute("address").Value;
                    EndPointsMapper.Add(new EndPointMap
                                        {
                                            EndPoint = endpoint,
                                            Type = endpointType
                                        });
                }
            }
        }

        public static string GetEndPoint(string type)
        {
            var endpoint = EndPointsMapper.FirstOrDefault(p => p.Type.Contains(type) || type.Contains(p.Type));
            return endpoint!=null ? endpoint.EndPoint : null;
        }
    }
}
