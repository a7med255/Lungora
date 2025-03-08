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
        public async Task<Article> UpdateAsync(int Id, Article Article)
        {
                var UpdatedArticle = await GetSingleAsync(x => x.Id == Id);
                if (UpdatedArticle is not null)
                {
                    UpdatedArticle.Title =Article.Title;
                    UpdatedArticle.Description =Article.Description;
                    UpdatedArticle.UpdatedAt = Article.UpdatedAt;
                    UpdatedArticle.Content =Article.Content;
                    UpdatedArticle.CoverImage =Article.CoverImage;
                    UpdatedArticle.UpdatedAt = DateTime.Now;
                    context.Update(UpdatedArticle).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    await context.SaveChangesAsync();
                    return Article;
                }
            return new Article();
        }
        

    }
}
