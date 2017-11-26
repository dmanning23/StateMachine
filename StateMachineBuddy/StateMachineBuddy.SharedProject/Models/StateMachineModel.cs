using FilenameBuddy;
using System.Collections.Generic;
using System.Xml;
using XmlBuddy;

namespace StateMachineBuddy.Models
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
		public string Initial { get; private set; }

		/// <summary>
		/// list of all the states in the state machine
		/// </summary>
		public List<string> StateNames { get; private set; }

		/// <summary>
		/// list of all the messages in the state machine
		/// </summary>
		public List<string> MessageNames { get; private set; }

		/// <summary>
		/// all the state transition data for this state machine
		/// </summary>
		public List<StateTableModel> States { get; private set; }

		#endregion //Properties

		#region Methods

		public StateMachineModel(Filename file) : base("StateMachine", file)
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
				case "stateNames":
					{
						ReadChildNodes(node, ParseStateNames);
					}
					break;
				case "messageNames":
					{
						ReadChildNodes(node, ParseMessageNames);
					}
					break;
				case "states":
					{
						ReadChildNodes(node, ParseStates);
					}
					break;
				default:
					{
						NodeError(node);
					}
					break;
			}
		}

		private void ParseStateNames(XmlNode node)
		{
			StateNames.Add(node.InnerText);
		}

		private void ParseMessageNames(XmlNode node)
		{
			MessageNames.Add(node.InnerText);
		}

		private void ParseStates(XmlNode node)
		{
			var stateTable = new StateTableModel();
			XmlFileBuddy.ReadChildNodes(node, stateTable.ParseXmlNode);
			States.Add(stateTable);
		}

#if !WINDOWS_UWP
		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
		{
			xmlWriter.WriteAttributeString("initial", Initial);

			xmlWriter.WriteStartElement("stateNames");
			foreach (var state in StateNames)
			{
				xmlWriter.WriteStartElement("state");
				xmlWriter.WriteAttributeString("name", state);
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();

			xmlWriter.WriteStartElement("messageNames");
			foreach (var message in MessageNames)
			{
				xmlWriter.WriteStartElement("message");
				xmlWriter.WriteAttributeString("name", message);
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();

			xmlWriter.WriteStartElement("states");
			foreach (var state in States)
			{
				state.WriteXmlNodes(xmlWriter);
			}
			xmlWriter.WriteEndElement();
		}
#endif

#endregion //Methods
	}
}
