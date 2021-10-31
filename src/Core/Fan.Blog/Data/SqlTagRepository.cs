﻿using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Blog.Data
{
    /// <summary>
    /// Sql implementation of the <see cref="ITagRepository"/> contract.
    /// </summary>
    public class SqlTagRepository : EntityRepository<Tag>, ITagRepository
    {
        private readonly FanDbContext _db;
        public SqlTagRepository(FanDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns all tags or empty list if no tags found. The returned list is ordered by 
        /// <see cref="Tag.Count"/> desc and then by <see cref="Tag.Title"/>.
        /// </summary>
        public async Task<List<Tag>> GetListAsync()
        {
            return await (from t in _entities
                          select new Tag
                          {
                              Id = t.Id,
                              Title = t.Title,
                              Slug = t.Slug,
                              Color = t.Color,
                              Description = t.Description,
                              Count = (from p in _db.Set<Post>()
                                       from pt in p.PostTags
                                       where pt.TagId == t.Id && p.Status == EPostStatus.Published
                                       select pt).Count(),
                          }).OrderByDescending(t => t.Count).ThenBy(t => t.Title).ToListAsync();
        }
    }
}
