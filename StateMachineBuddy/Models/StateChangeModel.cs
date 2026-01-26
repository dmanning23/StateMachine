using System.Xml;
using XmlBuddy;

namespace StateMachineBuddy
{
    /// <summary>
    /// this object records the change from one state to another when the state machine receives a message
    /// </summary>
    public class StateChangeModel : XmlObject
    {
        #region Properties

        /// <summary>
        /// message recieved, must match one of teh messages in the state machine
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// target state, must match one of the states in the state machine
        /// </summary>
        public string TargetState { get; set; }

        #endregion //Properties

        #region Methods

        public StateChangeModel()
        {
        }

        public StateChangeModel(string message, string targetState)
        {
            Message = message;
            TargetState = targetState;
        }

        public override void ParseXmlNode(XmlNode node)
        {
            //what is in this node?
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "message":
                    {
                        Message = value;
                    }
                    break;
                case "state":
                    {
                        TargetState = value;
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        public override void WriteXmlNodes(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("transition");
            xmlWriter.WriteAttributeString("message", Message);
            xmlWriter.WriteAttributeString("state", TargetState);
            xmlWriter.WriteEndElement();
        }

        #endregion //Methods
    }
}
