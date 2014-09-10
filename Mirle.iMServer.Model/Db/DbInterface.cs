using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.Common;
using System.Data;

namespace Mirle.iMServer.Model.Db
{
    /// <summary>
    /// 資料庫基本操作程序介面
    /// </summary>
    interface DbInterface
    {
        // 需實作連線取得方法
        DbConnection getConnection();
        DbConnection getConnection(string dataSource);

        // 需實作查詢方法
        DbDataReader execQuery(DbCommand cmd);

        // 需實作更新方法
        int execUpdate(DbCommand cmd);
        int execUpdate(string cmd);
        // 需實作插入方法
        int execInsert(DbCommand cmd);

        // 需實作取得最後變更值之ID方法
        int getLastInsertRowId();

        // 資料查詢方法，回傳為DataTable
        DataTable getDataTable(DbCommand cmd);
    }
}