using System;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections;

class Program
{
    [STAThread]
    static void Main()
    {
        var oc = new ObservableCollection<string> { "A", "B", "C" };
        var cvs = new CollectionViewSource { Source = oc };
        var view = cvs.View;
        if (view is ICollection coll)
        {
            Console.WriteLine("Count via ICollection: " + coll.Count);
        }
        else
        {
            Console.WriteLine("Not an ICollection");
        }
    }
}
