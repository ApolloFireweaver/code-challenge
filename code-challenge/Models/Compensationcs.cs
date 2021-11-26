using System;

namespace challenge.Models
{
    public class Compensation
    {
        public Employee employee { get; set; }
        //I set this to float as technically it should represent a figure with up to two decimal points. 
        //If that precision isn't needed, this could be changed to an int
        public float salary { get; set; }
        public DateTime effectiveDate { get; set; }
    }
}
