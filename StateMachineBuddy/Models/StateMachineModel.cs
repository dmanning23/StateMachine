using FilenameBuddy;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XmlBuddy;

namespace StateMachineBuddy
{
    /// <summary>
    /// this is all the data for a state machine
    /// </summary>
    public class StateMachineModel : XmlFileBuddy
    {
        #region Properties

        /// <summary>
        /// the initial state of this state machine
        /// </summary>
        public string Initial { get; set; }

        /// <summary>
        /// list of all the states in the state machine
        /// </summary>
        public List<string> StateNames { get; set; } = new List<string>();

        /// <summary>
        /// list of all the messages in the state machine
        /// </summary>
        public List<string> MessageNames { get; set; } = new List<string>();

        /// <summary>
        /// all the state transition data for this state machine
        /// </summary>
        public List<StateModel> States { get; set; } = new List<StateModel>();

        #endregion //Properties

        #region Methods

        public StateMachineModel() : base("StateMachine")
        {
        }

        public StateMachineModel(Filename file) : base("StateMachine", file)
        {
        }

        public StateMachineModel(Filename file, StringStateMachine stateMachine) : this(file)
        {
            Initial = stateMachine.InitialState;
            foreach (var stateName in stateMachine.States)
            {
                StateNames.Add(stateName);
            }
            foreach (var messageName in stateMachine.Messages)
            {
                MessageNames.Add(messageName);
            }

            foreach (var state in stateMachine.StateTable)
            {
                States.Add(new StateModel(state.Key, state.Value));
            }
        }

        public override void ParseXmlNode(XmlNode node)
        {
            //what is in this node?
            var name = node.Name;
            var value = node.InnerText;

            switch (name)
            {
                case "initial":
                    {
                        Initial = value;
                    }
                    break;
                case "states":
                    {
                        ReadChildNodes(node, ParseStates);
                    }
                    break;
                case "messages":
                    {
                        ReadChildNodes(node, ParseMessages);
                    }
                    break;
                case "stateChanges":
                    {
                        ReadChildNodes(node, ParseStateChanges);
                    }
                    break;
                default:
                    {
                        NodeError(node);
                    }
                    break;
            }
        }

        private void ParseStates(XmlNode node)
        {
            StateNames.Add(node.Attributes["name"].InnerText);
        }

        private void ParseMessages(XmlNode node)
        {
            MessageNames.Add(node.Attributes["name"].InnerText);
        }

        private void ParseStateChanges(XmlNode node)
        {
            var stateTable = new StateModel();
            XmlFileBuddy.ReadChildNodes(node, stateTable.ParseXmlNode);
            States.Add(stateTable);
        }

        public override void WriteXmlNodes(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("initial", Initial);

            xmlWriter.WriteStartElement("states");

            foreach (var state in StateNames)
            {
                xmlWriter.WriteStartElement("state");
                xmlWriter.WriteAttributeString("name", state);
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("messages");

            foreach (var message in MessageNames)
            {
                xmlWriter.WriteStartElement("message");
                xmlWriter.WriteAttributeString("name", message);
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("stateChanges");

            States = States.OrderBy(x => x.Name).ToList();

            foreach (var state in States)
            {
                state.WriteXmlNodes(xmlWriter);
            }
            xmlWriter.WriteEndElement();
        }

        #endregion //Methods
    }
}
