using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFLab.Messaging {
	//public static class MessengerExtensions {
	//	/// <summary>
	//	/// Usage: new TextMessage("Test").Send();
	//	/// Returns the input message for chaining. 
	//	/// </summary>
	//	/// <typeparam name="T"></typeparam>
	//	/// <param name="msg"></param>
	//	public static T Send<T>(this T msg) {
	//		Messenger.Send(msg);
	//		return msg;
	//	}
	//}
	class MessageDescriptor {
		public object Receiver;
		public Type Type;
		public object Action;
	}
	
	public interface IMessenger {
		T Send<T>(T message);
		void Register<T>(Action<T> action);
	}

	public class Messenger : IMessenger {
		private readonly List<MessageDescriptor> actions = new List<MessageDescriptor>();

		/// <summary>
		/// Registers an action for the given receiver. WARNING: You have to unregister the action to avoid memory leaks!
		/// </summary>
		/// <typeparam name="T">Type of the message</typeparam>
		/// <param name="receiver">Receiver to use as identifier</param>
		/// <param name="action">Action to register</param>
		public void Register<T>(object receiver, Action<T> action) {
			actions.Add(new MessageDescriptor { Receiver = receiver, Type = typeof(T), Action = action });
		}

		/// <summary>
		/// Registers an action for no receiver. 
		/// </summary>
		/// <typeparam name="T">Type of the message</typeparam>
		/// <param name="action">Action to register</param>
		public void Register<T>(Action<T> action) {
			Register(null, action);
		}

		/// <summary>
		/// Unregisters all actions with no receiver
		/// </summary>
		public void Unregister() {
			Unregister(null);
		}

		/// <summary>
		/// Unregisters all actions with the given receiver
		/// </summary>
		/// <param name="receiver"></param>
		public void Unregister(object receiver) {
			foreach (var a in actions.Where(a => a.Receiver == receiver).ToArray())
				actions.Remove(a);
		}

		/// <summary>
		/// Unregisters the specified action
		/// </summary>
		/// <typeparam name="T">Type of the message</typeparam>
		/// <param name="action">Action to unregister</param>
		public void Unregister<T>(Action<T> action) {
			foreach (var a in actions.Where(a => (Action<T>)a.Action == action).ToArray())
				actions.Remove(a);
		}

		/// <summary>
		/// Unregisters the specified action
		/// </summary>
		/// <typeparam name="T">Type of the message</typeparam>
		public void Unregister<T>() {
			foreach (var a in actions.Where(a => a.Type == typeof(T)).ToArray())
				actions.Remove(a);
		}

		/// <summary>
		/// Unregisters the specified action
		/// </summary>
		/// <param name="receiver"></param>
		/// <typeparam name="T">Type of the message</typeparam>
		public void Unregister<T>(object receiver) {
			foreach (var a in actions.Where(a => a.Receiver == receiver && a.Type == typeof(T)).ToArray())
				actions.Remove(a);
		}

		/// <summary>
		/// Unregisters an action for the specified receiver. 
		/// </summary>
		/// <typeparam name="T">Type of the message</typeparam>
		/// <param name="receiver"></param>
		/// <param name="action"></param>
		public void Unregister<T>(object receiver, Action<T> action) {
			foreach (var a in actions.Where(a => a.Receiver == receiver && (Action<T>)a.Action == action).ToArray())
				actions.Remove(a);
		}

		/// <summary>
		/// Sends a message to the registered receivers. 
		/// </summary>
		/// <typeparam name="T">Type of the message</typeparam>
		/// <param name="message"></param>
		public T Send<T>(T message) {
			var type = typeof(T);
			foreach (var a in actions.Where(a => a.Type == type).ToArray()) {
				((Action<T>)a.Action)(message);
			}
			return message;
		}
	}
}
