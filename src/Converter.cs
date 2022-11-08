using System.Collections.Generic;
using System.Xml.Serialization;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace MSPROJ_XML_to_LBD
{
    /// <summary>
    /// Converter Class
    /// </summary>
    public class Converter
    {
        private IGraph _graph;
        private string _inst;
        private string _inputXML;

        /// <summary>
        /// Converter constructor pointing to the XML document to be converted
        /// </summary>
        /// <param name="xmlPath"></param>
        public Converter(string xmlPath)
        {
            _graph = new Graph();
            _inst = "http://example.com/msprojxml/" + Guid.NewGuid() + "/instances";
            _graph.NamespaceMap.Import(Namespaces.MSPROJ_NAMESPACE_MAP);
            _graph.BaseUri = new Uri(_inst);
            _graph.NamespaceMap.AddNamespace("inst", new Uri(_inst + "#"));

            _inputXML = xmlPath;

        }

        /// <summary>
        /// Method that converts XML to a LBD in turtle format, optionally defines and output file path
        /// </summary>
        /// <param name="outputPath"></param>
        /// <returns></returns>
        public bool Convert(string outputPath = null)
        {
            XmlSerializer serializer = new(typeof(MsProjectXML.Project));
            MsProjectXML.Project project;
            using (Stream reader = new FileStream(_inputXML, FileMode.Open))
            {
                // Call the Deserialize method to restore the object's state.
                project = serializer.Deserialize(reader) as MsProjectXML.Project;
            }

            if (project == null)
                WriteProjectToRDF(project);
            else
                return false;

            try
            {
                CompressingTurtleWriter writer = new(TurtleSyntax.W3C);
                if (string.IsNullOrEmpty(outputPath))
                    outputPath = Path.ChangeExtension(_inputXML, "ttl");

                writer.Save(_graph, outputPath);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        internal void WriteProjectToRDF(MsProjectXML.Project project)
        {
            IUriNode projectNode = !string.IsNullOrEmpty(project?.Name) ? CreateInstanceNode(RemoveNotAllowedChars(project.Name)) : CreateInstanceNode("ConstructionProject");
            IUriNode typePredicateNode = CreateNode(Namespaces.RDF, "type");
            IUriNode typeObjectNode = CreateNode(Namespaces.CTO, "TaskContext");
            IUriNode typeObjectNode2 = CreateNode(Namespaces.MSPROJ, "Project");
            _graph.Assert(new Triple(projectNode, typePredicateNode, typeObjectNode));
            _graph.Assert(new Triple(projectNode, typePredicateNode, typeObjectNode2));


            foreach (MsProjectXML.ProjectTask task in project.Tasks)
            {
                try
                {
                    IUriNode predNode = CreateNode(Namespaces.CTO, "hasTaskContext");
                    IUriNode taskNode = CreateInstanceNode(GetUriFriendlyName(task));
                    IUriNode taskTypeObjectNode = CreateNode(Namespaces.CTO, "Task");
                    IUriNode taskTypeObjectNode2 = CreateNode(Namespaces.MSPROJ, "Task");
                    _graph.Assert(new Triple(taskNode, typePredicateNode, taskTypeObjectNode));
                    _graph.Assert(new Triple(taskNode, typePredicateNode, taskTypeObjectNode2));
                    _graph.Assert(new Triple(taskNode, predNode, projectNode));


                    if (!string.IsNullOrEmpty(task.Name))
                    {
                        IUriNode taskDescriptionNode = CreateNode(Namespaces.CTO, "hasSimpleTaskMethodDescription");
                        ILiteralNode descriptionNode = _graph.CreateLiteralNode(task.Name);
                        _graph.Assert(new Triple(taskNode, taskDescriptionNode, descriptionNode));
                    }

                    if (!string.IsNullOrEmpty(task.UID))
                    {
                        IUriNode predicateNode = CreateNode(Namespaces.MSPROJ, "UID");
                        ILiteralNode objectNode = _graph.CreateLiteralNode(task.UID);
                        _graph.Assert(new Triple(taskNode, predicateNode, objectNode));
                    }

                    {
                        IUriNode predicateNode = CreateNode(Namespaces.PROV, "startedAtTime");
                        ILiteralNode objectNode = task.Start.ToLiteral(_graph);
                        _graph.Assert(new Triple(taskNode, predicateNode, objectNode));
                    }

                    {
                        IUriNode predicateNode = CreateNode(Namespaces.PROV, "endedAtTime");
                        ILiteralNode objectNode = task.Finish.ToLiteral(_graph);
                        _graph.Assert(new Triple(taskNode, predicateNode, objectNode));
                    }

                    if (task?.PredecessorLink == null || task?.PredecessorLink.Length == 0) continue;

                    foreach (MsProjectXML.ProjectTaskPredecessorLink predecessorLink in task.PredecessorLink)
                    {
                        if (string.IsNullOrEmpty(predecessorLink.PredecessorUID)) continue;

                        MsProjectXML.ProjectTask predecessor = project.Tasks.ToList().Find(tsk => tsk.UID == predecessorLink.PredecessorUID);
                        if (predecessor == null) continue;

                        IUriNode predecessorNode = CreateInstanceNode(GetUriFriendlyName(predecessor));
                        IUriNode afterFinishedTaskNode = CreateNode(Namespaces.CTO, "afterFinishedTask");
                        _graph.Assert(new Triple(taskNode, afterFinishedTaskNode, predecessorNode));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error when converting Task " + task?.Name + "_" + task?.UID + ": " + e);
                }
            }
        }

        internal string RemoveNotAllowedChars(string originalString)
        {
            return originalString.Replace(" ", string.Empty).ToUpper()
                .Replace("Ä", "AE")
                .Replace("Ö", "OE")
                .Replace("Ü", "UE")
                .Replace("ß", "SS");
        }

        internal IUriNode CreateInstanceNode(string nodeIdentifier)
        {
            return _graph.CreateUriNode(new Uri(_inst + "#" + RemoveNotAllowedChars(nodeIdentifier)));
        }

        internal IUriNode CreateNode(string prefix, string nodeIdentifier)
        {
            return _graph.CreateUriNode(prefix + ":" + RemoveNotAllowedChars(nodeIdentifier));
        }

        internal string GetUriFriendlyName(MsProjectXML.ProjectTask task)
        {
            return task?.Name == null ? "Task_" + task?.UID : "Task_" + task.UID + "_" + RemoveNotAllowedChars(task.Name);

        }
    }
}
