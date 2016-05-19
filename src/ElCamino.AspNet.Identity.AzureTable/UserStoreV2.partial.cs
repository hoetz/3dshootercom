﻿// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
#if !net45
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;
using Microsoft.WindowsAzure.Storage.Table;
using ElCamino.AspNet.Identity.AzureTable.Helpers;
using System.Threading;
using Microsoft.AspNetCore.Identity;

namespace ElCamino.AspNet.Identity.AzureTable
{
    public class UserStore<TUser> : UserStore<TUser, IdentityCloudContext>
        where TUser : Model.IdentityUser, new()
    {
        public UserStore()
            : this(new IdentityCloudContext())
        {
        }

        public UserStore(IdentityCloudContext context)
            : base(context)
        {
        }
    }

    public class UserStore<TUser, TContext> : UserStore<TUser, Model.IdentityRole, TContext>
		, IUserStore<TUser> 
		where TUser : Model.IdentityUser, new()
		where TContext : IdentityCloudContext, new()
    {
		public UserStore(TContext context)
			: base(context)
		{
		}

		/// <summary>
		/// Simple table queries allowed. Projections are only allowed for TUser types. 
		/// </summary>
		public override IQueryable<TUser> Users
		{
			get
			{
				//TODO: Fix IQueryable
				//TableQueryHelper<TUser> helper = new TableQueryHelper<TUser>(
				//	(from t in base.Users
				//	 where t.RowKey.CompareTo(Constants.RowKeyConstants.PreFixIdentityUserName) > 0
				//	 select t).AsTableQuery()
				//	, base.GetUserAggregateQuery);

				//return helper;
				throw new NotImplementedException();

			}
		}
		//Fixing code analysis issue CA1063
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}

	public class UserStore<TUser, TRole, TContext> : UserStore<TUser, TRole, string, Model.IdentityUserLogin, Model.IdentityUserRole, Model.IdentityUserClaim, TContext>
		, IUserStore<TUser>
		where TUser : Model.IdentityUser, new()
		where TRole : Model.IdentityRole<string, Model.IdentityUserRole>, new()
		where TContext : IdentityCloudContext, new()
	{
		public UserStore(TContext context)
			: base(context)
		{
		}
	}

    public class UserStore<TUser, TRole, TKey, TUserLogin, TUserRole, TUserClaim, TContext> : 
		IUserLoginStore<TUser>
		, IUserClaimStore<TUser>
		, IUserRoleStore<TUser>
		, IUserPasswordStore<TUser>
		, IUserSecurityStampStore<TUser>
		//, IQueryableUserStore<TUser>
		, IUserEmailStore<TUser>
		, IUserPhoneNumberStore<TUser>
		, IUserTwoFactorStore<TUser>
		, IUserLockoutStore<TUser>
		, IUserStore<TUser>
		, IDisposable
		where TUser : Model.IdentityUser<TKey, TUserLogin, TUserRole, TUserClaim>, new()
		where TRole : Model.IdentityRole<TKey, TUserRole>, new()
		where TKey : IEquatable<TKey>
		where TUserLogin : Model.IdentityUserLogin<TKey>, new()
		where TUserRole : Model.IdentityUserRole<TKey>, new()
		where TUserClaim : Model.IdentityUserClaim<TKey>, new()
		where TContext : IdentityCloudContext, new()
    {
		private bool _disposed;

		private CloudTable _userTable;
		private CloudTable _indexTable;

		public UserStore(TContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}
			this.Context = context;
			this._userTable = context.UserTable;
			this._indexTable = context.IndexTable;
		}

		public async Task CreateTablesIfNotExists()
		{
			Task<bool>[] tasks = new Task<bool>[]
					{
						Context.RoleTable.CreateIfNotExistsAsync(),
						Context.UserTable.CreateIfNotExistsAsync(),
						Context.IndexTable.CreateIfNotExistsAsync(),
					};
			await Task.WhenAll(tasks);
		}

		public virtual async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (claims == null)
			{
				throw new ArgumentNullException("claims");
			}

