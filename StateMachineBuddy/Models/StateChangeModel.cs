using System.Xml;
using XmlBuddy;

namespace StateMachineBuddy
{
    /// <summary>
    /// Represents a state transition triggered by a message in the serializable state machine model.
    /// </summary>
    public class StateChangeModel : XmlObject
    {
        #region Properties

        /// <summary>
        /// Gets or sets the message that triggers this transition.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the target state after the transition.
        /// </summary>
        public string TargetState { get; set; }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the StateChangeModel class.
        /// </summary>
        public StateChangeModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the StateChangeModel class with the specified message and target state.
        /// </summary>
        /// <param name="message">The message that triggers this transition.</param>
        /// <param name="targetState">The target state after the transition.</param>
        public StateChangeModel(string message, string targetState)
        {
            Message = message;
            TargetState = targetState;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
