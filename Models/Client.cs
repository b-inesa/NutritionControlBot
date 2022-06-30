using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot
{
    public class Client
    {
        public string product { get; set; }
        public int calories { get; set; }
        public bool productbool { get; set; } = false;

        public int daynorm { get; set; }
        public string datenorm { get; set; }
        public int Daynorm { get; set; }
        public bool daynormbool { get; set; } = false;

        public bool addeatenproductbool { get; set; } = false;
        public string dateeaten { get; set; }
        public string resulteaten { get; set; }
        public string dateshownorm { get; set; }
        public int resultshownorm {get; set;}
        public string datechecknorm { get; set; }
        public string resultchecknorm { get; set; }

        public bool addnamebool { get; set; } = false;
        public bool addcaloriesbool { get; set; } = false;
        public string namevalue { get; set; } = null;
        public int caloriesvalue { get; set; } = 0;
        public bool addproteinsbool { get; set; } = false;
        public double proteinvalue { get; set; } = 0;
        public bool addfatsbool { get; set; } = false;
        public double fatsvalue { get; set; } = 0;
        public bool addcarbohydratesbool { get; set; } = false;
        public double carbohydratesvalue { get; set; } = 0;

        public bool addowneatenproductbool { get; set; } = false;
        public string resultnewproduct { get; set; }
        public string resultproductlist { get; set; }
        public string dateownproduct { get; set; }
        public string resultownproduct { get; set; }
        public bool deleteproductbool { get; set; } = false;
        public string productdelete { get; set; } 
        public string resultproductdelete { get; set; }

        public bool barcodenumberbool { get; set; } = false;
        public bool portionbool { get; set; } = false;
        public string barcodenumber { get; set; }
        public double portion { get; set; }
        public string datebarcode { get; set; }
        public string resultbarcode { get; set; }

        public bool datedaybool { get; set; } = false;
        public string dateday { get; set; }
        public string resultday { get; set; }
        public string resultmonth { get; set; }
        public bool datemonthbool { get; set; } = false;
        public string datemonth { get; set; }
        public bool foodhistorybool { get; set; } = false;
        public string datehistory { get; set; }
        public string resulthistory { get; set; }  
        
        public int dated { get; set; }
        public DateTime dateh { get; set; }
        public DateTime datem { get; set; }
    }
}
