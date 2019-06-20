using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Translationator
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Translationator());
            Translationator frm = new Translationator();
            Application.Run(frm);
            if (frm.ReturnArgs != null)
                Application.Run(new Magic(frm.ReturnArgs, frm.Magic));
        }
    }
}
