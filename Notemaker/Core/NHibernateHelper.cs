using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Cfg;
using JayDev.Notemaker;

namespace JayDev.Notemaker.Core
{
    public class NHibernateHelper
    {
        private static ISessionFactory _sessionFactory;
 
        private static ISessionFactory SessionFactory
        {
            get
            {
                if(_sessionFactory == null)
                {
                    var configuration = new Configuration();
                    configuration.Configure();
                    configuration.AddAssembly("Notemaker");
                    _sessionFactory = configuration.BuildSessionFactory();
                }
                return _sessionFactory;
            }
        }
 
        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}