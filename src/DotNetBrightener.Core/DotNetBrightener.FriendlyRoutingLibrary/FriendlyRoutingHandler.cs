using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.FriendlyRoutingLibrary
{
    /// <summary>
	///		A custom router for handling friendly routes to specific content.
	///		When applied, a URL like '/path-to-some-dynamic-content' can be routed to 
	///		the content's specific controller/action like [Content]/[DisplayContent]/{content-id}.
	///		So we can have a single controller/action with different parameters to handle multiple and dynamic URLs
	/// </summary>
	public class FriendlyRoutingHandler : IRouter
	{
	    private readonly IActionContextAccessor _actionContextAccessor;
	    private readonly IActionInvokerFactory  _actionInvokerFactory;
	    private readonly IActionSelector        _actionSelector;
	    private readonly ILogger                _logger;

        public FriendlyRoutingHandler(IActionInvokerFactory actionInvokerFactory,
                                      FriendlyRouteActionSelector actionSelector,
                                      ILoggerFactory loggerFactory)
			: this(actionInvokerFactory, actionSelector, loggerFactory, actionContextAccessor: null)
		{
		}

		public FriendlyRoutingHandler(
            IActionInvokerFactory actionInvokerFactory,
            FriendlyRouteActionSelector actionSelector,
            ILoggerFactory loggerFactory,
            IActionContextAccessor actionContextAccessor)
		{
			_actionContextAccessor = actionContextAccessor;

			_actionInvokerFactory = actionInvokerFactory;
			_actionSelector       = actionSelector;
			_logger               = loggerFactory.CreateLogger<FriendlyRoutingHandler>();
		}

		public VirtualPathData GetVirtualPath(VirtualPathContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			// We return null here because we're not responsible for generating the url, the route is.
			return null;
		}

		public Task RouteAsync(RouteContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			var candidates = _actionSelector.SelectCandidates(context);
			if (candidates == null || candidates.Count == 0)
			{
				//_logger.NoActionsMatched(context.RouteData.Values);

				return Task.CompletedTask;
			}

			var actionDescriptor = _actionSelector.SelectBestCandidate(context, candidates);
			if (actionDescriptor == null)
			{
				//_logger.NoActionsMatched(context.RouteData.Values);
				return Task.CompletedTask;
			}

			context.Handler = (c) =>
							  {
								  var routeData = c.GetRouteData();

								  var actionContext = new ActionContext(context.HttpContext, routeData, actionDescriptor);
								  if (_actionContextAccessor != null)
								  {
									  _actionContextAccessor.ActionContext = actionContext;
								  }

								  var invoker = _actionInvokerFactory.CreateInvoker(actionContext);
								  if (invoker == null)
								  {
									  throw new InvalidOperationException($"Cannot create invoker {actionDescriptor.DisplayName}");
								  }

								  return invoker.InvokeAsync();
							  };

			return Task.CompletedTask;
		}
	}
}