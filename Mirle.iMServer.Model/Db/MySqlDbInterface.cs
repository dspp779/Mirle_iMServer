using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.IO;
using System.Data.Common;
using System.Data;

namespace Mirle.iMServer.Model.Db
{
    public class MySqlDbInterface : DbInterface
    {
        // default db data source
        public static string server = "localhost";
        public static string userid = "root";
        public static string password = " ";
        public static string database = "mirle";
        public static string charset = "utf8";
        public static bool persistSecurityInfo = true;

        public string DataSource
        {
            get
            {
                return string.Format("server={0};user id={1};Password={2};"
                + "database={3};charset={4};persist security info={5};",
                server, userid, password, database, charset, persistSecurityInfo);
            }
        }

        // get SQL connection
        public DbConnection getConnection()
        {
            return getConnection(DataSource);
        }

        // get SQL connection with specified db file path
        public DbConnection getConnection(string dataSource)
        {
            return new MySqlConnection(dataSource);
        }

        // execute an query SQL command such as SELECT
        public DbDataReader execQuery(DbCommand cmd)
        {
            using (DbConnection conn = getConnection())
            {
                conn.Open();
                cmd.Connection = conn;
                return cmd.ExecuteReader();
            }
        }

        /* execute an update SQL command such as CREATE TABLE, INSERT, UPDATE...etc.
         * return number of modified record
         * */
        public int execUpdate(DbCommand cmd)
        {
            using (DbConnection conn = getConnection())
            {
                conn.Open();
                cmd.Connection = conn;
                return cmd.ExecuteNonQuery();
            }
        }

        // get row ID of the last inserted record
        public int getLastInsertRowId()
        {
            using (DbConnection conn = getConnection())
            {
                conn.Open();
                DbCommand cmd = new MySqlCommand("select last_insert_rowid()");
                cmd.Connection = conn;
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    return (reader.Read()) ? reader.GetInt32(0) : -1;
                }
            }
        }

        /* execute an insert command
         * return row id of the inserted record
         * */
        public int execInsert(DbCommand cmd)
        {
            using (DbConnection conn = getConnection())
            {
                conn.Open();
                cmd.Connection = conn;
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
