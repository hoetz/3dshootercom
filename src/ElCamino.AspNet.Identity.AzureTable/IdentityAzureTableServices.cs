﻿// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.

#if !net45
using System;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using ElCamino.AspNet.Identity.AzureTable;
using System.Collections.Generic;

namespace Microsoft.AspNet.Identity
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

            ServiceDescriptor sdUserStore = new ServiceDescriptor(typeof(IUserStore<>).MakeGenericType(userType),
                userStoreType);
            ServiceDescriptor sdroleStore = new ServiceDescriptor(typeof(IRoleStore<>).MakeGenericType(roleType),
            roleStoreType);

            return new ServiceDescriptor[] { sdUserStore, sdroleStore };

        }
    }
}
#endif