//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace JayDev.MediaScribe.Core
//{
//    public class SqLiteConnectionProvider : NHibernate.Connection.DriverConnectionProvider
//    {
//        private static System.Data.IDbConnection m_Connection;
//        public override System.Data.IDbConnection GetConnection()
//        {
//            if (m_Connection == null)
//                m_Connection = base.GetConnection();
//            return m_Connection;
//        }

//        public override void CloseConnection(System.Data.IDbConnection conn)
//        {
//            //Do nothing
//        }
//    }
//}