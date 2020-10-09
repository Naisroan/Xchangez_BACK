using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Xchangez.Interfaces
{
    public interface IRepository<T, N> where T : class where N : class
    {
        Task CreateAsync(T nodo);

        void Update(T nodo);

        void Delete(T nodo);

        Task<List<N>> GetAsync();

        Task<List<N>> GetAsync(Expression<Func<T, bool>> where = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string properties = "");

        Task<int> Commit();

        IMapper GetMapper();

        XchangezContext GetContext();
    }
}
