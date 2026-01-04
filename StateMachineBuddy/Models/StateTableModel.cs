using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XmlBuddy;

namespace StateMachineBuddy
{
    /// <summary>
    /// this is a list of all the state changes for one state
    /// </summary>
    public class StateTableModel : XmlObject
    {
        #region Properties

        /// <summary>
        /// name of this state, must match one of the states in the state machine
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// list of all the state changes for this state
        /// </summary>
        public List<StateChangeModel> Transitions { get; set; }

        public bool AddAllMessages { get; set; }

        #endregion //Properties

        #region Methods

        public StateTableModel()
        {
            Transitions = new List<StateChangeModel>();
        }

        public StateTableModel(string name) : this()
        {
            Name = name;
        }

        public StateTableModel(StateMachine stateMachine, int stateIndex) : this()
        {
            Name = stateMachine.GetStateName(stateIndex);

            for (var i = 0; i < stateMachine.NumMessages; i++)
            {
                var targetState = stateMachine.GetEntry(stateIndex, i);
                if (targetState != stateIndex)
                {
                    Transitions.Add(new StateChangeModel(stateMachine, targetState, i));
                }
            }
        }

        public StateTableModel(State state, string stateName, bool addAllMessages = false) : this()
        {
            Name = stateName;
            AddAllMessages = addAllMessages;

            foreach (var transition in state.StateChanges)
            {
                if (transition.Value != stateName)
                {
                    Transitions.Add(new StateChangeModel(transition.Key, transition.Value));
                }
            }
        }

        public override void ParseXmlNode(XmlNode node)
        {
            //what is in this node?
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "type":
                    {
                        //Really skip these old ass nodes
                    }
                    break;
                case "name":
                    {
                        Name = value;
                    }
                    break;
                case "transition":
                    {
                        var stateChange = new StateChangeModel(node.Attributes["message"].InnerText, node.Attributes["state"].InnerText);
                        Transitions.Add(stateChange);
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

        private void ParseStateTransitions(XmlNode node)
        {
            var stateChange = new StateChangeModel();
            XmlFileBuddy.ReadChildNodes(node, stateChange.ParseXmlNode);
            Transitions.Add(stateChange);
        }

        public override void WriteXmlNodes(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("state");
            xmlWriter.WriteAttributeString("name", Name);
            xmlWriter.WriteStartElement("transitions");

            //sort the transitions
            Transitions = Transitions.OrderBy(x => x.Message).ToList();

            foreach (var transition in Transitions)
            {
                if (transition.State != Name || AddAllMessages)
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