			BatchOperationHelper bop = new BatchOperationHelper();
			foreach (Claim c in claims)
			{
				TUserClaim item = Activator.CreateInstance<TUserClaim>();
				item.UserId = user.Id;
				item.ClaimType = c.Type;
				item.ClaimValue = c.Value;
				((Model.IGenerateKeys)item).GenerateKeys();

				user.Claims.Add(item);

				bop.Add(TableOperation.Insert(item));
			}

			await bop.ExecuteBatchAsync(_userTable);
		}

        public virtual async Task AddClaimAsync(TUser user, Claim claim)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (claim == null)
			{
				throw new ArgumentNullException("claim");
			}
			TUserClaim item = Activator.CreateInstance<TUserClaim>();
			item.UserId = user.Id;
			item.ClaimType = claim.Type;
			item.ClaimValue = claim.Value;
			((Model.IGenerateKeys)item).GenerateKeys();

			user.Claims.Add(item);

			await _userTable.ExecuteAsync(TableOperation.Insert(item));
		}

		public virtual async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (login == null)
			{
				throw new ArgumentNullException("login");
			}
			TUserLogin item = Activator.CreateInstance<TUserLogin>();
			item.UserId = user.Id;
			item.ProviderKey = login.ProviderKey;
			item.LoginProvider = login.LoginProvider;
			((Model.IGenerateKeys)item).GenerateKeys();

			user.Logins.Add(item);
			Model.IdentityUserIndex index = CreateLoginIndex(item.UserId.ToString(), item);

			await Task.WhenAll(_userTable.ExecuteAsync(TableOperation.Insert(item))
				, _indexTable.ExecuteAsync(TableOperation.InsertOrReplace(index)));

		}

		public virtual async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (string.IsNullOrWhiteSpace(roleName))
			{
				throw new ArgumentException(IdentityResources.ValueCannotBeNullOrEmpty, "roleName");
			}

			TRole roleT = Activator.CreateInstance<TRole>();
			roleT.Name = roleName;
			((Model.IGenerateKeys)roleT).GenerateKeys();

			TUserRole userToRole = Activator.CreateInstance<TUserRole>();
			userToRole.UserId = user.Id;
			userToRole.RoleId = roleT.Id;
			userToRole.RoleName = roleT.Name;
			TUserRole item = userToRole;

			((Model.IGenerateKeys)item).GenerateKeys();

			user.Roles.Add(item);
			roleT.Users.Add(item);

			await _userTable.ExecuteAsync(TableOperation.Insert(item));

		}

		public async virtual Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			((Model.IGenerateKeys)user).GenerateKeys();

			List<Task> tasks = new List<Task>(2);
			tasks.Add(_userTable.ExecuteAsync(TableOperation.Insert(user)));

			if (!string.IsNullOrWhiteSpace(user.Email))
			{
				Model.IdentityUserIndex index = CreateEmailIndex(user.Id.ToString(), user.Email);
				tasks.Add(_indexTable.ExecuteAsync(TableOperation.InsertOrReplace(index)));
			}

			try
			{
				await Task.WhenAll(tasks.ToArray());
				return IdentityResult.Success;
			}
			catch (AggregateException aggex)
			{
				aggex.Flatten();
				return IdentityResult.Failed(new IdentityError() { Code = "001", Description = "User Creation Failed." });
			}
		}

		public async virtual Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			List<Task> tasks = new List<Task>(50);

			BatchOperationHelper userBatch = new BatchOperationHelper();

			userBatch.Add(TableOperation.Delete(user));
			//Don't use the BatchHelper for login index table, partition keys are likely not the same
			//since they are based on provider
			foreach (var userLogin in user.Logins)
			{
				userBatch.Add(TableOperation.Delete(userLogin));

				Model.IdentityUserIndex indexLogin = CreateLoginIndex(user.Id.ToString(), userLogin);

				tasks.Add(_indexTable.ExecuteAsync(TableOperation.Delete(indexLogin)));
			}

			foreach (var userRole in user.Roles)
			{
				userBatch.Add(TableOperation.Delete(userRole));
			}

			foreach (var userClaim in user.Claims)
			{
				userBatch.Add(TableOperation.Delete(userClaim));
			}

			tasks.Add(userBatch.ExecuteBatchAsync(_userTable));
			if (!string.IsNullOrWhiteSpace(user.Email))
			{
				Model.IdentityUserIndex indexEmail = CreateEmailIndex(user.Id.ToString(), user.Email);
				tasks.Add(_indexTable.ExecuteAsync(TableOperation.Delete(indexEmail)));
			}

			try
			{
				await Task.WhenAll(tasks.ToArray());
				return IdentityResult.Success;
			}
			catch (AggregateException aggex)
			{
				aggex.Flatten();
				return IdentityResult.Failed(new IdentityError() { Code = "003", Description = "Delete user failed." });
			}

		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed && disposing)
			{
				if (this.Context != null)
				{
					this.Context.Dispose();
				}
				this._indexTable = null;
				this._userTable = null;
				this.Context = null;
				this._disposed = true;
			}
		}

		public async virtual Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
		{
            ThrowIfDisposed();
			//if (login == null)
			//{
			//	throw new ArgumentNullException("login");
			//}

			string rowKey = KeyHelper.GenerateRowKeyUserLoginInfo(loginProvider, providerKey);
			string partitionKey = KeyHelper.GeneratePartitionKeyIndexByLogin(loginProvider);
			var loginQuery = GetUserIdByIndex(partitionKey, rowKey);

			return await GetUserAggregateAsync(loginQuery);
		}

		public async Task<TUser> FindByEmailAsync(string plainEmail, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			return await this.GetUserAggregateAsync(FindByEmailQuery(plainEmail));
		}

		public async Task<IEnumerable<TUser>> FindAllByEmailAsync(string plainEmail)
		{
			this.ThrowIfDisposed();
			return await this.GetUsersAggregateAsync(FindByEmailQuery(plainEmail));
		}

		private TableQuery FindByEmailQuery(string plainEmail)
		{
			return GetUserIdsByIndex(KeyHelper.GenerateRowKeyUserEmail(plainEmail));
		}

		private TableQuery GetUserIdByIndex(string partitionkey, string rowkey)
		{
			TableQuery tq = new TableQuery();
			tq.TakeCount = 1;
			tq.SelectColumns = new List<string>() { "Id" };
			tq.FilterString = TableQuery.CombineFilters(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionkey),
				TableOperators.And,
				TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowkey));
			return tq;
		}

		private TableQuery GetUserIdsByIndex(string rowkey)
		{
			TableQuery tq = new TableQuery();
			tq.SelectColumns = new List<string>() { "Id" };
			tq.FilterString = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowkey);
			return tq;
		}

		public virtual Task<TUser> FindByIdAsync(TKey userId)
		{
			this.ThrowIfDisposed();
			return this.GetUserAggregateAsync(userId.ToString());
		}

		//public virtual Task<TUser> FindByNameAsync(string userName, CancellationToken cancellationToken = default(CancellationToken))
		//{
		//	cancellationToken.ThrowIfCancellationRequested();
		//	this.ThrowIfDisposed();
		//	return this.GetUserAggregateAsync(KeyHelper.GenerateRowKeyUserName(userName));
		//}

		public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			return this.GetUserAggregateAsync(userId.ToString());
		}

		public Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			return this.GetUserAggregateAsync(KeyHelper.GenerateRowKeyUserName(normalizedUserName));
		}

		public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult<int>(user.AccessFailedCount);
		}

		public virtual Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult<IList<Claim>>(user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList());
		}

		public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult<string>(user.Email);
		}

		public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult<bool>(user.EmailConfirmed);
		}

		public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult<bool>(user.LockoutEnabled);
		}

		public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult<DateTimeOffset?>(user.LockoutEndDateUtc);
		}

		public virtual Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult<IList<UserLoginInfo>>((from l in user.Logins select new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName)).ToList<UserLoginInfo>());
		}

		public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult<string>(user.PasswordHash);
		}

		public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult<string>(user.PhoneNumber);
		}

		public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult<bool>(user.PhoneNumberConfirmed);
		}

		public virtual Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			return Task.FromResult<IList<string>>(user.Roles.ToList().Select(r => r.RoleName).ToList());
		}

		public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult<string>(user.SecurityStamp);
		}

		public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult<bool>(user.TwoFactorEnabled);
		}

		private Task<TUser> GetUserAggregateAsync(string userId)
		{
			var userResults = GetUserAggregateQuery(userId).ToList();
			return Task.FromResult<TUser>(GetUserAggregate(userId, userResults));
		}

		private IEnumerable<DynamicTableEntity> GetUserAggregateQuery(string userId)
		{
			TableQuery tq = new TableQuery();
			tq.FilterString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId);

			return _userTable.ExecuteQuery(tq);
		}

		protected IEnumerable<TUser> GetUserAggregateQuery(IList<string> userIds)
		{
			const double pageSize = 50.0;
			int pages = (int)Math.Ceiling(((double)userIds.Count / pageSize));
			List<TableQuery> listTqs = new List<TableQuery>(pages);
			List<string> tempUserIds = null;

			for (int currentPage = 1; currentPage <= pages; currentPage++)
			{
				if (currentPage > 1)
				{
					tempUserIds = userIds.Skip(((currentPage - 1) * (int)pageSize)).Take((int)pageSize).ToList();
				}
				else
				{
					tempUserIds = userIds.Take((int)pageSize).ToList();
				}

				TableQuery tq = new TableQuery();
				for (int i = 0; i < tempUserIds.Count; i++)
				{

					string temp = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, tempUserIds[i]);
					if (i > 0)
					{
						tq.FilterString = TableQuery.CombineFilters(tq.FilterString, TableOperators.Or, temp);
					}
					else
					{
						tq.FilterString = temp;
					}

				}
				listTqs.Add(tq);

			}

			ConcurrentBag<TUser> bag = new ConcurrentBag<TUser>();
