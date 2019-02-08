﻿using FilenameBuddy;
using System.Collections.Generic;
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

		public StateMachineModel(Filename file, HybridStateMachine stateMachine) : this(file)
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
				States.Add(new StateTableModel(stateTable.Value, stateTable.Key));
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
						//legacy statemachine XML
						ReadChildNodes(node, ParseStateNames);
					}
					break;
				case "states1":
					{
						ReadChildNodes(node, ParseStates);
					}
					break;
				case "messageNames":
					{
						//legacy statemachine XML
						ReadChildNodes(node, ParseMessageNames);
					}
					break;
				case "messages":
					{
						ReadChildNodes(node, ParseMessages);
					}
					break;
				case "states":
					{
						//legacy statemachine XML
						ReadChildNodes(node, ParseStateChanges);
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

		private void ParseStateNames(XmlNode node)
		{
			StateNames.Add(node.InnerText);
		}

		private void ParseMessageNames(XmlNode node)
		{
			MessageNames.Add(node.InnerText);
		}

		private void ParseStateChanges(XmlNode node)
		{
			var stateTable = new StateTableModel();
			XmlFileBuddy.ReadChildNodes(node, stateTable.ParseXmlNode);
			States.Add(stateTable);
		}

#if !WINDOWS_UWP
		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
		{
			xmlWriter.WriteAttributeString("initial", Initial);

			xmlWriter.WriteStartElement("states1");
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
