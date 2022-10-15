using System.Collections.Generic;
using DotNetBrightener.MultiTenancy.Contexts;

namespace DotNetBrightener.MultiTenancy.Services
{
    /// <summary>
    ///		Represents APIs to initialize the entire application to contain multiple <see cref="TenantSetting" />
    /// </summary>
    public interface IApplicationHost
	{
		/// <summary>
		///		Ensure that all the <see cref="TenantContext"/> are created and available to process requests. 
		/// </summary>
		void Initialize();

		/// <summary>
		///		Returns an existing <see cref="TenantContext"/> or creates a new one if necessary. 
		/// </summary>
		/// <param name="settings">
		///		The <see cref="TenantSetting"/> object representing the shell to get.
		/// </param>
		/// <returns></returns>
		TenantContext GetOrCreateTenantContext(ITenant settings);

		/// <summary>
		///		Updates an existing <see cref="ITenant"/>. It will then reload the tenant.
		/// </summary>
		/// <param name="settings">The <see cref="ITenant" /> to update</param>
		void UpdateTenantSetting(ITenant settings);

		/// <summary>
		///		Reloads a tenant
		/// </summary>
		/// <param name="settings">The tenant to reload</param>
		void ReloadTenant(ITenant settings);

        /// <summary>
        ///		Removes a tenant from the application
        /// </summary>
        /// <param name="settings">The tenant settings to remove</param>
	    void RemoveTenant(ITenant settings);

		/// <summary>
		///		Lists all available <see cref="TenantContext"/> instances. 
		/// </summary>
		/// <remarks>A shell might not be listed if it hasn't been created yet, for instance if it has been removed and not yet recreated.</remarks>
		IEnumerable<TenantContext> ListTenantContexts();
	}
}