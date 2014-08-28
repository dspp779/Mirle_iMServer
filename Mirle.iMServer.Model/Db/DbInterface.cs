using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.Common;

namespace Mirle.iMServer.Model.Db
{
    interface DbInterface
    {
        DbConnection getConnection();
        DbConnection getConnection(string dataSource);

        DbDataReader execQuery(DbCommand cmd);

        int execUpdate(DbCommand cmd);
        int execUpdate(string cmd);
        int execInsert(DbCommand cmd);

        int getLastInsertRowId();
    }
}