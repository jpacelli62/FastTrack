using Faaast.DatabaseModel;
using Faaast.Metadata;
using Faaast.Orm;
using Faaast.Orm.Reader;
using Faaast.Tests.Orm.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class ExpressionTests
    {

        [Fact]
        public void Check_Dommel_mapping()
        {
            //var test1 = VisitExpression<SimpleModel>(x => x.V1 == 1);
            //var test2 = VisitExpression<SimpleModel>(x => x.V8);
            //var test3 = VisitExpression<SimpleModel>(x => !x.V8);
            //var test3 = VisitExpression<SimpleModel>(x => x.V3 != DateTime.Now);
            //var sampleObject = new SimpleModel { V3 = DateTime.Now };
            //var test4 = VisitExpression<SimpleModel>(x => x.V3 != sampleObject.V3);


            //var test5 = VisitExpression<SimpleModel>(x => x.V1 == 1 || x.V1 == 2);
            var test6 = VisitExpression<SimpleModel>(x => x.V2 == "test" && (x.V1 == 1 || x.V1 == 2) && x.V2 == "test2");


        }

        private object VisitExpression<T>(Expression<Func<T, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            //if (result is PropertyClause)
            //{
            //    return new BinaryColumnClause
            //    {
            //        Left = result,
            //        Operation = "=",
            //        Right = new ConstantClause { Value = 1 }
            //    };
            //}
            return result;
        }
    }
}
