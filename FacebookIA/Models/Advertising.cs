using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facebook.Models
{
    public class Advertising
    {
        private static Advertising mInstance;
        private List<string> list = null;

        public static Advertising getInstance()
        {
            if (mInstance == null)
                mInstance = new Advertising();

            return mInstance;
        }

        private Advertising()
        {
            list = new List<string>();
        }
        public List<string> getArray()
        {
            return this.list;
        }
        public void addToArray(string value)
        {
            list.Add(value);
        }
    }
}