using System.Linq.Expressions;
using Infrastructure.Base;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
namespace Infrastructure.Base;
public class GenericRepository<T> : IGenericRepository<T> where T : class 
{

    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context) {
       
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();

    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(new object[] { id }); 
    }

    public async Task<IReadOnlyList<T>> ListAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<T> GetEntityWithSpec(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    public IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), spec);
    }

    public async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null )
    {
        IQueryable<T> query = _dbSet;

        if (include != null)
        {
            query = include(query);
        }

        return await query.Where(predicate).ToListAsync();
    }

    public async Task AddAsync (T entity ){
        await _dbSet.AddAsync(entity);
    }

     public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<T, bool>> predicate,
        int pageNumber,
        int pageSize,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
    {
        if (pageNumber <= 0 || pageSize <= 0)
        {
            throw new ArgumentException("Page number and page size must be greater than zero.");
        }

        IQueryable<T> query = _dbSet;

        if (include != null)
        {
            query = include(query);
        }

        query = query.Where(predicate);

        int totalCount = await query.CountAsync();

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }

    public async Task<IEnumerable<T>> FindBySpecificationAsync(
        ISpecification<T> specification)
    {
        IQueryable<T> query = _dbSet;

        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        foreach (var include in specification.Includes)
        {
            query = include(query);
        }

        if (specification.OrderBy != null)
        {
            query = specification.OrderBy(query);
        }

        if (specification.Skip.HasValue)
        {
            query = query.Skip(specification.Skip.Value);
        }

        if (specification.Take.HasValue)
        {
            query = query.Take(specification.Take.Value);
        }

        return await query.ToListAsync();
    }
}