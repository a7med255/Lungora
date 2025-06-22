using Lungora.Bl.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lungora.Bl.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LungoraContext context;
        public IArticle ClsArticles { get; private set; }
        public ICategory ClsCategories { get; private set; }
        public IImageService IImageService { get; private set; }
        public IModelService modelService { get; private set; }
        public IDoctor ClsDoctors { get; private set; }
        public IWorkingHour ClsWorkingHours { get; private set; }
        public IUserService UserService { get; private set; }
        public UnitOfWork(LungoraContext context, IArticle article, ICategory category, IModelService modelService, 
            IImageService imageService, IDoctor doctor, IWorkingHour workingHour, IUserService userService)
        {
            this.context = context;
            ClsArticles = article;
            ClsCategories = category;
            this.modelService = modelService;
            IImageService = imageService;
            ClsDoctors = doctor;
            ClsWorkingHours = workingHour;
            UserService = userService;
        }


        public async Task<int> SaveChangesAsync()
        {
            return await context.SaveChangesAsync();
        }
    }
}
