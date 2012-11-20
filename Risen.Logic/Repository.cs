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

namespace Risen.Server
{
    public interface IRepository
    {
        T FindOne<T>(Func<T, bool> func);
        IEnumerable<T> FindMany<T>();
        IEnumerable<T> FindMany<T>(Expression<Func<T, bool>> expression);
    }

    public class Repository : IRepository
    {
        public T FindOne<T>(Func<T, bool> func)
        {
            var sessionFactory = CreateSessionFactory();

            using (var session = sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        return session.Query<T>().Where(func).FirstOrDefault();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }

                session.Flush();
            }

            return default(T);
        }

        public IEnumerable<T> FindMany<T>()
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
