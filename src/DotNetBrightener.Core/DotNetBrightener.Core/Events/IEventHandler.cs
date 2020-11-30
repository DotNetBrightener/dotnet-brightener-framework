using System.Threading.Tasks;

namespace DotNetBrightener.Core.Events
{
	public interface IEventHandler { }

	public interface IEventHandler<T>  : IEventHandler
		where T : BaseEventMessage
	{
		/// <summary>
		///		Retrieves the priority of the event handler, the higher number will be run first
		/// </summary>
		int Priority { get; }

		/// <summary>
		///		Process the given event message
		/// </summary>
		/// <param name="eventMessage">The event message</param>
		/// <returns>An asynchronous task for non-blocking thread</returns>
		Task HandleEvent(T eventMessage);
	}
}