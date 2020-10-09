using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xchangez.Interfaces;

namespace Xchangez.Clases
{
    public class Repository<T, N> : IRepository<T, N> where T : class where N : class
    {
        public XchangezContext Context;

        public IMapper Mapper;

        public Repository(XchangezContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }

        public async Task CreateAsync(T nodo)
        {
            try
            {
                await Context.Set<T>().AddAsync(nodo);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(T nodo)
        {
            try
            {
                Context.Set<T>().Remove(nodo);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Update(T nodo)
        {
            try
            {
                Context.Set<T>().Update(nodo);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> Commit()
        {
            return await Context.SaveChangesAsync();
        }

        public async Task<List<N>> GetAsync()
        {
            return Mapper.Map<List<N>>(await Context.Set<T>().ToListAsync());
        }

        public async Task<List<N>> GetAsync(Expression<Func<T, bool>> where = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string properties = "")
        {
            IQueryable<T> query = Context.Set<T>();

            if (where != null)
            {
                query = query.Where(where);
            }

            foreach (var prop in properties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(prop);
            }

            if (orderBy != null)
            {
                return Mapper.Map<List<N>>(await orderBy(query).ToListAsync());
            }
            else
            {
                return Mapper.Map<List<N>>(await query.ToListAsync());
            }
        }

        public IMapper GetMapper()
        {
            return Mapper;
        }

        public XchangezContext GetContext()
        {
            return Context;
        }
    }
}
