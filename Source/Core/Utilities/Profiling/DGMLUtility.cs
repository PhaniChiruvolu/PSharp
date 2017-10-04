﻿using System.Xml;
using System.Xml.Serialization;

namespace Core.Utilities.Profiling
{
    /// <summary>
    /// A node in the DGML representation
    /// </summary>
    public struct Node
    {
        /// <summary>
        /// Unique id for the node
        /// </summary>
        [XmlAttribute]
        public string Id;

        /// <summary>
        /// Label for the node
        /// </summary>
        [XmlAttribute]
        public string Label;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        public Node(string id, string label)
        {
            this.Id = id;
            this.Label = label;
        }
    }

    /// <summary>
    /// An edge in the DGML representation
    /// </summary>
    public struct Link
    {
        /// <summary>
        /// The source node.
        /// </summary>
        [XmlAttribute]
        public string Source;

        /// <summary>
        /// The target node.
        /// </summary>
        [XmlAttribute]
        public string Target;

        /// <summary>
        /// The edge label.
        /// </summary>
        [XmlAttribute]
        public string Label;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="label"></param>
        public Link(string source, string target, string label)
        {
            this.Source = source;
            this.Target = target;
            this.Label = label;
        }
    }

    /// <summary>
    /// The Graph used internally by the XML writer
    /// </summary>
    public struct Graph
    {
        /// <summary>
        /// Nodes to be serialized
        /// </summary>
        public Node[] Nodes;

        /// <summary>
        /// Links to be serialized
        /// </summary>
        public Link[] Links;
    }
}