#if DEBUG
			DateTime startUserAggTotal = DateTime.UtcNow;
#endif
#if DNX451
			Parallel.ForEach(listTqs, (q) =>
			{
				Parallel.ForEach(_userTable.ExecuteQuery(q)
					.ToList()
					.GroupBy(g => g.PartitionKey), (s) =>
					{
						bag.Add(GetUserAggregate(s.Key, s));
					});
			});
#else
			List<Task> tasks = new List<Task>(listTqs.Count);
			listTqs.ForEach((q) =>
		   {
			   tasks.Add(new TaskFactory().StartNew(new Action(() =>
			   {
				   _userTable.ExecuteQuery(q)
				   .ToList()
				   .GroupBy(g => g.PartitionKey)
				   .ToList().ForEach((s) =>
				   {
					   bag.Add(GetUserAggregate(s.Key, s));
				   });
			   })));
			});
			Task.WaitAll(tasks.ToArray());
#endif
#if DEBUG
			Debug.WriteLine("GetUserAggregateQuery (GetUserAggregateTotal): {0} seconds", (DateTime.UtcNow - startUserAggTotal).TotalSeconds);
#endif
			return bag;
		}

		private TUser GetUserAggregate(string userId, IEnumerable<DynamicTableEntity> userResults)
		{
			TUser user = default(TUser);
			var vUser = userResults.Where(u => u.RowKey.Equals(userId) && u.PartitionKey.Equals(userId)).SingleOrDefault();
			var op = new OperationContext();

			if (vUser != null)
			{
				//User
				user = Activator.CreateInstance<TUser>();
				user.ReadEntity(vUser.Properties, op);
				user.PartitionKey = vUser.PartitionKey;
				user.RowKey = vUser.RowKey;
				user.ETag = vUser.ETag;
				user.Timestamp = vUser.Timestamp;

				//Roles                            
				foreach (var log in userResults.Where(u => u.RowKey.StartsWith(Constants.RowKeyConstants.PreFixIdentityUserRole)
					 && u.PartitionKey.Equals(userId)))
				{
					TUserRole trole = Activator.CreateInstance<TUserRole>();
					trole.ReadEntity(log.Properties, op);
					trole.PartitionKey = log.PartitionKey;
					trole.RowKey = log.RowKey;
					trole.ETag = log.ETag;
					trole.Timestamp = log.Timestamp;
					user.Roles.Add(trole);
				}
				foreach (var log in userResults.Where(u => u.RowKey.StartsWith(Constants.RowKeyConstants.PreFixIdentityUserClaim)
					 && u.PartitionKey.Equals(userId)))
				{
					TUserClaim tclaim = Activator.CreateInstance<TUserClaim>();
					tclaim.ReadEntity(log.Properties, op);
					tclaim.PartitionKey = log.PartitionKey;
					tclaim.RowKey = log.RowKey;
					tclaim.ETag = log.ETag;
					tclaim.Timestamp = log.Timestamp;
					user.Claims.Add(tclaim);
				}
				//Logins
				foreach (var log in userResults.Where(u => u.RowKey.StartsWith(Constants.RowKeyConstants.PreFixIdentityUserLogin)
					 && u.PartitionKey.Equals(userId)))
				{
					TUserLogin tlogin = Activator.CreateInstance<TUserLogin>();
					tlogin.ReadEntity(log.Properties, op);
					tlogin.PartitionKey = log.PartitionKey;
					tlogin.RowKey = log.RowKey;
					tlogin.ETag = log.ETag;
					tlogin.Timestamp = log.Timestamp;
					user.Logins.Add(tlogin);
				}
			}
			return user;
		}

		private async Task<TUser> GetUserAggregateAsync(TableQuery queryUser)
		{
			return await new TaskFactory<TUser>().StartNew(() =>
			{
				var user = _indexTable.ExecuteQuery(queryUser).FirstOrDefault();
				if (user != null)
				{
					string userId = user.Properties["Id"].StringValue;
					var userResults = GetUserAggregateQuery(userId).ToList();
					return GetUserAggregate(userId, userResults);
				}

				return default(TUser);
			});
		}

		private async Task<IEnumerable<TUser>> GetUsersAggregateAsync(TableQuery queryUser)
		{
#if DEBUG
			DateTime startIndex = DateTime.UtcNow;
#endif
			var userIds = _indexTable.ExecuteQuery(queryUser).ToList().Select(u => u.Properties["Id"].StringValue).Distinct().ToList();
#if DEBUG
			Debug.WriteLine("GetUsersAggregateAsync (Index query): {0} seconds", (DateTime.UtcNow - startIndex).TotalSeconds);
#endif
			List<TUser> list = new List<TUser>(userIds.Count);

			return await GetUsersAggregateByIdsAsync(userIds);
		}

		protected async Task<IEnumerable<TUser>> GetUsersAggregateByIdsAsync(IList<string> userIds)
		{
			return await new TaskFactory<IEnumerable<TUser>>().StartNew(() =>
			{
				return GetUserAggregateQuery(userIds);
			});
		}

		public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult<bool>(user.PasswordHash != null);
		}

		public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.AccessFailedCount++;
			return Task.FromResult<int>(user.AccessFailedCount);
		}

		public virtual Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (string.IsNullOrWhiteSpace(roleName))
			{
				throw new ArgumentException(IdentityResources.ValueCannotBeNullOrEmpty, "roleName");
			}

			//Removing the live query. UserManager calls FindById to hydrate the user object first.
			//No need to go to the table again.
			return Task.FromResult<bool>(user.Roles.Any(r => r.RowKey == KeyHelper.GenerateRowKeyIdentityRole(roleName)));
		}

		public virtual async Task RemoveClaimAsync(TUser user, Claim claim)
		{
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (claim == null)
			{
				throw new ArgumentNullException("claim");
			}

			if (string.IsNullOrWhiteSpace(claim.Type))
			{
				throw new ArgumentException(IdentityResources.ValueCannotBeNullOrEmpty, "claim.Type");
			}

			// Claim ctor doesn't allow Claim.Value to be null. Need to allow string.empty.

			TUserClaim local = (from uc in user.Claims
								where uc.RowKey == KeyHelper.GenerateRowKeyIdentityUserClaim(claim.Type, claim.Value)
								select uc).FirstOrDefault();
			{
				user.Claims.Remove(local);
				TableOperation deleteOperation = TableOperation.Delete(local);
				await _userTable.ExecuteAsync(deleteOperation);
			}

		}
		public async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (claim == null)
			{
				throw new ArgumentNullException("claim");
			}
			if (newClaim == null)
			{
				throw new ArgumentNullException("newClaim");
			}

			// Claim ctor doesn't allow Claim.Value to be null. Need to allow string.empty.
			BatchOperationHelper bop = new BatchOperationHelper();

			TUserClaim local = (from uc in user.Claims
								where uc.RowKey == KeyHelper.GenerateRowKeyIdentityUserClaim(claim.Type, claim.Value)
								select uc).FirstOrDefault();
			if (local != null)
			{
				user.Claims.Remove(local);
				TableOperation deleteOperation = TableOperation.Delete(local);
				bop.Add(deleteOperation);
			}
			TUserClaim item = Activator.CreateInstance<TUserClaim>();
			item.UserId = user.Id;
			item.ClaimType = newClaim.Type;
			item.ClaimValue = newClaim.Value;
			((Model.IGenerateKeys)item).GenerateKeys();

			user.Claims.Add(item);

			bop.Add(TableOperation.Insert(item));
			await bop.ExecuteBatchAsync(_userTable);
		}

		public async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (claims == null)
			{
				throw new ArgumentNullException("claims");
			}

			// Claim ctor doesn't allow Claim.Value to be null. Need to allow string.empty.
			BatchOperationHelper bop = new BatchOperationHelper();

			foreach (Claim claim in claims)
			{
				TUserClaim local = (from uc in user.Claims
									where uc.RowKey == KeyHelper.GenerateRowKeyIdentityUserClaim(claim.Type, claim.Value)
									select uc).FirstOrDefault();
				{
					user.Claims.Remove(local);
					TableOperation deleteOperation = TableOperation.Delete(local);
					bop.Add(deleteOperation);
				}
			}
			await bop.ExecuteBatchAsync(_userTable);
		}


		public virtual async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (string.IsNullOrWhiteSpace(roleName))
			{
				throw new ArgumentException(IdentityResources.ValueCannotBeNullOrEmpty, "roleName");
			}

			TUserRole item = user.Roles.FirstOrDefault<TUserRole>(r => r.RowKey == KeyHelper.GenerateRowKeyIdentityRole(roleName));
			if (item != null)
			{
				user.Roles.Remove(item);
				TableOperation deleteOperation = TableOperation.Delete(item);

				await _userTable.ExecuteAsync(deleteOperation);
			}
		}

		public virtual async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			//if (login == null)
			//{
			//	throw new ArgumentNullException("login");
			//}
			TUserLogin item = user.Logins.SingleOrDefault<TUserLogin>(l => l.RowKey == KeyHelper.GenerateRowKeyIdentityUserLogin(loginProvider, providerKey));
			if (item != null)
			{
				user.Logins.Remove(item);
				Model.IdentityUserIndex index = CreateLoginIndex(item.UserId.ToString(), item);
				await Task.WhenAll(_indexTable.ExecuteAsync(TableOperation.Delete(index)),
									_userTable.ExecuteAsync(TableOperation.Delete(item)));
			}
		}

		public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.AccessFailedCount = 0;
			return Task.FromResult<int>(0);
		}

		public async Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			//Only remove the email if different
			//The UserManager calls UpdateAsync which will generate the new email index record
			if (!string.IsNullOrWhiteSpace(user.Email) && user.Email != email)
			{
				await DeleteEmailIndexAsync(user.Id.ToString(), user.Email);
			}
			user.Email = email;
		}

		//Fixes deletes for non-unique emails for users.
		private async Task DeleteEmailIndexAsync(string userId, string plainEmail)
		{
			TableQuery tq = new TableQuery();
			tq.FilterString = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, KeyHelper.GenerateRowKeyUserEmail(plainEmail));
			tq.SelectColumns = new List<string>() { "Id" };

			var indexes = _indexTable.ExecuteQuery(tq);

			foreach (DynamicTableEntity de in indexes)
			{
				if (de.Properties["Id"].StringValue.Equals(userId, StringComparison.OrdinalIgnoreCase))
				{
					await _indexTable.ExecuteAsync(TableOperation.Delete(de));
				}
			}
		}

		public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.EmailConfirmed = confirmed;
			return Task.FromResult<int>(0);
		}

		public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.LockoutEnabled = enabled;
			return Task.FromResult<int>(0);
		}

		public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
            user.LockoutEndDateUtc = lockoutEnd.HasValue ? new DateTime?(lockoutEnd.Value.DateTime.ToUniversalTime()) : null;
			return Task.FromResult<int>(0);
		}

		public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();

			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.PasswordHash = passwordHash;
			return Task.FromResult<int>(0);
		}

		public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.PhoneNumber = phoneNumber;
			return Task.FromResult<int>(0);
		}

		public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.PhoneNumberConfirmed = confirmed;
			return Task.FromResult<int>(0);
		}

		public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.SecurityStamp = stamp;
			return Task.FromResult<int>(0);
		}

		public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.TwoFactorEnabled = enabled;
			return Task.FromResult<int>(0);
		}

		private void ThrowIfDisposed()
		{
			if (this._disposed)
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		private TUser ChangeUserName(TUser user)
		{
			List<Task> taskList = new List<Task>(50);
			string userNameKey = KeyHelper.GenerateRowKeyUserName(user.UserName);

			Debug.WriteLine("Old User.Id: {0}", user.Id);
			string oldUserId = user.Id.ToString();
			Debug.WriteLine(string.Format("New User.Id: {0}", KeyHelper.GenerateRowKeyUserName(user.UserName)));
			//Get the old user
			var userRows = GetUserAggregateQuery(user.Id.ToString()).ToList();
			//Insert the new user name rows
			BatchOperationHelper insertBatchHelper = new BatchOperationHelper();
			foreach (DynamicTableEntity oldUserRow in userRows)
			{
				ITableEntity dte = null;
				if (oldUserRow.RowKey == user.Id.ToString())
				{
					Model.IGenerateKeys ikey = (Model.IGenerateKeys)user;
					ikey.GenerateKeys();
					dte = user;
				}
				else
				{
					dte = new DynamicTableEntity(userNameKey, oldUserRow.RowKey,
						Constants.ETagWildcard,
						oldUserRow.Properties);
				}
				insertBatchHelper.Add(TableOperation.Insert(dte));
			}
			taskList.Add(insertBatchHelper.ExecuteBatchAsync(_userTable));
			//Delete the old user
			BatchOperationHelper deleteBatchHelper = new BatchOperationHelper();
			foreach (DynamicTableEntity delUserRow in userRows)
			{
				deleteBatchHelper.Add(TableOperation.Delete(delUserRow));
			}
			taskList.Add(deleteBatchHelper.ExecuteBatchAsync(_userTable));

			// Create the new email index
			if (!string.IsNullOrWhiteSpace(user.Email))
			{
				taskList.Add(DeleteEmailIndexAsync(oldUserId, user.Email));

				Model.IdentityUserIndex indexEmail = CreateEmailIndex(userNameKey, user.Email);

				taskList.Add(_indexTable.ExecuteAsync(TableOperation.InsertOrReplace(indexEmail)));
			}

			// Update the external logins
			foreach (var login in user.Logins)
			{
				Model.IdentityUserIndex indexLogin = CreateLoginIndex(userNameKey, login);
				taskList.Add(_indexTable.ExecuteAsync(TableOperation.InsertOrReplace(indexLogin)));
				login.PartitionKey = userNameKey;
			}

			// Update the claims partitionkeys
			foreach (var claim in user.Claims)
			{
				claim.PartitionKey = userNameKey;
			}

			// Update the roles partitionkeys
			foreach (var role in user.Roles)
			{
				role.PartitionKey = userNameKey;
			}

			Task.WaitAll(taskList.ToArray());
			return user;
		}


		public async virtual Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			List<Task> tasks = new List<Task>(2);

			string userNameKey = KeyHelper.GenerateRowKeyUserName(user.UserName);
			if (user.Id.ToString() != userNameKey)
			{
				tasks.Add(Task.FromResult<TUser>(ChangeUserName(user)));
			}
			else
			{
				tasks.Add(_userTable.ExecuteAsync(TableOperation.Replace(user)));

				if (!string.IsNullOrWhiteSpace(user.Email))
				{
					Model.IdentityUserIndex indexEmail = CreateEmailIndex(user.Id.ToString(), user.Email);

					tasks.Add(_indexTable.ExecuteAsync(TableOperation.InsertOrReplace(indexEmail)));
				}
			}

			try
			{
				await Task.WhenAll(tasks.ToArray());
				return IdentityResult.Success;
			}
			catch (AggregateException aggex)
			{
				aggex.Flatten();
				return IdentityResult.Failed(new IdentityError() { Code = "002", Description = "User Update Failed." });
			}

		}

		public TContext Context { get; private set; }


		public virtual IQueryable<TUser> Users
		{
			get
			{
				ThrowIfDisposed();
				return null; //TODO: _userTable.CreateQuery<TUser>();
			}
		}

		/// <summary>
		/// Creates an email index suitable for a crud operation
		/// </summary>
		/// <param name="userid">Formatted UserId from the KeyHelper or IdentityUser.Id.ToString()</param>
		/// <param name="email">Plain email address.</param>
		/// <returns></returns>
		private Model.IdentityUserIndex CreateEmailIndex(string userid, string email)
		{
			return new Model.IdentityUserIndex()
			{
				Id = userid,
				PartitionKey = userid,
				RowKey = KeyHelper.GenerateRowKeyUserEmail(email),
				KeyVersion = KeyHelper.KeyVersion,
				ETag = Constants.ETagWildcard
			};
		}

		private Model.IdentityUserIndex CreateLoginIndex(string userid, TUserLogin login)
		{
			return new Model.IdentityUserIndex()
			{
				Id = userid,
				PartitionKey = KeyHelper.GeneratePartitionKeyIndexByLogin(login.LoginProvider),
				RowKey = login.RowKey,
				KeyVersion = KeyHelper.KeyVersion,
				ETag = Constants.ETagWildcard
			};

		}

		public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			TableQuery tq = new TableQuery();
			tq.FilterString = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, 
				KeyHelper.GenerateRowKeyIdentityUserClaim(claim.Type, claim.Value));
			tq.SelectColumns = new List<string>() {  };
			List<string> claimids = _userTable.ExecuteQuery(tq).Select(c=> c.PartitionKey).ToList();
			return await new TaskFactory<IList<TUser>>().StartNew(() =>
			{
				return GetUserAggregateQuery(claimids).ToList();
			});
		}

		public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			TableQuery tq = new TableQuery();
			tq.FilterString = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,
				KeyHelper.GenerateRowKeyIdentityUserRole(roleName));
			tq.SelectColumns = new List<string>() { };
			List<string> roleids = _userTable.ExecuteQuery(tq).Select(c => c.PartitionKey).ToList();
			return await new TaskFactory<IList<TUser>>().StartNew(() =>
			{
				return GetUserAggregateQuery(roleids).ToList();
			});
		}

		public virtual Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
            if (user.Id != null)
                return Task.FromResult(user.Id.ToString());
            else
                return Task.FromResult("");
		}

        public virtual Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult(user.UserName);
		}

		public virtual Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.UserName = userName;
			return Task.FromResult(0);
		}

		public virtual Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult(user.NormalizedUserName);
		}

		public virtual Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.NormalizedUserName = normalizedName;
			return Task.FromResult(0);
		}

		public virtual Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			return Task.FromResult(user.NormalizedEmail);
		}

		public virtual Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			user.NormalizedEmail = normalizedEmail;
			return Task.FromResult(0);
		}

	}
}
#endif