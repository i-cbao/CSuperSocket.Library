using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;
using Dynamic.Core.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Dynamic.Core.DB.Repository
{
    //外部需将DbContext依赖注入进来供数据仓库使用
    public abstract class DRepository<T> where T : class
    {

        public  DRepository()
        {
            
        }
        public bool InitDB()
        {
            return  _context.Database.EnsureCreated();
        }
       
        public DRepository(DbContext context)
        {
            this._context = context;
        }
        protected DbContext _context;
        //添加
        public bool AddEntities(T entity)
        {

            _context.Set<T>().Attach(entity);
            _context.Entry<T>(entity).State = EntityState.Added;
            return _context.SaveChanges()>0;
            //return entity;
        }
        /// <summary>
        /// 更新对象属性（属性为空不更新）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="db"></param>
        public  void UpdateModelAttrs(T entity)
        {
            _context.Set<T>().Attach(entity);
            foreach (System.Reflection.PropertyInfo p in entity.GetType().GetProperties())
            {
                var attrValue = p.GetValue(entity);
                if (attrValue!= null&&!p.HaveCustomAttributes<KeyAttribute>())
                {
                    _context.Entry(entity).Property(p.Name).IsModified = true;
                }
            }
            _context.SaveChanges();
        }
        /// <summary>
        /// 修改整体对象，每个属性值都更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool UpdateEntities(T entity)
        {
            _context.Set<T>().Attach(entity);
            _context.Entry<T>(entity).State = EntityState.Modified;
            return _context.SaveChanges() > 0;
        }

        //修改
        public bool DeleteEntities(T entity)
        {
            _context.Set<T>().Attach(entity);
            _context.Entry<T>(entity).State = EntityState.Deleted;
            return _context.SaveChanges() > 0;
        }

        //查询
        public IQueryable<T> LoadEntities(Expression<Func<T, bool>> wherelambda)
        {
            
            return _context.Set<T>().Where<T>(wherelambda).AsQueryable();
        }

        //分页
        public IQueryable<T> LoadPagerEntities<S>(int pageSize, int pageIndex, out int total,
            Expression<Func<T, bool>> whereLambda, bool isAsc, Expression<Func<T, S>> orderByLambda)
        {
            var tempData = _context.Set<T>().Where<T>(whereLambda);
            total = tempData.Count();

            //排序获取当前页的数据
            if (isAsc)
            {
                tempData = tempData.OrderBy<T, S>(orderByLambda).
                      Skip<T>(pageSize * (pageIndex - 1)).
                      Take<T>(pageSize).AsQueryable();
            }
            else
            {
                tempData = tempData.OrderByDescending<T, S>(orderByLambda).
                     Skip<T>(pageSize * (pageIndex - 1)).
                     Take<T>(pageSize).AsQueryable();
            }
            return tempData.AsQueryable();
        }
        public IQueryable<T> ExecuteStoreQuery(string sql)
        {
            return (_context).Set<T>().FromSql<T>(sql);
        }
        public IQueryable<T1> ExecuteStoreQuery<T1>(string sql) where T1 :class
        {
            return (_context).Set<T1>().FromSql<T1>(sql);
        }
        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return this._context.Database.ExecuteSqlCommand(sql);

        }

    }
}
