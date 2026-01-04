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
        public List<string> StateNames { get; set; }

        /// <summary>
        /// list of all the messages in the state machine
        /// </summary>
        public List<string> MessageNames { get; set; }

        /// <summary>
        /// all the state transition data for this state machine
        /// </summary>
        public List<StateTableModel> States { get; set; }

        #endregion //Properties

        #region Methods

        public StateMachineModel() : base("StateMachine")
        {
            Setup();
        }

        public StateMachineModel(Filename file) : base("StateMachine", file)
        {
            Setup();
        }

        private void Setup()
        {
            StateNames = new List<string>();
            MessageNames = new List<string>();
            States = new List<StateTableModel>();
        }

        public StateMachineModel(Filename file, StateMachine stateMachine) : this(file)
        {
            Initial = stateMachine.GetStateName(stateMachine.InitialState);
            for (var i = 0; i < stateMachine.NumStates; i++)
            {
                StateNames.Add(stateMachine.GetStateName(i));
            }
            for (var i = 0; i < stateMachine.NumMessages; i++)
            {
                MessageNames.Add(stateMachine.GetMessageName(i));
            }

            for (var i = 0; i < stateMachine.NumStates; i++)
            {
                States.Add(new StateTableModel(stateMachine, i));
            }
        }

        public StateMachineModel(Filename file, HybridStateMachine stateMachine, bool addAllMessages = false) : this(file)
        {
            Initial = stateMachine.InitialState;
            foreach (var state in stateMachine.States)
            {
                StateNames.Add(state);
            }
            foreach (var message in stateMachine.Messages)
            {
                MessageNames.Add(message);
            }

            foreach (var stateTable in stateMachine.StateTable)
            {
                States.Add(new StateTableModel(stateTable.Value, stateTable.Key, addAllMessages));
            }

            if (addAllMessages)
            {
                foreach (var state in States)
                {
                    foreach (var message in MessageNames)
                    {
                        if (!state.Transitions.Any(x => x.Message == message))
                        {
                            state.Transitions.Add(new StateChangeModel(message, state.Name));
                        }
                    }
                }
            }
        }

        public override void ParseXmlNode(XmlNode node)
        {
            //what is in this node?
            var name = node.Name;
            var value = node.InnerText;

            switch (name)
            {
                case "Asset":
                    {
                        //skip these old ass nodes
                        XmlFileBuddy.ReadChildNodes(node, ParseXmlNode);
                    }
                    break;
                case "Type":
                    {
                        //Really skip these old ass nodes
                    }
                    break;
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
            var stateTable = new StateTableModel();
            XmlFileBuddy.ReadChildNodes(node, stateTable.ParseXmlNode);
            States.Add(stateTable);
        }

        public override void WriteXmlNodes(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("initial", Initial);

            xmlWriter.WriteStartElement("states");

            StateNames = StateNames.OrderBy(x => x).ToList();

            foreach (var state in StateNames)
            {
                xmlWriter.WriteStartElement("state");
                xmlWriter.WriteAttributeString("name", state);
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("messages");

            MessageNames = MessageNames.OrderBy(x => x).ToList();

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
