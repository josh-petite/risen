using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Linq;
using Risen.Server.Entities.Maps;

namespace Risen.Server.Data
{
    public interface IRepository
    {
        T FindOne<T>(Expression<Func<T, bool>> expression);
        IEnumerable<T> FindAll<T>();
        IEnumerable<T> FindMany<T>(Expression<Func<T, bool>> expression);
    }

    public class Repository : IRepository
    {
        public T FindOne<T>(Expression<Func<T, bool>> expression)
        {
            var sessionFactory = CreateSessionFactory();
            var result = default(T);

            using (var session = sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        result = session.Query<T>().FirstOrDefault(expression);
                        transaction.Commit();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        transaction.Rollback();
                    }
                }
            }

            return result;
        }

        public IEnumerable<T> FindAll<T>()
        {
            var sessionFactory = CreateSessionFactory();

            using (var session = sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        return session.Query<T>();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }

                session.Flush();
            }

            return default(IEnumerable<T>);
        }

        public IEnumerable<T> FindMany<T>(Expression<Func<T, bool>> expression)
        {
            var sessionFactory = CreateSessionFactory();

            using (var session = sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        return session.Query<T>().Where(expression);
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }

                session.Flush();
            }

            return default(IEnumerable<T>);
        }

        private ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008.ConnectionString(ConfigurationManager.AppSettings["ConnectionString"]))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<RoomMap>())
                .BuildSessionFactory();
        }
    }
}
