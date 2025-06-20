using Azure;
using Lungora.Bl.Interfaces;
using Lungora.Dtos.ArticleDtos;
using Lungora.Dtos.CategoryDtos;
using Lungora.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net;

namespace Lungora.Bl.Repositories
{
    public class ClsArticles: Repository<Article>,IArticle
    {
        private readonly LungoraContext context;
        public ClsArticles(LungoraContext context):base(context)
        {
            this.context = context;
        }
        public async Task<IEnumerable<Article>> GetAllAsync()
        {
            try
            {
                var article = await context.TbArticles.ToListAsync();
                return article;
            }
            catch
            {

                return new List<Article>();
            }
        }
        public async Task<ArticleDetailDto> GetById(int id)
        {
            try
            {
                var Article = await context.TbArticles.FirstOrDefaultAsync(a => a.Id == id);
                if (Article == null)
                {   
                    return new ArticleDetailDto();
                }

                return new ArticleDetailDto
                {
                    Content = Article.Content,
                    Title = Article.Title,
                    CoverImage = Article.CoverImage,
                    Description = Article.Description,
                };
            }
            catch
            {
                return new ArticleDetailDto();
            }
        }
        public async Task<List<Article>> GetByCategoryId(int CategoryId)
        {
            try
            {
                var Article = await context.TbArticles
                                .Where(c => c.CategoryId == CategoryId)
                                .ToListAsync();
                if (Article == null)
                {
                    return new List<Article>();
                }

                return Article;

            }
            catch
            {
                return new List<Article>();
            }
        }
        public async Task<Article?> UpdateAsync(int id, Article updatedArticle)
        {
            var existingArticle = await GetSingleAsync(x => x.Id == id);
            if (existingArticle is null)
                return null;

            // Update only fields that are allowed to change
            existingArticle.Title = updatedArticle.Title;
            existingArticle.Description = updatedArticle.Description;
            existingArticle.Content = updatedArticle.Content;
            existingArticle.CoverImage = updatedArticle.CoverImage;
            existingArticle.UpdatedAt = DateTime.UtcNow; // UTC is recommended for consistency
            existingArticle.UpdatedBy = updatedArticle.UpdatedBy;

            context.TbArticles.Update(existingArticle); // EF automatically tracks the entity
            await context.SaveChangesAsync();

            return existingArticle;
        }


    }
}
