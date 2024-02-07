using HrappRepositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace HrappRepositories
{
    public class Repository<Model> : IRepository<Model> where Model : class
    {
        protected readonly DbContext _dbContext;

        public Repository(DbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public void Add(Model model)
        {
            _dbContext.Set<Model>().Add(model);
            _dbContext.SaveChanges();
        }

        public void AddRange(IEnumerable<Model> models)
        {
            _dbContext.Set<Model>().AddRange(models);
            _dbContext.SaveChanges();
        }

        public IEnumerable<Model> Find(Expression<Func<Model, bool>> predicate)
        {
            return _dbContext.Set<Model>().Where(predicate);
        }

        public IEnumerable<Model> GetAll()
        {
            return _dbContext.Set<Model>().ToList();
        }

        public Model GetById(Guid id)
        {
            return _dbContext.Set<Model>().Find(id.ToString());
        }

        public void Remove(Model model)
        {
            _dbContext.Set<Model>().Remove(model);
            _dbContext.SaveChanges();
        }

        public void RemoveRange(IEnumerable<Model> models)
        {
            _dbContext.Set<Model>().RemoveRange(models);
            _dbContext.SaveChanges();
        }
    }
}
