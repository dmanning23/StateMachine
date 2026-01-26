using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XmlBuddy;

namespace StateMachineBuddy
{
    /// <summary>
    /// this is a list of all the state changes for one state
    /// </summary>
    public class StateModel : XmlObject
    {
        #region Properties

        /// <summary>
        /// name of this state, must match one of the states in the state machine
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// list of all the state changes for this state
        /// </summary>
        public List<StateChangeModel> Transitions { get; set; } = new List<StateChangeModel>();

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the StateModel class.
        /// </summary>
        public StateModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the StateModel class from a state name and state object.
        /// </summary>
        /// <param name="name">The name of the state.</param>
        /// <param name="state">The state containing transition data.</param>
        public StateModel(string name, State state)
        {
            Name = name;

            foreach (var transition in state.StateChanges)
            {
                if (transition.Value != Name)
                {
                    Transitions.Add(new StateChangeModel(transition.Key, transition.Value));
                }
            }
        }

        /// <inheritdoc/>
        public override void ParseXmlNode(XmlNode node)
        {
            //what is in this node?
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "name":
                    {
                        Name = value;
                    }
                    break;
                case "transitions":
                    {
                        XmlFileBuddy.ReadChildNodes(node, ParseStateTransitions);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        /// <summary>
        /// Parses a state transition node from XML.
        /// </summary>
        /// <param name="node">The XML node to parse.</param>
        private void ParseStateTransitions(XmlNode node)
        {
            var stateChange = new StateChangeModel();
            XmlFileBuddy.ReadChildNodes(node, stateChange.ParseXmlNode);
            Transitions.Add(stateChange);
        }

        /// <inheritdoc/>
        public override void WriteXmlNodes(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("state");
            xmlWriter.WriteAttributeString("name", Name);
            xmlWriter.WriteStartElement("transitions");

            //sort the transitions
            Transitions = Transitions.OrderBy(x => x.Message).ToList();

            foreach (var transition in Transitions)
            {
                if (transition.TargetState != Name)
                {
                    transition.WriteXmlNodes(xmlWriter);
                }
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
        }

        #endregion //Methods
    }
}
