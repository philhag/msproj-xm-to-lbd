using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;

namespace MSPROJ_XML_to_LBD
{
    internal class Namespaces
    {
        public static string RDF => "rdf";
        public static Uri RDF_URI => new("http://www.w3.org/1999/02/22-rdf-syntax-ns#");
        public static string OWL => "owl";
        public static Uri OWL_URI => new("http://www.w3.org/2002/07/owl#");
        public static string RDFS => "rdfs";
        public static Uri RDFS_URI => new("http://www.w3.org/2000/01/rdf-schema#");
        public static string XSD => "xsd";
        public static Uri XSD_URI => new("http://www.w3.org/2001/XMLSchema#");
        public static string MSPROJ => "msproj";
        public static Uri MSPROJ_URI => new("http://schemas.microsoft.com/project#");
        public static string CTO => "cto";
        public static Uri CTO_URI => new("https://w3id.org/cto#");
        public static string PROV => "prov";
        public static Uri PROV_URI => new("http://www.w3.org/ns/prov#");

        public static INamespaceMapper MSPROJ_NAMESPACE_MAP
        {
            get
            {
                var ns = new NamespaceMapper();
                ns.AddNamespace(RDF, RDF_URI);
                ns.AddNamespace(OWL, OWL_URI);
                ns.AddNamespace(RDFS, RDFS_URI);
                ns.AddNamespace(XSD, XSD_URI);
                ns.AddNamespace(MSPROJ, MSPROJ_URI);
                ns.AddNamespace(CTO, CTO_URI);
                ns.AddNamespace(PROV, PROV_URI);
                return ns;
            }
        }
    }
}
