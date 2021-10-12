
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace SysSped.Infra.Data
{
    public abstract class BaseRepository
    {
        public DbConnection _conn;
        public BaseRepository()
        {
            try
            {
                _conn = new MySqlConnection(@"server=localhost;user id=root;password=1234;persistsecurityinfo=True;database=sysspeddb;");
                _conn.Open();
            }
            catch (System.Exception ex )
            {
                _conn = new MySqlConnection(@"server=localhost;user id=root;password=abc123;persistsecurityinfo=True;database=sysspeddb;");
                _conn.Open();
            }
        }

        public BaseRepository(bool coisa)
        {
            try
            {
                _conn = new MySqlConnection(@"server=localhost;user id=root;password=1234;persistsecurityinfo=True;database=sys;");
                _conn.Open();
            }
            catch (System.Exception ex)
            {
                _conn = new MySqlConnection(@"server=localhost;user id=root;password=abc123;persistsecurityinfo=True;database=sys;");
                _conn.Open();
            }
        }
    }
}
