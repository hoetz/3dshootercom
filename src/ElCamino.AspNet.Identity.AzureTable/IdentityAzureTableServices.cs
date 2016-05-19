// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.

#if !net45
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ElCamino.AspNet.Identity.AzureTable;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Identity
{
    /// <summary>
    /// Default services
    /// </summary>
    public class IdentityAzureTableServices
    {
        public static ICollection<ServiceDescriptor> GetDefaultServices(Type userType,
            Type roleType,
            Type contextType,
            IConfiguration config = null)
        {
            Type userStoreType = typeof(UserStore<,,>).MakeGenericType(userType, roleType, contextType);
            Type roleStoreType = typeof(RoleStore<,>).MakeGenericType(roleType, contextType);


            var services = new ServiceCollection();
            services.AddScoped(
                typeof(IUserStore<>).MakeGenericType(userType),
                userStoreType);
            services.AddScoped(
                typeof(IRoleStore<>).MakeGenericType(roleType),
                roleStoreType);
            return services;
        }

    }
}
#endif