﻿using System;
using System.Collections.Concurrent;

using Autofac;

using Microsoft.AspNet.SignalR.Hubs;


namespace SignalR.Extras.Autofac
{

	internal class LifetimeHubManager : IDisposable
	{

		public T ResolveHub<T>(Type type, ILifetimeScope lifetimeScope)
			where T : ILifetimeHub
		{
			var scope = lifetimeScope.BeginLifetimeScope();
			var hub = (T)scope.Resolve(type);
			hub.OnDisposing += HubOnDisposing;
			_hubLifetimeScopes.TryAdd(hub, scope);
			return hub;
		}


		/// <summary>
		/// If the LifetimeHubManager is disposed, make sure that all lifetime-scopes that it's
		/// still tracking also get disposed.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		protected virtual void Dispose(bool disposing)
		{
			foreach (var hub in _hubLifetimeScopes.Keys) {
				hub.Dispose();
			}
		}


		/// <summary>
		/// Make sure that the lifetime-scope associated with the hub is disposed
		/// </summary>
		private void HubOnDisposing(object sender, EventArgs eventArgs)
		{
			ILifetimeScope scope;
			var hub = sender as IHub;
			if (hub != null && _hubLifetimeScopes.TryRemove(hub, out scope)) {
				scope?.Dispose();
			}
		}


		private readonly ConcurrentDictionary<IHub, ILifetimeScope> _hubLifetimeScopes = new ConcurrentDictionary<IHub, ILifetimeScope>();
	}

}
