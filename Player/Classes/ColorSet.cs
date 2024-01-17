using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Player.Classes
{
    public class ColorSet // 데이터 베이스에 저장되는 형식
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
        public String Back { get; set; }
        public String Bor { get; set; }
        public override string ToString()
        {
            return Name + "-" + Back + "-" + Bor;
        }
    }
}
