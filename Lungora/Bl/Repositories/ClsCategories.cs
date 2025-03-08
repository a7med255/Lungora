﻿using Lungora.Bl.Interfaces;
using Lungora.Dtos.ArticleDtos;
using Lungora.Dtos.CategoryDtos;
using Lungora.Models;
using Microsoft.EntityFrameworkCore;

namespace Lungora.Bl.Repositories
{
    public class ClsCategories : Repository<Category>, ICategory
    {
        private readonly LungoraContext context;
        public ClsCategories(LungoraContext context):base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            try
            {
                var Categories = await context.TbCategories.ToListAsync();

                return Categories.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName
                }).ToList();
            }
            catch{
                return new List<CategoryDto>();
            }
        }
        public async Task<CategoryWithArticles> GetById(int id)
        {
            try
            {
                var Category = await context.TbCategories.Include(c=>c.Articles)
                                .FirstOrDefaultAsync(c => c.Id == id);
                if (Category == null)
                {
                    return new CategoryWithArticles();
                }

                return new CategoryWithArticles
                {
                    CategoryName = Category.CategoryName,
                    Articles = Category.Articles.Select(c => new ArticlesDto
                    {
                        Id = c.Id,
                        Description = c.Description,
                        Title = c.Title,
                    }
                        ).ToList()
                };
            }
            catch
            {
                return new CategoryWithArticles();
            }
        }
        public async Task<Category> UpdateAsync(int Id, Category Article)
        {
            var UpdatedCategory = await GetSingleAsync(x => x.Id == Id);
            if (UpdatedCategory is not null)
            {
                UpdatedCategory.CategoryName = Article.CategoryName;
                UpdatedCategory.UpdatedBy = Article.UpdatedBy;
                UpdatedCategory.UpdatedAt= DateTime.Now;

                context.Update(UpdatedCategory).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await context.SaveChangesAsync();
                return Article;
            }
            return new Category();
        }
    }
}
