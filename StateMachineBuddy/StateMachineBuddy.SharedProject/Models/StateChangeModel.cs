using System.Xml;
using XmlBuddy;

namespace StateMachineBuddy.Models
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
		public string Message { get; private set; }

		/// <summary>
		/// target state, must match one of the states in the state machine
		/// </summary>
		public string State { get; private set; }

		#endregion //Properties

		#region Methods

		public StateChangeModel()
		{
		}

		public StateChangeModel(StateMachine stateMachine, int state, int message)
		{
			Message = stateMachine.GetMessageName(message);
			State = stateMachine.GetStateName(state);
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
				case "message":
					{
						Message = value;
					}
					break;
				case "state":
					{
						State = value;
					}
					break;
				default:
					{
						base.ParseXmlNode(node);
					}
					break;
			}
		}

#if !WINDOWS_UWP
		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("transition");
			xmlWriter.WriteAttributeString("message", Message);
			xmlWriter.WriteAttributeString("state", State);
			xmlWriter.WriteEndElement();
		}
#endif

#endregion //Methods
	}
}
