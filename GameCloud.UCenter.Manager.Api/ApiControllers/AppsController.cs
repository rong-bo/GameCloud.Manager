﻿using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GameCloud.Database.Adapters;
using GameCloud.UCenter.Common.Settings;
using GameCloud.UCenter.Database;
using GameCloud.UCenter.Database.Entities;
using GameCloud.UCenter.Web.Common.Modes;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace GameCloud.UCenter.Manager.Api.ApiControllers
{
    /// <summary>
    /// Provide a controller for users.
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AppsController : ApiControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppsController" /> class.
        /// </summary>
        /// <param name="database">Indicating the database context.</param>
        /// <param name="settings">Indicating the settings.</param>
        [ImportingConstructor]
        public AppsController(UCenterDatabaseContext database, Settings settings)
            : base(database, settings)
        {
        }

        /// <summary>
        /// Get user list.
        /// </summary>
        /// <param name="token">Indicating the cancellation token.</param>
        /// <param name="keyword">Indicating the keyword.</param>
        /// <param name="orderby">Indicating the order by name.</param>
        /// <param name="page">Indicating the page number.</param>
        /// <param name="count">Indicating the count.</param>
        /// <returns>Async return user list.</returns>
        [Route("api/apps")]
        public async Task<PaginationResponse<AppEntity>> Get(
            CancellationToken token,
            string keyword = null,
            string orderby = null,
            int page = 1,
            int count = 1000)
        {
            Expression<Func<AppEntity, bool>> filter = null;

            if (!string.IsNullOrEmpty(keyword))
            {
                filter = a => a.Name.Contains(keyword);
            }

            var total = await this.Database.Apps.CountAsync(filter, token);

            IQueryable<AppEntity> queryable = this.Database.Apps.Collection.AsQueryable();
            if (filter != null)
            {
                queryable = queryable.Where(filter);
            }
            queryable = queryable.OrderByDescending(a => a.CreatedTime);

            var result = queryable.Skip((page - 1) * count).Take(count).ToList();

            // todo: add orderby support.
            var model = new PaginationResponse<AppEntity>
            {
                Page = page,
                PageSize = count,
                Raws = result,
                Total = total
            };

            return model;
        }

        /// <summary>
        /// Get single user details.
        /// </summary>
        /// <param name="id">Indicating the user id.</param>
        /// <param name="token">Indicating the cancellation token.</param>
        /// <returns>Async return user details.</returns>
        public async Task<AppEntity> Get(string id, CancellationToken token)
        {
            var result = await this.Database.Apps.GetSingleAsync(id, token);

            return result;
        }
    }
}