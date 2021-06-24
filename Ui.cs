using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using Terminal.Gui;
using KSynthLib.K4;

namespace K4Tool
{
    public class Ui
    {
        Window win;

        public Ui()
        {
            Application.Init();
            var top = Application.Top;
            this.win = new Window("K4Tool")
            {
                X = 0,
                Y = 1, // Leave one row for the toplevel menu

	            // By using Dim.Fill(), it will automatically resize without manual intervention
	            Width = Dim.Fill(),
	            Height = Dim.Fill()
            };
            top.Add(this.win);

            var menu = new MenuBar(
                new MenuBarItem[]
                {
                    new MenuBarItem("_File",
                        new MenuItem []
                        {
                            new MenuItem("_Open", "Opens a new file", () => { OpenBankFile(); }),
                            new MenuItem("E_xit", "", () => { if (Quit()) top.Running = false; })
                        }
                    )
                }
            );
            top.Add(menu);
            Application.Run(top);
        }

        void OpenBankFile()
        {
            var d = new OpenDialog("Open", "Open a Bank File");
            Application.Run(d);
            if (!d.Canceled)
            {
                //MessageBox.Query(50, 7, "Selected File", d.FilePath, "OK");
                string fname = d.FilePath.ToString();
                byte[] data = File.ReadAllBytes(fname);
                Bank bank = new Bank(data);

                var source = System.IO.File.OpenRead(fname);

                var hex = new HexView(source)
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill()
                };

                this.win.Add(hex);
            }

        }

        bool Quit()
        {
            var n = MessageBox.Query(50, 7, "K4Tool", "Really quit?", "Yes", "No");
            return n == 0;
        }

    }

}