//using NHibernate;
//using NHibernate.SqlTypes;
//using System.Data;
//using System.Windows.Media;
//using System.Text;
//using System;
//using JayDev.MediaScribe.Common;

//namespace JayDev.MediaScribe.Core
//{
//    public class ColorUserType : BaseUserType<Color>
//    {
//        public override object NullSafeGet(IDataReader dr, string[] names,
//                                       object owner)
//        {
//            var colorString = ((string)NHibernateUtil.String
//                                          .NullSafeGet(dr, names[0]));
//            if (null != colorString)
//            {
//                Color color = ColorHelper.FromString(colorString);
//                return color;
//            }
//            else
//            {
//              return null;
//            }
//        }
//        public override void NullSafeSet(IDbCommand cmd, object value, int index)
//        {
//            var color = (Color)value;
//            object theValue;
//            if (null != color)
//            {
//                theValue = color.ToString();
//            }
//            else
//            {
//              theValue = DBNull.Value;
//            }
//            NHibernateUtil.String.NullSafeSet(cmd, theValue, index);
//        }
//        public override SqlType[] SqlTypes
//        {
//            get { return new[] { SqlTypeFactory.GetString(10000) }; }
//        }

//    }
//}