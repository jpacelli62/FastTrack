using System;
using System.Data.Common;

namespace Faaast.Orm.Reader
{
    public class FaaastCommand : BaseCommand, IDisposable
    {
        public FaaastCommand(FaaastDb db, DbConnection dbConnection, string commandText, object parameters = null) :
            base(db, dbConnection, commandText, parameters)
        {

        }

        public int ExecuteNonQuery()
        {
            using var dbCommand = this.CreateInternalCommand();
            var result = 0;
            Exception ex = null;
            try
            {
                result = dbCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                ex = e;
            }
            finally
            {
                if (this.AutoClose)
                {
                    this.Connection.Close();
                }
            }

            return ex != null ? throw ex : result;
        }

        public FaaastRowReader ExecuteReader()
        {
            this.CreateInternalCommand();
            var reader = new FaaastRowReader(this);
            reader.Prepare();
            return reader;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing && this.Command != null)
            {
                this.Command.Dispose();
            }

            disposed = true;
        }
    }
}
