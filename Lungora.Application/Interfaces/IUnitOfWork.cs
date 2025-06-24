namespace Lungora.Bl.Interfaces
{
    public interface IUnitOfWork
    {
        IArticle ClsArticles { get; }
        ICategory ClsCategories { get; }
        IImageService IImageService { get; }
        IModelService modelService { get; }
        IDoctor ClsDoctors { get; }
        IWorkingHour ClsWorkingHours { get; }
        IUserService UserService { get; }
        Task<int> SaveChangesAsync();


    }
}
