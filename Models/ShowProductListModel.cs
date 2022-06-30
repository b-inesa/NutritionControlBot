using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot.Models
{
    public class ShowProductListModel
    {
        public long Userid { get; set; }
        public string Name { get; set; }
        public Items ProductItems { get; set; }
    }
    public class Items
    {
        public int Calories { get; set; }
        public double Proteins { get; set; }
        public double Fats { get; set; }
        public double Carbohydrates { get; set; }       
    }
}
