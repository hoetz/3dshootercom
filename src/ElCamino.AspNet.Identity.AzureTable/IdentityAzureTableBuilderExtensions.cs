// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.

#if !net45
using System;
using Microsoft.AspNet.Identity;
using ElCamino.AspNet.Identity.AzureTable;
using ElCamino.AspNet.Identity.AzureTable.Model;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityAzureTableBuilderExtensions
    {

        public static IdentityBuilder AddAzureTableStores<TContext>(this IdentityBuilder builder, Func<IdentityConfiguration> configAction)
            where TContext : IdentityCloudContext, new()
        {
            builder.Services.AddSingleton<IdentityConfiguration>(new Func<IServiceProvider, IdentityConfiguration>(p => configAction()));
            builder.Services.AddScoped(typeof(IdentityCloudContext), typeof(TContext));

            var defaultServices = IdentityAzureTableServices.GetDefaultServices(builder.UserType,
                builder.RoleType,
                typeof(TContext));
            foreach (var service in defaultServices)
            {
                builder.Services.Add(service);
            }
            return builder;
        }


    }
}
#endif