using System.Collections.Generic;
using System.Xml;
using XmlBuddy;

namespace StateMachineBuddy.Models
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
		public string Name { get; private set; }

		/// <summary>
		/// list of all the state changes for this state
		/// </summary>
		public List<StateChangeModel> Transitions { get; private set; }

		#endregion //Properties

		#region Methods

		public StateTableModel()
		{
			Transitions = new List<StateChangeModel>();
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

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
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

		private void ParseStateTransitions(XmlNode node)
		{
			var stateChange = new StateChangeModel();
			XmlFileBuddy.ReadChildNodes(node, stateChange.ParseXmlNode);
			Transitions.Add(stateChange);
		}

#if !WINDOWS_UWP
		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("transitions");
			xmlWriter.WriteAttributeString("name", Name);
			foreach (var transition in Transitions)
			{
				transition.WriteXmlNodes(xmlWriter);
			}
			xmlWriter.WriteEndElement();
		}
#endif

		#endregion //Methods
	}
}
